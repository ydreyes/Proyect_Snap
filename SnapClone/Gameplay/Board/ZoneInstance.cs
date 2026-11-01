using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;

namespace SnapClone.Gameplay.Board
{
    public class ZoneInstance
    {
        public const int MaxCardsPerSide = 4;

        public string Id { get; set; }
        public ZoneDef Definition { get; set; }
        public List<CardInstance> PlayerCards { get; set; } = new List<CardInstance>();
        public List<CardInstance> AICards { get; set; } = new List<CardInstance>();
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
