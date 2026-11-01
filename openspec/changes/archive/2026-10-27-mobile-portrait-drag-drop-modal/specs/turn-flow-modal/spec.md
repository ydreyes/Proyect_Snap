# Spec Delta

## Purpose

The turn flow and modal system manages turn counting, explicit turn ending, win condition evaluation, and result display.

## ADDED Requirements

### Requirement: Turn Counter
The system SHALL track the current turn number.

#### Scenario: Turn tracking
- **WHEN** turns progress
- **THEN** the turn counter SHALL increment on each turn
- **AND** the current turn SHALL be displayed

### Requirement: End Turn Button
The system SHALL provide an explicit button to end turns.

#### Scenario: End turn action
- **WHEN** in PlayPhase
- **THEN** an "Finalizar Turno" button SHALL be visible and enabled
- **AND** clicking the button SHALL end the turn
- **AND** turn end logic SHALL execute (priority, reveals, effects)

### Requirement: Turn 6 Evaluation
The system SHALL evaluate the game at turn 6.

#### Scenario: Turn 6 completion
- **WHEN** turn 6 ends
- **THEN** the game SHALL evaluate win conditions
- **AND** by default SHALL compare total power of all cards in all zones (player vs AI)
- **AND** the evaluation system SHALL be extensible for additional win conditions

### Requirement: Victory/Defeat Modal
The system SHALL display results in a modal overlay.

#### Scenario: Modal display
- **WHEN** game is evaluated at turn 6
- **THEN** a modal SHALL appear on top of the combat scene
- **AND** the modal SHALL show "VICTORIA" or "DERROTA" based on result
- **AND** the modal SHALL display power totals
- **AND** the modal SHALL have a "Continuar" button
- **AND** the modal SHALL block interaction with the board below
- **AND** game state SHALL be preserved while modal is shown

#### Scenario: Continue from modal
- **WHEN** player clicks "Continuar" on the modal
- **THEN** the modal SHALL close
- **AND** the game SHALL proceed with the normal run flow
- **AND** combat scene state SHALL be handled appropriately
