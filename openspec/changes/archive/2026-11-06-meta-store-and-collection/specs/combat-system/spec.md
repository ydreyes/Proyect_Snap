# Spec Delta

## ADDED Requirements

### Requirement: Perk Rule Hooks

The system SHALL apply the active perks at the combat hooks.

#### Scenario: Power hooks

- **WHEN** the power of a player card is calculated
- **THEN** the active power perks SHALL be applied
- **AND** the hooks SHALL include first play, Ongoing, On Reveal, low cost, fewest-card zone and turn 5 onward
- **AND** each attached copy SHALL apply once

#### Scenario: Cost hook

- **WHEN** the player's first card of a turn is played with Agil active
- **THEN** its effective cost SHALL be reduced by 1
- **AND** the energy charge SHALL use the reduced cost
- **AND** a second play in the same turn SHALL NOT receive the reduction

#### Scenario: Economy hook

- **WHEN** a combat is won with Codicioso active
- **THEN** the run SHALL gain 3 money per attached copy
- **AND** the bonus SHALL be included in `MoneyEarned`
- **AND** it SHALL NOT apply on a defeat or a tie

#### Scenario: Perk rules are player-only

- **WHEN** the AI plays a card
- **THEN** the player's perks SHALL NOT apply to the AI side
- **AND** the AI SHALL use only its enemy deck and difficulty behavior
