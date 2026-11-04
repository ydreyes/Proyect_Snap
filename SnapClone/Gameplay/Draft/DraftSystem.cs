using System.Collections.Generic;
using SnapClone.Gameplay.Cards;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Core.Services;

namespace SnapClone.Gameplay.Draft
{
    /// <summary>
    /// Borrador de 8 rondas: cada ronda ofrece 3 cartas distintas y el
    /// jugador elige 1. Una carta no puede superar las 2 copias.
    /// </summary>
    public class DraftSystem
    {
        public const int TotalPicks = 8;
        public const int MaxCopies = 2;
        public const int ChoicesPerRound = 3;

        private readonly CardRegistry _cardRegistry;
        private readonly RandomService _randomService;
        private readonly List<CardInstance> _draftedCards = new List<CardInstance>();
        private readonly Dictionary<string, int> _copyCount = new Dictionary<string, int>();

        public DraftSystem(CardRegistry cardRegistry, RandomService randomService)
        {
            _cardRegistry = cardRegistry;
            _randomService = randomService;
        }

        public int PicksMade => _draftedCards.Count;
        public int RemainingPicks => TotalPicks - _draftedCards.Count;
        public bool IsDraftComplete() => _draftedCards.Count >= TotalPicks;

        public void Reset()
        {
            _draftedCards.Clear();
            _copyCount.Clear();
        }

        /// <summary>
        /// Devuelve hasta 3 cartas distintas que todavia pueden draftearse.
        /// </summary>
        public List<CardInstance> GetChoices()
        {
            List<CardInstance> choices = new List<CardInstance>();

            if (IsDraftComplete())
                return choices;

            List<CardDef> pool = new List<CardDef>();
            foreach (var def in _cardRegistry.GetDraftable())
            {
                if (GetCopyCount(def.Id) < MaxCopies)
                    pool.Add(def);
            }

            int count = pool.Count < ChoicesPerRound ? pool.Count : ChoicesPerRound;
            for (int i = 0; i < count; i++)
            {
                int index = _randomService.Next(pool.Count);
                choices.Add(new CardInstance(pool[index]));
                pool.RemoveAt(index);
            }

            return choices;
        }

        public bool PickCard(CardInstance card)
        {
            if (card?.Definition == null) return false;
            if (IsDraftComplete()) return false;

            string cardId = card.Definition.Id;
            int currentCopies = GetCopyCount(cardId);

            if (currentCopies >= MaxCopies)
                return false;

            _draftedCards.Add(card);
            _copyCount[cardId] = currentCopies + 1;
            return true;
        }

        public int GetCopyCount(string cardId) =>
            _copyCount.TryGetValue(cardId, out int copies) ? copies : 0;

        public List<CardInstance> GetDraftedCards() => _draftedCards;
    }
}
