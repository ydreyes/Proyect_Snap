using System;
using System.Collections.Generic;
using SnapClone.Effects;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Core.Services;

namespace SnapClone.Gameplay.Combat
{
    /// <summary>
    /// Autoridad unica del turno: incrementa el contador, roba para ambos
    /// lados, deriva la energia disponible del ledger y cierra el turno.
    /// </summary>
    public class TurnManager
    {
        public const int MaxTurns = 6;

        private readonly BoardState _boardState;
        private readonly PriorityCalculator _priorityCalculator;
        private readonly RevealResolver _revealResolver;
        private readonly TimeService _timeService;
        private readonly PowerResolver _powerResolver;
        private readonly EffectProcessor _effectProcessor;

        public TurnPhase CurrentPhase { get; private set; }

        public int CurrentTurn => _boardState.CurrentTurn;

        /// <summary>Energia que otorga el turno actual: min(turno, 6).</summary>
        public int EnergyPool => Math.Min(_boardState.CurrentTurn, MaxTurns);

        /// <summary>Energia del jugador, para la UI y las validaciones de arrastre.</summary>
        public int PlayerAvailableEnergy => AvailableEnergy(BoardSide.Player);

        public bool IsMatchOver => _boardState.CurrentTurn >= MaxTurns;

        public TurnManager(BoardState boardState, TimeService timeService,
            PowerResolver powerResolver = null, EffectProcessor effectProcessor = null)
        {
            _boardState = boardState;
            _timeService = timeService;
            _priorityCalculator = new PriorityCalculator();
            _revealResolver = new RevealResolver();
            _powerResolver = powerResolver ?? new PowerResolver();
            _effectProcessor = effectProcessor;
            CurrentPhase = TurnPhase.TurnStart;
        }

        /// <summary>Energia disponible del lado indicado segun su ledger del turno.</summary>
        public int AvailableEnergy(BoardSide side) =>
            _boardState.GetEnergy(side).Available(EnergyPool);

        public void StartTurn()
        {
            CurrentPhase = TurnPhase.TurnStart;
            _boardState.CurrentTurn++;
            _boardState.TurnCount = _boardState.CurrentTurn;
            _boardState.ResetTurnTracking();
            _boardState.CurrentEnergy = EnergyPool;

            DrawTurnCard(_boardState.PlayerDeck, _boardState.PlayerHand, BoardSide.Player);
            DrawTurnCard(_boardState.AIDeck, _boardState.AIHand, BoardSide.AI);

            RevealZones();

            _powerResolver.UpdateAllPowers(_boardState);

            CurrentPhase = TurnPhase.PlayPhase;
            _timeService.StartTurnTimer();
        }

        private void DrawTurnCard(List<CardInstance> deck, List<CardInstance> hand,
            BoardSide side)
        {
            if (deck.Count == 0) return;
            if (hand.Count >= BoardState.MaxHandSize) return;

            int index = -1;

            if (_boardState.CurrentTurn == 1)
            {
                for (int i = 0; i < deck.Count; i++)
                {
                    if (!OpeningHandDealer.IsForcedTurn2(deck[i]))
                    {
                        index = i;
                        break;
                    }
                }
            }
            else if (_boardState.CurrentTurn == 2)
            {
                for (int i = 0; i < deck.Count; i++)
                {
                    if (OpeningHandDealer.IsForcedTurn2(deck[i]))
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index < 0) index = 0;

            var card = deck[index];
            deck.RemoveAt(index);
            hand.Add(card);
            card.Location = CardLocation.Hand;

            _effectProcessor?.Dispatch(_boardState, new ActionEvent
            {
                Kind = ActionEventKind.Drawn,
                SourceCard = card,
                Side = side,
                OriginLocation = CardLocation.Deck,
                DestinationLocation = CardLocation.Hand,
                Turn = _boardState.CurrentTurn,
                Cause = ActionCause.Effect
            });
        }

        private void RevealZones()
        {
            // Una zona por turno: turno 1 revela la 1, turno 2 la 2 y turno 3
            // la 3. A partir de ahi no queda nada por revelar.
            int index = _boardState.CurrentTurn - 1;
            if (index >= 0 && index < _boardState.Zones.Count)
                _boardState.Zones[index].IsRevealed = true;
        }

        /// <summary>
        /// Revela todas las locaciones. Lo usa la escena cuando el combate
        /// termina antes del turno 3: el resultado se calcula con informacion
        /// completa.
        /// </summary>
        public void RevealAllZones()
        {
            foreach (var zone in _boardState.Zones)
                zone.IsRevealed = true;
        }

        public void EndTurn()
        {
            _timeService.StopTurnTimer();
            CurrentPhase = TurnPhase.TurnEnd;

            // Primero se resuelven los efectos con el tablero tal como quedo
            // al cerrar el turno; el poder se actualiza y solo despues se
            // decide la prioridad del turno siguiente.
            var revealOrder = _revealResolver.GetRevealOrder(_boardState);
            _effectProcessor?.ResolveEndOfTurn(_boardState, revealOrder);

            _powerResolver.UpdateAllPowers(_boardState);

            string newPriority = _priorityCalculator.CalculatePriority(_boardState, _boardState.PriorityOwner);
            _boardState.PriorityOwner = newPriority;

            CurrentPhase = TurnPhase.TurnStart;
        }
    }
    
    public enum TurnPhase
    {
        TurnStart,
        PlayPhase,
        TurnEnd
    }
}
