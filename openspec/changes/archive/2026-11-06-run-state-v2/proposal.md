# Proposal

## Why

The run is currently a 15-combat line with two lives and card/perk rewards after wins and losses. That makes every defeat a free retry and turns deck growth into a slot-machine of predefined pairs, so skill and money never enter the decision. The roguelike redesign needs a single run-state authority that the later changes (node graph, save/pause, shop, meta store) can read and write without re-migrating the run schema.

This change rebuilds that authority first: experience, level, money, upgrades and a real end-of-run summary. It deliberately keeps the existing 15-combat loop so the state can land and be verified before the node graph replaces the sequence.

## What Changes

- **Remove the life system. BREAKING** for the current two-life rule, the post-loss reward screen and the "repeat the same combat" loop. A loss or tie ends the run; the extra-life item of a later change repeats the same enemy instead of restoring a run-level life.
- **Replace card/perk rewards after combat with run currency**: every won combat grants experience and money according to its node type (normal, sub-boss, final boss, event). The player no longer picks a card or a perk directly after a fight.
- **Add run experience and levels**: XP thresholds `100 + 50*(level-1)`. Levels are gained at the end of a combat, resolved as a queue of level-ups so two thresholds crossed in one fight are both awarded.
- **Add the level-up node** as a safe decision point with two branches:
  - Card branch: add one `owned` card not yet in the deck when the deck has fewer than 12 cards; at 12 cards, apply an upgrade to a card in the deck instead.
  - Perk branch: attach one `owned` perk with fewer than 2 copies to a card in the deck.
  - Upgrades: each card tracks `CostDelta`, `PowerDelta` and `UpgradeCount`; a card may choose `-1 cost` or `+2 power`, up to 3 upgrades per card.
- **Add run money**: money is earned per combat, lives only inside the run, and the unspent balance dies with the run. This change stores and displays the balance; spending arrives with the in-game shop.
- **Add effective cost and power**: instances expose `EffectiveCost` and `EffectivePower` derived from the definition plus upgrade deltas plus temporary modifiers, and every cost check, play validation and power calculation reads those instead of the definition's raw values.
- **Change deck size rules**: minimum 6 and maximum 12 for the active run deck, measured at the end of the draft and after every add/remove.
- **Add a basic end-of-run summary**: outcome, level reached, money earned, combat count, final deck and the Main Menu / Re-run actions. The global-points breakdown arrives with the meta store change.
- **Keep the current 15-combat sequence** as the temporary implementation of the run progression; the node graph change replaces it with 47 combats across acts.

## Capabilities

### New Capabilities

- `run-economy`: run experience, level thresholds and level-up queue; money balance and per-combat rewards; upgrade types, caps and costs; effective cost/power contract for play validation and scoring.

### Modified Capabilities

- `roguelike-progression`: lives are removed; defeat and tie end the run; the post-combat card/perk reward is replaced by experience and money; deck size becomes 6-12; run state tracks level, XP, money and upgrades; a basic end-of-run summary is shown.
- `card-mechanics`: card instances carry upgrade state (`CostDelta`, `PowerDelta`, `UpgradeCount`), effective cost and effective power, and the upgrade caps; combat reset preserves upgrades.
- `combat-system`: energy charges, affordability checks, refunds and the hand/board cost display use the effective cost instead of the definition's base cost.

## Impact

- Run state and rewards: `SnapClone/Gameplay/Run/RunState.cs`, `RewardSystem.cs`, `ProgressionManager.cs`, `SnapClone/Scenes/RewardScene.cs`, `GameOverScene.cs`, `CombatScene.cs`, `MenuScene.cs`.
- Cards and combat: `SnapClone/Gameplay/Cards/CardInstance.cs`, `SnapClone/Gameplay/Combat/PowerResolver.cs`, `SnapClone/Gameplay/Actions/CardActionService.cs`, `SnapClone/Gameplay/Combat/TurnManager.cs`, `SnapClone/UI/Components/CardView.cs`.
- New presentation: a level-up scene and a run-summary scene replacing the current reward/game-over flow.
- Data: run XP, money and upgrade tables in content data; no new cards, perks or enemies in this change.
- Verified with `dotnet build SnapClone.sln` plus a manual pass: win, lose, tie, level-up at 11 and 12 cards, upgrade at the cap, and deck minimum after removals.
