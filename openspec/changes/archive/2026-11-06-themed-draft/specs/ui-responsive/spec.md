# Spec Delta

## ADDED Requirements

### Requirement: Themed Draft Layout

The system SHALL present the themed draft in the existing portrait layout.

#### Scenario: Themed round

- **WHEN** the draft is in one of the first three rounds
- **THEN** the theme name SHALL be visible above the 3 offers
- **AND** the three cards SHALL be visible without overlap
- **AND** the progress counter SHALL show the pick number out of 8
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Random round

- **WHEN** the draft is in rounds 4 through 8
- **THEN** the theme label SHALL be hidden or show `ALEATORIO`
- **AND** the three cards and the progress counter SHALL remain visible
- **AND** the player SHALL be able to pick with a finger

#### Scenario: Draft completion

- **WHEN** the 8th card is picked
- **THEN** the completion state SHALL show the 8/8 progress
- **AND** the continue control SHALL be reachable
- **AND** the 8-card deck SHALL be preserved into combat

#### Scenario: Draft resume

- **WHEN** a draft checkpoint is resumed
- **THEN** the same theme, offers and progress SHALL be visible
- **AND** the scene SHALL NOT flash or reset to round 1
