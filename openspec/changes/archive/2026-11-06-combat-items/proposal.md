# Proposal

## Why

The global store reserves an item slot but there are no items to sell or use. Combat has no free-action layer, and the player has no way to spend a profile resource during a fight. This change adds the eight combat items, their profile inventory and their in-combat use.

## What Changes

- **Add 8 combat item definitions**: energy surge, power boost, enemy destroy, draw two, enemy move, zone affliction, hand copy and zone heal.
- **Add the profile inventory**: item counts are stored in `profile.json`, persist across runs and are written immediately when an item is used.
- **Add combat item use**: a free action during the player's play phase. It consumes no energy and does not end the turn, and it is not a card play.
- **Use random legal targets**: when an item has several legal targets, it picks one at random. When it has no legal target, the use is rejected and the item is not consumed.
- **Resolve through the existing board/action authority**: destroy, move, draw, copy, power and affliction use the same action rules, protections and capacity checks as cards.
- **Add the item bar** to the combat UI with counts and a use control.
- **Activate the global-store item entry at 60 points**; it was reserved by the meta-store change.

## Capabilities

### New Capabilities

- `combat-items`: the 8 item definitions, the profile inventory, acquisition, free-action use, random targeting, consumption and immediate persistence.

### Modified Capabilities

- `combat-system`: item use is a free player action during the play phase and resolves through the action authority.
- `content-data`: the content library includes the 8 item definitions.
- `ui-responsive`: the combat item bar keeps the portrait layout and remains usable at the smallest viewport.

## Impact

- Data: `ItemDef`, `ItemRegistry`, `ContentLoader.LoadItems`.
- Combat: `CombatItemSystem`, `CombatScene`, `CardActionService`, `EffectProcessor`.
- Profile: item counts and the immediate write on use.
- Store: the item entry at 60 becomes purchasable.
- Verification: item catalog harness, target/consumption harness, action-authority harness and a manual combat pass with each item.
