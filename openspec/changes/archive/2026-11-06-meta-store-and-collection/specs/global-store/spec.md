# Spec Delta

## Purpose

Defines the global store: what it sells, how prices and bundles work, how the catalog is partitioned and how purchases persist.

## ADDED Requirements

### Requirement: Store Catalog

The system SHALL offer permanent meta purchases.

#### Scenario: Catalog entries

- **WHEN** the global store is opened
- **THEN** it SHALL offer an extra-life entry for 90 points
- **AND** it SHALL offer a perk entry for 120 points
- **AND** it SHALL offer a direct card for 150 points
- **AND** it SHALL offer six bundles
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

The system SHALL partition the 59-card catalog into owned, direct and bundle groups.

#### Scenario: Partition rules

- **WHEN** the content is loaded
- **THEN** the 13 starter cards SHALL be owned by default
- **AND** 6 cards SHALL be direct-sale cards
- **AND** the remaining 40 cards SHALL be split across six bundles
- **AND** no card SHALL belong to two groups
- **AND** every draftable card SHALL belong to exactly one group

#### Scenario: Bundle sizes

- **WHEN** the six bundles are created
- **THEN** three bundles SHALL contain 6 cards
- **AND** two bundles SHALL contain 7 cards
- **AND** one bundle SHALL contain 8 cards
- **AND** the total SHALL be 40 cards

#### Scenario: Direct cards

- **WHEN** a direct card is bought
- **THEN** that card SHALL be added to the owned set
- **AND** it SHALL be removable from the direct slot
- **AND** it SHALL NOT also appear in a bundle

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

### Requirement: Store Content Effects

The system SHALL apply the purchased content to the profile.

#### Scenario: Extra life

- **WHEN** the extra-life entry is bought
- **THEN** one extra-life charge SHALL be stored in the profile
- **AND** the next run SHALL start with that charge
- **AND** the charge SHALL be consumed on the first defeat

#### Scenario: Perk

- **WHEN** a perk entry is bought
- **THEN** that perk SHALL be added to the profile's owned perks
- **AND** the level-up perk pool SHALL be able to offer it
- **AND** the perk SHALL respect the 2-copy limit across the deck

#### Scenario: Bundle

- **WHEN** a bundle is bought
- **THEN** every card in the bundle SHALL be added to the owned set
- **AND** the bundle SHALL be marked as owned
- **AND** the purchase SHALL persist atomically with the profile
