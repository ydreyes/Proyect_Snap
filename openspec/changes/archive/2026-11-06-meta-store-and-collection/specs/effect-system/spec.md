# Spec Delta

## ADDED Requirements

### Requirement: Perk Effect Dispatch

The system SHALL resolve perk effect ids through the effect processor.

#### Scenario: Perk effects

- **WHEN** a perk with effect ids is attached to a card
- **THEN** the effect processor SHALL resolve every id
- **AND** each id SHALL map to a registered perk rule implementation
- **AND** an unknown id SHALL fail content validation rather than being ignored

#### Scenario: Global rule scope

- **WHEN** a perk rule runs
- **THEN** it SHALL affect the player's side according to its rule text
- **AND** it SHALL NOT require the host card to be in play
- **AND** the same rule SHALL be applied once per attached copy

#### Scenario: No double application

- **WHEN** a card's power is recalculated multiple times in a turn
- **THEN** the perk rule SHALL produce the same result
- **AND** it SHALL NOT accumulate into the card's stored power
