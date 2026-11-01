using System.Collections.Generic;
using SnapClone.Data.Definitions;

namespace SnapClone.Gameplay.Cards
{
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

        /// <summary>Turno en que se armo un bonus diferido; 0 si no hay ninguno.</summary>
        public int PendingBonusTurn { get; set; }

        /// <summary>Zona vigilada por un bonus diferido; -1 si no hay ninguno.</summary>
        public int PendingBonusZoneIndex { get; set; } = -1;

        /// <summary>Bonus que se aplicara si se cumple la condicion diferida.</summary>
        public int PendingBonusAmount { get; set; }

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
        /// y conserva el bonus persistente y los perks.
        /// </summary>
        public void ClearPlayState()
        {
            PlayedTurn = 0;
            PlayOrderIndex = 0;
            PlayedZoneIndex = -1;
            PendingBonusTurn = 0;
            PendingBonusZoneIndex = -1;
            PendingBonusAmount = 0;
        }

        /// <summary>
        /// Deja la carta lista para un combate nuevo. Los bonus de combate no
        /// pasan de una partida a la siguiente; los perks si.
        /// </summary>
        public void ResetForCombat()
        {
            IsInPlay = false;
            ClearPlayState();
            BonusPower = 0;
            Location = CardLocation.Deck;
        }
    }
}
