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

        public static int PowerOf(EffectContext context, CardInstance card,
            ZoneInstance zone, BoardSide side)
        {
            if (card == null) return 0;

            return context.BoardState?.PowerResolver != null
                ? context.BoardState.PowerResolver.CalculateCardPower(
                    card, zone, context.BoardState, side)
                : card.CurrentPower;
        }
    }

    public class OnRevealDestroyEffect : CardEffectBase
    {
        public string Target { get; set; } = "othersHere";
        public int PerDestroyed { get; set; }
        public bool AddDestroyedPower { get; set; }

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
                    .Where(c => c.Definition != null && c.EffectiveCost == 1)
                    .ToList();

                var chosen = Series1EffectHelpers.PickRandom(enemies, context.Random);
                if (chosen != null)
                    context.Actions.DestroyCard(chosen,
                        Series1EffectHelpers.Other(owner), ActionCause.Effect);
                return;
            }

            if (Target == "enemy10PlusHere")
            {
                var enemy = Series1EffectHelpers.Other(owner);
                var enemies = zone.CardsFor(enemy)
                    .Where(c => c.Definition != null &&
                                Series1EffectHelpers.PowerOf(context, c, zone,
                                    enemy) >= 10)
                    .ToList();

                var chosen = Series1EffectHelpers.PickRandom(enemies, context.Random);
                if (chosen != null)
                    context.Actions.DestroyCard(chosen, enemy, ActionCause.Effect);
                return;
            }

            if (Target == "all1Cost")
            {
                var targets = new List<(CardInstance Card, BoardSide Side)>();

                foreach (var targetZone in context.BoardState.Zones)
                {
                    foreach (var card in targetZone.PlayerCards)
                    {
                        if (card.Definition != null && card.EffectiveCost == 1)
                            targets.Add((card, BoardSide.Player));
                    }

                    foreach (var card in targetZone.AICards)
                    {
                        if (card.Definition != null && card.EffectiveCost == 1)
                            targets.Add((card, BoardSide.AI));
                    }
                }

                foreach (var target in targets)
                    context.Actions.DestroyCard(target.Card, target.Side,
                        ActionCause.Effect);

                return;
            }

            if (Target == "allHereAtMost3Power")
            {
                var all = zone.PlayerCards.Concat(zone.AICards).ToList();
                foreach (var card in all)
                {
                    if (ReferenceEquals(card, context.SourceCard)) continue;
                    var side = zone.PlayerCards.Contains(card)
                        ? BoardSide.Player
                        : BoardSide.AI;
                    int power = Series1EffectHelpers.PowerOf(context, card,
                        zone, side);
                    if (power > 3) continue;
                    context.Actions.DestroyCard(card, side, ActionCause.Effect);
                }

                return;
            }

            if (Target == "othersAtOtherLocations")
            {
                foreach (var targetZone in context.BoardState.Zones)
                {
                    if (targetZone == zone) continue;
                    foreach (var card in targetZone.CardsFor(owner).ToList())
                        context.Actions.DestroyCard(card, owner,
                            ActionCause.Effect);
                }

                return;
            }

            var cards = new List<CardInstance>(zone.CardsFor(owner));
            int destroyed = 0;
            int destroyedPower = 0;

            foreach (var card in cards)
            {
                if (ReferenceEquals(card, context.SourceCard)) continue;

                int power = Series1EffectHelpers.PowerOf(context, card, zone,
                    owner);
                var result = context.Actions.DestroyCard(card, owner, ActionCause.Effect);
                if (result != null && result.Success)
                {
                    destroyed++;
                    destroyedPower += power;
                }
            }

            if (AddDestroyedPower && destroyedPower != 0)
                context.AddPower(context.SourceCard, destroyedPower);

            if (PerDestroyed != 0)
                context.AddPower(context.SourceCard, PerDestroyed * destroyed);
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
                int max = hand.Max(c => c.EffectiveCost);
                chosen = Series1EffectHelpers.PickRandom(
                    hand.Where(c => c.EffectiveCost == max).ToList(), context.Random);
            }
            else if (Selection == "oddCost")
            {
                chosen = Series1EffectHelpers.PickRandom(
                    hand.Where(c => c.EffectiveCost % 2 == 1).ToList(), context.Random);
            }
            else if (Selection == "leastCost")
            {
                int min = hand.Min(c => c.EffectiveCost);
                chosen = Series1EffectHelpers.PickRandom(
                    hand.Where(c => c.EffectiveCost == min).ToList(),
                    context.Random);
            }
            else if (Selection == "all")
            {
                foreach (var card in hand.ToList())
                    context.Actions.DiscardCard(card, context.Owner,
                        ActionCause.Effect);
                return;
            }

            if (chosen != null)
                context.Actions.DiscardCard(chosen, context.Owner, ActionCause.Effect);
        }
    }

    public class OnRevealAfflictEffect : CardEffectBase
    {
        public int Amount { get; set; } = -1;
        public string Target { get; set; } = "enemy";

        public override void Apply(EffectContext context)
        {
            if (context.SourceZone == null) return;

            var side = Target == "ownOthers"
                ? context.Owner
                : Series1EffectHelpers.Other(context.Owner);

            foreach (var card in context.SourceZone.CardsFor(side))
            {
                if (Target == "ownOthers" &&
                    ReferenceEquals(card, context.SourceCard))
                    continue;
                context.AddPower(card, Amount);
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
            var enemy = Series1EffectHelpers.Other(owner);

            if (Mode == "enemyLowCostToHere")
            {
                var candidates = new List<(CardInstance card, int zone)>();
                foreach (var zone in context.BoardState.Zones)
                {
                    if (zone == sourceZone) continue;
                    foreach (var card in zone.CardsFor(enemy))
                    {
                        if (card.Definition != null && card.EffectiveCost <= 2)
                            candidates.Add((card, zone.ZoneIndex));
                    }
                }

                var chosen = Series1EffectHelpers.PickRandom(
                    candidates.Select(c => c.card).ToList(), context.Random);
                if (chosen != null)
                {
                    int from = candidates.First(c =>
                        ReferenceEquals(c.card, chosen)).zone;
                    moves.Add(new CardMoveRequest
                    {
                        Card = chosen,
                        Side = enemy,
                        FromZone = from,
                        ToZone = sourceZone.ZoneIndex
                    });
                }
            }
            else if (Mode == "enemyHereToRight")
            {
                int to = sourceZone.ZoneIndex + 1;
                if (to >= context.BoardState.Zones.Count) return;

                foreach (var card in sourceZone.CardsFor(enemy))
                {
                    moves.Add(new CardMoveRequest
                    {
                        Card = card,
                        Side = enemy,
                        FromZone = sourceZone.ZoneIndex,
                        ToZone = to
                    });
                }
            }
            else if (Mode == "enemyCost34ToHere")
            {
                foreach (var zone in context.BoardState.Zones)
                {
                    if (zone == sourceZone) continue;
                    foreach (var card in zone.CardsFor(enemy))
                    {
                        if (card.Definition == null) continue;
                        if (card.EffectiveCost != 3 && card.EffectiveCost != 4)
                            continue;
                        moves.Add(new CardMoveRequest
                        {
                            Card = card,
                            Side = enemy,
                            FromZone = zone.ZoneIndex,
                            ToZone = sourceZone.ZoneIndex
                        });
                    }
                }
            }
            else if (Mode == "othersLeft")
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
        public bool BothSides { get; set; }
        public int Copies { get; set; } = 1;
        public bool CopySourcePowerAsSet { get; set; }

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
            else if (Target == "randomOtherLocation")
            {
                var candidates = context.BoardState.Zones
                    .Where(zone => zone != context.SourceZone &&
                                   zone.CanAdd(context.Owner))
                    .ToList();

                var chosen = Series1EffectHelpers.PickRandom(candidates,
                    context.Random);
                if (chosen != null)
                    targets.Add(chosen);
            }
            else if (Target == "right")
            {
                if (context.BoardState.Zones.Count > 2)
                    targets.Add(context.BoardState.Zones[2]);
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
                var sides = BothSides
                    ? new[] { context.Owner, Series1EffectHelpers.Other(context.Owner) }
                    : new[] { context.Owner };

                foreach (var side in sides)
                {
                    if (!zone.CanAdd(side)) continue;

                    for (int copy = 0; copy < Math.Max(1, Copies); copy++)
                    {
                        if (!zone.CanAdd(side)) break;

                        var token = new CardInstance(definition)
                        {
                            Location = CardLocation.Board,
                            IsInPlay = true,
                            PlayedTurn = 0,
                            PlayedZoneIndex = -1
                        };

                        int sourcePower = 0;
                        if ((CopySourcePower || CopySourcePowerAsSet) &&
                            context.SourceCard != null)
                        {
                            sourcePower = context.BoardState?.PowerResolver != null
                                ? context.BoardState.PowerResolver.CalculateCardPower(
                                    context.SourceCard, context.SourceZone,
                                    context.BoardState, context.Owner)
                                : context.SourceCard.CurrentPower;
                        }

                        if (CopySourcePowerAsSet)
                            token.SetBasePower(sourcePower);
                        else if (CopySourcePower)
                            token.BonusPower += sourcePower;

                        if (PowerBonus != 0)
                            token.BonusPower += PowerBonus;

                        context.Actions.AddCardToZone(token, side, zone.ZoneIndex);
                    }
                }
            }
        }
    }

    public class OnRevealCopyEffect : CardEffectBase
    {
        public string Source { get; set; } = "enemyHandRandom";
        public bool CopyToSelf { get; set; }
        public int PowerBonus { get; set; }

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
                    int max = hand.Max(c => c.EffectiveCost);
                    source = Series1EffectHelpers.PickRandom(
                        hand.Where(c => c.EffectiveCost == max).ToList(),
                        context.Random);
                }
            }
            else if (Source == "enemyHandLowestCost")
            {
                var hand = Series1EffectHelpers.HandFor(context.BoardState, enemy);
                if (hand.Count > 0)
                {
                    int min = hand.Min(c => c.EffectiveCost);
                    source = Series1EffectHelpers.PickRandom(
                        hand.Where(c => c.EffectiveCost == min).ToList(),
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
            {
                if (PowerBonus != 0)
                    copyToHand.BonusPower += PowerBonus;
                context.Actions.AddToHand(copyToHand, owner);
            }
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
        public string Target { get; set; } = "opponent";
        public int Copies { get; set; } = 1;

        public override void Apply(EffectContext context)
        {
            if (context.CardRegistry == null || context.Actions == null) return;

            var definition = context.CardRegistry.Get(TokenId);
            if (definition == null) return;

            var side = Target == "owner"
                ? context.Owner
                : Series1EffectHelpers.Other(context.Owner);
            for (int i = 0; i < Math.Max(1, Copies); i++)
                context.Actions.ShuffleTokenIntoDeck(side, definition);
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

            context.AddPower(context.SourceCard, Amount);
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
                context.AddPower(card, Amount);
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

            context.AddPower(context.SourceCard, Amount);
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

            context.AddPower(context.SourceCard, Amount);
            context.Actions?.NotifyAddedToHand(context.SourceCard,
                context.Owner, CardLocation.Discard);
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

            var zones = context.BoardState.Zones
                .Where(z => z.CanAdd(context.Owner))
                .ToList();

            var target = Series1EffectHelpers.PickRandom(zones, context.Random);

            // Sin espacio la carta no regenera: la accion normal la mueve a su
            // pila de descartadas o destruidas.
            if (target == null || context.Actions == null)
                return false;

            var placed = context.Actions.AddCardToZone(card, context.Owner,
                target.ZoneIndex);
            if (!placed.Success)
                return false;

            context.AddPower(card, Amount);
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
        public string Direction { get; set; } = "right";

        public int GetAdditionForZone(EffectContext sourceContext,
            ZoneInstance targetZone, BoardSide targetSide)
        {
            if (sourceContext.SourceZone == null || targetZone == null) return 0;
            if (sourceContext.Owner != targetSide) return 0;

            int delta = targetZone.ZoneIndex - sourceContext.SourceZone.ZoneIndex;

            if (Direction == "both")
                return Math.Abs(delta) == 1 ? Amount : 0;

            return delta == 1 ? Amount : 0;
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
                case "discardCount":
                    return Amount * context.BoardState.GetDiscardCount(context.Owner);
                case "anyAllyZoneEmpty":
                    foreach (var zone in context.BoardState.Zones)
                    {
                        if (zone.CardsFor(context.Owner).Count == 0)
                            return Amount;
                    }

                    return 0;
                case "enemyHandCount":
                    return Amount * Series1EffectHelpers.HandFor(
                        context.BoardState,
                        Series1EffectHelpers.Other(context.Owner)).Count;
                case "fullAllyLocations":
                    int full = 0;
                    foreach (var zone in context.BoardState.Zones)
                    {
                        if (zone.CardsFor(context.Owner).Count >= 4)
                            full++;
                    }

                    return Amount * full;
                case "bothSidesFull":
                    return context.SourceZone.PlayerCards.Count >= 4 &&
                           context.SourceZone.AICards.Count >= 4
                        ? Amount : 0;
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
                    if (other.EffectiveCost > max)
                        max = other.EffectiveCost;
                }

                return any && card.Definition != null && card.EffectiveCost == max
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
