# effect-system Specification

## Purpose
The effect system provides a pluggable architecture for card and zone effects supporting multiple trigger types with registry/factory patterns to ensure extensibility without core system changes.

## Requirements

### Requirement: Effect Trigger Types

The system SHALL implement `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand`, `GameStart`, `StartOfTurn`, `EndOfTurn`, `Activate` and `EndOfGame` as active trigger families. Card and perk abilities SHALL use the same trigger families.

#### Scenario: Different trigger types

- **WHEN** effects are defined
- **THEN** each effect SHALL be associated with one of the supported trigger types
- **AND** `OnReveal` effects SHALL fire when their card is revealed
- **AND** `Ongoing` effects SHALL stay active while their card is in play
- **AND** `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand` and `GameStart` effects SHALL fire only when the matching action or combat event occurs
- **AND** `StartOfTurn` effects SHALL fire at the start of a turn
- **AND** `EndOfTurn` effects SHALL fire when a turn ends
- **AND** `Activate` effects SHALL fire when the owner activates the card and SHALL NOT fire on their own
- **AND** `EndOfGame` effects SHALL fire after the final turn ends and before victory is evaluated

#### Scenario: OnReveal fires once

- **WHEN** a card or perk with an `OnReveal` effect is revealed at the end of a turn
- **THEN** its `OnReveal` effect SHALL be applied exactly once
- **AND** it SHALL NOT be applied again on later turns
- **AND** it SHALL NOT be applied again if the card returns to hand and is played again during the same combat

#### Scenario: Ongoing is continuously active

- **WHEN** a card or perk with an `Ongoing` effect is on the board
- **THEN** its effect SHALL contribute to every power calculation while the card remains in play
- **AND** it SHALL NOT be applied as a one-time mutation

#### Scenario: EndOfTurn fires

- **WHEN** a turn ends
- **THEN** every active `EndOfTurn` effect SHALL resolve before priority, victory and the next turn are evaluated
- **AND** an `EndOfTurn` effect SHALL fire once per instance and turn

#### Scenario: StartOfTurn fires

- **WHEN** a turn starts
- **THEN** every active `StartOfTurn` effect SHALL resolve before the player's play phase begins
- **AND** any energy granted by it SHALL be available for that turn only

#### Scenario: Activate fires

- **WHEN** a player activates a card with an unused `Activate` effect during the play phase
- **THEN** the `Activate` effect SHALL be applied once
- **AND** the card SHALL NOT be activated again during the same combat
- **AND** the activation SHALL NOT create a play record or an `AfterPlayed` trigger

#### Scenario: EndOfGame fires

- **WHEN** the final turn ends
- **THEN** every active `EndOfGame` effect SHALL resolve before victory is evaluated
- **AND** delayed reveals SHALL resolve before `EndOfGame` effects
- **AND** no further turn SHALL start

#### Scenario: Inactive triggers do not fire

- **WHEN** a turn ends or a player activates a card
- **THEN** `EndOfTurn` effects SHALL resolve
- **AND** `Activate` effects SHALL fire only when the player explicitly activates the card
- **AND** no other trigger SHALL fire without its matching action or phase

#### Scenario: Action triggers do not fire without content

- **WHEN** a card is played, moved, destroyed, discarded, drawn or added to hand
- **AND** no card or perk references the matching action trigger
- **THEN** no effect SHALL be applied
- **AND** the action SHALL complete normally

#### Scenario: Action triggers fire with content

- **WHEN** a card or perk references an action trigger
- **AND** the matching play, move, destroy, discard, draw or hand addition occurs
- **THEN** the effect SHALL fire in the same resolution
- **AND** the action SHALL complete normally

#### Scenario: Added-to-hand fires

- **WHEN** a card enters its owner's hand because of an effect or because it was taken from the opponent's deck
- **THEN** every `AfterAddedToHand` effect of the owner SHALL fire
- **AND** a normal draw from the owner's own deck SHALL NOT fire it
- **AND** a manual board-to-hand return by the player SHALL NOT fire it

### Requirement: Pluggable Effect Architecture
The system SHALL allow new effects to be added without modifying core systems.

#### Scenario: Adding new effect
- **WHEN** a new effect type needs to be added
- **THEN** it SHALL be implemented as a class implementing the effect interface
- **AND** it SHALL be registered in the effect registry
- **AND** no changes to the effect processor core SHALL be required

### Requirement: Effect Registry
The system SHALL maintain a registry of all available effects.

#### Scenario: Effect lookup
- **WHEN** an effect needs to be applied
- **THEN** the system SHALL look up the effect by ID in the registry
- **AND** the correct effect implementation SHALL be instantiated

### Requirement: Effect Processor

The system SHALL process card effects and attached perk effects according to their trigger types at the appropriate times.

#### Scenario: OnReveal processing

- **WHEN** a turn ends
- **THEN** the system SHALL compute the reveal order of cards played this turn
- **AND** it SHALL apply each card's `OnReveal` effects and every `OnReveal` ability granted by its attached perks in that order
- **AND** each card's effect SHALL resolve completely before the next card's effect
- **AND** the resolution SHALL complete before priority, victory and the next turn are evaluated

#### Scenario: Cards that left play do not reveal

- **WHEN** a card is destroyed, discarded, returned to hand or otherwise leaves play before its reveal
- **THEN** its `OnReveal` effect and any `OnReveal` perk ability SHALL NOT be applied
- **AND** the reveal sequence SHALL continue with the next card

#### Scenario: Already-revealed cards do not reveal again

- **WHEN** a card has already revealed during the combat
- **THEN** it SHALL NOT be included in a later reveal sequence
- **AND** its `OnReveal` effects and perk abilities SHALL NOT trigger again

#### Scenario: StartOfTurn processing

- **WHEN** a turn starts
- **THEN** the processor SHALL resolve every active `StartOfTurn` effect before the play phase
- **AND** it SHALL evaluate energy conditions against the board as it stands at that moment

#### Scenario: EndOfTurn processing

- **WHEN** a turn ends
- **THEN** the processor SHALL resolve every active `EndOfTurn` effect after the reveal sequence
- **AND** it SHALL complete before priority and victory are evaluated

#### Scenario: Activate processing

- **WHEN** a player activates a card's ability
- **THEN** the processor SHALL apply the card's `Activate` effects once
- **AND** the action authority SHALL reject a second activation in the same combat
- **AND** the activation SHALL change the board only through the effect's declared actions

#### Scenario: EndOfGame processing

- **WHEN** the final turn ends
- **THEN** the processor SHALL reveal delayed cards and resolve their `OnReveal` effects
- **AND** it SHALL resolve every active `EndOfGame` effect in a deterministic side, zone and slot order
- **AND** it SHALL recompute powers after each effect
- **AND** it SHALL complete before victory is evaluated

#### Scenario: Ongoing processing

- **WHEN** card or zone power is recalculated
- **THEN** the system SHALL query the `Ongoing` effects of every card and attached perk in the zone
- **AND** it SHALL NOT write the result into the card's current power as an accumulated value

#### Scenario: No effect content

- **WHEN** a card has no effects and no attached perks
- **THEN** the processor SHALL leave its power and zone unchanged

### Requirement: Power Resolution

The system SHALL calculate card power dynamically from base power, perks, persistent bonuses, set/reset overrides and active Ongoing modifiers.

#### Scenario: Card power composition

- **WHEN** a card's power is calculated
- **THEN** it SHALL equal the card's effective base power plus perk power plus persistent bonus power plus Ongoing bonuses, then multiplied by active power multipliers
- **AND** an explicit set-power override SHALL replace the definition's base power
- **AND** it SHALL NOT read the card's previous current power as an input

#### Scenario: Set, reset and double

- **WHEN** Bast, Valkyrie, Shadow King, Black Panther, Shuri or Taskmaster resolves
- **THEN** the resolver SHALL apply the set, reset or double step exactly once
- **AND** a set or reset SHALL clear the card's persistent bonuses
- **AND** current Ongoing auras SHALL still apply after a set or reset
- **AND** a double SHALL snapshot the resolved power and add the difference as a persistent bonus

#### Scenario: Ongoing modifier application

- **WHEN** Ongoing effects or attached perks are active
- **THEN** the power resolver SHALL compute card power from those sources on every recalculation
- **AND** power SHALL be recalculated whenever board state changes
- **AND** modifiers from zones, cards and perks SHALL all be considered

#### Scenario: No accumulation across evaluations

- **WHEN** power is recalculated more than once for the same card
- **THEN** the result SHALL be identical
- **AND** modifiers SHALL NOT stack on top of a previously calculated value

#### Scenario: Zone multiplier

- **WHEN** a zone total is calculated
- **THEN** it SHALL be the sum of the card powers in that zone multiplied by the zone's Ongoing multipliers
- **AND** the multiplier SHALL NOT change the individual card powers

### Requirement: Effect Context

The system SHALL provide every effect with the context it needs to resolve without reaching into global state.

#### Scenario: Context contents

- **WHEN** an effect is applied or queried
- **THEN** the context SHALL include the source card, its zone, the owner side, the board state, the current turn and the card registry
- **AND** the effect SHALL use this context to find targets or create cards

#### Scenario: Action event context

- **WHEN** an effect is applied because of a play, move, destroy, discard or draw
- **THEN** the context SHALL include the event kind, the triggering card, the origin zone, the destination zone and the play log
- **AND** the effect SHALL be able to resolve against the event payload without reading global state

#### Scenario: Card creation

- **WHEN** an `OnReveal` effect creates a card, such as Sentinel
- **THEN** the new card SHALL be created from the card registry and added to the owner's hand
- **AND** it SHALL be a new instance, independent of the source card

### Requirement: Delayed OnReveal Conditions
The system SHALL support an OnReveal ability whose bonus depends on a play made on the following turn.

#### Scenario: Condition met
- **WHEN** a card with a delayed condition is revealed on turn N
- **AND** an allied card is played in the zone where it was played during turn N + 1
- **THEN** the source card SHALL gain its persistent bonus once at the end of turn N + 1
- **AND** the pending condition SHALL be cleared

#### Scenario: Condition not met
- **WHEN** a card with a delayed condition is revealed on turn N
- **AND** no allied card is played in the zone where it was played during turn N + 1
- **THEN** the source card SHALL NOT gain the bonus
- **AND** the pending condition SHALL be cleared

#### Scenario: Movement does not change the watched zone
- **WHEN** the source card or the card played on the following turn changes zone during that turn
- **THEN** the condition SHALL be evaluated against the zone where each card was played
- **AND** it SHALL NOT use the zone where either card currently sits

### Requirement: Action Event Dispatch

The system SHALL dispatch action events in a deterministic order and resolve every triggered effect before returning control to the action caller.

#### Scenario: Play event

- **WHEN** a card is played from hand
- **THEN** an `AfterPlayed` event SHALL be dispatched once for that card during the turn
- **AND** returning the card to hand and playing it again in the same turn SHALL NOT dispatch a second `AfterPlayed` event for the same instance
- **AND** the event SHALL include the side, the zone, the play order and the card

#### Scenario: Move event

- **WHEN** a card moves between zones
- **THEN** an `AfterMoved` event SHALL be dispatched
- **AND** it SHALL include the origin zone and the destination zone
- **AND** the card's recorded played zone SHALL remain unchanged

#### Scenario: Destroy event

- **WHEN** a destroy succeeds
- **THEN** an `AfterDestroyed` event SHALL be dispatched with the card and the zone it left
- **AND** when a destroy is prevented by an effect, no `AfterDestroyed` event SHALL be dispatched

#### Scenario: Discard event

- **WHEN** a card is discarded from the hand or deck
- **THEN** an `AfterDiscarded` event SHALL be dispatched with the card and its origin
- **AND** when a discard is prevented or replaced, the event SHALL reflect that outcome

#### Scenario: Draw event

- **WHEN** a card is drawn from a deck
- **THEN** an `AfterDrawn` event SHALL be dispatched with the side and the card

#### Scenario: Game start event

- **WHEN** a combat starts
- **THEN** `GameStart` events SHALL be dispatched before the first turn begins

#### Scenario: Deterministic order

- **WHEN** the same board state and action sequence are resolved more than once
- **THEN** the events SHALL be dispatched in the same order every time
- **AND** the order SHALL NOT depend on list iteration order or on which side initiated the action

#### Scenario: Loop guard

- **WHEN** resolving an action emits a cascade of events
- **THEN** the dispatcher SHALL cap the number of events resolved for that action
- **AND** it SHALL stop the cascade and report the limit instead of looping forever

### Requirement: Effect Implementation Coverage

The system SHALL verify that every card-referenced effect resolves to an implementation. Zone effect definitions that are explicitly declared unimplemented SHALL be exempt from this check.

#### Scenario: Every card effect reference resolves

- **WHEN** content is loaded
- **THEN** every effect id referenced by a card SHALL resolve to a registered effect
- **AND** the referenced effect's type SHALL map to an implementation
- **AND** a missing effect id or implementation SHALL be reported as a validation failure

#### Scenario: Zone effects may remain declared

- **WHEN** a zone effect definition is registered without an implementation
- **THEN** it SHALL NOT fail card-effect validation
- **AND** the zone SHALL keep referencing it
- **AND** the lack of implementation SHALL be an explicit, documented state rather than a silent surprise

#### Scenario: Missing implementation is not silent

- **WHEN** a card references an effect that has no implementation
- **THEN** the system SHALL NOT silently ignore it during play
- **AND** the failure SHALL be visible before a combat starts

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

### Requirement: Perk Effect Dispatch

The system SHALL resolve attached perk effect ids through the effect processor.

#### Scenario: Perk effects

- **WHEN** a perk with effect ids is attached to a card
- **THEN** the processor SHALL treat those effects as abilities of the host card
- **AND** it SHALL resolve every id on the matching trigger
- **AND** an unknown id SHALL fail content validation rather than being ignored
- **AND** each attached copy SHALL resolve once

#### Scenario: Host scope

- **WHEN** a perk ability runs
- **THEN** it SHALL use the host card's owner, zone and location
- **AND** it SHALL NOT affect the whole side as a global rule
- **AND** a perk that requires the host to be in play SHALL NOT resolve from hand or deck

#### Scenario: Global rule scope

- **WHEN** a perk effect runs
- **THEN** it SHALL act through the host card and SHALL NOT use a side-wide perk rule system
- **AND** the same effect SHALL apply once per attached copy
- **AND** it SHALL NOT require the host to be in play when the ability text works from hand or deck

#### Scenario: Hand and deck abilities

- **WHEN** a perk's text allows it to work from hand or deck
- **THEN** the processor SHALL include the host in the matching action reactor order
- **AND** the effect SHALL use the host's actual location for its checks

#### Scenario: Disabled and suppressed

- **WHEN** the host's abilities are disabled
- **THEN** its perk effects SHALL NOT run
- **AND** when an On Reveal granted by a perk resolves in a suppressed zone
- **THEN** that On Reveal SHALL NOT apply

#### Scenario: No double application

- **WHEN** a card's power, cost or energy is recalculated multiple times in a turn
- **THEN** the perk ability SHALL produce the same result
- **AND** it SHALL NOT accumulate into the host's stored power or cost

### Requirement: Turn Boundary Ability Effects

The system SHALL resolve abilities that depend on the start or end of a turn.

#### Scenario: Overload

- **WHEN** a turn starts and a host with Overload is in play
- **THEN** if its owner is winning the host's zone, the owner SHALL gain +1 Energy for that turn
- **AND** the bonus SHALL NOT carry to a later turn

#### Scenario: Overcharge

- **WHEN** a host with Overcharge is revealed
- **THEN** at the start of the next turn the system SHALL check whether the owner is winning the host's zone
- **AND** if so, the owner SHALL gain +1 Energy for that turn
- **AND** the pending condition SHALL be cleared after the check

#### Scenario: Energy timing

- **WHEN** a Start of Turn or delayed condition grants Energy
- **THEN** the Energy SHALL be available before affordability checks for that turn
- **AND** the energy indicator SHALL update in the same frame

### Requirement: Repeat-Own-Ability Hooks

The system SHALL support perks that repeat another ability of their host.

#### Scenario: Amplify

- **WHEN** a host with Amplify resolves an Ongoing effect
- **THEN** that effect SHALL apply one additional time per attached copy
- **AND** Amplify SHALL NOT repeat itself
- **AND** the repetition SHALL be deterministic and bounded by the copy limit

#### Scenario: Encore

- **WHEN** a host with Encore resolves its On Reveal effects
- **THEN** they SHALL resolve one additional time per attached copy
- **AND** the reveal marker SHALL NOT be cleared
- **AND** the repetition SHALL be bounded by the copy limit and the event limit

#### Scenario: Overtime

- **WHEN** a host with Overtime resolves its End of Turn effects
- **THEN** they SHALL resolve one additional time per attached copy
- **AND** the repetition SHALL be bounded by the copy limit

#### Scenario: No recursion

- **WHEN** a repeat hook is itself repeated
- **THEN** the system SHALL NOT recurse into the same hook
- **AND** the event limit SHALL stop the cascade and report the limit

### Requirement: Empowered Event

The system SHALL dispatch an Empowered event when another effect increases a card's Power.

#### Scenario: Empowered dispatch

- **WHEN** an effect other than Reinforce increases a card's Power
- **THEN** an Empowered event SHALL be dispatched once for that card
- **AND** the event SHALL include the card, its location and the source effect

#### Scenario: Reinforce reaction

- **WHEN** an Empowered event reaches a card with Reinforce
- **AND** that card is in hand, in deck or in play
- **THEN** it SHALL gain +1 persistent Power per attached Reinforce copy

#### Scenario: No self-trigger

- **WHEN** Reinforce grants its own +1 Power
- **THEN** that increase SHALL NOT dispatch another Empowered event
- **AND** the event limit SHALL bound any cascade

### Requirement: Series 2 On Reveal Effects

The system SHALL resolve the On Reveal abilities of the Series 2 and Collection Level cards according to their printed text.

#### Scenario: Spectrum buffs Ongoing cards

- **WHEN** Spectrum is revealed
- **THEN** each allied card in play with at least one active Ongoing effect SHALL gain +2 persistent Power
- **AND** cards without an Ongoing effect SHALL NOT gain Power
- **AND** Spectrum SHALL NOT buff itself

#### Scenario: Wolfsbane scales with allies in the zone

- **WHEN** Wolfsbane is revealed
- **THEN** it SHALL gain +2 persistent Power for each other allied card in its location
- **AND** the count SHALL be taken when the effect resolves
- **AND** it SHALL NOT destroy or move those cards

#### Scenario: White Tiger adds a Tiger

- **WHEN** White Tiger is revealed
- **THEN** a Tiger token with 10 base Power SHALL be added to a random other location with room
- **AND** no Tiger SHALL be added if every other location is full
- **AND** the token SHALL be a separate, non-draftable instance

#### Scenario: Agent 13 adds a random card

- **WHEN** Agent 13 is revealed
- **THEN** a random draftable card SHALL be created as a new instance and added to its owner's hand if the hand has room
- **AND** the card SHALL be chosen uniformly at random from the non-token catalog
- **AND** nothing SHALL be created when the hand is full

#### Scenario: Jubilee pulls the top deck card

- **WHEN** Jubilee is revealed
- **THEN** the top card of its owner's deck SHALL be moved to Jubilee's location if that location has room
- **AND** the card SHALL be revealed and its On Reveal ability SHALL resolve
- **AND** the card SHALL NOT be marked as played for play-triggered effects
- **AND** nothing SHALL move when the deck is empty or the location is full

#### Scenario: Iceman raises an opponent's cost

- **WHEN** Iceman is revealed
- **THEN** one random card in the opponent's hand SHALL cost 1 more for the rest of the combat
- **AND** its effective cost SHALL NOT exceed 6
- **AND** cards already at effective cost 6 SHALL NOT be raised

#### Scenario: Scorpion afflicts the opponent's hand

- **WHEN** Scorpion is revealed
- **THEN** every card in the opponent's hand SHALL gain -1 persistent Power
- **AND** the affliction SHALL follow a card if it is later played
- **AND** cards that enter the opponent's hand later SHALL NOT be afflicted

#### Scenario: Nakia and Okoye buff hand and deck

- **WHEN** Nakia is revealed
- **THEN** every card in its owner's hand SHALL gain +1 persistent Power
- **AND** when Okoye is revealed
- **THEN** every card in its owner's deck SHALL gain +1 persistent Power
- **AND** the buffs SHALL apply to the cards present at reveal time

#### Scenario: Rhino adds a Rock

- **WHEN** Rhino is revealed
- **THEN** a Rock token SHALL be added to its owner's side of Rhino's location if that side has room
- **AND** the Rock SHALL be a separate, non-draftable instance

#### Scenario: Leech removes printed text

- **WHEN** Leech is revealed
- **THEN** every opponent hand card with effective cost 6 or more SHALL have its printed abilities disabled for the rest of the combat
- **AND** abilities granted by attached perks SHALL remain active
- **AND** cards that enter the opponent's hand later SHALL NOT be affected

#### Scenario: Killmonger destroys 1-Cost cards

- **WHEN** Killmonger is revealed
- **THEN** every card in play with effective cost 1 SHALL be destroyed, on both sides
- **AND** cards protected from destruction SHALL survive
- **AND** each successful destroy SHALL dispatch its own destruction event
- **AND** the set of targets SHALL be taken when the effect resolves

#### Scenario: Shang-Chi destroys a high-Power enemy

- **WHEN** Shang-Chi is revealed
- **THEN** one random enemy card in his location with 10 or more current Power SHALL be destroyed
- **AND** protected cards SHALL survive
- **AND** nothing SHALL be destroyed when no enemy card meets the threshold

#### Scenario: Jessica Jones watches the next turn

- **WHEN** Jessica Jones is revealed on turn N
- **AND** no allied card is played in that location during turn N + 1
- **THEN** she SHALL gain +5 persistent Power at the end of turn N + 1
- **AND** she SHALL NOT gain it if an allied card was played there
- **AND** the condition SHALL be cleared after it resolves or fails

### Requirement: Series 2 Continuous Effects

The system SHALL resolve the Ongoing abilities of the Series 2 and Collection Level cards as derived contributions, not accumulated writes.

#### Scenario: Ka-Zar buffs 1-Cost cards

- **WHEN** Ka-Zar is in play
- **THEN** each allied card in play with effective cost 1 SHALL have +1 Power
- **AND** the bonus SHALL apply in every power recalculation
- **AND** it SHALL NOT buff enemy 1-Cost cards
- **AND** it SHALL NOT buff cards in hand or deck

#### Scenario: Mister Fantastic buffs adjacent locations

- **WHEN** Mister Fantastic is in play
- **THEN** its owner's side of each adjacent location SHALL gain +3 Power
- **AND** the addition SHALL affect the zone total, not individual card Power
- **AND** a Mister Fantastic in the leftmost or rightmost location SHALL affect only the one adjacent location

#### Scenario: Morbius counts discards

- **WHEN** Morbius is in play
- **THEN** it SHALL have +2 Power for each successful discard performed by its owner in the current combat
- **AND** discards that are later replaced SHALL count
- **AND** the count SHALL reset when a new combat starts
- **AND** the bonus SHALL NOT accumulate into the stored Power

#### Scenario: Warpath checks empty locations

- **WHEN** Warpath is in play
- **THEN** it SHALL have +5 Power if at least one of its owner's locations contains no allied card
- **AND** the condition SHALL be evaluated on every recalculation
- **AND** it SHALL NOT count a location that contains an allied card, including Warpath itself

### Requirement: Odin On Reveal Repetition

The system SHALL repeat the On Reveal abilities of other allied cards at Odin's location after the normal reveal sequence.

#### Scenario: Repeat allied reveals

- **WHEN** Odin is revealed and remains in play after the reveal sequence
- **THEN** each other allied card in his location that revealed during the same turn and is not ability-disabled SHALL resolve its On Reveal effects once more
- **AND** the repeated resolution SHALL use the repeated card as the source
- **AND** Odin SHALL NOT repeat himself
- **AND** Odin SHALL NOT repeat enemy cards

#### Scenario: Repeat does not revisit earlier turns

- **WHEN** Odin remains in play on a later turn
- **THEN** he SHALL NOT repeat cards that revealed on an earlier turn
- **AND** the repeat pass SHALL use the current turn's reveal set or a snapshot taken when the repeater resolved
- **AND** a repeated OnReveal SHALL NOT fire again on later turns

#### Scenario: Repeat respects suppression

- **WHEN** an active Cosmo is in Odin's location
- **THEN** Odin SHALL NOT repeat any On Reveal ability in that location
- **AND** when a repeated card's printed abilities are disabled
- **THEN** only its perk-granted On Reveal abilities SHALL resolve

#### Scenario: Repeat is bounded

- **WHEN** more than one Odin or another repeat hook is present
- **THEN** an Odin SHALL NOT repeat another repeat hook
- **AND** the repetition SHALL be deterministic and bounded by the event limit
- **AND** the reveal marker of the repeated card SHALL remain set

#### Scenario: Repeat interacts with delayed and created effects

- **WHEN** Odin repeats a card that creates a card, arms a delayed condition or adds Power
- **THEN** the repeated effect SHALL resolve again
- **AND** any created token or armed condition SHALL be independent of the first resolution
- **AND** the normal On Reveal resolution SHALL NOT be cancelled or reordered

### Requirement: Hand Entry Effects

The system SHALL resolve effects that react to a card entering its owner's hand.

#### Scenario: The Collector gains Power

- **WHEN** a card enters The Collector's owner's hand from anywhere except that owner's deck
- **THEN** The Collector SHALL gain +1 persistent Power
- **AND** the trigger SHALL apply to cards created by effects, copied to hand, returned by a replacement or taken from the opponent's deck
- **AND** a normal draw from the owner's deck SHALL NOT trigger it
- **AND** a manual board-to-hand return by the player SHALL NOT trigger it

#### Scenario: Multiple entries

- **WHEN** one effect adds several cards to the same hand
- **THEN** The Collector SHALL gain +1 for each card that actually enters the hand
- **AND** additions rejected by the hand cap SHALL NOT trigger it

#### Scenario: No self-trigger

- **WHEN** The Collector is played from hand
- **THEN** its own play SHALL NOT trigger its ability
- **AND** the trigger SHALL only fire for cards entering the hand after it is in play

### Requirement: Series 2 Movement and Play Restrictions

The system SHALL support printed movement permissions, temporary movement windows and play restrictions.

#### Scenario: Nightcrawler moves once

- **WHEN** Nightcrawler is in play and its controller moves it on a later turn
- **THEN** the move SHALL be allowed once per combat
- **AND** a later move of the same instance SHALL be rejected
- **AND** the destination SHALL have room

#### Scenario: Vision moves each turn

- **WHEN** Vision is in play and its controller moves it
- **THEN** the move SHALL be allowed once per turn
- **AND** a second move of the same instance in the same turn SHALL be rejected
- **AND** the permission SHALL reset at the start of the next turn

#### Scenario: Cloak opens a movement window

- **WHEN** Cloak is revealed on turn N
- **THEN** during turn N + 1 any card in play may be moved to Cloak's location if the destination has room
- **AND** the window SHALL apply to both sides
- **AND** the window SHALL expire at the end of turn N + 1
- **AND** the move SHALL be subject to the normal movement and lock rules

#### Scenario: Vulture reacts to its own move

- **WHEN** Vulture moves to another location
- **THEN** it SHALL gain +4 persistent Power
- **AND** the bonus SHALL apply once per completed move
- **AND** a failed or cancelled move SHALL NOT grant it

#### Scenario: Ebony Maw is play-restricted

- **WHEN** Ebony Maw is in hand
- **THEN** it SHALL NOT be playable after turn 3
- **AND** while it is in play, no card SHALL be played into its location by either side
- **AND** effects MAY still add cards to that location
- **AND** the play restriction SHALL be removed if its printed abilities are disabled

#### Scenario: The Infinaut is play-restricted

- **WHEN** The Infinaut is in hand
- **AND** its owner played a card during the previous turn
- **THEN** The Infinaut SHALL NOT be playable
- **AND** it SHALL be playable when its owner played no card during the previous turn
- **AND** the restriction SHALL NOT depend on the opponent's plays

#### Scenario: Storm floods a location

- **WHEN** Storm is revealed on turn N
- **THEN** her location SHALL be flooded
- **AND** turn N + 1 SHALL be the last turn in which cards may be played there
- **AND** from turn N + 2 onward no card SHALL be played there
- **AND** effects MAY still add cards to the flooded location

### Requirement: Series 2 Destruction, Discard and Side Change

The system SHALL resolve the replacement, discard and side-change abilities of the Series 2 cards.

#### Scenario: Bucky Barnes becomes the Winter Soldier

- **WHEN** Bucky Barnes is destroyed
- **THEN** it SHALL be replaced in its location by a Winter Soldier token with 8 base Power
- **AND** Bucky Barnes SHALL NOT enter the destroyed pile for that combat
- **AND** the destruction event SHALL still fire for reactions
- **AND** the Winter Soldier SHALL be a separate, non-draftable instance

#### Scenario: Sabretooth returns at zero cost

- **WHEN** Sabretooth is destroyed and its owner's hand has room
- **THEN** it SHALL return to its owner's hand instead of entering the destroyed pile
- **AND** its effective cost SHALL be 0 for the rest of the combat
- **AND** the destruction event SHALL still fire for reactions
- **AND** when the hand is full, it SHALL be destroyed normally

#### Scenario: Swarm creates zero-cost copies

- **WHEN** Swarm is discarded
- **THEN** it SHALL enter the discarded pile
- **AND** two copies of Swarm with effective cost 0 SHALL be added to its owner's hand while the hand has room
- **AND** each copy SHALL be an independent instance with the same abilities
- **AND** copies rejected by the hand cap SHALL NOT be created

#### Scenario: Hobgoblin switches sides

- **WHEN** Hobgoblin is revealed
- **THEN** it SHALL move to the opposing side of its location if that side has room
- **AND** it SHALL NOT enter a pile
- **AND** it SHALL NOT trigger a normal movement reaction
- **AND** it SHALL remain on its original side when the opposing side has no room or the location blocks removal or addition

### Requirement: Series 2 Cost and Deck Effects

The system SHALL support the cost increases, cost cap and deck-to-location effects introduced by the Series 2 content.

#### Scenario: Cost cap

- **WHEN** any effect increases a card's effective cost
- **THEN** the effective cost SHALL NOT exceed 6
- **AND** the cap SHALL apply after permanent upgrades, combat modifiers and turn modifiers
- **AND** the displayed cost SHALL match the capped value

#### Scenario: Sandman raises next-turn costs

- **WHEN** Sandman is revealed on turn N
- **THEN** every card in both hands SHALL cost 1 more during turn N + 1
- **AND** the increase SHALL apply to cards drawn or added during that turn where the cost is evaluated
- **AND** multiple Sandmen SHALL stack
- **AND** the increase SHALL expire at the end of turn N + 1
- **AND** the total effective cost SHALL NOT exceed 6

#### Scenario: Zero-cost copies and returns

- **WHEN** an effect sets a card's effective cost to 0 for the combat
- **THEN** later turn cost modifiers SHALL NOT raise it above 0
- **AND** the override SHALL clear when a new combat starts
- **AND** returning the card to hand SHALL preserve the override

#### Scenario: Jubilee moves a deck card to a location

- **WHEN** an effect moves the top card of a deck to a location
- **THEN** the card SHALL leave the deck only when the destination has room
- **AND** the card SHALL become a board instance and its Ongoing abilities SHALL be active
- **AND** the move SHALL NOT count as a play for play-triggered effects
- **AND** a failed addition SHALL leave the card on top of the deck

### Requirement: Activate Processing

The system SHALL expose activated abilities as a first-class action.

#### Scenario: Activation legality

- **WHEN** a player activates an owned card in play
- **THEN** the action authority SHALL require that the card has an `Activate` effect, is not ability-disabled and has not activated this combat
- **AND** a rejected activation SHALL leave the board, hand and energy unchanged
- **AND** the activation SHALL be available only during the play phase

#### Scenario: Activation once per combat

- **WHEN** a card activates
- **THEN** the instance SHALL record the activation
- **AND** it SHALL NOT activate again in the same combat
- **AND** returning the card to hand SHALL NOT clear the record
- **AND** a new combat SHALL clear it

#### Scenario: Activation effects

- **WHEN** Black Widow, Hellcow, Lockjaw, Mysterio or Spider-Ham activates
- **THEN** the effect SHALL resolve through the normal action authority
- **AND** it SHALL NOT consume energy
- **AND** it SHALL NOT count as a play for play records, `AfterPlayed` or taunt

### Requirement: Copied and Stolen Ability Text

The system SHALL support per-instance copied and suppressed effect ids.

#### Scenario: Copy last played

- **WHEN** Absorbing Man or Mystique resolves
- **THEN** it SHALL copy the matching printed effect ids of the last card its owner played
- **AND** Absorbing Man SHALL copy only `OnReveal` effects
- **AND** Mystique SHALL copy only `Ongoing` effects
- **AND** a card with no matching effect SHALL produce no copy

#### Scenario: Steal an Ongoing

- **WHEN** Rogue resolves
- **THEN** she SHALL copy the printed `Ongoing` effects of an enemy card in her zone
- **AND** the enemy card SHALL lose those effects through a suppressed-effect list
- **AND** the enemy card's other printed or perk effects SHALL remain

#### Scenario: Super-Skrull

- **WHEN** Super-Skrull is in play
- **THEN** it SHALL copy the printed `Ongoing` effects of all enemy cards in play, one level deep
- **AND** it SHALL skip perks, temporary effects and effects that are themselves copied
- **AND** it SHALL evaluate the copied effects with its owner as the source side
- **AND** two Super-Skrulls SHALL NOT recursively copy each other

#### Scenario: Copy presentation

- **WHEN** a card has copied or replaced text
- **THEN** its detail panel SHALL show the copied source or the replacement text
- **AND** the original card text SHALL remain available when the copy is additive
- **AND** Spider-Ham's target SHALL show `oink!` and lose its printed abilities

### Requirement: Delayed Reveal

The system SHALL support cards that stay unrevealed until the game ends.

#### Scenario: Invisible Woman

- **WHEN** a card is played in Invisible Woman's zone
- **THEN** it SHALL gain the delayed-reveal state
- **AND** it SHALL NOT reveal at end of turn
- **AND** its `OnReveal` effects SHALL NOT fire
- **AND** it SHALL still count its current power for the zone total

#### Scenario: End-of-game reveal

- **WHEN** the final turn ends and a delayed card is still in play
- **THEN** it SHALL reveal in play order
- **AND** its `OnReveal` effects SHALL resolve before `EndOfGame` effects
- **AND** the reveal marker SHALL be set

#### Scenario: Invisible Woman leaves play

- **WHEN** Invisible Woman leaves play
- **THEN** the delayed cards SHALL reveal at the next end of turn
- **AND** their `OnReveal` effects SHALL resolve normally

### Requirement: Destroyed-Card History

The system SHALL keep a combat-scoped history of destroyed cards for cost and power effects.

#### Scenario: Record a destroy

- **WHEN** a card is destroyed
- **THEN** the board SHALL record the card, owner, power at destruction, cost at destruction and turn
- **AND** the record SHALL remain even if the card is later resurrected
- **AND** the record SHALL reset when a new combat starts

#### Scenario: Death

- **WHEN** Death's cost is resolved
- **THEN** it SHALL be reduced by 1 for each destroyed-card record
- **AND** the cost SHALL NOT go below 0

#### Scenario: Knull

- **WHEN** Knull's power is resolved
- **THEN** it SHALL add the combined power at destruction of every destroyed-card record
- **AND** negative powers SHALL count
- **AND** the value SHALL be derived, not accumulated

### Requirement: New Action Surface

The system SHALL expose the actions required by the Series 3 cards through the effect actions contract.

#### Scenario: Draw

- **WHEN** an effect draws for a side
- **THEN** the action SHALL respect hand capacity and the Widow's Bite draw lock
- **AND** it SHALL dispatch the normal draw event
- **AND** it SHALL do nothing when the deck is empty

#### Scenario: Resurrect

- **WHEN** Ghost Rider or Hela resurrects a card
- **THEN** the card SHALL leave the discard pile and enter a zone with room
- **AND** it SHALL be marked revealed
- **AND** its `OnReveal` SHALL NOT resolve

#### Scenario: Copy to other zones

- **WHEN** Arnim Zola or Nimrod copies a card
- **THEN** independent copies SHALL be added to each other zone with room
- **AND** the copies SHALL be revealed
- **AND** their `OnReveal` SHALL NOT resolve

#### Scenario: Zone swap

- **WHEN** Quake resolves
- **THEN** the other two zone positions SHALL exchange definitions, cards, flood state and play constraints
- **AND** the zone indices and board references SHALL remain stable
- **AND** the views SHALL update from the board version

#### Scenario: Enemy hand

- **WHEN** Doctor Octopus, Black Bolt, Moon Knight, Silver Samurai, Master Mold or Black Widow resolves
- **THEN** the action SHALL use the enemy hand through the normal capacity, discard and hand-entry rules
- **AND** a pull SHALL mark the pulled card revealed and resolve its `OnReveal`
- **AND** a discard SHALL increment the enemy discard counter

#### Scenario: Max Energy

- **WHEN** Electro or Havok changes Max Energy
- **THEN** the board SHALL record a per-side Max Energy delta
- **AND** the next energy pool SHALL include it
- **AND** the delta SHALL reset when a new combat starts

#### Scenario: Created cards

- **WHEN** an effect adds a card to hand
- **THEN** the card SHALL be marked as created
- **AND** Quinjet's cost reduction SHALL apply to created cards
- **AND** a normal draw SHALL NOT mark the card as created

### Requirement: Play and Zone Restriction Effects

The system SHALL evaluate card and zone restrictions through one legality query shared by the UI, the AI and the action authority.

#### Scenario: Card restrictions

- **WHEN** Crossbones, Giganto or Sentry is evaluated
- **THEN** Crossbones SHALL require the owner to be winning the destination
- **AND** Giganto SHALL allow only the left zone
- **AND** Sentry SHALL reject the right zone

#### Scenario: Zone restrictions

- **WHEN** Goose or Jean Grey is active
- **THEN** Goose SHALL reject cards with effective cost 4 or more in that zone
- **AND** Jean Grey SHALL require the first card of each turn to be played there when the card can legally go there
- **AND** a full or illegal Jean Grey zone SHALL NOT block other zones

#### Scenario: Side restrictions

- **WHEN** Electro is in play
- **THEN** its owner SHALL play at most one card per turn
- **AND** the limit SHALL reset at the start of the next turn

#### Scenario: Global cost cap

- **WHEN** Wave resolves
- **THEN** all cards SHALL cost at most 4 until the end of the next turn
- **AND** the cap SHALL apply after reductions and before the cost floor
- **AND** it SHALL affect both sides, hand, deck and created cards

### Requirement: Series 3 Triggered Effects

The system SHALL resolve the new Series 3 triggered families through the shared processor.

#### Scenario: Reaction effects

- **WHEN** Echo, Negasonic Teenage Warhead, Silk, Titania, Hercules, Nebula, Kingpin, Dagger, Human Torch or Spider-Man 2099 reacts
- **THEN** it SHALL use the matching `AfterPlayed`, `AfterMoved` or `EndOfTurn` event
- **AND** it SHALL filter by event source, side and zone
- **AND** a self-play SHALL NOT trigger Silk or Titania
- **AND** Hercules and movement chains SHALL be bounded by the event limit

#### Scenario: Cost and deck effects

- **WHEN** Death, She-Hulk, Stature, Miles Morales, Sera, Quinjet, Zabu, Mobius M. Mobius, Wave, Baron Mordo, Beast, Mister Negative or Zabu resolves
- **THEN** the cost resolver SHALL apply the effect in the documented precedence order
- **AND** Mobius SHALL prevent increases for its owner and reductions for its opponent
- **AND** Mister Negative SHALL swap cost and power in the deck without mutating shared definitions

#### Scenario: End-of-game effects

- **WHEN** Captain Marvel, Dracula, M'Baku or Martyr resolves
- **THEN** it SHALL use the end-of-game phase
- **AND** it SHALL respect zone capacity and full locks
- **AND** it SHALL NOT start a new turn or re-open the play phase
