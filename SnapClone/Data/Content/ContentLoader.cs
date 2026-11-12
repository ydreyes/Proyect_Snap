using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Effects.Implementations;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Meta;
using System;
using System.Collections.Generic;

namespace SnapClone.Data.Content
{
    public class ContentLoader
    {
        public static void LoadAll(CardRegistry cardRegistry, ZoneRegistry zoneRegistry,
            EffectRegistry effectRegistry, PerkRegistry perkRegistry,
            EnemyRegistry enemyRegistry = null, ModifierRegistry modifierRegistry = null,
            RunEventRegistry eventRegistry = null, ItemRegistry itemRegistry = null)
        {
            LoadZones(zoneRegistry);
            LoadEffects(effectRegistry);
            LoadPerkEffects(effectRegistry);
            LoadPerks(perkRegistry);
            ValidatePerkCatalog(perkRegistry, effectRegistry);
            LoadCards(cardRegistry);
            LoadSeries3Cards(cardRegistry);
            ApplyStarterSeries(cardRegistry);
            ApplyDraftThemes(cardRegistry);
            LoadTokens(cardRegistry);
            ValidateDraftThemes(cardRegistry);
            ValidateCatalog(cardRegistry);
            ValidateStorePartition(cardRegistry);
            ValidateCardEffects(cardRegistry, effectRegistry);

            if (enemyRegistry != null)
            {
                LoadEnemies(enemyRegistry);
                enemyRegistry.Validate(cardRegistry);
            }

            if (modifierRegistry != null)
                LoadModifiers(modifierRegistry);

            if (eventRegistry != null)
            {
                LoadEvents(eventRegistry);
                eventRegistry.Validate(modifierRegistry);
            }

            if (itemRegistry != null)
            {
                LoadItems(itemRegistry);
                itemRegistry.Validate();
            }
        }

        /// <summary>
        /// Los 8 objetos de combate son contenido independiente del catalogo de
        /// cartas: nunca se registran como cartas, tokens ni opciones de draft.
        /// </summary>
        public static void LoadItems(ItemRegistry itemRegistry)
        {
            if (itemRegistry == null) return;

            Item(itemRegistry, "item_energy_surge", "Impulso de energia",
                "Anade 2 de energia durante el turno actual.",
                ItemRarity.Common, ItemEffectIds.EnergySurge, ItemTargetRule.None);

            Item(itemRegistry, "item_power_boost", "Refuerzo de poder",
                "+2 de Poder a una carta aliada aleatoria en juego.",
                ItemRarity.Common, ItemEffectIds.PowerBoost, ItemTargetRule.AllyCard);

            Item(itemRegistry, "item_destroy_enemy", "Ejecucion",
                "Destruye una carta enemiga aleatoria sin proteccion.",
                ItemRarity.Rare, ItemEffectIds.DestroyEnemy, ItemTargetRule.EnemyCard);

            Item(itemRegistry, "item_draw_two", "Robo doble",
                "Roba hasta 2 cartas si la mano tiene espacio.",
                ItemRarity.Common, ItemEffectIds.DrawTwo, ItemTargetRule.Draw);

            Item(itemRegistry, "item_move_enemy", "Reubicacion",
                "Mueve una carta enemiga aleatoria a otra zona legal.",
                ItemRarity.Rare, ItemEffectIds.MoveEnemy, ItemTargetRule.EnemyMove);

            Item(itemRegistry, "item_afflict_zone", "Maldicion de zona",
                "-2 de Poder a las cartas enemigas de una zona aleatoria.",
                ItemRarity.Rare, ItemEffectIds.AfflictZone, ItemTargetRule.EnemyZone);

            Item(itemRegistry, "item_copy_hand", "Copia de mano",
                "Copia una carta aleatoria de tu mano.",
                ItemRarity.Rare, ItemEffectIds.CopyHand, ItemTargetRule.HandCard);

            Item(itemRegistry, "item_heal_zone", "Bendicion de zona",
                "+3 de Poder a tus cartas de una zona aliada aleatoria.",
                ItemRarity.Rare, ItemEffectIds.HealZone, ItemTargetRule.AllyZone);
        }

        private static void Item(ItemRegistry registry, string id, string name,
            string description, ItemRarity rarity, string effectId,
            ItemTargetRule targetRule)
        {
            registry.Register(new ItemDef
            {
                Id = id,
                Name = name,
                Description = description,
                Rarity = rarity,
                EffectId = effectId,
                TargetRule = targetRule
            });
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

        /// <summary>
        /// Roster fijo de 23 encuentros: 18 normales (6 por acto), 4 subjefes
        /// y el jefe final. Los mazos son ids de carta, nunca instancias.
        /// </summary>
        private static void LoadEnemies(EnemyRegistry enemyRegistry)
        {
            // Acto 1.
            Enemy(enemyRegistry, "enemy_hand_ninjas", "Hand Ninjas",
                "La Mano ataca desde las sombras.", 1, 1, EnemyBossTier.None,
                "card_blade", "card_elektra", "card_misty_knight", "card_shocker",
                "card_hawkeye", "card_quicksilver", "card_cyclops", "card_star_lord");

            Enemy(enemyRegistry, "enemy_hydra_agents", "Hydra Agents",
                "Hydra no perdona una cabeza cortada.", 1, 1, EnemyBossTier.None,
                "card_blade", "card_shocker", "card_cyclops", "card_misty_knight",
                "card_punisher", "card_hawkeye", "card_sentinel", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_aim_scientists", "AIM Scientists",
                "Sus experimentos no respetan limites.", 2, 1, EnemyBossTier.None,
                "card_forge", "card_america_chavez", "card_medusa", "card_sentinel",
                "card_hawkeye", "card_star_lord", "card_iron_fist", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_ultron_drones", "Ultron Drones",
                "La colmena calcula cada movimiento.", 2, 1, EnemyBossTier.None,
                "card_mister_sinister", "card_squirrel_girl", "card_sentinel",
                "card_shocker", "card_misty_knight", "card_cyclops",
                "card_quicksilver", "card_blade");

            Enemy(enemyRegistry, "enemy_kree_sentries", "Kree Sentries",
                "La patrulla Kree no retrocede.", 2, 1, EnemyBossTier.None,
                "card_the_thing", "card_cyclops", "card_punisher", "card_shocker",
                "card_sentinel", "card_medusa", "card_star_lord", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_skrull_infiltrators", "Skrull Infiltrators",
                "Cualquiera puede ser un Skrull.", 3, 1, EnemyBossTier.None,
                "card_white_queen", "card_mantis", "card_morph", "card_cable",
                "card_sentinel", "card_medusa", "card_hawkeye", "card_quicksilver");

            // Acto 2.
            Enemy(enemyRegistry, "enemy_brotherhood", "Brotherhood",
                "La hermandad quiere tu lugar.", 2, 2, EnemyBossTier.None,
                "card_kraven", "card_multiple_man", "card_doctor_strange",
                "card_iron_fist", "card_heimdall", "card_elektra", "card_shocker",
                "card_quicksilver");

            Enemy(enemyRegistry, "enemy_sentinels", "Sentinels",
                "Los centinelas no sienten piedad.", 2, 2, EnemyBossTier.None,
                "card_sentinel", "card_iron_man", "card_punisher",
                "card_captain_america", "card_klaw", "card_cyclops",
                "card_shocker", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_dark_elves", "Dark Elves",
                "La oscuridad de Svartalfheim avanza.", 2, 2, EnemyBossTier.None,
                "card_spider_woman", "card_carnage", "card_deathlok", "card_elektra",
                "card_blade", "card_shocker", "card_misty_knight", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_frost_giants", "Frost Giants",
                "El hielo reclama cada zona.", 3, 2, EnemyBossTier.None,
                "card_hulk", "card_the_thing", "card_abomination", "card_heimdall",
                "card_apocalypse", "card_shocker", "card_cyclops", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_chitauri", "Chitauri",
                "La invasion no se detiene.", 2, 2, EnemyBossTier.None,
                "card_squirrel_girl", "card_mister_sinister", "card_moon_girl",
                "card_sentinel", "card_shocker", "card_misty_knight", "card_blade",
                "card_quicksilver");

            Enemy(enemyRegistry, "enemy_black_order_scouts", "Black Order Scouts",
                "La Orden Negra observa y espera.", 3, 2, EnemyBossTier.None,
                "card_punisher", "card_captain_america", "card_angela", "card_bishop",
                "card_lizard", "card_star_lord", "card_shocker", "card_quicksilver");

            // Acto 3.
            Enemy(enemyRegistry, "enemy_kree_accusers", "Kree Accusers",
                "El veredicto Kree ya esta escrito.", 3, 3, EnemyBossTier.None,
                "card_iron_man", "card_klaw", "card_onslaught", "card_captain_america",
                "card_punisher", "card_cyclops", "card_shocker", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_shiar_guard", "Shi'ar Guard",
                "La guardia imperial no cede.", 3, 3, EnemyBossTier.None,
                "card_devil_dinosaur", "card_strong_guy", "card_namor", "card_lizard",
                "card_captain_america", "card_iron_man", "card_shocker",
                "card_quicksilver");

            Enemy(enemyRegistry, "enemy_brood", "Brood",
                "El enjambre devora el tablero.", 3, 3, EnemyBossTier.None,
                "card_squirrel_girl", "card_mister_sinister", "card_moon_girl",
                "card_carnage", "card_deathlok", "card_nova", "card_wolverine",
                "card_quicksilver");

            Enemy(enemyRegistry, "enemy_symbiotes", "Symbiotes",
                "La simbiosis quiere un anfitrion.", 3, 3, EnemyBossTier.None,
                "card_carnage", "card_deathlok", "card_spider_woman", "card_wolverine",
                "card_nova", "card_angel", "card_elektra", "card_blade");

            Enemy(enemyRegistry, "enemy_doombots", "Doombots",
                "Doom nunca envia su mejor copia.", 3, 3, EnemyBossTier.None,
                "card_iron_man", "card_onslaught", "card_klaw", "card_professor_x",
                "card_cosmo", "card_sentinel", "card_shocker", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_heralds_of_galactus", "Heralds of Galactus",
                "Los heraldos anuncian el final.", 3, 3, EnemyBossTier.None,
                "card_hulk", "card_heimdall", "card_apocalypse",
                "card_devil_dinosaur", "card_iron_man", "card_onslaught",
                "card_shocker", "card_quicksilver");

            // Subjefes fijos.
            Enemy(enemyRegistry, "enemy_red_skull", "Red Skull",
                "El craneo rojo cierra el primer acto.", 3, 1, EnemyBossTier.Act,
                "card_hulk", "card_the_thing", "card_abomination", "card_punisher",
                "card_captain_america", "card_cyclops", "card_shocker",
                "card_quicksilver");

            Enemy(enemyRegistry, "enemy_magneto", "Magneto",
                "El amo del magnetismo no retrocede.", 4, 2, EnemyBossTier.Act,
                "card_iron_man", "card_klaw", "card_onslaught",
                "card_doctor_strange", "card_heimdall", "card_multiple_man",
                "card_shocker", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_doctor_doom", "Doctor Doom",
                "La armadura de Doom sella el acto.", 4, 3, EnemyBossTier.Act,
                "card_iron_man", "card_onslaught", "card_klaw",
                "card_professor_x", "card_cosmo", "card_devil_dinosaur",
                "card_shocker", "card_quicksilver");

            Enemy(enemyRegistry, "enemy_thanos", "Thanos",
                "El titan impone su voluntad.", 5, 4, EnemyBossTier.Act,
                "card_hulk", "card_apocalypse", "card_heimdall", "card_onslaught",
                "card_iron_man", "card_devil_dinosaur", "card_shocker",
                "card_quicksilver");

            // Jefe final unico.
            Enemy(enemyRegistry, "enemy_galactus", "Galactus",
                "El devorador de mundos cierra la run.", 5, 4, EnemyBossTier.Final,
                "card_hulk", "card_apocalypse", "card_heimdall", "card_onslaught",
                "card_iron_man", "card_devil_dinosaur", "card_the_thing",
                "card_abomination");
        }

        private static void Enemy(EnemyRegistry registry, string id, string name,
            string phrase, int difficulty, int actIndex, EnemyBossTier tier,
            params string[] cardIds)
        {
            var deck = new List<EnemyDeckCard>();
            if (cardIds != null)
            {
                foreach (string cardId in cardIds)
                {
                    deck.Add(new EnemyDeckCard
                    {
                        CardId = cardId,
                        Count = 1
                    });
                }
            }

            registry.Register(new EnemyDef
            {
                Id = id,
                Name = name,
                Phrase = phrase,
                Difficulty = difficulty,
                ActIndex = actIndex,
                IsBoss = tier != EnemyBossTier.None,
                BossTier = tier,
                Deck = deck
            });
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

            zoneRegistry.Register(new ZoneDef
            {
                Id = "zone_limbo",
                Name = "LIMBO",
                Description = "La partida dura 7 turnos.",
                EffectIds = new List<string>()
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

            // Collection Level 1-14 y Series 2.
            effectRegistry.Register(new EffectDef { Id = "effect_jessica_jones", EffectType = "OnRevealArmNotPlayedNextTurn", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 5 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_ka_zar", EffectType = "OngoingCostAura", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "cost", 1 }, { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mister_fantastic", EffectType = "KlawZoneAddition", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 3 }, { "direction", "both" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_nightcrawler", EffectType = "PassiveMovePermission", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object> { { "frequency", "oncePerCombat" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_odin", EffectType = "OdinRepeatZone", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_spectrum", EffectType = "OnRevealBuffOngoing", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_white_tiger", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_tiger" }, { "target", "randomOtherLocation" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_wolfsbane", EffectType = "OnRevealBonusPerOtherCardHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 2 } } });

            effectRegistry.Register(new EffectDef { Id = "effect_agent_13", EffectType = "OnRevealAddRandomCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_bucky_barnes", EffectType = "AfterDestroyedReplaceToken", Trigger = EffectTrigger.AfterDestroyed, Parameters = new Dictionary<string, object> { { "tokenId", "token_winter_soldier" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_cloak", EffectType = "OnRevealOpenMoveWindow", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "turnOffset", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_ebony_maw_play", EffectType = "PassivePlayRestriction", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object> { { "maxTurn", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_ebony_maw_lock", EffectType = "PlayOnlyLock", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_hobgoblin", EffectType = "OnRevealSwitchSides", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_iceman", EffectType = "OnRevealRaiseEnemyCost", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 1 }, { "cap", 6 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_jubilee", EffectType = "OnRevealPullTopDeck", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_killmonger", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "all1Cost" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_leech", EffectType = "OnRevealDisableHighCostHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "threshold", 6 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_morbius", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "discardCount" }, { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_nakia", EffectType = "OnRevealBuffContainer", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "hand" }, { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_okoye", EffectType = "OnRevealBuffContainer", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "deck" }, { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_rhino", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_rock" }, { "target", "here" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_sabretooth", EffectType = "AfterDestroyedReturnZeroCost", Trigger = EffectTrigger.AfterDestroyed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_sandman", EffectType = "OnRevealNextTurnCostTax", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_scorpion", EffectType = "OnRevealAfflictHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", -1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_shang_chi", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "enemy10PlusHere" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_storm", EffectType = "OnRevealFloodZone", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_sunspot", EffectType = "EndOfTurnUnspentEnergy", Trigger = EffectTrigger.EndOfTurn, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_swarm", EffectType = "AfterDiscardedAddZeroCostCopies", Trigger = EffectTrigger.AfterDiscarded, Parameters = new Dictionary<string, object> { { "copies", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_the_collector", EffectType = "AfterAddedToHandSelfBuff", Trigger = EffectTrigger.AfterAddedToHand, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_the_infinaut", EffectType = "PassivePlayRestriction", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object> { { "requiresNoPlayLastTurn", true } } });
            effectRegistry.Register(new EffectDef { Id = "effect_vision", EffectType = "PassiveMovePermission", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object> { { "frequency", "oncePerTurn" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_vulture", EffectType = "AfterMovedSelfBuff", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object> { { "amount", 4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_warpath", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "anyAllyZoneEmpty" }, { "amount", 5 } } });

            effectRegistry.Register(new EffectDef { Id = "effect_drax", EffectType = "OnRevealBonusIfEnemyPlayedHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_green_goblin", EffectType = "OnRevealSwitchSides", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_debrii", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_rock" }, { "target", "otherLocations" }, { "bothSides", true } } });
            effectRegistry.Register(new EffectDef { Id = "effect_doctor_doom", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_doombot" }, { "target", "otherLocations" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_rescue", EffectType = "OnRevealArmNextTurnBonus", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 5 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_rockslide", EffectType = "ShuffleToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_rock" }, { "target", "opponent" }, { "copies", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_silver_surfer", EffectType = "OnRevealBuffCost", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cost", 3 }, { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_the_hood", EffectType = "OnRevealAddCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cardId", "token_demon" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_thor", EffectType = "ShuffleToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_mjolnir" }, { "target", "owner" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_wong", EffectType = "OngoingRepeatOnReveal", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_yellowjacket", EffectType = "OnRevealAfflict", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", -1 }, { "target", "ownOthers" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_orka", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "onlyCardHere" }, { "amount", 7 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_luke_cage", EffectType = "LukeCageAura", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_omega_red", EffectType = "OngoingWinningZoneAddition", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_red_skull", EffectType = "EnemyCardsHereAura", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_typhoid_mary", EffectType = "AllyOtherCardsHereAura", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", -2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mojo", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "bothSidesFull" }, { "amount", 6 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_cerebro", EffectType = "HighestPowerCardsAura", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_patriot", EffectType = "NoAbilityCardsAura", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mjolnir", EffectType = "AddPowerToNamedCard", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cardName", "Thor" }, { "amount", 7 } } });

            effectRegistry.Register(new EffectDef { Id = "effect_ronan", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "enemyHandCount" }, { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_dazzler", EffectType = "OngoingConditionalPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object> { { "condition", "fullAllyLocations" }, { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_polaris", EffectType = "OnRevealMove", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "mode", "enemyLowCostToHere" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_stegron", EffectType = "OnRevealMove", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "mode", "enemyHereToRight" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_magneto", EffectType = "OnRevealMove", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "mode", "enemyCost34ToHere" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_kingpin", EffectType = "WhenEnemyMovedHereAfflict", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object> { { "amount", -2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_dagger", EffectType = "AfterMovedSelfBuffPerEnemyCard", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_human_torch", EffectType = "AfterMovedSelfDouble", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_spider_man_2099_move", EffectType = "PassiveMovePermission", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object> { { "frequency", "oncePerCombat" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_spider_man_2099_afflict", EffectType = "AfterMovedAfflictEnemyHere", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object> { { "amount", -3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_lady_deathstrike", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "allHereAtMost3Power" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_venom", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "othersHere" }, { "addDestroyedPower", true } } });
            effectRegistry.Register(new EffectDef { Id = "effect_modok", EffectType = "OnRevealDiscard", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "selection", "all" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_selene", EffectType = "OnRevealAfflictLowestBothHands", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", -3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_hazmat", EffectType = "OnRevealAfflictAllOtherCards", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", -1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_gambit", EffectType = "OnRevealDiscardThenDestroyRandomEnemy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_falcon", EffectType = "OnRevealReturnCostCards", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cost", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_ghost", EffectType = "RemoveAbilitiesByTrigger", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "abilityTrigger", "EndOfTurn" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_psylocke", EffectType = "OnRevealNextTurnEnergy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_kitty_pryde", EffectType = "StartOfTurnReturnToHandAndBuff", Trigger = EffectTrigger.StartOfTurn, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_aero", EffectType = "OnRevealMoveLastEnemyPlayed", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });

            effectRegistry.Register(new EffectDef { Id = "effect_agent_coulson_4", EffectType = "OnRevealAddRandomCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "minCost", 4 }, { "maxCost", 4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_agent_coulson_5", EffectType = "OnRevealAddRandomCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "minCost", 5 }, { "maxCost", 5 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_attuma", EffectType = "EndOfTurnDestroyWeakerHere", Trigger = EffectTrigger.EndOfTurn, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_brood", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_broodling" }, { "target", "here" }, { "copies", 2 }, { "copySourcePowerAsSet", true } } });
            effectRegistry.Register(new EffectDef { Id = "effect_colleen_wing", EffectType = "OnRevealDiscard", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "selection", "leastCost" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_destroyer", EffectType = "OnRevealDestroy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "target", "othersAtOtherLocations" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_ghost_spider", EffectType = "OnRevealMoveLastAllyPlayed", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_maria_hill", EffectType = "OnRevealAddRandomCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "minCost", 2 }, { "maxCost", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mirage", EffectType = "OnRevealCopy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "source", "enemyHandLowestCost" }, { "powerBonus", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_nick_fury", EffectType = "OnRevealAddRandomCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "minCost", 6 }, { "maxCost", 6 }, { "count", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_sauron", EffectType = "RemoveAbilitiesFromDeckByTrigger", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "abilityTrigger", "Ongoing" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_shanna", EffectType = "OnRevealAddRandomCostToEachLocation", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cost", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_ultron", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_drone" }, { "target", "otherLocations" }, { "copies", 4 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_viper", EffectType = "OnRevealSwitchLowestOtherHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_zero", EffectType = "NextPlaySilence", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });

            effectRegistry.Register(new EffectDef { Id = "effect_death", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_she_hulk", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_stature", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_miles_morales", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_sera", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_quinjet", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_zabu", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_mobius", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_knull", EffectType = "OngoingDestroyedPower", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_wave", EffectType = "OnRevealGlobalCostCap", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "maxCost", 4 }, { "turnOffset", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_baron_mordo", EffectType = "OnRevealSetEnemyDeckTopCost", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cost", 6 }, { "untilTurn", 6 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_beast", EffectType = "OnRevealReturnOthersHereDiscount", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_bast", EffectType = "OnRevealSetHandBasePower", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "power", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_valkyrie", EffectType = "OnRevealSetZoneBasePower", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "power", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_shadow_king", EffectType = "OnRevealResetZonePower", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_black_panther", EffectType = "OnRevealDoubleSelf", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_shuri", EffectType = "NextPlayDoublePower", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_taskmaster", EffectType = "OnRevealCopyLastPlayedPower", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_deadpool", EffectType = "AfterDestroyedReturnWithDoublePower", Trigger = EffectTrigger.AfterDestroyed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_mister_negative", EffectType = "OnRevealSwapDeckCostPower", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });

            effectRegistry.Register(new EffectDef { Id = "effect_adam_warlock", EffectType = "EndOfTurnDrawIfWinningOrBuff", Trigger = EffectTrigger.EndOfTurn, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_crystal", EffectType = "OnRevealBothDraw", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_maximus", EffectType = "OnRevealEnemyDraws", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "count", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_jane_foster", EffectType = "OnRevealDrawCostFromDeck", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cost", 0 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_ghost_rider", EffectType = "OnRevealResurrectFromDiscard", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_hela", EffectType = "OnRevealResurrectDistinctCosts", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_doctor_octopus", EffectType = "OnRevealPullEnemyHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_black_bolt", EffectType = "OnRevealDiscardFromEnemyHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "selection", "leastCost" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_moon_knight", EffectType = "OnRevealDiscardEvenBoth", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_silver_samurai", EffectType = "OnRevealDiscardLowestPowerBoth", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_master_mold", EffectType = "OnRevealAddCardToEnemyHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cardId", "card_sentinel" }, { "count", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_electro_energy", EffectType = "OnRevealChangeMaxEnergy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "amount", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_electro", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_havok", EffectType = "EndOfTurnLoseMaxEnergyGainPower", Trigger = EffectTrigger.EndOfTurn, Parameters = new Dictionary<string, object> { { "maxEnergyDelta", -1 }, { "amount", 3 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_black_cat", EffectType = "EndOfTurnDiscardSelfFromHand", Trigger = EffectTrigger.EndOfTurn, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_helicarrier", EffectType = "AfterDiscardedFillHand", Trigger = EffectTrigger.AfterDiscarded, Parameters = new Dictionary<string, object>() });

            effectRegistry.Register(new EffectDef { Id = "effect_arnim_zola", EffectType = "OnRevealDestroyAndCopyToOtherZones", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_leader", EffectType = "OnRevealCopyEnemyPlays", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_nimrod", EffectType = "AfterDestroyedCopyToOtherZones", Trigger = EffectTrigger.AfterDestroyed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_quake", EffectType = "OnRevealSwapOtherZones", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_echo", EffectType = "AfterEnemyOngoingPlayedHereSilence", Trigger = EffectTrigger.AfterPlayed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_negasonic", EffectType = "AfterEnemyPlayedHereDestroyOnce", Trigger = EffectTrigger.AfterPlayed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_silk", EffectType = "AfterAnyCardPlayedHereMoveSelf", Trigger = EffectTrigger.AfterPlayed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_titania", EffectType = "AfterAnyCardPlayedHereSwitchSides", Trigger = EffectTrigger.AfterPlayed, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_hercules", EffectType = "AfterAllyMovedHereMoveAgain", Trigger = EffectTrigger.AfterMoved, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_nebula", EffectType = "EndOfTurnNebula", Trigger = EffectTrigger.EndOfTurn, Parameters = new Dictionary<string, object> { { "amount", 2 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_juggernaut", EffectType = "OnRevealMoveEnemyPlayedHere", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_spider_man", EffectType = "OnRevealMoveSelfAndPullEnemy", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });

            effectRegistry.Register(new EffectDef { Id = "effect_crossbones", EffectType = "CrossbonesRestriction", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_giganto", EffectType = "GigantoRestriction", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_sentry_play", EffectType = "SentryRestriction", Trigger = EffectTrigger.Passive, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_sentry_void", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_void" }, { "target", "right" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_goose", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_jean_grey", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_magik", EffectType = "OnRevealReplaceWithLimbo", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });

            effectRegistry.Register(new EffectDef { Id = "effect_absorbing_man", EffectType = "OnRevealCopyLastPlayedEffects", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "filter", "OnReveal" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mystique", EffectType = "OnRevealCopyLastPlayedEffects", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "filter", "Ongoing" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_rogue", EffectType = "OnRevealStealOngoing", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_super_skrull", EffectType = "SuperSkrullOngoing", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_black_widow", EffectType = "ActivateAddCardToEnemyHand", Trigger = EffectTrigger.Activate, Parameters = new Dictionary<string, object> { { "cardId", "token_widows_bite" }, { "count", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_hellcow", EffectType = "ActivateDiscardRightmost", Trigger = EffectTrigger.Activate, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_lockjaw", EffectType = "ActivateArmNextPlaySwap", Trigger = EffectTrigger.Activate, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_mysterio_illusion", EffectType = "OnRevealAddToken", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "tokenId", "token_illusion" }, { "target", "randomOtherLocation" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_mysterio_swap", EffectType = "ActivateSwapPowerWithIllusion", Trigger = EffectTrigger.Activate, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_spider_ham", EffectType = "ActivateSpiderHam", Trigger = EffectTrigger.Activate, Parameters = new Dictionary<string, object>() });

            effectRegistry.Register(new EffectDef { Id = "effect_captain_marvel", EffectType = "EndOfGameMoveToWin", Trigger = EffectTrigger.EndOfGame, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_dracula", EffectType = "EndOfGameDiscardGainPower", Trigger = EffectTrigger.EndOfGame, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_m_baku", EffectType = "EndOfGameJumpFromDeck", Trigger = EffectTrigger.EndOfGame, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_martyr", EffectType = "EndOfGameMoveToLose", Trigger = EffectTrigger.EndOfGame, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_invisible_woman", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_snowguard_hawk", EffectType = "OnRevealAddCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cardId", "token_snowguard_hawk" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_snowguard_bear", EffectType = "OnRevealAddCardToHand", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "cardId", "token_snowguard_bear" } } });
            effectRegistry.Register(new EffectDef { Id = "effect_hawk", EffectType = "OnRevealSuppressZoneEffects", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object> { { "turnOffset", 1 } } });
            effectRegistry.Register(new EffectDef { Id = "effect_bear", EffectType = "OnRevealRepeatZone", Trigger = EffectTrigger.OnReveal, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_agatha", EffectType = "AgathaGameStart", Trigger = EffectTrigger.GameStart, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_daredevil", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
            effectRegistry.Register(new EffectDef { Id = "effect_howard", EffectType = "PassiveMarker", Trigger = EffectTrigger.Ongoing, Parameters = new Dictionary<string, object>() });
        }

        /// <summary>
        /// Registra las implementaciones compartidas que usan los perks. Los
        /// ids son estables y los resuelve el mismo procesador que las cartas.
        /// </summary>
        private static void LoadPerkEffects(EffectRegistry effectRegistry)
        {
            PerkEffect(effectRegistry, PerkCatalog.EffectAfterimage,
                "Afterimage", EffectTrigger.AfterMoved);
            PerkEffect(effectRegistry, PerkCatalog.EffectAmplify,
                "Amplify", EffectTrigger.Passive);
            PerkEffect(effectRegistry, PerkCatalog.EffectApex,
                "Apex", EffectTrigger.Ongoing);
            PerkEffect(effectRegistry, PerkCatalog.EffectClone,
                "Clone", EffectTrigger.GameStart);
            PerkEffect(effectRegistry, PerkCatalog.EffectDeadlink,
                "Deadlink", EffectTrigger.OnReveal,
                new Dictionary<string, object> { { "amount", 4 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectEncore,
                "Encore", EffectTrigger.Passive);
            PerkEffect(effectRegistry, PerkCatalog.EffectHotPursuit,
                "HotPursuit", EffectTrigger.Passive,
                new Dictionary<string, object> { { "amount", 3 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectManeuverable,
                "Maneuverable", EffectTrigger.Passive);
            PerkEffect(effectRegistry, PerkCatalog.EffectMentor,
                "Mentor", EffectTrigger.OnReveal);
            PerkEffect(effectRegistry, PerkCatalog.EffectOvercharge,
                "Overcharge", EffectTrigger.OnReveal);
            PerkEffect(effectRegistry, PerkCatalog.EffectOverload,
                "Overload", EffectTrigger.StartOfTurn,
                new Dictionary<string, object> { { "amount", 1 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectOvertime,
                "Overtime", EffectTrigger.Passive);
            PerkEffect(effectRegistry, PerkCatalog.EffectQuickdrawDiscount,
                "QuickdrawDiscount", EffectTrigger.AfterDrawn,
                new Dictionary<string, object> { { "amount", 2 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectQuickdrawPower,
                "QuickdrawPower", EffectTrigger.AfterDrawn);
            PerkEffect(effectRegistry, PerkCatalog.EffectQuickdrawPowerGrant,
                "QuickdrawPowerGrant", EffectTrigger.OnReveal,
                new Dictionary<string, object> { { "amount", 3 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectRally,
                "Rally", EffectTrigger.Ongoing,
                new Dictionary<string, object> { { "amount", 1 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectRegroup,
                "Regroup", EffectTrigger.OnReveal,
                new Dictionary<string, object> { { "amount", 3 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectReinforce,
                "Reinforce", EffectTrigger.Passive);
            PerkEffect(effectRegistry, PerkCatalog.EffectSalvage,
                "Salvage", EffectTrigger.OnReveal,
                new Dictionary<string, object> { { "amount", 5 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectSidekick,
                "Sidekick", EffectTrigger.OnReveal,
                new Dictionary<string, object> { { "amount", 2 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectSilence,
                "Silence", EffectTrigger.OnReveal);
            PerkEffect(effectRegistry, PerkCatalog.EffectSlingshot,
                "Slingshot", EffectTrigger.OnReveal,
                new Dictionary<string, object> { { "amount", 1 } });
            PerkEffect(effectRegistry, PerkCatalog.EffectSuperstar,
                "Superstar", EffectTrigger.Passive);
            PerkEffect(effectRegistry, PerkCatalog.EffectTaunt,
                "Taunt", EffectTrigger.OnReveal);
            PerkEffect(effectRegistry, PerkCatalog.EffectUnstoppable,
                "Unstoppable", EffectTrigger.Passive);
            PerkEffect(effectRegistry, PerkCatalog.EffectWakandanTech,
                "WakandanTech", EffectTrigger.OnReveal);
        }

        private static void PerkEffect(EffectRegistry effectRegistry, string id,
            string effectType, EffectTrigger trigger,
            Dictionary<string, object> parameters = null)
        {
            effectRegistry.Register(new EffectDef
            {
                Id = id,
                EffectType = effectType,
                Trigger = trigger,
                Parameters = parameters ?? new Dictionary<string, object>()
            });
        }

        /// <summary>Registra el catalogo de 28 perks como fuente unica.</summary>
        private static void LoadPerks(PerkRegistry perkRegistry)
        {
            foreach (var perk in PerkCatalog.Definitions)
                perkRegistry.Register(perk);
        }

        /// <summary>
        /// Falla antes de empezar una run si el catalogo no tiene 28 perks, si
        /// vuelve un id retirado o si un efecto no tiene implementacion.
        /// </summary>
        public static void ValidatePerkCatalog(PerkRegistry perkRegistry,
            EffectRegistry effectRegistry)
        {
            var errors = new List<string>();

            if (perkRegistry == null)
                throw new InvalidOperationException("Catalogo de perks nulo.");

            var all = perkRegistry.GetAll();
            if (all.Count != PerkCatalog.AllIds.Length)
                errors.Add($"El catalogo debe tener {PerkCatalog.AllIds.Length} perks, tiene {all.Count}.");

            foreach (string retired in PerkCatalog.RetiredIds)
            {
                if (perkRegistry.Get(retired) != null)
                    errors.Add($"{retired}: perk retirado registrado.");
            }

            foreach (string perkId in PerkCatalog.AllIds)
            {
                var perk = perkRegistry.Get(perkId);
                if (perk == null)
                {
                    errors.Add($"{perkId}: perk no registrado.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(perk.Name))
                    errors.Add($"{perkId}: nombre vacio.");

                if (string.IsNullOrWhiteSpace(perk.RuleText))
                    errors.Add($"{perkId}: texto de regla vacio.");

                if (perk.EffectIds == null || perk.EffectIds.Count == 0)
                {
                    errors.Add($"{perkId}: sin ids de efecto.");
                    continue;
                }

                if (!Enum.IsDefined(typeof(PerkCompatibility), perk.Compatibility))
                    errors.Add($"{perkId}: compatibilidad no soportada.");

                foreach (string effectId in perk.EffectIds)
                {
                    var definition = effectRegistry?.Get(effectId);
                    if (definition == null)
                    {
                        errors.Add($"{perkId}: efecto no registrado '{effectId}'.");
                        continue;
                    }

                    if (BaseEffectFactory.CreateEffect(definition) == null)
                        errors.Add($"{perkId}: efecto sin implementacion '{effectId}'.");
                }
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Contenido de perks invalido:\n" + string.Join("\n", errors));
            }
        }

        /// <summary>
        /// La particion de la tienda global se valida al cargar contenido. Si
        /// los starters cambian, la particion se recalcula y falla si deja
        /// huecos o solapamientos.
        /// </summary>
        private static void ValidateStorePartition(CardRegistry cardRegistry)
        {
            GlobalStoreCatalog.Build(cardRegistry,
                SnapClone.Persistence.StarterSet.CardIds);
        }

        public static void LoadModifiers(ModifierRegistry modifierRegistry)
        {
            if (modifierRegistry == null) return;

            foreach (var modifier in SnapClone.Gameplay.Run.RunModifierSystem.Catalog)
                modifierRegistry.Register(modifier);
        }

        /// <summary>
        /// 12 eventos declinables. El accept solo usa XP, dinero, carta,
        /// mejora, perk o modificador; ninguno inicia un combate.
        /// </summary>
        public static void LoadEvents(RunEventRegistry eventRegistry)
        {
            if (eventRegistry == null) return;

            Event(eventRegistry, "event_dimensional_rift", "La grieta dimensional",
                "Una herida en el cielo muestra caminos que no deberian existir. Cruzarla duele, pero ensena.",
                "PAGAR 40 DINERO: GANAR 30 XP", 40, RunEventEffect.Xp, xp: 30);

            Event(eventRegistry, "event_wandering_merchant", "El mercader errante",
                "Un vendedor sin rostro ofrece afilar una de tus cartas a cambio de todo tu bolsillo.",
                "PAGAR 60 DINERO: MEJORAR UNA CARTA AL AZAR", 60, RunEventEffect.Upgrade);

            Event(eventRegistry, "event_combat_training", "Entrenamiento de combate",
                "Un veterano te corrige la postura durante horas. No cuesta nada escucharlo.",
                "GANAR 20 XP", 0, RunEventEffect.Xp, xp: 20);

            Event(eventRegistry, "event_battle_loot", "Botin de la batalla",
                "Entre los restos del ultimo encuentro todavia hay algo que sirve.",
                "GANAR 50 DINERO", 0, RunEventEffect.Money, money: 50);

            Event(eventRegistry, "event_altar_of_power", "El altar del poder",
                "El altar pide una ofrenda y devuelve un fragmento de poder antiguo.",
                "PAGAR 30 DINERO: GANAR UN PERK AL AZAR", 30, RunEventEffect.Perk);

            Event(eventRegistry, "event_energy_fountain", "La fuente de energia",
                "El agua de la fuente arde sin consumirse. Beberla cambia el ritmo de la batalla.",
                "GANAR EL MODIFICADOR ENERGIA EXTRA", 0, RunEventEffect.Modifier,
                modifierId: SnapClone.Gameplay.Run.RunModifierSystem.EnergyBonus);

            Event(eventRegistry, "event_collector_contract", "El contrato del coleccionista",
                "El coleccionista paga por una pieza rara, siempre que aceptes su carta misteriosa.",
                "PAGAR 30 DINERO: GANAR UNA CARTA AL AZAR", 30, RunEventEffect.Card);

            Event(eventRegistry, "event_trial_of_valor", "La prueba del valor",
                "Una prueba breve pero brutal. El orgullo exige un pago, aunque el aprendizaje compensa.",
                "GANAR 30 XP; PERDER 20 DINERO", 0, RunEventEffect.Xp, xp: 30, money: -20);

            Event(eventRegistry, "event_black_market", "El mercado negro",
                "Un puesto escondido vende lo que no deberia existir: una carta y una mejora en un solo trato.",
                "PAGAR 80 DINERO: GANAR UNA CARTA Y UNA MEJORA", 80, RunEventEffect.CardAndUpgrade);

            Event(eventRegistry, "event_vigilante_blessing", "La bendicion del vigilante",
                "Una figura silenciosa marca tu armadura. El aire parece pesar menos.",
                "GANAR EL MODIFICADOR TOPE DE ENERGIA", 0, RunEventEffect.Modifier,
                modifierId: SnapClone.Gameplay.Run.RunModifierSystem.EnergyCap);

            Event(eventRegistry, "event_traveler_toll", "El peaje del viajero",
                "El camino tiene dueño. Cruzar sin pagar no es una opcion.",
                "PERDER 40 DINERO", 0, RunEventEffect.Money, money: -40);

            Event(eventRegistry, "event_hand_of_fate", "La mano del destino",
                "Una mano invisible reordena tus cartas. La primera jugada ya no sera la misma.",
                "GANAR EL MODIFICADOR MANO INICIAL", 0, RunEventEffect.Modifier,
                modifierId: SnapClone.Gameplay.Run.RunModifierSystem.StartingHand);
        }

        private static void Event(RunEventRegistry registry, string id, string title,
            string text, string acceptText, int costMoney, RunEventEffect effectKind,
            int xp = 0, int money = 0, string modifierId = null)
        {
            registry.Register(new RunEventDef
            {
                Id = id,
                Title = title,
                Text = text,
                AcceptText = acceptText,
                CostMoney = costMoney,
                EffectKind = effectKind,
                XpAmount = xp,
                MoneyDelta = money,
                ModifierId = modifierId
            });
        }

        private static void ApplyStarterSeries(CardRegistry cardRegistry)
        {
            foreach (string id in SnapClone.Persistence.StarterSet.CardIds)
            {
                var card = cardRegistry.Get(id);
                if (card != null)
                    card.Series = CardSeries.CollectionLevel;
            }
        }

        /// <summary>
        /// Tabla de temas del draft. Es data: cambiar el arquetipo de una carta
        /// no requiere tocar ninguna escena.
        /// </summary>
        private static void ApplyDraftThemes(CardRegistry cardRegistry)
        {
            AssignThemes(cardRegistry, DraftTheme.Control, new[]
            {
                "card_iron_man", "card_punisher", "card_hulk", "card_the_thing",
                "card_cyclops", "card_angela", "card_armor", "card_bishop",
                "card_captain_america", "card_cosmo", "card_devil_dinosaur",
                "card_enchantress", "card_klaw", "card_lizard", "card_namor",
                "card_onslaught", "card_professor_x", "card_strong_guy",
                "card_abomination", "card_misty_knight", "card_shocker",
                "card_jessica_jones", "card_ka_zar", "card_mister_fantastic",
                "card_spectrum", "card_ebony_maw", "card_sandman",
                "card_storm", "card_warpath", "card_leech",
                "card_green_goblin", "card_debrii", "card_luke_cage",
                "card_omega_red", "card_red_skull", "card_typhoid_mary",
                "card_mojo", "card_cerebro", "card_patriot", "card_orka",
                "card_ronan", "card_dazzler", "card_kingpin", "card_magneto",
                "card_spider_man_2099", "card_modok",
                "card_sera", "card_mobius", "card_wave", "card_zabu",
                "card_bast", "card_valkyrie", "card_adam_warlock",
                "card_electro",
                "card_quake", "card_hercules", "card_juggernaut",
                "card_echo", "card_spider_man",
                "card_crossbones", "card_giganto", "card_sentry",
                "card_goose", "card_jean_grey", "card_magik",
                "card_super_skrull", "card_invisible_woman"
            });

            AssignThemes(cardRegistry, DraftTheme.Destroy, new[]
            {
                "card_elektra", "card_carnage", "card_deathlok", "card_angel",
                "card_apocalypse", "card_blade", "card_lady_sif", "card_nova",
                "card_spider_woman", "card_sword_master", "card_wolverine",
                "card_bucky_barnes", "card_killmonger", "card_sabretooth",
                "card_shang_chi",
                "card_lady_deathstrike", "card_venom", "card_gambit",
                "card_hazmat", "card_attuma", "card_destroyer",
                "card_colleen_wing", "card_zero",
                "card_death", "card_knull", "card_deadpool",
                "card_hela", "card_ghost_rider", "card_black_cat",
                "card_arnim_zola", "card_nimrod", "card_negasonic"
            });

            AssignThemes(cardRegistry, DraftTheme.OnReveal, new[]
            {
                "card_hawkeye", "card_medusa", "card_sentinel", "card_star_lord",
                "card_america_chavez", "card_cable", "card_doctor_strange",
                "card_domino", "card_forge", "card_groot", "card_heimdall",
                "card_iron_fist", "card_mantis", "card_mister_sinister",
                "card_moon_girl", "card_morph", "card_rocket_raccoon",
                "card_scarlet_witch", "card_squirrel_girl", "card_uatu",
                "card_white_queen", "card_odin", "card_white_tiger",
                "card_wolfsbane", "card_agent_13", "card_iceman",
                "card_jubilee", "card_nakia", "card_okoye", "card_rhino",
                "card_scorpion",
                "card_drax", "card_doctor_doom", "card_rescue",
                "card_rockslide", "card_silver_surfer", "card_the_hood",
                "card_thor", "card_wong", "card_yellowjacket",
                "card_polaris", "card_stegron", "card_dagger",
                "card_human_torch", "card_selene", "card_falcon",
                "card_ghost", "card_psylocke", "card_aero",
                "card_agent_coulson", "card_brood", "card_ghost_spider",
                "card_maria_hill", "card_mirage", "card_nick_fury",
                "card_shanna", "card_ultron",
                "card_baron_mordo", "card_beast", "card_shadow_king",
                "card_black_panther", "card_shuri", "card_taskmaster",
                "card_mister_negative",
                "card_crystal", "card_maximus", "card_jane_foster",
                "card_doctor_octopus", "card_black_bolt", "card_moon_knight",
                "card_silver_samurai", "card_master_mold",
                "card_leader", "card_silk", "card_titania", "card_nebula",
                "card_absorbing_man", "card_mystique", "card_rogue",
                "card_captain_marvel", "card_dracula", "card_m_baku",
                "card_martyr", "card_snowguard", "card_agatha",
                "card_daredevil"
            });

            AssignThemes(cardRegistry, DraftTheme.Neutral, new[]
            {
                "card_quicksilver", "card_hulkbuster", "card_korg", "card_kraven",
                "card_multiple_man", "card_yondu", "card_nightcrawler",
                "card_cloak", "card_hobgoblin", "card_morbius",
                "card_sunspot", "card_swarm", "card_the_collector",
                "card_the_infinaut", "card_vision", "card_vulture",
                "card_wasp", "card_kitty_pryde", "card_sauron",
                "card_viper", "card_she_hulk", "card_stature",
                "card_miles_morales", "card_quinjet", "card_havok",
                "card_helicarrier", "card_black_widow", "card_hellcow",
                "card_lockjaw", "card_mysterio", "card_spider_ham",
                "card_howard"
            });
        }

        private static void AssignThemes(CardRegistry cardRegistry, DraftTheme theme,
            string[] cardIds)
        {
            foreach (string id in cardIds)
            {
                var card = cardRegistry.Get(id);
                if (card == null)
                    throw new InvalidOperationException(
                        $"Tema {theme}: la carta '{id}' no esta registrada.");
                if (card.DraftTheme.HasValue)
                    throw new InvalidOperationException(
                        $"Tema {theme}: la carta '{id}' ya declara '{card.DraftTheme}'.");

                card.DraftTheme = theme;
            }
        }

        /// <summary>
        /// Toda carta jugable debe declarar tema y ningun token debe hacerlo.
        /// Falla antes de iniciar una run si la tabla quedo incompleta.
        /// </summary>
        private static void ValidateDraftThemes(CardRegistry cardRegistry)
        {
            var errors = new List<string>();

            foreach (var card in cardRegistry.GetAll())
            {
                if (card == null) continue;

                if (card.IsToken)
                {
                    if (card.DraftTheme.HasValue)
                        errors.Add($"{card.Id}: un token no debe declarar tema.");
                    continue;
                }

                if (!card.DraftTheme.HasValue)
                    errors.Add($"{card.Id}: carta jugable sin tema de draft.");
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Contenido de temas invalido:\n" + string.Join("\n", errors));
            }
        }

        /// <summary>
        /// Regla de union del catalogo: draftables esperados, ids y nombres
        /// unicos y curva 1..6. Falla al cargar si un cambio de starters deja
        /// duplicados o cartas sin clasificar.
        /// </summary>
        private static void ValidateCatalog(CardRegistry cardRegistry)
        {
            var errors = new List<string>();
            var draftable = cardRegistry.GetDraftable();

            if (draftable.Count != GlobalStoreCatalog.ExpectedDraftable)
                errors.Add($"El catalogo debe tener {GlobalStoreCatalog.ExpectedDraftable} cartas draftables, tiene {draftable.Count}.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var costs = new HashSet<int>();

            foreach (var card in draftable)
            {
                if (string.IsNullOrWhiteSpace(card.Id) || !ids.Add(card.Id))
                    errors.Add($"{card.Id}: id vacio o duplicado en el catalogo.");

                if (string.IsNullOrWhiteSpace(card.Name))
                    errors.Add($"{card.Id}: nombre vacio.");
                else if (!names.Add(card.Name))
                    errors.Add($"{card.Id}: nombre duplicado '{card.Name}'.");

                costs.Add(card.Cost);
            }

            for (int cost = 1; cost <= 6; cost++)
            {
                if (!costs.Contains(cost))
                    errors.Add($"El catalogo no cubre el coste {cost}.");
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Catalogo de cartas invalido:\n" + string.Join("\n", errors));
            }
        }

        private static void LoadCards(CardRegistry cardRegistry)
        {
            // Base del catalogo: incluye a los starters y a los tres ex starters
            // que siguen disponibles como cartas no iniciales.
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

            // Collection Level 1-14.
            Register(cardRegistry, "card_jessica_jones", "Jessica Jones", 4, 5, "Al revelarse: si no juegas una carta aqui el siguiente turno, gana +5 de Poder.", new[] { "effect_jessica_jones" }, null, CardSeries.CollectionLevel);
            Register(cardRegistry, "card_ka_zar", "Ka-Zar", 4, 6, "Continuo: tus cartas de coste 1 tienen +1 de Poder.", new[] { "effect_ka_zar" }, null, CardSeries.CollectionLevel);
            Register(cardRegistry, "card_mister_fantastic", "Mister Fantastic", 3, 1, "Continuo: las locaciones adyacentes tienen +3 de Poder.", new[] { "effect_mister_fantastic" }, null, CardSeries.CollectionLevel);
            Register(cardRegistry, "card_nightcrawler", "Nightcrawler", 1, 3, "Movible una vez.", new[] { "effect_nightcrawler" }, null, CardSeries.CollectionLevel);
            Register(cardRegistry, "card_odin", "Odin", 6, 10, "Al revelarse: repite las habilidades Al revelarse de tus otras cartas aqui.", new[] { "effect_odin" }, null, CardSeries.CollectionLevel);
            Register(cardRegistry, "card_spectrum", "Spectrum", 6, 7, "Al revelarse: tus cartas Continuas ganan +2 de Poder.", new[] { "effect_spectrum" }, null, CardSeries.CollectionLevel);
            Register(cardRegistry, "card_white_tiger", "White Tiger", 5, 1, "Al revelarse: agrega un Tigre de 10 de Poder a otra locacion.", new[] { "effect_white_tiger" }, null, CardSeries.CollectionLevel);
            Register(cardRegistry, "card_wolfsbane", "Wolfsbane", 3, 2, "Al revelarse: +2 de Poder por cada otra carta que tengas aqui.", new[] { "effect_wolfsbane" }, null, CardSeries.CollectionLevel);

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

            // Series 2.
            RegisterSeries2(cardRegistry, "card_agent_13", "Agent 13", 1, 3, "Al revelarse: agrega una carta aleatoria a tu mano.", new[] { "effect_agent_13" });
            RegisterSeries2(cardRegistry, "card_bucky_barnes", "Bucky Barnes", 2, 2, "Al ser destruida: reemplazala con el Soldado del Invierno.", new[] { "effect_bucky_barnes" });
            RegisterSeries2(cardRegistry, "card_cloak", "Cloak", 2, 4, "Al revelarse: el siguiente turno, todas las cartas son Movibles a esta locacion.", new[] { "effect_cloak" });
            RegisterSeries2(cardRegistry, "card_ebony_maw", "Ebony Maw", 1, 7, "No puedes jugar esto despues del turno 3. Continuo: no puedes jugar cartas aqui.", new[] { "effect_ebony_maw_play", "effect_ebony_maw_lock" });
            RegisterSeries2(cardRegistry, "card_hobgoblin", "Hobgoblin", 5, -8, "Al revelarse: cambia de lado.", new[] { "effect_hobgoblin" });
            RegisterSeries2(cardRegistry, "card_iceman", "Iceman", 1, 2, "Al revelarse: da +1 de Coste a una carta en la mano rival. (maximo 6)", new[] { "effect_iceman" });
            RegisterSeries2(cardRegistry, "card_jubilee", "Jubilee", 4, 1, "Al revelarse: agrega la carta superior de tu mazo a esta locacion.", new[] { "effect_jubilee" });
            RegisterSeries2(cardRegistry, "card_killmonger", "Killmonger", 3, 3, "Al revelarse: destruye TODAS las cartas de coste 1.", new[] { "effect_killmonger" });
            RegisterSeries2(cardRegistry, "card_leech", "Leech", 4, 5, "Al revelarse: quita el texto a las cartas de coste 6 o mas en la mano rival.", new[] { "effect_leech" });
            RegisterSeries2(cardRegistry, "card_morbius", "Morbius", 2, 0, "Continuo: +2 de Poder por cada vez que descartaste una carta en esta partida.", new[] { "effect_morbius" });
            RegisterSeries2(cardRegistry, "card_nakia", "Nakia", 3, 4, "Al revelarse: da +1 de Poder a todas las cartas de tu mano.", new[] { "effect_nakia" });
            RegisterSeries2(cardRegistry, "card_okoye", "Okoye", 2, 3, "Al revelarse: da +1 de Poder a cada carta de tu mazo.", new[] { "effect_okoye" });
            RegisterSeries2(cardRegistry, "card_rhino", "Rhino", 2, 5, "Al revelarse: agrega una Roca a tu lado de esta locacion.", new[] { "effect_rhino" });
            RegisterSeries2(cardRegistry, "card_sabretooth", "Sabretooth", 3, 5, "Al ser destruida: vuelve a tu mano. Cuesta 0.", new[] { "effect_sabretooth" });
            RegisterSeries2(cardRegistry, "card_sandman", "Sandman", 5, 8, "Al revelarse: el siguiente turno, las cartas cuestan 1 mas. (maximo 6)", new[] { "effect_sandman" });
            RegisterSeries2(cardRegistry, "card_scorpion", "Scorpion", 2, 3, "Al revelarse: aflige las cartas en la mano rival con -1 de Poder.", new[] { "effect_scorpion" });
            RegisterSeries2(cardRegistry, "card_shang_chi", "Shang-Chi", 4, 3, "Al revelarse: destruye una carta enemiga aqui que tenga 10 o mas de Poder.", new[] { "effect_shang_chi" });
            RegisterSeries2(cardRegistry, "card_storm", "Storm", 3, 1, "Al revelarse: inunda esta locacion. El siguiente turno es el ultimo en el que se pueden jugar cartas aqui.", new[] { "effect_storm" });
            RegisterSeries2(cardRegistry, "card_sunspot", "Sunspot", 1, 0, "Fin de turno: gana +1 de Poder por cada Energia sin gastar.", new[] { "effect_sunspot" });
            RegisterSeries2(cardRegistry, "card_swarm", "Swarm", 1, 3, "Al ser descartada: agrega dos copias de esta que cuestan 0 a tu mano.", new[] { "effect_swarm" });
            RegisterSeries2(cardRegistry, "card_the_collector", "The Collector", 2, 3, "Cuando una carta entra a tu mano desde cualquier lugar (excepto tu mazo), gana +1 de Poder.", new[] { "effect_the_collector" });
            RegisterSeries2(cardRegistry, "card_the_infinaut", "The Infinaut", 6, 20, "Si jugaste una carta el turno anterior, no puedes jugar esta.", new[] { "effect_the_infinaut" });
            RegisterSeries2(cardRegistry, "card_vision", "Vision", 5, 9, "Movible.", new[] { "effect_vision" });
            RegisterSeries2(cardRegistry, "card_vulture", "Vulture", 1, 3, "Cuando esta carta se mueve, gana +4 de Poder.", new[] { "effect_vulture" });
            RegisterSeries2(cardRegistry, "card_warpath", "Warpath", 4, 5, "Continuo: si alguna de tus locaciones esta vacia, gana +5 de Poder.", new[] { "effect_warpath" });
        }

        private static void LoadSeries3Cards(CardRegistry cardRegistry)
        {
            // Wave 1: efectos locales simples.
            RegisterSeries3(cardRegistry, "card_drax", "Drax", 4, 5, "Al revelarse: si el rival jugo una carta aqui este turno, gana +4 de Poder.", new[] { "effect_drax" });
            RegisterSeries3(cardRegistry, "card_green_goblin", "Green Goblin", 3, -4, "Al revelarse: cambia de lado.", new[] { "effect_green_goblin" });
            RegisterSeries3(cardRegistry, "card_debrii", "Debrii", 3, 7, "Al revelarse: agrega una Roca a cada otra locacion, para ambos jugadores.", new[] { "effect_debrii" });
            RegisterSeries3(cardRegistry, "card_doctor_doom", "Doctor Doom", 6, 5, "Al revelarse: agrega un DoomBot de 5 de Poder a cada otra locacion.", new[] { "effect_doctor_doom" });
            RegisterSeries3(cardRegistry, "card_rescue", "Rescue", 4, 4, "Al revelarse: si juegas una carta aqui el siguiente turno, gana +5 de Poder.", new[] { "effect_rescue" });
            RegisterSeries3(cardRegistry, "card_rockslide", "Rockslide", 4, 5, "Al revelarse: baraja 2 Rocas en el mazo rival.", new[] { "effect_rockslide" });
            RegisterSeries3(cardRegistry, "card_silver_surfer", "Silver Surfer", 3, 2, "Al revelarse: tus otras cartas de coste 3 ganan +2 de Poder.", new[] { "effect_silver_surfer" });
            RegisterSeries3(cardRegistry, "card_the_hood", "The Hood", 1, -3, "Al revelarse: agrega un Demonio a tu mano.", new[] { "effect_the_hood" });
            RegisterSeries3(cardRegistry, "card_thor", "Thor", 3, 5, "Al revelarse: baraja Mjolnir en tu mazo.", new[] { "effect_thor" });
            RegisterSeries3(cardRegistry, "card_wasp", "Wasp", 0, 1, "Mi poder secreto es que las cosas se hacen.");
            RegisterSeries3(cardRegistry, "card_wong", "Wong", 4, 2, "Continuo: tus habilidades Al revelarse aqui se repiten.", new[] { "effect_wong" });
            RegisterSeries3(cardRegistry, "card_yellowjacket", "Yellowjacket", 0, 2, "Al revelarse: aflige a tus otras cartas aqui con -1 de Poder.", new[] { "effect_yellowjacket" });
            RegisterSeries3(cardRegistry, "card_orka", "Orka", 6, 11, "Continuo: +7 de Poder si es tu unica carta aqui.", new[] { "effect_orka" });
            RegisterSeries3(cardRegistry, "card_luke_cage", "Luke Cage", 2, 2, "Continuo: tus cartas en juego no pueden perder Poder.", new[] { "effect_luke_cage" });
            RegisterSeries3(cardRegistry, "card_omega_red", "Omega Red", 4, 6, "Continuo: si ganas aqui, tus otras locaciones ganan +3 de Poder.", new[] { "effect_omega_red" });
            RegisterSeries3(cardRegistry, "card_red_skull", "Red Skull", 5, 14, "Continuo: las cartas enemigas aqui ganan +2 de Poder.", new[] { "effect_red_skull" });
            RegisterSeries3(cardRegistry, "card_typhoid_mary", "Typhoid Mary", 4, 10, "Continuo: tus otras cartas aqui tienen -2 de Poder.", new[] { "effect_typhoid_mary" });
            RegisterSeries3(cardRegistry, "card_mojo", "Mojo", 2, 2, "Continuo: si ambos lados aqui estan llenos, gana +6 de Poder.", new[] { "effect_mojo" });
            RegisterSeries3(cardRegistry, "card_cerebro", "Cerebro", 3, 0, "Continuo: tus cartas de mayor Poder ganan +3 de Poder.", new[] { "effect_cerebro" });
            RegisterSeries3(cardRegistry, "card_patriot", "Patriot", 3, 2, "Continuo: tus cartas sin habilidades ganan +2 de Poder.", new[] { "effect_patriot" });

            // Wave 2: efectos locales, movimiento y mano/mazo.
            RegisterSeries3(cardRegistry, "card_ronan", "Ronan the Accuser", 5, 5, "Continuo: +2 de Poder por cada carta en la mano rival.", new[] { "effect_ronan" });
            RegisterSeries3(cardRegistry, "card_dazzler", "Dazzler", 2, 2, "Continuo: +2 de Poder por cada locacion llena de tu lado.", new[] { "effect_dazzler" });
            RegisterSeries3(cardRegistry, "card_polaris", "Polaris", 3, 5, "Al revelarse: mueve una carta enemiga de coste 1 o 2 a esta locacion.", new[] { "effect_polaris" });
            RegisterSeries3(cardRegistry, "card_stegron", "Stegron", 4, 7, "Al revelarse: mueve una carta enemiga de aqui una locacion a la derecha.", new[] { "effect_stegron" });
            RegisterSeries3(cardRegistry, "card_magneto", "Magneto", 6, 12, "Al revelarse: mueve todas las cartas enemigas de coste 3 y 4 a esta locacion.", new[] { "effect_magneto" });
            RegisterSeries3(cardRegistry, "card_kingpin", "Kingpin", 1, 3, "Cuando una carta enemiga se mueve aqui, afligela con -2 de Poder.", new[] { "effect_kingpin" });
            RegisterSeries3(cardRegistry, "card_dagger", "Dagger", 2, 2, "Cuando esta carta se mueve a una locacion, gana +2 de Poder por cada carta enemiga alli.", new[] { "effect_dagger" });
            RegisterSeries3(cardRegistry, "card_human_torch", "Human Torch", 3, 2, "Cuando esta carta se mueve, duplica su Poder.", new[] { "effect_human_torch" });
            RegisterSeries3(cardRegistry, "card_spider_man_2099", "Spider-Man 2099", 4, 5, "Movible una vez. Cuando se mueve a una locacion, aflige una carta enemiga alli con -3 de Poder.", new[] { "effect_spider_man_2099_move", "effect_spider_man_2099_afflict" });
            RegisterSeries3(cardRegistry, "card_lady_deathstrike", "Lady Deathstrike", 5, 7, "Al revelarse: destruye cada carta aqui con 3 o menos de Poder.", new[] { "effect_lady_deathstrike" });
            RegisterSeries3(cardRegistry, "card_venom", "Venom", 3, 3, "Al revelarse: destruye tus otras cartas aqui. Suma su Poder a esta carta.", new[] { "effect_venom" });
            RegisterSeries3(cardRegistry, "card_modok", "M.O.D.O.K.", 5, 7, "Al revelarse: descarta tu mano.", new[] { "effect_modok" });
            RegisterSeries3(cardRegistry, "card_selene", "Selene", 1, 2, "Al revelarse: aflige la carta de menor Poder de cada mano con -3 de Poder.", new[] { "effect_selene" });
            RegisterSeries3(cardRegistry, "card_hazmat", "Hazmat", 3, 3, "Al revelarse: aflige a todas las demas cartas con -1 de Poder.", new[] { "effect_hazmat" });
            RegisterSeries3(cardRegistry, "card_gambit", "Gambit", 3, 3, "Al revelarse: descarta una carta de tu mano para destruir una carta enemiga aleatoria.", new[] { "effect_gambit" });
            RegisterSeries3(cardRegistry, "card_falcon", "Falcon", 2, 3, "Al revelarse: devuelve tus cartas de coste 1 a tu mano.", new[] { "effect_falcon" });
            RegisterSeries3(cardRegistry, "card_ghost", "Ghost", 3, 5, "Al revelarse: quita las habilidades a las cartas de Fin de turno aqui.", new[] { "effect_ghost" });
            RegisterSeries3(cardRegistry, "card_psylocke", "Psylocke", 2, 2, "Al revelarse: el siguiente turno, ganas +1 de Energia.", new[] { "effect_psylocke" });
            RegisterSeries3(cardRegistry, "card_kitty_pryde", "Kitty Pryde", 1, 1, "Inicio de turno: vuelve a tu mano. Cuando vuelve, gana +1 de Poder.", new[] { "effect_kitty_pryde" });
            RegisterSeries3(cardRegistry, "card_aero", "Aero", 5, 9, "Al revelarse: la ultima carta jugada por el rival se mueve a esta locacion.", new[] { "effect_aero" });

            // Wave 3: tokens, copia y mazo.
            RegisterSeries3(cardRegistry, "card_agent_coulson", "Agent Coulson", 3, 5, "Al revelarse: agrega una carta aleatoria de coste 4 y otra de coste 5 a tu mano.", new[] { "effect_agent_coulson_4", "effect_agent_coulson_5" });
            RegisterSeries3(cardRegistry, "card_attuma", "Attuma", 4, 10, "Fin de turno: destruye una de tus cartas aqui con menos Poder.", new[] { "effect_attuma" });
            RegisterSeries3(cardRegistry, "card_brood", "Brood", 3, 2, "Al revelarse: agrega 2 Broodlings aqui con el mismo Poder.", new[] { "effect_brood" });
            RegisterSeries3(cardRegistry, "card_colleen_wing", "Colleen Wing", 2, 3, "Al revelarse: descarta la carta de menor coste de tu mano.", new[] { "effect_colleen_wing" });
            RegisterSeries3(cardRegistry, "card_destroyer", "Destroyer", 6, 18, "Al revelarse: destruye tus cartas en las otras locaciones.", new[] { "effect_destroyer" });
            RegisterSeries3(cardRegistry, "card_ghost_spider", "Ghost-Spider", 1, 3, "Al revelarse: la ultima carta que jugaste se mueve aqui.", new[] { "effect_ghost_spider" });
            RegisterSeries3(cardRegistry, "card_maria_hill", "Maria Hill", 1, 3, "Al revelarse: agrega una carta aleatoria de coste 2 a tu mano.", new[] { "effect_maria_hill" });
            RegisterSeries3(cardRegistry, "card_mirage", "Mirage", 2, 3, "Al revelarse: copia la carta de menor coste de la mano rival a tu mano. Dale +2 de Poder.", new[] { "effect_mirage" });
            RegisterSeries3(cardRegistry, "card_nick_fury", "Nick Fury", 4, 6, "Al revelarse: agrega 3 cartas aleatorias de coste 6 a tu mano.", new[] { "effect_nick_fury" });
            RegisterSeries3(cardRegistry, "card_sauron", "Sauron", 1, 2, "Al revelarse: quita las habilidades a las cartas Continuas de tu mazo.", new[] { "effect_sauron" });
            RegisterSeries3(cardRegistry, "card_shanna", "Shanna", 3, 4, "Al revelarse: agrega una carta aleatoria de coste 1 a cada locacion.", new[] { "effect_shanna" });
            RegisterSeries3(cardRegistry, "card_ultron", "Ultron", 6, 8, "Al revelarse: despliega cuatro Drones de 2 de Poder en cada otra locacion.", new[] { "effect_ultron" });
            RegisterSeries3(cardRegistry, "card_viper", "Viper", 3, 4, "Al revelarse: una de tus otras cartas aqui con el menor Poder cambia de lado.", new[] { "effect_viper" });
            RegisterSeries3(cardRegistry, "card_zero", "Zero", 1, 3, "Al revelarse: quita las habilidades a la siguiente carta que juegues.", new[] { "effect_zero" });

            // Wave 4: coste, poder y cartas destruidas.
            RegisterSeries3(cardRegistry, "card_death", "Death", 8, 12, "Cuesta 1 menos por cada carta destruida en esta partida.", new[] { "effect_death" });
            RegisterSeries3(cardRegistry, "card_knull", "Knull", 6, 0, "Continuo: tiene el Poder combinado de todas las cartas destruidas en esta partida.", new[] { "effect_knull" });
            RegisterSeries3(cardRegistry, "card_she_hulk", "She-Hulk", 6, 10, "Cuesta 1 menos por cada Energia sin gastar el turno anterior.", new[] { "effect_she_hulk" });
            RegisterSeries3(cardRegistry, "card_stature", "Stature", 5, 7, "Cuesta 1 si el rival descarto una carta en esta partida.", new[] { "effect_stature" });
            RegisterSeries3(cardRegistry, "card_miles_morales", "Miles Morales Spider-Man", 4, 6, "Si una carta se movio el turno anterior, cuesta 1.", new[] { "effect_miles_morales" });
            RegisterSeries3(cardRegistry, "card_sera", "Sera", 5, 5, "Continuo: las cartas en tu mano cuestan 1 menos. (minimo 1)", new[] { "effect_sera" });
            RegisterSeries3(cardRegistry, "card_quinjet", "Quinjet", 1, 1, "Continuo: tus cartas creadas cuestan 1 menos. (minimo 1)", new[] { "effect_quinjet" });
            RegisterSeries3(cardRegistry, "card_zabu", "Zabu", 1, 1, "Al revelarse: da -1 de Coste a cada carta de coste 4 de tu mazo.", new[] { "effect_zabu" });
            RegisterSeries3(cardRegistry, "card_mobius", "Mobius M. Mobius", 3, 3, "Continuo: tus Costes no pueden subir. Los Costes rivales no pueden bajar.", new[] { "effect_mobius" });
            RegisterSeries3(cardRegistry, "card_wave", "Wave", 3, 6, "Al revelarse: todas las cartas cuestan como maximo 4 hasta el final del siguiente turno.", new[] { "effect_wave" });
            RegisterSeries3(cardRegistry, "card_baron_mordo", "Baron Mordo", 2, 3, "Al revelarse: la carta superior del mazo rival cuesta 6 hasta el turno 6.", new[] { "effect_baron_mordo" });
            RegisterSeries3(cardRegistry, "card_beast", "Beast", 3, 5, "Al revelarse: devuelve tus otras cartas aqui a tu mano. Cuestan 1 menos el siguiente turno.", new[] { "effect_beast" });
            RegisterSeries3(cardRegistry, "card_bast", "Bast", 1, 1, "Al revelarse: el Poder de todas las cartas en tu mano pasa a 3.", new[] { "effect_bast" });
            RegisterSeries3(cardRegistry, "card_valkyrie", "Valkyrie", 5, 3, "Al revelarse: todas las cartas aqui pasan a 3 de Poder.", new[] { "effect_valkyrie" });
            RegisterSeries3(cardRegistry, "card_shadow_king", "Shadow King", 3, 4, "Al revelarse: vuelve todas las cartas aqui a su Poder original.", new[] { "effect_shadow_king" });
            RegisterSeries3(cardRegistry, "card_black_panther", "Black Panther", 5, 5, "Al revelarse: duplica el Poder de esta carta.", new[] { "effect_black_panther" });
            RegisterSeries3(cardRegistry, "card_shuri", "Shuri", 4, 3, "Al revelarse: si juegas tu siguiente carta aqui, duplica su Poder.", new[] { "effect_shuri" });
            RegisterSeries3(cardRegistry, "card_taskmaster", "Taskmaster", 6, 0, "Al revelarse: iguala su Poder al de la ultima carta que jugaste.", new[] { "effect_taskmaster" });
            RegisterSeries3(cardRegistry, "card_deadpool", "Deadpool", 1, 1, "Al ser destruida: vuelve a tu mano con el doble de Poder.", new[] { "effect_deadpool" });
            RegisterSeries3(cardRegistry, "card_mister_negative", "Mister Negative", 4, -1, "Al revelarse: intercambia el Poder y el Coste de todas las cartas de tu mazo.", new[] { "effect_mister_negative" });

            // Wave 5: robo, resurreccion, mano rival y Energia maxima.
            RegisterSeries3(cardRegistry, "card_adam_warlock", "Adam Warlock", 2, 0, "Fin de turno: roba una carta si ganas aqui. Si no, gana +1 de Poder.", new[] { "effect_adam_warlock" });
            RegisterSeries3(cardRegistry, "card_crystal", "Crystal", 3, 6, "Al revelarse: cada jugador roba una carta.", new[] { "effect_crystal" });
            RegisterSeries3(cardRegistry, "card_maximus", "Maximus", 2, 6, "Al revelarse: el rival roba 2 cartas.", new[] { "effect_maximus" });
            RegisterSeries3(cardRegistry, "card_jane_foster", "Jane Foster Mighty Thor", 5, 9, "Al revelarse: roba todas las cartas de coste 0 de tu mazo.", new[] { "effect_jane_foster" });
            RegisterSeries3(cardRegistry, "card_ghost_rider", "Ghost Rider", 4, 5, "Al revelarse: devuelve una de tus cartas descartadas a esta locacion.", new[] { "effect_ghost_rider" });
            RegisterSeries3(cardRegistry, "card_hela", "Hela", 6, 6, "Al revelarse: por cada coste distinto, resucita una carta descartada en una locacion aleatoria.", new[] { "effect_hela" });
            RegisterSeries3(cardRegistry, "card_doctor_octopus", "Doctor Octopus", 4, 8, "Al revelarse: trae la carta de menor Poder de la mano rival a su lado de esta locacion.", new[] { "effect_doctor_octopus" });
            RegisterSeries3(cardRegistry, "card_black_bolt", "Black Bolt", 5, 8, "Al revelarse: descarta la carta de menor coste de la mano rival.", new[] { "effect_black_bolt" });
            RegisterSeries3(cardRegistry, "card_moon_knight", "Moon Knight", 3, 3, "Al revelarse: descarta una carta de coste par de cada mano.", new[] { "effect_moon_knight" });
            RegisterSeries3(cardRegistry, "card_silver_samurai", "Silver Samurai", 4, 6, "Al revelarse: cada jugador descarta su carta de menor Poder.", new[] { "effect_silver_samurai" });
            RegisterSeries3(cardRegistry, "card_master_mold", "Master Mold", 2, 3, "Al revelarse: agrega 2 Sentinel a la mano rival.", new[] { "effect_master_mold" });
            RegisterSeries3(cardRegistry, "card_electro", "Electro", 3, 5, "Al revelarse: +1 de Energia maxima. Continuo: solo puedes jugar 1 carta por turno.", new[] { "effect_electro_energy", "effect_electro" });
            RegisterSeries3(cardRegistry, "card_havok", "Havok", 2, 1, "Fin de turno: pierdes 1 de Energia maxima y esta carta gana +3 de Poder.", new[] { "effect_havok" });
            RegisterSeries3(cardRegistry, "card_black_cat", "Black Cat", 1, 4, "Fin de turno: descarta esta carta de tu mano.", new[] { "effect_black_cat" });
            RegisterSeries3(cardRegistry, "card_helicarrier", "Helicarrier", 6, 12, "Al ser descartada: llena tu mano con cartas aleatorias.", new[] { "effect_helicarrier" });

            // Wave 6: copia a zonas, intercambio y reacciones de movimiento.
            RegisterSeries3(cardRegistry, "card_arnim_zola", "Arnim Zola", 6, 0, "Al revelarse: destruye una de tus otras cartas aqui para copiarla en las otras locaciones.", new[] { "effect_arnim_zola" });
            RegisterSeries3(cardRegistry, "card_leader", "Leader", 6, 5, "Al revelarse: copia las cartas enemigas de mayor Poder jugadas este turno, en tu lado.", new[] { "effect_leader" });
            RegisterSeries3(cardRegistry, "card_nimrod", "Nimrod", 5, 6, "Al ser destruida: agrega una copia de esta a cada otra locacion.", new[] { "effect_nimrod" });
            RegisterSeries3(cardRegistry, "card_quake", "Quake", 2, 3, "Al revelarse: intercambia las posiciones de las otras dos locaciones.", new[] { "effect_quake" });
            RegisterSeries3(cardRegistry, "card_echo", "Echo", 1, 2, "Despues de que el rival juegue una carta Continua aqui, quitale sus habilidades.", new[] { "effect_echo" });
            RegisterSeries3(cardRegistry, "card_negasonic", "Negasonic Teenage Warhead", 4, 4, "Despues de que se juegue una carta enemiga aqui, destruyela. (una vez por partida)", new[] { "effect_negasonic" });
            RegisterSeries3(cardRegistry, "card_silk", "Silk", 2, 5, "Despues de que CUALQUIER carta se juegue aqui, mueve esta a otra locacion.", new[] { "effect_silk" });
            RegisterSeries3(cardRegistry, "card_titania", "Titania", 1, 5, "Despues de que CUALQUIER carta se juegue aqui, esta carta cambia de lado.", new[] { "effect_titania" });
            RegisterSeries3(cardRegistry, "card_hercules", "Hercules", 3, 5, "La primera vez que otra de tus cartas se mueva aqui cada turno, muevela a otra locacion.", new[] { "effect_hercules" });
            RegisterSeries3(cardRegistry, "card_nebula", "Nebula", 1, 1, "Cada turno que el rival no juegue una carta aqui, gana +2 de Poder. (excepto el turno en que la juegas)", new[] { "effect_nebula" });
            RegisterSeries3(cardRegistry, "card_juggernaut", "Juggernaut", 4, 5, "Al revelarse: aleja todas las cartas enemigas jugadas aqui este turno. (incluye las no reveladas)", new[] { "effect_juggernaut" });
            RegisterSeries3(cardRegistry, "card_spider_man", "Spider-Man", 2, 4, "Al revelarse: muevete a otra locacion y arrastra una carta enemiga de aqui hacia alla.", new[] { "effect_spider_man" });

            // Wave 7: restricciones, zonas y Limbo.
            RegisterSeries3(cardRegistry, "card_crossbones", "Crossbones", 4, 10, "Solo puedes jugar esta carta en locaciones donde ganas.", new[] { "effect_crossbones" });
            RegisterSeries3(cardRegistry, "card_giganto", "Giganto", 6, 16, "Solo puedes jugar esta carta en la locacion izquierda.", new[] { "effect_giganto" });
            RegisterSeries3(cardRegistry, "card_sentry", "Sentry", 4, 9, "No puedes jugar esta carta en la locacion derecha. Al revelarse: agrega un Vacio de -9 de Poder a la locacion derecha.", new[] { "effect_sentry_play", "effect_sentry_void" });
            RegisterSeries3(cardRegistry, "card_goose", "Goose", 2, 2, "Continuo: nadie puede jugar cartas de coste 4 o mas aqui.", new[] { "effect_goose" });
            RegisterSeries3(cardRegistry, "card_jean_grey", "Jean Grey", 3, 4, "Continuo: los jugadores deben jugar su primera carta aqui cada turno. (si es posible)", new[] { "effect_jean_grey" });
            RegisterSeries3(cardRegistry, "card_magik", "Magik", 3, 2, "Al revelarse: reemplaza esta locacion por Limbo. No funciona despues del turno 5.", new[] { "effect_magik" });

            // Wave 8: Activate y texto copiado.
            RegisterSeries3(cardRegistry, "card_absorbing_man", "Absorbing Man", 4, 5, "Al revelarse: si la ultima carta que jugaste tiene Al revelarse, copia su texto. (si es posible)", new[] { "effect_absorbing_man" });
            RegisterSeries3(cardRegistry, "card_mystique", "Mystique", 3, 1, "Al revelarse: si la ultima carta que jugaste tiene Continuo, copia su texto. (si es posible)", new[] { "effect_mystique" });
            RegisterSeries3(cardRegistry, "card_rogue", "Rogue", 3, 3, "Al revelarse: roba el texto de una carta Continuo enemiga aqui.", new[] { "effect_rogue" });
            RegisterSeries3(cardRegistry, "card_super_skrull", "Super-Skrull", 4, 2, "Continuo: tiene los efectos Continuos de todas las cartas enemigas.", new[] { "effect_super_skrull" });
            RegisterSeries3(cardRegistry, "card_black_widow", "Black Widow", 3, 3, "Activar: agrega una Mordida de Viuda a la mano rival.", new[] { "effect_black_widow" });
            RegisterSeries3(cardRegistry, "card_hellcow", "Hellcow", 4, 7, "Activar: descarta una carta de tu mano.", new[] { "effect_hellcow" });
            RegisterSeries3(cardRegistry, "card_lockjaw", "Lockjaw", 2, 3, "Activar: despues de jugar tu siguiente personaje, intercambialo por una carta de tu mazo.", new[] { "effect_lockjaw" });
            RegisterSeries3(cardRegistry, "card_mysterio", "Mysterio", 2, 5, "Al revelarse: agrega una Ilusion a otra locacion. Activar: intercambia Poder con ella.", new[] { "effect_mysterio_illusion", "effect_mysterio_swap" });
            RegisterSeries3(cardRegistry, "card_spider_ham", "Spider-Ham", 1, 1, "Activar: reemplaza el texto de un personaje aleatorio de la mano rival por 'oink!'.", new[] { "effect_spider_ham" });

            // Wave 9: fin de partida, revelado diferido e informacion.
            RegisterSeries3(cardRegistry, "card_captain_marvel", "Captain Marvel", 4, 6, "Al final de la partida, muevete a una locacion que te haga ganar. (si es posible)", new[] { "effect_captain_marvel" });
            RegisterSeries3(cardRegistry, "card_dracula", "Dracula", 4, 1, "Al final de la partida, descarta una carta de tu mano y gana su Poder.", new[] { "effect_dracula" });
            RegisterSeries3(cardRegistry, "card_m_baku", "M'Baku", 1, 3, "Al final de la partida, salta de tu mazo a tu locacion de menor Poder. (que no este llena)", new[] { "effect_m_baku" });
            RegisterSeries3(cardRegistry, "card_martyr", "Martyr", 1, 5, "Al final de la partida, muevete a una locacion que te haga perder. (si es posible)", new[] { "effect_martyr" });
            RegisterSeries3(cardRegistry, "card_invisible_woman", "Invisible Woman", 2, 4, "Continuo: las cartas que juegues aqui no se revelan hasta el final de la partida.", new[] { "effect_invisible_woman" });
            RegisterSeries3(cardRegistry, "card_snowguard", "Snowguard", 1, 2, "Al revelarse: agrega las auroras Hawk y Bear a tu mano.", new[] { "effect_snowguard_hawk", "effect_snowguard_bear" });
            RegisterSeries3(cardRegistry, "card_agatha", "Agatha Harkness", 6, 12, "Inicio de partida: roba a Agatha. Ella controla tus cartas en los turnos pares.", new[] { "effect_agatha" }, new[] { SnapClone.Gameplay.Combat.OpeningHandDealer.StartsInHandTag });
            RegisterSeries3(cardRegistry, "card_daredevil", "Daredevil", 2, 3, "En el turno 5, ves las jugadas del rival antes de hacer las tuyas.", new[] { "effect_daredevil" });
            RegisterSeries3(cardRegistry, "card_howard", "Howard the Duck", 1, 2, "Continuo: toca esta carta para ver la carta superior de tu mazo.", new[] { "effect_howard" });
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

            cardRegistry.Register(new CardDef
            {
                Id = PerkCatalog.ZombieHordeCardId,
                Name = "Horda Zombie",
                Cost = 0,
                BasePower = 0,
                Description = "Una horda sin habilidad. Deadlink la potencia.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_tiger",
                Name = "Tigre",
                Cost = 0,
                BasePower = 10,
                Description = "Un tigre de 10 de Poder sin habilidad.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_winter_soldier",
                Name = "Soldado del Invierno",
                Cost = 0,
                BasePower = 8,
                Description = "El Soldado del Invierno reemplaza a Bucky Barnes.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_widows_bite",
                Name = "Mordida de Viuda",
                Cost = 0,
                BasePower = -1,
                Description = "Mientras esta en la mano, su dueno no puede robar cartas.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_broodling",
                Name = "Broodling",
                Cost = 0,
                BasePower = 2,
                Description = "Una cria sin habilidad. Copia el Poder de Brood al crearse.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_doombot",
                Name = "DoomBot",
                Cost = 0,
                BasePower = 5,
                Description = "Un DoomBot de 5 de Poder sin habilidad.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_drone",
                Name = "Dron",
                Cost = 0,
                BasePower = 2,
                Description = "Un dron de 2 de Poder sin habilidad.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_illusion",
                Name = "Ilusion",
                Cost = 0,
                BasePower = 0,
                Description = "Una ilusion sin habilidad.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_demon",
                Name = "Demonio",
                Cost = 0,
                BasePower = 6,
                Description = "Un demonio de 6 de Poder sin habilidad.",
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_mjolnir",
                Name = "Mjolnir",
                Cost = 0,
                BasePower = 0,
                Description = "Al revelarse: da +7 de Poder a Thor.",
                EffectIds = new List<string> { "effect_mjolnir" },
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_snowguard_hawk",
                Name = "Hawk",
                Cost = 3,
                BasePower = 3,
                Description = "Al revelarse: ignora las habilidades de las zonas hasta el final del turno siguiente.",
                EffectIds = new List<string> { "effect_hawk" },
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_snowguard_bear",
                Name = "Bear",
                Cost = 3,
                BasePower = 4,
                Description = "Al revelarse: repite el efecto de esta zona.",
                EffectIds = new List<string> { "effect_bear" },
                IsToken = true
            });

            cardRegistry.Register(new CardDef
            {
                Id = "token_void",
                Name = "Vacio",
                Cost = 0,
                BasePower = -9,
                Description = "El Vacio de Sentry. Sin habilidad.",
                IsToken = true
            });
        }

        private static void Register(CardRegistry registry, string id, string name,
            int cost, int power, string description, string[] effectIds = null,
            string[] tags = null, CardSeries series = CardSeries.Series1)
        {
            registry.Register(new CardDef
            {
                Id = id,
                Name = name,
                Cost = cost,
                BasePower = power,
                Description = description,
                Series = series,
                EffectIds = effectIds == null
                    ? new List<string>()
                    : new List<string>(effectIds),
                Tags = tags == null ? new List<string>() : new List<string>(tags)
            });
        }

        private static void RegisterSeries2(CardRegistry registry, string id,
            string name, int cost, int power, string description,
            string[] effectIds = null, string[] tags = null)
        {
            Register(registry, id, name, cost, power, description, effectIds,
                tags, CardSeries.Series2);
        }

        private static void RegisterSeries3(CardRegistry registry, string id,
            string name, int cost, int power, string description,
            string[] effectIds = null, string[] tags = null)
        {
            Register(registry, id, name, cost, power, description, effectIds,
                tags, CardSeries.Series3);
        }
    }
}
