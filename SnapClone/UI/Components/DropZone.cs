using Microsoft.Xna.Framework;
using SnapClone.Gameplay.Cards;

namespace SnapClone.UI.Components
{
    public class DropZone
    {
        public Rectangle Bounds { get; set; }
        public int ZoneIndex { get; set; }
        public int SlotIndex { get; set; }
        public CardView OccupiedBy { get; set; }
        public bool IsPlayerSide { get; set; }
        
        public DropZone(Rectangle bounds, int zoneIndex, int slotIndex, bool isPlayerSide)
        {
            Bounds = bounds;
            ZoneIndex = zoneIndex;
            SlotIndex = slotIndex;
            IsPlayerSide = isPlayerSide;
            OccupiedBy = null;
        }
        
        public bool CanAccept(CardView cardView)
        {
            if (OccupiedBy != null) return false;
            if (!IsPlayerSide) return false;
            return true;
        }
        
        public void Accept(CardView cardView)
        {
            OccupiedBy = cardView;
            cardView.IsInZone = true;
            cardView.Position = new Vector2(Bounds.X, Bounds.Y);
        }
    }
}
