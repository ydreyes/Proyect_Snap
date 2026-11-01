# Tasks

## 1. Pile State

- [x] 1.1 Add `PileKind` (discard, destroyed, banished) and `CardLocation` (deck, hand, board, discard, destroyed, banished), and store `Location` on each card instance. Verify: `dotnet build SnapClone\SnapClone.csproj` succeeds and the harness reads a freshly constructed card as `Deck`.
- [x] 1.2 Add a pile set with one ordered list per kind, count helpers and a `Get(kind)` accessor, rejecting an instance that is already in the set. Verify: harness adds the same instance twice and the second add is rejected with counts unchanged.
- [x] 1.3 Add player and AI pile sets plus a `Version` counter to the board state, starting empty. Verify: harness asserts a new board has zero cards in all six piles and `Version == 0`.
- [x] 1.4 Assign `Deck`, `Hand` and `Board` locations at the existing transitions: deck to hand on draw, hand to slot, slot back to hand, slot to slot, and AI hand to slot. Verify: a manual turn (draw, play, drag back to hand, replay) shows each card in exactly one place and the next draw does not duplicate it.

## 2. Pile Movement

- [x] 2.1 Add a per-combat pile manager that moves a card to a side's pile: remove it from that side's hand, deck and every zone, then append it to the pile. Verify: harness moves a hand card to discard, a board card to destroyed and a deck card to discard, and asserts each is in the pile, absent from its old container and reads the matching `Location`.
- [x] 2.2 Enforce origins: destroyed only from the board, discarded only from hand or deck, banished from any of the three; reject a card already in a pile. Verify: harness confirms board-to-discard and hand-to-destroyed are rejected with piles, board and `Version` unchanged.
- [x] 2.3 Reset per-combat card state on every move and remove a banished player card from the run deck, leaving the AI side and the run deck untouched when an AI card is banished. Verify: harness asserts cleared `IsInPlay`, `PlayedTurn` and `PlayOrderIndex`; run deck shrinks by exactly one for the player and is unchanged for the AI.
- [x] 2.4 Increment `Version` exactly once per successful move and never on a rejected one. Verify: harness compares the counter across one accepted and one rejected move.
- [x] 2.5 Prove the combat lifecycle: after discard, destroy and banish, rebuild a second board from the same run deck the way combat entry does. Verify: harness asserts discarded and destroyed cards are dealt again, the banished card never appears, and the new board's piles start empty.
- [x] 2.6 Construct the manager at combat entry, after decks and hands are ready, and keep it reachable for future effects. Verify: `dotnet build` succeeds and a manual combat still enters, deals and plays normally with no pile contents appearing on its own.

## 3. View Synchronization

- [x] 3.1 Add a reconciliation pass that, when `Version` changed, keeps the view of every card still in hand, drops views of cards that left, creates views for new cards and recalculates location power. Verify: with a temporary debug key that discards the first hand card, its view disappears in the same frame and the remaining hand reflows.
- [x] 3.2 Reconcile slot views for both sides: clear player and rival slots whose card is no longer in the zone, place zone cards that lost their view, and refresh each location's totals. Verify: with the debug key destroying a board card, its slot empties, the location total drops and the other slots keep their cards.
- [x] 3.3 Cancel a drag whose card no longer exists, releasing the energy reservation, and never return that card to hand or board. Verify: start dragging a hand card, trigger the debug move, and the card is neither in hand nor on the board while the energy badge matches the remaining cards.
- [x] 3.4 Call the reconciliation from both the update and the draw pass, guarded by the last synced version. Verify: a card moved between turns is gone before the next turn draws, and an unchanged version does no work.

## 4. Viewer Layout and Drawing

- [x] 4.1 Add the viewer rectangle to the layout manager, derived from the avatar bounds: 46% width, 30% height, below the top bar, left-aligned for the player and right-aligned for the rival, plus inner header, card-area and tab-row rectangles. Verify: at 720x1280 and after resizing the window, the panel sits under its avatar, stays on screen and mirrors sides.
- [x] 4.2 Add the viewer component with open state, side, selected pile and page, drawing a framed panel, a header with the selected pile name and page indicator, and three tabs with names and counts including zero. Verify: with a seeded pile, counts update after a move, the selected tab is highlighted, and the header reads the selected pile.
- [x] 4.3 Draw the selected pile as three cards per page, newest first, with previous and next arrows disabled when there is only one page and the page clamped when the pile shrinks. Verify: seed four cards, page forward and back, and confirm the last page shows the remaining card without a blank page.
- [x] 4.4 Render cards facing up with cost, power, name and perk badge, and show an empty state when the selected pile has no cards. Verify: a card with a perk and an empty pile are both legible in the panel.
- [x] 4.5 Draw the viewer above the board and hand but below the result modal, with a light scrim over the board area. Verify: the panel covers board cards, the top bar and timer stay readable, and the result modal covers the viewer.

## 5. Viewer Interaction

- [x] 5.1 Make the avatar portrait clickable and open the viewer for that side, closing it on a second click on the same avatar or a click outside, and switching sides when the other avatar is clicked. Verify: both avatars open their own piles, only one viewer is open at a time, and outside clicks close it.
- [x] 5.2 Insert the viewer into the input order before drag and drop: while it is open, board presses start no drag and no card moves, while its tabs, pages, close and the end-turn control still respond. Verify: pressing a hand card with the viewer open does nothing, and ending the turn from that state works.
- [x] 5.3 Close the viewer and end the turn when the play-phase timer expires, without leaving a drag active. Verify: open the viewer, let the timer reach zero, and the viewer closes with the turn ending normally.
- [x] 5.4 Remove the temporary debug key and any debug seeding. Verify: a search for the debug hook finds nothing, `dotnet build SnapClone\SnapClone.csproj` succeeds, and a normal combat opens both viewers empty.

## 6. Integration Verification

- [x] 6.1 Run the harness suite from `%TEMP%\opencode\eps-verify` and confirm every pile, origin, version and lifecycle assertion passes. Verify: `dotnet run --project "$env:TEMP\opencode\eps-verify\Verify.csproj"` exits with success and no failed assertion.
- [x] 6.2 Play a full manual pass with seeded moves on both sides: discard, destroy and banish from hand, deck and board; open each viewer, switch tabs, page through a pile, and confirm discarded and destroyed return in the next combat while the banished card does not. Verify each observation directly in the running game.
- [x] 6.3 Confirm the repository has no test-only or debug artifacts left and the project builds clean. Verify: `dotnet build SnapClone\SnapClone.csproj` succeeds with no unused debug code.
