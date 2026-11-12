# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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
