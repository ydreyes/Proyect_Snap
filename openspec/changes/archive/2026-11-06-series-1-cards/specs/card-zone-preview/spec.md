# Spec Delta

## ADDED Requirements

### Requirement: Special Location Visibility

The system SHALL show hidden location information to a side that has Uatu, and SHALL show the new definition after a location is replaced.

#### Scenario: Uatu reveals the zone panel

- **WHEN** the player controls Uatu and opens the detail panel for an unrevealed zone
- **THEN** the panel SHALL show the location's name, art and effect text
- **AND** the zone SHALL still be treated as unrevealed for reveal timing and effects
- **AND** the panel SHALL still show both power totals

#### Scenario: Uatu does not reveal to the rival

- **WHEN** the player controls Uatu and the rival would view the same unrevealed zone
- **THEN** the rival view SHALL keep the unknown marker
- **AND** the real reveal order SHALL be unchanged

#### Scenario: Replaced location detail

- **WHEN** Scarlet Witch replaces a location
- **THEN** the zone detail panel SHALL show the new location's name, art and effect
- **AND** the panel SHALL keep the same zone index and power totals
- **AND** the previous location's text SHALL no longer be shown
