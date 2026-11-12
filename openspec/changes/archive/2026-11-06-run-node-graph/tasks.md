# Tasks

## 1. Run State and Director

- [x] 1.1 Add `ActIndex`, `CombatIndexInAct`, `TotalCombats`, `ShopEventChance`, `FinalActStep`, `PendingNode` and `PendingCombat` to `RunState`; replace `MaxCombats = 15` with 47 and reset every new field in `StartNewRun`. Verify with `dotnet build SnapClone.sln` and a harness asserting the initial graph position.
- [x] 1.2 Add the `RunNode` enum and the `PendingCombat` model with enemy identity, attempt, pre-combat snapshot and seed. Verify in the harness that a snapshot round-trips its fields and is cleared by the result application.
- [x] 1.3 Add `RunDirector` with `GetNextNode`, `OnCombatWon`, `OnCombatLost`, `RollShopEvent` and the act/final-act transitions; keep `ProgressionManager` as a thin facade. Verify in the harness that a normal win, a sub-boss win and a final-boss win return the expected node.

## 2. Acts and Roll

- [x] 2.1 Implement the 47-combat act table and the combat-type classification: 15th of acts 1-3 and sub-boss 4 are sub-bosses; the final boss is final. Verify in the harness with every combat index and with the final act steps.
- [x] 2.2 Implement the shop/event roll at 25% +15% with a 100% cap, reset on success and reset per act. Verify in the harness with a seeded random service: four failures force a node, a success resets to 25%, and each act starts at 25%.
- [x] 2.3 Implement the act transition after sub-bosses 1-3 and the final-act sequence (50/50, guaranteed shop, sub-boss 4, final boss). Verify in the harness that act 1 -> act 2 -> act 3 -> final produces the expected node order.

## 3. Node Scenes

- [x] 3.1 Build `EnemySelectScene` with two distinct temporary rival candidates, name, flavor phrase, difficulty and deck seed, and wire the selected candidate into `PendingCombat`. Verify by starting a run and reaching the first enemy selection.
- [x] 3.2 Build `SubBossIntroScene` for the fixed sub-boss of each act and `ActTransitionScene` with act, level, deck size and money. Verify by reaching combat 15 and confirming the transition opens before act 2.
- [x] 3.3 Register the three scenes in `Game1` and route every director node to its scene; make `CombatScene` ask the director after the result modal. Verify with a build and a manual act-boundary pass.

## 4. Presentation and Cleanup

- [x] 4.1 Update the combat top bar to show act and combat position plus level and money, with a final-act variant. Verify in combat at act 1, act 2 and the final act.
- [x] 4.2 Update `RunSummaryScene` to show the final act/combat totals and keep the existing economy fields. Verify by finishing a run on a win and a loss.
- [x] 4.3 Remove the old 15-combat position assumptions and the dead `ProgressionManager` branches. Verify with `dotnet build SnapClone.sln` and a grep for the old 15-combat constants.

## 5. Integration Verification

- [x] 5.1 Simulate all 47 combats through `RunDirector` with a scripted win path and verify the node order, act transitions, roll cadence and final summary.
- [x] 5.2 Simulate defeat and tie at a normal combat, a sub-boss and the final boss and verify the run ends without advancing the graph.
- [x] 5.3 Run the desktop build and a manual pass: act 1 sub-boss, act transition, one shop/event, final-act sequence and HUD at each stage.
