# Design

## Context

See `proposal.md` for motivation and `specs/` for the behavioral contract.

The combat scene already has three input consumers stacked by priority: the end-of-match modal, the pile viewer, and drag-and-drop. Drag starts on the press frame (`HandleDragAndDrop` calls `BeginDrag` immediately), and `BeginDrag` mutates the model: it removes a hand card from the hand, lifts a board card from its slot, and reserves energy. There is no notion of a tap.

The location plaque is built once per combat from `ZoneDef` into `LocationView.Name` and `LocationView.EffectText`, and `ZoneInstance.IsRevealed` exists but is only written by `TurnManager.RevealZones()`, which reveals all three zones on every turn. Nothing reads it.

`CardDef` has no descriptive text. All 20 cards are name, cost and base power. `FontService` already provides `DrawTextWrapped` and `DrawTextCenteredFit`, so the panel needs no new text machinery. Zone art is procedural (circles by index), so a large version needs no assets.

## Goals / Non-Goals

**Goals:**

- Make inspection and play different gestures without changing the drag feel.
- One panel component that renders either a card or a zone, with the same frame and close control.
- Keep the reveal rule in one authority (the turn manager), with views only reading it.
- Keep the change reversible: no persisted data, no new dependencies.

**Non-Goals:**

- Implementing card or zone effects. Descriptions are text; the effect processor stays unwired.
- Animations, transitions, card art assets, or a zoom/pan gesture inside the panel.
- Changing zone legality, priority calculation or turn length.

## Decisions

### Deferred drag with a movement threshold

A press no longer starts a drag. `CombatScene` records a *press candidate* (the card view under the pointer, its origin, and the press point) and starts the drag only when the pointer moves more than `TapThreshold` (10 logical pixels). Release before the threshold is a tap and opens the panel. Release after it is the existing drop logic.

Why not intercept on release while keeping the current immediate `BeginDrag`: the model would be mutated (card lifted out of hand, energy reserved) and then rolled back on every tap, which produces a visible twitch, a one-frame energy flicker, and makes the tap depend on the drop rules. Deferring is less code and no rollback.

Why not long-press: it delays inspection by a timer, competes with the 45-second turn, and is unnatural with a mouse.

The threshold is measured in screen pixels rather than card-relative units so it behaves the same on a hand card and on a small board card. Starting the drag at the threshold (not at the press point) means the card jumps by up to 10 px on the first drag frame; the drag offset is recomputed from the threshold-crossing point, so the jump is invisible.

### Tap targets and resolution order

On a tap, the candidate is resolved in this order:

1. Card in the hand (topmost hit by `HitTestHand`).
2. Card in a player slot, then in a rival slot.
3. Zone: the plaque or the column area for the tapped point.

The first hit wins, so tapping a card inside a zone opens the card. The end-turn button and the energy badge sit outside the location columns, so a tap there never opens a zone panel.

Inside the pile viewer, the tap goes to the viewer first: tabs, page arrows, avatar, outside-close, then a visible card. A card tap opens the panel and leaves the viewer open; closing the panel returns to the viewer with the same side, tab and page.

### One panel, two contents

`DetailPreviewView` exposes `OpenCard(CardInstance, BoardSide)` and `OpenZone(ZoneInstance, LocationView)`, plus `Close()`, `IsOpen`, `Kind`, and `Draw(...)`. The frame, dimmed backdrop and close control are shared; only the content block differs.

- Card: large card frame with the cost circle, power hexagon (including the perk badge), the name band, and the description wrapped below the card. The frame uses the player or rival accent already used by `CardView`.
- Zone revealed: the same planet art drawn at panel scale, the name, the effect text and two hexagon totals.
- Zone unrevealed: locked frame, `LOCACION DESCONOCIDA`, the reveal turn (`Se revela en el turno N`), no name, effect or planet; totals still shown.

Why not a compact popover like the pile viewer: the reference is a full-screen card view, and a plaque-sized popover would not solve the legibility problem that motivates the change. Why not two components: the close control, dismissal, stacking and timer rules would be duplicated.

### Reveal authority and view sync

`TurnManager.RevealZones()` reveals `Zones[CurrentTurn - 1]` when the index is in range and leaves it revealed. The turn manager keeps `IsMatchOver` as is. `CombatScene` force-reveals every zone inside `EvaluateMatch()` before computing the result, so a match that ends before turn 3 still scores complete information.

`LocationView` gains `IsRevealed` and draws the locked plaque when false. `UpdateViewLayout()` copies the flag from the zone each frame (three booleans; no new version counter). The detail panel reads the same `ZoneInstance`, so plaque and panel cannot disagree.

Why not trigger reveal effects: the effect processor is not wired and the specs keep `MAY trigger zone effects`; wiring it would be a different change.

### Descriptions as content

`CardDef.Description` is added and filled for all 20 definitions in `ContentLoader`. Texts are written in Spanish, at most two short lines at the panel width, and describe the card's role. They do not claim triggers that the game does not implement. The panel shows the text as-is; no derivation from `EffectIds`, which are empty for every card.

Why not a separate content file: the project loads all content from `ContentLoader`; introducing a second source for one field would fragment the pipeline.

### Drawing and input order

`Draw` becomes: background, locations, hand, avatars, top bar, turn controls, pile viewer, detail panel, modal. The panel draws its own full-screen dim so it does not depend on the drawing order of the scene. `Update` checks in this order: modal, timer expiry, detail panel, pile viewer, pile-viewer opener, drag/tap, end turn. Timer expiry closes both the panel and the viewer before ending the turn.

## Risks / Trade-offs

- **Threshold feels wrong on some devices** → 10 px is generous for touch and still smaller than the smallest card width; the value lives in one constant.
- **Tap on a card in the hand that overlaps another** → `HitTestHand` iterates in draw order, so the topmost card wins, matching what the player sees.
- **Panel covers the board during the timer** → dismissal is a single tap anywhere outside the content; the timer expiring already closes the panel and ends the turn.
- **Descriptions overflow the panel at the smallest window** → the panel scales with the viewport and the text uses `DrawTextWrapped`; texts are authored short, and the harness checks the longest one against the smallest supported width.
- **Unrevealed zones still accept plays** → totals stay visible so the player can evaluate the play; this matches the spec and the genre convention.
- **Viewer and panel stacking** → closing the panel never changes the viewer state; the viewer is redrawn underneath on the same frame.
