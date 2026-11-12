# Spec Delta

## Purpose

Defines the 59-card collection, its five states, the filters and the perks tab.

## ADDED Requirements

### Requirement: Card Collection

The system SHALL show all 59 cards in a collection grid.

#### Scenario: Grid

- **WHEN** the collection opens
- **THEN** all 59 draftable cards SHALL be listed
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
- **THEN** all 8 perks SHALL be listed
- **AND** each perk SHALL show its name, rule text and owned copies
- **AND** an unowned perk SHALL show how to obtain it
- **AND** the tab SHALL remain usable at the smallest supported viewport

#### Scenario: Preview does not discover

- **WHEN** the collection is opened
- **THEN** viewing a card or perk SHALL NOT mark it as discovered
- **AND** only a run context SHALL discover content
