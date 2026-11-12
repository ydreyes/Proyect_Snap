# Design

## Context

See `proposal.md` - Why.

The combat authority already exposes every action items need:

- `CardActionService` has play, move, destroy, discard, draw, copy, token, merge and power paths.
- `EffectProcessor` dispatches card and zone effects.
- `BoardState` tracks version, energy, piles and protections.
- `CombatScene` owns the play phase and the board views.
- The profile reserves item counts and the global store reserves the item entry at 60.

There is no item definition, inventory use or item bar.

## Goals / Non-Goals

**Goals:**

- Make items data-driven and reusable through the existing action authority.
- Keep item use free, immediate and persistent.
- Make random targeting deterministic and testable.
- Never consume an item on a rejected use or a failed profile write.

**Non-Goals:**

- Item crafting, upgrades or item-specific rarity effects.
- Items that start a new combat or change the run graph.
- Enemy item use.
- A general-purpose item scripting language.

## Decisions

### 1. `ItemDef` is content, `CombatItemSystem` is behavior

```
ItemDef
  Id
  Name
  Description
  Rarity (Common, Rare)
  EffectId
  TargetRule (None, AllyCard, EnemyCard, AllyZone, EnemyZone, HandCard, Draw)
```

`ItemRegistry` loads the 8 definitions. `CombatItemSystem` validates the use, picks the target, calls the existing action or effect and consumes the profile count.

### 2. Item catalog

| Item | Rarity | Effect | Target |
|---|---|---|---|
| `item_energy_surge` | Common | +2 energy this turn | None |
| `item_power_boost` | Common | +2 power | Random allied card in play |
| `item_destroy_enemy` | Rare | Destroy | Random unprotected enemy card |
| `item_draw_two` | Common | Draw up to 2 | Deck/hand capacity |
| `item_move_enemy` | Rare | Move to another zone | Random enemy card and destination |
| `item_afflict_zone` | Rare | -2 power to all enemies | Random enemy zone with cards |
| `item_copy_hand` | Rare | Copy a card | Random card in hand |
| `item_heal_zone` | Rare | +3 power to allies | Random allied zone with cards |

The exact names are content; the effect ids are stable.

### 3. Free action

`CombatItemSystem.TryUse(itemId)` is only accepted when:

- the combat is in the player's play phase;
- the item count is above 0;
- the effect has a legal target.

It does not call `PlayCard`, does not charge energy, does not advance the turn and does not add a `PlayRecord`. It calls the specific action path (`DestroyCard`, `MoveCard`, `DrawCard`, `CreateCopy`, power adjustment) and then refreshes powers.

Why not implement items as zero-cost cards: cards have zones, hand slots, reveal markers and play events. Items are profile objects with no board identity, so treating them as cards would create false triggers and dead slots.

### 4. Random targeting and legality

The target rule determines the candidate set:

- `AllyCard`: player cards in play.
- `EnemyCard`: AI cards in play, respecting `BoardState.CanDestroy` for destroy.
- `AllyZone`/`EnemyZone`: zones with at least one card of that side.
- `HandCard`: player hand cards.
- `Draw`: deck and hand capacity.

The system picks uniformly with `RandomService`. If the candidate set is empty, it returns a rejection and does not consume.

### 5. Consumption and persistence

The use flow is:

```
validate -> resolve effect -> decrement profile count -> write profile
```

If the profile write fails, the count is restored and the use is reported as failed. The board effect cannot be undone, so the item is not consumed but the effect remains; this is the same trade-off as a failed checkpoint and is surfaced to the player. The alternative, writing before the effect, would consume the item without applying it. The design chooses "effect applied, item not lost, error shown" as the least frustrating.

The profile write happens immediately, even during combat, because the item belongs to the meta layer and is not part of the run checkpoint.

### 6. Acquisition

The global store's reserved item entry becomes active at 60 points. Buying it adds one random item to the profile and marks the store entry sold until the next restock. The item is not tied to a run.

### 7. UI

`CombatScene` adds an item bar in the controls area. Each entry shows the item name, effect summary and count. The bar is hidden outside the play phase or when the inventory is empty. A rejected use shows a short message.

## Risks / Trade-offs

- **[Items can trivialize a combat]** → Rarity and store price control supply; effects use the existing caps and protections.
- **[A failed profile write after applying the effect]** → Keep the item and show the error; do not retry the effect automatically.
- **[Random targeting can feel unfair]** → The item description states "aleatorio" and the legal set excludes invalid targets.
- **[Item power can drift from card rules]** → Every destructive or movement item goes through the action authority.

## Migration Plan

1. Add `ItemDef`, `ItemRegistry` and the 8 definitions.
2. Add `CombatItemSystem` with target selection and the action calls.
3. Add the profile count read/decrement and the immediate write.
4. Activate the store item entry.
5. Add the item bar.
6. Verify with the item catalog, target/consumption and action-authority harnesses and a manual pass with each item.

Rollback is a revert; items are additive and the store entry returns to reserved.
