# Spec Delta

## MODIFIED Requirements

### Requirement: Three Zones

The system SHALL provide exactly 3 zones for combat.

#### Scenario: Zone initialization

- **WHEN** combat begins
- **THEN** exactly 3 zones SHALL be created
- **AND** each zone SHALL support up to 4 cards per side (player and AI)
- **AND** each side's 4 slots SHALL be displayed as a 2 row by 2 column grid

#### Scenario: Capacity applies to every source

- **WHEN** a card is added to a zone by a play, a move, a token effect, a copy, a merge or a regeneration
- **THEN** the addition SHALL be rejected if that side already has 4 cards in the zone
- **AND** the board SHALL remain unchanged
- **AND** the effect that attempted the addition SHALL complete without adding the card

### Requirement: Energy Expenditure and Refund

The system SHALL derive each side's available energy from an explicit per-turn ledger of charges and refunds.

#### Scenario: Available energy after a play

- **WHEN** the player plays a card of cost n during the current turn
- **THEN** the available energy SHALL decrease by n
- **AND** the card SHALL record the turn in which it was played

#### Scenario: Cards from earlier turns do not consume energy

- **WHEN** a card was played on an earlier turn and remains in a zone
- **THEN** its cost SHALL NOT reduce the current turn's available energy

#### Scenario: Returning a card refunds its cost

- **WHEN** a card played during the current turn is returned to the player's hand
- **THEN** its cost SHALL be added back to the available energy
- **AND** it SHALL be charged again if replayed

#### Scenario: Moving a card already in play

- **WHEN** a card is moved from one zone slot to another
- **THEN** the available energy SHALL NOT change

#### Scenario: Destroying, merging or banishing does not refund

- **WHEN** a card played during the current turn is destroyed, merged or banished
- **THEN** its cost SHALL remain spent for that turn
- **AND** the available energy SHALL NOT increase

#### Scenario: Lifting a card does not free its cost

- **WHEN** a card played during the current turn is lifted from the board
- **THEN** its cost SHALL remain committed while the drag is active
- **AND** the available energy SHALL NOT increase
- **AND** dropping it into another slot SHALL NOT change its cost
- **AND** returning it to the hand SHALL refund the cost

#### Scenario: Both sides use the ledger

- **WHEN** the AI plays, destroys, merges, banishes, moves or returns a card
- **THEN** the AI's available energy SHALL follow the same charge and refund rules as the player's

### Requirement: Turn Phases

The system SHALL follow a defined turn structure.

#### Scenario: Turn structure

- **WHEN** a turn progresses
- **THEN** it SHALL go through TurnStart (draw for both sides)
- **AND** PlayPhase (time-limited play)
- **AND** TurnEnd (OnReveal resolution, power recalculation, priority and turn advance)

#### Scenario: In-play actions resolve immediately

- **WHEN** a card is played, moved, destroyed, discarded or drawn during PlayPhase
- **THEN** the action's triggered effects SHALL resolve before control returns to the caller
- **AND** the turn SHALL remain in PlayPhase
- **AND** the end of the turn SHALL still resolve OnReveal and pending effects

#### Scenario: Match length

- **WHEN** the sixth turn ends
- **THEN** the match SHALL be evaluated after that turn's OnReveal effects and power recalculation
- **AND** no further turns SHALL start

### Requirement: Card Reveal Ordering

The system SHALL reveal cards in order based on priority and play order.

#### Scenario: Reveal sequence

- **WHEN** cards are revealed at TurnEnd
- **THEN** all cards played during the current turn that are still in play and have not already revealed SHALL be included
- **AND** all cards of the player with priority SHALL be revealed first, in the order they were played
- **AND** then all cards of the other player SHALL be revealed, in the order they were played
- **AND** OnReveal effects SHALL trigger in this reveal order
- **AND** each card's OnReveal effect SHALL resolve before the next card in the sequence

#### Scenario: Reveal is instantaneous

- **WHEN** the player ends the turn
- **THEN** the reveal sequence and its effects SHALL resolve in the same frame
- **AND** no extra phase, input block or animation SHALL be required
- **AND** the next turn SHALL start after resolution

#### Scenario: Cards reveal once

- **WHEN** a card has already been revealed on an earlier turn
- **THEN** it SHALL NOT be included in a later reveal sequence
- **AND** its OnReveal effect SHALL NOT trigger again

#### Scenario: Returning to hand does not re-arm reveal

- **WHEN** a revealed card returns to hand and is played again during the same combat
- **THEN** it SHALL NOT be included in a later reveal sequence
- **AND** its OnReveal effect SHALL NOT trigger again

#### Scenario: Card leaves play before its reveal

- **WHEN** a card is destroyed, discarded or otherwise leaves play before its reveal
- **THEN** it SHALL NOT be included in the reveal sequence
- **AND** the sequence SHALL continue with the next card
