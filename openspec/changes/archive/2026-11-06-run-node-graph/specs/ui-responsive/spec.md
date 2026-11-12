# Spec Delta

## ADDED Requirements

### Requirement: Node Graph Screens

The system SHALL lay out the new run nodes in the existing portrait layout.

#### Scenario: Enemy selection

- **WHEN** enemy selection is shown
- **THEN** the two candidates SHALL be visible side by side or stacked without overlap
- **AND** each candidate SHALL show its name, flavor phrase, difficulty and a deck preview
- **AND** each candidate SHALL be selectable with a finger at the smallest supported viewport

#### Scenario: Sub-boss intro

- **WHEN** a sub-boss intro is shown
- **THEN** the sub-boss name and flavor phrase SHALL be legible
- **AND** the continue control SHALL be reachable at the smallest supported viewport
- **AND** no combat control SHALL be visible

#### Scenario: Act transition

- **WHEN** an act transition is shown
- **THEN** the new act, level, deck size and money SHALL be legible
- **AND** the continue control SHALL be reachable at the smallest supported viewport

#### Scenario: Node screens do not reset the run

- **WHEN** the player moves between enemy selection, sub-boss intro, act transition and combat
- **THEN** the run state SHALL be preserved
- **AND** only the new run start action SHALL reset it
