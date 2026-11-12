# Design

## Context

See `proposal.md` - Why. The current implementation has the Series 1 foundation:

- `ContentLoader` registers 59 draftable cards, 3 zones, 28 perks, 23 enemies, 12 events, 6 modifiers and 8 items.
- `EffectProcessor` resolves `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `GameStart`, `StartOfTurn` and `EndOfTurn`. `Ongoing` is derived from self modifiers, auras, zone additions, zone multipliers and a bounded repeat count.
- `CardActionService` is the single mutation authority for play, move, destroy, discard, draw, banish, add-to-hand, add-to-zone, copy, transform, merge, replace-zone, buff-top-card, draw-from-enemy-deck, shuffle, banish-lowest, add-energy and add-bonus-power.
- `BoardState` owns zones, hands, decks, piles, play log, energy ledgers, pending next-play entries, taunt reservations, visibility flags and the dirty version.
- `CardInstance` owns persistent power, run upgrades, combat cost deltas, reveal state, ability-disabled state, pending effects, attached perks and pile location.
- `GlobalStoreCatalog` partitions the 59-card catalog into 13 starters, 6 direct cards and 6 bundles (3x6, 2x7, 1x8 = 40). Bundle contents are positional and derived from the sorted pool.
- `AIStrategy` chooses affordable cards by a heuristic and zones by card count / player weakness. It does not know about card restrictions or location locks, so an illegal choice can stop its whole play phase.
- The current profile may predate the Series 1 starter change; `ProfileDto.Normalize` does not add missing starters.

The new content uses hooks that do not exist yet: hand-entry events, repeated On Reveal, printed play restrictions, printed movement permissions, a temporary movement window, flooded locations, play-only locks, a real cost cap, discard counters, a delayed inverse condition, side switching and zero-cost combat overrides.

## Goals / Non-Goals

**Goals:**

- Add the 33 requested cards and keep the existing 59 cards unchanged.
- Keep the existing Series 1 store bundles byte-for-byte stable while adding a separate Series 2 batch.
- Make the new rules derived and idempotent: no `CurrentPower +=` paths, no per-frame mutation.
- Keep every wave buildable and verifiable on its own.
- Keep the AI legal and non-stalling.
- Keep the UI legible at 720x1280 with the new indicators.

**Non-Goals:**

- Redesigning the 23 enemy decks or adding new enemies.
- Adding new locations beyond the existing three.
- Adding or rebalancing the 28 perks.
- A search-based AI or a full rules planner.
- Changing the existing 0-cost token convention.
- A general scripting language for card abilities.

## Decisions

### 1. Content batch and series model

The 8 Collection Level cards become starters; the 25 Series 2 cards become catalog/story content. The pool goes from 59 to 92 (21 + 46 + 25).

`CardDef` gains a `CardSeries` value (`CollectionLevel`, `Series1`, `Series2`) with `Series1` as the default so existing definitions do not change. The 13 current starters and the 8 new CL cards are `CollectionLevel`, the 46 existing Series 1 cards are `Series1`, and the 25 new cards are `Series2`. `CardDef.Clone` copies the value.

Why a series field instead of a static id list: the store partition, collection filters and future content grouping all need the same classification, and duplicating id arrays in multiple files is error-prone. The default keeps the existing content declarations unchanged.

Alternative considered: derive the batch from the `card_` prefix or from a hardcoded array in `GlobalStoreCatalog`. Rejected because it puts content classification in the store and cannot be reused by the collection.

### 2. Stable store partition per series

`GlobalStoreCatalog` stops treating the pool as one list. It builds two partitions:

- Legacy Series 1: the 46 `Series1` non-starters, sorted by id, with 6 direct cards and bundles `bundle_6_a`, `bundle_6_b`, `bundle_6_c`, `bundle_7_a`, `bundle_7_b`, `bundle_8`.
- Series 2: the 25 `Series2` cards, sorted by id, with 6 direct cards and bundles `bundle_s2_6_a`, `bundle_s2_6_b`, `bundle_s2_7_a`.

The legacy partition runs the same algorithm over the same 46 cards as today, so its direct and bundle membership is unchanged. The new batch uses a `bundle_s2_` prefix so it cannot collide with a legacy id. The flat `StoreCatalogData` still exposes 12 direct ids and 9 bundles; `BundleCount` becomes 9 and the rotation modulo follows.

Why split by series instead of expanding the global constants: the current bundle ids are persisted in `ProfileDto.OwnedBundleIds`. Repartitioning the 92 cards would change what `bundle_6_a` contains for a player who already bought it. The split preserves the purchase contract and keeps the new content independently testable.

Alternative considered: accept the repartition and migrate purchased bundles into explicit card grants. Rejected because it is a larger data migration with no gameplay benefit.

### 3. Additive profile migration

`StarterSet.CardIds` grows from 13 to 21 by appending the 8 CL cards. `ProfileDto.Normalize` (or the profile load path) ensures every current starter is in `OwnedCardIds`. The migration is additive: it adds missing starters, including the old Series 1 starters that a stale profile may lack (`elektra`, `carnage`, `deathlok`), and never removes a card.

Why in `Normalize`: it already runs on load and save, it is idempotent, and it is the established place for profile repairs. The Series 2 cards are not added; they remain store content.

### 4. Parameterized effect families

`BaseEffectFactory` grows by mechanic, not by card. The new branches reuse existing classes where possible and add small parameterized classes for the rest:

| Effect family | Cards | Parameters |
|---|---|---|
| `OnRevealBonusPerOtherCardHere` | Wolfsbane | amount |
| `OnRevealBuffContainer` | Nakia, Okoye | target (hand/deck), amount |
| `OnRevealAfflictHand` | Scorpion | amount |
| `OnRevealAddRandomCardToHand` | Agent 13 | none |
| `OnRevealAddToken` (extended) | White Tiger, Rhino | tokenId, target (`randomOtherLocation`, `here`) |
| `OnRevealBuffOngoing` | Spectrum | amount |
| `OnRevealDisableHighCostHand` | Leech | threshold, printed-only |
| `OnRevealPullTopDeck` | Jubilee | reveal |
| `OnRevealRepeatZone` | Odin | none |
| `OnRevealArmNotPlayedNextTurn` | Jessica Jones | amount, zone |
| `OnRevealSwitchSides` | Hobgoblin | none |
| `OnRevealFloodZone` | Storm | none |
| `OnRevealNextTurnCostTax` | Sandman | amount, cap |
| `OnRevealRaiseEnemyCost` | Iceman | amount, cap |
| `OnRevealDestroy` (extended) | Shang-Chi, Killmonger | target (`enemy10PlusHere`, `all1Cost`) |
| `OngoingCostAura` | Ka-Zar | cost, amount |
| `OngoingAdjacentZoneAddition` (extended Klaw) | Mister Fantastic | direction, amount |
| `OngoingConditionalPower` (extended) | Morbius, Warpath | condition (`discardCount`, `anyAllyZoneEmpty`), amount |
| `AfterMovedSelfBuff` | Vulture | amount |
| `AfterDestroyedReplaceToken` | Bucky Barnes | tokenId |
| `AfterDestroyedReturnZeroCost` | Sabretooth | none |
| `AfterDiscardedAddZeroCostCopies` | Swarm | copies |
| `AfterAddedToHandSelfBuff` | The Collector | amount |
| `EndOfTurnUnspentEnergy` | Sunspot | none |
| `PassivePlayRestriction` | Ebony Maw, The Infinaut | maxTurn, requiresNoPlayLastTurn |
| `PassiveMovePermission` | Nightcrawler, Vision | frequency |
| `PassivePlayLock` | Ebony Maw | none |

Why parameterized: 33 one-off classes would duplicate targeting, capacity and event logic. The existing `EffectDef.Parameters` map is the established extension point, and the unit tests can cover a family once with different parameters.

### 5. Added-to-hand event

`ActionEventKind` gains `AddedToHand` and `EffectTrigger` gains `AfterAddedToHand`. `EffectProcessor.TriggerFor` maps the event. `CardActionService.AddToHand` dispatches the event after a card actually enters the hand. `DrawFromEnemyDeck` dispatches both `Drawn` and `AddedToHand` because the card is drawn and enters the hand from the opponent's deck. Normal draws and manual board-to-hand returns do not dispatch it.

`The Collector` is an `AfterAddedToHand` effect that adds +1 persistent power when the event side matches its owner. `Swarm` copies, `Sabretooth` returns and `Apocalypse`'s replacement use `AddToHand` or an explicit `AddedToHand` event, so they count. The manual drag return stays excluded to avoid the play/return exploit.

Why a new event instead of reusing `AfterDrawn`: Quickdraw and other draw perks must not fire when Sentinel adds a card to hand. The two concepts are distinct.

### 6. Odin repeat pass

`EffectProcessor.ResolveEndOfTurn` gets a third phase between the normal reveal loop and the End of Turn effects:

```text
reveal cards played this turn
repeat OnReveal for Odin-like cards
resolve End of Turn effects
resolve pending bonuses
```

The repeat pass scans the board in a deterministic order. For each revealed, non-disabled Odin in play:

1. Skip if an active Cosmo is in its zone.
2. Snapshot the other allied cards there that have already revealed, are not ability-disabled and are not another repeat hook.
3. Apply each target's OnReveal effects once, with the target as the source.

Odin does not repeat himself, other Odin-like cards, enemy cards or cards that have not revealed. The repeat uses one application; the target's Encore perk still applied during its own normal reveal and does not multiply the Odin repeat. This keeps the cascade bounded and deterministic. `HasRevealed` remains set on the target.

Why a post-reveal pass instead of an immediate repeat: in the current sequential reveal order, an Odin played before another card would miss that card if it repeated immediately. The post-pass sees every card that revealed this turn, which matches "your other cards here" better. The trade-off is that an Odin destroyed during an earlier reveal will not repeat; that is recorded as a known edge.

### 7. Play restrictions and one legality query

A `PassivePlayRestriction` effect implements `IPlayRestriction`. `EffectProcessor.CanPlayCard(card, side, board)` evaluates it:

- Ebony Maw: `CurrentTurn > 3` is illegal.
- The Infinaut: an allied play on the previous turn makes it illegal.
- Leech's `PrintedAbilitiesDisabled` removes the restriction because `EffectiveEffectIds(includePrinted=false)` skips it.

Zone legality uses one query: capacity, `zone.CanPlay`, `!IsZonePlayLocked(zone)` and `!zone.IsFlooded(CurrentTurn)`. `CardActionService.PlayCard` uses the query for the actual play; `AIStrategy` uses the same query to filter cards and zones. This prevents the AI from choosing an illegal card or a locked/flooded zone and ending its play loop.

### 8. Play-only lock versus full lock

`EffectProcessor` distinguishes two markers:

- `PlayLockEffect` (Ebony Maw): blocks playing cards into the location, but allows movement and effect additions.
- `ZoneLockEffect` (Professor X): blocks play, effect additions, destroy, merge, banish and return to hand, while movement remains allowed.

`IsZonePlayLocked` checks either marker. `IsZoneRemovalLocked` checks only the full lock. `AddCardToZone` checks the full lock, not the play-only lock. This is a behavior split from the current `AddCardToZone`, which uses the play lock for everything.

Storm's flood is a zone state, not a card marker: `ZoneInstance.FloodedLastPlayableTurn`. `PlayCard` rejects a play when `CurrentTurn > lastPlayableTurn`; `AddCardToZone` and `MoveCard` do not. A replacement clears the flood unless the replacement is also flooded. Multiple Storms keep the later grace turn.

### 9. Movement permissions and Cloak

`CardInstance` gains `MoveUsedThisCombat`, `MoveUsedTurn` and `CombatCostOverride` (nullable). A `PassiveMovePermission` effect exposes `OncePerCombat` or `OncePerTurn`.

`CardActionService` gains one `CanMoveCard` query:

- A card played this turn can be moved freely, as today.
- A later-turn move is allowed by an unused printed permission.
- A later-turn move is allowed if the destination has an active Cloak window and the card has not used that window.
- Destination capacity and full-lock removal/addition rules apply.
- Effect-driven moves are not gated by a printed permission.

`BoardState` gains a list of `MoveWindow` entries (zone, turn). Cloak's reveal adds a window for the next turn. The window expires at turn end. `CombatScene` changes from "only current-turn board cards can be lifted" to "current-turn cards or cards with a permitted move"; the board-origin drop calls `MoveCardByPlayer` so the same query is enforced. Dropping an older card on the hand area still returns it to its slot. A failed move leaves the permission unused.

Why a board-level window instead of a temporary effect on every card: Cloak's ability applies to both sides and to cards that are already in play, so stamping an effect on every card would be invasive and hard to expire. A board window is one small state with one expiry.

### 10. Cost cap, overrides and Sandman

`CardInstance.EffectiveCost` becomes:

```text
if CombatCostOverride is set: return it
else return min(6, max(0, base + CostDelta + TemporaryCostDelta + TurnCostDelta))
```

`CombatCostOverride` is a nullable int and is cleared by `ResetForCombat`. Sabretooth sets it to 0 when it returns; Swarm copies set it to 0 at creation. This is more robust than a temporary delta because Sandman's next-turn tax cannot raise a zero-cost card.

Iceman adds +1 to `TemporaryCostDelta` on a random enemy hand card whose effective cost is below 6. The cap is applied by `EffectiveCost`.

Sandman records a next-turn tax on `BoardState` (`amount`, `turn`). `EffectProcessor.ResolveStartOfTurn` resets `TurnCostDelta` for every card and applies the tax to hand and deck cards, plus Hot Pursuit where applicable. `CardActionService.AddToHand`, `ReturnToHand` and the draw paths apply the current tax to cards that enter hand during the taxed turn. The tax expires at turn end and the effective cost is capped at 6.

Why apply the tax to hand and deck only: in-play cards do not pay costs, and applying the tax to them would incorrectly remove Ka-Zar's "effective cost 1" aura for the turn.

### 11. Discard counters and delayed inverse condition

`BoardState` gains `PlayerDiscardCount` and `AIDiscardCount`, reset in `ResetCombatFlags`. `CardActionService.DiscardCard` increments the side's counter after a successful discard, including discards that are later replaced. `OngoingConditionalPowerEffect` gains `discardCount` for Morbius (+2 per count).

`PendingEffectKind` gains `AllyNotPlayedInZoneNextTurn`. The existing pending resolver already uses `WasAllyPlayedInZone`; the inverse case resolves when the armed turn is the previous turn and no allied card was played in the watched zone. Jessica Jones uses this pending kind. The pending list already supports multiple entries, so Odin repeating Jessica arms two independent conditions.

### 12. Replacement and tokens

`token_winter_soldier` is 0/8 and `token_tiger` is 0/10, both non-draftable and ability-less. They follow the existing 0-cost token convention.

- Bucky Barnes: destroy replacement hook removes him from the zone and adds the Winter Soldier to the same side/location. The destroy event still fires for reactions. The token is marked revealed.
- Sabretooth: destroy replacement hook adds him to hand if there is room and sets `CombatCostOverride = 0`. If the hand is full, the replacement fails and the normal destroy proceeds.
- Swarm: `AfterDiscarded` creates up to two copies, sets `CombatCostOverride = 0`, and adds them to hand through the normal hand-cap path.
- Hobgoblin: a new `SwitchSides` action moves the instance between the zone's side lists at the same index. It checks the full lock and destination capacity, does not enter a pile, and does not dispatch a movement event. The scene's existing zone reconciliation removes the old view and creates the new side's view.

Why not implement Hobgoblin as a move: the card does not change location, and movement reactions (Kraven, Vulture, Multiple Man) should not treat a control change as a move. A separate action keeps the event semantics clean.

### 13. Jubilee reveal

Jubilee's pull is not a play. The card leaves the deck only if the location has room, becomes a board instance with `PlayedTurn = 0`, and is immediately revealed: `HasRevealed = true`, then its OnReveal effects resolve unless Cosmo or ability-disabled state suppresses them. It does not dispatch `AfterPlayed`. This matches the expectation that a pulled card can create another token or buff the board, while `The Infinaut` and other "played last turn" checks do not see it.

### 14. AI safety

`AIStrategy` receives the same card/zone legality queries as `CardActionService` (either through a small `Func` or the processor). It filters:

- cards whose printed play restriction fails;
- zones that are play-locked, flooded or otherwise closed to the card;
- taunt reservations that point at an illegal zone, which expire instead of blocking the turn.

The AI keeps its existing heuristic and fixed enemy decks. It does not move cards. A misplayed Killmonger or Hobgoblin is acceptable for this change; the requirement is legality and no stall, not optimal play.

### 15. Presentation

- `CardView` gains a small moveable indicator driven by the same `CanMoveCard`/window query. It does not cover the name, cost or power.
- `LocationView` gains a short status label (`INUNDADA`, `BLOQUEADA`) driven by the zone state. `UpdateViewLayout` refreshes it with the zone definition, and the detail panel uses the same value.
- The collection and store already paginate; 92 cards and 12 direct cards fit the existing paging. Comments and harness counts are updated.
- Card art remains procedural and lazy. 92 textures is a bounded increase; the cache is not pre-generated.

### 16. Verification

The repo harness grows with the content:

- Counts: 92 draftable, 21 starters, 12 direct, 9 bundles, 5x6 / 3x7 / 1x8, 59 bundle cards.
- Store: legacy bundle membership is identical to the 59-card catalog; Series 2 batch is 6 direct + 3 bundles; purchases are atomic.
- Profile: migration adds the 21 starters and does not remove owned cards.
- Per-wave card checks: one or more scenarios per card, with seeded random where the target is random.
- Cross-cutting checks: Odin repeat, Collector hand-entry, movement permissions, Cloak window, Storm/Ebony Maw locks, Infinaut restriction, cost cap, Sabretooth/Swarm zero cost, Bucky/Winter Soldier, Hobgoblin switch, Morbius discard count, Jessica pending, Jubilee reveal, Leech suppression, Killmonger/Shang-Chi destruction, White Tiger/Tiger, Agent 13 random.
- AI: no stall with Ebony Maw, Storm or The Infinaut; illegal cards and zones are skipped.
- A full build and a manual combat pass per wave.

The external `%TEMP%\opencode\eps-verify` harness is legacy and predates the current run flow; it is not the source of truth. The repository harness is extended instead.

## Risks / Trade-offs

- **33 cards plus engine changes is large** -> Deliver in six waves; each wave builds and has its own checks.
- **Store bundle stability** -> The per-series partition is mandatory; the legacy bundle test is a release gate. If the legacy membership changes, the change is not ready.
- **Odin timing** -> The post-reveal pass misses an Odin destroyed during the earlier reveal. Documented; the common case (Odin played last) is correct and deterministic.
- **Added-to-hand cascades** -> Only effect additions and opponent-deck steals dispatch the event; the action dispatcher's 256-event guard bounds any cascade.
- **Movement UI is the largest behavioral change** -> Use one `CanMoveCard` query for the scene, the action authority and the tests; a failed move must leave the permission unused.
- **Play-only vs full lock split** -> `AddCardToZone` changes from play-lock to full-lock. Add explicit Professor X and Ebony Maw tests so a regression is visible.
- **Sandman and cards added later** -> Apply the tax on hand entry during the taxed turn; if a path is missed, the card is not taxed. Add a test for a card drawn and a card added by effect during the taxed turn.
- **Ka-Zar and cost modifiers** -> Use effective cost; apply the turn tax only to hand/deck so in-play 1-cost cards keep the aura.
- **Killmonger and 0-cost tokens** -> The current Rocks, Squirrels and Clones cost 0, so Killmonger does not destroy them. This is a deliberate data convention, not a bug; changing token costs would alter Series 1.
- **Leech and perks** -> Leech disables printed text only; perks remain. This follows the existing Silence rule and is recorded in the specs.
- **AI misplays** -> The AI may play Killmonger into its own 1-cost cards or avoid Hobgoblin because of its negative base power. The change guarantees legality and no stall, not optimal play.
- **92 procedural textures** -> The lazy cache grows to at most 92 textures (about 9 MB of pixel data plus overhead). The lazy cache and existing disposal path keep this bounded.
- **Profile migration** -> Additive only; a code rollback leaves profiles with extra owned cards, which is harmless.

## Migration Plan

1. **Wave 0 - content and plumbing:** add `CardSeries`, register the 33 definitions and the two tokens, add the 8 CL starters to `StarterSet`, implement the profile migration, add the Series 2 store batch and update the harness counts. The game must build and all existing cards must still work.
2. **Wave 1 - simple effects:** the 20 cards in the first group, their effect definitions and factory branches. Update the harness per card.
3. **Wave 2 - destroy/discard/hand:** Bucky, Sabretooth, Swarm, The Collector, the `AddedToHand` event, discard counters and zero-cost overrides.
4. **Wave 3 - zones/restrictions:** Ebony Maw, The Infinaut, Sandman, Storm, the play-only lock, flood state, cost tax and the AI legality filters.
5. **Wave 4 - movement:** Nightcrawler, Vision, Cloak, Vulture's movement reaction and the drag/drop changes.
6. **Wave 5 - Odin and Hobgoblin:** the repeat pass and side switching.
7. **Wave 6 - integration:** presentation indicators, collection/store paging, full harness, AI pass and manual combat pass.

Rollback is a code revert. The profile migration is additive, and the store batch is additive; no legacy bundle ids are repurposed. No profile schema version change is required because the new fields are content-only or combat-only.

## Open Questions

None blocking. The remaining details are recorded as decisions: tokens are 0/8 and 0/10; enemy decks stay unchanged; Odin's repeat is a post-reveal pass that does not repeat repeat hooks; The Collector excludes manual returns; Morbius counts replaced discards; Leech disables printed text only; Storm and Ebony Maw use play-only restrictions; Killmonger uses effective cost 1.
