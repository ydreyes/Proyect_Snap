# Tasks

## 1. Content and draft state

- [x] 1.1 Add `card_9` through `card_20` to `ContentLoader.LoadCards` with the stats in `design.md`; verify `dotnet build` passes and a draft round can show cards with cost 0 and cost 6.
- [x] 1.2 Add `Reset()`, remaining-picks progress, exhausted-card filtering and per-row de-duplication to `DraftSystem`; verify a full 8-pick draft never offers a card already at 2 copies and never repeats a card inside the same row.
- [x] 1.3 Add `RunState.StartNewRun()` and write the 8 drafted cards into `RunState.Deck`; verify the deck has exactly 8 cards when the draft completes.

## 2. Scene lifecycle and services

- [x] 2.1 Add `OnEnter()` to `IScene` and call it from `SceneManager.ChangeScene` on every entry, with empty implementations in Menu/Reward/GameOver; verify entering Draft twice restarts progress at 0/8 instead of showing the previous picks.
- [x] 2.2 Register `RunState` and `DraftSystem` in `Game1.Initialize`; verify Draft and Combat resolve them without a service exception.
- [x] 2.3 Move combat setup (board, decks, hands, timer, turn 1) out of `LoadContent` into `CombatScene.OnEnter`; verify a second run starts with empty zones and 3+1 cards in hand.

## 3. Draft scene

- [x] 3.1 Rebuild `DraftScene` to render 3 choices, the `n/8` progress and a continue button disabled until 8 picks; verify the button cannot advance early.
- [x] 3.2 Implement pick, round advance and completion handoff to `Combat`; verify 8 clicks produce 8 distinct picks and the combat scene starts from that deck.

## 4. Combat layout

- [x] 4.1 Rewrite `LayoutManager` bands and `GetCardSlot` to a 2x2 grid; verify each side shows 4 slots in 2 rows and each slot is at least 78x105 px at 538x958.
- [x] 4.2 Redesign `LocationView`'s plaque for the 0.345-0.470 band (name, effect, large totals, art as backplate); verify name, effect and both totals are visible and totals update on play.
- [x] 4.3 Add drag-origin metadata and `ReturnToOrigin` support to `CardView`, and hover-slot highlighting in `CombatScene`; verify the nearest free slot highlights while dragging and clears on drop.
- [x] 4.4 Wire the energy badge, the `FINALIZAR TURNO n/6` label and the hand band to the new layout; verify the label reads the current turn and the hand stays below the controls.

## 5. Turn flow, draws and AI

- [x] 5.1 Extend `TurnManager` to draw 1 card for each side, expose `CurrentEnergy = min(turn, 6)` and report match end after turn 6; verify turn 1 starts with 4 player cards and 4 AI cards (3 dealt + 1 drawn).
- [x] 5.2 Make `CombatScene` use `TurnManager` for turn start/end instead of its own counter; verify the modal appears only after turn 6 and the timer restarts each turn.
- [x] 5.3 Rewrite `AIStrategy` to return at most one decision, prefer the zone with fewer AI cards and use a random tie-break; verify the AI plays exactly one card per turn and spreads across the 3 zones.
- [x] 5.4 Update `AIController` to return the played card/zone/slot and let `CombatScene` bind the rival `CardView`; verify rival cards appear on the board immediately and zone totals update.

## 6. Drag and drop interactions

- [x] 6.1 Implement hand-to-slot play with the column-magnet resolver; verify dropping anywhere on a location's player half lands in the nearest free slot.
- [x] 6.2 Implement board-to-hand re-drag; verify a played card returns to the hand, leaves the zone total and can be played again in the same turn.
- [x] 6.3 Implement board-to-board re-drag; verify the card moves to another free slot and both zone totals recalculate.
- [x] 6.4 Cancel invalid drops to the card's origin and timer-expiry drags to the hand; verify a board card dropped outside returns to its slot and a drag at timer 0 returns to hand with the turn closed.

## 7. Integration verification

- [x] 7.1 Run `dotnet build` and fix every warning-as-error or compile failure introduced by the change.
- [x] 7.2 Run a scripted playthrough: complete the draft, play 6 turns with hand, board and rival plays, reach the result modal; verify totals and capture screenshots at 538x958.
- [x] 7.3 Run `openspec validate draft-8-combat-2x2` and confirm every task above is checked.
