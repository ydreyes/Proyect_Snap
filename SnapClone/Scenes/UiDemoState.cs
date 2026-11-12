using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Estado de demostracion para la galeria de UI. Solo se usa cuando la
    /// galeria se abre con la bandera de desarrollo; no participa del flujo
    /// normal del juego.
    /// </summary>
    public static class UiDemoState
    {
        public static void Prepare(GameServices services, string screen)
        {
            if (services == null || string.IsNullOrEmpty(screen)) return;

            var checkpoint = services.Get<RunCheckpointService>();
            EnsureProfilePoints(checkpoint);

            if (screen == "Menu" || screen == "GlobalStore")
                return;

            var run = services.Get<RunState>();

            checkpoint.DeleteRun();
            checkpoint.StartNewRun();

            PrepareRun(services, run, screen);
        }

        private static void EnsureProfilePoints(RunCheckpointService checkpoint)
        {
            var profile = checkpoint?.Profile;
            if (profile == null || profile.Points >= 1500) return;

            profile.Points = 1500;
            checkpoint.SaveProfile(out _);
        }

        private static void PrepareRun(GameServices services, RunState run,
            string screen)
        {
            var registry = services.Get<CardRegistry>();
            FillDeck(registry, run,
                screen == "Summary" ? RunState.MaxDeckSize : 8);

            run.Level = 9;
            run.Xp = 120;
            run.XpEarned = 320;
            run.Money = 900;
            run.MoneyEarned = 4200;
            run.ActIndex = screen == "Summary" ? RunActTable.FinalAct : 3;
            run.TotalCombats = screen == "Summary"
                ? RunState.MaxCombats
                : 26;
            run.NormalCombatsWon = screen == "Summary" ? 43 : 22;
            run.SubBossesWon = screen == "Summary" ? 4 : 2;
            run.FinalBossDefeated = screen == "Summary";
            run.CombatsWon = run.TotalCombats;

            switch (screen)
            {
                case "Draft":
                    run.PendingNode = RunNode.Draft;
                    break;

                case "Shop":
                    run.PendingNode = RunNode.Shop;
                    run.ShopState = new ShopState();
                    services.Get<IngameShopSystem>().EnsureStock(run);
                    break;

                case "LevelUp":
                    run.PendingNode = RunNode.LevelUp;
                    run.PendingLevelUps.Clear();
                    run.PendingLevelUps.Add(new LevelUpState(run.Level + 1));
                    break;

                case "Summary":
                    run.Outcome = RunOutcome.Won;
                    run.PendingNode = RunNode.Summary;
                    break;
            }
        }

        private static void FillDeck(CardRegistry registry, RunState run,
            int count)
        {
            var catalog = registry.GetDraftable()
                .OrderBy(card => card.Id, StringComparer.Ordinal)
                .ToList();

            run.Deck.Clear();

            for (int i = 0; i < catalog.Count && run.Deck.Count < count; i++)
            {
                if (catalog[i] == null) continue;
                run.Deck.Add(new CardInstance(catalog[i]));
            }

            // Variedad de costes/poder para que los estados se vean distintos.
            for (int i = 0; i < run.Deck.Count; i++)
            {
                if (i % 3 == 0 && run.Deck[i].CanUpgrade)
                    run.Deck[i].TryUpgradePower();
            }

            // Los perks demo viajan con su anfitrion y muestran compatibilidad.
            if (run.Deck.Count > 0)
                run.Deck[0].AttachedPerks.Add(PerkCatalog.Rally);
            if (run.Deck.Count > 1)
                run.Deck[1].AttachedPerks.Add(PerkCatalog.Apex);
            if (run.Deck.Count > 2)
                run.Deck[2].AttachedPerks.Add(PerkCatalog.Taunt);
            if (run.Deck.Count > 3)
                run.Deck[3].AttachedPerks.Add(PerkCatalog.Unstoppable);

            foreach (var card in run.Deck)
                run.DiscoverCard(card?.Definition?.Id);
        }
    }
}
