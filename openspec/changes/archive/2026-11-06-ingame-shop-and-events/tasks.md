# Tasks

## 1. Run State and Modifiers

- [x] 1.1 Add `RunModifiers`, `ExtraLifeCharges`, `ShopState`, `EventState` and `SeenEventIds` to `RunState` and reset them in `StartNewRun`. Verify with a harness that a new run starts empty.
- [x] 1.2 Add `RunModifierSystem` with the 6 modifier ids, max-one stacking, display names and application helpers. Verify in the harness that a duplicate is refused and that the six helpers return the expected values.
- [x] 1.3 Add the modifier fields to the run checkpoint DTO and verify a run with modifiers round-trips.

## 2. Shop

- [x] 2.1 Implement `IngameShopSystem` stock generation with 4 distinct cards, the new-card guarantee, the second-copy guarantee and the price table. Verify in the harness across a fresh deck, an 11-card deck and a 12-card deck.
- [x] 2.2 Implement card purchase, the 12-card replacement flow, upgrade offers by act, removal at 55 and the 6-card floor. Verify each purchase updates money, deck and checkpoint exactly once.
- [x] 2.3 Implement the extra-life purchase at 220 with a max of one charge. Verify the option is unavailable after purchase and that the charge appears in the state.

## 3. Events

- [x] 3.1 Add the 12 `RunEventDef` entries with title, text, cost and effect. Verify the content loader registers all 12 and every accept effect is one of the supported kinds.
- [x] 3.2 Implement event selection without repeats until all 12 are seen, the decline path and the accept resolution for XP, money, card, upgrade, perk and modifier. Verify each reward type in the harness and that declining changes nothing.
- [x] 3.3 Persist `EventState` and `SeenEventIds` and verify that re-entering an event shows the same event and that resolving it writes the checkpoint.

## 4. Combat and Outcome Integration

- [x] 4.1 Apply `energy_bonus` and `energy_cap` to the turn pool and `starting_hand` to the opening hand; keep the AI on the same values. Verify with a combat at turns 1, 6 and 7.
- [x] 4.2 Apply `shop_discount` to shop prices, `money_bonus` and `xp_bonus` to a won combat. Verify each in the harness.
- [x] 4.3 Consume an extra-life charge on defeat, repeat the same enemy with a new seed and end the run only when no charge remains. Verify the repeat, the charge consumption and the second defeat.

## 5. Presentation and Verification

- [x] 5.1 Build `ShopScene` and `EventScene` with the portrait layout, the deck grid for cap/removal and the modifier list. Verify at the smallest supported viewport.
- [x] 5.2 Manual pass: buy a card, an upgrade, a removal and an extra life; decline and accept events; trigger an extra-life repeat.
- [x] 5.3 Run the shop/event/modifier harnesses and the desktop and Android builds, and regenerate the APK.
