using System.Collections.Generic;
using SnapClone.Data.Definitions;

namespace SnapClone.Gameplay.Cards
{
    public class CardInstance
    {
        public string Id { get; set; }
        public CardDef Definition { get; set; }
        public int CurrentPower { get; set; }
        public int PerkPowerBonus { get; set; }
        public int PlayedTurn { get; set; }
        public int PlayOrderIndex { get; set; }
        public List<string> AttachedPerks { get; set; } = new List<string>();
        public bool IsInPlay { get; set; }

        /// <summary>Donde vive la carta. Las pilas solo las asigna CardPileManager.</summary>
        public CardLocation Location { get; set; } = CardLocation.Deck;
        
        public CardInstance(CardDef def)
        {
            Definition = def;
            Id = def.Id;
            CurrentPower = def.BasePower;
        }
    }
}
