using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Effects;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Run;

namespace SnapClone.Gameplay.Actions
{
    /// <summary>
    /// Autoridad unica de las acciones de carta de un combate. La escena y la
    /// IA llaman aqui; ninguna de las dos muta zonas, manos o pilas por su cuenta.
    /// No conoce tipos de MonoGame.
    /// </summary>
    public class CardActionService : ICardEffectActions
    {
        private readonly BoardState _board;
        private readonly CardPileManager _pileManager;
        private readonly PowerResolver _powerResolver;
        private readonly EffectProcessor _effectProcessor;
        private readonly ZoneRegistry _zoneRegistry;
        private readonly RandomService _random;
        private readonly HashSet<CardInstance> _liftedCards =
            new HashSet<CardInstance>();
        private bool _gameStarted;

        public CardActionService(BoardState boardState,
            CardPileManager pileManager,
            PowerResolver powerResolver,
            EffectProcessor effectProcessor,
            RunState runState = null,
            ZoneRegistry zoneRegistry = null)
        {
            _board = boardState;
            _pileManager = pileManager;
            _powerResolver = powerResolver;
            _effectProcessor = effectProcessor;
            _zoneRegistry = zoneRegistry;
            _random = effectProcessor?.Random ?? new RandomService(1);

            if (_effectProcessor != null)
            {
                _effectProcessor.Actions = this;
                _effectProcessor.BoundBoard = _board;
                if (_effectProcessor.Random == null)
                    _effectProcessor.Random = _random;
            }
        }

        public int EnergyPool => Math.Min(_board.CurrentTurn, TurnManager.MaxTurns);

        public int AvailableEnergy(BoardSide side) =>
            _board.GetEnergy(side).Available(EnergyPool);

        public ActionResult StartCombat()
        {
            if (_gameStarted) return ActionResult.Ok();
            _gameStarted = true;
            _board.ResetCombatFlags();

            int events = 0;
            bool hitLimit = false;

            events += Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.GameStart,
                Side = BoardSide.Player,
                Turn = _board.CurrentTurn,
                Cause = ActionCause.Effect
            });
            hitLimit |= LastHitLimit;

            events += Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.GameStart,
                Side = BoardSide.AI,
                Turn = _board.CurrentTurn,
                Cause = ActionCause.Effect
            });
            hitLimit |= LastHitLimit;

            RefreshPowers();
            return ActionResult.Ok(events, hitLimit);
        }

        public PlayResult PlayCard(CardInstance card, BoardSide side, int zoneIndex, int slot)
        {
            if (card?.Definition == null)
                return FailPlay("Carta invalida.");

            if (!HandFor(side).Contains(card))
                return FailPlay("La carta no esta en la mano del lado indicado.");

            if (zoneIndex < 0 || zoneIndex >= _board.Zones.Count)
                return FailPlay("Zona invalida.");

            var zone = _board.Zones[zoneIndex];
            if (!zone.CanAdd(side))
                return FailPlay("La zona esta llena.");

            if (!zone.CanPlay(card, side))
                return FailPlay("La zona no permite jugar esta carta.");

            if (_effectProcessor != null && _effectProcessor.IsZonePlayLocked(zone))
                return FailPlay("La zona esta bloqueada.");

            int cost = card.Definition.Cost;
            if (cost > AvailableEnergy(side))
                return FailPlay("Energia insuficiente.");

            HandFor(side).Remove(card);
            _board.RemovePlayRecords(card);

            zone.CardsFor(side).Add(card);
            card.PlayedTurn = _board.CurrentTurn;
            card.PlayOrderIndex = _board.NextPlayOrder++;
            card.PlayedZoneIndex = zoneIndex;
            card.IsInPlay = true;
            card.IsMerged = false;
            card.Location = CardLocation.Board;

            _board.GetEnergy(side).Charge(card, cost);
            bool firstPlayThisTurn = _board.PlayedThisTurn.Add(card);

            _board.AddPlayRecord(new PlayRecord
            {
                Side = side,
                Card = card,
                ZoneIndex = zoneIndex,
                Turn = _board.CurrentTurn,
                PlayOrder = card.PlayOrderIndex
            });

            _board.Version++;
            RefreshPowers();

            int playZone = zoneIndex;
            ApplyPendingNextPlay(side, card, playZone);

            int events = 0;
            bool hitLimit = false;
            if (firstPlayThisTurn)
            {
                events = Dispatch(new ActionEvent
                {
                    Kind = ActionEventKind.Played,
                    SourceCard = card,
                    Side = side,
                    DestinationZoneIndex = playZone,
                    OriginLocation = CardLocation.Hand,
                    DestinationLocation = CardLocation.Board,
                    Turn = _board.CurrentTurn,
                    PlayOrder = card.PlayOrderIndex,
                    Cause = ActionCause.Manual
                });
                hitLimit = LastHitLimit;
            }

            RefreshPowers();

            return new PlayResult
            {
                Success = true,
                Card = card,
                Side = side,
                ZoneIndex = playZone,
                Slot = slot,
                PlayedTurn = card.PlayedTurn,
                PlayOrder = card.PlayOrderIndex,
                EventsDispatched = events,
                HitEventLimit = hitLimit
            };
        }

        public ActionResult ReturnToHand(CardInstance card, BoardSide side)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            if (HandFor(side).Contains(card))
            {
                card.IsInPlay = false;
                card.Location = CardLocation.Hand;
                return ActionResult.Ok();
            }

            if (HandFor(side).Count >= BoardState.MaxHandSize)
                return ActionResult.Fail("La mano esta llena.");

            var zone = FindZone(card, side);
            if (zone != null && _effectProcessor != null &&
                _effectProcessor.IsZoneRemovalLocked(zone))
                return ActionResult.Fail("La zona no permite retirar la carta.");

            bool wasLifted = _liftedCards.Remove(card);
            RemoveFromContainers(card, side);

            if (!wasLifted && zone == null && !HandFor(side).Contains(card))
                return ActionResult.Fail("La carta no esta en juego.");

            // Solo devuelve coste si la carta fue cobrada este turno.
            _board.GetEnergy(side).Refund(card);

            card.IsInPlay = false;
            card.Location = CardLocation.Hand;
            card.ClearPlayState();
            _board.RemovePlayRecords(card);

            if (!HandFor(side).Contains(card))
                HandFor(side).Add(card);

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult LiftCard(CardInstance card, BoardSide side)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");
            if (card.PlayedTurn != _board.CurrentTurn)
                return ActionResult.Fail("Solo se puede levantar una carta jugada este turno.");

            var zone = FindZone(card, side);
            if (zone == null) return ActionResult.Fail("La carta no esta en el tablero.");

            zone.CardsFor(side).Remove(card);
            _liftedCards.Add(card);

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult MoveCard(CardInstance card, BoardSide side,
            int fromZone, int toZone, ActionCause cause = ActionCause.Effect)
        {
            return MoveCards(new List<CardMoveRequest>
            {
                new CardMoveRequest
                {
                    Card = card,
                    Side = side,
                    FromZone = fromZone,
                    ToZone = toZone
                }
            }, cause);
        }

        public ActionResult MoveCards(List<CardMoveRequest> moves,
            ActionCause cause = ActionCause.Effect)
        {
            if (moves == null || moves.Count == 0) return ActionResult.Ok();

            var valid = new List<CardMoveRequest>();
            foreach (var move in moves)
            {
                if (move?.Card == null) continue;
                if (move.FromZone < 0 || move.FromZone >= _board.Zones.Count) continue;
                if (move.ToZone < 0 || move.ToZone >= _board.Zones.Count) continue;

                var from = _board.Zones[move.FromZone];
                bool wasLifted = _liftedCards.Contains(move.Card);
                if (!wasLifted && !from.CardsFor(move.Side).Contains(move.Card))
                    continue;
                if (move.FromZone == move.ToZone && !wasLifted)
                    continue;

                valid.Add(move);
            }

            if (valid.Count == 0) return ActionResult.Ok();

            // Snapshot: retirar todos los que se mueven y luego colocarlos.
            foreach (var move in valid)
            {
                if (_liftedCards.Remove(move.Card)) continue;
                _board.Zones[move.FromZone].CardsFor(move.Side).Remove(move.Card);
            }

            var successful = new List<CardMoveRequest>();
            bool placed = false;

            foreach (var move in valid)
            {
                var destination = _board.Zones[move.ToZone];
                move.Card.IsInPlay = true;
                move.Card.Location = CardLocation.Board;

                if (destination.CanAdd(move.Side))
                {
                    destination.CardsFor(move.Side).Add(move.Card);
                    placed = true;

                    // Una recolocacion dentro de la misma zona es solo vista:
                    // no emite AfterMoved ni dispara Kraven/Multiple Man.
                    if (move.FromZone != move.ToZone)
                        successful.Add(move);
                }
                else
                {
                    var origin = _board.Zones[move.FromZone];
                    if (origin.CanAdd(move.Side))
                        origin.CardsFor(move.Side).Add(move.Card);
                }
            }

            _board.Version++;
            RefreshPowers();

            if (successful.Count == 0)
                return placed
                    ? ActionResult.Ok()
                    : ActionResult.Fail("Ningun movimiento pudo completarse.");

            int events = 0;
            bool hitLimit = false;
            foreach (var move in successful)
            {
                events += Dispatch(new ActionEvent
                {
                    Kind = ActionEventKind.Moved,
                    SourceCard = move.Card,
                    Side = move.Side,
                    OriginZoneIndex = move.FromZone,
                    DestinationZoneIndex = move.ToZone,
                    OriginLocation = CardLocation.Board,
                    DestinationLocation = CardLocation.Board,
                    Turn = _board.CurrentTurn,
                    PlayOrder = move.Card.PlayOrderIndex,
                    Cause = cause
                });
                hitLimit |= LastHitLimit;
            }

            RefreshPowers();
            return ActionResult.Ok(events, hitLimit);
        }

        public ActionResult DestroyCard(CardInstance card, BoardSide side,
            ActionCause cause = ActionCause.Effect)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            var zone = FindZone(card, side);
            if (zone == null) return ActionResult.Fail("La carta no esta en el tablero.");
            if (!_board.CanDestroy(card, side))
                return ActionResult.Fail("La carta no puede ser destruida.");
            if (_effectProcessor != null &&
                (_effectProcessor.IsZoneRemovalLocked(zone) ||
                 _effectProcessor.IsCardProtectedFromDestroy(card, side)))
                return ActionResult.Fail("La carta esta protegida.");

            int originZone = zone.ZoneIndex;
            int playOrder = card.PlayOrderIndex;

            var replacementContext = ContextFor(card, side, zone);
            bool replaced = TryReplaceDestroy(card, replacementContext);

            if (!replaced && !_pileManager.Move(card, side, PileKind.Destroyed))
                return ActionResult.Fail("No se pudo mover la carta a destruidas.");

            _liftedCards.Remove(card);
            _board.Version++;
            RefreshPowers();

            int events = Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.Destroyed,
                SourceCard = card,
                Side = side,
                OriginZoneIndex = originZone,
                OriginLocation = CardLocation.Board,
                DestinationLocation = CardLocation.Destroyed,
                Turn = _board.CurrentTurn,
                PlayOrder = playOrder,
                Cause = cause
            });
            bool hitLimit = LastHitLimit;

            RefreshPowers();
            return ActionResult.Ok(events, hitLimit);
        }

        public ActionResult DiscardCard(CardInstance card, BoardSide side,
            ActionCause cause = ActionCause.Effect)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            bool inHand = HandFor(side).Contains(card);
            bool inDeck = DeckFor(side).Contains(card);
            if (!inHand && !inDeck)
                return ActionResult.Fail("La carta no esta en mano ni en el mazo.");

            int playOrder = card.PlayOrderIndex;
            var origin = inHand ? CardLocation.Hand : CardLocation.Deck;

            var replacementContext = ContextFor(card, side, null);
            bool replaced = TryReplaceDiscard(card, replacementContext);

            if (!replaced && !_pileManager.Move(card, side, PileKind.Discard))
                return ActionResult.Fail("No se pudo mover la carta a descartadas.");

            _board.Version++;
            RefreshPowers();

            int events = Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.Discarded,
                SourceCard = card,
                Side = side,
                OriginLocation = origin,
                DestinationLocation = CardLocation.Discard,
                Turn = _board.CurrentTurn,
                PlayOrder = playOrder,
                Cause = cause
            });
            bool hitLimit = LastHitLimit;

            RefreshPowers();
            return ActionResult.Ok(events, hitLimit);
        }

        public ActionResult BanishCard(CardInstance card, BoardSide side,
            ActionCause cause = ActionCause.Effect)
        {
            return BanishCard(card, side, false, cause);
        }

        public ActionResult BanishCard(CardInstance card, BoardSide side,
            bool combatScoped, ActionCause cause = ActionCause.Effect)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            var zone = FindZone(card, side);
            if (zone != null && _effectProcessor != null &&
                _effectProcessor.IsZoneRemovalLocked(zone))
                return ActionResult.Fail("La zona no permite retirar la carta.");

            bool inHand = HandFor(side).Contains(card);
            bool inDeck = DeckFor(side).Contains(card);
            if (!inHand && !inDeck && zone == null)
                return ActionResult.Fail("La carta no esta en juego.");

            if (!_pileManager.Move(card, side, PileKind.Banished, combatScoped))
                return ActionResult.Fail("No se pudo mover la carta a desterradas.");

            _liftedCards.Remove(card);
            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult AddToHand(CardInstance card, BoardSide side)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");
            if (HandFor(side).Contains(card))
                return ActionResult.Ok();
            if (FindZone(card, side) != null)
                return ActionResult.Fail("La carta esta en el tablero; usa ReturnToHand.");
            if (HandFor(side).Count >= BoardState.MaxHandSize)
                return ActionResult.Fail("La mano esta llena.");

            HandFor(side).Add(card);
            card.Location = CardLocation.Hand;
            card.IsInPlay = false;
            card.ClearPlayState();

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult AddTokenToZone(CardDef tokenDefinition, BoardSide side,
            int zoneIndex)
        {
            if (tokenDefinition == null) return ActionResult.Fail("Token invalido.");
            var token = new CardInstance(tokenDefinition)
            {
                Location = CardLocation.Board,
                IsInPlay = true,
                PlayedTurn = 0,
                PlayedZoneIndex = -1
            };
            return AddCardToZone(token, side, zoneIndex);
        }

        public ActionResult AddCardToZone(CardInstance card, BoardSide side,
            int zoneIndex)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");
            if (zoneIndex < 0 || zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var zone = _board.Zones[zoneIndex];
            if (!zone.CanAdd(side))
                return ActionResult.Fail("La zona esta llena.");
            if (_effectProcessor != null && _effectProcessor.IsZonePlayLocked(zone))
                return ActionResult.Fail("La zona esta bloqueada.");

            RemoveFromContainers(card, side);

            zone.CardsFor(side).Add(card);
            card.Location = CardLocation.Board;
            card.IsInPlay = true;
            card.IsMerged = false;
            card.PlayedTurn = 0;
            card.PlayedZoneIndex = -1;

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult RemoveFromHand(CardInstance card, BoardSide side)
        {
            if (card == null || !HandFor(side).Remove(card))
                return ActionResult.Fail("La carta no esta en la mano.");

            card.IsInPlay = false;
            card.ClearPlayState();
            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult RemoveFromContainers(CardInstance card, BoardSide side)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            bool removed = HandFor(side).Remove(card);
            removed |= DeckFor(side).Remove(card);
            foreach (var zone in _board.Zones)
                removed |= zone.CardsFor(side).Remove(card);

            _liftedCards.Remove(card);
            card.IsInPlay = false;
            card.ClearPlayState();

            if (removed)
            {
                _board.Version++;
                RefreshPowers();
            }

            return removed ? ActionResult.Ok() : ActionResult.Fail("La carta no esta en juego.");
        }

        public CardInstance CreateCopy(CardInstance source, bool copyPower = true,
            bool copyPerks = false)
        {
            return CardInstanceFactory.Copy(source, copyPower, copyPerks);
        }

        public ActionResult Transform(CardInstance card, CardDef newDefinition)
        {
            if (card == null || newDefinition == null)
                return ActionResult.Fail("Transformacion invalida.");

            CardInstanceFactory.Transform(card, newDefinition);
            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult MergeInto(CardInstance host, CardInstance merged,
            BoardSide side)
        {
            if (host == null || merged == null || ReferenceEquals(host, merged))
                return ActionResult.Fail("Merge invalido.");

            var zone = FindZone(merged, side);
            if (zone == null || !zone.CardsFor(side).Contains(host))
                return ActionResult.Fail("Las cartas no comparten zona.");
            if (_effectProcessor != null && _effectProcessor.IsZoneRemovalLocked(zone))
                return ActionResult.Fail("La zona no permite fusionar.");

            RefreshPowers();
            int mergedPower = merged.CurrentPower;
            host.BonusPower += mergedPower;

            zone.CardsFor(side).Remove(merged);
            merged.IsInPlay = false;
            merged.IsMerged = true;
            merged.Location = CardLocation.Merged;

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult ReplaceZone(int zoneIndex)
        {
            if (_zoneRegistry == null || zoneIndex < 0 ||
                zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var zone = _board.Zones[zoneIndex];
            var candidates = new List<ZoneDef>();
            foreach (var def in _zoneRegistry.GetAll())
            {
                if (def == null || def.Id == zone.Definition?.Id) continue;
                candidates.Add(def);
            }

            if (candidates.Count == 0)
                return ActionResult.Fail("No hay otra locacion disponible.");

            var chosen = candidates[_random.Next(candidates.Count)];
            zone.Definition = chosen;
            zone.Id = chosen.Id;
            zone.IsRevealed = true;

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult BuffTopCard(BoardSide side, int amount)
        {
            var deck = DeckFor(side);
            if (deck.Count == 0) return ActionResult.Fail("El mazo esta vacio.");

            deck[0].BonusPower += amount;
            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult DrawFromEnemyDeck(BoardSide side)
        {
            var opponent = Other(side);
            var deck = DeckFor(opponent);
            var hand = HandFor(side);

            if (deck.Count == 0) return ActionResult.Fail("El mazo rival esta vacio.");
            if (hand.Count >= BoardState.MaxHandSize)
                return ActionResult.Fail("La mano esta llena.");

            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
            card.Location = CardLocation.Hand;
            card.IsInPlay = false;

            _board.Version++;
            RefreshPowers();

            int events = Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.Drawn,
                SourceCard = card,
                Side = side,
                OriginLocation = CardLocation.Deck,
                DestinationLocation = CardLocation.Hand,
                Turn = _board.CurrentTurn,
                Cause = ActionCause.Effect
            });

            RefreshPowers();
            return ActionResult.Ok(events, LastHitLimit);
        }

        public ActionResult ShuffleTokenIntoDeck(BoardSide side,
            CardDef tokenDefinition)
        {
            if (tokenDefinition == null) return ActionResult.Fail("Token invalido.");

            var deck = DeckFor(side);
            var token = new CardInstance(tokenDefinition) { Location = CardLocation.Deck };
            int index = deck.Count == 0 ? 0 : _random.Next(deck.Count + 1);
            deck.Insert(index, token);

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult BanishLowestCost(BoardSide side, bool combatScoped)
        {
            var deck = DeckFor(side);
            if (deck.Count == 0) return ActionResult.Fail("El mazo esta vacio.");

            int lowest = deck.Min(c => c.Definition.Cost);
            var candidates = deck.Where(c => c.Definition.Cost == lowest).ToList();
            var chosen = candidates[_random.Next(candidates.Count)];

            return BanishCard(chosen, side, combatScoped, ActionCause.Effect);
        }

        public DrawResult DrawCard(BoardSide side)
        {
            var deck = DeckFor(side);
            var hand = HandFor(side);

            if (deck.Count == 0)
            {
                return new DrawResult
                {
                    Success = false,
                    Side = side,
                    Reason = "El mazo esta vacio."
                };
            }

            if (hand.Count >= BoardState.MaxHandSize)
            {
                return new DrawResult
                {
                    Success = false,
                    Side = side,
                    Reason = "La mano esta llena."
                };
            }

            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
            card.Location = CardLocation.Hand;

            _board.Version++;
            RefreshPowers();

            int events = Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.Drawn,
                SourceCard = card,
                Side = side,
                OriginLocation = CardLocation.Deck,
                DestinationLocation = CardLocation.Hand,
                Turn = _board.CurrentTurn,
                Cause = ActionCause.Effect
            });
            bool hitLimit = LastHitLimit;

            RefreshPowers();
            return new DrawResult
            {
                Success = true,
                Card = card,
                Side = side,
                EventsDispatched = events,
                HitEventLimit = hitLimit
            };
        }

        public void RefreshPowers()
        {
            _powerResolver?.UpdateAllPowers(_board);
        }

        private bool LastHitLimit =>
            _effectProcessor != null && _effectProcessor.LastActionHitLimit;

        private int Dispatch(ActionEvent actionEvent)
        {
            if (_effectProcessor == null) return 0;

            int events = _effectProcessor.Dispatch(_board, actionEvent);
            _board.Version++;
            return events;
        }

        private EffectContext ContextFor(CardInstance card, BoardSide side,
            ZoneInstance zone)
        {
            if (_effectProcessor == null)
            {
                return new EffectContext
                {
                    SourceCard = card,
                    SourceZone = zone,
                    Owner = side,
                    BoardState = _board,
                    TurnNumber = _board.CurrentTurn,
                    Actions = this,
                    Random = _random
                };
            }

            return _effectProcessor.CreateContext(_board, card, side);
        }

        private bool TryReplaceDestroy(CardInstance card, EffectContext context)
        {
            if (card?.Definition?.EffectIds == null || _effectProcessor == null)
                return false;

            foreach (var effectId in card.Definition.EffectIds)
            {
                var effect = _effectProcessor.GetEffect(effectId);
                if (effect is ICardDestroyReplacement replacement &&
                    replacement.TryReplaceDestroy(context))
                    return true;
            }

            return false;
        }

        private bool TryReplaceDiscard(CardInstance card, EffectContext context)
        {
            if (card?.Definition?.EffectIds == null || _effectProcessor == null)
                return false;

            foreach (var effectId in card.Definition.EffectIds)
            {
                var effect = _effectProcessor.GetEffect(effectId);
                if (effect is ICardDiscardReplacement replacement &&
                    replacement.TryReplaceDiscard(context))
                    return true;
            }

            return false;
        }

        private void ApplyPendingNextPlay(BoardSide side, CardInstance card,
            int playZone)
        {
            var pending = _board.GetPendingNextPlay(side);
            if (pending.Count == 0) return;

            var entries = new List<PendingNextPlayEffect>(pending);
            pending.Clear();

            int currentZone = playZone;
            foreach (var entry in entries)
            {
                if (entry == null) continue;

                if (entry.Kind == PendingNextPlayKind.BonusPower)
                {
                    card.BonusPower += entry.Amount;
                }
                else if (entry.Kind == PendingNextPlayKind.MoveLeft && currentZone > 0)
                {
                    var result = MoveCard(card, side, currentZone, currentZone - 1,
                        ActionCause.Effect);
                    if (result.Success)
                        currentZone--;
                }
            }
        }

        private ZoneInstance FindZone(CardInstance card, BoardSide side)
        {
            if (card == null) return null;

            foreach (var zone in _board.Zones)
            {
                if (zone.CardsFor(side).Contains(card))
                    return zone;
            }

            return null;
        }

        private List<CardInstance> HandFor(BoardSide side) =>
            side == BoardSide.Player ? _board.PlayerHand : _board.AIHand;

        private List<CardInstance> DeckFor(BoardSide side) =>
            side == BoardSide.Player ? _board.PlayerDeck : _board.AIDeck;

        private static BoardSide Other(BoardSide side) =>
            side == BoardSide.Player ? BoardSide.AI : BoardSide.Player;

        private static PlayResult FailPlay(string reason) => new PlayResult
        {
            Success = false,
            Reason = reason
        };
    }
}
