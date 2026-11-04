namespace SnapClone.Data.Definitions
{
    public enum CardType
    {
        OnReveal,
        Ongoing,
        EndOfTurn,
        Activate
    }
    
    public enum EffectTrigger
    {
        OnReveal,
        Ongoing,
        EndOfTurn,
        Activate,
        ZoneContinuous,
        ZoneOnTrigger,
        AfterPlayed,
        AfterMoved,
        AfterDestroyed,
        AfterDiscarded,
        AfterDrawn,
        GameStart
    }
    
    public enum Targeting
    {
        Self,
        AllCards,
        AllCardsInZone,
        EnemyCards,
        HighestCostInZone,
        Random
    }
}
