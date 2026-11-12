# Spec Delta

## Purpose

Defines the eight rule perks, how they attach to cards, how copies stack and which power, cost and economy hooks apply them.

## ADDED Requirements

### Requirement: Perk Catalog

The system SHALL provide eight rule perks.

#### Scenario: Perk list

- **WHEN** the perk catalog is loaded
- **THEN** it SHALL contain exactly 8 perks
- **AND** 4 SHALL be owned by default: Vanguardia, Aura, Francotirador and Letal
- **AND** 4 SHALL be store perks: Agil, Guardian, Imparable and Codicioso
- **AND** each perk SHALL have a name, rule text and effect ids
- **AND** no perk SHALL be a flat `+1/-1/+2 power` modifier

#### Scenario: Rule text

- **WHEN** a perk is shown
- **THEN** its rule text SHALL describe its trigger and amount
- **AND** a store perk SHALL show how to obtain it
- **AND** the text SHALL be written in Spanish without accents

### Requirement: Perk Attachment

The system SHALL attach perks to cards.

#### Scenario: Attaching

- **WHEN** a perk is granted by level-up or the store
- **THEN** it SHALL be attached to a card in the run deck
- **AND** the same perk SHALL NOT be attached more than 2 times across the deck
- **AND** removing the card SHALL remove its attached perks

#### Scenario: Active while in the deck

- **WHEN** a perk is attached to a card
- **THEN** its rule SHALL be active while that card is in the run deck
- **AND** it SHALL NOT require the card to be in play, in hand or revealed
- **AND** two copies of the same perk SHALL apply twice

### Requirement: Rule Effects

The system SHALL apply the eight rules.

#### Scenario: Power rules

- **WHEN** a card's power is calculated
- **THEN** Vanguardia SHALL give +2 power to the first card played each combat
- **AND** Aura SHALL give +1 power to each of your Ongoing cards
- **AND** Francotirador SHALL give +1 power to each of your On Reveal cards
- **AND** Letal SHALL give +2 power to each of your cards with effective cost 1 or less
- **AND** Guardian SHALL give +1 power to your cards in the zone where you have the fewest cards
- **AND** Imparable SHALL give +1 power to your cards from turn 5 onward

#### Scenario: Cost rule

- **WHEN** Agil is active
- **THEN** the first card you play each turn SHALL cost 1 less
- **AND** the cost SHALL not go below 0
- **AND** the discount SHALL NOT carry to a second play in the same turn

#### Scenario: Economy rule

- **WHEN** Codicioso is active and a combat is won
- **THEN** the run SHALL gain 3 money per attached copy
- **AND** the bonus SHALL be applied after the normal reward
- **AND** it SHALL NOT apply on a defeat or a tie
