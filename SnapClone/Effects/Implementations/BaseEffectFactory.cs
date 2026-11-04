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

                case "OnRevealDestroy":
                    return new OnRevealDestroyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Target = GetString(effectDef, "target", "othersHere"),
                        PerDestroyed = GetInt(effectDef, "perDestroyed", 0)
                    };

                case "OnRevealDiscard":
                    return new OnRevealDiscardEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Selection = GetString(effectDef, "selection", "rightmost")
                    };

                case "OnRevealAfflict":
                    return new OnRevealAfflictEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", -1)
                    };

                case "OnRevealMove":
                    return new OnRevealMoveEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Mode = GetString(effectDef, "mode", "highestPowerToHere")
                    };

                case "OnRevealAddToken":
                    return new OnRevealAddTokenEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        TokenId = GetString(effectDef, "tokenId", ""),
                        Target = GetString(effectDef, "target", "here"),
                        CopySourcePower = GetBool(effectDef, "copySourcePower", false),
                        PowerBonus = GetInt(effectDef, "powerBonus", 0)
                    };

                case "OnRevealCopy":
                    return new OnRevealCopyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Source = GetString(effectDef, "source", "enemyHandRandom"),
                        CopyToSelf = GetBool(effectDef, "copyToSelf", false)
                    };

                case "OnRevealMerge":
                    return new OnRevealMergeEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealReplaceLocation":
                    return new OnRevealReplaceLocationEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "NextPlayBonus":
                    return new NextPlayBonusEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "NextPlayMove":
                    return new NextPlayMoveEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "DeckTopBonus":
                    return new DeckTopBonusEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 3)
                    };

                case "DrawFromEnemyDeck":
                    return new DrawFromEnemyDeckEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "ShuffleToken":
                    return new ShuffleTokenEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        TokenId = GetString(effectDef, "tokenId", "token_rock")
                    };

                case "BanishLowestCost":
                    return new BanishLowestCostEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        CombatScoped = GetBool(effectDef, "combatScoped", true)
                    };

                case "AfterPlayedCounter":
                    return new AfterPlayedCounterEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Scope = GetString(effectDef, "scope", "global"),
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "WhenDestroyedBuffAllies":
                    return new WhenDestroyedBuffAlliesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "WhenMovedHere":
                    return new WhenMovedHereEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "SelfMovedCopy":
                    return new SelfMovedCopyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AllyDestroyedFlyIn":
                    return new AllyDestroyedFlyInEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "RemoveAbilities":
                    return new RemoveAbilitiesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "GameStartVisibility":
                    return new GameStartVisibilityEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "ApocalypseDiscardReplacement":
                    return new ApocalypseDiscardReplacement
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 4)
                    };

                case "WolverineRegenerate":
                    return new WolverineRegenerateEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "CaptainAmericaAura":
                    return new CaptainAmericaAuraEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "KlawZoneAddition":
                    return new KlawZoneAdditionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 6)
                    };

                case "OngoingConditionalPower":
                    return new OngoingConditionalPowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Condition = GetString(effectDef, "condition", ""),
                        Amount = GetInt(effectDef, "amount", 0)
                    };

                case "ZonePowerModifier":
                    return new ZonePowerModifierEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 0),
                        Target = GetString(effectDef, "target", "all")
                    };

                case "DestroyProtection":
                    return new DestroyProtectionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "SuppressOnReveal":
                    return new SuppressOnRevealEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "ZoneLock":
                    return new ZoneLockEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OngoingRepeat":
                    return new OngoingRepeatEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };
            }

            return null;
        }

        private static int GetInt(EffectDef effectDef, string key, int fallback)
        {
            if (effectDef.Parameters != null &&
                effectDef.Parameters.TryGetValue(key, out object value))
            {
                if (value is int number) return number;
                if (value is string text && int.TryParse(text, out int parsed))
                    return parsed;
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

        private static bool GetBool(EffectDef effectDef, string key, bool fallback)
        {
            if (effectDef.Parameters != null &&
                effectDef.Parameters.TryGetValue(key, out object value) &&
                value != null)
            {
                if (value is bool flag) return flag;
                if (bool.TryParse(value.ToString(), out bool parsed)) return parsed;
            }

            return fallback;
        }
    }
}
