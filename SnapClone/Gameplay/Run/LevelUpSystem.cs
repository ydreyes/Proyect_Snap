using System;
using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Meta;

namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Genera y aplica las decisiones del nodo de level-up. Las ofertas se
    /// guardan en el LevelUpState para que no se rebarajen al volver a entrar.
    /// </summary>
    public class LevelUpSystem
    {
        public const int OfferCount = 3;
        public const int MaxCardCopies = 2;
        public const int MaxPerkCopies = 2;

        private readonly CardRegistry _cardRegistry;
        private readonly PerkRegistry _perkRegistry;
        private readonly EffectRegistry _effectRegistry;
        private readonly RandomService _randomService;
        private readonly PowerResolver _powerResolver;
        private readonly Func<IEnumerable<string>> _ownedCardIds;

        public LevelUpSystem(CardRegistry cardRegistry, PerkRegistry perkRegistry,
            RandomService randomService, PowerResolver powerResolver,
            Func<IEnumerable<string>> ownedCardIds = null,
            EffectRegistry effectRegistry = null)
        {
            _cardRegistry = cardRegistry;
            _perkRegistry = perkRegistry;
            _effectRegistry = effectRegistry;
            _randomService = randomService;
            _powerResolver = powerResolver;
            _ownedCardIds = ownedCardIds;
        }

        public void EnsureOffers(RunState runState, LevelUpState levelUp)
        {
            if (runState == null || levelUp == null || levelUp.OffersGenerated)
                return;

            levelUp.CardOffers.Clear();
            levelUp.UpgradeCandidates.Clear();
            levelUp.PerkOffers.Clear();

            var owned = _ownedCardIds?.Invoke();
            HashSet<string> allowed = owned == null
                ? null
                : new HashSet<string>(owned, StringComparer.Ordinal);

            List<CardDef> cardCandidates = new List<CardDef>();
            foreach (var def in _cardRegistry.GetDraftable())
            {
                if (allowed != null && !allowed.Contains(def.Id))
                    continue;
                if (CountInDeck(runState, def.Id) < MaxCardCopies)
                    cardCandidates.Add(def);
            }

            int cardOfferCount = System.Math.Min(OfferCount, cardCandidates.Count);
            for (int i = 0; i < cardOfferCount; i++)
            {
                int index = _randomService.Next(cardCandidates.Count);
                levelUp.CardOffers.Add(new CardInstance(cardCandidates[index]));
                cardCandidates.RemoveAt(index);
            }

            foreach (var card in runState.Deck)
            {
                if (card.CanUpgrade)
                    levelUp.UpgradeCandidates.Add(card);
            }

            var perkCandidates = GetPerkCandidates(runState);
            int perkOfferCount = System.Math.Min(OfferCount, perkCandidates.Count);
            for (int i = 0; i < perkOfferCount; i++)
            {
                int index = _randomService.Next(perkCandidates.Count);
                levelUp.PerkOffers.Add(perkCandidates[index]);
                perkCandidates.RemoveAt(index);
            }

            foreach (var offer in levelUp.CardOffers)
                runState.DiscoverCard(offer.Definition?.Id);

            foreach (string perkId in levelUp.PerkOffers)
                runState.DiscoverPerk(perkId);

            levelUp.OffersGenerated = true;
        }

        public bool CanUseCardBranch(RunState runState, LevelUpState levelUp)
        {
            if (runState == null || levelUp == null) return false;

            EnsureOffers(runState, levelUp);

            if (runState.Deck.Count < RunState.MaxDeckSize)
                return levelUp.CardOffers.Count > 0;

            return levelUp.UpgradeCandidates.Count > 0;
        }

        public bool CanUsePerkBranch(RunState runState, LevelUpState levelUp)
        {
            if (runState == null || levelUp == null || runState.Deck.Count == 0)
                return false;

            EnsureOffers(runState, levelUp);
            return levelUp.PerkOffers.Count > 0;
        }

        public bool AddCard(RunState runState, LevelUpState levelUp, CardInstance offer)
        {
            if (runState.Deck.Count >= RunState.MaxDeckSize) return false;
            if (offer?.Definition == null) return false;
            if (levelUp == null || !levelUp.CardOffers.Contains(offer)) return false;
            if (CountInDeck(runState, offer.Definition.Id) >= MaxCardCopies) return false;

            _powerResolver.ResolveCard(offer);
            runState.Deck.Add(offer);
            return true;
        }

        public bool ApplyUpgrade(RunState runState, LevelUpState levelUp,
            CardInstance card, UpgradeType type)
        {
            if (runState == null || levelUp == null || card == null) return false;
            if (!runState.Deck.Contains(card)) return false;
            if (!levelUp.UpgradeCandidates.Contains(card)) return false;

            bool applied = type == UpgradeType.Cost
                ? card.TryUpgradeCost()
                : card.TryUpgradePower();

            if (applied)
                _powerResolver.ResolveCard(card);

            return applied;
        }

        public bool ApplyPerk(RunState runState, LevelUpState levelUp,
            string perkId, CardInstance target)
        {
            if (runState == null || levelUp == null || target == null) return false;
            if (!runState.Deck.Contains(target)) return false;
            if (levelUp.PerkOffers == null || !levelUp.PerkOffers.Contains(perkId)) return false;
            if (CountPerkCopies(runState, perkId) >= MaxPerkCopies) return false;
            if (!CanAttach(perkId, target)) return false;

            target.AttachedPerks.Add(perkId);
            _powerResolver.ResolveCard(target);
            return true;
        }

        /// <summary>La carta cumple el requisito del perk.</summary>
        public bool CanAttach(string perkId, CardInstance card) =>
            PerkAbilities.CanAttach(perkId, card, _effectRegistry);

        /// <summary>Razon de incompatibilidad para la grilla de anfitriones.</summary>
        public string IncompatibleReason(string perkId, CardInstance card) =>
            PerkAbilities.IncompatibleReason(perkId, card, _effectRegistry);

        /// <summary>Al menos una carta del mazo puede hospedar el perk.</summary>
        public bool HasCompatibleHost(RunState runState, string perkId)
        {
            if (runState?.Deck == null) return false;

            foreach (var card in runState.Deck)
            {
                if (CanAttach(perkId, card))
                    return true;
            }

            return false;
        }

        public void ResolveHead(RunState runState)
        {
            if (runState == null || runState.PendingLevelUps.Count == 0) return;
            runState.PendingLevelUps.RemoveAt(0);
        }

        /// <summary>Refresca el poder mostrado de todo el mazo fuera de combate.</summary>
        public void RefreshDeck(RunState runState)
        {
            if (runState == null) return;

            foreach (var card in runState.Deck)
                _powerResolver.ResolveCard(card);
        }

        public int CountInDeck(RunState runState, string cardId)
        {
            int count = 0;
            foreach (var card in runState.Deck)
            {
                if (card.Definition?.Id == cardId)
                    count++;
            }
            return count;
        }

        public int CountPerkCopies(RunState runState, string perkId)
        {
            int count = 0;
            foreach (var card in runState.Deck)
            {
                foreach (var attached in card.AttachedPerks)
                {
                    if (attached == perkId)
                        count++;
                }
            }
            return count;
        }

        private List<string> GetPerkCandidates(RunState runState)
        {
            var result = new List<string>();
            if (_perkRegistry == null) return result;

            // Los 28 perks estan disponibles. Solo se ofrecen los que tienen al
            // menos un anfitrion compatible y menos de dos copias en el mazo.
            foreach (string perkId in PerkCatalog.AllIds)
            {
                if (_perkRegistry.Get(perkId) == null) continue;
                if (CountPerkCopies(runState, perkId) >= MaxPerkCopies) continue;
                if (!HasCompatibleHost(runState, perkId)) continue;

                result.Add(perkId);
            }

            return result;
        }
    }

    public enum UpgradeType
    {
        Cost,
        Power
    }
}
