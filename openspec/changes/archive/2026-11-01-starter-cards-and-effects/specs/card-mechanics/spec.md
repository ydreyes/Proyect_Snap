# Spec Delta

## MODIFIED Requirements

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
