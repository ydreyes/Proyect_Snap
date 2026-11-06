# Design

## Context

See `proposal.md` - Why. The current run implementation is small and centralized:

- `RunState` holds `CurrentFightIndex`, `LivesRemaining`, `Deck`, `PendingReward`, `AdvanceAfterReward`, `IsGameOver` and `IsVictory`. The deck is a `List<CardInstance>`.
- `ProgressionManager` maps the combat position to a `FightType` and returns scene names. Combats 1 and 3 of each cycle open the card reward, combat 2 opens the perk reward, combat 4 is normal and combat 5 is a boss.
- `CombatScene` shows the result modal, then calls `ProgressionManager.ResolveCombat` and changes scene. Each `OnEnter` rebuilds the board and deals a fresh combat from `RunState.Deck`.
- `RewardScene` generates offers through `RewardSystem`, mutates the deck and calls `CompleteReward`.
- `GameOverScene` only paints text and returns to the menu on click.
- `CardInstance` stores the definition and a `CurrentPower`; `PowerResolver` writes it as base power + perk bonus + persistent bonus + Ongoing. `CardActionService` charges `Definition.Cost`; `CardView`, the affordability highlight and `AIStrategy` read the same raw cost.
- Scenes and single-instance services are registered manually in `Game1`. There is no persistence: the run lives in memory only.

This change stays inside that structure. The node graph, save system and shops arrive in later changes, so the run state and the reward pipeline are built to be extended, not rewritten again.

## Goals / Non-Goals

**Goals:**

- Make `RunState` the single owner of experience, level, money, the level-up queue, the deck and the outcome.
- Give every combat a deterministic reward and a deterministic next node.
- Resolve level-ups one at a time from a queue, with offers that do not reroll on scene re-entry.
- Represent upgrades per card instance and make effective cost/power the only values used by combat, AI and UI.
- Replace the reward and game-over screens with a level-up node and a run summary.

**Non-Goals:**

- The 47-combat act structure, sub-boss roster, shop/event graph and act transitions.
- Saving or pausing the run; `RunState` is still in memory.
- Meta progression, global points, the global store, collection and card ownership.
- Combat items, extra-life items and the events that grant experience.
- Any new card, perk or enemy content.

## Decisions

### 1. `RunState` becomes the run aggregate

`RunState` keeps `CurrentFightIndex`, `Deck` and `MaxCombats = 15`, and gains:

| Field | Meaning |
|---|---|
| `Xp` | Experience accumulated inside the current level |
| `Level` | Current level, starts at 1 |
| `Money` | Current run balance |
| `XpEarned`, `MoneyEarned` | Totals for the summary |
| `CombatsWon` | Won combats, for the summary |
| `Outcome` | `Active`, `Won` or `Lost` |
| `PendingLevelUps` | Ordered list of unresolved level-ups |

It loses `LivesRemaining`, `MaxLives`, `LoseLife`, `PendingRewardKind`, `PendingReward` and `AdvanceAfterReward`. `StartNewRun` resets every field, including the deck, and `MinDeckSize = 6` joins `MaxDeckSize = 12`.

Why put the totals in the run instead of recomputing them in the summary: the later save change serializes this object, and a derived total would have to be reconstructed from a combat log the run does not keep. Storing the totals now avoids a second schema change.

Alternative considered: keep lives as a field set to 1. Rejected because every later rule ("extra life repeats the enemy") reads better against "no lives, one repeat token" than against a one-life counter that looks like the old system.

### 2. `ProgressionManager` stays the only run router, backed by a reward table

`ProgressionManager` keeps its role as the scene router, but its job changes:

- `GetCombatKind()` returns `Normal`, `SubBoss` or `FinalBoss`: positions 5 and 10 are sub-boss, position 15 is final boss, everything else is normal.
- `ResolveCombat(result)`:
  - Loss or tie: `Outcome = Lost`, return `Summary`.
  - Victory: apply the reward table, increment `CombatsWon`, queue level-ups, then:
    - final combat: `Outcome = Won`, return `Summary`;
    - otherwise advance `CurrentFightIndex` and return `LevelUp` if the queue is non-empty, else `Combat`.
- A static `RunRewardTable` holds the XP and money amounts per combat kind. Keeping it as data, not branches, is what the later act/event change reuses.

Why not introduce the `RunDirector` here: the node graph change owns the node sequence and the shop/event roll. Adding a director now would create two authorities for one decision, and the router would have to be deleted a change later. `ProgressionManager` is the thin seam the director replaces.

Alternative considered: keep rewards in `RewardSystem` and let it also grant XP. Rejected because reward generation and run progression would both mutate `RunState`, making the order of operations ambiguous.

### 3. Experience is applied by subtracting thresholds

`XpToNext(level) = 100 + 50 * (level - 1)`. Awarding XP loops:

```
Xp += reward
while Xp >= XpToNext(Level):
    Xp -= XpToNext(Level)
    Level += 1
    PendingLevelUps.Add(new LevelUpState(Level))
```

Subtracting instead of comparing against a cumulative threshold table keeps one source of truth and avoids a table that has to be regenerated for every level. The behavior is the same as a cumulative table: level N requires the sum of the first N-1 costs.

The curve is calibrated for the 47-combat run (about level 5 at the end of act 1, 7 at the end of act 2, 9 at the end of act 3 and 10 at the final boss). The temporary 15-combat sequence awards 1090 XP and therefore reaches about level 6. That is expected: this change does not tune pacing for a sequence it is about to replace.

### 4. A level-up is a queued node with pre-generated offers

`LevelUpState` stores the level reached, the branch chosen and the generated offers. The list is ordered; only the head is active. Offers are generated the first time the head is shown and are kept until it resolves, so leaving and re-entering the scene cannot reroll them.

- Card branch, deck below 12: 3 distinct draftable cards, respecting the 2-copy limit used by the draft.
- Card branch, deck at 12: the scene lists the deck cards with fewer than 3 upgrades; the player picks a card and then `-1 cost` or `+2 power`. `-1 cost` is disabled at effective cost 0.
- Perk branch: up to 3 registered perks with fewer than 2 copies across the deck; the player picks a perk and a target card.

The scene resolves the head, pops it, and returns to `LevelUp` while the queue is non-empty, otherwise to `Combat`. This is deliberately the same shape as the later `pendingLevelUps` checkpoint field, so the save change serializes the list without redesigning it.

Alternative considered: open a modal inside `CombatScene` for each level. Rejected because a 12-card deck needs a full-screen picker, and because a separate scene already fits the existing `SceneManager`.

### 5. Upgrades live on the card instance

`CardInstance` gains `CostDelta`, `PowerDelta`, `UpgradeCount` and `TemporaryCostDelta`, plus two computed values:

```
EffectiveCost  = max(0, Definition.Cost + CostDelta + TemporaryCostDelta)
EffectivePower = Definition.BasePower + PowerDelta + PerkPowerBonus + BonusPower
```

`ResetForCombat` clears `TemporaryCostDelta`, `BonusPower`, reveal and disabled flags, and keeps the permanent deltas and perks. Upgrading one instance never touches the shared `CardDef`, so two copies of the same card can have different costs and powers.

Why not mutate `CardDef`: definitions are shared by the registry, the AI deck and every copy. Mutating one would leak the upgrade into the opponent, every future instance and every new run.

Alternative considered: store a single `UpgradeLevel` and derive both values from a fixed table. Rejected because the player chooses `-1 cost` or `+2 power`, so the two deltas are independent and the count alone cannot reconstruct them.

### 6. `PowerResolver` starts from `EffectivePower`

`CalculateCardPower` currently computes `BasePower + perk bonus + BonusPower + Ongoing`. It becomes `EffectivePower + Ongoing`, and `WritePower` refreshes `PerkPowerBonus` before calculating, exactly as it does today. `EffectivePower` must not add the perk bonus a second time.

Deck-side screens (level-up, summary) have no combat `WritePower`, so `LevelUpSystem` refreshes `PerkPowerBonus` when a perk is attached and the summary refreshes it for the whole deck before drawing. This keeps the computed property meaningful outside combat without giving `CardInstance` a registry dependency.

### 7. Effective cost is the only cost used at runtime

`CardActionService` charges `card.EffectiveCost`; `TurnEnergyLedger` already stores the charged amount, so refunds and re-charges need no change. The affordability highlight, `AIStrategy`, `CardView.DrawInHand` and any cost selector use `EffectiveCost` too.

The cost-based card effects are part of the same contract: Elektra's 1-cost target, Lady Sif and Sword Master's highest-cost discard, Yondu's lowest-cost banish and Doctor Strange's highest-power selection must read effective values. Otherwise an upgraded card would be selected by a cost it no longer has.

Board cards keep hiding the cost; only hand, deck and summary show the upgraded number.

### 8. Replace the reward and game-over scenes with `LevelUp` and `Summary`

`Game1` registers `LevelUp` and `Summary` and stops registering `Reward` and `GameOver`. `RewardScene` and `RewardSystem` are replaced by `LevelUpScene` and `LevelUpSystem`; `GameOverScene` is replaced by `RunSummaryScene`.

- `LevelUpScene` is a two-step screen: branch buttons first, then the offer grid, then an optional target-card picker for perks and cost upgrades.
- `RunSummaryScene` shows outcome, level, XP earned, money earned, money unspent, combats won and the final deck with effective cost/power. It offers `Menu principal` and `Re-run`. `Re-run` calls `StartNewRun` and goes to `Draft`.
- The combat HUD replaces `VIDAS n` with level and money.

Why remove instead of keeping hidden: a registered but unreachable reward scene is dead code that will be mistaken for the shop when the shop change lands. Deleting it makes the next change start from an honest scene list.

### 9. Nothing is persisted in this change

`RunState` stays in memory. The save change owns the JSON format, atomic write, `.bak` and schema version, and it will serialize `PendingLevelUps` as designed above. Adding a stopgap serializer now would create a format that the save change has to version immediately.

## Risks / Trade-offs

- **[Only about level 6 is reached in the temporary 15-combat loop]** → Treat the curve as fixed for the 47-combat run; do not retune XP per combat here. The node graph change is the pacing change.
- **[`PerkPowerBonus` can be stale outside combat]** → Refresh it when a perk is attached and when the summary opens; keep `WritePower` as the combat refresh point.
- **[Upgrades change the targets of existing cost-based cards]** → Include Elektra, Lady Sif, Sword Master, Yondu and Doctor Strange in the manual pass with upgraded cards.
- **[Removing lives breaks HUD and copy that still reference them]** → The build catches the field removals; the manual pass checks the combat HUD and both end-of-run paths.
- **[Deleting `RewardScene` loses the only card-picker UI]** → `LevelUpScene` reuses the same card-grid and detail-preview components before the old scene is removed.
- **[No save means a run dies with the process]** → Intentional and isolated to this change; the save change follows once the state and graph are stable.

## Migration Plan

1. Change `RunState` and add the reward/level-up services; keep the old reward scenes compiling only long enough to verify the state.
2. Move `CardInstance`, `PowerResolver`, `CardActionService`, AI, effects and `CardView` to effective values.
3. Rewire `ProgressionManager` and `CombatScene` to the new pipeline.
4. Register `LevelUp` and `Summary`, remove `Reward` and `GameOver`, update the HUD.
5. Run `dotnet build SnapClone.sln` and the manual matrix: win a normal combat, win a sub-boss, win the final boss, lose, tie, cross two thresholds in one combat, level up at 11 and 12 cards, upgrade at the 3-upgrade cap and at cost 0, and remove cards down to 6.

Rollback is a revert: the change adds no persisted data and no external dependency, so there is nothing to migrate back.
