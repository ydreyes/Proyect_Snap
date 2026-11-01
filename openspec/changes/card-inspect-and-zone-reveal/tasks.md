# Tasks

## 1. Card descriptions

- [x] 1.1 Add `Description` to `CardDef` and verify the solution builds with `dotnet build SnapClone.sln`
- [x] 1.2 Write a Spanish description for all 20 cards in `ContentLoader` and verify the harness reports a non-empty description for each registered card

## 2. Zone reveal

- [x] 2.1 Reveal only zone `CurrentTurn - 1` in `TurnManager.RevealZones()` and verify with the harness that turn 1 reveals zone 1 only, turn 2 adds zone 2 and turn 3 adds zone 3
- [x] 2.2 Reveal every zone in `CombatScene.EvaluateMatch()` before the result is computed and verify that a match ending before turn 3 leaves all zones revealed
- [x] 2.3 Add the unrevealed state to `LocationView` (unknown marker, no name, effect text or art, index and totals kept) and verify visually that only zone 1 is legible on turn 1
- [x] 2.4 Sync `LocationView.IsRevealed` from `ZoneInstance` on every layout update and verify the plaque changes on the turn the zone is revealed
- [x] 2.5 Verify a hidden zone accepts a card from both sides and that its totals count for priority, by playing into it on turn 1 and checking the totals and the turn result

## 3. Tap gesture

- [x] 3.1 Add the tap threshold and the press-candidate state so a press alone does not call `BeginDrag`, and verify that pressing and releasing a hand card leaves it in the hand with the same energy
- [x] 3.2 Start the drag when the pointer crosses the threshold, recomputing the drag offset at that point, and verify a hand-to-zone drag still places the card
- [x] 3.3 Keep the existing release rules after a drag (place, return to hand, return to origin) and verify all three outcomes still work
- [x] 3.4 Resolve a tap against hand card, player slot card, rival slot card and zone, in that order, and verify tapping a card inside a zone opens the card and not the zone

## 4. Detail panel

- [x] 4.1 Add the detail panel bounds and close-control bounds to `LayoutManager` as percentages and verify they stay inside the viewport at 442 px and at the normal window width
- [x] 4.2 Implement the card layout in `DetailPreviewView` (frame, cost, power with perk bonus, name, wrapped description) and verify the longest description stays inside the panel at the smallest width
- [x] 4.3 Implement the revealed zone layout (planet art, name, effect text, both totals) and verify it matches the plaque values
- [x] 4.4 Implement the unrevealed zone layout (unknown marker, reveal turn, totals, no name, effect or art) and verify it on turn 1
- [x] 4.5 Implement dismissal by close control and by tap outside, and verify both close the panel without changing the board
- [x] 4.6 Draw the panel above the pile viewer and verify that closing it returns to the same side, tab and page

## 5. Integration

- [x] 5.1 Wire the update order (modal, timer expiry, detail panel, pile viewer, tap/drag, end turn) and verify that a single click never triggers two actions
- [x] 5.2 Close the panel and the pile viewer when the turn timer expires before ending the turn and verify the turn ends with the cards already played
- [x] 5.3 Open a card panel from a card inside the pile viewer and verify the viewer is still open after closing the panel
- [x] 5.4 Run `dotnet build SnapClone.sln` and the `%TEMP%\opencode\eps-verify` harness with zero failures
- [x] 5.5 Manual visual pass: tap every surface (hand, own board, rival board, pile viewer, zone), both panel kinds, hidden zone, close by X and outside, and the timer running with the panel open
