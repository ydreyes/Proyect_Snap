# Design

## Context

See `proposal.md` - Why.

The current state:

- The run summary shows economy and deck but no persistent currency.
- `run-save-and-pause` defines a profile with points, owned/discovered ids, perk copies, item counts, `lastAwardedRunId` and store rotation as reserved fields.
- `ContentLoader` registers three flat perks (`perk_plus1`, `perk_minus1`, `perk_plus2`) and the level-up perk branch offers every registered perk.
- `PowerResolver` only sums `PerkDef.Modifiers["power"]`; `PerkDef.EffectIds` is not dispatched.
- There is no ownership, global store or collection.
- The 59-card catalog is fixed.

This change uses the reserved profile fields and replaces the flat perk model with rules.

## Goals / Non-Goals

**Goals:**

- Award points once per run and show the breakdown.
- Give the profile a clear ownership model and a deterministic catalog partition.
- Sell bundles, direct cards, perks and extra lives without duplicates.
- Show the collection with the five agreed states.
- Make perks real rules applied through the effect/power/energy hooks.

**Non-Goals:**

- Combat items (next change) and their inventory.
- A general-purpose perk scripting language.
- Changing the 59-card catalog or the draft pool (themed-draft does that).
- Online accounts, cloud sync or multiple profiles.

## Decisions

### 1. Run identity and idempotent points

Each run receives a `RunId` (a GUID or a timestamp-based id) at `StartNewRun`. The summary computes the award from the run state:

```
3 * normalCombatsWon
+ 40 * subBossesWon
+ 8 * finalLevel
+ (150 if final boss defeated)
```

`normalCombatsWon` and `subBossesWon` are derived from `CombatsWon` and the run graph. If the run state does not keep the split, the director records both counters when a combat is won.

The award is written to the profile only if `profile.LastAwardedRunId != run.RunId`. The summary shows the breakdown, the total and a "already collected" state when the id matches. The run id is set after a successful profile write, so a failed write can be retried without double-awarding.

### 2. Profile ownership and catalog partition

`Profile` holds:

```
Points
OwnedCardIds
DiscoveredCardIds
DiscoveredPerkIds
DiscoveredEnemyIds
PerkCopies (id -> count)
ItemCounts (id -> count)
LastAwardedRunId
BundleRotationIndex
```

The 13 starters are owned by default. The remaining 46 cards are partitioned at content load:

- 6 direct-sale cards;
- 40 cards across six bundles (3×6, 2×7, 1×8).

The partition is validated: no overlap, no missing card, no token. The themed-draft change can change the 13 starters; the partition is recomputed from the new starter set, so the rule survives.

Why compute the partition instead of hardcoding a list: the starter set is scheduled to change, and a hardcoded 59-card split would silently break. The rule "starter + direct + bundle = all draftable" is the invariant.

### 3. Store entries and rotation

| Entry | Price | Effect |
|---|---:|---|
| Item | 60 | Reserved; hidden until item definitions exist |
| Extra life | 90 | +1 profile extra-life charge for the next run |
| Perk | 120 | Adds one owned perk |
| Direct card | 150 | Adds one specific card to owned |
| Bundle 6 | 450 | 6 cards |
| Bundle 7 | 650 | 7 cards |
| Bundle 8 | 850 | 8 cards |

Three bundles are visible at a time from `BundleRotationIndex`. A purchase marks the bundle owned and replaces it with the next unowned bundle. Starting a new run advances the index by one. Owned entries are disabled and show their sold state.

The store never sells an owned card or bundle twice. Direct cards are single entries; after purchase they show as owned.

### 4. Collection states

The collection derives one base state per card:

1. `Owned` - in `OwnedCardIds`.
2. `Discovered` - in `DiscoveredCardIds` but not owned.
3. `InStore` - not owned or discovered, but present in a visible store entry.
4. `Hidden` - none of the above.
5. `InRunDeck` - an overlay on `Owned` when the card is in the current deck.

The grid shows all 59 cards. The state filter, theme filter and cost filter combine. Hidden cards show a silhouette and the "descúbrela en la partida" text; their name, cost and power are withheld. Opening the collection never discovers anything.

The perks tab lists the 8 perks with owned copies and rule text.

### 5. Perk rules

The 3 flat perks are replaced by 8:

| Perk | Source | Rule |
|---|---|---|
| Vanguardia | initial | first card played each combat gets +2 power |
| Aura | initial | your Ongoing cards get +1 power |
| Francotirador | initial | your On Reveal cards get +1 power |
| Letal | initial | your cards with effective cost ≤ 1 get +2 power |
| Agil | store | first card played each turn costs 1 less |
| Guardian | store | your cards in your fewest-card zone get +1 power |
| Imparable | store | your cards get +1 power from turn 5 |
| Codicioso | store | +3 money after each won combat |

Each perk has `EffectIds` pointing to a rule implementation. A perk is attached to a card, but its rule is global while the card is in the run deck. Two copies of the same perk apply twice. The rule engine is a new small dispatcher that the effect processor, `PowerResolver`, `CardActionService` and `RunDirector` call at their existing hooks.

Why global while in the deck: the player chooses a card to host the perk, but the rule is a build-defining modifier. Requiring the host to be in play would make most perks dead most of the time.

### 6. Level-up and store integration

The level-up perk branch reads `profile.OwnedPerkIds`, not every registered perk. The 4 initial perks are owned by default; the 4 store perks become available after purchase. The 2-copy limit is unchanged.

### 7. Summary

`RunSummaryScene` adds:

```
COMBATES NORMALES x 3 = N
SUBJEFES x 40 = N
NIVEL x 8 = N
JEFE FINAL = 150
TOTAL = N
```

The award is attempted on entry. If the profile write fails, the summary shows an error and a retry; the run id is not stored until the write succeeds.

### 8. Persistence

The profile fields are already reserved by the save change. This change fills them and uses the same atomic writer. The run id and the awarded flag are profile data; the run summary reads them to display "already collected".

## Risks / Trade-offs

- **[Points inflation]** → The formula is fixed and visible; the store prices are the only sink. Tune prices rather than the formula.
- **[Bundle partition drift when starters change]** → Validate at load and fail with a clear content error.
- **[Perk stacking can become too strong]** → Two-copy cap and per-copy application; no hidden multiplicative effects.
- **[Global perk rules can surprise the player]** → Show the rule text in the collection and the level-up picker.
- **[Profile write failure during award]** → Retry and do not store the run id until the write succeeds.

## Migration Plan

1. Replace the 3 flat perks with the 8 rule definitions and the rule dispatcher.
2. Add the profile ownership/catalog partition and the store.
3. Add the collection and the summary breakdown.
4. Wire the level-up owned-perk pool and the perk hooks.
5. Verify with points/idempotency, partition, purchase, collection-state and perk-hook harnesses.
6. Manual pass through a run award, a bundle purchase, a direct card, a perk purchase and the collection states.

Rollback is a revert; the profile fields are additive and the flat perks can be restored.
