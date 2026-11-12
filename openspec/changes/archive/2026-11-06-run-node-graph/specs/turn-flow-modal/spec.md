# Spec Delta

## MODIFIED Requirements

### Requirement: Run Progress Display

The system SHALL display act and combat progress in the combat top bar.

#### Scenario: Combat and lives display

- **WHEN** a combat is in progress
- **THEN** the top bar SHALL show the current act
- **AND** it SHALL show the current combat position inside the act
- **AND** it SHALL show the current level and money
- **AND** it SHALL NOT show a lives counter

#### Scenario: Final act display

- **WHEN** the run reaches the final act
- **THEN** the top bar SHALL show the final-act step or the boss encounter instead of a normal combat position
- **AND** the level and money SHALL remain visible
