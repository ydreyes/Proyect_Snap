# content-data Specification

## Purpose
The content data system provides initial game content definitions for cards, zones, effects, and perks that can be extended easily.

## Requirements

### Requirement: Initial Zone Content
The system SHALL include 3 test zones with specific effects.

#### Scenario: Test zones
- **WHEN** the game starts
- **THEN** 3 test zones SHALL be available
- **AND** one zone SHALL give +1 power to all cards
- **AND** one zone SHALL give -1 power to all cards
- **AND** one zone SHALL give +2 power to the card of highest cost

### Requirement: Effect Library
The system SHALL include 20 effects total across 5 effect types.

#### Scenario: Effect content
- **WHEN** effects are initialized
- **THEN** there SHALL be 20 total effects
- **AND** they SHALL be distributed as 5 effects of each trigger type (OnReveal, Ongoing, EndOfTurn, Activate)
- **AND** effects SHALL be usable by cards and zones

### Requirement: Perk Library
The system SHALL include 3 initial perks.

#### Scenario: Initial perks
- **WHEN** perks are initialized
- **THEN** there SHALL be 3 perks available
- **AND** they SHALL provide +1 power, -1 power, and +2 power modifiers

### Requirement: Card Pool
The system SHALL include a starter card pool of at least 20 card definitions for drafting.

#### Scenario: Card pool initialization
- **WHEN** the game starts
- **THEN** a card pool of at least 20 cards SHALL be available for drafting
- **AND** cards SHALL cover varied costs from 0 to 6 and varied powers
- **AND** new cards SHALL be easily addable to the pool

### Requirement: Data-Driven Content
The system SHALL load content in a data-driven manner where possible.

#### Scenario: Extensible content
- **WHEN** adding new content
- **THEN** cards, zones, effects, and perks SHALL be definable through data structures
- **AND** new content SHALL not require core system changes
- **AND** content definitions SHALL be separate from game logic
