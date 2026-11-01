# Spec Delta

## MODIFIED Requirements

### Requirement: AI Game Rule Compliance
The AI SHALL follow all the same game rules as the player.

#### Scenario: AI follows rules
- **WHEN** the AI takes actions
- **THEN** it SHALL respect zone limits (max 4 cards per side)
- **AND** it SHALL follow zone play constraints
- **AND** it SHALL play cards during its play phase
- **AND** card cost SHALL NOT block its play while energy is informational

### Requirement: AI Card Play
The AI SHALL play exactly one card per turn when a free slot exists.

#### Scenario: AI decision making
- **WHEN** it is the AI's turn to play
- **THEN** the AI SHALL evaluate the cards in its hand
- **AND** it SHALL play exactly one card
- **AND** any remaining playable cards SHALL stay in its hand

#### Scenario: Zone spread
- **WHEN** the AI chooses where to play
- **THEN** it SHALL prefer the zone where it already has the fewest cards
- **AND** ties SHALL be resolved randomly
- **AND** it SHALL place the card in a free slot of that zone

#### Scenario: No free slot
- **WHEN** all 12 AI slots are occupied
- **THEN** the AI SHALL play nothing for that turn

### Requirement: AI Turn Participation
The AI SHALL participate fully in all turn phases.

#### Scenario: AI turn flow
- **WHEN** a turn occurs
- **THEN** the AI SHALL draw 1 card at turn start if its deck has cards
- **AND** it SHALL play exactly one card during the play phase
- **AND** its played card SHALL appear on the board immediately
- **AND** its hand SHALL NOT be rendered
