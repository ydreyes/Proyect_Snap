# Spec Delta

## ADDED Requirements

### Requirement: Series 1 Presentation

The system SHALL keep the new Series 1 states legible with the existing portrait layout.

#### Scenario: Seven-card hand

- **WHEN** the player's hand has 7 cards
- **THEN** every card SHALL remain visible in the hand area
- **AND** the cards SHALL overlap without hiding the name or power of the selected card
- **AND** the layout SHALL remain usable at the smallest supported window size

#### Scenario: Token cards

- **WHEN** a token card is on the board or in a pile viewer
- **THEN** it SHALL show its name, current power and cost badge
- **AND** it SHALL be visually distinguishable from a draftable card

#### Scenario: Merged host

- **WHEN** Hulkbuster merges into a host
- **THEN** the host's card view SHALL show the increased power in the same frame
- **AND** Hulkbuster's slot SHALL show as empty
- **AND** no separate merged-card slot SHALL be added to the board

#### Scenario: Uatu plaque

- **WHEN** the player controls Uatu
- **THEN** the location plaque of an unrevealed location SHALL show its name, effect and art
- **AND** the opponent plaque SHALL remain hidden
- **AND** the power totals SHALL remain visible and update normally

#### Scenario: Replaced location plaque

- **WHEN** Scarlet Witch replaces a location
- **THEN** the plaque SHALL show the new name and effect in the same frame
- **AND** the plaque SHALL keep the same index and power totals
