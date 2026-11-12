# Spec Delta

## MODIFIED Requirements

### Requirement: Level-Up Resolution

The system SHALL resolve each level-up at a safe decision point with a card branch and a perk branch.

#### Scenario: Choosing a branch

- **WHEN** a level-up is resolved
- **THEN** the player SHALL choose exactly one of the card branch or the perk branch
- **AND** a branch with no legal option SHALL be shown as unavailable
- **AND** if neither branch has a legal option, the level-up SHALL resolve without changing the run

#### Scenario: Card branch below the deck cap

- **WHEN** the player chooses the card branch and the deck has fewer than 12 cards
- **THEN** the system SHALL offer 3 distinct cards eligible for the run deck
- **AND** the player SHALL pick 1 card to add to the deck
- **AND** the chosen card SHALL become a card instance in the deck

#### Scenario: Card branch at the deck cap

- **WHEN** the player chooses the card branch and the deck has 12 cards
- **THEN** the system SHALL offer the upgrade choice instead of adding a card
- **AND** the player SHALL choose a deck card with fewer than 3 upgrades and then choose one upgrade type
- **AND** the deck size SHALL remain 12

#### Scenario: Perk branch

- **WHEN** the player chooses the perk branch
- **THEN** the system SHALL offer up to 3 distinct perks from the full 28-perk catalog that have fewer than 2 copies across the deck and at least one compatible host
- **AND** the player SHALL pick 1 perk and 1 compatible card in the deck
- **AND** the perk SHALL be attached to that card
- **AND** no card SHALL be added to or removed from the deck
- **AND** a perk SHALL NOT exceed 2 copies across the deck

#### Scenario: No compatible perk

- **WHEN** no catalog perk has a compatible host in the deck with fewer than 2 copies
- **THEN** the perk branch SHALL be shown as unavailable
- **AND** the player SHALL only see the card branch

### Requirement: Effective Cost and Power

The system SHALL expose effective cost and effective power for every card instance and use them for play validation, energy and scoring.

#### Scenario: Effective cost

- **WHEN** the system needs a card's cost
- **THEN** the effective cost SHALL be `max(0, base cost + cost delta + temporary cost delta)`
- **AND** affordability checks, energy charges, refunds, energy previews and hand displays SHALL use the effective cost
- **AND** a perk that changes the cost SHALL be included in the effective cost

#### Scenario: Effective power

- **WHEN** the system calculates a card's contribution before zone and Ongoing effects
- **THEN** the effective power SHALL include the base power, power delta, persistent bonus and any power granted by attached perk abilities
- **AND** the combat power calculation SHALL start from the effective power
- **AND** temporary power modifiers SHALL be applied without changing the stored power delta

#### Scenario: Display shows the upgraded values

- **WHEN** an upgraded card is shown in the hand, in the deck or in the run summary
- **THEN** the displayed cost and power SHALL be the effective values
- **AND** the card SHALL indicate that it has upgrades
