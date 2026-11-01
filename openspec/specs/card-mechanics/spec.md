# card-mechanics Specification

## Purpose
The card mechanics system defines card structure, types, costs, power, and ability activation rules.

## Requirements

### Requirement: Card Structure
The system SHALL represent cards with cost, power, and effects.

#### Scenario: Card properties
- **WHEN** a card is defined or instantiated
- **THEN** it SHALL have an ID, name, cost (energy), and base power
- **AND** it MAY have associated effects by trigger type
- **AND** it MAY have tags

### Requirement: Card Trigger Types
The system SHALL support cards with OnReveal, Ongoing, EndOfTurn, and Activate abilities.

#### Scenario: Card abilities
- **WHEN** a card has abilities
- **THEN** abilities SHALL be categorized by their trigger type
- **AND** each ability SHALL trigger at the appropriate game event

### Requirement: Activate Abilities
The system SHALL support cards with activated abilities that players can trigger.

#### Scenario: Activating ability
- **WHEN** a player activates a card's ability during play phase
- **THEN** the ability SHALL only activate if conditions are met
- **AND** energy costs SHALL be paid if required
- **AND** the activation SHALL be tracked to prevent invalid reuse

### Requirement: Card Instances
The system SHALL track card instances in play with dynamic state.

#### Scenario: Dynamic card state
- **WHEN** cards are in play
- **THEN** each instance SHALL track current power (modified by ongoing effects)
- **AND** SHALL track when it was played (for play order)
- **AND** SHALL track attached perks
- **AND** power SHALL update as board state changes
