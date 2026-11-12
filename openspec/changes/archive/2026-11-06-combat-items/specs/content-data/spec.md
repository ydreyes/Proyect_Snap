# Spec Delta

## ADDED Requirements

### Requirement: Combat Item Library

The system SHALL include the 8 combat items as data-driven content.

#### Scenario: Item content

- **WHEN** the content library is loaded
- **THEN** the 8 item definitions SHALL be registered
- **AND** each item SHALL declare its id, name, description, rarity, effect id and target rule
- **AND** every effect id SHALL resolve to a registered implementation
- **AND** items SHALL NOT be registered as cards or tokens

#### Scenario: Rarity

- **WHEN** items are defined
- **THEN** each item SHALL declare a rarity
- **AND** the rarity SHALL be used by the store or future reward tables
- **AND** rarity SHALL NOT change the item effect
