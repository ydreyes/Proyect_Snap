# Spec Delta

## MODIFIED Requirements

### Requirement: Card Structure
The system SHALL represent cards with cost, power, effects, and a player-facing description.

#### Scenario: Card properties
- **WHEN** a card is defined or instantiated
- **THEN** it SHALL have an ID, name, cost (energy), and base power
- **AND** it SHALL have a descriptive text intended for the player
- **AND** it MAY have associated effects by trigger type
- **AND** it MAY have tags

#### Scenario: Description is shown during inspection
- **WHEN** the player inspects a card
- **THEN** the descriptive text SHALL be available to the detail panel
- **AND** the text SHALL describe what the card is meant to do
