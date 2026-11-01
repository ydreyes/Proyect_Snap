# Spec Delta

## Purpose

Readable, player-triggered inspection of a card or a zone through a full-screen detail panel opened by a tap, without playing the card and without pausing the turn timer.

## ADDED Requirements

### Requirement: Tap Opens Card Detail

The system SHALL open a card detail panel when the player taps a card during the play phase.

#### Scenario: Tapping a hand card

- **WHEN** the player taps a card in their hand during the play phase
- **THEN** the card detail panel SHALL open for that card
- **AND** the card SHALL remain in the hand
- **AND** no energy SHALL be spent or reserved

#### Scenario: Tapping a board card

- **WHEN** the player taps a card in a player or rival zone slot
- **THEN** the card detail panel SHALL open for that card
- **AND** the card SHALL remain in its slot
- **AND** the zone power totals SHALL remain unchanged

#### Scenario: Tapping a card in the pile viewer

- **WHEN** the pile viewer is open and the player taps one of its visible cards
- **THEN** the card detail panel SHALL open for that card
- **AND** the pile viewer SHALL remain open behind the panel

#### Scenario: Dragging is not a tap

- **WHEN** the player presses a card and moves beyond the tap threshold before releasing
- **THEN** the card SHALL be dragged and no detail panel SHALL open

### Requirement: Tap Opens Zone Detail

The system SHALL open a zone detail panel when the player taps a zone whose area is not occupied by a card.

#### Scenario: Tapping the location plaque

- **WHEN** the player taps the plaque of any zone during the play phase
- **THEN** the zone detail panel SHALL open for that zone

#### Scenario: Tapping an empty slot or column background

- **WHEN** the player taps an empty player slot, an empty rival slot or the background of a location column
- **THEN** the zone detail panel SHALL open for that zone

#### Scenario: A card wins the tap

- **WHEN** the player taps a card inside a zone
- **THEN** the card detail panel SHALL open
- **AND** the zone detail panel SHALL NOT open

### Requirement: Card Detail Contents

The system SHALL show a card large enough to read, with its identity, cost, current power and descriptive text.

#### Scenario: Card contents

- **WHEN** the card detail panel opens for a card
- **THEN** it SHALL show the card's name, cost, current power and descriptive text
- **AND** the current power SHALL include the total perk bonus
- **AND** it SHALL show the perk bonus when it is nonzero

#### Scenario: Long description

- **WHEN** the descriptive text is longer than one line
- **THEN** it SHALL wrap inside the panel
- **AND** every line SHALL remain inside the panel bounds

#### Scenario: Rival card

- **WHEN** the player taps a rival card
- **THEN** the panel SHALL show the same card contents as for a player card

### Requirement: Zone Detail Contents

The system SHALL show a zone's identity, effect and current standing when it is revealed, and withhold the hidden information when it is not.

#### Scenario: Revealed zone

- **WHEN** the zone detail panel opens for a revealed zone
- **THEN** it SHALL show the zone name, its art, its effect text and both power totals
- **AND** the totals SHALL match the values shown on the location plaque

#### Scenario: Unrevealed zone

- **WHEN** the zone detail panel opens for an unrevealed zone
- **THEN** it SHALL show an unknown marker instead of the name, effect text and art
- **AND** it SHALL state the turn on which the zone will be revealed
- **AND** it SHALL still show both power totals

### Requirement: Detail Panel Dismissal

The system SHALL close the detail panel on an explicit close action and return to the previous interaction state.

#### Scenario: Close control

- **WHEN** the player taps the panel's close control
- **THEN** the panel SHALL close

#### Scenario: Tapping outside

- **WHEN** the player taps outside the panel's content
- **THEN** the panel SHALL close

#### Scenario: Returning to the pile viewer

- **WHEN** the panel was opened from the pile viewer and the player closes it
- **THEN** the pile viewer SHALL still be open with the same side, tab and page

#### Scenario: Timer expiry

- **WHEN** the play phase time limit expires while the panel is open
- **THEN** the panel SHALL close
- **AND** the turn SHALL end as usual with the cards already played

### Requirement: Inspection Does Not Pause the Turn

The system SHALL keep the play phase timer running while the detail panel is open.

#### Scenario: Timer keeps counting

- **WHEN** the panel opens during the play phase
- **THEN** the remaining turn time SHALL keep decreasing
- **AND** closing the panel SHALL NOT extend the turn

#### Scenario: No board mutation

- **WHEN** any card or zone detail panel opens
- **THEN** no card SHALL change location
- **AND** no energy SHALL be spent or reserved
- **AND** zone power totals SHALL NOT change

### Requirement: Detail Panel Isolation

The system SHALL prevent board interaction while the detail panel is open.

#### Scenario: Board is blocked while the panel is open

- **WHEN** the panel is open and the player presses on a hand card, a board card or a board slot
- **THEN** no drag SHALL start and no card SHALL move
- **AND** the tap SHALL close the panel instead
