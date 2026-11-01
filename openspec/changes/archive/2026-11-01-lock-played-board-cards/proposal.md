# Proposal

## Why

A card that has already been played and revealed can still be dragged from its zone back to the hand on a later turn. Returning it to the hand clears its play state, so replaying it stamps it as played again and the reveal resolver triggers its On Reveal effect a second time. With cards such as Medusa, Star-Lord, Hawkeye or Sentinel, this recycles the same instance every turn and stacks permanent bonuses or creates extra copies. The current `card-drag-drop` spec allows this behavior, so the rule itself has to change, not just the code.

## What Changes

- A card on the board SHALL be draggable only during the turn in which it was played.
- A card played on an earlier turn SHALL stay locked in its slot: it cannot be lifted, moved to the hand or moved to another slot.
- Tapping a locked card without moving the pointer SHALL still open the card detail panel.
- A card played during the current turn SHALL keep the existing behavior: it can move between slots or return to the hand, releasing its energy cost.
- The reveal model is unchanged. Cards remain revealed once because a locked card can no longer leave the board and be replayed.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `card-drag-drop`: `Card Dragging` now limits board drags to cards played during the current turn, and `Energy Refund on Return` now states that only those cards can return to the hand. `Drop Zone Validation` is clarified so its hand-drop scenario names the same eligibility rule.

## Impact

- Code: `SnapClone/Scenes/CombatScene.cs` (`BeginDrag`, `AddToHand`, `CancelDragToHand` review, `BeginPress`/`OpenPreviewForPress` preservation), relying on `CardInstance.PlayedTurn` and `TurnManager.CurrentTurn`.
- Specs: `openspec/specs/card-drag-drop/spec.md`.
- Dependency: the completed but unarchived change `card-inspect-and-zone-reveal` also modifies `Card Dragging` (tap threshold). Archive it before archiving this change, or rebase this delta after it, so the two MODIFIED blocks do not overwrite each other.
- No changes to effect definitions, power resolution, content data or the AI.
