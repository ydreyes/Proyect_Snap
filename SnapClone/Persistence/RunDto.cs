using System.Collections.Generic;
using SnapClone.Gameplay.Run;

namespace SnapClone.Persistence
{
    /// <summary>
    /// Documento raiz de la run. Se escribe en run.json.
    /// </summary>
    public class RunDto
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;

        public RunStatus Status { get; set; } = RunStatus.Active;
        public string RunId { get; set; } = string.Empty;
        public int ActIndex { get; set; }
        public int CombatIndexInAct { get; set; }
        public int TotalCombats { get; set; }
        public int NormalCombatsWon { get; set; }
        public int SubBossesWon { get; set; }
        public bool FinalBossDefeated { get; set; }
        public int ShopEventChance { get; set; }
        public int FinalActStep { get; set; }
        public RunNode PendingNode { get; set; }

        public int Xp { get; set; }
        public int Level { get; set; }
        public int Money { get; set; }
        public int XpEarned { get; set; }
        public int MoneyEarned { get; set; }
        public int CombatsWon { get; set; }
        public RunOutcome Outcome { get; set; }

        public List<CardSnapshotDto> Deck { get; set; } = new List<CardSnapshotDto>();
        public List<LevelUpDto> PendingLevelUps { get; set; } = new List<LevelUpDto>();
        public DraftDto Draft { get; set; }
        public PendingCombatDto PendingCombat { get; set; }

        /// <summary>Combate perdido disponible para reintentar desde el resumen.</summary>
        public PendingCombatDto RematchCombat { get; set; }

        public List<string> RunModifiers { get; set; } = new List<string>();
        public int ExtraLifeCharges { get; set; }
        public ShopStateDto Shop { get; set; }
        public EventStateDto Event { get; set; }
        public List<string> SeenEventIds { get; set; } = new List<string>();

        public void Normalize()
        {
            Deck ??= new List<CardSnapshotDto>();
            PendingLevelUps ??= new List<LevelUpDto>();
            RunModifiers ??= new List<string>();
            SeenEventIds ??= new List<string>();
            Shop?.Normalize();

            foreach (var levelUp in PendingLevelUps)
                levelUp.Normalize();

            Draft?.Normalize();
        }
    }
}
