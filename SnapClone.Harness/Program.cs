using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Content;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Effects;
using SnapClone.Effects.Implementations;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.AI;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Draft;
using SnapClone.Gameplay.Meta;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;

namespace SnapClone.Harness
{
    /// <summary>
    /// Verificacion ejecutable de los cambios de meta, tienda, coleccion y
    /// perks. No abre ventana ni toca disco real.
    /// </summary>
    internal static partial class Program
    {
        private static int _failures;

        private static readonly List<string> Failures = new List<string>();

        private static int Main()
        {
            CardRegistry cards = new CardRegistry();
            ZoneRegistry zones = new ZoneRegistry();
            EffectRegistry effects = new EffectRegistry();
            PerkRegistry perks = new PerkRegistry();
            ItemRegistry items = new ItemRegistry();
            EnemyRegistry enemies = new EnemyRegistry();
            ModifierRegistry modifiers = new ModifierRegistry();
            RunEventRegistry runEvents = new RunEventRegistry();
            // Se cargan todos los catalogos, igual que Game1.Initialize, para
            // que una validacion de arranque rota falle aqui y no solo en la APK.
            ContentLoader.LoadAll(cards, zones, effects, perks, enemies,
                modifiers, runEvents, items);

            RunPerkCatalogChecks(perks, cards, effects);
            RunPointsChecks();
            RunAwardChecks();
            RunProfileRoundTripChecks(items);
            RunPartitionChecks(cards);
            RunStoreChecks(cards, perks);
            RunCollectionChecks(cards, perks);
            RunLevelUpChecks(cards, perks, effects);
            RunPerkAbilityChecks(cards, effects, zones, perks);
            RunSeries2Checks(cards, effects, zones);
        RunSeries3FoundationChecks(cards, effects, zones);
        RunSeries3Wave1Checks(cards, effects, zones);
        RunSeries3Wave2Checks(cards, effects, zones);
        RunSeries3Wave3Checks(cards, effects, zones);
        RunSeries3Wave4Checks(cards, effects, zones);
        RunSeries3Wave5Checks(cards, effects, zones);
        RunSeries3Wave6Checks(cards, effects, zones);
        RunSeries3Wave7Checks(cards, effects, zones);
        RunSeries3Wave8Checks(cards, effects, zones);
        RunSeries3Wave9Checks(cards, effects, zones);
        RunSeries3EndToEndChecks(cards, effects, zones);
            RunPerkPersistenceChecks(cards, effects, perks);
            RunItemCatalogChecks(cards, items);
            RunItemUseChecks(cards, effects, items);
            RunItemStoreChecks(cards, perks, items);
            RunEndToEndChecks(cards, perks);
            RunUiFoundationChecks();

            Console.WriteLine();
            if (_failures == 0)
            {
                Console.WriteLine("HARNESS META: TODO OK");
                return 0;
            }

            Console.WriteLine($"HARNESS META: {_failures} fallo(s)");
            foreach (string failure in Failures)
                Console.WriteLine(" - " + failure);

            return 1;
        }

        // ------------------------------------------------------------------
        // Catalogo de perks
        // ------------------------------------------------------------------

        private static void RunPerkCatalogChecks(PerkRegistry perks,
            CardRegistry cards, EffectRegistry effects)
        {
            var all = perks.GetAll();
            Check("perks: 28 registrados", all.Count == 28, $"hay {all.Count}");
            Check("perks: AllIds tiene 28", PerkCatalog.AllIds.Length == 28,
                PerkCatalog.AllIds.Length.ToString());

            var shared = new HashSet<string>(StringComparer.Ordinal);
            foreach (string perkId in PerkCatalog.AllIds)
            {
                var perk = perks.Get(perkId);
                Check($"perks: {perkId} registrado", perk != null, "no existe");
                if (perk == null) continue;

                Check($"perks: {perkId} con texto de regla",
                    !string.IsNullOrWhiteSpace(perk.RuleText), "texto vacio");
                Check($"perks: {perkId} con efectos",
                    perk.EffectIds != null && perk.EffectIds.Count > 0,
                    "sin efectos");
                Check($"perks: {perkId} con compatibilidad valida",
                    Enum.IsDefined(typeof(PerkCompatibility), perk.Compatibility),
                    perk.Compatibility.ToString());

                foreach (string effectId in perk.EffectIds)
                {
                    Check($"perks: {perkId} efecto {effectId} implementado",
                        effects.Get(effectId) != null &&
                        BaseEffectFactory.CreateEffect(effects.Get(effectId)) != null,
                        "sin implementacion");
                    shared.Add(effectId);
                }
            }

            foreach (string retired in PerkCatalog.RetiredIds)
            {
                Check($"perks: retirado {retired} ausente",
                    perks.Get(retired) == null, "sigue registrado");
            }

            // Efectos compartidos con cartas actuales.
            Check("perks: Reality Warp comparte efecto con Scarlet Witch",
                PerkCatalog.EffectIdsFor(PerkCatalog.RealityWarp)
                    .Contains("effect_scarlet_witch"), "no comparte");
            Check("perks: Tag In comparte efecto con Angel",
                PerkCatalog.EffectIdsFor(PerkCatalog.TagIn)
                    .Contains("effect_angel"), "no comparte");

            var zombie = cards.Get(PerkCatalog.ZombieHordeCardId);
            Check("perks: token Zombie Horde existe", zombie != null, "ausente");
            Check("perks: Zombie Horde coste 0 y poder 0",
                zombie != null && zombie.Cost == 0 && zombie.BasePower == 0,
                "valores incorrectos");
            Check("perks: Zombie Horde no es draftable",
                zombie != null && zombie.IsToken &&
                !cards.GetDraftable().Any(card => card.Id == PerkCatalog.ZombieHordeCardId),
                "entra al draft");
            Check("perks: catalogo sigue en 92 draftables",
                cards.GetDraftable().Count == GlobalStoreCatalog.ExpectedDraftable,
                cards.GetDraftable().Count.ToString());

            ContentLoader.ValidatePerkCatalog(perks, effects);
            Check("perks: validacion de catalogo", true, null);
        }

        // ------------------------------------------------------------------
        // Puntos
        // ------------------------------------------------------------------

        private static void RunPointsChecks()
        {
            var meta = new MetaProgressionSystem();

            var run = NewRun();
            run.NormalCombatsWon = 10;
            run.SubBossesWon = 2;
            run.Level = 5;
            run.FinalBossDefeated = false;

            var breakdown = meta.Compute(run);
            Check("puntos: formula derrota",
                breakdown.NormalCombatPoints == 30 &&
                breakdown.SubBossPoints == 80 &&
                breakdown.LevelPoints == 40 &&
                breakdown.FinalBossBonus == 0 &&
                breakdown.Total == 150,
                $"total {breakdown.Total}");

            run.FinalBossDefeated = true;
            Check("puntos: bono de jefe final",
                meta.Compute(run).Total == 300,
                $"total {meta.Compute(run).Total}");

            var defeat = NewRun();
            defeat.Level = 3;
            defeat.NormalCombatsWon = 4;
            defeat.Outcome = RunOutcome.Lost;
            var defeatBreakdown = meta.Compute(defeat);
            Check("puntos: derrota conserva lo ganado",
                defeatBreakdown.NormalCombatPoints == 12 &&
                defeatBreakdown.LevelPoints == 24 &&
                defeatBreakdown.Total == 36,
                $"total {defeatBreakdown.Total}");

            var victory = NewRun();
            victory.Level = 1;
            victory.FinalBossDefeated = true;
            Check("puntos: run minima con jefe",
                meta.Compute(victory).Total == 158,
                $"total {meta.Compute(victory).Total}");
        }

        private static void RunAwardChecks()
        {
            var meta = new MetaProgressionSystem();
            var profile = ProfileDto.CreateDefault(StarterSet.CardIds);
            var run = NewRun();
            run.NormalCombatsWon = 2;
            run.SubBossesWon = 1;
            run.Level = 4;

            int saves = 0;
            var first = meta.Award(run, profile, p => { saves++; return true; });
            Check("award: primera vez", first.Status == AwardStatus.Awarded, first.Error);
            Check("award: puntos sumados", profile.Points == 78, $"puntos {profile.Points}");
            Check("award: run id guardado",
                profile.LastAwardedRunId == run.RunId, profile.LastAwardedRunId);

            var second = meta.Award(run, profile, p => { saves++; return true; });
            Check("award: segunda vez no suma",
                second.Status == AwardStatus.AlreadyCollected && profile.Points == 78,
                $"estado {second.Status}, puntos {profile.Points}");
            Check("award: segunda vez no escribe", saves == 1, $"escrituras {saves}");

            var failedRun = NewRun();
            failedRun.Level = 2;
            failedRun.NormalCombatsWon = 1;
            int pointsBefore = profile.Points;
            string idBefore = profile.LastAwardedRunId;

            var failed = meta.Award(failedRun, profile, p => false);
            Check("award: escritura fallida no cobra",
                failed.Status == AwardStatus.Failed &&
                profile.Points == pointsBefore &&
                profile.LastAwardedRunId == idBefore,
                $"estado {failed.Status}, puntos {profile.Points}");

            var retry = meta.Award(failedRun, profile, p => true);
            Check("award: reintento cobra",
                retry.Status == AwardStatus.Awarded && profile.Points == pointsBefore + 19,
                $"estado {retry.Status}, puntos {profile.Points}");

            var storage = new InMemoryStorage { FailWrites = true };
            var store = new JsonProfileStore(storage);
            var diskProfile = ProfileDto.CreateDefault(StarterSet.CardIds);
            var diskRun = NewRun();
            diskRun.Level = 2;

            var diskFailed = meta.Award(diskRun, diskProfile,
                p => { store.Save(p); return true; });
            Check("award: fallo de disco se reporta",
                diskFailed.Status == AwardStatus.Failed && diskProfile.Points == 0,
                $"estado {diskFailed.Status}, puntos {diskProfile.Points}");

            storage.FailWrites = false;
            var diskRetry = meta.Award(diskRun, diskProfile,
                p => { store.Save(p); return true; });
            Check("award: reintento tras disco",
                diskRetry.Status == AwardStatus.Awarded && diskProfile.Points == 16,
                $"estado {diskRetry.Status}, puntos {diskProfile.Points}");
        }

        // ------------------------------------------------------------------
        // Perfil
        // ------------------------------------------------------------------

        private static void RunProfileRoundTripChecks(ItemRegistry items)
        {
            var storage = new InMemoryStorage();
            var store = new JsonProfileStore(storage);
            var profile = ProfileDto.CreateDefault(StarterSet.CardIds);

            string combatItemId = items.GetAll()[0].Id;
            profile.Points = 321;
            profile.DiscoveredCardIds.Add("card_angel");
            profile.OwnedCardIds.Add("card_america_chavez");
            profile.OwnedPerkIds.Add(PerkCatalog.Rally);
            profile.PerkCopyCounts[PerkCatalog.Rally] = 1;
            profile.OwnedPerkIds.Add("perk_vanguardia");
            profile.PerkCopyCounts["perk_vanguardia"] = 2;
            profile.OwnedPerkIds.Add("perk_desconocido");
            profile.PerkCopyCounts["perk_desconocido"] = 1;
            profile.ItemCounts[GlobalStoreSystem.ExtraLifeItemId] = 1;
            profile.ItemCounts[combatItemId] = 3;
            profile.LastAwardedRunId = "run-roundtrip";
            profile.VisibleBundleIds.Add("bundle_6_a");
            profile.OwnedBundleIds.Add("bundle_6_b");
            profile.RotationIndex = 2;

            store.Save(profile);
            var loaded = store.Load();

            Check("perfil: puntos", loaded.Points == 321, $"puntos {loaded.Points}");
            Check("perfil: cartas propias",
                loaded.OwnedCardIds.Contains("card_america_chavez"), "falta carta");
            Check("perfil: cartas descubiertas",
                loaded.DiscoveredCardIds.Contains("card_angel"), "falta descubierta");
            Check("perfil: perks propios",
                loaded.OwnedPerkIds.Contains(PerkCatalog.Rally), "falta perk");
            Check("perfil: copias de perk",
                loaded.PerkCopyCounts.TryGetValue(PerkCatalog.Rally, out int copies) && copies == 1,
                "copias incorrectas");
            Check("perfil: migracion elimina retirados",
                !loaded.OwnedPerkIds.Contains("perk_vanguardia") &&
                !loaded.PerkCopyCounts.ContainsKey("perk_vanguardia"),
                "quedo un retirado");
            Check("perfil: migracion elimina desconocidos",
                !loaded.OwnedPerkIds.Contains("perk_desconocido") &&
                !loaded.PerkCopyCounts.ContainsKey("perk_desconocido"),
                "quedo un desconocido");
            Check("perfil: contadores de objeto",
                loaded.ItemCounts.TryGetValue(GlobalStoreSystem.ExtraLifeItemId, out int life) &&
                life == 1, "item incorrecto");
            Check("perfil: inventario de combate no vacio",
                loaded.ItemCounts.TryGetValue(combatItemId, out int combatItems) &&
                combatItems == 3, "inventario incorrecto");
            Check("perfil: ultima run cobrada",
                loaded.LastAwardedRunId == "run-roundtrip", loaded.LastAwardedRunId);
            Check("perfil: rotacion de lotes",
                loaded.VisibleBundleIds.Contains("bundle_6_a") &&
                loaded.OwnedBundleIds.Contains("bundle_6_b") &&
                loaded.RotationIndex == 2,
                "rotacion incorrecta");
            Check("perfil: los 28 perks presentes",
                PerkCatalog.AllIds.All(id => loaded.OwnedPerkIds.Contains(id)),
                "faltan perks");
        }

        // ------------------------------------------------------------------
        // Particion y tienda
        // ------------------------------------------------------------------

        private static void RunPartitionChecks(CardRegistry cards)
        {
            var catalog = GlobalStoreCatalog.Build(cards, StarterSet.CardIds);

            Check("particion: 18 cartas directas",
                catalog.DirectCardIds.Count == 18, $"hay {catalog.DirectCardIds.Count}");
            Check("particion: 26 lotes",
                catalog.Bundles.Count == 26, $"hay {catalog.Bundles.Count}");

            int four = catalog.Bundles.Count(b => b.Size == 4);
            int five = catalog.Bundles.Count(b => b.Size == 5);
            int six = catalog.Bundles.Count(b => b.Size == 6);
            int seven = catalog.Bundles.Count(b => b.Size == 7);
            int eight = catalog.Bundles.Count(b => b.Size == 8);
            Check("particion: tamanos 5x6, 20x7, 1x8",
                four == 0 && five == 0 && six == 5 && seven == 20 && eight == 1,
                $"{four}/{five}/{six}/{seven}/{eight}");

            int total = catalog.Bundles.Sum(b => b.Size);
            Check("particion: 178 cartas en lotes", total == 178, $"hay {total}");

            var assigned = new HashSet<string>(StarterSet.CardIds, StringComparer.Ordinal);
            foreach (string cardId in catalog.DirectCardIds)
            {
                Check($"particion: directa {cardId} existe",
                    cards.Get(cardId) != null, "no registrada");
                Check($"particion: directa {cardId} sin solape",
                    assigned.Add(cardId), "duplicada");
            }

            foreach (var bundle in catalog.Bundles)
            {
                Check($"particion: {bundle.Id} con precio",
                    bundle.Price == GlobalStoreCatalog.PriceForSize(bundle.Size),
                    $"precio {bundle.Price}");

                foreach (string cardId in bundle.CardIds)
                {
                    var card = cards.Get(cardId);
                    Check($"particion: {bundle.Id} carta {cardId} existe",
                        card != null && !card.IsToken, "no registrada o token");
                    Check($"particion: {bundle.Id} carta {cardId} sin solape",
                        assigned.Add(cardId), "duplicada");
                }
            }

            var draftable = cards.GetDraftable();
            Check("particion: todas las draftables asignadas",
                assigned.Count == draftable.Count &&
                draftable.All(card => assigned.Contains(card.Id)),
                $"asignadas {assigned.Count}, draftables {draftable.Count}");
        }

        private static void RunStoreChecks(CardRegistry cards, PerkRegistry perks)
        {
            var store = new GlobalStoreSystem(cards, perks);
            var storage = new InMemoryStorage();
            var profileStore = new JsonProfileStore(storage);
            var profile = ProfileDto.CreateDefault(StarterSet.CardIds);
            Func<ProfileDto, bool> save = p => { profileStore.Save(p); return true; };

            var entries = store.GetEntries(profile);
            var extraLife = entries.FirstOrDefault(e => e.Kind == GlobalStoreEntryKind.ExtraLife);
            var directEntry = entries.FirstOrDefault(e => e.Kind == GlobalStoreEntryKind.DirectCard);
            var bundleEntry = entries.FirstOrDefault(e => e.Kind == GlobalStoreEntryKind.Bundle);

            Check("tienda: entrada de vida extra", extraLife != null, "no existe");
            Check("tienda: sin entrada de perk",
                !entries.Any(e => e.Id.StartsWith("perk", StringComparison.Ordinal)),
                "sigue vendiendo perks");
            Check("tienda: entrada de carta directa", directEntry != null, "no existe");
            Check("tienda: tres lotes visibles", bundleEntry != null, "no existe");
            Check("tienda: item reservado a 60",
                entries.Any(e => e.Kind == GlobalStoreEntryKind.Item && e.Price == 60),
                "item ausente");

            profile.Points = 0;
            Check("tienda: no compra sin puntos",
                !store.CanPurchase(profile, extraLife), "compro sin puntos");

            profile.Points = 1000;
            var life = store.Purchase(profile, extraLife, save);
            Check("tienda: compra vida extra",
                life.Success && profile.Points == 910 &&
                profile.ItemCounts[GlobalStoreSystem.ExtraLifeItemId] == 1,
                life.Error);

            Check("tienda: vida extra no se repite",
                !store.CanPurchase(profile, extraLife), "se puede repetir");

            var cardId = directEntry.CardId;
            var card = store.Purchase(profile, directEntry, save);
            Check("tienda: compra carta directa",
                card.Success && profile.Points == 760 &&
                profile.OwnedCardIds.Contains(cardId),
                card.Error);

            var bundleId = bundleEntry.BundleId;
            var bundle = store.Catalog.GetBundle(bundleId);
            var purchase = store.Purchase(profile, bundleEntry, save);
            Check("tienda: compra lote",
                purchase.Success && profile.Points == 760 - bundle.Price,
                purchase.Error);
            Check("tienda: lote marca cartas",
                bundle.CardIds.All(id => profile.OwnedCardIds.Contains(id)),
                "faltan cartas");
            Check("tienda: lote marcado como propio",
                profile.OwnedBundleIds.Contains(bundleId), "no marcado");
            Check("tienda: lote comprado se reemplaza",
                !profile.VisibleBundleIds.Contains(bundleId) &&
                profile.VisibleBundleIds.Count == 3,
                string.Join(",", profile.VisibleBundleIds));

            // Fallo de escritura: la compra no debe quedar a medias.
            var failingStorage = new InMemoryStorage { FailWrites = true };
            var failingStore = new JsonProfileStore(failingStorage);
            var failingProfile = ProfileDto.CreateDefault(StarterSet.CardIds);
            failingProfile.Points = 1000;
            var failingEntries = store.GetEntries(failingProfile);
            var failingTarget = failingEntries.First(
                e => e.Kind == GlobalStoreEntryKind.DirectCard &&
                     !failingProfile.OwnedCardIds.Contains(e.CardId));

            var failed = store.Purchase(failingProfile, failingTarget,
                p => { failingStore.Save(p); return true; });
            Check("tienda: escritura fallida revierte",
                !failed.Success && failingProfile.Points == 1000 &&
                !failingProfile.OwnedCardIds.Contains(failingTarget.CardId),
                failed.Error);

            // Rotacion de run: el indice avanza y no vuelven lotes propios.
            int rotationBefore = profile.RotationIndex;
            store.AdvanceRotation(profile);
            Check("tienda: rotacion avanza",
                profile.RotationIndex == (rotationBefore + 1) % store.BundleCount,
                $"indice {profile.RotationIndex}");
            Check("tienda: lotes propios no visibles",
                !profile.VisibleBundleIds.Any(id => profile.OwnedBundleIds.Contains(id)),
                string.Join(",", profile.VisibleBundleIds));

            // Vida extra del perfil a la run nueva.
            var run = new RunState();
            run.StartNewRun();
            profile.ItemCounts[GlobalStoreSystem.ExtraLifeItemId] = 1;
            store.PrepareNewRun(profile, run);
            Check("tienda: vida extra pasa a la run",
                run.ExtraLifeCharges == 1 &&
                profile.ItemCounts[GlobalStoreSystem.ExtraLifeItemId] == 0,
                $"cargas {run.ExtraLifeCharges}");
        }

        // ------------------------------------------------------------------
        // Coleccion
        // ------------------------------------------------------------------

        private static void RunCollectionChecks(CardRegistry cards, PerkRegistry perks)
        {
            var store = new GlobalStoreSystem(cards, perks);
            var collection = new CollectionSystem(cards, perks, store);
            var profile = ProfileDto.CreateDefault(StarterSet.CardIds);

            string discovered = cards.GetDraftable()
                .First(card => !profile.OwnedCardIds.Contains(card.Id) &&
                               !store.Catalog.IsDirect(card.Id)).Id;
            profile.DiscoveredCardIds.Add(discovered);

            var run = new RunState();
            run.StartNewRun();
            string deckCardId = profile.OwnedCardIds.First();
            run.Deck.Add(new CardInstance(cards.Get(deckCardId)));

            var entries = collection.BuildCards(profile, run);
            Check("coleccion: 92 cartas",
                entries.Count == GlobalStoreCatalog.ExpectedDraftable,
                $"hay {entries.Count}");

            var owned = entries.First(e => e.Card.Id == deckCardId);
            Check("coleccion: propia", owned.State == CollectionCardState.Owned, owned.State.ToString());
            Check("coleccion: overlay en run deck", owned.InRunDeck, "no marcada");

            var discoveredEntry = entries.First(e => e.Card.Id == discovered);
            Check("coleccion: descubierta",
                discoveredEntry.State == CollectionCardState.Discovered,
                discoveredEntry.State.ToString());
            Check("coleccion: descubierta con obtencion",
                !string.IsNullOrWhiteSpace(discoveredEntry.ObtainPath),
                "sin ruta");

            string directCard = store.Catalog.DirectCardIds
                .First(id => !profile.OwnedCardIds.Contains(id) &&
                             !profile.DiscoveredCardIds.Contains(id));
            var directEntry = entries.First(e => e.Card.Id == directCard);
            Check("coleccion: en tienda",
                directEntry.State == CollectionCardState.InStore,
                directEntry.State.ToString());

            var hidden = entries.FirstOrDefault(e => e.State == CollectionCardState.Hidden);
            Check("coleccion: hay ocultas", hidden != null, "ninguna");
            if (hidden != null)
            {
                Check("coleccion: oculta sin nombre revelado",
                    hidden.ObtainPath == "DESCUBRELA EN LA PARTIDA",
                    hidden.ObtainPath);
            }

            int discoveredBefore = profile.DiscoveredCardIds.Count;
            collection.BuildCards(profile, run);
            Check("coleccion: abrir no descubre",
                profile.DiscoveredCardIds.Count == discoveredBefore,
                "cambio el perfil");

            var filter = new CollectionFilter
            {
                State = CollectionCardState.Owned,
                Theme = DraftTheme.Control,
                Cost = 6
            };
            var filtered = entries.Where(e => CollectionSystem.Matches(e, filter)).ToList();
            Check("coleccion: filtros combinados",
                filtered.Count > 0 &&
                filtered.All(e => e.State == CollectionCardState.Owned &&
                                  e.Card.DraftTheme == DraftTheme.Control &&
                                  e.Card.Cost == 6),
                $"resultados {filtered.Count}");

            filter.Clear();
            Check("coleccion: limpiar filtros",
                entries.Count(e => CollectionSystem.Matches(e, filter)) == entries.Count,
                "no restaura");

            var perkEntries = collection.BuildPerks(profile, run);
            Check("coleccion: 28 perks", perkEntries.Count == 28,
                $"hay {perkEntries.Count}");
            Check("coleccion: los 28 estan propios",
                perkEntries.All(p => p.Owned), "hay uno no propio");
            Check("coleccion: requisito visible",
                perkEntries.First(p => p.Perk.Id == PerkCatalog.Amplify)
                    .CompatibilityText.Contains("CONTINUO"),
                "sin requisito");
            Check("coleccion: perk adjunto contado",
                perkEntries.First(p => p.Perk.Id == PerkCatalog.Rally)
                    .AttachedCopies == 0,
                "copias adjuntas incorrectas");
        }

        // ------------------------------------------------------------------
        // Compatibilidad y level-up
        // ------------------------------------------------------------------

        private static void RunLevelUpChecks(CardRegistry cards, PerkRegistry perks,
            EffectRegistry effects)
        {
            bool sawAmplify = false;
            bool sawOvertime = false;
            bool sawIncompatible = false;

            for (int seed = 1; seed <= 40; seed++)
            {
                var run = new RunState();
                run.StartNewRun();
                run.Deck.Add(new CardInstance(cards.Get("card_iron_man")));
                run.Deck.Add(new CardInstance(cards.Get("card_hawkeye")));
                run.Deck.Add(new CardInstance(cards.Get("card_hulk")));

                var levelUp = new LevelUpState(2);
                var system = new LevelUpSystem(cards, perks, new RandomService(seed),
                    new PowerResolver(perks), null, effects);

                system.EnsureOffers(run, levelUp);

                foreach (string offer in levelUp.PerkOffers)
                {
                    if (offer == PerkCatalog.Amplify) sawAmplify = true;
                    if (offer == PerkCatalog.Overtime) sawOvertime = true;
                    if (!system.HasCompatibleHost(run, offer))
                        sawIncompatible = true;
                }
            }

            Check("level-up: ofrece Amplify con host Continuo", sawAmplify,
                "nunca ofrecio Amplify");
            Check("level-up: no ofrece Overtime sin host EndOfTurn",
                !sawOvertime, "ofrecio Overtime");
            Check("level-up: no ofrece perks sin anfitrion",
                !sawIncompatible, "ofrecio un perk incompatible");

            // Perks meta: el anfitrion debe tener el disparador requerido.
            var hulk = new CardInstance(cards.Get("card_hulk"));
            var ironMan = new CardInstance(cards.Get("card_iron_man"));
            var hawkeye = new CardInstance(cards.Get("card_hawkeye"));
            var compatibility = new LevelUpSystem(cards, perks,
                new RandomService(1), new PowerResolver(perks), null, effects);

            Check("compatibilidad: Amplify exige Continuo",
                !compatibility.CanAttach(PerkCatalog.Amplify, hulk) &&
                compatibility.CanAttach(PerkCatalog.Amplify, ironMan),
                "predicado incorrecto");
            Check("compatibilidad: Encore exige Al revelarse",
                !compatibility.CanAttach(PerkCatalog.Encore, hulk) &&
                compatibility.CanAttach(PerkCatalog.Encore, hawkeye),
                "predicado incorrecto");
            Check("compatibilidad: Overtime sin hosts disponibles",
                !compatibility.CanAttach(PerkCatalog.Overtime, hulk) &&
                !compatibility.CanAttach(PerkCatalog.Overtime, hawkeye),
                "predicado incorrecto");
            Check("compatibilidad: Reinforce acepta cualquier carta",
                compatibility.CanAttach(PerkCatalog.Reinforce, hulk),
                "rechazo una carta valida");
            Check("compatibilidad: razon de rechazo",
                !string.IsNullOrWhiteSpace(compatibility.IncompatibleReason(
                    PerkCatalog.Amplify, hulk)), "sin razon");

            // Una carta con el disparador requerido vuelve elegible al perk.
            var ongoingRun = new RunState();
            ongoingRun.StartNewRun();
            ongoingRun.Deck.Add(new CardInstance(cards.Get("card_hulk")));
            var amplifyState = new LevelUpState(2);
            var amplifySystem = new LevelUpSystem(cards, perks, new RandomService(3),
                new PowerResolver(perks), null, effects);
            amplifySystem.EnsureOffers(ongoingRun, amplifyState);
            Check("compatibilidad: sin host no ofrece Amplify",
                !amplifyState.PerkOffers.Contains(PerkCatalog.Amplify),
                "lo ofrecio sin host");

            ongoingRun.Deck.Add(new CardInstance(cards.Get("card_iron_man")));
            Check("compatibilidad: el nuevo host habilita el perk",
                amplifySystem.HasCompatibleHost(ongoingRun, PerkCatalog.Amplify),
                "sigue inelegible");

            var dynamicState = new LevelUpState(3);
            amplifySystem.EnsureOffers(ongoingRun, dynamicState);
            Check("level-up: nueva oferta solo con hosts compatibles",
                dynamicState.PerkOffers.All(offer =>
                    amplifySystem.HasCompatibleHost(ongoingRun, offer)),
                "ofrecio un perk sin host");

            // Limite de dos copias y reentrada sin rebarajar.
            var limitRun = new RunState();
            limitRun.StartNewRun();
            var host = new CardInstance(cards.Get("card_hulk"));
            host.AttachedPerks.Add(PerkCatalog.Rally);
            host.AttachedPerks.Add(PerkCatalog.Rally);
            limitRun.Deck.Add(host);

            var limitState = new LevelUpState(2) { OffersGenerated = true };
            limitState.PerkOffers.Add(PerkCatalog.Rally);
            var limitSystem = new LevelUpSystem(cards, perks, new RandomService(1),
                new PowerResolver(perks), null, effects);

            Check("level-up: limite de 2 copias",
                !limitSystem.ApplyPerk(limitRun, limitState, PerkCatalog.Rally, host),
                "aplico una tercera copia");
            Check("level-up: contador de copias",
                limitSystem.CountPerkCopies(limitRun, PerkCatalog.Rally) == 2,
                limitSystem.CountPerkCopies(limitRun, PerkCatalog.Rally).ToString());

            var resumeRun = new RunState();
            resumeRun.StartNewRun();
            resumeRun.Deck.Add(new CardInstance(cards.Get("card_hulk")));
            var resumeState = new LevelUpState(2);
            var resumeSystem = new LevelUpSystem(cards, perks, new RandomService(9),
                new PowerResolver(perks), null, effects);
            resumeSystem.EnsureOffers(resumeRun, resumeState);
            var snapshot = new List<string>(resumeState.PerkOffers);
            resumeSystem.EnsureOffers(resumeRun, resumeState);
            Check("level-up: reentrada no rebaraja",
                snapshot.SequenceEqual(resumeState.PerkOffers),
                "cambiaron las ofertas");

            var emptyRun = new RunState();
            emptyRun.StartNewRun();
            var emptyState = new LevelUpState(2);
            var emptySystem = new LevelUpSystem(cards, perks, new RandomService(1),
                new PowerResolver(perks), null, effects);
            Check("level-up: rama de perk no disponible sin mazo",
                !emptySystem.CanUsePerkBranch(emptyRun, emptyState),
                "la habilito");
        }


        // ------------------------------------------------------------------
        // Habilidades de los 28 perks
        // ------------------------------------------------------------------

        private static void RunSeries2Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var series2 = new (string Id, int Cost, int Power)[]
            {
                ("card_agent_13", 1, 3), ("card_bucky_barnes", 2, 2),
                ("card_cloak", 2, 4), ("card_ebony_maw", 1, 7),
                ("card_hobgoblin", 5, -8), ("card_iceman", 1, 2),
                ("card_jubilee", 4, 1), ("card_killmonger", 3, 3),
                ("card_leech", 4, 5), ("card_morbius", 2, 0),
                ("card_nakia", 3, 4), ("card_okoye", 2, 3),
                ("card_rhino", 2, 5), ("card_sabretooth", 3, 5),
                ("card_sandman", 5, 8), ("card_scorpion", 2, 3),
                ("card_shang_chi", 4, 3), ("card_storm", 3, 1),
                ("card_sunspot", 1, 0), ("card_swarm", 1, 3),
                ("card_the_collector", 2, 3), ("card_the_infinaut", 6, 20),
                ("card_vision", 5, 9), ("card_vulture", 1, 3),
                ("card_warpath", 4, 5)
            };

            var collectionLevel = new (string Id, int Cost, int Power)[]
            {
                ("card_jessica_jones", 4, 5), ("card_ka_zar", 4, 6),
                ("card_mister_fantastic", 3, 1), ("card_nightcrawler", 1, 3),
                ("card_odin", 6, 10), ("card_spectrum", 6, 7),
                ("card_white_tiger", 5, 1), ("card_wolfsbane", 3, 2)
            };

            foreach (var entry in series2)
            {
                var card = cards.Get(entry.Id);
                Check($"series2: {entry.Id} registrada",
                    card != null && card.Cost == entry.Cost &&
                    card.BasePower == entry.Power &&
                    card.Series == CardSeries.Series2,
                    card == null ? "ausente" : $"{card.Cost}/{card.BasePower}");
                Check($"series2: {entry.Id} con descripcion",
                    card != null && !string.IsNullOrWhiteSpace(card.Description),
                    "descripcion vacia");
            }

            foreach (var entry in collectionLevel)
            {
                var card = cards.Get(entry.Id);
                Check($"cl: {entry.Id} registrada",
                    card != null && card.Cost == entry.Cost &&
                    card.BasePower == entry.Power &&
                    card.Series == CardSeries.CollectionLevel,
                    card == null ? "ausente" : $"{card.Cost}/{card.BasePower}");
            }

            Check("series2: 25 cartas Series 2",
                cards.GetDraftable().Count(c => c.Series == CardSeries.Series2) == 25,
                cards.GetDraftable().Count(c => c.Series == CardSeries.Series2).ToString());
            Check("series2: 21 starters",
                StarterSet.CardIds.Length == 21 &&
                StarterSet.CardIds.All(id => cards.Get(id) != null),
                StarterSet.CardIds.Length.ToString());
            Check("series2: 8 CL en starters",
                collectionLevel.All(entry => StarterSet.CardIds.Contains(entry.Id)),
                "faltan CL");
            Check("series2: tiger 0/10",
                cards.Get("token_tiger")?.Cost == 0 &&
                cards.Get("token_tiger")?.BasePower == 10 &&
                !cards.GetDraftable().Any(c => c.Id == "token_tiger"),
                "token invalido");
            Check("series2: winter soldier 0/8",
                cards.Get("token_winter_soldier")?.Cost == 0 &&
                cards.Get("token_winter_soldier")?.BasePower == 8 &&
                !cards.GetDraftable().Any(c => c.Id == "token_winter_soldier"),
                "token invalido");

            // Wolfsbane: +2 por cada otra carta aliada en la zona.
            var wolf = NewPerkFixture(cards, effects, zones, 201);
            var wolfsbane = Place(cards, wolf.Board.Zones[0], "card_wolfsbane", 1);
            Place(cards, wolf.Board.Zones[0], "card_hulk", 2);
            Place(cards, wolf.Board.Zones[0], "card_cyclops", 3);
            wolf.Effects.ResolveEndOfTurn(wolf.Board,
                new List<CardInstance> { wolfsbane });
            Check("series2: Wolfsbane +4", wolfsbane.BonusPower == 4,
                wolfsbane.BonusPower.ToString());

            // Spectrum: +2 a cada Ongoing aliado.
            var spectrumFixture = NewPerkFixture(cards, effects, zones, 202);
            var spectrum = Place(cards, spectrumFixture.Board.Zones[0],
                "card_spectrum", 1);
            var kaZar = Place(cards, spectrumFixture.Board.Zones[0],
                "card_ka_zar", 2);
            spectrumFixture.Effects.ResolveEndOfTurn(spectrumFixture.Board,
                new List<CardInstance> { spectrum });
            Check("series2: Spectrum +2 a Ongoing", kaZar.BonusPower == 2,
                kaZar.BonusPower.ToString());

            // White Tiger: un Tigre en otra locacion.
            var tigerFixture = NewPerkFixture(cards, effects, zones, 203);
            var whiteTiger = Place(cards, tigerFixture.Board.Zones[0],
                "card_white_tiger", 1);
            tigerFixture.Effects.ResolveEndOfTurn(tigerFixture.Board,
                new List<CardInstance> { whiteTiger });
            Check("series2: White Tiger agrega Tigre",
                tigerFixture.Board.Zones.Sum(zone => zone.PlayerCards
                    .Count(card => card.Definition.Id == "token_tiger")) == 1,
                "sin Tigre");

            // Agent 13: una carta aleatoria a la mano.
            var agentFixture = NewPerkFixture(cards, effects, zones, 204);
            var agent = Place(cards, agentFixture.Board.Zones[0],
                "card_agent_13", 1);
            int agentHand = agentFixture.Board.PlayerHand.Count;
            agentFixture.Effects.ResolveEndOfTurn(agentFixture.Board,
                new List<CardInstance> { agent });
            Check("series2: Agent 13 agrega carta",
                agentFixture.Board.PlayerHand.Count == agentHand + 1,
                agentFixture.Board.PlayerHand.Count.ToString());

            // Jubilee: carta superior del mazo a la locacion.
            var jubileeFixture = NewPerkFixture(cards, effects, zones, 205);
            var jubilee = Place(cards, jubileeFixture.Board.Zones[0],
                "card_jubilee", 1);
            var topCard = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Deck };
            jubileeFixture.Board.PlayerDeck.Add(topCard);
            jubileeFixture.Effects.ResolveEndOfTurn(jubileeFixture.Board,
                new List<CardInstance> { jubilee });
            Check("series2: Jubilee mueve y revela",
                jubileeFixture.Board.Zones[0].PlayerCards.Contains(topCard) &&
                topCard.HasRevealed &&
                !jubileeFixture.Board.PlayerDeck.Contains(topCard),
                "no movio");

            // Iceman y Scorpion sobre la mano rival.
            var iceFixture = NewPerkFixture(cards, effects, zones, 206);
            var iceman = Place(cards, iceFixture.Board.Zones[0],
                "card_iceman", 1);
            var iceTarget = new CardInstance(cards.Get("card_cyclops"))
                { Location = CardLocation.Hand };
            iceFixture.Board.AIHand.Add(iceTarget);
            int iceBefore = iceTarget.EffectiveCost;
            iceFixture.Effects.ResolveEndOfTurn(iceFixture.Board,
                new List<CardInstance> { iceman });
            Check("series2: Iceman sube coste",
                iceTarget.EffectiveCost == Math.Min(6, iceBefore + 1),
                iceTarget.EffectiveCost.ToString());

            var scorpionFixture = NewPerkFixture(cards, effects, zones, 207);
            var scorpion = Place(cards, scorpionFixture.Board.Zones[0],
                "card_scorpion", 1);
            var scorpionTarget = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            scorpionFixture.Board.AIHand.Add(scorpionTarget);
            scorpionFixture.Effects.ResolveEndOfTurn(scorpionFixture.Board,
                new List<CardInstance> { scorpion });
            Check("series2: Scorpion -1 a mano rival",
                scorpionTarget.BonusPower == -1,
                scorpionTarget.BonusPower.ToString());

            // Nakia y Okoye.
            var buffFixture = NewPerkFixture(cards, effects, zones, 208);
            var nakia = Place(cards, buffFixture.Board.Zones[0],
                "card_nakia", 1);
            var handCard = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            buffFixture.Board.PlayerHand.Add(handCard);
            buffFixture.Effects.ResolveEndOfTurn(buffFixture.Board,
                new List<CardInstance> { nakia });
            Check("series2: Nakia +1 a mano", handCard.BonusPower == 1,
                handCard.BonusPower.ToString());

            var okoyeFixture = NewPerkFixture(cards, effects, zones, 209);
            var okoye = Place(cards, okoyeFixture.Board.Zones[0],
                "card_okoye", 1);
            var deckCard = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Deck };
            okoyeFixture.Board.PlayerDeck.Add(deckCard);
            okoyeFixture.Effects.ResolveEndOfTurn(okoyeFixture.Board,
                new List<CardInstance> { okoye });
            Check("series2: Okoye +1 a mazo", deckCard.BonusPower == 1,
                deckCard.BonusPower.ToString());

            // Rhino: Roca.
            var rhinoFixture = NewPerkFixture(cards, effects, zones, 210);
            var rhino = Place(cards, rhinoFixture.Board.Zones[0],
                "card_rhino", 1);
            rhinoFixture.Effects.ResolveEndOfTurn(rhinoFixture.Board,
                new List<CardInstance> { rhino });
            Check("series2: Rhino agrega Roca",
                rhinoFixture.Board.Zones[0].PlayerCards
                    .Any(card => card.Definition.Id == "token_rock"),
                "sin Roca");

            // Leech: apaga texto impreso de 6+.
            var leechFixture = NewPerkFixture(cards, effects, zones, 211);
            var leech = Place(cards, leechFixture.Board.Zones[0],
                "card_leech", 1);
            var bigTarget = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            var smallTarget = new CardInstance(cards.Get("card_cyclops"))
                { Location = CardLocation.Hand };
            leechFixture.Board.AIHand.Add(bigTarget);
            leechFixture.Board.AIHand.Add(smallTarget);
            leechFixture.Effects.ResolveEndOfTurn(leechFixture.Board,
                new List<CardInstance> { leech });
            Check("series2: Leech apaga 6+",
                bigTarget.PrintedAbilitiesDisabled &&
                !smallTarget.PrintedAbilitiesDisabled,
                "estado incorrecto");

            // Killmonger y Shang-Chi.
            var killFixture = NewPerkFixture(cards, effects, zones, 212);
            var killmonger = Place(cards, killFixture.Board.Zones[0],
                "card_killmonger", 1);
            var ownOne = Place(cards, killFixture.Board.Zones[0],
                "card_elektra", 2);
            var enemyOne = PlaceEnemy(cards, killFixture.Board.Zones[1],
                "card_iceman", 3);
            var enemyHigh = PlaceEnemy(cards, killFixture.Board.Zones[1],
                "card_hulk", 4);
            killFixture.Effects.ResolveEndOfTurn(killFixture.Board,
                new List<CardInstance> { killmonger });
            Check("series2: Killmonger destruye coste 1",
                !killFixture.Board.Zones[0].PlayerCards.Contains(ownOne) &&
                !killFixture.Board.Zones[1].AICards.Contains(enemyOne) &&
                killFixture.Board.Zones[1].AICards.Contains(enemyHigh),
                "objetivos incorrectos");

            var shangFixture = NewPerkFixture(cards, effects, zones, 213);
            var shang = Place(cards, shangFixture.Board.Zones[0],
                "card_shang_chi", 1);
            var infinaut = PlaceEnemy(cards, shangFixture.Board.Zones[0],
                "card_the_infinaut", 2);
            shangFixture.Effects.ResolveEndOfTurn(shangFixture.Board,
                new List<CardInstance> { shang });
            Check("series2: Shang-Chi destruye 10+",
                !shangFixture.Board.Zones[0].AICards.Contains(infinaut),
                "no destruyo");

            // Ka-Zar, Mister Fantastic, Morbius y Warpath.
            var ongoingFixture = NewPerkFixture(cards, effects, zones, 214);
            Place(cards, ongoingFixture.Board.Zones[0], "card_ka_zar", 1);
            var oneCost = Place(cards, ongoingFixture.Board.Zones[0],
                "card_iceman", 2);
            Place(cards, ongoingFixture.Board.Zones[1],
                "card_mister_fantastic", 3);
            ongoingFixture.Actions.RefreshPowers();
            Check("series2: Ka-Zar +1 a coste 1", oneCost.CurrentPower == 3,
                oneCost.CurrentPower.ToString());
            Check("series2: Mister Fantastic +3 adyacente",
                ongoingFixture.Board.Zones[0].GetPlayerPower() >= 3 &&
                ongoingFixture.Board.Zones[2].GetPlayerPower() >= 3,
                $"{ongoingFixture.Board.Zones[0].GetPlayerPower()}/" +
                $"{ongoingFixture.Board.Zones[2].GetPlayerPower()}");

            var morbiusFixture = NewPerkFixture(cards, effects, zones, 215);
            var morbius = Place(cards, morbiusFixture.Board.Zones[0],
                "card_morbius", 1);
            var discardTarget = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            morbiusFixture.Board.PlayerHand.Add(discardTarget);
            morbiusFixture.Actions.DiscardCard(discardTarget, BoardSide.Player);
            morbiusFixture.Actions.RefreshPowers();
            Check("series2: Morbius +2 por descarte",
                morbiusFixture.Board.GetDiscardCount(BoardSide.Player) == 1 &&
                morbius.CurrentPower == 2,
                morbius.CurrentPower.ToString());

            var warpathFixture = NewPerkFixture(cards, effects, zones, 216);
            var warpath = Place(cards, warpathFixture.Board.Zones[0],
                "card_warpath", 1);
            warpathFixture.Actions.RefreshPowers();
            Check("series2: Warpath +5 con locacion vacia",
                warpath.CurrentPower == 10, warpath.CurrentPower.ToString());

            // Vulture y Sunspot.
            var vultureFixture = NewPerkFixture(cards, effects, zones, 217);
            var vulture = Place(cards, vultureFixture.Board.Zones[0],
                "card_vulture", 1);
            vultureFixture.Actions.MoveCard(vulture, BoardSide.Player, 0, 1);
            Check("series2: Vulture +4 al moverse",
                vulture.BonusPower == 4, vulture.BonusPower.ToString());

            var sunspotFixture = NewPerkFixture(cards, effects, zones, 218);
            var sunspot = Place(cards, sunspotFixture.Board.Zones[0],
                "card_sunspot", 1);
            sunspotFixture.Board.CurrentTurn = 1;
            sunspotFixture.Board.CurrentEnergy = 3;
            sunspotFixture.Effects.ResolveEndOfTurn(sunspotFixture.Board,
                new List<CardInstance>());
            Check("series2: Sunspot +3 con 3 energia",
                sunspot.BonusPower == 3, sunspot.BonusPower.ToString());

            // Jessica Jones: no jugar el siguiente turno.
            var jessicaFixture = NewPerkFixture(cards, effects, zones, 219);
            var jessica = Place(cards, jessicaFixture.Board.Zones[0],
                "card_jessica_jones", 1);
            jessica.PlayedTurn = 1;
            jessicaFixture.Board.CurrentTurn = 1;
            jessicaFixture.Effects.ResolveEndOfTurn(jessicaFixture.Board,
                new List<CardInstance> { jessica });
            jessicaFixture.Board.CurrentTurn = 2;
            jessicaFixture.Effects.ResolveEndOfTurn(jessicaFixture.Board,
                new List<CardInstance>());
            Check("series2: Jessica +5 sin juego",
                jessica.BonusPower == 5, jessica.BonusPower.ToString());

            // Bucky, Sabretooth, Swarm y The Collector.
            var buckyFixture = NewPerkFixture(cards, effects, zones, 220);
            var bucky = Place(cards, buckyFixture.Board.Zones[0],
                "card_bucky_barnes", 1);
            buckyFixture.Actions.DestroyCard(bucky, BoardSide.Player);
            Check("series2: Bucky -> Soldado del Invierno",
                buckyFixture.Board.Zones[0].PlayerCards
                    .Any(card => card.Definition.Id == "token_winter_soldier") &&
                !buckyFixture.Board.PlayerPiles.Get(PileKind.Destroyed)
                    .Contains(bucky),
                "reemplazo incorrecto");

            var sabreFixture = NewPerkFixture(cards, effects, zones, 221);
            var sabretooth = Place(cards, sabreFixture.Board.Zones[0],
                "card_sabretooth", 1);
            sabreFixture.Actions.DestroyCard(sabretooth, BoardSide.Player);
            Check("series2: Sabretooth vuelve a coste 0",
                sabreFixture.Board.PlayerHand.Contains(sabretooth) &&
                sabretooth.EffectiveCost == 0 &&
                !sabreFixture.Board.PlayerPiles.Get(PileKind.Destroyed)
                    .Contains(sabretooth),
                sabretooth.EffectiveCost.ToString());

            var swarmFixture = NewPerkFixture(cards, effects, zones, 222);
            var swarm = new CardInstance(cards.Get("card_swarm"))
                { Location = CardLocation.Hand };
            swarmFixture.Board.PlayerHand.Add(swarm);
            swarmFixture.Actions.DiscardCard(swarm, BoardSide.Player);
            var swarmCopies = swarmFixture.Board.PlayerHand
                .Where(card => card.Definition.Id == "card_swarm").ToList();
            Check("series2: Swarm dos copias a coste 0",
                swarmCopies.Count == 2 &&
                swarmCopies.All(card => card.EffectiveCost == 0) &&
                swarmFixture.Board.PlayerPiles.Get(PileKind.Discard)
                    .Contains(swarm),
                swarmCopies.Count.ToString());

            var collectorFixture = NewPerkFixture(cards, effects, zones, 223);
            var collector = Place(cards, collectorFixture.Board.Zones[0],
                "card_the_collector", 1);
            collectorFixture.Actions.AddToHand(
                new CardInstance(cards.Get("card_hulk")), BoardSide.Player);
            Check("series2: The Collector +1", collector.BonusPower == 1,
                collector.BonusPower.ToString());

            // Ebony Maw, Infinaut, Storm y Sandman.
            var mawFixture = NewPerkFixture(cards, effects, zones, 224);
            mawFixture.Board.CurrentTurn = 4;
            var maw = new CardInstance(cards.Get("card_ebony_maw"))
                { Location = CardLocation.Hand };
            mawFixture.Board.PlayerHand.Add(maw);
            Check("series2: Ebony Maw no jugable tras turno 3",
                !mawFixture.Effects.CanPlayCard(maw, mawFixture.Board,
                    BoardSide.Player), "se permite");

            var infinautFixture = NewPerkFixture(cards, effects, zones, 225);
            infinautFixture.Board.CurrentTurn = 3;
            var infinautCard = new CardInstance(cards.Get("card_the_infinaut"))
                { Location = CardLocation.Hand };
            infinautFixture.Board.PlayerHand.Add(infinautCard);
            infinautFixture.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.Player,
                Card = new CardInstance(cards.Get("card_hulk")),
                ZoneIndex = 0,
                Turn = 2
            });
            Check("series2: Infinaut bloqueado tras jugar",
                !infinautFixture.Effects.CanPlayCard(infinautCard,
                    infinautFixture.Board, BoardSide.Player),
                "se permite");
            infinautFixture.Board.PlayLog.Clear();
            Check("series2: Infinaut jugable sin jugada previa",
                infinautFixture.Effects.CanPlayCard(infinautCard,
                    infinautFixture.Board, BoardSide.Player),
                "se bloquea");

            var stormFixture = NewPerkFixture(cards, effects, zones, 226);
            stormFixture.Board.CurrentTurn = 1;
            var storm = Place(cards, stormFixture.Board.Zones[0],
                "card_storm", 1);
            storm.PlayedTurn = 1;
            stormFixture.Effects.ResolveEndOfTurn(stormFixture.Board,
                new List<CardInstance> { storm });
            Check("series2: Storm permite el turno de gracia",
                !stormFixture.Board.Zones[0].IsFlooded(2) &&
                stormFixture.Board.Zones[0].IsFlooded(3),
                "frontera incorrecta");
            stormFixture.Board.CurrentTurn = 3;
            Check("series2: Storm bloquea jugar en el turno 3",
                !stormFixture.Actions.CanPlayCardInZone(
                    new CardInstance(cards.Get("card_hulk")), BoardSide.Player,
                    stormFixture.Board.Zones[0]),
                "se permite");
            Check("series2: Storm permite agregar por efecto",
                stormFixture.Actions.AddCardToZone(
                    new CardInstance(cards.Get("card_hulk")), BoardSide.Player, 0)
                    .Success,
                "se bloquea");

            var sandmanFixture = NewPerkFixture(cards, effects, zones, 227);
            sandmanFixture.Board.CurrentTurn = 1;
            var sandman = Place(cards, sandmanFixture.Board.Zones[0],
                "card_sandman", 1);
            sandman.PlayedTurn = 1;
            sandmanFixture.Effects.ResolveEndOfTurn(sandmanFixture.Board,
                new List<CardInstance> { sandman });
            Check("series2: Sandman registra impuesto",
                sandmanFixture.Board.GetCostTaxForTurn(2) == 1,
                sandmanFixture.Board.GetCostTaxForTurn(2).ToString());
            sandmanFixture.Board.CurrentTurn = 2;
            sandmanFixture.Board.StartOfTurnResolvedTurn = -1;
            var taxedCard = new CardInstance(cards.Get("card_cyclops"))
                { Location = CardLocation.Hand };
            sandmanFixture.Board.PlayerHand.Add(taxedCard);
            sandmanFixture.Effects.ResolveStartOfTurn(sandmanFixture.Board);
            Check("series2: Sandman +1 al coste",
                taxedCard.TurnCostDelta == 1,
                taxedCard.TurnCostDelta.ToString());

            // Movimiento: Nightcrawler, Vision y Cloak.
            var nightFixture = NewPerkFixture(cards, effects, zones, 228);
            var night = Place(cards, nightFixture.Board.Zones[0],
                "card_nightcrawler", 1);
            night.PlayedTurn = 1;
            nightFixture.Board.CurrentTurn = 2;
            var nightFirst = nightFixture.Actions.MoveCardByPlayer(night,
                BoardSide.Player, 0, 1);
            var nightSecond = nightFixture.Actions.MoveCardByPlayer(night,
                BoardSide.Player, 1, 2);
            Check("series2: Nightcrawler una vez",
                nightFirst.Success && !nightSecond.Success &&
                night.MoveUsedThisCombat,
                nightSecond.Reason);

            var visionFixture = NewPerkFixture(cards, effects, zones, 229);
            var vision = Place(cards, visionFixture.Board.Zones[0],
                "card_vision", 1);
            vision.PlayedTurn = 1;
            visionFixture.Board.CurrentTurn = 2;
            var visionFirst = visionFixture.Actions.MoveCardByPlayer(vision,
                BoardSide.Player, 0, 1);
            var visionSecond = visionFixture.Actions.MoveCardByPlayer(vision,
                BoardSide.Player, 1, 2);
            visionFixture.Board.CurrentTurn = 3;
            var visionThird = visionFixture.Actions.MoveCardByPlayer(vision,
                BoardSide.Player, 2, 0);
            Check("series2: Vision una vez por turno",
                visionFirst.Success && !visionSecond.Success &&
                visionThird.Success,
                visionSecond.Reason);

            var cloakFixture = NewPerkFixture(cards, effects, zones, 230);
            cloakFixture.Board.CurrentTurn = 1;
            var cloak = Place(cards, cloakFixture.Board.Zones[1],
                "card_cloak", 1);
            cloak.PlayedTurn = 1;
            cloakFixture.Effects.ResolveEndOfTurn(cloakFixture.Board,
                new List<CardInstance> { cloak });
            Check("series2: Cloak abre ventana",
                cloakFixture.Board.HasMoveWindow(1, 2, BoardSide.Player),
                "sin ventana");
            cloakFixture.Board.CurrentTurn = 2;
            var oldCard = Place(cards, cloakFixture.Board.Zones[0],
                "card_hulk", 2);
            oldCard.PlayedTurn = 1;
            var cloakMove = cloakFixture.Actions.MoveCardByPlayer(oldCard,
                BoardSide.Player, 0, 1);
            Check("series2: Cloak mueve carta vieja", cloakMove.Success,
                cloakMove.Reason);

            // Odin y Hobgoblin.
            var odinFixture = NewPerkFixture(cards, effects, zones, 231);
            odinFixture.Board.CurrentTurn = 1;
            var tiger = Place(cards, odinFixture.Board.Zones[0],
                "card_white_tiger", 1);
            tiger.HasRevealed = true;
            var odin = Place(cards, odinFixture.Board.Zones[0],
                "card_odin", 2);
            odin.PlayedTurn = 1;
            odinFixture.Effects.ResolveEndOfTurn(odinFixture.Board,
                new List<CardInstance> { odin });
            Check("series2: Odin repite White Tiger",
                odinFixture.Board.Zones.Sum(zone => zone.PlayerCards
                    .Count(card => card.Definition.Id == "token_tiger")) == 1,
                "sin repeticion");

            var comboFixture = NewPerkFixture(cards, effects, zones, 236);
            comboFixture.Board.CurrentTurn = 1;
            var comboWt = Place(cards, comboFixture.Board.Zones[0],
                "card_white_tiger", 1);
            var comboJess = Place(cards, comboFixture.Board.Zones[0],
                "card_jessica_jones", 2);
            var comboOdin = Place(cards, comboFixture.Board.Zones[0],
                "card_odin", 3);
            comboFixture.Effects.ResolveEndOfTurn(comboFixture.Board,
                new List<CardInstance> { comboWt, comboJess, comboOdin });
            Check("series2: Odin + White Tiger + Jessica",
                comboFixture.Board.Zones.Sum(zone => zone.PlayerCards
                    .Count(card => card.Definition.Id == "token_tiger")) == 2 &&
                comboJess.PendingEffects.Count == 2,
                $"{comboJess.PendingEffects.Count} pendientes");

            var hobFixture = NewPerkFixture(cards, effects, zones, 232);
            hobFixture.Board.CurrentTurn = 1;
            var hob = Place(cards, hobFixture.Board.Zones[0],
                "card_hobgoblin", 1);
            hob.PlayedTurn = 1;
            hobFixture.Effects.ResolveEndOfTurn(hobFixture.Board,
                new List<CardInstance> { hob });
            Check("series2: Hobgoblin cambia de lado",
                !hobFixture.Board.Zones[0].PlayerCards.Contains(hob) &&
                hobFixture.Board.Zones[0].AICards.Contains(hob),
                "no cambio");

            // Migracion de perfil.
            var stale = ProfileDto.CreateDefault(new[] { "card_abomination" });
            stale.Normalize();
            Check("series2: migracion de starters",
                stale.OwnedCardIds.Contains("card_abomination") &&
                stale.OwnedCardIds.Contains("card_elektra") &&
                stale.OwnedCardIds.Contains("card_jessica_jones") &&
                stale.OwnedCardIds.Contains("card_odin"),
                "migracion incompleta");

            // Particion por series.
            var catalog = GlobalStoreCatalog.Build(cards, StarterSet.CardIds);
            Check("series2: lote legacy sin Series 2 ni Series 3",
                catalog.Bundles
                    .Where(bundle =>
                        !bundle.Id.StartsWith("bundle_s2_") &&
                        !bundle.Id.StartsWith("bundle_s3_"))
                    .All(bundle => bundle.CardIds.All(id =>
                        cards.Get(id)?.Series == CardSeries.Series1)),
                "mezcla de series");
            Check("series2: lote S2 separado",
                catalog.Bundles
                    .Where(bundle => bundle.Id.StartsWith("bundle_s2_"))
                    .All(bundle => bundle.CardIds.All(id =>
                        cards.Get(id)?.Series == CardSeries.Series2)),
                "mezcla de series");
            Check("series3: lote S3 separado",
                catalog.Bundles
                    .Where(bundle => bundle.Id.StartsWith("bundle_s3_"))
                    .All(bundle => bundle.CardIds.All(id =>
                        cards.Get(id)?.Series == CardSeries.Series3)),
                "mezcla de series");

            // Tope de coste y reset de combate.
            var capCard = new CardInstance(cards.Get("card_hulk"));
            capCard.TemporaryCostDelta = 3;
            Check("series2: tope de coste 6", capCard.EffectiveCost == 6,
                capCard.EffectiveCost.ToString());
            capCard.CombatCostOverride = 0;
            capCard.MoveUsedThisCombat = true;
            capCard.ResetForCombat();
            Check("series2: reset de estado de combate",
                capCard.CombatCostOverride == null &&
                !capCard.MoveUsedThisCombat &&
                capCard.EffectiveCost == 6,
                capCard.EffectiveCost.ToString());

            // Professor X: el lock total si bloquea adiciones por efecto.
            var professorFixture = NewPerkFixture(cards, effects, zones, 234);
            Place(cards, professorFixture.Board.Zones[0], "card_professor_x", 1);
            Check("series2: Professor X bloquea adiciones",
                professorFixture.Effects.IsZoneRemovalLocked(
                    professorFixture.Board.Zones[0]) &&
                !professorFixture.Actions.AddCardToZone(
                    new CardInstance(cards.Get("card_hulk")), BoardSide.Player, 0)
                    .Success,
                "no bloquea");

            // Ids legacy estables.
            Check("series2: ids legacy estables",
                catalog.Bundles.Any(bundle => bundle.Id == "bundle_6_a") &&
                catalog.Bundles.Any(bundle => bundle.Id == "bundle_6_b") &&
                catalog.Bundles.Any(bundle => bundle.Id == "bundle_6_c") &&
                catalog.Bundles.Any(bundle => bundle.Id == "bundle_7_a") &&
                catalog.Bundles.Any(bundle => bundle.Id == "bundle_7_b") &&
                catalog.Bundles.Any(bundle => bundle.Id == "bundle_8") &&
                catalog.Bundles.Count(bundle =>
                    bundle.Id.StartsWith("bundle_s2_")) == 3 &&
                catalog.Bundles.Count(bundle =>
                    bundle.Id.StartsWith("bundle_s3_")) == 17,
                "ids incorrectos");

            // Smoke de combate de 6 turnos con una carta de cada ola.
            var combat = NewPerkFixture(cards, effects, zones, 235);
            var combatIds = new[]
            {
                "card_agent_13", "card_bucky_barnes", "card_ebony_maw",
                "card_nightcrawler", "card_odin", "card_hulk",
                "card_cyclops", "card_ka_zar"
            };

            foreach (string id in combatIds)
            {
                combat.Board.PlayerHand.Add(new CardInstance(cards.Get(id))
                    { Location = CardLocation.Hand });
            }

            for (int turnNumber = 1; turnNumber <= 6; turnNumber++)
            {
                combat.Board.CurrentTurn = turnNumber;
                combat.Board.ResetTurnTracking();

                foreach (var card in combat.Board.PlayerHand.ToList())
                {
                    bool played = false;
                    for (int zone = 0; zone < combat.Board.Zones.Count; zone++)
                    {
                        if (combat.Actions.PlayCard(card, BoardSide.Player, zone, 0)
                            .Success)
                        {
                            played = true;
                            break;
                        }
                    }

                    if (played) break;
                }

                combat.Board.EndOfTurnResolvedTurn = -1;
                var reveal = combat.Board.Zones
                    .SelectMany(zone => zone.PlayerCards)
                    .Where(card => card.PlayedTurn == turnNumber &&
                                   !card.HasRevealed)
                    .ToList();

                combat.Effects.ResolveEndOfTurn(combat.Board, reveal);
            }

            Check("series2: combate de 6 turnos sin excepcion", true, null);

            // IA: no elige una carta restringida ni se atasca.
            var aiFixture = NewPerkFixture(cards, effects, zones, 233);
            aiFixture.Board.CurrentTurn = 4;
            var aiMaw = new CardInstance(cards.Get("card_ebony_maw"))
                { Location = CardLocation.Hand };
            var aiCyclops = new CardInstance(cards.Get("card_cyclops"))
                { Location = CardLocation.Hand };
            aiFixture.Board.AIHand.Add(aiMaw);
            aiFixture.Board.AIHand.Add(aiCyclops);
            var aiTurn = new TurnManager(aiFixture.Board, new TimeService(),
                aiFixture.Board.PowerResolver, aiFixture.Effects, new RunState());
            var aiStrategy = new AIStrategy(new RandomService(1), aiTurn, 1,
                aiFixture.Actions);
            var aiDecision = aiStrategy.MakeDecision(aiFixture.Board);
            Check("series2: IA evita carta restringida",
                aiDecision.HasValue &&
                aiDecision.Value.card.Definition.Id == "card_cyclops",
                aiDecision.HasValue
                    ? aiDecision.Value.card.Definition.Id
                    : "sin decision");

            var aiCombat = NewPerkFixture(cards, effects, zones, 237);
            aiCombat.Board.CurrentTurn = 4;
            foreach (string id in new[]
                     {
                         "card_ebony_maw", "card_storm", "card_the_infinaut",
                         "card_cyclops", "card_hulk"
                     })
            {
                aiCombat.Board.AIHand.Add(new CardInstance(cards.Get(id))
                    { Location = CardLocation.Hand });
            }

            var aiCombatTurn = new TurnManager(aiCombat.Board, new TimeService(),
                aiCombat.Board.PowerResolver, aiCombat.Effects, new RunState());
            var aiCombatController = new AIController(aiCombat.Board,
                new RandomService(1), aiCombat.Actions, aiCombatTurn, 1);
            var aiPlays = aiCombatController.TryPlayAll();
            Check("series2: IA juega sin atascarse",
                aiPlays.Count > 0 && aiPlays.All(play => play.Success),
                aiPlays.Count.ToString());
        }

        private static void RunPerkAbilityChecks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones, PerkRegistry perks)
        {
            // Efectivos: dos perks distintos exponen dos efectos distintos.
            var effective = new CardInstance(cards.Get("card_hulk"));
            effective.AttachedPerks.Add(PerkCatalog.Rally);
            effective.AttachedPerks.Add(PerkCatalog.Apex);
            var ids = PerkAbilities.EffectiveEffectIds(effective);
            Check("perk: dos perks exponen ambos efectos",
                ids.Count(id => id == PerkCatalog.EffectRally) == 1 &&
                ids.Count(id => id == PerkCatalog.EffectApex) == 1,
                string.Join(",", ids));

            var stacked = new CardInstance(cards.Get("card_hulk"));
            stacked.AttachedPerks.Add(PerkCatalog.Rally);
            stacked.AttachedPerks.Add(PerkCatalog.Rally);
            Check("perk: dos copias se resuelven dos veces",
                PerkAbilities.EffectiveEffectIds(stacked)
                    .Count(id => id == PerkCatalog.EffectRally) == 2,
                "no apilo");

            // Rally: +1 por cada otra carta aliada en la zona.
            var rallyFixture = NewPerkFixture(cards, effects, zones, 41);
            var rallyHost = Place(cards, rallyFixture.Board.Zones[0], "card_hulk", 1);
            rallyHost.AttachedPerks.Add(PerkCatalog.Rally);
            Check("perk rally: solo sin bonus",
                rallyFixture.Board.PowerResolver.CalculateCardPower(rallyHost,
                    rallyFixture.Board.Zones[0], rallyFixture.Board,
                    BoardSide.Player) == rallyHost.Definition.BasePower,
                "sumo sin companero");
            Place(cards, rallyFixture.Board.Zones[0], "card_shocker", 2);
            Check("perk rally: +1 por otra carta",
                rallyFixture.Board.PowerResolver.CalculateCardPower(rallyHost,
                    rallyFixture.Board.Zones[0], rallyFixture.Board,
                    BoardSide.Player) == rallyHost.Definition.BasePower + 1,
                "no sumo");

            // Apex: duplica solo si esta sola.
            var apexFixture = NewPerkFixture(cards, effects, zones, 42);
            var apexZone = apexFixture.Board.Zones[0];
            var apexHost = Place(cards, apexZone, "card_hulk", 1);
            apexHost.AttachedPerks.Add(PerkCatalog.Apex);
            Check("perk apex: duplica si esta sola",
                apexFixture.Board.PowerResolver.CalculateCardPower(apexHost,
                    apexZone, apexFixture.Board, BoardSide.Player) ==
                apexHost.Definition.BasePower * 2,
                "no duplico");
            Place(cards, apexZone, "card_shocker", 2);
            Check("perk apex: deja de duplicar con compania",
                apexFixture.Board.PowerResolver.CalculateCardPower(apexHost,
                    apexZone, apexFixture.Board, BoardSide.Player) ==
                apexHost.Definition.BasePower,
                "siguio duplicando");

            // Wakandan Tech: bono persistente igual al poder al revelarse.
            var wakandaFixture = NewPerkFixture(cards, effects, zones, 43);
            var wakandaHost = Place(cards, wakandaFixture.Board.Zones[0],
                "card_hulk", 1);
            wakandaHost.AttachedPerks.Add(PerkCatalog.WakandanTech);
            int revealPower = wakandaFixture.Board.PowerResolver.CalculateCardPower(
                wakandaHost, wakandaFixture.Board.Zones[0], wakandaFixture.Board,
                BoardSide.Player);
            wakandaFixture.Effects.ResolveEndOfTurn(wakandaFixture.Board,
                new List<CardInstance> { wakandaHost });
            Check("perk wakandan: duplica con bono persistente",
                wakandaHost.BonusPower == revealPower &&
                wakandaFixture.Board.PowerResolver.CalculateCardPower(wakandaHost,
                    wakandaFixture.Board.Zones[0], wakandaFixture.Board,
                    BoardSide.Player) == revealPower * 2,
                $"bono {wakandaHost.BonusPower}");

            // Reality Warp: reemplaza la zona conservando el indice.
            var warpFixture = NewPerkFixture(cards, effects, zones, 44);
            var warpZone = warpFixture.Board.Zones[0];
            string oldZoneId = warpZone.Id;
            var warpHost = Place(cards, warpZone, "card_hulk", 1);
            warpHost.AttachedPerks.Add(PerkCatalog.RealityWarp);
            warpFixture.Effects.ResolveEndOfTurn(warpFixture.Board,
                new List<CardInstance> { warpHost });
            Check("perk reality warp: nueva locacion mismo indice",
                warpZone.ZoneIndex == 0 && warpZone.Id != oldZoneId,
                warpZone.Id);

            // Mentor: la carta superior del mazo gana el poder del anfitrion.
            var mentorFixture = NewPerkFixture(cards, effects, zones, 45);
            var mentorHost = Place(cards, mentorFixture.Board.Zones[0],
                "card_hulk", 1);
            mentorHost.AttachedPerks.Add(PerkCatalog.Mentor);
            var topDeck = new CardInstance(cards.Get("card_shocker"))
                { Location = CardLocation.Deck };
            mentorFixture.Board.PlayerDeck.Add(topDeck);
            mentorFixture.Effects.ResolveEndOfTurn(mentorFixture.Board,
                new List<CardInstance> { mentorHost });
            Check("perk mentor: bono a la carta superior",
                topDeck.BonusPower == mentorHost.Definition.BasePower,
                topDeck.BonusPower.ToString());

            // Deadlink: +4 a cada Horda Zombie.
            var deadlinkFixture = NewPerkFixture(cards, effects, zones, 46);
            var horde = new CardInstance(cards.Get(PerkCatalog.ZombieHordeCardId))
            {
                Location = CardLocation.Board,
                IsInPlay = true
            };
            deadlinkFixture.Board.Zones[0].PlayerCards.Add(horde);
            var deadlinkHost = Place(cards, deadlinkFixture.Board.Zones[0],
                "card_hulk", 1);
            deadlinkHost.AttachedPerks.Add(PerkCatalog.Deadlink);
            deadlinkFixture.Effects.ResolveEndOfTurn(deadlinkFixture.Board,
                new List<CardInstance> { deadlinkHost });
            Check("perk deadlink: +4 a la Horda",
                horde.BonusPower == 4, horde.BonusPower.ToString());

            // Sidekick: personaje aleatorio a la mano con -2 de coste.
            var sidekickFixture = NewPerkFixture(cards, effects, zones, 47);
            var sidekickHost = Place(cards, sidekickFixture.Board.Zones[0],
                "card_hulk", 1);
            sidekickHost.AttachedPerks.Add(PerkCatalog.Sidekick);
            sidekickFixture.Board.PlayerHand.Clear();
            sidekickFixture.Effects.ResolveEndOfTurn(sidekickFixture.Board,
                new List<CardInstance> { sidekickHost });
            Check("perk sidekick: carta a la mano con descuento",
                sidekickFixture.Board.PlayerHand.Count == 1 &&
                sidekickFixture.Board.PlayerHand[0].TemporaryCostDelta == -2,
                sidekickFixture.Board.PlayerHand.Count.ToString());

            // Salvage: destruye la otra carta de menor poder y gana +5.
            var salvageFixture = NewPerkFixture(cards, effects, zones, 48);
            var salvageHost = Place(cards, salvageFixture.Board.Zones[0],
                "card_hulk", 1);
            salvageHost.AttachedPerks.Add(PerkCatalog.Salvage);
            var salvageVictim = Place(cards, salvageFixture.Board.Zones[0],
                "card_shocker", 2);
            salvageFixture.Effects.ResolveEndOfTurn(salvageFixture.Board,
                new List<CardInstance> { salvageHost });
            Check("perk salvage: destruye y suma +5",
                !salvageFixture.Board.Zones[0].PlayerCards.Contains(salvageVictim) &&
                salvageFixture.Board.PlayerPiles.Count(PileKind.Destroyed) == 1 &&
                salvageHost.BonusPower == 5,
                salvageHost.BonusPower.ToString());

            // Amplify: repite el efecto Continuo del anfitrion.
            var amplifyFixture = NewPerkFixture(cards, effects, zones, 49);
            var testOngoing = new TestOngoingPowerEffect
            {
                EffectId = "test_ongoing_power",
                Trigger = EffectTrigger.Ongoing,
                Amount = 2
            };
            amplifyFixture.Effects.RegisterEffect("test_ongoing_power", testOngoing);
            var amplifyHost = PlaceCustom(amplifyFixture.Board.Zones[0],
                "test_host", 1, 1, "test_ongoing_power");
            amplifyHost.AttachedPerks.Add(PerkCatalog.Amplify);
            Check("perk amplify: repite el Continuo una vez",
                amplifyFixture.Board.PowerResolver.CalculateCardPower(amplifyHost,
                    amplifyFixture.Board.Zones[0], amplifyFixture.Board,
                    BoardSide.Player) == 1 + 4,
                "no repitio");
            amplifyHost.AttachedPerks.Add(PerkCatalog.Amplify);
            Check("perk amplify: dos copias repiten dos veces",
                amplifyFixture.Board.PowerResolver.CalculateCardPower(amplifyHost,
                    amplifyFixture.Board.Zones[0], amplifyFixture.Board,
                    BoardSide.Player) == 1 + 6,
                "no apilo");

            // Encore: repite el Al revelarse del anfitrion.
            var encoreFixture = NewPerkFixture(cards, effects, zones, 50);
            var encoreEffect = new TestCountingEffect
            {
                EffectId = "test_on_reveal",
                Trigger = EffectTrigger.OnReveal
            };
            encoreFixture.Effects.RegisterEffect("test_on_reveal", encoreEffect);
            var encoreHost = PlaceCustom(encoreFixture.Board.Zones[0],
                "test_encore", 1, 1, "test_on_reveal");
            encoreHost.AttachedPerks.Add(PerkCatalog.Encore);
            encoreFixture.Effects.ResolveEndOfTurn(encoreFixture.Board,
                new List<CardInstance> { encoreHost });
            Check("perk encore: repite el Al revelarse",
                encoreEffect.Count == 2, encoreEffect.Count.ToString());

            // Overtime: repite el Fin de turno y dispara una vez por turno.
            var overtimeFixture = NewPerkFixture(cards, effects, zones, 51);
            var overtimeEffect = new TestCountingEffect
            {
                EffectId = "test_end_of_turn",
                Trigger = EffectTrigger.EndOfTurn
            };
            overtimeFixture.Effects.RegisterEffect("test_end_of_turn", overtimeEffect);
            var overtimeHost = PlaceCustom(overtimeFixture.Board.Zones[0],
                "test_overtime", 1, 1, "test_end_of_turn");
            overtimeHost.AttachedPerks.Add(PerkCatalog.Overtime);
            overtimeFixture.Board.CurrentTurn = 2;
            overtimeFixture.Effects.ResolveEndOfTurn(overtimeFixture.Board, null);
            overtimeFixture.Effects.ResolveEndOfTurn(overtimeFixture.Board, null);
            Check("perk overtime: repite y dispara una vez por turno",
                overtimeEffect.Count == 2, overtimeEffect.Count.ToString());

            // Overload: +1 de Energia al inicio del turno si gana la zona.
            var overloadFixture = NewPerkFixture(cards, effects, zones, 52);
            var overloadHost = Place(cards, overloadFixture.Board.Zones[0],
                "card_hulk", 1);
            overloadHost.AttachedPerks.Add(PerkCatalog.Overload);
            overloadFixture.Board.CurrentTurn = 2;
            overloadFixture.Board.StartOfTurnResolvedTurn = -1;
            int energyBefore = overloadFixture.Actions.AvailableEnergy(BoardSide.Player);
            overloadFixture.Effects.ResolveStartOfTurn(overloadFixture.Board);
            Check("perk overload: +1 de Energia al inicio",
                overloadFixture.Actions.AvailableEnergy(BoardSide.Player) ==
                energyBefore + 1,
                overloadFixture.Actions.AvailableEnergy(BoardSide.Player).ToString());

            // Overcharge: arma la comprobacion para el siguiente turno.
            var overchargeFixture = NewPerkFixture(cards, effects, zones, 53);
            var overchargeHost = Place(cards, overchargeFixture.Board.Zones[0],
                "card_hulk", 1);
            overchargeHost.AttachedPerks.Add(PerkCatalog.Overcharge);
            overchargeFixture.Board.CurrentTurn = 2;
            overchargeFixture.Effects.ResolveEndOfTurn(overchargeFixture.Board,
                new List<CardInstance> { overchargeHost });
            overchargeFixture.Board.CurrentTurn = 3;
            overchargeFixture.Board.StartOfTurnResolvedTurn = -1;
            int overchargeBefore =
                overchargeFixture.Actions.AvailableEnergy(BoardSide.Player);
            overchargeFixture.Effects.ResolveStartOfTurn(overchargeFixture.Board);
            Check("perk overcharge: +1 de Energia el turno siguiente",
                overchargeFixture.Actions.AvailableEnergy(BoardSide.Player) ==
                overchargeBefore + 1,
                overchargeFixture.Actions.AvailableEnergy(BoardSide.Player).ToString());

            // Quickdraw: descuento y habilidad concedida al robar.
            var quickFixture = NewPerkFixture(cards, effects, zones, 54);
            var quickHost = new CardInstance(cards.Get("card_shocker"))
            {
                Location = CardLocation.Deck
            };
            quickHost.AttachedPerks.Add(PerkCatalog.QuickdrawDiscount);
            quickHost.AttachedPerks.Add(PerkCatalog.QuickdrawPower);
            quickFixture.Board.PlayerDeck.Add(quickHost);
            quickFixture.Board.CurrentTurn = 2;
            var draw = quickFixture.Actions.DrawCard(BoardSide.Player);
            Check("perk quickdraw: descuento de 2 al robar",
                draw.Success && quickHost.TemporaryCostDelta == -2 &&
                quickHost.EffectiveCost ==
                Math.Max(0, quickHost.Definition.Cost - 2),
                quickHost.TemporaryCostDelta.ToString());
            Check("perk quickdraw: concede OnReveal temporal",
                quickHost.TemporaryEffectIds.Contains(
                    PerkCatalog.EffectQuickdrawPowerGrant),
                "sin efecto temporal");
            quickFixture.Actions.AddCardToZone(quickHost, BoardSide.Player, 0);
            quickFixture.Effects.ResolveEndOfTurn(quickFixture.Board,
                new List<CardInstance> { quickHost });
            Check("perk quickdraw: +3 al revelarse",
                quickHost.BonusPower == 3, quickHost.BonusPower.ToString());

            // Hot Pursuit: -3 si hubo movimiento el turno anterior.
            var hotFixture = NewPerkFixture(cards, effects, zones, 55);
            var hotHost = Place(cards, hotFixture.Board.Zones[0],
                "card_misty_knight", 1);
            hotHost.AttachedPerks.Add(PerkCatalog.HotPursuit);
            hotFixture.Board.CurrentTurn = 2;
            hotFixture.Board.LastMoveTurn = 1;
            hotFixture.Board.StartOfTurnResolvedTurn = -1;
            hotFixture.Effects.ResolveStartOfTurn(hotFixture.Board);
            Check("perk hot pursuit: -3 tras movimiento",
                hotFixture.Effects.GetCostReduction(hotHost,
                    hotFixture.Board, BoardSide.Player) == 3 &&
                hotHost.TurnCostDelta == -3 &&
                hotHost.EffectiveCost ==
                Math.Max(0, hotHost.Definition.Cost - 3),
                hotHost.TurnCostDelta.ToString());
            hotFixture.Board.CurrentTurn = 3;
            hotFixture.Board.LastMoveTurn = 0;
            hotFixture.Board.StartOfTurnResolvedTurn = -1;
            hotFixture.Effects.ResolveStartOfTurn(hotFixture.Board);
            Check("perk hot pursuit: no apila",
                hotFixture.Effects.GetCostReduction(hotHost,
                    hotFixture.Board, BoardSide.Player) == 0 &&
                hotHost.TurnCostDelta == 0,
                "siguio reduciendo");

            // Maneuverable: un movimiento manual, los de efecto no lo gastan.
            var moveFixture = NewPerkFixture(cards, effects, zones, 56);
            var moveHost = Place(cards, moveFixture.Board.Zones[0],
                "card_hulk", 1);
            moveHost.AttachedPerks.Add(PerkCatalog.Maneuverable);
            var firstMove = moveFixture.Actions.MoveCardByPlayer(moveHost,
                BoardSide.Player, 0, 1);
            var secondMove = moveFixture.Actions.MoveCardByPlayer(moveHost,
                BoardSide.Player, 1, 2);
            Check("perk maneuverable: solo un movimiento manual",
                firstMove.Success && !secondMove.Success && moveHost.PlayerMoveUsed,
                secondMove.Reason);

            var effectMoveFixture = NewPerkFixture(cards, effects, zones, 57);
            var effectMoveHost = Place(cards, effectMoveFixture.Board.Zones[0],
                "card_hulk", 1);
            effectMoveHost.AttachedPerks.Add(PerkCatalog.Maneuverable);
            effectMoveFixture.Actions.MoveCard(effectMoveHost, BoardSide.Player,
                0, 1, ActionCause.Effect);
            var afterEffectMove = effectMoveFixture.Actions.MoveCardByPlayer(
                effectMoveHost, BoardSide.Player, 1, 2);
            Check("perk maneuverable: el movimiento de efecto no consume",
                afterEffectMove.Success, afterEffectMove.Reason);

            // Unstoppable: juega en zona bloqueada, no muere, no pierde poder.
            var unstoppableFixture = NewPerkFixture(cards, effects, zones, 58);
            Place(cards, unstoppableFixture.Board.Zones[0], "card_professor_x", 1);
            var unstoppableCard = new CardInstance(cards.Get("card_misty_knight"))
                { Location = CardLocation.Hand };
            unstoppableCard.AttachedPerks.Add(PerkCatalog.Unstoppable);
            unstoppableFixture.Board.PlayerHand.Add(unstoppableCard);
            unstoppableFixture.Board.CurrentTurn = 1;
            var unstoppablePlay = unstoppableFixture.Actions.PlayCard(
                unstoppableCard, BoardSide.Player, 0, 0);
            Check("perk unstoppable: ignora bloqueo de zona",
                unstoppablePlay.Success, unstoppablePlay.Reason);

            var unstoppableDestroy = unstoppableFixture.Actions.DestroyCard(
                unstoppableCard, BoardSide.Player);
            Check("perk unstoppable: no puede ser destruida",
                !unstoppableDestroy.Success, "la destruyo");

            unstoppableCard.BonusPower = 0;
            unstoppableFixture.Actions.AddBonusPower(unstoppableCard, -3);
            Check("perk unstoppable: ignora reduccion de poder",
                unstoppableCard.BonusPower == 0,
                unstoppableCard.BonusPower.ToString());

            // Reinforce: +1 por copia cuando otro efecto potencia la carta.
            var reinforceFixture = NewPerkFixture(cards, effects, zones, 59);
            var reinforceHost = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            reinforceHost.AttachedPerks.Add(PerkCatalog.Reinforce);
            reinforceFixture.Board.PlayerHand.Add(reinforceHost);
            reinforceFixture.Actions.AddBonusPower(reinforceHost, 2);
            Check("perk reinforce: +1 al ser potenciada",
                reinforceHost.BonusPower == 3,
                reinforceHost.BonusPower.ToString());
            reinforceFixture.Actions.AddBonusPower(reinforceHost, 1);
            Check("perk reinforce: no se auto-dispara",
                reinforceHost.BonusPower == 5,
                reinforceHost.BonusPower.ToString());

            // Run events: nunca adjuntan un perk incompatible.
            var eventRegistry = new RunEventRegistry();
            eventRegistry.Register(new RunEventDef
            {
                Id = "test_perk_event",
                Title = "Test",
                Text = "Test",
                AcceptText = "OK",
                EffectKind = RunEventEffect.Perk
            });

            for (int seed = 1; seed <= 10; seed++)
            {
                var eventRun = new RunState();
                eventRun.StartNewRun();
                eventRun.Deck.Add(new CardInstance(cards.Get("card_hulk")));
                eventRun.Deck.Add(new CardInstance(cards.Get("card_iron_man")));
                eventRun.EventState = new EventState
                {
                    EventId = "test_perk_event",
                    Resolved = false
                };

                var eventSystem = new RunEventSystem(eventRegistry, cards, perks,
                    new RandomService(seed), null, null, effects);
                var resolution = eventSystem.Resolve(eventRun, true);
                Check($"eventos: perk compatible (seed {seed})",
                    resolution.Success && eventRun.Deck.All(card =>
                        card.AttachedPerks.All(perkId =>
                            PerkAbilities.CanAttach(perkId, card, effects))),
                    resolution.Error ?? "perk incompatible");
            }

            // Tag In: reemplaza a la carta destruida desde mano.
            var tagFixture = NewPerkFixture(cards, effects, zones, 60);
            var tagVictim = Place(cards, tagFixture.Board.Zones[0],
                "card_shocker", 1);
            var tagHost = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            tagHost.AttachedPerks.Add(PerkCatalog.TagIn);
            tagFixture.Board.PlayerHand.Add(tagHost);
            tagFixture.Actions.DestroyCard(tagVictim, BoardSide.Player);
            Check("perk tag in: entra en la zona destruida",
                tagFixture.Board.Zones[0].PlayerCards.Contains(tagHost) &&
                tagFixture.Board.PlayerPiles.Count(PileKind.Destroyed) == 1,
                "no entro");

            // Afterimage: copia sin mejoras en la zona abandonada.
            var afterimageFixture = NewPerkFixture(cards, effects, zones, 61);
            var afterimageHost = Place(cards, afterimageFixture.Board.Zones[0],
                "card_hulk", 1);
            afterimageHost.AttachedPerks.Add(PerkCatalog.Afterimage);
            afterimageHost.BonusPower = 4;
            afterimageFixture.Actions.MoveCard(afterimageHost, BoardSide.Player,
                0, 1, ActionCause.Effect);
            Check("perk afterimage: copia sin mejoras en la zona anterior",
                afterimageFixture.Board.Zones[0].PlayerCards.Count == 1 &&
                !ReferenceEquals(afterimageFixture.Board.Zones[0].PlayerCards[0],
                    afterimageHost) &&
                afterimageFixture.Board.Zones[0].PlayerCards[0].BonusPower == 0 &&
                afterimageFixture.Board.Zones[0].PlayerCards[0].AttachedPerks.Count == 0,
                "copia incorrecta");

            // Clone: copia sin mejoras barajada al mazo al iniciar combate.
            var cloneFixture = NewPerkFixture(cards, effects, zones, 62);
            var cloneHost = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Deck };
            cloneHost.AttachedPerks.Add(PerkCatalog.Clone);
            cloneFixture.Board.PlayerDeck.Add(cloneHost);
            cloneFixture.Actions.StartCombat();
            Check("perk clone: baraja una copia sin perks",
                cloneFixture.Board.PlayerDeck.Count == 2 &&
                cloneFixture.Board.PlayerDeck.Count(card =>
                    card.AttachedPerks.Count == 0) == 1,
                cloneFixture.Board.PlayerDeck.Count.ToString());

            // Regeneration: descarte y destruccion vuelven con +2.
            var regenFixture = NewPerkFixture(cards, effects, zones, 63);
            var regenHost = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            regenHost.AttachedPerks.Add(PerkCatalog.Regeneration);
            regenFixture.Board.PlayerHand.Add(regenHost);
            regenFixture.Actions.DiscardCard(regenHost, BoardSide.Player);
            Check("perk regeneration: vuelve con +2 y no entra a la pila",
                regenHost.IsInPlay && regenHost.BonusPower == 2 &&
                regenFixture.Board.PlayerPiles.Count(PileKind.Discard) == 0,
                $"bono {regenHost.BonusPower}");

            var regenFullFixture = NewPerkFixture(cards, effects, zones, 74);
            foreach (var zone in regenFullFixture.Board.Zones)
            {
                for (int i = 0; i < ZoneInstance.MaxCardsPerSide; i++)
                {
                    zone.PlayerCards.Add(new CardInstance(cards.Get("card_shocker"))
                    {
                        Location = CardLocation.Board,
                        IsInPlay = true
                    });
                }
            }

            var regenFullHost = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Hand };
            regenFullHost.AttachedPerks.Add(PerkCatalog.Regeneration);
            regenFullFixture.Board.PlayerHand.Add(regenFullHost);
            regenFullFixture.Actions.DiscardCard(regenFullHost, BoardSide.Player);
            Check("perk regeneration: sin espacio queda en la pila",
                regenFullFixture.Board.PlayerPiles.Count(PileKind.Discard) == 1 &&
                regenFullHost.BonusPower == 0,
                $"pila {regenFullFixture.Board.PlayerPiles.Count(PileKind.Discard)}, bono {regenFullHost.BonusPower}");

            var regenDestroyFixture = NewPerkFixture(cards, effects, zones, 64);
            var regenDestroyHost = Place(cards, regenDestroyFixture.Board.Zones[0],
                "card_hulk", 1);
            regenDestroyHost.AttachedPerks.Add(PerkCatalog.Regeneration);
            regenDestroyFixture.Actions.DestroyCard(regenDestroyHost,
                BoardSide.Player);
            Check("perk regeneration: no entra a destruidas",
                regenDestroyFixture.Board.PlayerPiles.Count(PileKind.Destroyed) == 0 &&
                regenDestroyHost.BonusPower == 2,
                $"bono {regenDestroyHost.BonusPower}");

            // Regroup: devuelve la carta de menor coste y le da +3.
            var regroupFixture = NewPerkFixture(cards, effects, zones, 65);
            var regroupHost = Place(cards, regroupFixture.Board.Zones[0],
                "card_hulk", 1);
            regroupHost.AttachedPerks.Add(PerkCatalog.Regroup);
            var regroupAlly = Place(cards, regroupFixture.Board.Zones[0],
                "card_misty_knight", 2);
            regroupFixture.Effects.ResolveEndOfTurn(regroupFixture.Board,
                new List<CardInstance> { regroupHost });
            Check("perk regroup: vuelve a la mano con +3",
                regroupFixture.Board.PlayerHand.Contains(regroupAlly) &&
                regroupAlly.BonusPower == 3,
                regroupAlly.BonusPower.ToString());

            // Slingshot: +1 y movimiento a la derecha.
            var slingshotFixture = NewPerkFixture(cards, effects, zones, 66);
            var slingshotHost = Place(cards, slingshotFixture.Board.Zones[0],
                "card_hulk", 1);
            slingshotHost.AttachedPerks.Add(PerkCatalog.Slingshot);
            var slingshotAlly = Place(cards, slingshotFixture.Board.Zones[0],
                "card_shocker", 2);
            slingshotFixture.Effects.ResolveEndOfTurn(slingshotFixture.Board,
                new List<CardInstance> { slingshotHost });
            Check("perk slingshot: +1 y mueve a la derecha",
                slingshotAlly.BonusPower == 1 &&
                slingshotFixture.Board.Zones[1].PlayerCards.Contains(slingshotAlly),
                "no movio");

            // Silence: apaga lo impreso y conserva los perks.
            var silenceFixture = NewPerkFixture(cards, effects, zones, 67);
            var silenceHost = Place(cards, silenceFixture.Board.Zones[0],
                "card_hawkeye", 1);
            silenceHost.AttachedPerks.Add(PerkCatalog.Silence);
            silenceFixture.Effects.ResolveEndOfTurn(silenceFixture.Board,
                new List<CardInstance> { silenceHost });
            Check("perk silence: impreso apagado y perk activo",
                silenceHost.PrintedAbilitiesDisabled &&
                !silenceHost.AbilitiesDisabled &&
                PerkAbilities.EffectiveEffectIds(silenceHost, false)
                    .Contains(PerkCatalog.EffectSilence),
                "estado incorrecto");

            // Habilidades desactivadas y OnReveal suprimido por Cosmo.
            var disabledFixture = NewPerkFixture(cards, effects, zones, 70);
            var disabledHost = Place(cards, disabledFixture.Board.Zones[0],
                "card_hulk", 1);
            disabledHost.AttachedPerks.Add(PerkCatalog.Rally);
            Place(cards, disabledFixture.Board.Zones[0], "card_shocker", 2);
            disabledHost.AbilitiesDisabled = true;
            Check("perk: habilidades desactivadas no aplican",
                disabledFixture.Board.PowerResolver.CalculateCardPower(disabledHost,
                    disabledFixture.Board.Zones[0], disabledFixture.Board,
                    BoardSide.Player) == disabledHost.Definition.BasePower,
                "siguio aplicando");

            var suppressedFixture = NewPerkFixture(cards, effects, zones, 71);
            var cosmo = Place(cards, suppressedFixture.Board.Zones[0], "card_cosmo", 1);
            cosmo.HasRevealed = true;
            var suppressedHost = Place(cards, suppressedFixture.Board.Zones[0],
                "card_hulk", 2);
            suppressedHost.AttachedPerks.Add(PerkCatalog.WakandanTech);
            suppressedFixture.Effects.ResolveEndOfTurn(suppressedFixture.Board,
                new List<CardInstance> { suppressedHost });
            Check("perk: OnReveal suprimido no aplica",
                suppressedHost.BonusPower == 0,
                suppressedHost.BonusPower.ToString());

            // Superstar: entra en la mano inicial antes que las aleatorias.
            var superstarCard = new CardInstance(cards.Get("card_hulk"))
                { Location = CardLocation.Deck };
            superstarCard.AttachedPerks.Add(PerkCatalog.Superstar);
            var superstarDeck = new List<CardInstance>
            {
                new CardInstance(cards.Get("card_shocker"))
                    { Location = CardLocation.Deck },
                superstarCard
            };
            var superstarHand = new List<CardInstance>();
            OpeningHandDealer.Deal(superstarDeck, superstarHand, 1);
            Check("perk superstar: primera carta de la mano",
                superstarHand.Count == 1 &&
                ReferenceEquals(superstarHand[0], superstarCard),
                "no entro primero");

            var excessDeck = new List<CardInstance>
            {
                new CardInstance(cards.Get("card_hulk"))
                    { Location = CardLocation.Deck },
                new CardInstance(cards.Get("card_cyclops"))
                    { Location = CardLocation.Deck }
            };
            excessDeck[0].AttachedPerks.Add(PerkCatalog.Superstar);
            excessDeck[1].AttachedPerks.Add(PerkCatalog.Superstar);
            var excessHand = new List<CardInstance>();
            OpeningHandDealer.Deal(excessDeck, excessHand, 1);
            Check("perk superstar: excedente queda en el mazo",
                excessHand.Count == 1 && excessDeck.Count == 1,
                $"mano {excessHand.Count}, mazo {excessDeck.Count}");

            // Taunt: reserva la zona para la primera carta del rival.
            var tauntFixture = NewPerkFixture(cards, effects, zones, 68);
            var tauntHost = Place(cards, tauntFixture.Board.Zones[0],
                "card_hulk", 1);
            tauntHost.AttachedPerks.Add(PerkCatalog.Taunt);
            tauntFixture.Board.CurrentTurn = 1;
            tauntFixture.Effects.ResolveEndOfTurn(tauntFixture.Board,
                new List<CardInstance> { tauntHost });
            Check("perk taunt: reserva la zona rival",
                tauntFixture.Board.GetTauntReservation(BoardSide.AI, 2) != null,
                "sin reserva");

            tauntFixture.Board.CurrentTurn = 2;
            tauntFixture.Board.ResetTurnTracking();
            var aiWrong = new CardInstance(cards.Get("card_misty_knight"))
                { Location = CardLocation.Hand };
            tauntFixture.Board.AIHand.Add(aiWrong);
            var wrongPlay = tauntFixture.Actions.PlayCard(aiWrong,
                BoardSide.AI, 1, 0);
            Check("perk taunt: rechaza la zona equivocada",
                !wrongPlay.Success, "la acepto");
            var aiRight = new CardInstance(cards.Get("card_misty_knight"))
                { Location = CardLocation.Hand };
            tauntFixture.Board.AIHand.Add(aiRight);
            var rightPlay = tauntFixture.Actions.PlayCard(aiRight,
                BoardSide.AI, 0, 1);
            Check("perk taunt: acepta la zona reservada",
                rightPlay.Success, rightPlay.Reason);

            var playerTauntFixture = NewPerkFixture(cards, effects, zones, 72);
            var playerTauntHost = PlaceEnemy(cards,
                playerTauntFixture.Board.Zones[1], "card_hulk", 1);
            playerTauntHost.AttachedPerks.Add(PerkCatalog.Taunt);
            playerTauntFixture.Board.CurrentTurn = 1;
            playerTauntFixture.Effects.ResolveEndOfTurn(playerTauntFixture.Board,
                new List<CardInstance> { playerTauntHost });
            playerTauntFixture.Board.CurrentTurn = 2;
            playerTauntFixture.Board.ResetTurnTracking();
            var playerWrong = new CardInstance(cards.Get("card_misty_knight"))
                { Location = CardLocation.Hand };
            playerTauntFixture.Board.PlayerHand.Add(playerWrong);
            var playerWrongPlay = playerTauntFixture.Actions.PlayCard(playerWrong,
                BoardSide.Player, 0, 0);
            Check("perk taunt: restringe al jugador",
                !playerWrongPlay.Success, "lo dejo jugar");
            var playerRight = new CardInstance(cards.Get("card_misty_knight"))
                { Location = CardLocation.Hand };
            playerTauntFixture.Board.PlayerHand.Add(playerRight);
            var playerRightPlay = playerTauntFixture.Actions.PlayCard(playerRight,
                BoardSide.Player, 1, 1);
            Check("perk taunt: el jugador puede usar la zona reservada",
                playerRightPlay.Success, playerRightPlay.Reason);

            // Reserva expirada cuando la zona esta llena.
            var fullTauntFixture = NewPerkFixture(cards, effects, zones, 73);
            var fullTauntHost = Place(cards, fullTauntFixture.Board.Zones[0],
                "card_hulk", 1);
            fullTauntHost.AttachedPerks.Add(PerkCatalog.Taunt);
            fullTauntFixture.Board.CurrentTurn = 1;
            fullTauntFixture.Effects.ResolveEndOfTurn(fullTauntFixture.Board,
                new List<CardInstance> { fullTauntHost });
            for (int i = 0; i < ZoneInstance.MaxCardsPerSide; i++)
            {
                fullTauntFixture.Board.Zones[0].AICards.Add(
                    new CardInstance(cards.Get("card_shocker"))
                    { Location = CardLocation.Board, IsInPlay = true });
            }
            fullTauntFixture.Board.CurrentTurn = 2;
            fullTauntFixture.Board.ResetTurnTracking();
            var fullAiCard = new CardInstance(cards.Get("card_misty_knight"))
                { Location = CardLocation.Hand };
            fullTauntFixture.Board.AIHand.Add(fullAiCard);
            var fullAiPlay = fullTauntFixture.Actions.PlayCard(fullAiCard,
                BoardSide.AI, 1, 0);
            Check("perk taunt: zona llena expira sin bloquear",
                fullAiPlay.Success, fullAiPlay.Reason);

            // Loop bound: una cascada forzada se detiene en el limite.
            var loopFixture = NewPerkFixture(cards, effects, zones, 69);
            var loopEffect = new TestLoopEffect();
            loopFixture.Effects.RegisterEffect("test_loop", loopEffect);
            var loopHost = PlaceCustom(loopFixture.Board.Zones[0],
                "test_loop_host", 1, 1, "test_loop");
            loopFixture.Effects.Dispatch(loopFixture.Board, new ActionEvent
            {
                Kind = ActionEventKind.Moved,
                SourceCard = loopHost,
                Side = BoardSide.Player,
                Turn = 1
            });
            Check("perk: el limite de eventos corta la cascada",
                loopFixture.Effects.LastActionHitLimit,
                loopFixture.Effects.LastActionDispatched.ToString());
        }

        // ------------------------------------------------------------------
        // Migracion de saves
        // ------------------------------------------------------------------

        private static void RunPerkPersistenceChecks(CardRegistry cards,
            EffectRegistry effects, PerkRegistry perks)
        {
            var storage = new InMemoryStorage();
            var runStore = new JsonRunStore(storage);
            var profileStore = new JsonProfileStore(storage);
            var run = new RunState();
            var draft = new DraftSystem(cards, new RandomService(1));
            var service = new RunCheckpointService(run, draft, runStore,
                profileStore, cards, new RandomService(1));
            service.Initialize();
            service.StartNewRun();

            var savedCard = new CardInstance(cards.Get("card_hulk"));
            savedCard.CostDelta = -1;
            savedCard.PowerDelta = 4;
            savedCard.UpgradeCount = 2;
            savedCard.AttachedPerks.Add("perk_vanguardia");
            savedCard.AttachedPerks.Add(PerkCatalog.Rally);
            savedCard.AttachedPerks.Add("perk_desconocido");
            run.Deck.Clear();
            run.Deck.Add(savedCard);
            run.PendingNode = RunNode.EnemySelect;
            service.SaveSafeNode();

            var restored = new RunState();
            var restoredDraft = new DraftSystem(cards, new RandomService(1));
            var restoredService = new RunCheckpointService(restored,
                restoredDraft, runStore, new JsonProfileStore(storage), cards,
                new RandomService(1));
            restoredService.Initialize();
            Check("migracion: run guardada",
                restoredService.HasRun, "sin run");
            restoredService.RestoreRun();

            Check("migracion: solo queda el perk actual",
                restored.Deck.Count == 1 &&
                restored.Deck[0].AttachedPerks.Count == 1 &&
                restored.Deck[0].AttachedPerks[0] == PerkCatalog.Rally,
                string.Join(",", restored.Deck.FirstOrDefault()?.AttachedPerks ??
                    new List<string>()));
            Check("migracion: mejoras y deltas sobreviven",
                restored.Deck[0].CostDelta == -1 &&
                restored.Deck[0].PowerDelta == 4 &&
                restored.Deck[0].UpgradeCount == 2 &&
                restored.PendingNode == RunNode.EnemySelect,
                "se perdio estado");

            // El siguiente guardado escribe el estado limpio y conserva el schema.
            restoredService.SaveSafeNode();
            var reloaded = runStore.Load();
            Check("migracion: el siguiente save queda limpio",
                reloaded.Deck.Count == 1 &&
                reloaded.Deck[0].AttachedPerks.Count == 1 &&
                reloaded.Deck[0].AttachedPerks[0] == PerkCatalog.Rally,
                "no limpio");
            Check("migracion: schema sin cambios",
                reloaded.SchemaVersion == RunDto.CurrentSchemaVersion,
                reloaded.SchemaVersion.ToString());
        }

        // ------------------------------------------------------------------
        // Flujo completo: run -> cobro -> tienda -> coleccion -> resumen
        // ------------------------------------------------------------------

        private static void RunEndToEndChecks(CardRegistry cards, PerkRegistry perks)
        {
            var storage = new InMemoryStorage();
            var profileStore = new JsonProfileStore(storage);
            var meta = new MetaProgressionSystem();
            var store = new GlobalStoreSystem(cards, perks);
            var collection = new CollectionSystem(cards, perks, store);

            var profile = ProfileDto.CreateDefault(StarterSet.CardIds);
            profile.Points = 500;
            profileStore.Save(profile);

            var run = NewRun();
            run.NormalCombatsWon = 43;
            run.SubBossesWon = 4;
            run.Level = 30;
            run.FinalBossDefeated = true;
            run.Outcome = RunOutcome.Won;

            var award = meta.Award(run, profile,
                p => { profileStore.Save(p); return true; });
            Check("flujo: run cobrada",
                award.Status == AwardStatus.Awarded && profile.Points == 1179,
                $"estado {award.Status}, puntos {profile.Points}");

            var entries = store.GetEntries(profile);
            var bundleEntry = entries.First(e => e.Kind == GlobalStoreEntryKind.Bundle);
            var directEntry = entries.First(e => e.Kind == GlobalStoreEntryKind.DirectCard);
            var lifeEntry = entries.First(e => e.Kind == GlobalStoreEntryKind.ExtraLife);
            Check("flujo: tienda sin perks",
                !entries.Any(e => e.Id.StartsWith("perk", StringComparison.Ordinal)),
                "sigue vendiendo");

            var bundle = store.Catalog.GetBundle(bundleEntry.BundleId);

            Func<ProfileDto, bool> save = p => { profileStore.Save(p); return true; };

            Check("flujo: compra lote",
                store.Purchase(profile, bundleEntry, save).Success, "fallo el lote");
            Check("flujo: compra carta directa",
                store.Purchase(profile, directEntry, save).Success, "fallo la carta");
            Check("flujo: compra vida extra",
                store.Purchase(profile, lifeEntry, save).Success, "fallo la vida");

            int expectedPoints = 1179 - bundle.Price -
                                 GlobalStoreSystem.DirectCardPrice -
                                 GlobalStoreSystem.ExtraLifePrice;
            Check("flujo: saldo tras compras",
                profile.Points == expectedPoints, $"puntos {profile.Points}");

            var nextRun = new RunState();
            nextRun.StartNewRun();
            store.PrepareNewRun(profile, nextRun);
            Check("flujo: vida extra en la siguiente run",
                nextRun.ExtraLifeCharges == 1, nextRun.ExtraLifeCharges.ToString());
            Check("flujo: lote comprado no vuelve a la tienda",
                !profile.VisibleBundleIds.Contains(bundle.Id), "sigue visible");

            var cardEntries = collection.BuildCards(profile, nextRun);
            foreach (string cardId in bundle.CardIds)
            {
                Check($"flujo: carta del lote {cardId} en coleccion",
                    cardEntries.First(e => e.Card.Id == cardId).State ==
                    CollectionCardState.Owned,
                    "no propia");
            }

            Check("flujo: carta directa en coleccion",
                cardEntries.First(e => e.Card.Id == directEntry.CardId).State ==
                CollectionCardState.Owned,
                "no propia");

            var perkEntries = collection.BuildPerks(profile, nextRun);
            Check("flujo: 28 perks en coleccion",
                perkEntries.Count == 28 && perkEntries.All(p => p.Owned),
                "catalogo incorrecto");

            var summary = meta.Compute(run);
            Check("flujo: resumen suma al total",
                summary.NormalCombatPoints + summary.SubBossPoints +
                summary.LevelPoints + summary.FinalBossBonus == summary.Total,
                summary.Total.ToString());
            Check("flujo: resumen con jefe final",
                summary.FinalBossBonus == 150, summary.FinalBossBonus.ToString());

            var reloaded = new JsonProfileStore(storage).Load();
            Check("flujo: perfil persistido",
                reloaded.LastAwardedRunId == run.RunId &&
                reloaded.OwnedBundleIds.Contains(bundle.Id),
                "no persistio");
        }

        // ------------------------------------------------------------------
        // Objetos de combate
        // ------------------------------------------------------------------

        private static void RunItemCatalogChecks(CardRegistry cards, ItemRegistry items)
        {
            var all = items.GetAll();
            Check("objetos: 8 registrados", all.Count == 8, $"hay {all.Count}");
            Check("objetos: 8 efectos distintos",
                all.Select(item => item.EffectId).Distinct().Count() == 8,
                "efectos repetidos");

            foreach (var item in all)
            {
                Check($"objetos: {item.Id} con nombre y descripcion",
                    !string.IsNullOrWhiteSpace(item.Name) &&
                    !string.IsNullOrWhiteSpace(item.Description), "texto vacio");
                Check($"objetos: {item.Id} con rareza",
                    item.Rarity == ItemRarity.Common || item.Rarity == ItemRarity.Rare,
                    item.Rarity.ToString());
                Check($"objetos: {item.Id} con efecto implementado",
                    ItemEffectIds.IsKnown(item.EffectId), item.EffectId);
                Check($"objetos: {item.Id} no es carta ni token",
                    cards.Get(item.Id) == null, "registrado como carta");
            }
        }

        private static void RunItemUseChecks(CardRegistry cards, EffectRegistry effects,
            ItemRegistry items)
        {
            string energy = "item_energy_surge";
            string power = "item_power_boost";
            string destroy = "item_destroy_enemy";
            string draw = "item_draw_two";
            string move = "item_move_enemy";
            string afflict = "item_afflict_zone";
            string copy = "item_copy_hand";
            string heal = "item_heal_zone";

            // Energia: +2 solo en el turno y consumo inmediato.
            var energyFixture = NewCombat(cards, effects, items, 11);
            energyFixture.Board.CurrentTurn = 1;
            var energyProfile = ProfileWith(energy, 1);
            int energyBefore = energyFixture.Actions.AvailableEnergy(BoardSide.Player);
            int saves = 0;
            var energyUse = energyFixture.Items.TryUse(energy, energyProfile, true,
                p => { saves++; return true; });
            Check("objetos: energia suma 2 al turno",
                energyUse.Success &&
                energyFixture.Actions.AvailableEnergy(BoardSide.Player) == energyBefore + 2 &&
                energyProfile.ItemCounts[energy] == 0 && saves == 1,
                energyUse.Error);

            // Fuera de la fase de juego no se puede usar ni consumir.
            var outside = NewCombat(cards, effects, items, 12);
            outside.Board.CurrentTurn = 1;
            var outsideProfile = ProfileWith(energy, 1);
            int outsideVersion = outside.Board.Version;
            var outsideUse = outside.Items.TryUse(energy, outsideProfile, false, p => true);
            Check("objetos: fuera de fase se rechaza",
                !outsideUse.Success &&
                outsideProfile.ItemCounts[energy] == 1 &&
                outside.Board.Version == outsideVersion,
                outsideUse.Error);

            // Poder: +2 a una carta aliada en juego.
            var powerFixture = NewCombat(cards, effects, items, 13);
            var powerZone = powerFixture.Board.Zones[0];
            var ally = Place(cards, powerZone, "card_hulk", 1);
            var powerProfile = ProfileWith(power, 1);
            var powerUse = powerFixture.Items.TryUse(power, powerProfile, true, p => true);
            Check("objetos: poder aliado +2",
                powerUse.Success && ally.BonusPower == 2 &&
                powerProfile.ItemCounts[power] == 0,
                powerUse.Error);

            // Destruir: usa la autoridad y la pila de destruidas.
            var destroyFixture = NewCombat(cards, effects, items, 14);
            var destroyZone = destroyFixture.Board.Zones[0];
            var enemy = PlaceEnemy(cards, destroyZone, "card_hulk", 1);
            var destroyProfile = ProfileWith(destroy, 1);
            var destroyUse = destroyFixture.Items.TryUse(destroy, destroyProfile, true, p => true);
            Check("objetos: destruir enemigo",
                destroyUse.Success && !destroyZone.AICards.Contains(enemy) &&
                destroyFixture.Board.AIPiles.Count(PileKind.Destroyed) == 1 &&
                destroyProfile.ItemCounts[destroy] == 0,
                destroyUse.Error);

            // Proteccion: Armor impide que el objetivo sea elegible.
            var protectedFixture = NewCombat(cards, effects, items, 15);
            var protectedZone = protectedFixture.Board.Zones[0];
            var protectedEnemy = PlaceEnemy(cards, protectedZone, "card_hulk", 1);
            Place(cards, protectedZone, "card_armor", 2);
            var protectedProfile = ProfileWith(destroy, 1);
            int protectedVersion = protectedFixture.Board.Version;
            var protectedCheck = protectedFixture.Items.CheckUse(destroy,
                protectedProfile, true);
            var protectedUse = protectedFixture.Items.TryUse(destroy,
                protectedProfile, true, p => true);
            Check("objetos: proteccion invalida el objetivo",
                !protectedCheck.CanUse && !protectedUse.Success &&
                protectedZone.AICards.Contains(protectedEnemy) &&
                protectedProfile.ItemCounts[destroy] == 1 &&
                protectedFixture.Board.Version == protectedVersion,
                protectedUse.Error);

            // Robar 2 y rechazo con la mano llena.
            var drawFixture = NewCombat(cards, effects, items, 16);
            drawFixture.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_hulk")) { Location = CardLocation.Deck });
            drawFixture.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_cyclops")) { Location = CardLocation.Deck });
            drawFixture.Board.PlayerHand.Add(
                new CardInstance(cards.Get("card_shocker")) { Location = CardLocation.Hand });
            var drawProfile = ProfileWith(draw, 1);
            var drawUse = drawFixture.Items.TryUse(draw, drawProfile, true, p => true);
            Check("objetos: robar hasta 2",
                drawUse.Success && drawFixture.Board.PlayerHand.Count == 3 &&
                drawFixture.Board.PlayerDeck.Count == 0,
                drawUse.Error);

            var fullHandFixture = NewCombat(cards, effects, items, 17);
            for (int i = 0; i < BoardState.MaxHandSize; i++)
            {
                fullHandFixture.Board.PlayerHand.Add(
                    new CardInstance(cards.Get("card_shocker")) { Location = CardLocation.Hand });
            }
            fullHandFixture.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_hulk")) { Location = CardLocation.Deck });
            var fullDrawProfile = ProfileWith(draw, 1);
            var fullDrawCheck = fullHandFixture.Items.CheckUse(draw, fullDrawProfile, true);
            Check("objetos: mano llena rechaza robo",
                !fullDrawCheck.CanUse &&
                fullDrawProfile.ItemCounts[draw] == 1 &&
                fullHandFixture.Board.PlayerHand.Count == BoardState.MaxHandSize,
                fullDrawCheck.Reason);

            // Mover enemigo y bloqueo de zona.
            var moveFixture = NewCombat(cards, effects, items, 18);
            var moveOrigin = moveFixture.Board.Zones[0];
            var moveDestination = moveFixture.Board.Zones[1];
            var movedCard = PlaceEnemy(cards, moveOrigin, "card_hulk", 1);
            var moveProfile = ProfileWith(move, 1);
            var moveUse = moveFixture.Items.TryUse(move, moveProfile, true, p => true);
            Check("objetos: mover enemigo a otra zona",
                moveUse.Success && moveOrigin.AICards.Count == 0 &&
                moveDestination.AICards.Contains(movedCard),
                moveUse.Error);

            var lockFixture = NewCombat(cards, effects, items, 19);
            var lockOrigin = lockFixture.Board.Zones[0];
            var lockDestination = lockFixture.Board.Zones[1];
            PlaceEnemy(cards, lockOrigin, "card_hulk", 1);
            Place(cards, lockOrigin, "card_professor_x", 2);
            var lockProfile = ProfileWith(move, 1);
            var lockCheck = lockFixture.Items.CheckUse(move, lockProfile, true);
            var lockUse = lockFixture.Items.TryUse(move, lockProfile, true, p => true);
            Check("objetos: bloqueo de zona impide mover",
                !lockCheck.CanUse && !lockUse.Success &&
                lockProfile.ItemCounts[move] == 1 &&
                lockOrigin.AICards.Count == 1 &&
                lockDestination.AICards.Count == 0,
                lockUse.Error);

            // Afliccion y curacion de zona.
            var afflictFixture = NewCombat(cards, effects, items, 20);
            var afflictZone = afflictFixture.Board.Zones[0];
            var afflictedA = PlaceEnemy(cards, afflictZone, "card_hulk", 1);
            var afflictedB = PlaceEnemy(cards, afflictZone, "card_cyclops", 2);
            var afflictProfile = ProfileWith(afflict, 1);
            var afflictUse = afflictFixture.Items.TryUse(afflict, afflictProfile, true, p => true);
            Check("objetos: afliccion -2 a la zona",
                afflictUse.Success && afflictedA.BonusPower == -2 &&
                afflictedB.BonusPower == -2,
                afflictUse.Error);

            var healFixture = NewCombat(cards, effects, items, 21);
            var healZone = healFixture.Board.Zones[1];
            var healedA = Place(cards, healZone, "card_hulk", 1);
            var healedB = Place(cards, healZone, "card_cyclops", 2);
            var healProfile = ProfileWith(heal, 1);
            var healUse = healFixture.Items.TryUse(heal, healProfile, true, p => true);
            Check("objetos: curacion +3 a la zona",
                healUse.Success && healedA.BonusPower == 3 &&
                healedB.BonusPower == 3,
                healUse.Error);

            // Copiar de la mano y rechazo con la mano llena.
            var copyFixture = NewCombat(cards, effects, items, 22);
            var copySource = new CardInstance(cards.Get("card_hulk"))
            {
                Location = CardLocation.Hand
            };
            copyFixture.Board.PlayerHand.Add(copySource);
            var copyProfile = ProfileWith(copy, 1);
            var copyUse = copyFixture.Items.TryUse(copy, copyProfile, true, p => true);
            Check("objetos: copiar carta de la mano",
                copyUse.Success && copyFixture.Board.PlayerHand.Count == 2 &&
                !ReferenceEquals(copyFixture.Board.PlayerHand[1], copySource),
                copyUse.Error);

            var fullCopyFixture = NewCombat(cards, effects, items, 23);
            for (int i = 0; i < BoardState.MaxHandSize; i++)
            {
                fullCopyFixture.Board.PlayerHand.Add(
                    new CardInstance(cards.Get("card_shocker")) { Location = CardLocation.Hand });
            }
            var fullCopyProfile = ProfileWith(copy, 1);
            var fullCopyCheck = fullCopyFixture.Items.CheckUse(copy, fullCopyProfile, true);
            Check("objetos: mano llena rechaza copia",
                !fullCopyCheck.CanUse &&
                fullCopyProfile.ItemCounts[copy] == 1,
                fullCopyCheck.Reason);

            // Sin objetivo: rechazo sin tocar tablero ni contador.
            var emptyFixture = NewCombat(cards, effects, items, 24);
            var emptyProfile = ProfileWith(power, 1);
            int emptyVersion = emptyFixture.Board.Version;
            var emptyUse = emptyFixture.Items.TryUse(power, emptyProfile, true, p => true);
            Check("objetos: sin objetivo no consume",
                !emptyUse.Success && emptyProfile.ItemCounts[power] == 1 &&
                emptyFixture.Board.Version == emptyVersion,
                emptyUse.Error);

            // Semilla: el mismo target para el mismo seed.
            var seededA = NewCombat(cards, effects, items, 99);
            Place(cards, seededA.Board.Zones[0], "card_hulk", 1);
            Place(cards, seededA.Board.Zones[1], "card_shocker", 2);
            var seededB = NewCombat(cards, effects, items, 99);
            Place(cards, seededB.Board.Zones[0], "card_hulk", 1);
            Place(cards, seededB.Board.Zones[1], "card_shocker", 2);

            seededA.Items.TryUse(power, ProfileWith(power, 1), true, p => true);
            seededB.Items.TryUse(power, ProfileWith(power, 1), true, p => true);
            Check("objetos: mismo seed mismo objetivo",
                BoostedZoneIndex(seededA) == BoostedZoneIndex(seededB),
                $"{BoostedZoneIndex(seededA)} vs {BoostedZoneIndex(seededB)}");

            // Escritura fallida: se restaura el contador, el efecto queda.
            var failingFixture = NewCombat(cards, effects, items, 25);
            var failingAlly = Place(cards, failingFixture.Board.Zones[0],
                "card_hulk", 1);
            var failingProfile = ProfileWith(power, 1);
            var failingUse = failingFixture.Items.TryUse(power, failingProfile,
                true, p => false);
            Check("objetos: escritura fallida restaura contador",
                !failingUse.Success && failingUse.EffectApplied &&
                failingProfile.ItemCounts[power] == 1 &&
                failingAlly.BonusPower == 2,
                failingUse.Error);

            // Persistencia: el consumo sobrevive a una recarga.
            var storage = new InMemoryStorage();
            var profileStore = new JsonProfileStore(storage);
            var persistedFixture = NewCombat(cards, effects, items, 26);
            Place(cards, persistedFixture.Board.Zones[0], "card_hulk", 1);
            var persistedProfile = ProfileWith(power, 2);
            profileStore.Save(persistedProfile);
            var persistedUse = persistedFixture.Items.TryUse(power, persistedProfile,
                true, p => { profileStore.Save(p); return true; });
            var reloaded = new JsonProfileStore(storage).Load();
            Check("objetos: consumo persiste tras reiniciar",
                persistedUse.Success &&
                reloaded.ItemCounts.TryGetValue(power, out int remaining) &&
                remaining == 1,
                persistedUse.Error);
        }

        private static void RunItemStoreChecks(CardRegistry cards, PerkRegistry perks,
            ItemRegistry items)
        {
            var store = new GlobalStoreSystem(cards, perks, items, new RandomService(31));
            var storage = new InMemoryStorage();
            var profileStore = new JsonProfileStore(storage);
            var profile = ProfileDto.CreateDefault(StarterSet.CardIds);
            profile.Points = 1000;

            var entry = store.GetEntries(profile)
                .FirstOrDefault(e => e.Kind == GlobalStoreEntryKind.Item);
            Check("tienda: objeto visible a 60",
                entry != null && !entry.Hidden && entry.Available && entry.Price == 60,
                entry == null ? "ausente" : entry.Description);

            int itemTotalBefore = profile.ItemCounts.Values.Sum();
            var purchase = store.Purchase(profile, entry,
                p => { profileStore.Save(p); return true; });
            int itemTotalAfter = profile.ItemCounts.Values.Sum();

            Check("tienda: compra un objeto aleatorio",
                purchase.Success && profile.Points == 940 &&
                itemTotalAfter == itemTotalBefore + 1 &&
                profile.ItemEntrySold,
                purchase.Error);

            var reloaded = new JsonProfileStore(storage).Load();
            Check("tienda: objeto vendido persiste",
                reloaded.ItemEntrySold, "no persistio");

            var soldEntry = store.GetEntries(profile)
                .First(e => e.Kind == GlobalStoreEntryKind.Item);
            Check("tienda: objeto agotado hasta descanso",
                soldEntry.Owned && !soldEntry.Available &&
                !store.CanPurchase(profile, soldEntry),
                "sigue disponible");

            store.AdvanceRotation(profile);
            var restocked = store.GetEntries(profile)
                .First(e => e.Kind == GlobalStoreEntryKind.Item);
            Check("tienda: objeto vuelve tras descanso",
                !profile.ItemEntrySold && restocked.Available && !restocked.Owned,
                "no reaparecio");

            var failingStorage = new InMemoryStorage { FailWrites = true };
            var failingProfileStore = new JsonProfileStore(failingStorage);
            var failingProfile = ProfileDto.CreateDefault(StarterSet.CardIds);
            failingProfile.Points = 1000;
            var failingEntry = store.GetEntries(failingProfile)
                .First(e => e.Kind == GlobalStoreEntryKind.Item);
            var failed = store.Purchase(failingProfile, failingEntry,
                p => { failingProfileStore.Save(p); return true; });
            Check("tienda: escritura fallida no regala objeto",
                !failed.Success && failingProfile.Points == 1000 &&
                !failingProfile.ItemEntrySold &&
                failingProfile.ItemCounts.Values.Sum() == 0,
                failed.Error);
        }

        private sealed class CombatFixture
        {
            public BoardState Board;
            public CardActionService Actions;
            public EffectProcessor Effects;
            public CombatItemSystem Items;
        }

        private static CombatFixture NewCombat(CardRegistry cards,
            EffectRegistry effects, ItemRegistry items, int seed)
        {
            var run = new RunState();
            run.StartNewRun();

            var board = NewBoard();
            var effectProcessor = new EffectProcessor(effects, cards, new RandomService(seed));
            var resolver = new PowerResolver(null, effectProcessor);
            board.PowerResolver = resolver;

            var actions = new CardActionService(board, new CardPileManager(board, run),
                resolver, effectProcessor, run);

            return new CombatFixture
            {
                Board = board,
                Actions = actions,
                Effects = effectProcessor,
                Items = new CombatItemSystem(board, actions, effectProcessor, items,
                    new RandomService(seed))
            };
        }

        private static void RunSeries3FoundationChecks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var fx = NewPerkFixture(cards, effects, zones, 301);
            fx.Board.CurrentTurn = 3;

            var plain = PlaceCustom(fx.Board.Zones[0], "test_plain", 4, 0);
            Check("series3: coste base",
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player) == 4,
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player).ToString());

            plain.CostDelta = -1;
            Check("series3: coste con mejora",
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player) == 3,
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player).ToString());

            plain.TemporaryCostDelta = 2;
            Check("series3: coste temporal",
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player) == 5,
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player).ToString());

            plain.CombatCostOverride = 0;
            Check("series3: override de combate",
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player) == 0,
                fx.Actions.GetEffectiveCost(plain, BoardSide.Player).ToString());

            var cap = PlaceCustom(fx.Board.Zones[0], "test_cap", 6, 0);
            cap.CostDelta = 2;
            Check("series3: tope global 6",
                fx.Actions.GetEffectiveCost(cap, BoardSide.Player) == 6,
                fx.Actions.GetEffectiveCost(cap, BoardSide.Player).ToString());

            var floor = PlaceCustom(fx.Board.Zones[0], "test_floor", 1, 0);
            floor.CostDelta = -2;
            Check("series3: suelo 0",
                fx.Actions.GetEffectiveCost(floor, BoardSide.Player) == 0,
                fx.Actions.GetEffectiveCost(floor, BoardSide.Player).ToString());

            var death = PlaceCustom(fx.Board.Zones[0], "test_death", 8, 0,
                "effect_death");
            fx.Board.RecordDestroyedCard(plain, BoardSide.Player, 1, 1, 1);
            fx.Board.RecordDestroyedCard(plain, BoardSide.Player, 1, 1, 1);
            fx.Board.RecordDestroyedCard(plain, BoardSide.Player, 1, 1, 1);
            Check("series3: Death -3",
                fx.Actions.GetEffectiveCost(death, BoardSide.Player) == 5,
                fx.Actions.GetEffectiveCost(death, BoardSide.Player).ToString());

            var sheHulk = PlaceCustom(fx.Board.Zones[0], "test_she_hulk", 6, 0,
                "effect_she_hulk");
            fx.Board.SetUnspentEnergyLastTurn(BoardSide.Player, 2);
            Check("series3: She-Hulk -2",
                fx.Actions.GetEffectiveCost(sheHulk, BoardSide.Player) == 4,
                fx.Actions.GetEffectiveCost(sheHulk, BoardSide.Player).ToString());

            var stature = PlaceCustom(fx.Board.Zones[0], "test_stature", 5, 0,
                "effect_stature");
            fx.Board.IncrementDiscard(BoardSide.AI);
            Check("series3: Stature a 1",
                fx.Actions.GetEffectiveCost(stature, BoardSide.Player) == 1,
                fx.Actions.GetEffectiveCost(stature, BoardSide.Player).ToString());

            var miles = PlaceCustom(fx.Board.Zones[0], "test_miles", 4, 0,
                "effect_miles_morales");
            fx.Board.LastMoveTurn = 2;
            Check("series3: Miles a 1",
                fx.Actions.GetEffectiveCost(miles, BoardSide.Player) == 1,
                fx.Actions.GetEffectiveCost(miles, BoardSide.Player).ToString());

            var sera = PlaceCustom(fx.Board.Zones[1], "test_sera", 5, 0,
                "effect_sera");
            var hulk = new CardInstance(cards.Get("card_hulk"))
            {
                Location = CardLocation.Hand
            };
            fx.Board.PlayerHand.Add(hulk);
            Check("series3: Sera -1",
                fx.Actions.GetEffectiveCost(hulk, BoardSide.Player) == 5,
                fx.Actions.GetEffectiveCost(hulk, BoardSide.Player).ToString());
            fx.Board.Zones[1].PlayerCards.Remove(sera);

            var quinjet = PlaceCustom(fx.Board.Zones[1], "test_quinjet", 1, 0,
                "effect_quinjet");
            var created = new CardInstance(cards.Get("card_hulk"))
            {
                Location = CardLocation.Hand,
                IsCreated = true
            };
            fx.Board.PlayerHand.Add(created);
            Check("series3: Quinjet -1 a creada",
                fx.Actions.GetEffectiveCost(created, BoardSide.Player) == 5,
                fx.Actions.GetEffectiveCost(created, BoardSide.Player).ToString());

            var zabu = PlaceCustom(fx.Board.Zones[1], "test_zabu", 1, 0,
                "effect_zabu");
            var fourCost = new CardInstance(new CardDef
            {
                Id = "test_four_cost", Name = "test_four_cost", Cost = 4,
                BasePower = 0, Description = string.Empty,
                EffectIds = new List<string>()
            })
            {
                Location = CardLocation.Deck
            };
            fx.Board.PlayerDeck.Add(fourCost);
            Check("series3: Zabu -1 en mazo",
                fx.Actions.GetEffectiveCost(fourCost, BoardSide.Player) == 3,
                fx.Actions.GetEffectiveCost(fourCost, BoardSide.Player).ToString());

            var mobius = PlaceCustom(fx.Board.Zones[2], "test_mobius", 3, 0,
                "effect_mobius");
            var taxed = PlaceCustom(fx.Board.Zones[2], "test_taxed", 4, 0);
            taxed.TemporaryCostDelta = 1;
            Check("series3: Mobius no sube",
                fx.Actions.GetEffectiveCost(taxed, BoardSide.Player) == 4,
                fx.Actions.GetEffectiveCost(taxed, BoardSide.Player).ToString());

            fx.Board.SetGlobalMaxCost(4, fx.Board.CurrentTurn);
            Check("series3: Wave tope 4",
                fx.Actions.GetEffectiveCost(cap, BoardSide.Player) == 4,
                fx.Actions.GetEffectiveCost(cap, BoardSide.Player).ToString());

            var enemyMobius = NewPerkFixture(cards, effects, zones, 302);
            enemyMobius.Board.CurrentTurn = 3;
            var enemyMobiusCard = PlaceCustom(enemyMobius.Board.Zones[0],
                "test_enemy_mobius", 3, 0, "effect_mobius");
            enemyMobius.Board.Zones[0].PlayerCards.Remove(enemyMobiusCard);
            enemyMobius.Board.Zones[0].AICards.Add(enemyMobiusCard);
            var reduced = PlaceCustom(enemyMobius.Board.Zones[0],
                "test_reduced", 4, 0);
            reduced.CostDelta = -1;
            Check("series3: Mobius rival no baja",
                enemyMobius.Actions.GetEffectiveCost(reduced,
                    BoardSide.AI) == 4,
                enemyMobius.Actions.GetEffectiveCost(reduced,
                    BoardSide.AI).ToString());
        }

        private static void RunSeries3Wave1Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave1 = new (string Id, int Cost, int Power)[]
            {
                ("card_drax", 4, 5), ("card_green_goblin", 3, -4),
                ("card_debrii", 3, 7), ("card_doctor_doom", 6, 5),
                ("card_rescue", 4, 4), ("card_rockslide", 4, 5),
                ("card_silver_surfer", 3, 2), ("card_the_hood", 1, -3),
                ("card_thor", 3, 5), ("card_wasp", 0, 1),
                ("card_wong", 4, 2), ("card_yellowjacket", 0, 2),
                ("card_orka", 6, 11), ("card_luke_cage", 2, 2),
                ("card_omega_red", 4, 6), ("card_red_skull", 5, 14),
                ("card_typhoid_mary", 4, 10), ("card_mojo", 2, 2),
                ("card_cerebro", 3, 0), ("card_patriot", 3, 2)
            };

            foreach (var entry in wave1)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var draxFx = NewPerkFixture(cards, effects, zones, 401);
            var drax = Place(cards, draxFx.Board.Zones[0], "card_drax", 1);
            draxFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.AI,
                Card = new CardInstance(cards.Get("card_hulk")),
                ZoneIndex = 0,
                Turn = 0
            });
            draxFx.Effects.ResolveEndOfTurn(draxFx.Board,
                new List<CardInstance> { drax });
            Check("series3: Drax +4", drax.BonusPower == 4,
                drax.BonusPower.ToString());

            var gobFx = NewPerkFixture(cards, effects, zones, 402);
            var goblin = Place(cards, gobFx.Board.Zones[0],
                "card_green_goblin", 1);
            gobFx.Effects.ResolveEndOfTurn(gobFx.Board,
                new List<CardInstance> { goblin });
            Check("series3: Green Goblin cambia de lado",
                gobFx.Board.Zones[0].AICards.Contains(goblin) &&
                !gobFx.Board.Zones[0].PlayerCards.Contains(goblin),
                "no cambio");

            var debriiFx = NewPerkFixture(cards, effects, zones, 403);
            var debrii = Place(cards, debriiFx.Board.Zones[0], "card_debrii", 1);
            debriiFx.Effects.ResolveEndOfTurn(debriiFx.Board,
                new List<CardInstance> { debrii });
            Check("series3: Debrii dos Rocas por lado",
                debriiFx.Board.Zones[1].PlayerCards.Count == 1 &&
                debriiFx.Board.Zones[1].AICards.Count == 1 &&
                debriiFx.Board.Zones[2].PlayerCards.Count == 1 &&
                debriiFx.Board.Zones[2].AICards.Count == 1,
                "rocas incompletas");

            var doomFx = NewPerkFixture(cards, effects, zones, 404);
            var doom = Place(cards, doomFx.Board.Zones[0], "card_doctor_doom", 1);
            doomFx.Effects.ResolveEndOfTurn(doomFx.Board,
                new List<CardInstance> { doom });
            Check("series3: Doctor Doom DoomBots",
                doomFx.Board.Zones[1].PlayerCards.Any(c =>
                    c.Definition.Id == "token_doombot") &&
                doomFx.Board.Zones[2].PlayerCards.Any(c =>
                    c.Definition.Id == "token_doombot"),
                "sin DoomBot");

            var rescueFx = NewPerkFixture(cards, effects, zones, 405);
            var rescue = Place(cards, rescueFx.Board.Zones[0], "card_rescue", 1);
            rescueFx.Effects.ResolveEndOfTurn(rescueFx.Board,
                new List<CardInstance> { rescue });
            Check("series3: Rescue arma +5",
                rescue.PendingEffects.Any(p =>
                    p.Kind == PendingEffectKind.AllyPlayedInZoneNextTurn &&
                    p.Amount == 5),
                "sin pendiente");

            var rockslideFx = NewPerkFixture(cards, effects, zones, 406);
            var rockslide = Place(cards, rockslideFx.Board.Zones[0],
                "card_rockslide", 1);
            rockslideFx.Effects.ResolveEndOfTurn(rockslideFx.Board,
                new List<CardInstance> { rockslide });
            Check("series3: Rockslide 2 Rocas",
                rockslideFx.Board.AIDeck.Count(c =>
                    c.Definition.Id == "token_rock") == 2,
                "rocas incorrectas");

            var surferFx = NewPerkFixture(cards, effects, zones, 407);
            var surfer = Place(cards, surferFx.Board.Zones[0],
                "card_silver_surfer", 1);
            var threeA = Place(cards, surferFx.Board.Zones[0],
                "card_cyclops", 2);
            var threeB = Place(cards, surferFx.Board.Zones[1],
                "card_gambit", 3);
            surferFx.Effects.ResolveEndOfTurn(surferFx.Board,
                new List<CardInstance> { surfer });
            Check("series3: Silver Surfer +2 a coste 3",
                threeA.BonusPower == 2 && threeB.BonusPower == 2 &&
                surfer.BonusPower == 0,
                $"{threeA.BonusPower}/{threeB.BonusPower}");

            var hoodFx = NewPerkFixture(cards, effects, zones, 408);
            var hood = Place(cards, hoodFx.Board.Zones[0], "card_the_hood", 1);
            hoodFx.Effects.ResolveEndOfTurn(hoodFx.Board,
                new List<CardInstance> { hood });
            Check("series3: The Hood agrega Demonio",
                hoodFx.Board.PlayerHand.Any(c =>
                    c.Definition.Id == "token_demon"),
                "sin Demonio");

            var thorFx = NewPerkFixture(cards, effects, zones, 409);
            var thor = Place(cards, thorFx.Board.Zones[0], "card_thor", 1);
            thorFx.Effects.ResolveEndOfTurn(thorFx.Board,
                new List<CardInstance> { thor });
            Check("series3: Thor baraja Mjolnir",
                thorFx.Board.PlayerDeck.Any(c =>
                    c.Definition.Id == "token_mjolnir"),
                "sin Mjolnir");

            var wongFx = NewPerkFixture(cards, effects, zones, 410);
            var wong = Place(cards, wongFx.Board.Zones[1], "card_wong", 1);
            var medusa = Place(cards, wongFx.Board.Zones[1], "card_medusa", 2);
            wongFx.Effects.ResolveEndOfTurn(wongFx.Board,
                new List<CardInstance> { wong, medusa });
            Check("series3: Wong repite +3", medusa.BonusPower == 6,
                medusa.BonusPower.ToString());

            var yjFx = NewPerkFixture(cards, effects, zones, 411);
            var yellowjacket = Place(cards, yjFx.Board.Zones[0],
                "card_yellowjacket", 1);
            var yjAlly = Place(cards, yjFx.Board.Zones[0], "card_hulk", 2);
            yjFx.Effects.ResolveEndOfTurn(yjFx.Board,
                new List<CardInstance> { yellowjacket });
            Check("series3: Yellowjacket -1 a aliados",
                yjAlly.BonusPower == -1 && yellowjacket.BonusPower == 0,
                $"{yjAlly.BonusPower}/{yellowjacket.BonusPower}");

            var orkaFx = NewPerkFixture(cards, effects, zones, 412);
            var orka = Place(cards, orkaFx.Board.Zones[0], "card_orka", 1);
            orkaFx.Board.PowerResolver.UpdateAllPowers(orkaFx.Board);
            Check("series3: Orka +7 solo", orka.CurrentPower == 18,
                orka.CurrentPower.ToString());

            var lcFx = NewPerkFixture(cards, effects, zones, 413);
            var luke = Place(cards, lcFx.Board.Zones[0], "card_luke_cage", 1);
            var lcAlly = Place(cards, lcFx.Board.Zones[0], "card_hulk", 2);
            lcFx.Actions.AddBonusPower(lcAlly, -2);
            Check("series3: Luke Cage protege", lcAlly.BonusPower == 0,
                lcAlly.BonusPower.ToString());

            var omegaFx = NewPerkFixture(cards, effects, zones, 414);
            var omega = Place(cards, omegaFx.Board.Zones[0],
                "card_omega_red", 1);
            Place(cards, omegaFx.Board.Zones[0], "card_hulk", 2);
            omegaFx.Board.PowerResolver.UpdateAllPowers(omegaFx.Board);
            Check("series3: Omega Red +3 a otras zonas",
                omegaFx.Effects.GetZoneAddition(omegaFx.Board.Zones[1],
                    BoardSide.Player, omegaFx.Board) == 3,
                "sin bonus");

            var skullFx = NewPerkFixture(cards, effects, zones, 415);
            var skull = Place(cards, skullFx.Board.Zones[0],
                "card_red_skull", 1);
            var skullEnemy = PlaceEnemy(cards, skullFx.Board.Zones[0],
                "card_hulk", 1);
            skullFx.Board.PowerResolver.UpdateAllPowers(skullFx.Board);
            Check("series3: Red Skull +2 enemigo",
                skullEnemy.CurrentPower == 16,
                skullEnemy.CurrentPower.ToString());

            var maryFx = NewPerkFixture(cards, effects, zones, 416);
            var mary = Place(cards, maryFx.Board.Zones[0],
                "card_typhoid_mary", 1);
            var maryAlly = Place(cards, maryFx.Board.Zones[0], "card_hulk", 2);
            maryFx.Board.PowerResolver.UpdateAllPowers(maryFx.Board);
            Check("series3: Typhoid Mary -2 aliado",
                maryAlly.CurrentPower == 12,
                maryAlly.CurrentPower.ToString());

            var mojoFx = NewPerkFixture(cards, effects, zones, 417);
            var mojo = Place(cards, mojoFx.Board.Zones[0], "card_mojo", 1);
            Place(cards, mojoFx.Board.Zones[0], "card_hulk", 2);
            Place(cards, mojoFx.Board.Zones[0], "card_cyclops", 3);
            Place(cards, mojoFx.Board.Zones[0], "card_shocker", 4);
            for (int i = 0; i < 4; i++)
                PlaceEnemy(cards, mojoFx.Board.Zones[0], "card_hulk", i + 1);
            mojoFx.Board.PowerResolver.UpdateAllPowers(mojoFx.Board);
            Check("series3: Mojo +6 lleno", mojo.CurrentPower == 8,
                mojo.CurrentPower.ToString());

            var cerebroFx = NewPerkFixture(cards, effects, zones, 418);
            var cerebro = Place(cards, cerebroFx.Board.Zones[0],
                "card_cerebro", 1);
            var cerebroHulk = Place(cards, cerebroFx.Board.Zones[0],
                "card_hulk", 2);
            var cerebroCyclops = Place(cards, cerebroFx.Board.Zones[0],
                "card_cyclops", 3);
            cerebroFx.Board.PowerResolver.UpdateAllPowers(cerebroFx.Board);
            Check("series3: Cerebro +3 al mayor",
                cerebroHulk.CurrentPower == 17 &&
                cerebroCyclops.CurrentPower == 5,
                $"{cerebroHulk.CurrentPower}/{cerebroCyclops.CurrentPower}");

            var patriotFx = NewPerkFixture(cards, effects, zones, 419);
            var patriot = Place(cards, patriotFx.Board.Zones[0],
                "card_patriot", 1);
            var patriotRock = Place(cards, patriotFx.Board.Zones[0],
                "token_rock", 2);
            var patriotHulk = Place(cards, patriotFx.Board.Zones[0],
                "card_hulk", 3);
            patriotFx.Board.PowerResolver.UpdateAllPowers(patriotFx.Board);
            Check("series3: Patriot +2 sin habilidad",
                patriotRock.CurrentPower == 3 &&
                patriotHulk.CurrentPower == 16,
                $"{patriotRock.CurrentPower}/{patriotHulk.CurrentPower}");
        }

        private static void RunSeries3Wave2Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave2 = new (string Id, int Cost, int Power)[]
            {
                ("card_ronan", 5, 5), ("card_dazzler", 2, 2),
                ("card_polaris", 3, 5), ("card_stegron", 4, 7),
                ("card_magneto", 6, 12), ("card_kingpin", 1, 3),
                ("card_dagger", 2, 2), ("card_human_torch", 3, 2),
                ("card_spider_man_2099", 4, 5), ("card_lady_deathstrike", 5, 7),
                ("card_venom", 3, 3), ("card_modok", 5, 7),
                ("card_selene", 1, 2), ("card_hazmat", 3, 3),
                ("card_gambit", 3, 3), ("card_falcon", 2, 3),
                ("card_ghost", 3, 5), ("card_psylocke", 2, 2),
                ("card_kitty_pryde", 1, 1), ("card_aero", 5, 9)
            };

            foreach (var entry in wave2)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var ronanFx = NewPerkFixture(cards, effects, zones, 501);
            var ronan = Place(cards, ronanFx.Board.Zones[0], "card_ronan", 1);
            ronanFx.Board.AIHand.Add(new CardInstance(cards.Get("card_hulk")));
            ronanFx.Board.AIHand.Add(new CardInstance(cards.Get("card_cyclops")));
            ronanFx.Board.PowerResolver.UpdateAllPowers(ronanFx.Board);
            Check("series3: Ronan +4", ronan.CurrentPower == 9,
                ronan.CurrentPower.ToString());

            var dazzlerFx = NewPerkFixture(cards, effects, zones, 502);
            var dazzler = Place(cards, dazzlerFx.Board.Zones[0], "card_dazzler", 1);
            for (int i = 0; i < 3; i++)
                Place(cards, dazzlerFx.Board.Zones[0], "card_cyclops", i + 2);
            dazzlerFx.Board.PowerResolver.UpdateAllPowers(dazzlerFx.Board);
            Check("series3: Dazzler +2 zona llena", dazzler.CurrentPower == 4,
                dazzler.CurrentPower.ToString());

            var polarisFx = NewPerkFixture(cards, effects, zones, 503);
            var polaris = Place(cards, polarisFx.Board.Zones[0],
                "card_polaris", 1);
            var polarisTarget = PlaceEnemy(cards, polarisFx.Board.Zones[1],
                "card_elektra", 1);
            polarisFx.Effects.ResolveEndOfTurn(polarisFx.Board,
                new List<CardInstance> { polaris });
            Check("series3: Polaris mueve coste 1",
                polarisFx.Board.Zones[0].AICards.Contains(polarisTarget),
                "no movio");

            var stegronFx = NewPerkFixture(cards, effects, zones, 504);
            var stegron = Place(cards, stegronFx.Board.Zones[0],
                "card_stegron", 1);
            var stegronTarget = PlaceEnemy(cards, stegronFx.Board.Zones[0],
                "card_hulk", 1);
            stegronFx.Effects.ResolveEndOfTurn(stegronFx.Board,
                new List<CardInstance> { stegron });
            Check("series3: Stegron mueve a la derecha",
                stegronFx.Board.Zones[1].AICards.Contains(stegronTarget),
                "no movio");

            var magnetoFx = NewPerkFixture(cards, effects, zones, 505);
            var magneto = Place(cards, magnetoFx.Board.Zones[0],
                "card_magneto", 1);
            var magnetoTarget = PlaceEnemy(cards, magnetoFx.Board.Zones[2],
                "card_cyclops", 1);
            magnetoFx.Effects.ResolveEndOfTurn(magnetoFx.Board,
                new List<CardInstance> { magneto });
            Check("series3: Magneto mueve coste 3",
                magnetoFx.Board.Zones[0].AICards.Contains(magnetoTarget),
                "no movio");

            var kingpinFx = NewPerkFixture(cards, effects, zones, 506);
            var kingpin = Place(cards, kingpinFx.Board.Zones[0],
                "card_kingpin", 1);
            var kingpinTarget = PlaceEnemy(cards, kingpinFx.Board.Zones[1],
                "card_hulk", 1);
            kingpinFx.Actions.MoveCard(kingpinTarget, BoardSide.AI, 1, 0,
                ActionCause.Effect);
            Check("series3: Kingpin -2", kingpinTarget.BonusPower == -2,
                kingpinTarget.BonusPower.ToString());

            var daggerFx = NewPerkFixture(cards, effects, zones, 507);
            var dagger = Place(cards, daggerFx.Board.Zones[0],
                "card_dagger", 1);
            PlaceEnemy(cards, daggerFx.Board.Zones[1], "card_hulk", 1);
            PlaceEnemy(cards, daggerFx.Board.Zones[1], "card_cyclops", 2);
            daggerFx.Actions.MoveCard(dagger, BoardSide.Player, 0, 1,
                ActionCause.Effect);
            Check("series3: Dagger +4", dagger.BonusPower == 4,
                dagger.BonusPower.ToString());

            var torchFx = NewPerkFixture(cards, effects, zones, 508);
            var torch = Place(cards, torchFx.Board.Zones[0],
                "card_human_torch", 1);
            torchFx.Actions.MoveCard(torch, BoardSide.Player, 0, 1,
                ActionCause.Effect);
            Check("series3: Human Torch duplica", torch.BonusPower == 2,
                torch.BonusPower.ToString());

            var spideyFx = NewPerkFixture(cards, effects, zones, 509);
            var spidey = Place(cards, spideyFx.Board.Zones[0],
                "card_spider_man_2099", 1);
            var spideyTarget = PlaceEnemy(cards, spideyFx.Board.Zones[1],
                "card_hulk", 1);
            spideyFx.Actions.MoveCard(spidey, BoardSide.Player, 0, 1,
                ActionCause.Effect);
            Check("series3: Spider-Man 2099 -3",
                spideyTarget.BonusPower == -3,
                spideyTarget.BonusPower.ToString());

            var deathstrikeFx = NewPerkFixture(cards, effects, zones, 510);
            var deathstrike = Place(cards, deathstrikeFx.Board.Zones[0],
                "card_lady_deathstrike", 1);
            var weak = Place(cards, deathstrikeFx.Board.Zones[0],
                "card_wasp", 2);
            var strong = Place(cards, deathstrikeFx.Board.Zones[0],
                "card_hulk", 3);
            deathstrikeFx.Effects.ResolveEndOfTurn(deathstrikeFx.Board,
                new List<CardInstance> { deathstrike });
            Check("series3: Lady Deathstrike umbral 3",
                !deathstrikeFx.Board.Zones[0].PlayerCards.Contains(weak) &&
                deathstrikeFx.Board.Zones[0].PlayerCards.Contains(strong),
                "umbral incorrecto");

            var venomFx = NewPerkFixture(cards, effects, zones, 511);
            var venom = Place(cards, venomFx.Board.Zones[0], "card_venom", 1);
            Place(cards, venomFx.Board.Zones[0], "card_shocker", 2);
            Place(cards, venomFx.Board.Zones[0], "card_cyclops", 3);
            venomFx.Effects.ResolveEndOfTurn(venomFx.Board,
                new List<CardInstance> { venom });
            Check("series3: Venom suma 9", venom.BonusPower == 9,
                venom.BonusPower.ToString());

            var modokFx = NewPerkFixture(cards, effects, zones, 512);
            var modok = Place(cards, modokFx.Board.Zones[0], "card_modok", 1);
            modokFx.Board.PlayerHand.Add(new CardInstance(cards.Get("card_hulk")));
            modokFx.Board.PlayerHand.Add(new CardInstance(cards.Get("card_cyclops")));
            modokFx.Effects.ResolveEndOfTurn(modokFx.Board,
                new List<CardInstance> { modok });
            Check("series3: M.O.D.O.K. vacia mano",
                modokFx.Board.PlayerHand.Count == 0,
                modokFx.Board.PlayerHand.Count.ToString());

            var seleneFx = NewPerkFixture(cards, effects, zones, 513);
            var selene = Place(cards, seleneFx.Board.Zones[0], "card_selene", 1);
            var playerLow = new CardInstance(cards.Get("card_wasp"));
            var aiLow = new CardInstance(cards.Get("card_wasp"));
            seleneFx.Board.PlayerHand.Add(playerLow);
            seleneFx.Board.AIHand.Add(aiLow);
            seleneFx.Effects.ResolveEndOfTurn(seleneFx.Board,
                new List<CardInstance> { selene });
            Check("series3: Selene -3 a ambas manos",
                playerLow.BonusPower == -3 && aiLow.BonusPower == -3,
                $"{playerLow.BonusPower}/{aiLow.BonusPower}");

            var hazmatFx = NewPerkFixture(cards, effects, zones, 514);
            var hazmat = Place(cards, hazmatFx.Board.Zones[0], "card_hazmat", 1);
            var hazmatAlly = Place(cards, hazmatFx.Board.Zones[0],
                "card_hulk", 2);
            var hazmatEnemy = PlaceEnemy(cards, hazmatFx.Board.Zones[1],
                "card_cyclops", 1);
            hazmatFx.Effects.ResolveEndOfTurn(hazmatFx.Board,
                new List<CardInstance> { hazmat });
            Check("series3: Hazmat -1 global",
                hazmatAlly.BonusPower == -1 && hazmatEnemy.BonusPower == -1,
                $"{hazmatAlly.BonusPower}/{hazmatEnemy.BonusPower}");

            var gambitFx = NewPerkFixture(cards, effects, zones, 515);
            var gambit = Place(cards, gambitFx.Board.Zones[0], "card_gambit", 1);
            gambitFx.Board.PlayerHand.Add(new CardInstance(cards.Get("card_hulk")));
            var gambitEnemy = PlaceEnemy(cards, gambitFx.Board.Zones[1],
                "card_cyclops", 1);
            gambitFx.Effects.ResolveEndOfTurn(gambitFx.Board,
                new List<CardInstance> { gambit });
            Check("series3: Gambit descarta y destruye",
                gambitFx.Board.PlayerHand.Count == 0 &&
                !gambitFx.Board.Zones[1].AICards.Contains(gambitEnemy),
                "no resolvio");

            var falconFx = NewPerkFixture(cards, effects, zones, 516);
            var falcon = Place(cards, falconFx.Board.Zones[0], "card_falcon", 1);
            var oneCost = Place(cards, falconFx.Board.Zones[0], "card_elektra", 2);
            falconFx.Effects.ResolveEndOfTurn(falconFx.Board,
                new List<CardInstance> { falcon });
            Check("series3: Falcon devuelve coste 1",
                falconFx.Board.PlayerHand.Contains(oneCost),
                "no devolvio");

            var ghostFx = NewPerkFixture(cards, effects, zones, 517);
            var ghost = Place(cards, ghostFx.Board.Zones[0], "card_ghost", 1);
            var sunspot = Place(cards, ghostFx.Board.Zones[0], "card_sunspot", 2);
            var plain = Place(cards, ghostFx.Board.Zones[0], "card_hulk", 3);
            ghostFx.Effects.ResolveEndOfTurn(ghostFx.Board,
                new List<CardInstance> { ghost });
            Check("series3: Ghost apaga Fin de turno",
                sunspot.AbilitiesDisabled && !plain.AbilitiesDisabled,
                "no apago");

            var psylockeFx = NewPerkFixture(cards, effects, zones, 518);
            var psylocke = Place(cards, psylockeFx.Board.Zones[0],
                "card_psylocke", 1);
            psylockeFx.Effects.ResolveEndOfTurn(psylockeFx.Board,
                new List<CardInstance> { psylocke });
            Check("series3: Psylocke +1 energia siguiente",
                psylockeFx.Board.GetNextTurnEnergyBonus(BoardSide.Player) == 1,
                psylockeFx.Board.GetNextTurnEnergyBonus(BoardSide.Player)
                    .ToString());

            var kittyFx = NewPerkFixture(cards, effects, zones, 519);
            var kitty = Place(cards, kittyFx.Board.Zones[0],
                "card_kitty_pryde", 1);
            kittyFx.Board.CurrentTurn = 2;
            kittyFx.Effects.ResolveStartOfTurn(kittyFx.Board);
            Check("series3: Kitty vuelve y +1",
                kittyFx.Board.PlayerHand.Contains(kitty) &&
                kitty.BonusPower == 1,
                $"{kittyFx.Board.PlayerHand.Contains(kitty)}/{kitty.BonusPower}");

            var aeroFx = NewPerkFixture(cards, effects, zones, 520);
            var aero = Place(cards, aeroFx.Board.Zones[0], "card_aero", 1);
            var aeroTarget = PlaceEnemy(cards, aeroFx.Board.Zones[1],
                "card_hulk", 1);
            aeroFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.AI,
                Card = aeroTarget,
                ZoneIndex = 1,
                Turn = 0,
                PlayOrder = 1
            });
            aeroFx.Effects.ResolveEndOfTurn(aeroFx.Board,
                new List<CardInstance> { aero });
            Check("series3: Aero mueve ultima rival",
                aeroFx.Board.Zones[0].AICards.Contains(aeroTarget),
                "no movio");
        }

        private static void RunSeries3Wave3Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave3 = new (string Id, int Cost, int Power)[]
            {
                ("card_agent_coulson", 3, 5), ("card_attuma", 4, 10),
                ("card_brood", 3, 2), ("card_colleen_wing", 2, 3),
                ("card_destroyer", 6, 18), ("card_ghost_spider", 1, 3),
                ("card_maria_hill", 1, 3), ("card_mirage", 2, 3),
                ("card_nick_fury", 4, 6), ("card_sauron", 1, 2),
                ("card_shanna", 3, 4), ("card_ultron", 6, 8),
                ("card_viper", 3, 4), ("card_zero", 1, 3)
            };

            foreach (var entry in wave3)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var coulsonFx = NewPerkFixture(cards, effects, zones, 601);
            var coulson = Place(cards, coulsonFx.Board.Zones[0],
                "card_agent_coulson", 1);
            coulsonFx.Effects.ResolveEndOfTurn(coulsonFx.Board,
                new List<CardInstance> { coulson });
            Check("series3: Agent Coulson 4 y 5",
                coulsonFx.Board.PlayerHand.Count == 2 &&
                coulsonFx.Board.PlayerHand.Any(c => c.Definition.Cost == 4) &&
                coulsonFx.Board.PlayerHand.Any(c => c.Definition.Cost == 5),
                coulsonFx.Board.PlayerHand.Count.ToString());

            var attumaFx = NewPerkFixture(cards, effects, zones, 602);
            var attuma = Place(cards, attumaFx.Board.Zones[0], "card_attuma", 1);
            var attumaWeak = Place(cards, attumaFx.Board.Zones[0],
                "card_cyclops", 2);
            attumaFx.Effects.ResolveEndOfTurn(attumaFx.Board,
                new List<CardInstance> { attuma });
            Check("series3: Attuma destruye debil",
                !attumaFx.Board.Zones[0].PlayerCards.Contains(attumaWeak),
                "no destruyo");

            var broodFx = NewPerkFixture(cards, effects, zones, 603);
            var brood = Place(cards, broodFx.Board.Zones[0], "card_brood", 1);
            broodFx.Effects.ResolveEndOfTurn(broodFx.Board,
                new List<CardInstance> { brood });
            Check("series3: Brood 2 Broodlings",
                broodFx.Board.Zones[0].PlayerCards.Count(c =>
                    c.Definition.Id == "token_broodling") == 2 &&
                broodFx.Board.Zones[0].PlayerCards
                    .Where(c => c.Definition.Id == "token_broodling")
                    .All(c => c.CurrentPower == 2),
                "broodlings incorrectos");

            var colleenFx = NewPerkFixture(cards, effects, zones, 604);
            var colleen = Place(cards, colleenFx.Board.Zones[0],
                "card_colleen_wing", 1);
            var cheap = new CardInstance(cards.Get("card_wasp"));
            var expensive = new CardInstance(cards.Get("card_hulk"));
            colleenFx.Board.PlayerHand.Add(cheap);
            colleenFx.Board.PlayerHand.Add(expensive);
            colleenFx.Effects.ResolveEndOfTurn(colleenFx.Board,
                new List<CardInstance> { colleen });
            Check("series3: Colleen descarta barata",
                !colleenFx.Board.PlayerHand.Contains(cheap) &&
                colleenFx.Board.PlayerHand.Contains(expensive),
                "descarte incorrecto");

            var destroyerFx = NewPerkFixture(cards, effects, zones, 605);
            var destroyer = Place(cards, destroyerFx.Board.Zones[0],
                "card_destroyer", 1);
            var otherAlly = Place(cards, destroyerFx.Board.Zones[1],
                "card_hulk", 2);
            destroyerFx.Effects.ResolveEndOfTurn(destroyerFx.Board,
                new List<CardInstance> { destroyer });
            Check("series3: Destroyer destruye otras zonas",
                !destroyerFx.Board.Zones[1].PlayerCards.Contains(otherAlly),
                "no destruyo");

            var ghostSpiderFx = NewPerkFixture(cards, effects, zones, 606);
            var ghostSpider = Place(cards, ghostSpiderFx.Board.Zones[0],
                "card_ghost_spider", 2);
            var lastAlly = Place(cards, ghostSpiderFx.Board.Zones[1],
                "card_hulk", 1);
            ghostSpiderFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.Player,
                Card = lastAlly,
                ZoneIndex = 1,
                Turn = 0,
                PlayOrder = 1
            });
            ghostSpiderFx.Effects.ResolveEndOfTurn(ghostSpiderFx.Board,
                new List<CardInstance> { ghostSpider });
            Check("series3: Ghost-Spider mueve ultima aliada",
                ghostSpiderFx.Board.Zones[0].PlayerCards.Contains(lastAlly),
                "no movio");

            var hillFx = NewPerkFixture(cards, effects, zones, 607);
            var hill = Place(cards, hillFx.Board.Zones[0], "card_maria_hill", 1);
            hillFx.Effects.ResolveEndOfTurn(hillFx.Board,
                new List<CardInstance> { hill });
            Check("series3: Maria Hill coste 2",
                hillFx.Board.PlayerHand.Count == 1 &&
                hillFx.Board.PlayerHand[0].Definition.Cost == 2,
                hillFx.Board.PlayerHand.Count.ToString());

            var mirageFx = NewPerkFixture(cards, effects, zones, 608);
            var mirage = Place(cards, mirageFx.Board.Zones[0], "card_mirage", 1);
            mirageFx.Board.AIHand.Add(new CardInstance(cards.Get("card_wasp")));
            mirageFx.Board.AIHand.Add(new CardInstance(cards.Get("card_hulk")));
            mirageFx.Effects.ResolveEndOfTurn(mirageFx.Board,
                new List<CardInstance> { mirage });
            Check("series3: Mirage copia barata +2",
                mirageFx.Board.PlayerHand.Count == 1 &&
                mirageFx.Board.PlayerHand[0].Definition.Id == "card_wasp" &&
                mirageFx.Board.PlayerHand[0].BonusPower == 2,
                mirageFx.Board.PlayerHand.Count.ToString());

            var furyFx = NewPerkFixture(cards, effects, zones, 609);
            var fury = Place(cards, furyFx.Board.Zones[0], "card_nick_fury", 1);
            furyFx.Effects.ResolveEndOfTurn(furyFx.Board,
                new List<CardInstance> { fury });
            Check("series3: Nick Fury 3 de coste 6",
                furyFx.Board.PlayerHand.Count == 3 &&
                furyFx.Board.PlayerHand.All(c => c.Definition.Cost == 6),
                furyFx.Board.PlayerHand.Count.ToString());

            var sauronFx = NewPerkFixture(cards, effects, zones, 610);
            var sauron = Place(cards, sauronFx.Board.Zones[0], "card_sauron", 1);
            var ongoingDeck = new CardInstance(cards.Get("card_iron_man"));
            sauronFx.Board.PlayerDeck.Add(ongoingDeck);
            sauronFx.Effects.ResolveEndOfTurn(sauronFx.Board,
                new List<CardInstance> { sauron });
            Check("series3: Sauron apaga Ongoing del mazo",
                ongoingDeck.PrintedAbilitiesDisabled,
                "no apago");

            var shannaFx = NewPerkFixture(cards, effects, zones, 611);
            var shanna = Place(cards, shannaFx.Board.Zones[0], "card_shanna", 1);
            shannaFx.Effects.ResolveEndOfTurn(shannaFx.Board,
                new List<CardInstance> { shanna });
            Check("series3: Shanna coste 1 por zona",
                shannaFx.Board.Zones.All(z =>
                    z.PlayerCards.Any(c => c.Definition.Cost == 1 &&
                        c.Definition.Id != "card_shanna")),
                "zonas incompletas");

            var ultronFx = NewPerkFixture(cards, effects, zones, 612);
            var ultron = Place(cards, ultronFx.Board.Zones[0], "card_ultron", 1);
            ultronFx.Effects.ResolveEndOfTurn(ultronFx.Board,
                new List<CardInstance> { ultron });
            Check("series3: Ultron 4 Drones",
                ultronFx.Board.Zones[1].PlayerCards.Count(c =>
                    c.Definition.Id == "token_drone") == 4 &&
                ultronFx.Board.Zones[2].PlayerCards.Count(c =>
                    c.Definition.Id == "token_drone") == 4,
                "drones incorrectos");

            var viperFx = NewPerkFixture(cards, effects, zones, 613);
            var viper = Place(cards, viperFx.Board.Zones[0], "card_viper", 1);
            var viperAlly = Place(cards, viperFx.Board.Zones[0],
                "card_cyclops", 2);
            viperFx.Effects.ResolveEndOfTurn(viperFx.Board,
                new List<CardInstance> { viper });
            Check("series3: Viper cambia de lado",
                viperFx.Board.Zones[0].AICards.Contains(viperAlly),
                "no cambio");

            var zeroFx = NewPerkFixture(cards, effects, zones, 614);
            var zero = Place(cards, zeroFx.Board.Zones[0], "card_zero", 1);
            zeroFx.Effects.ResolveEndOfTurn(zeroFx.Board,
                new List<CardInstance> { zero });
            var zeroTarget = new CardInstance(cards.Get("card_wasp"));
            zeroFx.Board.PlayerHand.Add(zeroTarget);
            zeroFx.Board.CurrentTurn = 3;
            zeroFx.Board.CurrentEnergy = 3;
            zeroFx.Actions.PlayCard(zeroTarget, BoardSide.Player, 1, 0);
            Check("series3: Zero silencia siguiente",
                zeroTarget.PrintedAbilitiesDisabled,
                "no silencio");
        }

        private static void RunSeries3Wave4Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave4 = new (string Id, int Cost, int Power)[]
            {
                ("card_death", 8, 12), ("card_knull", 6, 0),
                ("card_she_hulk", 6, 10), ("card_stature", 5, 7),
                ("card_miles_morales", 4, 6), ("card_sera", 5, 5),
                ("card_quinjet", 1, 1), ("card_zabu", 1, 1),
                ("card_mobius", 3, 3), ("card_wave", 3, 6),
                ("card_baron_mordo", 2, 3), ("card_beast", 3, 5),
                ("card_bast", 1, 1), ("card_valkyrie", 5, 3),
                ("card_shadow_king", 3, 4), ("card_black_panther", 5, 5),
                ("card_shuri", 4, 3), ("card_taskmaster", 6, 0),
                ("card_deadpool", 1, 1), ("card_mister_negative", 4, -1)
            };

            foreach (var entry in wave4)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var deathFx = NewPerkFixture(cards, effects, zones, 701);
            for (int i = 0; i < 3; i++)
                deathFx.Board.RecordDestroyedCard(
                    new CardInstance(cards.Get("card_hulk")),
                    BoardSide.Player, 1, 1, 1);
            var death = new CardInstance(cards.Get("card_death"));
            Check("series3: Death coste 5",
                deathFx.Actions.GetEffectiveCost(death, BoardSide.Player) == 5,
                deathFx.Actions.GetEffectiveCost(death, BoardSide.Player)
                    .ToString());

            var knullFx = NewPerkFixture(cards, effects, zones, 702);
            knullFx.Board.RecordDestroyedCard(
                new CardInstance(cards.Get("card_hulk")), BoardSide.Player,
                12, 6, 1);
            var knull = Place(cards, knullFx.Board.Zones[0], "card_knull", 1);
            knullFx.Board.PowerResolver.UpdateAllPowers(knullFx.Board);
            Check("series3: Knull 12", knull.CurrentPower == 12,
                knull.CurrentPower.ToString());

            var sheHulkFx = NewPerkFixture(cards, effects, zones, 703);
            sheHulkFx.Board.SetUnspentEnergyLastTurn(BoardSide.Player, 2);
            var sheHulk = new CardInstance(cards.Get("card_she_hulk"));
            Check("series3: She-Hulk coste 4",
                sheHulkFx.Actions.GetEffectiveCost(sheHulk,
                    BoardSide.Player) == 4,
                sheHulkFx.Actions.GetEffectiveCost(sheHulk,
                    BoardSide.Player).ToString());

            var statureFx = NewPerkFixture(cards, effects, zones, 704);
            statureFx.Board.IncrementDiscard(BoardSide.AI);
            var stature = new CardInstance(cards.Get("card_stature"));
            Check("series3: Stature coste 1",
                statureFx.Actions.GetEffectiveCost(stature,
                    BoardSide.Player) == 1,
                statureFx.Actions.GetEffectiveCost(stature,
                    BoardSide.Player).ToString());

            var milesFx = NewPerkFixture(cards, effects, zones, 705);
            milesFx.Board.CurrentTurn = 3;
            milesFx.Board.LastMoveTurn = 2;
            var miles = new CardInstance(cards.Get("card_miles_morales"));
            Check("series3: Miles coste 1",
                milesFx.Actions.GetEffectiveCost(miles, BoardSide.Player) == 1,
                milesFx.Actions.GetEffectiveCost(miles,
                    BoardSide.Player).ToString());

            var seraFx = NewPerkFixture(cards, effects, zones, 706);
            Place(cards, seraFx.Board.Zones[0], "card_sera", 1);
            var seraTarget = new CardInstance(cards.Get("card_hulk"));
            seraFx.Board.PlayerHand.Add(seraTarget);
            Check("series3: Sera -1 en mano",
                seraFx.Actions.GetEffectiveCost(seraTarget,
                    BoardSide.Player) == 5,
                seraFx.Actions.GetEffectiveCost(seraTarget,
                    BoardSide.Player).ToString());

            var quinjetFx = NewPerkFixture(cards, effects, zones, 707);
            Place(cards, quinjetFx.Board.Zones[0], "card_quinjet", 1);
            var created = new CardInstance(cards.Get("card_hulk"))
            {
                IsCreated = true
            };
            quinjetFx.Board.PlayerHand.Add(created);
            Check("series3: Quinjet -1 creada",
                quinjetFx.Actions.GetEffectiveCost(created,
                    BoardSide.Player) == 5,
                quinjetFx.Actions.GetEffectiveCost(created,
                    BoardSide.Player).ToString());

            var zabuFx = NewPerkFixture(cards, effects, zones, 708);
            Place(cards, zabuFx.Board.Zones[0], "card_zabu", 1);
            var fourCost = new CardInstance(cards.Get("card_the_thing"));
            zabuFx.Board.PlayerDeck.Add(fourCost);
            Check("series3: Zabu -1 en mazo",
                zabuFx.Actions.GetEffectiveCost(fourCost,
                    BoardSide.Player) == 3,
                zabuFx.Actions.GetEffectiveCost(fourCost,
                    BoardSide.Player).ToString());

            var mobiusFx = NewPerkFixture(cards, effects, zones, 709);
            Place(cards, mobiusFx.Board.Zones[0], "card_mobius", 1);
            var taxed = new CardInstance(cards.Get("card_hulk"))
            {
                TemporaryCostDelta = 1
            };
            mobiusFx.Board.PlayerHand.Add(taxed);
            Check("series3: Mobius no sube",
                mobiusFx.Actions.GetEffectiveCost(taxed,
                    BoardSide.Player) == 6,
                mobiusFx.Actions.GetEffectiveCost(taxed,
                    BoardSide.Player).ToString());

            var waveFx = NewPerkFixture(cards, effects, zones, 710);
            var wave = Place(cards, waveFx.Board.Zones[0], "card_wave", 1);
            waveFx.Effects.ResolveEndOfTurn(waveFx.Board,
                new List<CardInstance> { wave });
            var waveTarget = new CardInstance(cards.Get("card_hulk"));
            waveFx.Board.PlayerHand.Add(waveTarget);
            Check("series3: Wave tope 4",
                waveFx.Actions.GetEffectiveCost(waveTarget,
                    BoardSide.Player) == 4,
                waveFx.Actions.GetEffectiveCost(waveTarget,
                    BoardSide.Player).ToString());

            var mordoFx = NewPerkFixture(cards, effects, zones, 711);
            var mordo = Place(cards, mordoFx.Board.Zones[0],
                "card_baron_mordo", 1);
            var top = new CardInstance(cards.Get("card_wasp"));
            mordoFx.Board.AIDeck.Add(top);
            mordoFx.Effects.ResolveEndOfTurn(mordoFx.Board,
                new List<CardInstance> { mordo });
            Check("series3: Baron Mordo fija 6",
                top.FixedCost == 6 &&
                mordoFx.Actions.GetEffectiveCost(top, BoardSide.AI) == 6,
                top.FixedCost?.ToString() ?? "null");

            var beastFx = NewPerkFixture(cards, effects, zones, 712);
            var beast = Place(cards, beastFx.Board.Zones[0], "card_beast", 1);
            var beastAlly = Place(cards, beastFx.Board.Zones[0],
                "card_cyclops", 2);
            beastFx.Effects.ResolveEndOfTurn(beastFx.Board,
                new List<CardInstance> { beast });
            Check("series3: Beast devuelve y abarata",
                beastFx.Board.PlayerHand.Contains(beastAlly) &&
                beastAlly.PendingCostReduction == 1,
                "no devolvio");

            var bastFx = NewPerkFixture(cards, effects, zones, 713);
            var bast = Place(cards, bastFx.Board.Zones[0], "card_bast", 1);
            var bastTarget = new CardInstance(cards.Get("card_hulk"));
            bastFx.Board.PlayerHand.Add(bastTarget);
            bastFx.Effects.ResolveEndOfTurn(bastFx.Board,
                new List<CardInstance> { bast });
            Check("series3: Bast fija 3",
                bastTarget.BasePowerOverride == 3,
                bastTarget.BasePowerOverride?.ToString() ?? "null");

            var valkFx = NewPerkFixture(cards, effects, zones, 714);
            var valk = Place(cards, valkFx.Board.Zones[0], "card_valkyrie", 1);
            var valkAlly = Place(cards, valkFx.Board.Zones[0], "card_hulk", 2);
            valkFx.Effects.ResolveEndOfTurn(valkFx.Board,
                new List<CardInstance> { valk });
            Check("series3: Valkyrie fija 3",
                valkAlly.BasePowerOverride == 3,
                valkAlly.BasePowerOverride?.ToString() ?? "null");

            var shadowFx = NewPerkFixture(cards, effects, zones, 715);
            var shadow = Place(cards, shadowFx.Board.Zones[0],
                "card_shadow_king", 1);
            var shadowAlly = Place(cards, shadowFx.Board.Zones[0],
                "card_hulk", 2);
            shadowAlly.BonusPower = 5;
            shadowFx.Effects.ResolveEndOfTurn(shadowFx.Board,
                new List<CardInstance> { shadow });
            Check("series3: Shadow King resetea",
                shadowAlly.BonusPower == 0 &&
                shadowAlly.BasePowerOverride == 14,
                $"{shadowAlly.BonusPower}/{shadowAlly.BasePowerOverride}");

            var pantherFx = NewPerkFixture(cards, effects, zones, 716);
            var panther = Place(cards, pantherFx.Board.Zones[0],
                "card_black_panther", 1);
            pantherFx.Effects.ResolveEndOfTurn(pantherFx.Board,
                new List<CardInstance> { panther });
            Check("series3: Black Panther duplica",
                panther.BonusPower == 5,
                panther.BonusPower.ToString());

            var shuriFx = NewPerkFixture(cards, effects, zones, 717);
            var shuri = Place(cards, shuriFx.Board.Zones[0], "card_shuri", 1);
            shuriFx.Effects.ResolveEndOfTurn(shuriFx.Board,
                new List<CardInstance> { shuri });
            Check("series3: Shuri arma doble",
                shuriFx.Board.GetPendingNextPlay(BoardSide.Player)
                    .Any(p => p.Kind == PendingNextPlayKind.DoublePower),
                "sin pendiente");

            var taskFx = NewPerkFixture(cards, effects, zones, 718);
            var task = Place(cards, taskFx.Board.Zones[0], "card_taskmaster", 2);
            var lastPlayed = Place(cards, taskFx.Board.Zones[1], "card_hulk", 1);
            lastPlayed.CurrentPower = 9;
            taskFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.Player,
                Card = lastPlayed,
                ZoneIndex = 1,
                Turn = 0,
                PlayOrder = 1
            });
            taskFx.Effects.ResolveEndOfTurn(taskFx.Board,
                new List<CardInstance> { task });
            Check("series3: Taskmaster copia 9",
                task.BasePowerOverride == 9,
                task.BasePowerOverride?.ToString() ?? "null");

            var deadpoolFx = NewPerkFixture(cards, effects, zones, 719);
            var deadpool = Place(cards, deadpoolFx.Board.Zones[0],
                "card_deadpool", 1);
            deadpoolFx.Actions.DestroyCard(deadpool, BoardSide.Player,
                ActionCause.Effect);
            Check("series3: Deadpool vuelve con doble",
                deadpoolFx.Board.PlayerHand.Contains(deadpool) &&
                deadpool.BonusPower == 1,
                $"{deadpoolFx.Board.PlayerHand.Contains(deadpool)}/{deadpool.BonusPower}");

            var negativeFx = NewPerkFixture(cards, effects, zones, 720);
            var negative = Place(cards, negativeFx.Board.Zones[0],
                "card_mister_negative", 1);
            var negativeTarget = new CardInstance(cards.Get("card_hulk"));
            negativeFx.Board.PlayerDeck.Add(negativeTarget);
            negativeFx.Effects.ResolveEndOfTurn(negativeFx.Board,
                new List<CardInstance> { negative });
            Check("series3: Mister Negative intercambia",
                negativeTarget.Definition.Cost == 14 &&
                negativeTarget.Definition.BasePower == 6,
                $"{negativeTarget.Definition.Cost}/{negativeTarget.Definition.BasePower}");
        }

        private static void RunSeries3Wave5Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave5 = new (string Id, int Cost, int Power)[]
            {
                ("card_adam_warlock", 2, 0), ("card_crystal", 3, 6),
                ("card_maximus", 2, 6), ("card_jane_foster", 5, 9),
                ("card_ghost_rider", 4, 5), ("card_hela", 6, 6),
                ("card_doctor_octopus", 4, 8), ("card_black_bolt", 5, 8),
                ("card_moon_knight", 3, 3), ("card_silver_samurai", 4, 6),
                ("card_master_mold", 2, 3), ("card_electro", 3, 5),
                ("card_havok", 2, 1), ("card_black_cat", 1, 4),
                ("card_helicarrier", 6, 12)
            };

            foreach (var entry in wave5)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var adamWinFx = NewPerkFixture(cards, effects, zones, 801);
            var adam = Place(cards, adamWinFx.Board.Zones[0],
                "card_adam_warlock", 1);
            Place(cards, adamWinFx.Board.Zones[0], "card_hulk", 2);
            adamWinFx.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_wasp")));
            adamWinFx.Effects.ResolveEndOfTurn(adamWinFx.Board,
                new List<CardInstance> { adam });
            Check("series3: Adam Warlock roba ganando",
                adamWinFx.Board.PlayerHand.Count == 1,
                adamWinFx.Board.PlayerHand.Count.ToString());

            var adamLoseFx = NewPerkFixture(cards, effects, zones, 802);
            var adamLose = Place(cards, adamLoseFx.Board.Zones[0],
                "card_adam_warlock", 1);
            PlaceEnemy(cards, adamLoseFx.Board.Zones[0], "card_hulk", 1);
            adamLoseFx.Effects.ResolveEndOfTurn(adamLoseFx.Board,
                new List<CardInstance> { adamLose });
            Check("series3: Adam Warlock +1 perdiendo",
                adamLose.BonusPower == 1, adamLose.BonusPower.ToString());

            var crystalFx = NewPerkFixture(cards, effects, zones, 803);
            var crystal = Place(cards, crystalFx.Board.Zones[0],
                "card_crystal", 1);
            crystalFx.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_wasp")));
            crystalFx.Board.AIDeck.Add(
                new CardInstance(cards.Get("card_wasp")));
            crystalFx.Effects.ResolveEndOfTurn(crystalFx.Board,
                new List<CardInstance> { crystal });
            Check("series3: Crystal ambos roban",
                crystalFx.Board.PlayerHand.Count == 1 &&
                crystalFx.Board.AIHand.Count == 1,
                $"{crystalFx.Board.PlayerHand.Count}/{crystalFx.Board.AIHand.Count}");

            var maximusFx = NewPerkFixture(cards, effects, zones, 804);
            var maximus = Place(cards, maximusFx.Board.Zones[0],
                "card_maximus", 1);
            maximusFx.Board.AIDeck.Add(
                new CardInstance(cards.Get("card_wasp")));
            maximusFx.Board.AIDeck.Add(
                new CardInstance(cards.Get("card_cyclops")));
            maximusFx.Effects.ResolveEndOfTurn(maximusFx.Board,
                new List<CardInstance> { maximus });
            Check("series3: Maximus rival roba 2",
                maximusFx.Board.AIHand.Count == 2,
                maximusFx.Board.AIHand.Count.ToString());

            var janeFx = NewPerkFixture(cards, effects, zones, 805);
            var jane = Place(cards, janeFx.Board.Zones[0],
                "card_jane_foster", 1);
            janeFx.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_wasp")));
            janeFx.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_cyclops")));
            janeFx.Effects.ResolveEndOfTurn(janeFx.Board,
                new List<CardInstance> { jane });
            Check("series3: Jane Foster roba coste 0",
                janeFx.Board.PlayerHand.Count == 1 &&
                janeFx.Board.PlayerHand[0].Definition.Id == "card_wasp",
                janeFx.Board.PlayerHand.Count.ToString());

            var riderFx = NewPerkFixture(cards, effects, zones, 806);
            var rider = Place(cards, riderFx.Board.Zones[0],
                "card_ghost_rider", 1);
            var discarded = new CardInstance(cards.Get("card_hulk"))
            {
                Location = CardLocation.Discard
            };
            riderFx.Board.GetPiles(BoardSide.Player).Add(discarded,
                PileKind.Discard);
            riderFx.Effects.ResolveEndOfTurn(riderFx.Board,
                new List<CardInstance> { rider });
            Check("series3: Ghost Rider resucita",
                riderFx.Board.Zones[0].PlayerCards.Contains(discarded),
                "no resucito");

            var helaFx = NewPerkFixture(cards, effects, zones, 807);
            var hela = Place(cards, helaFx.Board.Zones[0], "card_hela", 1);
            var helaA = new CardInstance(cards.Get("card_wasp"));
            var helaB = new CardInstance(cards.Get("card_cyclops"));
            helaFx.Board.GetPiles(BoardSide.Player).Add(helaA,
                PileKind.Discard);
            helaFx.Board.GetPiles(BoardSide.Player).Add(helaB,
                PileKind.Discard);
            helaFx.Board.Zones[0].PlayerCards.Remove(hela);
            helaFx.Board.Zones[0].PlayerCards.Add(hela);
            helaFx.Effects.ResolveEndOfTurn(helaFx.Board,
                new List<CardInstance> { hela });
            Check("series3: Hela resucita costes distintos",
                helaFx.Board.Zones.Sum(z => z.PlayerCards.Count(c =>
                    c == helaA || c == helaB)) == 2,
                "resurreccion incompleta");

            var ockFx = NewPerkFixture(cards, effects, zones, 808);
            var ock = Place(cards, ockFx.Board.Zones[0],
                "card_doctor_octopus", 1);
            ockFx.Board.AIHand.Add(new CardInstance(cards.Get("card_hulk")));
            ockFx.Board.AIHand.Add(new CardInstance(cards.Get("card_wasp")));
            ockFx.Effects.ResolveEndOfTurn(ockFx.Board,
                new List<CardInstance> { ock });
            Check("series3: Doctor Octopus trae debil",
                ockFx.Board.Zones[0].AICards.Any(c =>
                    c.Definition.Id == "card_wasp"),
                "no trajo");

            var boltFx = NewPerkFixture(cards, effects, zones, 809);
            var bolt = Place(cards, boltFx.Board.Zones[0],
                "card_black_bolt", 1);
            boltFx.Board.AIHand.Add(new CardInstance(cards.Get("card_hulk")));
            boltFx.Board.AIHand.Add(new CardInstance(cards.Get("card_wasp")));
            boltFx.Effects.ResolveEndOfTurn(boltFx.Board,
                new List<CardInstance> { bolt });
            Check("series3: Black Bolt descarta barata",
                boltFx.Board.AIHand.Count == 1 &&
                boltFx.Board.AIHand[0].Definition.Id == "card_hulk",
                boltFx.Board.AIHand.Count.ToString());

            var moonFx = NewPerkFixture(cards, effects, zones, 810);
            var moon = Place(cards, moonFx.Board.Zones[0],
                "card_moon_knight", 1);
            var ownOdd = new CardInstance(cards.Get("card_wasp"));
            var ownEven = new CardInstance(cards.Get("card_hulk"));
            moonFx.Board.PlayerHand.Add(ownOdd);
            moonFx.Board.PlayerHand.Add(ownEven);
            moonFx.Board.AIHand.Add(new CardInstance(cards.Get("card_wasp")));
            moonFx.Board.AIHand.Add(new CardInstance(cards.Get("card_hulk")));
            moonFx.Effects.ResolveEndOfTurn(moonFx.Board,
                new List<CardInstance> { moon });
            Check("series3: Moon Knight descarta pares",
                !moonFx.Board.PlayerHand.Contains(ownEven) &&
                moonFx.Board.AIHand.Count == 1,
                "descarte incorrecto");

            var samuraiFx = NewPerkFixture(cards, effects, zones, 811);
            var samurai = Place(cards, samuraiFx.Board.Zones[0],
                "card_silver_samurai", 1);
            samuraiFx.Board.PlayerHand.Add(
                new CardInstance(cards.Get("card_wasp")));
            samuraiFx.Board.PlayerHand.Add(
                new CardInstance(cards.Get("card_hulk")));
            samuraiFx.Board.AIHand.Add(
                new CardInstance(cards.Get("card_wasp")));
            samuraiFx.Board.AIHand.Add(
                new CardInstance(cards.Get("card_hulk")));
            samuraiFx.Effects.ResolveEndOfTurn(samuraiFx.Board,
                new List<CardInstance> { samurai });
            Check("series3: Silver Samurai menor Poder",
                samuraiFx.Board.PlayerHand.Count == 1 &&
                samuraiFx.Board.AIHand.Count == 1,
                $"{samuraiFx.Board.PlayerHand.Count}/{samuraiFx.Board.AIHand.Count}");

            var moldFx = NewPerkFixture(cards, effects, zones, 812);
            var mold = Place(cards, moldFx.Board.Zones[0],
                "card_master_mold", 1);
            moldFx.Effects.ResolveEndOfTurn(moldFx.Board,
                new List<CardInstance> { mold });
            Check("series3: Master Mold 2 Sentinel",
                moldFx.Board.AIHand.Count == 2 &&
                moldFx.Board.AIHand.All(c =>
                    c.Definition.Id == "card_sentinel"),
                moldFx.Board.AIHand.Count.ToString());

            var electroFx = NewPerkFixture(cards, effects, zones, 813);
            var electro = Place(cards, electroFx.Board.Zones[0],
                "card_electro", 1);
            electroFx.Effects.ResolveEndOfTurn(electroFx.Board,
                new List<CardInstance> { electro });
            Check("series3: Electro +1 max y limite",
                electroFx.Board.GetMaxEnergyDelta(BoardSide.Player) == 1 &&
                electroFx.Effects.IsSideLimitedToOneCard(
                    electroFx.Board, BoardSide.Player),
                electroFx.Board.GetMaxEnergyDelta(BoardSide.Player)
                    .ToString());

            var havokFx = NewPerkFixture(cards, effects, zones, 814);
            var havok = Place(cards, havokFx.Board.Zones[0], "card_havok", 1);
            havokFx.Effects.ResolveEndOfTurn(havokFx.Board,
                new List<CardInstance> { havok });
            Check("series3: Havok -1 max y +3",
                havokFx.Board.GetMaxEnergyDelta(BoardSide.Player) == -1 &&
                havok.BonusPower == 3,
                $"{havokFx.Board.GetMaxEnergyDelta(BoardSide.Player)}/{havok.BonusPower}");

            var catFx = NewPerkFixture(cards, effects, zones, 815);
            var cat = new CardInstance(cards.Get("card_black_cat"));
            catFx.Board.PlayerHand.Add(cat);
            catFx.Effects.ResolveEndOfTurn(catFx.Board,
                new List<CardInstance>());
            Check("series3: Black Cat se descarta",
                !catFx.Board.PlayerHand.Contains(cat),
                "sigue en mano");

            var carrierFx = NewPerkFixture(cards, effects, zones, 816);
            var carrier = new CardInstance(cards.Get("card_helicarrier"));
            carrierFx.Board.PlayerHand.Add(carrier);
            carrierFx.Actions.DiscardCard(carrier, BoardSide.Player,
                ActionCause.Effect);
            Check("series3: Helicarrier llena mano",
                carrierFx.Board.PlayerHand.Count == BoardState.MaxHandSize,
                carrierFx.Board.PlayerHand.Count.ToString());
        }

        private static void RunSeries3Wave6Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave6 = new (string Id, int Cost, int Power)[]
            {
                ("card_arnim_zola", 6, 0), ("card_leader", 6, 5),
                ("card_nimrod", 5, 6), ("card_quake", 2, 3),
                ("card_echo", 1, 2), ("card_negasonic", 4, 4),
                ("card_silk", 2, 5), ("card_titania", 1, 5),
                ("card_hercules", 3, 5), ("card_nebula", 1, 1),
                ("card_juggernaut", 4, 5), ("card_spider_man", 2, 4)
            };

            foreach (var entry in wave6)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var zolaFx = NewPerkFixture(cards, effects, zones, 901);
            var zola = Place(cards, zolaFx.Board.Zones[0], "card_arnim_zola", 1);
            var zolaAlly = Place(cards, zolaFx.Board.Zones[0], "card_hulk", 2);
            zolaFx.Effects.ResolveEndOfTurn(zolaFx.Board,
                new List<CardInstance> { zola });
            Check("series3: Arnim Zola copia",
                !zolaFx.Board.Zones[0].PlayerCards.Contains(zolaAlly) &&
                zolaFx.Board.Zones[1].PlayerCards.Any(c =>
                    c.Definition.Id == "card_hulk") &&
                zolaFx.Board.Zones[2].PlayerCards.Any(c =>
                    c.Definition.Id == "card_hulk"),
                "no copio");

            var leaderFx = NewPerkFixture(cards, effects, zones, 902);
            var leader = Place(cards, leaderFx.Board.Zones[0], "card_leader", 1);
            var leaderTarget = PlaceEnemy(cards, leaderFx.Board.Zones[1],
                "card_hulk", 1);
            leaderFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.AI,
                Card = leaderTarget,
                ZoneIndex = 1,
                Turn = 0,
                PlayOrder = 1
            });
            leaderFx.Effects.ResolveEndOfTurn(leaderFx.Board,
                new List<CardInstance> { leader });
            Check("series3: Leader copia enemigo",
                leaderFx.Board.Zones[1].PlayerCards.Any(c =>
                    c.Definition.Id == "card_hulk"),
                "no copio");

            var nimrodFx = NewPerkFixture(cards, effects, zones, 903);
            var nimrod = Place(cards, nimrodFx.Board.Zones[0], "card_nimrod", 1);
            nimrodFx.Actions.DestroyCard(nimrod, BoardSide.Player,
                ActionCause.Effect);
            Check("series3: Nimrod copia a otras zonas",
                nimrodFx.Board.Zones[1].PlayerCards.Any(c =>
                    c.Definition.Id == "card_nimrod") &&
                nimrodFx.Board.Zones[2].PlayerCards.Any(c =>
                    c.Definition.Id == "card_nimrod"),
                "no copio");

            var quakeFx = NewPerkFixture(cards, effects, zones, 904);
            var quake = Place(cards, quakeFx.Board.Zones[1], "card_quake", 1);
            string before0 = quakeFx.Board.Zones[0].Definition.Id;
            string before2 = quakeFx.Board.Zones[2].Definition.Id;
            quakeFx.Effects.ResolveEndOfTurn(quakeFx.Board,
                new List<CardInstance> { quake });
            Check("series3: Quake intercambia zonas",
                quakeFx.Board.Zones[0].Definition.Id == before2 &&
                quakeFx.Board.Zones[2].Definition.Id == before0,
                "no intercambio");

            var echoFx = NewPerkFixture(cards, effects, zones, 905);
            var echo = Place(cards, echoFx.Board.Zones[0], "card_echo", 1);
            var echoTarget = new CardInstance(cards.Get("card_iron_man"));
            echoFx.Board.AIHand.Add(echoTarget);
            echoFx.Board.CurrentTurn = 5;
            echoFx.Board.CurrentEnergy = 5;
            echoFx.Actions.PlayCard(echoTarget, BoardSide.AI, 0, 0);
            Check("series3: Echo silencia Ongoing",
                echoTarget.PrintedAbilitiesDisabled,
                "no silencio");

            var negasonicFx = NewPerkFixture(cards, effects, zones, 906);
            var negasonic = Place(cards, negasonicFx.Board.Zones[0],
                "card_negasonic", 1);
            var negasonicTarget = new CardInstance(cards.Get("card_hulk"));
            negasonicFx.Board.AIHand.Add(negasonicTarget);
            negasonicFx.Board.CurrentTurn = 5;
            negasonicFx.Board.CurrentEnergy = 5;
            negasonicFx.Actions.PlayCard(negasonicTarget, BoardSide.AI, 0, 0);
            Check("series3: Negasonic destruye",
                !negasonicFx.Board.Zones[0].AICards.Contains(negasonicTarget),
                "no destruyo");

            var silkFx = NewPerkFixture(cards, effects, zones, 907);
            var silk = Place(cards, silkFx.Board.Zones[0], "card_silk", 1);
            var silkTarget = new CardInstance(cards.Get("card_wasp"));
            silkFx.Board.PlayerHand.Add(silkTarget);
            silkFx.Board.CurrentTurn = 3;
            silkFx.Board.CurrentEnergy = 3;
            silkFx.Actions.PlayCard(silkTarget, BoardSide.Player, 0, 0);
            Check("series3: Silk se mueve",
                !silkFx.Board.Zones[0].PlayerCards.Contains(silk),
                "no se movio");

            var titaniaFx = NewPerkFixture(cards, effects, zones, 908);
            var titania = Place(cards, titaniaFx.Board.Zones[0],
                "card_titania", 1);
            var titaniaTarget = new CardInstance(cards.Get("card_wasp"));
            titaniaFx.Board.PlayerHand.Add(titaniaTarget);
            titaniaFx.Board.CurrentTurn = 3;
            titaniaFx.Board.CurrentEnergy = 3;
            titaniaFx.Actions.PlayCard(titaniaTarget, BoardSide.Player, 0, 0);
            Check("series3: Titania cambia de lado",
                titaniaFx.Board.Zones[0].AICards.Contains(titania),
                "no cambio");

            var herculesFx = NewPerkFixture(cards, effects, zones, 909);
            var hercules = Place(cards, herculesFx.Board.Zones[0],
                "card_hercules", 1);
            var herculesAlly = Place(cards, herculesFx.Board.Zones[1],
                "card_hulk", 2);
            herculesFx.Actions.MoveCard(herculesAlly, BoardSide.Player, 1, 0,
                ActionCause.Effect);
            Check("series3: Hercules re-mueve",
                !herculesFx.Board.Zones[0].PlayerCards.Contains(herculesAlly),
                "no re-movio");

            var nebulaFx = NewPerkFixture(cards, effects, zones, 910);
            var nebula = Place(cards, nebulaFx.Board.Zones[0], "card_nebula", 1);
            nebulaFx.Board.CurrentTurn = 2;
            nebula.PlayedTurn = 1;
            nebulaFx.Effects.ResolveEndOfTurn(nebulaFx.Board,
                new List<CardInstance> { nebula });
            Check("series3: Nebula +2", nebula.BonusPower == 2,
                nebula.BonusPower.ToString());

            var juggernautFx = NewPerkFixture(cards, effects, zones, 911);
            var juggernaut = Place(cards, juggernautFx.Board.Zones[0],
                "card_juggernaut", 1);
            var juggernautTarget = PlaceEnemy(cards, juggernautFx.Board.Zones[0],
                "card_hulk", 1);
            juggernautFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.AI,
                Card = juggernautTarget,
                ZoneIndex = 0,
                Turn = 0,
                PlayOrder = 1
            });
            juggernautFx.Effects.ResolveEndOfTurn(juggernautFx.Board,
                new List<CardInstance> { juggernaut });
            Check("series3: Juggernaut aleja",
                !juggernautFx.Board.Zones[0].AICards.Contains(
                    juggernautTarget),
                "no alejo");

            var spiderFx = NewPerkFixture(cards, effects, zones, 912);
            var spider = Place(cards, spiderFx.Board.Zones[0],
                "card_spider_man", 1);
            var spiderTarget = PlaceEnemy(cards, spiderFx.Board.Zones[0],
                "card_hulk", 1);
            spiderFx.Effects.ResolveEndOfTurn(spiderFx.Board,
                new List<CardInstance> { spider });
            Check("series3: Spider-Man arrastra",
                !spiderFx.Board.Zones[0].PlayerCards.Contains(spider) &&
                !spiderFx.Board.Zones[0].AICards.Contains(spiderTarget),
                "no arrastro");
        }

        private static void RunSeries3Wave7Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave7 = new (string Id, int Cost, int Power)[]
            {
                ("card_crossbones", 4, 10), ("card_giganto", 6, 16),
                ("card_sentry", 4, 9), ("card_goose", 2, 2),
                ("card_jean_grey", 3, 4), ("card_magik", 3, 2)
            };

            foreach (var entry in wave7)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var crossFx = NewPerkFixture(cards, effects, zones, 1001);
            var cross = new CardInstance(cards.Get("card_crossbones"));
            crossFx.Board.CurrentTurn = 3;
            Place(cards, crossFx.Board.Zones[0], "card_hulk", 1);
            PlaceEnemy(cards, crossFx.Board.Zones[1], "card_hulk", 1);
            Check("series3: Crossbones solo ganando",
                !crossFx.Effects.CanPlayCard(cross, crossFx.Board,
                    BoardSide.Player, crossFx.Board.Zones[1]) &&
                crossFx.Effects.CanPlayCard(cross, crossFx.Board,
                    BoardSide.Player, crossFx.Board.Zones[0]),
                "regla incorrecta");

            var gigantoFx = NewPerkFixture(cards, effects, zones, 1002);
            var giganto = new CardInstance(cards.Get("card_giganto"));
            Check("series3: Giganto solo izquierda",
                gigantoFx.Effects.CanPlayCard(giganto, gigantoFx.Board,
                    BoardSide.Player, gigantoFx.Board.Zones[0]) &&
                !gigantoFx.Effects.CanPlayCard(giganto, gigantoFx.Board,
                    BoardSide.Player, gigantoFx.Board.Zones[1]),
                "regla incorrecta");

            var sentryFx = NewPerkFixture(cards, effects, zones, 1003);
            var sentry = new CardInstance(cards.Get("card_sentry"));
            Check("series3: Sentry no derecha",
                sentryFx.Effects.CanPlayCard(sentry, sentryFx.Board,
                    BoardSide.Player, sentryFx.Board.Zones[0]) &&
                !sentryFx.Effects.CanPlayCard(sentry, sentryFx.Board,
                    BoardSide.Player, sentryFx.Board.Zones[2]),
                "regla incorrecta");
            var sentryCard = Place(cards, sentryFx.Board.Zones[0],
                "card_sentry", 1);
            sentryFx.Effects.ResolveEndOfTurn(sentryFx.Board,
                new List<CardInstance> { sentryCard });
            Check("series3: Sentry agrega Vacio",
                sentryFx.Board.Zones[2].PlayerCards.Any(c =>
                    c.Definition.Id == "token_void"),
                "sin Vacio");

            var gooseFx = NewPerkFixture(cards, effects, zones, 1004);
            Place(cards, gooseFx.Board.Zones[0], "card_goose", 1);
            var gooseBig = new CardInstance(cards.Get("card_hulk"));
            var gooseSmall = new CardInstance(cards.Get("card_cyclops"));
            Check("series3: Goose bloquea coste 4",
                !gooseFx.Actions.CanPlayCardInZone(gooseBig,
                    BoardSide.Player, gooseFx.Board.Zones[0]) &&
                gooseFx.Actions.CanPlayCardInZone(gooseSmall,
                    BoardSide.Player, gooseFx.Board.Zones[0]),
                "regla incorrecta");

            var jeanFx = NewPerkFixture(cards, effects, zones, 1005);
            Place(cards, jeanFx.Board.Zones[0], "card_jean_grey", 1);
            var jeanCard = new CardInstance(cards.Get("card_cyclops"));
            Check("series3: Jean Grey fuerza zona",
                jeanFx.Effects.GetForcedZone(jeanFx.Board,
                    BoardSide.Player) == 0 &&
                !jeanFx.Actions.CanPlayCardInZone(jeanCard,
                    BoardSide.Player, jeanFx.Board.Zones[1]),
                "no fuerza");

            var magikFx = NewPerkFixture(cards, effects, zones, 1006);
            var magik = Place(cards, magikFx.Board.Zones[0], "card_magik", 1);
            magikFx.Board.CurrentTurn = 5;
            magikFx.Effects.ResolveEndOfTurn(magikFx.Board,
                new List<CardInstance> { magik });
            Check("series3: Magik crea Limbo",
                magikFx.Board.Zones[0].Definition.Id == "zone_limbo" &&
                TurnManager.MaxTurnsFor(magikFx.Board) == 7,
                magikFx.Board.Zones[0].Definition.Id);
        }

        private static void RunSeries3Wave8Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave8 = new (string Id, int Cost, int Power)[]
            {
                ("card_absorbing_man", 4, 5), ("card_mystique", 3, 1),
                ("card_rogue", 3, 3), ("card_super_skrull", 4, 2),
                ("card_black_widow", 3, 3), ("card_hellcow", 4, 7),
                ("card_lockjaw", 2, 3), ("card_mysterio", 2, 5),
                ("card_spider_ham", 1, 1)
            };

            foreach (var entry in wave8)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var absorbFx = NewPerkFixture(cards, effects, zones, 1101);
            var absorb = Place(cards, absorbFx.Board.Zones[0],
                "card_absorbing_man", 2);
            var absorbSource = Place(cards, absorbFx.Board.Zones[1],
                "card_medusa", 1);
            absorbFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.Player,
                Card = absorbSource,
                ZoneIndex = 1,
                Turn = 0,
                PlayOrder = 1
            });
            absorbFx.Effects.ResolveEndOfTurn(absorbFx.Board,
                new List<CardInstance> { absorb });
            Check("series3: Absorbing Man copia OnReveal",
                absorb.CopiedEffectIds.Contains("effect_medusa_on_reveal"),
                "no copio");

            var mystiqueFx = NewPerkFixture(cards, effects, zones, 1102);
            var mystique = Place(cards, mystiqueFx.Board.Zones[0],
                "card_mystique", 2);
            var mystiqueSource = Place(cards, mystiqueFx.Board.Zones[1],
                "card_iron_man", 1);
            mystiqueFx.Board.AddPlayRecord(new PlayRecord
            {
                Side = BoardSide.Player,
                Card = mystiqueSource,
                ZoneIndex = 1,
                Turn = 0,
                PlayOrder = 1
            });
            mystiqueFx.Effects.ResolveEndOfTurn(mystiqueFx.Board,
                new List<CardInstance> { mystique });
            Check("series3: Mystique copia Ongoing",
                mystique.CopiedEffectIds.Contains("effect_iron_man_ongoing"),
                "no copio");

            var rogueFx = NewPerkFixture(cards, effects, zones, 1103);
            var rogue = Place(cards, rogueFx.Board.Zones[0], "card_rogue", 1);
            var rogueTarget = PlaceEnemy(cards, rogueFx.Board.Zones[0],
                "card_iron_man", 1);
            rogueFx.Effects.ResolveEndOfTurn(rogueFx.Board,
                new List<CardInstance> { rogue });
            Check("series3: Rogue roba Ongoing",
                rogue.CopiedEffectIds.Contains("effect_iron_man_ongoing") &&
                rogueTarget.SuppressedEffectIds.Contains(
                    "effect_iron_man_ongoing"),
                "no robo");

            var skrullFx = NewPerkFixture(cards, effects, zones, 1104);
            var skrull = Place(cards, skrullFx.Board.Zones[0],
                "card_super_skrull", 1);
            PlaceEnemy(cards, skrullFx.Board.Zones[0],
                "card_devil_dinosaur", 1);
            skrullFx.Board.AIHand.Add(new CardInstance(cards.Get("card_hulk")));
            skrullFx.Board.AIHand.Add(new CardInstance(cards.Get("card_cyclops")));
            skrullFx.Board.PowerResolver.UpdateAllPowers(skrullFx.Board);
            Check("series3: Super-Skrull copia Ongoing",
                skrull.CurrentPower > 2,
                skrull.CurrentPower.ToString());

            var widowFx = NewPerkFixture(cards, effects, zones, 1105);
            var widow = Place(cards, widowFx.Board.Zones[0],
                "card_black_widow", 1);
            widowFx.Actions.ActivateCard(widow, BoardSide.Player);
            Check("series3: Black Widow Mordida",
                widowFx.Board.AIHand.Any(c =>
                    c.Definition.Id == "token_widows_bite"),
                "sin Mordida");

            var hellcowFx = NewPerkFixture(cards, effects, zones, 1106);
            var hellcow = Place(cards, hellcowFx.Board.Zones[0],
                "card_hellcow", 1);
            hellcowFx.Board.PlayerHand.Add(
                new CardInstance(cards.Get("card_wasp")));
            hellcowFx.Actions.ActivateCard(hellcow, BoardSide.Player);
            Check("series3: Hellcow descarta",
                hellcowFx.Board.PlayerHand.Count == 0,
                hellcowFx.Board.PlayerHand.Count.ToString());

            var lockjawFx = NewPerkFixture(cards, effects, zones, 1107);
            var lockjaw = Place(cards, lockjawFx.Board.Zones[0],
                "card_lockjaw", 1);
            lockjawFx.Board.CurrentTurn = 3;
            lockjawFx.Board.CurrentEnergy = 3;
            lockjawFx.Board.PlayerDeck.Add(
                new CardInstance(cards.Get("card_hulk")));
            var lockjawPlay = new CardInstance(cards.Get("card_wasp"));
            lockjawFx.Board.PlayerHand.Add(lockjawPlay);
            lockjawFx.Actions.ActivateCard(lockjaw, BoardSide.Player);
            lockjawFx.Actions.PlayCard(lockjawPlay, BoardSide.Player, 0, 1);
            Check("series3: Lockjaw intercambia",
                lockjawFx.Board.PlayerDeck.Contains(lockjawPlay) &&
                lockjawFx.Board.Zones[0].PlayerCards.Any(c =>
                    c.Definition.Id == "card_hulk"),
                "no intercambio");

            var mysterioFx = NewPerkFixture(cards, effects, zones, 1108);
            var mysterio = Place(cards, mysterioFx.Board.Zones[0],
                "card_mysterio", 1);
            mysterioFx.Effects.ResolveEndOfTurn(mysterioFx.Board,
                new List<CardInstance> { mysterio });
            var illusion = mysterioFx.Board.Zones.SelectMany(z => z.PlayerCards)
                .FirstOrDefault(c => c.Definition.Id == "token_illusion");
            mysterioFx.Actions.ActivateCard(mysterio, BoardSide.Player);
            Check("series3: Mysterio intercambia Poder",
                illusion != null && mysterio.BasePowerOverride == 0 &&
                illusion.BasePowerOverride == 5,
                illusion == null ? "sin Ilusion" :
                    $"{mysterio.BasePowerOverride}/{illusion.BasePowerOverride}");

            var hamFx = NewPerkFixture(cards, effects, zones, 1109);
            var ham = Place(cards, hamFx.Board.Zones[0], "card_spider_ham", 1);
            var hamTarget = new CardInstance(cards.Get("card_hulk"));
            hamFx.Board.AIHand.Add(hamTarget);
            hamFx.Actions.ActivateCard(ham, BoardSide.Player);
            Check("series3: Spider-Ham oink",
                hamTarget.PrintedAbilitiesDisabled &&
                hamTarget.AbilityTextOverride == "oink!",
                hamTarget.AbilityTextOverride ?? "sin texto");
        }

        private static void RunSeries3Wave9Checks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var wave9 = new (string Id, int Cost, int Power)[]
            {
                ("card_captain_marvel", 4, 6), ("card_dracula", 4, 1),
                ("card_m_baku", 1, 3), ("card_martyr", 1, 5),
                ("card_invisible_woman", 2, 4), ("card_snowguard", 1, 2),
                ("card_agatha", 6, 12), ("card_daredevil", 2, 3),
                ("card_howard", 1, 2)
            };

            foreach (var entry in wave9)
                CheckSeries3Card(cards, effects, entry.Id, entry.Cost, entry.Power);

            var marvelFx = NewPerkFixture(cards, effects, zones, 1201);
            var marvel = Place(cards, marvelFx.Board.Zones[0],
                "card_captain_marvel", 1);
            marvelFx.Effects.ResolveEndOfGame(marvelFx.Board);
            Check("series3: Captain Marvel busca ganar",
                !marvelFx.Board.Zones[0].PlayerCards.Contains(marvel),
                "no se movio");

            var draculaFx = NewPerkFixture(cards, effects, zones, 1202);
            var dracula = Place(cards, draculaFx.Board.Zones[0],
                "card_dracula", 1);
            draculaFx.Board.PlayerHand.Add(
                new CardInstance(cards.Get("card_hulk")));
            draculaFx.Effects.ResolveEndOfGame(draculaFx.Board);
            Check("series3: Dracula gana Poder",
                dracula.BonusPower == 14,
                dracula.BonusPower.ToString());

            var mbakuFx = NewPerkFixture(cards, effects, zones, 1203);
            var mbaku = new CardInstance(cards.Get("card_m_baku"));
            mbakuFx.Board.PlayerDeck.Add(mbaku);
            mbakuFx.Effects.ResolveEndOfGame(mbakuFx.Board);
            Check("series3: M'Baku salta del mazo",
                mbakuFx.Board.Zones.Any(z => z.PlayerCards.Contains(mbaku)),
                "no salto");

            var martyrFx = NewPerkFixture(cards, effects, zones, 1204);
            var martyr = Place(cards, martyrFx.Board.Zones[0],
                "card_martyr", 1);
            PlaceEnemy(cards, martyrFx.Board.Zones[1], "card_hulk", 1);
            martyrFx.Effects.ResolveEndOfGame(martyrFx.Board);
            Check("series3: Martyr busca perder",
                martyrFx.Board.Zones[1].PlayerCards.Contains(martyr),
                "no se movio");

            var invisibleFx = NewPerkFixture(cards, effects, zones, 1205);
            var invisible = Place(cards, invisibleFx.Board.Zones[0],
                "card_invisible_woman", 1);
            var hidden = Place(cards, invisibleFx.Board.Zones[0],
                "card_medusa", 2);
            invisibleFx.Effects.ResolveEndOfTurn(invisibleFx.Board,
                new List<CardInstance> { hidden });
            Check("series3: Invisible Woman difiere",
                hidden.DelayedReveal && !hidden.HasRevealed,
                $"{hidden.DelayedReveal}/{hidden.HasRevealed}");
            invisibleFx.Effects.ResolveEndOfGame(invisibleFx.Board);
            Check("series3: Invisible Woman revela al final",
                hidden.HasRevealed && !hidden.DelayedReveal,
                $"{hidden.HasRevealed}/{hidden.DelayedReveal}");

            var snowFx = NewPerkFixture(cards, effects, zones, 1206);
            var snow = Place(cards, snowFx.Board.Zones[0], "card_snowguard", 1);
            snowFx.Effects.ResolveEndOfTurn(snowFx.Board,
                new List<CardInstance> { snow });
            Check("series3: Snowguard agrega Hawk y Bear",
                snowFx.Board.PlayerHand.Any(c =>
                    c.Definition.Id == "token_snowguard_hawk") &&
                snowFx.Board.PlayerHand.Any(c =>
                    c.Definition.Id == "token_snowguard_bear"),
                "faltan auroras");

            var agathaFx = NewPerkFixture(cards, effects, zones, 1207);
            var agatha = new CardInstance(cards.Get("card_agatha"));
            agathaFx.Board.PlayerHand.Add(agatha);
            agathaFx.Effects.Dispatch(agathaFx.Board, new ActionEvent
            {
                Kind = ActionEventKind.GameStart,
                Side = BoardSide.Player,
                Turn = 0,
                Cause = ActionCause.Effect
            });
            Check("series3: Agatha marca turnos pares",
                agathaFx.Board.AgathaControlsEvenTurns,
                "sin marca");

            Check("series3: Daredevil y Howard registrados",
                cards.Get("card_daredevil") != null &&
                cards.Get("card_howard") != null,
                "ausentes");
        }

        private static void RunSeries3EndToEndChecks(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones)
        {
            var fixture = NewPerkFixture(cards, effects, zones, 1301);
            var ids = new[]
            {
                "card_drax", "card_polaris", "card_brood", "card_death",
                "card_crystal", "card_quake", "card_goose",
                "card_black_widow", "card_captain_marvel"
            };

            foreach (string id in ids)
                fixture.Board.PlayerHand.Add(new CardInstance(cards.Get(id))
                    { Location = CardLocation.Hand });

            for (int turn = 1; turn <= 6; turn++)
            {
                fixture.Board.CurrentTurn = turn;
                fixture.Board.ResetTurnTracking();

                foreach (var card in fixture.Board.PlayerHand.ToList())
                {
                    bool played = false;
                    for (int zone = 0; zone < fixture.Board.Zones.Count; zone++)
                    {
                        if (fixture.Actions.PlayCard(card, BoardSide.Player,
                            zone, 0).Success)
                        {
                            played = true;
                            break;
                        }
                    }

                    if (played) break;
                }

                fixture.Board.EndOfTurnResolvedTurn = -1;
                var reveal = fixture.Board.Zones
                    .SelectMany(zone => zone.PlayerCards)
                    .Where(card => card.PlayedTurn == turn && !card.HasRevealed)
                    .ToList();
                fixture.Effects.ResolveEndOfTurn(fixture.Board, reveal);
            }

            fixture.Effects.ResolveEndOfGame(fixture.Board);
            fixture.Board.PowerResolver.UpdateAllPowers(fixture.Board);
            Check("series3: combate de 6 turnos sin excepcion", true, null);
        }

        private static void CheckSeries3Card(CardRegistry cards,
            EffectRegistry effects, string id, int cost, int power)        {
            var card = cards.Get(id);
            Check($"series3: {id} registrada",
                card != null && card.Cost == cost && card.BasePower == power &&
                card.Series == CardSeries.Series3,
                card == null ? "ausente" : $"{card.Cost}/{card.BasePower}");
            Check($"series3: {id} con descripcion",
                card != null && !string.IsNullOrWhiteSpace(card.Description),
                "descripcion vacia");

            if (card?.EffectIds == null) return;

            foreach (string effectId in card.EffectIds)
            {
                Check($"series3: {id} efecto {effectId}",
                    effects.Get(effectId) != null, "efecto ausente");
            }
        }

        private static CombatFixture NewPerkFixture(CardRegistry cards,
            EffectRegistry effects, ZoneRegistry zones, int seed)
        {
            var run = new RunState();
            run.StartNewRun();

            var board = NewBoard();
            var effectProcessor = new EffectProcessor(effects, cards,
                new RandomService(seed));
            var resolver = new PowerResolver(null, effectProcessor);
            board.PowerResolver = resolver;

            var actions = new CardActionService(board,
                new CardPileManager(board, run), resolver, effectProcessor, run,
                zones);

            return new CombatFixture
            {
                Board = board,
                Actions = actions,
                Effects = effectProcessor,
                Items = null
            };
        }

        private static CardInstance PlaceCustom(ZoneInstance zone, string id,
            int cost, int power, params string[] effectIds)
        {
            var definition = new CardDef
            {
                Id = id,
                Name = id,
                Cost = cost,
                BasePower = power,
                Description = string.Empty,
                EffectIds = new List<string>(effectIds ?? new string[0])
            };

            var card = new CardInstance(definition)
            {
                PlayOrderIndex = 1,
                PlayedTurn = 1,
                PlayedZoneIndex = zone.ZoneIndex,
                IsInPlay = true,
                Location = CardLocation.Board
            };

            zone.PlayerCards.Add(card);
            return card;
        }

        private sealed class TestOngoingPowerEffect : CardEffectBase,
            ICardPowerModifier
        {
            public int Amount { get; set; }

            public int GetPowerBonus(EffectContext context) => Amount;
        }

        private sealed class TestCountingEffect : IEffect
        {
            public string EffectId { get; set; }
            public EffectTrigger Trigger { get; set; }
            public int Count { get; private set; }

            public void Apply(EffectContext context) => Count++;
        }

        private sealed class TestLoopEffect : IEffect
        {
            public string EffectId { get; set; } = "test_loop";
            public EffectTrigger Trigger { get; set; } = EffectTrigger.AfterMoved;

            public void Apply(EffectContext context)
            {
                context.EmitEvent?.Invoke(new ActionEvent
                {
                    Kind = ActionEventKind.Moved,
                    SourceCard = context.SourceCard,
                    Side = context.Owner,
                    OriginZoneIndex = 0,
                    DestinationZoneIndex = 1,
                    Turn = context.TurnNumber
                });
            }
        }

        private static ProfileDto ProfileWith(string itemId, int count)
        {
            var profile = ProfileDto.CreateDefault(StarterSet.CardIds);
            profile.ItemCounts[itemId] = count;
            return profile;
        }

        private static CardInstance PlaceEnemy(CardRegistry cards, ZoneInstance zone,
            string cardId, int playOrder)
        {
            var card = new CardInstance(cards.Get(cardId))
            {
                PlayOrderIndex = playOrder,
                PlayedTurn = 1,
                PlayedZoneIndex = zone.ZoneIndex,
                IsInPlay = true,
                Location = CardLocation.Board
            };

            zone.AICards.Add(card);
            return card;
        }

        private static int BoostedZoneIndex(CombatFixture fixture)
        {
            for (int i = 0; i < fixture.Board.Zones.Count; i++)
            {
                foreach (var card in fixture.Board.Zones[i].PlayerCards)
                {
                    if (card.BonusPower != 0)
                        return i;
                }
            }

            return -1;
        }

        // ------------------------------------------------------------------
        // Utilidades
        // ------------------------------------------------------------------

        private static RunState NewRun()
        {
            var run = new RunState();
            run.StartNewRun();
            return run;
        }

        private static BoardState NewBoard()
        {
            var board = new BoardState();
            for (int i = 0; i < 3; i++)
            {
                var zone = new ZoneInstance(new ZoneDef
                {
                    Id = $"zone_{i}",
                    Name = $"ZONA {i + 1}",
                    Description = string.Empty
                }, i)
                {
                    Board = board
                };

                board.Zones.Add(zone);
            }

            return board;
        }

        private static CardInstance Place(CardRegistry cards, ZoneInstance zone,
            string cardId, int playOrder)
        {
            var card = new CardInstance(cards.Get(cardId))
            {
                PlayOrderIndex = playOrder,
                PlayedTurn = 1,
                PlayedZoneIndex = zone.ZoneIndex,
                IsInPlay = true,
                Location = CardLocation.Board
            };

            zone.PlayerCards.Add(card);
            return card;
        }

        private static PlayRecord Record(CardInstance card, int playOrder) =>
            new PlayRecord
            {
                Side = BoardSide.Player,
                Card = card,
                ZoneIndex = card.PlayedZoneIndex,
                Turn = card.PlayedTurn,
                PlayOrder = playOrder
            };

        private static void AddHost(RunState run, CardRegistry cards, string cardId,
            string perkId, int copies)
        {
            var host = new CardInstance(cards.Get(cardId));
            for (int i = 0; i < copies; i++)
                host.AttachedPerks.Add(perkId);

            run.Deck.Add(host);
        }

        private static void Check(string name, bool condition, string detail)
        {
            if (condition) return;

            _failures++;
            Failures.Add(detail == null ? name : $"{name} ({detail})");
        }
    }
}
