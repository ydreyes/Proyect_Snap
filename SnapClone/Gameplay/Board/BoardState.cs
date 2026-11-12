using System;
using System.Collections.Generic;
using SnapClone.Gameplay.Actions;

namespace SnapClone.Gameplay.Board
{
    public enum PendingNextPlayKind
    {
        BonusPower,
        MoveLeft,
        Silence,
        DoublePower,
        SwapWithDeck
    }

    public class PendingNextPlayEffect
    {
        public PendingNextPlayKind Kind { get; set; }
        public int Amount { get; set; }
    }

    /// <summary>Taunt: la primera carta del lado reservado debe ir a la zona.</summary>
    public class TauntReservation
    {
        public BoardSide Side { get; set; }
        public int ZoneIndex { get; set; }
        public int Turn { get; set; }
    }

    /// <summary>Ventana temporal de movimiento, como la de Cloak.</summary>
    public class MoveWindow
    {
        public int ZoneIndex { get; set; }
        public int Turn { get; set; }
        public BoardSide? Side { get; set; }
    }

    /// <summary>Registro estable de una carta destruida durante el combate.</summary>
    public class DestroyedCardRecord
    {
        public Gameplay.Cards.CardInstance Card { get; set; }
        public BoardSide Owner { get; set; }
        public int PowerAtDestruction { get; set; }
        public int CostAtDestruction { get; set; }
        public int Turn { get; set; }
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

        /// <summary>Reservas de Taunt que aplican al primer juego del lado.</summary>
        public List<TauntReservation> TauntReservations { get; } =
            new List<TauntReservation>();

        /// <summary>Ultimo turno en el que una carta se movio en el tablero.</summary>
        public int LastMoveTurn { get; set; } = -1;

        /// <summary>Descartes exitosos por lado en el combate actual.</summary>
        public int PlayerDiscardCount { get; private set; }
        public int AIDiscardCount { get; private set; }

        /// <summary>Ventanas temporales de movimiento (Cloak).</summary>
        public List<MoveWindow> MoveWindows { get; } = new List<MoveWindow>();

        /// <summary>Impuesto de coste para un turno futuro (Sandman).</summary>
        public int NextTurnCostTax { get; private set; }
        public int NextTurnCostTaxTurn { get; private set; } = -1;

        /// <summary>Cartas destruidas en el combate actual, para Death y Knull.</summary>
        public List<DestroyedCardRecord> DestroyedCards { get; } =
            new List<DestroyedCardRecord>();

        /// <summary>Energia maxima adicional o reducida por lado (Electro, Havok).</summary>
        public int PlayerMaxEnergyDelta { get; set; }
        public int AIMaxEnergyDelta { get; set; }

        /// <summary>Energia extra del siguiente turno (Psylocke).</summary>
        public int PlayerNextTurnEnergyBonus { get; set; }
        public int AINextTurnEnergyBonus { get; set; }

        /// <summary>Tope global de coste (Wave) y su ultimo turno vigente.</summary>
        public int GlobalMaxCost { get; private set; }
        public int GlobalMaxCostUntilTurn { get; private set; } = -1;

        /// <summary>Energia sin gastar del turno anterior (She-Hulk).</summary>
        public int PlayerUnspentEnergyLastTurn { get; private set; }
        public int AIUnspentEnergyLastTurn { get; private set; }

        /// <summary>Hawk: las habilidades de zona no aplican hasta este turno.</summary>
        public int ZoneEffectsSuppressedUntilTurn { get; private set; } = -1;

        /// <summary>Agatha: controla los turnos pares del jugador.</summary>
        public bool AgathaControlsEvenTurns { get; set; }


        /// <summary>Guardas para que Start/End of Turn corran una vez por turno.</summary>
        public int StartOfTurnResolvedTurn { get; set; } = -1;
        public int EndOfTurnResolvedTurn { get; set; } = -1;

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
            TauntReservations.Clear();
            MoveWindows.Clear();
            LastMoveTurn = -1;
            StartOfTurnResolvedTurn = -1;
            EndOfTurnResolvedTurn = -1;
            PlayerDiscardCount = 0;
            AIDiscardCount = 0;
            NextTurnCostTax = 0;
            NextTurnCostTaxTurn = -1;
            DestroyedCards.Clear();
            PlayerMaxEnergyDelta = 0;
            AIMaxEnergyDelta = 0;
            PlayerNextTurnEnergyBonus = 0;
            AINextTurnEnergyBonus = 0;
            GlobalMaxCost = 0;
            GlobalMaxCostUntilTurn = -1;
            PlayerUnspentEnergyLastTurn = 0;
            AIUnspentEnergyLastTurn = 0;
            ZoneEffectsSuppressedUntilTurn = -1;
            AgathaControlsEvenTurns = false;
        }

        public void RecordDestroyedCard(Gameplay.Cards.CardInstance card,
            BoardSide owner, int power, int cost, int turn)
        {
            if (card == null) return;

            DestroyedCards.Add(new DestroyedCardRecord
            {
                Card = card,
                Owner = owner,
                PowerAtDestruction = power,
                CostAtDestruction = cost,
                Turn = turn
            });
        }

        public int GetMaxEnergyDelta(BoardSide side) =>
            side == BoardSide.Player ? PlayerMaxEnergyDelta : AIMaxEnergyDelta;

        public void AddMaxEnergyDelta(BoardSide side, int delta)
        {
            if (side == BoardSide.Player)
                PlayerMaxEnergyDelta += delta;
            else
                AIMaxEnergyDelta += delta;
        }

        public int GetNextTurnEnergyBonus(BoardSide side) =>
            side == BoardSide.Player
                ? PlayerNextTurnEnergyBonus
                : AINextTurnEnergyBonus;

        public void AddNextTurnEnergyBonus(BoardSide side, int amount)
        {
            if (side == BoardSide.Player)
                PlayerNextTurnEnergyBonus += amount;
            else
                AINextTurnEnergyBonus += amount;
        }

        public int ConsumeNextTurnEnergyBonus(BoardSide side)
        {
            int amount = GetNextTurnEnergyBonus(side);
            if (side == BoardSide.Player)
                PlayerNextTurnEnergyBonus = 0;
            else
                AINextTurnEnergyBonus = 0;

            return amount;
        }

        public void SetGlobalMaxCost(int maxCost, int untilTurn)
        {
            if (maxCost < 0) return;
            if (untilTurn < GlobalMaxCostUntilTurn) return;

            GlobalMaxCost = maxCost;
            GlobalMaxCostUntilTurn = untilTurn;
        }

        public bool HasGlobalMaxCost(int turn) =>
            GlobalMaxCost > 0 && turn <= GlobalMaxCostUntilTurn;

        public void ClearGlobalMaxCost()
        {
            GlobalMaxCost = 0;
            GlobalMaxCostUntilTurn = -1;
        }

        public void SetUnspentEnergyLastTurn(BoardSide side, int amount)
        {
            if (side == BoardSide.Player)
                PlayerUnspentEnergyLastTurn = Math.Max(0, amount);
            else
                AIUnspentEnergyLastTurn = Math.Max(0, amount);
        }

        public int GetUnspentEnergyLastTurn(BoardSide side) =>
            side == BoardSide.Player
                ? PlayerUnspentEnergyLastTurn
                : AIUnspentEnergyLastTurn;

        public void SuppressZoneEffects(int untilTurn)
        {
            if (untilTurn > ZoneEffectsSuppressedUntilTurn)
                ZoneEffectsSuppressedUntilTurn = untilTurn;
        }

        public bool AreZoneEffectsSuppressed(int turn) =>
            turn <= ZoneEffectsSuppressedUntilTurn;

        public int GetDiscardCount(BoardSide side) =>
            side == BoardSide.Player ? PlayerDiscardCount : AIDiscardCount;

        public void IncrementDiscard(BoardSide side)
        {
            if (side == BoardSide.Player)
                PlayerDiscardCount++;
            else
                AIDiscardCount++;
        }

        public bool HasPlayedOnTurn(BoardSide side, int turn)
        {
            if (turn < 0) return false;

            foreach (var record in PlayLog)
            {
                if (record != null && record.Side == side && record.Turn == turn)
                    return true;
            }

            return false;
        }

        public int GetPlayedCount(BoardSide side, int turn)
        {
            int count = 0;
            foreach (var record in PlayLog)
            {
                if (record != null && record.Side == side && record.Turn == turn)
                    count++;
            }

            return count;
        }

        public void AddMoveWindow(int zoneIndex, int turn, BoardSide? side = null)
        {
            if (zoneIndex < 0 || turn < 0) return;

            MoveWindows.Add(new MoveWindow
            {
                ZoneIndex = zoneIndex,
                Turn = turn,
                Side = side
            });
        }

        public bool HasMoveWindow(int zoneIndex, int turn, BoardSide side)
        {
            foreach (var window in MoveWindows)
            {
                if (window == null) continue;
                if (window.ZoneIndex != zoneIndex || window.Turn != turn) continue;
                if (window.Side.HasValue && window.Side.Value != side) continue;
                return true;
            }

            return false;
        }

        public bool HasAnyMoveWindow(int turn, BoardSide side)
        {
            foreach (var window in MoveWindows)
            {
                if (window == null || window.Turn != turn) continue;
                if (window.Side.HasValue && window.Side.Value != side) continue;
                return true;
            }

            return false;
        }

        public void ExpireMoveWindows(int turn)
        {
            MoveWindows.RemoveAll(window => window == null || window.Turn < turn);
        }

        public void AddNextTurnCostTax(int turn, int amount)
        {
            if (turn < 0 || amount <= 0) return;

            if (NextTurnCostTaxTurn == turn)
            {
                NextTurnCostTax += amount;
                return;
            }

            if (NextTurnCostTaxTurn < turn)
            {
                NextTurnCostTaxTurn = turn;
                NextTurnCostTax = amount;
            }
        }

        public int GetCostTaxForTurn(int turn) =>
            NextTurnCostTaxTurn == turn ? NextTurnCostTax : 0;

        public void ClearNextTurnCostTax()
        {
            NextTurnCostTax = 0;
            NextTurnCostTaxTurn = -1;
        }

        public void AddTauntReservation(BoardSide side,
            int zoneIndex, int turn)
        {
            if (zoneIndex < 0) return;
            TauntReservations.Add(new TauntReservation
            {
                Side = side,
                ZoneIndex = zoneIndex,
                Turn = turn
            });
        }

        /// <summary>Primera reserva activa para el lado y turno indicados.</summary>
        public TauntReservation GetTauntReservation(BoardSide side,
            int turn)
        {
            foreach (var reservation in TauntReservations)
            {
                if (reservation != null && reservation.Side == side &&
                    reservation.Turn == turn)
                    return reservation;
            }

            return null;
        }

        public void ExpireTauntReservations(int turn)
        {
            TauntReservations.RemoveAll(r => r == null || r.Turn < turn);
        }

        /// <summary>El lado ya jugo al menos una carta durante el turno.</summary>
        public bool HasPlayedThisTurn(BoardSide side)
        {
            foreach (var record in PlayLog)
            {
                if (record != null && record.Side == side &&
                    record.Turn == CurrentTurn)
                    return true;
            }

            return false;
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
