using Microsoft.Xna.Framework;

namespace SnapClone.Core.Services
{
    public class TimeService
    {
        public float DeltaTime { get; private set; }
        public float TotalTime { get; private set; }
        public float TurnTimeRemaining { get; private set; }
        public bool TurnTimerActive { get; private set; }
        public float TurnTimeLimit { get; set; } = 45.0f;
        
        public void Update(GameTime gameTime)
        {
            DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TotalTime += DeltaTime;
            
            if (TurnTimerActive && TurnTimeRemaining > 0)
            {
                TurnTimeRemaining -= DeltaTime;
                if (TurnTimeRemaining <= 0)
                {
                    TurnTimeRemaining = 0;
                }
            }
        }
        
        public void StartTurnTimer()
        {
            TurnTimerActive = true;
            TurnTimeRemaining = TurnTimeLimit;
        }
        
        public void StopTurnTimer()
        {
            TurnTimerActive = false;
        }
        
        public bool IsTurnTimeExpired() => TurnTimerActive && TurnTimeRemaining <= 0;
    }
}
