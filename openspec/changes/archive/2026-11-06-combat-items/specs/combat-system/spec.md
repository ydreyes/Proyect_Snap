# Spec Delta

## ADDED Requirements

### Requirement: Combat Item Use

The system SHALL resolve item effects through the combat action authority.

#### Scenario: Action authority

- **WHEN** an item destroys, moves, draws, copies or changes power
- **THEN** it SHALL use the same action rules, capacity checks, protections and event dispatch as the equivalent card action
- **AND** it SHALL NOT mutate zones, hands or piles through a separate path
- **AND** it SHALL update the board version and power totals

#### Scenario: Random target

- **WHEN** an item has more than one legal target and its text does not define the set
- **THEN** the target SHALL be chosen uniformly at random
- **AND** the choice SHALL be deterministic for a seeded random service

#### Scenario: No target

- **WHEN** an item has no legal target
- **THEN** the use SHALL be rejected
- **AND** the board SHALL remain unchanged
- **AND** the item SHALL NOT be consumed

#### Scenario: Turn interaction

- **WHEN** an item adds energy
- **THEN** the added energy SHALL be available for the current turn only
- **AND** it SHALL NOT carry to the next turn
- **AND** the energy indicator SHALL update immediately
