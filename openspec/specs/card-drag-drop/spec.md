# card-drag-drop Specification

## Purpose
The card drag and drop system allows players to drag cards from their hand and place them in valid zone slots during play phase.

## Requirements

### Requirement: Card Dragging
The system SHALL allow dragging cards from the player hand or from a player slot during PlayPhase.

#### Scenario: Start drag
- **WHEN** player clicks/touches a card in their hand during PlayPhase
- **THEN** the card SHALL become draggable
- **AND** the card SHALL follow the mouse/finger position
- **AND** the card SHALL maintain drag offset

#### Scenario: Start drag from board
- **WHEN** player clicks/touches a card placed in a player slot during PlayPhase
- **THEN** the card SHALL be lifted from its slot and become draggable
- **AND** the slot SHALL be shown as free while the drag is active
- **AND** the zone power totals SHALL exclude the lifted card until it is placed again

### Requirement: Drag Only During PlayPhase
The system SHALL restrict dragging to PlayPhase only.

#### Scenario: Drag restriction
- **WHEN** not in PlayPhase
- **THEN** cards SHALL not be draggable
- **AND** no drag operations SHALL occur

### Requirement: Energy-Aware Drop Validation
The system SHALL reject drops that the player cannot afford.

#### Scenario: Unaffordable drop from hand
- **WHEN** a card from the hand is dropped on a valid free slot
- **AND** its cost exceeds the available energy
- **THEN** the card SHALL return to the hand
- **AND** no board state SHALL change
- **AND** the available energy SHALL NOT change

#### Scenario: Affordable drop from hand
- **WHEN** a card from the hand is dropped on a valid free slot
- **AND** its cost does not exceed the available energy
- **THEN** the card SHALL be placed and its cost SHALL be deducted

### Requirement: Energy Refund on Return
The system SHALL release the energy of a card returned to the hand.

#### Scenario: Drag from board to hand
- **WHEN** a card played during the current turn is dragged from the board to the hand area
- **THEN** the card SHALL leave the board
- **AND** its cost SHALL be returned to the available energy

#### Scenario: Card from an earlier turn
- **WHEN** a card played on an earlier turn is dragged back to the hand
- **THEN** the available energy SHALL NOT change

### Requirement: Drop Zone Validation
The system SHALL validate card drops against zone slots and the available energy.

#### Scenario: Valid drop
- **WHEN** a dragged card is released over the player half of a location
- **THEN** the nearest free slot in that location SHALL be selected
- **AND** if a free slot exists and the card is affordable, the card SHALL be placed there
- **AND** the card SHALL be removed from its origin (hand or previous slot)
- **AND** a card coming from the hand SHALL have its cost deducted from the available energy
- **AND** the zone power totals SHALL be recalculated

#### Scenario: Invalid drop
- **WHEN** a card is released outside the player half of every location, the target location has no free slot, or the card's cost exceeds the available energy
- **THEN** the card SHALL return to its origin
- **AND** no board state changes SHALL persist
- **AND** the available energy SHALL NOT change

#### Scenario: Drop on hand
- **WHEN** a card already on the board is released over the hand area
- **THEN** the card SHALL return to the player's hand
- **AND** it SHALL be playable again during the same turn
- **AND** if it was played during the current turn, its cost SHALL be released

#### Scenario: Timer expiry during drag
- **WHEN** the turn timer reaches zero while a card is being dragged
- **THEN** the card SHALL return to the player's hand
- **AND** the turn SHALL close

### Requirement: Zone Slot Visualization
The system SHALL show drop zones for all 3 zones.

#### Scenario: Slot display
- **WHEN** in combat scene
- **THEN** each of the 3 zones SHALL display 4 slots per side in a 2 by 2 grid
- **AND** empty slots SHALL be visually indicated
- **AND** occupied slots SHALL show placed cards

#### Scenario: Slot highlight
- **WHEN** a card is being dragged over the player half of a location
- **THEN** the nearest free slot SHALL be highlighted
- **AND** the highlight SHALL disappear when the drag leaves that location or ends
