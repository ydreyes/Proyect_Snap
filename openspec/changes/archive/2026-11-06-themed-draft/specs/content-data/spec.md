# Spec Delta

## MODIFIED Requirements

### Requirement: Card Pool

The system SHALL include exactly 59 unique card definitions for drafting.

#### Scenario: Card pool initialization

- **WHEN** the game starts
- **THEN** exactly 59 unique cards SHALL be available
- **AND** 13 SHALL be the starter cards listed in the Starter Card Set requirement
- **AND** 46 SHALL be the cards listed in the Series 1 Card Set requirement
- **AND** Abomination, Misty Knight and Shocker SHALL remain in the catalog as non-starter cards
- **AND** a card present in both the starter set and the Series 1 list SHALL be counted once
- **AND** no two cards SHALL share a name or an ID
- **AND** new cards SHALL be easily addable to the pool

#### Scenario: Curve coverage

- **WHEN** the card pool is loaded
- **THEN** costs SHALL cover 1 through 6
- **AND** at least one card SHALL exist at each cost from 1 to 6
- **AND** no 0-cost card SHALL be required

#### Scenario: Every card has a description

- **WHEN** the card pool is loaded
- **THEN** every card definition SHALL have a non-empty descriptive text
- **AND** the text SHALL be written in Spanish without accents
- **AND** the text SHALL be readable in the card detail panel

#### Scenario: Old generic Sentinel is gone

- **WHEN** the card pool is loaded
- **THEN** the 6-cost 5-power Sentinel SHALL NOT be registered
- **AND** the only card named Sentinel SHALL be the 2-cost 3-power card

#### Scenario: Displaced starters remain

- **WHEN** the starter set changes
- **THEN** Abomination, Misty Knight and Shocker SHALL remain available as non-starter cards
- **AND** they SHALL be obtainable through the global store or a bundle
- **AND** they SHALL NOT be part of the starting owned set

### Requirement: Starter Card Set

The system SHALL register the following 13 starter cards with their exact costs and base powers:
- Iron Man: cost 5, power 0, Ongoing.
- Punisher: cost 3, power 3, Ongoing.
- Hulk: cost 6, power 14, no ability.
- The Thing: cost 4, power 7, no ability.
- Cyclops: cost 3, power 5, no ability.
- Elektra: cost 1, power 2, On Reveal.
- Carnage: cost 2, power 2, On Reveal.
- Deathlok: cost 3, power 6, On Reveal.
- Hawkeye: cost 1, power 1, On Reveal.
- Medusa: cost 2, power 2, On Reveal.
- Sentinel: cost 2, power 3, On Reveal.
- Star-Lord: cost 2, power 2, On Reveal.
- Quicksilver: cost 1, power 2, opening hand.

#### Scenario: Starter cards are registered

- **WHEN** the card pool is loaded
- **THEN** each of the 13 cards SHALL exist with the listed cost and base power
- **AND** each SHALL declare its draft theme
- **AND** each card SHALL have a non-empty description

#### Scenario: Ability cards show ability text

- **WHEN** an ability card's description is read
- **THEN** it SHALL contain the card's ability text
- **AND** On Reveal text SHALL start with `Al revelarse:`
- **AND** Ongoing text SHALL start with `Continuo:`
- **AND** Quicksilver's text SHALL state that it starts in the opening hand

#### Scenario: Vanilla cards show generic text

- **WHEN** a card with no ability is inspected
- **THEN** its description SHALL describe its role or stats
- **AND** it SHALL NOT claim an ability the card does not have

#### Scenario: Removed starters

- **WHEN** the starter set is loaded
- **THEN** Abomination, Misty Knight and Shocker SHALL NOT be starters
- **AND** they SHALL remain in the 59-card catalog
- **AND** the first-run owned set SHALL contain only the 13 new starters

## ADDED Requirements

### Requirement: Draft Themes

The system SHALL assign a draft theme to every card.

#### Scenario: Theme values

- **WHEN** the card content is loaded
- **THEN** every draftable card SHALL declare `Control`, `Destroy`, `OnReveal` or `Neutral`
- **AND** a token SHALL NOT require a draft theme
- **AND** the theme SHALL be shown in the collection and the draft

#### Scenario: Starter themes

- **WHEN** the 13 starters are read
- **THEN** the Control starters SHALL be Iron Man, Punisher, Hulk, The Thing and Cyclops
- **AND** the Destroy starters SHALL be Elektra, Carnage and Deathlok
- **AND** the OnReveal starters SHALL be Hawkeye, Medusa, Sentinel and Star-Lord
- **AND** the Neutral starter SHALL be Quicksilver

#### Scenario: Theme groups

- **WHEN** the themed draft is generated
- **THEN** each group SHALL contain 3 distinct owned cards of its theme
- **AND** the groups SHALL be disjoint
- **AND** a Neutral card SHALL only enter the random phase unless a group needs a filler
- **AND** a group with fewer than 3 owned cards SHALL be filled with owned Neutral cards and marked as filled

#### Scenario: Theme coverage

- **WHEN** all 59 cards are loaded
- **THEN** every card SHALL have a theme
- **AND** the Control, Destroy and OnReveal groups SHALL each have enough owned cards for the draft once the starter set is owned
- **AND** the theme assignment SHALL be data, not a scene branch
