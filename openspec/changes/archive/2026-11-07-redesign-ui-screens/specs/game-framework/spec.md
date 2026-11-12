# Spec Delta

## MODIFIED Requirements

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
