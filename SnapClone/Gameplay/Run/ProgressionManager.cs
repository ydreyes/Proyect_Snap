namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Mapea el indice de combate a la estructura de la run (3 ciclos de 5)
    /// y decide a que escena va cada resultado.
    /// </summary>
    public class ProgressionManager
    {
        private readonly RunState _runState;

        public ProgressionManager(RunState runState)
        {
            _runState = runState;
        }

        public FightType GetCurrentFightType()
        {
            int combatNumber = _runState.CurrentFightIndex + 1;
            int position = ((combatNumber - 1) % 5) + 1;
            int cycle = (combatNumber - 1) / 5;

            if (position == 2)
                return FightType.PerkReward;

            if (position == 1 || position == 3)
                return FightType.CardReward;

            if (position == 4)
                return FightType.NormalCombat;

            return cycle >= 2 ? FightType.FinalBoss : FightType.SubBoss;
        }

        /// <summary>
        /// Resuelve un combate terminado. Devuelve el nombre de la escena a la
        /// que hay que ir. En victoria con recompensa la run no avanza hasta
        /// que el jugador termina la pantalla de recompensa.
        /// </summary>
        public string ResolveCombat(string result)
        {
            if (result == "Player")
            {
                var fightType = GetCurrentFightType();

                if (fightType == FightType.CardReward)
                {
                    _runState.PendingReward = PendingRewardKind.Card;
                    _runState.AdvanceAfterReward = true;
                    return "Reward";
                }

                if (fightType == FightType.PerkReward)
                {
                    _runState.PendingReward = PendingRewardKind.Perk;
                    _runState.AdvanceAfterReward = true;
                    return "Reward";
                }

                return AdvanceCombat();
            }

            // Derrota o empate: cuesta una vida y repite el mismo combate.
            _runState.LoseLife();

            if (_runState.IsGameOver)
                return "GameOver";

            _runState.PendingReward = PendingRewardKind.Card;
            _runState.AdvanceAfterReward = false;
            return "Reward";
        }

        /// <summary>
        /// Se llama al salir de la pantalla de recompensa. Avanza el combate
        /// solo si la recompensa venia de una victoria.
        /// </summary>
        public string CompleteReward()
        {
            bool advance = _runState.AdvanceAfterReward;

            _runState.PendingReward = PendingRewardKind.None;
            _runState.AdvanceAfterReward = false;

            return advance ? AdvanceCombat() : "Combat";
        }

        private string AdvanceCombat()
        {
            _runState.CurrentFightIndex++;

            if (IsRunComplete())
            {
                _runState.IsVictory = true;
                return "GameOver";
            }

            return "Combat";
        }

        public bool IsRunComplete()
        {
            return _runState.CurrentFightIndex >= RunState.MaxCombats;
        }
    }

    public enum FightType
    {
        NormalCombat,
        CardReward,
        PerkReward,
        SubBoss,
        FinalBoss
    }
}
