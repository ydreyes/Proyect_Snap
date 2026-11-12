# Spec Delta

## ADDED Requirements

### Requirement: Combat Item Bar

The system SHALL show the item inventory during combat.

#### Scenario: Item bar

- **WHEN** the combat scene is in the player's play phase
- **THEN** the item bar SHALL show every item with a count above 0
- **AND** each entry SHALL show the item name or icon, its effect summary and its count
- **AND** the bar SHALL be reachable without covering the hand, the end-turn control or the board
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Disabled item

- **WHEN** an item has no legal target or the profile write is in progress
- **THEN** its use control SHALL be disabled
- **AND** the count SHALL remain visible
- **AND** the reason SHALL be understandable from the item state

#### Scenario: Use feedback

- **WHEN** an item is used
- **THEN** the board or the energy indicator SHALL update in the same frame
- **AND** the count SHALL decrease immediately
- **AND** a rejected use SHALL show a short message without changing the count
