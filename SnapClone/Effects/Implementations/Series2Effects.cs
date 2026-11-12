using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Effects.Implementations
{
    internal static class Series2EffectHelpers
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
    }

    /// <summary>Wolfsbane: +2 por cada otra carta aliada en la zona.</summary>
    public class OnRevealBonusPerOtherCardHereEffect : CardEffectBase
    {
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null) return;

            int others = context.SourceZone.CardsFor(context.Owner)
                .Count(card => !ReferenceEquals(card, context.SourceCard));

            context.AddPower(context.SourceCard, Amount * others);
        }
    }

    /// <summary>Nakia y Okoye: buff persistente a la mano o al mazo.</summary>
    public class OnRevealBuffContainerEffect : CardEffectBase
    {
        public string Target { get; set; } = "hand";
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            var cards = Target == "deck"
                ? Series1EffectHelpers.DeckFor(context.BoardState, context.Owner)
                : Series1EffectHelpers.HandFor(context.BoardState, context.Owner);

            foreach (var card in cards.ToList())
                context.AddPower(card, Amount);
        }
    }

    /// <summary>Scorpion: -1 persistente a cada carta de la mano rival.</summary>
    public class OnRevealAfflictHandEffect : CardEffectBase
    {
        public int Amount { get; set; } = -1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            var enemy = Series1EffectHelpers.Other(context.Owner);
            foreach (var card in Series1EffectHelpers.HandFor(
                context.BoardState, enemy).ToList())
            {
                context.AddPower(card, Amount);
            }
        }
    }

    /// <summary>Agent 13: agrega una carta aleatoria no-token a la mano.</summary>
    public class OnRevealAddRandomCardToHandEffect : CardEffectBase
    {
        public int MinCost { get; set; }
        public int MaxCost { get; set; } = 6;
        public int Count { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.CardRegistry == null || context.BoardState == null ||
                context.Actions == null)
                return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState,
                context.Owner);

            for (int i = 0; i < Math.Max(1, Count); i++)
            {
                if (hand.Count >= BoardState.MaxHandSize) break;

                var pool = context.CardRegistry.GetDraftable()
                    .Where(card => card != null && !card.IsToken &&
                                   card.Cost >= MinCost &&
                                   card.Cost <= MaxCost)
                    .ToList();

                var definition = Series1EffectHelpers.PickRandom(pool,
                    context.Random);
                if (definition == null) break;

                context.Actions.AddToHand(new CardInstance(definition),
                    context.Owner);
            }
        }
    }

    /// <summary>Spectrum: +2 persistente a cada carta aliada con Ongoing.</summary>
    public class OnRevealBuffOngoingEffect : CardEffectBase
    {
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.EffectRegistry == null)
                return;

            foreach (var card in Series2EffectHelpers.AlliedCardsInPlay(
                context.BoardState, context.Owner))
            {
                if (card.AbilitiesDisabled) continue;
                if (!Series1EffectHelpers.HasOngoing(card,
                        context.EffectRegistry))
                    continue;

                context.AddPower(card, Amount);
            }
        }
    }

    /// <summary>Leech: apaga el texto impreso de las cartas caras de la mano rival.</summary>
    public class OnRevealDisableHighCostHandEffect : CardEffectBase
    {
        public int Threshold { get; set; } = 6;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            var enemy = Series1EffectHelpers.Other(context.Owner);
            foreach (var card in Series1EffectHelpers.HandFor(
                context.BoardState, enemy))
            {
                if (card.EffectiveCost >= Threshold)
                    card.PrintedAbilitiesDisabled = true;
            }
        }
    }

    /// <summary>Jubilee: mueve la carta superior del mazo a su locacion y la revela.</summary>
    public class OnRevealPullTopDeckEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.Actions == null) return;

            context.Actions.PullTopDeckToZone(context.Owner,
                context.SourceZone.ZoneIndex, true);
        }
    }

    /// <summary>Odin: marca la carta para la pasada de repeticion de OnReveal.</summary>
    public class OdinRepeatEffect : CardEffectBase, IRepeatOnRevealAtLocation
    {
    }

    /// <summary>Jessica Jones: arma el bonus si no se juega en la zona el siguiente turno.</summary>
    public class OnRevealArmNotPlayedNextTurnEffect : CardEffectBase
    {
        public int Amount { get; set; } = 5;

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.BoardState == null)
                return;

            context.SourceCard.PendingEffects.Add(new PendingCardEffect
            {
                Kind = PendingEffectKind.AllyNotPlayedInZoneNextTurn,
                ArmedTurn = context.BoardState.CurrentTurn,
                ZoneIndex = context.SourceZone.ZoneIndex,
                Amount = Amount
            });
        }
    }

    /// <summary>Cloak: abre una ventana para mover cartas a esta locacion el siguiente turno.</summary>
    public class OnRevealOpenMoveWindowEffect : CardEffectBase
    {
        public int TurnOffset { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null) return;

            context.BoardState.AddMoveWindow(context.SourceZone.ZoneIndex,
                context.BoardState.CurrentTurn + TurnOffset);
        }
    }

    /// <summary>Hobgoblin: cambia de lado en la misma locacion.</summary>
    public class OnRevealSwitchSidesEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            context.Actions.SwitchSides(context.SourceCard, context.Owner,
                context.SourceZone.ZoneIndex);
        }
    }

    /// <summary>Storm: inunda la locacion; el siguiente turno es el ultimo de juego.</summary>
    public class OnRevealFloodZoneEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.BoardState == null) return;

            context.SourceZone.Flood(context.BoardState.CurrentTurn);
        }
    }

    /// <summary>Sandman: el siguiente turno todas las cartas cuestan 1 mas.</summary>
    public class OnRevealNextTurnCostTaxEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            context.BoardState.AddNextTurnCostTax(
                context.BoardState.CurrentTurn + 1, Amount);
        }
    }

    /// <summary>Iceman: +1 coste de combate a una carta aleatoria de la mano rival.</summary>
    public class OnRevealRaiseEnemyCostEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;
        public int Cap { get; set; } = 6;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            var enemy = Series1EffectHelpers.Other(context.Owner);
            var candidates = Series1EffectHelpers.HandFor(context.BoardState,
                enemy).Where(card => card.EffectiveCost < Cap).ToList();

            var chosen = Series1EffectHelpers.PickRandom(candidates,
                context.Random);
            if (chosen != null)
                chosen.TemporaryCostDelta += Amount;
        }
    }

    /// <summary>Ka-Zar: tus cartas de coste 1 tienen +1.</summary>
    public class OngoingCostAuraEffect : CardEffectBase, ICardAura
    {
        public int Cost { get; set; } = 1;
        public int Amount { get; set; } = 1;

        public int GetBonusForCard(EffectContext sourceContext,
            CardInstance target, ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext.Owner != targetSide) return 0;
            if (target == null || target.AbilitiesDisabled) return 0;
            if (targetZone == null) return 0;

            return target.EffectiveCost == Cost ? Amount : 0;
        }
    }

    /// <summary>Vulture: +4 cuando esta carta se mueve.</summary>
    public class AfterMovedSelfBuffEffect : CardEffectBase
    {
        public int Amount { get; set; } = 4;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null) return;
            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;
            if (context.Event.OriginZoneIndex ==
                context.Event.DestinationZoneIndex)
                return;

            context.AddPower(context.SourceCard, Amount);
        }
    }

    /// <summary>Bucky Barnes: se reemplaza por el Soldado del Invierno.</summary>
    public class AfterDestroyedReplaceTokenEffect : CardEffectBase,
        ICardDestroyReplacement
    {
        public string TokenId { get; set; }

        public bool TryReplaceDestroy(EffectContext context)
        {
            if (context?.SourceCard == null || context.Actions == null ||
                context.CardRegistry == null)
                return false;

            var definition = context.CardRegistry.Get(TokenId);
            if (definition == null) return false;

            var result = context.Actions.ReplaceWithToken(context.SourceCard,
                context.Owner, definition);
            return result != null && result.Success;
        }
    }

    /// <summary>Sabretooth: vuelve a la mano a coste 0.</summary>
    public class AfterDestroyedReturnZeroCostEffect : CardEffectBase,
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

            context.SourceCard.CombatCostOverride = 0;
            var added = context.Actions.AddToHand(context.SourceCard,
                context.Owner);
            return added != null && added.Success;
        }
    }

    /// <summary>Swarm: al descartarse agrega dos copias a coste 0.</summary>
    public class AfterDiscardedAddZeroCostCopiesEffect : CardEffectBase
    {
        public int Copies { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.Actions == null || context.BoardState == null)
                return;

            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;

            for (int i = 0; i < Copies; i++)
            {
                var hand = Series1EffectHelpers.HandFor(context.BoardState,
                    context.Owner);
                if (hand.Count >= BoardState.MaxHandSize) break;

                var copy = context.Actions.CreateCopy(context.SourceCard);
                if (copy == null) break;

                copy.CombatCostOverride = 0;
                if (!context.Actions.AddToHand(copy, context.Owner).Success)
                    break;
            }
        }
    }

    /// <summary>The Collector: +1 cuando una carta entra a su mano.</summary>
    public class AfterAddedToHandSelfBuffEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null) return;
            if (context.Event.Side != context.Owner) return;
            if (ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;

            context.AddPower(context.SourceCard, Amount);
        }
    }

    /// <summary>Sunspot: +1 por cada energia sin gastar al final del turno.</summary>
    public class EndOfTurnUnspentEnergyEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            int available = context.BoardState.GetEnergy(context.Owner)
                .Available(context.BoardState.CurrentEnergy);

            if (available > 0)
                context.AddPower(context.SourceCard, available * Amount);
        }
    }

    /// <summary>Restriccion de jugada impresa (Ebony Maw, The Infinaut).</summary>
    public class PassivePlayRestrictionEffect : CardEffectBase, IPlayRestriction
    {
        public int MaxPlayTurn { get; set; }
        public bool RequiresNoPlayLastTurn { get; set; }

        public bool IsPlayable(EffectContext context)
        {
            if (context?.BoardState == null) return true;

            if (MaxPlayTurn > 0 && context.BoardState.CurrentTurn > MaxPlayTurn)
                return false;

            if (RequiresNoPlayLastTurn &&
                context.BoardState.HasPlayedOnTurn(context.Owner,
                    context.BoardState.CurrentTurn - 1))
                return false;

            return true;
        }
    }

    /// <summary>Permiso de movimiento impreso (Nightcrawler, Vision).</summary>
    public class PassiveMovePermissionEffect : CardEffectBase, IMovePermission
    {
        public MovePermission Frequency { get; set; } =
            MovePermission.OncePerCombat;

        public override void Apply(EffectContext context)
        {
        }
    }

    /// <summary>Ebony Maw: bloquea jugar cartas en su locacion, no las adiciones.</summary>
    public class PlayLockEffect : CardEffectBase
    {
    }
}
