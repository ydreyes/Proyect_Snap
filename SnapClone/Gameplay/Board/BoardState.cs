using System;
using System.Collections.Generic;
using SnapClone.Gameplay.Actions;

namespace SnapClone.Gameplay.Board
{
    public enum PendingNextPlayKind
    {
        BonusPower,
        MoveLeft
    }

    public class PendingNextPlayEffect
    {
        public PendingNextPlayKind Kind { get; set; }
        public int Amount { get; set; }
    }

    public class BoardState
    {
        public const int MaxHandSize = 7;

        public List<ZoneInstance> Zones { get; set; } = new List<ZoneInstance>();
        public List<Gameplay.Cards.CardInstance> PlayerHand { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public List<Gameplay.Cards.CardInstance> AIHand { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public List<Gameplay.Cards.CardInstance> PlayerDeck { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public List<Gameplay.Cards.CardInstance> AIDeck { get; set; } = new List<Gameplay.Cards.CardInstance>();
        public Gameplay.Cards.PileSet PlayerPiles { get; private set; } = new Gameplay.Cards.PileSet();
        public Gameplay.Cards.PileSet AIPiles { get; private set; } = new Gameplay.Cards.PileSet();

        /// <summary>Jugadas desde la mano, para condiciones "jugo aqui este turno".</summary>
        public List<PlayRecord> PlayLog { get; } = new List<PlayRecord>();

        /// <summary>Instancias que ya dispararon AfterPlayed en el turno actual.</summary>
        public HashSet<Gameplay.Cards.CardInstance> PlayedThisTurn { get; } =
            new HashSet<Gameplay.Cards.CardInstance>();

        public TurnEnergyLedger PlayerEnergy { get; } = new TurnEnergyLedger();
        public TurnEnergyLedger AIEnergy { get; } = new TurnEnergyLedger();

        /// <summary>Efectos "la proxima carta que juegues" por lado.</summary>
        public List<PendingNextPlayEffect> PlayerPendingNextPlay { get; } =
            new List<PendingNextPlayEffect>();
        public List<PendingNextPlayEffect> AIPendingNextPlay { get; } =
            new List<PendingNextPlayEffect>();

        /// <summary>Uatu: el lado ve la informacion de locaciones no reveladas.</summary>
        public bool PlayerSeesHiddenZones { get; set; }
        public bool AISeesHiddenZones { get; set; }

        /// <summary>
        /// Hooks que pueden impedir una destruccion. Vacios en este cambio;
        /// Armor y Professor X los usaran mas adelante.
        /// </summary>
        public List<Func<Gameplay.Cards.CardInstance, BoardSide, bool>> DestroyProtections
            { get; } = new List<Func<Gameplay.Cards.CardInstance, BoardSide, bool>>();

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

        public TurnEnergyLedger GetEnergy(BoardSide side) =>
            side == BoardSide.Player ? PlayerEnergy : AIEnergy;

        public void ResetTurnTracking()
        {
            PlayedThisTurn.Clear();
            PlayerEnergy.Reset();
            AIEnergy.Reset();
        }

        public void ResetCombatFlags()
        {
            PlayerSeesHiddenZones = false;
            AISeesHiddenZones = false;
            PlayerPendingNextPlay.Clear();
            AIPendingNextPlay.Clear();
        }

        public List<PendingNextPlayEffect> GetPendingNextPlay(BoardSide side) =>
            side == BoardSide.Player ? PlayerPendingNextPlay : AIPendingNextPlay;

        public void AddPendingNextPlay(BoardSide side, PendingNextPlayEffect effect)
        {
            if (effect == null) return;
            GetPendingNextPlay(side).Add(effect);
        }

        public void AddPlayRecord(PlayRecord record)
        {
            if (record != null)
                PlayLog.Add(record);
        }

        public void RemovePlayRecords(Gameplay.Cards.CardInstance card)
        {
            if (card == null) return;

            for (int i = PlayLog.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(PlayLog[i].Card, card))
                    PlayLog.RemoveAt(i);
            }
        }

        public bool CanDestroy(Gameplay.Cards.CardInstance card, BoardSide side)
        {
            foreach (var protection in DestroyProtections)
            {
                if (protection != null && !protection(card, side))
                    return false;
            }

            return true;
        }
    }
}
