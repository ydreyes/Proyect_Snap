using SnapClone.Data.Definitions;

namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Base de los efectos de carta. Las implementaciones son stateless: todo
    /// el estado mutable vive en CardInstance o BoardState, de modo que el
    /// processor puede cachearlas y reutilizarlas.
    /// </summary>
    public abstract class CardEffectBase : IEffect
    {
        public string EffectId { get; set; }
        public EffectTrigger Trigger { get; set; } = EffectTrigger.OnReveal;

        public virtual void Apply(EffectContext context) { }
    }
}
