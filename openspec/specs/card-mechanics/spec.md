# card-mechanics Specification

## Purpose
The card mechanics system defines card structure, types, costs, power, and ability activation rules.

## Requirements

### Requirement: Card Structure

The system SHALL represent cards with cost, power, effects, a player-facing description and draft eligibility.

#### Scenario: Card properties

- **WHEN** a card is defined or instantiated
- **THEN** it SHALL have an ID, name, cost (energy), and base power
- **AND** it SHALL have a descriptive text intended for the player
- **AND** it MAY have associated effects by trigger type
- **AND** it MAY have tags

#### Scenario: Draft eligibility

- **WHEN** a card definition is registered
- **THEN** it SHALL declare whether it is eligible for draft and reward offers
- **AND** a token definition SHALL NOT be eligible for draft or reward offers
- **AND** a token definition SHALL be creatable only by an effect during combat

#### Scenario: Description is shown during inspection

- **WHEN** the player inspects a card
- **THEN** the descriptive text SHALL be available to the detail panel
- **AND** the text SHALL describe what the card is meant to do

### Requirement: Card Trigger Types

The system SHALL support cards with `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand`, `GameStart`, `EndOfTurn` and `Activate` abilities.

#### Scenario: Card abilities

- **WHEN** a card has abilities
- **THEN** abilities SHALL be categorized by their trigger type
- **AND** each ability SHALL trigger at the appropriate game event

#### Scenario: Multiple trigger types on one card

- **WHEN** a card definition declares more than one effect
- **THEN** each effect SHALL be associated with exactly one trigger type
- **AND** the card SHALL be able to participate in more than one trigger family

#### Scenario: New triggers without content

- **WHEN** the card pool contains only `OnReveal` and `Ongoing` cards
- **THEN** the new action, hand-entry and game-start trigger families SHALL remain declared
- **AND** they SHALL NOT change the behavior of any existing card

### Requirement: Activate Abilities

The system SHALL support cards with activated abilities that players can trigger.

#### Scenario: Activating ability

- **WHEN** a player activates a card's ability during the play phase
- **THEN** the ability SHALL only activate if the card is owned, in play, not ability-disabled and has an unused `Activate` effect
- **AND** no energy SHALL be paid unless the effect explicitly declares a cost
- **AND** the activation SHALL be tracked to prevent reuse in the same combat
- **AND** returning the card to hand SHALL NOT clear the activation record
- **AND** the activation SHALL NOT create a play record or an `AfterPlayed` trigger

#### Scenario: Activation presentation

- **WHEN** an owned in-play card has an unused `Activate` effect
- **THEN** its detail panel SHALL show an `ACTIVAR` control
- **AND** the control SHALL be hidden when the card is not owned, not in play, already activated or ability-disabled
- **AND** the control SHALL remain reachable at the smallest supported viewport

### Requirement: Card Instances

The system SHALL track card instances in play with dynamic state.

#### Scenario: Dynamic card state

- **WHEN** cards are in play
- **THEN** each instance SHALL track its current power derived from base power, upgrades, perks, persistent bonus and Ongoing effects
- **AND** it SHALL track its cost delta, power delta, upgrade count and base-power override
- **AND** it SHALL expose an effective cost and an effective power
- **AND** it SHALL track when it was played (for play order)
- **AND** it SHALL track the zone where it was played
- **AND** it SHALL track attached perks
- **AND** it SHALL track activation, copied effects, suppressed effects and delayed reveal state
- **AND** power SHALL update as board state changes

#### Scenario: Persistent bonus survives movement

- **WHEN** a card gains a persistent bonus from its OnReveal effect
- **THEN** the bonus SHALL remain on the instance if the card moves to another zone
- **AND** it SHALL be included in every later power calculation

#### Scenario: Played zone is recorded once

- **WHEN** a card is played from the hand into a zone
- **THEN** the instance SHALL record that zone as the zone where it was played
- **AND** moving the card later SHALL NOT change the recorded zone

#### Scenario: Pending delayed condition

- **WHEN** a card arms a delayed condition for the next turn
- **THEN** the instance SHALL record the arming turn, the watched zone and the pending bonus amount
- **AND** the instance SHALL support more than one pending effect at the same time
- **AND** the condition SHALL be cleared after it resolves or is found not to apply

#### Scenario: Returning to hand resets play state

- **WHEN** a card returns to the hand
- **THEN** its played turn and played zone SHALL be cleared
- **AND** any pending delayed condition SHALL be cleared
- **AND** its persistent bonus, upgrades, perks and activation record SHALL remain
- **AND** its reveal marker SHALL remain set for the rest of the combat

#### Scenario: Reveal marker

- **WHEN** a card is processed by the reveal sequence
- **THEN** the instance SHALL be marked as revealed for the combat
- **AND** moving the card, returning it to hand or sending it to a pile SHALL NOT clear the marker
- **AND** the marker SHALL be cleared when a new combat starts

#### Scenario: Per-turn play marker

- **WHEN** a card is played from hand
- **THEN** the instance SHALL be marked as played during the current turn
- **AND** returning it to hand, moving it or replaying it SHALL NOT clear the marker during that turn
- **AND** the marker SHALL be cleared when the next turn starts
- **AND** play-triggered effects SHALL use the marker to fire once per instance per turn

#### Scenario: Disabled abilities

- **WHEN** an effect removes a card's abilities
- **THEN** the instance SHALL keep its definition but its effects SHALL NOT apply while disabled
- **AND** the disabled state SHALL be cleared when a new combat starts

#### Scenario: Token and copy identity

- **WHEN** an effect creates a token or a copy
- **THEN** the new card SHALL be a separate instance with its own identity
- **AND** it SHALL NOT be the same object as the source card
- **AND** it SHALL NOT be a draftable definition

#### Scenario: Upgrades survive combat reset

- **WHEN** a new combat starts
- **THEN** the instance SHALL clear temporary combat state such as temporary cost modifiers, revealed markers, activation, copied effects and disabled abilities
- **AND** it SHALL keep its cost delta, power delta, upgrade count and attached perks

### Requirement: Persistent Power from Play, Destroy and Discard

The system SHALL store persistent power changes on the card instance so they survive movement and later power recalculation.

#### Scenario: After-play counters

- **WHEN** Angela gains +1 from a card played in her location
- **THEN** the bonus SHALL be stored on Angela
- **AND** it SHALL apply in every later power calculation
- **AND** it SHALL survive Angela moving

#### Scenario: Global after-play counter

- **WHEN** Bishop gains +1 from an allied card play
- **THEN** the bonus SHALL be stored on Bishop
- **AND** it SHALL NOT be accumulated by replaying the same instance in the same turn

#### Scenario: Destruction and discard bonuses

- **WHEN** Carnage destroys cards
- **THEN** its +2 per destroyed card SHALL be stored on Carnage
- **AND** when Nova is destroyed
- **THEN** the +1 to allied cards SHALL be stored on each affected card
- **AND** when Apocalypse is discarded
- **THEN** its +4 SHALL be stored on Apocalypse when it returns to hand
- **AND** when Wolverine regenerates
- **THEN** its +2 SHALL be stored on Wolverine

#### Scenario: Next-play bonus

- **WHEN** Forge grants +2 to the next card played
- **THEN** the bonus SHALL be stored on the card that is played
- **AND** it SHALL survive that card returning to hand

### Requirement: Affliction

The system SHALL represent persistent negative power on a card instance.

#### Scenario: Afflicted card

- **WHEN** Spider-Woman afflicts an enemy card
- **THEN** the card SHALL store -1 persistent Power
- **AND** the affliction SHALL follow the card if it moves
- **AND** it SHALL NOT be applied to cards that enter the location later
- **AND** the card's current power SHALL include the affliction in every recalculation

#### Scenario: Multiple afflictions

- **WHEN** more than one affliction applies to the same card
- **THEN** each SHALL contribute once
- **AND** the result SHALL NOT be stored as an accumulated current power

### Requirement: Copy and Transform Identity

The system SHALL allow a card instance to become a copy of another card while keeping its own instance identity.

#### Scenario: Copy instance

- **WHEN** an effect creates a copy of a card
- **THEN** the copy SHALL have its own instance identity
- **AND** it SHALL use the source's definition, cost, base power and abilities
- **AND** it SHALL NOT share mutable state with the source

#### Scenario: Transform

- **WHEN** Morph becomes a copy of an opponent's card
- **THEN** Morph's definition, base power and abilities SHALL become that card's
- **AND** Morph's instance identity SHALL remain the same
- **AND** its persistent bonuses and perks SHALL be recalculated without double-counting

#### Scenario: Copies are not draftable

- **WHEN** a copy or token is created
- **THEN** its definition SHALL NOT become a draftable card definition
- **AND** it SHALL NOT be offered by draft or rewards

### Requirement: Merge State

The system SHALL represent a merged card as a card that left the board without entering a pile.

#### Scenario: Host gains power

- **WHEN** Hulkbuster merges into a host
- **THEN** the host SHALL gain Hulkbuster's current Power as persistent Power
- **AND** the host SHALL keep its own definition and abilities
- **AND** the host's current power SHALL include the merged power in every later calculation

#### Scenario: Merged card leaves play

- **WHEN** a card merges
- **THEN** it SHALL be removed from the board
- **AND** it SHALL NOT enter discarded, destroyed or banished
- **AND** it SHALL NOT be visible in the pile viewer for the rest of the combat
- **AND** it SHALL return with the run deck in the next combat

#### Scenario: Merge is not destruction

- **WHEN** a card merges
- **THEN** no destroy, discard or banish event SHALL fire for the merged card
- **AND** destroy protections SHALL NOT block the merge

### Requirement: Next-Play Pending Effects

The system SHALL store pending effects that trigger on the next card played by a side.

#### Scenario: Pending bonus and move

- **WHEN** Forge or Iron Fist arms a next-play effect
- **THEN** the side SHALL remember it until that side plays a card
- **AND** the effect SHALL apply to the next card played from hand
- **AND** it SHALL be consumed even if its target condition cannot be met

#### Scenario: Multiple pending effects

- **WHEN** more than one next-play effect is pending
- **THEN** all SHALL apply to the same next card in a deterministic order
- **AND** returning a card to hand SHALL NOT restore a consumed pending effect

#### Scenario: Across turns

- **WHEN** a next-play effect is armed and the turn ends without another play
- **THEN** the effect SHALL remain pending for the following turn
- **AND** it SHALL NOT expire at turn end

### Requirement: Card Upgrade State

The system SHALL represent upgrades as per-instance state rather than changes to the shared card definition.

#### Scenario: New instance has no upgrades

- **WHEN** a card instance is created
- **THEN** its cost delta SHALL be 0, its power delta SHALL be 0 and its upgrade count SHALL be 0

#### Scenario: Applying an upgrade

- **WHEN** an upgrade is applied to a card instance
- **THEN** a cost upgrade SHALL decrease the cost delta by 1
- **AND** a power upgrade SHALL increase the power delta by 2
- **AND** the upgrade count SHALL increase by 1
- **AND** the upgrade count SHALL NOT exceed 3

#### Scenario: Upgrades are per instance

- **WHEN** two copies of the same card definition exist
- **THEN** upgrading one instance SHALL NOT change the other instance
- **AND** the shared definition SHALL keep its base cost and base power

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

### Requirement: Series 2 Card Instance State

The system SHALL represent the per-instance state introduced by the Series 2 and Collection Level cards.

#### Scenario: Combat cost override

- **WHEN** Sabretooth returns to hand or Swarm creates a copy
- **THEN** the instance SHALL expose a combat-scoped cost override of 0
- **AND** the override SHALL take precedence over permanent and turn cost modifiers
- **AND** it SHALL clear when a new combat starts

#### Scenario: Movement usage

- **WHEN** Nightcrawler or Vision is moved on a turn later than the turn it was played
- **THEN** the instance SHALL record that its printed move was used
- **AND** Nightcrawler SHALL remain used for the rest of the combat
- **AND** Vision SHALL reset at the start of the next turn
- **AND** a same-turn move of a card played this turn SHALL NOT consume the printed permission

#### Scenario: Delayed inverse condition

- **WHEN** Jessica Jones arms her condition
- **THEN** the instance SHALL record the arming turn, the watched zone and the pending bonus
- **AND** the condition SHALL resolve at the end of the following turn
- **AND** it SHALL be cleared whether it succeeds or fails
- **AND** the instance SHALL support more than one pending condition at the same time

#### Scenario: Discard counters

- **WHEN** a side discards a card
- **THEN** the side's discard count for the combat SHALL increase by 1
- **AND** the count SHALL include discards that are later replaced
- **AND** the count SHALL reset when a new combat starts

#### Scenario: Printed ability suppression

- **WHEN** Leech suppresses a card in hand
- **THEN** the instance SHALL keep its definition but its printed abilities SHALL NOT resolve
- **AND** abilities granted by attached perks SHALL continue to resolve
- **AND** the state SHALL persist until the combat ends

#### Scenario: Side ownership

- **WHEN** Hobgoblin switches sides
- **THEN** the instance SHALL belong to the opposing side's zone list
- **AND** its recorded played zone SHALL remain unchanged
- **AND** its persistent bonuses and abilities SHALL follow the new controller
- **AND** no pile entry SHALL be created

### Requirement: Card Legality Queries

The system SHALL expose one legality query for playing and moving a card.

#### Scenario: Play legality

- **WHEN** a card is in hand
- **THEN** its play legality SHALL combine energy, zone capacity, zone constraints, zone locks, floods, taunt reservations, its own printed restrictions, the objective restrictions and the side-level restrictions
- **AND** the player UI, the AI and the action authority SHALL use the same result

#### Scenario: Move legality

- **WHEN** a card is in play
- **THEN** its move legality SHALL combine destination capacity, the card's printed movement permission, a per-combat or per-turn usage flag and any active movement window
- **AND** an effect-driven move SHALL NOT require a printed movement permission
- **AND** a rejected move SHALL leave the board unchanged

#### Scenario: Objective restrictions

- **WHEN** Crossbones, Giganto or Sentry is evaluated
- **THEN** Crossbones SHALL require the owner to be winning the destination
- **AND** Giganto SHALL allow only the left zone
- **AND** Sentry SHALL reject the right zone

#### Scenario: Zone and side restrictions

- **WHEN** Goose, Jean Grey or Electro is active
- **THEN** Goose SHALL reject cards with effective cost 4 or more in its zone
- **AND** Jean Grey SHALL require the first card of each turn to be played there when the card can legally go there
- **AND** Electro SHALL limit its owner to one played card per turn

### Requirement: Series 3 Card Instance State

The system SHALL represent the per-instance state introduced by the Series 3 cards.

#### Scenario: Activation state

- **WHEN** a card activates
- **THEN** the instance SHALL record the activation for the combat
- **AND** the record SHALL survive returning to hand
- **AND** the record SHALL clear when a new combat starts

#### Scenario: Copied and suppressed effects

- **WHEN** a card copies or steals ability text
- **THEN** the instance SHALL keep a list of copied effect ids
- **AND** the effective effect list SHALL include copied ids after printed, perk and temporary ids
- **AND** a stolen effect SHALL be suppressed on the source card without disabling its other effects
- **AND** the lists SHALL clear when a new combat starts

#### Scenario: Delayed reveal

- **WHEN** a card is played under Invisible Woman
- **THEN** the instance SHALL record the delayed-reveal state
- **AND** the state SHALL be clearable when Invisible Woman leaves play
- **AND** the state SHALL clear when a new combat starts

#### Scenario: Base power override

- **WHEN** a set, reset or copy-power effect applies
- **THEN** the instance SHALL expose a base-power override
- **AND** the override SHALL replace the definition's base power in every calculation
- **AND** the override SHALL clear when a new combat starts

#### Scenario: Created marker

- **WHEN** an effect adds a card to hand
- **THEN** the instance SHALL be marked as created
- **AND** the marker SHALL not affect normal draws
- **AND** the marker SHALL clear when a new combat starts

#### Scenario: Destroyed identity

- **WHEN** a card is destroyed
- **THEN** the board history SHALL record its owner, power and cost at destruction
- **AND** the record SHALL remain stable if the card is resurrected
- **AND** the record SHALL clear when a new combat starts

### Requirement: Series 3 Cost State

The system SHALL resolve effective cost from all Series 3 cost sources through one ordered authority.

#### Scenario: Precedence

- **WHEN** a card's effective cost is resolved
- **THEN** the authority SHALL apply the combat override or base cost, permanent and temporary deltas, dynamic self reductions, container auras, taxes, absolute sets, caps and floors in that order
- **AND** a minimum-1 effect SHALL not reduce the cost below 1
- **AND** the global maximum SHALL not exceed 6 unless an effect explicitly raises it

#### Scenario: Mobius

- **WHEN** Mobius M. Mobius is in play
- **THEN** its owner's costs SHALL NOT be increased
- **AND** its opponent's costs SHALL NOT be reduced
- **AND** an absolute set that raises a cost SHALL be treated as an increase

#### Scenario: Dynamic self cost

- **WHEN** Death, She-Hulk, Stature or Miles Morales is evaluated
- **THEN** the cost SHALL be derived from the destroyed-card history, the previous turn's unspent energy, the opponent's discard history or the previous turn's movement
- **AND** the result SHALL be idempotent and SHALL NOT be stored as an accumulated delta

#### Scenario: Created cards

- **WHEN** Quinjet is in play
- **THEN** created cards in its owner's hand SHALL cost 1 less
- **AND** the cost SHALL not go below 1
- **AND** a normal draw SHALL NOT receive the reduction
