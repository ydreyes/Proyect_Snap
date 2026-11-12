using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;

namespace SnapClone.Gameplay.Meta
{
    public enum CollectionCardState
    {
        Owned,
        Discovered,
        InStore,
        Hidden
    }

    public sealed class CollectionCardEntry
    {
        public CardDef Card { get; set; }
        public CollectionCardState State { get; set; }
        public bool InRunDeck { get; set; }
        public string ObtainPath { get; set; }
    }

    public sealed class CollectionPerkEntry
    {
        public PerkDef Perk { get; set; }
        public bool Owned { get; set; }
        public int OwnedCopies { get; set; }
        public int AttachedCopies { get; set; }
        public bool HasCompatibleHost { get; set; }
        public string TriggerText { get; set; }
        public string CompatibilityText { get; set; }
        public string FooterText { get; set; }
        public string ObtainPath { get; set; }
    }

    /// <summary>
    /// Deriva los cinco estados de la coleccion desde el perfil y la run. Es
    /// solo lectura: abrir la coleccion nunca descubre contenido.
    /// </summary>
    public class CollectionSystem
    {
        private readonly CardRegistry _cardRegistry;
        private readonly PerkRegistry _perkRegistry;
        private readonly GlobalStoreSystem _store;
        private readonly EffectRegistry _effectRegistry;

        public CollectionSystem(CardRegistry cardRegistry,
            PerkRegistry perkRegistry, GlobalStoreSystem store,
            EffectRegistry effectRegistry = null)
        {
            _cardRegistry = cardRegistry;
            _perkRegistry = perkRegistry;
            _store = store;
            _effectRegistry = effectRegistry;
        }

        public List<CollectionCardEntry> BuildCards(ProfileDto profile,
            RunState runState)
        {
            var entries = new List<CollectionCardEntry>();
            if (_cardRegistry == null || profile == null) return entries;

            profile.Normalize();

            var owned = new HashSet<string>(profile.OwnedCardIds, StringComparer.Ordinal);
            var discovered = new HashSet<string>(
                profile.DiscoveredCardIds, StringComparer.Ordinal);
            var inRunDeck = new HashSet<string>(StringComparer.Ordinal);

            if (runState?.Deck != null)
            {
                foreach (var card in runState.Deck)
                {
                    string id = card?.Definition?.Id;
                    if (!string.IsNullOrWhiteSpace(id))
                        inRunDeck.Add(id);
                }
            }

            var draftable = _cardRegistry.GetDraftable()
                .Where(card => card != null && !card.IsToken)
                .OrderBy(card => card.Cost)
                .ThenBy(card => card.Id, StringComparer.Ordinal)
                .ToList();

            foreach (var card in draftable)
            {
                bool isOwned = owned.Contains(card.Id);
                bool isDiscovered = discovered.Contains(card.Id);
                bool inStore = !isOwned && !isDiscovered && IsInVisibleStore(profile, card.Id);

                CollectionCardState state;
                if (isOwned)
                    state = CollectionCardState.Owned;
                else if (isDiscovered)
                    state = CollectionCardState.Discovered;
                else if (inStore)
                    state = CollectionCardState.InStore;
                else
                    state = CollectionCardState.Hidden;

                entries.Add(new CollectionCardEntry
                {
                    Card = card,
                    State = state,
                    InRunDeck = isOwned && inRunDeck.Contains(card.Id),
                    ObtainPath = ObtainPathFor(profile, card, state)
                });
            }

            return entries;
        }

        public List<CollectionPerkEntry> BuildPerks(ProfileDto profile,
            RunState runState = null)
        {
            var entries = new List<CollectionPerkEntry>();
            if (_perkRegistry == null || profile == null) return entries;

            profile.Normalize();

            foreach (string perkId in PerkCatalog.AllIds)
            {
                var perk = _perkRegistry.Get(perkId);
                if (perk == null) continue;

                bool owned = profile.OwnedPerkIds.Contains(perkId);
                int copies = profile.PerkCopyCounts.TryGetValue(perkId, out int count)
                    ? count
                    : (owned ? 1 : 0);

                int attached = 0;
                bool compatible = false;
                if (runState?.Deck != null)
                {
                    foreach (var card in runState.Deck)
                    {
                        if (card?.AttachedPerks != null)
                        {
                            foreach (string attachedPerk in card.AttachedPerks)
                            {
                                if (string.Equals(attachedPerk, perkId,
                                        StringComparison.Ordinal))
                                    attached++;
                            }
                        }

                        if (PerkAbilities.CanAttach(perkId, card, _effectRegistry))
                            compatible = true;
                    }
                }

                entries.Add(new CollectionPerkEntry
                {
                    Perk = perk,
                    Owned = true,
                    OwnedCopies = owned ? Math.Max(1, copies) : 1,
                    AttachedCopies = attached,
                    HasCompatibleHost = compatible,
                    TriggerText = perk.TriggerText,
                    CompatibilityText = perk.CompatibilityText,
                    FooterText = compatible
                        ? $"COPIAS {Math.Max(1, copies)}   ADJUNTOS {attached}"
                        : $"COPIAS {Math.Max(1, copies)}   SIN ANFITRION",
                    ObtainPath = "EN COLECCION"
                });
            }

            return entries;
        }

        public static bool Matches(CollectionCardEntry entry, CollectionFilter filter)
        {
            if (entry == null) return false;
            if (filter == null) return true;

            if (filter.State.HasValue && entry.State != filter.State.Value)
                return false;

            if (filter.Theme.HasValue &&
                entry.Card?.DraftTheme != filter.Theme.Value)
                return false;

            if (filter.Cost.HasValue && (entry.Card?.Cost ?? -1) != filter.Cost.Value)
                return false;

            return true;
        }

        public static string StateLabel(CollectionCardState state)
        {
            switch (state)
            {
                case CollectionCardState.Owned: return "EN COLECCION";
                case CollectionCardState.Discovered: return "DESCUBIERTA";
                case CollectionCardState.InStore: return "EN TIENDA";
                default: return "DESCUBRELA EN LA PARTIDA";
            }
        }

        public static string FilterStateLabel(CollectionCardState? state)
        {
            if (!state.HasValue) return "TODAS";
            switch (state.Value)
            {
                case CollectionCardState.Owned: return "PROPIAS";
                case CollectionCardState.Discovered: return "DESCUBIERTAS";
                case CollectionCardState.InStore: return "TIENDA";
                default: return "OCULTAS";
            }
        }

        public static string ThemeFilterLabel(DraftTheme? theme) =>
            theme.HasValue ? DraftThemeText.Label(theme.Value) : "TODOS";

        public static string CostFilterLabel(int? cost) =>
            cost.HasValue ? $"COSTE {cost.Value}" : "COSTE TODOS";

        private bool IsInVisibleStore(ProfileDto profile, string cardId)
        {
            return _store != null && _store.IsCardInVisibleStore(profile, cardId);
        }

        private string ObtainPathFor(ProfileDto profile, CardDef card,
            CollectionCardState state)
        {
            if (state == CollectionCardState.Owned)
                return "EN COLECCION";

            if (state == CollectionCardState.Hidden)
                return "DESCUBRELA EN LA PARTIDA";

            if (_store == null || card == null)
                return "EN LA PARTIDA";

            if (_store.Catalog.IsDirect(card.Id))
                return $"TIENDA: CARTA DIRECTA {GlobalStoreSystem.DirectCardPrice}";

            string bundleId = _store.Catalog.BundleIdFor(card.Id);
            var bundle = bundleId != null ? _store.Catalog.GetBundle(bundleId) : null;
            if (bundle != null)
                return $"TIENDA: {bundle.Name} {bundle.Price} ({bundle.Size} CARTAS)";

            return "EN LA PARTIDA";
        }
    }

    public sealed class CollectionFilter
    {
        public CollectionCardState? State { get; set; }
        public DraftTheme? Theme { get; set; }
        public int? Cost { get; set; }

        public bool IsClear => !State.HasValue && !Theme.HasValue && !Cost.HasValue;

        public void Clear()
        {
            State = null;
            Theme = null;
            Cost = null;
        }
    }
}
