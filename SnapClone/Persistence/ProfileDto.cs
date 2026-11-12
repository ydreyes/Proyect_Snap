using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Data.Definitions;

namespace SnapClone.Persistence
{
    /// <summary>
    /// Documento raiz del perfil. Los campos de cambios posteriores se
    /// reservan aqui con valores vacios.
    /// </summary>
    public class ProfileDto
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        public int Points { get; set; }
        public List<string> OwnedCardIds { get; set; } = new List<string>();
        public List<string> DiscoveredCardIds { get; set; } = new List<string>();
        public List<string> DiscoveredPerkIds { get; set; } = new List<string>();
        public List<string> DiscoveredEnemyIds { get; set; } = new List<string>();
        public List<string> OwnedPerkIds { get; set; } = new List<string>();
        public Dictionary<string, int> PerkCopyCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ItemCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>La entrada de objeto se vendio y espera el proximo descanso.</summary>
        public bool ItemEntrySold { get; set; }

        public string LastAwardedRunId { get; set; } = string.Empty;
        public List<string> VisibleBundleIds { get; set; } = new List<string>();
        public List<string> OwnedBundleIds { get; set; } = new List<string>();
        public int RotationIndex { get; set; }

        public static ProfileDto CreateDefault(IEnumerable<string> starterCardIds)
        {
            var profile = new ProfileDto
            {
                SchemaVersion = CurrentSchemaVersion,
                Points = 0,
                OwnedCardIds = starterCardIds?.Distinct().ToList() ?? new List<string>(),
                DiscoveredCardIds = new List<string>(),
                DiscoveredPerkIds = new List<string>(),
                DiscoveredEnemyIds = new List<string>(),
                OwnedPerkIds = new List<string>(PerkCatalog.AllIds),
                PerkCopyCounts = PerkCatalog.AllIds
                    .ToDictionary(id => id, id => 1, StringComparer.Ordinal),
                ItemCounts = new Dictionary<string, int>(),
                LastAwardedRunId = string.Empty,
                VisibleBundleIds = new List<string>(),
                OwnedBundleIds = new List<string>(),
                RotationIndex = 0
            };

            return profile;
        }

        public void Normalize()
        {
            Points = Math.Max(0, Points);
            OwnedCardIds ??= new List<string>();
            DiscoveredCardIds ??= new List<string>();
            DiscoveredPerkIds ??= new List<string>();
            DiscoveredEnemyIds ??= new List<string>();
            OwnedPerkIds ??= new List<string>();
            PerkCopyCounts ??= new Dictionary<string, int>();
            ItemCounts ??= new Dictionary<string, int>();
            VisibleBundleIds ??= new List<string>();
            OwnedBundleIds ??= new List<string>();
            LastAwardedRunId ??= string.Empty;

            // Migracion aditiva: todo starter actual debe estar en propiedad.
            foreach (string starterId in StarterSet.CardIds)
            {
                if (!OwnedCardIds.Contains(starterId))
                    OwnedCardIds.Add(starterId);
            }

            // Migracion: los ids retirados se eliminan del perfil; el resto del
            // perfil no cambia. Los 28 perks actuales quedan disponibles.
            OwnedPerkIds.RemoveAll(id => !PerkCatalog.IsKnown(id));
            foreach (string retired in PerkCatalog.RetiredIds)
                PerkCopyCounts.Remove(retired);
            foreach (string key in PerkCopyCounts.Keys.ToList())
            {
                if (!PerkCatalog.IsKnown(key))
                    PerkCopyCounts.Remove(key);
            }

            foreach (string perkId in PerkCatalog.AllIds)
            {
                if (!OwnedPerkIds.Contains(perkId))
                    OwnedPerkIds.Add(perkId);

                if (!PerkCopyCounts.ContainsKey(perkId))
                    PerkCopyCounts[perkId] = 1;
            }
        }
    }
}
