namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Medusa: gana +3 al revelarse solo si esta en la zona central.
    /// </summary>
    public class MedusaOnRevealEffect : CardEffectBase
    {
        public int Amount { get; set; } = 3;
        public int ZoneIndex { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null) return;
            if (context.SourceZone.ZoneIndex != ZoneIndex) return;

            context.AddPower(context.SourceCard, Amount);
        }
    }
}
