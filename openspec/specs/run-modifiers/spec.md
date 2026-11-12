# run-modifiers Specification

## Purpose
Defines the six run modifiers, their stacking rules and how they change combat energy, the opening hand, shop prices and combat rewards.

## Requirements

### Requirement: Modifier Types

The system SHALL support six run modifiers.

#### Scenario: Modifier catalog

- **WHEN** modifiers are initialized
- **THEN** the catalog SHALL contain `energy_bonus`, `energy_cap`, `starting_hand`, `shop_discount`, `money_bonus` and `xp_bonus`
- **AND** `energy_bonus` SHALL add 1 to the per-turn energy pool
- **AND** `energy_cap` SHALL raise the energy cap from 6 to 7
- **AND** `starting_hand` SHALL add 1 to the opening hand size
- **AND** `shop_discount` SHALL reduce shop prices by 20%
- **AND** `money_bonus` SHALL add 3 money to every won combat
- **AND** `xp_bonus` SHALL add 10 XP to every won combat

#### Scenario: Stacking

- **WHEN** a modifier is granted
- **THEN** the run SHALL hold at most one copy of each modifier
- **AND** a second copy SHALL be refused
- **AND** the refusal SHALL not consume the event or purchase that granted it

### Requirement: Modifier Application

The system SHALL apply active modifiers to combat and rewards.

#### Scenario: Energy

- **WHEN** a turn starts with `energy_bonus` active
- **THEN** the pool SHALL be the normal pool plus 1
- **AND** with `energy_cap` active the cap SHALL be 7
- **AND** both modifiers SHALL apply together

#### Scenario: Opening hand

- **WHEN** a combat starts with `starting_hand` active
- **THEN** the opening hand SHALL contain one additional card
- **AND** the hand cap SHALL still apply

#### Scenario: Shop and rewards

- **WHEN** `shop_discount` is active
- **THEN** every shop price SHALL be reduced by 20%, rounded down
- **AND** when `money_bonus` is active every won combat SHALL grant 3 additional money
- **AND** when `xp_bonus` is active every won combat SHALL grant 10 additional XP

### Requirement: Modifier Visibility and Persistence

The system SHALL show and persist the active modifiers.

#### Scenario: Visibility

- **WHEN** the player opens the act transition or the pause overlay
- **THEN** every active modifier SHALL be listed by name
- **AND** the list SHALL fit at the smallest supported viewport

#### Scenario: Persistence

- **WHEN** the run is checkpointed
- **THEN** the active modifier ids SHALL be stored
- **AND** they SHALL be restored on continue
- **AND** a new run SHALL start with no modifiers
