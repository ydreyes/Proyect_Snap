using System;
using System.Collections.Generic;
using SnapClone.Data.Definitions;

namespace SnapClone.Gameplay.Cards
{
    public enum PendingEffectKind
    {
        AllyPlayedInZoneNextTurn,
        AllyNotPlayedInZoneNextTurn,
        WinZoneNextTurnEnergy
    }

    /// <summary>
    /// Efecto diferido armado por una carta. Hawkeye usa el primero; la lista
    /// permite que varias cartas o varios efectos convivan en la misma instancia.
    /// </summary>
    public class PendingCardEffect
    {
        public PendingEffectKind Kind { get; set; }
        public int ArmedTurn { get; set; }
        public int ZoneIndex { get; set; } = -1;
        public int Amount { get; set; }
    }

    public class CardInstance
    {
        public const int MaxUpgrades = 3;

        public string Id { get; set; }
        public CardDef Definition { get; set; }
        public int CurrentPower { get; set; }
        public int PerkPowerBonus { get; set; }

        /// <summary>Bonus persistente de un efecto OnReveal resuelto.</summary>
        public int BonusPower { get; set; }

        /// <summary>Mejoras permanentes de la run. No se reinician entre combates.</summary>
        public int CostDelta { get; set; }
        public int PowerDelta { get; set; }
        public int UpgradeCount { get; set; }

        /// <summary>Modificador de coste del combate; se limpia al empezar un combate.</summary>
        public int TemporaryCostDelta { get; set; }

        /// <summary>Modificador de coste de este turno; se recalcula cada turno.</summary>
        public int TurnCostDelta { get; set; }

        /// <summary>Coste fijo de combate (Sabretooth, copias de Swarm).</summary>
        public int? CombatCostOverride { get; set; }

        /// <summary>Coste fijo absoluto con vencimiento (Baron Mordo).</summary>
        public int? FixedCost { get; set; }
        public int FixedCostUntilTurn { get; set; } = -1;

        /// <summary>Reduccion de coste de un solo turno (Beast).</summary>
        public int PendingCostReduction { get; set; }
        public int PendingCostReductionTurn { get; set; } = -1;

        /// <summary>Poder base fijado por Bast, Valkyrie, Shadow King o Taskmaster.</summary>
        public int? BasePowerOverride { get; set; }

        /// <summary>Marca una carta creada por un efecto; Quinjet la abarata.</summary>
        public bool IsCreated { get; set; }

        /// <summary>Activar una habilidad solo se permite una vez por combate.</summary>
        public bool ActivatedThisCombat { get; set; }

        /// <summary>Efectos copiados o robados por Absorbing Man, Mystique, Rogue o Super-Skrull.</summary>
        public List<string> CopiedEffectIds { get; set; } = new List<string>();
        public List<string> SuppressedEffectIds { get; set; } = new List<string>();

        /// <summary>Invisible Woman: la carta no revela hasta el final de la partida.</summary>
        public bool DelayedReveal { get; set; }

        /// <summary>Shuri: duplica el Poder al revelarse.</summary>
        public bool PendingDoublePower { get; set; }

        /// <summary>Negasonic: su destruccion ya se uso en el combate.</summary>
        public bool OncePerCombatUsed { get; set; }

        /// <summary>Hercules: turno en el que ya re-movio una carta.</summary>
        public int HerculesMovedTurn { get; set; } = -1;

        /// <summary>Texto alternativo para Spider-Ham y cartas copiadas.</summary>
        public string AbilityTextOverride { get; set; }

        /// <summary>Turno en el que ya se aplico el impuesto de Sandman a esta instancia.</summary>
        public int CostTaxAppliedTurn { get; set; } = -1;

        /// <summary>Nightcrawler: ya uso su movimiento del combate.</summary>
        public bool MoveUsedThisCombat { get; set; }

        /// <summary>Vision: turno en el que uso su movimiento.</summary>
        public int MoveUsedTurn { get; set; } = -1;

        /// <summary>Turno en el que uso la ventana de Cloak.</summary>
        public int CloakMoveUsedTurn { get; set; } = -1;

        public int EffectiveCost =>
            CombatCostOverride.HasValue
                ? Math.Max(0, Math.Min(6, CombatCostOverride.Value))
                : Math.Min(6, Math.Max(0, (Definition?.Cost ?? 0) + CostDelta +
                                         TemporaryCostDelta + TurnCostDelta));

        public int EffectivePower =>
            (BasePowerOverride ?? Definition?.BasePower ?? 0) + PowerDelta + PerkPowerBonus + BonusPower;

        public bool HasUpgrades => UpgradeCount > 0;
        public bool CanUpgrade => UpgradeCount < MaxUpgrades;
        public bool CanUpgradeCost => CanUpgrade && EffectiveCost > 0;

        public int PlayedTurn { get; set; }
        public int PlayOrderIndex { get; set; }

        /// <summary>Zona donde se jugo desde la mano; -1 si no se jugo.</summary>
        public int PlayedZoneIndex { get; set; } = -1;

        /// <summary>Efectos diferidos que esperan una condicion futura.</summary>
        public List<PendingCardEffect> PendingEffects { get; set; } =
            new List<PendingCardEffect>();

        /// <summary>
        /// Marca que la carta ya paso por la secuencia de reveal del combate.
        /// No se limpia al volver a la mano ni al entrar en una pila.
        /// </summary>
        public bool HasRevealed { get; set; }

        /// <summary>Turno en el que la carta paso por la secuencia de reveal.</summary>
        public int RevealedTurn { get; set; } = -1;

        /// <summary>Enchantress y efectos equivalentes apagan todas las habilidades.</summary>
        public bool AbilitiesDisabled { get; set; }

        /// <summary>Silence apaga solo las habilidades impresas; los perks siguen.</summary>
        public bool PrintedAbilitiesDisabled { get; set; }

        /// <summary>Habilidades temporales concedidas durante el combate.</summary>
        public List<string> TemporaryEffectIds { get; set; } = new List<string>();

        /// <summary>Maneuverable: el jugador ya uso su movimiento en este combate.</summary>
        public bool PlayerMoveUsed { get; set; }

        /// <summary>Quickdraw: Discount ya aplico en este combate.</summary>
        public bool QuickdrawDiscountApplied { get; set; }

        /// <summary>Quickdraw: Power ya concedio su efecto en este combate.</summary>
        public bool QuickdrawPowerApplied { get; set; }

        /// <summary>Wakandan Tech ya aplico su bono en este combate.</summary>
        public bool WakandanTechApplied { get; set; }

        /// <summary>Hulkbuster deja el tablero fusionado, sin entrar en una pila.</summary>
        public bool IsMerged { get; set; }

        public List<string> AttachedPerks { get; set; } = new List<string>();
        public bool IsInPlay { get; set; }

        /// <summary>Donde vive la carta. Las pilas solo las asigna CardPileManager.</summary>
        public CardLocation Location { get; set; } = CardLocation.Deck;

        public CardInstance(CardDef def)
        {
            Definition = def;
            Id = def.Id;
            CurrentPower = def.BasePower;
        }

        public bool TryUpgradeCost()
        {
            if (!CanUpgradeCost) return false;

            CostDelta--;
            UpgradeCount++;
            return true;
        }

        public bool TryUpgradePower()
        {
            if (!CanUpgrade) return false;

            PowerDelta += 2;
            UpgradeCount++;
            return true;
        }

        /// <summary>Bast, Valkyrie y Taskmaster: fija el poder base de la instancia.</summary>
        public void SetBasePower(int value)
        {
            BasePowerOverride = value;
            PowerDelta = 0;
            BonusPower = 0;
            PerkPowerBonus = 0;
        }

        /// <summary>Shadow King: vuelve al poder base original y limpia bonus.</summary>
        public void ResetBasePower()
        {
            BasePowerOverride = Definition?.BasePower ?? 0;
            PowerDelta = 0;
            BonusPower = 0;
            PerkPowerBonus = 0;
        }

        /// <summary>Black Panther, Human Torch o Shuri: duplica el poder resuelto.</summary>
        public void DoublePower()
        {
            int current = EffectivePower;
            if (current == 0) return;
            BonusPower += current;
        }

        /// <summary>
        /// Limpia el estado de "jugada este turno". Se usa al volver a la mano
        /// y conserva el bonus persistente, los perks, las mejoras, la marca de
        /// reveal y el estado de habilidades desactivadas.
        /// </summary>
        public void ClearPlayState()
        {
            PlayedTurn = 0;
            PlayOrderIndex = 0;
            PlayedZoneIndex = -1;
            PendingEffects.Clear();
        }

        /// <summary>
        /// Deja la carta lista para un combate nuevo. Los bonus, el reveal y
        /// las habilidades desactivadas son de combate; las mejoras y los perks
        /// son de run.
        /// </summary>
        public void ResetForCombat()
        {
            IsInPlay = false;
            ClearPlayState();
            BonusPower = 0;
            TemporaryCostDelta = 0;
            TurnCostDelta = 0;
            CombatCostOverride = null;
            FixedCost = null;
            FixedCostUntilTurn = -1;
            PendingCostReduction = 0;
            PendingCostReductionTurn = -1;
            BasePowerOverride = null;
            IsCreated = false;
            ActivatedThisCombat = false;
            CopiedEffectIds.Clear();
            SuppressedEffectIds.Clear();
            DelayedReveal = false;
            PendingDoublePower = false;
            OncePerCombatUsed = false;
            HerculesMovedTurn = -1;
            AbilityTextOverride = null;
            CostTaxAppliedTurn = -1;
            MoveUsedThisCombat = false;
            MoveUsedTurn = -1;
            CloakMoveUsedTurn = -1;
            TemporaryEffectIds.Clear();
            HasRevealed = false;
            RevealedTurn = -1;
            AbilitiesDisabled = false;
            PrintedAbilitiesDisabled = false;
            PlayerMoveUsed = false;
            QuickdrawDiscountApplied = false;
            QuickdrawPowerApplied = false;
            WakandanTechApplied = false;
            IsMerged = false;
            Location = CardLocation.Deck;
        }
    }
}
