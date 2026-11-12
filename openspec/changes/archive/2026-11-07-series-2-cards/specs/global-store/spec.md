# Spec Delta

## MODIFIED Requirements

### Requirement: Store Catalog

The system SHALL offer permanent meta purchases.

#### Scenario: Catalog entries

- **WHEN** the global store is opened
- **THEN** it SHALL offer an extra-life entry for 90 points
- **AND** it SHALL NOT offer a perk entry
- **AND** it SHALL offer twelve direct cards for 150 points each
- **AND** it SHALL offer nine bundles across the visible rotation
- **AND** it SHALL reserve an item entry at 60 points for the combat-items change
- **AND** the item entry SHALL be hidden or disabled until item definitions exist

#### Scenario: Bundle prices

- **WHEN** a bundle is offered
- **THEN** a 6-card bundle SHALL cost 450 points
- **AND** a 7-card bundle SHALL cost 650 points
- **AND** an 8-card bundle SHALL cost 850 points

#### Scenario: Purchase rules

- **WHEN** the player buys a store entry
- **THEN** the points SHALL be deducted
- **AND** the content SHALL be added to the profile before the screen closes
- **AND** an owned card or bundle SHALL NOT be sold twice
- **AND** an unaffordable entry SHALL be disabled and SHALL NOT change the balance

### Requirement: Catalog Partition

The system SHALL partition the 92-card catalog into owned, direct and bundle groups.

#### Scenario: Partition rules

- **WHEN** the content is loaded
- **THEN** the 21 starter cards SHALL be owned by default
- **AND** 12 cards SHALL be direct-sale cards
- **AND** the remaining 59 cards SHALL be split across nine bundles
- **AND** no card SHALL belong to two groups
- **AND** every draftable card SHALL belong to exactly one group

#### Scenario: Bundle sizes

- **WHEN** the nine bundles are created
- **THEN** five bundles SHALL contain 6 cards
- **AND** three bundles SHALL contain 7 cards
- **AND** one bundle SHALL contain 8 cards
- **AND** the total SHALL be 59 cards

#### Scenario: Direct cards

- **WHEN** a direct card is bought
- **THEN** that card SHALL be added to the owned set
- **AND** it SHALL be removable from the direct slot
- **AND** it SHALL NOT also appear in a bundle

#### Scenario: Series 1 bundle stability

- **WHEN** the catalog expands from 59 to 92 cards
- **THEN** the six legacy Series 1 bundles SHALL keep the same card membership
- **AND** the six legacy direct cards SHALL keep the same card ids
- **AND** a profile that already owns a legacy bundle SHALL keep the cards it bought

## ADDED Requirements

### Requirement: Series 2 Batch

The system SHALL add the 25 Series 2 cards as a separate store batch without repartitioning the Series 1 content.

#### Scenario: Series 2 partition

- **WHEN** the Series 2 batch is built
- **THEN** 6 Series 2 cards SHALL be direct-sale cards
- **AND** the remaining 19 Series 2 cards SHALL be split into three bundles of 6, 6 and 7 cards
- **AND** every Series 2 card SHALL belong to exactly one group
- **AND** no Series 2 card SHALL be a starter

#### Scenario: Stable identifiers

- **WHEN** the new bundles are created
- **THEN** their ids SHALL use a `bundle_s2_` prefix
- **AND** the legacy ids such as `bundle_6_a`, `bundle_7_a` and `bundle_8` SHALL remain valid
- **AND** no new bundle SHALL reuse a legacy id

#### Scenario: Store visibility

- **WHEN** the global store opens
- **THEN** the three visible bundles SHALL be drawn from the nine-bundle rotation
- **AND** unowned Series 2 cards SHALL appear as direct entries or inside a visible bundle
- **AND** purchased Series 2 content SHALL be added to the profile atomically

#### Scenario: Ownership

- **WHEN** a Series 2 bundle or direct card is purchased
- **THEN** every card in that entry SHALL enter the owned set
- **AND** the entry SHALL be marked as owned
- **AND** the entry SHALL NOT be sold again
