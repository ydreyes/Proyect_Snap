# Spec Delta

## ADDED Requirements

### Requirement: Run Progress Display
The system SHALL display run progress in the combat top bar.

#### Scenario: Combat and lives display
- **WHEN** a combat is in progress
- **THEN** the top bar SHALL show the current combat number out of 15
- **AND** it SHALL show the remaining lives

## MODIFIED Requirements

### Requirement: Energy Indicator
The system SHALL display the player's remaining energy for the current turn.

#### Scenario: Energy progression
- **WHEN** turn n starts
- **THEN** the energy indicator SHALL show the available energy, starting at the smaller of n and 6
- **AND** it SHALL update after every play and refund bound to the current turn
- **AND** it SHALL reset to the turn's pool when a new turn starts

### Requirement: Turn 6 Evaluation
The system SHALL evaluate the game at turn 6.

#### Scenario: Turn 6 completion
- **WHEN** turn 6 ends
- **THEN** the game SHALL evaluate win conditions
- **AND** by default SHALL compare total power of all cards in all zones (player vs AI)
- **AND** equal total power SHALL be treated as a defeat
- **AND** the evaluation system SHALL be extensible for additional win conditions

### Requirement: Victory/Defeat Modal
The system SHALL display results in a modal overlay.

#### Scenario: Modal display
- **WHEN** game is evaluated at turn 6
- **THEN** a modal SHALL appear on top of the combat scene
- **AND** the modal SHALL show "VICTORIA", "DERROTA" or "EMPATE" based on result
- **AND** the modal SHALL display power totals
- **AND** the modal SHALL have a "Continuar" button
- **AND** the modal SHALL block interaction with the board below
- **AND** game state SHALL be preserved while modal is shown

#### Scenario: Continue from modal
- **WHEN** player clicks "Continuar" on the modal
- **THEN** the modal SHALL close
- **AND** the run flow SHALL continue according to the combat result and the combat's position in the run
- **AND** combat scene state SHALL be rebuilt whenever a combat is entered again
