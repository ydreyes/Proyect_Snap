# Tasks

## 1. Points and Profile

- [x] 1.1 Add `RunId` and the normal/sub-boss win counters to the run state; compute the points formula and the breakdown in `MetaProgressionSystem`. Verify in a harness with a victory, a defeat, a level 5 run and a final-boss run.
- [x] 1.2 Implement the idempotent award with `profile.LastAwardedRunId`, a retry path and a failed-write state. Verify in the harness that two summary entries award once and that a failed write can be retried.
- [x] 1.3 Add the points balance to the main menu and the summary breakdown. Verify the breakdown sums to the total at the smallest supported viewport.

## 2. Ownership and Catalog Partition

- [x] 2.1 Add the profile owned sets, perk copies, item counts, bundle rotation and `LastAwardedRunId` to the profile mapper. Verify a round-trip with a populated profile.
- [x] 2.2 Implement the catalog partition at content load: 13 starter owned, 6 direct cards, 40 bundle cards, no overlap and no missing draftable card. Verify with a harness that fails on a duplicate or an unassigned card.
- [x] 2.3 Implement the 6 bundle definitions (3×6, 2×7, 1×8) and the direct-card list. Verify every bundle card resolves and every bundle totals its declared size.

## 3. Global Store

- [x] 3.1 Implement the store catalog and prices (item 60 reserved, extra life 90, perk 120, direct card 150, bundles 450/650/850) with purchase validation. Verify the unaffordable, owned and sold states.
- [x] 3.2 Implement the three-visible-bundle rotation and the replacement after purchase. Verify that a bought bundle is replaced and never returns.
- [x] 3.3 Apply purchases: extra life to the next run, perk to owned perks, direct card and bundle cards to owned. Verify each effect in the harness and that the profile write is atomic.

## 4. Collection

- [x] 4.1 Build the 59-card grid with the owned, discovered, in-store, hidden and in-run-deck states and the four-column layout. Verify each state with a seeded profile.
- [x] 4.2 Add the state, theme and cost filters and the card detail with the obtain path. Verify the combined filters and that hidden cards hide their identity.
- [x] 4.3 Add the perks tab with the 8 perks, rule text and owned copies. Verify an unowned store perk shows its obtain path.

## 5. Perk Rules

- [x] 5.1 Replace the 3 flat perks with the 8 rule definitions and the effect ids; validate every id at content load. Verify the loader registers 8 perks and no flat modifiers.
- [x] 5.2 Implement the rule dispatcher and the power hooks (Vanguardia, Aura, Francotirador, Letal, Guardian, Imparable), the Agil cost hook and the Codicioso economy hook. Verify each rule with a focused board and the two-copy stacking.
- [x] 5.3 Make the level-up perk branch offer owned perks only. Verify with a profile that owns the 4 initial perks and one store perk.

## 6. Integration Verification

- [x] 6.1 Run the points, partition, store, collection and perk harnesses together.
- [x] 6.2 Manual pass: finish a run, collect points, buy a bundle, a direct card, a perk and an extra life, then open the collection and the summary.
- [x] 6.3 Run the desktop and Android builds and regenerate the APK.
