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
        private readonly TurnManager _turnManager;

        public AIStrategy(RandomService randomService, TurnManager turnManager)
        {
            _randomService = randomService;
            _turnManager = turnManager;
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
                if (!zone.CanPlay(chosenCard, BoardSide.AI)) continue;

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
            int available = _turnManager.AvailableEnergy(BoardSide.AI);

            var affordable = new List<CardInstance>();
            foreach (var card in boardState.AIHand)
            {
                if (card.Definition.Cost <= available)
                    affordable.Add(card);
            }

            var safe = new List<CardInstance>();
            foreach (var card in affordable)
            {
                if (!IsSelfHarmful(card))
                    safe.Add(card);
            }

            var candidates = safe.Count > 0 ? safe : affordable;
            List<CardInstance> best = new List<CardInstance>();
            float bestScore = float.MinValue;

            foreach (var card in candidates)
            {
                int cost = card.Definition.Cost;

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

        private static bool IsSelfHarmful(CardInstance card)
        {
            var tags = card?.Definition?.Tags;
            if (tags == null) return false;

            return tags.Contains("self_discard") || tags.Contains("self_destroy");
        }
    }
}
