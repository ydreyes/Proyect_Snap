# Spec Delta

## MODIFIED Requirements

### Requirement: Perk Library

The system SHALL include 8 rule perks.

#### Scenario: Initial perks

- **WHEN** perks are initialized
- **THEN** there SHALL be 8 perks available
- **AND** Vanguardia, Aura, Francotirador and Letal SHALL be owned by default
- **AND** Agil, Guardian, Imparable and Codicioso SHALL be store perks
- **AND** none of them SHALL be a flat power modifier

#### Scenario: Store perks

- **WHEN** the store sells a perk
- **THEN** that perk SHALL become owned in the profile
- **AND** the level-up perk pool SHALL be able to offer it
- **AND** it SHALL be listed in the collection's perks tab

#### Scenario: Perk rule data

- **WHEN** a perk is defined
- **THEN** it SHALL have an id, name, rule text and effect ids
- **AND** the effect ids SHALL resolve to the perk rule hooks
- **AND** a perk SHALL NOT be implemented as a one-off branch in a scene
