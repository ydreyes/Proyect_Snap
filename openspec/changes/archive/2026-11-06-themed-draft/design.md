# Design

## Context

See `proposal.md` - Why.

Current state:

- `DraftSystem` runs 8 uniform rounds, offers any draftable card and allows 2 copies.
- `DraftScene.OnEnter` always calls `Reset`, so it cannot resume.
- `CardDef` has no theme.
- The 13 starters are the original set; the profile's owned set is not consulted by the draft.
- `run-save-and-pause` already defines a draft checkpoint with phase, step, groups, offers, picks and copy counts.
- `meta-store-and-collection` provides the owned card set and the new starter set.

## Goals / Non-Goals

**Goals:**

- Teach Control, Destroy and OnReveal in the first three picks.
- Make ownership matter in the draft.
- Prevent duplicate copies during the draft.
- Keep the catalog at 59 unique cards while changing the starter set.
- Make the themed groups reproducible across a resume.

**Non-Goals:**

- New cards beyond the themed starters.
- A draft reroll, ban or skip.
- Perk assignment in the draft; perks come from level-up.
- Changing the combat rules for the new starters.

## Decisions

### 1. `DraftTheme` is card content

Add `DraftTheme` to `CardDef` with `Control`, `Destroy`, `OnReveal` and `Neutral`. Every draftable card declares one; tokens do not need one. The theme is displayed in the collection and in the draft.

The assignment is data in `ContentLoader`, not a scene branch. The full assignment:

| Theme | Cards |
|---|---|
| Control | Iron Man, Punisher, Hulk, The Thing, Cyclops, Angela, Armor, Bishop, Captain America, Cosmo, Devil Dinosaur, Enchantress, Klaw, Lizard, Namor, Onslaught, Professor X, Strong Guy, Abomination, Misty Knight, Shocker |
| Destroy | Elektra, Carnage, Deathlok, Angel, Apocalypse, Blade, Lady Sif, Nova, Spider-Woman, Sword Master, Wolverine |
| OnReveal | Hawkeye, Medusa, Sentinel, Star-Lord, America Chavez, Cable, Doctor Strange, Domino, Forge, Groot, Heimdall, Iron Fist, Mantis, Mister Sinister, Moon Girl, Morph, Rocket Raccoon, Scarlet Witch, Squirrel Girl, Uatu, White Queen |
| Neutral | Quicksilver, Hulkbuster, Korg, Kraven, Multiple Man, Yondu |

Counts: 21 Control, 11 Destroy, 21 OnReveal, 6 Neutral = 59.

### 2. New starter set and catalog

The new 13 starters are the designed Control/Destroy/OnReveal/Neutral set. Abomination, Misty Knight and Shocker leave the starter set but remain in the catalog as non-starter Control cards. Elektra, Carnage and Deathlok are both starters and members of the Series 1 list; the catalog counts them once. The 59 unique set is the union of the 13 starters, the 46 Series 1 cards and the 3 displaced starters, with duplicates removed.

The first-run owned set is the new 13 starters. The global store and bundles make the rest obtainable.

### 3. Draft state

```
DraftState
  Phase: Themed | Random | Complete
  StepIndex: 0..7
  Groups: 3 lists of 3 card ids
  CurrentOffers: 3 card ids
  Picks: 8 card ids
  CopyCount: id -> count
```

`DraftSystem` builds the state at the start of a run. `DraftScene` reads it from the checkpoint, not from a fresh reset.

### 4. Group generation

At draft start:

1. For each of Control, Destroy and OnReveal, take the owned cards of that theme.
2. Pick 3 distinct cards. If fewer than 3 exist, fill from owned Neutral cards and mark the group as filled.
3. Remove the chosen cards from the general pool so groups are disjoint.
4. Store the three groups in the draft state.

The initial owned set has 5 Control, 3 Destroy, 4 OnReveal and 1 Neutral, so the groups are always complete for a first run. The filler path exists for a future starter change or a partial ownership state.

### 5. Round flow

- Rounds 0, 1, 2: one group each, 1-of-3. The theme name is shown.
- Rounds 3-7: 1-of-3 from the remaining owned pool. The card picked in a themed round is removed; the two unpicked cards return to the pool.
- A card already picked is excluded by the 1-copy limit.
- The result is 8 cards, exactly the deck size.

Why 1-of-3 in the random phase instead of auto-add: the draft is the main deck-building decision; removing five choices would turn it into a formality.

### 6. Copy limit

During the draft, `MaxCopies = 1`. The in-game shop and the level-up card branch can add a second copy later. This keeps the themed groups from colliding and makes the shop's second-copy guarantee meaningful.

### 7. Ownership

The draft pool is `profile.OwnedCardIds` intersected with the draftable catalog. The level-up card branch applies the same owned-only rule. The in-game shop keeps using the full catalog because it sells into the run deck, not into ownership.

### 8. Persistence

The save change's draft DTO already carries phase, step, groups, offers, picks and copy counts. This change fills the themed phase and ensures the groups are generated once. A resumed draft must show the same theme and offers; the scene never rerolls.

### 9. UI

`DraftScene` shows the theme name in rounds 0-2, `ALEATORIO` in rounds 3-7 and the 8-pick progress. The completion state is unchanged.

## Risks / Trade-offs

- **[Theme assignment is subjective]** → Keep it data-driven; balance can change one card's theme without code.
- **[A small owned pool could starve a theme]** → Fill from Neutral and mark the group; the initial 13 always cover the three themes.
- **[The 59-card count can drift when starters change]** → Use the union rule and a content validation that fails on duplicates or missing cards.
- **[The draft can feel samey with only 3 Destroy starters]** → The random phase and the store expand the pool; the first run is intentionally focused.

## Migration Plan

1. Add `DraftTheme` and the 59 assignments.
2. Replace the 13 starters and update the catalog union rule.
3. Replace `DraftSystem` with the themed state and the 3+5 flow.
4. Update `DraftScene` for the theme label and resume.
5. Make the level-up card branch owned-only.
6. Verify theme coverage, starter set, group generation, copy limit, resume and a manual draft-to-combat pass.

Rollback is a revert; the catalog remains 59 and the old 13 starters can be restored.
