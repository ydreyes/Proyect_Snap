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
- **THEN** every text SHALL remain legible using the default font
- **AND** every control SHALL remain reachable with a finger
- **AND** no layout region SHALL overlap another region
