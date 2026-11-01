# Proposal

## Why

Combat has no resource economy: the energy badge is decorative, so a full hand can be dumped on turn 1 regardless of cost. The run also stops after a single fight. Nothing increments `RunState.CurrentFightIndex`, `ProgressionManager` and `RewardSystem` are never instantiated, and the reward screen is a placeholder that returns to the menu. The deck produced by the draft is never tested or improved.

This change turns energy into the per-turn constraint that shapes each turn and closes the 15-fight loop (combat → reward → next combat), so a run becomes a sequence of fights that grows and upgrades the deck.

## What Changes

- **Real energy pool**: each turn grants `min(turn, 6)` energy, spent within that turn and not carried over.
- **Cost-aware plays**: a card may only move from hand to a zone if its cost can be paid. Cards played on earlier turns do not consume the current pool; moving a card already in play costs nothing.
- **Refund on return**: dragging a card back to hand releases the energy it was paid with, so it can be replayed or replaced.
- **Energy display**: the badge shows *remaining* energy, not the turn number, and updates after every play or refund.
- **AI energy parity**: the AI spends its own pool, may make several plays per turn, and picks affordable cards by power per cost. It plays nothing when it cannot afford any card.
- **Run structure**: 3 cycles of 5 combats (15 total). Within each cycle, winning combat 1 grants a card reward, combat 2 a perk reward, combat 3 a card reward, combat 4 nothing, and combat 5 is the boss (the 15th is the FinalBoss).
- **Card reward screen**: 3 distinct cards from the catalog, each carrying a perk; pick one to add to the deck. At the 12-card cap the player removes one card or skips the reward.
- **Perk reward screen**: 3 cards from the player's own deck, each offered with a perk; pick one pair and the perk attaches to that card.
- **Losing or drawing** costs 1 life, shows the same card reward screen, and repeats the same combat without advancing the fight index. At 0 lives the run ends and returns to the menu.
- **Perks become real**: card power is base power plus perk modifiers, recomputed from base every time it is evaluated.
- **Run HUD**: the combat top bar shows the current combat out of 15 and the remaining lives.
- **BREAKING** relative to the completed-but-unarchived `draft-8-combat-2x2` decision that energy is display-only. The main specs still carry the original "energy/cost requirements SHALL be satisfied" contract; this change turns that contract into a concrete economy.

## Capabilities

### New Capabilities

None. Every behavior below extends a capability that already exists in `openspec/specs/`.

### Modified Capabilities

- `combat-system`: adds the turn energy pool, the cost gate on plays, the derivation of available energy from cards played this turn, and the refund on returning a card to hand.
- `card-drag-drop`: drops are rejected when the card is unaffordable; returning a card to hand refunds its cost; moving an already-paid card costs nothing.
- `ai-opponent`: the AI pays its own per-turn pool, makes repeated plays while it can afford cards, and picks by power per cost.
- `turn-flow-modal`: the energy indicator shows remaining energy, the top bar shows combat progress and lives, and the result modal routes into the run flow (win → next combat, loss or draw → retry).
- `roguelike-progression`: the fight sequence becomes 3 cycles of 5 combats; card and perk rewards, loss and draw handling, the 12-card cap with swap-or-skip, and run end are defined.
- `effect-system`: power resolution combines base power with zone, card and perk modifiers without accumulating across evaluations.

## Impact

**Affected code:**

- `Gameplay/Run/RunState.cs` - pending reward context, fight index as a combat counter, deck cap helpers
- `Gameplay/Run/ProgressionManager.cs` - rewritten cycle mapping over combat counts
- `Gameplay/Run/RewardSystem.cs` - duplicate-free card offers, perk offers from the deck, positive-perk filter
- `Gameplay/Combat/TurnManager.cs` - per-turn energy pool, available-energy derivation
- `Gameplay/Combat/PowerResolver.cs` - base + perk modifiers, non-cumulative
- `Gameplay/Cards/CardInstance.cs` - reset of per-combat state; perk modifiers
- `Gameplay/AI/AIStrategy.cs`, `Gameplay/AI/AIController.cs` - affordable multi-play loop
- `Scenes/CombatScene.cs` - energy-aware drops, refunds, result routing, HUD, card-state normalization on entry
- `Scenes/RewardScene.cs` - real reward screen (cards, perks, pick, swap-or-skip)
- `UI/Components/CardView.cs` - visible perk marker
- `Game1.cs` - register `RewardSystem` and `ProgressionManager`
- `Data/Content/ContentLoader.cs` - no new definitions required; the existing perk library is enough

**Dependencies:** no new external dependencies; MonoGame primitives and `DefaultFont` only.

**Constraints:**

- The previous change `draft-8-combat-2x2` is complete but not archived, so its deltas are not in the main specs. This change is written against the current main specs and supersedes that change's energy decision. Archiving order matters.
- The 12-card cap is kept: with 3 initial cards plus 6 draws, a 12-card deck already leaves a quarter unseen per match.
- Perk rewards offer only perks with a positive power modifier; `perk_minus1` stays defined but is never offered.
