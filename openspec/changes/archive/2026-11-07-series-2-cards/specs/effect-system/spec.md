# Spec Delta

## MODIFIED Requirements

### Requirement: Effect Trigger Types

The system SHALL implement `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand`, `GameStart`, `StartOfTurn` and `EndOfTurn` as active trigger families. `Activate` SHALL remain a declared trigger type that does not fire because no content uses it. Card and perk abilities SHALL use the same trigger families.

#### Scenario: Different trigger types

- **WHEN** effects are defined
- **THEN** each effect SHALL be associated with one of the supported trigger types
- **AND** `OnReveal` effects SHALL fire when their card is revealed
- **AND** `Ongoing` effects SHALL stay active while their card is in play
- **AND** `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand` and `GameStart` effects SHALL fire only when the matching action or combat event occurs
- **AND** `StartOfTurn` effects SHALL fire at the start of a turn
- **AND** `EndOfTurn` effects SHALL fire when a turn ends
- **AND** `Activate` effects SHALL NOT fire in this iteration

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

#### Scenario: Inactive triggers do not fire

- **WHEN** a turn ends or a player activates a card
- **THEN** `EndOfTurn` effects SHALL resolve and no `Activate` effect SHALL be applied
- **AND** the activation SHALL NOT change the board

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
- **THEN every `AfterAddedToHand` effect of the owner SHALL fire
- **AND** a normal draw from the owner's own deck SHALL NOT fire it
- **AND** a manual board-to-hand return by the player SHALL NOT fire it

## ADDED Requirements

### Requirement: Series 2 On Reveal Effects

The system SHALL resolve the On Reveal abilities of the Series 2 and Collection Level cards according to their printed text.

#### Scenario: Spectrum buffs Ongoing cards

- **WHEN** Spectrum is revealed
- **THEN** each allied card in play with at least one active Ongoing effect SHALL gain +2 persistent Power
- **AND** cards without an Ongoing effect SHALL NOT gain Power
- **AND** Spectrum SHALL NOT buff itself

#### Scenario: Wolfsbane scales with allies in the zone

- **WHEN** Wolfsbane is revealed
- **THEN** it SHALL gain +2 persistent Power for each other allied card in its location
- **AND** the count SHALL be taken when the effect resolves
- **AND** it SHALL NOT destroy or move those cards

#### Scenario: White Tiger adds a Tiger

- **WHEN** White Tiger is revealed
- **THEN** a Tiger token with 10 base Power SHALL be added to a random other location with room
- **AND** no Tiger SHALL be added if every other location is full
- **AND** the token SHALL be a separate, non-draftable instance

#### Scenario: Agent 13 adds a random card

- **WHEN** Agent 13 is revealed
- **THEN** a random draftable card SHALL be created as a new instance and added to its owner's hand if the hand has room
- **AND** the card SHALL be chosen uniformly at random from the non-token catalog
- **AND** nothing SHALL be created when the hand is full

#### Scenario: Jubilee pulls the top deck card

- **WHEN** Jubilee is revealed
- **THEN** the top card of its owner's deck SHALL be moved to Jubilee's location if that location has room
- **AND** the card SHALL be revealed and its On Reveal ability SHALL resolve
- **AND** the card SHALL NOT be marked as played for play-triggered effects
- **AND** nothing SHALL move when the deck is empty or the location is full

#### Scenario: Iceman raises an opponent's cost

- **WHEN** Iceman is revealed
- **THEN** one random card in the opponent's hand SHALL cost 1 more for the rest of the combat
- **AND** its effective cost SHALL NOT exceed 6
- **AND** cards already at effective cost 6 SHALL NOT be raised

#### Scenario: Scorpion afflicts the opponent's hand

- **WHEN** Scorpion is revealed
- **THEN** every card in the opponent's hand SHALL gain -1 persistent Power
- **AND** the affliction SHALL follow a card if it is later played
- **AND** cards that enter the opponent's hand later SHALL NOT be afflicted

#### Scenario: Nakia and Okoye buff hand and deck

- **WHEN** Nakia is revealed
- **THEN** every card in its owner's hand SHALL gain +1 persistent Power
- **AND** when Okoye is revealed
- **THEN** every card in its owner's deck SHALL gain +1 persistent Power
- **AND** the buffs SHALL apply to the cards present at reveal time

#### Scenario: Rhino adds a Rock

- **WHEN** Rhino is revealed
- **THEN** a Rock token SHALL be added to its owner's side of Rhino's location if that side has room
- **AND** the Rock SHALL be a separate, non-draftable instance

#### Scenario: Leech removes printed text

- **WHEN** Leech is revealed
- **THEN** every opponent hand card with effective cost 6 or more SHALL have its printed abilities disabled for the rest of the combat
- **AND** abilities granted by attached perks SHALL remain active
- **AND** cards that enter the opponent's hand later SHALL NOT be affected

#### Scenario: Killmonger destroys 1-Cost cards

- **WHEN** Killmonger is revealed
- **THEN** every card in play with effective cost 1 SHALL be destroyed, on both sides
- **AND** cards protected from destruction SHALL survive
- **AND** each successful destroy SHALL dispatch its own destruction event
- **AND** the set of targets SHALL be taken when the effect resolves

#### Scenario: Shang-Chi destroys a high-Power enemy

- **WHEN** Shang-Chi is revealed
- **THEN** one random enemy card in his location with 10 or more current Power SHALL be destroyed
- **AND** protected cards SHALL survive
- **AND** nothing SHALL be destroyed when no enemy card meets the threshold

#### Scenario: Jessica Jones watches the next turn

- **WHEN** Jessica Jones is revealed on turn N
- **AND** no allied card is played in that location during turn N + 1
- **THEN** she SHALL gain +5 persistent Power at the end of turn N + 1
- **AND** she SHALL NOT gain it if an allied card was played there
- **AND** the condition SHALL be cleared after it resolves or fails

### Requirement: Series 2 Continuous Effects

The system SHALL resolve the Ongoing abilities of the Series 2 and Collection Level cards as derived contributions, not accumulated writes.

#### Scenario: Ka-Zar buffs 1-Cost cards

- **WHEN** Ka-Zar is in play
- **THEN** each allied card in play with effective cost 1 SHALL have +1 Power
- **AND** the bonus SHALL apply in every power recalculation
- **AND** it SHALL NOT buff enemy 1-Cost cards
- **AND** it SHALL NOT buff cards in hand or deck

#### Scenario: Mister Fantastic buffs adjacent locations

- **WHEN** Mister Fantastic is in play
- **THEN** its owner's side of each adjacent location SHALL gain +3 Power
- **AND** the addition SHALL affect the zone total, not individual card Power
- **AND** a Mister Fantastic in the leftmost or rightmost location SHALL affect only the one adjacent location

#### Scenario: Morbius counts discards

- **WHEN** Morbius is in play
- **THEN** it SHALL have +2 Power for each successful discard performed by its owner in the current combat
- **AND** discards that are later replaced SHALL count
- **AND** the count SHALL reset when a new combat starts
- **AND** the bonus SHALL NOT accumulate into the stored Power

#### Scenario: Warpath checks empty locations

- **WHEN** Warpath is in play
- **THEN** it SHALL have +5 Power if at least one of its owner's locations contains no allied card
- **AND** the condition SHALL be evaluated on every recalculation
- **AND** it SHALL NOT count a location that contains an allied card, including Warpath itself

### Requirement: Odin On Reveal Repetition

The system SHALL repeat the On Reveal abilities of other allied cards at Odin's location after the normal reveal sequence.

#### Scenario: Repeat allied reveals

- **WHEN** Odin is revealed and remains in play after the reveal sequence
- **THEN** each other allied card in his location that has already revealed and is not ability-disabled SHALL resolve its On Reveal effects once more
- **AND** the repeated resolution SHALL use the repeated card as the source
- **AND** Odin SHALL NOT repeat himself
- **AND** Odin SHALL NOT repeat enemy cards

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

### Requirement: Hand Entry Effects

The system SHALL resolve effects that react to a card entering its owner's hand.

#### Scenario: The Collector gains Power

- **WHEN** a card enters The Collector's owner's hand from anywhere except that owner's deck
- **THEN** The Collector SHALL gain +1 persistent Power
- **AND** the trigger SHALL apply to cards created by effects, copied to hand, returned by a replacement or taken from the opponent's deck
- **AND** a normal draw from the owner's deck SHALL NOT trigger it
- **AND** a manual board-to-hand return by the player SHALL NOT trigger it

#### Scenario: Multiple entries

- **WHEN** one effect adds several cards to the same hand
- **THEN** The Collector SHALL gain +1 for each card that actually enters the hand
- **AND** additions rejected by the hand cap SHALL NOT trigger it

#### Scenario: No self-trigger

- **WHEN** The Collector is played from hand
- **THEN** its own play SHALL NOT trigger its ability
- **AND** the trigger SHALL only fire for cards entering the hand after it is in play

### Requirement: Series 2 Movement and Play Restrictions

The system SHALL support printed movement permissions, temporary movement windows and play restrictions.

#### Scenario: Nightcrawler moves once

- **WHEN** Nightcrawler is in play and its controller moves it on a later turn
- **THEN** the move SHALL be allowed once per combat
- **AND** a later move of the same instance SHALL be rejected
- **AND** the destination SHALL have room

#### Scenario: Vision moves each turn

- **WHEN** Vision is in play and its controller moves it
- **THEN** the move SHALL be allowed once per turn
- **AND** a second move of the same instance in the same turn SHALL be rejected
- **AND** the permission SHALL reset at the start of the next turn

#### Scenario: Cloak opens a movement window

- **WHEN** Cloak is revealed on turn N
- **THEN** during turn N + 1 any card in play may be moved to Cloak's location if the destination has room
- **AND** the window SHALL apply to both sides
- **AND** the window SHALL expire at the end of turn N + 1
- **AND** the move SHALL be subject to the normal movement and lock rules

#### Scenario: Vulture reacts to its own move

- **WHEN** Vulture moves to another location
- **THEN** it SHALL gain +4 persistent Power
- **AND** the bonus SHALL apply once per completed move
- **AND** a failed or cancelled move SHALL NOT grant it

#### Scenario: Ebony Maw is play-restricted

- **WHEN** Ebony Maw is in hand
- **THEN** it SHALL NOT be playable after turn 3
- **AND** while it is in play, no card SHALL be played into its location by either side
- **AND** effects MAY still add cards to that location
- **AND** the play restriction SHALL be removed if its printed abilities are disabled

#### Scenario: The Infinaut is play-restricted

- **WHEN** The Infinaut is in hand
- **AND** its owner played a card during the previous turn
- **THEN** The Infinaut SHALL NOT be playable
- **AND** it SHALL be playable when its owner played no card during the previous turn
- **AND** the restriction SHALL NOT depend on the opponent's plays

#### Scenario: Storm floods a location

- **WHEN** Storm is revealed on turn N
- **THEN** her location SHALL be flooded
- **AND** turn N + 1 SHALL be the last turn in which cards may be played there
- **AND** from turn N + 2 onward no card SHALL be played there
- **AND** effects MAY still add cards to the flooded location

### Requirement: Series 2 Destruction, Discard and Side Change

The system SHALL resolve the replacement, discard and side-change abilities of the Series 2 cards.

#### Scenario: Bucky Barnes becomes the Winter Soldier

- **WHEN** Bucky Barnes is destroyed
- **THEN** it SHALL be replaced in its location by a Winter Soldier token with 8 base Power
- **AND** Bucky Barnes SHALL NOT enter the destroyed pile for that combat
- **AND** the destruction event SHALL still fire for reactions
- **AND** the Winter Soldier SHALL be a separate, non-draftable instance

#### Scenario: Sabretooth returns at zero cost

- **WHEN** Sabretooth is destroyed and its owner's hand has room
- **THEN** it SHALL return to its owner's hand instead of entering the destroyed pile
- **AND** its effective cost SHALL be 0 for the rest of the combat
- **AND** the destruction event SHALL still fire for reactions
- **AND** when the hand is full, it SHALL be destroyed normally

#### Scenario: Swarm creates zero-cost copies

- **WHEN** Swarm is discarded
- **THEN** it SHALL enter the discarded pile
- **AND** two copies of Swarm with effective cost 0 SHALL be added to its owner's hand while the hand has room
- **AND** each copy SHALL be an independent instance with the same abilities
- **AND** copies rejected by the hand cap SHALL NOT be created

#### Scenario: Hobgoblin switches sides

- **WHEN** Hobgoblin is revealed
- **THEN** it SHALL move to the opposing side of its location if that side has room
- **AND** it SHALL NOT enter a pile
- **AND** it SHALL NOT trigger a normal movement reaction
- **AND** it SHALL remain on its original side when the opposing side has no room or the location blocks removal or addition

### Requirement: Series 2 Cost and Deck Effects

The system SHALL support the cost increases, cost cap and deck-to-location effects introduced by the Series 2 content.

#### Scenario: Cost cap

- **WHEN** any effect increases a card's effective cost
- **THEN** the effective cost SHALL NOT exceed 6
- **AND** the cap SHALL apply after permanent upgrades, combat modifiers and turn modifiers
- **AND** the displayed cost SHALL match the capped value

#### Scenario: Sandman raises next-turn costs

- **WHEN** Sandman is revealed on turn N
- **THEN** every card in both hands SHALL cost 1 more during turn N + 1
- **AND** the increase SHALL apply to cards drawn or added during that turn where the cost is evaluated
- **AND** multiple Sandmen SHALL stack
- **AND** the increase SHALL expire at the end of turn N + 1
- **AND** the total effective cost SHALL NOT exceed 6

#### Scenario: Zero-cost copies and returns

- **WHEN** an effect sets a card's effective cost to 0 for the combat
- **THEN** later turn cost modifiers SHALL NOT raise it above 0
- **AND** the override SHALL clear when a new combat starts
- **AND** returning the card to hand SHALL preserve the override

#### Scenario: Jubilee moves a deck card to a location

- **WHEN** an effect moves the top card of a deck to a location
- **THEN** the card SHALL leave the deck only when the destination has room
- **AND** the card SHALL become a board instance and its Ongoing abilities SHALL be active
- **AND** the move SHALL NOT count as a play for play-triggered effects
- **AND** a failed addition SHALL leave the card on top of the deck
