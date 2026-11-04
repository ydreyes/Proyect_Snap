# Design

## Context

See `proposal.md` - Why.

The current engine is small and deliberately state-light:

- `EffectProcessor` caches stateless effect implementations and exposes only two continuous queries (`ICardPowerModifier`, `IZonePowerMultiplier`) plus the `OnReveal` pass.
- `RevealResolver` computes a snapshot of cards played this turn. `TurnManager.EndTurn` resolves `OnReveal`, then pending bonuses, then `PowerResolver.UpdateAllPowers`, then priority.
- `CardPileManager` is the only writer of pile membership. It removes a card from hand, deck or zone, sets `Location`, bumps `BoardState.Version`, and removes banished player cards from `RunState.Deck`.
- `CombatScene.PlaceInSlot` owns player plays; `AIController.TryPlayOne` owns AI plays. They duplicate validation, recording and view updates.
- `TurnManager.SpentEnergy` is derived from the cards that remain in a zone with `PlayedTurn == current`. `AIStrategy` recomputes the same thing independently.
- `CardInstance` has a single pending-bonus channel (`PendingBonusTurn`, `PendingBonusZoneIndex`, `PendingBonusAmount`) used by Hawkeye.
- `CardRegistry` is shared by content lookup, draft, rewards and the fallback player/AI deck. There is no token distinction.
- There is no test project. The `%TEMP%\opencode\eps-verify` console harness references `SnapClone.csproj` and can construct gameplay types without a graphics device.

The Series 1 program has six card batches after this change. Each depends on the same core, so this change must fix the core once and stay content-free.

## Goals / Non-Goals

**Goals:**

- Make one model-level authority for play, move, destroy, discard, draw, banish and creation, used by both the player and the AI.
- Make action events first-class and deterministic, so the six card batches can attach triggers without touching the core again.
- Enforce the four agreed core rules: reveal once per instance per combat, no reveal after leaving play, no energy refund on destroy/merge/banish, and `AfterPlayed` once per instance per turn.
- Keep the existing 20-card pool playable, with the three bug fixes as the only visible behavior changes.
- Leave the engine ready for tokens, copies, auras and zone locks without implementing them yet.

**Non-Goals:**

- `C0b`: continuous auras, cross-zone contributions, Onslaught repetition, Enchantress suppression.
- Any of the 46 Series 1 cards.
- Merge destination, Yondu's run-scoped banish, target-selection prompts, hand limit, location effects.
- AI heuristic improvements. The AI keeps its current "highest power per energy" choice.
- New UI screens, animations or a visual event log.

## Decisions

### 1. A combat-scoped `CardActionService`, not a scene helper

Add `Gameplay/Actions/CardActionService.cs`, constructed in `CombatScene.OnEnter()` with the current `BoardState`, `CardPileManager`, `PowerResolver`, `EffectProcessor` and `RunState`. It is not registered in `GameServices`, for the same reason `CardPileManager` is not: it is bound to one combat.

Public surface:

```
PlayCard(card, side, zoneIndex, slot) -> PlayResult
MoveCard(card, side, fromZone, toZone) -> ActionResult
LiftCard(card, side) -> ActionResult          // UI lift, no event, no energy change
ReturnToHand(card, side) -> ActionResult      // refunds if played this turn
DestroyCard(card, side, cause) -> ActionResult
DiscardCard(card, side, cause) -> ActionResult
DrawCard(side) -> DrawResult
BanishCard(card, side, cause) -> ActionResult
AddToHand(card, side) -> ActionResult
AddTokenToZone(tokenDef, side, zoneIndex) -> ActionResult
```

The service mutates the model and returns records. It never references MonoGame types. The scene creates or moves `CardView` objects from the returned records; `PlayAIAll` does the same for AI plays.

Why one authority: the Series 1 batches add play-triggered effects (Forge, Iron Fist, Angela, Bishop), destroy reactions (Nova, Angel, Wolverine), and movement reactions (Kraven, Multiple Man). If the scene and the AI keep separate paths, every such rule needs two implementations and two test suites. The `ai-opponent` spec already requires rule parity; this makes it structural.

Alternatives considered:

- Keep mutations in `CombatScene` and add a static event helper: rejected because the AI path would still bypass it.
- Put the service in `GameServices`: rejected because it needs a board reference that changes every combat, and the existing pile manager already establishes the per-combat pattern.

The service raises `BoardState.Version` on every model change, including power-only changes. `CombatScene.SyncViewsIfDirty()` already refreshes power totals and reconciles hand/slot views when the version changes. This avoids a second "power version" and guarantees no stale plaque after a triggered effect. The extra reconciliation is cheap at this board size.

### 2. Events are synchronous, queued, and ordered by a stable card scan

`ActionEvent` carries:

| Field | Meaning |
|---|---|
| `Kind` | Played, Moved, Destroyed, Discarded, Drawn, GameStart |
| `SourceCard` | The card the action happened to |
| `Side` | Owner of the source card |
| `OriginZoneIndex` | Zone the card left, or -1 |
| `DestinationZoneIndex` | Zone the card entered, or -1 |
| `Turn` | Current turn |
| `PlayOrder` | Play order when the event is a play |
| `Cause` | Manual or Effect; reserved for C2 |

`EffectProcessor.Dispatch(ActionEvent)` drains a queue. For each event it builds a snapshot of candidate cards in a defined order:

1. The player side first, then the AI side.
2. Within a side: zones left to right, then hand, then deck, then the three piles.
3. Within each container: list order.

Each candidate's `EffectIds` are filtered by the event's trigger. This means a card can react to an event that happened to another card (Angel, Nova, Kraven) without a separate subscription system, and the scan is bounded by the board size (at most a few dozen cards).

Why this order: it is deterministic and independent of which side initiated the action, which the spec requires. `GameStart` is the exception: it is dispatched once per side and each event scans only its own side. C1 and C2 can add card-specific ordering inside an effect if a rule needs it; they do not need to change the dispatcher.

Why a synchronous queue: the game already resolves turns in one frame. A queue gives cascade ordering without introducing timers, coroutines or a new phase. The loop guard is a hard cap of 256 events per top-level action; the service exposes `LastActionHitLimit` for the harness. Production play stops the cascade rather than throwing.

Alternatives considered:

- C# events/subscriptions: rejected for lifetime and ordering reasons, matching the archived `card-piles` decision.
- One broad "anything happened" event: rejected because the card batches need to filter by trigger and by origin/destination zone.
- Query only the source card: rejected because Angel, Nova and Kraven react to another card's action.

`GameStart` is dispatched by `CardActionService.StartCombat()` before the first turn. It scans the player's deck, hand and board; the AI's GameStart effects are not used by the current content, but the same dispatch runs for both sides.

### 3. Reveal-once is a marker on the instance; in-play is checked at resolution time

`CardInstance` gains `HasRevealed`. `EffectProcessor.ApplyOnReveal` does:

1. Skip if `HasRevealed` is already true.
2. Skip if the card is not in a zone. Zone membership is the authority; `IsInPlay` is a view convenience that direct test construction may not initialize. This is the destroyed-before-reveal guard.
3. Set `HasRevealed = true` before applying effects. The marker means "processed by the reveal sequence", not "ability successfully resolved"; C5 can use it for Cosmo suppression without re-opening the reveal question.
4. Apply the card's `OnReveal` effects.

`RevealResolver` keeps selecting cards with `PlayedTurn == current` and excludes `HasRevealed`. It cannot know which cards will be destroyed by an earlier reveal, so the in-play check lives in `ApplyOnReveal`.

`ResetForCombat` clears `HasRevealed`. `CardPileManager.Move` and `CardInstance.ClearPlayState` must not clear it. `AddToHand` must not clear it.

Why not a `RevealedTurn` field: the rule is once per combat, not once per turn. A bool is enough. If a future card needs "revealed this turn", it can compare `PlayedTurn` or add a turn marker later.

Why not filter in `RevealResolver` only: a card destroyed by an earlier card in the same reveal sequence is still in the resolver's snapshot. The resolution-time check is the only correct place.

### 4. The per-turn play marker lives on the board, not on the card

`BoardState` gains `HashSet<CardInstance> PlayedThisTurn`, cleared in `TurnManager.StartTurn`. `CardActionService.PlayCard` adds the card and emits `AfterPlayed` only when the card was not already in the set.

Why not a `PlayedThisTurn` bool on `CardInstance`: `ClearPlayState()` is called when a card returns to hand, and a bool there would be cleared at exactly the moment the rule says not to clear it. A board-level set is explicit and resets with the turn.

`CardInstance.PlayedTurn` stays as the last play turn, because reveal order and the play log use it. Returning to hand clears it, as today; replaying sets it again. `HasRevealed` is what prevents a second reveal.

### 5. The energy ledger replaces derived spending

Add `TurnEnergyLedger` per side to `BoardState`:

```
Charged   : total cost of cards played this turn
Refunded  : total cost returned to hand this turn
Charges   : card -> cost, for refund eligibility
Available = min(turn, 6) - (Charged - Refunded)
```

Rules:

- `PlayCard`: add the cost to `Charged` and to `Charges`.
- `ReturnToHand`: if the card is in `Charges`, remove it, add its cost to `Refunded`. This is the only refund path.
- `LiftCard`: no ledger change. The cost stays charged, so lifting a played card does not free energy.
- `DestroyCard`, `BanishCard`, merge: no ledger change.
- `MoveCard`: no ledger change.
- `StartTurn`: reset both ledgers and `PlayedThisTurn`.

Why this replaces `ReserveEnergy`: today the lift path removes the card from its zone, which made `SpentEnergy` drop, so the scene had to reserve the cost separately. With an explicit ledger, the charge survives the lift. Reserving would double-subtract. The `TurnManager.ReserveEnergy` and `ReleaseReservation` API is removed, and the scene's calls go away.

`TurnManager.AvailableEnergy` becomes `AvailableEnergy(BoardSide)`. `AIStrategy` uses the AI side instead of recomputing from zones. `BoardState.CurrentEnergy` remains the player's value for the energy badge.

Alternatives considered:

- Keep derived spending and add a "removal reason" flag: rejected because every new action would need to remember whether it refunds.
- Track only a single player ledger and give the AI a separate path: rejected by the parity requirement.

### 6. Capacity and constraints are checked by the service, not by the views

`ZoneInstance` gains:

- `CanAdd(BoardSide side)` returning `cards.Count < MaxCardsPerSide`.
- A list of play constraints (`IZonePlayConstraint`), empty in this change.

`CardActionService` checks capacity for every add: play, move, token, copy, merge and regeneration. If the destination is full, it returns a failure and the effect completes without adding the card. This is what prevents a fifth card from breaking the 2x2 view.

Movement between zones is validated after removing the card from its origin, so a full zone can receive a card if the same action frees a slot. C2's simultaneous Heimdall move will use a batch variant; this change only needs single moves.

Manual same-zone slot rearranges remain a view concern. They do not change the model, energy or play log.

`IZonePlayConstraint` is the hook C6 uses for Professor X. C0a registers no constraint, so the existing behavior is unchanged.

### 7. The play log replaces board scanning for "played here this turn"

`BoardState.PlayLog` holds `PlayRecord { Side, Card, ZoneIndex, Turn, PlayOrder }`.

- `PlayCard` appends a record.
- `ReturnToHand` removes records for that card, because the takeback is a correction: the card was not played in the final state.
- `DestroyCard` and `DiscardCard` keep the record; the card was played even if it later left play.
- `MoveCard` keeps the original zone.

`StarLordOnRevealEffect` migrates from scanning the board to reading the log. This closes the known gap from the archived starter-card change: a rival card that was played in the zone and then moved or destroyed still counts. The spec already says "played", so this is a fidelity fix, not a new rule.

The log is per combat and resets with the board. It is the foundation for Groot/Rocket (C7), Mantis (C4) and any future "played this turn" effect.

### 8. Tokens are definitions with an explicit non-draftable marker

`CardDef` gains `IsToken` (default `false`). `CardRegistry` keeps one store and adds `GetDraftable()`, which returns only non-token definitions.

Consumers:

| Consumer | Uses |
|---|---|
| `DraftSystem.GetChoices` | `GetDraftable()` |
| `RewardSystem.GetCardRewards` | `GetDraftable()` |
| `CombatScene.GetCardDefinitions` (fallback deck and AI deck) | `GetDraftable()` |
| Effect card creation | `Get(id)` on the full registry |

Why not a separate `TokenRegistry`: a second registry means every content lookup needs to know which registry to ask. A flag keeps one lookup and makes the exclusion explicit at the draft/reward boundary. The spec says a definition declares eligibility, which matches the flag.

C0a adds no token content, so the visible draft/reward pools are unchanged. The harness seeds a synthetic token and asserts it is not offered.

`CardDef` stays a shared, mutable definition. C4's copy/transform/merge work must clone a definition when it needs a different base power; C0a documents that expectation and adds no clone helper yet.

### 9. Scene and AI integration

```
CombatScene drag/drop
  +--> PlayCard from hand        -> CardActionService.PlayCard
  +--> drop back to hand         -> CardActionService.ReturnToHand
  +--> board-to-board same zone  -> view-only slot change
  +--> board-to-board new zone   -> CardActionService.MoveCard
  +--> lift from board           -> CardActionService.LiftCard
  +--> card destroyed while drag -> CancelVanishedDrag, no return

CombatScene.PlayAIAll
  +--> for each AIController play -> CardActionService.PlayCard via AIController
  +--> create the opponent CardView at the returned slot
```

`AIController.TryPlayOne` returns the play record from the service. `AIStrategy` uses `TurnManager.AvailableEnergy(BoardSide.AI)`. The AI's zone choice and card choice are unchanged.

The scene remains the only owner of `CardView`, `LocationView` and drop zones. The service returns enough information (card, side, zone, slot) for the scene to place the view without re-validating.

### 10. Content validation at load

`ContentLoader.LoadAll` gains a validation pass over card effects:

- Every effect id referenced by a card exists.
- `BaseEffectFactory.CreateEffect(def)` returns non-null for that definition.
- Cards do not reference zone-only trigger types (`ZoneContinuous`, `ZoneOnTrigger`).
- Zone effect definitions are exempt: they remain registered without implementations, as the content-data spec requires.

A failure throws during content load, before a combat starts. The harness calls the validator directly and asserts the current 20-card pool passes.

### 11. Harness

The project has no test project. This change follows the existing convention and extends `%TEMP%\opencode\eps-verify` with synthetic cards and effects:

- A card played, returned to hand and replayed reveals once.
- A card destroyed by an earlier reveal does not apply `OnReveal`.
- Destroying, merging and banishing a card played this turn do not change available energy.
- Returning a card to hand refunds its cost; replaying charges it again.
- Lifting a card does not free its cost.
- `AfterPlayed` fires once per instance per turn.
- Event order is identical across repeated runs and across player/AI initiators.
- The loop guard stops a synthetic self-triggering cascade.
- A token definition is not offered by draft or rewards.
- The current 20-card pool still passes content validation.
- Star-Lord counts a rival card that was played in its zone and then moved or destroyed.

Moving the harness into the repository is a separate decision; C0a does not change the solution structure.

## Risks / Trade-offs

- **Cascade complexity** -> The dispatcher is synchronous with a hard event cap; each batch is small; the harness covers a synthetic cascade. C1/C2 must add their own cascade checks.
- **Broad reaction scan cost** -> The board is small (3 zones, 4 cards per side, plus hand/deck/piles). A linear scan per event is negligible. If the card count grows, the scan can be indexed by trigger.
- **Energy ledger touches every drag path** -> The scene's `ReserveEnergy`/`ReleaseReservation` calls and `SpentEnergy` are removed. A missed call would show the wrong badge. The harness covers play, lift, drop, return, replay, destroy and banish; the scene is manually checked.
- **Removing `PendingBonus*` risks Hawkeye** -> The list-based `PendingEffects` must preserve the arming turn, watched zone, amount and clear-after-resolution behavior. The harness keeps the existing Hawkeye checks.
- **Star-Lord's play-log migration changes edge cases** -> A rival card that moved or was destroyed now counts. This matches the spec's "played" wording and is documented as a fix.
- **Manual cross-zone drags could become movement triggers in C2** -> The event payload carries `Cause`; C2 decides whether manual moves fire `AfterMoved`. C0a does not register movement triggers.
- **Token flag is only groundwork** -> No token content ships in C0a. C3/C4 must use `GetDraftable()` consistently; a missed consumer could offer a token. The harness seeds a token and checks draft and reward paths.
- **`CardDef` remains mutable** -> C4's copy/transform/merge must clone before changing base power. C0a documents this and does not add a clone helper.
- **No test project** -> The temp harness can drift from the repository. The tasks include running it; a permanent test project remains a backlog decision.
- **AI still plays every affordable card** -> C0a does not fix self-harm from Blade/Carnage; that is C1/C7. The AI's rules are now shared, so the fix will be one place.

## Migration Plan

No persisted state or content migration. Build `SnapClone.sln`, run the harness, and play one combat with the existing 20-card pool. Rollback is a code revert; the change adds no data files outside the OpenSpec change directory.

## Open Questions

None that block C0a. The following are deliberately deferred to the batch that introduces the content:

- Whether manual cross-zone drags count as `AfterMoved` for Kraven and Multiple Man (`C2`).
- The exact merge destination and whether a merged card enters a pile (`C4`).
- Whether Yondu's banish is run-scoped or combat-scoped (`C3`).
- The Onslaught repetition formula and cap (`C5`).
- Whether a card suppressed by Cosmo still gets its reveal marker; the design says the marker means "processed", and C5 will use that.
