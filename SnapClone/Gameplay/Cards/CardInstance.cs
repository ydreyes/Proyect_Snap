using System.Collections.Generic;
using SnapClone.Data.Definitions;

namespace SnapClone.Gameplay.Cards
{
    public enum PendingEffectKind
    {
        AllyPlayedInZoneNextTurn
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
        public string Id { get; set; }
        public CardDef Definition { get; set; }
        public int CurrentPower { get; set; }
        public int PerkPowerBonus { get; set; }

        /// <summary>Bonus persistente de un efecto OnReveal resuelto.</summary>
        public int BonusPower { get; set; }

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

        /// <summary>Enchantress y efectos equivalentes apagan las habilidades.</summary>
        public bool AbilitiesDisabled { get; set; }

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

        /// <summary>
        /// Limpia el estado de "jugada este turno". Se usa al volver a la mano
        /// y conserva el bonus persistente, los perks, la marca de reveal y
        /// el estado de habilidades desactivadas.
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
        /// las habilidades desactivadas son de combate, no de run; los perks si.
        /// </summary>
        public void ResetForCombat()
        {
            IsInPlay = false;
            ClearPlayState();
            BonusPower = 0;
            HasRevealed = false;
            AbilitiesDisabled = false;
            IsMerged = false;
            Location = CardLocation.Deck;
        }
    }
}
