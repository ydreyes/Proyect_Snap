# Spec Delta

## MODIFIED Requirements

### Requirement: Perks Tab

The system SHALL show the perk collection.

#### Scenario: Perk list

- **WHEN** the perks tab is opened
- **THEN** all 28 ability perks SHALL be listed
- **AND** each perk SHALL show its name, ability text and owned copies
- **AND** each perk SHALL show its compatibility requirement when it has one
- **AND** every perk SHALL be shown as owned
- **AND** the tab SHALL remain usable at the smallest supported viewport

#### Scenario: Perk detail

- **WHEN** the player opens a perk
- **THEN** the detail SHALL show its full rule text, its trigger, its compatibility and the number of copies attached across the deck
- **AND** it SHALL show whether the current run has a compatible host

#### Scenario: Preview does not discover

- **WHEN** the collection is opened
- **THEN** viewing a card or perk SHALL NOT mark it as discovered
- **AND** only a run context SHALL discover content
