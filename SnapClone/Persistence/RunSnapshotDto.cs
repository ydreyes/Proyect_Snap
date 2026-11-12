using SnapClone.Gameplay.Run;

namespace SnapClone.Persistence
{
    /// <summary>Estado escalar previo a un combate.</summary>
    public class RunSnapshotDto
    {
        public int ActIndex { get; set; }
        public int CombatIndexInAct { get; set; }
        public int TotalCombats { get; set; }
        public int Xp { get; set; }
        public int Level { get; set; }
        public int Money { get; set; }
        public int XpEarned { get; set; }
        public int MoneyEarned { get; set; }
        public int CombatsWon { get; set; }
        public int ShopEventChance { get; set; }
        public int FinalActStep { get; set; }
        public RunNode PendingNode { get; set; }
        public RunOutcome Outcome { get; set; }
        public int DeckCount { get; set; }
    }
}
