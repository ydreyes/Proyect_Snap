# Spec Delta

## MODIFIED Requirements

### Requirement: Fight Sequence

The system SHALL follow a specific fight sequence over the course of a run.

#### Scenario: Run progression

- **WHEN** a run progresses
- **THEN** combats SHALL be grouped in 3 cycles of 5 combats
- **AND** the fifth combat of each cycle SHALL be a sub-boss
- **AND** the sub-boss of the third cycle SHALL be the FinalBoss
- **AND** the total SHALL be 15 combats
- **AND** the sequence SHALL NOT grant a card or perk reward based on the combat position
- **AND** every won combat SHALL grant the experience and money defined for its combat type

#### Scenario: Combat type by position

- **WHEN** the current combat is classified for rewards
- **THEN** combats 5 and 10 SHALL count as sub-boss combats
- **AND** combat 15 SHALL count as the final-boss combat
- **AND** every other combat SHALL count as a normal combat

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

The system SHALL track run progress.

#### Scenario: Progress tracking

- **WHEN** a run is in progress
- **THEN** the system SHALL track the current combat (1..15), experience, level, money, the deck and the pending level-up queue
- **AND** this state SHALL persist through the run
- **AND** run state SHALL be reset when a new run starts
- **AND** losing or tying a combat SHALL NOT change the current combat index until the run is restarted

### Requirement: Run Outcomes

The system SHALL end a run on victory or on a lost or tied combat.

#### Scenario: Run victory

- **WHEN** the player wins combat 15
- **THEN** the run SHALL be marked as won
- **AND** the player SHALL NOT enter another combat
- **AND** the run summary SHALL be shown

#### Scenario: Run defeat

- **WHEN** the player loses or ties any combat
- **THEN** the run SHALL end immediately
- **AND** no further combat SHALL be played in that run
- **AND** the run summary SHALL be shown

## REMOVED Requirements

### Requirement: Life System

**Reason**: The run no longer has lives. A defeat ends the run, and the later extra-life item repeats the same enemy instead of restoring a run-level life.

**Migration**: Remove the life counter and all life-loss paths. Replace the "lose a life and repeat" behavior with an immediate run end. Extra-life items, when introduced, consume themselves to repeat the current enemy once.

### Requirement: Win Rewards

**Reason**: Winning a combat no longer offers a card. Rewards are now experience and money, and deck growth happens through the level-up node.

**Migration**: Replace the post-win reward screen with the experience and money pipeline in `run-economy`. Card additions move to the level-up card branch.

### Requirement: Perk Rewards

**Reason**: Perks are no longer attached through a post-combat pair offer. They are chosen in the level-up perk branch, capped at 2 copies per perk across the deck.

**Migration**: Remove the perk-pair reward screen. Use the level-up perk branch defined in `run-economy`.

### Requirement: Reward Offer Integrity

**Reason**: The card and perk reward screens this requirement governs no longer exist. Offer distinctness and the per-card copy limit remain enforced by the draft, the level-up node and the shops.

**Migration**: Enforce distinct offers and copy limits in `draft-system`, `run-economy` and the shop capabilities instead.

### Requirement: Loss Rewards

**Reason**: A loss or tie now ends the run. There is no post-loss reward and no repeat of the same combat at run level.

**Migration**: Remove the post-loss reward screen and the life decrement. Route a loss or tie directly to the run summary. The extra-life item, when introduced, handles the single repeat case.

## ADDED Requirements

### Requirement: Basic Run Summary

The system SHALL show a run summary when the run ends, whether won or lost.

#### Scenario: Summary content

- **WHEN** the run summary is shown
- **THEN** it SHALL show the outcome, the final level, experience earned, money earned, money left unspent, the number of combats won and the final deck
- **AND** the final deck SHALL show each card's effective cost and power
- **AND** the summary SHALL NOT award or display global points in this change

#### Scenario: Summary actions

- **WHEN** the player views the run summary
- **THEN** the system SHALL offer `Menu principal` and `Re-run`
- **AND** `Menu principal` SHALL return to the main menu and leave the finished run
- **AND** `Re-run` SHALL start a new run with reset experience, level, money and deck

#### Scenario: Summary after defeat

- **WHEN** the run ends because of a lost or tied combat
- **THEN** the summary SHALL show the same content as a victory summary
- **AND** the outcome SHALL indicate the defeat
