using System.Collections.Generic;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Combat
{
    public class PriorityCalculator
    {
        public string CalculatePriority(BoardState boardState, string currentPriority)
        {
            int playerZonesControlled = 0;
            int aiZonesControlled = 0;
            
            foreach (var zone in boardState.Zones)
            {
                int playerPower = zone.GetPlayerPower();
                int aiPower = zone.GetAIPower();
                
                if (playerPower > aiPower)
                    playerZonesControlled++;
                else if (aiPower > playerPower)
                    aiZonesControlled++;
                // Tie - no one controls
            }
            
            if (playerZonesControlled > aiZonesControlled)
                return "Player";
            else if (aiZonesControlled > playerZonesControlled)
                return "AI";
            else
                return currentPriority; // Tie-breaker: retain previous priority
        }
    }
}
