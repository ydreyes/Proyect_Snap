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
The system SHALL calculate zone control based on card power.

#### Scenario: Control determination
- **WHEN** determining zone control
- **THEN** total power of all cards on each side in the zone SHALL be calculated
- **AND** the side with higher total power SHALL control the zone
- **AND** ties SHALL be handled by a defined rule
- **AND** zone control SHALL be used for priority calculation

### Requirement: Zone Play Constraints
The system SHALL enforce zone-specific play constraints.

#### Scenario: Constrained play
- **WHEN** a zone has play constraints
- **THEN** those constraints SHALL be enforced during card play
- **AND** invalid plays SHALL be prevented
- **AND** constraints SHALL be expressed through zone effects
