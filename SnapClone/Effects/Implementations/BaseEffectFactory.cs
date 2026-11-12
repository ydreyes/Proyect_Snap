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
                        PerDestroyed = GetInt(effectDef, "perDestroyed", 0),
                        AddDestroyedPower = GetBool(effectDef,
                            "addDestroyedPower", false)
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
                        Amount = GetInt(effectDef, "amount", -1),
                        Target = GetString(effectDef, "target", "enemy")
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
                        PowerBonus = GetInt(effectDef, "powerBonus", 0),
                        BothSides = GetBool(effectDef, "bothSides", false),
                        Copies = GetInt(effectDef, "copies", 1),
                        CopySourcePowerAsSet = GetBool(effectDef,
                            "copySourcePowerAsSet", false)
                    };

                case "OnRevealCopy":
                    return new OnRevealCopyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Source = GetString(effectDef, "source", "enemyHandRandom"),
                        CopyToSelf = GetBool(effectDef, "copyToSelf", false),
                        PowerBonus = GetInt(effectDef, "powerBonus", 0)
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
                        TokenId = GetString(effectDef, "tokenId", "token_rock"),
                        Target = GetString(effectDef, "target", "opponent"),
                        Copies = GetInt(effectDef, "copies", 1)
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
                        Amount = GetInt(effectDef, "amount", 6),
                        Direction = GetString(effectDef, "direction", "right")
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

                case "Afterimage":
                    return new AfterimageEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Amplify":
                    return new AmplifyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Apex":
                    return new ApexEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Clone":
                    return new CloneEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Deadlink":
                    return new DeadlinkEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 4)
                    };

                case "Encore":
                    return new EncoreEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "HotPursuit":
                    return new HotPursuitEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Reduction = GetInt(effectDef, "amount", 3)
                    };

                case "Maneuverable":
                    return new ManeuverableEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Mentor":
                    return new MentorEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Overcharge":
                    return new OverchargeEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Overload":
                    return new OverloadEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "Overtime":
                    return new OvertimeEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "QuickdrawDiscount":
                    return new QuickdrawDiscountEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "QuickdrawPower":
                    return new QuickdrawPowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "QuickdrawPowerGrant":
                    return new QuickdrawPowerGrantEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 3)
                    };

                case "Rally":
                    return new RallyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "Regroup":
                    return new RegroupEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Bonus = GetInt(effectDef, "amount", 3)
                    };

                case "Reinforce":
                    return new ReinforceEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Salvage":
                    return new SalvageEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Bonus = GetInt(effectDef, "amount", 5)
                    };

                case "Sidekick":
                    return new SidekickEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        CostReduction = GetInt(effectDef, "amount", 2)
                    };

                case "Silence":
                    return new SilenceEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Slingshot":
                    return new SlingshotEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Bonus = GetInt(effectDef, "amount", 1)
                    };

                case "Superstar":
                    return new SuperstarEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Taunt":
                    return new TauntEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "Unstoppable":
                    return new UnstoppableEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "WakandanTech":
                    return new WakandanTechEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealBonusPerOtherCardHere":
                    return new OnRevealBonusPerOtherCardHereEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "OnRevealBuffContainer":
                    return new OnRevealBuffContainerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Target = GetString(effectDef, "target", "hand"),
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "OnRevealAfflictHand":
                    return new OnRevealAfflictHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", -1)
                    };

                case "OnRevealAddRandomCardToHand":
                    return new OnRevealAddRandomCardToHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        MinCost = GetInt(effectDef, "minCost", 0),
                        MaxCost = GetInt(effectDef, "maxCost", 6),
                        Count = GetInt(effectDef, "count", 1)
                    };

                case "OnRevealBuffOngoing":
                    return new OnRevealBuffOngoingEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "OnRevealDisableHighCostHand":
                    return new OnRevealDisableHighCostHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Threshold = GetInt(effectDef, "threshold", 6)
                    };

                case "OnRevealPullTopDeck":
                    return new OnRevealPullTopDeckEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OdinRepeatZone":
                    return new OdinRepeatEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealArmNotPlayedNextTurn":
                    return new OnRevealArmNotPlayedNextTurnEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 5)
                    };

                case "OnRevealOpenMoveWindow":
                    return new OnRevealOpenMoveWindowEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        TurnOffset = GetInt(effectDef, "turnOffset", 1)
                    };

                case "OnRevealSwitchSides":
                    return new OnRevealSwitchSidesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealFloodZone":
                    return new OnRevealFloodZoneEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealNextTurnCostTax":
                    return new OnRevealNextTurnCostTaxEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "OnRevealRaiseEnemyCost":
                    return new OnRevealRaiseEnemyCostEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1),
                        Cap = GetInt(effectDef, "cap", 6)
                    };

                case "OngoingCostAura":
                    return new OngoingCostAuraEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Cost = GetInt(effectDef, "cost", 1),
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "AfterMovedSelfBuff":
                    return new AfterMovedSelfBuffEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 4)
                    };

                case "AfterDestroyedReplaceToken":
                    return new AfterDestroyedReplaceTokenEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        TokenId = GetString(effectDef, "tokenId", "")
                    };

                case "AfterDestroyedReturnZeroCost":
                    return new AfterDestroyedReturnZeroCostEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterDiscardedAddZeroCostCopies":
                    return new AfterDiscardedAddZeroCostCopiesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Copies = GetInt(effectDef, "copies", 2)
                    };

                case "AfterAddedToHandSelfBuff":
                    return new AfterAddedToHandSelfBuffEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "EndOfTurnUnspentEnergy":
                    return new EndOfTurnUnspentEnergyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "PassivePlayRestriction":
                    return new PassivePlayRestrictionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        MaxPlayTurn = GetInt(effectDef, "maxTurn", 0),
                        RequiresNoPlayLastTurn = GetBool(effectDef,
                            "requiresNoPlayLastTurn", false)
                    };

                case "PassiveMovePermission":
                    return new PassiveMovePermissionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Frequency = GetString(effectDef, "frequency",
                            "oncePerCombat") == "oncePerTurn"
                            ? MovePermission.OncePerTurn
                            : MovePermission.OncePerCombat
                    };

                case "PlayOnlyLock":
                    return new PlayLockEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealBuffCost":
                    return new OnRevealBuffCostEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Cost = GetInt(effectDef, "cost", 3),
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "OngoingRepeatOnReveal":
                    return new OngoingRepeatOnRevealEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OngoingWinningZoneAddition":
                    return new OngoingWinningZoneAdditionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 3)
                    };

                case "EnemyCardsHereAura":
                    return new EnemyCardsHereAuraEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "AllyOtherCardsHereAura":
                    return new AllyOtherCardsHereAuraEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", -2)
                    };

                case "HighestPowerCardsAura":
                    return new HighestPowerCardsAuraEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 3)
                    };

                case "NoAbilityCardsAura":
                    return new NoAbilityCardsAuraEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "LukeCageAura":
                    return new LukeCageAuraEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AddPowerToNamedCard":
                    return new AddPowerToNamedCardEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        CardName = GetString(effectDef, "cardName", "Thor"),
                        Amount = GetInt(effectDef, "amount", 7)
                    };

                case "WhenEnemyMovedHereAfflict":
                    return new WhenEnemyMovedHereAfflictEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", -2)
                    };

                case "AfterMovedSelfBuffPerEnemyCard":
                    return new AfterMovedSelfBuffPerEnemyCardEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "AfterMovedSelfDouble":
                    return new AfterMovedSelfDoubleEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterMovedAfflictEnemyHere":
                    return new AfterMovedAfflictEnemyHereEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", -3)
                    };

                case "OnRevealAfflictLowestBothHands":
                    return new OnRevealAfflictLowestBothHandsEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", -3)
                    };

                case "OnRevealAfflictAllOtherCards":
                    return new OnRevealAfflictAllOtherCardsEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", -1)
                    };

                case "OnRevealDiscardThenDestroyRandomEnemy":
                    return new OnRevealDiscardThenDestroyRandomEnemyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealReturnCostCards":
                    return new OnRevealReturnCostCardsEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Cost = GetInt(effectDef, "cost", 1)
                    };

                case "RemoveAbilitiesByTrigger":
                    return new RemoveAbilitiesByTriggerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        AbilityTrigger = GetString(effectDef, "abilityTrigger",
                            "EndOfTurn") == "Ongoing"
                            ? EffectTrigger.Ongoing
                            : EffectTrigger.EndOfTurn
                    };

                case "OnRevealNextTurnEnergy":
                    return new OnRevealNextTurnEnergyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "StartOfTurnReturnToHandAndBuff":
                    return new StartOfTurnReturnToHandAndBuffEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "OnRevealMoveLastEnemyPlayed":
                    return new OnRevealMoveLastEnemyPlayedEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "EndOfTurnDestroyWeakerHere":
                    return new EndOfTurnDestroyWeakerHereEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealMoveLastAllyPlayed":
                    return new OnRevealMoveLastAllyPlayedEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "RemoveAbilitiesFromDeckByTrigger":
                    return new RemoveAbilitiesFromDeckByTriggerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        AbilityTrigger = GetString(effectDef, "abilityTrigger",
                            "Ongoing") == "EndOfTurn"
                            ? EffectTrigger.EndOfTurn
                            : EffectTrigger.Ongoing
                    };

                case "OnRevealAddRandomCostToEachLocation":
                    return new OnRevealAddRandomCostToEachLocationEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Cost = GetInt(effectDef, "cost", 1)
                    };

                case "OnRevealSwitchLowestOtherHere":
                    return new OnRevealSwitchLowestOtherHereEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "NextPlaySilence":
                    return new NextPlaySilenceEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OngoingDestroyedPower":
                    return new OngoingDestroyedPowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "PassiveMarker":
                    return new PassiveMarkerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealGlobalCostCap":
                    return new OnRevealGlobalCostCapEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        MaxCost = GetInt(effectDef, "maxCost", 4),
                        TurnOffset = GetInt(effectDef, "turnOffset", 1)
                    };

                case "OnRevealSetEnemyDeckTopCost":
                    return new OnRevealSetEnemyDeckTopCostEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Cost = GetInt(effectDef, "cost", 6),
                        UntilTurn = GetInt(effectDef, "untilTurn", 6)
                    };

                case "OnRevealReturnOthersHereDiscount":
                    return new OnRevealReturnOthersHereDiscountEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "OnRevealSetHandBasePower":
                    return new OnRevealSetHandBasePowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Power = GetInt(effectDef, "power", 3)
                    };

                case "OnRevealSetZoneBasePower":
                    return new OnRevealSetZoneBasePowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Power = GetInt(effectDef, "power", 3)
                    };

                case "OnRevealResetZonePower":
                    return new OnRevealResetZonePowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealDoubleSelf":
                    return new OnRevealDoubleSelfEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "NextPlayDoublePower":
                    return new NextPlayDoublePowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealCopyLastPlayedPower":
                    return new OnRevealCopyLastPlayedPowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterDestroyedReturnWithDoublePower":
                    return new AfterDestroyedReturnWithDoublePowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealSwapDeckCostPower":
                    return new OnRevealSwapDeckCostPowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "EndOfTurnDrawIfWinningOrBuff":
                    return new EndOfTurnDrawIfWinningOrBuffEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "OnRevealBothDraw":
                    return new OnRevealBothDrawEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealEnemyDraws":
                    return new OnRevealEnemyDrawsEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Count = GetInt(effectDef, "count", 2)
                    };

                case "OnRevealDrawCostFromDeck":
                    return new OnRevealDrawCostFromDeckEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Cost = GetInt(effectDef, "cost", 0)
                    };

                case "OnRevealResurrectFromDiscard":
                    return new OnRevealResurrectFromDiscardEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealResurrectDistinctCosts":
                    return new OnRevealResurrectDistinctCostsEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealDiscardEvenBoth":
                    return new OnRevealDiscardEvenBothEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealDiscardLowestPowerBoth":
                    return new OnRevealDiscardLowestPowerBothEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealAddCardToEnemyHand":
                    return new OnRevealAddCardToEnemyHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        CardId = GetString(effectDef, "cardId", ""),
                        Count = GetInt(effectDef, "count", 1)
                    };

                case "OnRevealChangeMaxEnergy":
                    return new OnRevealChangeMaxEnergyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 1)
                    };

                case "EndOfTurnLoseMaxEnergyGainPower":
                    return new EndOfTurnLoseMaxEnergyGainPowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        MaxEnergyDelta = GetInt(effectDef, "maxEnergyDelta", -1),
                        Amount = GetInt(effectDef, "amount", 3)
                    };

                case "EndOfTurnDiscardSelfFromHand":
                    return new EndOfTurnDiscardSelfFromHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterDiscardedFillHand":
                    return new AfterDiscardedFillHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealPullEnemyHand":
                    return new OnRevealPullEnemyHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealDiscardFromEnemyHand":
                    return new OnRevealDiscardFromEnemyHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Selection = GetString(effectDef, "selection",
                            "leastCost")
                    };

                case "OnRevealDestroyAndCopyToOtherZones":
                    return new OnRevealDestroyAndCopyToOtherZonesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealCopyEnemyPlays":
                    return new OnRevealCopyEnemyPlaysEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterDestroyedCopyToOtherZones":
                    return new AfterDestroyedCopyToOtherZonesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealSwapOtherZones":
                    return new OnRevealSwapOtherZonesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterEnemyOngoingPlayedHereSilence":
                    return new AfterEnemyOngoingPlayedHereSilenceEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterEnemyPlayedHereDestroyOnce":
                    return new AfterEnemyPlayedHereDestroyOnceEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterAnyCardPlayedHereMoveSelf":
                    return new AfterAnyCardPlayedHereMoveSelfEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterAnyCardPlayedHereSwitchSides":
                    return new AfterAnyCardPlayedHereSwitchSidesEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AfterAllyMovedHereMoveAgain":
                    return new AfterAllyMovedHereMoveAgainEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "EndOfTurnNebula":
                    return new EndOfTurnNebulaEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Amount = GetInt(effectDef, "amount", 2)
                    };

                case "OnRevealMoveEnemyPlayedHere":
                    return new OnRevealMoveEnemyPlayedHereEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealMoveSelfAndPullEnemy":
                    return new OnRevealMoveSelfAndPullEnemyEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "CrossbonesRestriction":
                    return new CrossbonesRestrictionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "GigantoRestriction":
                    return new GigantoRestrictionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "SentryRestriction":
                    return new SentryRestrictionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealReplaceWithLimbo":
                    return new OnRevealReplaceWithLimboEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealCopyLastPlayedEffects":
                    return new OnRevealCopyLastPlayedEffectsEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        Filter = GetString(effectDef, "filter", "OnReveal") ==
                                 "Ongoing"
                            ? EffectTrigger.Ongoing
                            : EffectTrigger.OnReveal
                    };

                case "OnRevealStealOngoing":
                    return new OnRevealStealOngoingEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "SuperSkrullOngoing":
                    return new SuperSkrullOngoingEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "ActivateAddCardToEnemyHand":
                    return new ActivateAddCardToEnemyHandEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        CardId = GetString(effectDef, "cardId",
                            "token_widows_bite"),
                        Count = GetInt(effectDef, "count", 1)
                    };

                case "ActivateDiscardRightmost":
                    return new ActivateDiscardRightmostEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "ActivateArmNextPlaySwap":
                    return new ActivateArmNextPlaySwapEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "ActivateSwapPowerWithIllusion":
                    return new ActivateSwapPowerWithIllusionEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "ActivateSpiderHam":
                    return new ActivateSpiderHamEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "EndOfGameMoveToWin":
                    return new EndOfGameMoveToWinEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "EndOfGameMoveToLose":
                    return new EndOfGameMoveToLoseEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "EndOfGameJumpFromDeck":
                    return new EndOfGameJumpFromDeckEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "EndOfGameDiscardGainPower":
                    return new EndOfGameDiscardGainPowerEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "OnRevealSuppressZoneEffects":
                    return new OnRevealSuppressZoneEffectsEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger,
                        TurnOffset = GetInt(effectDef, "turnOffset", 1)
                    };

                case "OnRevealRepeatZone":
                    return new OnRevealRepeatZoneEffect
                    {
                        EffectId = effectDef.Id,
                        Trigger = effectDef.Trigger
                    };

                case "AgathaGameStart":
                    return new AgathaGameStartEffect
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
