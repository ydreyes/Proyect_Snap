# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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
