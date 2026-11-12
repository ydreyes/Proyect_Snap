namespace SnapClone.Persistence
{
    /// <summary>Combate confirmado y aun no resuelto.</summary>
    public class PendingCombatDto
    {
        public string EnemyId { get; set; }
        public string EnemyName { get; set; }
        public string EnemyPhrase { get; set; }
        public int Difficulty { get; set; }
        public int Attempt { get; set; }
        public int Seed { get; set; }
        public int CompositionSeed { get; set; }
        public RunSnapshotDto PreCombat { get; set; }
    }
}
