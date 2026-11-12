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
using SnapClone.Gameplay.Meta;

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
        private readonly CostResolver _costResolver;
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
            _costResolver = new CostResolver(this);
            Random = random;
        }

        /// <summary>Coste efectivo compartido por la UI, la IA y las acciones.</summary>
        public int GetEffectiveCost(CardInstance card, BoardSide side,
            BoardState boardState = null) =>
            _costResolver.Resolve(card, side, boardState ?? BoundBoard);

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

            ResolveRepeatOnReveal(boardState);
            RevealOrphanedDelayedCards(boardState);
            ResolveEndOfTurnEffects(boardState);
            ResolvePendingBonuses(boardState);
        }

        /// <summary>Resuelve Start of Turn antes de la fase de juego.</summary>
        public int ResolveStartOfTurn(BoardState boardState)
        {
            LastActionHitLimit = false;
            LastActionDispatched = 0;

            if (boardState == null) return 0;
            if (boardState.StartOfTurnResolvedTurn == boardState.CurrentTurn)
                return 0;

            boardState.StartOfTurnResolvedTurn = boardState.CurrentTurn;

            // Hot Pursuit y otros modificadores de coste de turno se aplican a
            // toda instancia para que la mano y la IA vean el coste real.
            if (boardState.NextTurnCostTaxTurn >= 0 &&
                boardState.NextTurnCostTaxTurn < boardState.CurrentTurn)
            {
                boardState.ClearNextTurnCostTax();
            }

            bool movedLastTurn = boardState.LastMoveTurn >= 0 &&
                                 boardState.LastMoveTurn == boardState.CurrentTurn - 1;
            int costTax = boardState.GetCostTaxForTurn(boardState.CurrentTurn);

            foreach (var card in AllCardsInContainers(boardState))
            {
                if (card == null) continue;
                card.TurnCostDelta = 0;
            }

            foreach (var card in AllCardsInContainers(boardState))
            {
                if (card == null) continue;

                if (movedLastTurn && PerkAbilities.HasEffect(card,
                        PerkCatalog.EffectHotPursuit))
                {
                    card.TurnCostDelta = -3;
                }
            }

            foreach (var card in AllCardsInHandOrDeck(boardState))
            {
                if (card == null || costTax <= 0) continue;

                card.TurnCostDelta += costTax;
                card.CostTaxAppliedTurn = boardState.CurrentTurn;
            }

            ResolvePendingStartOfTurn(boardState);

            int dispatched = 0;
            foreach (var card in AllCardsInPlay(boardState))
            {
                if (card == null || card.AbilitiesDisabled) continue;

                var zone = FindZone(boardState, card);
                if (zone == null) continue;

                var owner = FindOwner(boardState, card);
                var context = CreateContext(card, zone, boardState, owner);

                dispatched += ApplyEffects(card, context, EffectTrigger.StartOfTurn, 1);
            }

            LastActionDispatched = dispatched;
            return dispatched;
        }

        private void ResolveEndOfTurnEffects(BoardState boardState)
        {
            if (boardState.EndOfTurnResolvedTurn == boardState.CurrentTurn)
                return;

            boardState.EndOfTurnResolvedTurn = boardState.CurrentTurn;

            int dispatched = 0;
            foreach (var card in AllCardsInPlay(boardState))
            {
                if (card == null || card.AbilitiesDisabled) continue;

                var zone = FindZone(boardState, card);
                if (zone == null) continue;

                var owner = FindOwner(boardState, card);
                var context = CreateContext(card, zone, boardState, owner);

                int repeats = 1 + CountRepeatHook(card, typeof(IRepeatEndOfTurn));
                dispatched += ApplyEffects(card, context,
                    EffectTrigger.EndOfTurn, repeats);
            }

            // Algunas cartas de Fin de turno viven en la mano (Black Cat).
            ApplyHandEndOfTurn(boardState, BoardSide.Player, ref dispatched);
            ApplyHandEndOfTurn(boardState, BoardSide.AI, ref dispatched);

            LastActionDispatched += dispatched;
        }

        private void ApplyHandEndOfTurn(BoardState boardState, BoardSide side,
            ref int dispatched)
        {
            var hand = side == BoardSide.Player
                ? boardState.PlayerHand
                : boardState.AIHand;

            foreach (var card in hand.ToList())
            {
                if (card == null || card.AbilitiesDisabled) continue;

                var context = CreateContext(card, null, boardState, side);
                dispatched += ApplyEffects(card, context,
                    EffectTrigger.EndOfTurn, 1);
            }
        }

        /// <summary>Overcharge: consume la condicion armada al inicio del turno.</summary>
        private void ResolvePendingStartOfTurn(BoardState boardState)
        {
            foreach (var card in AllCardsInContainers(boardState))
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

                    if (pending.Kind != PendingEffectKind.WinZoneNextTurnEnergy)
                        continue;

                    if (pending.ArmedTurn >= boardState.CurrentTurn)
                        continue;

                    var side = FindOwner(boardState, card);
                    var zone = pending.ZoneIndex >= 0 &&
                               pending.ZoneIndex < boardState.Zones.Count
                        ? boardState.Zones[pending.ZoneIndex]
                        : FindZone(boardState, card);

                    bool winning = zone != null &&
                        (side == BoardSide.Player
                            ? zone.GetPlayerPower() > zone.GetAIPower()
                            : zone.GetAIPower() > zone.GetPlayerPower());

                    if (winning)
                        Actions?.AddEnergy(side, Math.Max(1, pending.Amount));

                    card.PendingEffects.RemoveAt(i);
                }
            }
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
                bool empower = current.Kind == ActionEventKind.Empowered;

                foreach (var card in reactors)
                {
                    if (card == null || card.AbilitiesDisabled) continue;

                    var zone = FindZone(boardState, card);
                    var owner = FindOwner(boardState, card);
                    var context = CreateContext(card, zone, boardState, owner, current,
                        next => queue.Enqueue(next));

                    // Reinforce es una reaccion por anfitrion, no una regla de lado.
                    if (empower)
                    {
                        if (!ReferenceEquals(current.SourceCard, card)) continue;
                        ApplyEmpoweredReactors(card, context, current.Amount);
                        continue;
                    }

                    ApplyEffects(card, context, TriggerFor(current.Kind), 1);
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

            if (HasInvisibleWoman(zone) &&
                !PerkAbilities.HasEffect(card, "effect_invisible_woman"))
            {
                card.DelayedReveal = true;
                return;
            }

            card.HasRevealed = true;
            card.RevealedTurn = boardState.CurrentTurn;
            ApplyRevealEffects(boardState, card, zone);
        }

        private bool HasInvisibleWoman(ZoneInstance zone)
        {
            if (zone == null) return false;

            foreach (var card in zone.PlayerCards.Concat(zone.AICards))
            {
                if (card == null || card.AbilitiesDisabled) continue;
                if (PerkAbilities.HasEffect(card, "effect_invisible_woman"))
                    return true;
            }

            return false;
        }

        private void RevealOrphanedDelayedCards(BoardState boardState)
        {
            foreach (var card in AllCardsInPlay(boardState).ToList())
            {
                if (card == null || !card.DelayedReveal) continue;

                var zone = FindZone(boardState, card);
                if (zone == null || HasInvisibleWoman(zone)) continue;

                card.DelayedReveal = false;
                if (card.HasRevealed) continue;

                card.HasRevealed = true;
                card.RevealedTurn = boardState.CurrentTurn;
                ApplyRevealEffects(boardState, card, zone);
            }
        }

        /// <summary>Resuelve el fin de partida: revelados diferidos y EndOfGame.</summary>
        public int ResolveEndOfGame(BoardState boardState)
        {
            if (boardState == null) return 0;

            foreach (var card in AllCardsInPlay(boardState).ToList())
            {
                if (card == null || !card.DelayedReveal) continue;

                var zone = FindZone(boardState, card);
                if (zone == null) continue;

                card.DelayedReveal = false;
                if (!card.HasRevealed)
                {
                    card.HasRevealed = true;
                    card.RevealedTurn = boardState.CurrentTurn;
                    ApplyRevealEffects(boardState, card, zone);
                }
            }

            int dispatched = 0;
            foreach (var card in AllCardsInPlay(boardState).ToList())
            {
                if (card == null || card.AbilitiesDisabled) continue;

                var zone = FindZone(boardState, card);
                if (zone == null) continue;

                var owner = FindOwner(boardState, card);
                var context = CreateContext(card, zone, boardState, owner);
                dispatched += ApplyEffects(card, context,
                    EffectTrigger.EndOfGame, 1);
            }

            return dispatched;
        }

        /// <summary>Revela una carta que entro al tablero por un efecto, como Jubilee.</summary>
        public void RevealAddedCard(BoardState boardState, CardInstance card)
        {
            if (card == null || card.HasRevealed) return;

            var zone = FindZone(boardState, card);
            if (zone == null) return;

            card.HasRevealed = true;
            card.RevealedTurn = boardState.CurrentTurn;
            ApplyRevealEffects(boardState, card, zone);
        }

        private void ApplyRevealEffects(BoardState boardState, CardInstance card,
            ZoneInstance zone)
        {
            if (card.AbilitiesDisabled) return;
            if (HasActiveCosmo(zone)) return;

            // Silence apaga el texto impreso desde su propio reveal; los perks
            // adjuntos siguen resolviendose.
            if (PerkAbilities.HasEffect(card, PerkCatalog.EffectSilence))
                card.PrintedAbilitiesDisabled = true;

            var owner = FindOwner(boardState, card);
            var context = CreateContext(card, zone, boardState, owner);

            int repeats = 1 + CountRepeatHook(card, typeof(IRepeatOnReveal));
            ApplyEffects(card, context, EffectTrigger.OnReveal, repeats);

            if (card.PendingDoublePower)
            {
                card.PendingDoublePower = false;
                card.DoublePower();
            }
        }

        /// <summary>
        /// Odin: tras la secuencia normal de reveals, repite una vez los OnReveal
        /// aliados ya revelados en su zona. No repite a otros repetidores ni a si
        /// mismo, y respeta Cosmo y las habilidades desactivadas.
        /// </summary>
        private void ResolveRepeatOnReveal(BoardState boardState)
        {
            if (boardState == null) return;

            var repeaters = new List<CardInstance>();
            foreach (var card in AllCardsInPlay(boardState))
            {
                if (card == null || !card.HasRevealed || card.AbilitiesDisabled)
                    continue;
                if (!HasRepeatZoneHook(card)) continue;
                if (card.RevealedTurn != boardState.CurrentTurn &&
                    !IsPersistentRepeatHook(card))
                    continue;
                repeaters.Add(card);
            }

            foreach (var repeater in repeaters)
            {
                var zone = FindZone(boardState, repeater);
                if (zone == null || HasActiveCosmo(zone)) continue;

                var owner = FindOwner(boardState, repeater);
                var targets = new List<CardInstance>();

                foreach (var card in zone.CardsFor(owner))
                {
                    if (ReferenceEquals(card, repeater)) continue;
                    if (card == null || !card.HasRevealed || card.AbilitiesDisabled)
                        continue;
                    if (card.RevealedTurn != boardState.CurrentTurn)
                        continue;
                    if (HasRepeatZoneHook(card)) continue;

                    targets.Add(card);
                }

                foreach (var target in targets)
                {
                    var context = CreateContext(target, zone, boardState, owner);
                    ApplyEffects(target, context, EffectTrigger.OnReveal, 1);
                }
            }
        }

        private bool HasRepeatZoneHook(CardInstance card)
        {
            if (card == null) return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                if (GetEffect(effectId) is IRepeatOnRevealAtLocation)
                    return true;
            }

            return false;
        }

        private bool IsPersistentRepeatHook(CardInstance card)
        {
            if (card == null) return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect is IRepeatOnRevealAtLocation &&
                    effect.Trigger == EffectTrigger.Ongoing)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Aplica los efectos efectivos de una carta para un disparador. Cada
        /// copia de un perk aporta su efecto una vez; la repeticion meta se
        /// limita a las copias adjuntas y nunca se aplica sobre si misma.
        /// </summary>
        private int ApplyEffects(CardInstance card, EffectContext context,
            EffectTrigger trigger, int repeats)
        {
            if (card == null || context == null) return 0;
            if (trigger == EffectTrigger.Passive) return 0;

            int applied = 0;
            var effectIds = PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled);

            for (int repeat = 0; repeat < repeats; repeat++)
            {
                foreach (string effectId in effectIds)
                {
                    var effect = GetEffect(effectId);
                    if (effect == null || effect.Trigger != trigger) continue;

                    // Las meta-habilidades no se repiten a si mismas.
                    if (repeat > 0 && IsRepeatHook(effect)) continue;

                    effect.Apply(context);
                    applied++;
                }
            }

            return applied;
        }

        private static bool IsRepeatHook(IEffect effect) =>
            effect is IRepeatOngoing || effect is IRepeatOnReveal ||
            effect is IRepeatEndOfTurn;

        private int CountRepeatHook(CardInstance card, Type hookType)
        {
            if (card?.AttachedPerks == null) return 0;

            int count = 0;
            foreach (string perkId in card.AttachedPerks)
            {
                foreach (string effectId in PerkCatalog.EffectIdsFor(perkId))
                {
                    var effect = GetEffect(effectId);
                    if (effect != null && hookType.IsInstanceOfType(effect))
                        count++;
                }
            }

            return count;
        }

        private int ApplyEmpoweredReactors(CardInstance card, EffectContext context,
            int amount)
        {
            if (card == null || amount <= 0) return 0;

            int applied = 0;
            var effectIds = PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled);

            foreach (string effectId in effectIds)
            {
                var effect = GetEffect(effectId);
                if (effect is IEmpoweredReactor reactor)
                {
                    reactor.OnEmpowered(context, amount);
                    applied++;
                }
            }

            return applied;
        }

        /// <summary>
        /// Dispara Empowered sobre la carta potenciada. Reinforce agrega su +1
        /// por copia sin volver a emitir el evento.
        /// </summary>
        public int NotifyEmpowered(CardInstance card, int amount)
        {
            if (BoundBoard == null || card == null || amount <= 0) return 0;
            if (card.AbilitiesDisabled) return 0;

            var zone = FindZone(BoundBoard, card);
            var owner = FindOwner(BoundBoard, card);
            var context = CreateContext(card, zone, BoundBoard, owner);
            return ApplyEmpoweredReactors(card, context, amount);
        }

        private bool HasActiveCosmo(ZoneInstance zone)
        {
            if (zone == null) return false;

            foreach (var card in zone.PlayerCards.Concat(zone.AICards))
            {
                if (!card.HasRevealed || card.AbilitiesDisabled) continue;

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                    !card.PrintedAbilitiesDisabled))
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
                if (other.AbilitiesDisabled) continue;

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(other,
                    !other.PrintedAbilitiesDisabled))
                {
                    if (GetEffect(effectId) is DestroyProtectionEffect)
                        return true;
                }
            }

            return false;
        }

        public bool IsZonePlayLocked(ZoneInstance zone) =>
            HasPlayZoneLock(zone) || HasFullZoneLock(zone);

        public bool IsZoneRemovalLocked(ZoneInstance zone) => HasFullZoneLock(zone);

        private bool HasFullZoneLock(ZoneInstance zone)
        {
            if (zone == null) return false;

            foreach (var card in zone.PlayerCards.Concat(zone.AICards))
            {
                if (card.AbilitiesDisabled) continue;

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                    !card.PrintedAbilitiesDisabled))
                {
                    if (GetEffect(effectId) is ZoneLockEffect)
                        return true;
                }
            }

            return false;
        }

        private bool HasPlayZoneLock(ZoneInstance zone)
        {
            if (zone == null) return false;

            foreach (var card in zone.PlayerCards.Concat(zone.AICards))
            {
                if (card.AbilitiesDisabled) continue;

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                    !card.PrintedAbilitiesDisabled))
                {
                    if (GetEffect(effectId) is PlayLockEffect)
                        return true;
                }
            }

            return false;
        }

        /// <summary>Resuelve los efectos Activate de una carta.</summary>
        public int ResolveActivate(BoardState boardState, CardInstance card)
        {
            if (boardState == null || card == null || card.AbilitiesDisabled)
                return 0;

            var zone = FindZone(boardState, card);
            if (zone == null) return 0;

            var owner = FindOwner(boardState, card);
            var context = CreateContext(card, zone, boardState, owner);
            return ApplyEffects(card, context, EffectTrigger.Activate, 1);
        }

        /// <summary>True si la carta tiene una habilidad Activar sin usar.</summary>
        public bool CanActivate(CardInstance card)
        {
            if (card == null || !card.IsInPlay || card.AbilitiesDisabled ||
                card.ActivatedThisCombat)
                return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect != null && effect.Trigger == EffectTrigger.Activate)
                    return true;
            }

            return false;
        }

        /// <summary>Consulta unica de legalidad de una carta en mano.</summary>
        public bool CanPlayCard(CardInstance card, BoardState boardState,
            BoardSide side) =>
            CanPlayCard(card, boardState, side, null);

        /// <summary>Consulta de legalidad con la zona de destino incluida.</summary>
        public bool CanPlayCard(CardInstance card, BoardState boardState,
            BoardSide side, ZoneInstance targetZone)
        {
            if (card == null || boardState == null) return false;

            var context = CreateContext(card, targetZone, boardState, side);
            context.TargetZone = targetZone;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect is IPlayRestriction restriction &&
                    !restriction.IsPlayable(context))
                    return false;
            }

            return true;
        }

        /// <summary>True cuando un efecto de lado limita a una carta por turno.</summary>
        public bool IsSideLimitedToOneCard(BoardState boardState, BoardSide side) =>
            HasSideEffect(boardState, side, "effect_electro");

        /// <summary>Goose: nadie juega cartas de coste 4 o mas en su zona.</summary>
        public bool IsBlockedByGoose(ZoneInstance zone, CardInstance card,
            BoardSide side)
        {
            if (zone == null || card == null) return false;

            foreach (var other in zone.PlayerCards.Concat(zone.AICards))
            {
                if (other == null || other.AbilitiesDisabled) continue;
                if (!PerkAbilities.HasEffect(other, "effect_goose")) continue;

                int cost = GetEffectiveCost(card, side, zone.Board);
                return cost >= 4;
            }

            return false;
        }

        /// <summary>Zona forzada por Jean Grey para la primera carta del turno.</summary>
        public int GetForcedZone(BoardState boardState, BoardSide side)
        {
            if (boardState == null) return -1;
            if (boardState.GetPlayedCount(side, boardState.CurrentTurn) > 0)
                return -1;

            foreach (var zone in boardState.Zones)
            {
                foreach (var card in zone.CardsFor(side))
                {
                    if (card == null || card.AbilitiesDisabled) continue;
                    if (!PerkAbilities.HasEffect(card, "effect_jean_grey")) continue;
                    if (zone.CanAdd(side)) return zone.ZoneIndex;
                }
            }

            return -1;
        }

        public bool HasSideEffect(BoardState boardState, BoardSide side,
            string effectId)
        {
            if (boardState == null || string.IsNullOrWhiteSpace(effectId))
                return false;

            foreach (var zone in boardState.Zones)
            {
                foreach (var card in zone.CardsFor(side))
                {
                    if (card == null || card.AbilitiesDisabled) continue;
                    if (PerkAbilities.HasEffect(card, effectId)) return true;
                }
            }

            return false;
        }

        /// <summary>Permiso de movimiento impreso de una carta.</summary>
        public MovePermission GetMovePermission(CardInstance card)
        {
            if (card == null || card.AbilitiesDisabled)
                return MovePermission.None;

            MovePermission result = MovePermission.None;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect is IMovePermission permission)
                {
                    if (permission.Frequency == MovePermission.OncePerTurn)
                        return MovePermission.OncePerTurn;

                    result = permission.Frequency;
                }
                else if (effect is IPlayerMovePermission)
                {
                    result = MovePermission.OncePerCombat;
                }
            }

            return result;
        }

        public bool HasOngoing(CardInstance card)
        {
            if (card == null) return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect != null && effect.Trigger == EffectTrigger.Ongoing)
                    return true;
            }

            return false;
        }

        public bool HasTrigger(CardInstance card, EffectTrigger trigger)
        {
            if (card == null) return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect != null && effect.Trigger == trigger)
                    return true;
            }

            return false;
        }

        public bool IsDestroyImmune(CardInstance card) =>
            card != null && card.IsInPlay && !card.AbilitiesDisabled &&
            HasMarker<IDestroyImmune>(card);

        public bool NegatesPowerReduction(CardInstance card)
        {
            if (card == null) return false;

            if (card.IsInPlay && !card.AbilitiesDisabled &&
                HasMarker<IPowerReductionImmune>(card))
                return true;

            if (!card.IsInPlay || BoundBoard == null) return false;

            var owner = FindOwner(BoundBoard, card);
            foreach (var zone in BoundBoard.Zones)
            {
                foreach (var ally in zone.CardsFor(owner))
                {
                    if (ally == null || ally.AbilitiesDisabled) continue;
                    if (PerkAbilities.EffectiveEffectIds(ally,
                        !ally.PrintedAbilitiesDisabled).Contains("effect_luke_cage"))
                        return true;
                }
            }

            return false;
        }

        public bool AllowsAnyZone(CardInstance card) =>
            !card.AbilitiesDisabled && HasMarker<IPlayAnywhere>(card);

        public bool HasPlayerMovePermission(CardInstance card) =>
            !card.AbilitiesDisabled && HasMarker<IPlayerMovePermission>(card);

        private bool HasMarker<T>(CardInstance card) where T : class
        {
            if (card == null) return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                if (GetEffect(effectId) is T)
                    return true;
            }

            return false;
        }

        /// <summary>Reduccion de coste de las habilidades del anfitrion.</summary>
        public int GetCostReduction(CardInstance card, BoardState boardState,
            BoardSide owner)
        {
            if (card == null || boardState == null || card.AbilitiesDisabled)
                return 0;

            var zone = FindZone(boardState, card);
            var context = CreateContext(card, zone, boardState, owner);

            int total = 0;
            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect is ICardCostModifier modifier)
                    total += modifier.GetCostReduction(context);
            }

            return total;
        }

        /// <summary>Multiplicador final de Poder (Apex).</summary>
        public int GetPowerMultiplier(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner)
        {
            if (card == null || zone == null || card.AbilitiesDisabled) return 1;

            var context = CreateContext(card, zone, boardState, owner);
            int multiplier = 1;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = GetEffect(effectId);
                if (effect is IPowerDoubler doubler && doubler.ShouldDouble(context))
                    multiplier *= 2;
            }

            return multiplier;
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

                    bool playedInZone = WasAllyPlayedInZone(boardState, side,
                        boardState.CurrentTurn, pending.ZoneIndex);

                    bool met = pending.ArmedTurn == boardState.CurrentTurn - 1 &&
                        ((pending.Kind == PendingEffectKind.AllyPlayedInZoneNextTurn &&
                          playedInZone) ||
                         (pending.Kind == PendingEffectKind.AllyNotPlayedInZoneNextTurn &&
                          !playedInZone));

                    if (met)
                    {
                        if (Actions != null)
                            Actions.AddBonusPower(card, pending.Amount);
                        else
                            card.BonusPower += pending.Amount;
                    }

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
            if (card == null || zone == null) return 0;

            int total = 0;
            bool immuneToReduction = NegatesPowerReduction(card);

            if (!card.AbilitiesDisabled)
            {
                int selfRepeat = GetRepeatCount(card, owner, zone, boardState);
                var context = CreateContext(card, zone, boardState, owner);

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                    !card.PrintedAbilitiesDisabled))
                {
                    var effect = GetEffect(effectId);
                    if (effect == null || effect.Trigger != EffectTrigger.Ongoing)
                        continue;
                    if (effect is ICardPowerModifier modifier)
                    {
                        int bonus = modifier.GetPowerBonus(context) * selfRepeat;
                        if (immuneToReduction && bonus < 0) continue;
                        total += bonus;
                    }
                }

                foreach (var source in AllCardsInPlay(boardState))
                {
                    if (source.AbilitiesDisabled) continue;

                    var sourceZone = FindZone(boardState, source);
                    if (sourceZone == null) continue;

                    var sourceSide = FindOwner(boardState, source);
                    int repeat = GetRepeatCount(source, sourceSide, sourceZone, boardState);
                    var sourceContext = CreateContext(source, sourceZone, boardState, sourceSide);

                    foreach (var effectId in PerkAbilities.EffectiveEffectIds(source,
                        !source.PrintedAbilitiesDisabled))
                    {
                        var effect = GetEffect(effectId);
                        if (effect is ICardAura aura)
                        {
                            int bonus = aura.GetBonusForCard(sourceContext, card,
                                zone, owner) * repeat;
                            if (immuneToReduction && bonus < 0) continue;
                            total += bonus;
                        }
                    }
                }
            }

            return total;
        }

        public int GetZoneCardPowerBonus(ZoneInstance zone, CardInstance card,
            BoardSide owner)
        {
            if (zone?.Definition?.EffectIds == null) return 0;
            if (zone.Board != null &&
                zone.Board.AreZoneEffectsSuppressed(
                    zone.Board.CurrentTurn))
                return 0;

            int total = 0;
            foreach (var effectId in zone.Definition.EffectIds)
            {
                var effect = GetEffect(effectId);
                if (effect is IZoneCardPowerModifier modifier)
                    total += modifier.GetBonusForCard(zone, card, owner);
            }

            if (total < 0 && NegatesPowerReduction(card)) return 0;
            return total;
        }

        /// <summary>Producto de los multiplicadores Ongoing del lado en la zona.</summary>
        public int GetZoneMultiplier(ZoneInstance zone, BoardSide owner, BoardState boardState)
        {
            if (zone == null) return 1;

            int multiplier = 1;
            foreach (var card in zone.CardsFor(owner))
            {
                if (card == null || card.AbilitiesDisabled)
                    continue;

                int repeat = GetRepeatCount(card, owner, zone, boardState);
                var context = CreateContext(card, zone, boardState, owner);

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                    !card.PrintedAbilitiesDisabled))
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
                if (source.AbilitiesDisabled) continue;

                var sourceZone = FindZone(boardState, source);
                if (sourceZone == null) continue;

                var sourceSide = FindOwner(boardState, source);
                int repeat = GetRepeatCount(source, sourceSide, sourceZone, boardState);
                var sourceContext = CreateContext(source, sourceZone, boardState, sourceSide);

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(source,
                    !source.PrintedAbilitiesDisabled))
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
                if (card.AbilitiesDisabled) continue;

                foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                    !card.PrintedAbilitiesDisabled))
                {
                    if (GetEffect(effectId) is OngoingRepeatEffect)
                    {
                        onslaughts++;
                        break;
                    }
                }
            }

            int baseRepeat = onslaughts <= 0
                ? 1
                : 1 << System.Math.Min(onslaughts, 2);

            // Amplify suma una repeticion por copia; nunca se repite a si mismo.
            return baseRepeat + CountRepeatHook(source, typeof(IRepeatOngoing));
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

        /// <summary>Instancias en mano y mazo; el impuesto de coste solo aplica aqui.</summary>
        private static List<CardInstance> AllCardsInHandOrDeck(BoardState boardState)
        {
            var cards = new List<CardInstance>();
            if (boardState == null) return cards;

            cards.AddRange(boardState.PlayerHand);
            cards.AddRange(boardState.AIHand);
            cards.AddRange(boardState.PlayerDeck);
            cards.AddRange(boardState.AIDeck);
            return cards;
        }

        /// <summary>Todas las instancias del tablero, manos, mazos y pilas.</summary>
        private static List<CardInstance> AllCardsInContainers(BoardState boardState)
        {
            var cards = new List<CardInstance>();
            if (boardState == null) return cards;

            cards.AddRange(AllCardsInPlay(boardState));
            cards.AddRange(boardState.PlayerHand);
            cards.AddRange(boardState.AIHand);
            cards.AddRange(boardState.PlayerDeck);
            cards.AddRange(boardState.AIDeck);

            foreach (PileKind kind in new[]
                     { PileKind.Discard, PileKind.Destroyed, PileKind.Banished })
            {
                cards.AddRange(boardState.PlayerPiles.Get(kind));
                cards.AddRange(boardState.AIPiles.Get(kind));
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
                case ActionEventKind.AddedToHand: return EffectTrigger.AfterAddedToHand;
                case ActionEventKind.GameStart: return EffectTrigger.GameStart;
                case ActionEventKind.Empowered: return EffectTrigger.Empowered;
                case ActionEventKind.Activated: return EffectTrigger.Activate;
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
