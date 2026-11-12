# Spec Delta

## MODIFIED Requirements

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

### Requirement: Run State Tracking

The system SHALL track run progress through the node graph.

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

### Requirement: Run Outcomes

The system SHALL end a run on victory or on a lost or tied combat.

#### Scenario: Run victory

- **WHEN** the player wins the final boss of the final act
- **THEN** the run SHALL be marked as won
- **AND** the player SHALL NOT enter another combat
- **AND** the run summary SHALL be shown

#### Scenario: Run defeat

- **WHEN** the player loses or ties any combat
- **THEN** the run SHALL end immediately
- **AND** no further combat SHALL be played in that run
- **AND** the run summary SHALL be shown
