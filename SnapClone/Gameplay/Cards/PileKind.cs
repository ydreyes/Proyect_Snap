namespace SnapClone.Gameplay.Cards
{
    /// <summary>Las tres pilas que puede tener cada lado en un combate.</summary>
    public enum PileKind
    {
        Discard,
        Destroyed,
        Banished
    }

    /// <summary>
    /// Donde esta una carta. Las tres ultimas son pilas y solo las asigna
    /// CardPileManager; el resto lo asignan las transiciones normales.
    /// </summary>
    public enum CardLocation
    {
        Deck,
        Hand,
        Board,
        Discard,
        Destroyed,
        Banished
    }

    public static class PileKindExtensions
    {
        public static string DisplayName(this PileKind kind)
        {
            switch (kind)
            {
                case PileKind.Destroyed: return "DESTRUIDAS";
                case PileKind.Banished: return "DESTERRADAS";
                default: return "DESCARTADAS";
            }
        }

        public static CardLocation ToLocation(this PileKind kind)
        {
            switch (kind)
            {
                case PileKind.Destroyed: return CardLocation.Destroyed;
                case PileKind.Banished: return CardLocation.Banished;
                default: return CardLocation.Discard;
            }
        }
    }
}
