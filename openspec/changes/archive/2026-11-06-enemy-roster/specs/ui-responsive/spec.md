# Spec Delta

## ADDED Requirements

### Requirement: Encounter Identity Display

The system SHALL show which enemy the player is facing.

#### Scenario: Rival avatar

- **WHEN** a combat starts against a named enemy
- **THEN** the rival avatar SHALL show the enemy name or a short initial
- **AND** it SHALL NOT show the generic label `RIVAL`
- **AND** the label SHALL fit at the smallest supported viewport

#### Scenario: Enemy selection cards

- **WHEN** the two candidates are shown
- **THEN** each card SHALL show the name, flavor phrase, difficulty and deck preview
- **AND** the two candidates SHALL remain distinguishable at the smallest supported viewport

#### Scenario: Boss framing

- **WHEN** a sub-boss or the final boss is announced
- **THEN** the screen SHALL distinguish it from a normal enemy
- **AND** the boss name and phrase SHALL be legible
- **AND** the continue control SHALL be reachable at the smallest supported viewport
