using System;
using System.Collections.Generic;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Actions
{
    /// <summary>
    /// Energia del turno por lado. Solo devuelve coste cuando la carta vuelve
    /// a la mano; destruir, fusionar o desterrar no devuelve nada.
    /// </summary>
    public class TurnEnergyLedger
    {
        private readonly Dictionary<CardInstance, int> _charges =
            new Dictionary<CardInstance, int>();

        public int Charged { get; private set; }
        public int Refunded { get; private set; }

        public int Available(int pool) =>
            Math.Max(0, pool - (Charged - Refunded));

        public bool IsCharged(CardInstance card) =>
            card != null && _charges.ContainsKey(card);

        public bool Charge(CardInstance card, int cost)
        {
            if (card == null || cost < 0) return false;
            if (_charges.ContainsKey(card)) return false;

            _charges[card] = cost;
            Charged += cost;
            return true;
        }

        public bool Refund(CardInstance card)
        {
            if (card == null) return false;
            if (!_charges.TryGetValue(card, out int cost)) return false;

            _charges.Remove(card);
            Refunded += cost;
            return true;
        }

        public void Reset()
        {
            _charges.Clear();
            Charged = 0;
            Refunded = 0;
        }
    }
}
