# zone-mechanics Specification

## Purpose
The zone mechanics system manages the three combat zones, their effects, play constraints, and control calculation.

## Requirements

### Requirement: Zone Structure
The system SHALL have zones that can contain cards from both players.

#### Scenario: Zone composition
- **WHEN** a zone exists
- **THEN** it SHALL hold up to 4 cards for the player
- **AND** it SHALL hold up to 4 cards for the AI
- **AND** it SHALL have associated zone effects

### Requirement: Zone Effects
The system SHALL support zone effects that apply to cards in the zone or globally.

#### Scenario: Zone effect application
- **WHEN** a zone has effects
- **THEN** zone effects SHALL be applied according to their trigger conditions
- **AND** new zone types with different effects SHALL be addable without modifying core systems
- **AND** zone effects SHALL work through the same effect system

### Requirement: Extensible Zone Types
The system SHALL allow adding new zone types easily.

#### Scenario: Adding new zone
- **WHEN** a new zone type is needed (e.g., "only one card can be played this turn")
- **THEN** it SHALL be defined as a zone definition with associated effects
- **AND** no changes to core combat logic SHALL be required
- **AND** the zone SHALL be added to the zone pool

### Requirement: Zone Control Calculation
The system SHALL calculate zone control based on the final card power and zone multipliers.

#### Scenario: Control determination
- **WHEN** determining zone control
- **THEN** total power of all cards on each side in the zone SHALL be calculated
- **AND** the side with higher total power SHALL control the zone
- **AND** ties SHALL be handled by a defined rule
- **AND** zone control SHALL be used for priority calculation

#### Scenario: Ongoing bonuses count
- **WHEN** a card in the zone has an Ongoing power bonus
- **THEN** that bonus SHALL be included in the zone total
- **AND** it SHALL update when the board changes

#### Scenario: One calculation for every consumer
- **WHEN** zone power is displayed or evaluated
- **THEN** priority, victory, the location plaque and the detail panel SHALL use the same calculation
- **AND** they SHALL NOT sum card power through separate paths

### Requirement: Zone Power Multipliers
The system SHALL apply Ongoing zone multipliers to the total power of one side in a zone.

#### Scenario: Iron Man doubles the allied total
- **WHEN** Iron Man is in a zone
- **THEN** the total power of its side in that zone SHALL be doubled
- **AND** the individual power of each card SHALL remain unchanged

#### Scenario: Multiple multipliers
- **WHEN** two Iron Men are in the same zone
- **THEN** the allied zone total SHALL be multiplied by four

#### Scenario: Multiplier affects only its side
- **WHEN** Iron Man is in a zone
- **THEN** only the total of Iron Man's side SHALL be multiplied
- **AND** the rival total SHALL remain unchanged

#### Scenario: Derived, not accumulated
- **WHEN** zone power is calculated more than once
- **THEN** the multiplier SHALL be applied to the raw sum each time
- **AND** it SHALL NOT be stored in card power or added repeatedly

### Requirement: Zone Play Constraints

The system SHALL enforce zone-specific play constraints.

#### Scenario: Constrained play

- **WHEN** a zone has play constraints
- **THEN** those constraints SHALL be enforced during card play
- **AND** invalid plays SHALL be prevented
- **AND** constraints SHALL be expressed through zone effects

#### Scenario: Goose constraint

- **WHEN** Goose is in a zone
- **THEN** no card with effective cost 4 or more SHALL be played in that zone by either side
- **AND** movement and effect additions SHALL remain allowed
- **AND** the restriction SHALL be evaluated against the resolved cost

#### Scenario: Jean Grey constraint

- **WHEN** Jean Grey is in a zone and a side has not played a card this turn
- **THEN** that side's first card SHALL be played in her zone when the card can legally go there
- **AND** a full or illegal Jean Grey zone SHALL NOT block other zones
- **AND** the player UI, the AI and the action authority SHALL use the same result

### Requirement: Implemented Zone Effects

The system SHALL apply the three starting-zone effects as real power modifiers.

#### Scenario: Plus-one zone

- **WHEN** the +1 zone is revealed
- **THEN** every card in that zone SHALL have +1 Power
- **AND** the bonus SHALL update when a card enters or leaves the zone

#### Scenario: Minus-one zone

- **WHEN** the -1 zone is revealed
- **THEN** every card in that zone SHALL have -1 Power
- **AND** the bonus SHALL update when a card enters or leaves the zone

#### Scenario: Highest-cost zone

- **WHEN** the highest-cost zone is revealed
- **THEN** the highest-Cost card in that zone SHALL have +2 Power
- **AND** ties SHALL apply the bonus to every tied highest-Cost card
- **AND** the bonus SHALL be derived, not accumulated

### Requirement: Zone Power Additions

The system SHALL support additive bonuses to a side's zone total and their repetition by other continuous effects.

#### Scenario: Klaw

- **WHEN** Klaw is in a location
- **THEN** its owner's side of the location to the right SHALL gain +6 Power
- **AND** the addition SHALL change the zone total, not the individual card power
- **AND** it SHALL update when Klaw moves or leaves play

#### Scenario: Mister Fantastic

- **WHEN** Mister Fantastic is in a location
- **THEN** its owner's side of each adjacent location SHALL gain +3 Power
- **AND** the addition SHALL change the zone total, not the individual card power
- **AND** a Mister Fantastic in the leftmost or rightmost location SHALL affect only the one adjacent location
- **AND** the addition SHALL update when Mister Fantastic moves or leaves play

#### Scenario: Repetition

- **WHEN** Onslaught is in a location with another Ongoing source
- **THEN** that source's additive or multiplicative contribution SHALL apply an additional time
- **AND** the result SHALL be deterministic and bounded
- **AND** a repeated multiplication SHALL apply to the raw sum, not to a stored total

### Requirement: Zone Play and Removal Lock

The system SHALL support a zone lock that allows movement but blocks every other addition and removal.

#### Scenario: Professor X lock

- **WHEN** Professor X is in a location
- **THEN** cards SHALL NOT be played into that location
- **AND** cards SHALL NOT be destroyed, merged, banished or returned to hand from that location
- **AND** tokens and copies SHALL NOT be added to that location
- **AND** moving a card into or out of the location SHALL remain allowed

#### Scenario: Lock follows Professor X

- **WHEN** Professor X moves or leaves play
- **THEN** the lock SHALL move with him or stop
- **AND** the previous location SHALL return to normal rules

#### Scenario: Lock and protection

- **WHEN** a destroy effect targets a card in Professor X's location
- **THEN** the destroy SHALL be rejected
- **AND** no destroy event SHALL fire

### Requirement: Location Replacement

The system SHALL support replacing a location with another location definition from the pool.

#### Scenario: Scarlet Witch replaces

- **WHEN** Scarlet Witch is revealed
- **THEN** her location SHALL be replaced by a different registered location
- **AND** the replacement SHALL NOT be the same location
- **AND** the replacement pool SHALL exclude Limbo
- **AND** the new location SHALL be revealed immediately

#### Scenario: Contents survive

- **WHEN** a location is replaced
- **THEN** every card in that location SHALL remain in its slot and side
- **AND** the zone's power totals SHALL be recalculated with the new effect
- **AND** the location index SHALL remain the same

#### Scenario: Views update

- **WHEN** a location is replaced
- **THEN** the plaque and the zone detail panel SHALL show the new name and effect
- **AND** no other location SHALL be replaced

#### Scenario: Magik replaces with Limbo

- **WHEN** Magik is revealed on turn 5 or earlier
- **THEN** her location SHALL be replaced with Limbo
- **AND** Magik SHALL do nothing on turn 6
- **AND** the replacement SHALL count as a location replacement for flood clearing

### Requirement: Per-Side Location Visibility

The system SHALL support revealing hidden location information to one side without changing the real reveal state.

#### Scenario: Uatu owner

- **WHEN** a side has Uatu the Watcher in its deck or hand at combat start
- **THEN** that side SHALL see the name, effect and art of unrevealed locations
- **AND** the location SHALL still be treated as unrevealed for reveal timing and effects

#### Scenario: Opponent does not see

- **WHEN** one side controls Uatu
- **THEN** the other side SHALL continue to see the unrevealed marker
- **AND** the real reveal order SHALL be unchanged

### Requirement: Play-Only Zone Lock

The system SHALL support a location lock that prevents playing cards without blocking other additions, movement or removal.

#### Scenario: Ebony Maw locks play

- **WHEN** Ebony Maw is in play in a location
- **THEN** neither side SHALL play a card into that location
- **AND** the lock SHALL apply from the moment Ebony Maw is in play
- **AND** movement into or out of the location SHALL remain allowed
- **AND** effects MAY still add cards to the location

#### Scenario: Lock follows the card

- **WHEN** Ebony Maw moves or leaves play
- **THEN** the lock SHALL move with him or stop
- **AND** the previous location SHALL return to normal play rules

#### Scenario: Disabled lock

- **WHEN** Ebony Maw's printed abilities are disabled
- **THEN** the play lock SHALL stop
- **AND** cards MAY be played into the location again

#### Scenario: Full lock is distinct

- **WHEN** Professor X is in a location
- **THEN** the full lock SHALL still block play, effect additions, destruction, merge, banish and return to hand
- **AND** the play-only lock of Ebony Maw SHALL NOT block those additions or removals
- **AND** movement SHALL remain allowed under both locks

### Requirement: Flooded Location

The system SHALL support a location that stops accepting played cards after a grace turn.

#### Scenario: Storm floods a location

- **WHEN** Storm is revealed on turn N
- **THEN** her location SHALL be marked as flooded
- **AND** turn N + 1 SHALL be the last turn in which cards may be played there
- **AND** from turn N + 2 onward no card SHALL be played there by either side
- **AND** movement into or out of the location SHALL remain allowed
- **AND** effects MAY still add cards to the location

#### Scenario: Repeated flood

- **WHEN** another Storm floods an already flooded location on a later turn
- **THEN** the last playable turn SHALL be the later of the existing and new grace turns
- **AND** the earlier grace turn SHALL NOT shorten the flood

#### Scenario: Replaced flooded location

- **WHEN** a flooded location is replaced by another location definition
- **THEN** the flood state SHALL clear unless the replacement is also flooded
- **AND** the cards already in the location SHALL remain

#### Scenario: Flood and play-only lock

- **WHEN** a location is both flooded and play-locked
- **THEN** both restrictions SHALL apply independently
- **AND** removing one restriction SHALL NOT remove the other

### Requirement: Limbo Zone

The system SHALL support a Limbo zone that extends the match.

#### Scenario: Definition

- **WHEN** the content is loaded
- **THEN** a `zone_limbo` definition SHALL exist
- **AND** it SHALL be revealed immediately when it replaces a location
- **AND** it SHALL be excluded from Scarlet Witch's random replacement pool

#### Scenario: Match length

- **WHEN** any zone is Limbo
- **THEN** the match SHALL last seven turns
- **AND** the turn display SHALL show the dynamic maximum
- **AND** the existing six-turn cap SHALL return when Limbo leaves play or is replaced

#### Scenario: Card contents

- **WHEN** a zone becomes Limbo
- **THEN** the cards in the zone SHALL remain in their slots and sides
- **AND** the zone power SHALL be recalculated
- **AND** the zone index SHALL remain the same

### Requirement: Zone Swapping

The system SHALL support swapping the positions of two zones.

#### Scenario: Quake swap

- **WHEN** Quake is revealed
- **THEN** the other two zone positions SHALL exchange definitions, cards, flood state and play constraints
- **AND** the zone indices and board references SHALL remain stable
- **AND** the views SHALL reconcile from the board version

#### Scenario: Power and status

- **WHEN** zones are swapped
- **THEN** both zone totals SHALL be recalculated with the new definitions
- **AND** the status labels SHALL move with the definitions
- **AND** the cards SHALL remain in their slots and sides

### Requirement: Zone Effect Suppression

The system SHALL support temporarily suppressing zone effects.

#### Scenario: Hawk

- **WHEN** Snowguard Hawk is revealed
- **THEN** all zone effects SHALL be suppressed for both sides until the end of the next turn
- **AND** the suppression SHALL NOT remove cards or change zone totals from cards
- **AND** zone effects SHALL resume for scoring at game end
- **AND** the status SHALL be visible on the affected plaques

#### Scenario: Expiry

- **WHEN** the suppression turn ends
- **THEN** the zone effects SHALL apply again
- **AND** the power totals SHALL be recalculated

### Requirement: Zone Effect Repetition

The system SHALL support repeating a zone's effect once.

#### Scenario: Bear

- **WHEN** Snowguard Bear is revealed
- **THEN** the current zone's simple effect SHALL resolve once more
- **AND** a zone without a triggerable effect SHALL produce no change
- **AND** the repeat SHALL NOT create an infinite loop

#### Scenario: Power update

- **WHEN** a zone effect is repeated
- **THEN** the affected card or zone powers SHALL be recalculated
- **AND** the result SHALL be idempotent outside the single repeat
