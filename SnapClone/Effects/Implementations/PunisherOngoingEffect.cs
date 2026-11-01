using SnapClone.Gameplay.Board;

namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Punisher: +1 de poder por cada carta enemiga en su zona actual.
    /// El bonus se deriva en cada calculo, nunca se acumula.
    /// </summary>
    public class PunisherOngoingEffect : CardEffectBase, ICardPowerModifier
    {
        public int Amount { get; set; } = 1;

        public int GetPowerBonus(EffectContext context)
        {
            if (context.SourceZone == null) return 0;

            int enemies = context.Owner == BoardSide.Player
                ? context.SourceZone.AICards.Count
                : context.SourceZone.PlayerCards.Count;

            return Amount * enemies;
        }
    }
}
