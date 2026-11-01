using SnapClone.Gameplay.Board;

namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Star-Lord: gana +4 al revelarse si al menos una carta rival fue jugada
    /// en su zona durante el mismo turno.
    /// </summary>
    public class StarLordOnRevealEffect : CardEffectBase
    {
        public int Amount { get; set; } = 4;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null ||
                context.SourceZone == null ||
                context.BoardState == null)
                return;

            int zoneIndex = context.SourceZone.ZoneIndex;

            foreach (var zone in context.BoardState.Zones)
            {
                var rivals = context.Owner == BoardSide.Player
                    ? zone.AICards
                    : zone.PlayerCards;

                foreach (var rival in rivals)
                {
                    if (rival.PlayedTurn != context.TurnNumber) continue;
                    if (rival.PlayedZoneIndex != zoneIndex) continue;

                    context.SourceCard.BonusPower += Amount;
                    return;
                }
            }
        }
    }
}
