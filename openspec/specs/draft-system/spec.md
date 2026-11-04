# draft-system Specification

## Purpose
The draft system handles initial deck building through card selection with copy limits and perk assignment.

## Requirements

### Requirement: Initial Draft
The system SHALL allow the player to build an initial 8-card deck through eight rounds of card selection in a dedicated draft scene.

#### Scenario: Draft selection
- **WHEN** starting a new run
- **THEN** the player SHALL be shown 3 card choices for the current round
- **AND** the player SHALL pick exactly 1 card from those 3
- **AND** the next round SHALL present 3 new choices
- **AND** this process SHALL repeat until 8 cards are selected

#### Scenario: Draft progress
- **WHEN** the player is drafting
- **THEN** the scene SHALL display how many cards have been picked out of 8
- **AND** the continue action SHALL remain disabled until 8 cards are selected

#### Scenario: New run reset
- **WHEN** the player starts a new run after a previous one
- **THEN** previously drafted cards SHALL be discarded
- **AND** draft progress SHALL restart at 0 of 8

### Requirement: Copy Limits in Draft
The system SHALL enforce copy limits during draft selection.

#### Scenario: Copy limit enforcement
- **WHEN** selecting cards during draft
- **THEN** the same card MAY be offered again in a later round
- **AND** the same card SHALL NOT be added more than 2 times total to the final 8-card deck
- **AND** the system SHALL track copies used

#### Scenario: Exhausted card excluded
- **WHEN** a card has already reached 2 copies in the current draft
- **THEN** that card SHALL NOT appear among the choices of any later round

#### Scenario: Distinct choices
- **WHEN** presenting the 3 choices of a round
- **THEN** the 3 choices SHALL be distinct card definitions

### Requirement: Perk Assignment to Draft Cards
The system SHALL assign random perks to the last 3 cards of the draft.

#### Scenario: Draft perk assignment
- **WHEN** the 8-card draft is completed
- **THEN** the last 3 cards selected SHALL each receive a random perk
- **AND** the player SHALL decide whether to equip each perk or not
- **AND** perks SHALL be attached to specific cards

### Requirement: Card Pool

The system SHALL maintain a pool of at least 20 draftable card definitions available for drafting.

#### Scenario: Card selection from pool

- **WHEN** presenting draft choices
- **THEN** cards SHALL be selected from the available draftable card pool
- **AND** the pool SHALL contain at least 20 cards with varied costs and powers
- **AND** the pool SHALL support adding new cards

#### Scenario: Token definitions are excluded

- **WHEN** presenting draft choices
- **THEN** token definitions SHALL NOT be offered
- **AND** a card created by an effect during combat SHALL NOT enter the draft pool
- **AND** the draft SHALL continue to offer only definitions declared as draftable

### Requirement: Draft Completion
The system SHALL hand the drafted deck to combat when the draft completes.

#### Scenario: Deck handoff
- **WHEN** the player completes the 8th pick
- **THEN** the 8 drafted cards SHALL become the player's combat deck
- **AND** the game SHALL advance to the combat scene
- **AND** the drafted deck SHALL be preserved for the duration of the run
