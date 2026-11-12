# combat-system Specification

## Purpose
The combat system manages zone-based battles with priority calculation, card play, turn phases, time limits, and proper reveal ordering of cards based on priority and play order.

## Requirements

### Requirement: Three Zones

The system SHALL provide exactly 3 zones for combat.

#### Scenario: Zone initialization

- **WHEN** combat begins
- **THEN** exactly 3 zones SHALL be created
- **AND** each zone SHALL support up to 4 cards per side (player and AI)
- **AND** each side's 4 slots SHALL be displayed as a 2 row by 2 column grid

#### Scenario: Capacity applies to every source

- **WHEN** a card is added to a zone by a play, a move, a token effect, a copy, a merge or a regeneration
- **THEN** the addition SHALL be rejected if that side already has 4 cards in the zone
- **AND** the board SHALL remain unchanged
- **AND** the effect that attempted the addition SHALL complete without adding the card

### Requirement: Zone Reveal Order
The system SHALL reveal one zone at the start of each of the first three turns of a combat, in order, and keep it revealed for the rest of that combat.

#### Scenario: Turn start zone reveals
- **WHEN** a turn begins
- **THEN** the next unrevealed zone SHALL be revealed
- **AND** previously revealed zones SHALL stay revealed
- **AND** each zone reveal MAY trigger zone effects

#### Scenario: Combat starts with zones hidden

- **WHEN** a combat begins
- **THEN** all three zones SHALL start unrevealed
- **AND** the zone name, effect text and art SHALL be withheld until the zone is revealed
- **AND** the zone index and both power totals SHALL remain visible

#### Scenario: First turn reveals zone 1

- **WHEN** the first turn of a combat begins
- **THEN** zone 1 SHALL be revealed
- **AND** zones 2 and 3 SHALL remain unrevealed

#### Scenario: Second turn reveals zone 2

- **WHEN** the second turn begins
- **THEN** zone 2 SHALL be revealed
- **AND** zone 1 SHALL remain revealed
- **AND** zone 3 SHALL remain unrevealed

#### Scenario: Third turn reveals zone 3

- **WHEN** the third turn begins
- **THEN** zone 3 SHALL be revealed
- **AND** all three zones SHALL remain revealed for the rest of the combat
- **AND** each zone reveal MAY trigger zone effects

#### Scenario: Hidden zones remain playable

- **WHEN** a zone is unrevealed
- **THEN** it SHALL accept cards from both sides
- **AND** its power totals SHALL count for control and priority
- **AND** its play constraints SHALL apply once revealed

#### Scenario: Reveal before resolution

- **WHEN** a combat resolves with zones still unrevealed
- **THEN** every unrevealed zone SHALL be revealed before control and the match result are computed

### Requirement: Priority Calculation
The system SHALL determine turn priority based on zone control at the end of each turn, after all effects and power changes for that turn have resolved.

#### Scenario: Priority determination
- **WHEN** a turn ends
- **THEN** the system SHALL apply OnReveal effects and recalculate card and zone power first
- **AND** then it SHALL calculate which player controls each zone based on total power
- **AND** the player who controls more zones SHALL have priority for the next turn's reveal order
- **AND** priority ties SHALL retain the previous priority owner

#### Scenario: Priority sees final power
- **WHEN** an OnReveal or Ongoing effect changes zone power at turn end
- **THEN** the priority calculation SHALL use the changed total
- **AND** it SHALL NOT use a total calculated before the effects resolved

### Requirement: Turn Energy Pool

The system SHALL grant a per-turn energy pool equal to the current turn number, capped at 6, adjusted by active run modifiers.

#### Scenario: Pool size follows the turn

- **WHEN** a turn starts
- **THEN** the available energy pool SHALL equal the turn number
- **AND** on turns beyond 6 the pool SHALL remain 6
- **AND** with `energy_bonus` active the pool SHALL be 1 higher
- **AND** with `energy_cap` active the cap SHALL be 7

#### Scenario: Energy does not carry over

- **WHEN** a turn ends with unspent energy
- **THEN** the unspent energy SHALL be lost
- **AND** the next turn SHALL start with a fresh pool for that turn

#### Scenario: Energy modifiers are visible

- **WHEN** a run modifier changes the pool or the cap
- **THEN** the energy indicator SHALL show the modified value
- **AND** the AI SHALL use the same modified pool

### Requirement: Energy Expenditure and Refund

The system SHALL derive each side's available energy from an explicit per-turn ledger of charges and refunds, using each card's effective cost.

#### Scenario: Available energy after a play

- **WHEN** the player plays a card with effective cost n during the current turn
- **THEN** the available energy SHALL decrease by n
- **AND** n SHALL include the card's upgrades and any temporary cost modifier active at the moment of the play
- **AND** the card SHALL record the turn in which it was played

#### Scenario: Cards from earlier turns do not consume energy

- **WHEN** a card was played on an earlier turn and remains in a zone
- **THEN** its cost SHALL NOT reduce the current turn's available energy

#### Scenario: Returning a card refunds its cost

- **WHEN** a card played during the current turn is returned to the player's hand
- **THEN** the amount charged for that play SHALL be added back to the available energy
- **AND** it SHALL be charged again at its effective cost if replayed

#### Scenario: Moving a card already in play

- **WHEN** a card is moved from one zone slot to another
- **THEN** the available energy SHALL NOT change

#### Scenario: Destroying, merging or banishing does not refund

- **WHEN** a card played during the current turn is destroyed, merged or banished
- **THEN** its cost SHALL remain spent for that turn
- **AND** the available energy SHALL NOT increase

#### Scenario: Lifting a card does not free its cost

- **WHEN** a card played during the current turn is lifted from the board
- **THEN** its cost SHALL remain committed while the drag is active
- **AND** the available energy SHALL NOT increase
- **AND** dropping it into another slot SHALL NOT change its cost
- **AND** returning it to the hand SHALL refund the cost

#### Scenario: Both sides use the ledger

- **WHEN** the AI plays, destroys, merges, banishes, moves or returns a card
- **THEN** the AI's available energy SHALL follow the same charge and refund rules as the player's

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

### Requirement: Turn Phases

The system SHALL follow a defined turn structure.

#### Scenario: Turn structure

- **WHEN** a turn progresses
- **THEN** it SHALL go through TurnStart (draw for both sides)
- **AND** PlayPhase (time-limited play)
- **AND** TurnEnd (OnReveal resolution, power recalculation, priority and turn advance)

#### Scenario: In-play actions resolve immediately

- **WHEN** a card is played, moved, destroyed, discarded, activated or drawn during PlayPhase
- **THEN** the action's triggered effects SHALL resolve before control returns to the caller
- **AND** the turn SHALL remain in PlayPhase
- **AND** the end of the turn SHALL still resolve OnReveal and pending effects

#### Scenario: Match length

- **WHEN** the final turn ends
- **THEN** the match SHALL resolve delayed reveals and EndOfGame effects after that turn's OnReveal effects and power recalculation
- **AND** the match SHALL be evaluated only after the end-of-game resolution
- **AND** no further turns SHALL start
- **AND** the final turn SHALL be turn 6 unless a Limbo zone extends the match to turn 7

#### Scenario: Activate during play

- **WHEN** a player activates a card during PlayPhase
- **THEN** the activation SHALL resolve immediately through the action authority
- **AND** it SHALL NOT consume energy or advance the turn
- **AND** it SHALL NOT create a play record

### Requirement: Time Limits
The system SHALL enforce time limits on play phases.

#### Scenario: Time limit expiration
- **WHEN** the play phase time limit expires
- **THEN** the game SHALL proceed with whatever cards were played
- **AND** no additional cards SHALL be playable
- **AND** the turn SHALL advance to TurnEnd

### Requirement: Card Reveal Ordering

The system SHALL reveal cards in order based on priority and play order.

#### Scenario: Reveal sequence

- **WHEN** cards are revealed at TurnEnd
- **THEN** all cards played during the current turn that are still in play and have not already revealed SHALL be included
- **AND** all cards of the player with priority SHALL be revealed first, in the order they were played
- **AND** then all cards of the other player SHALL be revealed, in the order they were played
- **AND** OnReveal effects SHALL trigger in this reveal order
- **AND** each card's OnReveal effect SHALL resolve before the next card in the sequence

#### Scenario: Reveal is instantaneous

- **WHEN** the player ends the turn
- **THEN** the reveal sequence and its effects SHALL resolve in the same frame
- **AND** no extra phase, input block or animation SHALL be required
- **AND** the next turn SHALL start after resolution

#### Scenario: Cards reveal once

- **WHEN** a card has already been revealed on an earlier turn
- **THEN** it SHALL NOT be included in a later reveal sequence
- **AND** its OnReveal effect SHALL NOT trigger again

#### Scenario: Returning to hand does not re-arm reveal

- **WHEN** a revealed card returns to hand and is played again during the same combat
- **THEN** it SHALL NOT be included in a later reveal sequence
- **AND** its OnReveal effect SHALL NOT trigger again

#### Scenario: Card leaves play before its reveal

- **WHEN** a card is destroyed, discarded or otherwise leaves play before its reveal
- **THEN** it SHALL NOT be included in the reveal sequence
- **AND** the sequence SHALL continue with the next card

### Requirement: Opening Hand
The system SHALL deal an opening hand to both sides at the start of a combat, guaranteeing cards tagged to start in hand.

#### Scenario: Tagged cards are dealt first
- **WHEN** a combat starts
- **THEN** every card in a side's deck tagged to start in hand SHALL be moved to that side's opening hand before the random fill
- **AND** this SHALL apply to the player and the AI
- **AND** if the side has multiple copies, every copy SHALL be moved

#### Scenario: Fill to the starting size
- **WHEN** the tagged cards have been moved
- **THEN** the hand SHALL be filled to the starting hand size from the remaining deck
- **AND** if the deck runs out, the hand SHALL contain what was available
- **AND** a hand with more tagged cards than the starting size SHALL keep all of them

### Requirement: Per-Turn Draw

The system SHALL draw one card for each side at the start of every turn while that side's deck has cards and its hand has room.

#### Scenario: Both sides draw

- **WHEN** a turn starts
- **THEN** the player SHALL draw 1 card from the player deck if available and the hand has room
- **AND** the AI SHALL draw 1 card from the AI deck if available and the hand has room
- **AND** both drawn cards SHALL be added to the corresponding hand

#### Scenario: Empty deck

- **WHEN** a side's deck is empty at turn start
- **THEN** that side SHALL draw nothing
- **AND** the turn SHALL still start normally

#### Scenario: Full hand

- **WHEN** a side's hand already has 7 cards at turn start
- **THEN** that side SHALL NOT draw
- **AND** the top card SHALL stay in the deck
- **AND** the turn SHALL still start normally

### Requirement: Hand Capacity

The system SHALL cap each hand at 7 cards and define the behavior of every addition path when the hand is full.

#### Scenario: Effect addition to a full hand

- **WHEN** an effect would add or copy a card into a hand that already has 7 cards
- **THEN** the card SHALL NOT be created or moved
- **AND** the source card, deck or opponent hand SHALL remain unchanged
- **AND** the effect SHALL complete without an error

#### Scenario: Return to a full hand

- **WHEN** a player returns a played card from the board to a hand that already has 7 cards
- **THEN** the return SHALL be rejected
- **AND** the card SHALL stay in its board slot
- **AND** its energy SHALL remain committed

#### Scenario: Token and copy additions

- **WHEN** a token or copy would enter a hand
- **THEN** the hand-cap check SHALL apply before the instance is created
- **AND** a rejected addition SHALL NOT leave a partial instance in any container

### Requirement: Card Movement

The system SHALL support moving cards between locations as a board action, including single moves and simultaneous batches.

#### Scenario: Single move

- **WHEN** an effect moves a card to another location
- **THEN** the card SHALL leave its origin and enter the destination if the destination has room
- **AND** its recorded played zone SHALL remain the zone where it was played
- **AND** its energy SHALL NOT change

#### Scenario: Batch move

- **WHEN** an effect moves several cards at once, such as Heimdall
- **THEN** the moves SHALL resolve from one board snapshot
- **AND** a card SHALL NOT move if its destination would exceed 4 cards for that side
- **AND** the final board SHALL NOT depend on list iteration order

#### Scenario: Movement events

- **WHEN** a card moves into a location
- **THEN** an `AfterMoved` event SHALL be dispatched with origin and destination
- **AND** movement reactions SHALL resolve before the action returns

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

### Requirement: Forced Turn-2 Draw

The system SHALL guarantee that Domino is drawn on turn 2 and not before, while still respecting the hand cap.

#### Scenario: Domino is not in the opening hand

- **WHEN** a combat starts with Domino in the deck
- **THEN** she SHALL NOT be dealt in the opening hand
- **AND** she SHALL remain in the deck

#### Scenario: Domino on turn 2

- **WHEN** turn 2 starts and Domino is in the deck
- **THEN** she SHALL be the card drawn for that side if the hand has room
- **AND** she SHALL NOT be drawn on turn 1 or earlier

#### Scenario: Full hand on turn 2

- **WHEN** turn 2 starts with a full hand
- **THEN** Domino SHALL stay in the deck
- **AND** she SHALL be eligible for a later draw

### Requirement: Combat Item Use

The system SHALL resolve item effects through the combat action authority.

#### Scenario: Action authority

- **WHEN** an item destroys, moves, draws, copies or changes power
- **THEN** it SHALL use the same action rules, capacity checks, protections and event dispatch as the equivalent card action
- **AND** it SHALL NOT mutate zones, hands or piles through a separate path
- **AND** it SHALL update the board version and power totals

#### Scenario: Random target

- **WHEN** an item has more than one legal target and its text does not define the set
- **THEN** the target SHALL be chosen uniformly at random
- **AND** the choice SHALL be deterministic for a seeded random service

#### Scenario: No target

- **WHEN** an item has no legal target
- **THEN** the use SHALL be rejected
- **AND** the board SHALL remain unchanged
- **AND** the item SHALL NOT be consumed

#### Scenario: Turn interaction

- **WHEN** an item adds energy
- **THEN** the added energy SHALL be available for the current turn only
- **AND** it SHALL NOT carry to the next turn
- **AND** the energy indicator SHALL update immediately

### Requirement: Perk Ability Hooks

The system SHALL apply attached perk abilities at the same combat hooks as card abilities.

#### Scenario: Ability hooks

- **WHEN** a player card's power, cost, energy, movement, destruction or play legality is resolved
- **THEN** every ability granted by an attached perk SHALL be included in the result
- **AND** each attached copy SHALL apply once
- **AND** the result SHALL update in the same frame as the board change

#### Scenario: No global rule perks

- **WHEN** a combat starts
- **THEN** there SHALL be no side-wide first-play, Ongoing, On Reveal, low-cost, fewest-zone or turn-5 perk rule
- **AND** only abilities attached to the host card SHALL apply

#### Scenario: AI isolation

- **WHEN** the AI plays or scores a card
- **THEN** perks attached to player cards SHALL NOT apply to the AI side
- **AND** the AI SHALL use only its enemy deck, difficulty behavior and its own card abilities

#### Scenario: Deterministic stacking

- **WHEN** two copies of the same perk are attached
- **THEN** they SHALL resolve as two independent grants in a deterministic order
- **AND** a recalculation SHALL produce the same result
- **AND** the repetition SHALL be bounded by the copy limit and the event limit

#### Scenario: Scoring before resolution

- **WHEN** a perk ability changes power or cost
- **THEN** the change SHALL be applied before priority, victory and the next turn are evaluated
- **AND** the displayed power and energy SHALL match the resolved values

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

The system SHALL support cost increases, reductions, absolute sets and caps that persist for the combat or for the next turn, resolved by one ordered authority.

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

#### Scenario: Dynamic reductions

- **WHEN** Death, She-Hulk, Stature or Miles Morales is evaluated
- **THEN** Death SHALL cost 1 less for each destroyed card
- **AND** She-Hulk SHALL cost 1 less for each unspent Energy last turn
- **AND** Stature SHALL cost 1 if the opponent discarded a card this game
- **AND** Miles Morales SHALL cost 1 if a card moved last turn
- **AND** no reduction SHALL produce a negative cost

#### Scenario: Auras and containers

- **WHEN** Sera, Quinjet or Zabu is in play
- **THEN** Sera SHALL reduce cards in hand by 1, minimum 1
- **AND** Quinjet SHALL reduce created cards by 1, minimum 1
- **AND** Zabu SHALL reduce 4-Cost cards in the deck by 1
- **AND** the reductions SHALL be derived, not accumulated

#### Scenario: Absolute sets and caps

- **WHEN** Baron Mordo, Wave or Mobius M. Mobius is active
- **THEN** Baron Mordo SHALL set the top card of the opponent's deck to 6 until turn 6
- **AND** Wave SHALL cap all cards at 4 until the end of the next turn
- **AND** Mobius SHALL prevent increases for its owner and reductions for its opponent
- **AND** the cap SHALL apply after reductions and before the floor

### Requirement: End-of-Game Resolution

The system SHALL resolve end-of-game effects after the final turn and before victory.

#### Scenario: Resolution order

- **WHEN** the final turn ends
- **THEN** delayed-reveal cards SHALL reveal in play order and resolve their OnReveal effects
- **AND** EndOfGame effects SHALL resolve in side, zone and slot order
- **AND** powers SHALL be recomputed after each effect
- **AND** victory SHALL be evaluated after the resolution completes

#### Scenario: Captain Marvel

- **WHEN** Captain Marvel resolves at end of game
- **THEN** she SHALL move to a legal zone where her owner would win the location after moving
- **AND** if multiple zones qualify, the choice SHALL be deterministic
- **AND** if none qualify, she SHALL stay

#### Scenario: Martyr

- **WHEN** Martyr resolves at end of game
- **THEN** she SHALL move to a legal zone where her owner would lose the location after moving
- **AND** if multiple zones qualify, the choice SHALL be deterministic
- **AND** if none qualify, she SHALL stay

#### Scenario: M'Baku

- **WHEN** M'Baku is in the deck at end of game
- **THEN** he SHALL jump to his owner's lowest-power zone that is not full
- **AND** he SHALL enter revealed
- **AND** he SHALL NOT trigger a play event

#### Scenario: Dracula

- **WHEN** Dracula resolves at end of game
- **THEN** he SHALL discard a random card from his owner's hand
- **AND** he SHALL gain that card's power
- **AND** the discard SHALL dispatch the normal discard event

#### Scenario: No loop

- **WHEN** an end-of-game effect moves a card
- **THEN** the moved card SHALL NOT trigger another end-of-game resolution
- **AND** each end-of-game card SHALL resolve once
- **AND** the phase SHALL be bounded

### Requirement: Series 3 Actions

The system SHALL expose the new actions through the action authority.

#### Scenario: Draw and resurrect

- **WHEN** Adam Warlock, Crystal, Maximus or Jane Foster draws, or Ghost Rider or Hela resurrects
- **THEN** the action SHALL use the normal hand and zone capacity rules
- **AND** a draw SHALL respect the Widow's Bite lock
- **AND** a resurrected card SHALL be revealed without resolving its OnReveal

#### Scenario: Copy to other zones

- **WHEN** Arnim Zola or Nimrod creates copies
- **THEN** the action SHALL create independent copies in each other zone with room
- **AND** the copies SHALL be revealed without resolving their OnReveal

#### Scenario: Zone swap

- **WHEN** Quake resolves
- **THEN** the other two zone positions SHALL exchange definitions, cards, flood state and play constraints
- **AND** the zone indices SHALL remain stable
- **AND** the UI SHALL reconcile from the board version

#### Scenario: Enemy hand

- **WHEN** Doctor Octopus, Black Bolt, Moon Knight, Silver Samurai, Master Mold or Black Widow affects the enemy hand
- **THEN** the action SHALL respect hand capacity
- **AND** a pull SHALL mark the card revealed and resolve its OnReveal
- **AND** a discard SHALL increment the enemy discard counter
- **AND** an addition SHALL dispatch the hand-entry event for the enemy

#### Scenario: Max Energy

- **WHEN** Electro or Havok resolves
- **THEN** the board SHALL record a per-side Max Energy delta
- **AND** the next energy pool SHALL include the delta
- **AND** the delta SHALL reset when a new combat starts

#### Scenario: Next-turn energy

- **WHEN** Psylocke resolves
- **THEN** her owner SHALL receive +1 Energy at the start of the next turn
- **AND** the bonus SHALL expire after that turn

### Requirement: Series 3 Play Restrictions

The system SHALL apply the Series 3 card, zone and side restrictions through the shared legality query.

#### Scenario: Objective cards

- **WHEN** Crossbones, Giganto or Sentry is played
- **THEN** Crossbones SHALL be legal only where its owner is winning
- **AND** Giganto SHALL be legal only in the left zone
- **AND** Sentry SHALL be illegal in the right zone
- **AND** Sentry's Void SHALL enter the right zone if it has room

#### Scenario: Goose

- **WHEN** Goose is in play
- **THEN** no card with effective cost 4 or more SHALL be played in that zone by either side
- **AND** movement and effect additions SHALL remain allowed

#### Scenario: Jean Grey

- **WHEN** Jean Grey is in play and a side has not played a card this turn
- **THEN** that side's first card SHALL be played in Jean Grey's zone when the card can legally go there
- **AND** a full, flooded or otherwise illegal Jean Grey zone SHALL NOT block other zones

#### Scenario: Electro

- **WHEN** Electro is in play
- **THEN** its owner SHALL play at most one card per turn
- **AND** the limit SHALL reset at the start of the next turn

#### Scenario: Wave

- **WHEN** Wave resolves
- **THEN** all cards SHALL cost at most 4 until the end of the next turn
- **AND** the cap SHALL affect both sides, hand, deck and created cards

#### Scenario: Limbo

- **WHEN** Magik resolves on turn 5 or earlier
- **THEN** her zone SHALL be replaced with Limbo
- **AND** the match SHALL last seven turns
- **AND** Magik SHALL do nothing on turn 6
- **AND** Scarlet Witch's random replacement SHALL NOT choose Limbo

### Requirement: Series 3 Reactions

The system SHALL resolve the new play and movement reactions through the shared event dispatch.

#### Scenario: Play reactions

- **WHEN** Echo, Negasonic Teenage Warhead, Silk or Titania reacts to a play
- **THEN** the effect SHALL filter by source, side and zone
- **AND** a self-play SHALL NOT trigger Silk or Titania
- **AND** Echo SHALL disable the played enemy Ongoing card
- **AND** Negasonic SHALL destroy the first enemy card played in her zone once per combat

#### Scenario: Movement reactions

- **WHEN** Hercules, Kingpin, Dagger, Human Torch or Spider-Man 2099 reacts to a move
- **THEN** the effect SHALL use the destination zone and the moving card
- **AND** Hercules SHALL move the card again only once per card per turn
- **AND** the event dispatch SHALL remain bounded

#### Scenario: Turn reactions

- **WHEN** Nebula reaches the end of a turn
- **THEN** she SHALL gain +2 Power if the opponent did not play in her zone that turn
- **AND** the turn she was played SHALL NOT count
