using System;
using System.Collections.Generic;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Meta;

namespace SnapClone.Gameplay.Cards
{
    /// <summary>
    /// Autoridad unica del coste efectivo. Compone el coste base, las
    /// mejoras de run, los deltas temporales, las reducciones dinamicas, las
    /// auras, los impuestos, los valores absolutos, los topes y el suelo en
    /// un orden determinista. Mobius se aplica al final de cada grupo.
    /// </summary>
    public class CostResolver
    {
        public const string EffectDeath = "effect_death";
        public const string EffectSheHulk = "effect_she_hulk";
        public const string EffectStature = "effect_stature";
        public const string EffectMiles = "effect_miles_morales";
        public const string EffectSera = "effect_sera";
        public const string EffectQuinjet = "effect_quinjet";
        public const string EffectZabu = "effect_zabu";
        public const string EffectMobius = "effect_mobius";

        private readonly Effects.EffectProcessor _effects;

        public CostResolver(Effects.EffectProcessor effects = null)
        {
            _effects = effects;
        }

        public int Resolve(CardInstance card, BoardSide side, BoardState board)
        {
            if (card?.Definition == null) return 0;
            if (board == null)
                return Math.Max(0, Math.Min(6, card.EffectiveCost));

            bool ownerMobius = SideHasEffect(board, side, EffectMobius);
            bool enemyMobius = SideHasEffect(board, Other(side), EffectMobius);

            // 1. Override de combate (Sabretooth, Swarm) o base + deltas.
            if (card.CombatCostOverride.HasValue)
                return Clamp(card.CombatCostOverride.Value, 0, 6);

            int baseCost = Math.Max(0, card.Definition.Cost);
            int rawDelta = card.CostDelta + card.TemporaryCostDelta +
                           card.TurnCostDelta;
            if (card.PendingCostReductionTurn == board.CurrentTurn)
                rawDelta -= card.PendingCostReduction;

            int positive = ownerMobius ? 0 : Math.Max(0, rawDelta);
            int negative = enemyMobius ? 0 : Math.Min(0, rawDelta);
            int cost = baseCost + positive + negative;

            // 2. Valor absoluto con vencimiento (Baron Mordo).
            if (card.FixedCost.HasValue &&
                (card.FixedCostUntilTurn < 0 ||
                 board.CurrentTurn <= card.FixedCostUntilTurn))
            {
                bool increases = card.FixedCost.Value > cost;
                if (!(increases && ownerMobius))
                    cost = card.FixedCost.Value;
            }

            // 3. Reducciones dinamicas.
            if (!enemyMobius)
            {
                if (HasEffect(card, EffectDeath))
                    cost -= board.DestroyedCards.Count;
                if (HasEffect(card, EffectSheHulk))
                    cost -= board.GetUnspentEnergyLastTurn(side);
            }

            // 4. Auras y contenedores.
            int floor = 0;
            if (!enemyMobius)
            {
                if (IsInHand(card, side, board) &&
                    SideHasEffect(board, side, EffectSera))
                {
                    cost -= 1;
                    floor = Math.Max(floor, 1);
                }

                if (card.IsCreated &&
                    SideHasEffect(board, side, EffectQuinjet))
                {
                    cost -= 1;
                    floor = Math.Max(floor, 1);
                }

                if (baseCost == 4 && IsInDeck(card, side, board) &&
                    SideHasEffect(board, side, EffectZabu))
                {
                    cost -= 1;
                }
            }

            // 5. Valores absolutos a 1 (Stature, Miles).
            if (!enemyMobius)
            {
                if (HasEffect(card, EffectStature) &&
                    board.GetDiscardCount(Other(side)) > 0)
                {
                    cost = Math.Min(cost, 1);
                }

                if (HasEffect(card, EffectMiles) &&
                    board.LastMoveTurn == board.CurrentTurn - 1)
                {
                    cost = Math.Min(cost, 1);
                }
            }

            // 6. Topes globales.
            if (board.HasGlobalMaxCost(board.CurrentTurn))
                cost = Math.Min(cost, board.GlobalMaxCost);

            cost = Math.Min(cost, 6);
            return Math.Max(floor, Math.Max(0, cost));
        }

        private static bool HasEffect(CardInstance card, string effectId)
        {
            if (card == null || string.IsNullOrWhiteSpace(effectId)) return false;
            return PerkAbilities.EffectiveEffectIds(card,
                !card.PrintedAbilitiesDisabled).Contains(effectId);
        }

        private static bool SideHasEffect(BoardState board, BoardSide side,
            string effectId)
        {
            if (board == null) return false;

            foreach (var zone in board.Zones)
            {
                foreach (var card in zone.CardsFor(side))
                {
                    if (card == null || card.AbilitiesDisabled) continue;
                    if (HasEffect(card, effectId)) return true;
                }
            }

            return false;
        }

        private static bool IsInHand(CardInstance card, BoardSide side,
            BoardState board) =>
            (side == BoardSide.Player ? board.PlayerHand : board.AIHand)
            .Contains(card);

        private static bool IsInDeck(CardInstance card, BoardSide side,
            BoardState board) =>
            (side == BoardSide.Player ? board.PlayerDeck : board.AIDeck)
            .Contains(card);

        private static BoardSide Other(BoardSide side) =>
            side == BoardSide.Player ? BoardSide.AI : BoardSide.Player;

        private static int Clamp(int value, int min, int max) =>
            Math.Max(min, Math.Min(max, value));
    }
}
