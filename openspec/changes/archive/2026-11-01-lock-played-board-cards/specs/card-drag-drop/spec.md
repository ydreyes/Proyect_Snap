# Spec Delta

## MODIFIED Requirements

### Requirement: Card Dragging

The system SHALL allow dragging cards from the player hand, and from a player slot only when the card was played during the current turn, during PlayPhase, once the pointer moves beyond the tap threshold.

#### Scenario: Start drag

- **WHEN** player presses a card in their hand during PlayPhase and moves the pointer beyond the tap threshold
- **THEN** the card SHALL become draggable
- **AND** the card SHALL follow the mouse/finger position
- **AND** the card SHALL maintain drag offset

#### Scenario: Start drag from board

- **WHEN** player presses a card placed in a player slot during PlayPhase that was played during the current turn and moves the pointer beyond the tap threshold
- **THEN** the card SHALL be lifted from its slot and become draggable
- **AND** the slot SHALL be shown as free while the drag is active
- **AND** the zone power totals SHALL exclude the lifted card until it is placed again

#### Scenario: Older board card stays locked

- **WHEN** player presses a card placed in a player slot during PlayPhase that was played on an earlier turn and moves the pointer beyond the tap threshold
- **THEN** no drag SHALL start
- **AND** the card SHALL remain in its slot
- **AND** the card SHALL NOT be lifted, moved to the hand or moved to another slot
- **AND** the zone power totals SHALL NOT change
- **AND** the available energy SHALL NOT change

#### Scenario: Press without movement

- **WHEN** player presses a card and releases it without moving beyond the tap threshold
- **THEN** no drag SHALL start
- **AND** the card SHALL NOT be lifted, removed from the hand or removed from its slot
- **AND** no energy SHALL be reserved
- **AND** the tap SHALL open the card detail panel

### Requirement: Energy Refund on Return

The system SHALL release the energy of a card returned to the hand. Only a card played during the current turn may return from the board to the hand.

#### Scenario: Drag from board to hand

- **WHEN** a card played during the current turn is dragged from the board to the hand area
- **THEN** the card SHALL leave the board
- **AND** its cost SHALL be returned to the available energy

#### Scenario: Card from an earlier turn

- **WHEN** a card played on an earlier turn is pressed on the board
- **THEN** it SHALL NOT be lifted or returned to the hand
- **AND** it SHALL remain in its slot
- **AND** the available energy SHALL NOT change

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

- **WHEN** a card played during the current turn already on the board is released over the hand area
- **THEN** the card SHALL return to the player's hand
- **AND** it SHALL be playable again during the same turn
- **AND** its cost SHALL be released

#### Scenario: Timer expiry during drag

- **WHEN** the turn timer reaches zero while a card is being dragged
- **THEN** the card SHALL return to the player's hand
- **AND** the turn SHALL close
