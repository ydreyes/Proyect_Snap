# Spec Delta

## MODIFIED Requirements

### Requirement: Run State Tracking

The system SHALL track run progress and its checkpoint.

#### Scenario: Progress tracking

- **WHEN** a run is in progress
- **THEN** the system SHALL track the act, the combat index inside the act, the total combat index, experience, level, money, the deck and the pending level-up queue
- **AND** it SHALL track the shop/event chance, the final-act step and the pending node
- **AND** it SHALL track the pending combat snapshot while a combat is being played
- **AND** this state SHALL persist through the run
- **AND** run state SHALL be reset when a new run starts

#### Scenario: Combat index bounds

- **WHEN** a new act starts
- **THEN** the combat index inside the act SHALL start at 0
- **AND** it SHALL never exceed 15 for acts 1, 2 and 3
- **AND** the final act SHALL NOT use a normal combat index

#### Scenario: Checkpoint state

- **WHEN** a run is checkpointed
- **THEN** the checkpoint SHALL store the pending node and every field needed to rebuild it
- **AND** it SHALL store `status` as `Active` or `Ended`
- **AND** it SHALL NOT store the live board, hand or piles
- **AND** the checkpoint SHALL be the only source used by `Continuar`

### Requirement: Basic Run Summary

The system SHALL show a run summary when the run ends, whether won or lost, and clear the finished run on exit.

#### Scenario: Summary content

- **WHEN** the run summary is shown
- **THEN** it SHALL show the outcome, the final level, experience earned, money earned, money left unspent, the number of combats won and the final deck
- **AND** the final deck SHALL show each card's effective cost and power
- **AND** the summary SHALL NOT award or display global points in this change

#### Scenario: Summary actions

- **WHEN** the player views the run summary
- **THEN** the system SHALL offer `Menu principal` and `Re-run`
- **AND** `Menu principal` SHALL return to the main menu and delete the finished `run.json`
- **AND** `Re-run` SHALL delete the finished `run.json` and start a new run with reset experience, level, money and deck

#### Scenario: Summary after defeat

- **WHEN** the run ends because of a lost or tied combat
- **THEN** the summary SHALL show the same content as a victory summary
- **AND** the outcome SHALL indicate the defeat
