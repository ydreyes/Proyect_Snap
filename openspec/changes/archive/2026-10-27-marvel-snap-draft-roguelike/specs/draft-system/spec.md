# Spec Delta

## Purpose

The draft system handles initial deck building through card selection with copy limits and perk assignment.

## ADDED Requirements

### Requirement: Initial Draft
The system SHALL allow the player to build an initial 8-card deck through drafting.

#### Scenario: Draft selection
- **WHEN** starting a new run
- **THEN** the player SHALL be shown 3 card choices
- **AND** the player SHALL pick 1 card from those 3
- **AND** this process SHALL repeat until 8 cards are selected

### Requirement: Copy Limits in Draft
The system SHALL enforce copy limits during draft selection.

#### Scenario: Copy limit enforcement
- **WHEN** selecting cards during draft
- **THEN** the same card can be repeated in selections
- **AND** the same card SHALL NOT be added more than 2 times total to the final 8-card deck
- **AND** the system SHALL track copies used

### Requirement: Perk Assignment to Draft Cards
The system SHALL assign random perks to the last 3 cards of the draft.

#### Scenario: Draft perk assignment
- **WHEN** the 8-card draft is completed
- **THEN** the last 3 cards selected SHALL each receive a random perk
- **AND** the player SHALL decide whether to equip each perk or not
- **AND** perks SHALL be attached to specific cards

### Requirement: Card Pool
The system SHALL maintain a pool of available cards for drafting.

#### Scenario: Card selection from pool
- **WHEN** presenting draft choices
- **THEN** cards SHALL be selected from the available card pool
- **AND** the pool SHALL support adding new cards
