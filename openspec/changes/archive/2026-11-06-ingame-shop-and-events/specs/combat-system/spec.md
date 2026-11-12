# Spec Delta

## MODIFIED Requirements

### Requirement: Turn Energy Pool

The system SHALL grant a per-turn energy pool equal to the current turn number, capped at 6, adjusted by active run modifiers.

#### Scenario: Pool size follows the turn

- **WHEN** a turn starts
- **THEN** the available energy pool SHALL equal the turn number
- **AND** on turns beyond 6 the pool SHALL remain 6
- **AND** with `energy_bonus` active the pool SHALL be 1 higher
- **AND** with `energy_cap` active the cap SHALL be 7

#### Scenario: Energy does not carry over

- **WHEN** a turn ends with unspent energy
- **THEN** the unspent energy SHALL be lost
- **AND** the next turn SHALL start with a fresh pool for that turn

#### Scenario: Energy modifiers are visible

- **WHEN** a run modifier changes the pool or the cap
- **THEN** the energy indicator SHALL show the modified value
- **AND** the AI SHALL use the same modified pool
