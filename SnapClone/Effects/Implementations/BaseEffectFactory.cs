using SnapClone.Effects;
using SnapClone.Data.Definitions;

namespace SnapClone.Effects.Implementations
{
    public class BaseEffectFactory
    {
        public static IEffect CreateEffect(EffectDef effectDef)
        {
            if (effectDef == null) return null;

            switch (effectDef.EffectType)
            {
                case "OnRevealArmNextTurnBonus":
                    return new HawkeyeOnRevealEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 5)
                    };

                case "OngoingZonePowerMultiplier":
                    return new IronManOngoingEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Multiplier = GetInt(effectDef, "multiplier", 2)
                    };

                case "OnRevealBonusIfMiddle":
                    return new MedusaOnRevealEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 3),
                        ZoneIndex = GetInt(effectDef, "zoneIndex", 1)
                    };

                case "OngoingPowerPerEnemyCard":
                    return new PunisherOngoingEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "OnRevealAddCardToHand":
                    return new SentinelOnRevealEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        CardId = GetString(effectDef, "cardId", "card_sentinel")
                    };

                case "OnRevealBonusIfEnemyPlayedHere":
                    return new StarLordOnRevealEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 4)
                    };
            }

            return null;
        }

        private static int GetInt(EffectDef effectDef, string key, int fallback)
        {
            if (effectDef.Parameters != null &&
                effectDef.Parameters.TryGetValue(key, out object value) &&
                value is int number)
            {
                return number;
            }

            return fallback;
        }

        private static string GetString(EffectDef effectDef, string key, string fallback)
        {
            if (effectDef.Parameters != null &&
                effectDef.Parameters.TryGetValue(key, out object value) &&
                value != null)
            {
                return value.ToString();
            }

            return fallback;
        }
    }
}
