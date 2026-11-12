# Tasks

## 1. Run State and Economy Core

- [x] 1.1 Rewrite `RunState` with `Xp`, `Level`, `Money`, `XpEarned`, `MoneyEarned`, `CombatsWon`, `Outcome` and `PendingLevelUps`; remove `LivesRemaining`, `MaxLives`, `LoseLife`, `PendingRewardKind`, `PendingReward` and `AdvanceAfterReward`; add `MinDeckSize = 6`. Verify with `dotnet build SnapClone.sln` and a grep confirming no remaining references to the removed members.
- [x] 1.2 Add the static reward table (normal 45 XP / 9 money, sub-boss 150 / 35, final boss 250 / 70) and the combat-kind mapping (5 and 10 sub-boss, 15 final boss, rest normal). Verify with a temporary harness in `%TEMP%\opencode\run-state-verify` that asserts the amounts and the three classifications.
- [x] 1.3 Implement the XP loop with `100 + 50 * (level - 1)` per level, surplus carry-over and one `LevelUpState` appended per threshold crossed. Verify in the harness that 0, 95, 100, 250 and a 1090 total produce the expected level and queue counts.
- [x] 1.4 Rewire `ProgressionManager` so a win grants the reward table, increments `CombatsWon`, queues level-ups and returns `LevelUp` or `Combat`, the final win returns `Summary`, and a loss or tie sets `Outcome = Lost` and returns `Summary`. Verify in the harness that win, loss, tie and final-combat outcomes return the expected scene name and leave the expected state.

## 2. Card Upgrades and Effective Values

- [x] 2.1 Add `CostDelta`, `PowerDelta`, `UpgradeCount`, `TemporaryCostDelta`, `EffectiveCost` and `EffectivePower` to `CardInstance`; clear only temporary and combat state in `ResetForCombat`. Verify in the harness that a new instance reports base values and that a reset clears `TemporaryCostDelta` while keeping permanent deltas and perks.
- [x] 2.2 Implement upgrade application with `-1 cost` or `+2 power`, a maximum of 3 upgrades per instance, cost floor 0 and per-instance isolation. Verify in the harness that the fourth upgrade is rejected, cost stops at 0, and upgrading one of two copies leaves the other unchanged.
- [x] 2.3 Make `PowerResolver.CalculateCardPower` start from `EffectivePower` while `WritePower` refreshes `PerkPowerBonus`, with no double count of the perk bonus. Verify with a combat where a perk and a `+2 power` upgrade are both present and the displayed power equals base + 2 + perk.
- [x] 2.4 Replace runtime `Definition.Cost` reads with `EffectiveCost` in `CardActionService` (charge), `AIStrategy`, the affordability highlight, `CardView.DrawInHand`, and the cost selectors in `Series1Effects` and `CardActionService` (Elektra, Lady Sif, Sword Master, Yondu). Verify with a build and a manual combat using an upgraded `-1 cost` card: it is playable at the reduced cost and is not selected by its old cost.

## 3. Level-Up Node

- [x] 3.1 Replace `RewardSystem` with `LevelUpSystem` that generates the card-branch offers, the upgrade candidates at deck 12, and the perk-branch offers, and that applies the chosen card, upgrade or perk. Verify in the harness that offers are distinct, respect the 2-copy limit and the 3-upgrade cap, and that no offer is generated for an unavailable branch.
- [x] 3.2 Build `LevelUpScene` with the branch choice, the offer grid, the target-card picker for perks and the card/upgrade picker for the deck-cap branch; register it as `LevelUp` in `Game1`. Verify by reaching a level-up: the scene opens after the combat, shows the correct branch for an 11-card and a 12-card deck, and applies the pick to the deck.
- [x] 3.3 Make the scene resolve one `LevelUpState` at a time, persist offers inside the pending entry so re-entry does not reroll, and route back to `LevelUp` while the queue is non-empty or to `Combat` when it empties. Verify by crossing two thresholds in one combat and resolving both level-ups in order.

## 4. Run Summary and Flow Cleanup

- [x] 4.1 Replace `GameOverScene` with `RunSummaryScene` showing outcome, level, XP earned, money earned, money unspent, combats won and the final deck with effective cost and power; add `Menu principal` and `Re-run` buttons (the latter calls `StartNewRun` and goes to `Draft`). Verify by finishing a run on a win and on a loss, and by checking that `Re-run` starts at level 1 with 0 money and a fresh draft.
- [x] 4.2 Register `Summary` in `Game1`, stop registering `Reward` and `GameOver`, and delete `RewardScene` and the obsolete `RewardSystem` paths. Verify with `dotnet build SnapClone.sln` and a grep confirming no scene name or member references the removed screens.
- [x] 4.3 Replace the combat HUD's lives counter with level and money, and remove any remaining copy or comment that mentions two lives. Verify in combat that the HUD shows the current level and money and updates after a won combat and a level-up.
- [x] 4.4 Remove the dead reward and life members from the run folder and update the affected comments. Verify with `dotnet build SnapClone.sln` and the grep from 4.2.

## 5. Integration Verification

- [x] 5.1 Build the desktop and Android targets (`dotnet build SnapClone.sln` and `scripts/build-android.ps1`) and confirm both succeed with the new scene registrations.
- [x] 5.2 Run the manual matrix: win a normal combat, a sub-boss and the final boss; lose; tie; cross two thresholds; level up at 11 cards (add), at 12 cards (upgrade), at cost 0 (cost upgrade unavailable) and at 3 upgrades (card unavailable); remove cards down to 6. Confirm each path reaches the expected node and leaves the expected state.
- [x] 5.3 Recheck the draft-to-combat flow end to end: the 8-card draft reaches combat, the deck survives the new level-up/summary routing, and a new run resets experience, level, money, deck and pending level-ups.
