# game-framework Specification

## Purpose
The game framework provides core infrastructure for the SnapClone game including scene management, state machine, services, and primitive UI drawing using only built-in MonoGame capabilities with no image assets.

## Requirements

### Requirement: Scene Management System
The system SHALL provide a scene management system that allows switching between different game scenes.

#### Scenario: Scene transition
- **WHEN** the game needs to change from one scene to another
- **THEN** the scene manager SHALL deactivate the current scene and activate the new scene
- **AND** the new scene SHALL be updated and drawn

### Requirement: State Machine
The system SHALL implement a state machine for game flow control.

#### Scenario: State transitions
- **WHEN** game state changes occur
- **THEN** the state machine SHALL transition between states appropriately
- **AND** each state SHALL have enter, update, and exit lifecycle methods

### Requirement: Core Services
The system SHALL provide core services accessible throughout the game.

#### Scenario: Service access
- **WHEN** any system needs input, time, or random functionality
- **THEN** it SHALL access them through a centralized service container
- **AND** services SHALL be initialized before use

### Requirement: Primitive UI Drawing
The system SHALL provide UI drawing capabilities using MonoGame primitives and textures generated in memory, with no required external image assets.

#### Scenario: Draw UI elements
- **WHEN** drawing UI elements like cards, zones, or menus
- **THEN** the system SHALL use SpriteBatch with primitive shapes (rectangles, borders) and procedurally generated textures
- **AND** no external image asset SHALL be required to render any UI element
- **AND** text SHALL be rendered with the shared typography system

#### Scenario: Optional card art asset
- **WHEN** a card has a provided art asset
- **THEN** the art layer MAY use that asset
- **AND** the frame, badges, text and layout SHALL remain the same as the procedural version

### Requirement: Game Loop Integration
The system SHALL integrate all systems into MonoGame's Game class.

#### Scenario: Game initialization
- **WHEN** the game starts
- **THEN** Game1 SHALL initialize all core systems
- **AND** the initial scene SHALL be loaded
- **AND** the game loop SHALL run properly
