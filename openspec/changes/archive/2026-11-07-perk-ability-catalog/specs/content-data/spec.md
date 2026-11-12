# Spec Delta

## MODIFIED Requirements

### Requirement: Perk Library

The system SHALL include 28 ability perks, the Zombie Horde token and no global rule perks.

#### Scenario: Initial perks

- **WHEN** perks are initialized
- **THEN** there SHALL be exactly 28 ability perks available
- **AND** all 28 SHALL be owned by default
- **AND** no perk SHALL be a store perk
- **AND** the retired rule perk ids SHALL NOT be registered
- **AND** each perk SHALL declare the effect ids and compatibility metadata for its ability

#### Scenario: Store perks

- **WHEN** the store catalog is built
- **THEN** no perk entry SHALL be present
- **AND** no perk SHALL require a purchase to enter the level-up pool

#### Scenario: Perk rule data

- **WHEN** a perk is defined
- **THEN** it SHALL have an id, name, rule text, effect ids and compatibility metadata
- **AND** its effect ids SHALL resolve to registered implementations
- **AND** an effect implementation MAY be shared with a current or future card
- **AND** a perk SHALL NOT be implemented as a one-off branch in a scene

#### Scenario: Zombie Horde token

- **WHEN** the content is loaded
- **THEN** a non-draftable `token_zombie_horde` SHALL exist in the card registry
- **AND** it SHALL have 0 cost and 0 base Power
- **AND** it SHALL NOT be counted in the 59 draftable cards
- **AND** Deadlink SHALL be able to add Power to it

#### Scenario: Validation

- **WHEN** content validation runs
- **THEN** every perk effect id SHALL resolve to a registered implementation
- **AND** every compatibility requirement SHALL name a supported host trait or a board condition
- **AND** a missing implementation or malformed compatibility SHALL fail validation before a run starts
