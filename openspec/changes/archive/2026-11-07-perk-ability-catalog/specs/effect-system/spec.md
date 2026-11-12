# Spec Delta

## MODIFIED Requirements

### Requirement: Effect Trigger Types

The system SHALL implement `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `GameStart`, `StartOfTurn` and `EndOfTurn` as active trigger families. `Activate` SHALL remain a declared trigger type that does not fire because no content uses it. Card and perk abilities SHALL use the same trigger families.

#### Scenario: Different trigger types

- **WHEN** effects are defined
- **THEN** each effect SHALL be associated with one of the supported trigger types
- **AND** `OnReveal` effects SHALL fire when their card is revealed
- **AND** `Ongoing` effects SHALL stay active while their card is in play
- **AND** `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn` and `GameStart` effects SHALL fire only when the matching action or combat event occurs
- **AND** `StartOfTurn` effects SHALL fire at the start of a turn
- **AND** `EndOfTurn` effects SHALL fire when a turn ends
- **AND** `Activate` effects SHALL NOT fire in this iteration

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

#### Scenario: Inactive triggers do not fire

- **WHEN** a turn ends or a player activates a card
- **THEN** `EndOfTurn` effects SHALL resolve and no `Activate` effect SHALL be applied
- **AND** the activation SHALL NOT change the board

#### Scenario: Action triggers do not fire without content

- **WHEN** a card is played, moved, destroyed, discarded or drawn
- **AND** no card or perk references the matching action trigger
- **THEN** no effect SHALL be applied
- **AND** the action SHALL complete normally

#### Scenario: Action triggers fire with content

- **WHEN** a card or perk references an action trigger
- **AND** the matching play, move, destroy, discard or draw occurs
- **THEN** the effect SHALL fire in the same resolution
- **AND** the action SHALL complete normally

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
- **THEN** no `Activate` effect SHALL be applied in this iteration
- **AND** the activation SHALL NOT change the board

#### Scenario: Ongoing processing

- **WHEN** card or zone power is recalculated
- **THEN** the system SHALL query the `Ongoing` effects of every card and attached perk in the zone
- **AND** it SHALL NOT write the result into the card's current power as an accumulated value

#### Scenario: No effect content

- **WHEN** a card has no effects and no attached perks
- **THEN** the processor SHALL leave its power and zone unchanged

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

## ADDED Requirements

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
