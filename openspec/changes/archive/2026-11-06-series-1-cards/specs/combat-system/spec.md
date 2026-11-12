# Spec Delta

## MODIFIED Requirements

### Requirement: Per-Turn Draw

The system SHALL draw one card for each side at the start of every turn while that side's deck has cards and its hand has room.

#### Scenario: Both sides draw

- **WHEN** a turn starts
- **THEN** the player SHALL draw 1 card from the player deck if available and the hand has room
- **AND** the AI SHALL draw 1 card from the AI deck if available and the hand has room
- **AND** both drawn cards SHALL be added to the corresponding hand

#### Scenario: Empty deck

- **WHEN** a side's deck is empty at turn start
- **THEN** that side SHALL draw nothing
- **AND** the turn SHALL still start normally

#### Scenario: Full hand

- **WHEN** a side's hand already has 7 cards at turn start
- **THEN** that side SHALL NOT draw
- **AND** the top card SHALL stay in the deck
- **AND** the turn SHALL still start normally

## ADDED Requirements

### Requirement: Hand Capacity

The system SHALL cap each hand at 7 cards and define the behavior of every addition path when the hand is full.

#### Scenario: Effect addition to a full hand

- **WHEN** an effect would add or copy a card into a hand that already has 7 cards
- **THEN** the card SHALL NOT be created or moved
- **AND** the source card, deck or opponent hand SHALL remain unchanged
- **AND** the effect SHALL complete without an error

#### Scenario: Return to a full hand

- **WHEN** a player returns a played card from the board to a hand that already has 7 cards
- **THEN** the return SHALL be rejected
- **AND** the card SHALL stay in its board slot
- **AND** its energy SHALL remain committed

#### Scenario: Token and copy additions

- **WHEN** a token or copy would enter a hand
- **THEN** the hand-cap check SHALL apply before the instance is created
- **AND** a rejected addition SHALL NOT leave a partial instance in any container

### Requirement: Card Movement

The system SHALL support moving cards between locations as a board action, including single moves and simultaneous batches.

#### Scenario: Single move

- **WHEN** an effect moves a card to another location
- **THEN** the card SHALL leave its origin and enter the destination if the destination has room
- **AND** its recorded played zone SHALL remain the zone where it was played
- **AND** its energy SHALL NOT change

#### Scenario: Batch move

- **WHEN** an effect moves several cards at once, such as Heimdall
- **THEN** the moves SHALL resolve from one board snapshot
- **AND** a card SHALL NOT move if its destination would exceed 4 cards for that side
- **AND** the final board SHALL NOT depend on list iteration order

#### Scenario: Movement events

- **WHEN** a card moves into a location
- **THEN** an `AfterMoved` event SHALL be dispatched with origin and destination
- **AND** movement reactions SHALL resolve before the action returns

### Requirement: Card Destruction and Discard

The system SHALL route destruction and discard through the action authority, including protection and replacement.

#### Scenario: Destroy from board

- **WHEN** an effect destroys a board card
- **THEN** the card SHALL leave its zone and enter the destroyed pile
- **AND** its cost SHALL NOT be refunded
- **AND** an `AfterDestroyed` event SHALL be dispatched if the destroy succeeds

#### Scenario: Discard from hand or deck

- **WHEN** an effect discards a card from the hand or deck
- **THEN** the card SHALL leave that container and enter the discarded pile
- **AND** an `AfterDiscarded` event SHALL be dispatched

#### Scenario: Protection and replacement

- **WHEN** a destroy or discard is protected or replaced
- **THEN** the action SHALL NOT move the card to a pile
- **AND** the replacement effect SHALL resolve instead
- **AND** no destroy or discard event SHALL fire for a prevented action

### Requirement: Deck Manipulation

The system SHALL support top-deck access, opponent-deck draw, shuffling, lowest-cost search and combat-scoped banish.

#### Scenario: Top of deck

- **WHEN** an effect targets the top card of a deck
- **THEN** it SHALL target the first card in draw order
- **AND** a buff on that card SHALL remain when it is drawn

#### Scenario: Opponent deck draw

- **WHEN** an effect draws from the opponent's deck
- **THEN** the drawn card SHALL leave that deck and enter the effect owner's hand if it has room
- **AND** the opponent SHALL NOT draw that card later in the combat

#### Scenario: Shuffle and search

- **WHEN** an effect shuffles a token into a deck
- **THEN** the token SHALL enter the deck in a random position
- **AND** when an effect searches for the lowest-Cost card
- **THEN** ties SHALL be broken randomly
- **AND** the selected card SHALL leave the deck

#### Scenario: Combat-scoped banish

- **WHEN** Yondu banishes a card
- **THEN** the card SHALL enter the banished pile for the current combat
- **AND** it SHALL NOT be removed from the run deck
- **AND** it SHALL return to the run deck for the next combat

### Requirement: Card Creation and Merge

The system SHALL support creating tokens, copying instances, transforming a card and merging cards.

#### Scenario: Token creation

- **WHEN** an effect creates a token in a zone
- **THEN** the token SHALL be a separate instance with its own definition and current power
- **AND** the four-card limit SHALL apply
- **AND** the token SHALL NOT enter the draft or reward pool

#### Scenario: Copy creation

- **WHEN** an effect copies a card
- **THEN** the copy SHALL be a separate instance
- **AND** it SHALL carry the source's definition, current power and abilities according to the card's text
- **AND** it SHALL respect the hand or zone destination limit

#### Scenario: Transform

- **WHEN** Morph becomes a copy of a card
- **THEN** its definition, power and abilities SHALL change to the copied card
- **AND** its existing persistent bonuses SHALL NOT be double-counted
- **AND** its reveal SHALL NOT trigger again

#### Scenario: Merge

- **WHEN** Hulkbuster merges into a host
- **THEN** the host SHALL gain Hulkbuster's current power as persistent power
- **AND** Hulkbuster SHALL leave the board without entering a pile
- **AND** no destroy, discard or banish event SHALL fire for the merge
- **AND** the host SHALL keep its own definition and abilities

### Requirement: Forced Turn-2 Draw

The system SHALL guarantee that Domino is drawn on turn 2 and not before, while still respecting the hand cap.

#### Scenario: Domino is not in the opening hand

- **WHEN** a combat starts with Domino in the deck
- **THEN** she SHALL NOT be dealt in the opening hand
- **AND** she SHALL remain in the deck

#### Scenario: Domino on turn 2

- **WHEN** turn 2 starts and Domino is in the deck
- **THEN** she SHALL be the card drawn for that side if the hand has room
- **AND** she SHALL NOT be drawn on turn 1 or earlier

#### Scenario: Full hand on turn 2

- **WHEN** turn 2 starts with a full hand
- **THEN** Domino SHALL stay in the deck
- **AND** she SHALL be eligible for a later draw
