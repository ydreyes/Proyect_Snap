# ingame-shop Specification

## Purpose
Defines the in-game shop: what it offers, how prices are set, how purchases change the run deck and how the shop state survives a checkpoint.

## Requirements

### Requirement: Shop Stock

The system SHALL generate a shop stock when the player enters a shop node.

#### Scenario: Card offers

- **WHEN** a shop opens
- **THEN** it SHALL offer 4 distinct cards from the draftable catalog
- **AND** at least 1 card SHALL be a card that is not in the current deck
- **AND** when a card in the deck has fewer than 2 copies, at least 1 offer SHALL be a second copy of a card already in the deck
- **AND** the remaining offers SHALL be chosen at random
- **AND** the stock SHALL be generated once and SHALL NOT reroll when the scene is re-entered

#### Scenario: Card prices

- **WHEN** a card is offered
- **THEN** a cost 1-2 card SHALL cost 110 money
- **AND** a cost 3-4 card SHALL cost 140 money
- **AND** a cost 5-6 card SHALL cost 170 money
- **AND** the price SHALL use the card's base cost, not its effective cost

### Requirement: Card Purchase

The system SHALL let the player buy one or more card offers when the deck has room.

#### Scenario: Buying a card

- **WHEN** the player buys a card and the deck has fewer than 12 cards
- **THEN** the money SHALL be deducted
- **AND** the card SHALL become a deck instance
- **AND** the purchase SHALL be written to the run checkpoint immediately
- **AND** the offer SHALL be removed from the stock

#### Scenario: Deck at the cap

- **WHEN** the player buys a card and the deck has 12 cards
- **THEN** the player SHALL choose a deck card to remove before the new card is added
- **AND** the deck size SHALL remain 12
- **AND** cancelling the removal SHALL cancel the purchase without spending money

#### Scenario: Unaffordable purchase

- **WHEN** the player cannot pay the price
- **THEN** the purchase SHALL be rejected
- **AND** the money and deck SHALL remain unchanged

### Requirement: Upgrade Offers

The system SHALL offer permanent card upgrades in the shop.

#### Scenario: Upgrade count by act

- **WHEN** a shop opens in act 1
- **THEN** it SHALL offer 0 upgrades
- **AND** act 2 SHALL offer 1 upgrade
- **AND** act 3 SHALL offer 2 upgrades
- **AND** the final act SHALL offer 3 upgrades

#### Scenario: Buying an upgrade

- **WHEN** the player buys an upgrade
- **THEN** the price SHALL be 70 money
- **AND** the player SHALL choose a deck card with fewer than 3 upgrades
- **AND** the player SHALL choose `-1 cost` or `+2 power`
- **AND** the upgrade SHALL be applied to that instance
- **AND** the purchase SHALL be written to the checkpoint

### Requirement: Card Removal

The system SHALL let the player remove a card from the deck.

#### Scenario: Removing a card

- **WHEN** the player pays 55 money and chooses a deck card
- **THEN** the card SHALL be removed from the deck
- **AND** the deck SHALL NOT go below 6 cards
- **AND** the removal SHALL be written to the checkpoint

#### Scenario: Deck at the minimum

- **WHEN** the deck has 6 cards
- **THEN** the removal option SHALL be unavailable
- **AND** no money SHALL be spent

### Requirement: Extra Life

The system SHALL sell an extra-life charge that repeats the same enemy once.

#### Scenario: Buying the charge

- **WHEN** the player pays 220 money
- **THEN** the run SHALL gain one extra-life charge
- **AND** the run SHALL hold at most one charge
- **AND** the purchase SHALL be written to the checkpoint
- **AND** the option SHALL be unavailable when the run already holds a charge

#### Scenario: Consuming the charge

- **WHEN** the player loses a combat while holding an extra-life charge
- **THEN** the charge SHALL be consumed
- **AND** the same enemy SHALL be repeated once
- **AND** the run SHALL NOT end
- **AND** the checkpoint SHALL record the consumed charge before the rematch

### Requirement: Shop State Persistence

The system SHALL checkpoint the shop after every purchase.

#### Scenario: Re-entering the shop

- **WHEN** the player leaves and re-enters a shop without a new node
- **THEN** the same stock, prices, upgrade offers and remaining extra-life option SHALL be shown
- **AND** already purchased offers SHALL remain sold
- **AND** the stock SHALL NOT reroll
