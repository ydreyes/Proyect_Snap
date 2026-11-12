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
        StartOfTurn,
        Empowered,
        Activate,
        ZoneContinuous,
        ZoneOnTrigger,
        AfterPlayed,
        AfterMoved,
        AfterDestroyed,
        AfterDiscarded,
        AfterDrawn,
        AfterAddedToHand,
        GameStart,
        EndOfGame,

        /// <summary>Marca una meta-habilidad; nunca se dispara por si sola.</summary>
        Passive
    }

    /// <summary>Lote de contenido al que pertenece una carta.</summary>
    public enum CardSeries
    {
        CollectionLevel,
        Series1,
        Series2,
        Series3
    }

    /// <summary>Frecuencia con la que una carta puede moverse por habilidad impresa.</summary>
    public enum MovePermission
    {
        None,
        OncePerCombat,
        OncePerTurn
    }

    /// <summary>Naturaleza de un perk de habilidad.</summary>
    public enum PerkKind
    {
        GrantedEffect,
        MetaAbility,
        RuleKeyword
    }

    /// <summary>Requisito de la carta anfitriona para recibir el perk.</summary>
    public enum PerkCompatibility
    {
        Any,
        RequiresOngoing,
        RequiresOnReveal,
        RequiresEndOfTurn,
        RequiresEmpower
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

    /// <summary>Rareza de un objeto de combate. No altera su efecto.</summary>
    public enum ItemRarity
    {
        Common,
        Rare
    }

    /// <summary>Conjunto legal de objetivos que un objeto puede elegir.</summary>
    public enum ItemTargetRule
    {
        None,
        AllyCard,
        EnemyCard,
        EnemyMove,
        AllyZone,
        EnemyZone,
        HandCard,
        Draw
    }

    /// <summary>Arquetipo que usa el draft tematico para las tres primeras rondas.</summary>
    public enum DraftTheme
    {
        Control,
        Destroy,
        OnReveal,
        Neutral
    }

    /// <summary>Etiquetas visibles de los temas. ASCII para el contenido actual.</summary>
    public static class DraftThemeText
    {
        public static string Label(DraftTheme theme)
        {
            switch (theme)
            {
                case DraftTheme.Control: return "CONTROL";
                case DraftTheme.Destroy: return "DESTRUIR";
                case DraftTheme.OnReveal: return "AL REVELARSE";
                default: return "NEUTRAL";
            }
        }

        public static string RoundLabel(DraftTheme theme) =>
            theme == DraftTheme.OnReveal ? "AL REVELARSE" : Label(theme);
    }
}
