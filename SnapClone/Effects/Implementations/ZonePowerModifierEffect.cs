using SnapClone.Effects;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using System.Collections.Generic;

namespace SnapClone.Effects.Implementations
{
    public class ZonePowerModifierEffect : IEffect
    {
        public string EffectId { get; set; }
        public EffectTrigger Trigger { get; set; } = EffectTrigger.ZoneContinuous;
        string IEffect.EffectId => EffectId;
        public int Amount { get; set; }
        public string Target { get; set; }
        
        public void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null) return;
            
            if (Target == "all")
            {
                foreach (var card in context.SourceZone.PlayerCards)
                {
                    card.CurrentPower += Amount;
                }
                foreach (var card in context.SourceZone.AICards)
                {
                    card.CurrentPower += Amount;
                }
            }
            else if (Target == "highest_cost")
            {
                // Find highest cost card in zone
                CardInstance highestCostCard = null;
                int highestCost = -1;
                
                foreach (var card in context.SourceZone.PlayerCards)
                {
                    if (card.Definition.Cost > highestCost)
                    {
                        highestCost = card.Definition.Cost;
                        highestCostCard = card;
                    }
                }
                foreach (var card in context.SourceZone.AICards)
                {
                    if (card.Definition.Cost > highestCost)
                    {
                        highestCost = card.Definition.Cost;
                        highestCostCard = card;
                    }
                }
                
                if (highestCostCard != null)
                {
                    highestCostCard.CurrentPower += Amount;
                }
            }
        }
    }
}
