# ui-responsive Specification

## Purpose
The UI responsive system provides adaptive portrait layout that works on any screen size using percentage-based positioning and scaling.

## Requirements

### Requirement: Portrait Orientation
The system SHALL force portrait orientation for mobile gameplay.

#### Scenario: Game initialization
- **WHEN** the game starts
- **THEN** the display SHALL be set to portrait orientation
- **AND** the resolution SHALL be adaptable to any screen size

### Requirement: Adaptive Layout System
The system SHALL provide a layout manager that calculates positions using percentages.

#### Scenario: Layout calculation
- **WHEN** the screen size changes or game initializes
- **THEN** the LayoutManager SHALL calculate all UI regions based on percentages
- **AND** all positions SHALL scale proportionally
- **AND** the layout SHALL work on any screen size

### Requirement: Portrait Layout Structure
The system SHALL arrange UI elements in portrait layout.

#### Scenario: Combat layout
- **WHEN** in combat scene
- **THEN** the screen SHALL be divided vertically into a top bar, a location area, a controls area and a hand area
- **AND** the location area SHALL show 3 location columns side by side
- **AND** each column SHALL show the rival grid above the location plaque and the player grid below it
- **AND** each side of a column SHALL show 4 card slots in a 2 by 2 grid
- **AND** the layout SHALL use the full portrait screen

#### Scenario: Slot scaling
- **WHEN** the viewport size changes
- **THEN** every card slot SHALL scale proportionally with its location column
- **AND** slots SHALL remain large enough to be dragged onto without pixel-precise aiming

### Requirement: Location Plaque
The system SHALL show a compact location plaque between the rival and player grids.

#### Scenario: Plaque contents
- **WHEN** a location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals
- **AND** the power totals SHALL remain legible at the smallest supported window size
- **AND** the totals SHALL update as cards are played, moved or removed

### Requirement: Text Rendering
The system SHALL display all texts using DefaultFont.

#### Scenario: Text display
- **WHEN** displaying text in any scene
- **THEN** text SHALL be rendered using SpriteFont
- **AND** DefaultFont SHALL be loaded with fallback handling
- **AND** all UI labels SHALL be visible
