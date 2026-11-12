# Spec Delta

## MODIFIED Requirements

### Requirement: Adaptive Layout System
The system SHALL provide a layout manager that calculates positions using percentages of the drawable area, where the drawable area excludes the platform safe-area insets.

#### Scenario: Layout calculation
- **WHEN** the screen size changes or game initializes
- **THEN** the LayoutManager SHALL calculate all UI regions based on percentages
- **AND** all positions SHALL scale proportionally
- **AND** the layout SHALL work on any screen size

#### Scenario: Safe-area insets
- **WHEN** the game runs on a device whose display has a cutout, rounded corners or a system gesture area
- **THEN** the layout SHALL reserve the top, bottom, left and right safe-area insets reported by the platform
- **AND** no control, plaque or hand card SHALL be drawn or hit-tested inside those insets
- **AND** the remaining area SHALL still be divided using the portrait percentages

#### Scenario: Smallest supported phone viewport
- **WHEN** the drawable area is at least 720 by 1280 pixels in portrait orientation
- **THEN** every text SHALL remain legible using the shared typography system
- **AND** every control SHALL remain reachable with a finger
- **AND** no layout region SHALL overlap another region

### Requirement: Detail Panel Layout

The system SHALL place the card and zone detail panels over the combat board using percentage-based geometry.

#### Scenario: Panel placement
- **WHEN** a detail panel opens
- **THEN** it SHALL cover the board area and dim what is behind it
- **AND** its bounds SHALL scale with the viewport
- **AND** its close control SHALL be reachable at every supported window size

#### Scenario: Panel content fits

- **WHEN** the card or zone panel content is drawn at the smallest supported window size
- **THEN** the card, its badges and its descriptive text SHALL stay inside the panel
- **AND** the descriptive text SHALL remain legible using the shared typography system

### Requirement: Text Rendering
The system SHALL display all texts using the shared typography system, which provides display, UI and body roles.

#### Scenario: Text display
- **WHEN** displaying text in any scene
- **THEN** text SHALL be rendered with the shared typography roles
- **AND** the system SHALL fall back to the existing default font when the shared fonts cannot be loaded
- **AND** all UI labels SHALL be visible
