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

### Requirement: Level-Up and Upgrade Presentation

The system SHALL present level-up and upgrade selection with one ceremony template that adapts to the current choice phase.

#### Scenario: Ceremony

- **WHEN** a level-up starts
- **THEN** the screen SHALL show an `UPGRADE UNLOCKED` treatment with the current level

#### Scenario: Branch choice

- **WHEN** the player must choose between a card and a perk
- **THEN** the two options SHALL be shown as large tiles
- **AND** the perk tile SHALL be disabled when no compatible host exists in the deck

#### Scenario: Card offer

- **WHEN** card offers are shown
- **THEN** each card SHALL appear as a medallion with its art, name, cost and power

#### Scenario: Upgrade type

- **WHEN** an upgrade type is chosen
- **THEN** `-1 COSTE` and `+2 PODER` SHALL be shown as large tiles with their effect

#### Scenario: Perk offer

- **WHEN** perk offers are shown
- **THEN** each perk SHALL show its name, its ability text and its compatibility requirement
- **AND** the offer SHALL include only perks with at least one compatible host in the deck

#### Scenario: Target choice

- **WHEN** a card or perk requires a deck target
- **THEN** the deck SHALL be shown as a selectable carousel with current cost and power
- **AND** only compatible hosts SHALL be selectable for a perk
- **AND** an incompatible card SHALL be shown disabled with the reason it cannot host the perk

#### Scenario: Unavailable

- **WHEN** no upgrade is available
- **THEN** the screen SHALL show an unavailable state
- **AND** the exit control SHALL be reachable

#### Scenario: No compatible perk

- **WHEN** no perk in the catalog has a compatible host in the deck
- **THEN** the perk branch SHALL show an unavailable state
- **AND** the card branch SHALL remain reachable
