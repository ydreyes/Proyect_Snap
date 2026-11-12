# Spec Delta

## ADDED Requirements

### Requirement: AI Legal Play Filtering

The AI SHALL only choose plays that the action authority can accept for the current turn.

#### Scenario: Play-restricted cards

- **WHEN** the AI evaluates its hand
- **THEN** it SHALL skip cards whose printed play restriction is not met
- **AND** it SHALL consider the remaining affordable cards normally
- **AND** a hand containing only restricted cards SHALL result in no play, without an error or a loop

#### Scenario: Locked and flooded zones

- **WHEN** the AI chooses a destination
- **THEN** it SHALL skip locations that are play-locked, flooded for the current turn or otherwise closed to the card
- **AND** it SHALL use another legal location when one exists
- **AND** it SHALL NOT choose an illegal location and stop its whole play phase

#### Scenario: Taunt and locks

- **WHEN** a taunt reservation points at a location that is play-locked or flooded
- **THEN** the reservation SHALL expire
- **AND** the AI SHALL play normally in a legal location
- **AND** it SHALL NOT stall waiting for the reserved location

#### Scenario: No legal option

- **WHEN** no combination of affordable card and legal location exists
- **THEN** the AI SHALL play nothing for that turn
- **AND** the turn SHALL continue normally

### Requirement: AI Series 2 Compliance

The AI SHALL use the same action authority and rules as the player for every Series 2 and Collection Level card.

#### Scenario: Cost increases

- **WHEN** Iceman or Sandman increases the cost of an AI hand card
- **THEN** the AI SHALL evaluate the increased effective cost
- **AND** it SHALL NOT play a card whose increased cost exceeds its available energy
- **AND** the cost SHALL be capped at 6

#### Scenario: Suppressed text

- **WHEN** Leech disables the printed abilities of an AI hand card
- **THEN** the AI SHALL NOT rely on those printed abilities for its decision
- **AND** attached perk abilities SHALL remain available to the AI evaluation

#### Scenario: Switch sides and tokens

- **WHEN** an AI card switches sides, creates a token or replaces itself
- **THEN** the board, hand and energy SHALL remain consistent
- **AND** the combat SHALL continue without an exception
- **AND** the AI SHALL NOT treat the switched card as one of its own for later plays

#### Scenario: Shared action authority

- **WHEN** the AI resolves any Series 2 effect that destroys, discards, moves, copies, draws or creates a card
- **THEN** it SHALL use the same action rules, capacity checks, protections and event dispatch as the player
- **AND** it SHALL NOT mutate zones, hands or piles through a separate path
