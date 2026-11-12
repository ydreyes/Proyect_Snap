using System.Collections.Generic;
using SnapClone.Gameplay.Draft;

namespace SnapClone.Persistence
{
    /// <summary>Estado completo del draft para reanudar la misma ronda.</summary>
    public class DraftDto
    {
        public DraftPhase Phase { get; set; }
        public int StepIndex { get; set; }
        public List<List<string>> Groups { get; set; } = new List<List<string>>();
        public List<bool> GroupFilled { get; set; } = new List<bool>();
        public List<string> CurrentOffers { get; set; } = new List<string>();
        public List<string> Picks { get; set; } = new List<string>();
        public Dictionary<string, int> CopyCounts { get; set; } = new Dictionary<string, int>();

        public void Normalize()
        {
            Groups ??= new List<List<string>>();
            GroupFilled ??= new List<bool>();
            CurrentOffers ??= new List<string>();
            Picks ??= new List<string>();
            CopyCounts ??= new Dictionary<string, int>();

            for (int i = 0; i < Groups.Count; i++)
                Groups[i] ??= new List<string>();

            while (GroupFilled.Count < Groups.Count)
                GroupFilled.Add(false);
        }
    }
}
