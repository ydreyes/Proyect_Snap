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
The system SHALL reveal one zone at the start of each of the first three turns of a combat, in order, and keep it revealed for the rest of that combat.

#### Scenario: Turn start zone reveals
- **WHEN** a turn begins
- **THEN** the next unrevealed zone SHALL be revealed
- **AND** previously revealed zones SHALL stay revealed
- **AND** each zone reveal MAY trigger zone effects

#### Scenario: Combat starts with zones hidden

- **WHEN** a combat begins
- **THEN** all three zones SHALL start unrevealed
- **AND** the zone name, effect text and art SHALL be withheld until the zone is revealed
- **AND** the zone index and both power totals SHALL remain visible

#### Scenario: First turn reveals zone 1

- **WHEN** the first turn of a combat begins
- **THEN** zone 1 SHALL be revealed
- **AND** zones 2 and 3 SHALL remain unrevealed

#### Scenario: Second turn reveals zone 2

- **WHEN** the second turn begins
- **THEN** zone 2 SHALL be revealed
- **AND** zone 1 SHALL remain revealed
- **AND** zone 3 SHALL remain unrevealed

#### Scenario: Third turn reveals zone 3

- **WHEN** the third turn begins
- **THEN** zone 3 SHALL be revealed
- **AND** all three zones SHALL remain revealed for the rest of the combat
- **AND** each zone reveal MAY trigger zone effects

#### Scenario: Hidden zones remain playable

- **WHEN** a zone is unrevealed
- **THEN** it SHALL accept cards from both sides
- **AND** its power totals SHALL count for control and priority
- **AND** its play constraints SHALL apply once revealed

#### Scenario: Reveal before resolution

- **WHEN** a combat resolves with zones still unrevealed
- **THEN** every unrevealed zone SHALL be revealed before control and the match result are computed

### Requirement: Priority Calculation
The system SHALL determine turn priority based on zone control at the end of each turn, after all effects and power changes for that turn have resolved.

#### Scenario: Priority determination
- **WHEN** a turn ends
- **THEN** the system SHALL apply OnReveal effects and recalculate card and zone power first
- **AND** then it SHALL calculate which player controls each zone based on total power
- **AND** the player who controls more zones SHALL have priority for the next turn's reveal order
- **AND** priority ties SHALL retain the previous priority owner

#### Scenario: Priority sees final power
- **WHEN** an OnReveal or Ongoing effect changes zone power at turn end
- **THEN** the priority calculation SHALL use the changed total
- **AND** it SHALL NOT use a total calculated before the effects resolved

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
- **AND** TurnEnd (OnReveal resolution, power recalculation, priority and turn advance)

#### Scenario: Match length
- **WHEN** the sixth turn ends
- **THEN** the match SHALL be evaluated after that turn's OnReveal effects and power recalculation
- **AND** no further turns SHALL start

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
- **THEN** all cards played during the current turn SHALL be included
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

### Requirement: Opening Hand
The system SHALL deal an opening hand to both sides at the start of a combat, guaranteeing cards tagged to start in hand.

#### Scenario: Tagged cards are dealt first
- **WHEN** a combat starts
- **THEN** every card in a side's deck tagged to start in hand SHALL be moved to that side's opening hand before the random fill
- **AND** this SHALL apply to the player and the AI
- **AND** if the side has multiple copies, every copy SHALL be moved

#### Scenario: Fill to the starting size
- **WHEN** the tagged cards have been moved
- **THEN** the hand SHALL be filled to the starting hand size from the remaining deck
- **AND** if the deck runs out, the hand SHALL contain what was available
- **AND** a hand with more tagged cards than the starting size SHALL keep all of them

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
