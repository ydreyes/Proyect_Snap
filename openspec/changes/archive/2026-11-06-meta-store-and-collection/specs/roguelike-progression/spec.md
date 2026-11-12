# Spec Delta

## MODIFIED Requirements

### Requirement: Basic Run Summary

The system SHALL show a run summary when the run ends, whether won or lost, and award global points once.

#### Scenario: Summary content

- **WHEN** the run summary is shown
- **THEN** it SHALL show the outcome, the final level, experience earned, money earned, money left unspent, the number of combats won and the final deck
- **AND** the final deck SHALL show each card's effective cost and power
- **AND** it SHALL show the global points breakdown and total

#### Scenario: Summary actions

- **WHEN** the player views the run summary
- **THEN** the system SHALL offer `Menu principal` and `Re-run`
- **AND** `Menu principal` SHALL return to the main menu and delete the finished `run.json`
- **AND** `Re-run` SHALL delete the finished `run.json` and start a new run with reset experience, level, money and deck

#### Scenario: Summary after defeat

- **WHEN** the run ends because of a lost or tied combat
- **THEN** the summary SHALL show the same content as a victory summary
- **AND** the outcome SHALL indicate the defeat
- **AND** the points award SHALL include the combats and levels earned before the defeat

#### Scenario: Points award

- **WHEN** the summary is shown for a run that was not awarded
- **THEN** the system SHALL add the `meta-progression` award to the profile
- **AND** it SHALL store the run id in `lastAwardedRunId`
- **AND** reopening the summary SHALL NOT add the award again
