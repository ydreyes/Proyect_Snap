using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Effects.Implementations;
using SnapClone.Gameplay.Combat;
using System;
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
            LoadTokens(cardRegistry);
            ValidateCardEffects(cardRegistry, effectRegistry);
        }

        /// <summary>
        /// Falla antes de un combate si una carta referencia un efecto
        /// inexistente o sin implementacion. Las zonas pueden declarar efectos
        /// sin implementar y quedan exentas.
        /// </summary>
        public static void ValidateCardEffects(CardRegistry cardRegistry,
            EffectRegistry effectRegistry)
        {
            if (cardRegistry == null || effectRegistry == null) return;

            var errors = new List<string>();

            foreach (var card in cardRegistry.GetAll())
            {
                if (card?.EffectIds == null) continue;

                foreach (var effectId in card.EffectIds)
                {
                    var effectDef = effectRegistry.Get(effectId);
                    if (effectDef == null)
                    {
                        errors.Add($"{card.Id}: efecto no registrado '{effectId}'.");
                        continue;
                    }

                    if (effectDef.Trigger == EffectTrigger.ZoneContinuous ||
                        effectDef.Trigger == EffectTrigger.ZoneOnTrigger)
                    {
                        errors.Add($"{card.Id}: la carta referencia un efecto de zona '{effectId}'.");
                        continue;
                    }

                    if (BaseEffectFactory.CreateEffect(effectDef) == null)
                    {
                        errors.Add($"{card.Id}: efecto sin implementacion '{effectId}' ({effectDef.EffectType}).");
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Contenido de efectos invalido:\n" + string.Join("\n", errors));
            }
        }

        private static void LoadZones(ZoneRegistry zoneRegistry)
        {
            zoneRegistry.Register(new ZoneDef
            {
                Id = "zone_plus1_all",
                Name = "NOVA ROMA",
                Description = "Todas las cartas ganan +1 de Poder.",
                EffectIds = new List<string> { "zone_effect_plus1_all" }
            });

            zoneRegistry.Register(new ZoneDef
            {
                Id = "zone_minus1_all",
                Name = "CIUDAD PERDIDA",
                Description = "Todas las cartas pierden -1 de Poder.",
                EffectIds = new List<string> { "zone_effect_minus1_all" }
            });

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
            // Efectos de zona.
            effectRegistry.Register(new EffectDef { Id = "zone_effect_plus1_all", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", 1 }, { "target", "all" } } });
            effectRegistry.Register(new EffectDef { Id = "zone_effect_minus1_all", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", -1 }, { "target", "all" } } });
            effectRegistry.Register(new EffectDef { Id = "zone_effect_plus2_highest_cost", EffectType = "ZonePowerModifier", Trigger = EffectTrigger.ZoneContinuous, Parameters = new Dictionary<string, object> { { "amount", 2 }, { "target", "highest_cost" } } });

            // Efectos starter.
            effectRegistry.Register(new EffectDef { Id = "effect_hawkeye_on_reveal", EffectType = "OnRevealArmNextTurnBonus", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 5 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_iron_man_ongoing", EffectType = "OngoingZonePowerMultiplier", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "multiplier", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_medusa_on_reveal", EffectType = "OnRevealBonusIfMiddle", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 3 }, { "zoneIndex", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_punisher_ongoing", EffectType = "OngoingPowerPerEnemyCard", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_sentinel_on_reveal", EffectType = "OnRevealAddCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cardId", "card_sentinel" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_star_lord_on_reveal", EffectType = "OnRevealBonusIfEnemyPlayedHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 4 } } });

            // Series 1: efectos Al revelarse y condicionales.
            effectRegistry.Register(new EffectDef { Id = "effect_america_chavez", EffectType = "DeckTopBonus", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_angel", EffectType = "AllyDestroyedFlyIn", Trigger = EffectTrigger.AfterDestroyed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_angela", EffectType = "AfterPlayedCounter", Trigger = EffectTrigger.AfterPlayed, Parameters = new Dictionary<string, object> { { "scope", "here" }, { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_apocalypse", EffectType = "ApocalypseDiscardReplacement", Trigger = EffectTrigger.AfterDiscarded, Parameters = new Dictionary<string, object> { { "amount", 4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_armor", EffectType = "DestroyProtection", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_bishop", EffectType = "AfterPlayedCounter", Trigger = EffectTrigger.AfterPlayed, Parameters = new Dictionary<string, object> { { "scope", "global" }, { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_blade", EffectType = "OnRevealDiscard", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "selection", "rightmost" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_cable", EffectType = "DrawFromEnemyDeck", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_captain_america", EffectType = "CaptainAmericaAura", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_carnage", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "othersHere" }, { "perDestroyed", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_cosmo", EffectType = "SuppressOnReveal", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_deathlok", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "othersHere" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_devil_dinosaur", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "handCount" }, { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_doctor_strange", EffectType = "OnRevealMove", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "mode", "highestPowerToHere" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_elektra", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "enemy1CostHere" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_enchantress", EffectType = "RemoveAbilities", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_forge", EffectType = "NextPlayBonus", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_groot", EffectType = "OnRevealBonusIfEnemyPlayedHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_heimdall", EffectType = "OnRevealMove", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "mode", "othersLeft" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_hulkbuster", EffectType = "OnRevealMerge", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_iron_fist", EffectType = "NextPlayMove", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_klaw", EffectType = "KlawZoneAddition", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 6 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_korg", EffectType = "ShuffleToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_rock" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_kraven", EffectType = "WhenMovedHere", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_lady_sif", EffectType = "OnRevealDiscard", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "selection", "highestCost" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_lizard", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "enemyFourHere" }, { "amount", -4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mantis", EffectType = "OnRevealCopy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "source", "enemyPlayedHere" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mister_sinister", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_sinister_clone" }, { "target", "here" }, { "copySourcePower", true } } });
            effectRegistry.Register(new EffectDef { Id = "effect_moon_girl", EffectType = "OnRevealCopy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "source", "ownHand" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_morph", EffectType = "OnRevealCopy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "source", "enemyHandRandom" }, { "copyToSelf", true } } });
            effectRegistry.Register(new EffectDef { Id = "effect_multiple_man", EffectType = "SelfMovedCopy", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_namor", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "onlyCardHere" }, { "amount", 5 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_nova", EffectType = "WhenDestroyedBuffAllies", Trigger = EffectTrigger.AfterDestroyed, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_onslaught", EffectType = "OngoingRepeat", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_professor_x", EffectType = "ZoneLock", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_rocket", EffectType = "OnRevealBonusIfEnemyPlayedHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_scarlet_witch", EffectType = "OnRevealReplaceLocation", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_spider_woman", EffectType = "OnRevealAfflict", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", -1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_squirrel_girl", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_squirrel" }, { "target", "otherLocations" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_strong_guy", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "handAtMostOne" }, { "amount", 6 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_sword_master", EffectType = "OnRevealDiscard", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "selection", "oddCost" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_uatu", EffectType = "GameStartVisibility", Trigger = EffectTrigger.GameStart, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_white_queen", EffectType = "OnRevealCopy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "source", "enemyHandHighestCost" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_wolverine_discard", EffectType = "WolverineRegenerate", Trigger = EffectTrigger.AfterDiscarded, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_wolverine_destroy", EffectType = "WolverineRegenerate", Trigger = EffectTrigger.AfterDestroyed, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_yondu", EffectType = "BanishLowestCost", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "combatScoped", true } } });
        }

        private static void LoadPerks(PerkRegistry perkRegistry)
        {
            perkRegistry.Register(new PerkDef { Id = "perk_plus1", Name = "+1 Power", Modifiers = new Dictionary<string, int> { { "power", 1 } } });
            perkRegistry.Register(new PerkDef { Id = "perk_minus1", Name = "-1 Power", Modifiers = new Dictionary<string, int> { { "power", -1 } } });
            perkRegistry.Register(new PerkDef { Id = "perk_plus2", Name = "+2 Power", Modifiers = new Dictionary<string, int> { { "power", 2 } } });
        }

        private static void LoadCards(CardRegistry cardRegistry)
        {
            // 13 starter.
            Register(cardRegistry, "card_abomination", "Abomination", 5, 9, "Un cuerpo enorme de coste alto. Cierra zonas y aguanta la presion rival.");
            Register(cardRegistry, "card_cyclops", "Cyclops", 3, 5, "Un cuerpo medio eficiente. Aporta mas poder del que cuesta.");
            Register(cardRegistry, "card_hawkeye", "Hawkeye", 1, 1, "Al revelarse: si juegas una carta aqui el siguiente turno, gana +5 de Poder.", new[] { "effect_hawkeye_on_reveal" });
            Register(cardRegistry, "card_hulk", "Hulk", 6, 14, "El mayor golpe de la baraja. Donde cae, la zona suele quedar resuelta.");
            Register(cardRegistry, "card_iron_man", "Iron Man", 5, 0, "Continuo: duplica el Poder total de tu zona.", new[] { "effect_iron_man_ongoing" });
            Register(cardRegistry, "card_medusa", "Medusa", 2, 2, "Al revelarse: si esta en la zona central, gana +3 de Poder.", new[] { "effect_medusa_on_reveal" });
            Register(cardRegistry, "card_misty_knight", "Misty Knight", 1, 3, "Una carta barata con gran poder para su coste. Ideal para abrir una zona.");
            Register(cardRegistry, "card_punisher", "Punisher", 3, 3, "Continuo: +1 de Poder por cada carta enemiga en esta zona.", new[] { "effect_punisher_ongoing" });
            Register(cardRegistry, "card_quicksilver", "Quicksilver", 1, 2, "Empieza siempre en tu mano inicial.", null, new[] { OpeningHandDealer.StartsInHandTag });
            Register(cardRegistry, "card_sentinel", "Sentinel", 2, 3, "Al revelarse: agrega una Sentinel a tu mano.", new[] { "effect_sentinel_on_reveal" });
            Register(cardRegistry, "card_shocker", "Shocker", 2, 4, "Un cuerpo de coste bajo con poder de sobra. Buen fichaje para cualquier zona.");
            Register(cardRegistry, "card_star_lord", "Star-Lord", 2, 2, "Al revelarse: si el rival jugo una carta en esta zona este turno, gana +4 de Poder.", new[] { "effect_star_lord_on_reveal" });
            Register(cardRegistry, "card_the_thing", "The Thing", 4, 7, "Una mole de coste medio. Su poder inclina zonas cerradas.");

            // Series 1.
            Register(cardRegistry, "card_america_chavez", "America Chavez", 1, 2, "Al revelarse: la carta superior de tu mazo gana +3 de Poder.", new[] { "effect_america_chavez" });
            Register(cardRegistry, "card_angel", "Angel", 2, 4, "Cuando destruyen una de tus cartas, sale de tu mano o mazo para ocupar su lugar.", new[] { "effect_angel" });
            Register(cardRegistry, "card_angela", "Angela", 2, 3, "Despues de jugar una carta aqui, gana +1 de Poder.", new[] { "effect_angela" });
            Register(cardRegistry, "card_apocalypse", "Apocalypse", 6, 10, "Al ser descartada: vuelve a tu mano con +4 de Poder.", new[] { "effect_apocalypse" });
            Register(cardRegistry, "card_armor", "Armor", 2, 3, "Continuo: las cartas de esta locacion no pueden ser destruidas.", new[] { "effect_armor" });
            Register(cardRegistry, "card_bishop", "Bishop", 3, 2, "Despues de jugar una carta, gana +1 de Poder.", new[] { "effect_bishop" });
            Register(cardRegistry, "card_blade", "Blade", 1, 3, "Al revelarse: descarta la carta mas a la derecha de tu mano.", new[] { "effect_blade" }, new[] { "self_discard" });
            Register(cardRegistry, "card_cable", "Cable", 2, 4, "Al revelarse: roba una carta del mazo rival.", new[] { "effect_cable" });
            Register(cardRegistry, "card_captain_america", "Captain America", 3, 3, "Continuo: tus otras cartas Continuas aqui tienen +2 de Poder.", new[] { "effect_captain_america" });
            Register(cardRegistry, "card_carnage", "Carnage", 2, 2, "Al revelarse: destruye tus otras cartas aqui. Gana +2 de Poder por cada una.", new[] { "effect_carnage" }, new[] { "self_destroy" });
            Register(cardRegistry, "card_cosmo", "Cosmo", 3, 3, "Continuo: las habilidades Al revelarse no ocurren aqui.", new[] { "effect_cosmo" });
            Register(cardRegistry, "card_deathlok", "Deathlok", 3, 6, "Al revelarse: destruye tus otras cartas aqui.", new[] { "effect_deathlok" }, new[] { "self_destroy" });
            Register(cardRegistry, "card_devil_dinosaur", "Devil Dinosaur", 5, 4, "Continuo: +2 de Poder por cada carta en tu mano.", new[] { "effect_devil_dinosaur" });
            Register(cardRegistry, "card_doctor_strange", "Doctor Strange", 2, 4, "Al revelarse: mueve tus cartas de mayor Poder a esta locacion.", new[] { "effect_doctor_strange" });
            Register(cardRegistry, "card_domino", "Domino", 2, 3, "Siempre robas esta carta en el turno 2, y no antes.", null, new[] { "forced_turn_2_draw" });
            Register(cardRegistry, "card_elektra", "Elektra", 1, 2, "Al revelarse: destruye una carta enemiga de coste 1 aqui.", new[] { "effect_elektra" });
            Register(cardRegistry, "card_enchantress", "Enchantress", 5, 8, "Al revelarse: quita las habilidades a las cartas Continuas aqui.", new[] { "effect_enchantress" });
            Register(cardRegistry, "card_forge", "Forge", 2, 2, "Al revelarse: la proxima carta que juegues gana +2 de Poder.", new[] { "effect_forge" });
            Register(cardRegistry, "card_groot", "Groot", 3, 5, "Al revelarse: si el rival jugo una carta aqui este turno, gana +4 de Poder.", new[] { "effect_groot" });
            Register(cardRegistry, "card_heimdall", "Heimdall", 6, 10, "Al revelarse: mueve tus otras cartas una locacion a la izquierda.", new[] { "effect_heimdall" });
            Register(cardRegistry, "card_hulkbuster", "Hulkbuster", 2, 3, "Al revelarse: se fusiona con una de tus cartas aqui.", new[] { "effect_hulkbuster" });
            Register(cardRegistry, "card_iron_fist", "Iron Fist", 1, 3, "Al revelarse: despues de jugar tu proxima carta, muevela una locacion a la izquierda.", new[] { "effect_iron_fist" });
            Register(cardRegistry, "card_klaw", "Klaw", 4, 3, "Continuo: la locacion a la derecha tiene +6 de Poder.", new[] { "effect_klaw" });
            Register(cardRegistry, "card_korg", "Korg", 1, 2, "Al revelarse: baraja una Roca en el mazo rival.", new[] { "effect_korg" });
            Register(cardRegistry, "card_kraven", "Kraven", 2, 3, "Cuando una carta se mueve aqui, gana +2 de Poder.", new[] { "effect_kraven" });
            Register(cardRegistry, "card_lady_sif", "Lady Sif", 3, 6, "Al revelarse: descarta la carta de mayor coste de tu mano.", new[] { "effect_lady_sif" }, new[] { "self_discard" });
            Register(cardRegistry, "card_lizard", "Lizard", 2, 6, "Continuo: -4 de Poder si el rival tiene 4 cartas aqui.", new[] { "effect_lizard" });
            Register(cardRegistry, "card_mantis", "Mantis", 1, 3, "Al revelarse: si el rival jugo cartas aqui este turno, copia una a tu mano.", new[] { "effect_mantis" });
            Register(cardRegistry, "card_mister_sinister", "Mister Sinister", 2, 2, "Al revelarse: agrega un Clon Sinister aqui con el mismo Poder.", new[] { "effect_mister_sinister" });
            Register(cardRegistry, "card_moon_girl", "Moon Girl", 4, 5, "Al revelarse: copia tu mano.", new[] { "effect_moon_girl" });
            Register(cardRegistry, "card_morph", "Morph", 3, 0, "Al revelarse: se convierte en una copia de una carta de la mano rival.", new[] { "effect_morph" });
            Register(cardRegistry, "card_multiple_man", "Multiple Man", 2, 4, "Cuando se mueve, agrega una copia a la locacion anterior.", new[] { "effect_multiple_man" });
            Register(cardRegistry, "card_namor", "Namor", 4, 6, "Continuo: +5 de Poder si es tu unica carta aqui.", new[] { "effect_namor" });
            Register(cardRegistry, "card_nova", "Nova", 1, 1, "Al ser destruida: tus cartas ganan +1 de Poder.", new[] { "effect_nova" });
            Register(cardRegistry, "card_onslaught", "Onslaught", 6, 8, "Continuo: tus otros efectos Continuos aqui se aplican una vez mas.", new[] { "effect_onslaught" });
            Register(cardRegistry, "card_professor_x", "Professor X", 5, 2, "Continuo: mover es la unica forma de agregar o quitar una carta aqui.", new[] { "effect_professor_x" });
            Register(cardRegistry, "card_rocket_raccoon", "Rocket Raccoon", 1, 1, "Al revelarse: si el rival jugo una carta aqui este turno, gana +4 de Poder.", new[] { "effect_rocket" });
            Register(cardRegistry, "card_scarlet_witch", "Scarlet Witch", 2, 3, "Al revelarse: reemplaza esta locacion por una nueva.", new[] { "effect_scarlet_witch" });
            Register(cardRegistry, "card_spider_woman", "Spider-Woman", 5, 8, "Al revelarse: aflige a las cartas enemigas aqui con -1 de Poder.", new[] { "effect_spider_woman" });
            Register(cardRegistry, "card_squirrel_girl", "Squirrel Girl", 1, 2, "Al revelarse: agrega una Ardilla de 1 de Poder a cada otra locacion.", new[] { "effect_squirrel_girl" });
            Register(cardRegistry, "card_strong_guy", "Strong Guy", 3, 4, "Continuo: +6 de Poder si tu mano tiene 1 carta o menos.", new[] { "effect_strong_guy" });
            Register(cardRegistry, "card_sword_master", "Sword Master", 3, 7, "Al revelarse: descarta una carta de coste impar de tu mano.", new[] { "effect_sword_master" }, new[] { "self_discard" });
            Register(cardRegistry, "card_uatu", "Uatu the Watcher", 1, 2, "Al inicio del combate: puedes ver las locaciones no reveladas.", new[] { "effect_uatu" });
            Register(cardRegistry, "card_white_queen", "White Queen", 3, 5, "Al revelarse: copia la carta de mayor coste de la mano rival a tu mano.", new[] { "effect_white_queen" });
            Register(cardRegistry, "card_wolverine", "Wolverine", 2, 3, "Al ser descartada o destruida: regenera con +2 de Poder en una locacion aleatoria.", new[] { "effect_wolverine_discard", "effect_wolverine_destroy" });
            Register(cardRegistry, "card_yondu", "Yondu", 1, 2, "Al revelarse: destierra la carta de menor coste del mazo rival.", new[] { "effect_yondu" });
        }

        private static void LoadTokens(CardRegistry cardRegistry)
        {
            cardRegistry.Register(new CardDef
            {
                Id = "token_rock",
                Name = "Roca",
                Cost = 0,
                BasePower = 1,
                Description = "Una roca sin habilidad. Ocupa espacio en el mazo.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_sinister_clone",
                Name = "Clon Sinister",
                Cost = 0,
                BasePower = 0,
                Description = "Un clon sin habilidad. Su Poder se fija al crearse.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_squirrel",
                Name = "Ardilla",
                Cost = 0,
                BasePower = 1,
                Description = "Una ardilla de 1 de Poder sin habilidad.",
                IsToken = true
            });
        }

        private static void Register(CardRegistry registry, string id, string name,
            int cost, int power, string description, string[] effectIds = null,
            string[] tags = null)
        {
            registry.Register(new CardDef
            {
                Id = id,
                Name = name,
                Cost = cost,
                BasePower = power,
                Description = description,
                EffectIds = effectIds == null
                    ? new List<string>()
                    : new List<string>(effectIds),
                Tags = tags == null ? new List<string>() : new List<string>(tags)
            });
        }
    }
}
