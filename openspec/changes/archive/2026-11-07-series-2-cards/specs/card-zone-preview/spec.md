# Spec Delta

## MODIFIED Requirements

### Requirement: Zone Detail Contents

The system SHALL show a zone's identity, effect and current standing when it is revealed, and withhold the hidden information when it is not.

#### Scenario: Revealed zone

- **WHEN** the zone detail panel opens for a revealed zone
- **THEN** it SHALL show the zone name, its art, its effect text and both power totals
- **AND** the totals SHALL match the values shown on the location plaque

#### Scenario: Unrevealed zone

- **WHEN** the zone detail panel opens for an unrevealed zone
- **THEN** it SHALL show an unknown marker instead of the name, effect text and art
- **AND** it SHALL state the turn on which the zone will be revealed
- **AND** it SHALL still show both power totals

#### Scenario: Zone status

- **WHEN** the zone detail panel opens for a zone the player can see
- **THEN** it SHALL show `INUNDADA` when the location is flooded
- **AND** it SHALL show `BLOQUEADA` when the location is play-locked or fully locked
- **AND** the status SHALL match the location plaque
- **AND** it SHALL NOT show a status for a zone the player cannot see

## ADDED Requirements

### Requirement: Card Movement Status

The system SHALL show whether a card can currently be moved by the player.

#### Scenario: Moveable card

- **WHEN** the card detail panel opens for Nightcrawler or Vision
- **THEN** it SHALL show that the card is moveable
- **AND** it SHALL show whether the move is currently available or already used
- **AND** the status SHALL update after the card moves or the turn changes

#### Scenario: Cloak window

- **WHEN** the card detail panel opens during an active Cloak window
- **THEN** it SHALL state that the card can move to Cloak's location
- **AND** it SHALL NOT claim that the card can move to any other location

#### Scenario: No movement ability

- **WHEN** the card detail panel opens for a card without a movement permission
- **THEN** it SHALL NOT show a movement status
- **AND** it SHALL NOT imply that the card can be moved on a later turn

### Requirement: Location Plaque Status

The system SHALL expose flooded and locked status on the location plaque.

#### Scenario: Plaque status

- **WHEN** a location is flooded or play-locked
- **THEN** the plaque SHALL show a short status label
- **AND** the label SHALL remain legible at the smallest supported viewport
- **AND** the plaque SHALL keep the location name, effect and power totals visible

#### Scenario: Status clears

- **WHEN** the flood or lock stops applying
- **THEN** the plaque SHALL remove the status label in the same frame as the change
- **AND** the location SHALL return to its normal presentation
