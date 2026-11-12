# Spec Delta

## ADDED Requirements

### Requirement: Enemy Definition Library

The system SHALL include the 23 enemy definitions used by the run.

#### Scenario: Enemy content

- **WHEN** the content library is loaded
- **THEN** the 18 normal enemies, 4 sub-bosses and 1 final boss SHALL be registered
- **AND** each definition SHALL declare its name, flavor phrase, difficulty, act pool and 8-card deck
- **AND** every deck card id SHALL resolve to a registered draftable card
- **AND** the content SHALL be data-driven and separate from combat logic

#### Scenario: Act pools

- **WHEN** the enemy pools are read
- **THEN** act 1, act 2 and act 3 SHALL each contain 6 normal enemies
- **AND** the sub-boss pool SHALL contain 4 bosses
- **AND** the final-boss pool SHALL contain 1 boss

#### Scenario: No playable-card collision

- **WHEN** an enemy name is compared with a card name
- **THEN** an enemy SHALL NOT reuse a playable card name
- **AND** enemy ids SHALL use an `enemy_` prefix
