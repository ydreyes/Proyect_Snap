# Spec Delta

## MODIFIED Requirements

### Requirement: Card Dragging

The system SHALL allow dragging cards from the player hand or from a player slot during PlayPhase, once the pointer moves beyond the tap threshold.

#### Scenario: Start drag

- **WHEN** player presses a card in their hand during PlayPhase and moves the pointer beyond the tap threshold
- **THEN** the card SHALL become draggable
- **AND** the card SHALL follow the mouse/finger position
- **AND** the card SHALL maintain drag offset

#### Scenario: Start drag from board

- **WHEN** player presses a card placed in a player slot during PlayPhase and moves the pointer beyond the tap threshold
- **THEN** the card SHALL be lifted from its slot and become draggable
- **AND** the slot SHALL be shown as free while the drag is active
- **AND** the zone power totals SHALL exclude the lifted card until it is placed again

#### Scenario: Press without movement

- **WHEN** player presses a card and releases it without moving beyond the tap threshold
- **THEN** no drag SHALL start
- **AND** the card SHALL NOT be lifted, removed from the hand or removed from its slot
- **AND** no energy SHALL be reserved
- **AND** the tap SHALL open the card detail panel
