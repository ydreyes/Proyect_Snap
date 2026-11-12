# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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
