# Design

## Context

See `proposal.md` - Why.

The economy and graph are already in place:

- `RunState` has XP, level, money, deck and level-up queue; money has no sink.
- `RunDirector`/`ProgressionManager` routes `Shop` and `Event` nodes, but both are placeholders.
- Energy is `min(turn, 6)` in `TurnManager` and `CardActionService`; the opening hand is dealt by `OpeningHandDealer`.
- `run-save-and-pause` already defines a run checkpoint and a pending-combat snapshot.
- There is no item system yet; combat items arrive in the next change.

This design adds the shop, events, modifiers and the first run-level extra life without adding combat items.

## Goals / Non-Goals

**Goals:**

- Give money a meaningful sink inside the run.
- Make the shop stock bounded, varied and persistent.
- Make every event declinable and every reward deterministic.
- Apply modifiers at the few points where combat and economy already converge.

**Non-Goals:**

- Combat items and their inventory.
- The global store, meta points, ownership and bundles.
- New cards, perks or enemies.
- A general-purpose event scripting language.

## Decisions

### 1. Run state additions

```
RunModifiers        set of modifier ids (max 1 each)
ExtraLifeCharges   0 or 1
ShopState           stock, sold flags, upgrade offers, extra-life sold flag
EventState          event id, resolved flag
SeenEventIds        event ids already shown in this run
```

`StartNewRun` clears all of them. The checkpoint DTO writes and reads them.

### 2. Shop stock generation

The shop uses the draftable catalog, not the owned collection: the in-game shop sells cards into the current deck, while ownership belongs to the global store in the next change.

Algorithm:

1. Pick a card not in the deck, if one exists. This is the "new" offer.
2. If a deck card has fewer than 2 copies and was not already chosen, pick it. This is the "second copy" offer.
3. Fill the remaining slots from the catalog, excluding duplicates already in the stock.
4. Price by base cost: 1-2 = 110, 3-4 = 140, 5-6 = 170.
5. Generate once and store in `ShopState`.

If the deck has no room or no new card exists, the guarantee degrades gracefully to the available candidates. The spec's "when possible" covers the 12-card and all-cards-owned edge cases.

### 3. Purchases

- Buying a card deducts money and adds an instance. At 12 cards, the scene asks for a deck card to remove first; the net size stays 12. At 6 cards, removal is disabled.
- Buying an upgrade costs 70 and asks for a deck card with fewer than 3 upgrades, then the upgrade type.
- Removal costs 55 and refuses when the deck has 6 cards.
- Extra life costs 220, sets `ExtraLifeCharges = 1`, and is unavailable when already held.
- Every successful purchase writes the checkpoint immediately. `MoneyEarned` is not reduced by spending.
- `shop_discount` applies to all prices with `floor(price * 0.8)`.

### 4. Extra life in the outcome pipeline

`RunDirector.OnCombatLost()` checks `ExtraLifeCharges`. If one is held, it consumes the charge, writes the checkpoint and repeats the same `PendingCombat` with a new seed. Otherwise it sets `Outcome = Lost` and opens the summary.

The repeat is the same enemy and attempt; only the seed changes. This is the same mechanism the save change uses for an interrupted combat, which keeps a single "repeat the encounter" path.

### 5. Event definitions

`RunEventDef` contains:

```
Id, Title, Text
Accept: EffectKind + amount + optional card/perk/modifier selector
CostMoney
```

The 12 definitions live in content data. The event engine resolves the accept effect through the existing run services:

- XP: `RunState.AddExperience`.
- Money: `RunState.AddMoney` or a guarded spend.
- Card: build an instance and apply the deck-cap rule.
- Upgrade: apply to a random eligible deck card (or ask the player if the deck has several).
- Perk: attach one from the perk pool with the 2-copy limit.
- Modifier: `RunModifierSystem.Add`.

Events are chosen from `SeenEventIds`; after all 12 are seen, repeats are allowed. The event is stored in `EventState` so a re-entry shows the same event.

Decline is always available and returns to the director without changing the run.

### 6. Modifiers

The 6 modifiers are applied at these points:

| Id | Point |
|---|---|
| `energy_bonus` | `TurnManager`/`CardActionService` pool |
| `energy_cap` | the same pool cap |
| `starting_hand` | `OpeningHandDealer` |
| `shop_discount` | shop price calculation |
| `money_bonus` | `RunDirector` after a win |
| `xp_bonus` | `RunDirector` after a win |

Each modifier can be held once; a second grant is refused and the source does not consume its cost. The list is rendered in the act transition and the pause overlay.

Why not use the existing `PerkDef` for modifiers: perks are attached to individual cards and are part of a run deck; run modifiers are global and change combat or economy rules. Keeping them separate avoids overloading the perk model.

### 7. Persistence and re-entry

The shop and event states are stored in the run checkpoint. The stock is generated once on node entry and never rerolled. A purchase or resolution writes before the next node. The pause/save changes already provide the writer; this change only adds fields to the DTO.

### 8. Presentation

- `ShopScene`: 4 card offers with price and sold state; upgrade/removal/extra-life/leave controls.
- `EventScene`: title, narrative, accept with cost/reward, decline.
- Modifier list in the act transition and pause overlay.
- The deck grid for cap/removal reuses the level-up card layout with effective cost and power.

## Risks / Trade-offs

- **[Shop stock can be useless at 12 cards]** → Keep the removal path and the upgrade offers; the shop is never a dead end.
- **[Event rewards can accelerate the run too much]** → Cap XP at 30, use the existing deck/upgrade caps and make every event declinable.
- **[Modifiers can silently change combat]** → Display them at the act transition and pause, and apply them only at the documented points.
- **[Extra life can create an infinite loop if the defeat repeats]** → It is consumed before the repeat and holds at most one charge.
- **[Event text and effects can drift]** → Keep the 12 definitions data-driven and validated at load.

## Migration Plan

1. Add the run fields and the modifier system.
2. Add the 12 event definitions and the shop stock generator.
3. Add `ShopScene` and `EventScene` and route the graph nodes.
4. Wire extra life into the outcome pipeline.
5. Apply the 6 modifiers in combat and rewards.
6. Verify with shop/event/modifier harnesses and a manual pass through a shop, an event and an extra-life defeat.

Rollback is a revert; the new fields are additive and the checkpoint DTO can ignore them if the change is removed.
