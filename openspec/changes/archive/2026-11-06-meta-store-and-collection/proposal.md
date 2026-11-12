# Proposal

## Why

The run ends with a summary but awards nothing persistent, and the 59-card catalog has no ownership model. The redesign needs a meta layer: global points, a store that turns points into permanent content, a collection that records what the player has seen and owns, and perks that behave as rules rather than flat power numbers.

## What Changes

- **Add global points**: a run awards `3 × normal combats + 40 × sub-bosses + 8 × level + 150 if the final boss is defeated`. The summary shows the breakdown and the award is idempotent through `profile.lastAwardedRunId`.
- **Add the global store**: extra life 90, perk 120, direct card 150 and six bundles at 450/650/850 for 6/7/8 cards. The item slot is reserved for the combat-items change at 60.
- **Partition the catalog**: 13 starter cards owned, 6 direct-sale cards, 40 cards across six bundles. No card belongs to two groups and every draftable card belongs to exactly one.
- **Add bundle rotation**: three bundles visible at a time from the six; the visible set advances after a purchase or a new run.
- **Add the collection**: 59-card grid with owned, discovered, in-store, hidden and in-run-deck states; filters by state, theme and cost; a perks tab.
- **Replace the 3 flat perks with 8 rule perks**: 4 initial and 4 store perks. Perks are attached to cards, stack up to 2 copies and are applied by the effect/power/energy hooks.
- **Make the level-up perk branch owned-only**: it offers only perks the profile owns.
- **Extend the run summary** with the points breakdown and the persistent award.

## Capabilities

### New Capabilities

- `meta-progression`: global points, the run award formula, idempotency and the points balance.
- `global-store`: store catalog, prices, bundles, rotation, catalog partition and purchase.
- `collection`: 59-card collection states, filters, detail and the perks tab.
- `perk-rules`: 8 rule perks, attachment, stacking and the effect/power/energy hooks.

### Modified Capabilities

- `roguelike-progression`: the run summary shows the points breakdown and awards the run once.
- `content-data`: the perk library becomes 8 rule perks; the store catalog and price table are content.
- `run-economy`: the level-up perk branch offers only owned perks.
- `effect-system`: perk `EffectIds` are dispatched through the effect processor.
- `combat-system`: perk rules are applied to power, energy and combat rewards.
- `ui-responsive`: the store, collection and expanded summary keep the portrait layout.

## Impact

- Meta: new `Profile`/`MetaProgression`, `GlobalStoreSystem`, `CollectionSystem`, `PerkRuleSystem`.
- Content: 8 `PerkDef` entries, 6 bundle definitions, the direct-card list and the catalog partition.
- Combat: `PowerResolver`, `CardActionService`, `RunDirector` hooks for the perk rules.
- Run: `RunState` level-up perk pool, `RunSummaryScene`, `MenuScene` entry points.
- Persistence: the profile fields defined by `run-save-and-pause` are now used for points, ownership, perks, bundles and `lastAwardedRunId`.
- Verification: points formula harness, idempotency harness, store purchase/bundle/partition harness, collection state harness, perk hook harness and a manual store/collection/summary pass.
