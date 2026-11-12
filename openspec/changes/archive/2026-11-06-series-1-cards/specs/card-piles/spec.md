# Spec Delta

## MODIFIED Requirements

### Requirement: Combat Lifecycle

The system SHALL treat discarded and destroyed as combat-scoped, and SHALL support both combat-scoped and run-scoped banishment.

#### Scenario: Discarded and destroyed return next combat

- **WHEN** a combat ends with cards in the discarded or destroyed pile
- **THEN** those cards SHALL be available to the player's deck in the next combat
- **AND** the piles SHALL be empty when that combat begins

#### Scenario: Yondu's banish is combat-scoped

- **WHEN** Yondu banishes a card
- **THEN** the card SHALL enter the banished pile for the current combat
- **AND** it SHALL NOT be removed from the run deck
- **AND** it SHALL be available to the run deck again in the next combat
- **AND** it SHALL appear in the banished pile viewer for the current combat

#### Scenario: Banished never return

- **WHEN** a card is banished
- **THEN** it SHALL be removed from the run deck
- **AND** that card instance SHALL NOT appear in any later combat of the run
- **AND** it SHALL NOT be offered as a reward, a draft choice or a perk target
- **AND** a new card of the same definition MAY still be offered, as a separate instance

#### Scenario: The run continues with a smaller deck

- **WHEN** cards are banished for the run and the run deck shrinks below its maximum
- **THEN** the next combat SHALL start with the reduced deck
- **AND** rewards SHALL still be able to add cards up to the maximum size

## ADDED Requirements

### Requirement: Merged Cards

The system SHALL represent a merged card as a card that left the combat without entering any pile.

#### Scenario: Merge removes the card from the board

- **WHEN** Hulkbuster merges into a host
- **THEN** Hulkbuster SHALL be removed from its zone slot
- **AND** it SHALL NOT enter discarded, destroyed or banished
- **AND** the zone power totals SHALL be recalculated without its individual card power

#### Scenario: Merge transfers power

- **WHEN** Hulkbuster merges into a host
- **THEN** the host's persistent power SHALL increase by Hulkbuster's current Power
- **AND** the host SHALL keep its own definition and abilities
- **AND** the host's current power SHALL include the merged power

#### Scenario: Merged cards are not viewer-visible

- **WHEN** a card is merged
- **THEN** it SHALL NOT appear in any pile viewer tab
- **AND** it SHALL NOT be playable, drawn or counted toward zone power for the rest of the combat

#### Scenario: Merged cards return next combat

- **WHEN** a combat ends with a merged card
- **THEN** its run-deck instance SHALL remain available for the next combat
- **AND** the merge state SHALL NOT persist into the next combat

#### Scenario: Merge is not destruction

- **WHEN** a card merges
- **THEN** no destroy, discard or banish event SHALL fire
- **AND** destroy protections SHALL NOT block the merge
- **AND** no pile count SHALL change
