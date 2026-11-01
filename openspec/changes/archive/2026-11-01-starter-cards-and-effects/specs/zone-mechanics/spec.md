# Spec Delta

## MODIFIED Requirements

### Requirement: Zone Control Calculation
The system SHALL calculate zone control based on the final card power and zone multipliers.

#### Scenario: Control determination
- **WHEN** determining zone control
- **THEN** total power of all cards on each side in the zone SHALL be calculated
- **AND** the side with higher total power SHALL control the zone
- **AND** ties SHALL be handled by a defined rule
- **AND** zone control SHALL be used for priority calculation

#### Scenario: Ongoing bonuses count
- **WHEN** a card in the zone has an Ongoing power bonus
- **THEN** that bonus SHALL be included in the zone total
- **AND** it SHALL update when the board changes

#### Scenario: One calculation for every consumer
- **WHEN** zone power is displayed or evaluated
- **THEN** priority, victory, the location plaque and the detail panel SHALL use the same calculation
- **AND** they SHALL NOT sum card power through separate paths

## ADDED Requirements

### Requirement: Zone Power Multipliers
The system SHALL apply Ongoing zone multipliers to the total power of one side in a zone.

#### Scenario: Iron Man doubles the allied total
- **WHEN** Iron Man is in a zone
- **THEN** the total power of its side in that zone SHALL be doubled
- **AND** the individual power of each card SHALL remain unchanged

#### Scenario: Multiple multipliers
- **WHEN** two Iron Men are in the same zone
- **THEN** the allied zone total SHALL be multiplied by four

#### Scenario: Multiplier affects only its side
- **WHEN** Iron Man is in a zone
- **THEN** only the total of Iron Man's side SHALL be multiplied
- **AND** the rival total SHALL remain unchanged

#### Scenario: Derived, not accumulated
- **WHEN** zone power is calculated more than once
- **THEN** the multiplier SHALL be applied to the raw sum each time
- **AND** it SHALL NOT be stored in card power or added repeatedly
