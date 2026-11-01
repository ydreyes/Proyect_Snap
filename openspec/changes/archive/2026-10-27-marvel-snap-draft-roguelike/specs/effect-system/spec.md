# Spec Delta

## Purpose

The effect system provides a pluggable architecture for card and zone effects supporting multiple trigger types with registry/factory patterns to ensure extensibility without core system changes.

## ADDED Requirements

### Requirement: Effect Trigger Types
The system SHALL support the following effect trigger types: OnReveal, Ongoing, EndOfTurn, and Activate.

#### Scenario: Different trigger types
- **WHEN** effects are defined
- **THEN** each effect SHALL be associated with one of the supported trigger types
- **AND** effects SHALL only trigger at their designated times

### Requirement: Pluggable Effect Architecture
The system SHALL allow new effects to be added without modifying core systems.

#### Scenario: Adding new effect
- **WHEN** a new effect type needs to be added
- **THEN** it SHALL be implemented as a class implementing the effect interface
- **AND** it SHALL be registered in the effect registry
- **AND** no changes to the effect processor core SHALL be required

### Requirement: Effect Registry
The system SHALL maintain a registry of all available effects.

#### Scenario: Effect lookup
- **WHEN** an effect needs to be applied
- **THEN** the system SHALL look up the effect by ID in the registry
- **AND** the correct effect implementation SHALL be instantiated

### Requirement: Effect Processor
The system SHALL process effects according to their trigger types at appropriate times.

#### Scenario: OnReveal processing
- **WHEN** cards are revealed in reveal order
- **THEN** OnReveal effects SHALL trigger in that reveal order
- **AND** each effect SHALL be applied with appropriate context

#### Scenario: EndOfTurn processing
- **WHEN** a turn ends
- **THEN** EndOfTurn effects SHALL be processed
- **AND** effects SHALL be applied after reveal resolution

#### Scenario: Activate processing
- **WHEN** a player activates a card's ability
- **THEN** the Activate effect SHALL be processed if conditions are met
- **AND** the ability usage SHALL be tracked

### Requirement: Power Resolution
The system SHALL calculate card power dynamically based on base power and ongoing modifiers.

#### Scenario: Ongoing modifier application
- **WHEN** ongoing effects are active
- **THEN** the power resolver SHALL apply all relevant modifiers to card power
- **AND** power SHALL be recalculated when board state changes
- **AND** modifiers from zones, cards, and perks SHALL all be considered
