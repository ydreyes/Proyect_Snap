using System;
using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;

namespace SnapClone.Gameplay.AI
{
    /// <summary>
    /// IA de prototipo. La dificultad del enemigo decide la carta y la zona:
    /// 1-2 mantiene el heuristico seguro de poder por energia; 3 prioriza el
    /// mayor poder actual; 4 agrega la zona mas debil del jugador y 5 elige
    /// primero esa zona y despues la carta. El filtro de autodano, la energia
    /// y los limites de zona se respetan en todos los niveles.
    /// </summary>
    public class AIStrategy
    {
        private readonly RandomService _randomService;
        private readonly TurnManager _turnManager;
        private readonly CardActionService _actions;
        private readonly int _difficulty;

        public AIStrategy(RandomService randomService, TurnManager turnManager,
            int difficulty = 1, CardActionService actions = null)
        {
            _randomService = randomService;
            _turnManager = turnManager;
            _actions = actions;
            _difficulty = Math.Max(1, Math.Min(5, difficulty));
        }

        public (CardInstance card, ZoneInstance zone, int slot)? MakeDecision(BoardState boardState)
        {
            if (boardState.AIHand.Count == 0) return null;

            // Taunt: la primera carta del turno usa la zona reservada mientras
            // tenga espacio y exista una carta legal alli. Si no, expira.
            if (!boardState.HasPlayedThisTurn(BoardSide.AI))
            {
                var reservation = boardState.GetTauntReservation(BoardSide.AI,
                    boardState.CurrentTurn);
                if (reservation != null)
                {
                    if (reservation.ZoneIndex < 0 ||
                        reservation.ZoneIndex >= boardState.Zones.Count ||
                        !boardState.Zones[reservation.ZoneIndex].CanAdd(BoardSide.AI))
                    {
                        boardState.TauntReservations.Remove(reservation);
                    }
                    else
                    {
                        var tauntedZone = boardState.Zones[reservation.ZoneIndex];
                        var tauntedCard = ChooseCard(boardState, tauntedZone);
                        if (tauntedCard != null)
                            return (tauntedCard, tauntedZone, tauntedZone.AICards.Count);

                        boardState.TauntReservations.Remove(reservation);
                    }
                }
            }

            // Dificultad 5: la zona mas debil del jugador manda antes que el
            // poder bruto de la carta.
            if (_difficulty >= 5)
            {
                var weakest = ChooseWeakestPlayerZone(boardState, null);
                if (weakest != null)
                {
                    var zoneCard = ChooseCard(boardState, weakest);
                    if (zoneCard != null)
                        return (zoneCard, weakest, weakest.AICards.Count);
                }
            }

            var chosenCard = ChooseCard(boardState, null);
            if (chosenCard == null) return null;

            var chosenZone = ChooseZone(boardState, chosenCard);
            if (chosenZone == null) return null;

            return (chosenCard, chosenZone, chosenZone.AICards.Count);
        }

        private CardInstance ChooseCard(BoardState boardState, ZoneInstance requiredZone)
        {
            int available = _turnManager.AvailableEnergy(BoardSide.AI);

            var affordable = new List<CardInstance>();
            foreach (var card in boardState.AIHand)
            {
                int cardCost = _actions != null
                    ? _actions.GetEffectiveCost(card, BoardSide.AI)
                    : card.EffectiveCost;
                if (cardCost > available) continue;
                if (_actions != null &&
                    !_actions.CanPlayCard(card, BoardSide.AI))
                    continue;
                if (requiredZone != null && !IsZoneUsable(requiredZone, card))
                    continue;

                affordable.Add(card);
            }

            var safe = new List<CardInstance>();
            foreach (var card in affordable)
            {
                if (!IsSelfHarmful(card))
                    safe.Add(card);
            }

            var candidates = safe.Count > 0 ? safe : affordable;
            if (candidates.Count == 0) return null;

            List<CardInstance> best = new List<CardInstance>();
            float bestScore = float.MinValue;

            foreach (var card in candidates)
            {
                float score = _difficulty <= 2
                    ? PowerPerEnergy(card)
                    : card.CurrentPower;

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

        private ZoneInstance ChooseZone(BoardState boardState, CardInstance card)
        {
            if (_difficulty >= 4)
            {
                var weakest = ChooseWeakestPlayerZone(boardState, card);
                if (weakest != null) return weakest;
            }

            List<ZoneInstance> candidates = new List<ZoneInstance>();
            int fewest = int.MaxValue;

            foreach (var zone in boardState.Zones)
            {
                if (!IsZoneUsable(zone, card)) continue;

                if (zone.AICards.Count < fewest)
                {
                    fewest = zone.AICards.Count;
                    candidates.Clear();
                }

                if (zone.AICards.Count == fewest)
                    candidates.Add(zone);
            }

            if (candidates.Count == 0) return null;

            return candidates[_randomService.Next(candidates.Count)];
        }

        private ZoneInstance ChooseWeakestPlayerZone(BoardState boardState,
            CardInstance card)
        {
            List<ZoneInstance> candidates = new List<ZoneInstance>();
            int weakest = int.MaxValue;

            foreach (var zone in boardState.Zones)
            {
                if (!IsZoneUsable(zone, card)) continue;

                int power = zone.GetPlayerPower();
                if (power < weakest)
                {
                    weakest = power;
                    candidates.Clear();
                }

                if (power == weakest)
                    candidates.Add(zone);
            }

            if (candidates.Count == 0) return null;

            return candidates[_randomService.Next(candidates.Count)];
        }

        private bool IsZoneUsable(ZoneInstance zone, CardInstance card)
        {
            if (zone == null) return false;
            if (zone.AICards.Count >= ZoneInstance.MaxCardsPerSide) return false;

            if (_actions != null)
                return _actions.CanPlayCardInZone(card, BoardSide.AI, zone);

            if (card != null && !zone.CanPlay(card, BoardSide.AI)) return false;
            return true;
        }

        private float PowerPerEnergy(CardInstance card)
        {
            int cost = _actions != null
                ? _actions.GetEffectiveCost(card, BoardSide.AI)
                : card.EffectiveCost;

            // Coste 0 siempre primero; el resto, poder por energia.
            return cost == 0 ? float.MaxValue : card.CurrentPower / (float)cost;
        }

        private static bool IsSelfHarmful(CardInstance card)
        {
            var tags = card?.Definition?.Tags;
            if (tags == null) return false;

            return tags.Contains("self_discard") || tags.Contains("self_destroy");
        }
    }
}
