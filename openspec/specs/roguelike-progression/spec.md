# roguelike-progression Specification

## Purpose
The roguelike progression system manages the run structure, fight sequence, rewards, lives, and deck progression through sub-bosses and final boss.

## Requirements

### Requirement: Fight Sequence

The system SHALL follow a 47-combat sequence across four acts.

#### Scenario: Run progression

- **WHEN** a run progresses
- **THEN** acts 1, 2 and 3 SHALL each contain 14 normal combats and one fixed sub-boss
- **AND** the sub-boss SHALL be the 15th combat of its act
- **AND** the final act SHALL contain the unique sub-boss 4 and the final boss
- **AND** the total SHALL be 47 combats
- **AND** every won combat SHALL grant the experience and money defined for its combat type

#### Scenario: Combat type by position

- **WHEN** the current combat is classified for rewards
- **THEN** every 15th combat of acts 1, 2 and 3 SHALL be a sub-boss combat
- **AND** sub-boss 4 SHALL be a sub-boss combat
- **AND** the final boss SHALL be the final-boss combat
- **AND** every other combat SHALL be a normal combat

#### Scenario: Rewards are unchanged by the act

- **WHEN** a normal, sub-boss or final-boss combat is won
- **THEN** the reward amounts SHALL be the same in every act
- **AND** the act SHALL NOT add a flat reward multiplier

### Requirement: Deck Size Limits

The system SHALL enforce minimum and maximum deck size limits.

#### Scenario: Deck management

- **WHEN** managing the run deck
- **THEN** the initial draft SHALL create 8 cards
- **AND** the deck SHALL hold at least 6 cards
- **AND** the deck SHALL NOT exceed 12 cards
- **AND** at 12 cards, adding a card SHALL require removing an existing card first
- **AND** removing a card SHALL NOT take the deck below 6 cards

#### Scenario: Deck cap during level-up

- **WHEN** a level-up is resolved while the deck has 12 cards
- **THEN** the level-up SHALL offer a card upgrade instead of adding a card
- **AND** the deck size SHALL remain 12

### Requirement: Run State Tracking

The system SHALL track run progress and its checkpoint.

#### Scenario: Progress tracking

- **WHEN** a run is in progress
- **THEN** the system SHALL track the act, the combat index inside the act, the total combat index, experience, level, money, the deck and the pending level-up queue
- **AND** it SHALL track the shop/event chance, the final-act step and the pending node
- **AND** it SHALL track the pending combat snapshot while a combat is being played
- **AND** this state SHALL persist through the run
- **AND** run state SHALL be reset when a new run starts

#### Scenario: Combat index bounds

- **WHEN** a new act starts
- **THEN** the combat index inside the act SHALL start at 0
- **AND** it SHALL never exceed 15 for acts 1, 2 and 3
- **AND** the final act SHALL NOT use a normal combat index

#### Scenario: Checkpoint state

- **WHEN** a run is checkpointed
- **THEN** the checkpoint SHALL store the pending node and every field needed to rebuild it
- **AND** it SHALL store `status` as `Active` or `Ended`
- **AND** it SHALL NOT store the live board, hand or piles
- **AND** the checkpoint SHALL be the only source used by `Continuar`

### Requirement: Run Outcomes

The system SHALL end a run on victory or on a lost or tied combat.

#### Scenario: Run victory

- **WHEN** the player wins the final boss of the final act
- **THEN** the run SHALL be marked as won
- **AND** the player SHALL NOT enter another combat
- **AND** the run summary SHALL be shown

#### Scenario: Run defeat

- **WHEN** the player loses or ties any combat without an extra-life charge
- **THEN** the run SHALL end immediately
- **AND** no further combat SHALL be played in that run
- **AND** the run summary SHALL be shown

#### Scenario: Extra life

- **WHEN** the player loses or ties a combat while holding an extra-life charge
- **THEN** the charge SHALL be consumed
- **AND** the same enemy SHALL be repeated once
- **AND** the run SHALL remain active
- **AND** the repeat SHALL use a new seed and the same enemy composition

### Requirement: Basic Run Summary

The system SHALL show a run summary when the run ends, whether won or lost, and award global points once.

#### Scenario: Summary content

- **WHEN** the run summary is shown
- **THEN** it SHALL show the outcome, the final level, experience earned, money earned, money left unspent, the number of combats won and the final deck
- **AND** the final deck SHALL show each card's effective cost and power
- **AND** it SHALL show the global points breakdown and total

#### Scenario: Summary actions

- **WHEN** the player views the run summary
- **THEN** the system SHALL offer `Menu principal` and `Re-run`
- **AND** `Menu principal` SHALL return to the main menu and delete the finished `run.json`
- **AND** `Re-run` SHALL delete the finished `run.json` and start a new run with reset experience, level, money and deck

#### Scenario: Summary after defeat

- **WHEN** the run ends because of a lost or tied combat
- **THEN** the summary SHALL show the same content as a victory summary
- **AND** the outcome SHALL indicate the defeat
- **AND** the points award SHALL include the combats and levels earned before the defeat

#### Scenario: Points award

- **WHEN** the summary is shown for a run that was not awarded
- **THEN** the system SHALL add the `meta-progression` award to the profile
- **AND** it SHALL store the run id in `lastAwardedRunId`
- **AND** reopening the summary SHALL NOT add the award again
