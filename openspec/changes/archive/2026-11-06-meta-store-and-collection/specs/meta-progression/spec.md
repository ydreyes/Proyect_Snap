# Spec Delta

## Purpose

Defines the persistent global points earned by a run, the award breakdown and the once-per-run rule that prevents duplicate awards.

## ADDED Requirements

### Requirement: Global Points Balance

The system SHALL persist a global points balance in the profile.

#### Scenario: Balance rules

- **WHEN** the profile is loaded
- **THEN** the points balance SHALL be a non-negative integer
- **AND** it SHALL increase only through a run award
- **AND** it SHALL decrease only through a store purchase
- **AND** it SHALL never become negative

#### Scenario: New profile

- **WHEN** a first-run profile is created
- **THEN** the points balance SHALL be 0
- **AND** the last awarded run id SHALL be empty

### Requirement: Run Points Award

The system SHALL award global points when a run ends.

#### Scenario: Award formula

- **WHEN** a run ends
- **THEN** the award SHALL be `3 × normal combats won + 40 × sub-bosses won + 8 × final level`
- **AND** it SHALL add 150 points when the final boss was defeated
- **AND** a defeat SHALL still award the points earned before the defeat
- **AND** the level term SHALL use the final level reached

#### Scenario: Breakdown

- **WHEN** the summary shows the award
- **THEN** it SHALL list normal combats, sub-bosses, level and the final-boss bonus separately
- **AND** it SHALL show the total award
- **AND** the sum of the lines SHALL equal the total

#### Scenario: Award once

- **WHEN** the summary is opened for a run id that was already awarded
- **THEN** the points SHALL NOT be added again
- **AND** the summary SHALL indicate that the run was already collected
- **AND** `profile.lastAwardedRunId` SHALL be updated only after a successful profile write

#### Scenario: Write failure

- **WHEN** the profile write fails during the award
- **THEN** the points SHALL NOT be considered awarded
- **AND** the summary SHALL show an error and allow a retry
- **AND** a retry SHALL not double-award

### Requirement: Points Entry Points

The system SHALL expose the points balance outside the summary.

#### Scenario: Main menu

- **WHEN** the main menu is shown
- **THEN** the current points balance SHALL be visible
- **AND** it SHALL update after a run award or a store purchase
- **AND** it SHALL fit at the smallest supported viewport
