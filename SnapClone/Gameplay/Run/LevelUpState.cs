using System.Collections.Generic;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Run
{
    public enum LevelUpBranch
    {
        None,
        Card,
        Perk
    }

    /// <summary>
    /// Nivel pendiente de resolver. Las ofertas se generan una sola vez para
    /// que volver a entrar en la escena no las rebaraje.
    /// </summary>
    public class LevelUpState
    {
        public LevelUpState(int level)
        {
            Level = level;
        }

        public int Level { get; }

        public LevelUpBranch Branch { get; set; } = LevelUpBranch.None;
        public bool OffersGenerated { get; set; }

        public List<CardInstance> CardOffers { get; } = new List<CardInstance>();
        public List<CardInstance> UpgradeCandidates { get; } = new List<CardInstance>();
        public List<string> PerkOffers { get; } = new List<string>();
    }
}
