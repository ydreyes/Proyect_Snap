# Spec Delta

## ADDED Requirements

### Requirement: Destruction and Discard Effects

The system SHALL support card effects that destroy, discard, protect and replace cards, including the reactions those actions trigger.

#### Scenario: Destructive OnReveal

- **WHEN** Carnage is revealed
- **THEN** it SHALL destroy the other allied cards in its location
- **AND** it SHALL gain +2 persistent Power for each card destroyed
- **AND** cards protected from destruction SHALL NOT be destroyed or counted

#### Scenario: Deathlok and Elektra

- **WHEN** Deathlok is revealed
- **THEN** it SHALL destroy the other allied cards in its location
- **AND** when Elektra is revealed
- **THEN** she SHALL destroy a random enemy 1-Cost card in her location

#### Scenario: Discard selection

- **WHEN** Blade is revealed
- **THEN** it SHALL discard the rightmost card in its owner's hand
- **AND** when Lady Sif is revealed
- **THEN** she SHALL discard the highest-Cost card in hand, choosing randomly among ties
- **AND** when Sword Master is revealed
- **THEN** he SHALL discard a random odd-Costed card in hand

#### Scenario: Discard replacement

- **WHEN** Apocalypse is discarded
- **THEN** it SHALL return to its owner's hand with +4 persistent Power
- **AND** it SHALL NOT enter the discarded pile
- **AND** when Wolverine is discarded or destroyed
- **THEN** it SHALL regenerate with +2 persistent Power at a random location with room
- **AND** it SHALL NOT remain in the pile

#### Scenario: Destruction reactions

- **WHEN** one of your cards is destroyed
- **THEN** Angel SHALL fly from your hand or deck to the destroyed card's location if it has room
- **AND** when Nova is destroyed
- **THEN** it SHALL give your other cards +1 persistent Power
- **AND** when an effect prevents the destruction
- **THEN** no destruction reaction SHALL fire

#### Scenario: Armor protection

- **WHEN** Armor is in a location
- **THEN** cards in that location SHALL NOT be destroyed
- **AND** a destroy attempt SHALL leave the board, piles and energy unchanged
- **AND** if Armor's ability is removed, protection SHALL stop

#### Scenario: Affliction

- **WHEN** Spider-Woman is revealed
- **THEN** each enemy card in her location SHALL gain -1 persistent Power
- **AND** the affliction SHALL follow the card if it moves
- **AND** it SHALL NOT affect enemy cards that enter the location later

### Requirement: Movement Effects

The system SHALL support moving cards between locations, batch movement, next-play movement and movement reactions.

#### Scenario: Doctor Strange

- **WHEN** Doctor Strange is revealed
- **THEN** the highest-Power allied card(s) from other locations SHALL move to his location if there is room
- **AND** ties SHALL move every tied highest-Power card
- **AND** cards that cannot fit SHALL stay in their location

#### Scenario: Heimdall

- **WHEN** Heimdall is revealed
- **THEN** each other allied card SHALL move one location to the left
- **AND** the movement SHALL resolve as one batch
- **AND** a card SHALL NOT move if its destination would exceed the four-card limit
- **AND** cards already in the leftmost location SHALL stay

#### Scenario: Iron Fist

- **WHEN** Iron Fist is revealed
- **THEN** the next card its owner plays SHALL move one location to the left after it is played
- **AND** the pending move SHALL persist across turns until the next play
- **AND** it SHALL be consumed even if the destination has no room

#### Scenario: Movement reactions

- **WHEN** a card moves into Kraven's location
- **THEN** Kraven SHALL gain +2 persistent Power
- **AND** when Multiple Man moves
- **THEN** a copy of Multiple Man SHALL be added to the old location if it has room
- **AND** a moved card SHALL keep its recorded played zone

### Requirement: Play-Triggered and Next-Play Effects

The system SHALL support effects that react to a card being played and effects that modify the next card played.

#### Scenario: Angela

- **WHEN** an allied card is played in Angela's location
- **THEN** Angela SHALL gain +1 persistent Power
- **AND** the bonus SHALL apply once per card play, not once per replay of the same instance in the same turn
- **AND** it SHALL survive Angela moving

#### Scenario: Bishop

- **WHEN** an allied card is played
- **THEN** Bishop SHALL gain +1 persistent Power
- **AND** the bonus SHALL be derived from the number of qualifying plays
- **AND** it SHALL NOT double-count a replay of the same instance in the same turn

#### Scenario: Forge

- **WHEN** Forge is revealed
- **THEN** the next card its owner plays SHALL gain +2 persistent Power
- **AND** the pending bonus SHALL persist across turns until the next play
- **AND** multiple Forge effects SHALL stack

### Requirement: Continuous Auras, Additions and Repetition

The system SHALL support continuous effects that target other cards, other locations, and repeat other continuous effects.

#### Scenario: Captain America

- **WHEN** Captain America is in a location
- **THEN** each other allied Ongoing card in that location SHALL gain +2 Power
- **AND** Captain America SHALL NOT buff itself
- **AND** two Captain Americas SHALL buff each other

#### Scenario: Klaw

- **WHEN** Klaw is in a location
- **THEN** its owner's side of the location to the right SHALL gain +6 Power
- **AND** the bonus SHALL be additive to the zone total, not to individual card power
- **AND** Klaw in the rightmost location SHALL have no target

#### Scenario: Onslaught

- **WHEN** Onslaught is in a location
- **THEN** each other Ongoing effect in that location SHALL apply one additional time
- **AND** the repetition SHALL affect auras, multipliers and additive zone bonuses
- **AND** Onslaught SHALL NOT repeat itself
- **AND** multiple Onslaughts SHALL use a bounded, deterministic repetition rule

#### Scenario: Conditional Ongoing

- **WHEN** Devil Dinosaur is in play
- **THEN** it SHALL have +2 Power for each card in its owner's hand
- **AND** when Lizard is in play
- **THEN** it SHALL have -4 Power if the opponent has 4 cards in its location
- **AND** when Namor is in play
- **THEN** it SHALL have +5 Power if it is its side's only card in the location
- **AND** when Strong Guy is in play
- **THEN** it SHALL have +6 Power if its owner's hand has 1 or fewer cards

#### Scenario: Opponent-played-here

- **WHEN** Star-Lord, Groot or Rocket Raccoon is revealed
- **AND** the opponent played at least one card in that location this turn
- **THEN** the card SHALL gain +4 persistent Power
- **AND** the check SHALL use the play log, so a card that later moved or was destroyed still counts

### Requirement: Suppression and Ability Removal

The system SHALL support effects that suppress OnReveal abilities and effects that remove abilities permanently for the combat.

#### Scenario: Cosmo

- **WHEN** Cosmo is in a location
- **THEN** cards revealed in that location after Cosmo SHALL NOT apply OnReveal effects
- **AND** cards that already revealed SHALL keep their resolved effects
- **AND** if Cosmo's ability is removed, later OnReveal effects SHALL work again

#### Scenario: Enchantress

- **WHEN** Enchantress is revealed
- **THEN** all Ongoing cards in her location SHALL lose their abilities for the rest of the combat
- **AND** their Ongoing contributions SHALL stop immediately
- **AND** the disabled state SHALL clear at the start of the next combat

### Requirement: Copy, Transform, Merge and Tokens

The system SHALL support copying cards, transforming a card into another definition, merging cards and creating tokens.

#### Scenario: Copy from hand or zone

- **WHEN** Moon Girl is revealed
- **THEN** a copy of each card in her owner's hand SHALL be added to that hand while it has room
- **AND** when White Queen is revealed
- **THEN** a copy of the highest-Cost card in the opponent's hand SHALL be added to her owner's hand, choosing randomly among ties
- **AND** when Mantis is revealed
- **THEN** a copy of a random card the opponent played in her location this turn SHALL be added to her owner's hand
- **AND** a copy SHALL be a separate instance

#### Scenario: Morph

- **WHEN** Morph is revealed
- **THEN** it SHALL become a copy of a random card in the opponent's hand
- **AND** its definition, power and abilities SHALL become that card's
- **AND** if the opponent's hand is empty, Morph SHALL remain a 3/0 card

#### Scenario: Hulkbuster merge

- **WHEN** Hulkbuster is revealed
- **THEN** it SHALL merge into a random other allied card in its location
- **AND** the host SHALL gain Hulkbuster's current Power as persistent Power
- **AND** Hulkbuster SHALL leave the board without entering a pile
- **AND** the merge SHALL NOT trigger destroy, discard or banish reactions
- **AND** if there is no other allied card, Hulkbuster SHALL remain on the board

#### Scenario: Token creation

- **WHEN** Mister Sinister is revealed
- **THEN** a Sinister Clone with the same Power SHALL be added to his location if it has room
- **AND** when Squirrel Girl is revealed
- **THEN** a 1-Power Squirrel SHALL be added to each other location if it has room
- **AND** tokens SHALL be separate instances and SHALL NOT be draftable

### Requirement: Deck and Hand Effects

The system SHALL support effects that manipulate the top of the deck, the opponent's deck, shuffling and banishment, plus the turn-2 draw override.

#### Scenario: America Chavez

- **WHEN** America Chavez is revealed
- **THEN** the top card of her owner's deck SHALL gain +3 persistent Power
- **AND** if the deck is empty, nothing SHALL happen

#### Scenario: Cable

- **WHEN** Cable is revealed
- **THEN** a card SHALL be drawn from the opponent's deck into Cable's owner's hand if the hand has room
- **AND** if the hand is full or the deck is empty, nothing SHALL move

#### Scenario: Korg and Yondu

- **WHEN** Korg is revealed
- **THEN** a Rock token SHALL be shuffled into the opponent's deck
- **AND** when Yondu is revealed
- **THEN** the lowest-Cost card in the opponent's deck SHALL be banished for the combat, choosing randomly among ties
- **AND** the card SHALL return to the run deck for the next combat

#### Scenario: Domino

- **WHEN** Domino is in a deck
- **THEN** she SHALL be excluded from the opening hand
- **AND** she SHALL be drawn on turn 2 if she is still in the deck
- **AND** she SHALL NOT be drawn before turn 2

### Requirement: Game-Start and Visibility Effects

The system SHALL support effects that resolve at the start of a combat and reveal hidden location information to their owner.

#### Scenario: Uatu

- **WHEN** a combat starts
- **AND** Uatu the Watcher is in the owner's deck or hand
- **THEN** that side SHALL see the name, effect and art of unrevealed locations
- **AND** the visibility SHALL NOT change the real reveal order
- **AND** the opponent SHALL NOT gain that visibility

#### Scenario: Visibility is informational

- **WHEN** Uatu's owner views an unrevealed location
- **THEN** the location SHALL still be treated as unrevealed for reveal timing and effects
- **AND** only the display SHALL show the hidden information
