using SnapClone.Effects;
using SnapClone.Data.Definitions;

namespace SnapClone.Effects.Implementations
{
    public class BaseEffectFactory
    {
        public static IEffect CreateEffect(EffectDef effectDef)
        {
            if (effectDef.EffectType == "ZonePowerModifier")
            {
                var effect = new ZonePowerModifierEffect
                {
                    EffectId = effectDef.Id,
                    Trigger = effectDef.Trigger
                };
                
                if (effectDef.Parameters.ContainsKey("amount"))
                    effect.Amount = (int)effectDef.Parameters["amount"];
                if (effectDef.Parameters.ContainsKey("target"))
                    effect.Target = effectDef.Parameters["target"].ToString();
                    
                return effect;
            }
            
            return null;
        }
    }
}
