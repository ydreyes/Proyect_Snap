# Spec Delta

## MODIFIED Requirements

### Requirement: Pile Viewer Placement

The system SHALL anchor the viewer to the avatar it belongs to and scale it with the viewport.

#### Scenario: Player side placement

- **WHEN** the player's viewer opens
- **THEN** it SHALL appear below the player avatar, aligned to the avatar's left edge

#### Scenario: Rival side placement

- **WHEN** the rival's viewer opens
- **THEN** it SHALL appear below the rival avatar, aligned to the avatar's right edge

#### Scenario: Scaling

- **WHEN** the window size changes while the viewer is open
- **THEN** the viewer SHALL keep its position relative to its avatar
- **AND** its text SHALL stay legible using the shared typography system
