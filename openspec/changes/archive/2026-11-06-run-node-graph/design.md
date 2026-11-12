# Design

## Context

See `proposal.md` - Why.

The run is still the temporary sequence from `run-state-v2`:

- `RunState` holds `CurrentFightIndex` (0..14), XP, level, money, deck and the level-up queue. `MaxCombats` is 15.
- `ProgressionManager` classifies the current combat by position and returns `LevelUp`, `Combat` or `Summary`.
- `CombatScene` shows the result modal, then calls `ResolveCombat` and changes scene. Every `OnEnter` rebuilds the board and deals from `RunState.Deck`.
- `MenuScene` starts a new run through `DraftScene`. `LevelUpScene` and `RunSummaryScene` already exist.
- There is no enemy definition, enemy selection, act transition or shop/event node.

This design replaces the sequence authority while keeping the already-stable run economy, cards and scenes.

## Goals / Non-Goals

**Goals:**

- Make the 47-combat act structure and node order explicit and testable in one place.
- Make the shop/event roll deterministic given the run state and a seed.
- Record a pre-combat snapshot so a combat result cannot be applied twice.
- Keep every node reachable without persistence; persistence is the next change.

**Non-Goals:**

- Saving or pausing the run.
- The 23-enemy roster, enemy discovery and real enemy decks.
- Shop stock, purchases, events, modifiers and extra lives.
- Meta points, collection and the global store.
- Combat items.

## Decisions

### 1. `RunState` grows a graph position, not a second run model

Add:

| Field | Meaning |
|---|---|
| `ActIndex` | 1, 2, 3 or 4 for the final act |
| `CombatIndexInAct` | 0..15 for acts 1-3; unused in the final act |
| `TotalCombats` | 0..47, for display and the summary |
| `ShopEventChance` | 25..100, reset per act |
| `FinalActStep` | 0..3: roll, guaranteed shop, sub-boss 4, final boss |
| `PendingNode` | The node the director will open next |
| `PendingCombat` | Snapshot of the combat being played |

`CurrentFightIndex` and `MaxCombats` are replaced by `TotalCombats` and `RunState.MaxCombats = 47`. The old property can remain as a computed alias during the change, then be removed.

The run keeps the existing XP, level, money, deck and level-up queue. The act fields are additive, so the level-up and summary scenes need only display changes.

### 2. `RunDirector` is the only sequence authority

`RunDirector` owns:

- `GetNextNode()` - reads `PendingNode` and opens the right scene.
- `OnCombatWon()` - applies rewards, queues level-ups, then decides between level-up, shop/event roll, act transition, enemy selection or summary.
- `OnCombatLost()` - ends the run.
- `RollShopEvent()` - advances the chance, chooses shop or event, updates the node.
- `OnActCompleted()` - resets the chance and moves to the next act or the final act.
- `OnEnemySelected(enemyId)` - creates the `PendingCombat` snapshot.

`ProgressionManager` is reduced to a compatibility facade that delegates to the director, or is deleted. Scenes never compute the next node themselves; `CombatScene` asks the director after the result modal.

Why not keep the position-based branches in `ProgressionManager`: every new node would add another special case to the same class, and the save change would have to serialize the implicit position. A director with an explicit `PendingNode` is the seam the save and pause changes need.

### 3. Act structure

| Act | Combats | Special |
|---|---:|---|
| 1 | 15 | sub-boss at 15 |
| 2 | 15 | sub-boss at 30 |
| 3 | 15 | sub-boss at 45 |
| Final | 2 | sub-boss 4 and final boss |
| **Total** | **47** | |

Acts 1-3 use the same 15-combat shape. The sub-boss is fixed and has no enemy selection. The final act uses a separate step counter, not `CombatIndexInAct`.

### 4. Roll math

```
chance = 25
on roll:
    if chance >= 100 or random(100) < chance:
        node = shop or event (50/50)
        chance = 25
    else:
        chance = min(100, chance + 15)
```

The roll occurs after the 3rd, 6th, 9th and 12th won combat of the act, and never after the sub-boss. The chance is stored in `RunState`, so a save can resume the exact cadence.

Why a stored chance instead of a fixed schedule: the design wants a probabilistic cadence with a pity floor. A schedule would remove the roll; a pure random roll would have no floor. Storing the chance is the minimal state that preserves both.

### 5. Node pipeline

```
combat won
  -> rewards + XP
  -> level-ups (queue)
  -> if sub-boss: ActTransition
  -> else if combat position in {3,6,9,12}: roll -> Shop/Event
  -> else: EnemySelect
```

The level-up queue is drained before the roll, so a card added there can appear in that same shop. The roll is applied before the next enemy selection, so a shop/event is never skipped.

### 6. Final act

```
sub-boss 3
  -> ActTransition
  -> FinalActStep 0: 50/50 shop/event
  -> FinalActStep 1: guaranteed shop
  -> FinalActStep 2: SubBossIntro (sub-boss 4)
  -> FinalActStep 3: SubBossIntro (final boss)
  -> Summary
```

The guaranteed shop is the second node, matching the economy decision: the player can always spend before the last two bosses. There is no enemy selection in the final act.

### 7. Temporary enemy selection

Until `enemy-roster` lands, `EnemySelect` builds two distinct rival candidates from the card pool. Each candidate stores a display name, a flavor phrase, a difficulty and a deck seed; the combat uses that seed to build the rival deck. This keeps the node real and gives the next change a stable contract to replace.

The candidate selection is an addition to the run state only as a `PendingCombat` snapshot. It does not add ownership, discovery or persistence.

### 8. Pending combat snapshot

`PendingCombat` contains the enemy identity, the attempt number, the pre-combat run state and the combat seed. It is created when the player confirms an enemy or a sub-boss and cleared after the result is applied.

The snapshot is the only place a combat result can be applied from. If the scene is re-entered, it must not re-apply wins or rewards. The save change will serialize this object and use the attempt number to reseed a resumed combat.

### 9. Scenes and HUD

- New `EnemySelectScene`, `SubBossIntroScene` and `ActTransitionScene`.
- `Game1` registers them.
- `CombatScene` top bar shows `ACTO n · COMBATE x/15` for acts 1-3 and `ACTO FINAL · PASO x` for the final act, plus level and money.
- `LevelUpScene` and `RunSummaryScene` keep their layouts; the summary adds the act/combat totals.

## Risks / Trade-offs

- **[47 combats is much longer than 15]** → The act structure is the intended pacing; keep reward values fixed and measure a full run before tuning.
- **[The shop/event roll can produce a dry act]** → The +15% pity floor guarantees a node by the fourth roll at the latest.
- **[Temporary enemy selection is throwaway UI]** → Keep the candidate contract narrow (name, phrase, difficulty, deck seed) so `enemy-roster` replaces the source, not the scene.
- **[Pending combat could be applied twice]** → Make the result application idempotent and clear the snapshot before opening the next node.
- **[No persistence yet]** → A process restart still loses the run; the snapshot is designed for the next change.

## Migration Plan

1. Add the graph fields to `RunState` and the `PendingCombat` model.
2. Add `RunDirector` and move `ProgressionManager` behind it.
3. Implement the roll, act transition and final-act step.
4. Add the three node scenes and register them.
5. Update the HUD and the summary totals.
6. Verify with a 47-combat simulation and a manual pass through an act boundary and the final act.

Rollback is a revert; no persisted data or external dependency is added.
