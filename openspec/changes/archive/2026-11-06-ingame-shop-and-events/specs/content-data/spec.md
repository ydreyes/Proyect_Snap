# Spec Delta

## ADDED Requirements

### Requirement: Run Event and Modifier Library

The system SHALL include the 12 run events and the 6 run modifiers as data-driven content.

#### Scenario: Event content

- **WHEN** the content library is loaded
- **THEN** 12 event definitions SHALL be registered
- **AND** each event SHALL have a title, narrative text, accept effect and decline option
- **AND** the accept effect SHALL use only XP, money, card, upgrade, perk or modifier grants
- **AND** no event SHALL start a combat

#### Scenario: Modifier content

- **WHEN** the content library is loaded
- **THEN** the 6 modifier definitions SHALL be registered
- **AND** each modifier SHALL have an id, a display name and its parameter values
- **AND** the catalog SHALL match the `run-modifiers` specification

#### Scenario: Content is separate from logic

- **WHEN** a new event or modifier is added
- **THEN** it SHALL be definable as data
- **AND** it SHALL NOT require a change to the event or modifier engine
