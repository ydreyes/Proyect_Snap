# Spec Delta

## MODIFIED Requirements

### Requirement: Location Plaque
The system SHALL show a compact location plaque between the rival and player grids, with different contents depending on whether the zone is revealed.

#### Scenario: Plaque contents
- **WHEN** a location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals when the zone is revealed
- **AND** it SHALL withhold the name, effect text and art when the zone is unrevealed
- **AND** the totals SHALL update as cards are played, moved or removed

#### Scenario: Revealed plaque contents
- **WHEN** a revealed location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals
- **AND** the power totals SHALL remain legible at the smallest supported window size
- **AND** the totals SHALL update as cards are played, moved or removed

#### Scenario: Unrevealed plaque contents
- **WHEN** an unrevealed location is displayed
- **THEN** the plaque SHALL withhold the name, effect text and art
- **AND** it SHALL show an unknown marker instead of the name
- **AND** it SHALL still show the location index and both power totals
- **AND** the totals SHALL update as cards are played, moved or removed

## ADDED Requirements

### Requirement: Detail Panel Layout

The system SHALL place the card and zone detail panels over the combat board using percentage-based geometry.

#### Scenario: Panel placement
- **WHEN** a detail panel opens
- **THEN** it SHALL cover the board area and dim what is behind it
- **AND** its bounds SHALL scale with the viewport
- **AND** its close control SHALL be reachable at every supported window size

#### Scenario: Panel content fits

- **WHEN** the card or zone panel content is drawn at the smallest supported window size
- **THEN** the card, its badges and its descriptive text SHALL stay inside the panel
- **AND** the descriptive text SHALL remain legible using the default font
