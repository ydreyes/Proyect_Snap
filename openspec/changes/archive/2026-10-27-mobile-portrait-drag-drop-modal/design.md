# Design

## Context

See proposal.md. Current CombatScene uses hardcoded coordinates, no drag/drop, uses Space to end turn, no turn counting to 6, no modal. Need responsive portrait layout.

## Goals / Non-Goals

**Goals:**
- Responsive portrait layout adaptable to any screen size
- Drag and drop cards from hand to valid zone slots during PlayPhase
- Explicit "Finalizar Turno" button
- Turn counting to 6 with modal showing victory/defeat
- Extensible win condition system
- Ensure all texts visible with DefaultFont

**Non-Goals:**
- Actual mobile touch optimizations beyond mouse (DesktopGL)
- Animations for card return (instant as specified)
- Complex modal animations

## Decisions

### Decision 1: Layout System
**Choice:** LayoutManager with percentage-based regions
- **Rationale:** Works on any screen size, easy to maintain
- **Implementation:** Calculate regions based on viewport, support design resolution scaling
- **Alternatives:** Hardcoded breakpoints (less flexible)

### Decision 2: Drag & Drop Architecture
**Choice:** CardView components with DropZone hit testing
- **Rationale:** Clean separation, reusable
- **Flow:** InputService tracks drag state, CardView handles visual, DropZone validates
- **Alternatives:** Direct manipulation in CombatScene (messy)

### Decision 3: Win Conditions
**Choice:** IWinCondition interface with VictoryResolver
- **Rationale:** Extensible for future rules
- **Default:** TotalPowerWinCondition summing all cards in all zones
- **Alternatives:** Hardcoded logic (not extensible)

### Decision 4: Modal Overlay
**Choice:** Draw modal on top of CombatScene without scene change
- **Rationale:** Preserves board state for display
- **Implementation:** Flag in CombatScene to draw modal overlay last
- **Alternatives:** Separate scene (loses state context)

### Decision 5: Font Handling
**Choice:** FontService with fallback
- **Rationale:** Centralized, handles missing DefaultFont gracefully
- **Alternatives:** Try/catch in each scene (repetitive)

## Risks / Trade-offs

- **[Layout complexity]** Migrating all hardcoded positions. *Mitigation:* Do systematically, test with different sizes
- **[Drag hit testing]** Cards must drop precisely. *Mitigation:* Use center point for hit testing
- **[Modal input blocking]** Must block all input below. *Mitigation:* Check modal state first in input handling
- **[Font missing]** DefaultFont may not exist. *Mitigation:* FontService with fallback, ensure Content setup
