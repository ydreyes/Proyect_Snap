# ai-opponent Specification

## Purpose
The AI opponent system provides a functional opponent that plays according to the same game rules as the player.

## Requirements

### Requirement: AI Game Rule Compliance

The AI SHALL follow all the same game rules as the player.

#### Scenario: AI follows rules

- **WHEN** the AI takes actions
- **THEN** it SHALL respect zone limits (max 4 cards per side)
- **AND** it SHALL follow zone play constraints
- **AND** it SHALL play cards during its play phase
- **AND** card cost SHALL gate its plays against its own available energy

#### Scenario: AI uses the shared action authority

- **WHEN** the AI plays, moves, destroys, discards, draws, banishes or creates a card
- **THEN** it SHALL go through the same action rules as the player
- **AND** it SHALL NOT mutate zones, hands or piles through a separate path

#### Scenario: AI play events

- **WHEN** the AI plays a card from hand
- **THEN** the same `AfterPlayed` event SHALL be dispatched as for a player card
- **AND** the event SHALL be dispatched once per AI card instance per turn

#### Scenario: AI energy ledger

- **WHEN** the AI destroys, merges or banishes a card it played during the current turn
- **THEN** its available energy SHALL NOT increase
- **AND** when the AI returns a card to its hand during the current turn, its cost SHALL be refunded

#### Scenario: AI capacity for created cards

- **WHEN** an AI effect adds a card to a zone
- **THEN** the four-card limit SHALL apply to the AI side
- **AND** an addition that would exceed the limit SHALL be rejected

### Requirement: AI Energy Spending
The AI SHALL spend its own per-turn energy pool under the same rules as the player.

#### Scenario: AI follows the pool
- **WHEN** a turn starts
- **THEN** the AI's available energy SHALL equal the turn number, capped at 6
- **AND** the AI's unspent energy SHALL be lost at turn end

#### Scenario: Repeated plays
- **WHEN** the AI's play phase runs
- **THEN** the AI SHALL keep playing cards while it can afford a card and a free slot exists
- **AND** it SHALL choose among affordable cards by highest power per energy spent
- **AND** it SHALL NOT play a card whose cost exceeds its available energy

#### Scenario: Nothing affordable
- **WHEN** no card in the AI's hand can be paid with its available energy
- **THEN** the AI SHALL play nothing
- **AND** this SHALL NOT be treated as an error

### Requirement: AI Card Play
The AI SHALL play cards strategically during combat, limited by its energy pool and free slots.

#### Scenario: AI decision making
- **WHEN** it is the AI's turn to play
- **THEN** the AI SHALL evaluate the cards in its hand that it can afford
- **AND** it SHALL play a card when a free slot exists
- **AND** it SHALL keep playing while it can afford a card and a free slot exists
- **AND** any remaining unaffordable cards SHALL stay in its hand

#### Scenario: Zone spread
- **WHEN** the AI chooses where to play
- **THEN** it SHALL prefer the zone where it already has the fewest cards
- **AND** ties SHALL be resolved randomly
- **AND** it SHALL place the card in a free slot of that zone

#### Scenario: No free slot
- **WHEN** all 12 AI slots are occupied
- **THEN** the AI SHALL play nothing for that turn

### Requirement: AI Turn Participation
The AI SHALL participate fully in all turn phases.

#### Scenario: AI turn flow
- **WHEN** a turn occurs
- **THEN** the AI SHALL draw 1 card at turn start if its deck has cards
- **AND** it SHALL play affordable cards during the play phase until no affordable card or free slot remains
- **AND** each played card SHALL appear on the board immediately
- **AND** its hand SHALL NOT be rendered

### Requirement: AI Series 1 Action Use

The AI SHALL use the same action authority as the player for every Series 1 action.

#### Scenario: New actions

- **WHEN** the AI destroys, discards, moves, copies, merges, banishes or creates a token
- **THEN** it SHALL use the same action rules, capacity checks, energy ledger and event dispatch as the player
- **AND** it SHALL NOT mutate zones, hands or piles through a separate path

#### Scenario: Hand cap

- **WHEN** the AI's hand has 7 cards
- **THEN** its turn draw SHALL be skipped and the card SHALL stay in its deck
- **AND** its effects that add or copy to hand SHALL not create the card

#### Scenario: Random targets

- **WHEN** an AI card has multiple eligible targets and its text does not define the set
- **THEN** the AI SHALL choose the target randomly under the same rule as the player
- **AND** the choice SHALL be deterministic for a seeded random service

### Requirement: AI Series 1 Safety

The AI SHALL avoid playing a card whose mandatory effect would make its own position worse when another affordable card is available.

#### Scenario: Self-damaging discard or destroy

- **WHEN** the AI can afford a card with a mandatory discard or destroy of its own cards
- **AND** another affordable card does not have that drawback
- **THEN** the AI SHALL prefer the safer card
- **AND** it SHALL still play the damaging card when it is the only affordable option

#### Scenario: No legal target

- **WHEN** an AI card's mandatory effect needs a target that does not exist
- **THEN** the AI SHALL still be able to play the card
- **AND** the effect SHALL resolve without a target according to the card's text

#### Scenario: No crash on new content

- **WHEN** the AI plays or resolves any of the 46 Series 1 cards
- **THEN** the combat SHALL continue without an exception
- **AND** the AI's hand, board and energy SHALL remain consistent

### Requirement: Encounter Deck and Difficulty

The AI SHALL play the deck and difficulty of the selected enemy.

#### Scenario: Enemy deck source

- **WHEN** a combat starts
- **THEN** the AI deck SHALL come from the selected enemy definition
- **AND** the deck SHALL contain exactly the cards declared by that enemy
- **AND** the AI SHALL NOT use a random deck from the player's pool

#### Scenario: Difficulty choice

- **WHEN** the AI chooses among affordable cards
- **THEN** a difficulty of 1 or 2 SHALL use the safe power-per-energy heuristic
- **AND** a difficulty of 3 SHALL prefer the highest-power affordable card
- **AND** a difficulty of 4 or 5 SHALL also prefer the zone where the player has the least power
- **AND** the AI SHALL continue to respect energy, zone limits and self-harm safety

#### Scenario: Boss decks

- **WHEN** a sub-boss or the final boss is played
- **THEN** the AI SHALL use the boss deck
- **AND** the boss difficulty SHALL be at least 3
- **AND** the final boss SHALL use difficulty 5

### Requirement: Taunt Reservation

The system SHALL make the AI respect a taunt reservation on its first card of the following turn.

#### Scenario: First card obeys taunt

- **WHEN** the player's Taunt resolved on turn N
- **AND** the AI plays its first card on turn N + 1
- **THEN** the AI SHALL place that card in the taunted zone when the zone has room
- **AND** the AI SHALL still pay the card's cost from its own energy

#### Scenario: Taunted zone full

- **WHEN** the taunted zone has no room for the AI's first card
- **THEN** the reservation SHALL expire
- **AND** the AI SHALL play normally without being blocked

#### Scenario: No legal first card

- **WHEN** the AI has no affordable card or no legal card for the taunted zone
- **THEN** it SHALL play nothing or choose another legal zone according to the card's own restrictions
- **AND** the reservation SHALL expire at the end of the turn

#### Scenario: Multiple reservations

- **WHEN** more than one taunt reservation exists for the same AI turn
- **THEN** the system SHALL resolve them in a deterministic order
- **AND** only the first applicable reservation SHALL constrain the first card
- **AND** the remaining reservations SHALL expire

#### Scenario: AI safety

- **WHEN** the taunt reservation conflicts with the AI's self-harm safety
- **THEN** the AI SHALL still obey the taunt if a legal card exists
- **AND** it SHALL NOT play a card that is illegal in the taunted zone

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
