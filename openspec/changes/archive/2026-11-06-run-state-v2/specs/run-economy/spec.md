# Spec Delta

## Purpose

Defines how a run earns experience, levels and money, how level-ups and card upgrades are resolved, and how upgraded cards expose their effective cost and power to combat.

## ADDED Requirements

### Requirement: Run Experience and Levels

The system SHALL track run experience and the current level as part of the run state.

#### Scenario: Experience cost of a level

- **WHEN** the run is at level L
- **THEN** advancing from level L to level L + 1 SHALL require `100 + 50 * (L - 1)` experience points
- **AND** experience SHALL carry over the surplus into the next level
- **AND** the run SHALL start at level 1 with 0 experience

#### Scenario: Multiple level-ups in one combat

- **WHEN** a combat reward raises the experience above more than one threshold
- **THEN** the system SHALL queue one level-up per threshold crossed
- **AND** the queued level-ups SHALL be resolved one at a time before the next combat starts
- **AND** no queued level-up SHALL be discarded

#### Scenario: Experience resets with a new run

- **WHEN** a new run starts
- **THEN** experience SHALL be 0 and the level SHALL be 1
- **AND** no progress from the previous run SHALL remain

### Requirement: Combat Experience and Money Rewards

The system SHALL grant experience and money when the player wins a combat, based on the combat type.

#### Scenario: Reward amounts

- **WHEN** the player wins a combat
- **THEN** a normal combat SHALL grant 45 experience and 9 money
- **AND** a sub-boss combat SHALL grant 150 experience and 35 money
- **AND** a final-boss combat SHALL grant 250 experience and 70 money
- **AND** a loss or a tie SHALL grant no experience and no money

#### Scenario: Rewards are granted once

- **WHEN** a combat is repeated after a defeat
- **THEN** the defeat SHALL NOT grant rewards
- **AND** the rewards SHALL be granted only when that combat is finally won
- **AND** winning the same combat twice SHALL NOT grant the reward twice

#### Scenario: Reward resolution order

- **WHEN** a combat is won
- **THEN** experience and money SHALL be applied before any level-up node opens
- **AND** the level-up nodes SHALL be resolved before the next combat is offered

### Requirement: Run Money

The system SHALL track a money balance that belongs to the current run only.

#### Scenario: Money balance

- **WHEN** the run starts
- **THEN** the money balance SHALL be 0
- **AND** it SHALL increase only through run rewards and run events
- **AND** it SHALL never become negative

#### Scenario: Unspent money dies with the run

- **WHEN** a run ends for any reason
- **THEN** the unspent money SHALL be discarded
- **AND** it SHALL NOT be converted into global points or any persistent currency
- **AND** a new run SHALL start again with 0 money

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
- **THEN** the system SHALL offer up to 3 distinct perks that already have fewer than 2 copies across the deck
- **AND** the player SHALL pick 1 perk and 1 card in the deck
- **AND** the perk SHALL be attached to that card
- **AND** no card SHALL be added to or removed from the deck
- **AND** a perk SHALL NOT exceed 2 copies across the deck

### Requirement: Card Upgrades

The system SHALL support permanent per-card upgrades that modify cost or power.

#### Scenario: Upgrade types and cap

- **WHEN** a card is upgraded
- **THEN** the upgrade SHALL be either `-1 cost` or `+2 power`
- **AND** each card SHALL hold at most 3 upgrades in total
- **AND** the upgrade count, cost delta and power delta SHALL be tracked per card instance

#### Scenario: Cost floor

- **WHEN** a card receives `-1 cost` upgrades
- **THEN** its effective cost SHALL NOT go below 0
- **AND** the `-1 cost` upgrade SHALL be unavailable when the effective cost is already 0

#### Scenario: Upgrades persist across combats

- **WHEN** a new combat starts or a card returns to the hand
- **THEN** its upgrades, cost delta and power delta SHALL remain
- **AND** only temporary combat modifiers SHALL be cleared

### Requirement: Effective Cost and Power

The system SHALL expose effective cost and effective power for every card instance and use them for play validation, energy and scoring.

#### Scenario: Effective cost

- **WHEN** the system needs a card's cost
- **THEN** the effective cost SHALL be `max(0, base cost + cost delta + temporary cost delta)`
- **AND** affordability checks, energy charges, refunds, energy previews and hand displays SHALL use the effective cost

#### Scenario: Effective power

- **WHEN** the system calculates a card's contribution before zone and Ongoing effects
- **THEN** the effective power SHALL be `base power + power delta + attached perk modifiers + persistent bonus`
- **AND** the combat power calculation SHALL start from the effective power
- **AND** temporary power modifiers SHALL be applied without changing the stored power delta

#### Scenario: Display shows the upgraded values

- **WHEN** an upgraded card is shown in the hand, in the deck or in the run summary
- **THEN** the displayed cost and power SHALL be the effective values
- **AND** the card SHALL indicate that it has upgrades
