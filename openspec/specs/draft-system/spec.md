# draft-system Specification

## Purpose
The draft system handles initial deck building through three themed rounds, five random rounds, owned-card selection and a one-copy limit.

## Requirements

### Requirement: Initial Draft

The system SHALL build the initial 8-card deck through 3 themed rounds and 5 random rounds.

#### Scenario: Draft selection

- **WHEN** starting a new run
- **THEN** the draft SHALL present three themed rounds followed by five random rounds
- **AND** each round SHALL show 3 card choices
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
- **AND** the themed groups SHALL be regenerated

#### Scenario: Draft checkpoint

- **WHEN** the player makes a draft pick
- **THEN** the draft state SHALL be written to `run.json`
- **AND** the state SHALL include the phase, step index, groups, current offers, picks and copy counts
- **AND** the write SHALL complete before the next round is drawn

#### Scenario: Draft resume

- **WHEN** the app is reopened with a draft checkpoint
- **THEN** `Continuar` SHALL resume the same round
- **AND** the current offers SHALL be the same
- **AND** the draft SHALL NOT reroll the groups or the offers
- **AND** completing the draft SHALL clear the draft state from the checkpoint

#### Scenario: Themed phase

- **WHEN** the draft is in one of the first three rounds
- **THEN** the round SHALL use one of the Control, Destroy or OnReveal groups
- **AND** the three offers SHALL come from that group's owned cards
- **AND** the theme name SHALL be shown above the offers
- **AND** each group SHALL be used exactly once

#### Scenario: Random phase

- **WHEN** the draft reaches rounds 4 through 8
- **THEN** the offers SHALL come from the remaining owned pool
- **AND** the offers SHALL NOT be constrained by theme
- **AND** the cards not picked in the themed phase SHALL return to the pool

### Requirement: Copy Limits in Draft

The system SHALL enforce a one-copy limit during the draft.

#### Scenario: Copy limit enforcement

- **WHEN** selecting cards during draft
- **THEN** a card already picked in the draft SHALL NOT be offered again
- **AND** a card SHALL NOT be added more than once to the draft deck
- **AND** the second copy SHALL only come from the in-game shop or the level-up node
- **AND** the system SHALL track copies used

#### Scenario: Exhausted card excluded

- **WHEN** a card has already been picked in the current draft
- **THEN** that card SHALL NOT appear among the choices of any later round
- **AND** the pool SHALL be refilled from the remaining owned cards

#### Scenario: Distinct choices

- **WHEN** presenting the 3 choices of a round
- **THEN** the 3 choices SHALL be distinct card definitions
- **AND** a choice SHALL be an owned card
- **AND** a token definition SHALL NOT be offered

### Requirement: Card Pool

The system SHALL draft from the owned card pool.

#### Scenario: Card selection from pool

- **WHEN** presenting draft choices
- **THEN** cards SHALL be selected from the cards the profile owns
- **AND** the pool SHALL contain the 21 starter cards plus every card unlocked in the global store
- **AND** the pool SHALL support adding new cards through ownership
- **AND** the pool SHALL be able to grow to 217 cards

#### Scenario: Token definitions are excluded

- **WHEN** presenting draft choices
- **THEN** token definitions SHALL NOT be offered
- **AND** a card created by an effect during combat SHALL NOT enter the draft pool
- **AND** the draft SHALL continue to offer only owned, draftable definitions

#### Scenario: Series 3 cards enter the pool

- **WHEN** a player owns a Series 3 card
- **THEN** it SHALL be eligible for the draft
- **AND** it SHALL participate in the themed and random rounds like any other card
- **AND** an unowned Series 3 card SHALL NOT be offered
- **AND** the 8-pick structure SHALL remain unchanged

### Requirement: Draft Completion

The system SHALL hand the drafted deck to combat when the draft completes.

#### Scenario: Deck handoff

- **WHEN** the player completes the 8th pick
- **THEN** the 8 drafted cards SHALL become the player's combat deck
- **AND** the game SHALL advance to the combat scene
- **AND** the drafted deck SHALL be preserved for the duration of the run
- **AND** the draft state SHALL be cleared from the checkpoint
