# Spec Delta

## MODIFIED Requirements

### Requirement: Energy Expenditure and Refund

The system SHALL derive each side's available energy from an explicit per-turn ledger of charges and refunds, using each card's effective cost.

#### Scenario: Available energy after a play

- **WHEN** the player plays a card with effective cost n during the current turn
- **THEN** the available energy SHALL decrease by n
- **AND** n SHALL include the card's upgrades and any temporary cost modifier active at the moment of the play
- **AND** the card SHALL record the turn in which it was played

#### Scenario: Cards from earlier turns do not consume energy

- **WHEN** a card was played on an earlier turn and remains in a zone
- **THEN** its cost SHALL NOT reduce the current turn's available energy

#### Scenario: Returning a card refunds its cost

- **WHEN** a card played during the current turn is returned to the player's hand
- **THEN** the amount charged for that play SHALL be added back to the available energy
- **AND** it SHALL be charged again at its effective cost if replayed

#### Scenario: Moving a card already in play

- **WHEN** a card is moved from one zone slot to another
- **THEN** the available energy SHALL NOT change

#### Scenario: Destroying, merging or banishing does not refund

- **WHEN** a card played during the current turn is destroyed, merged or banished
- **THEN** its cost SHALL remain spent for that turn
- **AND** the available energy SHALL NOT increase

#### Scenario: Lifting a card does not free its cost

- **WHEN** a card played during the current turn is lifted from the board
- **THEN** its cost SHALL remain committed while the drag is active
- **AND** the available energy SHALL NOT increase
- **AND** dropping it into another slot SHALL NOT change its cost
- **AND** returning it to the hand SHALL refund the cost

#### Scenario: Both sides use the ledger

- **WHEN** the AI plays, destroys, merges, banishes, moves or returns a card
- **THEN** the AI's available energy SHALL follow the same charge and refund rules as the player's

### Requirement: Cost-Gated Card Play

The system SHALL allow players to play cards from hand to zone slots during the play phase, paying each card's effective energy cost.

#### Scenario: Playing a card

- **WHEN** a player plays a card
- **THEN** the card SHALL be placed in the chosen free slot if the zone has space (max 4 per side)
- **AND** the play order SHALL be recorded
- **AND** the card's effective cost SHALL NOT exceed the available energy
- **AND** the available energy SHALL be reduced by the card's effective cost
- **AND** zone play constraints SHALL be respected

#### Scenario: Unaffordable card

- **WHEN** a player attempts to play a card whose effective cost exceeds the available energy
- **THEN** the play SHALL be rejected
- **AND** the board SHALL remain unchanged

#### Scenario: Upgraded card becomes affordable

- **WHEN** a card has a `-1 cost` upgrade
- **THEN** it SHALL be playable when the available energy equals its effective cost even if that is below its base cost
- **AND** the energy preview and hand display SHALL show the effective cost

#### Scenario: Temporary cost modifier

- **WHEN** an effect changes a card's cost for the current turn
- **THEN** the effective cost SHALL include that modifier while it is active
- **AND** the modifier SHALL be cleared when the turn ends
- **AND** clearing it SHALL NOT alter the card's permanent cost delta
