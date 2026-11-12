# card-drag-drop Specification

## Purpose
The card drag and drop system allows players to drag cards from their hand and place them in valid zone slots during play phase.

## Requirements

### Requirement: Card Dragging

The system SHALL allow dragging cards from the player hand, and from a player slot when the card was played during the current turn or when its printed movement permission or an active movement window allows moving it, during PlayPhase, once the pointer moves beyond the tap threshold.

#### Scenario: Start drag

- **WHEN** player presses a card in their hand during PlayPhase and moves the pointer beyond the tap threshold
- **THEN** the card SHALL become draggable
- **AND** the card SHALL follow the mouse/finger position
- **AND** the card SHALL maintain drag offset

#### Scenario: Start drag from board

- **WHEN** player presses a card placed in a player slot during PlayPhase that was played during the current turn and moves the pointer beyond the tap threshold
- **THEN** the card SHALL be lifted from its slot and become draggable
- **AND** the slot SHALL be shown as free while the drag is active
- **AND** the zone power totals SHALL exclude the lifted card until it is placed again

#### Scenario: Older board card stays locked

- **WHEN** player presses a card placed in a player slot during PlayPhase that was played on an earlier turn and moves the pointer beyond the tap threshold
- **AND** the card has no printed movement permission and no active movement window applies
- **THEN** no drag SHALL start
- **AND** the card SHALL remain in its slot
- **AND** the card SHALL NOT be lifted, moved to the hand or moved to another slot
- **AND** the zone power totals SHALL NOT change
- **AND** the available energy SHALL NOT change

#### Scenario: Older moveable card can be lifted

- **WHEN** player presses a card played on an earlier turn that has an unused printed movement permission or is covered by an active Cloak window
- **THEN** the card SHALL be lifted and become draggable
- **AND** its slot SHALL be shown as free while the drag is active
- **AND** dropping it on a legal destination SHALL resolve the permitted move
- **AND** dropping it on the hand area SHALL return it to its origin slot

#### Scenario: Press without movement

- **WHEN** player presses a card and releases it without moving beyond the tap threshold
- **THEN** no drag SHALL start
- **AND** the card SHALL NOT be lifted, removed from the hand or removed from its slot
- **AND** no energy SHALL be reserved
- **AND** the tap SHALL open the card detail panel

### Requirement: Drag Only During PlayPhase
The system SHALL restrict dragging to PlayPhase only.

#### Scenario: Drag restriction
- **WHEN** not in PlayPhase
- **THEN** cards SHALL not be draggable
- **AND** no drag operations SHALL occur

### Requirement: Energy-Aware Drop Validation
The system SHALL reject drops that the player cannot afford.

#### Scenario: Unaffordable drop from hand
- **WHEN** a card from the hand is dropped on a valid free slot
- **AND** its cost exceeds the available energy
- **THEN** the card SHALL return to the hand
- **AND** no board state SHALL change
- **AND** the available energy SHALL NOT change

#### Scenario: Affordable drop from hand
- **WHEN** a card from the hand is dropped on a valid free slot
- **AND** its cost does not exceed the available energy
- **THEN** the card SHALL be placed and its cost SHALL be deducted

### Requirement: Energy Refund on Return

The system SHALL release the energy of a card returned to the hand. Only a card played during the current turn may return from the board to the hand; a card from an earlier turn may be lifted only to perform a permitted move and SHALL NOT return to the hand.

#### Scenario: Drag from board to hand

- **WHEN** a card played during the current turn is dragged from the board to the hand area
- **THEN** the card SHALL leave the board
- **AND** its cost SHALL be returned to the available energy

#### Scenario: Card from an earlier turn

- **WHEN** a card played on an earlier turn is dragged from the board to the hand area
- **THEN** the return SHALL be rejected
- **AND** the card SHALL return to its origin slot
- **AND** the available energy SHALL NOT change
- **AND** an unused movement permission SHALL remain unused

### Requirement: Drop Zone Validation

The system SHALL validate card drops against zone slots and the available energy.

#### Scenario: Valid drop

- **WHEN** a dragged card is released over the player half of a location
- **THEN** the nearest free slot in that location SHALL be selected
- **AND** if a free slot exists and the card is affordable, the card SHALL be placed there
- **AND** the card SHALL be removed from its origin (hand or previous slot)
- **AND** a card coming from the hand SHALL have its cost deducted from the available energy
- **AND** the zone power totals SHALL be recalculated

#### Scenario: Invalid drop

- **WHEN** a card is released outside the player half of every location, the target location has no free slot, or the card's cost exceeds the available energy
- **THEN** the card SHALL return to its origin
- **AND** no board state changes SHALL persist
- **AND** the available energy SHALL NOT change

#### Scenario: Drop on hand

- **WHEN** a card played during the current turn already on the board is released over the hand area
- **THEN** the card SHALL return to the player's hand
- **AND** it SHALL be playable again during the same turn
- **AND** its cost SHALL be released

#### Scenario: Timer expiry during drag

- **WHEN** the turn timer reaches zero while a card is being dragged
- **THEN** the card SHALL return to the player's hand
- **AND** the turn SHALL close

### Requirement: Zone Slot Visualization
The system SHALL show drop zones for all 3 zones.

#### Scenario: Slot display
- **WHEN** in combat scene
- **THEN** each of the 3 zones SHALL display 4 slots per side in a 2 by 2 grid
- **AND** empty slots SHALL be visually indicated
- **AND** occupied slots SHALL show placed cards

#### Scenario: Slot highlight
- **WHEN** a card is being dragged over the player half of a location
- **THEN** the nearest free slot SHALL be highlighted
- **AND** the highlight SHALL disappear when the drag leaves that location or ends

### Requirement: Hand Capacity on Return

The system SHALL respect the 7-card hand limit when a played card is returned from the board to the hand.

#### Scenario: Full hand rejects the return

- **WHEN** a card played during the current turn is dragged from the board to the hand area
- **AND** the hand already has 7 cards
- **THEN** the return SHALL be rejected
- **AND** the card SHALL stay in its board slot
- **AND** its energy SHALL remain committed
- **AND** the drag SHALL cancel to its origin

#### Scenario: Room in the hand

- **WHEN** a card played during the current turn is dragged to the hand area
- **AND** the hand has fewer than 7 cards
- **THEN** the card SHALL leave the board and enter the hand
- **AND** its cost SHALL be refunded
- **AND** it SHALL be playable again during the same turn

#### Scenario: Timer expires during a full-hand drag

- **WHEN** the play phase timer expires while a played card is being dragged
- **AND** the hand has 7 cards
- **THEN** the card SHALL return to its board slot
- **AND** the turn SHALL close normally
- **AND** its energy SHALL remain committed

#### Scenario: Hand view stays consistent

- **WHEN** a return is rejected because the hand is full
- **THEN** the hand view SHALL show the same 7 cards
- **AND** the board view SHALL show the returned card in its slot
- **AND** no duplicate hand view SHALL appear

### Requirement: Moveable Cards

The system SHALL support cards whose printed abilities allow moving them after the turn they were played.

#### Scenario: Nightcrawler

- **WHEN** Nightcrawler is moved on a turn later than the turn it was played
- **THEN** the move SHALL be allowed once per combat
- **AND** a later move of the same instance SHALL be rejected
- **AND** the destination SHALL have room and SHALL NOT be closed to movement

#### Scenario: Vision

- **WHEN** Vision is moved on a turn later than the turn it was played
- **THEN** the move SHALL be allowed once per turn
- **AND** a second move of the same instance in the same turn SHALL be rejected
- **AND** the permission SHALL reset at the start of the next turn

#### Scenario: Same-turn move does not consume

- **WHEN** a card with a printed movement permission is moved during the turn it was played
- **THEN** the move SHALL resolve under the normal same-turn movement rule
- **AND** the printed permission SHALL remain unused
- **AND** the card SHALL still be able to use it on a later turn

#### Scenario: Failed move

- **WHEN** a permitted move is dropped on an illegal destination
- **THEN** the card SHALL return to its origin slot
- **AND** the movement permission SHALL remain unused
- **AND** the zone power totals and energy SHALL remain unchanged

### Requirement: Cloak Movement Window

The system SHALL support a temporary window that lets any card move to a specific location on the following turn.

#### Scenario: Window opens

- **WHEN** Cloak is revealed on turn N
- **THEN** during turn N + 1 any card in play may be moved to Cloak's location
- **AND** the window SHALL apply to both sides
- **AND** the destination SHALL have room and SHALL NOT be closed to movement

#### Scenario: Window scope

- **WHEN** the window is active
- **THEN** it SHALL only authorize moves whose destination is Cloak's location
- **AND** a move to another location SHALL use the normal movement rules
- **AND** the window SHALL NOT authorize returning a card to hand

#### Scenario: Window usage

- **WHEN** a card is moved through a window
- **THEN** that card SHALL NOT be moved through the same window again
- **AND** the printed movement permission of the card SHALL remain unused
- **AND** the window SHALL expire at the end of the turn

#### Scenario: Window blocked

- **WHEN** Cloak's location is full or Professor X's full lock blocks the addition
- **THEN** the window SHALL NOT allow the move
- **AND** the card SHALL remain in its origin
