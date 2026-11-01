# Spec Delta

## MODIFIED Requirements

### Requirement: Turn Counter
The system SHALL track the current turn number out of six.

#### Scenario: Turn tracking
- **WHEN** turns progress
- **THEN** the turn counter SHALL increment on each turn
- **AND** the current turn SHALL be displayed as the current number out of six
- **AND** the end-turn button SHALL show the current turn number

### Requirement: End Turn Button
The system SHALL provide an explicit button to end turns.

#### Scenario: End turn action
- **WHEN** in PlayPhase
- **THEN** a "FINALIZAR TURNO n/6" button SHALL be visible and enabled
- **AND** clicking the button SHALL end the turn
- **AND** the turn SHALL advance to the next one

#### Scenario: Timer closes the turn
- **WHEN** the play phase timer reaches zero
- **THEN** the turn SHALL end without requiring the button
- **AND** any active drag SHALL be cancelled

## ADDED Requirements

### Requirement: Energy Indicator
The system SHALL display an energy indicator for the current turn.

#### Scenario: Energy progression
- **WHEN** turn n starts
- **THEN** the energy indicator SHALL show the smaller of n and 6
- **AND** the indicator SHALL NOT block any card play
- **AND** the displayed energy SHALL NOT be consumed or refunded
