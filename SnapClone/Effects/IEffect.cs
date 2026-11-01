using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Board;

namespace SnapClone.Effects
{
    public class EffectContext
    {
        public CardInstance SourceCard { get; set; }
        public ZoneInstance SourceZone { get; set; }
        public BoardSide Owner { get; set; }
        public List<CardInstance> Targets { get; set; } = new List<CardInstance>();
        public BoardState BoardState { get; set; }
        public int TurnNumber { get; set; }
        public string PriorityOwner { get; set; }
        public CardRegistry CardRegistry { get; set; }
    }

    public interface IEffect
    {
        string EffectId { get; }
        EffectTrigger Trigger { get; }
        void Apply(EffectContext context);
    }

    /// <summary>
    /// Contrato continuo para el poder de una carta. Se consulta en cada
    /// calculo y nunca escribe el poder actual, asi el resultado es idempotente.
    /// </summary>
    public interface ICardPowerModifier
    {
        int GetPowerBonus(EffectContext context);
    }

    /// <summary>
    /// Contrato continuo para el total de una zona. Varios multiplicadores
    /// de la misma zona se multiplican entre si.
    /// </summary>
    public interface IZonePowerMultiplier
    {
        int GetMultiplier(EffectContext context);
    }
}
