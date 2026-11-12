using System.Collections.Generic;
using SnapClone.Data.Registries;
using SnapClone.Effects;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Board;

namespace SnapClone.Gameplay.Combat
{
    /// <summary>
    /// Resuelve el poder de una carta como poder base mas perks, bonus
    /// persistentes y modificadores Ongoing. Nunca lee el poder actual como
    /// entrada, asi el calculo es idempotente y no se acumula entre
    /// evaluaciones.
    /// </summary>
    public class PowerResolver
    {
        private readonly EffectProcessor _effectProcessor;

        public PowerResolver(PerkRegistry perkRegistry = null,
            EffectProcessor effectProcessor = null)
        {
            _effectProcessor = effectProcessor;
        }

        /// <summary>
        /// Poder puro de una carta. Con zona, incluye los efectos Ongoing que
        /// dependen de la zona; sin zona, solo base, perks y bonus persistente.
        /// </summary>
        public int CalculateCardPower(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner)
        {
            if (card?.Definition == null) return 0;

            // El bonus de perk se cachea en la instancia para que EffectivePower
            // funcione tambien fuera de combate. Se refresca antes de leerlo.
            RefreshPerkBonus(card);

            int power = card.EffectivePower;

            if (_effectProcessor != null && zone != null)
            {
                power += _effectProcessor.GetCardPowerBonus(card, zone, boardState, owner);
                power += _effectProcessor.GetZoneCardPowerBonus(zone, card, owner);

                // Apex duplica desde el valor ya bonificado y nunca se acumula:
                // cada recalculo parte de EffectivePower y repite el mismo calculo.
                power *= _effectProcessor.GetPowerMultiplier(card, zone, boardState, owner);
            }

            return power;
        }

        public int CalculatePower(CardInstance card, ZoneInstance zone, BoardState boardState)
        {
            return CalculateCardPower(card, zone, boardState, FindOwner(boardState, card));
        }

        /// <summary>
        /// Total de un lado en la zona: suma de los poderes individuales por el
        /// producto de los multiplicadores Ongoing de ese lado.
        /// </summary>
        public int CalculateZonePower(ZoneInstance zone, BoardSide side)
        {
            if (zone == null) return 0;

            var cards = side == BoardSide.Player ? zone.PlayerCards : zone.AICards;

            int total = 0;
            foreach (var card in cards)
                total += CalculateCardPower(card, zone, zone.Board, side);

            int multiplier = 1;
            int addition = 0;
            if (_effectProcessor != null && zone.Board != null)
            {
                multiplier = _effectProcessor.GetZoneMultiplier(zone, side, zone.Board);
                addition = _effectProcessor.GetZoneAddition(zone, side, zone.Board);
            }

            return total * multiplier + addition;
        }

        /// <summary>Recalcula y escribe el poder de una carta fuera de una zona.</summary>
        public void ResolveCard(CardInstance card)
        {
            WritePower(card, null, null, BoardSide.Player);
        }

        public void UpdateAllPowers(BoardState boardState)
        {
            if (boardState == null) return;

            foreach (var zone in boardState.Zones)
            {
                foreach (var card in zone.PlayerCards)
                    WritePower(card, zone, boardState, BoardSide.Player);

                foreach (var card in zone.AICards)
                    WritePower(card, zone, boardState, BoardSide.AI);
            }

            foreach (var card in boardState.PlayerHand)
                WritePower(card, null, boardState, BoardSide.Player);

            foreach (var card in boardState.AIHand)
                WritePower(card, null, boardState, BoardSide.AI);

            foreach (var card in boardState.PlayerDeck)
                WritePower(card, null, boardState, BoardSide.Player);

            foreach (var card in boardState.AIDeck)
                WritePower(card, null, boardState, BoardSide.AI);
        }

        private void WritePower(CardInstance card, ZoneInstance zone,
            BoardState boardState, BoardSide owner)
        {
            if (card?.Definition == null) return;

            card.CurrentPower = CalculateCardPower(card, zone, boardState, owner);
        }

        /// <summary>Recalcula el bonus de perks que EffectivePower expone.</summary>
        public void RefreshPerkBonus(CardInstance card)
        {
            if (card?.Definition == null) return;
            card.PerkPowerBonus = GetPerkPowerBonus(card);
        }

        private static BoardSide FindOwner(BoardState boardState, CardInstance card)
        {
            if (boardState == null || card == null) return BoardSide.Player;

            foreach (var zone in boardState.Zones)
            {
                if (zone.PlayerCards.Contains(card)) return BoardSide.Player;
                if (zone.AICards.Contains(card)) return BoardSide.AI;
            }

            if (boardState.AIHand.Contains(card) || boardState.AIDeck.Contains(card))
                return BoardSide.AI;

            return BoardSide.Player;
        }

        /// <summary>
        /// Los perks no son modificadores planos: sus efectos se resuelven como
        /// habilidades del anfitrion a traves del procesador.
        /// </summary>
        private int GetPerkPowerBonus(CardInstance card) => 0;
    }
}
