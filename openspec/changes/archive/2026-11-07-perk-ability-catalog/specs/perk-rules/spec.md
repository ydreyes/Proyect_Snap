# Spec Delta

## MODIFIED Requirements

### Requirement: Perk Catalog

The system SHALL provide a catalog of exactly 28 ability perks and no global rule perks.

#### Scenario: Perk list

- **WHEN** the perk catalog is loaded
- **THEN** it SHALL contain exactly 28 ability perks, one for each entry in the catalog appendix
- **AND** all 28 SHALL be owned by default
- **AND** no perk SHALL be store-only
- **AND** the retired rule perk ids SHALL NOT be registered
- **AND** each perk SHALL have an id, a name, a rule text, one or more effect ids and compatibility metadata
- **AND** a perk MAY share an effect implementation with a current or future card without sharing its perk id

#### Scenario: Rule text

- **WHEN** a perk is shown
- **THEN** its rule text SHALL describe its trigger, amount and target
- **AND** the text SHALL be written in Spanish without accents
- **AND** a perk with a host requirement SHALL show that requirement

#### Scenario: All perks available

- **WHEN** the player reaches a level-up perk choice
- **THEN** the offer pool SHALL include every perk that has at least one compatible host in the deck
- **AND** the player SHALL NOT need to buy or unlock a perk first
- **AND** the global store SHALL NOT sell perks

### Requirement: Perk Attachment

The system SHALL attach ability perks to compatible cards in the run deck.

#### Scenario: Attaching

- **WHEN** a perk is granted by a level-up, an event or any run source
- **THEN** it SHALL be attached to a compatible card in the run deck
- **AND** the same perk SHALL NOT be attached more than 2 times across the deck
- **AND** removing the host card SHALL remove its attached perks

#### Scenario: Active while in the deck

- **WHEN** a perk is attached to a card in the run deck
- **THEN** its ability SHALL be available whenever the host is in hand, deck or play as its text allows
- **AND** it SHALL NOT apply as a global side rule
- **AND** two copies of the same perk SHALL each resolve once

#### Scenario: Ability follows the host

- **WHEN** a perk is attached to a card
- **THEN** its ability SHALL use the host card's owner, zone and location
- **AND** it SHALL be active only while the host satisfies the ability's location requirement
- **AND** it SHALL NOT apply as a global rule to the whole side
- **AND** two copies of the same perk SHALL be two independent grants that each resolve once

#### Scenario: Disabled and suppressed

- **WHEN** the host's abilities are disabled
- **THEN** the perk abilities SHALL NOT apply
- **AND** when an On Reveal granted by a perk is resolved in a zone with an active suppression effect
- **THEN** that On Reveal SHALL NOT apply

## REMOVED Requirements

### Requirement: Rule Effects

**Reason**: The eight global rule perks are replaced by the 28 host-attached ability perks.

**Migration**: Old perk ids are removed from profiles and saved decks. The new ability behaviors are defined by the `Ability Perk Behaviors` requirement.

## ADDED Requirements

### Requirement: Perk Compatibility

The system SHALL filter perk offers and host targets by compatibility.

#### Scenario: Offer filter

- **WHEN** level-up offers perks
- **THEN** a perk SHALL be included only when at least one card in the run deck is a compatible host
- **AND** an incompatible perk SHALL NOT be offered
- **AND** when no compatible perk exists, the perk branch SHALL be unavailable

#### Scenario: Host filter

- **WHEN** the player must choose the host for a perk
- **THEN** only compatible cards SHALL be selectable
- **AND** incompatible cards SHALL be shown disabled with the reason they cannot host the perk

#### Scenario: Dynamic eligibility

- **WHEN** a new card with a required trait enters the deck
- **THEN** the affected perks SHALL become eligible for later level-up offers
- **AND** an already generated offer SHALL NOT be re-rolled

### Requirement: Ability Perk Behaviors

The system SHALL implement the behavior of every catalog perk.

#### Scenario: Afterimage

- **WHEN** the host moves
- **THEN** an unmodified copy of the host SHALL be added to the zone it left if that zone has room
- **AND** the copy SHALL have its own instance identity and base values
- **AND** it SHALL NOT copy upgrades, persistent bonuses or attached perks

#### Scenario: Amplify

- **WHEN** one of the host's Ongoing effects is resolved
- **THEN** that Ongoing effect SHALL apply one additional time per attached Amplify copy
- **AND** Amplify SHALL NOT repeat itself
- **AND** the repetition SHALL be deterministic and bounded by the copy limit

#### Scenario: Apex

- **WHEN** the host is the only allied card in its zone
- **THEN** its final power SHALL be doubled
- **AND** the doubling SHALL be calculated from the pre-doubling power and SHALL NOT accumulate
- **AND** when another allied card enters the zone, the doubling SHALL stop

#### Scenario: Clone

- **WHEN** a combat starts and the host is in the run deck or hand
- **THEN** an unmodified copy of the host SHALL be shuffled into the owner's deck
- **AND** the copy SHALL be a separate instance
- **AND** the copy SHALL NOT copy upgrades, persistent bonuses or attached perks

#### Scenario: Deadlink

- **WHEN** the host is revealed
- **THEN** each Zombie Horde the owner controls SHALL gain +4 persistent Power
- **AND** when the owner controls no Zombie Horde, nothing SHALL happen

#### Scenario: Encore

- **WHEN** the host's On Reveal effects resolve
- **THEN** they SHALL resolve one additional time per attached Encore copy
- **AND** the host's reveal marker SHALL NOT be cleared
- **AND** the repetition SHALL be deterministic and bounded by the copy limit and the event limit

#### Scenario: Hot Pursuit

- **WHEN** at least one card moved during the previous turn
- **THEN** the host's effective cost SHALL be reduced by 3 for the current turn
- **AND** the cost SHALL NOT go below 0
- **AND** the reduction SHALL NOT stack with itself

#### Scenario: Maneuverable

- **WHEN** the host is on the board and has not used its move
- **THEN** the player MAY move it once per combat to another zone with room
- **AND** the permission SHALL be consumed by that player move
- **AND** effect-driven movement SHALL NOT consume the permission
- **AND** the permission SHALL reset at the start of a new combat

#### Scenario: Mentor

- **WHEN** the host is revealed
- **THEN** the top card of the owner's deck SHALL gain persistent Power equal to the host's current Power
- **AND** when the deck is empty, nothing SHALL happen

#### Scenario: Overcharge

- **WHEN** the host is revealed
- **THEN** at the start of the next turn, if the owner is winning the host's zone, the owner SHALL gain +1 Energy for that turn
- **AND** the condition SHALL be evaluated once and then cleared
- **AND** when the owner is not winning, no Energy SHALL be granted

#### Scenario: Overload

- **WHEN** a turn starts and the host is in play
- **THEN** if the owner is winning the host's zone, the owner SHALL gain +1 Energy for that turn
- **AND** the bonus SHALL NOT carry to a later turn

#### Scenario: Overtime

- **WHEN** the host's End of Turn effects resolve
- **THEN** they SHALL resolve one additional time per attached Overtime copy
- **AND** the repetition SHALL be deterministic and bounded by the copy limit

#### Scenario: Quickdraw Discount

- **WHEN** the host is drawn
- **THEN** its effective cost SHALL be reduced by 2 for the rest of the combat
- **AND** the reduction SHALL NOT go below 0
- **AND** a second draw of the same instance SHALL NOT apply it again

#### Scenario: Quickdraw Power

- **WHEN** the host is drawn
- **THEN** it SHALL gain `On Reveal: +3 Power` for the rest of the combat
- **AND** the granted ability SHALL resolve in addition to the host's other On Reveal effects
- **AND** it SHALL resolve once per combat

#### Scenario: Rally

- **WHEN** the host's power is calculated
- **THEN** it SHALL gain +1 Power for each other allied card in its zone
- **AND** the count SHALL include cards that entered after the host
- **AND** the bonus SHALL update when the board changes

#### Scenario: Reality Warp

- **WHEN** the host is revealed
- **THEN** the host's zone SHALL be replaced with a new location using the existing location replacement rule
- **AND** the zone index and current power totals SHALL be preserved
- **AND** the new location's effect SHALL apply immediately

#### Scenario: Regeneration

- **WHEN** the host is discarded or destroyed
- **THEN** it SHALL NOT enter the discarded or destroyed pile
- **AND** it SHALL be added to a random zone with room with +2 persistent Power
- **AND** when no zone has room, it SHALL remain in the pile

#### Scenario: Regroup

- **WHEN** the host is revealed
- **THEN** the lowest-Cost allied card in the host's zone, choosing randomly among ties, SHALL return to the owner's hand
- **AND** that card SHALL gain +3 persistent Power
- **AND** when the hand is full, nothing SHALL move

#### Scenario: Reinforce

- **WHEN** the host is Empowered while in hand, in deck or in play
- **THEN** it SHALL gain +1 persistent Power per attached Reinforce copy
- **AND** the +1 from Reinforce SHALL NOT trigger another Empowered event
- **AND** an Empowered event SHALL be a one-time Power increase caused by another effect

#### Scenario: Salvage

- **WHEN** the host is revealed
- **THEN** the lowest-Power other allied card in the host's zone, choosing randomly among ties, SHALL be destroyed
- **AND** the host SHALL gain +5 persistent Power
- **AND** when there is no other allied card, nothing SHALL happen

#### Scenario: Sidekick

- **WHEN** the host is revealed
- **THEN** a random draftable non-token character SHALL be added to the owner's hand
- **AND** its effective cost SHALL be reduced by 2 for the rest of the combat
- **AND** when the hand is full, nothing SHALL be added

#### Scenario: Silence

- **WHEN** the host is revealed
- **THEN** the host's printed card effects SHALL be disabled for the rest of the combat
- **AND** the attached perk abilities SHALL remain active
- **AND** the disabled state SHALL clear at the start of the next combat

#### Scenario: Slingshot

- **WHEN** the host is revealed
- **THEN** one other allied card in the host's zone, choosing randomly among eligible cards, SHALL gain +1 persistent Power
- **AND** it SHALL move one zone to the right when that zone has room
- **AND** when the destination has no room, the Power SHALL still apply

#### Scenario: Superstar

- **WHEN** a combat starts
- **THEN** the host SHALL be dealt in the opening hand before the random cards
- **AND** when more forced opening cards exist than hand slots, the excess SHALL stay in the deck

#### Scenario: Tag In

- **WHEN** another allied card is destroyed
- **THEN** if the host is in hand or deck and the destroyed card's zone has room, the host SHALL be played into that zone for 0 cost
- **AND** the host SHALL leave its previous location
- **AND** the destroyed card SHALL remain in the destroyed pile

#### Scenario: Taunt

- **WHEN** the host is revealed
- **THEN** the opposing side's first card played next turn SHALL be placed in the host's zone if that zone has room
- **AND** the reservation SHALL apply once
- **AND** when the zone is full, the reservation SHALL expire

#### Scenario: Unstoppable

- **WHEN** the host is in hand or in play
- **THEN** it SHALL ignore zone play restrictions and zone locks
- **AND** it SHALL NOT be destroyed
- **AND** negative Power modifiers SHALL NOT reduce its Power
- **AND** it SHALL still be movable and returnable to hand by effects

#### Scenario: Wakandan Tech

- **WHEN** the host is revealed
- **THEN** its Power SHALL double for the rest of the combat
- **AND** the bonus SHALL be a persistent bonus equal to the Power at reveal
- **AND** later calculations SHALL NOT double the bonus again

### Requirement: Perk Ability Resolution

The system SHALL resolve perk abilities with the same authority and ordering rules as card abilities.

#### Scenario: Effect dispatch

- **WHEN** a perk ability triggers
- **THEN** it SHALL be dispatched through the effect processor with the host as the source and the host's owner as the acting side
- **AND** it SHALL use the same action authority, capacity checks, protections and event ordering as a card effect

#### Scenario: Power, cost and energy

- **WHEN** a perk grants power, cost or energy
- **THEN** the contribution SHALL be included before scoring, affordability and energy checks
- **AND** it SHALL NOT accumulate across recalculations

#### Scenario: Ability location

- **WHEN** a perk ability must work from hand or deck according to its text
- **THEN** it SHALL be able to react there
- **AND** an ability that requires the board SHALL NOT resolve from hand or deck

#### Scenario: Loop bound

- **WHEN** perk repetitions or triggered abilities cascade
- **THEN** the event limit SHALL bound the cascade
- **AND** the resolution SHALL stop and report the limit instead of looping forever
