# Spec Delta

## REMOVED Requirements

### Requirement: Perk Rule Hooks

**Reason**: The eight global rule perks are removed. Perks now grant card abilities and no longer apply as side-wide combat rules.

**Migration**: Replace the power, cost and economy hooks with the per-card perk ability resolution defined by `Perk Ability Hooks` and `perk-rules`.

## ADDED Requirements

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
