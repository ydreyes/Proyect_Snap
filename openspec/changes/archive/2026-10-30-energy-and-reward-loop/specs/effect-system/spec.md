# Spec Delta

## MODIFIED Requirements

### Requirement: Power Resolution
The system SHALL calculate card power dynamically from base power and active modifiers.

#### Scenario: Ongoing modifier application
- **WHEN** ongoing effects or attached perks are active
- **THEN** the power resolver SHALL compute card power as base power plus all relevant modifiers
- **AND** power SHALL be recalculated from base power whenever board state changes
- **AND** modifiers from zones, cards, and perks SHALL all be considered

#### Scenario: No accumulation across evaluations
- **WHEN** power is recalculated more than once for the same card
- **THEN** the result SHALL be identical
- **AND** modifiers SHALL NOT stack on top of a previously calculated value
