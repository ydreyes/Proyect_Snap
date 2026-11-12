# Proposal

## Why

The run now earns money, but there is no way to spend it. The shop/event nodes exist in the graph but open nothing, and a defeat still ends the run with no second chance. This change gives money its sink, adds the event cadence and introduces the first run-level modifiers and the extra-life item.

## What Changes

- **Add the in-game shop** at `Shop` nodes: 4 card offers, card prices by cost band (110/140/170), 1-3 upgrade offers by act, card removal for 55, and an extra-life item for 220.
- **Guarantee shop diversity**: at least one card not in the deck and, when possible, at least one second copy of a card already in the deck; the rest are random.
- **Add 12 events** at `Event` nodes. Every event can be declined; accepting may grant XP (0-30), money, a card, an upgrade, a perk or a run modifier.
- **Add 6 run modifiers** that change energy, starting hand, shop prices or combat rewards. Modifiers stack once each, persist in the run and are visible in the act transition and pause.
- **Add the extra-life item**: buy it in the shop, carry at most one charge, and consume it on a defeat to repeat the same enemy once.
- **Persist the shop and event state** in the run checkpoint after every purchase or resolution. The shop/event offer does not reroll on scene re-entry.
- **Apply modifiers in combat and rewards**: energy bonus, energy cap, starting hand, shop discount, money per win and XP per win.

## Capabilities

### New Capabilities

- `ingame-shop`: stock generation, prices, card purchase, upgrades, removal, extra life and purchase persistence.
- `run-events`: 12 declinable events, rewards, costs and event resolution persistence.
- `run-modifiers`: the 6 modifier types, stacking limits, visibility and application to combat and rewards.

### Modified Capabilities

- `run-economy`: money can be spent in the shop, event costs cannot make the balance negative, and purchases do not reduce the earned total.
- `roguelike-progression`: a defeat consumes an extra-life charge and repeats the same enemy once; a second defeat ends the run.
- `combat-system`: the turn energy pool and the opening hand include the active run modifiers.
- `content-data`: the content library includes the 12 events, the 6 modifiers and the shop price table.
- `ui-responsive`: the shop and event screens keep the portrait layout and the smallest supported viewport.

## Impact

- Run state: `RunState` gains `RunModifiers`, `ExtraLifeCharges`, `ShopState`, `EventState` and `SeenEventIds`.
- Shop and events: new `IngameShopSystem`, `RunEventSystem`, `RunModifierSystem` and their screens.
- Combat: `TurnManager`/`CardActionService` energy pool, `OpeningHandDealer`, `RunDirector` reward pipeline.
- Persistence: the run DTO includes the shop/event state, modifiers and extra-life charges.
- Verification: shop stock/purchase harness, event resolution harness, modifier application harness and a manual shop/event/extra-life pass.
