# card-mechanics Specification

## Purpose
The card mechanics system defines card structure, types, costs, power, and ability activation rules.

## Requirements

### Requirement: Card Structure
The system SHALL represent cards with cost, power, effects, and a player-facing description.

#### Scenario: Card properties
- **WHEN** a card is defined or instantiated
- **THEN** it SHALL have an ID, name, cost (energy), and base power
- **AND** it SHALL have a descriptive text intended for the player
- **AND** it MAY have associated effects by trigger type
- **AND** it MAY have tags

#### Scenario: Description is shown during inspection
- **WHEN** the player inspects a card
- **THEN** the descriptive text SHALL be available to the detail panel
- **AND** the text SHALL describe what the card is meant to do

### Requirement: Card Trigger Types
The system SHALL support cards with OnReveal, Ongoing, EndOfTurn, and Activate abilities.

#### Scenario: Card abilities
- **WHEN** a card has abilities
- **THEN** abilities SHALL be categorized by their trigger type
- **AND** each ability SHALL trigger at the appropriate game event

### Requirement: Activate Abilities
The system SHALL support cards with activated abilities that players can trigger.

#### Scenario: Activating ability
- **WHEN** a player activates a card's ability during play phase
- **THEN** the ability SHALL only activate if conditions are met
- **AND** energy costs SHALL be paid if required
- **AND** the activation SHALL be tracked to prevent invalid reuse

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
- **AND** the condition SHALL be cleared after it resolves or is found not to apply

#### Scenario: Returning to hand resets play state
- **WHEN** a card returns to the hand
- **THEN** its played turn and played zone SHALL be cleared
- **AND** any pending delayed condition SHALL be cleared
- **AND** its persistent bonus and perks SHALL remain
