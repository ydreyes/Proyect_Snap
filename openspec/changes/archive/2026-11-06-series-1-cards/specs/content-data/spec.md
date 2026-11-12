# Spec Delta

## MODIFIED Requirements

### Requirement: Initial Zone Content

The system SHALL include 3 starting zones with specific effects and treat them as the location replacement pool.

#### Scenario: Test zones

- **WHEN** the game starts
- **THEN** 3 zones SHALL be available
- **AND** one zone SHALL give +1 power to all cards
- **AND** one zone SHALL give -1 power to all cards
- **AND** one zone SHALL give +2 power to the card of highest cost

#### Scenario: Zone effects are active

- **WHEN** a zone is revealed and its effect applies
- **THEN** the effect SHALL modify the appropriate card or zone power
- **AND** the modification SHALL update whenever the board changes
- **AND** the modification SHALL NOT be stored as an accumulated card power

#### Scenario: Replacement pool

- **WHEN** a card replaces a location
- **THEN** the new location SHALL come from the registered location pool
- **AND** the new location SHALL NOT be the location being replaced
- **AND** the new location SHALL be revealed immediately

### Requirement: Effect Library

The system SHALL include the effect definitions referenced by the 59-card pool and the zones, and every card effect SHALL have an implementation.

#### Scenario: Effect content

- **WHEN** effects are initialized
- **THEN** the library SHALL include the OnReveal effects for Hawkeye, Medusa, Sentinel and Star-Lord
- **AND** the Ongoing effects for Iron Man and Punisher
- **AND** each effect SHALL be referenced by the corresponding card definition

#### Scenario: Series 1 effect content

- **WHEN** effects are initialized
- **THEN** the library SHALL include an implementation for every effect referenced by a Series 1 card
- **AND** every Series 1 card's effect ids SHALL resolve to a registered effect
- **AND** no Series 1 card SHALL reference a placeholder or unimplemented effect

#### Scenario: Zone effects remain registered

- **WHEN** effects are initialized
- **THEN** the three zone effects used by the starting zones SHALL be registered and implemented
- **AND** the zones SHALL apply those effects through the same effect system as cards
- **AND** implementing the zone effects SHALL be part of this change

#### Scenario: No placeholder card effects

- **WHEN** effects are initialized
- **THEN** the library SHALL NOT register placeholder card effects that no card references
- **AND** every registered card effect SHALL have an implementation

### Requirement: Card Pool

The system SHALL include a pool of exactly 59 card definitions for drafting: the 13 starter cards and the 46 Series 1 cards, each with a player-facing description.

#### Scenario: Card pool initialization

- **WHEN** the game starts
- **THEN** exactly 59 cards SHALL be available for drafting
- **AND** 13 SHALL be the starter cards listed in the Starter Card Set requirement
- **AND** 46 SHALL be the Series 1 cards listed in the Series 1 Card Set requirement
- **AND** the 7 fillers Spark, Raven, Champion, Behemoth, Juggernaut, Titan and Leviathan SHALL NOT be registered
- **AND** no two cards SHALL share a name or an ID
- **AND** new cards SHALL be easily addable to the pool

#### Scenario: Curve coverage

- **WHEN** the card pool is loaded
- **THEN** costs SHALL cover 1 through 6
- **AND** at least one card SHALL exist at each cost from 1 to 6
- **AND** no 0-cost card SHALL be required

#### Scenario: Every card has a description

- **WHEN** the card pool is loaded
- **THEN** every card definition SHALL have a non-empty descriptive text
- **AND** the text SHALL be written in Spanish without accents
- **AND** the text SHALL be readable in the card detail panel

#### Scenario: Old generic Sentinel is gone

- **WHEN** the card pool is loaded
- **THEN** the 6-cost 5-power Sentinel SHALL NOT be registered
- **AND** the only card named Sentinel SHALL be the 2-cost 3-power card

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
- **THEN** a new Sentinel SHALL be added to its owner's hand if the hand has room
- **AND** the new Sentinel SHALL be a separate instance
- **AND** if the hand already has 7 cards, the Sentinel SHALL NOT be added and the hand SHALL remain unchanged

#### Scenario: Star-Lord gains power against a rival play

- **WHEN** Star-Lord is revealed on turn N
- **AND** at least one rival card was played in Star-Lord's zone during turn N
- **THEN** Star-Lord SHALL gain +4 permanent power
- **AND** the bonus SHALL apply exactly once

## ADDED Requirements

### Requirement: Series 1 Card Set

The system SHALL register the following 46 Series 1 cards with their exact costs, base powers and abilities.

| Cost | Power | Card | Ability |
|---|---:|---:|---|
| 1 | 2 | America Chavez | On Reveal: give the top card of your deck +3 Power |
| 2 | 4 | Angel | When one of your cards is destroyed, this flies out of your hand or deck to replace it |
| 2 | 3 | Angela | After you play a card here, +1 Power |
| 6 | 10 | Apocalypse | When Discarded: put this back with +4 Power |
| 2 | 3 | Armor | Ongoing: cards here can't be destroyed |
| 3 | 2 | Bishop | After you play a card, this gains +1 Power |
| 1 | 3 | Blade | On Reveal: discard the rightmost card from your hand |
| 2 | 4 | Cable | On Reveal: draw a card from your opponent's deck |
| 3 | 3 | Captain America | Ongoing: your other Ongoing cards here have +2 Power |
| 2 | 2 | Carnage | On Reveal: destroy your other cards here; +2 Power for each destroyed |
| 3 | 3 | Cosmo | Ongoing: On Reveal abilities won't happen here |
| 3 | 6 | Deathlok | On Reveal: destroy your other cards here |
| 5 | 4 | Devil Dinosaur | Ongoing: +2 Power for each card in your hand |
| 2 | 4 | Doctor Strange | On Reveal: move your highest-Power card(s) to this location |
| 2 | 3 | Domino | You always draw this card on turn 2, and not before |
| 1 | 2 | Elektra | On Reveal: destroy an enemy 1-Cost card here |
| 5 | 8 | Enchantress | On Reveal: remove the abilities from all Ongoing cards here |
| 2 | 2 | Forge | On Reveal: give the next card you play +2 Power |
| 3 | 5 | Groot | On Reveal: if your opponent played a card here this turn, +4 Power |
| 6 | 10 | Heimdall | On Reveal: move your other cards one location to the left |
| 2 | 3 | Hulkbuster | On Reveal: merge this into one of your cards here |
| 1 | 3 | Iron Fist | On Reveal: after you play your next card, move it one location to the left |
| 4 | 3 | Klaw | Ongoing: the location to the right has +6 Power |
| 1 | 2 | Korg | On Reveal: shuffle a Rock into your opponent's deck |
| 2 | 3 | Kraven | When a card moves here, this gains +2 Power |
| 3 | 6 | Lady Sif | On Reveal: discard the card that costs the most from your hand |
| 2 | 6 | Lizard | Ongoing: -4 Power if your opponent has 4 cards here |
| 1 | 3 | Mantis | On Reveal: if your opponent played any cards here this turn, copy one of them into your hand |
| 2 | 2 | Mister Sinister | On Reveal: add a Sinister Clone here with the same Power |
| 4 | 5 | Moon Girl | On Reveal: copy your hand |
| 3 | 0 | Morph | On Reveal: become a copy of a card in your opponent's hand |
| 2 | 4 | Multiple Man | When this moves, add a copy to the old location |
| 4 | 6 | Namor | Ongoing: +5 Power if this is your only card here |
| 1 | 1 | Nova | When Destroyed: give your cards +1 Power |
| 6 | 8 | Onslaught | Ongoing: your other Ongoing effects here apply an additional time |
| 5 | 2 | Professor X | Ongoing: moving is the only way to add or remove a character from here |
| 1 | 1 | Rocket Raccoon | On Reveal: if your opponent played a card here this turn, +4 Power |
| 2 | 3 | Scarlet Witch | On Reveal: replace this location with a new one |
| 5 | 8 | Spider-Woman | On Reveal: afflict all enemy cards here with -1 Power |
| 1 | 2 | Squirrel Girl | On Reveal: add a 1-Power Squirrel to each other location |
| 3 | 4 | Strong Guy | Ongoing: +6 Power if your hand has 1 or fewer cards |
| 3 | 7 | Sword Master | On Reveal: discard an odd-costed card from your hand |
| 1 | 2 | Uatu the Watcher | Game Start: you can see the unrevealed locations |
| 3 | 5 | White Queen | On Reveal: copy the card that costs the most from your opponent's hand into your hand |
| 2 | 3 | Wolverine | When Discarded or Destroyed: regenerate this with +2 Power at a random location |
| 1 | 2 | Yondu | On Reveal: banish the card that costs the least in your opponent's deck |

#### Scenario: Series 1 cards are registered

- **WHEN** the card pool is loaded
- **THEN** each of the 46 cards SHALL exist with the listed cost and base power
- **AND** the names SHALL use the official spelling
- **AND** each card SHALL have a non-empty description

#### Scenario: Series 1 descriptions carry the ability

- **WHEN** a Series 1 card's description is read
- **THEN** it SHALL describe the ability in Spanish without accents
- **AND** On Reveal text SHALL start with `Al revelarse:`
- **AND** Ongoing text SHALL start with `Continuo:`
- **AND** discard, destroy, move and game-start text SHALL use a consistent Spanish prefix

#### Scenario: Removed fillers are gone

- **WHEN** the card pool is loaded
- **THEN** Spark, Raven, Champion, Behemoth, Juggernaut, Titan and Leviathan SHALL NOT be registered
- **AND** the pool SHALL contain exactly the 13 starter cards and the 46 Series 1 cards
