using System.Collections.Generic;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Combat
{
    /// <summary>
    /// Reparte la mano inicial. Primero mueve todas las cartas etiquetadas
    /// para empezar en la mano y despues completa con el resto del mazo.
    /// </summary>
    public static class OpeningHandDealer
    {
        public const string StartsInHandTag = "starts_in_hand";

        public static void Deal(List<CardInstance> deck, List<CardInstance> hand, int size)
        {
            if (deck == null || hand == null) return;

            for (int i = deck.Count - 1; i >= 0; i--)
            {
                var card = deck[i];
                if (!StartsInHand(card)) continue;

                deck.RemoveAt(i);
                hand.Add(card);
                card.Location = CardLocation.Hand;
            }

            while (hand.Count < size && deck.Count > 0)
            {
                var card = deck[0];
                deck.RemoveAt(0);
                hand.Add(card);
                card.Location = CardLocation.Hand;
            }
        }

        private static bool StartsInHand(CardInstance card)
        {
            return card?.Definition?.Tags != null &&
                card.Definition.Tags.Contains(StartsInHandTag);
        }
    }
}
