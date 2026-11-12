using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Combat
{
    /// <summary>
    /// Reparte la mano inicial. Primero mueve todas las cartas etiquetadas
    /// para empezar en la mano y despues completa con el resto del mazo,
    /// saltando las cartas que se roban obligatoriamente en el turno 2.
    /// </summary>
    public static class OpeningHandDealer
    {
        public const string StartsInHandTag = "starts_in_hand";
        public const string ForcedTurn2Tag = "forced_turn_2_draw";

        public static void Deal(List<CardInstance> deck, List<CardInstance> hand, int size)
        {
            if (deck == null || hand == null) return;

            // Superstar y las cartas etiquetadas entran antes que las aleatorias.
            // Si hay mas forzadas que huecos, el excedente queda en el mazo.
            for (int i = deck.Count - 1; i >= 0; i--)
            {
                var card = deck[i];
                if (!StartsInHand(card)) continue;
                if (hand.Count >= size) continue;

                deck.RemoveAt(i);
                hand.Add(card);
                card.Location = CardLocation.Hand;
            }

            int index = 0;
            while (hand.Count < size && index < deck.Count)
            {
                var card = deck[index];
                if (IsForcedTurn2(card))
                {
                    index++;
                    continue;
                }

                deck.RemoveAt(index);
                hand.Add(card);
                card.Location = CardLocation.Hand;
            }
        }

        public static bool StartsInHand(CardInstance card) =>
            HasTag(card, StartsInHandTag) ||
            PerkCatalog.HasPerk(card?.AttachedPerks, PerkCatalog.Superstar);

        public static bool IsForcedTurn2(CardInstance card) =>
            HasTag(card, ForcedTurn2Tag);

        private static bool HasTag(CardInstance card, string tag) =>
            card?.Definition?.Tags != null && card.Definition.Tags.Contains(tag);
    }
}
