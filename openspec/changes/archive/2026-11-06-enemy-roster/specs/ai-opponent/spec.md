# Spec Delta

## ADDED Requirements

### Requirement: Encounter Deck and Difficulty

The AI SHALL play the deck and difficulty of the selected enemy.

#### Scenario: Enemy deck source

- **WHEN** a combat starts
- **THEN** the AI deck SHALL come from the selected enemy definition
- **AND** the deck SHALL contain exactly the cards declared by that enemy
- **AND** the AI SHALL NOT use a random deck from the player's pool

#### Scenario: Difficulty choice

- **WHEN** the AI chooses among affordable cards
- **THEN** a difficulty of 1 or 2 SHALL use the safe power-per-energy heuristic
- **AND** a difficulty of 3 SHALL prefer the highest-power affordable card
- **AND** a difficulty of 4 or 5 SHALL also prefer the zone where the player has the least power
- **AND** the AI SHALL continue to respect energy, zone limits and self-harm safety

#### Scenario: Boss decks

- **WHEN** a sub-boss or the final boss is played
- **THEN** the AI SHALL use the boss deck
- **AND** the boss difficulty SHALL be at least 3
- **AND** the final boss SHALL use difficulty 5
