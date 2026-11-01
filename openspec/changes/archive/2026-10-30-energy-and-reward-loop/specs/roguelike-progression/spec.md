# Spec Delta

## ADDED Requirements

### Requirement: Perk Rewards
The system SHALL offer a perk reward after winning the second combat of each cycle.

#### Scenario: Perk selection
- **WHEN** the player wins a combat in the second position of a cycle
- **THEN** the system SHALL offer 3 distinct cards from the player's deck
- **AND** each offered card SHALL be paired with a perk that has a positive power modifier
- **AND** the player SHALL pick one pair
- **AND** the chosen perk SHALL be attached to that card
- **AND** no card SHALL be added to or removed from the deck

### Requirement: Reward Offer Integrity
The system SHALL present reward offers without duplicates.

#### Scenario: Distinct offers
- **WHEN** any reward screen is shown
- **THEN** the offered cards SHALL be distinct from each other
- **AND** every offered perk SHALL have a positive power modifier
- **AND** an offered card SHALL respect the same per-card copy limit used by the draft

### Requirement: Run Outcomes
The system SHALL end a run on victory or on losing all lives.

#### Scenario: Run victory
- **WHEN** the player wins combat 15
- **THEN** the run SHALL be marked as won
- **AND** the player SHALL NOT enter another combat

#### Scenario: Run defeat
- **WHEN** the player's lives reach 0
- **THEN** the run SHALL end
- **AND** the player SHALL be returned to the main menu

## MODIFIED Requirements

### Requirement: Fight Sequence
The system SHALL follow a specific fight sequence over the course of a run.

#### Scenario: Run progression
- **WHEN** a run progresses
- **THEN** combats SHALL be grouped in 3 cycles of 5 combats
- **AND** within each cycle, winning the first and third combats SHALL grant a card reward
- **AND** winning the second combat SHALL grant a perk reward
- **AND** winning the fourth combat SHALL grant no reward
- **AND** the fifth combat SHALL be a boss and grant no reward
- **AND** the boss of the third cycle SHALL be the FinalBoss
- **AND** the total SHALL be 15 combats

### Requirement: Life System
The system SHALL track player lives.

#### Scenario: Life tracking
- **WHEN** a run starts
- **THEN** the player SHALL have a maximum of 2 lives
- **AND** losing a combat or tying it SHALL reduce lives by 1
- **AND** when lives reach 0, the game SHALL be over and return to main menu

### Requirement: Win Rewards
The system SHALL provide card rewards for winning a combat that grants one.

#### Scenario: Post-win rewards
- **WHEN** the player wins a combat that grants a card reward
- **THEN** the system SHALL show 3 distinct cards from the catalog, each with a perk that has a positive power modifier
- **AND** the player SHALL pick 1 card to add to their deck
- **AND** if the deck already has 12 cards, the player SHALL either remove 1 card and substitute it with the chosen card, or skip the reward
- **AND** the chosen card SHALL keep its perk

### Requirement: Loss Rewards
The system SHALL provide a card reward and repeat the combat after a loss or a tie.

#### Scenario: Post-loss rewards
- **WHEN** the player loses a combat or ties it
- **THEN** the player SHALL lose 1 life
- **AND** if lives remain, the system SHALL show the card reward screen
- **AND** the player SHALL pick a card or skip, under the same deck-size rules as a win
- **AND** the same combat SHALL be played again without advancing the fight index

#### Scenario: No lives remaining
- **WHEN** the loss reduces lives to 0
- **THEN** no reward SHALL be offered
- **AND** the run SHALL end

### Requirement: Deck Size Limits
The system SHALL enforce deck size limits.

#### Scenario: Deck management
- **WHEN** managing the deck
- **THEN** the initial draft SHALL create up to 8 cards
- **AND** the deck MAY grow to a maximum of 12 cards
- **AND** at 12 cards, adding a reward card SHALL require removing an existing card
- **AND** the player MAY skip a reward instead of replacing a card

### Requirement: Run State Tracking
The system SHALL track run progress.

#### Scenario: Progress tracking
- **WHEN** a run is in progress
- **THEN** the system SHALL track the current combat (1..15), lives remaining, and deck state
- **AND** this state SHALL persist through the run
- **AND** run state SHALL be reset when a new run starts
- **AND** losing a combat SHALL NOT change the current combat
