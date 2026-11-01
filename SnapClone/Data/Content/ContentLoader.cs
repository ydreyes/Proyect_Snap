using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using System.Collections.Generic;

namespace SnapClone.Data.Content
{
    public class ContentLoader
    {
        public static void LoadAll(CardRegistry cardRegistry, ZoneRegistry zoneRegistry, 
            EffectRegistry effectRegistry, PerkRegistry perkRegistry)
        {
            LoadZones(zoneRegistry);
            LoadEffects(effectRegistry);
            LoadPerks(perkRegistry);
            LoadCards(cardRegistry);
        }
        
        private static void LoadZones(ZoneRegistry zoneRegistry)
        {
            // Test zone 1: +1 power to all cards
            zoneRegistry.Register(new ZoneDef
            {
                Id = "zone_plus1_all",
                Name = "NOVA ROMA",
                Description = "Todas las cartas ganan +1 de Poder.",
                EffectIds = new List<string> { "zone_effect_plus1_all" }
            });
            
            // Test zone 2: -1 power to all cards
            zoneRegistry.Register(new ZoneDef
            {
                Id = "zone_minus1_all",
                Name = "CIUDAD PERDIDA",
                Description = "Todas las cartas pierden -1 de Poder.",
                EffectIds = new List<string> { "zone_effect_minus1_all" }
            });
            
            // Test zone 3: +2 power to highest cost card
            zoneRegistry.Register(new ZoneDef
            {
                Id = "zone_plus2_highest_cost",
                Name = "EMBAJADA WAKANDIANA",
                Description = "+2 de Poder a la carta de mayor coste.",
                EffectIds = new List<string> { "zone_effect_plus2_highest_cost" }
            });
        }
        
        private static void LoadEffects(EffectRegistry effectRegistry)
        {
            // Zone effects: siguen registrados porque las zonas los referencian,
            // aunque este cambio no los implementa.
            effectRegistry.Register(new EffectDef { Id = "zone_effect_plus1_all", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", 1 }, { "target", "all" } } });
            effectRegistry.Register(new EffectDef { Id = "zone_effect_minus1_all", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", -1 }, { "target", "all" } } });
            effectRegistry.Register(new EffectDef { Id = "zone_effect_plus2_highest_cost", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", 2 }, { "target", "highest_cost" } } });

            // Efectos de carta de este pool.
            effectRegistry.Register(new EffectDef { Id = "effect_hawkeye_on_reveal", EffectType = "OnRevealArmNextTurnBonus", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 5 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_iron_man_ongoing", EffectType = "OngoingZonePowerMultiplier", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "multiplier", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_medusa_on_reveal", EffectType = "OnRevealBonusIfMiddle", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 3 }, { "zoneIndex", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_punisher_ongoing", EffectType = "OngoingPowerPerEnemyCard", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_sentinel_on_reveal", EffectType = "OnRevealAddCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cardId", "card_sentinel" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_star_lord_on_reveal", EffectType = "OnRevealBonusIfEnemyPlayedHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 4 } } });
        }
        
        private static void LoadPerks(PerkRegistry perkRegistry)
        {
            perkRegistry.Register(new PerkDef { Id = "perk_plus1", Name = "+1 Power", Modifiers = new Dictionary<string, int> { { "power", 1 } } });
            perkRegistry.Register(new PerkDef { Id = "perk_minus1", Name = "-1 Power", Modifiers = new Dictionary<string, int> { { "power", -1 } } });
            perkRegistry.Register(new PerkDef { Id = "perk_plus2", Name = "+2 Power", Modifiers = new Dictionary<string, int> { { "power", 2 } } });
        }
        
        private static void LoadCards(CardRegistry cardRegistry)
        {
            // 13 cartas starter + 7 rellenos vanilla = 20 cartas.

            // Starter vanilla
            cardRegistry.Register(new CardDef { Id = "card_abomination", Name = "Abomination", Cost = 5, BasePower = 9, Description = "Un cuerpo enorme de coste alto. Su poder cierra zonas y aguanta la presion rival." });
            cardRegistry.Register(new CardDef { Id = "card_cyclops", Name = "Cyclops", Cost = 3, BasePower = 5, Description = "Un cuerpo medio eficiente. Aporta mas poder del que cuesta y ocupa una zona con solidez." });
            cardRegistry.Register(new CardDef { Id = "card_hulk", Name = "Hulk", Cost = 6, BasePower = 14, Description = "El mayor golpe de la baraja. Donde cae, la zona suele quedar resuelta." });
            cardRegistry.Register(new CardDef { Id = "card_misty_knight", Name = "Misty Knight", Cost = 1, BasePower = 3, Description = "Una carta barata con gran poder para su coste. Ideal para abrir una zona temprano." });
            cardRegistry.Register(new CardDef { Id = "card_shocker", Name = "Shocker", Cost = 2, BasePower = 4, Description = "Un cuerpo de coste bajo con poder de sobra. Buen fichaje para cualquier zona." });
            cardRegistry.Register(new CardDef { Id = "card_the_thing", Name = "The Thing", Cost = 4, BasePower = 7, Description = "Una mole de coste medio. Su poder inclina zonas cerradas y aguanta la respuesta rival." });

            // Starter con habilidad
            cardRegistry.Register(new CardDef
            {
                Id = "card_hawkeye",
                Name = "Hawkeye",
                Cost = 1,
                BasePower = 1,
                Description = "Al revelarse: si juegas una carta aqui el siguiente turno, gana +5 de Poder.",
                EffectIds = new List<string> { "effect_hawkeye_on_reveal" }
            });
            cardRegistry.Register(new CardDef
            {
                Id = "card_iron_man",
                Name = "Iron Man",
                Cost = 5,
                BasePower = 0,
                Description = "Continuo: duplica el Poder total de tu zona.",
                EffectIds = new List<string> { "effect_iron_man_ongoing" }
            });
            cardRegistry.Register(new CardDef
            {
                Id = "card_medusa",
                Name = "Medusa",
                Cost = 2,
                BasePower = 2,
                Description = "Al revelarse: si esta en la zona central, gana +3 de Poder.",
                EffectIds = new List<string> { "effect_medusa_on_reveal" }
            });
            cardRegistry.Register(new CardDef
            {
                Id = "card_punisher",
                Name = "Punisher",
                Cost = 3,
                BasePower = 3,
                Description = "Continuo: +1 de Poder por cada carta enemiga en esta zona.",
                EffectIds = new List<string> { "effect_punisher_ongoing" }
            });
            cardRegistry.Register(new CardDef
            {
                Id = "card_quicksilver",
                Name = "Quicksilver",
                Cost = 1,
                BasePower = 2,
                Description = "Empieza siempre en tu mano inicial.",
                Tags = new List<string> { "starts_in_hand" }
            });
            cardRegistry.Register(new CardDef
            {
                Id = "card_sentinel",
                Name = "Sentinel",
                Cost = 2,
                BasePower = 3,
                Description = "Al revelarse: agrega una Sentinel a tu mano.",
                EffectIds = new List<string> { "effect_sentinel_on_reveal" }
            });
            cardRegistry.Register(new CardDef
            {
                Id = "card_star_lord",
                Name = "Star-Lord",
                Cost = 2,
                BasePower = 2,
                Description = "Al revelarse: si el rival jugo una carta en esta zona este turno, gana +4 de Poder.",
                EffectIds = new List<string> { "effect_star_lord_on_reveal" }
            });

            // Rellenos vanilla
            cardRegistry.Register(new CardDef { Id = "card_spark", Name = "Spark", Cost = 0, BasePower = 1, Description = "Chispa gratuita. Ocupa una casilla sin gastar energia y suma desde el primer turno." });
            cardRegistry.Register(new CardDef { Id = "card_raven", Name = "Raven", Cost = 2, BasePower = 4, Description = "Ave de presa: mucha fuerza por poco coste. Castiga una zona que el rival descuida." });
            cardRegistry.Register(new CardDef { Id = "card_champion", Name = "Champion", Cost = 3, BasePower = 4, Description = "Campeon de coste medio y golpe firme. Destaca en zonas donde el rival no puede responder." });
            cardRegistry.Register(new CardDef { Id = "card_behemoth", Name = "Behemoth", Cost = 4, BasePower = 4, Description = "Criatura enorme a precio justo. Su peso sirve para inclinar una zona cerrada." });
            cardRegistry.Register(new CardDef { Id = "card_juggernaut", Name = "Juggernaut", Cost = 4, BasePower = 6, Description = "Imparable cuando llega. Un golpe de seis casi siempre decide una zona." });
            cardRegistry.Register(new CardDef { Id = "card_titan", Name = "Titan", Cost = 5, BasePower = 5, Description = "Un titan de coste alto. Cuando cae, su poder se nota en el total de la zona." });
            cardRegistry.Register(new CardDef { Id = "card_leviathan", Name = "Leviathan", Cost = 6, BasePower = 8, Description = "Leviatan: un cuerpo enorme de coste maximo. Su poder sostiene una zona disputada." });
        }
    }
}
