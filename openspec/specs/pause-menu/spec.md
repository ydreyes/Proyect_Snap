# pause-menu Specification

## Purpose

Defines the pause overlay, the adaptive actions available at safe nodes and in combat, the back-button routing and the main-menu continue flow.

## Requirements

### Requirement: Pause Overlay

The system SHALL provide a pause overlay in every run scene.

#### Scenario: Opening the pause

- **WHEN** the player presses the pause control, Escape on desktop or back on Android while in a run scene
- **THEN** the pause overlay SHALL open on top of the current scene
- **AND** the scene below SHALL stop updating
- **AND** the turn timer SHALL stop counting down
- **AND** the AI SHALL NOT take another action while the overlay is open

#### Scenario: Resuming

- **WHEN** the player chooses `Reanudar`
- **THEN** the overlay SHALL close
- **AND** the scene SHALL resume from the same state
- **AND** the turn timer SHALL resume from the remaining time, not from zero

#### Scenario: Pause across run scenes

- **WHEN** the run is in `Draft`, `EnemySelect`, `Combat`, `LevelUp`, `Shop`, `Event`, `ActTransition` or `SubBossIntro`
- **THEN** the pause overlay SHALL be available
- **AND** `Summary` and `Menu` SHALL NOT show the pause overlay

### Requirement: Safe-Node Pause Actions

The system SHALL show the checkpoint actions that apply to a safe node.

#### Scenario: Safe-node buttons

- **WHEN** the pause opens at a safe node
- **THEN** it SHALL offer `Reanudar`, `Guardar y salir` and `Abandonar run`
- **AND** `Guardar y salir` SHALL write the checkpoint and return to the main menu
- **AND** `Abandonar run` SHALL require a second confirmation
- **AND** confirming `Abandonar run` SHALL delete `run.json`

#### Scenario: Save failure

- **WHEN** `Guardar y salir` cannot write the checkpoint
- **THEN** the overlay SHALL show an error
- **AND** it SHALL NOT return to the menu
- **AND** the previous checkpoint SHALL remain intact

### Requirement: Combat Pause Actions

The system SHALL show a different action set while a combat is in progress.

#### Scenario: Combat buttons

- **WHEN** the pause opens during a combat
- **THEN** it SHALL offer `Reanudar`, `Salir al menú` and `Abandonar run`
- **AND** it SHALL NOT offer `Guardar y salir`
- **AND** `Salir al menú` SHALL keep the pre-combat `pendingCombat` checkpoint
- **AND** the next `Continuar` SHALL resume a new combat against the same enemy

#### Scenario: Abandon from combat

- **WHEN** the player confirms `Abandonar run` during a combat
- **THEN** `run.json` SHALL be deleted
- **AND** the system SHALL return to the main menu

### Requirement: Back Routing

The system SHALL route desktop and Android back input through the active scene.

#### Scenario: In a run scene

- **WHEN** the player presses Escape or Android back while in a run scene
- **THEN** the pause overlay SHALL toggle
- **AND** the app SHALL NOT exit
- **AND** the back event SHALL be consumed once

#### Scenario: In the menu

- **WHEN** the player presses Escape or Android back in the main menu
- **THEN** the app SHALL exit
- **AND** an active checkpoint SHALL remain intact

#### Scenario: In the summary

- **WHEN** the player presses Escape or Android back in the run summary
- **THEN** the system SHALL return to the main menu
- **AND** the finished run SHALL remain available as `Ver resumen` until the player chooses `Menu principal` or `Re-run`

### Requirement: Main Menu Continue Flow

The system SHALL make the last run reachable from the main menu.

#### Scenario: Active run

- **WHEN** `run.json.status` is `Active`
- **THEN** the menu SHALL show `Continuar`
- **AND** choosing it SHALL resume at the checkpoint's pending node
- **AND** it SHALL NOT start a new draft

#### Scenario: Ended run

- **WHEN** `run.json.status` is `Ended`
- **THEN** the menu SHALL show `Ver resumen` in the same slot
- **AND** choosing it SHALL open the run summary
- **AND** `Menu principal` or `Re-run` from that summary SHALL delete `run.json`

#### Scenario: No run

- **WHEN** there is no `run.json`
- **THEN** the menu SHALL show `Nueva run` only
- **AND** there SHALL be no continue or summary action

### Requirement: Checkpoint on Exit

The system SHALL preserve the last safe state when the app closes.

#### Scenario: Closing at a safe node

- **WHEN** the app closes while the run is at a safe node
- **THEN** the checkpoint SHALL be written before the process ends
- **AND** discovery SHALL be flushed to the profile

#### Scenario: Closing during combat

- **WHEN** the app closes while a combat is in progress
- **THEN** the pre-combat checkpoint SHALL remain
- **AND** the live combat SHALL NOT be written
- **AND** no result SHALL be recorded
