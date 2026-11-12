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
- **THEN** the system SHALL offer up to 3 distinct owned perks that already have fewer than 2 copies across the deck
- **AND** the player SHALL pick 1 perk and 1 card in the deck
- **AND** the perk SHALL be attached to that card
- **AND** no card SHALL be added to or removed from the deck
- **AND** a perk SHALL NOT exceed 2 copies across the deck
