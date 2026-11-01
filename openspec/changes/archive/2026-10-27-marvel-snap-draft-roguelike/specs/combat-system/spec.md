# Spec Delta

## Purpose

The combat system manages zone-based battles with priority calculation, card play, turn phases, time limits, and proper reveal ordering of cards based on priority and play order.

## ADDED Requirements

### Requirement: Three Zones
The system SHALL provide exactly 3 zones for combat.

#### Scenario: Zone initialization
- **WHEN** combat begins
- **THEN** exactly 3 zones SHALL be created
- **AND** each zone SHALL support up to 4 cards per side (player and AI)

### Requirement: Zone Reveal Order
The system SHALL reveal zones sequentially at the start of each turn.

#### Scenario: Turn start zone reveals
- **WHEN** a turn begins
- **THEN** zone 1 SHALL be revealed first
- **AND** zone 2 SHALL be revealed second
- **AND** zone 3 SHALL be revealed third
- **AND** each zone reveal MAY trigger zone effects

### Requirement: Priority Calculation
The system SHALL determine turn priority based on zone control at the end of each turn.

#### Scenario: Priority determination
- **WHEN** a turn ends
- **THEN** the system SHALL calculate which player controls each zone based on total power
- **AND** the player who controls more zones SHALL have priority for the next turn's reveal order
- **AND** priority ties SHALL have a defined resolution

### Requirement: Card Play
The system SHALL allow players to play cards from hand to zones during the play phase.

#### Scenario: Playing a card
- **WHEN** a player plays a card
- **THEN** the card SHALL be placed in the chosen zone if space is available (max 4 per side)
- **AND** the play order SHALL be recorded
- **AND** energy/cost requirements SHALL be satisfied
- **AND** zone play constraints SHALL be respected

### Requirement: Turn Phases
The system SHALL follow a defined turn structure.

#### Scenario: Turn structure
- **WHEN** a turn progresses
- **THEN** it SHALL go through TurnStart (draw, reveal zones)
- **AND** PlayPhase (time-limited play)
- **AND** TurnEnd (priority calculation, card reveals, end-of-turn effects)

### Requirement: Time Limits
The system SHALL enforce time limits on play phases.

#### Scenario: Time limit expiration
- **WHEN** the play phase time limit expires
- **THEN** the game SHALL proceed with whatever cards were played
- **AND** no additional cards SHALL be playable
- **AND** the turn SHALL advance to TurnEnd

### Requirement: Card Reveal Ordering
The system SHALL reveal cards in order based on priority and play order.

#### Scenario: Reveal sequence
- **WHEN** cards are revealed at TurnEnd
- **THEN** all cards of the player with priority SHALL be revealed first (in the order they were played)
- **AND** then all cards of the other player SHALL be revealed (in the order they were played)
- **AND** OnReveal effects SHALL trigger in this reveal order
- **AND** each card's OnReveal effect SHALL resolve before the next card in the sequence
