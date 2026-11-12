# Spec Delta

## MODIFIED Requirements

### Requirement: Run Outcomes

The system SHALL end a run on victory or on a lost or tied combat.

#### Scenario: Run victory

- **WHEN** the player wins the final boss of the final act
- **THEN** the run SHALL be marked as won
- **AND** the player SHALL NOT enter another combat
- **AND** the run summary SHALL be shown

#### Scenario: Run defeat

- **WHEN** the player loses or ties any combat without an extra-life charge
- **THEN** the run SHALL end immediately
- **AND** no further combat SHALL be played in that run
- **AND** the run summary SHALL be shown

#### Scenario: Extra life

- **WHEN** the player loses or ties a combat while holding an extra-life charge
- **THEN** the charge SHALL be consumed
- **AND** the same enemy SHALL be repeated once
- **AND** the run SHALL remain active
- **AND** the repeat SHALL use a new seed and the same enemy composition
