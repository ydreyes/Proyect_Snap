# Spec Delta

## MODIFIED Requirements

### Requirement: Android Host Lifecycle

The system SHALL host the game inside an Android activity that preserves game state across normal application lifecycle events and routes back input through the game.

#### Scenario: Launch

- **WHEN** the user launches the application
- **THEN** the game view SHALL fill the display
- **AND** the game SHALL start at the same initial scene as on desktop
- **AND** no desktop-only window operation SHALL run

#### Scenario: Background and resume

- **WHEN** the application goes to the background and later returns to the foreground
- **THEN** the game SHALL pause while in the background
- **AND** it SHALL resume at the scene and turn state it had before
- **AND** the active scene SHALL NOT be reset
- **AND** a safe-node checkpoint SHALL be written before the process is suspended

#### Scenario: Back navigation

- **WHEN** the user presses the system back gesture or button while the game is running
- **THEN** the activity SHALL forward the request to the game back service
- **AND** the active scene SHALL decide whether to open or close the pause overlay, return to the menu or exit
- **AND** the activity SHALL NOT call the default back behavior while a run is active
- **AND** no partially saved or corrupted run state SHALL remain

#### Scenario: Lifecycle checkpoint

- **WHEN** the activity is paused or stopped while a run is in progress
- **THEN** the game SHALL write the checkpoint if the current node is safe
- **AND** it SHALL keep the pre-combat checkpoint if the current node is combat
- **AND** it SHALL flush discovery to the profile
