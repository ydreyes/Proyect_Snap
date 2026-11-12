using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Meta;

namespace SnapClone.Effects.Implementations
{
    internal static class Series3EffectHelpers
    {
        public static List<CardInstance> AlliedCardsInPlay(BoardState board,
            BoardSide side)
        {
            var result = new List<CardInstance>();
            if (board == null) return result;

            foreach (var zone in board.Zones)
                result.AddRange(zone.CardsFor(side));

            return result;
        }

        public static BoardSide Other(BoardSide side) =>
            side == BoardSide.Player ? BoardSide.AI : BoardSide.Player;
    }

    /// <summary>Silver Surfer: +2 a las otras cartas de coste 3.</summary>
    public class OnRevealBuffCostEffect : CardEffectBase
    {
        public int Cost { get; set; } = 3;
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            foreach (var card in Series3EffectHelpers.AlliedCardsInPlay(
                context.BoardState, context.Owner))
            {
                if (ReferenceEquals(card, context.SourceCard)) continue;
                if (card.EffectiveCost == Cost)
                    context.AddPower(card, Amount);
            }
        }
    }

    /// <summary>Wong: las habilidades Al revelarse de la zona se repiten.</summary>
    public class OngoingRepeatOnRevealEffect : CardEffectBase,
        IRepeatOnRevealAtLocation
    {
    }

    /// <summary>Omega Red: +3 a las otras zonas si gana la suya.</summary>
    public class OngoingWinningZoneAdditionEffect : CardEffectBase,
        IZonePowerAddition
    {
        public int Amount { get; set; } = 3;

        public int GetAdditionForZone(EffectContext sourceContext,
            ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext?.SourceZone == null || targetZone == null)
                return 0;
            if (targetSide != sourceContext.Owner) return 0;
            if (ReferenceEquals(targetZone, sourceContext.SourceZone))
                return 0;

            var zone = sourceContext.SourceZone;
            bool winning = sourceContext.Owner == BoardSide.Player
                ? zone.GetPlayerPower() > zone.GetAIPower()
                : zone.GetAIPower() > zone.GetPlayerPower();

            return winning ? Amount : 0;
        }
    }

    /// <summary>Red Skull: las cartas enemigas aqui ganan +2.</summary>
    public class EnemyCardsHereAuraEffect : CardEffectBase, ICardAura
    {
        public int Amount { get; set; } = 2;

        public int GetBonusForCard(EffectContext sourceContext,
            CardInstance target, ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext?.SourceZone == null || targetZone == null)
                return 0;
            if (targetSide == sourceContext.Owner) return 0;
            if (!ReferenceEquals(targetZone, sourceContext.SourceZone))
                return 0;
            if (target == null || target.AbilitiesDisabled) return 0;

            return Amount;
        }
    }

    /// <summary>Typhoid Mary: tus otras cartas aqui tienen -2.</summary>
    public class AllyOtherCardsHereAuraEffect : CardEffectBase, ICardAura
    {
        public int Amount { get; set; } = -2;

        public int GetBonusForCard(EffectContext sourceContext,
            CardInstance target, ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext?.SourceZone == null || targetZone == null)
                return 0;
            if (targetSide != sourceContext.Owner) return 0;
            if (!ReferenceEquals(targetZone, sourceContext.SourceZone))
                return 0;
            if (target == null || ReferenceEquals(target, sourceContext.SourceCard))
                return 0;

            return Amount;
        }
    }

    /// <summary>Cerebro: las cartas de mayor Poder aliadas ganan +3.</summary>
    public class HighestPowerCardsAuraEffect : CardEffectBase, ICardAura
    {
        public int Amount { get; set; } = 3;

        public int GetBonusForCard(EffectContext sourceContext,
            CardInstance target, ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext?.BoardState == null || target == null)
                return 0;
            if (targetSide != sourceContext.Owner) return 0;

            int max = int.MinValue;
            foreach (var card in Series3EffectHelpers.AlliedCardsInPlay(
                sourceContext.BoardState, sourceContext.Owner))
            {
                if (card?.Definition == null) continue;
                max = Math.Max(max, card.EffectivePower);
            }

            return target.EffectivePower == max ? Amount : 0;
        }
    }

    /// <summary>Patriot: las cartas sin habilidades ganan +2.</summary>
    public class NoAbilityCardsAuraEffect : CardEffectBase, ICardAura
    {
        public int Amount { get; set; } = 2;

        public int GetBonusForCard(EffectContext sourceContext,
            CardInstance target, ZoneInstance targetZone, BoardSide targetSide)
        {
            if (target == null || targetSide != sourceContext.Owner) return 0;

            var effects = PerkAbilities.EffectiveEffectIds(target,
                !target.PrintedAbilitiesDisabled);
            return effects.Count == 0 ? Amount : 0;
        }
    }

    /// <summary>Luke Cage: tus cartas en juego no pierden Poder.</summary>
    public class LukeCageAuraEffect : CardEffectBase
    {
    }

    /// <summary>Mjolnir: da +7 de Poder a Thor.</summary>
    public class AddPowerToNamedCardEffect : CardEffectBase
    {
        public string CardName { get; set; } = "Thor";
        public int Amount { get; set; } = 7;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            foreach (var card in Series3EffectHelpers.AlliedCardsInPlay(
                context.BoardState, context.Owner))
            {
                if (string.Equals(card?.Definition?.Name, CardName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    context.AddPower(card, Amount);
                    return;
                }
            }
        }
    }

    /// <summary>Kingpin: cuando una carta enemiga se mueve aqui, -2.</summary>
    public class WhenEnemyMovedHereAfflictEffect : CardEffectBase
    {
        public int Amount { get; set; } = -2;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceZone == null)
                return;
            if (context.Event.Kind != ActionEventKind.Moved) return;
            if (context.Event.Side == context.Owner) return;
            if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                return;
            if (context.Event.SourceCard == null) return;

            context.AddPower(context.Event.SourceCard, Amount);
        }
    }

    /// <summary>Dagger: +2 por cada carta enemiga en la zona de destino.</summary>
    public class AfterMovedSelfBuffPerEnemyCardEffect : CardEffectBase
    {
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.BoardState == null)
                return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;
            if (context.Event.OriginZoneIndex ==
                context.Event.DestinationZoneIndex)
                return;

            int dest = context.Event.DestinationZoneIndex;
            if (dest < 0 || dest >= context.BoardState.Zones.Count) return;

            int enemies = context.BoardState.Zones[dest]
                .CardsFor(Series3EffectHelpers.Other(context.Owner)).Count;
            if (enemies > 0)
                context.AddPower(context.SourceCard, Amount * enemies);
        }
    }

    /// <summary>Human Torch: cuando se mueve, duplica su Poder.</summary>
    public class AfterMovedSelfDoubleEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null) return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;
            if (context.Event.OriginZoneIndex ==
                context.Event.DestinationZoneIndex)
                return;

            context.SourceCard.DoublePower();
            context.Actions?.RefreshPowers();
        }
    }

    /// <summary>Spider-Man 2099: al moverse, aflige un enemigo en el destino.</summary>
    public class AfterMovedAfflictEnemyHereEffect : CardEffectBase
    {
        public int Amount { get; set; } = -3;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.BoardState == null)
                return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;

            int dest = context.Event.DestinationZoneIndex;
            if (dest < 0 || dest >= context.BoardState.Zones.Count) return;

            var enemies = context.BoardState.Zones[dest]
                .CardsFor(Series3EffectHelpers.Other(context.Owner)).ToList();
            var chosen = Series1EffectHelpers.PickRandom(enemies, context.Random);
            if (chosen != null)
                context.AddPower(chosen, Amount);
        }
    }

    /// <summary>Selene: aflige la carta de menor Poder de cada mano.</summary>
    public class OnRevealAfflictLowestBothHandsEffect : CardEffectBase
    {
        public int Amount { get; set; } = -3;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            foreach (var side in new[] { BoardSide.Player, BoardSide.AI })
            {
                var hand = Series1EffectHelpers.HandFor(context.BoardState, side);
                if (hand.Count == 0) continue;

                int min = hand.Min(c => c.EffectivePower);
                var candidates = hand.Where(c => c.EffectivePower == min).ToList();
                var chosen = Series1EffectHelpers.PickRandom(candidates,
                    context.Random);
                if (chosen != null)
                    context.AddPower(chosen, Amount);
            }
        }
    }

    /// <summary>Hazmat: aflige a todas las demas cartas en juego.</summary>
    public class OnRevealAfflictAllOtherCardsEffect : CardEffectBase
    {
        public int Amount { get; set; } = -1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            foreach (var zone in context.BoardState.Zones)
            {
                foreach (var card in zone.PlayerCards.Concat(zone.AICards))
                {
                    if (ReferenceEquals(card, context.SourceCard)) continue;
                    context.AddPower(card, Amount);
                }
            }
        }
    }

    /// <summary>Gambit: descarta una carta y destruye un enemigo aleatorio.</summary>
    public class OnRevealDiscardThenDestroyRandomEnemyEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            if (hand.Count == 0) return;

            var discard = hand[hand.Count - 1];
            context.Actions.DiscardCard(discard, context.Owner,
                ActionCause.Effect);

            var enemy = Series3EffectHelpers.Other(context.Owner);
            var targets = new List<CardInstance>();
            foreach (var zone in context.BoardState.Zones)
                targets.AddRange(zone.CardsFor(enemy));

            var chosen = Series1EffectHelpers.PickRandom(targets, context.Random);
            if (chosen != null)
                context.Actions.DestroyCard(chosen, enemy, ActionCause.Effect);
        }
    }

    /// <summary>Falcon: devuelve a la mano tus cartas de coste 1.</summary>
    public class OnRevealReturnCostCardsEffect : CardEffectBase
    {
        public int Cost { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var cards = Series3EffectHelpers.AlliedCardsInPlay(
                context.BoardState, context.Owner)
                .Where(c => !ReferenceEquals(c, context.SourceCard) &&
                            c.EffectiveCost == Cost)
                .ToList();

            foreach (var card in cards)
                context.Actions.ReturnToHand(card, context.Owner);
        }
    }

    /// <summary>Ghost: quita habilidades a las cartas de un disparador aqui.</summary>
    public class RemoveAbilitiesByTriggerEffect : CardEffectBase
    {
        public EffectTrigger AbilityTrigger { get; set; } = EffectTrigger.EndOfTurn;

        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.EffectRegistry == null)
                return;

            foreach (var card in context.SourceZone.PlayerCards
                .Concat(context.SourceZone.AICards).ToList())
            {
                if (ReferenceEquals(card, context.SourceCard)) continue;
                bool has = PerkAbilities.EffectiveEffectIds(card,
                        !card.PrintedAbilitiesDisabled)
                    .Any(effectId =>
                    {
                        var definition = context.EffectRegistry.Get(effectId);
                        return definition != null &&
                               definition.Trigger == AbilityTrigger;
                    });
                if (has) card.AbilitiesDisabled = true;
            }
        }
    }

    /// <summary>Psylocke: +1 de energia el siguiente turno.</summary>
    public class OnRevealNextTurnEnergyEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            context.Actions?.AddNextTurnEnergy(context.Owner, Amount);
        }
    }

    /// <summary>Kitty Pryde: vuelve a la mano al inicio del turno y gana +1.</summary>
    public class StartOfTurnReturnToHandAndBuffEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.Actions == null) return;
            if (!context.SourceCard.IsInPlay) return;

            var result = context.Actions.ReturnToHand(context.SourceCard,
                context.Owner);
            if (result != null && result.Success)
                context.Actions.AddBonusPower(context.SourceCard, Amount, false);
        }
    }

    /// <summary>Aero: la ultima carta rival jugada se mueve aqui.</summary>
    public class OnRevealMoveLastEnemyPlayedEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            PlayRecord last = null;
            foreach (var record in context.BoardState.PlayLog)
            {
                if (record == null || record.Side != enemy) continue;
                if (last == null || record.PlayOrder > last.PlayOrder)
                    last = record;
            }

            if (last?.Card == null) return;
            int from = last.ZoneIndex;
            if (from < 0 || from >= context.BoardState.Zones.Count) return;
            if (!context.BoardState.Zones[from].CardsFor(enemy)
                .Contains(last.Card)) return;

            context.Actions.MoveCard(last.Card, enemy, from,
                context.SourceZone.ZoneIndex, ActionCause.Effect);
        }
    }

    /// <summary>Attuma: fin de turno, destruye una carta propia mas debil aqui.</summary>
    public class EndOfTurnDestroyWeakerHereEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            int sourcePower = context.SourceCard.CurrentPower;
            var weaker = context.SourceZone.CardsFor(context.Owner)
                .Where(card => !ReferenceEquals(card, context.SourceCard) &&
                               card.CurrentPower < sourcePower)
                .OrderBy(card => card.CurrentPower)
                .FirstOrDefault();

            if (weaker != null)
                context.Actions.DestroyCard(weaker, context.Owner,
                    ActionCause.Effect);
        }
    }

    /// <summary>Ghost-Spider: la ultima carta propia jugada se mueve aqui.</summary>
    public class OnRevealMoveLastAllyPlayedEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            PlayRecord last = null;
            foreach (var record in context.BoardState.PlayLog)
            {
                if (record == null || record.Side != context.Owner) continue;
                if (ReferenceEquals(record.Card, context.SourceCard)) continue;
                if (last == null || record.PlayOrder > last.PlayOrder)
                    last = record;
            }

            if (last?.Card == null) return;
            int from = last.ZoneIndex;
            if (from < 0 || from >= context.BoardState.Zones.Count) return;
            if (from == context.SourceZone.ZoneIndex) return;

            context.Actions.MoveCard(last.Card, context.Owner, from,
                context.SourceZone.ZoneIndex, ActionCause.Effect);
        }
    }

    /// <summary>Sauron: quita habilidades a las cartas Ongoing del mazo.</summary>
    public class RemoveAbilitiesFromDeckByTriggerEffect : CardEffectBase
    {
        public EffectTrigger AbilityTrigger { get; set; } = EffectTrigger.Ongoing;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.EffectRegistry == null)
                return;

            var deck = context.Owner == BoardSide.Player
                ? context.BoardState.PlayerDeck
                : context.BoardState.AIDeck;

            foreach (var card in deck)
            {
                bool has = PerkAbilities.EffectiveEffectIds(card,
                        !card.PrintedAbilitiesDisabled)
                    .Any(effectId =>
                    {
                        var definition = context.EffectRegistry.Get(effectId);
                        return definition != null &&
                               definition.Trigger == AbilityTrigger;
                    });
                if (has) card.PrintedAbilitiesDisabled = true;
            }
        }
    }

    /// <summary>Shanna: agrega una carta aleatoria de coste 1 a cada locacion.</summary>
    public class OnRevealAddRandomCostToEachLocationEffect : CardEffectBase
    {
        public int Cost { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.CardRegistry == null ||
                context.Actions == null)
                return;

            var pool = context.CardRegistry.GetDraftable()
                .Where(card => card != null && !card.IsToken &&
                               card.Cost == Cost)
                .ToList();

            foreach (var zone in context.BoardState.Zones)
            {
                if (!zone.CanAdd(context.Owner)) continue;

                var definition = Series1EffectHelpers.PickRandom(pool,
                    context.Random);
                if (definition == null) continue;

                var token = new CardInstance(definition)
                {
                    Location = CardLocation.Board,
                    IsInPlay = true,
                    PlayedTurn = 0,
                    PlayedZoneIndex = -1,
                    IsCreated = true
                };

                context.Actions.AddCardToZone(token, context.Owner,
                    zone.ZoneIndex);
            }
        }
    }

    /// <summary>Viper: cambia de lado tu otra carta de menor Poder aqui.</summary>
    public class OnRevealSwitchLowestOtherHereEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            var chosen = context.SourceZone.CardsFor(context.Owner)
                .Where(card => !ReferenceEquals(card, context.SourceCard))
                .OrderBy(card => card.CurrentPower)
                .FirstOrDefault();

            if (chosen != null)
                context.Actions.SwitchSides(chosen, context.Owner,
                    context.SourceZone.ZoneIndex);
        }
    }

    /// <summary>Zero: silencia la proxima carta que juegues.</summary>
    public class NextPlaySilenceEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            context.BoardState?.AddPendingNextPlay(context.Owner,
                new PendingNextPlayEffect
                {
                    Kind = PendingNextPlayKind.Silence
                });
        }
    }

    /// <summary>Marca pasiva usada por el resolver de coste.</summary>
    public class PassiveMarkerEffect : CardEffectBase
    {
    }

    /// <summary>Knull: poder combinado de las cartas destruidas.</summary>
    public class OngoingDestroyedPowerEffect : CardEffectBase,
        ICardPowerModifier
    {
        public int GetPowerBonus(EffectContext context)
        {
            if (context.BoardState == null) return 0;

            int total = 0;
            foreach (var record in context.BoardState.DestroyedCards)
                total += record.PowerAtDestruction;

            return total;
        }
    }

    /// <summary>Wave: todas las cartas cuestan como maximo 4 hasta el siguiente turno.</summary>
    public class OnRevealGlobalCostCapEffect : CardEffectBase
    {
        public int MaxCost { get; set; } = 4;
        public int TurnOffset { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;
            context.BoardState.SetGlobalMaxCost(MaxCost,
                context.BoardState.CurrentTurn + TurnOffset);
        }
    }

    /// <summary>Baron Mordo: la carta superior del mazo rival cuesta 6 hasta el turno 6.</summary>
    public class OnRevealSetEnemyDeckTopCostEffect : CardEffectBase
    {
        public int Cost { get; set; } = 6;
        public int UntilTurn { get; set; } = 6;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            var deck = enemy == BoardSide.Player
                ? context.BoardState.PlayerDeck
                : context.BoardState.AIDeck;
            if (deck.Count == 0) return;

            var top = deck[0];
            top.FixedCost = Cost;
            top.FixedCostUntilTurn = UntilTurn;
        }
    }

    /// <summary>Beast: devuelve tus otras cartas aqui; cuestan 1 menos el siguiente turno.</summary>
    public class OnRevealReturnOthersHereDiscountEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null || context.BoardState == null)
                return;

            var allies = context.SourceZone.CardsFor(context.Owner)
                .Where(card => !ReferenceEquals(card, context.SourceCard))
                .ToList();

            foreach (var card in allies)
            {
                if (!context.Actions.ReturnToHand(card, context.Owner).Success)
                    continue;

                card.PendingCostReduction = Amount;
                card.PendingCostReductionTurn =
                    context.BoardState.CurrentTurn + 1;
            }
        }
    }

    /// <summary>Bast: el Poder de toda tu mano pasa a 3.</summary>
    public class OnRevealSetHandBasePowerEffect : CardEffectBase
    {
        public int Power { get; set; } = 3;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            foreach (var card in hand)
                card.SetBasePower(Power);
        }
    }

    /// <summary>Valkyrie: todas las cartas aqui pasan a 3 de Poder.</summary>
    public class OnRevealSetZoneBasePowerEffect : CardEffectBase
    {
        public int Power { get; set; } = 3;

        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null) return;

            foreach (var card in context.SourceZone.PlayerCards
                .Concat(context.SourceZone.AICards))
                card.SetBasePower(Power);
        }
    }

    /// <summary>Shadow King: vuelve todas las cartas aqui a su Poder original.</summary>
    public class OnRevealResetZonePowerEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null) return;

            foreach (var card in context.SourceZone.PlayerCards
                .Concat(context.SourceZone.AICards))
                card.ResetBasePower();
        }
    }

    /// <summary>Black Panther: duplica su propio Poder.</summary>
    public class OnRevealDoubleSelfEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null) return;
            context.SourceCard.DoublePower();
        }
    }

    /// <summary>Shuri: la proxima carta jugada aqui duplica su Poder.</summary>
    public class NextPlayDoublePowerEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            context.BoardState?.AddPendingNextPlay(context.Owner,
                new PendingNextPlayEffect
                {
                    Kind = PendingNextPlayKind.DoublePower
                });
        }
    }

    /// <summary>Taskmaster: iguala el Poder de la ultima carta jugada.</summary>
    public class OnRevealCopyLastPlayedPowerEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.BoardState == null)
                return;

            PlayRecord last = null;
            foreach (var record in context.BoardState.PlayLog)
            {
                if (record == null || record.Side != context.Owner) continue;
                if (ReferenceEquals(record.Card, context.SourceCard)) continue;
                if (last == null || record.PlayOrder > last.PlayOrder)
                    last = record;
            }

            if (last?.Card == null) return;
            context.SourceCard.SetBasePower(last.Card.CurrentPower);
        }
    }

    /// <summary>Deadpool: al ser destruido vuelve a la mano con el doble de Poder.</summary>
    public class AfterDestroyedReturnWithDoublePowerEffect : CardEffectBase,
        ICardDestroyReplacement
    {
        public bool TryReplaceDestroy(EffectContext context)
        {
            if (context?.SourceCard == null || context.Actions == null ||
                context.BoardState == null)
                return false;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            if (hand.Count >= BoardState.MaxHandSize) return false;

            var removed = context.Actions.RemoveFromContainers(
                context.SourceCard, context.Owner);
            if (removed == null || !removed.Success) return false;

            context.SourceCard.DoublePower();
            var added = context.Actions.AddToHand(context.SourceCard,
                context.Owner);
            return added != null && added.Success;
        }
    }

    /// <summary>Mister Negative: intercambia coste y Poder en el mazo.</summary>
    public class OnRevealSwapDeckCostPowerEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var deck = context.Owner == BoardSide.Player
                ? context.BoardState.PlayerDeck
                : context.BoardState.AIDeck;

            foreach (var card in deck.ToList())
            {
                if (card.Definition == null) continue;

                var swapped = card.Definition.Clone();
                int cost = card.Definition.Cost;
                swapped.Cost = card.Definition.BasePower;
                swapped.BasePower = cost;
                context.Actions.Transform(card, swapped);
            }
        }
    }

    /// <summary>Adam Warlock: roba si gana aqui; si no, +1 Poder.</summary>
    public class EndOfTurnDrawIfWinningOrBuffEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.BoardState == null)
                return;

            var zone = context.SourceZone;
            bool winning = context.Owner == BoardSide.Player
                ? zone.GetPlayerPower() > zone.GetAIPower()
                : zone.GetAIPower() > zone.GetPlayerPower();

            if (winning)
                context.Actions?.DrawCard(context.Owner);
            else
                context.AddPower(context.SourceCard, Amount);
        }
    }

    /// <summary>Crystal: cada jugador roba una carta.</summary>
    public class OnRevealBothDrawEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            context.Actions?.DrawCard(BoardSide.Player);
            context.Actions?.DrawCard(BoardSide.AI);
        }
    }

    /// <summary>Maximus: el rival roba 2 cartas.</summary>
    public class OnRevealEnemyDrawsEffect : CardEffectBase
    {
        public int Count { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            var enemy = Series3EffectHelpers.Other(context.Owner);
            for (int i = 0; i < Count; i++)
                context.Actions?.DrawCard(enemy);
        }
    }

    /// <summary>Jane Foster: roba todas las cartas de coste 0 del mazo.</summary>
    public class OnRevealDrawCostFromDeckEffect : CardEffectBase
    {
        public int Cost { get; set; }

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var deck = context.Owner == BoardSide.Player
                ? context.BoardState.PlayerDeck
                : context.BoardState.AIDeck;

            foreach (var card in deck.ToList())
            {
                if (card.Definition == null || card.EffectiveCost != Cost)
                    continue;
                context.Actions.DrawSpecificFromDeck(card, context.Owner);
            }
        }
    }

    /// <summary>Ghost Rider: devuelve una carta descartada a esta locacion.</summary>
    public class OnRevealResurrectFromDiscardEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null ||
                context.Actions == null)
                return;

            var discard = context.BoardState.GetPiles(context.Owner)
                .Get(PileKind.Discard);
            if (discard.Count == 0) return;

            var chosen = Series1EffectHelpers.PickRandom(
                discard.ToList(), context.Random);
            if (chosen != null)
                context.Actions.ResurrectFromPile(chosen, context.Owner,
                    context.SourceZone.ZoneIndex);
        }
    }

    /// <summary>Hela: resucita una carta por cada coste distinto.</summary>
    public class OnRevealResurrectDistinctCostsEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var discard = context.BoardState.GetPiles(context.Owner)
                .Get(PileKind.Discard);
            var groups = discard
                .Where(card => card.Definition != null)
                .GroupBy(card => card.Definition.Cost)
                .ToList();

            foreach (var group in groups)
            {
                var chosen = Series1EffectHelpers.PickRandom(
                    group.ToList(), context.Random);
                if (chosen == null) continue;

                var zones = context.BoardState.Zones
                    .Where(zone => zone.CanAdd(context.Owner))
                    .ToList();
                var zone = Series1EffectHelpers.PickRandom(zones, context.Random);
                if (zone == null) break;

                context.Actions.ResurrectFromPile(chosen, context.Owner,
                    zone.ZoneIndex);
            }
        }
    }

    /// <summary>Moon Knight: descarta una carta de coste par de cada mano.</summary>
    public class OnRevealDiscardEvenBothEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            var own = hand.FirstOrDefault(c => c.EffectiveCost % 2 == 0);
            if (own != null)
                context.Actions.DiscardCard(own, context.Owner,
                    ActionCause.Effect);

            context.Actions.DiscardFromEnemyHand(context.Owner, "evenCost");
        }
    }

    /// <summary>Silver Samurai: cada jugador descarta su carta de menor Poder.</summary>
    public class OnRevealDiscardLowestPowerBothEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            var own = hand
                .OrderBy(c => c.EffectivePower)
                .ThenBy(c => c.Definition?.Cost ?? 0)
                .FirstOrDefault();
            if (own != null)
                context.Actions.DiscardCard(own, context.Owner,
                    ActionCause.Effect);

            context.Actions.DiscardFromEnemyHand(context.Owner, "lowestPower");
        }
    }

    /// <summary>Master Mold / Black Widow: agrega cartas a la mano rival.</summary>
    public class OnRevealAddCardToEnemyHandEffect : CardEffectBase
    {
        public string CardId { get; set; }
        public int Count { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.CardRegistry == null || context.Actions == null)
                return;

            var definition = context.CardRegistry.Get(CardId);
            if (definition == null) return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            for (int i = 0; i < Count; i++)
                context.Actions.AddToHand(new CardInstance(definition), enemy);
        }
    }

    /// <summary>Electro: +1 de Energia maxima.</summary>
    public class OnRevealChangeMaxEnergyEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            context.Actions?.ChangeMaxEnergy(context.Owner, Amount);
        }
    }

    /// <summary>Havok: fin de turno, -1 Energia maxima y +3 Poder.</summary>
    public class EndOfTurnLoseMaxEnergyGainPowerEffect : CardEffectBase
    {
        public int MaxEnergyDelta { get; set; } = -1;
        public int Amount { get; set; } = 3;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null) return;

            context.Actions?.ChangeMaxEnergy(context.Owner, MaxEnergyDelta);
            context.AddPower(context.SourceCard, Amount);
        }
    }

    /// <summary>Black Cat: fin de turno, se descarta de la mano.</summary>
    public class EndOfTurnDiscardSelfFromHandEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.BoardState == null ||
                context.Actions == null)
                return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            if (hand.Contains(context.SourceCard))
                context.Actions.DiscardCard(context.SourceCard,
                    context.Owner, ActionCause.Effect);
        }
    }

    /// <summary>Helicarrier: al descartarse, llena la mano con cartas aleatorias.</summary>
    public class AfterDiscardedFillHandEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.BoardState == null || context.CardRegistry == null ||
                context.Actions == null)
                return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            var pool = context.CardRegistry.GetDraftable()
                .Where(card => card != null && !card.IsToken)
                .ToList();

            while (hand.Count < BoardState.MaxHandSize)
            {
                var definition = Series1EffectHelpers.PickRandom(pool,
                    context.Random);
                if (definition == null) break;
                if (!context.Actions.AddToHand(new CardInstance(definition),
                        context.Owner).Success)
                    break;
            }
        }
    }

    /// <summary>Doctor Octopus: trae la carta rival de menor Poder a esta locacion.</summary>
    public class OnRevealPullEnemyHandEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.Actions == null) return;
            context.Actions.PullEnemyHandCard(context.Owner,
                context.SourceZone.ZoneIndex);
        }
    }

    /// <summary>Black Bolt: descarta la carta de menor coste de la mano rival.</summary>
    public class OnRevealDiscardFromEnemyHandEffect : CardEffectBase
    {
        public string Selection { get; set; } = "leastCost";

        public override void Apply(EffectContext context)
        {
            context.Actions?.DiscardFromEnemyHand(context.Owner, Selection);
        }
    }

    /// <summary>Arnim Zola: destruye otra carta aqui y copiala en las otras zonas.</summary>
    public class OnRevealDestroyAndCopyToOtherZonesEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            var allies = context.SourceZone.CardsFor(context.Owner)
                .Where(card => !ReferenceEquals(card, context.SourceCard))
                .ToList();
            var chosen = Series1EffectHelpers.PickRandom(allies, context.Random);
            if (chosen == null) return;

            var copySource = chosen;
            int origin = context.SourceZone.ZoneIndex;
            context.Actions.DestroyCard(chosen, context.Owner,
                ActionCause.Effect);

            foreach (var zone in context.BoardState.Zones)
            {
                if (zone.ZoneIndex == origin) continue;
                if (!zone.CanAdd(context.Owner)) continue;

                var copy = context.Actions.CreateCopy(copySource);
                if (copy == null) continue;
                copy.HasRevealed = true;
                copy.PlayedTurn = 0;
                copy.PlayedZoneIndex = -1;
                context.Actions.AddCardToZone(copy, context.Owner,
                    zone.ZoneIndex);
            }
        }
    }

    /// <summary>Leader: copia las cartas enemigas de mayor Poder jugadas este turno.</summary>
    public class OnRevealCopyEnemyPlaysEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            var records = context.BoardState.PlayLog
                .Where(record => record != null && record.Side == enemy &&
                                 record.Turn == context.TurnNumber &&
                                 record.Card != null)
                .ToList();
            if (records.Count == 0) return;

            int max = records.Max(record => record.Card.CurrentPower);
            foreach (var record in records.Where(r => r.Card.CurrentPower == max))
            {
                var zone = context.BoardState.Zones
                    .FirstOrDefault(z => z.ZoneIndex == record.ZoneIndex &&
                                         z.CanAdd(context.Owner));
                if (zone == null)
                {
                    zone = context.BoardState.Zones
                        .FirstOrDefault(z => z.CanAdd(context.Owner));
                }
                if (zone == null) break;

                var copy = context.Actions.CreateCopy(record.Card);
                if (copy == null) continue;
                copy.HasRevealed = true;
                copy.PlayedTurn = 0;
                copy.PlayedZoneIndex = -1;
                context.Actions.AddCardToZone(copy, context.Owner,
                    zone.ZoneIndex);
            }
        }
    }

    /// <summary>Nimrod: al ser destruido, agrega copias a las otras locaciones.</summary>
    public class AfterDestroyedCopyToOtherZonesEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.BoardState == null || context.Actions == null)
                return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;

            int origin = context.Event.OriginZoneIndex;
            foreach (var zone in context.BoardState.Zones)
            {
                if (zone.ZoneIndex == origin) continue;
                if (!zone.CanAdd(context.Owner)) continue;

                var copy = context.Actions.CreateCopy(context.SourceCard);
                if (copy == null) continue;
                copy.HasRevealed = true;
                copy.PlayedTurn = 0;
                copy.PlayedZoneIndex = -1;
                context.Actions.AddCardToZone(copy, context.Owner,
                    zone.ZoneIndex);
            }
        }
    }

    /// <summary>Quake: intercambia las otras dos locaciones.</summary>
    public class OnRevealSwapOtherZonesEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null ||
                context.Actions == null)
                return;

            var others = context.BoardState.Zones
                .Where(zone => zone.ZoneIndex != context.SourceZone.ZoneIndex)
                .ToList();
            if (others.Count != 2) return;
            context.Actions.SwapZones(others[0].ZoneIndex, others[1].ZoneIndex);
        }
    }

    /// <summary>Echo: silencia una carta Ongoing enemiga jugada aqui.</summary>
    public class AfterEnemyOngoingPlayedHereSilenceEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceZone == null ||
                context.Event.SourceCard == null)
                return;
            if (context.Event.Kind != ActionEventKind.Played) return;
            if (context.Event.Side == context.Owner) return;
            if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                return;

            var card = context.Event.SourceCard;
            bool ongoing = PerkAbilities.EffectiveEffectIds(card)
                .Any(effectId =>
                {
                    var definition = context.EffectRegistry?.Get(effectId);
                    return definition != null &&
                           definition.Trigger == EffectTrigger.Ongoing;
                });
            if (ongoing) card.PrintedAbilitiesDisabled = true;
        }
    }

    /// <summary>Negasonic: destruye la primera carta enemiga jugada aqui.</summary>
    public class AfterEnemyPlayedHereDestroyOnceEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceZone == null ||
                context.SourceCard == null || context.Actions == null)
                return;
            if (context.Event.Kind != ActionEventKind.Played) return;
            if (context.Event.Side == context.Owner) return;
            if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                return;
            if (context.SourceCard.OncePerCombatUsed) return;

            context.SourceCard.OncePerCombatUsed = true;
            context.Actions.DestroyCard(context.Event.SourceCard,
                Series3EffectHelpers.Other(context.Owner), ActionCause.Effect);
        }
    }

    /// <summary>Silk: se mueve cuando cualquier otra carta se juega aqui.</summary>
    public class AfterAnyCardPlayedHereMoveSelfEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.SourceZone == null || context.BoardState == null ||
                context.Actions == null)
                return;
            if (context.Event.Kind != ActionEventKind.Played) return;
            if (ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;
            if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                return;

            var zones = context.BoardState.Zones
                .Where(zone => zone.ZoneIndex != context.SourceZone.ZoneIndex &&
                               zone.CanAdd(context.Owner))
                .ToList();
            var zone = Series1EffectHelpers.PickRandom(zones, context.Random);
            if (zone != null)
                context.Actions.MoveCard(context.SourceCard, context.Owner,
                    context.SourceZone.ZoneIndex, zone.ZoneIndex,
                    ActionCause.Effect);
        }
    }

    /// <summary>Titania: cambia de lado cuando cualquier otra carta se juega aqui.</summary>
    public class AfterAnyCardPlayedHereSwitchSidesEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.SourceZone == null || context.Actions == null)
                return;
            if (context.Event.Kind != ActionEventKind.Played) return;
            if (ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;
            if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                return;

            context.Actions.SwitchSides(context.SourceCard, context.Owner,
                context.SourceZone.ZoneIndex);
        }
    }

    /// <summary>Hercules: re-mueve la primera carta aliada que llega aqui.</summary>
    public class AfterAllyMovedHereMoveAgainEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.SourceZone == null || context.BoardState == null ||
                context.Actions == null)
                return;
            if (context.Event.Kind != ActionEventKind.Moved) return;
            if (context.Event.Side != context.Owner) return;
            if (ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;
            if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                return;
            if (context.Event.SourceCard == null) return;
            if (context.Event.SourceCard.HerculesMovedTurn ==
                context.BoardState.CurrentTurn)
                return;

            context.Event.SourceCard.HerculesMovedTurn =
                context.BoardState.CurrentTurn;

            var zones = context.BoardState.Zones
                .Where(zone => zone.ZoneIndex != context.SourceZone.ZoneIndex &&
                               zone.CanAdd(context.Owner))
                .ToList();
            var zone = Series1EffectHelpers.PickRandom(zones, context.Random);
            if (zone != null)
                context.Actions.MoveCard(context.Event.SourceCard,
                    context.Owner, context.SourceZone.ZoneIndex,
                    zone.ZoneIndex, ActionCause.Effect);
        }
    }

    /// <summary>Nebula: +2 si el rival no jugo aqui este turno.</summary>
    public class EndOfTurnNebulaEffect : CardEffectBase
    {
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.BoardState == null)
                return;
            if (context.BoardState.CurrentTurn <= context.SourceCard.PlayedTurn)
                return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            bool playedHere = context.BoardState.PlayLog.Any(record =>
                record != null && record.Side == enemy &&
                record.ZoneIndex == context.SourceZone.ZoneIndex &&
                record.Turn == context.BoardState.CurrentTurn);
            if (!playedHere)
                context.AddPower(context.SourceCard, Amount);
        }
    }

    /// <summary>Juggernaut: mueve las cartas enemigas jugadas aqui este turno.</summary>
    public class OnRevealMoveEnemyPlayedHereEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null ||
                context.Actions == null)
                return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            var targets = context.BoardState.PlayLog
                .Where(record => record != null && record.Side == enemy &&
                                 record.ZoneIndex == context.SourceZone.ZoneIndex &&
                                 record.Turn == context.BoardState.CurrentTurn &&
                                 record.Card != null)
                .Select(record => record.Card)
                .Distinct()
                .ToList();

            foreach (var card in targets)
            {
                var zones = context.BoardState.Zones
                    .Where(zone => zone.ZoneIndex !=
                                   context.SourceZone.ZoneIndex &&
                                   zone.CanAdd(enemy))
                    .ToList();
                var zone = Series1EffectHelpers.PickRandom(zones, context.Random);
                if (zone == null) break;
                context.Actions.MoveCard(card, enemy,
                    context.SourceZone.ZoneIndex, zone.ZoneIndex,
                    ActionCause.Effect);
            }
        }
    }

    /// <summary>Spider-Man: se mueve y arrastra una carta enemiga con el.</summary>
    public class OnRevealMoveSelfAndPullEnemyEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.BoardState == null || context.Actions == null)
                return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            var zones = context.BoardState.Zones
                .Where(zone => zone.ZoneIndex != context.SourceZone.ZoneIndex &&
                               zone.CanAdd(context.Owner) &&
                               zone.CanAdd(enemy))
                .ToList();
            var zone = Series1EffectHelpers.PickRandom(zones, context.Random);
            if (zone == null) return;

            var enemies = context.SourceZone.CardsFor(enemy).ToList();
            var pulled = Series1EffectHelpers.PickRandom(enemies, context.Random);

            int from = context.SourceZone.ZoneIndex;
            context.Actions.MoveCard(context.SourceCard, context.Owner,
                from, zone.ZoneIndex, ActionCause.Effect);

            if (pulled != null)
                context.Actions.MoveCard(pulled, enemy, from, zone.ZoneIndex,
                    ActionCause.Effect);
        }
    }

    /// <summary>Crossbones: solo se juega donde ganas.</summary>
    public class CrossbonesRestrictionEffect : CardEffectBase,
        IPlayRestriction
    {
        public bool IsPlayable(EffectContext context)
        {
            if (context?.BoardState == null || context.TargetZone == null)
                return true;

            var zone = context.TargetZone;
            return context.Owner == BoardSide.Player
                ? zone.GetPlayerPower() > zone.GetAIPower()
                : zone.GetAIPower() > zone.GetPlayerPower();
        }
    }

    /// <summary>Giganto: solo se juega en la locacion izquierda.</summary>
    public class GigantoRestrictionEffect : CardEffectBase, IPlayRestriction
    {
        public bool IsPlayable(EffectContext context) =>
            context?.TargetZone == null || context.TargetZone.ZoneIndex == 0;
    }

    /// <summary>Sentry: no se juega en la locacion derecha.</summary>
    public class SentryRestrictionEffect : CardEffectBase, IPlayRestriction
    {
        public bool IsPlayable(EffectContext context) =>
            context?.TargetZone == null || context.TargetZone.ZoneIndex != 2;
    }

    /// <summary>Magik: convierte la locacion en Limbo hasta el turno 5.</summary>
    public class OnRevealReplaceWithLimboEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null ||
                context.Actions == null)
                return;
            if (context.BoardState.CurrentTurn > 5) return;

            context.Actions.ReplaceZoneWith(context.SourceZone.ZoneIndex,
                "zone_limbo");
        }
    }

    /// <summary>Absorbing Man / Mystique: copia el texto de la ultima carta jugada.</summary>
    public class OnRevealCopyLastPlayedEffectsEffect : CardEffectBase
    {
        public EffectTrigger Filter { get; set; } = EffectTrigger.OnReveal;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.BoardState == null ||
                context.EffectRegistry == null)
                return;

            PlayRecord last = null;
            foreach (var record in context.BoardState.PlayLog)
            {
                if (record == null || record.Side != context.Owner) continue;
                if (ReferenceEquals(record.Card, context.SourceCard)) continue;
                if (last == null || record.PlayOrder > last.PlayOrder)
                    last = record;
            }

            if (last?.Card == null) return;

            foreach (string effectId in PerkAbilities.EffectiveEffectIds(
                last.Card, true))
            {
                var definition = context.EffectRegistry.Get(effectId);
                if (definition == null || definition.Trigger != Filter) continue;
                if (!context.SourceCard.CopiedEffectIds.Contains(effectId))
                    context.SourceCard.CopiedEffectIds.Add(effectId);
            }
        }
    }

    /// <summary>Rogue: roba el texto Ongoing de una carta enemiga aqui.</summary>
    public class OnRevealStealOngoingEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.EffectRegistry == null)
                return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            var candidates = context.SourceZone.CardsFor(enemy)
                .Where(card => card != null)
                .ToList();
            var chosen = Series1EffectHelpers.PickRandom(candidates,
                context.Random);
            if (chosen == null) return;

            foreach (string effectId in PerkAbilities.EffectiveEffectIds(
                chosen, true))
            {
                var definition = context.EffectRegistry.Get(effectId);
                if (definition == null || definition.Trigger != EffectTrigger.Ongoing)
                    continue;

                if (!context.SourceCard.CopiedEffectIds.Contains(effectId))
                    context.SourceCard.CopiedEffectIds.Add(effectId);
                if (!chosen.SuppressedEffectIds.Contains(effectId))
                    chosen.SuppressedEffectIds.Add(effectId);
            }
        }
    }

    /// <summary>Super-Skrull: copia los auto-bonos Ongoing enemigos, un nivel.</summary>
    public class SuperSkrullOngoingEffect : CardEffectBase, ICardPowerModifier
    {
        public int GetPowerBonus(EffectContext context)
        {
            if (context.BoardState == null || context.SourceCard == null ||
                context.EffectRegistry == null)
                return 0;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            int total = 0;

            foreach (var zone in context.BoardState.Zones)
            {
                foreach (var card in zone.CardsFor(enemy))
                {
                    if (card == null || card.AbilitiesDisabled) continue;

                    var enemyContext = new EffectContext
                    {
                        SourceCard = card,
                        SourceZone = zone,
                        Owner = enemy,
                        BoardState = context.BoardState,
                        TurnNumber = context.TurnNumber,
                        CardRegistry = context.CardRegistry,
                        EffectRegistry = context.EffectRegistry,
                        Actions = context.Actions,
                        Random = context.Random
                    };

                    foreach (string effectId in PerkAbilities.EffectiveEffectIds(
                        card, !card.PrintedAbilitiesDisabled))
                    {
                        if (effectId == "effect_super_skrull") continue;
                        var definition = context.EffectRegistry.Get(effectId);
                        if (definition == null ||
                            definition.Trigger != EffectTrigger.Ongoing)
                            continue;
                        if (definition.EffectType == "SuperSkrullOngoing")
                            continue;

                        // Solo los auto-bonos Ongoing; las auras se omiten en
                        // esta version acotada para evitar recursion.
                        var implementation = context.EffectRegistry.Get(effectId);
                        var effect = BaseEffectFactory.CreateEffect(implementation);
                        if (effect is ICardPowerModifier modifier)
                            total += modifier.GetPowerBonus(enemyContext);
                    }
                }
            }

            return total;
        }
    }

    /// <summary>Black Widow: agrega una Mordida de Viuda a la mano rival.</summary>
    public class ActivateAddCardToEnemyHandEffect : CardEffectBase
    {
        public string CardId { get; set; } = "token_widows_bite";
        public int Count { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.CardRegistry == null || context.Actions == null) return;

            var definition = context.CardRegistry.Get(CardId);
            if (definition == null) return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            for (int i = 0; i < Count; i++)
                context.Actions.AddToHand(new CardInstance(definition), enemy);
        }
    }

    /// <summary>Hellcow: descarta la carta mas a la derecha de la mano.</summary>
    public class ActivateDiscardRightmostEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            if (hand.Count == 0) return;

            context.Actions.DiscardCard(hand[hand.Count - 1], context.Owner,
                ActionCause.Effect);
        }
    }

    /// <summary>Lockjaw: el siguiente personaje se intercambia con el mazo.</summary>
    public class ActivateArmNextPlaySwapEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            context.BoardState?.AddPendingNextPlay(context.Owner,
                new PendingNextPlayEffect
                {
                    Kind = PendingNextPlayKind.SwapWithDeck
                });
        }
    }

    /// <summary>Mysterio: intercambia Poder con su Ilusion.</summary>
    public class ActivateSwapPowerWithIllusionEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.BoardState == null)
                return;

            CardInstance illusion = null;
            foreach (var zone in context.BoardState.Zones)
            {
                illusion = zone.CardsFor(context.Owner)
                    .FirstOrDefault(card => card.Definition != null &&
                        card.Definition.Id == "token_illusion");
                if (illusion != null) break;
            }

            if (illusion == null) return;

            int sourcePower = context.SourceCard.CurrentPower;
            int illusionPower = illusion.CurrentPower;
            context.SourceCard.SetBasePower(illusionPower);
            illusion.SetBasePower(sourcePower);
            context.Actions?.RefreshPowers();
        }
    }

    /// <summary>Spider-Ham: reemplaza el texto de una carta rival por oink!.</summary>
    public class ActivateSpiderHamEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            var enemy = Series3EffectHelpers.Other(context.Owner);
            var hand = Series1EffectHelpers.HandFor(context.BoardState, enemy);
            var chosen = Series1EffectHelpers.PickRandom(hand, context.Random);
            if (chosen == null) return;

            chosen.AbilityTextOverride = "oink!";
            chosen.PrintedAbilitiesDisabled = true;
        }
    }

    /// <summary>Captain Marvel: al final, se mueve a una locacion ganadora.</summary>
    public class EndOfGameMoveToWinEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            MoveToBestZone(context, true);
        }

        internal static void MoveToBestZone(EffectContext context, bool win)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.BoardState == null || context.Actions == null)
                return;

            var source = context.SourceZone;
            int sourcePower = context.SourceCard.CurrentPower;
            ZoneInstance best = null;
            int bestMargin = win ? int.MinValue : int.MaxValue;

            foreach (var zone in context.BoardState.Zones)
            {
                if (ReferenceEquals(zone, source)) continue;
                if (!zone.CanAdd(context.Owner)) continue;

                int ownerPower = context.Owner == BoardSide.Player
                    ? zone.GetPlayerPower()
                    : zone.GetAIPower();
                int enemyPower = context.Owner == BoardSide.Player
                    ? zone.GetAIPower()
                    : zone.GetPlayerPower();
                int margin = ownerPower + sourcePower - enemyPower;

                if (win)
                {
                    if (margin > 0 && margin > bestMargin)
                    {
                        bestMargin = margin;
                        best = zone;
                    }
                }
                else
                {
                    if (margin < 0 && margin < bestMargin)
                    {
                        bestMargin = margin;
                        best = zone;
                    }
                }
            }

            if (best != null)
                context.Actions.MoveCard(context.SourceCard, context.Owner,
                    source.ZoneIndex, best.ZoneIndex, ActionCause.Effect);
        }
    }

    /// <summary>Martyr: al final, se mueve a una locacion perdedora.</summary>
    public class EndOfGameMoveToLoseEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            EndOfGameMoveToWinEffect.MoveToBestZone(context, false);
        }
    }

    /// <summary>M'Baku: al final, salta del mazo a la locacion mas debil.</summary>
    public class EndOfGameJumpFromDeckEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.BoardState == null ||
                context.Actions == null)
                return;

            var deck = context.Owner == BoardSide.Player
                ? context.BoardState.PlayerDeck
                : context.BoardState.AIDeck;
            if (!deck.Contains(context.SourceCard)) return;

            ZoneInstance best = null;
            int bestPower = int.MaxValue;
            foreach (var zone in context.BoardState.Zones)
            {
                if (!zone.CanAdd(context.Owner)) continue;
                int power = context.Owner == BoardSide.Player
                    ? zone.GetPlayerPower()
                    : zone.GetAIPower();
                if (power < bestPower)
                {
                    bestPower = power;
                    best = zone;
                }
            }

            if (best == null) return;

            context.Actions.RemoveFromContainers(context.SourceCard,
                context.Owner);
            context.SourceCard.HasRevealed = true;
            context.SourceCard.PlayedTurn = 0;
            context.SourceCard.PlayedZoneIndex = -1;
            context.Actions.AddCardToZone(context.SourceCard, context.Owner,
                best.ZoneIndex);
        }
    }

    /// <summary>Dracula: al final, descarta una carta y gana su Poder.</summary>
    public class EndOfGameDiscardGainPowerEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.BoardState == null ||
                context.Actions == null)
                return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);
            if (hand.Count == 0) return;

            var chosen = Series1EffectHelpers.PickRandom(hand, context.Random);
            if (chosen == null) return;

            int power = chosen.CurrentPower;
            context.Actions.DiscardCard(chosen, context.Owner,
                ActionCause.Effect);
            context.AddPower(context.SourceCard, power);
        }
    }

    /// <summary>Hawk: apaga los efectos de zona hasta el final del siguiente turno.</summary>
    public class OnRevealSuppressZoneEffectsEffect : CardEffectBase
    {
        public int TurnOffset { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;
            context.BoardState.SuppressZoneEffects(
                context.BoardState.CurrentTurn + TurnOffset);
        }
    }

    /// <summary>Bear: repite una vez el efecto simple de la zona actual.</summary>
    public class OnRevealRepeatZoneEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone?.Definition == null ||
                context.EffectRegistry == null)
                return;

            foreach (string effectId in context.SourceZone.Definition.EffectIds)
            {
                var definition = context.EffectRegistry.Get(effectId);
                if (definition == null ||
                    definition.EffectType != "ZonePowerModifier")
                    continue;

                int amount = 0;
                string target = "all";
                if (definition.Parameters != null)
                {
                    if (definition.Parameters.TryGetValue("amount",
                        out object value) && value is int parsed)
                        amount = parsed;
                    if (definition.Parameters.TryGetValue("target",
                        out object targetValue) && targetValue != null)
                        target = targetValue.ToString();
                }

                if (amount == 0) continue;

                var cards = context.SourceZone.PlayerCards
                    .Concat(context.SourceZone.AICards).ToList();
                if (target == "highest_cost")
                {
                    int max = cards.Where(c => c.Definition != null)
                        .Select(c => c.EffectiveCost)
                        .DefaultIfEmpty(int.MinValue)
                        .Max();
                    cards = cards.Where(c => c.Definition != null &&
                        c.EffectiveCost == max).ToList();
                }

                foreach (var card in cards)
                    context.AddPower(card, amount);
            }
        }
    }

    /// <summary>Agatha: marca el control de los turnos pares.</summary>
    public class AgathaGameStartEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState != null)
                context.BoardState.AgathaControlsEvenTurns = true;
        }
    }
}
