# Design

## Context

See `proposal.md` - Why and the card inventory.

The code already has the `effect-action-core` foundation:

- `CardActionService` owns `PlayCard`, `MoveCard`, `DestroyCard`, `DiscardCard`, `DrawCard`, `BanishCard`, `AddToHand` and `AddTokenToZone`.
- `EffectProcessor` dispatches `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn` and `GameStart`, with a deterministic reactor scan and a 256-event loop guard.
- `CardInstance` has `BonusPower`, `HasRevealed`, `AbilitiesDisabled` and a multi-entry `PendingEffects` list.
- `BoardState` has `PlayLog`, `PlayedThisTurn`, per-side `TurnEnergyLedger` and `DestroyProtections`.
- `ZoneInstance` has `IZonePlayConstraint`; `CardDef.IsToken` and `CardRegistry.GetDraftable()` separate tokens from draftable cards.
- `PowerResolver` still computes card power from the card's own Ongoing effects plus zone multipliers; it cannot express auras to other cards, additive cross-zone bonuses or Onslaught repetition.
- The content is 20 cards; the three zone effects are registered but unimplemented; the seven fillers include the only 0-cost card.

The card set introduces the first real users of every hook the core left open. The design therefore extends the core in focused layers, then adds card content in waves.

## Goals / Non-Goals

**Goals:**

- Register exactly 59 cards: the 13 starter cards plus 46 Series 1 cards; remove the seven fillers.
- Implement each Series 1 card with its printed cost, base power and behavior, using the existing action/event core.
- Add the minimum cross-cutting capabilities the cards need: continuous auras and repetition, destruction/discard replacements, movement batches and reactions, hand/deck manipulation, copy/transform/merge, zone effects, location replacement, visibility and locks.
- Keep every wave playable and testable on its own.
- Keep the power calculation derived and idempotent; no `CurrentPower +=` paths.

**Non-Goals:**

- New scenes, animation systems or a visual event log.
- A general-purpose scripting language for card effects.
- New run progression, reward cadence or draft rules beyond the 59-card catalog.
- More than the three existing locations as the replacement pool; Scarlet Witch uses the existing three and their newly implemented effects.
- A sophisticated AI planner; the AI gets a safety heuristic, not a search.

## Decisions

### 1. Content: 59 cards, no fillers, data-driven descriptions

`ContentLoader.LoadCards` registers the 13 starter cards and the 46 Series 1 cards. The seven fillers are removed. The pool is exactly 59; the curve is 1-6 because the Series 1 list has no 0-cost card.

Descriptions stay in Spanish without accents. The prefixes are:

| Trigger | Prefix |
|---|---|
| On Reveal | `Al revelarse:` |
| Ongoing | `Continuo:` |
| When discarded | `Al ser descartada:` |
| When destroyed | `Al ser destruida:` |
| When moved | `Cuando se mueve:` |
| After you play | `Despues de jugar una carta:` |
| Game Start | `Al inicio del combate:` |

Descriptions are kept as short as the ability allows. The detail panel wraps; the existing harness's "fits in two lines" check is updated to a realistic line budget rather than forcing every Series 1 text under the old 89-character limit.

Why remove the fillers instead of keeping them as padding: the user's rule is explicit and the 59-card set already covers costs 1-6 with 59 bodies. Keeping the fillers would contradict the requested card list.

### 2. Parameterized effect families, not 46 one-off classes

`BaseEffectFactory` already maps `EffectDef.EffectType` to a small class. For this set, the factory grows by *mechanic*, not by card. The same class handles several cards through `EffectDef.Parameters`:

| Effect family | Cards | Parameters |
|---|---|---|
| `OnRevealDestroy` | Carnage, Deathlok, Elektra | target, scale-per-destroyed |
| `OnRevealDiscard` | Blade, Lady Sif, Sword Master | selection |
| `OnRevealBonus` | Medusa, Star-Lord, Groot, Rocket, Hawkeye | condition, amount |
| `OnRevealMove` | Doctor Strange, Heimdall | selection, direction |
| `NextPlay` | Forge, Iron Fist | bonus, move direction |
| `OnRevealAddToken` | Mister Sinister, Squirrel Girl | tokenId, zones, power |
| `OnRevealCopy` | Mantis, Moon Girl, White Queen, Morph | source, destination |
| `OnRevealMerge` | Hulkbuster | target selection |
| `OnRevealAfflict` | Spider-Woman | target, amount |
| `OnRevealReplaceLocation` | Scarlet Witch | none |
| `DeckTopBonus` | America Chavez | amount |
| `DrawFromEnemyDeck` | Cable | none |
| `ShuffleToken` | Korg | tokenId |
| `BanishLowestCost` | Yondu | scope |
| `OngoingAura` | Captain America | target, amount |
| `OngoingZoneAddition` | Klaw | direction, amount |
| `OngoingRepeat` | Onslaught | target, cap |
| `OngoingConditionalPower` | Devil Dinosaur, Lizard, Namor, Strong Guy | condition, amount |
| `AfterPlayedCounter` | Angela, Bishop | scope, amount |
| `WhenDestroyed` / `WhenDiscarded` | Nova, Apocalypse, Wolverine | action |
| `WhenMoved` | Kraven, Multiple Man | action |
| `GameStartVisibility` | Uatu | side |
| `SuppressOnReveal` | Cosmo | none |
| `RemoveAbilities` | Enchantress | target |
| `OnRevealZoneLock` | Professor X | none (Ongoing query) |

Why parameterized: the archived starter-card change established one small class per effect type. Repeating that for 46 cards produces 46 near-duplicates and a factory switch that is hard to audit. A parameterized family keeps the factory near 25 branches and puts card differences in content data.

Alternative considered: one class per card. Rejected for duplication and because several cards are literally the same effect with a different amount or target.

### 3. `EffectContext` gets an action port and a random source

Effects that destroy, discard, move, copy, merge, draw, shuffle or create tokens must not mutate the board directly. The context gains:

```
ICardEffectActions Actions   // implemented by CardActionService
RandomService Random         // seeded, shared with the combat
```

`EffectProcessor` is constructed before `CardActionService`; after the service is created, the scene sets `_effectProcessor.Actions = _actionService` and `_effectProcessor.Random = randomService`. The processor's `CreateContext` copies both into every context. Tests can inject a fake actions port and a seeded random source.

Why not pass the service into the processor constructor: the service already depends on the processor to dispatch events. A late-bound port breaks the cycle without a service locator.

The action port exposes the operations the card effects need:

- `DestroyCard`, `DiscardCard`, `MoveCard`, `AddToHand`, `AddTokenToZone`, `CopyToHand`, `Transform`, `Merge`.
- `DrawFromDeck`, `ShuffleIntoDeck`, `BanishFromDeck`, `BuffTopCard`.
- `ReplaceZone`.

The port is a thin facade over `CardActionService`; the service remains the single mutation authority.

### 4. Continuous model: contributors, per-card auras, zone additions and repetition

`PowerResolver` currently asks each card about itself. The new model asks the board for *contributions*:

```
for each source card in a zone:
  source yields:
    self bonus            (Devil Dinosaur, Lizard, Namor, Strong Guy)
    other-card bonus      (Captain America)
    zone addition         (Klaw, to the right zone)
    zone multiplier       (Iron Man)
    repeat marker         (Onslaught)

apply suppression first (AbilitiesDisabled, Cosmo/Enchantress)
apply repeat count to every non-Onslaught source at the location
sum per-card bonuses into card power
sum card powers, apply multipliers, add zone additions
```

Onslaught rule: every non-Onslaught Ongoing source at a location applies `2^N` times, where `N` is the number of active Onslaughts at that location, capped at 4 (N=2 gives x4; N>=2 uses the cap). Onslaught does not repeat itself. This is deterministic, bounded and easy to test. Zone effects are not repeated by Onslaught; they are location effects, not card Ongoing sources.

Captain America targets "other allied Ongoing cards here"; the model checks whether the target card has at least one Ongoing effect and is not the source. Two Captain Americas buff each other. Enchantress flips `AbilitiesDisabled` on the targeted Ongoing cards; disabled sources stop contributing and stop receiving auras.

Klaw contributes a zone addition to the owner's side of the location to the right. The addition is applied after the multiplier, so the plaque matches `sum(cards) * multiplier + additions`.

Why a contributor pass: the existing `GetCardPowerBonus` cannot express "other cards" or "other zones". The pass makes the calculation explicit and keeps it idempotent. It is still O(cards²) at worst, which is negligible at 4 cards per side per zone.

### 5. Destruction, discard, replacement and regeneration

The action service gains pre-action replacement hooks:

```
BoardState.DestroyReplacements
BoardState.DiscardReplacements
```

A replacement returns `true` when it handled the action. The service checks replacements before `CardPileManager.Move`:

- Apocalypse: discard replacement -> remove from hand, add +4 persistent, return to hand (respecting the hand cap; if the hand is full after removal, it still returns because it left the hand).
- Wolverine: discard/destroy replacement -> remove from origin, add +2 persistent, place at a random location with room. If no location has room, it stays in the origin? The card is already removed; the replacement should leave it in the pile? The spec says regenerate at a random location; if no room, no placement is possible. We choose: it stays out of play for the combat and returns with the run deck next combat. This is a rare edge; record it.
- Angel: not a replacement; it reacts to `AfterDestroyed` by moving from hand/deck to the destroyed card's origin zone if there is room. It does not replace the destroyed card's identity; the destroyed card still goes to the destroyed pile.
- Nova: `AfterDestroyed` -> add +1 persistent to each other allied card.
- Armor: a `DestroyProtections` hook returns false for any card in its location.
- Professor X: a `ZoneRemovalConstraint` blocks destroy, merge, banish and return-to-hand from the location; movement is allowed.

Carnage and Deathlok destroy other allied cards at the location in list order. Each successful destroy emits its event and cascades. Carnage counts successful destroy events, including Wolverine's replacement? The `DestroyCard` result should report whether the destroy succeeded before replacement; Carnage counts the action, not the final pile. The spec says "for each destroyed"; we count successful destroy actions, even if Wolverine regenerates. If Armor/Prof X blocks, no count.

Why pre-action hooks: Apocalypse and Wolverine must not enter the pile and then be moved back; that would produce a transient pile entry and could fire the wrong viewer/energy behavior. The hooks keep the action atomic.

### 6. Movement: batches, pending next-play and reactions

`CardActionService.MoveCard` handles a single move. Add `MoveCards` for a batch:

```
snapshot zones
remove all moving cards
place each in its destination, left-to-right by source zone, if capacity
emit one AfterMoved per successful move
```

Heimdall moves each other allied card one left. A card in the leftmost location stays. The snapshot makes the result independent of iteration order. If a destination is full after removals, the card stays.

Doctor Strange selects the maximum Power among allied cards in other locations. All tied maximum cards are candidates. Move as many as fit in Strange's location; if not all fit, choose randomly among the tied candidates for the remaining slots.

Iron Fist writes a per-side `PendingNextPlay` entry. `PlayCard` consumes all pending entries after placing the card: Forge adds persistent Power, Iron Fist calls `MoveCard` one left. The entry persists across turns and is consumed even if the destination is full.

Kraven reacts to `AfterMoved` when another card enters his location; he does not trigger on his own movement. Multiple Man reacts to his own `AfterMoved` and adds a copy to the origin location if it has room. The copy is a new instance with the same definition and no played state.

Why a batch API: Heimdall's "all move left" is simultaneous. A sequence of single moves can fail or produce a different board depending on order.

### 7. Hand cap, draw order and deck operations

Hand cap is 7 for both sides.

- `TurnManager.DrawCard` checks `hand.Count < 7`; if full, the draw is skipped and the top card stays in the deck.
- `CardActionService.AddToHand`, `CopyToHand` and `DrawCard` check the cap before creating/moving. A rejected effect addition leaves the source untouched.
- `ReturnToHand` checks the cap; a board-to-hand return with a full hand is rejected, the card stays in its slot, and the energy stays committed. The drag path cancels to origin.
- `Moon Girl` copies hand cards in hand order until the hand reaches 7; remaining copies are not created.
- `Sentinel` adds only if the hand has room; this replaces the old "no hand limit" rule.

Deck order: `SetupDecks` shuffles the player's combat deck before `OpeningHandDealer.Deal`, using the shared `RandomService`. The AI deck is already generated randomly. This makes America Chavez, Domino, Korg, Cable and Yondu meaningful and matches the card-game expectation. `starts_in_hand` cards are moved before the random fill; Domino is explicitly excluded.

Domino: `OpeningHandDealer` skips any card tagged `forced_turn_2_draw`. `TurnManager.StartTurn` on turn 2 searches the deck for Domino and draws her if the hand has room; turn 1 draws skip her. If the hand is full on turn 2, she stays in the deck and can be drawn later.

Yondu's banish is combat-scoped: `CardPileManager` gains a `combatScoped` flag for banished moves. The card enters the side's banished pile, is visible in the viewer, and is not removed from `RunState.Deck`; it returns next combat. The existing run-scoped banish remains available for future content but no Series 1 card uses it.

America Chavez buffs `deck[0]` with persistent Power. Cable draws the opponent's top card into the owner's hand if there is room. Korg inserts a Rock token (0/1, `IsToken`) at a random deck position. The Rock is combat-scoped because it is not in the run deck.

### 8. Copy, transform and merge identity

Add a `CardInstanceFactory`:

- `Copy(source, copyPower, copyPerks)` clones the definition and creates a new instance. By decision, copies carry the source's current persistent Power but **not** attached perks; perks are run progression and duplicating them would duplicate run rewards.
- `Transform(instance, newDefinition)` replaces the instance's definition and recomputes power/abilities. `HasRevealed` stays true.
- `CreateToken(def)` creates a non-draftable token instance.

Moon Girl/White Queen/Mantis use `CopyToHand` with the source's current power; the copies are independent. Morph uses `Transform` on the random opponent hand card; its own instance identity and reveal marker remain. Morph's own persistent bonuses are kept and recalculated on top of the new base, not copied from the source.

Hulkbuster uses `Merge`:

```
choose a random other allied card in the location
host.BonusPower += hulkbuster.CurrentPower
remove hulkbuster from the zone, mark MergedInto/IsMerged
do not add to any pile
do not emit AfterDestroyed/AfterDiscarded
```

The merged card's run-deck instance remains; `ResetForCombat` clears the merged state next combat. The host keeps its definition and abilities. If there is no other allied card, Hulkbuster stays on the board and the merge does nothing.

Why not add a fourth pile: the pile viewer and lifecycle are defined around three piles; a merge is not a discard/destroy/banish and must not trigger those reactions. A combat-only merged flag is smaller and semantically correct.

### 9. Zones: implemented effects, replacement, visibility and locks

Implement the three existing zone effects:

- `ZonePowerModifier` with target `all` and amount +1/-1 -> per-card power.
- `ZonePowerModifier` with target `highest_cost` and amount +2 -> each highest-cost card in the zone.

The zone effects are queried by `EffectProcessor.GetZoneCardPowerBonus(zone, card, side)` and included in `PowerResolver.CalculateCardPower`. They use the same derived calculation and update on every board change.

Scarlet Witch replaces her zone with a different `ZoneDef` from `ZoneRegistry.GetAll`. `CardActionService.ReplaceZone` keeps the card lists, sets the new definition, marks the zone revealed, bumps `Version`, and updates the location view's name/effect text. The three existing zones are the pool; the replacement excludes the current zone.

Uatu sets a per-side visibility flag at `StartCombat` if it is in the player's deck or hand. `ZoneInstance.IsRevealed` remains the real reveal state. A `IsVisibleTo(side)` helper combines `IsRevealed` with the flag; the location plaque and detail panel use it. The AI has no rendered view, so only the player flag matters, but the helper is side-aware for tests.

Professor X registers a `ZonePlayConstraint` (no play) and a `ZoneRemovalConstraint` (no destroy, merge, banish or return to hand). Movement is not a removal in this sense and stays allowed. The constraints are derived from an active, non-disabled Professor X in the zone; if he moves or is disabled, the lock follows or stops.

Why implement the three zone effects now: Scarlet Witch without location effects would be a text-only card. The three effects already exist in content and are simple, derived modifiers; implementing them makes the replacement meaningful and improves the existing game.

### 10. AI safety

The AI keeps its current "highest power per energy" choice, with two additions:

1. `CardDef` gains optional AI metadata (`AiAvoidWhenAlternatives` / tags: `self_discard`, `self_destroy`). Cards with mandatory self-harm score lower when another affordable card exists.
2. The AI's new actions go through `CardActionService`, so hand cap, capacity, events and energy stay consistent. Random targets use the shared seeded `RandomService`.

Why not a search-based AI: the change is already large, and the user asked for card implementation, not a new AI. The safety heuristic prevents the worst self-sabotage while keeping the existing behavior predictable.

### 11. Presentation and view synchronization

- `LayoutManager.GetHandCard` already overlaps cards when the row is too wide; a 7-card hand uses the same path. Add a harness/manual check at 7.
- `CardView` already renders any `CardInstance`; tokens get their name/power from the token definition. No new renderer.
- Merged cards are removed from the zone list; `SyncSlotViews` removes the view and the host's power updates through `RefreshPowerTotals`.
- `LocationView` currently copies `Name/EffectText` only when built. `UpdateViewLayout` must refresh them from `ZoneInstance.Definition` so Scarlet Witch's replacement is visible. It also needs the player visibility flag for Uatu.
- `DetailPreviewView` uses the same visibility helper for zones. The card panel reads the current definition, so Morph/copies show the transformed identity automatically.
- `PileViewerView` continues to show three tabs; merged cards are absent from all three.

### 12. Verification strategy

The `%TEMP%\opencode\eps-verify` harness grows per wave:

- Pool: 59 cards, 13 starter + 46 Series 1, no fillers, unique names/ids, descriptions ASCII and prefixed.
- Content: every card effect id resolves to an implementation; every effect definition has a factory branch.
- Per wave: one or more scenarios per card, with seeded random where the target is random.
- Cross-cutting: hand cap, draw skip, effect fizzle, return rejection, merge, Yondu banish, zone effects, Uatu, Scarlet Witch, Prof X, Onslaught.
- Integration: full 6-turn combat with a deck containing each wave; build; manual visual pass.

## Risks / Trade-offs

- **59 cards in one change is large** -> Deliver in eight waves; each wave has its own tasks, harness checks and manual pass. Do not mark a wave complete until its cards and cross-cutting rules pass.
- **Parameterized effects can hide card-specific edge cases** -> Keep card differences in parameters and add a per-card harness check; a card that cannot be expressed by a family gets its own small class.
- **Onslaught formula is a design choice** -> Use the bounded `2^N` rule, cap at x4, and test Iron Man + Onslaught, Captain America + Onslaught, and two Onslaughts.
- **Copy semantics can duplicate perks or buffs** -> Copies keep current persistent Power but not perks; record this rule in the design and test Moon Girl with a perk in hand.
- **Merge has no pile** -> Use a combat-only merged flag; ensure no destroy/discard events fire and the card returns through the run deck next combat.
- **Yondu's combat-scoped banish conflicts with the existing run-scoped banished pile** -> Add an explicit scope flag to the banish action; keep the run-scoped path for future content and test both.
- **Hand cap changes existing Sentinel behavior** -> Update the starter Sentinel scenario and the harness; the old "no hand limit" path must not remain.
- **Deck shuffle changes the opening hand** -> It makes the new deck-order cards meaningful; existing tests that assume the draft order must be updated. The `starts_in_hand` cards still move first.
- **Zone effects were previously unimplemented** -> The three existing zones now change power; existing combat tests and the harness must be updated to include their effect. This is a deliberate behavior change for Scarlet Witch.
- **Uatu and Scarlet Witch touch both plaque and detail panel** -> Use one visibility/replacement refresh path so the two views cannot disagree.
- **AI safety heuristic is not a planner** -> It prevents self-sabotage when alternatives exist but does not optimize; acceptable for this change.
- **Manual visual checks are required for 7-card hand, tokens, merge and Uatu** -> Include a per-wave screenshot pass and a final combat pass.

## Migration Plan

1. Land the content pool and effect library with no behavior changes yet; the 59 definitions are registered and the old fillers are removed.
2. Land the continuous model and the zone effects behind the new card content; update the power calculation tests.
3. Land destroy/discard, movement, hand/deck, copy/merge, and zone/visibility/lock waves in that order.
4. Update the AI and the UI views after the underlying actions exist.
5. Run the full harness and a manual combat; archive only when every wave is green.

Rollback is a code revert; the change introduces no external data migration. The run deck and card instances are in memory.

## Open Questions

None blocking. The user resolved the pool composition, hand cap, merge, Yondu banish and random-target selection. The remaining details are recorded as decisions above: copies keep persistent power but not perks; Doctor Strange moves all tied highest-power cards that fit, choosing randomly when not all fit; Kraven does not trigger on his own movement; Wolverine with no free location stays out of play for the combat.
