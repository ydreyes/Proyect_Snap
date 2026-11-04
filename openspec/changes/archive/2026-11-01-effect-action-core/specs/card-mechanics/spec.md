# Spec Delta

## MODIFIED Requirements

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

The system SHALL support cards with `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `GameStart`, `EndOfTurn` and `Activate` abilities.

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
- **THEN** the new action and game-start trigger families SHALL remain declared
- **AND** they SHALL NOT change the behavior of any existing card

### Requirement: Card Instances

The system SHALL track card instances in play with dynamic state.

#### Scenario: Dynamic card state

- **WHEN** cards are in play
- **THEN** each instance SHALL track its current power derived from base power, perks, persistent bonus and Ongoing effects
- **AND** it SHALL track when it was played (for play order)
- **AND** it SHALL track the zone where it was played
- **AND** it SHALL track attached perks
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
- **AND** its persistent bonus and perks SHALL remain
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
