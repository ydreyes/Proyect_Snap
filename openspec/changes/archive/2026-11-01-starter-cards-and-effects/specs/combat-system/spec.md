# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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
