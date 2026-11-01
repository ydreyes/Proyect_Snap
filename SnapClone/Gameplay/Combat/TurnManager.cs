using System;
using System.Collections.Generic;
using SnapClone.Effects;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Core.Services;

namespace SnapClone.Gameplay.Combat
{
    /// <summary>
    /// Autoridad unica del turno: incrementa el contador, roba para ambos
    /// lados, deriva la energia disponible del tablero y cierra el turno.
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
        private int _reservedEnergy;

        public TurnPhase CurrentPhase { get; private set; }

        public int CurrentTurn => _boardState.CurrentTurn;

        /// <summary>Energia que otorga el turno actual: min(turno, 6).</summary>
        public int EnergyPool => Math.Min(_boardState.CurrentTurn, MaxTurns);

        /// <summary>
        /// Energia disponible: pool menos el coste de las cartas jugadas este
        /// turno menos la reserva de una carta levantada del tablero.
        /// </summary>
        public int AvailableEnergy => Math.Max(0, EnergyPool - SpentEnergy - _reservedEnergy);

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

        /// <summary>
        /// Coste de las cartas del jugador que siguen en el tablero y fueron
        /// jugadas en el turno actual. Las de turnos anteriores no cuentan.
        /// </summary>
        private int SpentEnergy
        {
            get
            {
                int total = 0;

                foreach (var zone in _boardState.Zones)
                {
                    foreach (var card in zone.PlayerCards)
                    {
                        if (_boardState.CurrentTurn > 0 &&
                            card.PlayedTurn == _boardState.CurrentTurn)
                        {
                            total += card.Definition.Cost;
                        }
                    }
                }

                return total;
            }
        }

        /// <summary>Reserva el coste de una carta levantada del tablero.</summary>
        public void ReserveEnergy(int cost) => _reservedEnergy = Math.Max(0, cost);

        public void ReleaseReservation() => _reservedEnergy = 0;

        public void StartTurn()
        {
            CurrentPhase = TurnPhase.TurnStart;
            _boardState.CurrentTurn++;
            _boardState.TurnCount = _boardState.CurrentTurn;
            _reservedEnergy = 0;
            _boardState.CurrentEnergy = EnergyPool;

            DrawCard(_boardState.PlayerDeck, _boardState.PlayerHand);
            DrawCard(_boardState.AIDeck, _boardState.AIHand);

            RevealZones();

            CurrentPhase = TurnPhase.PlayPhase;
            _timeService.StartTurnTimer();
        }

        private static void DrawCard(List<CardInstance> deck, List<CardInstance> hand)
        {
            if (deck.Count == 0) return;

            var card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
            card.Location = CardLocation.Hand;
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
            _reservedEnergy = 0;

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
