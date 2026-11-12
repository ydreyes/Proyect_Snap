# Spec Delta

## Purpose

Defines the 12 run events, the always-available decline, the accept costs and rewards, and the rule that events never start a combat.

## ADDED Requirements

### Requirement: Event Pool

The system SHALL provide 12 events for the run.

#### Scenario: Event selection

- **WHEN** an event node opens
- **THEN** the system SHALL choose an event from the 12 that has not appeared earlier in the run
- **AND** when all 12 have appeared, it MAY choose any event again
- **AND** the chosen event id SHALL be stored in the run checkpoint
- **AND** re-entering the event node SHALL show the same event

#### Scenario: Event content

- **WHEN** an event is shown
- **THEN** it SHALL have a title, a short narrative text, an accept option and a decline option
- **AND** the accept option SHALL state its cost and reward
- **AND** the decline option SHALL be available on every event

### Requirement: Declining an Event

The system SHALL let the player decline every event.

#### Scenario: Decline

- **WHEN** the player declines an event
- **THEN** the run SHALL continue to the next node
- **AND** no money, card, perk, upgrade, modifier or XP SHALL be granted
- **AND** the event SHALL be marked as seen

### Requirement: Accepting an Event

The system SHALL apply the event reward or cost when the player accepts.

#### Scenario: Reward types

- **WHEN** the player accepts an event
- **THEN** the event MAY grant 0-30 experience
- **AND** it MAY grant money
- **AND** it MAY grant a card, an upgrade, a perk or a run modifier
- **AND** it MAY cost money
- **AND** the result SHALL be applied once

#### Scenario: Unaffordable cost

- **WHEN** the accept option costs more money than the run has
- **THEN** the accept option SHALL be disabled
- **AND** the decline option SHALL remain available
- **AND** no money SHALL become negative

#### Scenario: Event is a safe node

- **WHEN** an event is resolved
- **THEN** the run checkpoint SHALL be written
- **AND** the event SHALL NOT start a combat
- **AND** the player SHALL continue to the next node

### Requirement: Event Content Inventory

The system SHALL include the following 12 events.

| Event | Accept |
|---|---|
| La grieta dimensional | Pay 40 money; gain 30 XP |
| El mercader errante | Pay 60 money; gain a random upgrade on a deck card |
| Entrenamiento de combate | Gain 20 XP |
| Botin de la batalla | Gain 50 money |
| El altar del poder | Pay 30 money; gain a random perk |
| La fuente de energia | Gain the energy bonus modifier |
| El contrato del coleccionista | Pay 30 money; gain a random card |
| La prueba del valor | Gain 30 XP; lose 20 money |
| El mercado negro | Pay 80 money; gain a random card and an upgrade |
| La bendicion del vigilante | Gain the energy cap modifier |
| El peaje del viajero | Lose 40 money |
| La mano del destino | Gain the starting hand modifier |

#### Scenario: Every event is declinable

- **WHEN** any of the 12 events is shown
- **THEN** the decline option SHALL be enabled
- **AND** declining SHALL always be safe
