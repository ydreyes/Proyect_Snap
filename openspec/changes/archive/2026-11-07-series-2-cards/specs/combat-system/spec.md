# Spec Delta

## MODIFIED Requirements

### Requirement: Cost-Gated Card Play

The system SHALL allow players to play cards from hand to zone slots during the play phase, paying each card's effective energy cost.

#### Scenario: Playing a card

- **WHEN** a player plays a card
- **THEN** the card SHALL be placed in the chosen free slot if the zone has space (max 4 per side)
- **AND** the play order SHALL be recorded
- **AND** the card's effective cost SHALL NOT exceed the available energy
- **AND** the available energy SHALL be reduced by the card's effective cost
- **AND** zone play constraints, zone locks and flooded zones SHALL be respected

#### Scenario: Unaffordable card

- **WHEN** a player attempts to play a card whose effective cost exceeds the available energy
- **THEN** the play SHALL be rejected
- **AND** the board SHALL remain unchanged

#### Scenario: Upgraded card becomes affordable

- **WHEN** a card has a `-1 cost` upgrade
- **THEN** it SHALL be playable when the available energy equals its effective cost even if that is below its base cost
- **AND** the energy preview and hand display SHALL show the effective cost

#### Scenario: Temporary cost modifier

- **WHEN** an effect changes a card's cost for the current turn
- **THEN** the effective cost SHALL include that modifier while it is active
- **AND** the modifier SHALL be cleared when the turn ends
- **AND** clearing it SHALL NOT alter the card's permanent cost delta

#### Scenario: Printed play restriction

- **WHEN** a card in hand has a printed play restriction that is not met
- **THEN** the play SHALL be rejected
- **AND** the board and energy SHALL remain unchanged
- **AND** the rejection SHALL be visible as a failed play rather than a crash

#### Scenario: Cost cap

- **WHEN** any combination of upgrades, Iceman, Sandman or another cost modifier produces an effective cost above 6
- **THEN** the effective cost SHALL be capped at 6
- **AND** the hand display, energy preview and action authority SHALL use the capped value

### Requirement: Card Destruction and Discard

The system SHALL route destruction and discard through the action authority, including protection and replacement.

#### Scenario: Destroy from board

- **WHEN** an effect destroys a board card
- **THEN** the card SHALL leave its zone and enter the destroyed pile
- **AND** its cost SHALL NOT be refunded
- **AND** an `AfterDestroyed` event SHALL be dispatched if the destroy succeeds

#### Scenario: Discard from hand or deck

- **WHEN** an effect discards a card from the hand or deck
- **THEN** the card SHALL leave that container and enter the discarded pile
- **AND** an `AfterDiscarded` event SHALL be dispatched

#### Scenario: Protection and replacement

- **WHEN** a destroy or discard is protected or replaced
- **THEN** the action SHALL NOT move the card to a pile
- **AND** the replacement effect SHALL resolve instead
- **AND** no destroy or discard event SHALL fire for a prevented action

#### Scenario: Replaced destroy still reacts

- **WHEN** Bucky Barnes or Sabretooth is destroyed and its replacement resolves
- **THEN** the replacement SHALL remove the card from play without adding it to the destroyed pile
- **AND** the destruction event SHALL still fire for other reactions
- **AND** the replacement SHALL respect the hand cap and the four-card limit

#### Scenario: Discard counters

- **WHEN** a discard succeeds
- **THEN** the discarding side's combat discard count SHALL increase by 1
- **AND** the count SHALL include discards whose card is later replaced
- **AND** Morbius SHALL read that count on every power recalculation

### Requirement: Deck Manipulation

The system SHALL support top-deck access, opponent-deck draw, shuffling, lowest-cost search, combat-scoped banish and deck-to-location pulls.

#### Scenario: Top of deck

- **WHEN** an effect targets the top card of a deck
- **THEN** it SHALL target the first card in draw order
- **AND** a buff on that card SHALL remain when it is drawn

#### Scenario: Opponent deck draw

- **WHEN** an effect draws from the opponent's deck
- **THEN** the drawn card SHALL leave that deck and enter the effect owner's hand if it has room
- **AND** the opponent SHALL NOT draw that card later in the combat

#### Scenario: Shuffle and search

- **WHEN** an effect shuffles a token into a deck
- **THEN** the token SHALL enter the deck in a random position
- **AND** when an effect searches for the lowest-Cost card
- **THEN** ties SHALL be broken randomly
- **AND** the selected card SHALL leave the deck

#### Scenario: Combat-scoped banish

- **WHEN** Yondu banishes a card
- **THEN** the card SHALL enter the banished pile for the current combat
- **AND** it SHALL NOT be removed from the run deck
- **AND** it SHALL return to the run deck for the next combat

#### Scenario: Deck card to a location

- **WHEN** Jubilee pulls the top card of its owner's deck to a location
- **THEN** the card SHALL leave the deck only if the destination has room
- **AND** it SHALL become a board instance with its Ongoing abilities active
- **AND** it SHALL be revealed and its On Reveal ability SHALL resolve
- **AND** it SHALL NOT count as played for play-triggered effects
- **AND** a failed addition SHALL leave the card on top of the deck

### Requirement: Card Creation and Merge

The system SHALL support creating tokens, copying instances, transforming a card and merging cards.

#### Scenario: Token creation

- **WHEN** an effect creates a token in a zone
- **THEN** the token SHALL be a separate instance with its own definition and current power
- **AND** the four-card limit SHALL apply
- **AND** the token SHALL NOT enter the draft or reward pool

#### Scenario: Copy creation

- **WHEN** an effect copies a card
- **THEN** the copy SHALL be a separate instance
- **AND** it SHALL carry the source's definition, current power and abilities according to the card's text
- **AND** it SHALL respect the hand or zone destination limit

#### Scenario: Transform

- **WHEN** Morph becomes a copy of a card
- **THEN** its definition, power and abilities SHALL change to the copied card
- **AND** its existing persistent bonuses SHALL NOT be double-counted
- **AND** its reveal SHALL NOT trigger again

#### Scenario: Merge

- **WHEN** Hulkbuster merges into a host
- **THEN** the host SHALL gain Hulkbuster's current power as persistent power
- **AND** Hulkbuster SHALL leave the board without entering a pile
- **AND** no destroy, discard or banish event SHALL fire for the merge
- **AND** the host SHALL keep its own definition and abilities

#### Scenario: Token replacement

- **WHEN** Bucky Barnes is destroyed
- **THEN** a Winter Soldier token SHALL be placed in the same side and location
- **AND** Bucky Barnes SHALL leave play without entering a pile
- **AND** the Winter Soldier SHALL be a new instance with 8 base Power

#### Scenario: Zero-cost copies

- **WHEN** Swarm is discarded
- **THEN** up to two copies of Swarm SHALL be created in the owner's hand
- **AND** each copy SHALL have effective cost 0 for the rest of the combat
- **AND** each copy SHALL be independent and SHALL NOT share mutable state with the source

## ADDED Requirements

### Requirement: Hand Entry Events

The system SHALL dispatch a hand-entry event when a card joins a side's hand from an effect or from the opponent's deck.

#### Scenario: Effect additions

- **WHEN** an effect creates, copies, draws or returns a card into its owner's hand
- **THEN** an `AfterAddedToHand` event SHALL be dispatched with the card, the side and the origin
- **AND** the event SHALL resolve hand-entry reactions before the action returns

#### Scenario: Excluded entries

- **WHEN** a card is drawn normally from its owner's deck
- **THEN** no `AfterAddedToHand` event SHALL be dispatched
- **AND** when the player manually returns a played card to hand
- **THEN** no `AfterAddedToHand` event SHALL be dispatched

#### Scenario: Cable and the opponent's deck

- **WHEN** Cable takes a card from the opponent's deck into its owner's hand
- **THEN** the draw event SHALL resolve for draw-triggered effects
- **AND** a hand-entry event SHALL also resolve for hand-entry effects
- **AND** the opponent SHALL NOT receive a hand-entry event

### Requirement: Side Switching

The system SHALL support a card changing controller while remaining in the same location.

#### Scenario: Hobgoblin switches

- **WHEN** Hobgoblin's On Reveal resolves
- **THEN** it SHALL be removed from its controller's side and added to the opposing side of the same location if that side has room
- **AND** its recorded played zone SHALL remain unchanged
- **AND** it SHALL NOT enter a pile
- **AND** it SHALL NOT dispatch a movement event

#### Scenario: Blocked switch

- **WHEN** the opposing side is full or the location blocks removal or addition
- **THEN** Hobgoblin SHALL remain on its original side
- **AND** no board state SHALL change except its reveal marker

### Requirement: Random Card Creation

The system SHALL support adding a random catalog card to a hand.

#### Scenario: Agent 13

- **WHEN** Agent 13 resolves
- **THEN** a random non-token card from the catalog SHALL be created as a new instance
- **AND** it SHALL be added to the owner's hand if the hand has room
- **AND** it SHALL NOT carry upgrades or attached perks from another instance
- **AND** nothing SHALL be created when the hand is full

### Requirement: Mass and Threshold Destruction

The system SHALL support destruction that targets a cost class or a Power threshold.

#### Scenario: Killmonger

- **WHEN** Killmonger resolves
- **THEN** every card in play with effective cost 1 SHALL be destroyed, on both sides
- **AND** the target set SHALL be taken before the first destroy resolves
- **AND** protected cards SHALL survive
- **AND** each successful destroy SHALL dispatch its own destruction event

#### Scenario: Shang-Chi

- **WHEN** Shang-Chi resolves
- **THEN** one random enemy card in his location with 10 or more current Power SHALL be destroyed
- **AND** protected or immune cards SHALL survive
- **AND** nothing SHALL be destroyed when no enemy card meets the threshold

### Requirement: Cost Modification and Cap

The system SHALL support cost increases that persist for the combat or for the next turn and a global cap of 6.

#### Scenario: Iceman

- **WHEN** Iceman resolves
- **THEN** one random card in the opponent's hand SHALL gain +1 effective cost for the rest of the combat
- **AND** the increase SHALL persist when the card is played or returned to hand
- **AND** the effective cost SHALL NOT exceed 6
- **AND** a card already at 6 SHALL NOT be selected for an increase

#### Scenario: Sandman

- **WHEN** Sandman resolves on turn N
- **THEN** every card in both hands SHALL cost 1 more during turn N + 1
- **AND** multiple Sandmen SHALL stack
- **AND** the increase SHALL expire at the end of turn N + 1
- **AND** the effective cost SHALL NOT exceed 6

#### Scenario: Zero-cost override

- **WHEN** Sabretooth returns to hand or Swarm creates a copy
- **THEN** the instance SHALL cost 0 for the rest of the combat
- **AND** later turn cost increases SHALL NOT raise it above 0
- **AND** the override SHALL clear when a new combat starts
