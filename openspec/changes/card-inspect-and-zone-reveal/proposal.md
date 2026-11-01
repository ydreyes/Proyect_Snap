# Proposal

## Why

A combat decision depends on information the player cannot currently read. Cards show cost and power but no ability text, and the only descriptive text that exists is on the three zones, squeezed into a plaque at a font size meant for labels. At the same time, every zone is revealed on turn 1 even though the combat spec calls for sequential reveals, so the reveal schedule buys the player nothing: hiding information is only meaningful if observing it is worthwhile.

These two gaps are the same problem. The player needs a readable inspector for cards and zones, and the zones need something worth revealing turn by turn. Fixing only the panel leaves the reveal cosmetic; fixing only the reveal leaves the effect text unreadable.

## What Changes

- **Tap to inspect, never to play**: a tap (press and release without moving) on a hand card, a board card on either side, a card inside the pile viewer, or a zone opens a detail panel. The tap does not lift the card, does not remove it from hand, does not reserve energy, and does not change board state.
- **Card panel**: a card rendered large, matching the attached Marvel Snap reference, with cost and current power (including the perk bonus), the name, and the card's descriptive text, wrapped to the panel width. Built entirely from MonoGame primitives; no image assets.
- **Zone panel**: the zone name, a larger version of its procedural art, its effect text, both power totals, and its reveal state. An unrevealed zone shows an unknown state and the turn it will be revealed instead of the name, text and art.
- **Panel lifecycle**: a full-screen overlay closed by its close control or by a tap outside its content, layered above the pile viewer and returning to it when opened from there. The turn timer keeps running while the panel is open, and the panel closes when the timer expires. While open, the panel covers the board and the end-turn control, so dismissing it takes one tap.
- **Drag starts on movement**: pressing a card no longer starts a drag. The drag begins only after the pointer moves past a tap threshold, so the same gesture surface supports both inspection and play without accidental lifts.
- **Card descriptions become content**: `CardDef` gains a descriptive text field and all 20 card definitions gain a text, in Spanish, describing what the card is meant to do. This makes the panel content real instead of an empty frame.
- **Sequential zone reveal**: one zone is revealed at the start of each turn during the first three turns of a combat (zone 1 on turn 1, zone 2 on turn 2, zone 3 on turn 3) and stays revealed for the rest of that combat. Each combat starts with all zones unrevealed.
- **Unrevealed zones stay playable and scorable**: the reveal changes information, not legality. A hidden zone keeps its index and both power totals, accepts cards, and counts for priority; only the name, effect text and art are withheld.
- **Reveal before resolution**: when a combat resolves before turn 3, any still-hidden zone is revealed before control and the result are computed.

**Out of scope:** implementing card or zone effects. The descriptions are text only, exactly like the existing zone descriptions; `EffectProcessor` stays unwired. No animations and no art assets.

## Capabilities

### New Capabilities

- `card-zone-preview`: the inspection panel for cards and zones, its contents per kind, its dismissal rules, its interaction with the turn timer and the pile viewer, and the tap gesture that opens it.

### Modified Capabilities

- `card-drag-drop`: a press alone no longer starts a drag; dragging begins once movement exceeds the tap threshold, so a tap cannot lift a card or reserve energy.
- `card-mechanics`: the card structure gains a descriptive text field alongside name, cost and power.
- `content-data`: the starter card pool gains a descriptive text per card.
- `combat-system`: zone reveal becomes one zone per turn across turns 1-3, unrevealed zones remain playable and scorable, and all zones reveal before combat resolution.
- `ui-responsive`: the location plaque gains an unrevealed state that withholds name, effect text and art while keeping index and totals, and the detail panel scales with the viewport.

## Impact

**Affected code:**

- `Data/Definitions/Definitions.cs` - `CardDef.Description`.
- `Data/Content/ContentLoader.cs` - 20 card descriptions.
- `Scenes/CombatScene.cs` - tap detection, inspect state, input priority against the pile viewer and the end-turn control, panel drawing order.
- `UI/Components/DetailPreviewView.cs` - new; card and zone layouts, wrapping text, close button.
- `UI/Components/LocationView.cs` - unrevealed plaque state and hit region.
- `UI/Components/CardView.cs` - large card rendering for the panel.
- `Core/Layout/LayoutManager.cs` - detail panel rectangle as percentages.
- `Gameplay/Combat/TurnManager.cs` - reveal one zone per turn, reveal all before resolution.
- `Gameplay/Board/ZoneInstance.cs` - reveal state already exists; used for drawing and play.

**Verification:** extend the existing harness at `%TEMP%\opencode\eps-verify` with reveal-order and description assertions, plus manual visual checks of both panel kinds, the hidden plaque, dismissal, the timer running with the panel open, and the tap-versus-drag threshold.

**Dependencies:** none new; MonoGame primitives and `DefaultFont` only.

**Assumptions recorded for review:**

- The tap threshold is measured in screen pixels and must be generous enough for a finger, not a mouse.
- The panel is informational: it never pauses the timer, because inspection must not buy free time.
- Totals remain visible on a hidden zone because they are derivable from the cards already on the board.
- Hidden zones accept plays, matching the genre convention that the reveal is information rather than permission.
