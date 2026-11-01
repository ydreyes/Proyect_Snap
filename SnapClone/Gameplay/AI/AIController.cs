using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.AI
{
    /// <summary>
    /// Aplica las decisiones de la IA al tablero. No conoce tipos de UI: la
    /// escena se encarga de crear la vista de cada carta jugada.
    /// </summary>
    public class AIController
    {
        private readonly AIStrategy _strategy;
        private readonly BoardState _boardState;

        public AIController(BoardState boardState, RandomService randomService)
        {
            _boardState = boardState;
            _strategy = new AIStrategy(randomService);
        }

        public (CardInstance card, ZoneInstance zone, int slot)? TryPlayOne()
        {
            var decision = _strategy.MakeDecision(_boardState);
            if (decision == null) return null;

            var (card, zone, slot) = decision.Value;
            if (zone.AICards.Count >= ZoneInstance.MaxCardsPerSide) return null;

            zone.AICards.Add(card);
            card.PlayedTurn = _boardState.CurrentTurn;
            card.PlayOrderIndex = _boardState.NextPlayOrder++;
            card.PlayedZoneIndex = zone.ZoneIndex;
            card.IsInPlay = true;
            card.Location = CardLocation.Board;
            _boardState.AIHand.Remove(card);

            return (card, zone, slot);
        }

        /// <summary>
        /// Juega mientras quede una carta pagable y un slot libre. Devuelve
        /// todas las jugadas para que la escena les asigne una vista.
        /// </summary>
        public List<(CardInstance card, ZoneInstance zone, int slot)> TryPlayAll()
        {
            var plays = new List<(CardInstance card, ZoneInstance zone, int slot)>();

            while (true)
            {
                var play = TryPlayOne();
                if (play == null) break;

                plays.Add(play.Value);
            }

            return plays;
        }
    }
}
