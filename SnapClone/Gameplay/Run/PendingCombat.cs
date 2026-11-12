namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Estado escalar de la run justo antes de entrar a un combate. Permite
    /// aplicar el resultado una sola vez y, mas adelante, serializar el
    /// encuentro para reanudarlo.
    /// </summary>
    public class RunSnapshot
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

        public static RunSnapshot Capture(RunState state) => new RunSnapshot
        {
            ActIndex = state.ActIndex,
            CombatIndexInAct = state.CombatIndexInAct,
            TotalCombats = state.TotalCombats,
            Xp = state.Xp,
            Level = state.Level,
            Money = state.Money,
            XpEarned = state.XpEarned,
            MoneyEarned = state.MoneyEarned,
            CombatsWon = state.CombatsWon,
            ShopEventChance = state.ShopEventChance,
            FinalActStep = state.FinalActStep,
            PendingNode = state.PendingNode,
            Outcome = state.Outcome,
            DeckCount = state.Deck.Count
        };
    }

    /// <summary>
    /// Combate confirmado pero todavia no resuelto: identidad del rival,
    /// numero de intento, semilla y estado previo al combate.
    /// </summary>
    public class PendingCombat
    {
        public string EnemyId { get; set; }
        public string EnemyName { get; set; }
        public string EnemyPhrase { get; set; }
        public int Difficulty { get; set; }
        public int Attempt { get; set; }

        /// <summary>Semilla del tablero/barajado del intento actual.</summary>
        public int Seed { get; set; }

        /// <summary>
        /// Semilla que define la composicion del mazo rival. Se conserva al
        /// reanudar un combate para mantener el mismo rival aunque cambie la
        /// semilla del tablero.
        /// </summary>
        public int CompositionSeed { get; set; }

        public RunSnapshot PreCombat { get; set; }
    }
}
