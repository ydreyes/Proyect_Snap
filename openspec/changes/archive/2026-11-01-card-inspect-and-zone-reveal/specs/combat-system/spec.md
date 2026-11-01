# Spec Delta

## MODIFIED Requirements

### Requirement: Zone Reveal Order
The system SHALL reveal one zone at the start of each of the first three turns of a combat, in order, and keep it revealed for the rest of that combat.

#### Scenario: Turn start zone reveals
- **WHEN** a turn begins
- **THEN** the next unrevealed zone SHALL be revealed
- **AND** previously revealed zones SHALL stay revealed
- **AND** each zone reveal MAY trigger zone effects

#### Scenario: Combat starts with zones hidden

- **WHEN** a combat begins
- **THEN** all three zones SHALL start unrevealed
- **AND** the zone name, effect text and art SHALL be withheld until the zone is revealed
- **AND** the zone index and both power totals SHALL remain visible

#### Scenario: First turn reveals zone 1

- **WHEN** the first turn of a combat begins
- **THEN** zone 1 SHALL be revealed
- **AND** zones 2 and 3 SHALL remain unrevealed

#### Scenario: Second turn reveals zone 2

- **WHEN** the second turn begins
- **THEN** zone 2 SHALL be revealed
- **AND** zone 1 SHALL remain revealed
- **AND** zone 3 SHALL remain unrevealed

#### Scenario: Third turn reveals zone 3

- **WHEN** the third turn begins
- **THEN** zone 3 SHALL be revealed
- **AND** all three zones SHALL remain revealed for the rest of the combat
- **AND** each zone reveal MAY trigger zone effects

#### Scenario: Hidden zones remain playable

- **WHEN** a zone is unrevealed
- **THEN** it SHALL accept cards from both sides
- **AND** its power totals SHALL count for control and priority
- **AND** its play constraints SHALL apply once revealed

#### Scenario: Reveal before resolution

- **WHEN** a combat resolves with zones still unrevealed
- **THEN** every unrevealed zone SHALL be revealed before control and the match result are computed
