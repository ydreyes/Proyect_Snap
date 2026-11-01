# Proposal

## Why

The current combat UI is designed for desktop/landscape with hardcoded positions and limited interactivity. To make the game playable on mobile devices in portrait orientation, we need responsive layout, proper text rendering, drag-and-drop card play, an explicit turn button, and a clear victory/defeat flow at turn 6 with a modal overlay.

## What Changes

- **Responsive Portrait Layout**: Adapt all combat UI to portrait orientation with adaptive scaling that works on any screen size. Replace hardcoded coordinates with percentage-based layout system.
- **Text Rendering Fix**: Ensure all texts are visible in scenes using DefaultFont with proper fallback mechanism.
- **Drag & Drop Card Play**: Allow players to drag cards from hand across the scene and drop them in valid zone slots (max 4 per side). Cards dropped outside valid zones return instantly to original hand position.
- **Explicit End Turn Button**: Add a "Finalizar Turno" button during PlayPhase to end turn explicitly (replacing reliance on Space key).
- **Turn Counter & Victory/Defeat**: Track turns and evaluate at turn 6 using total power sum (extensible for future win conditions). Show modal overlay with victory/defeat and "Continuar" button.
- **Zone Slot Visualization**: Show clear drop zones/slots for each of the 3 zones where cards can be played.

## Capabilities

### New Capabilities
- `ui-responsive`: Adaptive portrait layout system with LayoutManager, percentage-based positioning, and scaling for any screen size
- `card-drag-drop`: Drag and drop system for cards with hit testing, drop zones, validation, and instant return to hand on invalid drops
- `turn-flow-modal`: Turn tracking, explicit end turn button, turn 6 evaluation with extensible win conditions, and modal overlay for results

### Modified Capabilities
<!-- None -->

## Impact

**Affected code:**
- `Game1.cs` - Portrait orientation, layout initialization
- `CombatScene.cs` - Major refactor for responsive layout, drag/drop, turn button, modal
- `InputService.cs` - Drag state tracking
- `Scenes/*.cs` - Ensure text rendering works
- New: `Core/Layout/LayoutManager.cs`, `UI/Components/CardView.cs`, `UI/Components/DropZone.cs`, `UI/Components/Modal.cs`, `Gameplay/Combat/VictoryResolver.cs`, `Gameplay/Combat/IWinCondition.cs`

**Dependencies:**
- No new external dependencies - use MonoGame built-ins only

**Constraints:**
- Must work adaptively on any screen size in portrait
- Drag only allowed during PlayPhase
- Cards dropped outside valid zones return instantly to original position
- Modal appears on top of combat scene without destroying state
- Win condition evaluation extensible (start with total power sum)
- Use DefaultFont for all text
