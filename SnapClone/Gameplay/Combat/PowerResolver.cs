using System.Collections.Generic;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Board;

namespace SnapClone.Gameplay.Combat
{
    /// <summary>
    /// Resuelve el poder de una carta como poder base mas los modificadores
    /// de sus perks. Nunca lee el poder actual como entrada, asi el calculo
    /// es idempotente y no se acumula entre evaluaciones.
    /// </summary>
    public class PowerResolver
    {
        private readonly PerkRegistry _perkRegistry;

        public PowerResolver(PerkRegistry perkRegistry = null)
        {
            _perkRegistry = perkRegistry;
        }

        public int CalculatePower(CardInstance card, ZoneInstance zone, BoardState boardState)
        {
            return ResolvePower(card);
        }

        /// <summary>Recalcula y escribe el poder de una carta.</summary>
        public void ResolveCard(CardInstance card)
        {
            if (card?.Definition == null) return;

            int bonus = GetPerkPowerBonus(card);
            card.PerkPowerBonus = bonus;
            card.CurrentPower = card.Definition.BasePower + bonus;
        }

        private int GetPerkPowerBonus(CardInstance card)
        {
            if (_perkRegistry == null || card.AttachedPerks == null) return 0;

            int total = 0;
            foreach (var perkId in card.AttachedPerks)
            {
                var perk = _perkRegistry.Get(perkId);
                if (perk?.Modifiers != null &&
                    perk.Modifiers.TryGetValue("power", out int value))
                {
                    total += value;
                }
            }

            return total;
        }

        private int ResolvePower(CardInstance card)
        {
            if (card?.Definition == null) return 0;

            return card.Definition.BasePower + GetPerkPowerBonus(card);
        }

        public void UpdateAllPowers(BoardState boardState)
        {
            foreach (var zone in boardState.Zones)
            {
                foreach (var card in zone.PlayerCards)
                    ResolveCard(card);

                foreach (var card in zone.AICards)
                    ResolveCard(card);
            }

            foreach (var card in boardState.PlayerHand)
                ResolveCard(card);

            foreach (var card in boardState.AIHand)
                ResolveCard(card);

            foreach (var card in boardState.PlayerDeck)
                ResolveCard(card);

            foreach (var card in boardState.AIDeck)
                ResolveCard(card);
        }
    }
}
