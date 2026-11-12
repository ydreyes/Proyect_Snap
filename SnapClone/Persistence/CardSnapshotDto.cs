using System.Collections.Generic;

namespace SnapClone.Persistence
{
    /// <summary>
    /// Carta serializada por valor. La definicion se resuelve por id al cargar.
    /// </summary>
    public class CardSnapshotDto
    {
        public string DefinitionId { get; set; }
        public int CostDelta { get; set; }
        public int PowerDelta { get; set; }
        public int UpgradeCount { get; set; }
        public List<string> AttachedPerks { get; set; } = new List<string>();
        public int BonusPower { get; set; }
    }
}
