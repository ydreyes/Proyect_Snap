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
            // Zone effects
            effectRegistry.Register(new EffectDef { Id = "zone_effect_plus1_all", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", 1 }, { "target", "all" } } });
            effectRegistry.Register(new EffectDef { Id = "zone_effect_minus1_all", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", -1 }, { "target", "all" } } });
            effectRegistry.Register(new EffectDef { Id = "zone_effect_plus2_highest_cost", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", 2 }, { "target", "highest_cost" } } });
            
            // Create 20 effects across 5 types (5 of each)
            for (int i = 1; i <= 5; i++)
            {
                effectRegistry.Register(new EffectDef { Id = $"onreveal_{i}", EffectType = "OnRevealEffect", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
                effectRegistry.Register(new EffectDef { Id = $"ongoing_{i}", EffectType = "OngoingEffect", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
                effectRegistry.Register(new EffectDef { Id = $"endofturn_{i}", EffectType = "EndOfTurnEffect", Trigger = EffectTrigger.EndOfTurn, Parameters = new Dictionary<string, object>() });
                effectRegistry.Register(new EffectDef { Id = $"activate_{i}", EffectType = "ActivateEffect", Trigger = EffectTrigger.Activate, Parameters = new Dictionary<string, object>() });
                effectRegistry.Register(new EffectDef { Id = $"zone_{i}", EffectType = "ZoneEffect", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object>() });
            }
        }
        
        private static void LoadPerks(PerkRegistry perkRegistry)
        {
            perkRegistry.Register(new PerkDef { Id = "perk_plus1", Name = "+1 Power", Modifiers = new Dictionary<string, int> { { "power", 1 } } });
            perkRegistry.Register(new PerkDef { Id = "perk_minus1", Name = "-1 Power", Modifiers = new Dictionary<string, int> { { "power", -1 } } });
            perkRegistry.Register(new PerkDef { Id = "perk_plus2", Name = "+2 Power", Modifiers = new Dictionary<string, int> { { "power", 2 } } });
        }
        
        private static void LoadCards(CardRegistry cardRegistry)
        {
            cardRegistry.Register(new CardDef { Id = "card_1", Name = "Weakling", Cost = 1, BasePower = 1, Description = "Un cuerpo barato para abrir una zona sin gastar energia. Rinde cuando la partida apenas empieza." });
            cardRegistry.Register(new CardDef { Id = "card_2", Name = "Fighter", Cost = 2, BasePower = 2, Description = "Combatiente equilibrado: paga lo que vale y no pide mas. Un relleno fiable para cualquier zona." });
            cardRegistry.Register(new CardDef { Id = "card_3", Name = "Brute", Cost = 3, BasePower = 3, Description = "Fuerza bruta de coste medio. Util para igualar una zona que el rival ya esta disputando." });
            cardRegistry.Register(new CardDef { Id = "card_4", Name = "Behemoth", Cost = 4, BasePower = 4, Description = "Criatura enorme a precio justo. Su peso sirve para inclinar una zona cerrada." });
            cardRegistry.Register(new CardDef { Id = "card_5", Name = "Titan", Cost = 5, BasePower = 5, Description = "Un titan de coste alto. Cuando cae, su poder se nota en el total de la zona." });
            cardRegistry.Register(new CardDef { Id = "card_6", Name = "Scout", Cost = 1, BasePower = 2, Description = "Explorador barato con mas poder del que su coste sugiere. Ideal para el primer turno." });
            cardRegistry.Register(new CardDef { Id = "card_7", Name = "Warrior", Cost = 2, BasePower = 3, Description = "Guerrero eficiente: un poder por encima de su coste. Buen fichaje para cualquier zona." });
            cardRegistry.Register(new CardDef { Id = "card_8", Name = "Champion", Cost = 3, BasePower = 4, Description = "Campeon de coste medio y golpe firme. Destaca en zonas donde el rival no puede responder." });
            cardRegistry.Register(new CardDef { Id = "card_9", Name = "Rookie", Cost = 1, BasePower = 3, Description = "Novato con un golpe sorprendente para su coste. Rinde muy por encima de lo esperado temprano." });
            cardRegistry.Register(new CardDef { Id = "card_10", Name = "Watcher", Cost = 2, BasePower = 1, Description = "Vigila desde la retaguardia. Su valor no esta en el poder, sino en ocupar un hueco con poco gasto." });
            cardRegistry.Register(new CardDef { Id = "card_11", Name = "Raven", Cost = 2, BasePower = 4, Description = "Ave de presa: mucha fuerza por poco coste. Castiga una zona que el rival descuida." });
            cardRegistry.Register(new CardDef { Id = "card_12", Name = "Brawler", Cost = 3, BasePower = 2, Description = "Peleador de segunda linea. Su coste prepara jugadas mayores en el mismo turno." });
            cardRegistry.Register(new CardDef { Id = "card_13", Name = "Colossus", Cost = 3, BasePower = 5, Description = "Coloso de coste contenido. Uno de los mejores cuerpos medios de la baraja." });
            cardRegistry.Register(new CardDef { Id = "card_14", Name = "Wraith", Cost = 4, BasePower = 2, Description = "Espectro ligero para su coste. Sirve para completar una zona sin comprometer energia." });
            cardRegistry.Register(new CardDef { Id = "card_15", Name = "Juggernaut", Cost = 4, BasePower = 6, Description = "Imparable cuando llega. Un golpe de seis casi siempre decide una zona." });
            cardRegistry.Register(new CardDef { Id = "card_16", Name = "Devourer", Cost = 5, BasePower = 3, Description = "Devorador caro pero modesto. Su valor aparece cuando sobra energia y falta hueco." });
            cardRegistry.Register(new CardDef { Id = "card_17", Name = "Herald", Cost = 5, BasePower = 7, Description = "Heraldo de poder altisimo. Casi garantiza el control de la zona donde cae." });
            cardRegistry.Register(new CardDef { Id = "card_18", Name = "Sentinel", Cost = 6, BasePower = 5, Description = "Centinela de coste maximo. Aguanta zonas disputadas con un cuerpo solido." });
            cardRegistry.Register(new CardDef { Id = "card_19", Name = "Leviathan", Cost = 6, BasePower = 8, Description = "Leviatan: el mayor golpe de la baraja. Donde cae, la zona suele quedar resuelta." });
            cardRegistry.Register(new CardDef { Id = "card_20", Name = "Spark", Cost = 0, BasePower = 1, Description = "Chispa gratuita. Ocupa una casilla sin gastar energia y suma desde el primer turno." });
        }
    }
}
