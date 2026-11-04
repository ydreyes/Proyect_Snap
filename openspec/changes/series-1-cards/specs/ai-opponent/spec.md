# Spec Delta

## ADDED Requirements

### Requirement: AI Series 1 Action Use

The AI SHALL use the same action authority as the player for every Series 1 action.

#### Scenario: New actions

- **WHEN** the AI destroys, discards, moves, copies, merges, banishes or creates a token
- **THEN** it SHALL use the same action rules, capacity checks, energy ledger and event dispatch as the player
- **AND** it SHALL NOT mutate zones, hands or piles through a separate path

#### Scenario: Hand cap

- **WHEN** the AI's hand has 7 cards
- **THEN** its turn draw SHALL be skipped and the card SHALL stay in its deck
- **AND** its effects that add or copy to hand SHALL not create the card

#### Scenario: Random targets

- **WHEN** an AI card has multiple eligible targets and its text does not define the set
- **THEN** the AI SHALL choose the target randomly under the same rule as the player
- **AND** the choice SHALL be deterministic for a seeded random service

### Requirement: AI Series 1 Safety

The AI SHALL avoid playing a card whose mandatory effect would make its own position worse when another affordable card is available.

#### Scenario: Self-damaging discard or destroy

- **WHEN** the AI can afford a card with a mandatory discard or destroy of its own cards
- **AND** another affordable card does not have that drawback
- **THEN** the AI SHALL prefer the safer card
- **AND** it SHALL still play the damaging card when it is the only affordable option

#### Scenario: No legal target

- **WHEN** an AI card's mandatory effect needs a target that does not exist
- **THEN** the AI SHALL still be able to play the card
- **AND** the effect SHALL resolve without a target according to the card's text

#### Scenario: No crash on new content

- **WHEN** the AI plays or resolves any of the 46 Series 1 cards
- **THEN** the combat SHALL continue without an exception
- **AND** the AI's hand, board and energy SHALL remain consistent
