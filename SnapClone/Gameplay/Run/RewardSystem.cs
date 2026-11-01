using System.Collections.Generic;
using SnapClone.Gameplay.Cards;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Core.Services;

namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Genera ofertas de recompensa sin duplicados. Las cartas nuevas salen
    /// del catalogo respetando el limite de copias del draft; las de perk
    /// salen del mazo actual. Solo se ofrecen perks con modificador positivo.
    /// </summary>
    public class RewardSystem
    {
        public const int OfferCount = 3;
        public const int MaxCopiesPerCard = 2;

        private readonly CardRegistry _cardRegistry;
        private readonly PerkRegistry _perkRegistry;
        private readonly RandomService _randomService;

        public RewardSystem(CardRegistry cardRegistry, PerkRegistry perkRegistry, RandomService randomService)
        {
            _cardRegistry = cardRegistry;
            _perkRegistry = perkRegistry;
            _randomService = randomService;
        }

        /// <summary>Tres cartas distintas del catalogo, cada una con un perk.</summary>
        public List<(CardInstance card, string perkId)> GetCardRewards(RunState runState)
        {
            var offers = new List<(CardInstance card, string perkId)>();
            var candidates = new List<CardDef>();

            foreach (var def in _cardRegistry.GetAll())
            {
                if (CountInDeck(runState, def.Id) < MaxCopiesPerCard)
                    candidates.Add(def);
            }

            var perks = GetPositivePerkIds();

            for (int i = 0; i < OfferCount && candidates.Count > 0; i++)
            {
                int index = _randomService.Next(candidates.Count);
                var def = candidates[index];
                candidates.RemoveAt(index);

                var card = new CardInstance(def);
                string perkId = PickPerk(perks);

                // El regalo incluye el perk: la carta se ofrece ya mejorada.
                if (perkId != null)
                    card.AttachedPerks.Add(perkId);

                offers.Add((card, perkId));
            }

            return offers;
        }

        /// <summary>Tres cartas distintas del mazo, cada una emparejada con un perk.</summary>
        public List<(CardInstance card, string perkId)> GetPerkRewards(RunState runState)
        {
            var offers = new List<(CardInstance card, string perkId)>();
            var candidates = new List<CardInstance>(runState.Deck);
            var perks = GetPositivePerkIds();

            for (int i = 0; i < OfferCount && candidates.Count > 0; i++)
            {
                int index = _randomService.Next(candidates.Count);
                var card = candidates[index];
                candidates.RemoveAt(index);
                offers.Add((card, PickPerk(perks)));
            }

            return offers;
        }

        public bool CanSubstituteCard(RunState runState) => runState.IsDeckAtCap;

        private int CountInDeck(RunState runState, string cardId)
        {
            int count = 0;
            foreach (var card in runState.Deck)
            {
                if (card.Definition.Id == cardId)
                    count++;
            }
            return count;
        }

        private List<string> GetPositivePerkIds()
        {
            var result = new List<string>();

            foreach (var perk in _perkRegistry.GetAll())
            {
                if (perk.Modifiers != null &&
                    perk.Modifiers.TryGetValue("power", out int value) &&
                    value > 0)
                {
                    result.Add(perk.Id);
                }
            }

            return result;
        }

        private string PickPerk(List<string> perks)
        {
            if (perks.Count == 0) return null;
            return perks[_randomService.Next(perks.Count)];
        }
    }
}
