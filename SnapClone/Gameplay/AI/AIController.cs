using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Combat;

namespace SnapClone.Gameplay.AI
{
    /// <summary>
    /// Aplica las decisiones de la IA al tablero a traves de la misma autoridad
    /// de acciones que el jugador. No conoce tipos de UI.
    /// </summary>
    public class AIController
    {
        private readonly AIStrategy _strategy;
        private readonly BoardState _boardState;
        private readonly CardActionService _actionService;

        public AIController(BoardState boardState, RandomService randomService,
            CardActionService actionService, TurnManager turnManager)
        {
            _boardState = boardState;
            _actionService = actionService;
            _strategy = new AIStrategy(randomService, turnManager);
        }

        public PlayResult TryPlayOne()
        {
            var decision = _strategy.MakeDecision(_boardState);
            if (decision == null) return null;

            var (card, zone, slot) = decision.Value;
            var result = _actionService.PlayCard(card, BoardSide.AI, zone.ZoneIndex, slot);
            return result.Success ? result : null;
        }

        /// <summary>
        /// Juega mientras quede una carta pagable y un slot libre. Devuelve
        /// las jugadas para que la escena les asigne una vista.
        /// </summary>
        public List<PlayResult> TryPlayAll()
        {
            var plays = new List<PlayResult>();

            while (true)
            {
                var play = TryPlayOne();
                if (play == null) break;

                plays.Add(play);
            }

            return plays;
        }
    }
}
