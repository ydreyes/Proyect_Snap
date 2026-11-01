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
The system SHALL show a compact location plaque between the rival and player grids, with different contents depending on whether the zone is revealed.

#### Scenario: Plaque contents
- **WHEN** a location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals when the zone is revealed
- **AND** it SHALL withhold the name, effect text and art when the zone is unrevealed
- **AND** the totals SHALL update as cards are played, moved or removed

#### Scenario: Revealed plaque contents
- **WHEN** a revealed location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals
- **AND** the power totals SHALL remain legible at the smallest supported window size
- **AND** the totals SHALL update as cards are played, moved or removed

#### Scenario: Unrevealed plaque contents
- **WHEN** an unrevealed location is displayed
- **THEN** the plaque SHALL withhold the name, effect text and art
- **AND** it SHALL show an unknown marker instead of the name
- **AND** it SHALL still show the location index and both power totals
- **AND** the totals SHALL update as cards are played, moved or removed

### Requirement: Detail Panel Layout

The system SHALL place the card and zone detail panels over the combat board using percentage-based geometry.

#### Scenario: Panel placement
- **WHEN** a detail panel opens
- **THEN** it SHALL cover the board area and dim what is behind it
- **AND** its bounds SHALL scale with the viewport
- **AND** its close control SHALL be reachable at every supported window size

#### Scenario: Panel content fits

- **WHEN** the card or zone panel content is drawn at the smallest supported window size
- **THEN** the card, its badges and its descriptive text SHALL stay inside the panel
- **AND** the descriptive text SHALL remain legible using the default font

### Requirement: Text Rendering
The system SHALL display all texts using DefaultFont.

#### Scenario: Text display
- **WHEN** displaying text in any scene
- **THEN** text SHALL be rendered using SpriteFont
- **AND** DefaultFont SHALL be loaded with fallback handling
- **AND** all UI labels SHALL be visible
