namespace SnapClone.Gameplay.Run
{
    public enum CombatKind
    {
        Normal,
        SubBoss,
        FinalBoss
    }

    public readonly struct CombatReward
    {
        public CombatReward(int xp, int money)
        {
            Xp = xp;
            Money = money;
        }

        public int Xp { get; }
        public int Money { get; }
    }

    /// <summary>
    /// Recompensa de una victoria segun el tipo de combate. La run tiene tres
    /// actos de 15 combates con subjefe en 15/30/45, un subjefe 4 en el 46 y
    /// el jefe final en el 47. Los montos no cambian por acto.
    /// </summary>
    public static class RunRewardTable
    {
        public static CombatReward For(CombatKind kind)
        {
            switch (kind)
            {
                case CombatKind.SubBoss:
                    return new CombatReward(150, 35);
                case CombatKind.FinalBoss:
                    return new CombatReward(250, 70);
                default:
                    return new CombatReward(45, 9);
            }
        }

        /// <summary>Clasifica el numero total de combate, 1..47.</summary>
        public static CombatKind Classify(int totalCombatNumber)
        {
            if (RunActTable.IsFinalBossCombat(totalCombatNumber))
                return CombatKind.FinalBoss;

            if (RunActTable.IsSubBossCombat(totalCombatNumber))
                return CombatKind.SubBoss;

            return CombatKind.Normal;
        }
    }
}
