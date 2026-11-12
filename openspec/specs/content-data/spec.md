# content-data Specification

## Purpose
The content data system provides initial game content definitions for cards, zones, effects, and perks that can be extended easily.

## Requirements

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

The system SHALL include the effect definitions referenced by the 217-card pool and the zones, and every card effect SHALL have an implementation.

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

#### Scenario: Series 3 effect content

- **WHEN** effects are initialized
- **THEN** the library SHALL include an implementation for every effect referenced by a Series 3 card
- **AND** every Series 3 card's effect ids SHALL resolve to a registered effect
- **AND** no Series 3 card SHALL reference a placeholder or unimplemented effect

#### Scenario: Zone effects remain registered

- **WHEN** effects are initialized
- **THEN** the three zone effects used by the starting zones SHALL be registered and implemented
- **AND** the Limbo zone effect SHALL be registered and implemented
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

The system SHALL include exactly 217 unique card definitions for drafting.

#### Scenario: Card pool initialization

- **WHEN** the game starts
- **THEN** exactly 217 unique cards SHALL be available
- **AND** 21 SHALL be the starter cards listed in the Starter Card Set requirement
- **AND** 46 SHALL be the cards listed in the Series 1 Card Set requirement
- **AND** 25 SHALL be the cards listed in the Series 2 Card Set requirement
- **AND** 125 SHALL be the cards listed in the Series 3 Card Set requirement
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

### Requirement: Data-Driven Content
The system SHALL load content in a data-driven manner where possible.

#### Scenario: Extensible content
- **WHEN** adding new content
- **THEN** cards, zones, effects, and perks SHALL be definable through data structures
- **AND** new content SHALL not require core system changes
- **AND** content definitions SHALL be separate from game logic

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

### Requirement: Enemy Definition Library

The system SHALL include the 23 enemy definitions used by the run.

#### Scenario: Enemy content

- **WHEN** the content library is loaded
- **THEN** the 18 normal enemies, 4 sub-bosses and 1 final boss SHALL be registered
- **AND** each definition SHALL declare its name, flavor phrase, difficulty, act pool and 8-card deck
- **AND** every deck card id SHALL resolve to a registered draftable card
- **AND** the content SHALL be data-driven and separate from combat logic

#### Scenario: Act pools

- **WHEN** the enemy pools are read
- **THEN** act 1, act 2 and act 3 SHALL each contain 6 normal enemies
- **AND** the sub-boss pool SHALL contain 4 bosses
- **AND** the final-boss pool SHALL contain 1 boss

#### Scenario: No playable-card collision

- **WHEN** an enemy name is compared with a card name
- **THEN** an enemy SHALL NOT reuse a playable card name
- **AND** enemy ids SHALL use an `enemy_` prefix

### Requirement: Run Event and Modifier Library

The system SHALL include the 12 run events and the 6 run modifiers as data-driven content.

#### Scenario: Event content

- **WHEN** the content library is loaded
- **THEN** 12 event definitions SHALL be registered
- **AND** each event SHALL have a title, narrative text, accept effect and decline option
- **AND** the accept effect SHALL use only XP, money, card, upgrade, perk or modifier grants
- **AND** no event SHALL start a combat

#### Scenario: Modifier content

- **WHEN** the content library is loaded
- **THEN** the 6 modifier definitions SHALL be registered
- **AND** each modifier SHALL have an id, a display name and its parameter values
- **AND** the catalog SHALL match the `run-modifiers` specification

#### Scenario: Content is separate from logic

- **WHEN** a new event or modifier is added
- **THEN** it SHALL be definable as data
- **AND** it SHALL NOT require a change to the event or modifier engine

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

#### Scenario: Series 3 themes

- **WHEN** the 125 Series 3 cards are read
- **THEN** every Series 3 card SHALL declare a theme
- **AND** each theme SHALL include at least one Series 3 card
- **AND** the assignment SHALL be data, not a scene branch

#### Scenario: Theme groups

- **WHEN** the themed draft is generated
- **THEN** each group SHALL contain 3 distinct owned cards of its theme
- **AND** the groups SHALL be disjoint
- **AND** a Neutral card SHALL only enter the random phase unless a group needs a filler
- **AND** a group with fewer than 3 owned cards SHALL be filled with owned Neutral cards and marked as filled

#### Scenario: Theme coverage

- **WHEN** all 217 cards are loaded
- **THEN** every card SHALL have a theme
- **AND** the Control, Destroy and OnReveal groups SHALL each have enough owned cards for the draft once the starter set is owned
- **AND** the theme assignment SHALL be data, not a scene branch

### Requirement: Combat Item Library

The system SHALL include the 8 combat items as data-driven content.

#### Scenario: Item content

- **WHEN** the content library is loaded
- **THEN** the 8 item definitions SHALL be registered
- **AND** each item SHALL declare its id, name, description, rarity, effect id and target rule
- **AND** every effect id SHALL resolve to a registered implementation
- **AND** items SHALL NOT be registered as cards or tokens

#### Scenario: Rarity

- **WHEN** items are defined
- **THEN** each item SHALL declare a rarity
- **AND** the rarity SHALL be used by the store or future reward tables
- **AND** rarity SHALL NOT change the item effect

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
- **AND** neither token SHALL be part of the 217 draftable cards

### Requirement: Series 3 Card Set

The system SHALL register the following 125 Series 3 cards with their exact costs, base powers and abilities.

| Cost | Power | Card | Ability |
|---|---:|---:|---|
| 4 | 5 | Absorbing Man | On Reveal: If the last card you played has an On Reveal, copy its text. (if able) |
| 2 | 0 | Adam Warlock | End of Turn: Draw a card if you're winning here. Otherwise, +1 Power. |
| 5 | 9 | Aero | On Reveal: The last card your opponent played moves to this location. |
| 6 | 12 | Agatha Harkness | Game Start: Draw Agatha. She controls your cards on even turns. |
| 3 | 5 | Agent Coulson | On Reveal: Add a random 4-Cost and 5-Cost card to your hand. |
| 6 | 0 | Arnim Zola | On Reveal: Destroy one of your other cards here to copy it at the other locations. |
| 4 | 10 | Attuma | End of Turn: Destroy one of your cards here with less Power. |
| 2 | 3 | Baron Mordo | On Reveal: The top card of your opponent's deck costs 6 until turn 6. |
| 1 | 1 | Bast | On Reveal: Set the Power of all cards in your hand to 3. |
| 3 | 5 | Beast | On Reveal: Return your other cards here to your hand. They cost 1 less next turn. |
| 5 | 8 | Black Bolt | On Reveal: Discard the card that costs the least from your opponent's hand. |
| 1 | 4 | Black Cat | End of Turn: Discard this from your hand. |
| 5 | 5 | Black Panther | On Reveal: Double this card's Power. |
| 3 | 3 | Black Widow | Activate: Add a Widow's Bite to your opponent's hand. |
| 3 | 2 | Brood | On Reveal: Add 2 Broodlings here with the same Power. |
| 4 | 6 | Captain Marvel | At the end of the game, move to a location that wins you the game. (If possible) |
| 3 | 0 | Cerebro | Ongoing: Your highest-Power cards have +3 Power. |
| 2 | 3 | Colleen Wing | On Reveal: Discard the card that costs the least from your hand. |
| 4 | 10 | Crossbones | You can only play this at locations where you are winning. |
| 3 | 6 | Crystal | On Reveal: Each player draws a card. |
| 2 | 2 | Dagger | When this moves to a location, +2 Power for each enemy card there. |
| 2 | 3 | Daredevil | On turn 5, you get to see your opponent's plays before you make your own. |
| 2 | 2 | Dazzler | Ongoing: +2 Power for each location that's full on your side. |
| 1 | 1 | Deadpool | When Destroyed: Return this to your hand with double the Power. |
| 8 | 12 | Death | Costs 1 less for each card destroyed this game. |
| 3 | 7 | Debrii | On Reveal: Add a Rock to each other location. |
| 6 | 18 | Destroyer | On Reveal: Destroy your cards at other locations. |
| 6 | 5 | Doctor Doom | On Reveal: Add a 5-Power DoomBot to each other location. |
| 4 | 8 | Doctor Octopus | On Reveal: Pull the lowest-Power character from your opponent's hand to their side of this location. |
| 4 | 1 | Dracula | At the end of the game, discard a card from your hand to gain its Power. |
| 4 | 5 | Drax | On Reveal: If your opponent played a card here this turn, +4 Power. |
| 1 | 2 | Echo | After your opponent plays an Ongoing card here, remove its abilities. |
| 3 | 5 | Electro | On Reveal: +1 Max Energy. Ongoing: You can only play 1 card a turn. |
| 2 | 3 | Falcon | On Reveal: Return your 1-Cost cards to your hand. |
| 3 | 3 | Gambit | On Reveal: Discard a card from your hand to destroy a random enemy card. |
| 3 | 5 | Ghost | On Reveal: Remove the abilities from all End of Turn cards here. |
| 4 | 5 | Ghost Rider | On Reveal: Bring back one of your discarded cards to this location. |
| 1 | 3 | Ghost-Spider | On Reveal: The last card you played moves here. |
| 6 | 16 | Giganto | You can only play this at the left location. |
| 2 | 2 | Goose | Ongoing: Nobody can play cards that cost 4 or more here. |
| 3 | -4 | Green Goblin | On Reveal: Switch sides. |
| 2 | 1 | Havok | End of Turn: You lose 1 Max Energy and this gains +3 Power. |
| 3 | 3 | Hazmat | On Reveal: Afflict all other cards with -1 Power. |
| 6 | 6 | Hela | On Reveal: For each different Cost among them, resurrect a card you discarded to a random location. |
| 6 | 12 | Helicarrier | When Discarded: Fill your hand with random cards. |
| 4 | 7 | Hellcow | Activate: Discard a card from your hand. |
| 3 | 5 | Hercules | The first time another one of your cards moves here each turn, move it to another location. |
| 1 | 2 | Howard the Duck | Ongoing: Tap this to see the top card of your deck. |
| 3 | 2 | Human Torch | When this moves, double its Power. |
| 2 | 4 | Invisible Woman | Ongoing: Cards you play here are not revealed until the game ends. |
| 5 | 9 | Jane Foster Mighty Thor | On Reveal: Draw all cards that cost 0 from your deck. |
| 3 | 4 | Jean Grey | Ongoing: Players must play their first card here each turn. (if possible) |
| 4 | 5 | Juggernaut | On Reveal: Move away all enemy cards played here this turn. (including unrevealed cards) |
| 1 | 3 | Kingpin | When an enemy card moves here, afflict it with -2 Power. |
| 1 | 1 | Kitty Pryde | Start of Turn: Return this to your hand. When this returns to your hand, +1 Power. |
| 6 | 0 | Knull | Ongoing: Has the combined Power of all cards destroyed this game. |
| 5 | 7 | Lady Deathstrike | On Reveal: Destroy each card here with 3 or less Power. |
| 6 | 5 | Leader | On Reveal: Copy the enemy card(s) with the highest Power played this turn, but on your side. |
| 2 | 3 | Lockjaw | Activate: After you play your next character, swap it with one from your deck. |
| 2 | 2 | Luke Cage | Ongoing: Your front-row cards can't have their Power reduced. |
| 5 | 7 | M.O.D.O.K. | On Reveal: Discard your hand. |
| 1 | 3 | M'Baku | At the end of the game, this jumps from your deck to your lowest-Power location. (that isn't full) |
| 3 | 2 | Magik | On Reveal: Replace this location with 'Limbo'. Doesn't work after turn 5. |
| 6 | 12 | Magneto | On Reveal: Move all enemy 3 and 4-Cost cards to this location. |
| 1 | 3 | Maria Hill | On Reveal: Add a random 2-Cost card to your hand. |
| 1 | 5 | Martyr | At the end of the game, move to a location that LOSES you the game. (if possible) |
| 2 | 3 | Master Mold | On Reveal: Add 2 Sentinel to your opponent's hand. |
| 2 | 6 | Maximus | On Reveal: Your opponent draws 2 cards. |
| 4 | 6 | Miles Morales Spider-Man | If a card moved last turn, this costs 1. |
| 2 | 3 | Mirage | On Reveal: Copy the character that costs the least from your opponent's hand into your hand. Give it +2 Power. |
| 4 | -1 | Mister Negative | On Reveal: Swap the Power and Cost of all cards in your deck. |
| 3 | 3 | Mobius M. Mobius | Ongoing: Your Costs can't be increased. Your opponent's Costs can't be reduced. |
| 2 | 2 | Mojo | Ongoing: If both sides here are full, +6 Power. |
| 3 | 3 | Moon Knight | On Reveal: Discard an even-costed card from each player's hand. |
| 2 | 5 | Mysterio | On Reveal: Add an Illusion to another location. Activate: Swap Power with it. |
| 3 | 1 | Mystique | On Reveal: If the last card you played has an Ongoing, copy its text. (if able) |
| 1 | 1 | Nebula | Each turn your opponent doesn't play a card here, +2 Power. (except the turn you play this) |
| 4 | 4 | Negasonic Teenage Warhead | After an enemy character is played here, destroy it. (once per game) |
| 4 | 6 | Nick Fury | On Reveal: Add 3 random 6-Cost cards to your hand. |
| 5 | 6 | Nimrod | When Destroyed: Add a copy of this to each other location. |
| 4 | 6 | Omega Red | Ongoing: If you're winning here, +3 Power to other locations. |
| 6 | 11 | Orka | Ongoing: +7 Power if this is your only card here. |
| 3 | 2 | Patriot | Ongoing: Your cards with no abilities have +2 Power. |
| 3 | 5 | Polaris | On Reveal: Move an enemy 1 or 2-Cost card to this location. |
| 2 | 2 | Psylocke | On Reveal: Next turn, you get +1 Energy. |
| 2 | 3 | Quake | On Reveal: Swap the positions of the other two locations. |
| 1 | 1 | Quinjet | Ongoing: Your created cards cost 1 less. (minimum 1) |
| 5 | 14 | Red Skull | Ongoing: Enemy cards here have +2 Power. |
| 4 | 4 | Rescue | On Reveal: If you play a card at this location next turn, +5 Power. |
| 4 | 5 | Rockslide | On Reveal: Shuffle 2 Rocks into your opponent's deck. |
| 3 | 3 | Rogue | On Reveal: Steal the text from an enemy Ongoing card here. |
| 5 | 5 | Ronan the Accuser | Ongoing: +2 Power for each card in your opponent's hand. |
| 1 | 2 | Sauron | On Reveal: Remove the abilities from all Ongoing cards in your deck. |
| 1 | 2 | Selene | On Reveal: Afflict the lowest-Power card in each player's hand with -3 Power. |
| 4 | 9 | Sentry | You can't play this at the right location. On Reveal: Add a -9 Power Void to the right location. |
| 5 | 5 | Sera | Ongoing: Cards in your hand cost 1 less. (minimum 1) |
| 3 | 4 | Shadow King | On Reveal: Reset all cards here to their original Power. |
| 3 | 4 | Shanna | On Reveal: Add a random 1-Cost character to each location. |
| 6 | 10 | She-Hulk | Costs 1 less for each unspent Energy last turn. |
| 4 | 3 | Shuri | On Reveal: If you play your next card here, double its Power. |
| 2 | 5 | Silk | After ANY card is played here, move this to another location. |
| 4 | 6 | Silver Samurai | On Reveal: Each player discards the lowest-Power character from their hand. |
| 3 | 2 | Silver Surfer | On Reveal: Give your other 3-Cost cards +2 Power. |
| 1 | 2 | Snowguard | On Reveal: Add the Hawk and Bear auroras to your hand. |
| 1 | 1 | Spider-Ham | Activate: Replace the text of a random character in your opponent's hand with "oink!" |
| 2 | 4 | Spider-Man | On Reveal: Move to another location and pull an enemy card from here to there. |
| 4 | 5 | Spider-Man 2099 | Moveable once. When this moves to a location, afflict an enemy card there with -3 Power. |
| 5 | 7 | Stature | Costs 1 if your opponent discarded a card this game. |
| 4 | 7 | Stegron | On Reveal: Move an enemy card here one location to the right. |
| 4 | 2 | Super-Skrull | Ongoing: Has the Ongoing effects of all enemy cards. |
| 6 | 0 | Taskmaster | On Reveal: Set this card's Power equal to that of the last card you played. (if able) |
| 1 | -3 | The Hood | On Reveal: Add a Demon to your hand. |
| 3 | 5 | Thor | On Reveal: Shuffle Mjolnir into your deck. |
| 1 | 5 | Titania | After ANY card is played here, this card switches sides. |
| 4 | 10 | Typhoid Mary | Ongoing: Your other cards here have -2 Power. |
| 6 | 8 | Ultron | On Reveal: Deploy four 2-Power Drones to each other location. |
| 5 | 3 | Valkyrie | On Reveal: Set ALL cards here to 3 Power. |
| 3 | 3 | Venom | On Reveal: Destroy your other cards here. Add their Power to this card. |
| 3 | 4 | Viper | On Reveal: One of your other cards here with the lowest Power switches sides. |
| 0 | 1 | Wasp | "My secret power is that I get things done." |
| 3 | 6 | Wave | On Reveal: All cards cost a maximum of 4 until the end of next turn. |
| 4 | 2 | Wong | Ongoing: Your On Reveal abilities here repeat. |
| 0 | 2 | Yellowjacket | On Reveal: Afflict your other cards here with -1 Power. |
| 1 | 1 | Zabu | On Reveal: Give each 4-Cost card in your deck -1 Cost. |
| 1 | 3 | Zero | On Reveal: Remove the abilities on the next card you play. |

#### Scenario: Series 3 cards are registered

- **WHEN** the card pool is loaded
- **THEN** each of the 125 cards SHALL exist with the listed cost and base power
- **AND** each card SHALL have a non-empty description
- **AND** no two cards SHALL share a name or an ID
- **AND** every card SHALL declare `CardSeries.Series3`

#### Scenario: Series 3 descriptions carry the ability

- **WHEN** a Series 3 card's description is read
- **THEN** it SHALL describe the ability in Spanish without accents
- **AND** On Reveal text SHALL start with `Al revelarse:`
- **AND** Ongoing text SHALL start with `Continuo:`
- **AND** End of Turn text SHALL start with `Fin de turno:`
- **AND** Activate text SHALL start with `Activar:`
- **AND** destroy, discard, move and play-restriction text SHALL use a consistent Spanish prefix

#### Scenario: Series 3 cards are catalog content

- **WHEN** a new profile is created
- **THEN** the 125 Series 3 cards SHALL NOT be part of the starting owned set
- **AND** they SHALL be registered as draftable catalog cards
- **AND** they SHALL be obtainable through the global store

#### Scenario: Corrected values are canonical

- **WHEN** the Series 3 stats are validated
- **THEN** Death SHALL be 8/12, Giganto 6/16, Goose 2/2, Havok 2/1, Green Goblin 3/-4, Brood 3/2, Colleen Wing 2/3, Doctor Octopus 4/8, Dracula 4/1, Drax 4/5, Ghost Rider 4/5, Ghost-Spider 1/3, Hazmat 3/3, Hela 6/6, Hellcow 4/7, M.O.D.O.K. 5/7, Mirage 2/3, Mister Negative 4/-1, Mobius M. Mobius 3/3, Moon Knight 3/3, Mystique 3/1, Rescue 4/4 and Sauron 1/2
- **AND** the remaining 102 Series 3 cards SHALL keep the values in the table above

### Requirement: Series 3 Tokens

The system SHALL register the tokens created by the Series 3 cards.

#### Scenario: Token roster

- **WHEN** the content is loaded
- **THEN** `token_widows_bite` SHALL be 0/-1 with the hand draw lock
- **AND** `token_broodling` SHALL be 0/2 with no ability
- **AND** `token_doombot` SHALL be 0/5 with no ability
- **AND** `token_drone` SHALL be 0/2 with no ability
- **AND** `token_illusion` SHALL be 0/0 with no ability
- **AND** `token_demon` SHALL be 0/6 with no ability
- **AND** `token_mjolnir` SHALL be 0/0 and give Thor +7 Power on reveal
- **AND** `token_snowguard_hawk` SHALL be 3/3 and suppress zone effects until the end of the next turn
- **AND** `token_snowguard_bear` SHALL be 3/4 and repeat the current zone effect on reveal
- **AND** `token_void` SHALL be 0/-9 with no ability
- **AND** Master Mold SHALL reuse the existing `card_sentinel` definition

#### Scenario: Tokens are not draftable

- **WHEN** the draft or reward pool is built
- **THEN** no Series 3 token SHALL be offered
- **AND** no Series 3 token SHALL be part of the 217 draftable cards
- **AND** Hawk and Bear SHALL keep their 3 cost as an intentional exception to the 0-cost token convention

#### Scenario: Token creation

- **WHEN** a Series 3 card creates a token
- **THEN** the token SHALL be a new independent instance
- **AND** it SHALL use the same card frame and art pipeline as any other card
- **AND** it SHALL enter the correct hand, zone or pile
