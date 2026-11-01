namespace SnapClone.Gameplay.Combat
{
    public interface IWinCondition
    {
        string Evaluate(Gameplay.Board.BoardState boardState);
    }
    
    public class TotalPowerWinCondition : IWinCondition
    {
        public string Evaluate(Gameplay.Board.BoardState boardState)
        {
            int playerTotal = 0;
            int aiTotal = 0;
            
            foreach (var zone in boardState.Zones)
            {
                playerTotal += zone.GetPlayerPower();
                aiTotal += zone.GetAIPower();
            }
            
            if (playerTotal > aiTotal) return "Player";
            if (aiTotal > playerTotal) return "AI";
            return "Draw";
        }
    }
    
    public class VictoryResolver
    {
        private IWinCondition _winCondition;
        
        public VictoryResolver()
        {
            _winCondition = new TotalPowerWinCondition();
        }
        
        public void SetWinCondition(IWinCondition condition)
        {
            _winCondition = condition;
        }
        
        public string Evaluate(Gameplay.Board.BoardState boardState)
        {
            return _winCondition.Evaluate(boardState);
        }
    }
}
