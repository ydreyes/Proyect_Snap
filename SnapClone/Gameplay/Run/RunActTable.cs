namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Tabla fija de la estructura de actos: tres actos de 15 combates y un
    /// acto final de dos combates, 47 en total.
    /// </summary>
    public static class RunActTable
    {
        public const int ActCount = 4;
        public const int LastNormalAct = 3;
        public const int FinalAct = 4;
        public const int CombatsPerAct = 15;
        public const int FinalActCombats = 2;
        public const int TotalCombats = 47;

        /// <summary>Subjefe 4: el primer combate del acto final.</summary>
        public const int FourthSubBossCombat = 46;

        public static int CombatsInAct(int actIndex) =>
            actIndex >= FinalAct ? FinalActCombats : CombatsPerAct;

        /// <summary>El combate 15/30/45 y el 46 son subjefes; el 47 es el jefe final.</summary>
        public static bool IsSubBossCombat(int totalCombatNumber) =>
            totalCombatNumber == FourthSubBossCombat ||
            (totalCombatNumber > 0 &&
             totalCombatNumber % CombatsPerAct == 0 &&
             totalCombatNumber < TotalCombats);

        public static bool IsFinalBossCombat(int totalCombatNumber) =>
            totalCombatNumber == TotalCombats;
    }
}
