# Tasks

## 1. Layout System and Responsive UI
- [x] 1.1 Create Core/Layout/LayoutManager.cs with percentage-based region calculation and verify it compiles
- [x] 1.2 Update Game1.cs to force portrait orientation and initialize layout system
- [x] 1.3 Create UI/Components/FontService.cs for centralized font loading with fallback and verify DefaultFont handling
- [x] 1.4 Refactor CombatScene to use LayoutManager for all positions (zones, hand, button)
- [x] 1.5 Update other scenes to use proper text rendering with FontService

## 2. Drag and Drop System
- [x] 2.1 Extend InputService with drag state tracking (IsDragging, DraggedCard, StartPosition, Offset)
- [x] 2.2 Create UI/Components/CardView.cs for draggable card representation with original position tracking
- [x] 2.3 Create UI/Components/DropZone.cs for zone slots with hit testing and validation
- [x] 2.4 Integrate drag and drop into CombatScene during PlayPhase only
- [x] 2.5 Implement instant return to hand for invalid drops

## 3. Turn Flow and Modal
- [x] 3.1 Add turn counter to BoardState and update TurnManager
- [x] 3.2 Add "Finalizar Turno" button to CombatScene (enabled only in PlayPhase)
- [x] 3.3 Create Gameplay/Combat/IWinCondition.cs and Gameplay/Combat/VictoryResolver.cs with TotalPowerWinCondition
- [x] 3.4 Create UI/Components/Modal.cs for victory/defeat overlay
- [x] 3.5 Integrate turn 6 evaluation and modal display in CombatScene
- [x] 3.6 Add "Continuar" button functionality to proceed with run flow

## 4. Integration and Polish
- [x] 4.1 Ensure all 3 zones show 4 slots each in portrait layout
- [x] 4.2 Verify drag works across entire scene with proper hit testing
- [x] 4.3 Test turn flow up to turn 6 with modal display
- [x] 4.4 Build and verify no compilation errors
