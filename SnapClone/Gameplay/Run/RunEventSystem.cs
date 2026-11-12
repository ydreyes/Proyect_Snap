using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Meta;

namespace SnapClone.Gameplay.Run
{
    public readonly struct EventResolution
    {
        public bool Success { get; }
        public bool Declined { get; }
        public string Error { get; }

        private EventResolution(bool success, bool declined, string error)
        {
            Success = success;
            Declined = declined;
            Error = error;
        }

        public static EventResolution Ok(bool declined = false) =>
            new EventResolution(true, declined, null);

        public static EventResolution Fail(string error) =>
            new EventResolution(false, false, error);
    }

    /// <summary>
    /// Elige y resuelve los 12 eventos de la run. Ninguno inicia combate:
    /// aceptar aplica una recompensa y declinar no cambia el estado.
    /// </summary>
    public class RunEventSystem
    {
        public const int MaxCardCopies = 2;
        public const int MaxPerkCopies = 2;
        public const int MaxXpReward = 30;

        private readonly RunEventRegistry _eventRegistry;
        private readonly CardRegistry _cardRegistry;
        private readonly PerkRegistry _perkRegistry;
        private readonly RandomService _random;
        private readonly RunModifierSystem _modifiers;
        private readonly PowerResolver _powerResolver;
        private readonly EffectRegistry _effectRegistry;

        public RunEventSystem(RunEventRegistry eventRegistry, CardRegistry cardRegistry,
            PerkRegistry perkRegistry, RandomService random,
            RunModifierSystem modifiers = null, PowerResolver powerResolver = null,
            EffectRegistry effectRegistry = null)
        {
            _eventRegistry = eventRegistry;
            _cardRegistry = cardRegistry;
            _perkRegistry = perkRegistry;
            _random = random;
            _modifiers = modifiers;
            _powerResolver = powerResolver;
            _effectRegistry = effectRegistry;
        }

        /// <summary>
        /// Elige un evento no visto y lo guarda en la run. Si ya hay uno
        /// guardado, la reentrada muestra exactamente el mismo.
        /// </summary>
        public RunEventDef EnsureEvent(RunState runState)
        {
            if (runState == null || _eventRegistry == null) return null;

            runState.EventState ??= new EventState();
            runState.SeenEventIds ??= new HashSet<string>();

            var current = GetCurrent(runState);
            if (current != null) return current;

            var all = _eventRegistry.GetAll();
            if (all.Count == 0) return null;

            var unseen = all
                .Where(runEvent => !runState.SeenEventIds.Contains(runEvent.Id))
                .ToList();

            var pool = unseen.Count > 0 ? unseen : all;
            var chosen = pool[_random.Next(pool.Count)];

            runState.EventState.EventId = chosen.Id;
            runState.EventState.Resolved = false;
            runState.SeenEventIds.Add(chosen.Id);
            return chosen;
        }

        public RunEventDef GetCurrent(RunState runState)
        {
            if (runState?.EventState == null) return null;
            return _eventRegistry?.Get(runState.EventState.EventId);
        }

        public bool IsResolved(RunState runState) =>
            runState?.EventState != null && runState.EventState.Resolved;

        /// <summary>
        /// Aceptar exige dinero suficiente y un modificador que la run no
        /// tenga. Declinar siempre se puede.
        /// </summary>
        public bool CanAccept(RunState runState)
        {
            var runEvent = GetCurrent(runState);
            if (runEvent == null || IsResolved(runState)) return false;
            if (runEvent.CostMoney > runState.Money) return false;

            if (runEvent.EffectKind == RunEventEffect.Modifier)
            {
                if (string.IsNullOrWhiteSpace(runEvent.ModifierId)) return false;
                if (_modifiers != null && !_modifiers.IsKnown(runEvent.ModifierId)) return false;
                if (_modifiers != null && _modifiers.Has(runState, runEvent.ModifierId))
                    return false;
            }

            return true;
        }

        public EventResolution Resolve(RunState runState, bool accept)
        {
            if (runState?.EventState == null)
                return EventResolution.Fail("No hay evento activo.");

            var runEvent = GetCurrent(runState);
            if (runEvent == null)
                return EventResolution.Fail("El evento no existe en el catalogo.");

            if (runState.EventState.Resolved)
                return EventResolution.Fail("El evento ya fue resuelto.");

            if (!accept)
            {
                runState.EventState.Resolved = true;
                return EventResolution.Ok(declined: true);
            }

            if (runEvent.CostMoney > runState.Money)
                return EventResolution.Fail("No alcanza el dinero para aceptar.");

            if (runEvent.EffectKind == RunEventEffect.Modifier)
            {
                if (string.IsNullOrWhiteSpace(runEvent.ModifierId) ||
                    (_modifiers != null && !_modifiers.IsKnown(runEvent.ModifierId)))
                    return EventResolution.Fail("El modificador del evento no existe.");

                if (_modifiers != null && _modifiers.Has(runState, runEvent.ModifierId))
                    return EventResolution.Fail("Ya tienes ese modificador.");
            }

            // El coste se descuenta una sola vez y nunca deja saldo negativo.
            if (runEvent.CostMoney > 0)
                runState.Money -= runEvent.CostMoney;

            if (runEvent.XpAmount > 0)
                runState.AddExperience(Math.Min(runEvent.XpAmount, MaxXpReward));

            if (runEvent.MoneyDelta != 0)
                ApplyMoneyDelta(runState, runEvent.MoneyDelta);

            switch (runEvent.EffectKind)
            {
                case RunEventEffect.Card:
                    GrantCard(runState);
                    break;
                case RunEventEffect.Upgrade:
                    GrantUpgrade(runState);
                    break;
                case RunEventEffect.Perk:
                    GrantPerk(runState);
                    break;
                case RunEventEffect.Modifier:
                    if (_modifiers == null || !_modifiers.TryAdd(runState, runEvent.ModifierId))
                        return EventResolution.Fail("El modificador fue rechazado.");
                    break;
                case RunEventEffect.CardAndUpgrade:
                    GrantCard(runState);
                    GrantUpgrade(runState);
                    break;
            }

            runState.EventState.Resolved = true;
            return EventResolution.Ok();
        }

        // ------------------------------------------------------------------
        // Recompensas
        // ------------------------------------------------------------------

        private static void ApplyMoneyDelta(RunState runState, int delta)
        {
            if (delta > 0)
            {
                runState.AddMoney(delta);
                return;
            }

            runState.Money = Math.Max(0, runState.Money + delta);
        }

        /// <summary>
        /// Agrega una carta al azar. Con el mazo en 12 se aplica la regla del
        /// tope: la recompensa de carta se convierte en una mejora.
        /// </summary>
        private bool GrantCard(RunState runState)
        {
            var candidates = _cardRegistry?.GetDraftable()?
                .Where(card => CountInDeck(runState, card.Id) < MaxCardCopies)
                .ToList() ?? new List<CardDef>();

            if (runState.Deck.Count >= RunState.MaxDeckSize || candidates.Count == 0)
                return GrantUpgrade(runState);

            var definition = candidates[_random.Next(candidates.Count)];
            var instance = new CardInstance(definition);
            _powerResolver?.ResolveCard(instance);
            runState.Deck.Add(instance);
            runState.DiscoverCard(definition.Id);
            return true;
        }

        private bool GrantUpgrade(RunState runState)
        {
            var candidates = runState.Deck
                .Where(card => card != null && card.CanUpgrade)
                .ToList();

            if (candidates.Count == 0) return false;

            var card = candidates[_random.Next(candidates.Count)];
            bool cost = card.CanUpgradeCost && _random.Next(2) == 0;

            bool applied = cost ? card.TryUpgradeCost() : card.TryUpgradePower();
            if (applied)
                _powerResolver?.ResolveCard(card);

            return applied;
        }

        private bool GrantPerk(RunState runState)
        {
            if (_perkRegistry == null || runState.Deck.Count == 0) return false;

            var candidates = _perkRegistry.GetAll()
                .Where(perk => CountPerkCopies(runState, perk.Id) < MaxPerkCopies)
                .Where(perk => runState.Deck.Exists(card =>
                    PerkAbilities.CanAttach(perk.Id, card, _effectRegistry)))
                .ToList();

            if (candidates.Count == 0) return false;

            string perkId = candidates[_random.Next(candidates.Count)].Id;
            var hosts = runState.Deck
                .Where(card => PerkAbilities.CanAttach(perkId, card, _effectRegistry))
                .ToList();

            if (hosts.Count == 0) return false;

            var target = hosts[_random.Next(hosts.Count)];
            target.AttachedPerks.Add(perkId);
            runState.DiscoverPerk(perkId);
            _powerResolver?.ResolveCard(target);
            return true;
        }

        public int CountInDeck(RunState runState, string cardId)
        {
            int count = 0;
            foreach (var card in runState.Deck)
            {
                if (string.Equals(card?.Definition?.Id, cardId, StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        public int CountPerkCopies(RunState runState, string perkId)
        {
            int count = 0;
            foreach (var card in runState.Deck)
            {
                foreach (string attached in card.AttachedPerks)
                {
                    if (string.Equals(attached, perkId, StringComparison.Ordinal))
                        count++;
                }
            }

            return count;
        }
    }
}
