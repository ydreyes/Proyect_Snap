# ui-visual-design Specification

## Purpose

Defines the shared sci-fi visual language and the presentation rules for the seven player-facing screens, so every screen uses the same tokens, typography, chrome, card art, motion and transitions without changing game behavior.

## Requirements

### Requirement: Shared Design Tokens

The system SHALL apply one shared token set for color, typography, spacing, corner geometry and elevation across all seven player-facing screens.

#### Scenario: Consistent palette

- **WHEN** any of the seven player-facing screens is displayed
- **THEN** its background, panel, accent, text and state colors SHALL come from the shared token set
- **AND** no screen SHALL define its own independent palette

#### Scenario: Consistent geometry

- **WHEN** a panel, button, badge or chip is drawn
- **THEN** its corner cut, border, inner highlight and elevation SHALL match the shared chrome tokens

#### Scenario: Semantic state colors

- **WHEN** an element is affordable, unavailable, selected, sold or dangerous
- **THEN** it SHALL use the shared semantic color for that state

### Requirement: Typography Hierarchy

The system SHALL render all text with a shared typography system that provides display, UI and body roles with distinct sizes, weights and tracking.

#### Scenario: Display headings

- **WHEN** a screen title or a ceremony heading is shown
- **THEN** it SHALL use the display role in uppercase with increased tracking

#### Scenario: UI labels

- **WHEN** a button, tab, filter, price or status label is shown
- **THEN** it SHALL use the UI role
- **AND** it SHALL remain legible at the smallest supported viewport

#### Scenario: Body text

- **WHEN** rules, descriptions or narrative text is shown
- **THEN** it SHALL use the body role
- **AND** it SHALL wrap inside its container without clipping

#### Scenario: Numeric emphasis

- **WHEN** cost, power, currency or points are shown
- **THEN** the number SHALL be visually dominant over its label
- **AND** the number SHALL use tabular figures so changing values do not shift the layout

#### Scenario: Font fallback

- **WHEN** the shared fonts cannot be loaded
- **THEN** the system SHALL fall back to the existing default font
- **AND** every label SHALL still be rendered

### Requirement: Chrome Components

The system SHALL render panels, currency chips, ribbons, buttons, badges, progress slots and overlays through one shared chrome implementation with consistent states.

#### Scenario: Button states

- **WHEN** a button is idle, hovered, pressed or disabled
- **THEN** it SHALL show a distinct visual state within 200 ms
- **AND** a disabled button SHALL NOT activate when pressed

#### Scenario: Currency chip

- **WHEN** a balance is shown
- **THEN** it SHALL appear in a chip with an icon, the numeric value and the currency name or symbol

#### Scenario: Panel chrome

- **WHEN** a panel is shown
- **THEN** it SHALL use the shared angular cut, border highlight and shadow

#### Scenario: Progress slot

- **WHEN** progress is shown as a row of slots
- **THEN** filled, current and empty slots SHALL be visually distinct

#### Scenario: Overlay chrome

- **WHEN** a modal, detail panel or pause overlay is shown
- **THEN** it SHALL dim the content behind it
- **AND** it SHALL use the shared panel chrome and close treatment

### Requirement: Deterministic Procedural Card Art

The system SHALL generate card art procedurally from a stable seed derived from the card identity, theme and cost, and SHALL cache the result for reuse.

#### Scenario: Stable output

- **WHEN** the same card is displayed again in any screen or session
- **THEN** its procedural art SHALL be identical

#### Scenario: Thematic variation

- **WHEN** cards of different themes or costs are displayed
- **THEN** their art SHALL differ in base hue and composition

#### Scenario: Cached reuse

- **WHEN** a card is drawn more than once
- **THEN** the system SHALL reuse the cached art instead of regenerating it each frame

#### Scenario: Art and content separation

- **WHEN** card art is drawn
- **THEN** cost, power, name, description and badges SHALL be drawn as separate layers
- **AND** they SHALL NOT be baked into the art

#### Scenario: Legibility over art

- **WHEN** text or badges are drawn over art
- **THEN** a scrim or plate SHALL preserve contrast

### Requirement: Optional Real-Art Slot

The system SHALL allow a card's art layer to be replaced by a provided art asset without changing the frame, badges, text or layout.

#### Scenario: Art asset present

- **WHEN** a card has a provided art asset
- **THEN** the renderer SHALL use it in the art layer
- **AND** the frame, badges and text SHALL remain the same as the procedural version

#### Scenario: Art asset absent

- **WHEN** a card has no provided art asset
- **THEN** the renderer SHALL use the procedural art

#### Scenario: Aspect handling

- **WHEN** a provided art asset has a different aspect ratio than the art layer
- **THEN** the renderer SHALL crop it to the art layer's safe area
- **AND** it SHALL NOT distort the card

### Requirement: Ambient Background

The system SHALL draw a shared multi-layer ambient background behind the seven player-facing screens.

#### Scenario: Layer composition

- **WHEN** a player-facing screen is shown
- **THEN** the background SHALL include a vertical gradient, a hex or grid layer, a vignette and a particle or dust layer

#### Scenario: Depth

- **WHEN** the background animates
- **THEN** its layers SHALL move at different rates to create parallax

#### Scenario: Foreground contrast

- **WHEN** background motion is active
- **THEN** foreground panels and text SHALL remain readable

#### Scenario: Cached layers

- **WHEN** the background is drawn
- **THEN** its static layers SHALL be cached
- **AND** they SHALL NOT be regenerated every frame

### Requirement: Motion and Feedback

The system SHALL animate screen entry, element entry, selection and press feedback with short, interruptible tweens.

#### Scenario: Entrance

- **WHEN** a screen or a group of cards appears
- **THEN** its elements SHALL enter with a staggered fade or scale
- **AND** the entrance SHALL complete within 400 ms

#### Scenario: Press feedback

- **WHEN** the player presses a control
- **THEN** the control SHALL show a press state within 100 ms
- **AND** it SHALL return to its resting state within 200 ms of release

#### Scenario: Selection

- **WHEN** the player selects a card or an option
- **THEN** the selection SHALL show a clear visual confirmation before the screen changes

#### Scenario: Non-blocking motion

- **WHEN** an animation is running
- **THEN** input SHALL remain responsive
- **AND** the animation SHALL NOT delay a state transition beyond its duration

#### Scenario: Frame budget

- **WHEN** animations run on the smallest supported device
- **THEN** the target frame rate SHALL be maintained

### Requirement: Scene Transitions

The system SHALL transition between the seven player-facing screens with a shared transition.

#### Scenario: Transition

- **WHEN** the player moves from one screen to another
- **THEN** the outgoing screen SHALL fade or wipe out
- **AND** the incoming screen SHALL fade or wipe in

#### Scenario: Input safety

- **WHEN** a transition is in progress
- **THEN** taps SHALL NOT activate controls on the incoming screen

#### Scenario: Bounded duration

- **WHEN** a transition is triggered repeatedly
- **THEN** its duration SHALL remain bounded
- **AND** transitions SHALL NOT accumulate or stack

### Requirement: Main Menu Presentation

The system SHALL present the main menu as a run hub with currency chips, a run reactor, one primary action slot and bottom navigation.

#### Scenario: Run active

- **WHEN** a run is in progress
- **THEN** the reactor SHALL show the current enemy or act, the win count and the deck strip
- **AND** the primary action slot SHALL show `CONTINUAR`

#### Scenario: Run finished

- **WHEN** a run is finished and a summary is available
- **THEN** the primary action slot SHALL show `VER RESUMEN`

#### Scenario: No run

- **WHEN** there is no run
- **THEN** the primary action slot SHALL show `NUEVA RUN`
- **AND** the reactor SHALL show an empty state

#### Scenario: Navigation

- **WHEN** the main menu is shown
- **THEN** `COLECCION`, `TIENDA`, `NUEVA RUN` and `SALIR` SHALL be reachable from the bottom navigation
- **AND** the global points SHALL be visible in a currency chip

#### Scenario: Confirm new run

- **WHEN** a run is active and the player starts a new run
- **THEN** a confirmation dialog SHALL appear before the run is reset

### Requirement: Draft Presentation

The system SHALL present the draft as an eight-slot progress strip above three card offers.

#### Scenario: Progress

- **WHEN** the draft is shown
- **THEN** the strip SHALL show eight slots
- **AND** the strip SHALL show the number of picks made and the current pick number

#### Scenario: Themed round

- **WHEN** the draft is in one of the first three rounds
- **THEN** the theme SHALL be shown above the offers

#### Scenario: Random round

- **WHEN** the draft is in rounds four through eight
- **THEN** the theme SHALL show `ALEATORIO` or be hidden

#### Scenario: Pick feedback

- **WHEN** the player picks a card
- **THEN** the card SHALL animate toward the deck strip
- **AND** the next offers SHALL enter with a stagger

#### Scenario: Resume

- **WHEN** a draft checkpoint is resumed
- **THEN** the same theme, offers and progress SHALL be shown
- **AND** the screen SHALL NOT flash or reset to round one

### Requirement: Run Summary Presentation

The system SHALL present the run summary as a result screen with an outcome reactor, deck strip, points breakdown and actions.

#### Scenario: Defeat

- **WHEN** the run ends in defeat
- **THEN** the screen SHALL show a defeat treatment, the final deck, the points earned and the available actions

#### Scenario: Victory

- **WHEN** the run ends in victory
- **THEN** the screen SHALL show a victory treatment, the final deck, the points earned and the available actions

#### Scenario: Extra life consumed

- **WHEN** the run continued because an extra-life charge was consumed
- **THEN** the screen SHALL state that the charge was consumed
- **AND** it SHALL NOT present a choice that did not occur

#### Scenario: Actions

- **WHEN** the summary is shown
- **THEN** the menu and re-run controls SHALL be reachable

#### Scenario: Points breakdown

- **WHEN** the points breakdown is shown
- **THEN** each source and its amount SHALL be legible

### Requirement: Collection Presentation

The system SHALL present the collection as a four-column art-driven grid with filter chips, paging and a detail overlay.

#### Scenario: Grid

- **WHEN** the collection opens
- **THEN** each visible card SHALL show its procedural or provided art, name, cost and power

#### Scenario: Card states

- **WHEN** a card is undiscovered, discovered, owned or upgraded
- **THEN** its tile SHALL show the corresponding state treatment

#### Scenario: Filters

- **WHEN** the player changes the state, theme or cost filter
- **THEN** the grid SHALL update
- **AND** the filter chips SHALL show the active selection

#### Scenario: Detail

- **WHEN** the player opens a card
- **THEN** the detail overlay SHALL show the large card, its rules, its state and its obtain path

#### Scenario: Perks

- **WHEN** the player opens the perks tab
- **THEN** all 28 ability perks SHALL be shown with the same tile and detail treatment
- **AND** each tile SHALL show the perk name, its ability text and its compatibility requirement when it has one
- **AND** every perk SHALL be shown as owned
- **AND** the detail SHALL show the trigger, the compatibility and the copies attached across the current deck

#### Scenario: Paging

- **WHEN** the grid has more cards than one page
- **THEN** the page indicator and the previous and next controls SHALL be reachable
- **AND** the grid SHALL remain usable with 217 cards

### Requirement: Level-Up and Upgrade Presentation

The system SHALL present level-up and upgrade selection with one ceremony template that adapts to the current choice phase.

#### Scenario: Ceremony

- **WHEN** a level-up starts
- **THEN** the screen SHALL show an `UPGRADE UNLOCKED` treatment with the current level

#### Scenario: Branch choice

- **WHEN** the player must choose between a card and a perk
- **THEN** the two options SHALL be shown as large tiles
- **AND** the perk tile SHALL be disabled when no compatible host exists in the deck

#### Scenario: Card offer

- **WHEN** card offers are shown
- **THEN** each card SHALL appear as a medallion with its art, name, cost and power

#### Scenario: Upgrade type

- **WHEN** an upgrade type is chosen
- **THEN** `-1 COSTE` and `+2 PODER` SHALL be shown as large tiles with their effect

#### Scenario: Perk offer

- **WHEN** perk offers are shown
- **THEN** each perk SHALL show its name, its ability text and its compatibility requirement
- **AND** the offer SHALL include only perks with at least one compatible host in the deck

#### Scenario: Target choice

- **WHEN** a card or perk requires a deck target
- **THEN** the deck SHALL be shown as a selectable carousel with current cost and power
- **AND** only compatible hosts SHALL be selectable for a perk
- **AND** an incompatible card SHALL be shown disabled with the reason it cannot host the perk

#### Scenario: Unavailable

- **WHEN** no upgrade is available
- **THEN** the screen SHALL show an unavailable state
- **AND** the exit control SHALL be reachable

#### Scenario: No compatible perk

- **WHEN** no perk in the catalog has a compatible host in the deck
- **THEN** the perk branch SHALL show an unavailable state
- **AND** the card branch SHALL remain reachable

### Requirement: Global Store Presentation

The system SHALL present the global store with a currency header, tabs, an offer grid and a sold state.

#### Scenario: Header

- **WHEN** the global store opens
- **THEN** the points balance SHALL be visible in a currency chip

#### Scenario: Tabs

- **WHEN** the global store opens
- **THEN** the `OFERTAS` and `CARTAS` tabs SHALL be available
- **AND** the active tab SHALL be visually distinct

#### Scenario: Offers

- **WHEN** the offers tab is shown
- **THEN** the three visible bundles and the single entries SHALL be shown as cards with price and contents

#### Scenario: Sold state

- **WHEN** an entry is sold
- **THEN** it SHALL show a sold treatment
- **AND** its purchase control SHALL be disabled

#### Scenario: Purchase feedback

- **WHEN** a purchase succeeds
- **THEN** the balance and the entry state SHALL update in the same frame

### Requirement: In-Run Shop Presentation

The system SHALL present the in-run shop with a currency header, card offers and service rows.

#### Scenario: Header

- **WHEN** the in-run shop opens
- **THEN** the run money SHALL be visible in a currency chip

#### Scenario: Card offers

- **WHEN** the in-run shop opens
- **THEN** the four card offers SHALL be shown with art, price and sold state

#### Scenario: Services

- **WHEN** the services are shown
- **THEN** upgrade, removal, extra life and leave SHALL be reachable as distinct rows or tiles

#### Scenario: Purchase feedback

- **WHEN** a purchase succeeds
- **THEN** the money, the offer state and the affected card SHALL update in the same frame

#### Scenario: Deck selection

- **WHEN** a purchase requires choosing a deck card
- **THEN** the deck SHALL be shown with effective cost and power
- **AND** the confirm and cancel controls SHALL be reachable

### Requirement: Behavior Preservation

The redesign SHALL preserve the existing screen behavior, data and navigation.

#### Scenario: Draft rules

- **WHEN** the draft runs
- **THEN** it SHALL still make eight picks of one of three cards from the owned pool with the one-copy limit

#### Scenario: Collection contents

- **WHEN** the collection opens
- **THEN** it SHALL still contain the 92 draftable cards and the perks tab

#### Scenario: Currencies

- **WHEN** a store is shown
- **THEN** it SHALL use only the existing currency for that store
- **AND** points SHALL be used by the global store and run money by the in-run shop

#### Scenario: Extra life

- **WHEN** a combat is lost and an extra-life charge is available
- **THEN** the charge SHALL still be consumed automatically by the existing run flow

#### Scenario: Meta progression

- **WHEN** points are awarded
- **THEN** the existing points-only meta progression SHALL be unchanged

#### Scenario: Persistence

- **WHEN** the game saves or resumes
- **THEN** the save format and checkpoint behavior SHALL be unchanged

#### Scenario: No new systems

- **WHEN** the redesign is applied
- **THEN** no new currency, timer, season, saved deck, notification or in-app purchase SHALL be introduced

### Requirement: Responsive and Touch Constraints

The system SHALL keep the redesign inside the existing portrait and safe-area constraints.

#### Scenario: Smallest viewport

- **WHEN** the drawable area is 720 by 1280 pixels
- **THEN** every label SHALL remain legible
- **AND** every control SHALL remain reachable
- **AND** the Activate control and the end-of-game banner SHALL fit

#### Scenario: Safe area

- **WHEN** the device reports safe-area insets
- **THEN** no chrome, text or control SHALL be drawn or hit-tested inside them

#### Scenario: Touch target

- **WHEN** a control is interactive
- **THEN** its hit area SHALL be at least 44 by 44 logical pixels
- **AND** it SHALL NOT require hover to activate

#### Scenario: No overlap

- **WHEN** a screen is laid out
- **THEN** no region SHALL overlap another region

### Requirement: Performance and Resource Budget

The system SHALL keep the visual layer within a device-friendly budget.

#### Scenario: Cached textures

- **WHEN** a gradient, glow, noise, hex tile or card art is needed
- **THEN** it SHALL be generated once and reused

#### Scenario: Lazy generation

- **WHEN** the game starts
- **THEN** card art SHALL be generated on first use rather than for all cards at startup

#### Scenario: Bounded memory

- **WHEN** all 217 cards have been displayed
- **THEN** the card art cache SHALL stay within the documented memory budget

#### Scenario: Frame rate

- **WHEN** the seven screens run on the smallest supported device
- **THEN** the target frame rate SHALL be maintained

#### Scenario: No per-frame allocation

- **WHEN** a screen is drawn
- **THEN** the visual layer SHALL NOT allocate textures or large buffers per frame

### Requirement: Series 2 Indicators

The system SHALL present the new Series 2 states with the shared visual language.

#### Scenario: Moveable card

- **WHEN** a card can be moved by the player
- **THEN** its card view SHALL show a short moveable indicator
- **AND** the indicator SHALL use the shared semantic state colors
- **AND** it SHALL NOT cover the card name, cost or power

#### Scenario: Location status

- **WHEN** a location is flooded or locked
- **THEN** its plaque SHALL show a status label using the shared typography and chrome
- **AND** the label SHALL remain legible at the smallest supported viewport
- **AND** it SHALL NOT overlap the location name, effect text or power totals

#### Scenario: Token and switched card

- **WHEN** a token or a switched-side card is displayed
- **THEN** it SHALL use the same card frame and art layers as any other card
- **AND** its controller side SHALL be visually clear from its zone placement
- **AND** no separate token or switched-card scene SHALL be added

#### Scenario: Status update

- **WHEN** a movement permission is used, a location floods or a lock starts
- **THEN** the indicator SHALL update in the same frame as the board change
- **AND** no screen reload or scene transition SHALL be required

### Requirement: Series 3 Presentation

The system SHALL present the new Series 3 states with the shared visual language.

#### Scenario: Activate control

- **WHEN** an owned in-play card has an unused `Activate` effect
- **THEN** its detail panel SHALL show an `ACTIVAR` control
- **AND** the control SHALL use the shared button chrome and semantic accent
- **AND** it SHALL NOT overlap the close control or the description
- **AND** it SHALL be hidden when the card cannot activate

#### Scenario: Delayed reveal

- **WHEN** a card is delayed by Invisible Woman
- **THEN** its card view SHALL render face-down with `???`
- **AND** an enemy delayed card's identity SHALL remain hidden in the detail panel
- **AND** the card SHALL still show its side and slot

#### Scenario: End-of-game state

- **WHEN** the match enters the end-of-game resolution
- **THEN** the combat screen SHALL show a `FIN DE LA PARTIDA` banner
- **AND** the end-turn control SHALL be disabled
- **AND** the result modal SHALL appear only after the resolution completes

#### Scenario: Zone status

- **WHEN** a location is Limbo, suppressed by Hawk or restricted by Goose or Jean Grey
- **THEN** its plaque SHALL show a short status label
- **AND** the label SHALL use the shared typography and chrome
- **AND** it SHALL NOT overlap the name, effect text or power totals

#### Scenario: Copied text

- **WHEN** a card has copied or replaced text
- **THEN** its detail panel SHALL show a `COPIA: <name>` line or the replacement text
- **AND** the line SHALL remain legible at the smallest supported viewport
- **AND** it SHALL NOT cover the card name, cost or power

#### Scenario: Catalog counts

- **WHEN** the collection or store shows its count
- **THEN** it SHALL show 217 cards, 18 direct cards and 26 bundles
- **AND** the counts SHALL remain legible and paginated
