# Spec Delta

## ADDED Requirements

### Requirement: Retired Perk Migration

The system SHALL migrate perk ids that no longer exist in the catalog without discarding the profile or the run.

#### Scenario: Profile migration

- **WHEN** a profile is loaded
- **THEN** every owned perk id that is not in the current 28-perk catalog SHALL be removed from `OwnedPerkIds`
- **AND** the matching entries SHALL be removed from `PerkCopyCounts`
- **AND** the rest of the profile SHALL remain unchanged
- **AND** the current catalog perks SHALL be present as owned

#### Scenario: Run migration

- **WHEN** a run checkpoint is loaded
- **THEN** every attached perk id that is not in the current catalog SHALL be removed from the saved card
- **AND** the card, its upgrades, its cost delta and its power delta SHALL remain
- **AND** the run SHALL resume at the same node

#### Scenario: Save schema

- **WHEN** the migration runs
- **THEN** the `profile.json` and `run.json` field names SHALL remain unchanged
- **AND** the migration SHALL NOT add a new save file
- **AND** the next save SHALL write the cleaned ids

#### Scenario: No catalog regression

- **WHEN** a current perk id is present in the profile or a saved deck
- **THEN** the migration SHALL preserve it
- **AND** its copies and attachments SHALL remain valid
