# Spec Delta

## MODIFIED Requirements

### Requirement: Card Pool

The system SHALL draft from the owned card pool.

#### Scenario: Card selection from pool

- **WHEN** presenting draft choices
- **THEN** cards SHALL be selected from the cards the profile owns
- **AND** the pool SHALL contain the 21 starter cards plus every card unlocked in the global store
- **AND** the pool SHALL support adding new cards through ownership

#### Scenario: Token definitions are excluded

- **WHEN** presenting draft choices
- **THEN** token definitions SHALL NOT be offered
- **AND** a card created by an effect during combat SHALL NOT enter the draft pool
- **AND** the draft SHALL continue to offer only owned, draftable definitions
