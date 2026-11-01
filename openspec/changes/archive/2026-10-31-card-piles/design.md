# Design

## Context

See `proposal.md` for motivation. Current constraints that shape the approach:

- `BoardState` is recreated on every `CombatScene.OnEnter()`, so per-combat state placed there resets for free. It holds `PlayerHand`, `AIHand`, `PlayerDeck`, `AIDeck` and three `ZoneInstance`s, and nothing else.
- `SetupDecks()` copies the same `CardInstance` objects from `RunState.Deck` into `BoardState.PlayerDeck`, so discard and destruction do not need a "return" step: the next combat rebuilds the deck from the run deck. Banishing, however, must remove the card from `RunState.Deck`.
- Card identity is by reference. `CardInstance.Id` is the definition id, so two copies of the same card share `Id`; existing code already relies on `List.Remove(card)` and `Contains(card)` with default reference equality.
- Zone power is derived: `ZoneInstance.GetPlayerPower()`/`GetAIPower()` sum the cards currently in the list, so removing a card updates the totals without bookkeeping. `LocationView.RecalculatePower()` caches the totals for display and must be called after a change.
- Available energy is derived too: `TurnManager.AvailableEnergy` subtracts the cost of player cards in zones whose `PlayedTurn` equals the current turn. Removing a destroyed card from a zone therefore refunds its energy automatically, and no explicit refund call is needed.
- View state is manual: `CombatScene._handViews`, `LocationView.PlayerSlotCards` and `LocationView.OpponentSlotCards` are mutated by drag, drop and AI play. There is no reconciliation pass today.
- `CombatScene.Update()` order is modal → timer expiry → drag → end-turn button. The avatar is a plain `AvatarView` with no hit testing.
- `CardView.DrawInHand()` uses fixed font scales and draws cost, power, perk and name; at a card width around 90-100 px it is still legible on the 720x1280 design size.
- `Draw()` runs a first `spriteBatch.Begin()` pass for the board and hand, then a second pass for the modal. The viewer belongs to the first pass, after the hand, so it covers the board.

## Goals / Non-Goals

**Goals:**

- One place where a card changes pile, so future effects cannot invent a second removal path.
- Pile contents and board views that agree in the same frame as the change.
- A viewer that reads state only, with no mutation of its own beyond open/close, tab and page.

**Non-Goals:**

- Any effect implementation (OnReveal, Ongoing, EndOfTurn, Activate) or any card that uses piles.
- Animation of cards flying into a pile.
- Scrolling, search or filtering inside the viewer.
- Dropping a card from the board onto the viewer, or dragging cards out of a pile.
- Changing the deck cap, the 6-turn combat length or the 3-zone structure.

## Decisions

### 1. Piles live in `BoardState`, not in `RunState`

`BoardState` gains `PileSet PlayerPiles` and `PileSet AIPiles`, and `PileSet` holds three ordered lists plus a `Get(PileKind)` accessor. Because the board is recreated per combat, the "empty at combat start" rule and the "discarded and destroyed return next combat" rule need no code: they follow from `SetupDecks()` reading `RunState.Deck`.

Only banishing touches the run: `CardPileManager` calls `RunState.Deck.Remove(card)` for the player's side. AI cards never belong to the run deck, so banishing an AI card touches only its board containers.

Alternatives considered: a single flat list with a pile field per side (rejected: the viewer wants per-tab counts and ordered lists, and origin validation is clearer per pile); storing piles in `RunState` (rejected: it would make discarded cards survive the combat unless explicitly cleared, which is the opposite of the rule).

### 2. `CardInstance` carries an explicit location

`CardLocation { Deck, Hand, Board, Discard, Destroyed, Banished }` is stored on the card. The three pile values are set only by `CardPileManager`; the existing hand, deck and slot mutation points set `Deck`, `Hand` or `Board` inline. A card in a pile can never be re-added to a container: `PileSet.Add` rejects an instance already present, and `Move` rejects a card whose location is already a pile.

Alternatives considered: deriving the location from container membership on demand (rejected: effects will ask "is this card in the discard pile" often, and the marker makes double-adds a visible guard instead of a silent duplicate); a nullable `PileKind?` only, leaving hand/deck/board implicit (rejected: it cannot express "this card is in a pile" for the sync pass, which must decide whether a card still belongs to a hand or slot view).

### 3. `CardPileManager` is a per-combat gate

`CardPileManager` is constructed in `CombatScene.OnEnter()` with the current `BoardState` and `RunState`. It is not registered in `GameServices` because it is scoped to a combat and must see that combat's board.

```
Move(card, side, pile) -> bool
  v
validate origin
  Discard   <- Hand | Deck
  Destroyed <- Board
  Banished  <- Hand | Deck | Board
  (reject anything else, and reject a card already in a pile)
  v
remove from every container of that side
  hand / own deck / zone list / slot bookkeeping left to the view sync
  v
reset IsInPlay, PlayedTurn, PlayOrderIndex
  v
add to the pile list, set Location
  v
if pile == Banished and side == Player: RunState.Deck.Remove(card)
  v
BoardState.Version++
```

The manager owns only state, never views. Zone power is already derived and energy is already derived, so there is no recalculation to perform outside the view layer.

Origin rules are deliberate: destroying from the hand or deck would make "destroyed" indistinguishable from "discarded", and discarding from the board would silently remove power from a zone. When a move is rejected, nothing mutates and `Version` does not change.

Alternatives considered: a static helper with all lists passed in (rejected: every caller would repeat the same five removal steps); making the manager a service (rejected: it would need a board reference that changes every combat).

### 4. Version-based reconciliation instead of events

`BoardState.Version` increments only inside a successful `Move`. `CombatScene` keeps `_lastSyncedVersion` and calls `SyncViewsIfDirty()` at the start of `Update()` and at the start of `Draw()`. Two call sites are needed because an effect triggered at the end of `Update()` (for example during `EndTurn()`) must be reflected in the `Draw()` of the same frame.

```
SyncViewsIfDirty()
  if Version == lastSynced: return
  lastSynced = Version

  hand views: keep the view of every card still in PlayerHand (same instance),
              drop views whose card left, create views for new cards
  player slots: for each zone and slot, clear the slot and drop zone when its
                card is no longer in zone.PlayerCards; then place any zone card
                that has no view into the first free slot
  rival slots: same reconciliation against zone.AICards
  per location: RecalculatePower()
  if the dragged card is no longer in the hand or a zone:
      clear the drag and release the energy reservation
```

Reconciliation keeps existing `CardView` instances by card reference, so unrelated changes do not reset positions or drag state. `UpdateViewLayout()` then repositions the surviving views.

Dropping a `CardView` whose card is being dragged is safe because `BeginDrag` already removed it from `_handViews` and `PlayerHand` before the drop resolves.

Alternatives considered: C# events on the pile manager (rejected: subscription lifetime tied to the scene, and ordering against the draw pass is harder to reason about); rebuilding every view unconditionally (rejected: it destroys drag state and card sizes on every sync).

### 5. Viewer is a component, not a scene or a modal

`PileViewerView` holds `IsOpen`, `Side`, `SelectedPile` and `Page`, and draws itself. `CombatScene` owns one instance, updates layout on resize and forwards input. It is not a `Modal` because the modal blocks everything and stops the turn timer path; the viewer must leave the timer and the end-turn control working.

```text
+--------------------------------------------------------------+
| avatar |           status banner                 | avatar    |
+--------+-----------------------------------------+-----------+
   | click portrait                                    click |
   v                                                        v
+-------------------------------+          +-------------------------------+
| DESTERRADAS              1 / 2|          | DESCARTADAS            1 / 1  |
|  +------+  +------+  +------+ |          |  +------+  +------+  +------+ |
|  | card |  | card |  | card | |          |  | card |  | card |  | card | |
|  +------+  +------+  +------+ |          |  +------+  +------+  +------+ |
| (DESCARTADAS) (DESTRUIDAS) (DESTERRADAS) |          (tabs with counts)     |
+-------------------------------+          +-------------------------------+
   left-aligned under player                 right-aligned under rival
```

`LayoutManager.GetPileViewerBounds(bool player)` returns a percentage rectangle: width 46% of the screen, height 30%, top at the first location row (11.8%), left edge at `LeftAvatar.X` for the player and at `RightAvatar.Right - width` for the rival. Inside the panel, 12% is a header row, 56% is the card area and 32% is the tab row. Three card slots share the card area with a small gap; the card height follows the existing 1.35 aspect ratio and shrinks if the panel is short.

Pagination is three cards per page, newest first. The page is clamped when the pile shrinks while the viewer is open, so a card leaving a pile cannot leave the viewer pointing at an empty page.

Cards are drawn with the existing `CardView.DrawInHand()` after forcing `IsAffordable = true`, which already renders cost, power, perk badge and name at that size. A separate compact renderer would duplicate the whole card layout for no gain.

Alternatives considered: a full-screen overlay (rejected: the request asks for a small panel near the portrait, and the board should stay visible); one viewer per pile (rejected: three panels would cover the board and duplicate open/close logic).

### 6. Input priority and blocking

`Update()` becomes:

```text
1. modal visible                  -> only the modal button
2. timer expired                  -> close viewer, cancel drag, end turn
3. viewer open                    -> tabs, pages, close; end-turn button; return
4. avatar click                   -> open viewer for that side
5. drag and drop
6. end-turn button
```

Opening is handled before the drag check so a click on the top bar can never start a drag. While the viewer is open, only viewer controls and the end-turn button are processed; the board area is covered by a light scrim so the blocked state is visible, while the top bar and banner stay readable.

Closing happens on the same avatar, on a click outside the viewer, and on timer expiry. Clicking the other avatar switches sides without closing first.

Alternatives considered: pausing the timer (rejected: inspection would become free extra time); blocking the end-turn button too (rejected: it is a deliberate action and does not risk an accidental play).

### 7. Verification harness stays outside the repository

The project has no test project. The existing `%TEMP%\opencode\eps-verify` console project references `SnapClone.csproj` and can construct gameplay types without graphics. The harness seeds cards into a `BoardState`, exercises `CardPileManager` (valid and invalid origins, `Version` bumps, banished removal from `RunState.Deck`), and simulates the next combat by rebuilding the deck the way `SetupDecks()` does, asserting that discarded and destroyed cards are back and banished cards are not.

The viewer itself is verified manually in the running game, since it needs a viewport. The harness cannot open a window, and adding a graphics test would expand the change beyond its scope.

## Risks / Trade-offs

- **Explicit `Location` can desynchronize from container membership** → `CardPileManager` is the only writer for pile values, the existing container mutations set `Deck`/`Hand`/`Board`, and the harness asserts membership against `Location` after every move.
- **A new code path adds a card to a container without setting `Location`** → keep the mutation points small and centralized (`DrawFromDeck`, `AddToHand`, `PlaceInSlot`, `LiftFromSlot`); the sync pass logs nothing but the harness catches the mismatch for the paths it exercises.
- **Version-only sync misses changes made outside the manager** → existing non-pile paths already update views directly, so the sync pass is additive, not a replacement.
- **Reconciliation can duplicate a view** → match strictly by card reference, and treat a slot whose view's card is not in the zone list as stale before placing missing cards.
- **The popover covers part of the board** → it is anchored under the avatar, blocks board input while open, and closes on outside click; the timer and end-turn remain visible.
- **Piles only fill when effects exist** → the viewer is validated empty and with harness-seeded data; no temporary debug seeding ships in the scene.
- **Three cards per page may feel slow for large piles** → piles are bounded by deck size (12 for the player), so a full pile is at most four pages.
