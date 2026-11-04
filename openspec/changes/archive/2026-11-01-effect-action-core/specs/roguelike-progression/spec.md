# Spec Delta

## MODIFIED Requirements

### Requirement: Win Rewards

The system SHALL provide card rewards for winning a combat that grants one.

#### Scenario: Post-win rewards

- **WHEN** the player wins a combat that grants a card reward
- **THEN** the system SHALL show 3 distinct cards from the draftable catalog, each with a perk that has a positive power modifier
- **AND** the player SHALL pick 1 card to add to their deck
- **AND** if the deck already has 12 cards, the player SHALL either remove 1 card and substitute it with the chosen card, or skip the reward
- **AND** the chosen card SHALL keep its perk

#### Scenario: Token definitions are excluded from rewards

- **WHEN** a card reward is generated
- **THEN** token definitions SHALL NOT be offered
- **AND** only definitions declared as draftable SHALL be offered
- **AND** the same exclusion SHALL apply to the reward screen shown after a loss or a tie
