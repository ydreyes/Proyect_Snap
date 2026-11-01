using System.Collections.Generic;

namespace SnapClone.Gameplay.Cards
{
    /// <summary>
    /// Las tres pilas de un lado durante el combate actual. Cada pila conserva
    /// el orden de entrada; la vista las muestra de la mas reciente a la mas
    /// antigua.
    /// </summary>
    public class PileSet
    {
        private readonly Dictionary<PileKind, List<CardInstance>> _piles =
            new Dictionary<PileKind, List<CardInstance>>
            {
                { PileKind.Discard, new List<CardInstance>() },
                { PileKind.Destroyed, new List<CardInstance>() },
                { PileKind.Banished, new List<CardInstance>() }
            };

        public IReadOnlyList<CardInstance> Get(PileKind kind) => _piles[kind];

        public int Count(PileKind kind) => _piles[kind].Count;

        public int TotalCount =>
            _piles[PileKind.Discard].Count +
            _piles[PileKind.Destroyed].Count +
            _piles[PileKind.Banished].Count;

        public bool Contains(CardInstance card)
        {
            if (card == null) return false;

            foreach (var pile in _piles.Values)
            {
                if (pile.Contains(card)) return true;
            }

            return false;
        }

        /// <summary>Agrega una carta; rechaza duplicados del mismo ejemplar.</summary>
        public bool Add(CardInstance card, PileKind kind)
        {
            if (card == null || Contains(card)) return false;

            _piles[kind].Add(card);
            return true;
        }

        public void Clear()
        {
            foreach (var pile in _piles.Values)
                pile.Clear();
        }
    }
}
