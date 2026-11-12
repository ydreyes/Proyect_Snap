# global-store Specification

## Purpose

Defines the global store: what it sells, how prices and bundles work, how the catalog is partitioned and how purchases persist.

## Requirements

### Requirement: Store Catalog

The system SHALL offer permanent meta purchases.

#### Scenario: Catalog entries

- **WHEN** the global store is opened
- **THEN** it SHALL offer an extra-life entry for 90 points
- **AND** it SHALL NOT offer a perk entry
- **AND** it SHALL offer eighteen direct cards for 150 points each
- **AND** it SHALL offer twenty-six bundles across the rotation
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

The system SHALL partition the 217-card catalog into owned, direct and bundle groups.

#### Scenario: Partition rules

- **WHEN** the content is loaded
- **THEN** the 21 starter cards SHALL be owned by default
- **AND** 18 cards SHALL be direct-sale cards
- **AND** the remaining 178 cards SHALL be split across twenty-six bundles
- **AND** no card SHALL belong to two groups
- **AND** every draftable card SHALL belong to exactly one group

#### Scenario: Bundle sizes

- **WHEN** the twenty-six bundles are created
- **THEN** five bundles SHALL contain 6 cards
- **AND** twenty bundles SHALL contain 7 cards
- **AND** one bundle SHALL contain 8 cards
- **AND** the total SHALL be 178 cards

#### Scenario: Direct cards

- **WHEN** a direct card is bought
- **THEN** that card SHALL be added to the owned set
- **AND** it SHALL be removable from the direct slot
- **AND** it SHALL NOT also appear in a bundle

#### Scenario: Series 1 bundle stability

- **WHEN** the catalog expands from 92 to 217 cards
- **THEN** the six legacy Series 1 bundles SHALL keep the same card membership
- **AND** the six legacy direct cards SHALL keep the same card ids
- **AND** a profile that already owns a legacy bundle SHALL keep the cards it bought

#### Scenario: Series 2 stability

- **WHEN** the catalog expands from 92 to 217 cards
- **THEN** the three Series 2 bundles SHALL keep the same card membership
- **AND** the six Series 2 direct cards SHALL keep the same card ids
- **AND** a profile that already owns a Series 2 bundle SHALL keep the cards it bought

### Requirement: Bundle Rotation

The system SHALL show three bundles at a time.

#### Scenario: Visible bundles

- **WHEN** the store opens
- **THEN** exactly three unowned bundles SHALL be visible
- **AND** the visible set SHALL come from the profile's rotation index
- **AND** a purchased bundle SHALL be replaced by the next unowned bundle
- **AND** when fewer than three unowned bundles remain, the remaining slots SHALL show as sold or empty

#### Scenario: Rotation advance

- **WHEN** a new run starts
- **THEN** the rotation index SHALL advance by one
- **AND** the visible set SHALL be recomputed
- **AND** owned bundles SHALL NOT become visible again

#### Scenario: Large rotation

- **WHEN** the store has twenty-six bundles
- **THEN** the rotation SHALL cover every unowned bundle over time
- **AND** the three visible bundles SHALL remain reachable through the existing tab
- **AND** the direct-card paging SHALL remain usable with eighteen cards

### Requirement: Store Content Effects

The system SHALL apply the purchased content to the profile.

#### Scenario: Perk

- **WHEN** the store catalog is evaluated
- **THEN** no perk entry SHALL be offered or purchasable
- **AND** perks SHALL be obtained only from the level-up and run sources

#### Scenario: Extra life

- **WHEN** the extra-life entry is bought
- **THEN** one extra-life charge SHALL be stored in the profile
- **AND** the next run SHALL start with that charge
- **AND** the charge SHALL be consumed on the first defeat

#### Scenario: Bundle

- **WHEN** a bundle is bought
- **THEN** every card in the bundle SHALL be added to the owned set
- **AND** the bundle SHALL be marked as owned
- **AND** the purchase SHALL persist atomically with the profile

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

### Requirement: Series 3 Batch

The system SHALL add the 125 Series 3 cards as a separate store batch without repartitioning the Series 1 or Series 2 content.

#### Scenario: Series 3 partition

- **WHEN** the Series 3 batch is built
- **THEN** 6 Series 3 cards SHALL be direct-sale cards
- **AND** the remaining 119 Series 3 cards SHALL be split into seventeen bundles of 7 cards
- **AND** every Series 3 card SHALL belong to exactly one group
- **AND** no Series 3 card SHALL be a starter

#### Scenario: Stable identifiers

- **WHEN** the new bundles are created
- **THEN** their ids SHALL use a `bundle_s3_` prefix
- **AND** the legacy ids such as `bundle_6_a`, `bundle_7_a`, `bundle_8` and `bundle_s2_6_a` SHALL remain valid
- **AND** no new bundle SHALL reuse a legacy or Series 2 id

#### Scenario: Store visibility

- **WHEN** the global store opens
- **THEN** the three visible bundles SHALL be drawn from the twenty-six-bundle rotation
- **AND** unowned Series 3 cards SHALL appear as direct entries or inside a visible bundle
- **AND** purchased Series 3 content SHALL be added to the profile atomically

#### Scenario: Ownership

- **WHEN** a Series 3 bundle or direct card is purchased
- **THEN** every card in that entry SHALL enter the owned set
- **AND** the entry SHALL be marked as owned
- **AND** the entry SHALL NOT be sold again

#### Scenario: Counts

- **WHEN** the catalog partition is validated
- **THEN** the draftable count SHALL be 217
- **AND** the direct count SHALL be 18
- **AND** the bundle count SHALL be 26
- **AND** the bundle card total SHALL be 178
- **AND** the starter count SHALL remain 21
- **AND** 21 + 18 + 178 SHALL equal 217
