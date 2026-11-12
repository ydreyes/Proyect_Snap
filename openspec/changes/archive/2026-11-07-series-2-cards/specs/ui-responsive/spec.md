# Spec Delta

## ADDED Requirements

### Requirement: Series 2 Combat Indicators

The system SHALL keep the new combat indicators legible and reachable in the portrait layout.

#### Scenario: Plaque status

- **WHEN** a location plaque shows a flooded or locked status at 720 by 1280 pixels
- **THEN** the status label SHALL remain legible
- **AND** it SHALL NOT overlap the location name, effect text or power totals
- **AND** the plaque SHALL remain tappable

#### Scenario: Moveable card

- **WHEN** a moveable card is shown in the hand or on the board at 720 by 1280 pixels
- **THEN** its move indicator SHALL remain visible
- **AND** it SHALL NOT hide the card name, cost or power
- **AND** the card SHALL remain draggable with a finger

#### Scenario: Safe area

- **WHEN** the device reports safe-area insets
- **THEN** no status label or move indicator SHALL be drawn inside the insets
- **AND** no new control SHALL be placed inside them

#### Scenario: No overlap

- **WHEN** the combat screen shows a flooded or locked location with cards and a full hand
- **THEN** no indicator SHALL overlap the hand, the end-turn control or the power totals
- **AND** every interactive control SHALL remain reachable

### Requirement: Expanded Catalog Layout

The system SHALL keep the collection and global store usable with 92 cards and nine bundles.

#### Scenario: Collection paging

- **WHEN** the collection opens with 92 cards at the smallest supported viewport
- **THEN** the four-column grid SHALL remain usable
- **AND** the page indicator and previous and next controls SHALL remain reachable
- **AND** the filter chips SHALL remain reachable

#### Scenario: Store rotation

- **WHEN** the global store opens with nine bundles
- **THEN** the three visible bundles and the twelve direct cards SHALL remain reachable through the existing tabs and paging
- **AND** no entry SHALL overlap another
- **AND** the points balance and purchase controls SHALL remain reachable

#### Scenario: Card detail

- **WHEN** a Series 2 card or token detail is opened at the smallest supported viewport
- **THEN** its name, cost, power, description and state SHALL fit inside the panel
- **AND** the movement or zone status SHALL remain legible
