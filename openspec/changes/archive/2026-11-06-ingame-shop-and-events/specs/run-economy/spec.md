# Spec Delta

## MODIFIED Requirements

### Requirement: Run Money

The system SHALL track a money balance that belongs to the current run only.

#### Scenario: Money balance

- **WHEN** the run starts
- **THEN** the money balance SHALL be 0
- **AND** it SHALL increase only through run rewards and run events
- **AND** it SHALL never become negative

#### Scenario: Unspent money dies with the run

- **WHEN** a run ends for any reason
- **THEN** the unspent money SHALL be discarded
- **AND** it SHALL NOT be converted into global points or any persistent currency
- **AND** a new run SHALL start again with 0 money

#### Scenario: Purchases and event grants

- **WHEN** the player buys a shop offer or accepts an event
- **THEN** the money balance SHALL change by the price or grant
- **AND** a purchase SHALL reduce the balance but SHALL NOT reduce `MoneyEarned`
- **AND** an event cost that exceeds the balance SHALL be rejected
- **AND** the resulting balance SHALL be written to the run checkpoint
