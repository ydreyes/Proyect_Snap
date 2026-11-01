# Spec Delta

## ADDED Requirements

### Requirement: Turn Energy Pool
The system SHALL grant a per-turn energy pool equal to the current turn number, capped at 6.

#### Scenario: Pool size follows the turn
- **WHEN** a turn starts
- **THEN** the available energy pool SHALL equal the turn number
- **AND** on turns beyond 6 the pool SHALL remain 6

#### Scenario: Energy does not carry over
- **WHEN** a turn ends with unspent energy
- **THEN** the unspent energy SHALL be lost
- **AND** the next turn SHALL start with a fresh pool for that turn

### Requirement: Energy Expenditure and Refund
The system SHALL derive the player's available energy from the cards played during the current turn.

#### Scenario: Available energy after a play
- **WHEN** the player plays a card of cost n during the current turn
- **THEN** the available energy SHALL decrease by n
- **AND** the card SHALL record the turn in which it was played

#### Scenario: Cards from earlier turns do not consume energy
- **WHEN** a card was played on an earlier turn and remains in a zone
- **THEN** its cost SHALL NOT reduce the current turn's available energy

#### Scenario: Returning a card refunds its cost
- **WHEN** a card played during the current turn is returned to the player's hand
- **THEN** its cost SHALL be added back to the available energy
- **AND** it SHALL be charged again if replayed

#### Scenario: Moving a card already in play
- **WHEN** a card is moved from one zone slot to another
- **THEN** the available energy SHALL NOT change

### Requirement: Cost-Gated Card Play
The system SHALL allow players to play cards from hand to zone slots during the play phase, paying each card's energy cost.

#### Scenario: Playing a card
- **WHEN** a player plays a card
- **THEN** the card SHALL be placed in the chosen free slot if the zone has space (max 4 per side)
- **AND** the play order SHALL be recorded
- **AND** the card's cost SHALL NOT exceed the available energy
- **AND** the available energy SHALL be reduced by the card's cost
- **AND** zone play constraints SHALL be respected

#### Scenario: Unaffordable card
- **WHEN** a player attempts to play a card whose cost exceeds the available energy
- **THEN** the play SHALL be rejected
- **AND** the board SHALL remain unchanged

## REMOVED Requirements

### Requirement: Card Play
**Reason**: Replaced by "Cost-Gated Card Play"; energy is no longer informational and the old block no longer describes the contract.
**Migration**: Use "Cost-Gated Card Play" together with the energy pool, expenditure, and refund requirements added by this change.
