# Design

## Context

See `proposal.md` for motivation. Current constraints that shape the approach:

- All rendering is MonoGame primitives (`ShapeDrawer`) plus one `SpriteFont` (`DefaultFont`); no textures or external assets.
- `LayoutManager` already computes every combat region from viewport percentages, but `GetCardSlot` lays out 4 slots in one row.
- `CombatScene` owns its own turn counter (`_currentTurn`) and timer while `TurnManager` owns a second, unused turn model. Only one can survive.
- `SceneManager` keeps scene instances alive and calls `LoadContent()` once at startup and `Initialize()` only on first entry, so combat and draft state leak across runs.
- `CardView` has `OriginalPosition` but no concept of where a card came from; a card placed on the board is removed from the hand and can never return.
- The 7 capabilities being modified already exist in `openspec/specs/` and their delta files are validated.

## Goals / Non-Goals

**Goals:**

- One turn model used by the scene, with per-turn draws for both sides and display-only energy.
- A 2x2 slot grid that keeps all three locations visible at once in portrait.
- A drag model that supports hand, board and hand-return with deterministic cancellation.
- A draft that produces the deck combat consumes.
- A repeatable run: re-entering Draft/Combat starts clean.

**Non-Goals:**

- Real energy economy, card effects, priority/reveal, snaps, reward flow, deck editing.
- Changing the 3-zone or 6-turn structure.
- New art assets.

## Decisions

### 1. TurnManager is the single turn authority

`TurnManager` already has `CurrentPhase`, turn increment and `StartTurn`/`EndTurn`. Extend it with:

- `DrawForBothSides()` at turn start, using `PlayerDeck`/`AIDeck` and respecting empty decks.
- `CurrentEnergy => Math.Min(CurrentTurn, 6)` as a display value.
- `StartTurn()` returns after entering PlayPhase; `EndTurn()` advances and reports whether the match is over (turn > 6) instead of silently looping.

`CombatScene` drops `_currentTurn` and asks the manager. Priority/reveal calls stay in `EndTurn` but are harmless no-ops today; they are out of scope.

Alternative considered: keep the scene-owned counter (rejected: two turn models, exactly the bug being fixed); introduce a new `TurnFlow` class (rejected: a third concept when one already exists).

### 2. Layout bands and 2x2 geometry

Percentages of viewport height:

```
0.000 - 0.118  top bar
0.120 - 0.345  rival 2x2      (2 rows)
0.345 - 0.470  location plaque
0.472 - 0.697  player 2x2     (2 rows)
0.697 - 0.765  energy + end-turn button
0.765 - 1.000  hand
```

Within a column (`width = (1 - 2*pad - 2*gap) / 3`):

```
slotWidth  = (columnWidth - 2*innerPad - innerGap) / 2
slotHeight = slotWidth * 1.35
row = slotIndex / 2, col = slotIndex % 2
```

At 538x958 this yields ~78x105 px per slot (~8,200 px2, about 4.2x today's 37x53). A 1.35 aspect (instead of the hand's 1.42) buys ~10 px per row for the plaque.

The plaque keeps the name, effect and both totals but drops the dedicated 56%-height art block: the art becomes a low-opacity gradient/planet backplate behind the plaque. At ~120 px tall this is the only way to keep the totals legible.

Alternative considered: keep 1x4 but enlarge slots (rejected: 4 cards in a 169 px column are inherently narrow); overlap cards (rejected: hit-testing becomes ambiguous).

### 3. Drag origin model

`CardView` gains:

- `DragOrigin { Hand, Board }`
- `OriginZone`, `OriginSlot` (valid when `Board`)

Two explicit helpers own the hand/board invariants so no call site mutates both lists independently:

- `TakeFromHand(view)` / `AddToHand(view)`
- `LiftFromSlot(zone, slot)` / `PlaceInSlot(zone, slot, view)`

`PlaceInSlot` updates `LocationView.PlayerSlotCards`, `ZoneInstance.PlayerCards`, `DropZone.OccupiedBy`, card flags and zone power. `LiftFromSlot` reverses all of them. On drag end:

- valid column target → `PlaceInSlot`
- hand area and origin Board → `AddToHand`
- otherwise → return to origin (`AddToHand` if origin Hand, `PlaceInSlot` if Board)
- timer expiry → `AddToHand` (explicit product decision)

`PlayedTurn`/`PlayOrderIndex` are preserved when a board card is moved; without reveal effects, re-stamping them adds churn with no observable benefit.

### 4. Column magnet

While dragging, a target resolver maps the pointer to a location:

- candidate if the pointer is inside the location's player region (plaque + player 2x2), expanded by 8%;
- the chosen slot is the free slot whose center is nearest to the pointer;
- if the location has no free slot, the drop is invalid;
- the resolver returns a `(zone, slot)` pair used both for highlighting and for the drop, so highlight and result can never disagree.

Alternative considered: per-slot expanded rectangles (rejected: dead zones between slots, more code for a worse result).

### 5. Scene lifecycle reset

Add `OnEnter()` to `IScene`, called by `SceneManager.ChangeScene` on **every** entry. `Initialize()` stays one-time. `CombatScene.OnEnter` rebuilds `BoardState`, deals hands, resets the timer and phase; `DraftScene.OnEnter` resets draft progress. Menu/Reward/GameOver get empty implementations.

Alternative considered: reuse `Initialize()` on every entry (rejected: misleading contract, easy to reintroduce the bug); recreate scene instances in `SceneManager` (rejected: bigger refactor, breaks the service wiring).

### 6. Draft

`DraftSystem` gains `Reset()`, `RemainingPicks`, and a `GetChoices()` that:

- excludes definitions already at 2 copies;
- de-duplicates the row;
- degrades gracefully if fewer than 3 eligible cards remain (pool of 20 makes this unreachable in practice).

`DraftScene` renders the 3 choices, 8 progress pips, and a continue button enabled at 8. On completion it writes the picks into `RunState.Deck` and changes to Combat. The rival deck is 8 random definitions generated on combat entry; it does not use the draft.

### 7. Rival play

`AIStrategy.MakeDecision(boardState)` returns at most one `(card, zone, slot)`:

- if the hand is empty or all 12 slots are occupied → none;
- zone = fewest AI cards, random tie-break;
- slot = first free index in that zone;
- no cost filter while energy is informational (the filter stays behind a flag for the future change).

`AIController.TryPlayOne(boardState)` applies it and returns the played `(card, zone, slot)`. Combat creates the `CardView`, binds it to `OpponentSlotCards` and recalculates power. `AIHand` is never drawn.

### 8. Card content

20 definitions total; the existing 8 keep their ids, the 12 new ones are `card_9..card_20`:

| id | Name | Cost | Power |
|----|------|------|-------|
| card_9 | Rookie | 1 | 3 |
| card_10 | Watcher | 2 | 1 |
| card_11 | Raven | 2 | 4 |
| card_12 | Brawler | 3 | 2 |
| card_13 | Colossus | 3 | 5 |
| card_14 | Wraith | 4 | 2 |
| card_15 | Juggernaut | 4 | 6 |
| card_16 | Devourer | 5 | 3 |
| card_17 | Herald | 5 | 7 |
| card_18 | Sentinel | 6 | 5 |
| card_19 | Leviathan | 6 | 8 |
| card_20 | Spark | 0 | 1 |

### 9. Energy

`CurrentEnergy = min(turn, 6)`, painted in the existing badge. It does not gate drops, is not decremented and is not refunded. `combat-system` and `ai-opponent` deltas record this explicitly so the old cost requirements do not resurface at archive time.

## Risks / Trade-offs

- **Hand/board duplication** → single pair of helpers (`AddToHand`/`TakeFromHand`) and no direct list mutation elsewhere.
- **Timer fires mid-drag** → cancel before advancing the turn; at turn 6 cancel before evaluating so the modal never appears over a floating card.
- **`CardRegistry.GetAll()` order is not a contract** → never rely on it for hands; draft randomizes, rival deck randomizes.
- **Plaque text at 120 px** → shrink-to-fit on the name, wrap the effect at the smallest scale, big totals; if the effect does not fit it is clipped rather than overlapping the totals.
- **Scene reset is a new lifecycle hook** → implemented once in `SceneManager` and exercised by entering Draft twice in verification.
- **Energy is decorative, so high-cost cards dominate the draft** → accepted for the prototype; the filter lands in the deck/economy change.
