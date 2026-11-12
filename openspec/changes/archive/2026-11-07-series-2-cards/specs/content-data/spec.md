# Spec Delta

## MODIFIED Requirements

### Requirement: Effect Library

The system SHALL include the effect definitions referenced by the 92-card pool and the zones, and every card effect SHALL have an implementation.

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

#### Scenario: Series 2 effect content

- **WHEN** effects are initialized
- **THEN** the library SHALL include an implementation for every effect referenced by a Series 2 or Collection Level card
- **AND** every Series 2 and Collection Level card's effect ids SHALL resolve to a registered effect
- **AND** no Series 2 or Collection Level card SHALL reference a placeholder or unimplemented effect

#### Scenario: Zone effects remain registered

- **WHEN** effects are initialized
- **THEN** the three zone effects used by the starting zones SHALL be registered and implemented
- **AND** the zones SHALL apply those effects through the same effect system as cards
- **AND** implementing the zone effects SHALL be part of this change

#### Scenario: No placeholder card effects

- **WHEN** effects are initialized
- **THEN** the library SHALL NOT register placeholder card effects that no card references
- **AND** every registered card effect SHALL have an implementation

### Requirement: Perk Library

The system SHALL include 28 ability perks, the Zombie Horde token and no global rule perks.

#### Scenario: Initial perks

- **WHEN** perks are initialized
- **THEN** there SHALL be exactly 28 ability perks available
- **AND** all 28 SHALL be owned by default
- **AND** no perk SHALL be a store perk
- **AND** the retired rule perk ids SHALL NOT be registered
- **AND** each perk SHALL declare the effect ids and compatibility metadata for its ability

#### Scenario: Store perks

- **WHEN** the store catalog is built
- **THEN** no perk entry SHALL be present
- **AND** no perk SHALL require a purchase to enter the level-up pool

#### Scenario: Perk rule data

- **WHEN** a perk is defined
- **THEN** it SHALL have an id, name, rule text, effect ids and compatibility metadata
- **AND** its effect ids SHALL resolve to registered implementations
- **AND** an effect implementation MAY be shared with a current or future card
- **AND** a perk SHALL NOT be implemented as a one-off branch in a scene

#### Scenario: Zombie Horde token

- **WHEN** the content is loaded
- **THEN** a non-draftable `token_zombie_horde` SHALL exist in the card registry
- **AND** it SHALL have 0 cost and 0 base Power
- **AND** it SHALL NOT be counted in the 92 draftable cards
- **AND** Deadlink SHALL be able to add Power to it

#### Scenario: Validation

- **WHEN** content validation runs
- **THEN** every perk effect id SHALL resolve to a registered implementation
- **AND** every compatibility requirement SHALL name a supported host trait or a board condition
- **AND** a missing implementation or malformed compatibility SHALL fail validation before a run starts

### Requirement: Card Pool

The system SHALL include exactly 92 unique card definitions for drafting.

#### Scenario: Card pool initialization

- **WHEN** the game starts
- **THEN** exactly 92 unique cards SHALL be available
- **AND** 21 SHALL be the starter cards listed in the Starter Card Set requirement
- **AND** 46 SHALL be the cards listed in the Series 1 Card Set requirement
- **AND** 25 SHALL be the cards listed in the Series 2 Card Set requirement
- **AND** Abomination, Misty Knight and Shocker SHALL remain in the catalog as non-starter cards
- **AND** a card present in more than one list SHALL be counted once
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

#### Scenario: Displaced starters remain

- **WHEN** the starter set changes
- **THEN** Abomination, Misty Knight and Shocker SHALL remain available as non-starter cards
- **AND** they SHALL be obtainable through the global store or a bundle
- **AND** they SHALL NOT be part of the starting owned set

### Requirement: Starter Card Set

The system SHALL register the following 21 starter cards with their exact costs and base powers:
- Iron Man: cost 5, power 0, Ongoing.
- Punisher: cost 3, power 3, Ongoing.
- Hulk: cost 6, power 14, no ability.
- The Thing: cost 4, power 7, no ability.
- Cyclops: cost 3, power 5, no ability.
- Elektra: cost 1, power 2, On Reveal.
- Carnage: cost 2, power 2, On Reveal.
- Deathlok: cost 3, power 6, On Reveal.
- Hawkeye: cost 1, power 1, On Reveal.
- Medusa: cost 2, power 2, On Reveal.
- Sentinel: cost 2, power 3, On Reveal.
- Star-Lord: cost 2, power 2, On Reveal.
- Quicksilver: cost 1, power 2, opening hand.
- Jessica Jones: cost 4, power 5, On Reveal.
- Ka-Zar: cost 4, power 6, Ongoing.
- Mister Fantastic: cost 3, power 1, Ongoing.
- Nightcrawler: cost 1, power 3, moveable once.
- Odin: cost 6, power 10, On Reveal.
- Spectrum: cost 6, power 7, On Reveal.
- White Tiger: cost 5, power 1, On Reveal.
- Wolfsbane: cost 3, power 2, On Reveal.

#### Scenario: Starter cards are registered

- **WHEN** the card pool is loaded
- **THEN** each of the 21 cards SHALL exist with the listed cost and base power
- **AND** each SHALL declare its draft theme
- **AND** each card SHALL have a non-empty description

#### Scenario: Starter migration

- **WHEN** an existing profile is loaded
- **THEN** every current starter card SHALL be present in the owned set
- **AND** a missing starter SHALL be added to the owned set
- **AND** no previously owned card SHALL be removed
- **AND** the eight Collection Level starters SHALL be owned after the migration

#### Scenario: Ability cards show ability text

- **WHEN** an ability card's description is read
- **THEN** it SHALL contain the card's ability text
- **AND** On Reveal text SHALL start with `Al revelarse:`
- **AND** Ongoing text SHALL start with `Continuo:`
- **AND** Quicksilver's text SHALL state that it starts in the opening hand
- **AND** movement text SHALL use `Movible` or `Movible una vez`

#### Scenario: Vanilla cards show generic text

- **WHEN** a card with no ability is inspected
- **THEN** its description SHALL describe its role or stats
- **AND** it SHALL NOT claim an ability the card does not have

#### Scenario: Removed starters

- **WHEN** the starter set is loaded
- **THEN** Abomination, Misty Knight and Shocker SHALL NOT be starters
- **AND** they SHALL remain in the 92-card catalog
- **AND** the first-run owned set SHALL contain only the 21 starters

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
- **AND** the pool SHALL contain the 21 starter cards, the 46 Series 1 cards and the 25 Series 2 cards

### Requirement: Draft Themes

The system SHALL assign a draft theme to every card.

#### Scenario: Theme values

- **WHEN** the card content is loaded
- **THEN** every draftable card SHALL declare `Control`, `Destroy`, `OnReveal` or `Neutral`
- **AND** a token SHALL NOT require a draft theme
- **AND** the theme SHALL be shown in the collection and the draft

#### Scenario: Starter themes

- **WHEN** the 21 starters are read
- **THEN** the Control starters SHALL be Iron Man, Punisher, Hulk, The Thing, Cyclops, Jessica Jones, Ka-Zar, Mister Fantastic and Spectrum
- **AND** the Destroy starters SHALL be Elektra, Carnage and Deathlok
- **AND** the OnReveal starters SHALL be Hawkeye, Medusa, Sentinel, Star-Lord, Odin, White Tiger and Wolfsbane
- **AND** the Neutral starter SHALL be Quicksilver and Nightcrawler

#### Scenario: Series 2 themes

- **WHEN** the 25 Series 2 cards are read
- **THEN** Control SHALL include Ebony Maw, Sandman, Storm, Warpath and Leech
- **AND** Destroy SHALL include Bucky Barnes, Killmonger, Sabretooth and Shang-Chi
- **AND** OnReveal SHALL include Agent 13, Iceman, Jubilee, Nakia, Okoye, Rhino and Scorpion
- **AND** Neutral SHALL include Cloak, Hobgoblin, Morbius, Sunspot, Swarm, The Collector, The Infinaut, Vision and Vulture

#### Scenario: Theme groups

- **WHEN** the themed draft is generated
- **THEN** each group SHALL contain 3 distinct owned cards of its theme
- **AND** the groups SHALL be disjoint
- **AND** a Neutral card SHALL only enter the random phase unless a group needs a filler
- **AND** a group with fewer than 3 owned cards SHALL be filled with owned Neutral cards and marked as filled

#### Scenario: Theme coverage

- **WHEN** all 92 cards are loaded
- **THEN** every card SHALL have a theme
- **AND** the Control, Destroy and OnReveal groups SHALL each have enough owned cards for the draft once the starter set is owned
- **AND** the theme assignment SHALL be data, not a scene branch

## ADDED Requirements

### Requirement: Series 2 Card Set

The system SHALL register the following 25 Series 2 cards with their exact costs, base powers and abilities.

| Cost | Power | Card | Ability |
|---|---:|---:|---|
| 1 | 3 | Agent 13 | On Reveal: Add a random card to your hand. |
| 2 | 2 | Bucky Barnes | When Destroyed: Replace this with the Winter Soldier. |
| 2 | 4 | Cloak | On Reveal: Next turn, all cards are Moveable to this location. |
| 1 | 7 | Ebony Maw | You can't play this after turn 3. Ongoing: You can't play cards here. |
| 5 | -8 | Hobgoblin | On Reveal: Switch sides. |
| 1 | 2 | Iceman | On Reveal: Give a card in your opponent's hand +1 Cost. (maximum 6) |
| 4 | 1 | Jubilee | On Reveal: Add the top card of your deck to this location. |
| 3 | 3 | Killmonger | On Reveal: Destroy ALL 1-Cost cards. |
| 4 | 5 | Leech | On Reveal: Remove the text from 6-Cost or more cards in your opponent's hand. |
| 2 | 0 | Morbius | Ongoing: +2 Power for each time you discarded a card this game. |
| 3 | 4 | Nakia | On Reveal: Give all cards in your hand +1 Power. |
| 2 | 3 | Okoye | On Reveal: Give every card in your deck +1 Power. |
| 2 | 5 | Rhino | On Reveal: Add a Rock to your side of this location. |
| 3 | 5 | Sabretooth | When Destroyed: Return this to your hand. It costs 0. |
| 5 | 8 | Sandman | On Reveal: Next turn, cards cost 1 more. (maximum 6) |
| 2 | 3 | Scorpion | On Reveal: Afflict cards in your opponent's hand with -1 Power. |
| 4 | 3 | Shang-Chi | On Reveal: Destroy an enemy card here that has 10 or more Power. |
| 3 | 1 | Storm | On Reveal: Flood this location. Next turn is the last turn cards can be played here. |
| 1 | 0 | Sunspot | End of Turn: Gain +1 Power for each unspent Energy. |
| 1 | 3 | Swarm | When Discarded: Add two copies of this that cost 0 to your hand. |
| 2 | 3 | The Collector | When a card enters your hand from anywhere (except your deck), +1 Power. |
| 6 | 20 | The Infinaut | If you played a card last turn, you can't play this. |
| 5 | 9 | Vision | Moveable |
| 1 | 3 | Vulture | When this card moves, +4 Power. |
| 4 | 5 | Warpath | Ongoing: If any of your locations are empty, +5 Power. |

#### Scenario: Series 2 cards are registered

- **WHEN** the card pool is loaded
- **THEN** each of the 25 cards SHALL exist with the listed cost and base power
- **AND** each card SHALL have a non-empty description
- **AND** no two cards SHALL share a name or an ID

#### Scenario: Series 2 descriptions carry the ability

- **WHEN** a Series 2 card's description is read
- **THEN** it SHALL describe the ability in Spanish without accents
- **AND** On Reveal text SHALL start with `Al revelarse:`
- **AND** Ongoing text SHALL start with `Continuo:`
- **AND** destroy, discard, move and play-restriction text SHALL use a consistent Spanish prefix

#### Scenario: Series 2 cards are catalog content

- **WHEN** a new profile is created
- **THEN** the 25 Series 2 cards SHALL NOT be part of the starting owned set
- **AND** they SHALL be registered as draftable catalog cards
- **AND** they SHALL be obtainable through the global store

### Requirement: Series 2 Tokens

The system SHALL register the Winter Soldier and Tiger tokens used by the Series 2 cards.

#### Scenario: Winter Soldier token

- **WHEN** the content is loaded
- **THEN** a non-draftable `token_winter_soldier` SHALL exist in the card registry
- **AND** it SHALL have 0 cost and 8 base Power
- **AND** it SHALL have no ability
- **AND** Bucky Barnes SHALL replace itself with this token when destroyed

#### Scenario: Tiger token

- **WHEN** the content is loaded
- **THEN** a non-draftable `token_tiger` SHALL exist in the card registry
- **AND** it SHALL have 0 cost and 10 base Power
- **AND** it SHALL have no ability
- **AND** White Tiger SHALL add this token to another location when revealed

#### Scenario: Tokens are not draftable

- **WHEN** the draft or reward pool is built
- **THEN** neither series 2 token SHALL be offered
- **AND** neither token SHALL be part of the 92 draftable cards
