using SnapClone.Gameplay.Board;

namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Star-Lord: gana +4 al revelarse si al menos una carta rival fue jugada
    /// en su zona durante el mismo turno. Usa el registro de jugadas para que
    /// una carta movida o destruida despues de jugarse siga contando.
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

            var playLog = context.BoardState.PlayLog;
            if (playLog != null && playLog.Count > 0)
            {
                foreach (var record in playLog)
                {
                    if (record.Turn != context.TurnNumber) continue;
                    if (record.ZoneIndex != zoneIndex) continue;
                    if (record.Side == context.Owner) continue;

                    context.AddPower(context.SourceCard, Amount);
                    return;
                }

                return;
            }

            // Compatibilidad para tableros construidos directamente (tests).
            foreach (var zone in context.BoardState.Zones)
            {
                var rivals = context.Owner == BoardSide.Player
                    ? zone.AICards
                    : zone.PlayerCards;

                foreach (var rival in rivals)
                {
                    if (rival.PlayedTurn != context.TurnNumber) continue;
                    if (rival.PlayedZoneIndex != zoneIndex) continue;

                    context.AddPower(context.SourceCard, Amount);
                    return;
                }
            }
        }
    }
}
