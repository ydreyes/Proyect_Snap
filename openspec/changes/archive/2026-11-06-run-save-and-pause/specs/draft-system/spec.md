# Spec Delta

## MODIFIED Requirements

### Requirement: Initial Draft

The system SHALL build the initial 8-card deck through eight rounds of card selection and SHALL persist each round as a safe node.

#### Scenario: Draft selection

- **WHEN** starting a new run
- **THEN** the player SHALL be shown 3 card choices for the current round
- **AND** the player SHALL pick exactly 1 card from those 3
- **AND** the next round SHALL present 3 new choices
- **AND** this process SHALL repeat until 8 cards are selected

#### Scenario: Draft progress

- **WHEN** the player is drafting
- **THEN** the scene SHALL display how many cards have been picked out of 8
- **AND** the continue action SHALL remain disabled until 8 cards are selected

#### Scenario: New run reset

- **WHEN** the player starts a new run after a previous one
- **THEN** previously drafted cards SHALL be discarded
- **AND** draft progress SHALL restart at 0 of 8

#### Scenario: Draft checkpoint

- **WHEN** the player makes a draft pick
- **THEN** the draft state SHALL be written to `run.json`
- **AND** the state SHALL include the phase, step index, groups, current offers, picks and copy counts
- **AND** the write SHALL complete before the next round is drawn

#### Scenario: Draft resume

- **WHEN** the app is reopened with a draft checkpoint
- **THEN** `Continuar` SHALL resume the same round
- **AND** the current offers SHALL be the same
- **AND** the draft SHALL NOT reroll the groups or the offers
- **AND** completing the draft SHALL clear the draft state from the checkpoint
