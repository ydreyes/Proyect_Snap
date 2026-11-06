# Spec Delta

## Purpose

Lets SnapClone run on an Android phone from the same sources and content as the desktop build, with an installable APK, a proper mobile presentation and finger input that behaves like the existing pointer.

## ADDED Requirements

### Requirement: Installable Android Build

The system SHALL produce an installable Android package from the same game sources and content used by the desktop build.

#### Scenario: Debug APK is produced

- **WHEN** a developer builds the Android head in Debug configuration
- **THEN** an APK SHALL be produced for the `arm64-v8a` ABI
- **AND** its application id SHALL be `com.integritysolution.snapclone`
- **AND** its minimum supported Android version SHALL be API 24
- **AND** its target Android version SHALL be API 35
- **AND** the APK SHALL be signed with the debug key so it can be sideloaded

#### Scenario: Desktop build stays independent

- **WHEN** a developer builds the desktop solution on a machine without the Android workload, Android SDK or JDK
- **THEN** the desktop build SHALL succeed
- **AND** it SHALL NOT require the Android head to be restored or compiled
- **AND** the existing desktop executable SHALL keep its current behavior

#### Scenario: Single source of content

- **WHEN** the Android head is built
- **THEN** it SHALL compile the existing `Content.mgcb` rather than a copy
- **AND** no game content file SHALL be duplicated between the desktop and Android projects
- **AND** adding an asset to that manifest SHALL make it available to both platforms

### Requirement: Android Host Lifecycle

The system SHALL host the game inside an Android activity that preserves game state across normal application lifecycle events.

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

#### Scenario: Back navigation

- **WHEN** the user presses the system back gesture or button while the game is running
- **THEN** the application SHALL exit
- **AND** no partially saved or corrupted run state SHALL remain

### Requirement: Touch Pointer Parity

The system SHALL treat touch input as the single pointer used by the game, with the same press, move and release semantics as the desktop mouse.

#### Scenario: Tap

- **WHEN** the user touches a card and lifts the finger without moving beyond the tap threshold
- **THEN** the game SHALL register a press followed by a release at the same position
- **AND** the tap SHALL produce the same result as a mouse click, including opening the card detail panel

#### Scenario: Drag

- **WHEN** the user touches a card and moves the finger beyond the tap threshold
- **THEN** the game SHALL register a drag with the finger position following the pointer
- **AND** releasing the finger SHALL perform the same drop validation as a mouse release

#### Scenario: Single pointer authority

- **WHEN** more than one finger touches the screen
- **THEN** only the first active touch SHALL drive the pointer
- **AND** additional touches SHALL NOT start a second drag or duplicate a click
- **AND** the touch panel SHALL be sampled once per frame so held touches remain valid for the whole frame

#### Scenario: Interrupted touch

- **WHEN** the touch is cancelled by the system, for example by a gesture or an incoming call overlay
- **THEN** the game SHALL register the pointer release
- **AND** any card being dragged SHALL return to its origin
- **AND** no drag SHALL remain active after the interruption

### Requirement: Mobile Presentation

The system SHALL present the game as a portrait, edge-to-edge mobile application.

#### Scenario: Portrait lock

- **WHEN** the device is rotated
- **THEN** the game SHALL remain in portrait orientation
- **AND** the layout SHALL NOT rotate

#### Scenario: Immersive display

- **WHEN** the game is in the foreground
- **THEN** the status and navigation bars SHALL be hidden
- **AND** the hidden state SHALL be restored after the user reveals the system bars with a swipe
- **AND** the game SHALL continue to render edge to edge underneath the system bars while they are hidden

#### Scenario: Screen stays on

- **WHEN** the game is running and the user is not interacting
- **THEN** the display SHALL NOT turn off because of the inactivity timeout
- **AND** the screen SHALL be allowed to turn off again when the application leaves the foreground

### Requirement: Repeatable Build And Install

The system SHALL provide documented, repeatable commands to build and install the Android package.

#### Scenario: Build script

- **WHEN** a developer runs the Android build script
- **THEN** it SHALL build the Android head in Debug configuration
- **AND** it SHALL print the absolute path of the resulting APK
- **AND** it SHALL exit with a non-zero code and a readable message when the required workload or JDK is missing

#### Scenario: Install script

- **WHEN** a developer runs the Android install script with a device connected over USB debugging
- **THEN** it SHALL install the debug APK on that device and launch the application
- **AND** when no device is connected it SHALL exit with a non-zero code and a message naming the missing device

#### Scenario: Deterministic toolchain

- **WHEN** the repository is built on a machine with several .NET SDKs installed
- **THEN** the build SHALL use the SDK version pinned by the repository
- **AND** the pinned version SHALL support both the desktop and the Android targets
