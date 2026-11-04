using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Effects.Implementations
{
    internal static class Series1EffectHelpers
    {
        public static BoardSide Other(BoardSide side) =>
            side == BoardSide.Player ? BoardSide.AI : BoardSide.Player;

        public static List<CardInstance> HandFor(BoardState board, BoardSide side) =>
            side == BoardSide.Player ? board.PlayerHand : board.AIHand;

        public static List<CardInstance> DeckFor(BoardState board, BoardSide side) =>
            side == BoardSide.Player ? board.PlayerDeck : board.AIDeck;

        public static List<CardInstance> AlliedCards(BoardState board, BoardSide side)
        {
            var cards = new List<CardInstance>();
            foreach (var zone in board.Zones)
                cards.AddRange(zone.CardsFor(side));
            cards.AddRange(HandFor(board, side));
            cards.AddRange(DeckFor(board, side));
            return cards;
        }

        public static T PickRandom<T>(List<T> list, RandomService random)
        {
            if (list == null || list.Count == 0) return default(T);
            int index = random != null ? random.Next(list.Count) : 0;
            return list[index];
        }

        public static void Shuffle<T>(List<T> list, RandomService random)
        {
            if (list == null || random == null) return;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static bool HasOngoing(CardInstance card, EffectRegistry registry)
        {
            if (card?.Definition?.EffectIds == null || registry == null) return false;

            foreach (var effectId in card.Definition.EffectIds)
            {
                var def = registry.Get(effectId);
                if (def != null && def.Trigger == EffectTrigger.Ongoing)
                    return true;
            }

            return false;
        }
    }

    public class OnRevealDestroyEffect : CardEffectBase
    {
        public string Target { get; set; } = "othersHere";
        public int PerDestroyed { get; set; }

        public override void Apply(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            var owner = context.Owner;
            var zone = context.SourceZone;

            if (Target == "enemy1CostHere")
            {
                var enemies = zone.CardsFor(Series1EffectHelpers.Other(owner))
                    .Where(c => c.Definition != null && c.Definition.Cost == 1)
                    .ToList();

                var chosen = Series1EffectHelpers.PickRandom(enemies, context.Random);
                if (chosen != null)
                    context.Actions.DestroyCard(chosen,
                        Series1EffectHelpers.Other(owner), ActionCause.Effect);
                return;
            }

            var cards = new List<CardInstance>(zone.CardsFor(owner));
            int destroyed = 0;

            foreach (var card in cards)
            {
                if (ReferenceEquals(card, context.SourceCard)) continue;

                var result = context.Actions.DestroyCard(card, owner, ActionCause.Effect);
                if (result != null && result.Success)
                    destroyed++;
            }

            if (PerDestroyed != 0)
                context.SourceCard.BonusPower += PerDestroyed * destroyed;
        }
    }

    public class OnRevealDiscardEffect : CardEffectBase
    {
        public string Selection { get; set; } = "rightmost";

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var hand = Series1EffectHelpers.HandFor(context.BoardState, context.Owner);
            if (hand.Count == 0) return;

            CardInstance chosen = null;

            if (Selection == "rightmost")
            {
                chosen = hand[hand.Count - 1];
            }
            else if (Selection == "highestCost")
            {
                int max = hand.Max(c => c.Definition.Cost);
                chosen = Series1EffectHelpers.PickRandom(
                    hand.Where(c => c.Definition.Cost == max).ToList(), context.Random);
            }
            else if (Selection == "oddCost")
            {
                chosen = Series1EffectHelpers.PickRandom(
                    hand.Where(c => c.Definition.Cost % 2 == 1).ToList(), context.Random);
            }

            if (chosen != null)
                context.Actions.DiscardCard(chosen, context.Owner, ActionCause.Effect);
        }
    }

    public class OnRevealAfflictEffect : CardEffectBase
    {
        public int Amount { get; set; } = -1;

        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null) return;

            foreach (var card in context.SourceZone.CardsFor(
                Series1EffectHelpers.Other(context.Owner)))
            {
                card.BonusPower += Amount;
            }
        }
    }

    public class OnRevealMoveEffect : CardEffectBase
    {
        public string Mode { get; set; } = "highestPowerToHere";

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.SourceZone == null ||
                context.Actions == null)
                return;

            var owner = context.Owner;
            var sourceZone = context.SourceZone;
            var moves = new List<CardMoveRequest>();

            if (Mode == "othersLeft")
            {
                foreach (var zone in context.BoardState.Zones)
                {
                    if (zone.ZoneIndex == 0) continue;

                    foreach (var card in zone.CardsFor(owner))
                    {
                        if (ReferenceEquals(card, context.SourceCard)) continue;
                        moves.Add(new CardMoveRequest
                        {
                            Card = card,
                            Side = owner,
                            FromZone = zone.ZoneIndex,
                            ToZone = zone.ZoneIndex - 1
                        });
                    }
                }
            }
            else
            {
                var candidates = new List<(CardInstance card, int zone, int power)>();

                foreach (var zone in context.BoardState.Zones)
                {
                    if (zone == sourceZone) continue;

                    foreach (var card in zone.CardsFor(owner))
                    {
                        if (ReferenceEquals(card, context.SourceCard)) continue;

                        int power = context.BoardState.PowerResolver != null
                            ? context.BoardState.PowerResolver.CalculateCardPower(
                                card, zone, context.BoardState, owner)
                            : card.CurrentPower;

                        candidates.Add((card, zone.ZoneIndex, power));
                    }
                }

                if (candidates.Count > 0)
                {
                    int max = candidates.Max(c => c.power);
                    var tied = candidates.Where(c => c.power == max).ToList();
                    Series1EffectHelpers.Shuffle(tied, context.Random);

                    foreach (var candidate in tied)
                    {
                        moves.Add(new CardMoveRequest
                        {
                            Card = candidate.card,
                            Side = owner,
                            FromZone = candidate.zone,
                            ToZone = sourceZone.ZoneIndex
                        });
                    }
                }
            }

            if (moves.Count > 0)
                context.Actions.MoveCards(moves, ActionCause.Effect);
        }
    }

    public class OnRevealAddTokenEffect : CardEffectBase
    {
        public string TokenId { get; set; }
        public string Target { get; set; } = "here";
        public bool CopySourcePower { get; set; }
        public int PowerBonus { get; set; }

        public override void Apply(EffectContext context)
        {
            if (context.CardRegistry == null || context.Actions == null) return;

            var definition = context.CardRegistry.Get(TokenId);
            if (definition == null) return;

            var targets = new List<ZoneInstance>();
            if (Target == "here")
            {
                if (context.SourceZone != null)
                    targets.Add(context.SourceZone);
            }
            else
            {
                foreach (var zone in context.BoardState.Zones)
                {
                    if (zone != context.SourceZone)
                        targets.Add(zone);
                }
            }

            foreach (var zone in targets)
            {
                if (!zone.CanAdd(context.Owner)) continue;

                var token = new CardInstance(definition)
                {
                    Location = CardLocation.Board,
                    IsInPlay = true,
                    PlayedTurn = 0,
                    PlayedZoneIndex = -1
                };

                if (PowerBonus != 0)
                    token.BonusPower += PowerBonus;
                if (CopySourcePower && context.SourceCard != null)
                {
                    int sourcePower = context.BoardState?.PowerResolver != null
                        ? context.BoardState.PowerResolver.CalculateCardPower(
                            context.SourceCard, context.SourceZone,
                            context.BoardState, context.Owner)
                        : context.SourceCard.CurrentPower;
                    token.BonusPower += sourcePower;
                }

                context.Actions.AddCardToZone(token, context.Owner, zone.ZoneIndex);
            }
        }
    }

    public class OnRevealCopyEffect : CardEffectBase
    {
        public string Source { get; set; } = "enemyHandRandom";
        public bool CopyToSelf { get; set; }

        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null || context.Actions == null) return;

            var owner = context.Owner;
            var enemy = Series1EffectHelpers.Other(owner);

            if (Source == "ownHand")
            {
                var hand = Series1EffectHelpers.HandFor(context.BoardState, owner);
                var snapshot = new List<CardInstance>(hand);

                foreach (var card in snapshot)
                {
                    if (hand.Count >= BoardState.MaxHandSize) break;

                    var copy = context.Actions.CreateCopy(card);
                    if (copy == null) continue;
                    if (!context.Actions.AddToHand(copy, owner).Success)
                        break;
                }

                return;
            }

            CardInstance source = null;

            if (Source == "enemyPlayedHere")
            {
                var records = context.BoardState.PlayLog
                    .Where(r => r.Side == enemy &&
                                r.Turn == context.TurnNumber &&
                                r.ZoneIndex == context.SourceZone?.ZoneIndex)
                    .ToList();

                if (records.Count > 0)
                    source = Series1EffectHelpers.PickRandom(records, context.Random)?.Card;
            }
            else if (Source == "enemyHandHighestCost")
            {
                var hand = Series1EffectHelpers.HandFor(context.BoardState, enemy);
                if (hand.Count > 0)
                {
                    int max = hand.Max(c => c.Definition.Cost);
                    source = Series1EffectHelpers.PickRandom(
                        hand.Where(c => c.Definition.Cost == max).ToList(),
                        context.Random);
                }
            }
            else
            {
                var hand = Series1EffectHelpers.HandFor(context.BoardState, enemy);
                source = Series1EffectHelpers.PickRandom(hand, context.Random);
            }

            if (source == null) return;

            if (CopyToSelf)
            {
                context.Actions.Transform(context.SourceCard, source.Definition);
                return;
            }

            var copyToHand = context.Actions.CreateCopy(source);
            if (copyToHand != null)
                context.Actions.AddToHand(copyToHand, owner);
        }
    }

    public class OnRevealMergeEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.Actions == null) return;

            var allies = context.SourceZone.CardsFor(context.Owner)
                .Where(c => !ReferenceEquals(c, context.SourceCard))
                .ToList();

            var target = Series1EffectHelpers.PickRandom(allies, context.Random);
            if (target != null)
                context.Actions.MergeInto(target, context.SourceCard, context.Owner);
        }
    }

    public class OnRevealReplaceLocationEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.Actions == null) return;
            context.Actions.ReplaceZone(context.SourceZone.ZoneIndex);
        }
    }

    public class NextPlayBonusEffect : CardEffectBase
    {
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            context.BoardState?.AddPendingNextPlay(context.Owner,
                new PendingNextPlayEffect
                {
                    Kind = PendingNextPlayKind.BonusPower,
                    Amount = Amount
                });
        }
    }

    public class NextPlayMoveEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            context.BoardState?.AddPendingNextPlay(context.Owner,
                new PendingNextPlayEffect { Kind = PendingNextPlayKind.MoveLeft });
        }
    }

    public class DeckTopBonusEffect : CardEffectBase
    {
        public int Amount { get; set; } = 3;

        public override void Apply(EffectContext context)
        {
            context.Actions?.BuffTopCard(context.Owner, Amount);
        }
    }

    public class DrawFromEnemyDeckEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            context.Actions?.DrawFromEnemyDeck(context.Owner);
        }
    }

    public class ShuffleTokenEffect : CardEffectBase
    {
        public string TokenId { get; set; }

        public override void Apply(EffectContext context)
        {
            if (context.CardRegistry == null || context.Actions == null) return;

            var definition = context.CardRegistry.Get(TokenId);
            if (definition != null)
                context.Actions.ShuffleTokenIntoDeck(
                    Series1EffectHelpers.Other(context.Owner), definition);
        }
    }

    public class BanishLowestCostEffect : CardEffectBase
    {
        public bool CombatScoped { get; set; } = true;

        public override void Apply(EffectContext context)
        {
            context.Actions?.BanishLowestCost(
                Series1EffectHelpers.Other(context.Owner), CombatScoped);
        }
    }

    public class AfterPlayedCounterEffect : CardEffectBase
    {
        public string Scope { get; set; } = "global";
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null) return;
            if (context.Event.Side != context.Owner) return;
            if (ReferenceEquals(context.Event.SourceCard, context.SourceCard)) return;

            if (Scope == "here")
            {
                if (context.SourceZone == null) return;
                if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                    return;
            }

            context.SourceCard.BonusPower += Amount;
        }
    }

    public class WhenDestroyedBuffAlliesEffect : CardEffectBase
    {
        public int Amount { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.BoardState == null)
                return;

            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;

            foreach (var card in Series1EffectHelpers.AlliedCards(
                context.BoardState, context.Owner))
            {
                if (ReferenceEquals(card, context.SourceCard)) continue;
                card.BonusPower += Amount;
            }
        }
    }

    public class WhenMovedHereEffect : CardEffectBase
    {
        public int Amount { get; set; } = 2;

        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceZone == null) return;
            if (ReferenceEquals(context.Event.SourceCard, context.SourceCard)) return;
            if (context.Event.DestinationZoneIndex != context.SourceZone.ZoneIndex)
                return;

            context.SourceCard.BonusPower += Amount;
        }
    }

    public class SelfMovedCopyEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.Actions == null)
                return;

            if (!ReferenceEquals(context.Event.SourceCard, context.SourceCard))
                return;
            if (context.Event.OriginZoneIndex < 0) return;

            var copy = context.Actions.CreateCopy(context.SourceCard);
            if (copy != null)
                context.Actions.AddCardToZone(copy, context.Owner,
                    context.Event.OriginZoneIndex);
        }
    }

    public class AllyDestroyedFlyInEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.Event == null || context.SourceCard == null ||
                context.Actions == null || context.BoardState == null)
                return;

            if (context.Event.Side != context.Owner) return;
            if (context.Event.OriginZoneIndex < 0) return;
            if (context.SourceCard.IsInPlay) return;
            if (context.SourceCard.Location != CardLocation.Hand &&
                context.SourceCard.Location != CardLocation.Deck)
                return;

            context.Actions.AddCardToZone(context.SourceCard, context.Owner,
                context.Event.OriginZoneIndex);
        }
    }

    public class RemoveAbilitiesEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null || context.EffectRegistry == null) return;

            var cards = context.SourceZone.PlayerCards
                .Concat(context.SourceZone.AICards)
                .ToList();

            foreach (var card in cards)
            {
                if (ReferenceEquals(card, context.SourceCard)) continue;
                if (Series1EffectHelpers.HasOngoing(card, context.EffectRegistry))
                    card.AbilitiesDisabled = true;
            }
        }
    }

    public class GameStartVisibilityEffect : CardEffectBase
    {
        public override void Apply(EffectContext context)
        {
            if (context.BoardState == null) return;

            if (context.Owner == BoardSide.Player)
                context.BoardState.PlayerSeesHiddenZones = true;
            else
                context.BoardState.AISeesHiddenZones = true;
        }
    }

    public class ApocalypseDiscardReplacement : CardEffectBase,
        ICardDiscardReplacement
    {
        public int Amount { get; set; } = 4;

        public bool TryReplaceDiscard(EffectContext context)
        {
            if (context?.SourceCard == null) return false;

            context.SourceCard.BonusPower += Amount;
            return true;
        }
    }

    public class WolverineRegenerateEffect : CardEffectBase,
        ICardDiscardReplacement, ICardDestroyReplacement
    {
        public int Amount { get; set; } = 2;

        public bool TryReplaceDiscard(EffectContext context) =>
            Regenerate(context);

        public bool TryReplaceDestroy(EffectContext context) =>
            Regenerate(context);

        private bool Regenerate(EffectContext context)
        {
            if (context?.SourceCard == null || context.BoardState == null)
                return false;

            var card = context.SourceCard;
            card.BonusPower += Amount;

            var zones = context.BoardState.Zones
                .Where(z => z.CanAdd(context.Owner))
                .ToList();

            var target = Series1EffectHelpers.PickRandom(zones, context.Random);
            if (target != null && context.Actions != null)
            {
                var placed = context.Actions.AddCardToZone(card, context.Owner,
                    target.ZoneIndex);
                if (placed.Success) return true;
            }

            context.Actions?.RemoveFromContainers(card, context.Owner);
            card.Location = CardLocation.Destroyed;
            card.IsInPlay = false;
            return true;
        }
    }

    public class CaptainAmericaAuraEffect : CardEffectBase, ICardAura
    {
        public int Amount { get; set; } = 2;

        public int GetBonusForCard(EffectContext sourceContext,
            CardInstance target, ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext.SourceCard == null || sourceContext.SourceZone == null)
                return 0;
            if (targetZone == null ||
                sourceContext.SourceZone.ZoneIndex != targetZone.ZoneIndex)
                return 0;
            if (sourceContext.Owner != targetSide) return 0;
            if (ReferenceEquals(sourceContext.SourceCard, target)) return 0;
            if (target.AbilitiesDisabled) return 0;
            if (!Series1EffectHelpers.HasOngoing(target, sourceContext.EffectRegistry))
                return 0;

            return Amount;
        }
    }

    public class KlawZoneAdditionEffect : CardEffectBase, IZonePowerAddition
    {
        public int Amount { get; set; } = 6;

        public int GetAdditionForZone(EffectContext sourceContext,
            ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext.SourceZone == null || targetZone == null) return 0;
            if (sourceContext.Owner != targetSide) return 0;
            if (sourceContext.SourceZone.ZoneIndex + 1 != targetZone.ZoneIndex)
                return 0;

            return Amount;
        }
    }

    public class OngoingConditionalPowerEffect : CardEffectBase, ICardPowerModifier
    {
        public string Condition { get; set; }
        public int Amount { get; set; }

        public int GetPowerBonus(EffectContext context)
        {
            if (context.SourceCard == null || context.SourceZone == null ||
                context.BoardState == null)
                return 0;

            var hand = Series1EffectHelpers.HandFor(context.BoardState, context.Owner);

            switch (Condition)
            {
                case "handCount":
                    return Amount * hand.Count;
                case "enemyFourHere":
                    return context.SourceZone.CardsFor(
                        Series1EffectHelpers.Other(context.Owner)).Count >= 4
                        ? Amount : 0;
                case "onlyCardHere":
                    return context.SourceZone.CardsFor(context.Owner).Count == 1
                        ? Amount : 0;
                case "handAtMostOne":
                    return hand.Count <= 1 ? Amount : 0;
                default:
                    return 0;
            }
        }
    }

    public class ZonePowerModifierEffect : CardEffectBase, IZoneCardPowerModifier
    {
        public int Amount { get; set; }
        public string Target { get; set; } = "all";

        public int GetBonusForCard(ZoneInstance zone, CardInstance card, BoardSide side)
        {
            if (zone == null || !zone.IsRevealed) return 0;
            if (Target == "all") return Amount;

            if (Target == "highest_cost")
            {
                int max = 0;
                bool any = false;

                foreach (var other in zone.PlayerCards.Concat(zone.AICards))
                {
                    if (other.Definition == null) continue;
                    any = true;
                    if (other.Definition.Cost > max)
                        max = other.Definition.Cost;
                }

                return any && card.Definition != null && card.Definition.Cost == max
                    ? Amount : 0;
            }

            return 0;
        }
    }

    public class DestroyProtectionEffect : CardEffectBase { }
    public class SuppressOnRevealEffect : CardEffectBase { }
    public class ZoneLockEffect : CardEffectBase { }
    public class OngoingRepeatEffect : CardEffectBase { }
}
