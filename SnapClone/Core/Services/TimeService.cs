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

        private bool _turnTimerPaused;

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
            _turnTimerPaused = false;
            TurnTimerActive = true;
            TurnTimeRemaining = TurnTimeLimit;
        }
        
        public void StopTurnTimer()
        {
            _turnTimerPaused = false;
            TurnTimerActive = false;
        }

        /// <summary>Congela el contador conservando el tiempo restante.</summary>
        public void PauseTurnTimer()
        {
            if (TurnTimerActive)
            {
                _turnTimerPaused = true;
                TurnTimerActive = false;
            }
        }

        /// <summary>Reanuda el contador desde el tiempo que quedo al pausar.</summary>
        public void ResumeTurnTimer()
        {
            if (_turnTimerPaused)
            {
                _turnTimerPaused = false;
                TurnTimerActive = true;
            }
        }
        
        public bool IsTurnTimeExpired() => TurnTimerActive && TurnTimeRemaining <= 0;
    }
}
