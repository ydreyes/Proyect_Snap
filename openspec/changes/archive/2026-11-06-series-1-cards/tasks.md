# Tasks

## 1. Content pool and card definitions

- [x] 1.1 Remove Spark, Raven, Champion, Behemoth, Juggernaut, Titan and Leviathan from `ContentLoader.LoadCards`; verify with a harness check that the pool has exactly 59 cards, no filler id, unique names/ids and costs covering 1-6.
- [x] 1.2 Register the 46 Series 1 cards from the proposal table with their exact costs and base powers; verify with a harness check that all 46 ids and stats match and that `GetDraftable()` returns 59.
- [x] 1.3 Write every Series 1 description in Spanish without accents, using `Al revelarse:`, `Continuo:`, `Al ser descartada:`, `Al ser destruida:`, `Cuando se mueve:`, `Despues de jugar una carta:` and `Al inicio del combate:` as appropriate; verify with a harness check for non-empty ASCII text and correct prefixes.
- [x] 1.4 Register an effect definition for every Series 1 card and make every `EffectIds` entry resolve; verify with `ContentLoader.ValidateCardEffects` and a harness check that no card references a missing or unimplemented effect.
- [x] 1.5 Update the content-data harness checks for the 59-card pool, the removed fillers, the 1-6 curve and the 13 starter cards that remain unchanged; verify the harness passes before any card behavior is added.

## 2. Effect infrastructure

- [x] 2.1 Add an `ICardEffectActions` port and a seeded random source to `EffectContext`, wire `CardActionService` and `RandomService` into `EffectProcessor`, and provide a fake port for tests; verify with a harness check that an effect can call a fake action and receive seeded random values.
- [x] 2.2 Add the parameterized effect families and `BaseEffectFactory` branches from `design.md` (`OnRevealDestroy`, `OnRevealDiscard`, `OnRevealBonus`, `OnRevealMove`, `NextPlay`, `OnRevealAddToken`, `OnRevealCopy`, `OnRevealMerge`, `OnRevealAfflict`, `OnRevealReplaceLocation`, deck/hand effects and Ongoing families); verify each family round-trips through an `EffectDef` with its parameters.
- [x] 2.3 Add effect-type validation so every registered card effect has a factory branch, while zone effects are validated separately; verify with a harness check that a deliberately missing card implementation fails before combat.

## 3. Continuous model and Ongoing cards

- [x] 3.1 Replace the self-only Ongoing query with the contributor pass: per-card bonuses, other-card auras, zone additions, zone multipliers and repetition, all derived and idempotent; verify with harness checks for repeated power calculation and no `CurrentPower` accumulation.
- [x] 3.2 Implement Captain America: other allied Ongoing cards in its location gain +2, two Captain Americas buff each other, and the aura moves with him; verify with a harness check for the aura, self-exclusion and movement.
- [x] 3.3 Implement Klaw: its owner's side of the location to the right gains +6 as a zone addition, not card power; verify with a harness check for the right-zone total and the rightmost-location no-op.
- [x] 3.4 Implement Onslaught: other non-Onslaught Ongoing sources at its location apply an additional time under the bounded `2^N` rule, capped at x4; verify with harness checks for Iron Man + Onslaught, Captain America + Onslaught and two Onslaughts.
- [x] 3.5 Implement Devil Dinosaur (+2 per hand card), Lizard (-4 if the opponent has 4 here), Namor (+5 if the only allied card here) and Strong Guy (+6 if hand has 1 or fewer); verify one harness check per condition, including updates when the board or hand changes.
- [x] 3.6 Implement Angela and Bishop with persistent after-play counters that use `PlayedThisTurn` so a replay in the same turn does not double-count; verify with harness checks for multiple plays, replay, movement and idempotent power.
- [x] 3.7 Implement Cosmo (suppress OnReveal for cards revealed after it in the location) and Enchantress (disable Ongoing cards in the location for the combat); verify with harness checks for reveal order, disabled contributions, disabled protection and clearing at combat start.
- [x] 3.8 Implement the three zone effects: +1 to all cards, -1 to all cards and +2 to the highest-Cost card(s); verify with harness checks for each effect, ties and board-change updates.

## 4. Destruction, discard and affliction

- [x] 4.1 Add pre-action destroy/discard replacement hooks and the zone removal constraint hook; verify with harness checks that a replaced action does not enter a pile, a protected action does not enter a pile and a prevented action emits no event.
- [x] 4.2 Implement Carnage (destroy other allied cards here, +2 per success), Deathlok (destroy other allied cards here) and Elektra (destroy a random enemy 1-Cost card here); verify with harness checks for counts, protected targets and no target.
- [x] 4.3 Implement Blade (discard rightmost), Lady Sif (discard highest Cost, random ties) and Sword Master (discard random odd Cost); verify with harness checks for selection, empty hand and replacement interactions.
- [x] 4.4 Implement Apocalypse (discard replacement returns to hand with +4) and Wolverine (discard/destroy replacement regenerates at +2 in a random location with room); verify with harness checks for no pile entry, hand return, random placement and no-room behavior.
- [x] 4.5 Implement Angel (fly from hand/deck to the destroyed card's location if room) and Nova (destroyed -> other allied cards +1); verify with harness checks for the replacement, the buff, no room and no reaction when the destroy is prevented.
- [x] 4.6 Implement Armor (cards here cannot be destroyed) and Spider-Woman (afflict enemy cards here with -1); verify with harness checks for protection, affliction following a moved card and later cards not being afflicted.

## 5. Movement

- [x] 5.1 Add `MoveCards` batch movement using one board snapshot, capacity checks and one `AfterMoved` event per successful move; verify with harness checks for simultaneous Heimdall moves and destination-full skips.
- [x] 5.2 Implement Doctor Strange (move tied highest-Power allied cards from other locations here) and Heimdall (move every other allied card one left); verify with harness checks for ties, capacity and leftmost cards.
- [x] 5.3 Implement Iron Fist as a per-side next-play pending move that survives turns and is consumed even when the destination is full; verify with harness checks for the next play, a turn boundary and a full destination.
- [x] 5.4 Implement Kraven (other card moves here -> +2) and Multiple Man (when this moves, add a copy at the old location if room); verify with harness checks for Kraven not triggering on himself, Multiple Man's copy and a full old location.

## 6. Hand, deck and draw

- [x] 6.1 Enforce the 7-card hand cap in draw, effect additions, copies and board-to-hand returns; verify with harness checks for a skipped draw, a fizzled Sentinel/copy, a rejected return and no partial instance.
- [x] 6.2 Shuffle the player combat deck before dealing, exclude Domino from the opening hand and force her draw on turn 2 when the hand has room; verify with harness checks for the turn-2 draw, no earlier draw and a full-hand skip.
- [x] 6.3 Implement America Chavez: buff the top card of the deck by +3 persistent; verify with a harness check that the buff follows the card when drawn and that an empty deck is a no-op.
- [x] 6.4 Implement Cable (draw the opponent's top card into hand if room) and Korg (shuffle a 0/1 Rock token into the opponent's deck); verify with harness checks for deck transfer, full hand, empty deck and Rock exclusion from draft/rewards.
- [x] 6.5 Implement Yondu: banish the lowest-Cost opponent deck card for the combat, random ties, without removing it from the run deck; verify with harness checks for the banished pile, the run-deck return and the empty-deck no-op.

## 7. Copy, token and merge

- [x] 7.1 Add `CardInstanceFactory.Copy`, `Transform` and `CreateToken` with independent instances, cloned definitions and the agreed copy rules (current persistent power, no copied perks); verify with harness checks for independent state and token non-draftability.
- [x] 7.2 Implement Forge as a next-play +2 persistent bonus that stacks and survives turns; verify with harness checks for one play, a turn boundary and two Forges.
- [x] 7.3 Implement Hulkbuster merge into a random other allied card in its location, host gains its power, Hulkbuster leaves the board without a pile or destroy event, and no target leaves Hulkbuster in play; verify with harness checks for each branch.
- [x] 7.4 Implement Mantis (copy a random opponent card played here this turn), Moon Girl (copy the hand until the hand cap) and White Queen (copy the highest-Cost opponent hand card, random ties); verify with harness checks for source selection, hand cap and independent copies.
- [x] 7.5 Implement Morph as a transform into a random opponent hand card, keeping its instance identity and reveal marker, with an empty-hand fallback; verify with harness checks for definition change, power/ability change and no re-reveal.
- [x] 7.6 Implement Mister Sinister (Sinister Clone with the same Power here) and Squirrel Girl (1-Power Squirrel in each other location); verify with harness checks for tokens, capacity skips and token identity.

## 8. Zones, visibility and locks

- [x] 8.1 Implement Scarlet Witch location replacement: choose a different location from the configured pool, keep cards and slots, mark the new location revealed, update the plaque/detail text and recalculate power; verify with harness checks for a different location, surviving cards and a no-op with one location.
- [x] 8.2 Implement Uatu as a per-side visibility flag set at combat start from deck/hand, without changing the real reveal timing; verify with harness checks for player visibility, rival hidden and unchanged reveal order.
- [x] 8.3 Implement Professor X as a play constraint and removal constraint that blocks play, destroy, merge, banish and return-to-hand while allowing movement; verify with harness checks for each blocked action, movement allowed and the lock following him.
- [x] 8.4 Refresh `LocationView` and `DetailPreviewView` from the current `ZoneDefinition` and the per-side visibility flag so replacement and Uatu update both views; verify with manual checks for the plaque and zone panel after replacement and with Uatu.

## 9. AI

- [x] 9.1 Add AI safety metadata or tags for mandatory self-discard/self-destroy cards and prefer a safer affordable card when one exists, while still playing the only affordable option; verify with harness checks for a safer choice, the only-option case and a seeded random target.
- [x] 9.2 Run every new action through the shared action service for the AI: destroy, discard, move, copy, merge, banish, token and draw; verify with harness checks for energy, hand cap, capacity, play log and event parity.

## 10. Presentation

- [x] 10.1 Verify a 7-card hand, token cards and merged hosts in the existing `CardView`/`LayoutManager` paths; adjust only if the name, cost or power is unreadable, and verify with a manual 7-card hand and token/merge screenshots.
- [x] 10.2 Verify Uatu and Scarlet Witch in the plaque and zone detail panel; verify the player sees the hidden/updated text, the rival view stays hidden and the totals remain correct.
- [x] 10.3 Verify that merged cards are absent from all three pile tabs and that a copied/transformed card shows its current definition in the detail panel; verify with manual checks and a pile-viewer pass.

## 11. Integration

- [x] 11.1 Run `dotnet build SnapClone.sln` with zero errors and no new warnings.
- [x] 11.2 Run the extended `%TEMP%\opencode\eps-verify` harness with zero failures, including the 59-card audit, per-wave card checks, hand cap, merge, Yondu, zones, Uatu, Scarlet Witch, Professor X, Onslaught and AI parity.
- [x] 11.3 Play one full combat per wave with a deck containing that wave's cards; verify the wave's cards resolve without an exception and the result screen totals match the plaques.
- [x] 11.4 Audit the final content: exactly 59 cards, 13 starter + 46 Series 1, no fillers, no 0-cost card, every description ASCII and every effect id implemented; verify with the harness and a `ContentLoader` registration listing.
- [x] 11.5 Re-run the manual regression from the archived `effect-action-core`: takeback/replay does not re-reveal, destroy does not refund energy, and the 7-card hand/return rules hold.
