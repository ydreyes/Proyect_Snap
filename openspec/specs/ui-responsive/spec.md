# ui-responsive Specification

## Purpose
The UI responsive system provides adaptive portrait layout that works on any screen size using percentage-based positioning and scaling.

## Requirements

### Requirement: Portrait Orientation
The system SHALL force portrait orientation for mobile gameplay.

#### Scenario: Game initialization
- **WHEN** the game starts
- **THEN** the display SHALL be set to portrait orientation
- **AND** the resolution SHALL be adaptable to any screen size

### Requirement: Adaptive Layout System
The system SHALL provide a layout manager that calculates positions using percentages of the drawable area, where the drawable area excludes the platform safe-area insets.

#### Scenario: Layout calculation
- **WHEN** the screen size changes or game initializes
- **THEN** the LayoutManager SHALL calculate all UI regions based on percentages
- **AND** all positions SHALL scale proportionally
- **AND** the layout SHALL work on any screen size

#### Scenario: Safe-area insets
- **WHEN** the game runs on a device whose display has a cutout, rounded corners or a system gesture area
- **THEN** the layout SHALL reserve the top, bottom, left and right safe-area insets reported by the platform
- **AND** no control, plaque or hand card SHALL be drawn or hit-tested inside those insets
- **AND** the remaining area SHALL still be divided using the portrait percentages

#### Scenario: Smallest supported phone viewport
- **WHEN** the drawable area is at least 720 by 1280 pixels in portrait orientation
- **THEN** every text SHALL remain legible using the shared typography system
- **AND** every control SHALL remain reachable with a finger
- **AND** no layout region SHALL overlap another region

### Requirement: Portrait Layout Structure
The system SHALL arrange UI elements in portrait layout.

#### Scenario: Combat layout
- **WHEN** in combat scene
- **THEN** the screen SHALL be divided vertically into a top bar, a location area, a controls area and a hand area
- **AND** the location area SHALL show 3 location columns side by side
- **AND** each column SHALL show the rival grid above the location plaque and the player grid below it
- **AND** each side of a column SHALL show 4 card slots in a 2 by 2 grid
- **AND** the layout SHALL use the full portrait screen

#### Scenario: Slot scaling
- **WHEN** the viewport size changes
- **THEN** every card slot SHALL scale proportionally with its location column
- **AND** slots SHALL remain large enough to be dragged onto without pixel-precise aiming

### Requirement: Location Plaque
The system SHALL show a compact location plaque between the rival and player grids, with different contents depending on whether the zone is revealed.

#### Scenario: Plaque contents
- **WHEN** a location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals when the zone is revealed
- **AND** it SHALL withhold the name, effect text and art when the zone is unrevealed
- **AND** the totals SHALL update as cards are played, moved or removed

#### Scenario: Revealed plaque contents
- **WHEN** a revealed location is displayed
- **THEN** the plaque SHALL show the location name, its effect text and both power totals
- **AND** the power totals SHALL remain legible at the smallest supported window size
- **AND** the totals SHALL update as cards are played, moved or removed

#### Scenario: Unrevealed plaque contents
- **WHEN** an unrevealed location is displayed
- **THEN** the plaque SHALL withhold the name, effect text and art
- **AND** it SHALL show an unknown marker instead of the name
- **AND** it SHALL still show the location index and both power totals
- **AND** the totals SHALL update as cards are played, moved or removed

### Requirement: Detail Panel Layout

The system SHALL place the card and zone detail panels over the combat board using percentage-based geometry.

#### Scenario: Panel placement
- **WHEN** a detail panel opens
- **THEN** it SHALL cover the board area and dim what is behind it
- **AND** its bounds SHALL scale with the viewport
- **AND** its close control SHALL be reachable at every supported window size

#### Scenario: Panel content fits

- **WHEN** the card or zone panel content is drawn at the smallest supported window size
- **THEN** the card, its badges and its descriptive text SHALL stay inside the panel
- **AND** the descriptive text SHALL remain legible using the shared typography system

### Requirement: Text Rendering
The system SHALL display all texts using the shared typography system, which provides display, UI and body roles.

#### Scenario: Text display
- **WHEN** displaying text in any scene
- **THEN** text SHALL be rendered with the shared typography roles
- **AND** the system SHALL fall back to the existing default font when the shared fonts cannot be loaded
- **AND** all UI labels SHALL be visible

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

### Requirement: Pause and Main Menu Layout

The system SHALL lay out the pause overlay and the redesigned main menu in the existing portrait layout.

#### Scenario: Pause overlay

- **WHEN** the pause overlay is shown
- **THEN** it SHALL cover the scene with a dimmed background
- **AND** its buttons SHALL be reachable with a finger at the smallest supported viewport
- **AND** the number of buttons SHALL match the current node type
- **AND** no button SHALL be shown disabled for a missing action

#### Scenario: Pause button

- **WHEN** the player is in a run scene
- **THEN** a pause control SHALL be visible in a consistent corner
- **AND** it SHALL not overlap the hand, the top bar or the end-turn control
- **AND** it SHALL remain reachable at the smallest supported viewport

#### Scenario: Main menu

- **WHEN** the main menu is shown
- **THEN** `Continuar` and `Ver resumen` SHALL occupy the same slot
- **AND** `Nueva run`, `Tienda`, `Coleccion` and `Salir` SHALL remain reachable
- **AND** the layout SHALL not overlap the title or the global points
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Save error

- **WHEN** a save fails inside the pause overlay
- **THEN** the error message SHALL be legible
- **AND** the overlay SHALL remain open
- **AND** the retry and abandon controls SHALL remain reachable

### Requirement: Encounter Identity Display

The system SHALL show which enemy the player is facing.

#### Scenario: Rival avatar

- **WHEN** a combat starts against a named enemy
- **THEN** the rival avatar SHALL show the enemy name or a short initial
- **AND** it SHALL NOT show the generic label `RIVAL`
- **AND** the label SHALL fit at the smallest supported viewport

#### Scenario: Enemy selection cards

- **WHEN** the two candidates are shown
- **THEN** each card SHALL show the name, flavor phrase, difficulty and deck preview
- **AND** the two candidates SHALL remain distinguishable at the smallest supported viewport

#### Scenario: Boss framing

- **WHEN** a sub-boss or the final boss is announced
- **THEN** the screen SHALL distinguish it from a normal enemy
- **AND** the boss name and phrase SHALL be legible
- **AND** the continue control SHALL be reachable at the smallest supported viewport

### Requirement: Shop and Event Layout

The system SHALL lay out the shop and event nodes in the existing portrait layout.

#### Scenario: Shop cards

- **WHEN** the shop opens
- **THEN** the 4 card offers SHALL be visible without overlap
- **AND** each offer SHALL show its card, price and sold state
- **AND** the upgrade, removal, extra-life and leave controls SHALL be reachable
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Shop at the deck cap

- **WHEN** a card purchase requires removing a deck card
- **THEN** the deck grid SHALL be shown with the current effective cost and power
- **AND** the selected card SHALL be highlighted
- **AND** the confirm and cancel controls SHALL be reachable

#### Scenario: Event

- **WHEN** an event is shown
- **THEN** the title and narrative text SHALL be legible
- **AND** the accept option SHALL show its cost and reward
- **AND** the decline option SHALL be visible on every event
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Modifier list

- **WHEN** the active modifiers are shown in the shop, event, act transition or pause
- **THEN** every modifier SHALL be listed by name
- **AND** the list SHALL fit without hiding the primary controls

### Requirement: Themed Draft Layout

The system SHALL present the themed draft in the existing portrait layout.

#### Scenario: Themed round

- **WHEN** the draft is in one of the first three rounds
- **THEN** the theme name SHALL be visible above the 3 offers
- **AND** the three cards SHALL be visible without overlap
- **AND** the progress counter SHALL show the pick number out of 8
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Random round

- **WHEN** the draft is in rounds 4 through 8
- **THEN** the theme label SHALL be hidden or show `ALEATORIO`
- **AND** the three cards and the progress counter SHALL remain visible
- **AND** the player SHALL be able to pick with a finger

#### Scenario: Draft completion

- **WHEN** the 8th card is picked
- **THEN** the completion state SHALL show the 8/8 progress
- **AND** the continue control SHALL be reachable
- **AND** the 8-card deck SHALL be preserved into combat

#### Scenario: Draft resume

- **WHEN** a draft checkpoint is resumed
- **THEN** the same theme, offers and progress SHALL be visible
- **AND** the scene SHALL NOT flash or reset to round 1

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

### Requirement: Combat Item Bar

The system SHALL show the item inventory during combat.

#### Scenario: Item bar

- **WHEN** the combat scene is in the player's play phase
- **THEN** the item bar SHALL show every item with a count above 0
- **AND** each entry SHALL show the item name or icon, its effect summary and its count
- **AND** the bar SHALL be reachable without covering the hand, the end-turn control or the board
- **AND** the layout SHALL remain usable at the smallest supported viewport

#### Scenario: Disabled item

- **WHEN** an item has no legal target or the profile write is in progress
- **THEN** its use control SHALL be disabled
- **AND** the count SHALL remain visible
- **AND** the reason SHALL be understandable from the item state

#### Scenario: Use feedback

- **WHEN** an item is used
- **THEN** the board or the energy indicator SHALL update in the same frame
- **AND** the count SHALL decrease immediately
- **AND** a rejected use SHALL show a short message without changing the count

### Requirement: Series 2 Combat Indicators

The system SHALL keep the new combat indicators legible and reachable in the portrait layout.

#### Scenario: Plaque status

- **WHEN** a location plaque shows a flooded, locked or Series 3 status at 720 by 1280 pixels
- **THEN** the status label SHALL remain legible
- **AND** it SHALL NOT overlap the location name, effect text or power totals
- **AND** the plaque SHALL remain tappable

#### Scenario: Moveable card

- **WHEN** a moveable card is shown in the hand or on the board at 720 by 1280 pixels
- **THEN** its move indicator SHALL remain visible
- **AND** it SHALL NOT hide the card name, cost or power
- **AND** the card SHALL remain draggable with a finger

#### Scenario: Safe area

- **WHEN** the device reports safe-area insets
- **THEN** no status label or move indicator SHALL be drawn inside the insets
- **AND** no new control SHALL be placed inside them

#### Scenario: No overlap

- **WHEN** the combat screen shows a flooded or locked location with cards and a full hand
- **THEN** no indicator SHALL overlap the hand, the end-turn control or the power totals
- **AND** every interactive control SHALL remain reachable

### Requirement: Expanded Catalog Layout

The system SHALL keep the collection and global store usable with 217 cards and twenty-six bundles.

#### Scenario: Collection paging

- **WHEN** the collection opens with 217 cards at the smallest supported viewport
- **THEN** the four-column grid SHALL remain usable
- **AND** the page indicator and previous and next controls SHALL remain reachable
- **AND** the filter chips SHALL remain reachable

#### Scenario: Store rotation

- **WHEN** the global store opens with twenty-six bundles
- **THEN** the three visible bundles and the eighteen direct cards SHALL remain reachable through the existing tabs and paging
- **AND** no entry SHALL overlap another
- **AND** the points balance and purchase controls SHALL remain reachable

#### Scenario: Card detail

- **WHEN** a Series 3 card or token detail is opened at the smallest supported viewport
- **THEN** its name, cost, power, description and state SHALL fit inside the panel
- **AND** the movement or zone status SHALL remain legible

### Requirement: Series 3 Combat Layout

The system SHALL keep the new Series 3 controls and states reachable at the smallest supported viewport.

#### Scenario: Activate control

- **WHEN** an in-play owned card with an unused Activate effect is opened at 720 by 1280 pixels
- **THEN** the `ACTIVAR` control SHALL remain reachable
- **AND** its hit area SHALL be at least 44 by 44 logical pixels
- **AND** it SHALL NOT overlap the description or the close control
- **AND** the detail panel SHALL remain closable

#### Scenario: End-of-game banner

- **WHEN** the end-of-game resolution starts at 720 by 1280 pixels
- **THEN** the `FIN DE LA PARTIDA` banner SHALL remain legible
- **AND** the end-turn control SHALL be disabled and SHALL NOT be tappable
- **AND** the banner SHALL NOT cover the zone power totals

#### Scenario: Delayed card

- **WHEN** a delayed card is shown at 720 by 1280 pixels
- **THEN** its face-down `???` treatment SHALL remain legible
- **AND** the card SHALL NOT reveal its identity through the detail panel
- **AND** the slot SHALL remain tappable for legal interactions

#### Scenario: Limbo turn display

- **WHEN** Limbo is active and the turn display shows `TURNO n/7`
- **THEN** the label SHALL remain legible
- **AND** it SHALL NOT overlap the energy badge or the end-turn control
- **AND** the display SHALL return to `/6` when Limbo leaves play

#### Scenario: New status labels

- **WHEN** a location shows `LIMBO`, `SILENCIADA` or a Goose/Jean Grey restriction at 720 by 1280 pixels
- **THEN** the label SHALL remain legible
- **AND** it SHALL NOT overlap the name, effect text or power totals
- **AND** the plaque SHALL remain tappable
