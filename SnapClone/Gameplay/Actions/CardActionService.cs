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
using SnapClone.Gameplay.Meta;
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
        private readonly CostResolver _costResolver;
        private readonly ZoneRegistry _zoneRegistry;
        private readonly RandomService _random;
        private readonly RunState _runState;
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
            _costResolver = new CostResolver(effectProcessor);
            _zoneRegistry = zoneRegistry;
            _runState = runState;
            _random = effectProcessor?.Random ?? new RandomService(1);

            if (_effectProcessor != null)
            {
                _effectProcessor.Actions = this;
                _effectProcessor.BoundBoard = _board;
                if (_effectProcessor.Random == null)
                    _effectProcessor.Random = _random;
            }
        }

        public int EnergyPool => RunModifierSystem.EnergyPoolFor(
            _runState, _board.CurrentTurn);

        public int GetEnergyPool(BoardSide side) =>
            Math.Max(0, EnergyPool + _board.GetMaxEnergyDelta(side));

        public int AvailableEnergy(BoardSide side) =>
            _board.GetEnergy(side).Available(GetEnergyPool(side));

        public int GetEffectiveCost(CardInstance card, BoardSide side) =>
            _costResolver.Resolve(card, side, _board);

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

            // Unstoppable ignora restricciones y bloqueos de zona, pero no el
            // limite de cuatro cartas ni la energia.
            bool unstoppable = _effectProcessor?.AllowsAnyZone(card) ?? false;
            if (!unstoppable)
            {
                if (!zone.CanPlay(card, side))
                    return FailPlay("La zona no permite jugar esta carta.");

                if (_effectProcessor != null && _effectProcessor.IsZonePlayLocked(zone))
                    return FailPlay("La zona esta bloqueada.");
            }

            if (zone.IsFlooded(_board.CurrentTurn))
                return FailPlay("La locacion esta inundada.");

            if (_effectProcessor != null &&
                !_effectProcessor.CanPlayCard(card, _board, side))
                return FailPlay("La carta no se puede jugar este turno.");

            if (_effectProcessor != null &&
                _effectProcessor.IsBlockedByGoose(zone, card, side))
                return FailPlay("Goose impide jugar cartas de coste 4 o mas aqui.");

            if (_effectProcessor != null &&
                _effectProcessor.IsSideLimitedToOneCard(_board, side) &&
                _board.GetPlayedCount(side, _board.CurrentTurn) >= 1)
                return FailPlay("Solo puedes jugar 1 carta este turno.");

            int forcedZone = _effectProcessor?.GetForcedZone(_board, side) ?? -1;
            if (forcedZone >= 0 && zoneIndex != forcedZone &&
                forcedZone < _board.Zones.Count)
            {
                var jeanZone = _board.Zones[forcedZone];
                if (jeanZone.CanAdd(side) &&
                    !jeanZone.IsFlooded(_board.CurrentTurn) &&
                    jeanZone.CanPlay(card, side) &&
                    !_effectProcessor.IsZonePlayLocked(jeanZone))
                    return FailPlay("Tu primera carta debe ir a la zona de Jean Grey.");
            }

            bool firstCardThisTurn = !_board.HasPlayedThisTurn(side);
            if (!ValidateTaunt(side, zoneIndex))
                return FailPlay("Taunt: tu primera carta debe ir a la zona reservada.");

            // Los modificadores de coste ya viven en el resolver: Quickdraw en
            // el delta del combate, Hot Pursuit en el delta del turno y las
            // cartas de Series 3 en sus fuentes dinamicas.
            int cost = GetEffectiveCost(card, side);

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

            // Taunt es de un solo uso: cualquier reserva del lado se consume en
            // su primera carta, incluso si habia mas de una.
            if (firstCardThisTurn)
            {
                _board.TauntReservations.RemoveAll(r =>
                    r != null && r.Side == side && r.Turn == _board.CurrentTurn);
            }

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

            if (side == BoardSide.AI)
                _runState?.DiscoverCard(card.Definition?.Id);

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
            ApplyCurrentTurnCostTax(card);
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
            if (!CanLiftForMove(card, side))
                return ActionResult.Fail("La carta no se puede mover ahora.");

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
            if (successful.Count > 0)
                _board.LastMoveTurn = _board.CurrentTurn;
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

        /// <summary>
        /// Movimiento iniciado por el jugador. Respeta el movimiento libre del
        /// mismo turno, los permisos impresos y la ventana de Cloak.
        /// </summary>
        public ActionResult MoveCardByPlayer(CardInstance card, BoardSide side,
            int fromZone, int toZone)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            bool wasLifted = _liftedCards.Contains(card);
            var origin = FindZone(card, side);

            if (!wasLifted)
            {
                if (origin == null)
                    return ActionResult.Fail("La carta no esta en el tablero.");

                fromZone = origin.ZoneIndex;
            }

            if (fromZone < 0 || fromZone >= _board.Zones.Count)
                return ActionResult.Fail("Zona de origen invalida.");

            if (toZone < 0 || toZone >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            if (fromZone == toZone)
            {
                // Recolocar una carta levantada en su mismo slot: la devuelve
                // al modelo sin consumir permisos.
                if (wasLifted)
                    return MoveCard(card, side, fromZone, toZone,
                        ActionCause.Manual);

                return ActionResult.Ok();
            }

            if (!CanMoveCard(card, side, fromZone, toZone))
                return ActionResult.Fail("La carta no puede moverse a esa locacion.");

            bool playedThisTurn = card.PlayedTurn == _board.CurrentTurn;
            bool perkMove = _effectProcessor?.HasPlayerMovePermission(card) ??
                false;
            var permission = _effectProcessor?.GetMovePermission(card) ??
                MovePermission.None;
            bool permissionAvailable = permission == MovePermission.OncePerCombat
                ? !card.MoveUsedThisCombat && !(perkMove && card.PlayerMoveUsed)
                : permission == MovePermission.OncePerTurn &&
                  card.MoveUsedTurn != _board.CurrentTurn;

            var result = MoveCard(card, side, fromZone, toZone,
                ActionCause.Manual);
            if (!result.Success) return result;

            if (!playedThisTurn)
            {
                if (permissionAvailable)
                {
                    if (perkMove)
                        card.PlayerMoveUsed = true;

                    if (permission == MovePermission.OncePerCombat)
                        card.MoveUsedThisCombat = true;
                    else if (permission == MovePermission.OncePerTurn)
                        card.MoveUsedTurn = _board.CurrentTurn;
                }
                else
                {
                    card.CloakMoveUsedTurn = _board.CurrentTurn;
                }
            }

            return result;
        }

        /// <summary>Legalidad de un movimiento iniciado por el jugador.</summary>
        public bool CanMoveCard(CardInstance card, BoardSide side, int fromZone,
            int toZone)
        {
            if (card == null) return false;
            if (toZone < 0 || toZone >= _board.Zones.Count) return false;
            if (fromZone < 0 || fromZone >= _board.Zones.Count) return false;

            bool wasLifted = _liftedCards.Contains(card);
            if (!wasLifted && FindZone(card, side) == null) return false;

            if (_effectProcessor != null &&
                _effectProcessor.IsZoneRemovalLocked(_board.Zones[fromZone]))
                return false;

            if (!_board.Zones[toZone].CanAdd(side)) return false;

            if (fromZone == toZone) return true;
            if (card.PlayedTurn == _board.CurrentTurn) return true;

            var permission = _effectProcessor?.GetMovePermission(card) ??
                MovePermission.None;

            if (permission == MovePermission.OncePerCombat &&
                !card.MoveUsedThisCombat)
                return true;

            if (permission == MovePermission.OncePerTurn &&
                card.MoveUsedTurn != _board.CurrentTurn)
                return true;

            return card.CloakMoveUsedTurn != _board.CurrentTurn &&
                   _board.HasMoveWindow(toZone, _board.CurrentTurn, side);
        }

        /// <summary>Indica si la carta puede levantarse para un movimiento.</summary>
        public bool CanLiftForMove(CardInstance card, BoardSide side)
        {
            if (card == null) return false;
            if (FindZone(card, side) == null) return false;
            if (card.PlayedTurn == _board.CurrentTurn) return true;

            var permission = _effectProcessor?.GetMovePermission(card) ??
                MovePermission.None;

            if (permission == MovePermission.OncePerCombat &&
                !card.MoveUsedThisCombat)
                return true;

            if (permission == MovePermission.OncePerTurn &&
                card.MoveUsedTurn != _board.CurrentTurn)
                return true;

            return card.CloakMoveUsedTurn != _board.CurrentTurn &&
                   _board.HasAnyMoveWindow(_board.CurrentTurn, side);
        }

        /// <summary>Consulta de legalidad de carta para la IA y la UI.</summary>
        public bool CanPlayCard(CardInstance card, BoardSide side) =>
            _effectProcessor == null ||
            _effectProcessor.CanPlayCard(card, _board, side);

        /// <summary>Consulta combinada de carta y zona para la IA y la UI.</summary>
        public bool CanPlayCardInZone(CardInstance card, BoardSide side,
            ZoneInstance zone)
        {
            if (card == null || zone == null) return false;
            if (!zone.CanAdd(side)) return false;
            if (!zone.CanPlay(card, side)) return false;
            if (_effectProcessor != null &&
                _effectProcessor.IsZonePlayLocked(zone))
                return false;
            if (zone.IsFlooded(_board.CurrentTurn)) return false;
            if (_effectProcessor != null &&
                _effectProcessor.IsBlockedByGoose(zone, card, side))
                return false;
            if (_effectProcessor != null &&
                _effectProcessor.IsSideLimitedToOneCard(_board, side) &&
                _board.GetPlayedCount(side, _board.CurrentTurn) >= 1)
                return false;

            int forcedZone = _effectProcessor?.GetForcedZone(_board, side) ?? -1;
            if (forcedZone >= 0 && zone.ZoneIndex != forcedZone)
                return false;

            return _effectProcessor == null ||
                   _effectProcessor.CanPlayCard(card, _board, side, zone);
        }

        /// <summary>Texto corto de estado de movimiento para el panel de detalle.</summary>
        public string GetMoveStatus(CardInstance card, BoardSide side)
        {
            if (card == null || FindZone(card, side) == null) return null;
            if (card.PlayedTurn == _board.CurrentTurn) return null;

            var permission = _effectProcessor?.GetMovePermission(card) ??
                MovePermission.None;

            if (permission == MovePermission.OncePerCombat)
                return card.MoveUsedThisCombat ? "MOVIMIENTO USADO" : "MOVIBLE";

            if (permission == MovePermission.OncePerTurn)
                return card.MoveUsedTurn == _board.CurrentTurn
                    ? "MOVIMIENTO USADO"
                    : "MOVIBLE";

            if (_board.HasAnyMoveWindow(_board.CurrentTurn, side))
                return card.CloakMoveUsedTurn == _board.CurrentTurn
                    ? "MOVIMIENTO USADO"
                    : "MOVIBLE A CLOAK";

            return null;
        }

        public ActionResult DestroyCard(CardInstance card, BoardSide side,
            ActionCause cause = ActionCause.Effect)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");
            if (_effectProcessor != null && _effectProcessor.IsDestroyImmune(card))
                return ActionResult.Fail("La carta no puede ser destruida.");

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
            int destroyedPower = _powerResolver != null
                ? _powerResolver.CalculateCardPower(card, zone, _board, side)
                : card.CurrentPower;
            int destroyedCost = GetEffectiveCost(card, side);
            _board.RecordDestroyedCard(card, side, destroyedPower,
                destroyedCost, _board.CurrentTurn);

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

            _board.IncrementDiscard(side);
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

            var origin = card.Location;

            HandFor(side).Add(card);
            card.Location = CardLocation.Hand;
            card.IsInPlay = false;
            card.ClearPlayState();
            ApplyCurrentTurnCostTax(card);

            _board.Version++;
            RefreshPowers();

            int events = Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.AddedToHand,
                SourceCard = card,
                Side = side,
                OriginLocation = origin,
                DestinationLocation = CardLocation.Hand,
                Turn = _board.CurrentTurn,
                Cause = ActionCause.Effect
            });

            return ActionResult.Ok(events, LastHitLimit);
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
            if (_effectProcessor != null && _effectProcessor.IsZoneRemovalLocked(zone))
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

            zone.CardsFor(side).Remove(merged);
            merged.IsInPlay = false;
            merged.IsMerged = true;
            merged.Location = CardLocation.Merged;

            AddBonusPower(host, mergedPower);
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
                if (def.Id == "zone_limbo") continue;
                candidates.Add(def);
            }

            if (candidates.Count == 0)
                return ActionResult.Fail("No hay otra locacion disponible.");

            var chosen = candidates[_random.Next(candidates.Count)];
            ApplyZoneDefinition(zone, chosen);
            return ActionResult.Ok();
        }

        public ActionResult ReplaceZoneWith(int zoneIndex, string zoneId)
        {
            if (_zoneRegistry == null || zoneIndex < 0 ||
                zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var definition = _zoneRegistry.Get(zoneId);
            if (definition == null)
                return ActionResult.Fail("Locacion desconocida.");

            ApplyZoneDefinition(_board.Zones[zoneIndex], definition);
            return ActionResult.Ok();
        }

        private void ApplyZoneDefinition(ZoneInstance zone, ZoneDef definition)
        {
            zone.Definition = definition;
            zone.Id = definition.Id;
            zone.IsRevealed = true;
            zone.ClearFlood();
            _board.Version++;
            RefreshPowers();
        }

        public ActionResult BuffTopCard(BoardSide side, int amount)
        {
            var deck = DeckFor(side);
            if (deck.Count == 0) return ActionResult.Fail("El mazo esta vacio.");

            AddBonusPower(deck[0], amount);
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
            card.ClearPlayState();
            ApplyCurrentTurnCostTax(card);

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

            events += Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.AddedToHand,
                SourceCard = card,
                Side = side,
                OriginLocation = CardLocation.Deck,
                DestinationLocation = CardLocation.Hand,
                Turn = _board.CurrentTurn,
                Cause = ActionCause.Effect
            });
            hitLimit |= LastHitLimit;

            RefreshPowers();
            return ActionResult.Ok(events, hitLimit);
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

            int lowest = deck.Min(c => GetEffectiveCost(c, side));
            var candidates = deck.Where(c => GetEffectiveCost(c, side) == lowest).ToList();
            var chosen = candidates[_random.Next(candidates.Count)];

            return BanishCard(chosen, side, combatScoped, ActionCause.Effect);
        }

        public DrawResult DrawCard(BoardSide side)
        {
            var deck = DeckFor(side);
            var hand = HandFor(side);
            if (hand.Any(card => card?.Definition?.Id == "token_widows_bite"))
            {
                return new DrawResult
                {
                    Success = false,
                    Side = side,
                    Reason = "Una Mordida de Viuda impide robar."
                };
            }

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

        public ActionResult DrawSpecificFromDeck(CardInstance card, BoardSide side)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            var deck = DeckFor(side);
            var hand = HandFor(side);
            if (!deck.Contains(card))
                return ActionResult.Fail("La carta no esta en el mazo.");
            if (hand.Count >= BoardState.MaxHandSize)
                return ActionResult.Fail("La mano esta llena.");
            if (hand.Any(c => c?.Definition?.Id == "token_widows_bite"))
                return ActionResult.Fail("Una Mordida de Viuda impide robar.");

            deck.Remove(card);
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
            return ActionResult.Ok(events, hitLimit);
        }

        public void RefreshPowers()
        {
            _powerResolver?.UpdateAllPowers(_board);
        }

        /// <summary>Aplica el impuesto de Sandman del turno a una carta que entra a mano.</summary>
        private void ApplyCurrentTurnCostTax(CardInstance card)
        {
            if (card == null) return;

            int tax = _board.GetCostTaxForTurn(_board.CurrentTurn);
            if (tax <= 0 || card.CostTaxAppliedTurn == _board.CurrentTurn)
                return;

            card.TurnCostDelta += tax;
            card.CostTaxAppliedTurn = _board.CurrentTurn;
        }

        /// <summary>Concede energia extra solo para el turno en curso.</summary>
        public ActionResult AddEnergy(BoardSide side, int amount)
        {
            if (amount <= 0)
                return ActionResult.Fail("Cantidad de energia invalida.");

            _board.GetEnergy(side).GrantBonus(amount);
            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        /// <summary>
        /// Aplica un cambio persistente de Poder. Un aumento positivo dispara
        /// Empowered (salvo que lo origine Reinforce).
        /// </summary>
        public ActionResult AddBonusPower(CardInstance card, int amount,
            bool dispatchEmpowered = true)
        {
            if (card == null)
                return ActionResult.Fail("Carta invalida.");

            if (amount < 0 && _effectProcessor != null &&
                _effectProcessor.NegatesPowerReduction(card))
                return ActionResult.Ok();

            card.BonusPower += amount;

            if (amount > 0 && dispatchEmpowered)
                _effectProcessor?.NotifyEmpowered(card, amount);

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        /// <summary>Ajusta el poder de una carta en juego por la via comun.</summary>
        public ActionResult AdjustCardPower(CardInstance card, BoardSide side,
            int amount)
        {
            if (card == null)
                return ActionResult.Fail("Carta invalida.");

            if (FindZone(card, side) == null)
                return ActionResult.Fail("La carta no esta en el tablero.");

            return AddBonusPower(card, amount);
        }

        /// <summary>Ajusta el poder de todas las cartas de un lado en una zona.</summary>
        public ActionResult AdjustZonePower(int zoneIndex, BoardSide side, int amount)
        {
            if (zoneIndex < 0 || zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var cards = _board.Zones[zoneIndex].CardsFor(side);
            if (cards.Count == 0)
                return ActionResult.Fail("La zona no tiene cartas.");

            foreach (var card in cards)
            {
                if (amount < 0 && _effectProcessor != null &&
                    _effectProcessor.NegatesPowerReduction(card))
                    continue;

                card.BonusPower += amount;
                if (amount > 0)
                    _effectProcessor?.NotifyEmpowered(card, amount);
            }

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        /// <summary>Cambia el control de una carta sin moverla de locacion.</summary>
        public ActionResult SwitchSides(CardInstance card, BoardSide side,
            int zoneIndex)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");
            if (zoneIndex < 0 || zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var zone = _board.Zones[zoneIndex];
            if (!zone.CardsFor(side).Contains(card))
                return ActionResult.Fail("La carta no esta en esa zona.");

            if (_effectProcessor != null &&
                _effectProcessor.IsZoneRemovalLocked(zone))
                return ActionResult.Fail("La zona no permite cambiar de lado.");

            var other = Other(side);
            if (!zone.CanAdd(other))
                return ActionResult.Fail("El lado rival esta lleno.");

            zone.CardsFor(side).Remove(card);
            zone.CardsFor(other).Add(card);
            card.IsInPlay = true;
            card.Location = CardLocation.Board;

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        /// <summary>Mueve la carta superior del mazo a una locacion; puede revelarla.</summary>
        public ActionResult PullTopDeckToZone(BoardSide side, int zoneIndex,
            bool reveal)
        {
            if (zoneIndex < 0 || zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var deck = DeckFor(side);
            if (deck.Count == 0) return ActionResult.Fail("El mazo esta vacio.");

            var zone = _board.Zones[zoneIndex];
            if (!zone.CanAdd(side))
                return ActionResult.Fail("La zona esta llena.");

            if (_effectProcessor != null &&
                _effectProcessor.IsZoneRemovalLocked(zone))
                return ActionResult.Fail("La zona esta bloqueada.");

            var card = deck[0];
            deck.RemoveAt(0);
            zone.CardsFor(side).Add(card);
            card.Location = CardLocation.Board;
            card.IsInPlay = true;
            card.IsMerged = false;
            card.PlayedTurn = 0;
            card.PlayedZoneIndex = -1;

            _board.Version++;
            RefreshPowers();

            if (reveal)
                _effectProcessor?.RevealAddedCard(_board, card);

            return ActionResult.Ok();
        }

        /// <summary>Reemplaza una carta destruida por un token en el mismo slot.</summary>
        public ActionResult ReplaceWithToken(CardInstance source, BoardSide side,
            CardDef tokenDefinition)
        {
            if (source == null || tokenDefinition == null)
                return ActionResult.Fail("Reemplazo invalido.");

            var zone = FindZone(source, side);
            if (zone == null) return ActionResult.Fail("La carta no esta en el tablero.");

            if (_effectProcessor != null &&
                _effectProcessor.IsZoneRemovalLocked(zone))
                return ActionResult.Fail("La zona no permite reemplazar.");

            zone.CardsFor(side).Remove(source);
            source.IsInPlay = false;
            source.IsMerged = true;
            source.Location = CardLocation.Merged;

            var token = new CardInstance(tokenDefinition)
            {
                Location = CardLocation.Board,
                IsInPlay = true,
                HasRevealed = true,
                PlayedTurn = 0,
                PlayedZoneIndex = -1
            };

            zone.CardsFor(side).Add(token);

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        /// <summary>Emite el evento de entrada a mano sin mover la carta.</summary>
        public ActionResult NotifyAddedToHand(CardInstance card, BoardSide side,
            CardLocation origin)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            int events = Dispatch(new ActionEvent
            {
                Kind = ActionEventKind.AddedToHand,
                SourceCard = card,
                Side = side,
                OriginLocation = origin,
                DestinationLocation = CardLocation.Hand,
                Turn = _board.CurrentTurn,
                Cause = ActionCause.Effect
            });

            return ActionResult.Ok(events, LastHitLimit);
        }

        public ActionResult ActivateCard(CardInstance card, BoardSide side)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");

            var zone = FindZone(card, side);
            if (zone == null)
                return ActionResult.Fail("La carta no esta en el tablero.");
            if (card.AbilitiesDisabled || card.ActivatedThisCombat)
                return ActionResult.Fail("La carta no puede activarse.");

            bool hasActivate = PerkAbilities.EffectiveEffectIds(card,
                    !card.PrintedAbilitiesDisabled)
                .Any(effectId =>
                {
                    var effect = _effectProcessor?.GetEffect(effectId);
                    return effect != null &&
                           effect.Trigger == EffectTrigger.Activate;
                });
            if (!hasActivate)
                return ActionResult.Fail("La carta no tiene una habilidad Activar.");

            card.ActivatedThisCombat = true;
            int events = _effectProcessor?.ResolveActivate(_board, card) ?? 0;
            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok(events, LastHitLimit);
        }

        public ActionResult ResurrectFromPile(CardInstance card, BoardSide side,
            int zoneIndex)
        {
            if (card == null) return ActionResult.Fail("Carta invalida.");
            if (zoneIndex < 0 || zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var zone = _board.Zones[zoneIndex];
            if (!zone.CanAdd(side))
                return ActionResult.Fail("La zona esta llena.");

            var piles = _board.GetPiles(side);
            if (!piles.Get(PileKind.Discard).Contains(card))
                return ActionResult.Fail("La carta no esta en descartadas.");
            if (!piles.Remove(card, PileKind.Discard))
                return ActionResult.Fail("No se pudo sacar la carta de descartadas.");

            zone.CardsFor(side).Add(card);
            card.ClearPlayState();
            card.Location = CardLocation.Board;
            card.IsInPlay = true;
            card.HasRevealed = true;
            card.PlayedTurn = 0;
            card.PlayedZoneIndex = -1;

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult CopyToOtherZones(CardInstance source, BoardSide side)
        {
            if (source?.Definition == null)
                return ActionResult.Fail("Carta invalida.");

            var sourceZone = FindZone(source, side);

            foreach (var zone in _board.Zones)
            {
                if (ReferenceEquals(zone, sourceZone)) continue;
                if (!zone.CanAdd(side)) continue;

                var copy = CardInstanceFactory.Copy(source);
                if (copy == null) continue;

                copy.Location = CardLocation.Board;
                copy.IsInPlay = true;
                copy.HasRevealed = true;
                copy.PlayedTurn = 0;
                copy.PlayedZoneIndex = -1;
                zone.CardsFor(side).Add(copy);
            }

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult SwapZones(int zoneA, int zoneB)
        {
            if (zoneA < 0 || zoneA >= _board.Zones.Count ||
                zoneB < 0 || zoneB >= _board.Zones.Count || zoneA == zoneB)
                return ActionResult.Fail("Zonas invalidas.");

            _board.Zones[zoneA].SwapContentWith(_board.Zones[zoneB]);
            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult PullEnemyHandCard(BoardSide owner, int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= _board.Zones.Count)
                return ActionResult.Fail("Zona invalida.");

            var enemy = Other(owner);
            var hand = HandFor(enemy);
            if (hand.Count == 0)
                return ActionResult.Fail("La mano rival esta vacia.");

            var zone = _board.Zones[zoneIndex];
            if (!zone.CanAdd(enemy))
                return ActionResult.Fail("La zona esta llena.");

            var card = hand
                .OrderBy(c => c.CurrentPower)
                .ThenBy(c => c.Definition?.Cost ?? 0)
                .First();

            hand.Remove(card);
            zone.CardsFor(enemy).Add(card);
            card.Location = CardLocation.Board;
            card.IsInPlay = true;
            card.PlayedTurn = 0;
            card.PlayedZoneIndex = -1;
            _effectProcessor?.RevealAddedCard(_board, card);

            _board.Version++;
            RefreshPowers();
            return ActionResult.Ok();
        }

        public ActionResult DiscardFromEnemyHand(BoardSide owner, string selection)
        {
            var enemy = Other(owner);
            var hand = HandFor(enemy);
            if (hand.Count == 0)
                return ActionResult.Fail("La mano rival esta vacia.");

            CardInstance chosen;
            switch (selection)
            {
                case "evenCost":
                    chosen = hand.FirstOrDefault(c =>
                        GetEffectiveCost(c, enemy) % 2 == 0);
                    break;
                case "lowestPower":
                    chosen = hand
                        .OrderBy(c => c.CurrentPower)
                        .ThenBy(c => c.Definition?.Cost ?? 0)
                        .FirstOrDefault();
                    break;
                default:
                    chosen = hand
                        .OrderBy(c => GetEffectiveCost(c, enemy))
                        .ThenBy(c => c.CurrentPower)
                        .FirstOrDefault();
                    break;
            }

            if (chosen == null)
                return ActionResult.Fail("No hay una carta valida para descartar.");

            return DiscardCard(chosen, enemy, ActionCause.Effect);
        }

        public ActionResult ChangeMaxEnergy(BoardSide side, int delta)
        {
            _board.AddMaxEnergyDelta(side, delta);
            _board.Version++;
            return ActionResult.Ok();
        }

        public ActionResult AddNextTurnEnergy(BoardSide side, int amount)
        {
            if (amount <= 0) return ActionResult.Ok();
            _board.AddNextTurnEnergyBonus(side, amount);
            return ActionResult.Ok();
        }

        public void MarkCreated(CardInstance card)
        {
            if (card != null)
                card.IsCreated = true;
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
            if (card == null || _effectProcessor == null)
                return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
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
            if (card == null || _effectProcessor == null)
                return false;

            foreach (var effectId in PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled))
            {
                var effect = _effectProcessor.GetEffect(effectId);
                if (effect is ICardDiscardReplacement replacement &&
                    replacement.TryReplaceDiscard(context))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Taunt: la primera carta del lado debe ir a la zona reservada. Si la
        /// zona ya no tiene espacio, la reserva expira sin bloquear la jugada.
        /// </summary>
        private bool ValidateTaunt(BoardSide side, int zoneIndex)
        {
            if (_board.HasPlayedThisTurn(side)) return true;

            var reservation = _board.GetTauntReservation(side, _board.CurrentTurn);
            if (reservation == null) return true;

            if (reservation.ZoneIndex < 0 ||
                reservation.ZoneIndex >= _board.Zones.Count ||
                !_board.Zones[reservation.ZoneIndex].CanAdd(side))
            {
                _board.TauntReservations.Remove(reservation);
                return true;
            }

            return reservation.ZoneIndex == zoneIndex;
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
                    AddBonusPower(card, entry.Amount);
                }
                else if (entry.Kind == PendingNextPlayKind.MoveLeft && currentZone > 0)
                {
                    var result = MoveCard(card, side, currentZone, currentZone - 1,
                        ActionCause.Effect);
                    if (result.Success)
                        currentZone--;
                }
                else if (entry.Kind == PendingNextPlayKind.Silence)
                {
                    card.PrintedAbilitiesDisabled = true;
                }
                else if (entry.Kind == PendingNextPlayKind.DoublePower)
                {
                    card.PendingDoublePower = true;
                }
                else if (entry.Kind == PendingNextPlayKind.SwapWithDeck)
                {
                    var deck = DeckFor(side);
                    if (deck.Count == 0) continue;
                    if (currentZone < 0 || currentZone >= _board.Zones.Count)
                        continue;

                    var zone = _board.Zones[currentZone];
                    zone.CardsFor(side).Remove(card);
                    card.Location = CardLocation.Deck;
                    card.IsInPlay = false;
                    deck.Add(card);

                    var replacement = deck[_random.Next(deck.Count)];
                    deck.Remove(replacement);
                    zone.CardsFor(side).Add(replacement);
                    replacement.Location = CardLocation.Board;
                    replacement.IsInPlay = true;
                    replacement.PlayedTurn = _board.CurrentTurn;
                    replacement.PlayedZoneIndex = currentZone;
                    _effectProcessor?.RevealAddedCard(_board, replacement);
                    return;
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
