# Design

## Context

See `proposal.md` - Why.

The current run is in memory only:

- `RunState` holds the live deck, XP, level, money, level-up queue and (after `run-node-graph`) the act, node and pending-combat fields.
- `SceneManager` keeps one active scene and calls `OnEnter` on every entry.
- `Game1.Update` exits on Escape; `MainActivity` does not override back; there is no pause.
- `TimeService` has a turn timer that can only start or stop, not pause and resume.
- `DraftScene.OnEnter` always resets the draft, so it cannot resume.
- `MenuScene` only offers new run and quit.

The graph change made every run position explicit, which is the prerequisite for a checkpoint. This design adds the file layer, the checkpoint and the pause/back flow without changing combat rules.

## Goals / Non-Goals

**Goals:**

- Persist a run at safe nodes and recover it after a process restart.
- Resume a draft mid-round and a combat from its pre-combat snapshot.
- Give every run scene a consistent pause and back behavior.
- Keep the storage independent of MonoGame so the core and the Android head share it.

**Non-Goals:**

- Shop, event or collection state beyond what the checkpoint already needs to serialize.
- Global points awarding; only the `lastAwardedRunId` field is reserved.
- Cloud saves, multiple save slots or account sync.
- Rewriting the combat board to be serializable; combat is intentionally not saved.

## Decisions

### 1. Persistence sits behind an injected storage interface

```
IStorage
  bool Exists(string key)
  string Read(string key)
  void Write(string key, string content)
  void Replace(string key, string content)
  void Delete(string key)
```

`FileStorage` resolves a platform data directory (desktop user data, Android app data). `InMemoryStorage` supports the tests. The core never calls `System.IO` directly, so the Android project compiles the same code and the tests do not touch the real profile.

Why an interface instead of a static helper: Android and desktop have different writable directories, and the save tests need deterministic failure injection for the "save failed" path.

### 2. Two DTO roots, one schema version each

`ProfileDto` and `RunDto` are plain serializable objects with `schemaVersion`. The domain `RunState` is mapped to `RunDto` at write time and back at load time.

`RunDto` contains:

- status (`Active`/`Ended`), act, combat index, total combats, shop/event chance, final-act step and pending node;
- XP, level, money, earned totals, combats won and outcome;
- the deck as card snapshots (definition id, upgrade deltas, upgrade count, attached perks, bonus power);
- pending level-ups with their generated offers;
- the draft block when the pending node is `Draft`;
- `pendingCombat` with enemy identity, attempt, seed and the pre-combat fields needed to rebuild the encounter;
- run modifiers, extra-life charges and the current shop/event state when those changes land.

`ProfileDto` contains points, owned/discovered ids, perk copies, item counts, `lastAwardedRunId` and store rotation. Fields for later changes are written as empty defaults so the schema does not need a version bump for every change.

Card instances are serialized by value, not by reference to the registry. Loading resolves the definition id from `CardRegistry`; an unknown id is skipped with a warning rather than failing the whole run.

### 3. Atomic write and backup

```
write temp key
replace target with temp
copy previous target to .bak before replace
```

The exact order is: read the previous target, write the new content to `<key>.tmp`, copy the previous target to `<key>.bak`, then replace the target with the temp file. If any step fails, the target still contains the previous document. On load, try the target, then the `.bak`; if both fail, start empty and leave the files for inspection.

The profile and run files use the same writer. The run write never touches the profile; the profile write never touches the run.

### 4. Safe-node checkpoints

The safe nodes are `Draft`, `EnemySelect`, `LevelUp`, `Shop`, `Event`, `ActTransition` and `SubBossIntro`. The checkpoint is written when the director sets a safe `PendingNode` and after a safe node resolves its choice.

Combat is excluded because the board, hand, piles and reveal state are not serializable and a mid-combat save would create a second source of truth. The pre-combat snapshot is enough to replay the encounter.

Why not save after every scene `OnEnter`: `OnEnter` runs on a scene re-entry that may be a resume, and writing there could overwrite a newer checkpoint with an older in-memory state. The director is the only writer.

### 5. Pending combat resume

`pendingCombat` is created before the combat scene is entered. It stores the enemy identity, attempt, pre-combat run state and a seed. The combat builds the rival from the enemy identity and the seed.

If the app closes mid-combat, the run file still contains the pre-combat snapshot. `Continuar` opens `Combat` with the same enemy and attempt but a new seed, so the board is new and the composition is the same.

The result is applied only from the snapshot. The director clears the snapshot after applying rewards and before opening the result modal. A second application is rejected because the snapshot is gone.

### 6. Draft checkpoint

The draft state is a DTO with phase, step index, groups, current offers, picks and copy counts. `DraftScene.OnEnter` checks for a saved draft first; only a new run resets it. Every pick writes the DTO.

The current `DraftSystem` keeps groups only in memory; the save change adds a serializable `DraftState` so a resumed draft reproduces the same offers.

### 7. Discovery flush

`RunState` gains `DiscoveredCardIds`, `DiscoveredPerkIds` and `DiscoveredEnemyIds`. Card discovery is added by the draft, the in-game shop, level-up offers and enemy plays. The bundle preview does not discover cards.

The flush merges the run sets into the profile at every safe checkpoint and on close. The merge is a set union; a failed profile write is reported but does not discard the run checkpoint.

### 8. Pause overlay and back service

`PauseOverlay` is a component, not a scene: `SceneManager` keeps one active scene and an overlay is simpler than a scene stack. Each run scene owns an overlay instance and passes a `PauseContext` describing whether the node is safe.

`BackService` holds a one-frame `Request` flag. `Game1.Update` calls `BackService.Request()` on Escape; `MainActivity.OnBackPressed` calls `Game1.RequestBack()`. The active scene consumes the flag. Menu exits; summary returns to the menu; run scenes toggle the overlay.

`TimeService` gains `PauseTurnTimer()` and `ResumeTurnTimer()` that preserve `TurnTimeRemaining`. The scene skips its update while the overlay is open, so the AI and animations stop too.

### 9. Menu and summary flow

`MenuScene.OnEnter` reads the run status:

- `Active`: `Continuar` resumes at `PendingNode`.
- `Ended`: `Ver resumen` opens the summary.
- none: only `Nueva run`.

`Nueva run` with an active run asks for confirmation. `Menu principal` and `Re-run` from the summary delete `run.json`. `Abandonar run` deletes it after a double confirmation.

### 10. Save failure

A failed `Guardar y salir` keeps the overlay open and shows the error. The previous checkpoint is untouched because the atomic writer only replaces after a successful temp write. The player can retry or abandon; the menu is never reached with a half-written file.

## Risks / Trade-offs

- **[Serializing card instances by value can drift from the domain]** → Version the DTO, resolve definitions by id and keep a single mapper used by both save and load.
- **[Saving on close is not guaranteed on Android]** → Write at every safe node and in `OnPause`; the checkpoint is the normal path, not the last-resort path.
- **[A resumed combat can feel like a free retry]** → The attempt number is preserved and rewards are granted only once; a new seed prevents replaying the exact same board.
- **[Pause overlay and scene update can double-advance the timer]** → The scene returns before `TimeService.Update` while paused and the timer has an explicit pause/resume API.
- **[Profile writes during discovery flush can fail]** → Report the failure, keep the run checkpoint and retry on the next safe node.

## Migration Plan

1. Add `IStorage`, `FileStorage`, `InMemoryStorage`, DTOs and the atomic writer.
2. Map `RunState`/`Profile` to DTOs and back, including card snapshots and discovery.
3. Write checkpoints at the safe nodes and add the draft state.
4. Add `pendingCombat` creation and resume.
5. Add the pause overlay, back service, timer pause and menu flow.
6. Wire Android back/lifecycle and verify with the storage harness plus a manual desktop/Android pass.

Rollback is a revert. The files are additive and are ignored when absent; deleting them starts a fresh profile.
