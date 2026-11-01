using System.Collections.Generic;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Run
{
    public enum PendingRewardKind
    {
        None,
        Card,
        Perk
    }

    public class RunState
    {
        public const int MaxCombats = 15;
        public const int MaxDeckSize = 12;

        public int CurrentFightIndex { get; set; } = 0;
        public int LivesRemaining { get; set; } = 2;
        public int MaxLives { get; set; } = 2;
        public List<CardInstance> Deck { get; set; } = new List<CardInstance>();
        public bool IsGameOver { get; set; }
        public bool IsVictory { get; set; }

        /// <summary>Motivo por el que se abrio la pantalla de recompensa.</summary>
        public PendingRewardKind PendingReward { get; set; } = PendingRewardKind.None;

        /// <summary>
        /// Si la recompensa debe avanzar el combate (victoria) o repetirlo
        /// (derrota o empate).
        /// </summary>
        public bool AdvanceAfterReward { get; set; }

        public int CurrentCombatNumber => CurrentFightIndex + 1;
        public bool IsDeckAtCap => Deck.Count >= MaxDeckSize;

        public void StartNewRun()
        {
            Deck.Clear();
            CurrentFightIndex = 0;
            LivesRemaining = MaxLives;
            IsGameOver = false;
            IsVictory = false;
            PendingReward = PendingRewardKind.None;
            AdvanceAfterReward = false;
        }

        public void LoseLife()
        {
            LivesRemaining--;
            if (LivesRemaining <= 0)
            {
                LivesRemaining = 0;
                IsGameOver = true;
            }
        }

        public bool CanAddToDeck() => Deck.Count < MaxDeckSize;
    }
}
