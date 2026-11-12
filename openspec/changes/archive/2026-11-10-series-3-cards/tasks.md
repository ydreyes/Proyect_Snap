# Tasks

## 1. Foundation and data model

- [X] 1.1 Add `CardSeries.Series3`, copy it in `CardDef.Clone`, and add a `RegisterSeries3` helper; verify the solution builds and the harness still loads 92 draftables with every existing card unchanged.
- [X] 1.2 Register the new tokens (`token_widows_bite`, `token_broodling`, `token_doombot`, `token_drone`, `token_illusion`, `token_demon`, `token_mjolnir`, `token_snowguard_hawk`, `token_snowguard_bear`, `token_void`) and reuse `card_sentinel` for Master Mold; verify none enter the draft and each has the stats in the content-data spec.
- [X] 1.3 Add the `ExpectedDraftable` scaffolding for the ten waves (92, 112, 132, 146, 166, 181, 193, 199, 208, 217) and wire it through `ContentLoader.ValidateCatalog`, `GlobalStoreCatalog` and the harness; verify the build is green at 92.
- [X] 1.4 Add `CostResolver` with the documented precedence order and expose it through `CardActionService` and `EffectProcessor`; verify unit checks for base, upgrade, temporary, dynamic, aura, tax, absolute set, cap, floor and Mobius cases.
- [X] 1.5 Add `BasePowerOverride` and the set/reset/double helpers on `CardInstance`; verify a set clears persistent bonuses, a reset returns to the original base, and a double is idempotent across repeated power recalculations.
- [X] 1.6 Add `DestroyedCards` to `BoardState` and record every successful destroy before the pile move; verify the history survives a resurrection and resets at the start of a new combat.
- [X] 1.7 Add `ActionEventKind.Activated`, `EffectTrigger.Activate` handling and `EffectTrigger.EndOfGame`; verify the processor maps the events and a fixture effect fires for each.
- [X] 1.8 Extend `ICardEffectActions` with draw, resurrect, copy-to-other-zones, zone swap, enemy-hand pull/discard, enemy-hand add, Max Energy, next-turn energy and created marker; verify each action rejects invalid capacity and preserves board consistency.
- [X] 1.9 Add the zone-aware `CanPlayCard` overload, the side-level play-count query and the zone/global suppression and cap state; verify the player UI, the AI and the action authority return the same legality result.
- [X] 1.10 Add the per-side Max Energy delta and next-turn energy bonus to `BoardState`; verify Electro, Havok and Psylocke use them and that both reset on a new combat.
- [X] 1.11 Fix `ResolveRepeatOnReveal` so it repeats only cards revealed on the current turn, or snapshots the targets at the repeater's reveal; verify a multi-turn Odin does not repeat earlier turns and Wong does not double-fire.
- [X] 1.12 Add the Series 3 store scaffold while keeping the legacy and Series 2 partitions active; verify the existing direct ids and bundle membership are unchanged.
- [X] 1.13 Add harness helpers for a Series 3 fixture (board, hand, deck, effects, actions) and a per-card stats/description check; verify the helpers work on a representative card before Wave 1.

## 2. Wave 1 - simple local effects (20 cards)

- [X] 2.1 Register Drax, Green Goblin, Debrii, Doctor Doom, Rescue, Rockslide, Silver Surfer, The Hood, Thor, Wasp, Wong, Yellowjacket, Orka, Luke Cage, Omega Red, Red Skull, Typhoid Mary, Mojo, Cerebro and Patriot with their exact stats and Spanish ASCII descriptions; assign draft themes; verify the pool is 112 and each card's effect ids resolve.
- [X] 2.2 Implement the auras and conditions for Drax, Orka, Luke Cage, Omega Red, Red Skull, Typhoid Mary, Mojo, Cerebro, Patriot and Wong; verify each with a harness scenario, including max-power group, full-side count, only-card and no-ability conditions.
- [X] 2.3 Implement the token and utility effects for Debrii, Doctor Doom, The Hood, Thor, Wasp, Yellowjacket and Green Goblin; verify Rock, DoomBot, Demon and Mjolnir creation, the DoomBot per-zone limit and the switch-sides behavior.
- [X] 2.4 Implement the remaining Wave 1 effects for Rescue, Rockslide, Silver Surfer and Wong's interaction with the repeat pass; verify next-turn bonus, two Rocks, other 3-cost buff and the single-repeat rule.
- [X] 2.5 Add the Wave 1 harness checks and update the catalog count to 112; verify every card has a scenario and the full harness passes.

## 3. Wave 2 - local effects, movement and hand/deck (20 cards)

- [X] 3.1 Register Ronan, Dazzler, Polaris, Stegron, Magneto, Kingpin, Dagger, Human Torch, Spider-Man 2099, Lady Deathstrike, Venom, M.O.D.O.K., Selene, Hazmat, Gambit, Falcon, Ghost, Psylocke, Kitty Pryde and Aero with their exact stats and descriptions; verify the pool is 132.
- [X] 3.2 Implement the movement effects for Polaris, Stegron, Magneto, Aero and Ghost-Spider's Wave 3 sibling; verify enemy-cost filters, right-move, last-enemy-played and destination capacity.
- [X] 3.3 Implement the move reactions for Kingpin, Dagger, Human Torch and Spider-Man 2099; verify affliction, per-enemy-card power, doubling and once-per-combat movement.
- [X] 3.4 Implement the destroy, discard and affliction effects for Lady Deathstrike, Venom, M.O.D.O.K., Selene, Hazmat, Gambit and Falcon; verify thresholds, hand discard, mass affliction and return-by-cost.
- [X] 3.5 Implement the hand and energy effects for Psylocke and Kitty Pryde; verify next-turn energy, start-of-turn return, AddedToHand and the persistent +1.
- [X] 3.6 Implement the auras for Ronan and Dazzler; verify enemy-hand scaling and full-location scaling.
- [X] 3.7 Add the Wave 2 harness checks and update the catalog count to 132; verify every card has a scenario and the full harness passes.

## 4. Wave 3 - tokens, copy and deck effects (14 cards)

- [X] 4.1 Register Agent Coulson, Attuma, Brood, Colleen Wing, Destroyer, Ghost-Spider, Maria Hill, Mirage, Nick Fury, Sauron, Shanna, Ultron, Viper and Zero with their exact stats and descriptions; verify the pool is 146.
- [X] 4.2 Implement Agent Coulson, Maria Hill, Nick Fury, Shanna, Ultron and Brood; verify random cost buckets, per-zone tokens, Broodling power copy and hand capacity.
- [X] 4.3 Implement Attuma, Destroyer and Viper; verify end-of-turn own-card destruction, other-zone destruction and lowest-power side switch.
- [X] 4.4 Implement Colleen Wing, Ghost-Spider, Mirage and Zero; verify least-cost discard, last-played move, least-cost copy with +2 and next-play silence.
- [X] 4.5 Implement Sauron; verify only Ongoing cards in the deck lose their printed abilities and hand cards are untouched.
- [X] 4.6 Add the Wave 3 harness checks and update the catalog count to 146; verify every card has a scenario and the full harness passes.

## 5. Wave 4 - cost, power and destroyed cards (20 cards)

- [X] 5.1 Register Death, Knull, She-Hulk, Stature, Miles Morales, Sera, Quinjet, Zabu, Mobius M. Mobius, Wave, Baron Mordo, Beast, Bast, Valkyrie, Shadow King, Black Panther, Shuri, Taskmaster, Deadpool and Mister Negative with their exact stats and descriptions; verify the pool is 166.
- [X] 5.2 Implement the cost cards Death, She-Hulk, Stature, Miles Morales, Sera, Quinjet, Zabu, Mobius M. Mobius, Wave, Baron Mordo and Beast; verify every precedence case, the minimum-1 floors and the Mobius both-side rule.
- [X] 5.3 Implement the power cards Bast, Valkyrie, Shadow King, Black Panther, Shuri and Taskmaster; verify hand set, zone set, reset with an active aura, double once, next-play double and last-card power copy.
- [X] 5.4 Implement Deadpool and Mister Negative; verify destroy-return doubling and deck cost/power swap without mutating shared definitions.
- [X] 5.5 Integrate the cost resolver into the AI, the combat scene, the card views and the collection detail; verify the displayed cost equals the charged cost in all Series 3 cost cases.
- [X] 5.6 Add the Wave 4 harness checks and update the catalog count to 166; verify Death, Knull, Mobius, Wave, Baron Mordo, Bast, Valkyrie, Shadow King, Black Panther, Shuri and Mister Negative each have a scenario.

## 6. Wave 5 - draw, resurrection, enemy hand and Max Energy (15 cards)

- [X] 6.1 Register Adam Warlock, Crystal, Maximus, Jane Foster, Ghost Rider, Hela, Doctor Octopus, Black Bolt, Moon Knight, Silver Samurai, Master Mold, Electro, Havok, Black Cat and Helicarrier with their exact stats and descriptions; verify the pool is 181.
- [X] 6.2 Implement the draw cards Adam Warlock, Crystal, Maximus and Jane Foster; verify hand capacity, the Widow's Bite draw lock, both-side draws and the 0-cost filter.
- [X] 6.3 Implement Ghost Rider and Hela; verify discard-pile selection, random locations, distinct costs and no OnReveal on resurrection.
- [X] 6.4 Implement Doctor Octopus, Black Bolt, Moon Knight, Silver Samurai and Master Mold; verify enemy-hand pull, least/even/lowest discard, two Sentinels and capacity behavior.
- [X] 6.5 Implement Electro and Havok; verify the Max Energy delta, the one-card-per-turn restriction, the end-of-turn -1 and the energy pool floor.
- [X] 6.6 Implement Black Cat and Helicarrier; verify end-of-turn self-discard from hand, random hand fill and the discard trigger.
- [X] 6.7 Add the Wave 5 harness checks and update the catalog count to 181; verify every card has a scenario and the full harness passes.

## 7. Wave 6 - copy-to-zone, swap, movement and reactions (12 cards)

- [X] 7.1 Register Arnim Zola, Leader, Nimrod, Quake, Echo, Negasonic Teenage Warhead, Silk, Titania, Hercules, Nebula, Juggernaut and Spider-Man with their exact stats and descriptions; verify the pool is 193.
- [X] 7.2 Implement Arnim Zola and Nimrod; verify destroy-and-copy, other-zone capacity, revealed copies and no OnReveal.
- [X] 7.3 Implement Leader; verify highest-power enemy cards played this turn are copied to the owner's side and the copies do not trigger OnReveal.
- [X] 7.4 Implement Quake and the zone-swap action; verify definitions, cards, flood state and play constraints move together and the indices stay stable.
- [X] 7.5 Implement Echo, Negasonic, Silk and Titania; verify enemy-Ongoing silence, once-per-game destruction, self-play exclusion and side switching.
- [X] 7.6 Implement Hercules, Nebula, Juggernaut and Spider-Man; verify the once-per-card-per-turn re-move, turn-5 information, enemy-played-this-turn move and pull-to-another-zone.
- [X] 7.7 Add the Wave 6 harness checks and update the catalog count to 193; verify every card has a scenario and the full harness passes.

## 8. Wave 7 - restrictions, zones and Limbo (6 cards)

- [X] 8.1 Register Crossbones, Giganto, Sentry, Goose, Jean Grey and Magik with their exact stats and descriptions; verify the pool is 199.
- [X] 8.2 Implement Crossbones, Giganto and Sentry; verify winning-only, left-only, right-rejection and Void creation on the right.
- [X] 8.3 Implement Goose and Jean Grey; verify the cost-4 restriction for both sides, the first-card rule, and the full/illegal-zone fallback.
- [X] 8.4 Add the `zone_limbo` definition, Magik's replacement, the turn-5 cutoff, the exclusion from Scarlet Witch's pool and the dynamic seven-turn match; verify the turn display and end-of-turn guards.
- [X] 8.5 Add the Wave 7 harness checks and update the catalog count to 199; verify every card has a scenario, the AI does not stall and Limbo produces exactly seven turns.

## 9. Wave 8 - Activate and copied text (9 cards)

- [X] 9.1 Register Absorbing Man, Mystique, Rogue, Super-Skrull, Black Widow, Hellcow, Lockjaw, Mysterio and Spider-Ham with their exact stats and descriptions; verify the pool is 208.
- [X] 9.2 Implement the Activate action, the once-per-combat record and the five cards Black Widow, Hellcow, Lockjaw, Mysterio and Spider-Ham; verify each activation, the second-activation rejection and the return-to-hand persistence.
- [X] 9.3 Implement Absorbing Man, Mystique, Rogue and Taskmaster's copied power; verify last-played OnReveal, last-played Ongoing, enemy-Ongoing steal and the missing-source no-op.
- [X] 9.4 Implement Super-Skrull with the one-level printed-Ongoing copy and the recursion guard; verify two Super-Skrulls do not grow and the copied effects use the owner's side.
- [X] 9.5 Add the `ACTIVAR` control to the detail panel and the copied-text presentation; verify the control is hidden when illegal and remains reachable at 720x1280.
- [X] 9.6 Add the Wave 8 harness checks and update the catalog count to 208; verify every card has a scenario and the full harness passes.

## 10. Wave 9 - end of game, delayed reveal and information (9 cards)

- [X] 10.1 Register Captain Marvel, Dracula, M'Baku, Martyr, Invisible Woman, Snowguard, Agatha Harkness, Daredevil and Howard the Duck with their exact stats and descriptions; verify the pool is 217.
- [X] 10.2 Add the EndOfGame phase and implement Captain Marvel, Dracula, M'Baku and Martyr; verify the resolution order, one resolution per card, capacity checks and victory after the phase.
- [X] 10.3 Implement Invisible Woman's delayed reveal and the end-of-game reveal pass; verify no end-of-turn reveal, face-down rendering, the game-end reveal and the leave-play fallback.
- [X] 10.4 Implement Snowguard with Hawk and Bear; verify the zone-effect suppression, the expiry, the single zone-effect repeat and the game-end resumption.
- [X] 10.5 Implement Agatha's game-start draw and even-turn auto-play with the existing AI legality query; verify turns 2, 4 and 6 auto-play, odd turns remain manual and no legal play ends the turn cleanly.
- [X] 10.6 Implement Daredevil and Howard the Duck; verify the turn-5 information display and the top-deck tap without changing the board.
- [X] 10.7 Add the Wave 9 harness checks and update the catalog count to 217; verify every card has a scenario and the full harness passes.

## 11. Integration and verification

- [X] 11.1 Update the collection and global store counts, comments and paging for 217 cards, 18 direct cards and 26 bundles; verify every entry is reachable at 720x1280.
- [X] 11.2 Add the end-of-game banner, the dynamic `TURNO n/7` display, the delayed-reveal face-down state and the Limbo/Hawk/Goose/Jean Grey zone labels; verify they do not overlap names, effects, power totals or controls.
- [X] 11.3 Extend the harness to the final counts (217 draftable, 125 Series 3, 21 starters, 18 direct, 26 bundles, 178 bundle cards) and keep the per-wave card checks; verify the full harness passes.
- [X] 11.4 Run `dotnet build SnapClone.sln` and the repository harness; verify a full combat with a deck containing one card per wave, including Activate, end-of-game, delayed reveal, copied text, Death's cost, Limbo turn 7 and Agatha's even-turn control.
- [X] 11.5 Run a manual combat pass at 720x1280 with Goose, Jean Grey, Invisible Woman, Snowguard, Quake and Mysterio; verify the controls and status labels remain legible and reachable.
- [X] 11.6 Update the affected documentation (`docs/ui-visual-design.md` and any card or store counts) and verify the documented counts match the harness.
- [X] 11.7 Run `openspec validate "series-3-cards" --strict` and the repository harness one final time; verify both are green before archive.
- [X] 11.8 Increase the shared typography (`UiTheme` role sizes and `FontService.LegacyBaseSize`), rebuild the solution/harness, and regenerate the signed Android APK; verify the build is green and the APK file is produced.
