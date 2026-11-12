using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Effects.Implementations
{
    /// <summary>
    /// Implementaciones de las habilidades que otorgan los 28 perks. Todas
    /// usan la autoridad de acciones y el contexto del procesador; ninguna
    /// escena resuelve un perk por su cuenta.
    /// </summary>
    public class AfterimageEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.Event.Kind != ActionEventKind.Moved)
                return;
            if (context.SourceCard == null || context.Actions == null) return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard)) return;
            if (context.Event.OriginZoneIndex < 0) return;

            var origin = context.BoardState?.Zones
                .FirstOrDefault(z => z.ZoneIndex == context.Event.OriginZoneIndex);
            if (origin == null || !origin.CanAdd(context.Owner)) return;

            var copy = context.Actions.CreateCopy(context.SourceCard, false, false);
            if (copy == null) return;

            context.Actions.AddCardToZone(copy, context.Owner,
                context.Event.OriginZoneIndex);
        }
    }

    public class CloneEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard?.Definition == null || context.Actions == null)
                return;
            if (context.SourceCard.IsInPlay) return;

            context.Actions.ShuffleTokenIntoDeck(context.Owner,
                context.SourceCard.Definition);
        }
    }

    public class DeadlinkEffect : CardEffectBase
    {
        public int Amount { get; set; } = 4;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            foreach (var zone in context.BoardState.Zones)
            {
                foreach (var card in zone.CardsFor(context.Owner))
                {
                    if (card?.Definition == null) continue;
                    if (card.Definition.Id != PerkCatalog.ZombieHordeCardId) continue;
                    context.AddPower(card, Amount);
                }
            }
        }
    }

    public class HotPursuitEffect : CardEffectBase, ICardCostModifier
    {
        public int Reduction { get; set; } = 3;

        public int GetCostReduction(EffectContext context)
        {
            var board = context.BoardState;
            if (board == null) return 0;

            return board.LastMoveTurn >= 0 &&
                   board.LastMoveTurn == board.CurrentTurn - 1
                ? Reduction
                : 0;
        }
    }

    public class ManeuverableEffect : CardEffectBase, IPlayerMovePermission
    {
    }

    public class MentorEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.Actions == null) return;

            int power = context.SourceCard.CurrentPower;
            if (context.BoardState?.PowerResolver != null)
            {
                power = context.BoardState.PowerResolver.CalculateCardPower(
                    context.SourceCard, context.SourceZone, context.BoardState,
                    context.Owner);
            }

            if (power > 0)
                context.Actions.BuffTopCard(context.Owner, power);
        }
    }

    public class OverchargeEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null) return;

            context.SourceCard.PendingEffects.Add(new PendingCardEffect
            {
                Kind = PendingEffectKind.WinZoneNextTurnEnergy,
                ArmedTurn = context.TurnNumber,
                ZoneIndex = context.SourceZone.ZoneIndex,
                Amount = 1
            });
        }
    }

    public class OverloadEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null) return;
            if (!context.SourceCard.IsInPlay) return;

            if (IsWinning(context.SourceZone, context.Owner))
                context.Actions.AddEnergy(context.Owner, Amount);
        }

        private static bool IsWinning(ZoneInstance zone, BoardSide side) =>
            side == BoardSide.Player
                ? zone.GetPlayerPower() > zone.GetAIPower()
                : zone.GetAIPower() > zone.GetPlayerPower();
    }

    public class QuickdrawDiscountEffect : CardEffectBase
    {
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null ||
                context.Event.Kind != ActionEventKind.Drawn) return;
            if (context.SourceCard == null) return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard)) return;
            if (context.SourceCard.QuickdrawDiscountApplied) return;

            context.SourceCard.QuickdrawDiscountApplied = true;
            context.SourceCard.TemporaryCostDelta -= Amount;
        }
    }

    public class QuickdrawPowerEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null ||
                context.Event.Kind != ActionEventKind.Drawn) return;
            if (context.SourceCard == null) return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard)) return;
            if (context.SourceCard.QuickdrawPowerApplied) return;

            context.SourceCard.QuickdrawPowerApplied = true;
            if (!context.SourceCard.TemporaryEffectIds.Contains(
                    PerkCatalog.EffectQuickdrawPowerGrant))
            {
                context.SourceCard.TemporaryEffectIds.Add(
                    PerkCatalog.EffectQuickdrawPowerGrant);
            }
        }
    }

    public class QuickdrawPowerGrantEffect : CardEffectBase
    {
        public int Amount { get; set; } = 3;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null) return;
            context.AddPower(context.SourceCard, Amount);
        }
    }

    public class RallyEffect : CardEffectBase, ICardPowerModifier
    {
        public int Amount { get; set; } = 1;

        public int GetPowerBonus(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null) return 0;

            int others = 0;
            foreach (var card in context.SourceZone.CardsFor(context.Owner))
            {
                if (card == null || ReferenceEquals(card, context.SourceCard)) continue;
                others++;
            }

            return Amount * others;
        }
    }

    public class ApexEffect : CardEffectBase, IPowerDoubler
    {
        public bool ShouldDouble(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null) return false;
            if (!context.SourceCard.IsInPlay) return false;

            foreach (var card in context.SourceZone.CardsFor(context.Owner))
            {
                if (card == null || ReferenceEquals(card, context.SourceCard)) continue;
                return false;
            }

            return true;
        }
    }

    public class RegroupEffect : CardEffectBase
    {
        public int Bonus { get; set; } = 3;

        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.Actions == null ||
                context.BoardState == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState, context.Owner);
            if (hand.Count >= BoardState.MaxHandSize) return;

            var candidates = context.SourceZone.CardsFor(context.Owner)
                .Where(card => card != null)
                .ToList();
            if (candidates.Count == 0) return;

            int lowest = candidates.Min(card => card.EffectiveCost);
            var tied = candidates.Where(card => card.EffectiveCost == lowest).ToList();
            var chosen = Series1EffectHelpers.PickRandom(tied, context.Random);
            if (chosen == null) return;

            var result = context.Actions.ReturnToHand(chosen, context.Owner);
            if (result != null && result.Success)
                context.AddPower(chosen, Bonus);
        }
    }

    public class SalvageEffect : CardEffectBase
    {
        public int Bonus { get; set; } = 5;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null || context.BoardState == null) return;

            var candidates = context.SourceZone.CardsFor(context.Owner)
                .Where(card => card != null &&
                               !ReferenceEquals(card, context.SourceCard))
                .ToList();
            if (candidates.Count == 0) return;

            var resolver = context.BoardState.PowerResolver;
            int LowestPower(CardInstance card)
            {
                if (resolver == null) return card.CurrentPower;
                return resolver.CalculateCardPower(card, context.SourceZone,
                    context.BoardState, context.Owner);
            }

            int lowest = candidates.Min(LowestPower);
            var tied = candidates.Where(card => LowestPower(card) == lowest).ToList();
            var chosen = Series1EffectHelpers.PickRandom(tied, context.Random);
            if (chosen == null) return;

            var result = context.Actions.DestroyCard(chosen, context.Owner,
                ActionCause.Effect);
            if (result != null && result.Success)
                context.AddPower(context.SourceCard, Bonus);
        }
    }

    public class SidekickEffect : CardEffectBase
    {
        public int CostReduction { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.CardRegistry == null || context.Actions == null ||
                context.BoardState == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState, context.Owner);
            if (hand.Count >= BoardState.MaxHandSize) return;

            var pool = context.CardRegistry.GetDraftable()
                .Where(card => card != null && !card.IsToken)
                .ToList();
            if (pool.Count == 0) return;

            var definition = Series1EffectHelpers.PickRandom(pool, context.Random);
            if (definition == null) return;

            var card = new CardInstance(definition)
            {
                Location = CardLocation.Hand,
                TemporaryCostDelta = -CostReduction
            };

            context.Actions.AddToHand(card, context.Owner);
        }
    }

    public class SilenceEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null) return;
            context.SourceCard.PrintedAbilitiesDisabled = true;
        }
    }

    public class SlingshotEffect : CardEffectBase
    {
        public int Bonus { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null || context.BoardState == null) return;

            var candidates = context.SourceZone.CardsFor(context.Owner)
                .Where(card => card != null &&
                               !ReferenceEquals(card, context.SourceCard))
                .ToList();
            if (candidates.Count == 0) return;

            var chosen = Series1EffectHelpers.PickRandom(candidates, context.Random);
            if (chosen == null) return;

            context.AddPower(chosen, Bonus);

            int destination = context.SourceZone.ZoneIndex + 1;
            if (destination < context.BoardState.Zones.Count &&
                context.BoardState.Zones[destination].CanAdd(context.Owner))
            {
                context.Actions.MoveCard(chosen, context.Owner,
                    context.SourceZone.ZoneIndex, destination, ActionCause.Effect);
            }
        }
    }

    public class SuperstarEffect : CardEffectBase
    {
    }

    public class TauntEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null) return;

            var opposing = context.Owner == BoardSide.Player
                ? BoardSide.AI
                : BoardSide.Player;

            if (!context.SourceZone.CanAdd(opposing)) return;

            context.BoardState.AddTauntReservation(opposing,
                context.SourceZone.ZoneIndex, context.TurnNumber + 1);
        }
    }

    public class UnstoppableEffect : CardEffectBase, IPlayAnywhere, IDestroyImmune,
        IPowerReductionImmune
    {
    }

    public class WakandanTechEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null) return;
            if (context.SourceCard.WakandanTechApplied) return;

            int power = context.SourceCard.CurrentPower;
            if (context.BoardState?.PowerResolver != null &&
                context.SourceZone != null)
            {
                power = context.BoardState.PowerResolver.CalculateCardPower(
                    context.SourceCard, context.SourceZone, context.BoardState,
                    context.Owner);
            }

            context.SourceCard.WakandanTechApplied = true;
            if (power > 0)
                context.AddPower(context.SourceCard, power);
        }
    }

    // ---------------------------------------------------------------------
    // Meta-habilidades (marcadores que el procesador consulta)
    // ---------------------------------------------------------------------

    public class AmplifyEffect : CardEffectBase, IRepeatOngoing
    {
    }

    public class EncoreEffect : CardEffectBase, IRepeatOnReveal
    {
    }

    public class OvertimeEffect : CardEffectBase, IRepeatEndOfTurn
    {
    }

    public class ReinforceEffect : CardEffectBase, IEmpoweredReactor
    {
        public void OnEmpowered(EffectContext context, int amount)
        {
            if (context.SourceCard == null) return;
            context.AddPower(context.SourceCard, 1, false);
        }
    }
}
