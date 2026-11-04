# Spec Delta

## MODIFIED Requirements

### Requirement: Card Pool

The system SHALL maintain a pool of at least 20 draftable card definitions available for drafting.

#### Scenario: Card selection from pool

- **WHEN** presenting draft choices
- **THEN** cards SHALL be selected from the available draftable card pool
- **AND** the pool SHALL contain at least 20 cards with varied costs and powers
- **AND** the pool SHALL support adding new cards

#### Scenario: Token definitions are excluded

- **WHEN** presenting draft choices
- **THEN** token definitions SHALL NOT be offered
- **AND** a card created by an effect during combat SHALL NOT enter the draft pool
- **AND** the draft SHALL continue to offer only definitions declared as draftable
