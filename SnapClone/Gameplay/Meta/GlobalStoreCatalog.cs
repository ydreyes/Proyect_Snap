using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;

namespace SnapClone.Gameplay.Meta
{
    /// <summary>
    /// Particion determinista del catalogo de 92 cartas. Conserva la particion
    /// legacy de Series 1 (6 directas y 6 lotes) y agrega un lote separado de
    /// Series 2 (6 directas y 3 lotes) para no cambiar compras existentes.
    /// </summary>
    public static class GlobalStoreCatalog
    {
        public const int ExpectedDraftable = 217;
        public const int ExpectedStarters = 21;
        public const int ExpectedDirectCards = 18;
        public const int ExpectedBundleCards = 178;
        public const int ExpectedBundles = 26;

        private static readonly int[] LegacyBundleSizes = { 6, 6, 6, 7, 7, 8 };
        private static readonly int[] Series2BundleSizes = { 6, 6, 7 };

        public static StoreCatalogData Build(CardRegistry cardRegistry,
            IEnumerable<string> starterCardIds)
        {
            if (cardRegistry == null)
                throw new InvalidOperationException("Catalogo de cartas ausente.");

            var errors = new List<string>();
            var draftable = cardRegistry.GetDraftable()
                .Where(card => card != null && !card.IsToken)
                .OrderBy(card => card.Id, StringComparer.Ordinal)
                .ToList();

            if (draftable.Count != ExpectedDraftable)
            {
                errors.Add($"El catalogo debe tener {ExpectedDraftable} cartas " +
                           $"draftables, tiene {draftable.Count}.");
            }

            var starters = new HashSet<string>(
                starterCardIds ?? Array.Empty<string>(), StringComparer.Ordinal);

            if (starters.Count != ExpectedStarters)
                errors.Add($"El set inicial debe tener {ExpectedStarters} cartas, " +
                           $"tiene {starters.Count}.");

            foreach (string starter in starters)
            {
                if (draftable.All(card => card.Id != starter))
                    errors.Add($"{starter}: starter fuera del catalogo draftable.");
            }

            var legacyPool = draftable
                .Where(card => card.Series == CardSeries.Series1 &&
                               !starters.Contains(card.Id))
                .ToList();

            var series2Pool = draftable
                .Where(card => card.Series == CardSeries.Series2 &&
                               !starters.Contains(card.Id))
                .ToList();

            var series3Pool = draftable
                .Where(card => card.Series == CardSeries.Series3 &&
                               !starters.Contains(card.Id))
                .ToList();

            if (legacyPool.Count != 46)
            {
                errors.Add($"El lote legacy debe tener 46 cartas, tiene " +
                           $"{legacyPool.Count}.");
            }

            if (series2Pool.Count != 25)
            {
                errors.Add($"El lote Series 2 debe tener 25 cartas, tiene " +
                           $"{series2Pool.Count}.");
            }

            var direct = new List<string>();
            var bundles = new List<CardBundleDef>();
            var directByCard = new Dictionary<string, string>(StringComparer.Ordinal);
            var bundleByCard = new Dictionary<string, string>(StringComparer.Ordinal);

            BuildPartition(legacyPool, 6, LegacyBundleSizes, "bundle_",
                "Lote ", direct, bundles, directByCard, bundleByCard, errors);

            BuildPartition(series2Pool, 6, Series2BundleSizes, "bundle_s2_",
                "Lote S2 ", direct, bundles, directByCard, bundleByCard, errors);

            BuildFlexiblePartition(series3Pool, 6, 7, "bundle_s3_",
                "Lote S3 ", direct, bundles, directByCard, bundleByCard, errors);

            var assigned = new HashSet<string>(StringComparer.Ordinal);
            foreach (string cardId in starters)
                assigned.Add(cardId);

            foreach (string cardId in direct)
            {
                if (!assigned.Add(cardId))
                    errors.Add($"{cardId}: carta directa duplicada o ya inicial.");
            }

            foreach (var bundle in bundles)
            {
                foreach (string cardId in bundle.CardIds)
                {
                    if (!assigned.Add(cardId))
                        errors.Add($"{cardId}: pertenece a dos grupos del catalogo.");
                }
            }

            foreach (var card in draftable)
            {
                if (!assigned.Contains(card.Id))
                    errors.Add($"{card.Id}: carta sin grupo en la tienda global.");
            }

            if (direct.Count != ExpectedDirectCards)
            {
                errors.Add($"Deben existir {ExpectedDirectCards} cartas directas, " +
                           $"hay {direct.Count}.");
            }

            if (bundles.Count != ExpectedBundles)
            {
                errors.Add($"Deben existir {ExpectedBundles} lotes, hay " +
                           $"{bundles.Count}.");
            }

            int bundleTotal = bundles.Sum(bundle => bundle.CardIds.Count);
            if (bundleTotal != ExpectedBundleCards)
            {
                errors.Add($"Los {ExpectedBundles} lotes deben sumar " +
                           $"{ExpectedBundleCards} cartas, suman {bundleTotal}.");
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Particion del catalogo invalida:\n" + string.Join("\n", errors));
            }

            return new StoreCatalogData(direct, directByCard, bundles, bundleByCard);
        }

        private static void BuildPartition(List<CardDef> pool, int directCount,
            int[] bundleSizes, string bundlePrefix, string bundleNamePrefix,
            List<string> direct, List<CardBundleDef> bundles,
            Dictionary<string, string> directByCard,
            Dictionary<string, string> bundleByCard,
            List<string> errors)
        {
            if (pool == null || pool.Count == 0) return;

            var partitionDirect = BuildDirectCards(pool, directCount, errors);
            foreach (string cardId in partitionDirect)
            {
                direct.Add(cardId);
                directByCard[cardId] = cardId;
            }

            var remaining = pool
                .Where(card => !partitionDirect.Contains(card.Id))
                .Select(card => card.Id)
                .ToList();

            int cursor = 0;
            for (int i = 0; i < bundleSizes.Length; i++)
            {
                int size = bundleSizes[i];
                var cardIds = new List<string>();

                for (int j = 0; j < size && cursor < remaining.Count; j++)
                    cardIds.Add(remaining[cursor++]);

                if (cardIds.Count != size)
                {
                    errors.Add($"{bundlePrefix}{size}: el lote debe tener " +
                               $"{size} cartas, tiene {cardIds.Count}.");
                }

                string suffix = size == 8
                    ? "8"
                    : $"{size}_{(char)('a' + CountSize(bundles, size, bundlePrefix))}";

                string id = bundlePrefix + suffix;
                string letter = size == 8
                    ? string.Empty
                    : ((char)('a' + CountSize(bundles, size, bundlePrefix))).ToString();

                bundles.Add(new CardBundleDef
                {
                    Id = id,
                    Name = size == 8
                        ? $"{bundleNamePrefix}8 A"
                        : $"{bundleNamePrefix}{size} {letter}",
                    Price = PriceForSize(size),
                    CardIds = cardIds
                });

                foreach (string cardId in cardIds)
                    bundleByCard[cardId] = id;
            }

            if (cursor != remaining.Count)
            {
                errors.Add($"{bundlePrefix}: quedaron " +
                           $"{remaining.Count - cursor} cartas sin lote.");
            }
        }

        private static void BuildFlexiblePartition(List<CardDef> pool,
            int directCount, int bundleSize, string bundlePrefix,
            string bundleNamePrefix, List<string> direct,
            List<CardBundleDef> bundles,
            Dictionary<string, string> directByCard,
            Dictionary<string, string> bundleByCard, List<string> errors)
        {
            if (pool == null || pool.Count == 0) return;

            var partitionDirect = BuildDirectCards(pool, directCount, errors);
            foreach (string cardId in partitionDirect)
            {
                direct.Add(cardId);
                directByCard[cardId] = cardId;
            }

            var remaining = pool
                .Where(card => !partitionDirect.Contains(card.Id))
                .Select(card => card.Id)
                .ToList();

            int bundleIndex = 0;
            for (int i = 0; i < remaining.Count; i += bundleSize)
            {
                var cardIds = remaining.Skip(i).Take(bundleSize).ToList();
                string letter = ((char)('a' + bundleIndex)).ToString();
                string id = $"{bundlePrefix}{bundleSize}_{letter}";
                int price = PriceForSize(cardIds.Count);
                if (price <= 0) price = 650;

                bundles.Add(new CardBundleDef
                {
                    Id = id,
                    Name = $"{bundleNamePrefix}{bundleSize} {letter}",
                    Price = price,
                    CardIds = cardIds
                });

                foreach (string cardId in cardIds)
                    bundleByCard[cardId] = id;

                bundleIndex++;
            }
        }

        private static List<string> BuildDirectCards(List<CardDef> pool,
            int count, List<string> errors)
        {
            var direct = new List<string>();
            if (pool.Count == 0) return direct;

            int directCount = Math.Min(count, pool.Count);
            for (int i = 0; i < directCount; i++)
            {
                int index = (int)((long)i * pool.Count / count);
                if (index >= pool.Count) index = pool.Count - 1;

                string cardId = pool[index].Id;
                if (direct.Contains(cardId))
                {
                    for (int fallback = 0; fallback < pool.Count; fallback++)
                    {
                        if (!direct.Contains(pool[fallback].Id))
                        {
                            cardId = pool[fallback].Id;
                            break;
                        }
                    }
                }

                direct.Add(cardId);
            }

            return direct;
        }

        public static int PriceForSize(int size)
        {
            switch (size)
            {
                case 6: return 450;
                case 7: return 650;
                case 8: return 850;
                default: return 0;
            }
        }

        private static int BundleSizeFor(int price)
        {
            switch (price)
            {
                case 450: return 6;
                case 650: return 7;
                case 850: return 8;
                default: return 0;
            }
        }

        private static int CountSize(List<CardBundleDef> bundles, int size,
            string prefix)
        {
            return bundles.Count(bundle =>
                bundle.CardIds.Count == size &&
                bundle.Id.StartsWith(prefix, StringComparison.Ordinal));
        }
    }

    public sealed class StoreCatalogData
    {
        private readonly Dictionary<string, string> _directByCard;
        private readonly Dictionary<string, string> _bundleByCard;

        public StoreCatalogData(List<string> directCardIds,
            Dictionary<string, string> directByCard,
            List<CardBundleDef> bundles,
            Dictionary<string, string> bundleByCard)
        {
            DirectCardIds = directCardIds;
            Bundles = bundles;
            _directByCard = directByCard;
            _bundleByCard = bundleByCard;
        }

        public IReadOnlyList<string> DirectCardIds { get; }
        public IReadOnlyList<CardBundleDef> Bundles { get; }

        public bool IsDirect(string cardId) =>
            !string.IsNullOrWhiteSpace(cardId) && _directByCard.ContainsKey(cardId);

        public bool IsBundleCard(string cardId) =>
            !string.IsNullOrWhiteSpace(cardId) && _bundleByCard.ContainsKey(cardId);

        public string BundleIdFor(string cardId) =>
            _bundleByCard.TryGetValue(cardId ?? string.Empty, out string bundleId)
                ? bundleId
                : null;

        public CardBundleDef GetBundle(string bundleId) =>
            Bundles.FirstOrDefault(bundle => string.Equals(bundle.Id, bundleId,
                StringComparison.Ordinal));

        public bool IsStoreCard(string cardId) =>
            IsDirect(cardId) || IsBundleCard(cardId);
    }

    public sealed class CardBundleDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public List<string> CardIds { get; set; } = new List<string>();
        public int Size => CardIds?.Count ?? 0;
    }
}
