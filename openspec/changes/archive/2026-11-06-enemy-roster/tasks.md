# Tasks

## 1. Enemy Content

- [x] 1.1 Add `EnemyDef`, `EnemyDeckCard`, `EnemyRegistry` and the `EnemyDef` fields for id, name, phrase, difficulty, act, boss tier and deck. Verify with `dotnet build SnapClone.sln`.
- [x] 1.2 Register the 23 enemies in `ContentLoader` with the deck table from `design.md`; validate every deck id against `CardRegistry` and every deck total as 8. Verify with a catalog harness that all 23 load and every deck is valid.
- [x] 1.3 Add the act pools and the fixed boss lookup (`GetByAct`, `GetSubBoss`, `GetFinalBoss`). Verify in the harness that each act has 6 normals and the bosses are unique.

## 2. Selection and Encounter

- [x] 2.1 Replace the temporary enemy candidates with two distinct enemies from the current act pool and store the chosen id in `pendingCombat`. Verify by reaching enemy selection in act 1 and act 2.
- [x] 2.2 Route sub-bosses and the final boss to `SubBossIntro` with the fixed definition. Verify at combat 15, 30, 45, sub-boss 4 and the final boss.
- [x] 2.3 Build the AI deck from the selected enemy definition and the encounter seed; remove the random rival deck path. Verify in the harness that the same enemy and seed produce the same initial deck and that the deck has 8 cards.

## 3. Difficulty and Discovery

- [x] 3.1 Pass the enemy difficulty into `AIStrategy` and implement the 1-2, 3 and 4-5 tiers while preserving the self-harm filter and energy/zone limits. Verify with a scripted board for each tier.
- [x] 3.2 Add card discovery when the AI plays a card and merge it into the run discovery set. Verify in the harness that an enemy play discovers the card once and that a bundle preview does not discover it.

## 4. Presentation

- [x] 4.1 Update `EnemySelectScene` to show the real name, phrase, difficulty and deck preview. Verify at the smallest supported viewport.
- [x] 4.2 Update `SubBossIntroScene` and the rival avatar for boss framing and the enemy name. Verify at act 1 sub-boss, sub-boss 4 and the final boss.

## 5. Integration Verification

- [x] 5.1 Run the catalog, selection, deck and difficulty harnesses together and confirm no card id or scene route is missing.
- [x] 5.2 Manual pass: select a normal enemy in each act, win a normal combat, reach a sub-boss, reach the final boss, lose to an enemy and confirm the rematch uses the same enemy.
- [x] 5.3 Confirm the Android and desktop builds still compile with the new content and that the APK is regenerated.
