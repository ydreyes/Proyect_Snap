# Tasks

## 1. Baseline

- [x] 1.1 Confirm the main `card-drag-drop` spec already includes the tap-threshold wording from `card-inspect-and-zone-reveal`: run `openspec show card-drag-drop --type spec` and verify `Card Dragging` mentions the tap threshold. If it does not, archive/sync `card-inspect-and-zone-reveal` first and re-check.

## 2. Lock cards after their play turn

- [x] 2.1 In `CombatScene.BeginDrag`, reject a board-origin card when `view.Card.PlayedTurn != _turnManager.CurrentTurn` before calling `LiftFromSlot`; verify in a manual combat that pressing and moving an earlier-turn card leaves it in its slot, starts no drag, and leaves both zone power totals unchanged.
- [x] 2.2 Add the defensive hand guard in `CombatScene.AddToHand` so a board-origin card from an earlier turn is placed back in its origin slot instead of entering the hand, with a comment explaining the invariant; verify by code inspection that every hand-add path is downstream of `BeginDrag`, and by dropping a locked card over the hand area in a manual combat (it must stay on the board).
- [x] 2.3 Keep `BeginPress` and `OpenPreviewForPress` unchanged for locked cards; verify that a press-and-release on an earlier-turn card opens the detail panel, while a press-and-move does not lift the card.

## 3. Preserve current-turn behavior

- [x] 3.1 Verify a card played during the current turn can still be dragged from its slot to another free slot, that its `PlayedTurn` and `PlayedZoneIndex` are preserved, and that the available energy does not change.
- [x] 3.2 Verify a card played during the current turn can still be returned to the hand: it leaves the board, its cost is released, and it can be replayed in the same turn.
- [x] 3.3 Verify timer expiry with a current-turn card in the air still returns it to the hand and closes the turn.

## 4. Reveal and bonus regression

- [x] 4.1 Play Medusa in the middle zone on turn 1, end the turn, and on turn 2 try to drag her to the hand or another zone; verify she stays in place, does not reveal again, and keeps a single +3 bonus.
- [x] 4.2 Repeat the lock check with Star-Lord, Hawkeye and Sentinel: verify no second On Reveal, no extra Sentinel in hand, and no stacked bonus after the lock.
- [x] 4.3 Verify a card returned to the hand during its play turn still reveals exactly once if replayed that same turn: no reveal while it is in hand, one reveal when the turn ends.

## 5. Build and validation

- [x] 5.1 Run `dotnet build SnapClone.sln` and verify 0 errors and 0 warnings.
- [x] 5.2 Run `openspec validate "lock-played-board-cards"` and verify the change is valid.
- [x] 5.3 Run one full manual combat covering hand play, current-turn slot move, current-turn hand return with refund, locked card drag attempt, locked card tap preview, timer expiry and match end; confirm no card is duplicated or lost.
