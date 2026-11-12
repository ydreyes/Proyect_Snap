# Spec Delta

## MODIFIED Requirements

### Requirement: AI Legal Play Filtering

The AI SHALL only choose plays that the action authority can accept for the current turn.

#### Scenario: Play-restricted cards

- **WHEN** the AI evaluates its hand
- **THEN** it SHALL skip cards whose printed play restriction is not met
- **AND** it SHALL skip cards that are illegal for the chosen zone, including Crossbones, Giganto, Sentry and Goose restrictions
- **AND** it SHALL consider the remaining affordable cards normally
- **AND** a hand containing only restricted cards SHALL result in no play, without an error or a loop

#### Scenario: Locked and flooded zones

- **WHEN** the AI chooses a destination
- **THEN** it SHALL skip locations that are play-locked, flooded for the current turn or otherwise closed to the card
- **AND** it SHALL skip a Goose zone for cards that cost 4 or more
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

#### Scenario: Side-level restrictions

- **WHEN** Electro limits the AI to one card per turn, or Jean Grey forces the first card into her zone
- **THEN** the AI SHALL respect the limit and the forced zone
- **AND** it SHALL skip further plays after the Electro limit
- **AND** it SHALL use another zone only when the Jean Grey zone is unavailable to the card

## ADDED Requirements

### Requirement: AI Series 3 Compliance

The AI SHALL use the same action authority, cost resolver and restrictions as the player for every Series 3 card.

#### Scenario: Shared cost

- **WHEN** a Series 3 cost effect changes an AI hand card
- **THEN** the AI SHALL evaluate the resolved cost
- **AND** it SHALL NOT play a card whose cost exceeds its available energy
- **AND** Mobius, Wave and the global cap SHALL apply to the AI exactly as to the player

#### Scenario: Shared restrictions

- **WHEN** Crossbones, Giganto, Sentry, Goose, Jean Grey, Electro or Wave is active
- **THEN** the AI SHALL use the same legality query as the player
- **AND** it SHALL NOT stall or choose an illegal zone
- **AND** it SHALL end its play phase normally when no legal play exists

#### Scenario: Board consistency

- **WHEN** an AI card is destroyed, resurrected, copied, pulled, switched or moved by a Series 3 effect
- **THEN** the board, hands, piles and energy SHALL remain consistent
- **AND** the combat SHALL continue without an exception
- **AND** the AI SHALL NOT treat a switched or removed card as one of its own

#### Scenario: Activate

- **WHEN** an AI card has an `Activate` effect
- **THEN** the AI SHALL play the card for its body
- **AND** it SHALL NOT activate the ability in this change
- **AND** the card SHALL remain a legal normal play

### Requirement: Agatha Auto-Play

The system SHALL let Agatha Harkness auto-play the player's even turns within the same legality rules.

#### Scenario: Even turn

- **WHEN** Agatha has been drawn and the current turn is even
- **THEN** the AI strategy SHALL make the player's plays with the player's hand and energy
- **AND** it SHALL use the same legality query and cost resolver as the player
- **AND** it SHALL not move cards or activate abilities
- **AND** the turn SHALL end automatically after the legal plays resolve

#### Scenario: No legal play

- **WHEN** Agatha controls an even turn and no legal play exists
- **THEN** the turn SHALL end without an error or a loop
- **AND** the board SHALL remain consistent

#### Scenario: Odd turn

- **WHEN** Agatha has been drawn and the current turn is odd
- **THEN** the player SHALL control the turn normally
- **AND** the AI SHALL NOT play for the player

#### Scenario: Control persistence

- **WHEN** Agatha leaves the hand after being drawn
- **THEN** the control SHALL persist for the rest of the combat
- **AND** the UI SHALL show the automatic-turn state
