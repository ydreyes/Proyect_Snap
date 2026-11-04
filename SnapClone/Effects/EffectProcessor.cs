using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Registries;
using SnapClone.Data.Definitions;
using SnapClone.Effects.Implementations;
using SnapClone.Gameplay.Actions;
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
        public const int MaxEventsPerAction = 256;

        private readonly EffectRegistry _effectRegistry;
        private readonly CardRegistry _cardRegistry;
        private readonly Dictionary<string, IEffect> _effects =
            new Dictionary<string, IEffect>();

        public BoardState BoundBoard { get; set; }
        public ICardEffectActions Actions { get; set; }
        public RandomService Random { get; set; }

        public bool LastActionHitLimit { get; private set; }
        public int LastActionDispatched { get; private set; }

        public EffectProcessor(EffectRegistry effectRegistry, CardRegistry cardRegistry = null,
            RandomService random = null)
        {
            _effectRegistry = effectRegistry;
            _cardRegistry = cardRegistry;
            Random = random;
        }

        public void RegisterEffect(string effectId, IEffect effect)
        {
            if (string.IsNullOrEmpty(effectId) || effect == null) return;
            _effects[effectId] = effect;
        }

        public IEffect GetEffect(string effectId)
        {
            if (string.IsNullOrEmpty(effectId)) return null;

            if (_effects.TryGetValue(effectId, out IEffect cached))
                return cached;

            if (_effectRegistry == null) return null;

            var definition = _effectRegistry.Get(effectId);
            if (definition == null) return null;

            var effect = BaseEffectFactory.CreateEffect(definition);
            if (effect != null)
                _effects[effectId] = effect;

            return effect;
        }

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

        public int Dispatch(ActionEvent actionEvent) =>
            Dispatch(BoundBoard, actionEvent);

        public int Dispatch(BoardState boardState, ActionEvent actionEvent)
        {
            LastActionHitLimit = false;
            LastActionDispatched = 0;

            if (boardState == null || actionEvent == null) return 0;

            var queue = new Queue<ActionEvent>();
            queue.Enqueue(actionEvent);
            int dispatched = 0;

            while (queue.Count > 0)
            {
                if (dispatched >= MaxEventsPerAction)
                {
                    LastActionHitLimit = true;
                    break;
                }

                var current = queue.Dequeue();
                dispatched++;

                var reactors = BuildReactorOrder(boardState, current);
                foreach (var card in reactors)
                {
                    if (card?.Definition?.EffectIds == null) continue;
                    if (card.AbilitiesDisabled) continue;

                    var zone = FindZone(boardState, card);
                    var owner = FindOwner(boardState, card);
                    var context = CreateContext(card, zone, boardState, owner, current,
                        next => queue.Enqueue(next));

                    foreach (var effectId in card.Definition.EffectIds)
                    {
                        var effect = GetEffect(effectId);
                        if (effect == null || effect.Trigger != TriggerFor(current.Kind))
                            continue;

                        effect.Apply(context);
                    }
                }
            }

            LastActionDispatched = dispatched;
            return dispatched;
        }

        public EffectContext CreateContext(BoardState boardState, CardInstance card,
            BoardSide side)
        {
            var zone = FindZone(boardState, card);
            return CreateContext(card, zone, boardState, side, null, null);
        }

        private void ApplyOnReveal(BoardState boardState, CardInstance card)
        {
            if (card == null || card.HasRevealed) return;

            var zone = FindZone(boardState, card);
            if (zone == null) return;

            card.HasRevealed = true;

            if (card.Definition?.EffectIds == null || card.AbilitiesDisabled) return;
            if (HasActiveCosmo(zone)) return;

            var owner = FindOwner(boardState, card);
            var context = CreateContext(card, zone, boardState, owner);

            foreach (var effectId in card.Definition.EffectIds)
            {
                var effect = GetEffect(effectId);
                if (effect == null || effect.Trigger != EffectTrigger.OnReveal) continue;

                effect.Apply(context);
            }
        }

        private bool HasActiveCosmo(ZoneInstance zone)
        {
            if (zone == null) return false;

            foreach (var card in zone.PlayerCards.Concat(zone.AICards))
            {
                if (!card.HasRevealed || card.AbilitiesDisabled) continue;
                if (card.Definition?.EffectIds == null) continue;

                foreach (var effectId in card.Definition.EffectIds)
                {
                    if (GetEffect(effectId) is SuppressOnRevealEffect)
                        return true;
                }
            }

            return false;
        }

        public bool IsCardProtectedFromDestroy(CardInstance card, BoardSide side)
        {
            var zone = FindZone(BoundBoard, card);
            if (zone == null) return false;

            foreach (var other in zone.PlayerCards.Concat(zone.AICards))
            {
                if (other.AbilitiesDisabled || other.Definition?.EffectIds == null)
                    continue;

                foreach (var effectId in other.Definition.EffectIds)
                {
                    if (GetEffect(effectId) is DestroyProtectionEffect)
                        return true;
                }
            }

            return false;
        }

        public bool IsZonePlayLocked(ZoneInstance zone) => HasZoneLock(zone);

        public bool IsZoneRemovalLocked(ZoneInstance zone) => HasZoneLock(zone);

        private bool HasZoneLock(ZoneInstance zone)
        {
            if (zone == null) return false;

            foreach (var card in zone.PlayerCards.Concat(zone.AICards))
            {
                if (card.AbilitiesDisabled || card.Definition?.EffectIds == null)
                    continue;

                foreach (var effectId in card.Definition.EffectIds)
                {
                    if (GetEffect(effectId) is ZoneLockEffect)
                        return true;
                }
            }

            return false;
        }

        public bool HasOngoing(CardInstance card)
        {
            if (card?.Definition?.EffectIds == null) return false;

            foreach (var effectId in card.Definition.EffectIds)
            {
                var effect = GetEffect(effectId);
                if (effect != null && effect.Trigger == EffectTrigger.Ongoing)
                    return true;
            }

            return false;
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
                if (card.PendingEffects == null || card.PendingEffects.Count == 0)
                    continue;

                for (int i = card.PendingEffects.Count - 1; i >= 0; i--)
                {
                    var pending = card.PendingEffects[i];
                    if (pending == null)
                    {
                        card.PendingEffects.RemoveAt(i);
                        continue;
                    }

                    if (pending.ArmedTurn >= boardState.CurrentTurn)
                        continue;

                    bool met = pending.Kind == PendingEffectKind.AllyPlayedInZoneNextTurn &&
                        pending.ArmedTurn == boardState.CurrentTurn - 1 &&
                        WasAllyPlayedInZone(boardState, side, boardState.CurrentTurn,
                            pending.ZoneIndex);

                    if (met)
                        card.BonusPower += pending.Amount;

                    card.PendingEffects.RemoveAt(i);
                }
            }
        }

        private static bool WasAllyPlayedInZone(BoardState boardState, BoardSide side,
            int turnNumber, int zoneIndex)
        {
            if (zoneIndex < 0) return false;

            if (boardState.PlayLog != null && boardState.PlayLog.Count > 0)
            {
                foreach (var record in boardState.PlayLog)
                {
                    if (record.Side != side) continue;
                    if (record.Turn != turnNumber) continue;
                    if (record.ZoneIndex != zoneIndex) continue;
                    return true;
                }

                return false;
            }

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

        /// <summary>Suma self-bonuses, auras, efectos de zona y condiciones.</summary>
        public int GetCardPowerBonus(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner)
        {
            if (card?.Definition?.EffectIds == null || zone == null) return 0;

            int total = 0;

            if (!card.AbilitiesDisabled)
            {
                int selfRepeat = GetRepeatCount(card, owner, zone, boardState);
                var context = CreateContext(card, zone, boardState, owner);

                foreach (var effectId in card.Definition.EffectIds)
                {
                    var effect = GetEffect(effectId);
                    if (effect == null || effect.Trigger != EffectTrigger.Ongoing)
                        continue;
                    if (effect is ICardPowerModifier modifier)
                        total += modifier.GetPowerBonus(context) * selfRepeat;
                }

                foreach (var source in AllCardsInPlay(boardState))
                {
                    if (source.AbilitiesDisabled || source.Definition?.EffectIds == null)
                        continue;

                    var sourceZone = FindZone(boardState, source);
                    if (sourceZone == null) continue;

                    var sourceSide = FindOwner(boardState, source);
                    int repeat = GetRepeatCount(source, sourceSide, sourceZone, boardState);
                    var sourceContext = CreateContext(source, sourceZone, boardState, sourceSide);

                    foreach (var effectId in source.Definition.EffectIds)
                    {
                        var effect = GetEffect(effectId);
                        if (effect is ICardAura aura)
                            total += aura.GetBonusForCard(sourceContext, card, zone, owner) * repeat;
                    }
                }
            }

            return total;
        }

        public int GetZoneCardPowerBonus(ZoneInstance zone, CardInstance card,
            BoardSide owner)
        {
            if (zone?.Definition?.EffectIds == null) return 0;

            int total = 0;
            foreach (var effectId in zone.Definition.EffectIds)
            {
                var effect = GetEffect(effectId);
                if (effect is IZoneCardPowerModifier modifier)
                    total += modifier.GetBonusForCard(zone, card, owner);
            }

            return total;
        }

        /// <summary>Producto de los multiplicadores Ongoing del lado en la zona.</summary>
        public int GetZoneMultiplier(ZoneInstance zone, BoardSide owner, BoardState boardState)
        {
            if (zone == null) return 1;

            int multiplier = 1;
            foreach (var card in zone.CardsFor(owner))
            {
                if (card?.Definition?.EffectIds == null || card.AbilitiesDisabled)
                    continue;

                int repeat = GetRepeatCount(card, owner, zone, boardState);
                var context = CreateContext(card, zone, boardState, owner);

                foreach (var effectId in card.Definition.EffectIds)
                {
                    var effect = GetEffect(effectId);
                    if (effect == null || effect.Trigger != EffectTrigger.Ongoing)
                        continue;
                    if (effect is IZonePowerMultiplier zoneMultiplier)
                    {
                        int baseValue = zoneMultiplier.GetMultiplier(context);
                        multiplier *= (int)Math.Pow(baseValue, repeat);
                    }
                }
            }

            return multiplier;
        }

        /// <summary>Bonos aditivos de una carta a otra zona, como Klaw.</summary>
        public int GetZoneAddition(ZoneInstance targetZone, BoardSide targetSide,
            BoardState boardState)
        {
            if (targetZone == null || boardState == null) return 0;

            int total = 0;
            foreach (var source in AllCardsInPlay(boardState))
            {
                if (source.AbilitiesDisabled || source.Definition?.EffectIds == null)
                    continue;

                var sourceZone = FindZone(boardState, source);
                if (sourceZone == null) continue;

                var sourceSide = FindOwner(boardState, source);
                int repeat = GetRepeatCount(source, sourceSide, sourceZone, boardState);
                var sourceContext = CreateContext(source, sourceZone, boardState, sourceSide);

                foreach (var effectId in source.Definition.EffectIds)
                {
                    var effect = GetEffect(effectId);
                    if (effect is IZonePowerAddition addition)
                        total += addition.GetAdditionForZone(sourceContext, targetZone, targetSide) * repeat;
                }
            }

            return total;
        }

        private int GetRepeatCount(CardInstance source, BoardSide sourceSide,
            ZoneInstance sourceZone, BoardState boardState)
        {
            if (source == null || sourceZone == null) return 1;

            int onslaughts = 0;
            foreach (var card in sourceZone.CardsFor(sourceSide))
            {
                if (card.AbilitiesDisabled || card.Definition?.EffectIds == null)
                    continue;

                foreach (var effectId in card.Definition.EffectIds)
                {
                    if (GetEffect(effectId) is OngoingRepeatEffect)
                    {
                        onslaughts++;
                        break;
                    }
                }
            }

            if (onslaughts <= 0) return 1;
            if (onslaughts > 2) onslaughts = 2;
            return 1 << onslaughts;
        }

        private static List<CardInstance> AllCardsInPlay(BoardState boardState)
        {
            var cards = new List<CardInstance>();
            if (boardState == null) return cards;

            foreach (var zone in boardState.Zones)
            {
                cards.AddRange(zone.PlayerCards);
                cards.AddRange(zone.AICards);
            }

            return cards;
        }

        private static List<CardInstance> BuildReactorOrder(BoardState boardState,
            ActionEvent actionEvent)
        {
            var result = new List<CardInstance>();
            var seen = new HashSet<CardInstance>();

            if (actionEvent.Kind == ActionEventKind.GameStart)
            {
                AddSide(result, seen, boardState, actionEvent.Side);
                return result;
            }

            AddSide(result, seen, boardState, BoardSide.Player);
            AddSide(result, seen, boardState, BoardSide.AI);
            return result;
        }

        private static void AddSide(List<CardInstance> result,
            HashSet<CardInstance> seen, BoardState boardState, BoardSide side)
        {
            foreach (var zone in boardState.Zones)
                AddCards(result, seen, zone.CardsFor(side));

            AddCards(result, seen, side == BoardSide.Player
                ? boardState.PlayerHand
                : boardState.AIHand);

            AddCards(result, seen, side == BoardSide.Player
                ? boardState.PlayerDeck
                : boardState.AIDeck);

            var piles = boardState.GetPiles(side);
            AddCards(result, seen, piles.Get(PileKind.Discard));
            AddCards(result, seen, piles.Get(PileKind.Destroyed));
            AddCards(result, seen, piles.Get(PileKind.Banished));
        }

        private static void AddCards(List<CardInstance> result,
            HashSet<CardInstance> seen, IReadOnlyList<CardInstance> cards)
        {
            if (cards == null) return;

            foreach (var card in cards)
            {
                if (card == null || !seen.Add(card)) continue;
                result.Add(card);
            }
        }

        private static EffectTrigger TriggerFor(ActionEventKind kind)
        {
            switch (kind)
            {
                case ActionEventKind.Played: return EffectTrigger.AfterPlayed;
                case ActionEventKind.Moved: return EffectTrigger.AfterMoved;
                case ActionEventKind.Destroyed: return EffectTrigger.AfterDestroyed;
                case ActionEventKind.Discarded: return EffectTrigger.AfterDiscarded;
                case ActionEventKind.Drawn: return EffectTrigger.AfterDrawn;
                case ActionEventKind.GameStart: return EffectTrigger.GameStart;
                default: return EffectTrigger.OnReveal;
            }
        }

        private EffectContext CreateContext(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner)
        {
            return CreateContext(card, zone, boardState, owner, null, null);
        }

        private EffectContext CreateContext(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner, ActionEvent actionEvent,
            Action<ActionEvent> emit)
        {
            return new EffectContext
            {
                SourceCard = card,
                SourceZone = zone,
                Owner = owner,
                BoardState = boardState,
                TurnNumber = boardState?.CurrentTurn ?? 0,
                PriorityOwner = boardState?.PriorityOwner,
                CardRegistry = _cardRegistry,
                EffectRegistry = _effectRegistry,
                Event = actionEvent,
                EmitEvent = emit,
                Actions = Actions,
                Random = Random
            };
        }

        private static ZoneInstance FindZone(BoardState boardState, CardInstance card)
        {
            if (boardState == null || card == null) return null;

            foreach (var zone in boardState.Zones)
            {
                if (zone.PlayerCards.Contains(card) || zone.AICards.Contains(card))
                    return zone;
            }

            return null;
        }

        private static BoardSide FindOwner(BoardState boardState, CardInstance card)
        {
            if (boardState == null || card == null) return BoardSide.Player;

            foreach (var zone in boardState.Zones)
            {
                if (zone.PlayerCards.Contains(card)) return BoardSide.Player;
                if (zone.AICards.Contains(card)) return BoardSide.AI;
            }

            if (boardState.PlayerHand.Contains(card) ||
                boardState.PlayerDeck.Contains(card) ||
                boardState.PlayerPiles.Contains(card))
                return BoardSide.Player;

            if (boardState.AIHand.Contains(card) ||
                boardState.AIDeck.Contains(card) ||
                boardState.AIPiles.Contains(card))
                return BoardSide.AI;

            return BoardSide.Player;
        }
    }
}
