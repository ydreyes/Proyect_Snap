using System.Collections.Generic;

namespace SnapClone.Gameplay.Draft
{
    public enum DraftPhase
    {
        /// <summary>Rondas 0-2: un grupo tematico por ronda.</summary>
        Themed,

        /// <summary>Rondas 3-7: eleccion libre del pool propio restante.</summary>
        Random,

        Complete
    }

    /// <summary>
    /// Estado serializable del draft. Los tres grupos se generan una sola vez
    /// al iniciar la run y se conservan en el checkpoint para que reanudar no
    /// rebaraje nada.
    /// </summary>
    public class DraftState
    {
        public DraftPhase Phase { get; set; } = DraftPhase.Themed;
        public int StepIndex { get; set; }
        public List<List<string>> Groups { get; set; } = new List<List<string>>();

        /// <summary>Marca si el grupo necesito relleno por falta de cartas del tema.</summary>
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
