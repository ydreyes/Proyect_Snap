# effect-system Specification

## Purpose
The effect system provides a pluggable architecture for card and zone effects supporting multiple trigger types with registry/factory patterns to ensure extensibility without core system changes.

## Requirements

### Requirement: Effect Trigger Types
The system SHALL implement OnReveal and Ongoing card triggers in this iteration. EndOfTurn and Activate SHALL remain declared trigger types for future content but SHALL NOT fire, because no content uses them.

#### Scenario: Different trigger types
- **WHEN** effects are defined
- **THEN** each effect SHALL be associated with one of the supported trigger types
- **AND** OnReveal effects SHALL fire when their card is revealed
- **AND** Ongoing effects SHALL stay active while their card is in play
- **AND** EndOfTurn and Activate effects SHALL only fire when that behavior is implemented

#### Scenario: OnReveal fires once
- **WHEN** a card with an OnReveal effect is revealed at the end of a turn
- **THEN** its OnReveal effect SHALL be applied exactly once
- **AND** it SHALL NOT be applied again on later turns

#### Scenario: Ongoing is continuously active
- **WHEN** a card with an Ongoing effect is on the board
- **THEN** its effect SHALL contribute to every power calculation while the card remains in play
- **AND** it SHALL NOT be applied as a one-time mutation

#### Scenario: Inactive triggers do not fire
- **WHEN** a turn ends or a player activates a card
- **THEN** no EndOfTurn or Activate effect SHALL be applied in this iteration

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
The system SHALL process effects according to their trigger types at the appropriate times.

#### Scenario: OnReveal processing
- **WHEN** a turn ends
- **THEN** the system SHALL compute the reveal order of cards played this turn
- **AND** it SHALL apply each card's OnReveal effects in that order
- **AND** each card's effect SHALL resolve completely before the next card's effect
- **AND** the resolution SHALL complete before priority, victory and the next turn are evaluated

#### Scenario: EndOfTurn processing
- **WHEN** a turn ends
- **THEN** EndOfTurn effects SHALL remain inactive in this iteration
- **AND** the turn SHALL still resolve OnReveal effects and continue

#### Scenario: Activate processing
- **WHEN** a player activates a card's ability
- **THEN** Activate effects SHALL remain inactive in this iteration
- **AND** the activation SHALL NOT change the board

#### Scenario: Ongoing processing
- **WHEN** card or zone power is recalculated
- **THEN** the system SHALL query the Ongoing effects of every card in the zone
- **AND** it SHALL NOT write the result into the card's current power as an accumulated value

#### Scenario: No effect content
- **WHEN** a card has no effects
- **THEN** the processor SHALL leave its power and zone unchanged

### Requirement: Power Resolution
The system SHALL calculate card power dynamically from base power, perks, persistent bonuses and active Ongoing modifiers.

#### Scenario: Card power composition
- **WHEN** a card's power is calculated
- **THEN** it SHALL equal the card's base power plus perk power plus persistent bonus power plus Ongoing bonuses
- **AND** it SHALL NOT read the card's previous current power as an input

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

#### Scenario: Card creation
- **WHEN** an OnReveal effect creates a card, such as Sentinel
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
