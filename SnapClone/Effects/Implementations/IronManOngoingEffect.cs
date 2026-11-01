namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Iron Man: cada copia multiplica por dos el total de su lado en la zona.
    /// No cambia el poder individual de las cartas.
    /// </summary>
    public class IronManOngoingEffect : CardEffectBase, IZonePowerMultiplier
    {
        public int Multiplier { get; set; } = 2;

        public int GetMultiplier(EffectContext context) => Multiplier;
    }
}
