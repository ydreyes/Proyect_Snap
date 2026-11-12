using System.Collections.Generic;

namespace SnapClone.Data.Definitions
{
    public class CardDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public int BasePower { get; set; }
        public string Description { get; set; }
        public List<string> EffectIds { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>Los tokens no entran al draft ni a las ofertas de carta.</summary>
        public bool IsToken { get; set; }

        /// <summary>Tema del draft tematico. Los tokens no declaran tema.</summary>
        public DraftTheme? DraftTheme { get; set; }

        /// <summary>Lote de contenido: CollectionLevel, Series1 o Series2.</summary>
        public CardSeries Series { get; set; } = CardSeries.Series1;

        public bool HasTag(string tag) =>
            Tags != null && Tags.Contains(tag);

        /// <summary>Copia independiente para transformaciones y copias de carta.</summary>
        public CardDef Clone()
        {
            return new CardDef
            {
                Id = Id,
                Name = Name,
                Cost = Cost,
                BasePower = BasePower,
                Description = Description,
                IsToken = IsToken,
                DraftTheme = DraftTheme,
                Series = Series,
                EffectIds = new List<string>(EffectIds ?? new List<string>()),
                Tags = new List<string>(Tags ?? new List<string>())
            };
        }
    }
    
    public class ZoneDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> EffectIds { get; set; } = new List<string>();
    }
    
    public class EffectDef
    {
        public string Id { get; set; }
        public string EffectType { get; set; }
        public EffectTrigger Trigger { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Definicion de un perk de habilidad. El perk otorga sus efectos a la
    /// carta anfitriona; nunca se aplica como regla global del lado.
    /// </summary>
    public class PerkDef
    {
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>Texto de regla mostrado en la coleccion y el level-up.</summary>
        public string RuleText { get; set; }

        public List<string> EffectIds { get; set; } = new List<string>();

        /// <summary>Naturaleza del perk: efecto, meta-habilidad o regla.</summary>
        public PerkKind Kind { get; set; } = PerkKind.GrantedEffect;

        /// <summary>Requisito que debe cumplir la carta anfitriona.</summary>
        public PerkCompatibility Compatibility { get; set; } = PerkCompatibility.Any;

        /// <summary>Una segunda copia agrega otra concesion independiente.</summary>
        public bool Stackable { get; set; } = true;

        public bool HasHostRequirement =>
            Compatibility != PerkCompatibility.Any &&
            Compatibility != PerkCompatibility.RequiresEmpower;

        public string CompatibilityText
        {
            get
            {
                switch (Compatibility)
                {
                    case PerkCompatibility.RequiresOngoing:
                        return "REQUIERE UNA CARTA CON EFECTO CONTINUO";
                    case PerkCompatibility.RequiresOnReveal:
                        return "REQUIERE UNA CARTA CON EFECTO AL REVELARSE";
                    case PerkCompatibility.RequiresEndOfTurn:
                        return "REQUIERE UNA CARTA CON EFECTO DE FIN DE TURNO";
                    case PerkCompatibility.RequiresEmpower:
                        return "CUALQUIER CARTA PUEDE SER POTENCIADA";
                    default:
                        return "CUALQUIER CARTA";
                }
            }
        }

        public string TriggerText
        {
            get
            {
                switch (Kind)
                {
                    case PerkKind.MetaAbility:
                        return "META-HABILIDAD";
                    case PerkKind.RuleKeyword:
                        return "REGLA DE CARTA";
                    default:
                        return "HABILIDAD";
                }
            }
        }
    }

    /// <summary>Definicion data-driven de un modificador global de la run.</summary>
    public class RunModifierDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Definicion data-driven de un objeto de combate. Los objetos viven en el
    /// perfil, no en el tablero, y su efecto se resuelve por id estable.
    /// </summary>
    public class ItemDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemRarity Rarity { get; set; }
        public string EffectId { get; set; }
        public ItemTargetRule TargetRule { get; set; }
    }

    /// <summary>Ids estables de los efectos de objeto implementados.</summary>
    public static class ItemEffectIds
    {
        public const string EnergySurge = "item_effect_energy_surge";
        public const string PowerBoost = "item_effect_power_boost";
        public const string DestroyEnemy = "item_effect_destroy_enemy";
        public const string DrawTwo = "item_effect_draw_two";
        public const string MoveEnemy = "item_effect_move_enemy";
        public const string AfflictZone = "item_effect_afflict_zone";
        public const string CopyHand = "item_effect_copy_hand";
        public const string HealZone = "item_effect_heal_zone";

        public static bool IsKnown(string effectId)
        {
            switch (effectId)
            {
                case EnergySurge:
                case PowerBoost:
                case DestroyEnemy:
                case DrawTwo:
                case MoveEnemy:
                case AfflictZone:
                case CopyHand:
                case HealZone:
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>Recompensas/costes que un evento puede resolver sin iniciar combate.</summary>
    public enum RunEventEffect
    {
        Xp,
        Money,
        Card,
        Upgrade,
        Perk,
        Modifier,
        CardAndUpgrade
    }

    /// <summary>Definicion data-driven de un evento de la run.</summary>
    public class RunEventDef
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string AcceptText { get; set; }
        public int CostMoney { get; set; }
        public RunEventEffect EffectKind { get; set; }
        public int XpAmount { get; set; }
        public int MoneyDelta { get; set; }
        public string ModifierId { get; set; }
    }
}
