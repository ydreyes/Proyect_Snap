# Spec Delta

## MODIFIED Requirements

### Requirement: Three Zones
The system SHALL provide exactly 3 zones for combat.

#### Scenario: Zone initialization
- **WHEN** combat begins
- **THEN** exactly 3 zones SHALL be created
- **AND** each zone SHALL support up to 4 cards per side (player and AI)
- **AND** each side's 4 slots SHALL be displayed as a 2 row by 2 column grid

### Requirement: Card Play
The system SHALL allow players to play cards from hand to zone slots during the play phase.

#### Scenario: Playing a card
- **WHEN** a player plays a card
- **THEN** the card SHALL be placed in the chosen free slot if the zone has space (max 4 per side)
- **AND** the play order SHALL be recorded
- **AND** zone play constraints SHALL be respected

#### Scenario: Energy is informational
- **WHEN** a player plays a card during the prototype
- **THEN** the card's cost SHALL NOT block the play
- **AND** the displayed energy SHALL NOT be consumed or refunded

### Requirement: Turn Phases
The system SHALL follow a defined turn structure.

#### Scenario: Turn structure
- **WHEN** a turn progresses
- **THEN** it SHALL go through TurnStart (draw for both sides)
- **AND** PlayPhase (time-limited play)
- **AND** TurnEnd (turn advance and end-of-turn resolution)

#### Scenario: Match length
- **WHEN** the sixth turn ends
- **THEN** the match SHALL be evaluated and no further turns SHALL start

## ADDED Requirements

### Requirement: Per-Turn Draw
The system SHALL draw one card for each side at the start of every turn while that side's deck has cards.

#### Scenario: Both sides draw
- **WHEN** a turn starts
- **THEN** the player SHALL draw 1 card from the player deck if available
- **AND** the AI SHALL draw 1 card from the AI deck if available
- **AND** both drawn cards SHALL be added to the corresponding hand

#### Scenario: Empty deck
- **WHEN** a side's deck is empty at turn start
- **THEN** that side SHALL draw nothing
- **AND** the turn SHALL still start normally
