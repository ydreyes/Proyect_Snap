# Spec Delta

## MODIFIED Requirements

### Requirement: Series 2 Combat Indicators

The system SHALL keep the new combat indicators legible and reachable in the portrait layout.

#### Scenario: Plaque status

- **WHEN** a location plaque shows a flooded, locked or Series 3 status at 720 by 1280 pixels
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

The system SHALL keep the collection and global store usable with 217 cards and twenty-six bundles.

#### Scenario: Collection paging

- **WHEN** the collection opens with 217 cards at the smallest supported viewport
- **THEN** the four-column grid SHALL remain usable
- **AND** the page indicator and previous and next controls SHALL remain reachable
- **AND** the filter chips SHALL remain reachable

#### Scenario: Store rotation

- **WHEN** the global store opens with twenty-six bundles
- **THEN** the three visible bundles and the eighteen direct cards SHALL remain reachable through the existing tabs and paging
- **AND** no entry SHALL overlap another
- **AND** the points balance and purchase controls SHALL remain reachable

#### Scenario: Card detail

- **WHEN** a Series 3 card or token detail is opened at the smallest supported viewport
- **THEN** its name, cost, power, description and state SHALL fit inside the panel
- **AND** the movement or zone status SHALL remain legible

## ADDED Requirements

### Requirement: Series 3 Combat Layout

The system SHALL keep the new Series 3 controls and states reachable at the smallest supported viewport.

#### Scenario: Activate control

- **WHEN** an in-play owned card with an unused Activate effect is opened at 720 by 1280 pixels
- **THEN** the `ACTIVAR` control SHALL remain reachable
- **AND** its hit area SHALL be at least 44 by 44 logical pixels
- **AND** it SHALL NOT overlap the description or the close control
- **AND** the detail panel SHALL remain closable

#### Scenario: End-of-game banner

- **WHEN** the end-of-game resolution starts at 720 by 1280 pixels
- **THEN** the `FIN DE LA PARTIDA` banner SHALL remain legible
- **AND** the end-turn control SHALL be disabled and SHALL NOT be tappable
- **AND** the banner SHALL NOT cover the zone power totals

#### Scenario: Delayed card

- **WHEN** a delayed card is shown at 720 by 1280 pixels
- **THEN** its face-down `???` treatment SHALL remain legible
- **AND** the card SHALL NOT reveal its identity through the detail panel
- **AND** the slot SHALL remain tappable for legal interactions

#### Scenario: Limbo turn display

- **WHEN** Limbo is active and the turn display shows `TURNO n/7`
- **THEN** the label SHALL remain legible
- **AND** it SHALL NOT overlap the energy badge or the end-turn control
- **AND** the display SHALL return to `/6` when Limbo leaves play

#### Scenario: New status labels

- **WHEN** a location shows `LIMBO`, `SILENCIADA` or a Goose/Jean Grey restriction at 720 by 1280 pixels
- **THEN** the label SHALL remain legible
- **AND** it SHALL NOT overlap the name, effect text or power totals
- **AND** the plaque SHALL remain tappable
