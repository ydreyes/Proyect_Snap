using System.Collections.Generic;

namespace SnapClone.Gameplay.Board
{
    public class BoardState
    {
        public List<ZoneInstance> Zones { get; set; } = new List<ZoneInstance>();
        public List<Gameplay.Cards.CardInstance> PlayerHand { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public List<Gameplay.Cards.CardInstance> AIHand { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public List<Gameplay.Cards.CardInstance> PlayerDeck { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public List<Gameplay.Cards.CardInstance> AIDeck { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public Gameplay.Cards.PileSet PlayerPiles { get; private set; } = new Gameplay.Cards.PileSet();
        public Gameplay.Cards.PileSet AIPiles { get; private set; } = new Gameplay.Cards.PileSet();

        /// <summary>
        /// Se incrementa cada vez que una carta cambia de pila. La escena lo usa
        /// para reconciliar las vistas sin eventos.
        /// </summary>
        public int Version { get; set; }

        public int CurrentTurn { get; set; } = 0;
        public int TurnCount { get; set; } = 0;
        public int CurrentEnergy { get; set; }
        public string PriorityOwner { get; set; } = "Player"; // Player or AI
        public int NextPlayOrder { get; set; } = 0;

        private Combat.PowerResolver _powerResolver;

        /// <summary>
        /// Resolver compartido del combate. Al asignarlo, cada zona obtiene la
        /// referencia al tablero para calcular su total por el mismo camino.
        /// </summary>
        public Combat.PowerResolver PowerResolver
        {
            get => _powerResolver;
            set
            {
                _powerResolver = value;
                foreach (var zone in Zones)
                    zone.Board = this;
            }
        }

        public Gameplay.Cards.PileSet GetPiles(BoardSide side) =>
            side == BoardSide.Player ? PlayerPiles : AIPiles;
    }
}
