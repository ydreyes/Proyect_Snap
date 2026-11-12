# Spec Delta

## MODIFIED Requirements

### Requirement: Effect Trigger Types

The system SHALL implement `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand`, `GameStart`, `StartOfTurn`, `EndOfTurn`, `Activate` and `EndOfGame` as active trigger families. Card and perk abilities SHALL use the same trigger families.

#### Scenario: Different trigger types

- **WHEN** effects are defined
- **THEN** each effect SHALL be associated with one of the supported trigger types
- **AND** `OnReveal` effects SHALL fire when their card is revealed
- **AND** `Ongoing` effects SHALL stay active while their card is in play
- **AND** `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand` and `GameStart` effects SHALL fire only when the matching action or combat event occurs
- **AND** `StartOfTurn` effects SHALL fire at the start of a turn
- **AND** `EndOfTurn` effects SHALL fire when a turn ends
- **AND** `Activate` effects SHALL fire when the owner activates the card and SHALL NOT fire on their own
- **AND** `EndOfGame` effects SHALL fire after the final turn ends and before victory is evaluated

#### Scenario: OnReveal fires once

- **WHEN** a card or perk with an `OnReveal` effect is revealed at the end of a turn
- **THEN** its `OnReveal` effect SHALL be applied exactly once
- **AND** it SHALL NOT be applied again on later turns
- **AND** it SHALL NOT be applied again if the card returns to hand and is played again during the same combat

#### Scenario: Ongoing is continuously active

- **WHEN** a card or perk with an `Ongoing` effect is on the board
- **THEN** its effect SHALL contribute to every power calculation while the card remains in play
- **AND** it SHALL NOT be applied as a one-time mutation

#### Scenario: EndOfTurn fires

- **WHEN** a turn ends
- **THEN** every active `EndOfTurn` effect SHALL resolve before priority, victory and the next turn are evaluated
- **AND** an `EndOfTurn` effect SHALL fire once per instance and turn

#### Scenario: StartOfTurn fires

- **WHEN** a turn starts
- **THEN** every active `StartOfTurn` effect SHALL resolve before the player's play phase begins
- **AND** any energy granted by it SHALL be available for that turn only

#### Scenario: Activate fires

- **WHEN** a player activates a card with an unused `Activate` effect during the play phase
- **THEN** the `Activate` effect SHALL be applied once
- **AND** the card SHALL NOT be activated again during the same combat
- **AND** the activation SHALL NOT create a play record or an `AfterPlayed` trigger

#### Scenario: EndOfGame fires

- **WHEN** the final turn ends
- **THEN** every active `EndOfGame` effect SHALL resolve before victory is evaluated
- **AND** delayed reveals SHALL resolve before `EndOfGame` effects
- **AND** no further turn SHALL start

#### Scenario: Inactive triggers do not fire

- **WHEN** a turn ends or a player activates a card
- **THEN** `EndOfTurn` effects SHALL resolve
- **AND** `Activate` effects SHALL fire only when the player explicitly activates the card
- **AND** no other trigger SHALL fire without its matching action or phase

#### Scenario: Action triggers do not fire without content

- **WHEN** a card is played, moved, destroyed, discarded, drawn or added to hand
- **AND** no card or perk references the matching action trigger
- **THEN** no effect SHALL be applied
- **AND** the action SHALL complete normally

#### Scenario: Action triggers fire with content

- **WHEN** a card or perk references an action trigger
- **AND** the matching play, move, destroy, discard, draw or hand addition occurs
- **THEN** the effect SHALL fire in the same resolution
- **AND** the action SHALL complete normally

#### Scenario: Added-to-hand fires

- **WHEN** a card enters its owner's hand because of an effect or because it was taken from the opponent's deck
- **THEN** every `AfterAddedToHand` effect of the owner SHALL fire
- **AND** a normal draw from the owner's own deck SHALL NOT fire it
- **AND** a manual board-to-hand return by the player SHALL NOT fire it

### Requirement: Effect Processor

The system SHALL process card effects and attached perk effects according to their trigger types at the appropriate times.

#### Scenario: OnReveal processing

- **WHEN** a turn ends
- **THEN** the system SHALL compute the reveal order of cards played this turn
- **AND** it SHALL apply each card's `OnReveal` effects and every `OnReveal` ability granted by its attached perks in that order
- **AND** each card's effect SHALL resolve completely before the next card's effect
- **AND** the resolution SHALL complete before priority, victory and the next turn are evaluated

#### Scenario: Cards that left play do not reveal

- **WHEN** a card is destroyed, discarded, returned to hand or otherwise leaves play before its reveal
- **THEN** its `OnReveal` effect and any `OnReveal` perk ability SHALL NOT be applied
- **AND** the reveal sequence SHALL continue with the next card

#### Scenario: Already-revealed cards do not reveal again

- **WHEN** a card has already revealed during the combat
- **THEN** it SHALL NOT be included in a later reveal sequence
- **AND** its `OnReveal` effects and perk abilities SHALL NOT trigger again

#### Scenario: StartOfTurn processing

- **WHEN** a turn starts
- **THEN** the processor SHALL resolve every active `StartOfTurn` effect before the play phase
- **AND** it SHALL evaluate energy conditions against the board as it stands at that moment

#### Scenario: EndOfTurn processing

- **WHEN** a turn ends
- **THEN** the processor SHALL resolve every active `EndOfTurn` effect after the reveal sequence
- **AND** it SHALL complete before priority and victory are evaluated

#### Scenario: Activate processing

- **WHEN** a player activates a card's ability
- **THEN** the processor SHALL apply the card's `Activate` effects once
- **AND** the action authority SHALL reject a second activation in the same combat
- **AND** the activation SHALL change the board only through the effect's declared actions

#### Scenario: EndOfGame processing

- **WHEN** the final turn ends
- **THEN** the processor SHALL reveal delayed cards and resolve their `OnReveal` effects
- **AND** it SHALL resolve every active `EndOfGame` effect in a deterministic side, zone and slot order
- **AND** it SHALL recompute powers after each effect
- **AND** it SHALL complete before victory is evaluated

#### Scenario: Ongoing processing

- **WHEN** card or zone power is recalculated
- **THEN** the system SHALL query the `Ongoing` effects of every card and attached perk in the zone
- **AND** it SHALL NOT write the result into the card's current power as an accumulated value

#### Scenario: No effect content

- **WHEN** a card has no effects and no attached perks
- **THEN** the processor SHALL leave its power and zone unchanged

### Requirement: Power Resolution

The system SHALL calculate card power dynamically from base power, perks, persistent bonuses, set/reset overrides and active Ongoing modifiers.

#### Scenario: Card power composition

- **WHEN** a card's power is calculated
- **THEN** it SHALL equal the card's effective base power plus perk power plus persistent bonus power plus Ongoing bonuses, then multiplied by active power multipliers
- **AND** an explicit set-power override SHALL replace the definition's base power
- **AND** it SHALL NOT read the card's previous current power as an input

#### Scenario: Set, reset and double

- **WHEN** Bast, Valkyrie, Shadow King, Black Panther, Shuri or Taskmaster resolves
- **THEN** the resolver SHALL apply the set, reset or double step exactly once
- **AND** a set or reset SHALL clear the card's persistent bonuses
- **AND** current Ongoing auras SHALL still apply after a set or reset
- **AND** a double SHALL snapshot the resolved power and add the difference as a persistent bonus

#### Scenario: Ongoing modifier application

- **WHEN** Ongoing effects or attached perks are active
- **THEN** the power resolver SHALL compute card power from those sources on every recalculation
- **AND** power SHALL be recalculated whenever board state changes
- **AND** modifiers from zones, cards and perks SHALL all be considered

#### Scenario: No accumulation across evaluations

- **WHEN** power is recalculated more than once for the same card
- **THEN** the result SHALL be identical
- **AND** modifiers SHALL NOT stack on top of a previously calculated value

#### Scenario: Zone multiplier

- **WHEN** a zone total is calculated
- **THEN** it SHALL be the sum of the card powers in that zone multiplied by the zone's Ongoing multipliers
- **AND** the multiplier SHALL NOT change the individual card powers

### Requirement: Odin On Reveal Repetition

The system SHALL repeat the On Reveal abilities of other allied cards at Odin's location after the normal reveal sequence.

#### Scenario: Repeat allied reveals

- **WHEN** Odin is revealed and remains in play after the reveal sequence
- **THEN** each other allied card in his location that revealed during the same turn and is not ability-disabled SHALL resolve its On Reveal effects once more
- **AND** the repeated resolution SHALL use the repeated card as the source
- **AND** Odin SHALL NOT repeat himself
- **AND** Odin SHALL NOT repeat enemy cards

#### Scenario: Repeat does not revisit earlier turns

- **WHEN** Odin remains in play on a later turn
- **THEN** he SHALL NOT repeat cards that revealed on an earlier turn
- **AND** the repeat pass SHALL use the current turn's reveal set or a snapshot taken when the repeater resolved
- **AND** a repeated OnReveal SHALL NOT fire again on later turns

#### Scenario: Repeat respects suppression

- **WHEN** an active Cosmo is in Odin's location
- **THEN** Odin SHALL NOT repeat any On Reveal ability in that location
- **AND** when a repeated card's printed abilities are disabled
- **THEN** only its perk-granted On Reveal abilities SHALL resolve

#### Scenario: Repeat is bounded

- **WHEN** more than one Odin or another repeat hook is present
- **THEN** an Odin SHALL NOT repeat another repeat hook
- **AND** the repetition SHALL be deterministic and bounded by the event limit
- **AND** the reveal marker of the repeated card SHALL remain set

#### Scenario: Repeat interacts with delayed and created effects

- **WHEN** Odin repeats a card that creates a card, arms a delayed condition or adds Power
- **THEN** the repeated effect SHALL resolve again
- **AND** any created token or armed condition SHALL be independent of the first resolution
- **AND** the normal On Reveal resolution SHALL NOT be cancelled or reordered

## ADDED Requirements

### Requirement: Activate Processing

The system SHALL expose activated abilities as a first-class action.

#### Scenario: Activation legality

- **WHEN** a player activates an owned card in play
- **THEN** the action authority SHALL require that the card has an `Activate` effect, is not ability-disabled and has not activated this combat
- **AND** a rejected activation SHALL leave the board, hand and energy unchanged
- **AND** the activation SHALL be available only during the play phase

#### Scenario: Activation once per combat

- **WHEN** a card activates
- **THEN** the instance SHALL record the activation
- **AND** it SHALL NOT activate again in the same combat
- **AND** returning the card to hand SHALL NOT clear the record
- **AND** a new combat SHALL clear it

#### Scenario: Activation effects

- **WHEN** Black Widow, Hellcow, Lockjaw, Mysterio or Spider-Ham activates
- **THEN** the effect SHALL resolve through the normal action authority
- **AND** it SHALL NOT consume energy
- **AND** it SHALL NOT count as a play for play records, `AfterPlayed` or taunt

### Requirement: Copied and Stolen Ability Text

The system SHALL support per-instance copied and suppressed effect ids.

#### Scenario: Copy last played

- **WHEN** Absorbing Man or Mystique resolves
- **THEN** it SHALL copy the matching printed effect ids of the last card its owner played
- **AND** Absorbing Man SHALL copy only `OnReveal` effects
- **AND** Mystique SHALL copy only `Ongoing` effects
- **AND** a card with no matching effect SHALL produce no copy

#### Scenario: Steal an Ongoing

- **WHEN** Rogue resolves
- **THEN** she SHALL copy the printed `Ongoing` effects of an enemy card in her zone
- **AND** the enemy card SHALL lose those effects through a suppressed-effect list
- **AND** the enemy card's other printed or perk effects SHALL remain

#### Scenario: Super-Skrull

- **WHEN** Super-Skrull is in play
- **THEN** it SHALL copy the printed `Ongoing` effects of all enemy cards in play, one level deep
- **AND** it SHALL skip perks, temporary effects and effects that are themselves copied
- **AND** it SHALL evaluate the copied effects with its owner as the source side
- **AND** two Super-Skrulls SHALL NOT recursively copy each other

#### Scenario: Copy presentation

- **WHEN** a card has copied or replaced text
- **THEN** its detail panel SHALL show the copied source or the replacement text
- **AND** the original card text SHALL remain available when the copy is additive
- **AND** Spider-Ham's target SHALL show `oink!` and lose its printed abilities

### Requirement: Delayed Reveal

The system SHALL support cards that stay unrevealed until the game ends.

#### Scenario: Invisible Woman

- **WHEN** a card is played in Invisible Woman's zone
- **THEN** it SHALL gain the delayed-reveal state
- **AND** it SHALL NOT reveal at end of turn
- **AND** its `OnReveal` effects SHALL NOT fire
- **AND** it SHALL still count its current power for the zone total

#### Scenario: End-of-game reveal

- **WHEN** the final turn ends and a delayed card is still in play
- **THEN** it SHALL reveal in play order
- **AND** its `OnReveal` effects SHALL resolve before `EndOfGame` effects
- **AND** the reveal marker SHALL be set

#### Scenario: Invisible Woman leaves play

- **WHEN** Invisible Woman leaves play
- **THEN** the delayed cards SHALL reveal at the next end of turn
- **AND** their `OnReveal` effects SHALL resolve normally

### Requirement: Destroyed-Card History

The system SHALL keep a combat-scoped history of destroyed cards for cost and power effects.

#### Scenario: Record a destroy

- **WHEN** a card is destroyed
- **THEN** the board SHALL record the card, owner, power at destruction, cost at destruction and turn
- **AND** the record SHALL remain even if the card is later resurrected
- **AND** the record SHALL reset when a new combat starts

#### Scenario: Death

- **WHEN** Death's cost is resolved
- **THEN** it SHALL be reduced by 1 for each destroyed-card record
- **AND** the cost SHALL NOT go below 0

#### Scenario: Knull

- **WHEN** Knull's power is resolved
- **THEN** it SHALL add the combined power at destruction of every destroyed-card record
- **AND** negative powers SHALL count
- **AND** the value SHALL be derived, not accumulated

### Requirement: New Action Surface

The system SHALL expose the actions required by the Series 3 cards through the effect actions contract.

#### Scenario: Draw

- **WHEN** an effect draws for a side
- **THEN** the action SHALL respect hand capacity and the Widow's Bite draw lock
- **AND** it SHALL dispatch the normal draw event
- **AND** it SHALL do nothing when the deck is empty

#### Scenario: Resurrect

- **WHEN** Ghost Rider or Hela resurrects a card
- **THEN** the card SHALL leave the discard pile and enter a zone with room
- **AND** it SHALL be marked revealed
- **AND** its `OnReveal` SHALL NOT resolve

#### Scenario: Copy to other zones

- **WHEN** Arnim Zola or Nimrod copies a card
- **THEN** independent copies SHALL be added to each other zone with room
- **AND** the copies SHALL be revealed
- **AND** their `OnReveal` SHALL NOT resolve

#### Scenario: Zone swap

- **WHEN** Quake resolves
- **THEN** the other two zone positions SHALL exchange definitions, cards, flood state and play constraints
- **AND** the zone indices and board references SHALL remain stable
- **AND** the views SHALL update from the board version

#### Scenario: Enemy hand

- **WHEN** Doctor Octopus, Black Bolt, Moon Knight, Silver Samurai, Master Mold or Black Widow resolves
- **THEN** the action SHALL use the enemy hand through the normal capacity, discard and hand-entry rules
- **AND** a pull SHALL mark the pulled card revealed and resolve its `OnReveal`
- **AND** a discard SHALL increment the enemy discard counter

#### Scenario: Max Energy

- **WHEN** Electro or Havok changes Max Energy
- **THEN** the board SHALL record a per-side Max Energy delta
- **AND** the next energy pool SHALL include it
- **AND** the delta SHALL reset when a new combat starts

#### Scenario: Created cards

- **WHEN** an effect adds a card to hand
- **THEN** the card SHALL be marked as created
- **AND** Quinjet's cost reduction SHALL apply to created cards
- **AND** a normal draw SHALL NOT mark the card as created

### Requirement: Play and Zone Restriction Effects

The system SHALL evaluate card and zone restrictions through one legality query shared by the UI, the AI and the action authority.

#### Scenario: Card restrictions

- **WHEN** Crossbones, Giganto or Sentry is evaluated
- **THEN** Crossbones SHALL require the owner to be winning the destination
- **AND** Giganto SHALL allow only the left zone
- **AND** Sentry SHALL reject the right zone

#### Scenario: Zone restrictions

- **WHEN** Goose or Jean Grey is active
- **THEN** Goose SHALL reject cards with effective cost 4 or more in that zone
- **AND** Jean Grey SHALL require the first card of each turn to be played there when the card can legally go there
- **AND** a full or illegal Jean Grey zone SHALL NOT block other zones

#### Scenario: Side restrictions

- **WHEN** Electro is in play
- **THEN** its owner SHALL play at most one card per turn
- **AND** the limit SHALL reset at the start of the next turn

#### Scenario: Global cost cap

- **WHEN** Wave resolves
- **THEN** all cards SHALL cost at most 4 until the end of the next turn
- **AND** the cap SHALL apply after reductions and before the cost floor
- **AND** it SHALL affect both sides, hand, deck and created cards

### Requirement: Series 3 Triggered Effects

The system SHALL resolve the new Series 3 triggered families through the shared processor.

#### Scenario: Reaction effects

- **WHEN** Echo, Negasonic Teenage Warhead, Silk, Titania, Hercules, Nebula, Kingpin, Dagger, Human Torch or Spider-Man 2099 reacts
- **THEN** it SHALL use the matching `AfterPlayed`, `AfterMoved` or `EndOfTurn` event
- **AND** it SHALL filter by event source, side and zone
- **AND** a self-play SHALL NOT trigger Silk or Titania
- **AND** Hercules and movement chains SHALL be bounded by the event limit

#### Scenario: Cost and deck effects

- **WHEN** Death, She-Hulk, Stature, Miles Morales, Sera, Quinjet, Zabu, Mobius M. Mobius, Wave, Baron Mordo, Beast, Mister Negative or Zabu resolves
- **THEN** the cost resolver SHALL apply the effect in the documented precedence order
- **AND** Mobius SHALL prevent increases for its owner and reductions for its opponent
- **AND** Mister Negative SHALL swap cost and power in the deck without mutating shared definitions

#### Scenario: End-of-game effects

- **WHEN** Captain Marvel, Dracula, M'Baku or Martyr resolves
- **THEN** it SHALL use the end-of-game phase
- **AND** it SHALL respect zone capacity and full locks
- **AND** it SHALL NOT start a new turn or re-open the play phase
