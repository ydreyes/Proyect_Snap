# Spec Delta

## MODIFIED Requirements

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

### Requirement: Drop Zone Validation
The system SHALL validate card drops against zone slots.

#### Scenario: Valid drop
- **WHEN** a dragged card is released over the player half of a location
- **THEN** the nearest free slot in that location SHALL be selected
- **AND** if a free slot exists, the card SHALL be placed there
- **AND** the card SHALL be removed from its origin (hand or previous slot)
- **AND** the zone power totals SHALL be recalculated

#### Scenario: Invalid drop
- **WHEN** a card is released outside the player half of every location, or the target location has no free slot
- **THEN** the card SHALL return to its origin
- **AND** no board state changes SHALL persist

#### Scenario: Drop on hand
- **WHEN** a card already on the board is released over the hand area
- **THEN** the card SHALL return to the player's hand
- **AND** it SHALL be playable again during the same turn

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
