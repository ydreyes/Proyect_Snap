# Spec Delta

## MODIFIED Requirements

### Requirement: Behavior Preservation

The redesign SHALL preserve the existing screen behavior, data and navigation.

#### Scenario: Draft rules

- **WHEN** the draft runs
- **THEN** it SHALL still make eight picks of one of three cards from the owned pool with the one-copy limit

#### Scenario: Collection contents

- **WHEN** the collection opens
- **THEN** it SHALL still contain the 92 draftable cards and the perks tab

#### Scenario: Currencies

- **WHEN** a store is shown
- **THEN** it SHALL use only the existing currency for that store
- **AND** points SHALL be used by the global store and run money by the in-run shop

#### Scenario: Extra life

- **WHEN** a combat is lost and an extra-life charge is available
- **THEN** the charge SHALL still be consumed automatically by the existing run flow

#### Scenario: Meta progression

- **WHEN** points are awarded
- **THEN** the existing points-only meta progression SHALL be unchanged

#### Scenario: Persistence

- **WHEN** the game saves or resumes
- **THEN** the save format and checkpoint behavior SHALL be unchanged

#### Scenario: No new systems

- **WHEN** the redesign is applied
- **THEN** no new currency, timer, season, saved deck, notification or in-app purchase SHALL be introduced

### Requirement: Performance and Resource Budget

The system SHALL keep the visual layer within a device-friendly budget.

#### Scenario: Cached textures

- **WHEN** a gradient, glow, noise, hex tile or card art is needed
- **THEN** it SHALL be generated once and reused

#### Scenario: Lazy generation

- **WHEN** the game starts
- **THEN** card art SHALL be generated on first use rather than for all cards at startup

#### Scenario: Bounded memory

- **WHEN** all 92 cards have been displayed
- **THEN** the card art cache SHALL stay within the documented memory budget

#### Scenario: Frame rate

- **WHEN** the seven screens run on the smallest supported device
- **THEN** the target frame rate SHALL be maintained

#### Scenario: No per-frame allocation

- **WHEN** a screen is drawn
- **THEN** the visual layer SHALL NOT allocate textures or large buffers per frame

## ADDED Requirements

### Requirement: Series 2 Indicators

The system SHALL present the new Series 2 states with the shared visual language.

#### Scenario: Moveable card

- **WHEN** a card can be moved by the player
- **THEN** its card view SHALL show a short moveable indicator
- **AND** the indicator SHALL use the shared semantic state colors
- **AND** it SHALL NOT cover the card name, cost or power

#### Scenario: Location status

- **WHEN** a location is flooded or locked
- **THEN** its plaque SHALL show a status label using the shared typography and chrome
- **AND** the label SHALL remain legible at the smallest supported viewport
- **AND** it SHALL NOT overlap the location name, effect text or power totals

#### Scenario: Token and switched card

- **WHEN** a token or a switched-side card is displayed
- **THEN** it SHALL use the same card frame and art layers as any other card
- **AND** its controller side SHALL be visually clear from its zone placement
- **AND** no separate token or switched-card scene SHALL be added

#### Scenario: Status update

- **WHEN** a movement permission is used, a location floods or a lock starts
- **THEN** the indicator SHALL update in the same frame as the board change
- **AND** no screen reload or scene transition SHALL be required
