# Spec Delta

## ADDED Requirements

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
