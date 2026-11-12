# Spec Delta

## ADDED Requirements

### Requirement: Taunt Reservation

The system SHALL make the AI respect a taunt reservation on its first card of the following turn.

#### Scenario: First card obeys taunt

- **WHEN** the player's Taunt resolved on turn N
- **AND** the AI plays its first card on turn N + 1
- **THEN** the AI SHALL place that card in the taunted zone when the zone has room
- **AND** the AI SHALL still pay the card's cost from its own energy

#### Scenario: Taunted zone full

- **WHEN** the taunted zone has no room for the AI's first card
- **THEN** the reservation SHALL expire
- **AND** the AI SHALL play normally without being blocked

#### Scenario: No legal first card

- **WHEN** the AI has no affordable card or no legal card for the taunted zone
- **THEN** it SHALL play nothing or choose another legal zone according to the card's own restrictions
- **AND** the reservation SHALL expire at the end of the turn

#### Scenario: Multiple reservations

- **WHEN** more than one taunt reservation exists for the same AI turn
- **THEN** the system SHALL resolve them in a deterministic order
- **AND** only the first applicable reservation SHALL constrain the first card
- **AND** the remaining reservations SHALL expire

#### Scenario: AI safety

- **WHEN** the taunt reservation conflicts with the AI's self-harm safety
- **THEN** the AI SHALL still obey the taunt if a legal card exists
- **AND** it SHALL NOT play a card that is illegal in the taunted zone
