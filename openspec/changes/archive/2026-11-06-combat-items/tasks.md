# Tasks

## 1. Item Content

- [x] 1.1 Add `ItemDef`, `ItemRegistry` and the 8 definitions with id, name, description, rarity, effect id and target rule. Verify the loader registers 8 items and every effect id resolves.
- [x] 1.2 Add the profile item counts to the mapper and verify a round-trip with a non-empty inventory.

## 2. Item System

- [x] 2.1 Implement `CombatItemSystem` with the play-phase guard, count check and legal-target validation. Verify every rejection leaves the board and count unchanged.
- [x] 2.2 Implement the 8 effects through `CardActionService`/`EffectProcessor`, including destroy protection, zone locks, hand capacity and board version refresh. Verify each effect in a focused board.
- [x] 2.3 Implement uniform random target selection with the seeded random service. Verify the same seed selects the same target and that an empty candidate set rejects.

## 3. Consumption and Persistence

- [x] 3.1 Decrement the profile count only after a successful effect and write the profile immediately. Verify a successful use persists across a restart.
- [x] 3.2 Restore the count when the profile write fails and show the error. Verify no count is lost on a failed write.
- [x] 3.3 Activate the global-store item entry at 60 points and grant one random item per purchase. Verify the entry is sold until restock.

## 4. Presentation

- [x] 4.1 Add the combat item bar with name, effect summary, count and disabled state. Verify at the smallest supported viewport and outside the play phase.
- [x] 4.2 Add same-frame feedback for energy, power, board changes and rejected uses. Verify each item type in combat.

## 5. Integration Verification

- [x] 5.1 Run the item catalog, target, consumption and action-authority harnesses together.
- [x] 5.2 Manual pass: buy an item, use it in combat, reject a use with no target, lose the profile write once and verify the count, then restart and confirm the inventory.
- [x] 5.3 Run the desktop and Android builds and regenerate the APK.
