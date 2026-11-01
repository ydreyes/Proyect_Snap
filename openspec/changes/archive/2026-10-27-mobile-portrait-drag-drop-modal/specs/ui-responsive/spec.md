# Spec Delta

## Purpose

The UI responsive system provides adaptive portrait layout that works on any screen size using percentage-based positioning and scaling.

## ADDED Requirements

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
- **THEN** UI elements SHALL be stacked vertically: top bar, zone 1, zone 2, zone 3, player hand, end turn button
- **AND** each of the 3 zones SHALL show 4 slots for cards
- **AND** the layout SHALL use the full portrait screen

### Requirement: Text Rendering
The system SHALL display all texts using DefaultFont.

#### Scenario: Text display
- **WHEN** displaying text in any scene
- **THEN** text SHALL be rendered using SpriteFont
- **AND** DefaultFont SHALL be loaded with fallback handling
- **AND** all UI labels SHALL be visible
