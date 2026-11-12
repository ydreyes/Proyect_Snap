# Spec Delta

## MODIFIED Requirements

### Requirement: Card Instances

The system SHALL track card instances in play with dynamic state.

#### Scenario: Dynamic card state

- **WHEN** cards are in play
- **THEN** each instance SHALL track its current power derived from base power, upgrades, perks, persistent bonus and Ongoing effects
- **AND** it SHALL track its cost delta, power delta and upgrade count
- **AND** it SHALL expose an effective cost and an effective power
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
- **AND** its persistent bonus, upgrades and perks SHALL remain
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
- **THEN** the instance SHALL clear temporary combat state such as temporary cost modifiers, revealed markers and disabled abilities
- **AND** it SHALL keep its cost delta, power delta, upgrade count and attached perks

## ADDED Requirements

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
