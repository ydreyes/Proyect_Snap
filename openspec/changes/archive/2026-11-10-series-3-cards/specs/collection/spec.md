# Spec Delta

## MODIFIED Requirements

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
