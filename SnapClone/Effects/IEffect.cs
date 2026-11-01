using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Board;

namespace SnapClone.Effects
{
    public class EffectContext
    {
        public CardInstance SourceCard { get; set; }
        public ZoneInstance SourceZone { get; set; }
        public List<CardInstance> Targets { get; set; } = new List<CardInstance>();
        public BoardState BoardState { get; set; }
        public int TurnNumber { get; set; }
        public string PriorityOwner { get; set; }
    }
    
    public interface IEffect
    {
        string EffectId { get; }
        EffectTrigger Trigger { get; }
        void Apply(EffectContext context);
    }
}
