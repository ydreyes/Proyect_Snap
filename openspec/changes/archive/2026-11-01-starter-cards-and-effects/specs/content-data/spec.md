# Spec Delta

## MODIFIED Requirements

### Requirement: Effect Library
The system SHALL include the effect definitions referenced by the current cards and zones.

#### Scenario: Effect content
- **WHEN** effects are initialized
- **THEN** the library SHALL include the OnReveal effects for Hawkeye, Medusa, Sentinel and Star-Lord
- **AND** the Ongoing effects for Iron Man and Punisher
- **AND** each effect SHALL be referenced by the corresponding card definition
- **AND** each card effect SHALL be usable by the effect processor

#### Scenario: Zone effects remain registered
- **WHEN** effects are initialized
- **THEN** the three zone effects used by the zones SHALL remain registered
- **AND** implementing those zone effects SHALL NOT be required by this change

#### Scenario: No placeholder card effects
- **WHEN** effects are initialized
- **THEN** the library SHALL NOT register placeholder card effects that no card references
- **AND** every registered card effect SHALL have an implementation

### Requirement: Card Pool
The system SHALL include a starter card pool of exactly 20 card definitions for drafting, each with a player-facing description.

#### Scenario: Card pool initialization
- **WHEN** the game starts
- **THEN** exactly 20 cards SHALL be available for drafting
- **AND** 13 SHALL be the starter cards listed in the Starter Card Set requirement
- **AND** 7 SHALL be the vanilla fillers Spark, Raven, Champion, Behemoth, Juggernaut, Titan and Leviathan
- **AND** no two cards SHALL share a name or an ID
- **AND** new cards SHALL be easily addable to the pool

#### Scenario: Curve coverage
- **WHEN** the card pool is loaded
- **THEN** costs SHALL cover 0 through 6
- **AND** at least one card SHALL exist at each cost from 0 to 6

#### Scenario: Every card has a description
- **WHEN** the card pool is loaded
- **THEN** every card definition SHALL have a non-empty descriptive text
- **AND** the text SHALL be written in Spanish without accents
- **AND** the text SHALL be readable in the card detail panel

#### Scenario: Old generic Sentinel is gone
- **WHEN** the card pool is loaded
- **THEN** the 6-cost 5-power Sentinel SHALL NOT be registered
- **AND** the only card named Sentinel SHALL be the 2-cost 3-power card

## ADDED Requirements

### Requirement: Starter Card Set
The system SHALL register the following 13 starter cards with their exact costs and base powers:
- Abomination: cost 5, power 9, no ability.
- Cyclops: cost 3, power 5, no ability.
- Hawkeye: cost 1, power 1, On Reveal.
- Hulk: cost 6, power 14, no ability.
- Iron Man: cost 5, power 0, Ongoing.
- Medusa: cost 2, power 2, On Reveal.
- Misty Knight: cost 1, power 3, no ability.
- Punisher: cost 3, power 3, Ongoing.
- Quicksilver: cost 1, power 2, opening hand.
- Sentinel: cost 2, power 3, On Reveal.
- Shocker: cost 2, power 4, no ability.
- Star-Lord: cost 2, power 2, On Reveal.
- The Thing: cost 4, power 7, no ability.

#### Scenario: Starter cards are registered
- **WHEN** the card pool is loaded
- **THEN** each of the 13 cards SHALL exist with the listed cost and base power
- **AND** the names SHALL use the official spelling Medusa, Sentinel, Shocker, Punisher and Star-Lord
- **AND** each card SHALL have a non-empty description

#### Scenario: Ability cards show ability text
- **WHEN** an ability card's description is read
- **THEN** it SHALL contain the card's ability text
- **AND** On Reveal text SHALL start with `Al revelarse:`
- **AND** Ongoing text SHALL start with `Continuo:`
- **AND** Quicksilver's text SHALL state that it starts in the opening hand

#### Scenario: Vanilla cards show generic text
- **WHEN** a card with no ability is inspected
- **THEN** its description SHALL describe its role or stats
- **AND** it SHALL NOT claim an ability the card does not have

### Requirement: Starter Card Abilities
The system SHALL implement the following abilities for the starter cards.

#### Scenario: Hawkeye gains power on the next turn
- **WHEN** Hawkeye is revealed on turn N
- **AND** an allied card is played in the zone where Hawkeye was played during turn N + 1
- **THEN** Hawkeye SHALL gain +5 permanent power at the end of turn N + 1
- **AND** the bonus SHALL apply exactly once

#### Scenario: Iron Man doubles the allied zone
- **WHEN** Iron Man is on the board
- **THEN** the total power of its side in that zone SHALL be doubled
- **AND** each card in the zone SHALL keep its individual power
- **AND** two Iron Men in the same zone SHALL multiply the total by four

#### Scenario: Medusa gains power in the middle zone
- **WHEN** Medusa is revealed in the middle zone
- **THEN** she SHALL gain +3 permanent power
- **AND** she SHALL NOT gain it in either side zone

#### Scenario: Punisher scales with enemies
- **WHEN** Punisher is on the board
- **THEN** it SHALL have +1 power for each enemy card in its zone
- **AND** the bonus SHALL update when an enemy card enters or leaves the zone
- **AND** the bonus SHALL be derived, not accumulated

#### Scenario: Quicksilver starts in hand
- **WHEN** a combat starts
- **THEN** every Quicksilver in a side's deck SHALL be moved to that side's opening hand
- **AND** this SHALL apply to the player and the AI
- **AND** the rest of the opening hand SHALL be filled to the starting hand size

#### Scenario: Sentinel creates a Sentinel
- **WHEN** Sentinel is revealed
- **THEN** a new Sentinel SHALL be added to its owner's hand
- **AND** the new Sentinel SHALL be a separate instance
- **AND** there SHALL be no hand size limit for this addition

#### Scenario: Star-Lord gains power against a rival play
- **WHEN** Star-Lord is revealed on turn N
- **AND** at least one rival card was played in Star-Lord's zone during turn N
- **THEN** Star-Lord SHALL gain +4 permanent power
- **AND** the bonus SHALL apply exactly once
