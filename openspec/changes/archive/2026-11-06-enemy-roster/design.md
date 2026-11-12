# Design

## Context

See `proposal.md` - Why. The current rival is built in `CombatScene.SetupDecks` from `CardRegistry.GetDraftable()` with no identity. `AIStrategy` chooses by power per energy and zone spread. The `EnemySelect` node from `run-node-graph` currently offers two temporary rival candidates.

This change replaces the candidate source, the rival deck source and the AI input without changing the action/effect engine.

## Goals / Non-Goals

**Goals:**

- Provide 23 stable enemy definitions with fixed 8-card decks.
- Make 1-of-2 selection real and bounded by the act.
- Make boss encounters fixed and distinct.
- Use difficulty as a visible and testable AI parameter.
- Feed enemy plays into the existing discovery set.

**Non-Goals:**

- New cards, tokens or effects for enemies.
- Enemy collection or bestiary UI.
- Per-enemy combat music, art or animations.
- A new AI planner; difficulty only reweights the existing decision.

## Decisions

### 1. `EnemyDef` describes an encounter, not a card

```
EnemyDef
  Id
  Name
  Phrase
  Difficulty (1..5)
  ActIndex (1..3, 4 for final)
  IsBoss
  BossTier (None, Act, Final)
  Deck: list of { CardId, Count }
```

The deck is a list of card ids and counts, not card instances. The combat builds fresh instances from the registry each attempt, so the same enemy can be replayed with a new seed without sharing mutated state.

### 2. Enemy registry and content

`EnemyRegistry` provides `GetAll`, `GetByAct(act)`, `GetSubBoss(act)` and `GetFinalBoss`. `ContentLoader.LoadEnemies` registers the 23 definitions. The registry validates that every deck card id resolves and that the total count is 8.

The full roster:

| Act | Enemy | Diff | Deck |
|---|---|---:|---|
| 1 | Hand Ninjas | 1 | Blade, Elektra, Misty Knight, Shocker, Hawkeye, Quicksilver, Cyclops, Star-Lord |
| 1 | Hydra Agents | 1 | Blade, Shocker, Cyclops, Misty Knight, Punisher, Hawkeye, Sentinel, Quicksilver |
| 1 | AIM Scientists | 2 | Forge, America Chavez, Medusa, Sentinel, Hawkeye, Star-Lord, Iron Fist, Quicksilver |
| 1 | Ultron Drones | 2 | Mister Sinister, Squirrel Girl, Sentinel, Shocker, Misty Knight, Cyclops, Quicksilver, Blade |
| 1 | Kree Sentries | 2 | The Thing, Cyclops, Punisher, Shocker, Sentinel, Medusa, Star-Lord, Quicksilver |
| 1 | Skrull Infiltrators | 3 | White Queen, Mantis, Morph, Cable, Sentinel, Medusa, Hawkeye, Quicksilver |
| 2 | Brotherhood | 2 | Kraven, Multiple Man, Doctor Strange, Iron Fist, Heimdall, Elektra, Shocker, Quicksilver |
| 2 | Sentinels | 2 | Sentinel, Iron Man, Punisher, Captain America, Klaw, Cyclops, Shocker, Quicksilver |
| 2 | Dark Elves | 2 | Spider-Woman, Carnage, Deathlok, Elektra, Blade, Shocker, Misty Knight, Quicksilver |
| 2 | Frost Giants | 3 | Hulk, The Thing, Abomination, Heimdall, Apocalypse, Shocker, Cyclops, Quicksilver |
| 2 | Chitauri | 2 | Squirrel Girl, Mister Sinister, Moon Girl, Sentinel, Shocker, Misty Knight, Blade, Quicksilver |
| 2 | Black Order Scouts | 3 | Punisher, Captain America, Angela, Bishop, Lizard, Star-Lord, Shocker, Quicksilver |
| 3 | Kree Accusers | 3 | Iron Man, Klaw, Onslaught, Captain America, Punisher, Cyclops, Shocker, Quicksilver |
| 3 | Shi'ar Guard | 3 | Devil Dinosaur, Strong Guy, Namor, Lizard, Captain America, Iron Man, Shocker, Quicksilver |
| 3 | Brood | 3 | Squirrel Girl, Mister Sinister, Moon Girl, Carnage, Deathlok, Nova, Wolverine, Quicksilver |
| 3 | Symbiotes | 3 | Carnage, Deathlok, Spider-Woman, Wolverine, Nova, Angel, Elektra, Blade |
| 3 | Doombots | 3 | Iron Man, Onslaught, Klaw, Professor X, Cosmo, Sentinel, Shocker, Quicksilver |
| 3 | Heralds of Galactus | 3 | Hulk, Heimdall, Apocalypse, Devil Dinosaur, Iron Man, Onslaught, Shocker, Quicksilver |
| 1 | Red Skull | 3 | Hulk, The Thing, Abomination, Punisher, Captain America, Cyclops, Shocker, Quicksilver |
| 2 | Magneto | 4 | Iron Man, Klaw, Onslaught, Doctor Strange, Heimdall, Multiple Man, Shocker, Quicksilver |
| 3 | Doctor Doom | 4 | Iron Man, Onslaught, Klaw, Professor X, Cosmo, Devil Dinosaur, Shocker, Quicksilver |
| 4 | Thanos | 5 | Hulk, Apocalypse, Heimdall, Onslaught, Iron Man, Devil Dinosaur, Shocker, Quicksilver |
| 4 | Galactus | 5 | Hulk, Apocalypse, Heimdall, Onslaught, Iron Man, Devil Dinosaur, The Thing, Abomination |

### 3. Selection uses the act pool

`RunDirector` or `EnemySelectScene` asks `EnemyRegistry.GetByAct(currentAct)` for the two candidates. The two ids are distinct; when the pool has only one eligible enemy, the same candidate cannot be offered twice and the node waits for the next act. No immediate-repeat rule is needed because the act pool has six entries.

Boss nodes bypass selection: `GetSubBoss(act)` for combats 15/30/45 and `GetFinalBoss()` for the last fight.

### 4. Rival deck construction

`CombatScene.SetupDecks` stops calling `GetDraftable()` for the rival. It reads `PendingCombat.EnemyId`, resolves the definition, instantiates the declared counts and shuffles with `PendingCombat.Seed`.

The player deck still comes from `RunState.Deck`. The AI hand and draw rules are unchanged.

### 5. Difficulty

`AIStrategy` receives the enemy difficulty. The decision order is:

| Difficulty | Card choice | Zone choice |
|---:|---|---|
| 1-2 | current safe power-per-energy | current fewest-cards |
| 3 | highest current power among affordable | current fewest-cards |
| 4 | highest current power | player's weakest zone |
| 5 | highest current power, then player's weakest zone | player's weakest zone |

The self-harm filter stays first for every difficulty. This keeps the AI beatable and makes the selection label meaningful.

### 6. Discovery from enemy plays

`CardActionService` already receives the `RunState`. After a successful AI play, it adds the card id to `RunState.DiscoveredCardIds`. The save change flushes that set at the next safe checkpoint. The discovery is idempotent by definition id.

### 7. Presentation

- `EnemySelectScene` uses `EnemyDef` name, phrase, difficulty and deck preview. The temporary candidate contract becomes the real one.
- `SubBossIntroScene` shows the boss name and phrase.
- `CombatScene` rival avatar shows the enemy name or a short initial; the generic `RIVAL` label is removed.

### 8. Defeat rematch

A lost combat leaves a `RematchCombat` snapshot in the run: the same enemy identity, phrase, difficulty, attempt and pre-combat snapshot. `PendingCombat` is still cleared after the result is applied, so the existing "apply once" rule is unchanged.

`RunSummaryScene` offers `REINTENTAR` only for a defeat with a rematch snapshot. The action moves the snapshot back to `PendingCombat`, increments `Attempt`, generates a new `Seed`, sets the outcome back to `Active` and opens `Combat`. Enemy selection is not repeated, and the pre-combat snapshot is unchanged, so no reward is applied twice. A win clears the rematch; another loss stores the next attempt. The same path can later be reused by the extra-life item from `ingame-shop-and-events`.

## Risks / Trade-offs

- **[Fixed decks make encounters repetitive]** → The 1-of-2 selection and the six enemies per act provide variety; balance can be tuned per deck without changing the system.
- **[Difficulty 4-5 may be too strong]** → Difficulty only reweights choices; it does not grant resources or hidden cards.
- **[Deck ids can drift from the card registry]** → Validate at load and fail the content load with a clear error.
- **[Discovery from enemy plays leaks the enemy deck]** → That is intended: seeing a card in play discovers it, exactly like seeing it in the shop.

## Migration Plan

1. Add `EnemyDef`, `EnemyDeckCard`, `EnemyRegistry` and `ContentLoader.LoadEnemies`.
2. Add the 23 definitions and validate every deck id.
3. Make `EnemySelect` use the act pool and boss nodes use the fixed boss.
4. Build the rival deck from the enemy definition and seed.
5. Pass difficulty into `AIStrategy` and implement the four behavior tiers.
6. Add discovery on AI play and update the rival avatar.
7. Verify with a catalog test, a selection test per act, a deck test, a difficulty test and a manual boss pass.

Rollback is a revert; the enemy registry is additive and no saved format changes are required beyond the `pendingCombat.enemyId` already defined by the graph change.
