# Spec Delta

## ADDED Requirements

### Requirement: Shop and Event Layout

The system SHALL lay out the shop and event nodes in the existing portrait layout.

#### Scenario: Shop cards

- **WHEN** the shop opens
- **THEN** the 4 card offers SHALL be visible without overlap
- **AND** each offer SHALL show its card, price and sold state
- **AND** the upgrade, removal, extra-life and leave controls SHALL be reachable
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Shop at the deck cap

- **WHEN** a card purchase requires removing a deck card
- **THEN** the deck grid SHALL be shown with the current effective cost and power
- **AND** the selected card SHALL be highlighted
- **AND** the confirm and cancel controls SHALL be reachable

#### Scenario: Event

- **WHEN** an event is shown
- **THEN** the title and narrative text SHALL be legible
- **AND** the accept option SHALL show its cost and reward
- **AND** the decline option SHALL be visible on every event
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Modifier list

- **WHEN** the active modifiers are shown in the shop, event, act transition or pause
- **THEN** every modifier SHALL be listed by name
- **AND** the list SHALL fit without hiding the primary controls
