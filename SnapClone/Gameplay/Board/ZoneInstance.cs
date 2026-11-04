using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;

namespace SnapClone.Gameplay.Board
{
    /// <summary>
    /// Restriccion de juego por zona. En este cambio no hay ninguna
    /// registrada; Professor X la usara mas adelante.
    /// </summary>
    public interface IZonePlayConstraint
    {
        bool CanPlay(CardInstance card, BoardSide side, ZoneInstance zone);
    }

    public class ZoneInstance
    {
        public const int MaxCardsPerSide = 4;

        public string Id { get; set; }
        public ZoneDef Definition { get; set; }
        public List<CardInstance> PlayerCards { get; set; } = new List<CardInstance>();
        public List<CardInstance> AICards { get; set; } = new List<CardInstance>();
        public List<IZonePlayConstraint> PlayConstraints { get; } =
            new List<IZonePlayConstraint>();
        public bool IsRevealed { get; set; }
        public int ZoneIndex { get; set; }

        /// <summary>
        /// Tablero al que pertenece la zona. Lo asigna BoardState para que los
        /// totales deleguen en el mismo PowerResolver que el resto del juego.
        /// </summary>
        public BoardState Board { get; set; }
        
        public ZoneInstance(ZoneDef def, int index)
        {
            Definition = def;
            Id = def.Id;
            ZoneIndex = index;
            IsRevealed = false;
        }

        public List<CardInstance> CardsFor(BoardSide side) =>
            side == BoardSide.Player ? PlayerCards : AICards;

        public bool CanAdd(BoardSide side) =>
            CardsFor(side).Count < MaxCardsPerSide;

        public bool CanPlay(CardInstance card, BoardSide side)
        {
            foreach (var constraint in PlayConstraints)
            {
                if (constraint != null && !constraint.CanPlay(card, side, this))
                    return false;
            }

            return true;
        }

        /// <summary>Revelado real o visibilidad otorgada por Uatu.</summary>
        public bool IsVisibleTo(BoardSide side)
        {
            if (IsRevealed) return true;
            if (Board == null) return false;

            return side == BoardSide.Player
                ? Board.PlayerSeesHiddenZones
                : Board.AISeesHiddenZones;
        }
        
        public int GetPlayerPower()
        {
            var resolver = Board?.PowerResolver;
            if (resolver != null)
                return resolver.CalculateZonePower(this, BoardSide.Player);

            return SumRaw(PlayerCards);
        }
        
        public int GetAIPower()
        {
            var resolver = Board?.PowerResolver;
            if (resolver != null)
                return resolver.CalculateZonePower(this, BoardSide.AI);

            return SumRaw(AICards);
        }

        /// <summary>
        /// Suma de respaldo cuando la zona no esta conectada a un tablero con
        /// resolver (tests o construccion antigua).
        /// </summary>
        private static int SumRaw(List<CardInstance> cards)
        {
            int total = 0;
            foreach (var card in cards)
                total += card.CurrentPower;

            return total;
        }
    }
}
