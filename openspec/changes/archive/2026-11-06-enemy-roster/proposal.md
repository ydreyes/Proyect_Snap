# Proposal

## Why

The rival is still a random pile of eight cards drawn from the player's own card pool, with no name, identity or difficulty. Enemy selection is a placeholder and the combat HUD always says `RIVAL`. The act graph now has a real `EnemySelect` node and a fixed sub-boss slot, so the roster can replace both the random deck and the anonymous opponent.

## What Changes

- **Add 23 enemy definitions**: 18 normal enemies (6 per act), 4 sub-bosses (one per act plus the unique sub-boss 4) and 1 final boss.
- **Give every enemy a fixed 8-card deck**, a flavor phrase, a difficulty from 1 to 5 and an act pool. Decks use card definitions from the 59-card catalog, with no tokens and at most 2 copies of a card.
- **Replace the temporary enemy selection** with a 1-of-2 choice from the current act's normal pool. Each candidate shows name, phrase, difficulty and a deck preview. Sub-bosses and the final boss are fixed and open `SubBossIntro` instead of selection.
- **Use the selected enemy deck for the AI**. The AI no longer builds a random deck from the player's pool.
- **Use difficulty as an AI behavior input**: lower difficulties keep the current safe heuristic, higher difficulties prefer raw power, zone spread and the player's weakest zone.
- **Discover cards played by the enemy**: when the rival plays a card, it is added to the run's discovered card set, with the same no-duplicate rule as the other discovery sources.
- **Persist the encounter identity**: `pendingCombat` already stores an enemy identity; this change makes it the `EnemyDef` id and deck seed so a resumed combat rebuilds the same encounter.
- **Offer a rematch after a defeat**: the summary can repeat the same enemy with the next attempt and a new seed, without running enemy selection again.
- **Show the enemy identity** in the rival avatar and the sub-boss intro.

## Capabilities

### New Capabilities

- `enemy-roster`: enemy definitions, fixed decks, act pools, 1-of-2 selection, fixed bosses, difficulty behavior, encounter persistence and card discovery from enemy plays.

### Modified Capabilities

- `content-data`: the content library includes the 23 enemy definitions with act, role, difficulty and deck identity.
- `ai-opponent`: the AI deck comes from the selected enemy definition and difficulty influences its play choice.
- `ui-responsive`: the rival avatar and the encounter screens show the enemy identity instead of a generic `RIVAL`.

## Impact

- Content: new `EnemyDef`/`EnemyRegistry` and the 23 definitions in `ContentLoader`.
- Run and combat: `RunState.PendingCombat`, `RunDirector`, `CombatScene` rival deck construction, `AIStrategy`.
- Screens: `EnemySelectScene`, `SubBossIntroScene`, `CombatScene` avatar.
- Discovery: the enemy-play hook feeds `RunState.DiscoveredCardIds` and the profile flush from `run-save-and-pause`.
- Verification: a roster catalog check, a selection test per act, a deck-construction test, a difficulty behavior test and a manual pass through a normal enemy, a sub-boss and the final boss.
