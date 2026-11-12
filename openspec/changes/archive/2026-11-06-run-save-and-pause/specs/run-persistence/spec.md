# Spec Delta

## Purpose

Defines the two persistent files of a run, the safe-node checkpoint, the resume rules for combat and draft, and the discovery data that survives a run.

## ADDED Requirements

### Requirement: Separate Profile and Run Files

The system SHALL persist meta state and run state in separate files.

#### Scenario: File roles

- **WHEN** persistence is initialized
- **THEN** `profile.json` SHALL hold meta state
- **AND** `run.json` SHALL hold the active or last-finished run
- **AND** the two files SHALL be written independently
- **AND** a corrupt run file SHALL NOT prevent the profile from loading

#### Scenario: Schema version

- **WHEN** either file is written
- **THEN** it SHALL contain a `schemaVersion` field
- **AND** a reader SHALL reject a version it does not understand without crashing
- **AND** a missing file SHALL be treated as an empty profile or no run

### Requirement: Atomic Writes and Backup Recovery

The system SHALL write both files without leaving a partially written document.

#### Scenario: Atomic write

- **WHEN** a file is saved
- **THEN** the new content SHALL be written to a temporary file first
- **AND** the temporary file SHALL replace the target only after the write succeeds
- **AND** the previous target SHALL be kept as a `.bak` file
- **AND** a failed write SHALL leave the previous target intact

#### Scenario: Load with backup

- **WHEN** the main file is missing, malformed or has an unsupported schema version
- **THEN** the system SHALL try the `.bak` file
- **AND** it SHALL load the newest valid document
- **AND** if both files are invalid it SHALL start with an empty profile and no run
- **AND** it SHALL NOT crash or delete the invalid files automatically

### Requirement: Safe-Node Checkpoints

The system SHALL write `run.json` only at safe nodes.

#### Scenario: Safe node list

- **WHEN** the run reaches a node
- **THEN** `Draft`, `EnemySelect`, `LevelUp`, `Shop`, `Event`, `ActTransition` and `SubBossIntro` SHALL be safe nodes
- **AND** `Combat` SHALL NOT be a safe node
- **AND** a checkpoint SHALL be written on entering or completing a safe node
- **AND** no checkpoint SHALL be written while a combat is being played

#### Scenario: Run status

- **WHEN** a checkpoint is written
- **THEN** `status` SHALL be `Active` while the run can continue
- **AND** it SHALL be `Ended` after a victory or a defeat
- **AND** `Ended` SHALL preserve the summary data until the player leaves the summary

#### Scenario: New run with an active checkpoint

- **WHEN** the player starts a new run while `run.json.status` is `Active`
- **THEN** the system SHALL ask for confirmation
- **AND** confirming SHALL replace the old checkpoint with the new run
- **AND** cancelling SHALL keep the old checkpoint

### Requirement: Pending Combat Resume

The system SHALL resume an interrupted combat without saving the board.

#### Scenario: Mid-combat interruption

- **WHEN** the app closes or the player leaves the menu while a combat is in progress
- **THEN** `run.json` SHALL keep the pre-combat `pendingCombat` snapshot
- **AND** it SHALL NOT contain the live board, hand or piles
- **AND** `Continuar` SHALL start a new combat against the same enemy
- **AND** the new combat SHALL use a new seed
- **AND** the enemy composition and attempt number SHALL be preserved

#### Scenario: Result is applied once

- **WHEN** a resumed combat ends
- **THEN** the result SHALL be applied exactly once to the run
- **AND** the checkpoint SHALL be written before the result modal is dismissed
- **AND** re-entering the same attempt SHALL NOT grant rewards twice

### Requirement: Draft Checkpoint

The system SHALL resume a draft at the same round.

#### Scenario: Draft state

- **WHEN** a draft is checkpointed
- **THEN** it SHALL store the phase, the step index, the three themed groups, the current offers, the picks and the copy counts
- **AND** every pick SHALL update the checkpoint
- **AND** closing and reopening the app SHALL resume the same round with the same offers
- **AND** the draft SHALL NOT reroll because the scene was re-entered

#### Scenario: Draft completion

- **WHEN** the 8th pick is made
- **THEN** the completed deck SHALL be written before the combat node opens
- **AND** the draft state SHALL be cleared from the checkpoint

### Requirement: Discovery Flush

The system SHALL accumulate discovery in the run and flush it to the profile at safe points.

#### Scenario: Discovery sources

- **WHEN** the player sees a card in the draft, the in-game shop, a level-up offer or an enemy play
- **THEN** the card SHALL be added to the run's discovered card set
- **AND** seeing the same card again SHALL NOT duplicate the entry
- **AND** the bundle preview SHALL NOT discover cards

#### Scenario: Flush points

- **WHEN** a safe-node checkpoint is written or the app closes
- **THEN** the run's discovered card, perk and enemy sets SHALL be merged into `profile.json`
- **AND** the merge SHALL preserve ids already in the profile
- **AND** a failure to write the profile SHALL be reported without discarding the run checkpoint

### Requirement: Profile Schema

The system SHALL persist the meta fields needed by the later store and collection changes.

#### Scenario: Profile fields

- **WHEN** the profile is saved
- **THEN** it SHALL contain global points, owned card ids, discovered card ids, discovered perk ids, discovered enemy ids, perk copy counts, item counts, the last awarded run id and the store rotation state
- **AND** the first-run profile SHALL start with empty discovery, no owned cards beyond the starter set and zero points
- **AND** `lastAwardedRunId` SHALL be reserved for the meta-points change
- **AND** the profile SHALL be written atomically with the same backup rules as the run file
