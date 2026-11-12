# enemy-roster Specification

## Purpose

Defines the 23 run enemies, their fixed decks and act pools, the 1-of-2 selection, the fixed boss encounters, the difficulty behavior and the discovery produced when an enemy plays a card.

## Requirements

### Requirement: Enemy Roster

The system SHALL provide 23 enemy definitions for the run.

#### Scenario: Roster composition

- **WHEN** the enemy content is loaded
- **THEN** the roster SHALL contain 18 normal enemies
- **AND** it SHALL contain 4 sub-bosses
- **AND** it SHALL contain 1 final boss
- **AND** each act SHALL have exactly 6 normal enemies
- **AND** no two enemies SHALL share an id or a name

#### Scenario: Enemy identity

- **WHEN** an enemy is displayed
- **THEN** it SHALL have a name, a flavor phrase, a difficulty from 1 to 5 and an act pool
- **AND** a sub-boss or final boss SHALL be marked as a boss
- **AND** the final boss SHALL be unique

### Requirement: Enemy Decks

The system SHALL give every enemy a fixed 8-card deck.

#### Scenario: Deck composition

- **WHEN** an enemy encounter starts
- **THEN** its deck SHALL contain exactly 8 cards
- **AND** every card SHALL resolve to a registered, draftable definition
- **AND** no token definition SHALL be included
- **AND** a card SHALL appear at most 2 times in the same enemy deck
- **AND** the deck SHALL be shuffled with the encounter seed

#### Scenario: No random pool

- **WHEN** the AI builds its deck for an encounter
- **THEN** it SHALL use the selected enemy deck
- **AND** it SHALL NOT draw a random deck from the player's card pool
- **AND** the same enemy and seed SHALL produce the same initial deck

### Requirement: Enemy Selection

The system SHALL present two normal enemies from the current act before a normal combat.

#### Scenario: Two candidates

- **WHEN** the run opens `EnemySelect` in act 1, 2 or 3
- **THEN** the system SHALL offer exactly two distinct normal enemies from that act's pool
- **AND** the two candidates SHALL have different ids
- **AND** the player SHALL choose one before the combat starts
- **AND** the chosen enemy SHALL be stored in the pending combat snapshot

#### Scenario: Rematch after a defeat

- **WHEN** a combat against an enemy is repeated
- **THEN** the same enemy SHALL be used
- **AND** the selection SHALL NOT run again
- **AND** the new attempt SHALL use a new seed

#### Scenario: No selection for bosses

- **WHEN** the next combat is a sub-boss or the final boss
- **THEN** enemy selection SHALL NOT open
- **AND** the fixed boss SHALL be announced in `SubBossIntro`

### Requirement: Fixed Boss Encounters

The system SHALL use fixed bosses at the act boundaries and the final act.

#### Scenario: Act sub-bosses

- **WHEN** the run reaches combat 15 of act 1, act 2 or act 3
- **THEN** the sub-boss SHALL be the one assigned to that act
- **AND** the same sub-boss SHALL be used every run

#### Scenario: Final act bosses

- **WHEN** the run reaches the final act
- **THEN** sub-boss 4 SHALL be a unique enemy not used in acts 1-3
- **AND** the final boss SHALL be a unique enemy not used as a sub-boss
- **AND** both SHALL be fixed encounters

### Requirement: Difficulty Behavior

The system SHALL use the enemy difficulty to shape the AI play choice.

#### Scenario: Low difficulty

- **WHEN** the enemy difficulty is 1 or 2
- **THEN** the AI SHALL use the existing safe, power-per-energy heuristic
- **AND** it SHALL avoid self-damaging cards when a safe affordable card exists

#### Scenario: Medium difficulty

- **WHEN** the enemy difficulty is 3
- **THEN** the AI SHALL prefer the affordable card with the highest current power
- **AND** it SHALL still respect energy, zone limits and self-harm safety

#### Scenario: High difficulty

- **WHEN** the enemy difficulty is 4 or 5
- **THEN** the AI SHALL prefer high power and the zone where the player has the least power
- **AND** at difficulty 5 it SHALL prioritize the player's weakest zone before raw power when both choices are legal

### Requirement: Enemy Card Discovery

The system SHALL discover cards played by the enemy.

#### Scenario: Enemy play discovers

- **WHEN** the enemy plays a card from its hand
- **THEN** that card's definition id SHALL be added to the run's discovered card set
- **AND** the same card SHALL NOT be added twice
- **AND** the discovery SHALL be flushed to the profile at the next safe checkpoint

#### Scenario: Preview does not discover

- **WHEN** the player views a bundle preview or the collection
- **THEN** no card SHALL become discovered from that preview
- **AND** only a run context SHALL discover cards

### Requirement: Encounter Persistence

The system SHALL persist the enemy identity with the combat snapshot.

#### Scenario: Snapshot identity

- **WHEN** the player confirms an enemy or a boss
- **THEN** the pending combat snapshot SHALL store the enemy id and the encounter seed
- **AND** a resumed combat SHALL rebuild the same enemy and deck composition
- **AND** the attempt number SHALL be preserved
