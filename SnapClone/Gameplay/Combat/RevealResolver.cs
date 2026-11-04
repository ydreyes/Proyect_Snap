using System.Collections.Generic;
using System.Linq;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Combat
{
    public class RevealResolver
    {
        public List<CardInstance> GetRevealOrder(BoardState boardState)
        {
            List<CardInstance> revealOrder = new List<CardInstance>();
            string priorityOwner = boardState.PriorityOwner;
            
            // Get all cards played this turn
            List<CardInstance> playerCardsThisTurn = new List<CardInstance>();
            List<CardInstance> aiCardsThisTurn = new List<CardInstance>();
            
            foreach (var zone in boardState.Zones)
            {
                foreach (var card in zone.PlayerCards)
                {
                    if (card.PlayedTurn == boardState.CurrentTurn && !card.HasRevealed)
                        playerCardsThisTurn.Add(card);
                }
                foreach (var card in zone.AICards)
                {
                    if (card.PlayedTurn == boardState.CurrentTurn && !card.HasRevealed)
                        aiCardsThisTurn.Add(card);
                }
            }
            
            // Sort by play order
            playerCardsThisTurn = playerCardsThisTurn.OrderBy(c => c.PlayOrderIndex).ToList();
            aiCardsThisTurn = aiCardsThisTurn.OrderBy(c => c.PlayOrderIndex).ToList();
            
            // Priority owner first
            if (priorityOwner == "Player")
            {
                revealOrder.AddRange(playerCardsThisTurn);
                revealOrder.AddRange(aiCardsThisTurn);
            }
            else
            {
                revealOrder.AddRange(aiCardsThisTurn);
                revealOrder.AddRange(playerCardsThisTurn);
            }
            
            return revealOrder;
        }
    }
}
