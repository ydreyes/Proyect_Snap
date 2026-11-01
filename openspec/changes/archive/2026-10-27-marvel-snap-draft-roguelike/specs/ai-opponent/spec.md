# Spec Delta

## Purpose

The AI opponent system provides a functional opponent that plays according to the same game rules as the player.

## ADDED Requirements

### Requirement: AI Game Rule Compliance
The AI SHALL follow all the same game rules as the player.

#### Scenario: AI follows rules
- **WHEN** the AI takes actions
- **THEN** it SHALL respect zone limits (max 4 cards per side)
- **AND** it SHALL respect card costs/energy
- **AND** it SHALL follow zone play constraints
- **AND** it SHALL play cards during its play phase

### Requirement: AI Card Play
The AI SHALL play cards strategically during combat.

#### Scenario: AI decision making
- **WHEN** it's the AI's turn to play
- **THEN** the AI SHALL evaluate playable cards in its hand
- **AND** it SHALL make play decisions based on board state
- **AND** it SHALL place cards in appropriate zones

### Requirement: AI Turn Participation
The AI SHALL participate fully in all turn phases.

#### Scenario: AI turn flow
- **WHEN** a turn occurs
- **THEN** the AI SHALL draw cards at turn start
- **AND** it SHALL play cards during play phase
- **AND** its cards SHALL be revealed according to priority rules
- **AND** its effects SHALL trigger properly
