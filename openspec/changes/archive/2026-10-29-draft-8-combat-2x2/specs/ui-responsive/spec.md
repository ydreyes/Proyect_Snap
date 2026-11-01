# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

### Requirement: Location Plaque
The system SHALL show a compact location plaque between the rival and player grids.

#### Scenario: Plaque contents
- **WHEN** a location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals
- **AND** the power totals SHALL remain legible at the smallest supported window size
- **AND** the totals SHALL update as cards are played, moved or removed
