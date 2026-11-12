# Spec Delta

## ADDED Requirements

### Requirement: Hand Capacity on Return

The system SHALL respect the 7-card hand limit when a played card is returned from the board to the hand.

#### Scenario: Full hand rejects the return

- **WHEN** a card played during the current turn is dragged from the board to the hand area
- **AND** the hand already has 7 cards
- **THEN** the return SHALL be rejected
- **AND** the card SHALL stay in its board slot
- **AND** its energy SHALL remain committed
- **AND** the drag SHALL cancel to its origin

#### Scenario: Room in the hand

- **WHEN** a card played during the current turn is dragged to the hand area
- **AND** the hand has fewer than 7 cards
- **THEN** the card SHALL leave the board and enter the hand
- **AND** its cost SHALL be refunded
- **AND** it SHALL be playable again during the same turn

#### Scenario: Timer expires during a full-hand drag

- **WHEN** the play phase timer expires while a played card is being dragged
- **AND** the hand has 7 cards
- **THEN** the card SHALL return to its board slot
- **AND** the turn SHALL close normally
- **AND** its energy SHALL remain committed

#### Scenario: Hand view stays consistent

- **WHEN** a return is rejected because the hand is full
- **THEN** the hand view SHALL show the same 7 cards
- **AND** the board view SHALL show the returned card in its slot
- **AND** no duplicate hand view SHALL appear
