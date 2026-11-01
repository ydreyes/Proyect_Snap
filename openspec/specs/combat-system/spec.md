# combat-system Specification

## Purpose
The combat system manages zone-based battles with priority calculation, card play, turn phases, time limits, and proper reveal ordering of cards based on priority and play order.

## Requirements

### Requirement: Three Zones
The system SHALL provide exactly 3 zones for combat.

#### Scenario: Zone initialization
- **WHEN** combat begins
- **THEN** exactly 3 zones SHALL be created
- **AND** each zone SHALL support up to 4 cards per side (player and AI)
- **AND** each side's 4 slots SHALL be displayed as a 2 row by 2 column grid

### Requirement: Zone Reveal Order
The system SHALL reveal zones sequentially at the start of each turn.

#### Scenario: Turn start zone reveals
- **WHEN** a turn begins
- **THEN** zone 1 SHALL be revealed first
- **AND** zone 2 SHALL be revealed second
- **AND** zone 3 SHALL be revealed third
- **AND** each zone reveal MAY trigger zone effects

### Requirement: Priority Calculation
The system SHALL determine turn priority based on zone control at the end of each turn.

#### Scenario: Priority determination
- **WHEN** a turn ends
- **THEN** the system SHALL calculate which player controls each zone based on total power
- **AND** the player who controls more zones SHALL have priority for the next turn's reveal order
- **AND** priority ties SHALL have a defined resolution

### Requirement: Turn Energy Pool
The system SHALL grant a per-turn energy pool equal to the current turn number, capped at 6.

#### Scenario: Pool size follows the turn
- **WHEN** a turn starts
- **THEN** the available energy pool SHALL equal the turn number
- **AND** on turns beyond 6 the pool SHALL remain 6

#### Scenario: Energy does not carry over
- **WHEN** a turn ends with unspent energy
- **THEN** the unspent energy SHALL be lost
- **AND** the next turn SHALL start with a fresh pool for that turn

### Requirement: Energy Expenditure and Refund
The system SHALL derive the player's available energy from the cards played during the current turn.

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

### Requirement: Cost-Gated Card Play
The system SHALL allow players to play cards from hand to zone slots during the play phase, paying each card's energy cost.

#### Scenario: Playing a card
- **WHEN** a player plays a card
- **THEN** the card SHALL be placed in the chosen free slot if the zone has space (max 4 per side)
- **AND** the play order SHALL be recorded
- **AND** the card's cost SHALL NOT exceed the available energy
- **AND** the available energy SHALL be reduced by the card's cost
- **AND** zone play constraints SHALL be respected

#### Scenario: Unaffordable card
- **WHEN** a player attempts to play a card whose cost exceeds the available energy
- **THEN** the play SHALL be rejected
- **AND** the board SHALL remain unchanged

### Requirement: Turn Phases
The system SHALL follow a defined turn structure.

#### Scenario: Turn structure
- **WHEN** a turn progresses
- **THEN** it SHALL go through TurnStart (draw for both sides)
- **AND** PlayPhase (time-limited play)
- **AND** TurnEnd (turn advance and end-of-turn resolution)

#### Scenario: Match length
- **WHEN** the sixth turn ends
- **THEN** the match SHALL be evaluated and no further turns SHALL start

### Requirement: Time Limits
The system SHALL enforce time limits on play phases.

#### Scenario: Time limit expiration
- **WHEN** the play phase time limit expires
- **THEN** the game SHALL proceed with whatever cards were played
- **AND** no additional cards SHALL be playable
- **AND** the turn SHALL advance to TurnEnd

### Requirement: Card Reveal Ordering
The system SHALL reveal cards in order based on priority and play order.

#### Scenario: Reveal sequence
- **WHEN** cards are revealed at TurnEnd
- **THEN** all cards of the player with priority SHALL be revealed first (in the order they were played)
- **AND** then all cards of the other player SHALL be revealed (in the order they were played)
- **AND** OnReveal effects SHALL trigger in this reveal order
- **AND** each card's OnReveal effect SHALL resolve before the next card in the sequence

### Requirement: Per-Turn Draw
The system SHALL draw one card for each side at the start of every turn while that side's deck has cards.

#### Scenario: Both sides draw
- **WHEN** a turn starts
- **THEN** the player SHALL draw 1 card from the player deck if available
- **AND** the AI SHALL draw 1 card from the AI deck if available
- **AND** both drawn cards SHALL be added to the corresponding hand

#### Scenario: Empty deck
- **WHEN** a side's deck is empty at turn start
- **THEN** that side SHALL draw nothing
- **AND** the turn SHALL still start normally
