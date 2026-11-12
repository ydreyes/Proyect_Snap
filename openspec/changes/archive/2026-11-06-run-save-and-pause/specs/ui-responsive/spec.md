# Spec Delta

## ADDED Requirements

### Requirement: Pause and Main Menu Layout

The system SHALL lay out the pause overlay and the redesigned main menu in the existing portrait layout.

#### Scenario: Pause overlay

- **WHEN** the pause overlay is shown
- **THEN** it SHALL cover the scene with a dimmed background
- **AND** its buttons SHALL be reachable with a finger at the smallest supported viewport
- **AND** the number of buttons SHALL match the current node type
- **AND** no button SHALL be shown disabled for a missing action

#### Scenario: Pause button

- **WHEN** the player is in a run scene
- **THEN** a pause control SHALL be visible in a consistent corner
- **AND** it SHALL not overlap the hand, the top bar or the end-turn control
- **AND** it SHALL remain reachable at the smallest supported viewport

#### Scenario: Main menu

- **WHEN** the main menu is shown
- **THEN** `Continuar` and `Ver resumen` SHALL occupy the same slot
- **AND** `Nueva run`, `Tienda`, `Coleccion` and `Salir` SHALL remain reachable
- **AND** the layout SHALL not overlap the title or the global points
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Save error

- **WHEN** a save fails inside the pause overlay
- **THEN** the error message SHALL be legible
- **AND** the overlay SHALL remain open
- **AND** the retry and abandon controls SHALL remain reachable
