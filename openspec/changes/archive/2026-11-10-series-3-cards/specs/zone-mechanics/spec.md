# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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
