# Spec Delta

## ADDED Requirements

### Requirement: Meta Store and Collection Layout

The system SHALL lay out the global store, the collection and the expanded summary in the portrait layout.

#### Scenario: Global store

- **WHEN** the global store opens
- **THEN** the points balance SHALL be visible
- **AND** the three visible bundles and the single entries SHALL be reachable
- **AND** sold entries SHALL show their sold state
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Collection grid

- **WHEN** the collection opens
- **THEN** the four-column grid SHALL be scrollable
- **AND** the state, theme and cost filters SHALL be reachable
- **AND** a card detail SHALL show the card, its state and its obtain path
- **AND** the perks tab SHALL be reachable

#### Scenario: Summary

- **WHEN** the expanded summary opens
- **THEN** the points breakdown SHALL be legible
- **AND** the final deck SHALL remain visible
- **AND** the menu and re-run controls SHALL remain reachable
- **AND** no region SHALL overlap at the smallest supported viewport
