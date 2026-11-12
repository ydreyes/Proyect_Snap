# Proposal

## Why

The draft still offers eight uniform rounds from all 59 cards with a 2-copy limit, and the starter deck is the original 13. Ownership exists in the profile but the draft ignores it, so the meta collection does not change what the player can draft. The redesign wants a themed opening that teaches the three archetypes, followed by five free picks, using only owned cards.

## What Changes

- **Add a `DraftTheme` to every card**: Control, Destroy, OnReveal or Neutral.
- **Replace the 13 starters** with the designed set:
  - Control: Iron Man, Punisher, Hulk, The Thing, Cyclops.
  - Destroy: Elektra, Carnage, Deathlok.
  - OnReveal: Hawkeye, Medusa, Sentinel, Star-Lord.
  - Neutral: Quicksilver.
  - Abomination, Misty Knight and Shocker leave the starter set but remain in the 59-card catalog.
- **Change the draft to 3 themed rounds + 5 random rounds**: one 1-of-3 pick from each themed group, then five 1-of-3 picks from the remaining owned pool. The result is the 8-card deck.
- **Draft only owned cards**, not the full catalog.
- **Limit copies to 1 during the draft**: a card already picked cannot be offered again. Second copies come from the in-game shop or level-up.
- **Persist the themed groups and the current offers**; a resumed draft shows the same round and never rerolls.
- **Show the theme name** in the first three rounds.
- **Remove the old perk assignment to the last 3 draft cards**; perks now come from the level-up node.
- **Make the level-up card branch owned-only**, matching the draft.
- **Keep the catalog at 59 unique cards**: the 3 displaced starters remain available as non-starter cards, and the 3 Series 1 cards promoted to starters are counted once.

## Capabilities

### New Capabilities

- None. The change modifies the existing draft and content capabilities.

### Modified Capabilities

- `draft-system`: the initial draft becomes 3 themed + 5 random rounds, owned-only, max 1 copy, with persisted groups; the perk-assignment requirement is removed.
- `content-data`: every card declares a draft theme; the starter set changes to the new 13; the catalog stays at 59 unique cards with the displaced starters retained.
- `run-economy`: the level-up card branch offers owned cards only.
- `ui-responsive`: the themed draft rounds show the theme name and the 8-pick progress at the smallest viewport.

## Impact

- Content: `CardDef.DraftTheme`, `DraftTheme` enum, `ContentLoader` assigns themes to all 59 cards and the new starter set.
- Draft: `DraftSystem`, `DraftScene`, `DraftState`/DTO, `LevelUpSystem` card branch.
- Persistence: the draft checkpoint already stores phase, step, groups, offers, picks and copy counts; this change fills the themed phase.
- Meta: the starter set and the owned-card pool change; the collection and store partition derive from the new starter set.
- Verification: theme coverage for 59 cards, starter-set checks, draft generation per phase, copy-limit checks, resume checks and a manual draft-to-combat pass.
