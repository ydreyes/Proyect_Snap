using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;

namespace SnapClone.Gameplay.Meta
{
    public enum GlobalStoreEntryKind
    {
        Item,
        ExtraLife,
        DirectCard,
        Bundle
    }

    /// <summary>Una entrada visible de la tienda global.</summary>
    public sealed class GlobalStoreEntry
    {
        public string Id { get; set; }
        public GlobalStoreEntryKind Kind { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public bool Owned { get; set; }
        public bool Available { get; set; } = true;
        public bool Hidden { get; set; }
        public string CardId { get; set; }
        public string BundleId { get; set; }
        public int BundleSize { get; set; }
    }

    public readonly struct StorePurchaseResult
    {
        public bool Success { get; }
        public string Error { get; }

        private StorePurchaseResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public static StorePurchaseResult Ok() => new StorePurchaseResult(true, null);

        public static StorePurchaseResult Fail(string error) =>
            new StorePurchaseResult(false, error);
    }

    /// <summary>
    /// Tienda permanente: precios, lotes visibles, rotacion y compras. Cada
    /// compra se aplica al perfil y se persiste de forma atomica; si el
    /// guardado falla, el perfil en memoria vuelve al estado anterior.
    /// </summary>
    public class GlobalStoreSystem
    {
        public const int ItemPrice = 60;
        public const int ExtraLifePrice = 90;
        public const int DirectCardPrice = 150;
        public const string ExtraLifeItemId = "item_extra_life";
        public const int VisibleBundleCount = 3;

        private readonly CardRegistry _cardRegistry;
        private readonly PerkRegistry _perkRegistry;
        private readonly ItemRegistry _itemRegistry;
        private readonly RandomService _random;

        public GlobalStoreSystem(CardRegistry cardRegistry,
            PerkRegistry perkRegistry, ItemRegistry itemRegistry = null,
            RandomService random = null)
        {
            _cardRegistry = cardRegistry;
            _perkRegistry = perkRegistry;
            _itemRegistry = itemRegistry;
            _random = random ?? new RandomService(1);
            Catalog = GlobalStoreCatalog.Build(cardRegistry, StarterSet.CardIds);
        }

        public StoreCatalogData Catalog { get; }

        public int BundleCount => Catalog.Bundles.Count;

        // ------------------------------------------------------------------
        // Catalogo visible
        // ------------------------------------------------------------------

        public List<GlobalStoreEntry> GetEntries(ProfileDto profile)
        {
            var entries = new List<GlobalStoreEntry>();
            if (profile == null) return entries;

            profile.Normalize();
            EnsureRotation(profile);

            bool hasItemCatalog = _itemRegistry != null &&
                                  _itemRegistry.GetAll().Count > 0;
            entries.Add(new GlobalStoreEntry
            {
                Id = "item",
                Kind = GlobalStoreEntryKind.Item,
                Name = "Objeto de combate",
                Description = "Un objeto de combate al azar. Se agota hasta el siguiente descanso.",
                Price = ItemPrice,
                Owned = profile.ItemEntrySold,
                Available = hasItemCatalog && !profile.ItemEntrySold,
                Hidden = false
            });

            bool hasExtraLife = profile.ItemCounts.TryGetValue(
                ExtraLifeItemId, out int lifeCount) && lifeCount > 0;

            entries.Add(new GlobalStoreEntry
            {
                Id = "extra_life",
                Kind = GlobalStoreEntryKind.ExtraLife,
                Name = "Vida extra",
                Description = "+1 carga para la proxima run. Se consume al perder.",
                Price = ExtraLifePrice,
                Owned = hasExtraLife,
                Available = !hasExtraLife
            });

            // La tienda permanente ya no vende perks: se obtienen solo en la run.
            foreach (string cardId in Catalog.DirectCardIds)
            {
                var card = _cardRegistry?.Get(cardId);
                if (card == null) continue;

                bool owned = profile.OwnedCardIds.Contains(cardId);
                entries.Add(new GlobalStoreEntry
                {
                    Id = "card:" + cardId,
                    Kind = GlobalStoreEntryKind.DirectCard,
                    Name = card.Name,
                    Description = $"Carta directa - coste {card.Cost}, poder {card.BasePower}.",
                    Price = DirectCardPrice,
                    Owned = owned,
                    Available = !owned,
                    CardId = cardId
                });
            }

            foreach (string bundleId in profile.VisibleBundleIds)
            {
                var bundle = Catalog.GetBundle(bundleId);
                if (bundle == null) continue;

                bool owned = profile.OwnedBundleIds.Contains(bundleId);
                entries.Add(new GlobalStoreEntry
                {
                    Id = "bundle:" + bundleId,
                    Kind = GlobalStoreEntryKind.Bundle,
                    Name = bundle.Name,
                    Description = $"{bundle.Size} cartas garantizadas.",
                    Price = bundle.Price,
                    Owned = owned,
                    Available = !owned,
                    BundleId = bundleId,
                    BundleSize = bundle.Size
                });
            }

            // Si quedan menos de tres lotes sin comprar, los huecos se muestran
            // como agotados en vez de desaparecer de la tienda.
            int visibleBundles = entries.Count(entry =>
                entry.Kind == GlobalStoreEntryKind.Bundle);
            for (int i = visibleBundles; i < VisibleBundleCount; i++)
            {
                entries.Add(new GlobalStoreEntry
                {
                    Id = "bundle_empty_" + i,
                    Kind = GlobalStoreEntryKind.Bundle,
                    Name = "Lote agotado",
                    Description = "No quedan lotes por vender.",
                    Price = 0,
                    Owned = true,
                    Available = false,
                    BundleSize = 0
                });
            }

            return entries;
        }

        public List<string> GetVisibleBundleIds(ProfileDto profile)
        {
            if (profile == null) return new List<string>();

            profile.Normalize();
            EnsureRotation(profile);
            return new List<string>(profile.VisibleBundleIds);
        }

        public bool IsCardInVisibleStore(ProfileDto profile, string cardId)
        {
            if (profile == null || string.IsNullOrWhiteSpace(cardId)) return false;

            if (Catalog.IsDirect(cardId))
                return !profile.OwnedCardIds.Contains(cardId);

            string bundleId = Catalog.BundleIdFor(cardId);
            if (bundleId == null) return false;
            if (profile.OwnedBundleIds.Contains(bundleId)) return false;

            EnsureRotation(profile);
            return profile.VisibleBundleIds.Contains(bundleId);
        }

        public void EnsureRotation(ProfileDto profile)
        {
            if (profile == null) return;

            var owned = new HashSet<string>(profile.OwnedBundleIds, StringComparer.Ordinal);
            var visible = new List<string>();

            foreach (string bundleId in profile.VisibleBundleIds)
            {
                if (owned.Contains(bundleId)) continue;
                if (Catalog.GetBundle(bundleId) == null) continue;
                if (visible.Contains(bundleId)) continue;

                visible.Add(bundleId);
                if (visible.Count >= VisibleBundleCount) break;
            }

            int total = Catalog.Bundles.Count;
            if (total > 0)
            {
                int start = ((profile.RotationIndex % total) + total) % total;

                for (int i = 0; i < total && visible.Count < VisibleBundleCount; i++)
                {
                    var bundle = Catalog.Bundles[(start + i) % total];
                    if (owned.Contains(bundle.Id) || visible.Contains(bundle.Id))
                        continue;

                    visible.Add(bundle.Id);
                }
            }

            profile.VisibleBundleIds = visible;
        }

        /// <summary>Avanza la rotacion al empezar una run.</summary>
        public void AdvanceRotation(ProfileDto profile)
        {
            if (profile == null) return;

            // El descanso entre runs reabastece la entrada de objeto.
            profile.ItemEntrySold = false;

            int total = Catalog.Bundles.Count;
            if (total <= 0) return;

            profile.RotationIndex = (profile.RotationIndex + 1) % total;
            profile.VisibleBundleIds.Clear();
            EnsureRotation(profile);
        }

        /// <summary>
        /// Prepara el perfil para una run nueva: avanza la rotacion y convierte
        /// una carga de vida extra del perfil en la carga de la run.
        /// </summary>
        public void PrepareNewRun(ProfileDto profile, RunState run)
        {
            if (profile == null || run == null) return;

            AdvanceRotation(profile);

            if (profile.ItemCounts.TryGetValue(ExtraLifeItemId, out int count) && count > 0)
            {
                profile.ItemCounts[ExtraLifeItemId] = count - 1;
                run.ExtraLifeCharges = Math.Max(run.ExtraLifeCharges, 1);
            }
        }

        // ------------------------------------------------------------------
        // Compras
        // ------------------------------------------------------------------

        public bool CanPurchase(ProfileDto profile, GlobalStoreEntry entry)
        {
            if (profile == null || entry == null) return false;
            if (entry.Hidden || !entry.Available || entry.Owned) return false;
            if (entry.Price <= 0 || profile.Points < entry.Price) return false;

            switch (entry.Kind)
            {
                case GlobalStoreEntryKind.Item:
                    return !profile.ItemEntrySold &&
                           _itemRegistry != null &&
                           _itemRegistry.GetAll().Count > 0;

                case GlobalStoreEntryKind.ExtraLife:
                    return !(profile.ItemCounts.TryGetValue(
                        ExtraLifeItemId, out int count) && count > 0);

                case GlobalStoreEntryKind.DirectCard:
                    return entry.CardId != null &&
                           Catalog.IsDirect(entry.CardId) &&
                           !profile.OwnedCardIds.Contains(entry.CardId);

                case GlobalStoreEntryKind.Bundle:
                    return entry.BundleId != null &&
                           Catalog.GetBundle(entry.BundleId) != null &&
                           !profile.OwnedBundleIds.Contains(entry.BundleId);

                default:
                    return false;
            }
        }

        public StorePurchaseResult Purchase(ProfileDto profile,
            GlobalStoreEntry entry, Func<ProfileDto, bool> save)
        {
            if (profile == null)
                return StorePurchaseResult.Fail("No hay perfil.");

            if (entry == null)
                return StorePurchaseResult.Fail("Entrada invalida.");

            profile.Normalize();

            if (entry.Owned)
                return StorePurchaseResult.Fail("Ese contenido ya esta en tu perfil.");

            if (!CanPurchase(profile, entry))
            {
                if (profile.Points < entry.Price)
                    return StorePurchaseResult.Fail("No alcanzan los puntos.");
                return StorePurchaseResult.Fail("La entrada no esta disponible.");
            }

            var snapshot = ProfileSnapshot.Capture(profile);

            try
            {
                profile.Points -= entry.Price;

                switch (entry.Kind)
                {
                    case GlobalStoreEntryKind.Item:
                        var pool = _itemRegistry?.GetAll();
                        if (pool == null || pool.Count == 0)
                            throw new InvalidOperationException("No hay objetos en el catalogo.");

                        var chosen = pool[_random.Next(pool.Count)];
                        int owned = profile.ItemCounts.TryGetValue(chosen.Id, out int count)
                            ? count
                            : 0;
                        profile.ItemCounts[chosen.Id] = owned + 1;
                        profile.ItemEntrySold = true;
                        break;

                    case GlobalStoreEntryKind.ExtraLife:
                        profile.ItemCounts[ExtraLifeItemId] = 1;
                        break;

                    case GlobalStoreEntryKind.DirectCard:
                        AddOwnedCard(profile, entry.CardId);
                        break;

                    case GlobalStoreEntryKind.Bundle:
                        var bundle = Catalog.GetBundle(entry.BundleId);
                        if (bundle == null)
                            throw new InvalidOperationException("El lote ya no existe.");

                        foreach (string cardId in bundle.CardIds)
                            AddOwnedCard(profile, cardId);

                        if (!profile.OwnedBundleIds.Contains(bundle.Id))
                            profile.OwnedBundleIds.Add(bundle.Id);

                        profile.VisibleBundleIds.Remove(bundle.Id);
                        EnsureRotation(profile);
                        break;
                }

                if (save != null && !save(profile))
                {
                    snapshot.Restore(profile);
                    return StorePurchaseResult.Fail(
                        "No se pudo guardar el perfil. La compra no se aplico.");
                }
            }
            catch (Exception ex)
            {
                snapshot.Restore(profile);
                return StorePurchaseResult.Fail(
                    "No se pudo completar la compra: " + ex.Message);
            }

            return StorePurchaseResult.Ok();
        }

        private static void AddOwnedCard(ProfileDto profile, string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId)) return;
            if (!profile.OwnedCardIds.Contains(cardId))
                profile.OwnedCardIds.Add(cardId);
        }

        /// <summary>Instantanea reversible para que una escritura fallida no deje
        /// una compra a medias en memoria.</summary>
        private sealed class ProfileSnapshot
        {
            private int _points;
            private List<string> _ownedCards;
            private List<string> _ownedPerks;
            private Dictionary<string, int> _perkCopies;
            private Dictionary<string, int> _items;
            private List<string> _ownedBundles;
            private List<string> _visibleBundles;
            private int _rotationIndex;
            private bool _itemEntrySold;
            private string _lastAwardedRunId;

            public static ProfileSnapshot Capture(ProfileDto profile)
            {
                return new ProfileSnapshot
                {
                    _points = profile.Points,
                    _ownedCards = new List<string>(profile.OwnedCardIds),
                    _ownedPerks = new List<string>(profile.OwnedPerkIds),
                    _perkCopies = new Dictionary<string, int>(
                        profile.PerkCopyCounts, StringComparer.Ordinal),
                    _items = new Dictionary<string, int>(
                        profile.ItemCounts, StringComparer.Ordinal),
                    _ownedBundles = new List<string>(profile.OwnedBundleIds),
                    _visibleBundles = new List<string>(profile.VisibleBundleIds),
                    _rotationIndex = profile.RotationIndex,
                    _itemEntrySold = profile.ItemEntrySold,
                    _lastAwardedRunId = profile.LastAwardedRunId
                };
            }

            public void Restore(ProfileDto profile)
            {
                profile.Points = _points;
                profile.OwnedCardIds = new List<string>(_ownedCards);
                profile.OwnedPerkIds = new List<string>(_ownedPerks);
                profile.PerkCopyCounts = new Dictionary<string, int>(
                    _perkCopies, StringComparer.Ordinal);
                profile.ItemCounts = new Dictionary<string, int>(
                    _items, StringComparer.Ordinal);
                profile.OwnedBundleIds = new List<string>(_ownedBundles);
                profile.VisibleBundleIds = new List<string>(_visibleBundles);
                profile.RotationIndex = _rotationIndex;
                profile.ItemEntrySold = _itemEntrySold;
                profile.LastAwardedRunId = _lastAwardedRunId;
            }
        }
    }
}
