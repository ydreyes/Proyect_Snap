# Spec Delta

## MODIFIED Requirements

### Requirement: AI Game Rule Compliance

The AI SHALL follow all the same game rules as the player.

#### Scenario: AI follows rules

- **WHEN** the AI takes actions
- **THEN** it SHALL respect zone limits (max 4 cards per side)
- **AND** it SHALL follow zone play constraints
- **AND** it SHALL play cards during its play phase
- **AND** card cost SHALL gate its plays against its own available energy

#### Scenario: AI uses the shared action authority

- **WHEN** the AI plays, moves, destroys, discards, draws, banishes or creates a card
- **THEN** it SHALL go through the same action rules as the player
- **AND** it SHALL NOT mutate zones, hands or piles through a separate path

#### Scenario: AI play events

- **WHEN** the AI plays a card from hand
- **THEN** the same `AfterPlayed` event SHALL be dispatched as for a player card
- **AND** the event SHALL be dispatched once per AI card instance per turn

#### Scenario: AI energy ledger

- **WHEN** the AI destroys, merges or banishes a card it played during the current turn
- **THEN** its available energy SHALL NOT increase
- **AND** when the AI returns a card to its hand during the current turn, its cost SHALL be refunded

#### Scenario: AI capacity for created cards

- **WHEN** an AI effect adds a card to a zone
- **THEN** the four-card limit SHALL apply to the AI side
- **AND** an addition that would exceed the limit SHALL be rejected
