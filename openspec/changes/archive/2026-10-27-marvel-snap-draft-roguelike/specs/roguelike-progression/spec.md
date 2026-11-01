# Spec Delta

## Purpose

The roguelike progression system manages the run structure, fight sequence, rewards, lives, and deck progression through sub-bosses and final boss.

## ADDED Requirements

### Requirement: Fight Sequence
The system SHALL follow a specific fight sequence over the course of a run.

#### Scenario: Run progression
- **WHEN** a run progresses
- **THEN** fights SHALL follow the pattern: Combat, CardNew, Combat, NewPerk, Combat, CardNew, Combat, CardNew, SubBoss (5th combat)
- **AND** this pattern SHALL repeat 2 more times
- **AND** the last repetition SHALL have a FinalBoss instead of SubBoss
- **AND** the total SHALL be approximately 15 fights

### Requirement: Life System
The system SHALL track player lives.

#### Scenario: Life tracking
- **WHEN** a run starts
- **THEN** the player SHALL have a maximum of 2 lives
- **AND** losing a combat SHALL reduce lives by 1
- **AND** when lives reach 0, the game SHALL be over and return to main menu

### Requirement: Win Rewards
The system SHALL provide rewards for winning combats.

#### Scenario: Post-win rewards
- **WHEN** the player wins a combat
- **THEN** the system SHALL show 3 cards each with a perk
- **AND** the player SHALL pick 1 card to add to their deck
- **AND** if the deck already has 12 cards, the player MAY remove 1 card and substitute it with the chosen card
- **AND** the new card's perk SHALL be handled according to reward rules

### Requirement: Loss Rewards
The system SHALL provide rewards for losing combats.

#### Scenario: Post-loss rewards
- **WHEN** the player loses a combat
- **THEN** the player SHALL lose 1 life
- **AND** if lives remain, the player SHALL pick a perk for 1 card from 3 cards of their current deck
- **AND** the perk SHALL be attached to the chosen card

### Requirement: Deck Size Limits
The system SHALL enforce deck size limits.

#### Scenario: Deck management
- **WHEN** managing the deck
- **THEN** the initial draft SHALL create up to 8 cards
- **AND** the deck MAY grow to a maximum of 12 cards
- **AND** when at maximum size, adding new cards requires removal of existing cards

### Requirement: Run State Tracking
The system SHALL track run progress.

#### Scenario: Progress tracking
- **WHEN** a run is in progress
- **THEN** the system SHALL track current fight number, lives remaining, and deck state
- **AND** this state SHALL persist through the run
- **AND** run state SHALL be reset on game over or victory
