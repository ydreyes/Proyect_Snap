# Spec Delta

## Purpose

Per-side card piles that record what left the hand, deck or board during a combat, expose that record to the player through an avatar popover, and define which cards survive into the next combat of the run.

## ADDED Requirements

### Requirement: Pile Sets per Side

The system SHALL maintain three piles for each side of a combat: discarded, destroyed and banished.

#### Scenario: Piles start empty

- **WHEN** a combat begins
- **THEN** the player's discarded, destroyed and banished piles SHALL be empty
- **AND** the AI's discarded, destroyed and banished piles SHALL be empty

#### Scenario: Both sides are tracked

- **WHEN** a card from the player enters a pile
- **THEN** only the player's pile SHALL change
- **AND** the AI's piles SHALL keep their contents

#### Scenario: Card identity is preserved

- **WHEN** a card enters a pile
- **THEN** the pile SHALL keep the same card instance, with its definition, current power and attached perks
- **AND** the card SHALL be identifiable by its instance identity, not only by its definition

### Requirement: Single Card Location

The system SHALL keep every card in exactly one location at a time: deck, hand, board, discarded, destroyed or banished.

#### Scenario: A card cannot be in two places

- **WHEN** a card that is in the hand, deck or board enters a pile
- **THEN** it SHALL be removed from its previous location
- **AND** it SHALL NOT appear in the hand, the deck or a board slot afterwards

#### Scenario: Cards in a pile are inert

- **WHEN** a card is in a pile
- **THEN** it SHALL NOT be playable, drawn, dragged or counted toward zone power
- **AND** it SHALL only be visible through the pile viewer

### Requirement: Pile Entry by Origin

The system SHALL accept a card into a pile according to the pile's origin: destroyed from the board, discarded from the hand or deck, banished from any location.

#### Scenario: Destroyed card leaves the board

- **WHEN** a card on the board is destroyed
- **THEN** it SHALL be removed from its zone slot
- **AND** it SHALL be added to the destroyed pile of its side
- **AND** the zone power totals SHALL be recalculated without it

#### Scenario: Discarded card leaves the hand or deck

- **WHEN** a card in the hand is discarded
- **THEN** it SHALL be removed from the hand and added to the discarded pile
- **AND** when a card in the deck is discarded
- **THEN** it SHALL be removed from the deck and added to the discarded pile

#### Scenario: Banished card leaves any location

- **WHEN** a card is banished from the hand, the deck or the board
- **THEN** it SHALL be removed from that location and added to the banished pile

#### Scenario: Invalid origin is rejected

- **WHEN** a card on the board is sent to the discarded pile, or a card in the hand is sent to the destroyed pile
- **THEN** the move SHALL be rejected
- **AND** all piles and the board SHALL remain unchanged

### Requirement: Combat Lifecycle

The system SHALL treat discarded and destroyed as combat-scoped and banished as run-scoped.

#### Scenario: Discarded and destroyed return next combat

- **WHEN** a combat ends with cards in the discarded or destroyed pile
- **THEN** those cards SHALL be available to the player's deck in the next combat
- **AND** the piles SHALL be empty when that combat begins

#### Scenario: Banished never return

- **WHEN** a card is banished
- **THEN** it SHALL be removed from the run deck
- **AND** that card instance SHALL NOT appear in any later combat of the run
- **AND** it SHALL NOT be offered as a reward, a draft choice or a perk target
- **AND** a new card of the same definition MAY still be offered, as a separate instance

#### Scenario: The run continues with a smaller deck

- **WHEN** cards are banished and the run deck shrinks below its maximum
- **THEN** the next combat SHALL start with the reduced deck
- **AND** rewards SHALL still be able to add cards up to the maximum size

### Requirement: Board View Synchronization

The system SHALL update the board and hand views whenever a card changes pile, without waiting for the next turn.

#### Scenario: Destroyed card disappears from the board

- **WHEN** a card is destroyed during the play phase
- **THEN** its slot SHALL show as empty in the same frame
- **AND** the location's player or rival power total SHALL reflect the removal

#### Scenario: Discarded card disappears from the hand

- **WHEN** a card is discarded during the play phase
- **THEN** its hand view SHALL disappear
- **AND** the remaining hand cards SHALL keep their positions and affordability

#### Scenario: Drag is cancelled when its card leaves

- **WHEN** the card being dragged enters a pile
- **THEN** the drag SHALL be cancelled
- **AND** the card SHALL NOT be returned to the hand or placed on the board
- **AND** any reserved energy SHALL be released

#### Scenario: Sync also applies outside the play phase

- **WHEN** a card changes pile while the viewer is open or between turns
- **THEN** the views SHALL still reconcile before the next frame is drawn

### Requirement: Pile Viewer Access

The system SHALL open a pile viewer for a side when the player clicks that side's avatar portrait.

#### Scenario: Opening the player's viewer

- **WHEN** the player clicks the portrait area of the player avatar
- **THEN** the player's pile viewer SHALL open
- **AND** it SHALL show the pile selected by default

#### Scenario: Opening the rival's viewer

- **WHEN** the player clicks the portrait area of the rival avatar
- **THEN** the rival's pile viewer SHALL open
- **AND** it SHALL show the cards the AI discarded, destroyed or banished

#### Scenario: Closing the viewer

- **WHEN** the viewer is open and the player clicks the same avatar again, or clicks outside the viewer
- **THEN** the viewer SHALL close
- **AND** the board SHALL return to normal interaction

#### Scenario: One viewer at a time

- **WHEN** the player's viewer is open and the player clicks the rival avatar
- **THEN** only the rival's viewer SHALL remain open

### Requirement: Pile Viewer Contents

The system SHALL display the three piles as tabs with a name and a count, and the selected pile as a page of cards.

#### Scenario: Tab labels and counts

- **WHEN** the viewer opens
- **THEN** it SHALL show the tabs `DESCARTADAS`, `DESTRUIDAS` and `DESTERRADAS`
- **AND** each tab SHALL show its current count, including zero

#### Scenario: Selecting a tab

- **WHEN** the player clicks a tab
- **THEN** the panel SHALL show the cards in that pile
- **AND** the selected tab SHALL be visually distinguishable

#### Scenario: Card details

- **WHEN** a card is shown in the panel
- **THEN** its cost, power, name and perk bonus SHALL be legible
- **AND** it SHALL be rendered facing up

#### Scenario: Empty pile

- **WHEN** the selected pile has no cards
- **THEN** the panel SHALL show an empty state instead of cards

#### Scenario: Pagination

- **WHEN** the selected pile has more cards than fit on one page
- **THEN** the panel SHALL show three cards per page
- **AND** it SHALL offer previous and next navigation with a page indicator
- **AND** navigation SHALL be unavailable when there is only one page

#### Scenario: Ordering

- **WHEN** a pile contains cards added at different times
- **THEN** the panel SHALL list them from most recently added to oldest

### Requirement: Pile Viewer Interaction

The system SHALL isolate the viewer from board interaction while the turn timer keeps running.

#### Scenario: Board is blocked while the viewer is open

- **WHEN** the viewer is open and the player presses on a hand card, a board card or a board slot
- **THEN** no drag SHALL start and no card SHALL move

#### Scenario: Viewer controls remain usable

- **WHEN** the viewer is open
- **THEN** its tabs, page navigation and closing SHALL respond to clicks
- **AND** the end-turn control SHALL remain usable

#### Scenario: Timer expires with the viewer open

- **WHEN** the play phase time limit expires while the viewer is open
- **THEN** the viewer SHALL close
- **AND** the turn SHALL end as usual with the cards already played

### Requirement: Pile Viewer Placement

The system SHALL anchor the viewer to the avatar it belongs to and scale it with the viewport.

#### Scenario: Player side placement

- **WHEN** the player's viewer opens
- **THEN** it SHALL appear below the player avatar, aligned to the avatar's left edge

#### Scenario: Rival side placement

- **WHEN** the rival's viewer opens
- **THEN** it SHALL appear below the rival avatar, aligned to the avatar's right edge

#### Scenario: Scaling

- **WHEN** the window size changes while the viewer is open
- **THEN** the viewer SHALL keep its position relative to its avatar
- **AND** its text SHALL stay legible using the default font
