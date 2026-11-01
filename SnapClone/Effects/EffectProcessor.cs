using System.Collections.Generic;
using SnapClone.Data.Registries;
using SnapClone.Data.Definitions;
using SnapClone.Effects.Implementations;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Effects
{
    /// <summary>
    /// Resuelve los efectos de carta. Las implementaciones se crean una vez y
    /// se cachean porque son stateless; el estado mutable vive en las cartas y
    /// en el tablero. Las consultas Ongoing nunca escriben el poder actual.
    /// </summary>
    public class EffectProcessor
    {
        private readonly EffectRegistry _effectRegistry;
        private readonly CardRegistry _cardRegistry;
        private readonly Dictionary<string, IEffect> _effects =
            new Dictionary<string, IEffect>();

        public EffectProcessor(EffectRegistry effectRegistry, CardRegistry cardRegistry = null)
        {
            _effectRegistry = effectRegistry;
            _cardRegistry = cardRegistry;
        }

        public IEffect GetEffect(string effectId)
        {
            if (string.IsNullOrEmpty(effectId) || _effectRegistry == null) return null;

            if (_effects.TryGetValue(effectId, out IEffect cached))
                return cached;

            var definition = _effectRegistry.Get(effectId);
            if (definition == null) return null;

            var effect = BaseEffectFactory.CreateEffect(definition);
            if (effect != null)
                _effects[effectId] = effect;

            return effect;
        }

        /// <summary>
        /// Aplica los efectos OnReveal en el orden recibido y despues resuelve
        /// los bonus diferidos (Hawkeye) que ya cumplieron su turno.
        /// </summary>
        public void ResolveEndOfTurn(BoardState boardState, List<CardInstance> revealOrder)
        {
            if (boardState == null) return;

            if (revealOrder != null)
            {
                foreach (var card in revealOrder)
                    ApplyOnReveal(boardState, card);
            }

            ResolvePendingBonuses(boardState);
        }

        private void ApplyOnReveal(BoardState boardState, CardInstance card)
        {
            if (card?.Definition?.EffectIds == null) return;

            var zone = FindZone(boardState, card);
            var owner = FindOwner(boardState, card);
            var context = CreateContext(card, zone, boardState, owner);

            foreach (var effectId in card.Definition.EffectIds)
            {
                var effect = GetEffect(effectId);
                if (effect == null || effect.Trigger != EffectTrigger.OnReveal) continue;

                effect.Apply(context);
            }
        }

        private void ResolvePendingBonuses(BoardState boardState)
        {
            foreach (var zone in boardState.Zones)
            {
                ResolvePendingBonuses(boardState, zone.PlayerCards, BoardSide.Player);
                ResolvePendingBonuses(boardState, zone.AICards, BoardSide.AI);
            }
        }

        private void ResolvePendingBonuses(BoardState boardState,
            List<CardInstance> cards, BoardSide side)
        {
            foreach (var card in cards)
            {
                if (card.PendingBonusTurn <= 0) continue;

                // El bonus se arma en el turno N y se resuelve al final del N+1.
                if (card.PendingBonusTurn >= boardState.CurrentTurn) continue;

                bool met = card.PendingBonusTurn == boardState.CurrentTurn - 1 &&
                    WasAllyPlayedInZone(boardState, side, boardState.CurrentTurn,
                        card.PendingBonusZoneIndex);

                if (met)
                    card.BonusPower += card.PendingBonusAmount;

                ClearPending(card);
            }
        }

        private static bool WasAllyPlayedInZone(BoardState boardState, BoardSide side,
            int turnNumber, int zoneIndex)
        {
            if (zoneIndex < 0) return false;

            foreach (var zone in boardState.Zones)
            {
                var cards = side == BoardSide.Player ? zone.PlayerCards : zone.AICards;
                foreach (var card in cards)
                {
                    if (card.PlayedTurn == turnNumber &&
                        card.PlayedZoneIndex == zoneIndex)
                        return true;
                }
            }

            return false;
        }

        private static void ClearPending(CardInstance card)
        {
            card.PendingBonusTurn = 0;
            card.PendingBonusZoneIndex = -1;
            card.PendingBonusAmount = 0;
        }

        /// <summary>Suma los bonus Ongoing que afectan al poder de una carta.</summary>
        public int GetCardPowerBonus(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner)
        {
            if (card?.Definition?.EffectIds == null || zone == null) return 0;

            var context = CreateContext(card, zone, boardState, owner);
            int total = 0;

            foreach (var effectId in card.Definition.EffectIds)
            {
                var effect = GetEffect(effectId);
                if (effect is ICardPowerModifier modifier)
                    total += modifier.GetPowerBonus(context);
            }

            return total;
        }

        /// <summary>Producto de los multiplicadores Ongoing del lado en la zona.</summary>
        public int GetZoneMultiplier(ZoneInstance zone, BoardSide owner, BoardState boardState)
        {
            if (zone == null) return 1;

            var cards = owner == BoardSide.Player ? zone.PlayerCards : zone.AICards;
            int multiplier = 1;

            foreach (var card in cards)
            {
                if (card?.Definition?.EffectIds == null) continue;

                var context = CreateContext(card, zone, boardState, owner);
                foreach (var effectId in card.Definition.EffectIds)
                {
                    var effect = GetEffect(effectId);
                    if (effect is IZonePowerMultiplier zoneMultiplier)
                        multiplier *= zoneMultiplier.GetMultiplier(context);
                }
            }

            return multiplier;
        }

        private EffectContext CreateContext(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner)
        {
            return new EffectContext
            {
                SourceCard = card,
                SourceZone = zone,
                Owner = owner,
                BoardState = boardState,
                TurnNumber = boardState?.CurrentTurn ?? 0,
                PriorityOwner = boardState?.PriorityOwner,
                CardRegistry = _cardRegistry
            };
        }

        private static ZoneInstance FindZone(BoardState boardState, CardInstance card)
        {
            foreach (var zone in boardState.Zones)
            {
                if (zone.PlayerCards.Contains(card) || zone.AICards.Contains(card))
                    return zone;
            }

            return null;
        }

        private static BoardSide FindOwner(BoardState boardState, CardInstance card)
        {
            foreach (var zone in boardState.Zones)
            {
                if (zone.PlayerCards.Contains(card)) return BoardSide.Player;
                if (zone.AICards.Contains(card)) return BoardSide.AI;
            }

            return BoardSide.Player;
        }
    }
}
