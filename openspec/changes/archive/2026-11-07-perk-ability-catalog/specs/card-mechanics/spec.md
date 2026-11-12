# Spec Delta

## ADDED Requirements

### Requirement: Perk-Bearing Instance State

The system SHALL track the per-instance state introduced by ability perks.

#### Scenario: Attached perk state

- **WHEN** a card instance has attached perks
- **THEN** it SHALL track their ids for the whole run
- **AND** the ids SHALL survive combat resets, movement and returning to hand
- **AND** removing the card from the deck SHALL remove its perks

#### Scenario: One-time move state

- **WHEN** a card has the Maneuverable ability
- **THEN** the instance SHALL track whether its player move was already used this combat
- **AND** the flag SHALL reset at the start of a new combat

#### Scenario: Immunity state

- **WHEN** a card has the Unstoppable ability
- **THEN** the instance SHALL expose destroy immunity and negative-Power immunity
- **AND** the immunity SHALL end when the card leaves play or the combat ends

#### Scenario: Empowered state

- **WHEN** a card with Reinforce is in hand, deck or play
- **THEN** the instance SHALL be able to receive Empowered events
- **AND** the +1 from Reinforce SHALL NOT re-trigger itself

#### Scenario: Combat reset

- **WHEN** a new combat starts
- **THEN** temporary combat state SHALL clear
- **AND** attached perks, upgrades, cost delta and power delta SHALL remain
- **AND** the one-time move and immunity flags SHALL reset

### Requirement: Play and Movement Rules

The system SHALL support play restrictions, movement permissions and next-turn reservations.

#### Scenario: Unstoppable play

- **WHEN** a card with Unstoppable is played
- **THEN** it SHALL ignore zone play restrictions and zone locks
- **AND** it SHALL still respect the four-card zone limit and the hand requirement

#### Scenario: Unstoppable destruction

- **WHEN** an effect tries to destroy a card with Unstoppable
- **THEN** the destroy SHALL be rejected
- **AND** the board, piles and energy SHALL remain unchanged
- **AND** no destroy reaction SHALL fire

#### Scenario: Unstoppable Power reduction

- **WHEN** an effect tries to reduce a card with Unstoppable's Power
- **THEN** the reduction SHALL be ignored
- **AND** the card's Power SHALL remain at its calculated value

#### Scenario: Maneuverable move

- **WHEN** the player moves a card with Maneuverable
- **THEN** the move SHALL be allowed once per combat
- **AND** the destination SHALL have room
- **AND** a later player move SHALL be rejected until the next combat

#### Scenario: Taunt reservation

- **WHEN** a Taunt host is revealed
- **THEN** the opposing side's first card played next turn SHALL be reserved for the host's zone
- **AND** the reservation SHALL apply once and expire at the end of that turn
- **AND** when the zone is full, the reservation SHALL expire without blocking the play

### Requirement: Opening Hand and Deck Copy

The system SHALL support abilities that act at combat start from the deck.

#### Scenario: Superstar

- **WHEN** a combat starts
- **THEN** a host with Superstar SHALL be dealt in the opening hand before the random cards
- **AND** when more forced cards exist than hand slots, the excess SHALL stay in the deck
- **AND** the forced cards SHALL NOT be drawn again by the opening fill

#### Scenario: Clone

- **WHEN** a combat starts and a host with Clone is in the deck or hand
- **THEN** an unmodified copy of the host SHALL be shuffled into the owner's deck
- **AND** the copy SHALL have its own instance identity and base values
- **AND** it SHALL NOT copy upgrades, persistent bonuses or attached perks

#### Scenario: Deck order

- **WHEN** a deck copy or opening-hand move occurs
- **THEN** the remaining deck order SHALL stay deterministic for the same seed
- **AND** a later draw SHALL NOT duplicate the moved or copied instance
