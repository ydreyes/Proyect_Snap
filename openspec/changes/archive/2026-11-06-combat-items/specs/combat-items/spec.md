# Spec Delta

## Purpose

Defines the eight combat items, their profile inventory, their acquisition and the free-action use that resolves through the combat authority.

## ADDED Requirements

### Requirement: Item Catalog

The system SHALL provide eight combat items.

#### Scenario: Item list

- **WHEN** item content is loaded
- **THEN** the catalog SHALL contain exactly 8 items
- **AND** each item SHALL have an id, name, description, rarity and effect id
- **AND** every effect id SHALL resolve to a registered item effect
- **AND** no item SHALL be a card, token or draftable definition

#### Scenario: Item effects

- **WHEN** an item is used
- **THEN** `energy_surge` SHALL add 2 energy to the current turn
- **AND** `power_boost` SHALL add 2 power to a random allied card in play
- **AND** `destroy_enemy` SHALL destroy a random enemy card
- **AND** `draw_two` SHALL draw up to 2 cards
- **AND** `move_enemy` SHALL move a random enemy card to another zone
- **AND** `afflict_zone` SHALL give -2 power to all enemy cards in a random zone
- **AND** `copy_hand` SHALL copy a random card in hand
- **AND** `heal_zone` SHALL give +3 power to all allied cards in a random zone

### Requirement: Profile Inventory

The system SHALL store item counts in the profile.

#### Scenario: Inventory

- **WHEN** the profile is loaded
- **THEN** it SHALL expose a count per item id
- **AND** an item with count 0 SHALL be shown as unavailable
- **AND** item counts SHALL persist across runs and app restarts

#### Scenario: Acquisition

- **WHEN** the player buys the global-store item entry
- **THEN** it SHALL cost 60 points
- **AND** one random item SHALL be added to the profile
- **AND** the item entry SHALL become unavailable until the store restocks
- **AND** the profile write SHALL be atomic

### Requirement: Combat Item Use

The system SHALL let the player use an item during combat.

#### Scenario: Free action

- **WHEN** the player uses an item in the play phase
- **THEN** the item SHALL NOT consume energy
- **AND** it SHALL NOT end the turn
- **AND** it SHALL NOT count as playing a card
- **AND** it SHALL NOT trigger AfterPlayed or On Reveal events
- **AND** it SHALL NOT use a board slot

#### Scenario: Legal target

- **WHEN** an item needs a target
- **THEN** the system SHALL choose uniformly at random among the legal targets
- **AND** a target protected from destruction SHALL NOT be eligible for `destroy_enemy`
- **AND** a move that violates a zone lock SHALL NOT be eligible for `move_enemy`
- **AND** a full hand SHALL NOT be eligible for `copy_hand` or `draw_two`
- **AND** if no legal target exists the use SHALL be rejected

#### Scenario: Consumption

- **WHEN** an item use succeeds
- **THEN** one count SHALL be removed from the profile
- **AND** the profile SHALL be written immediately
- **AND** a failed profile write SHALL restore the count and report the failure
- **AND** a rejected use SHALL NOT consume the item

#### Scenario: Outside combat

- **WHEN** the player is not in a combat play phase
- **THEN** items SHALL NOT be usable
- **AND** no count SHALL change
