using System;
using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;

namespace SnapClone.Gameplay.AI
{
    /// <summary>
    /// IA de prototipo: elige entre las cartas que puede pagar con su energia
    /// disponible, priorizando poder por coste, y reparte las jugadas entre
    /// las zonas con menos cartas.
    /// </summary>
    public class AIStrategy
    {
        private readonly RandomService _randomService;

        public AIStrategy(RandomService randomService)
        {
            _randomService = randomService;
        }

        public (CardInstance card, ZoneInstance zone, int slot)? MakeDecision(BoardState boardState)
        {
            if (boardState.AIHand.Count == 0) return null;

            var chosenCard = ChooseCard(boardState);
            if (chosenCard == null) return null;

            List<ZoneInstance> candidates = new List<ZoneInstance>();
            int fewest = int.MaxValue;

            foreach (var zone in boardState.Zones)
            {
                if (zone.AICards.Count >= ZoneInstance.MaxCardsPerSide) continue;

                if (zone.AICards.Count < fewest)
                {
                    fewest = zone.AICards.Count;
                    candidates.Clear();
                }

                if (zone.AICards.Count == fewest)
                    candidates.Add(zone);
            }

            if (candidates.Count == 0) return null;

            var chosenZone = candidates[_randomService.Next(candidates.Count)];
            int slot = chosenZone.AICards.Count;
            return (chosenCard, chosenZone, slot);
        }

        private CardInstance ChooseCard(BoardState boardState)
        {
            int available = GetAvailableEnergy(boardState);
            List<CardInstance> best = new List<CardInstance>();
            float bestScore = float.MinValue;

            foreach (var card in boardState.AIHand)
            {
                int cost = card.Definition.Cost;
                if (cost > available) continue;

                // Coste 0 siempre primero; el resto, poder por energia.
                float score = cost == 0
                    ? float.MaxValue
                    : card.CurrentPower / (float)cost;

                if (score > bestScore + 0.0001f)
                {
                    bestScore = score;
                    best.Clear();
                    best.Add(card);
                }
                else if (Math.Abs(score - bestScore) <= 0.0001f)
                {
                    best.Add(card);
                }
            }

            if (best.Count == 0) return null;

            return best[_randomService.Next(best.Count)];
        }

        /// <summary>
        /// Energia disponible de la IA: pool del turno menos el coste de sus
        /// cartas jugadas en el turno actual.
        /// </summary>
        private static int GetAvailableEnergy(BoardState boardState)
        {
            int pool = Math.Min(boardState.CurrentTurn, TurnManager.MaxTurns);
            int spent = 0;

            if (boardState.CurrentTurn > 0)
            {
                foreach (var zone in boardState.Zones)
                {
                    foreach (var card in zone.AICards)
                    {
                        if (card.PlayedTurn == boardState.CurrentTurn)
                            spent += card.Definition.Cost;
                    }
                }
            }

            return Math.Max(0, pool - spent);
        }
    }
}
