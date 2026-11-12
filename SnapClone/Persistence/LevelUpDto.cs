using System.Collections.Generic;
using SnapClone.Gameplay.Run;

namespace SnapClone.Persistence
{
    /// <summary>Nivel pendiente con sus ofertas ya generadas.</summary>
    public class LevelUpDto
    {
        public int Level { get; set; }
        public LevelUpBranch Branch { get; set; }
        public bool OffersGenerated { get; set; }
        public List<CardSnapshotDto> CardOffers { get; set; } = new List<CardSnapshotDto>();
        public List<int> UpgradeCandidateIndices { get; set; } = new List<int>();
        public List<string> PerkOffers { get; set; } = new List<string>();

        public void Normalize()
        {
            CardOffers ??= new List<CardSnapshotDto>();
            UpgradeCandidateIndices ??= new List<int>();
            PerkOffers ??= new List<string>();
        }
    }
}
