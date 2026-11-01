# ai-opponent Specification

## Purpose
The AI opponent system provides a functional opponent that plays according to the same game rules as the player.

## Requirements

### Requirement: AI Game Rule Compliance
The AI SHALL follow all the same game rules as the player.

#### Scenario: AI follows rules
- **WHEN** the AI takes actions
- **THEN** it SHALL respect zone limits (max 4 cards per side)
- **AND** it SHALL follow zone play constraints
- **AND** it SHALL play cards during its play phase
- **AND** card cost SHALL gate its plays against its own available energy

### Requirement: AI Energy Spending
The AI SHALL spend its own per-turn energy pool under the same rules as the player.

#### Scenario: AI follows the pool
- **WHEN** a turn starts
- **THEN** the AI's available energy SHALL equal the turn number, capped at 6
- **AND** the AI's unspent energy SHALL be lost at turn end

#### Scenario: Repeated plays
- **WHEN** the AI's play phase runs
- **THEN** the AI SHALL keep playing cards while it can afford a card and a free slot exists
- **AND** it SHALL choose among affordable cards by highest power per energy spent
- **AND** it SHALL NOT play a card whose cost exceeds its available energy

#### Scenario: Nothing affordable
- **WHEN** no card in the AI's hand can be paid with its available energy
- **THEN** the AI SHALL play nothing
- **AND** this SHALL NOT be treated as an error

### Requirement: AI Card Play
The AI SHALL play cards strategically during combat, limited by its energy pool and free slots.

#### Scenario: AI decision making
- **WHEN** it is the AI's turn to play
- **THEN** the AI SHALL evaluate the cards in its hand that it can afford
- **AND** it SHALL play a card when a free slot exists
- **AND** it SHALL keep playing while it can afford a card and a free slot exists
- **AND** any remaining unaffordable cards SHALL stay in its hand

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
- **AND** it SHALL play affordable cards during the play phase until no affordable card or free slot remains
- **AND** each played card SHALL appear on the board immediately
- **AND** its hand SHALL NOT be rendered
