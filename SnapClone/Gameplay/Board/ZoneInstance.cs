using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;

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
        
        public ZoneInstance(ZoneDef def, int index)
        {
            Definition = def;
            Id = def.Id;
            ZoneIndex = index;
            IsRevealed = false;
        }
        
        public int GetPlayerPower()
        {
            int total = 0;
            foreach (var card in PlayerCards)
            {
                total += card.CurrentPower;
            }
            return total;
        }
        
        public int GetAIPower()
        {
            int total = 0;
            foreach (var card in AICards)
            {
                total += card.CurrentPower;
            }
            return total;
        }
    }
}
