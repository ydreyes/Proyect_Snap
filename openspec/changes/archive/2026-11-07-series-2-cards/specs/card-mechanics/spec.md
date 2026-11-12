# Spec Delta

## MODIFIED Requirements

### Requirement: Card Trigger Types

The system SHALL support cards with `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand`, `GameStart`, `EndOfTurn` and `Activate` abilities.

#### Scenario: Card abilities

- **WHEN** a card has abilities
- **THEN** abilities SHALL be categorized by their trigger type
- **AND** each ability SHALL trigger at the appropriate game event

#### Scenario: Multiple trigger types on one card

- **WHEN** a card definition declares more than one effect
- **THEN** each effect SHALL be associated with exactly one trigger type
- **AND** the card SHALL be able to participate in more than one trigger family

#### Scenario: New triggers without content

- **WHEN** the card pool contains only `OnReveal` and `Ongoing` cards
- **THEN** the new action, hand-entry and game-start trigger families SHALL remain declared
- **AND** they SHALL NOT change the behavior of any existing card

## ADDED Requirements

### Requirement: Series 2 Card Instance State

The system SHALL represent the per-instance state introduced by the Series 2 and Collection Level cards.

#### Scenario: Combat cost override

- **WHEN** Sabretooth returns to hand or Swarm creates a copy
- **THEN** the instance SHALL expose a combat-scoped cost override of 0
- **AND** the override SHALL take precedence over permanent and turn cost modifiers
- **AND** it SHALL clear when a new combat starts

#### Scenario: Movement usage

- **WHEN** Nightcrawler or Vision is moved on a turn later than the turn it was played
- **THEN** the instance SHALL record that its printed move was used
- **AND** Nightcrawler SHALL remain used for the rest of the combat
- **AND** Vision SHALL reset at the start of the next turn
- **AND** a same-turn move of a card played this turn SHALL NOT consume the printed permission

#### Scenario: Delayed inverse condition

- **WHEN** Jessica Jones arms her condition
- **THEN** the instance SHALL record the arming turn, the watched zone and the pending bonus
- **AND** the condition SHALL resolve at the end of the following turn
- **AND** it SHALL be cleared whether it succeeds or fails
- **AND** the instance SHALL support more than one pending condition at the same time

#### Scenario: Discard counters

- **WHEN** a side discards a card
- **THEN** the side's discard count for the combat SHALL increase by 1
- **AND** the count SHALL include discards that are later replaced
- **AND** the count SHALL reset when a new combat starts

#### Scenario: Printed ability suppression

- **WHEN** Leech suppresses a card in hand
- **THEN** the instance SHALL keep its definition but its printed abilities SHALL NOT resolve
- **AND** abilities granted by attached perks SHALL continue to resolve
- **AND** the state SHALL persist until the combat ends

#### Scenario: Side ownership

- **WHEN** Hobgoblin switches sides
- **THEN** the instance SHALL belong to the opposing side's zone list
- **AND** its recorded played zone SHALL remain unchanged
- **AND** its persistent bonuses and abilities SHALL follow the new controller
- **AND** no pile entry SHALL be created

### Requirement: Card Legality Queries

The system SHALL expose one legality query for playing and moving a card.

#### Scenario: Play legality

- **WHEN** a card is in hand
- **THEN** its play legality SHALL combine energy, zone capacity, zone constraints, zone locks, floods, taunt reservations and its own printed restrictions
- **AND** the player UI, the AI and the action authority SHALL use the same result

#### Scenario: Move legality

- **WHEN** a card is in play
- **THEN** its move legality SHALL combine destination capacity, the card's printed movement permission, a per-combat or per-turn usage flag and any active movement window
- **AND** an effect-driven move SHALL NOT require a printed movement permission
- **AND** a rejected move SHALL leave the board unchanged
