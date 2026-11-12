# Proposal

## Why

The run is still the temporary 15-combat line inherited from `run-state-v2`. It has no acts, no shop/event cadence and no final act, so the economy and the future enemy roster have nowhere to land. The node sequence is the structural change that turns the run into the designed 47-combat roguelike.

## What Changes

- **Replace the 15-combat loop with 4 acts. BREAKING** for the current `Fight Sequence` requirement and every caller that assumes `CurrentFightIndex` runs 0..14.
- Acts 1, 2 and 3 each contain 15 combats: 14 normal combats plus a fixed sub-boss at the 15th position.
- Add a **`RunDirector`** as the single authority for "what node comes next"; `ProgressionManager` stops being the sequence owner.
- Add **4 shop/event rolls per act**, after combats 3, 6, 9 and 12. The roll starts at 25%, adds 15% per failure, resets to 25% after a success, and resolves 50/50 between shop and event.
- Add **`ActTransition`** after sub-bosses 1, 2 and 3. It resets the shop/event chance and shows a short run snapshot.
- Add the **final act**: a 50/50 shop/event node, then a guaranteed shop, then the unique sub-boss 4, then the final boss. The final boss is combat 47.
- Add **`SubBossIntro`** and **`EnemySelect`** nodes. Until the enemy roster change lands, `EnemySelect` offers two temporary rival decks drawn from the card pool.
- Add the **pending-combat snapshot**: choosing an enemy records the full pre-combat state and attempt before the combat begins; the result is applied and the next node chosen before the result modal is dismissed.
- Update the combat HUD to show act, combat position, level and money instead of lives.

## Capabilities

### New Capabilities

- `run-node-graph`: act structure, node sequence, shop/event roll, act transition, final act, enemy-selection node and pending-combat snapshot.

### Modified Capabilities

- `roguelike-progression`: the fight sequence becomes 47 combats across 4 acts; run state tracks act, combat-in-act, shop/event chance, final-act step and pending node; run outcomes resolve on the final boss of the final act.
- `turn-flow-modal`: run progress display shows act and combat position plus level and money, with no lives.
- `ui-responsive`: new layouts for enemy selection, sub-boss intro and act transition.

## Impact

- Run state and routing: `SnapClone/Gameplay/Run/RunState.cs`, `ProgressionManager.cs`, new `RunDirector`, `RunRewardTable.cs`.
- Scenes: new `EnemySelectScene`, `SubBossIntroScene`, `ActTransitionScene`; `CombatScene`, `LevelUpScene`, `RunSummaryScene`, `MenuScene`.
- HUD: `CombatScene` top bar.
- Persistence is not included; the pending-combat snapshot is designed so the next change can serialize it.
- Verified with `dotnet build SnapClone.sln`, a director-level simulation of all 47 combats, and a manual pass through an act boundary, the final act and a shop/event roll.
