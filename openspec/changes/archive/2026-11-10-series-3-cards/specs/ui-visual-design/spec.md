# Spec Delta

## MODIFIED Requirements

### Requirement: Collection Presentation

The system SHALL present the collection as a four-column art-driven grid with filter chips, paging and a detail overlay.

#### Scenario: Grid

- **WHEN** the collection opens
- **THEN** each visible card SHALL show its procedural or provided art, name, cost and power

#### Scenario: Card states

- **WHEN** a card is undiscovered, discovered, owned or upgraded
- **THEN** its tile SHALL show the corresponding state treatment

#### Scenario: Filters

- **WHEN** the player changes the state, theme or cost filter
- **THEN** the grid SHALL update
- **AND** the filter chips SHALL show the active selection

#### Scenario: Detail

- **WHEN** the player opens a card
- **THEN** the detail overlay SHALL show the large card, its rules, its state and its obtain path

#### Scenario: Perks

- **WHEN** the player opens the perks tab
- **THEN** all 28 ability perks SHALL be shown with the same tile and detail treatment
- **AND** each tile SHALL show the perk name, its ability text and its compatibility requirement when it has one
- **AND** every perk SHALL be shown as owned
- **AND** the detail SHALL show the trigger, the compatibility and the copies attached across the current deck

#### Scenario: Paging

- **WHEN** the grid has more cards than one page
- **THEN** the page indicator and the previous and next controls SHALL be reachable
- **AND** the grid SHALL remain usable with 217 cards

### Requirement: Responsive and Touch Constraints

The system SHALL keep the redesign inside the existing portrait and safe-area constraints.

#### Scenario: Smallest viewport

- **WHEN** the drawable area is 720 by 1280 pixels
- **THEN** every label SHALL remain legible
- **AND** every control SHALL remain reachable
- **AND** the Activate control and the end-of-game banner SHALL fit

#### Scenario: Safe area

- **WHEN** the device reports safe-area insets
- **THEN** no chrome, text or control SHALL be drawn or hit-tested inside them

#### Scenario: Touch target

- **WHEN** a control is interactive
- **THEN** its hit area SHALL be at least 44 by 44 logical pixels
- **AND** it SHALL NOT require hover to activate

#### Scenario: No overlap

- **WHEN** a screen is laid out
- **THEN** no region SHALL overlap another region

### Requirement: Performance and Resource Budget

The system SHALL keep the visual layer within a device-friendly budget.

#### Scenario: Cached textures

- **WHEN** a gradient, glow, noise, hex tile or card art is needed
- **THEN** it SHALL be generated once and reused

#### Scenario: Lazy generation

- **WHEN** the game starts
- **THEN** card art SHALL be generated on first use rather than for all cards at startup

#### Scenario: Bounded memory

- **WHEN** all 217 cards have been displayed
- **THEN** the card art cache SHALL stay within the documented memory budget

#### Scenario: Frame rate

- **WHEN** the seven screens run on the smallest supported device
- **THEN** the target frame rate SHALL be maintained

#### Scenario: No per-frame allocation

- **WHEN** a screen is drawn
- **THEN** the visual layer SHALL NOT allocate textures or large buffers per frame

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

## ADDED Requirements

### Requirement: Series 3 Presentation

The system SHALL present the new Series 3 states with the shared visual language.

#### Scenario: Activate control

- **WHEN** an owned in-play card has an unused `Activate` effect
- **THEN** its detail panel SHALL show an `ACTIVAR` control
- **AND** the control SHALL use the shared button chrome and semantic accent
- **AND** it SHALL NOT overlap the close control or the description
- **AND** it SHALL be hidden when the card cannot activate

#### Scenario: Delayed reveal

- **WHEN** a card is delayed by Invisible Woman
- **THEN** its card view SHALL render face-down with `???`
- **AND** an enemy delayed card's identity SHALL remain hidden in the detail panel
- **AND** the card SHALL still show its side and slot

#### Scenario: End-of-game state

- **WHEN** the match enters the end-of-game resolution
- **THEN** the combat screen SHALL show a `FIN DE LA PARTIDA` banner
- **AND** the end-turn control SHALL be disabled
- **AND** the result modal SHALL appear only after the resolution completes

#### Scenario: Zone status

- **WHEN** a location is Limbo, suppressed by Hawk or restricted by Goose or Jean Grey
- **THEN** its plaque SHALL show a short status label
- **AND** the label SHALL use the shared typography and chrome
- **AND** it SHALL NOT overlap the name, effect text or power totals

#### Scenario: Copied text

- **WHEN** a card has copied or replaced text
- **THEN** its detail panel SHALL show a `COPIA: <name>` line or the replacement text
- **AND** the line SHALL remain legible at the smallest supported viewport
- **AND** it SHALL NOT cover the card name, cost or power

#### Scenario: Catalog counts

- **WHEN** the collection or store shows its count
- **THEN** it SHALL show 217 cards, 18 direct cards and 26 bundles
- **AND** the counts SHALL remain legible and paginated
