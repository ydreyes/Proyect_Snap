using SnapClone.Gameplay.Cards;

namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Hawkeye: arma un bonus de +5 que se resuelve al final del turno
    /// siguiente si se jugo una carta aliada en la zona donde se jugo Hawkeye.
    /// </summary>
    public class HawkeyeOnRevealEffect : CardEffectBase
    {
        public int Amount { get; set; } = 5;

        public override void Apply(EffectContext context)
        {
            var card = context.SourceCard;
            if (card == null) return;

            int watchedZone = card.PlayedZoneIndex;
            if (watchedZone < 0 && context.SourceZone != null)
                watchedZone = context.SourceZone.ZoneIndex;

            card.PendingEffects.Add(new PendingCardEffect
            {
                Kind = PendingEffectKind.AllyPlayedInZoneNextTurn,
                ArmedTurn = context.TurnNumber,
                ZoneIndex = watchedZone,
                Amount = Amount
            });
        }
    }
}
