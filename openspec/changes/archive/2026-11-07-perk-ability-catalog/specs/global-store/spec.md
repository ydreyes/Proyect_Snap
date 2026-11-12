# Spec Delta

## MODIFIED Requirements

### Requirement: Store Catalog

The system SHALL offer permanent meta purchases.

#### Scenario: Catalog entries

- **WHEN** the global store is opened
- **THEN** it SHALL offer an extra-life entry for 90 points
- **AND** it SHALL NOT offer a perk entry
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
