using System.Collections.Generic;
using SnapClone.Gameplay.Cards;
using SnapClone.Data.Registries;
using SnapClone.Core.Services;

namespace SnapClone.Gameplay.Draft
{
    public class PerkAssigner
    {
        private PerkRegistry _perkRegistry;
        private RandomService _randomService;
        
        public PerkAssigner(PerkRegistry perkRegistry, RandomService randomService)
        {
            _perkRegistry = perkRegistry;
            _randomService = randomService;
        }
        
        public void AssignPerksToLastThree(List<CardInstance> draftedCards)
        {
            if (draftedCards.Count < 8) return;
            
            // Last 3 cards get random perks
            for (int i = draftedCards.Count - 3; i < draftedCards.Count; i++)
            {
                var perks = _perkRegistry.GetAll();
                if (perks.Count > 0)
                {
                    int index = _randomService.Next(perks.Count);
                    draftedCards[i].AttachedPerks.Add(perks[index].Id);
                }
            }
        }
    }
}
