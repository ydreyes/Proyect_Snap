# Spec Delta

## Purpose

The card drag and drop system allows players to drag cards from their hand and place them in valid zone slots during play phase.

## ADDED Requirements

### Requirement: Card Dragging
The system SHALL allow dragging cards from the player hand.

#### Scenario: Start drag
- **WHEN** player clicks/touches a card in their hand during PlayPhase
- **THEN** the card SHALL become draggable
- **AND** the card SHALL follow the mouse/finger position
- **AND** the card SHALL maintain drag offset

### Requirement: Drag Only During PlayPhase
The system SHALL restrict dragging to PlayPhase only.

#### Scenario: Drag restriction
- **WHEN** not in PlayPhase
- **THEN** cards SHALL not be draggable
- **AND** no drag operations SHALL occur

### Requirement: Drop Zone Validation
The system SHALL validate card drops against zone slots.

#### Scenario: Valid drop
- **WHEN** a dragged card is dropped over a valid zone slot
- **THEN** the slot SHALL be checked for availability (max 4 cards per side)
- **AND** if valid, the card SHALL be placed in that slot
- **AND** the card SHALL be removed from hand
- **AND** play order SHALL be recorded

#### Scenario: Invalid drop
- **WHEN** a dragged card is dropped outside valid zones or on occupied slots
- **THEN** the card SHALL return instantly to its original position in hand
- **AND** no changes to board state SHALL occur

### Requirement: Zone Slot Visualization
The system SHALL show drop zones for all 3 zones.

#### Scenario: Slot display
- **WHEN** in combat scene
- **THEN** each of the 3 zones SHALL display 4 slots per side
- **AND** empty slots SHALL be visually indicated
- **AND** occupied slots SHALL show placed cards
