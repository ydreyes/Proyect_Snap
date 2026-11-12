# collection Specification

## Purpose

Defines the draftable-card collection, its five states, the filters and the perks tab.

## Requirements

### Requirement: Card Collection

The system SHALL show all 217 cards in a collection grid.

#### Scenario: Grid

- **WHEN** the collection opens
- **THEN** all 217 draftable cards SHALL be listed
- **AND** the grid SHALL use four columns and as many rows as needed
- **AND** each entry SHALL show the card name, cost and power
- **AND** the grid SHALL remain usable at the smallest supported viewport

#### Scenario: Five states

- **WHEN** a card is displayed
- **THEN** it SHALL be in exactly one base state: owned, discovered, in store or hidden
- **AND** an owned card that is in the current run deck SHALL show the `en run deck` overlay as the fifth state
- **AND** the visual treatment SHALL distinguish the states

#### Scenario: Owned

- **WHEN** a card is owned
- **THEN** it SHALL show its full color and name
- **AND** it SHALL show the `EN COLECCION` tag
- **AND** it SHALL show whether it is in the current run deck

#### Scenario: Discovered

- **WHEN** a card is discovered but not owned
- **THEN** it SHALL show its full color and name
- **AND** it SHALL show the `DESCUBIERTA` tag
- **AND** it SHALL show how it can be obtained

#### Scenario: In store

- **WHEN** a card is not owned but belongs to a currently visible store entry
- **THEN** it SHALL show a grey treatment and the `EN TIENDA` tag
- **AND** it SHALL be selectable to open its detail

#### Scenario: Hidden

- **WHEN** a card is neither owned, discovered nor in the store
- **THEN** it SHALL show a silhouette
- **AND** it SHALL show `DESCUBRELA EN LA PARTIDA`
- **AND** it SHALL NOT reveal its name, cost or power

#### Scenario: Series 2 cards

- **WHEN** the collection opens
- **THEN** the 25 Series 2 cards SHALL appear in the grid with their own state
- **AND** unowned Series 2 cards SHALL show as in store or hidden
- **AND** the grid paging SHALL remain usable with 217 cards

#### Scenario: Series 3 cards

- **WHEN** the collection opens
- **THEN** the 125 Series 3 cards SHALL appear in the grid with their own state
- **AND** unowned Series 3 cards SHALL show as in store or hidden
- **AND** the grid paging SHALL remain usable with 217 cards
- **AND** the detail panel SHALL show the corrected Series 3 cost and power

### Requirement: Collection Filters

The system SHALL filter the collection.

#### Scenario: State filter

- **WHEN** the player chooses a state filter
- **THEN** the grid SHALL show only the matching cards
- **AND** the options SHALL include all, owned, discovered, in store and hidden

#### Scenario: Theme and cost filters

- **WHEN** the player chooses a theme or cost filter
- **THEN** the grid SHALL show only matching cards
- **AND** the filters SHALL combine with the state filter
- **AND** clearing the filters SHALL restore the full grid

### Requirement: Perks Tab

The system SHALL show the perk collection.

#### Scenario: Perk list

- **WHEN** the perks tab is opened
- **THEN** all 28 ability perks SHALL be listed
- **AND** each perk SHALL show its name, ability text and owned copies
- **AND** each perk SHALL show its compatibility requirement when it has one
- **AND** every perk SHALL be shown as owned
- **AND** the tab SHALL remain usable at the smallest supported viewport

#### Scenario: Perk detail

- **WHEN** the player opens a perk
- **THEN** the detail SHALL show its full rule text, its trigger, its compatibility and the number of copies attached across the deck
- **AND** it SHALL show whether the current run has a compatible host

#### Scenario: Preview does not discover

- **WHEN** the collection is opened
- **THEN** viewing a card or perk SHALL NOT mark it as discovered
- **AND** only a run context SHALL discover content
