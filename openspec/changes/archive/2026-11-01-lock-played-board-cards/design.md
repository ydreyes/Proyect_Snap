# Design

## Context

See `proposal.md` for the motivation. The relevant current behavior:

- `CombatScene.BeginDrag` lifts any player board card during PlayPhase, removes it from `ZoneInstance.PlayerCards` through `LiftFromSlot`, and starts the drag.
- Dropping over the hand area calls `AddToHand`, which sets `IsInPlay = false`, `Location = Hand` and calls `CardInstance.ClearPlayState()`. That clears `PlayedTurn`, `PlayOrderIndex`, `PlayedZoneIndex` and any pending delayed condition, while keeping `BonusPower` and perks.
- `RevealResolver.GetRevealOrder` includes a card only when `PlayedTurn == CurrentTurn`. Replaying a returned card stamps the current turn, so the card is revealed and its On Reveal effect fires again.
- `TurnManager.CurrentTurn` is the play-turn identity. A card's `PlayedTurn` is stamped only when it enters a zone from the hand and is preserved when the card moves between slots.
- The current `card-drag-drop` spec allows an earlier-turn card to return to the hand. The completed but unarchived change `card-inspect-and-zone-reveal` also modifies `Card Dragging` to add the tap threshold.

## Goals / Non-Goals

**Goals:**

- Lock a board card once the turn in which it was played ends.
- Keep every card played during the current turn fully draggable: slot to slot, or back to the hand with its cost released.
- Preserve tap-to-inspect for locked cards.
- Enforce the rule at the single point where a board card leaves its slot, with a defensive check on the hand path.
- Keep the existing reveal model; no new revealed flag.

**Non-Goals:**

- No visual lock indicator, tooltip or rejection animation.
- No change to the AI, effect definitions, power resolution or content data.
- No new bounce/return-to-hand effects.
- No change to how `PlayedTurn` is stamped or preserved.

## Decisions

### Eligibility predicate

A board card is draggable only when `view.Card.PlayedTurn == _turnManager.CurrentTurn`. A card with `PlayedTurn == 0` (for example, a card placed on the board by a future effect) is treated as locked unless the effect explicitly stamps the current turn. This keeps the predicate small and reuses the state the reveal system already depends on.

Alternative considered: a separate `CanBeMoved` flag on `CardInstance`. Rejected because it duplicates `PlayedTurn` and would need to be cleared at the same points, creating a second source of truth.

### Gate at the lift point

`BeginDrag` is the only place a player board card is removed from its slot. Add the predicate there, before `LiftFromSlot`:

```text
if origin == Board and card.PlayedTurn != CurrentTurn:
    do not set drag state
    do not call LiftFromSlot
    return
```

This guarantees the card never leaves its slot, so zone power never drops and no drop, timer or end-turn path can see a locked card in the air. `BeginPress` and `OpenPreviewForPress` stay unchanged: a press without movement still records the card and opens the detail panel.

Alternative considered: reject only the drop over the hand area. Rejected because the user chose a full lock; allowing the lift would still let a locked card be repositioned between zones and would make the power total flicker during the drag.

### Defensive hand guard

`AddToHand` gets an eligibility check as a second line of defense:

```text
if view.Origin == Board and card.PlayedTurn != CurrentTurn:
    put the card back in its origin slot
    return
```

This should be unreachable once `BeginDrag` is gated, but it keeps a future caller from bypassing the rule. Hand-origin drops remain allowed: the card was removed from the hand when the drag began, so returning it is not a board return.

`CancelDragToHand` needs no new branch. With the lift gate, only hand-origin cards or current-turn board cards can be in the air when the timer expires, and today's behavior (return to hand and close the turn) stays correct.

### Spec delta includes the pending tap-threshold wording

`card-inspect-and-zone-reveal` is complete but unarchived and modifies `Card Dragging`. The safe order is to archive it first. The delta for this change nevertheless carries its tap-threshold text plus the new lock rule, so archiving this change after the pending one does not drop the tap behavior. Archiving this change before the pending one would let the later archive overwrite `Card Dragging` with a version that lacks the lock rule, so that order must be avoided.

### No revealed flag

The lock closes the only current path that clears `PlayedTurn` on a revealed board card. Adding a `HasRevealed` flag now would change how future bounce effects behave without a product decision. If bounce effects are added later, that decision belongs to that change.

## Risks / Trade-offs

- **Cards become immobile after their turn** -> intentional. If repositioning revealed cards is wanted later, it needs its own rule and spec change.
- **A failed drag gives no feedback** -> the card simply stays in its slot. Tap-to-inspect still works. A lock cue can be added later without changing this design.
- **Effect-placed cards are locked immediately** -> intended default; an effect that wants a card to be movable must stamp the current turn.
- **Pending spec change collision** -> mitigated by archiving `card-inspect-and-zone-reveal` first and by carrying the tap-threshold text in this delta.
- **Future return-to-hand effects** -> they will re-trigger On Reveal as today. That is acceptable until a bounce mechanic explicitly defines its behavior.

## Open Questions

- Should a locked card show a visual cue or a brief rejection state when the player tries to drag it? Default for this change: no; keep the interaction silent and preserve tap-to-inspect.
