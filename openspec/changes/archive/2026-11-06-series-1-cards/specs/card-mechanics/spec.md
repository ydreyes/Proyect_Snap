# Spec Delta

## ADDED Requirements

### Requirement: Persistent Power from Play, Destroy and Discard

The system SHALL store persistent power changes on the card instance so they survive movement and later power recalculation.

#### Scenario: After-play counters

- **WHEN** Angela gains +1 from a card played in her location
- **THEN** the bonus SHALL be stored on Angela
- **AND** it SHALL apply in every later power calculation
- **AND** it SHALL survive Angela moving

#### Scenario: Global after-play counter

- **WHEN** Bishop gains +1 from an allied card play
- **THEN** the bonus SHALL be stored on Bishop
- **AND** it SHALL NOT be accumulated by replaying the same instance in the same turn

#### Scenario: Destruction and discard bonuses

- **WHEN** Carnage destroys cards
- **THEN** its +2 per destroyed card SHALL be stored on Carnage
- **AND** when Nova is destroyed
- **THEN** the +1 to allied cards SHALL be stored on each affected card
- **AND** when Apocalypse is discarded
- **THEN** its +4 SHALL be stored on Apocalypse when it returns to hand
- **AND** when Wolverine regenerates
- **THEN** its +2 SHALL be stored on Wolverine

#### Scenario: Next-play bonus

- **WHEN** Forge grants +2 to the next card played
- **THEN** the bonus SHALL be stored on the card that is played
- **AND** it SHALL survive that card returning to hand

### Requirement: Affliction

The system SHALL represent persistent negative power on a card instance.

#### Scenario: Afflicted card

- **WHEN** Spider-Woman afflicts an enemy card
- **THEN** the card SHALL store -1 persistent Power
- **AND** the affliction SHALL follow the card if it moves
- **AND** it SHALL NOT be applied to cards that enter the location later
- **AND** the card's current power SHALL include the affliction in every recalculation

#### Scenario: Multiple afflictions

- **WHEN** more than one affliction applies to the same card
- **THEN** each SHALL contribute once
- **AND** the result SHALL NOT be stored as an accumulated current power

### Requirement: Copy and Transform Identity

The system SHALL allow a card instance to become a copy of another card while keeping its own instance identity.

#### Scenario: Copy instance

- **WHEN** an effect creates a copy of a card
- **THEN** the copy SHALL have its own instance identity
- **AND** it SHALL use the source's definition, cost, base power and abilities
- **AND** it SHALL NOT share mutable state with the source

#### Scenario: Transform

- **WHEN** Morph becomes a copy of an opponent's card
- **THEN** Morph's definition, base power and abilities SHALL become that card's
- **AND** Morph's instance identity SHALL remain the same
- **AND** its persistent bonuses and perks SHALL be recalculated without double-counting

#### Scenario: Copies are not draftable

- **WHEN** a copy or token is created
- **THEN** its definition SHALL NOT become a draftable card definition
- **AND** it SHALL NOT be offered by draft or rewards

### Requirement: Merge State

The system SHALL represent a merged card as a card that left the board without entering a pile.

#### Scenario: Host gains power

- **WHEN** Hulkbuster merges into a host
- **THEN** the host SHALL gain Hulkbuster's current Power as persistent Power
- **AND** the host SHALL keep its own definition and abilities
- **AND** the host's current power SHALL include the merged power in every later calculation

#### Scenario: Merged card leaves play

- **WHEN** a card merges
- **THEN** it SHALL be removed from the board
- **AND** it SHALL NOT enter discarded, destroyed or banished
- **AND** it SHALL NOT be visible in the pile viewer for the rest of the combat
- **AND** it SHALL return with the run deck in the next combat

#### Scenario: Merge is not destruction

- **WHEN** a card merges
- **THEN** no destroy, discard or banish event SHALL fire for the merged card
- **AND** destroy protections SHALL NOT block the merge

### Requirement: Next-Play Pending Effects

The system SHALL store pending effects that trigger on the next card played by a side.

#### Scenario: Pending bonus and move

- **WHEN** Forge or Iron Fist arms a next-play effect
- **THEN** the side SHALL remember it until that side plays a card
- **AND** the effect SHALL apply to the next card played from hand
- **AND** it SHALL be consumed even if its target condition cannot be met

#### Scenario: Multiple pending effects

- **WHEN** more than one next-play effect is pending
- **THEN** all SHALL apply to the same next card in a deterministic order
- **AND** returning a card to hand SHALL NOT restore a consumed pending effect

#### Scenario: Across turns

- **WHEN** a next-play effect is armed and the turn ends without another play
- **THEN** the effect SHALL remain pending for the following turn
- **AND** it SHALL NOT expire at turn end
