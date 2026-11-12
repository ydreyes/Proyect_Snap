# run-node-graph Specification

## Purpose

Defines the act structure and node sequence of a run, including shop/event rolls, act transitions, the final act and the pre-combat snapshot that makes every combat resumable.

## Requirements

### Requirement: Run Act Structure

The system SHALL organize a run into four acts.

#### Scenario: Act sizes

- **WHEN** a run starts
- **THEN** act 1, act 2 and act 3 SHALL each contain 15 combats
- **AND** the final act SHALL contain sub-boss 4 and the final boss
- **AND** the run SHALL contain 47 combats in total
- **AND** each of acts 1, 2 and 3 SHALL end with a fixed sub-boss

#### Scenario: Act identity

- **WHEN** the run state is inspected
- **THEN** it SHALL expose the current act index
- **AND** it SHALL expose the combat index inside the current act
- **AND** it SHALL expose the total combat index
- **AND** starting a new run SHALL reset all three to the first combat of act 1

### Requirement: Node Sequence

The system SHALL route the run through an explicit ordered set of nodes.

#### Scenario: Node after a won combat

- **WHEN** a combat is won
- **THEN** experience and money SHALL be applied first
- **AND** every pending level-up SHALL be resolved before the next combat is offered
- **AND** a shop/event roll SHALL occur when the won combat is the 3rd, 6th, 9th or 12th of its act
- **AND** an act transition SHALL occur after the sub-boss of acts 1, 2 and 3
- **AND** otherwise the next node SHALL be enemy selection

#### Scenario: Supported nodes

- **WHEN** the director chooses the next node
- **THEN** the node SHALL be one of `EnemySelect`, `Combat`, `LevelUp`, `Shop`, `Event`, `ActTransition`, `SubBossIntro` or `Summary`
- **AND** no scene SHALL choose the next node on its own

#### Scenario: Level-up before the roll

- **WHEN** a combat win crosses a level threshold and the combat is a roll position
- **THEN** the level-up node SHALL be resolved before the shop/event roll
- **AND** a card added by that level-up SHALL be eligible for that shop's stock

### Requirement: Shop/Event Roll

The system SHALL decide whether a shop or an event appears after certain combats.

#### Scenario: Roll chance

- **WHEN** an act starts
- **THEN** the shop/event chance SHALL be 25%
- **AND** a failed roll SHALL add 15 percentage points to the next roll in that act
- **AND** a successful roll SHALL reset the chance to 25%
- **AND** the chance SHALL be capped at 100%, where the node becomes guaranteed

#### Scenario: Roll positions

- **WHEN** the run is in acts 1, 2 or 3
- **THEN** a roll SHALL occur after the 3rd, 6th, 9th and 12th won combats of the act
- **AND** no roll SHALL occur after the sub-boss

#### Scenario: Shop or event

- **WHEN** a roll succeeds
- **THEN** the node SHALL be a shop with 50% probability
- **AND** it SHALL be an event with 50% probability

### Requirement: Act Transition

The system SHALL show an act transition after each sub-boss of acts 1, 2 and 3.

#### Scenario: Transition content

- **WHEN** an act transition opens
- **THEN** it SHALL show the new act number, the current level, the deck size and the money balance
- **AND** it SHALL require a continue action before the next enemy selection
- **AND** it SHALL reset the shop/event chance to 25%

#### Scenario: Act transition is safe

- **WHEN** the player is in an act transition
- **THEN** no combat SHALL be in progress
- **AND** the node SHALL be eligible for a checkpoint in the save change

### Requirement: Final Act

The system SHALL use a fixed sequence for the final act.

#### Scenario: Final act nodes

- **WHEN** act 3 ends with its sub-boss
- **THEN** an act transition SHALL open the final act
- **AND** the next node SHALL be a 50/50 shop/event node
- **AND** the following node SHALL be a guaranteed shop
- **AND** the following combat SHALL be the unique sub-boss 4
- **AND** the final combat SHALL be the final boss

#### Scenario: No enemy selection in the final act

- **WHEN** the run is in the final act
- **THEN** the player SHALL NOT choose a normal enemy
- **AND** sub-boss 4 and the final boss SHALL be fixed encounters

### Requirement: Enemy Selection

The system SHALL present an enemy-selection node before each normal combat.

#### Scenario: Two candidates

- **WHEN** the director opens enemy selection
- **THEN** it SHALL offer exactly two distinct rival candidates
- **AND** each candidate SHALL show a name, a flavor phrase and a difficulty rating
- **AND** the player SHALL pick one before the combat starts
- **AND** the chosen candidate SHALL determine the rival deck for that combat

#### Scenario: Temporary rival pool

- **WHEN** the enemy roster change has not landed yet
- **THEN** the two candidates SHALL be generated from the current card pool
- **AND** they SHALL remain distinct
- **AND** the node SHALL otherwise behave like the final enemy-selection node

#### Scenario: Sub-boss has no selection

- **WHEN** the next combat is a sub-boss
- **THEN** a sub-boss intro SHALL open instead of enemy selection
- **AND** the sub-boss SHALL be fixed for its act

### Requirement: Pending Combat Snapshot

The system SHALL record the combat before it starts so a result is never applied twice.

#### Scenario: Snapshot creation

- **WHEN** the player confirms an enemy or a sub-boss
- **THEN** the system SHALL record the enemy identity, the attempt number, the pre-combat run state and the combat seed
- **AND** the snapshot SHALL exist before the combat scene is entered

#### Scenario: Result application

- **WHEN** the combat ends
- **THEN** the result SHALL be applied to the run once
- **AND** experience, money, win count and level-ups SHALL be updated before the next node is chosen
- **AND** the snapshot SHALL be cleared after the result is applied
- **AND** dismissing the result modal SHALL NOT apply the result again
