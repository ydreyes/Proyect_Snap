using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Sentinel: al revelarse crea una nueva instancia de Sentinel en la mano
    /// de su duenio, si la mano tiene espacio.
    /// </summary>
    public class SentinelOnRevealEffect : CardEffectBase
    {
        public string CardId { get; set; } = "card_sentinel";

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.CardRegistry == null) return;

            var definition = context.CardRegistry.Get(CardId);
            if (definition == null) return;

            var copy = new CardInstance(definition) { Location = CardLocation.Hand };

            var hand = context.Owner == BoardSide.Player
                ? context.BoardState.PlayerHand
                : context.BoardState.AIHand;

            if (hand.Count >= BoardState.MaxHandSize) return;

            hand.Add(copy);
        }
    }
}
