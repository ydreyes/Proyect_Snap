# Spec Delta

## MODIFIED Requirements

### Requirement: Card Pool
The system SHALL include a starter card pool of at least 20 card definitions for drafting, each with a player-facing description.

#### Scenario: Card pool initialization
- **WHEN** the game starts
- **THEN** a card pool of at least 20 cards SHALL be available for drafting
- **AND** cards SHALL cover varied costs from 0 to 6 and varied powers
- **AND** new cards SHALL be easily addable to the pool

#### Scenario: Every card has a description
- **WHEN** the card pool is loaded
- **THEN** every card definition SHALL have a non-empty descriptive text
- **AND** the text SHALL be written in Spanish
- **AND** the text SHALL be readable in the card detail panel
