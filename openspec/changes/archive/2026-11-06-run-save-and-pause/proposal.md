# Proposal

## Why

A run currently exists only in memory. Closing the app, pressing Escape or losing the process destroys the run, and there is no pause or continue path. The node graph is now stable enough to define a checkpoint: every safe node can be written, every combat can be resumed from its snapshot, and the profile can hold the first persistent fields.

## What Changes

- **Add two persistent files**: `profile.json` for meta state and `run.json` for the active or last-finished run. Both carry `schemaVersion`, are written atomically through a temporary file plus replace, and keep a `.bak` fallback.
- **Save only at safe nodes**: `Draft`, `EnemySelect`, `LevelUp`, `Shop`, `Event`, `ActTransition` and `SubBossIntro`. A combat is never written while it is being played.
- **Resume a combat safely**: `pendingCombat` stores the enemy, attempt, pre-combat snapshot and seed. If the app dies mid-combat, `Continuar` starts a new combat against the same enemy with a new seed and the same composition.
- **Persist the draft mid-round**: each pick writes the draft phase, step, groups, current offers, picks and copy counts. Closing the app resumes the same round with the same offers.
- **Add the pause overlay**: `Reanudar`, `Guardar y salir` and `Abandonar run` at safe nodes; `Reanudar`, `Salir al menú` and `Abandonar run` in combat. Guardar is not shown in combat.
- **Add a back service**: desktop Escape and Android back open or close the pause overlay instead of exiting the app. In the menu, back exits; in the summary, back returns to the menu.
- **Freeze the clock while paused**: the turn timer and the scene update stop, so the AI and timer do not advance behind the modal.
- **Redesign the main menu**: `Continuar` when `run.json.status` is `Active`, `Ver resumen` when it is `Ended`, both in the same slot; `Nueva run` asks for confirmation while a run is active.
- **Delete the checkpoint on exit**: `Menu principal` or `Re-run` after the summary deletes `run.json`; abandoning a run deletes it after a double confirmation.
- **Add discovery accumulation**: cards, perks and enemies seen in run context are accumulated in `RunState` and flushed to `profile.json` at safe nodes and on close. `profile.lastAwardedRunId` is reserved for the meta-points change.
- **Add a storage boundary**: persistence uses an injected storage interface so the core remains cross-platform and testable. Desktop uses the user data folder; Android uses the app data directory.

## Capabilities

### New Capabilities

- `run-persistence`: profile/run files, schema version, atomic writes, backup recovery, safe-node checkpoints, pending combat resume, draft DTO, run status and discovery flush.
- `pause-menu`: pause overlay, adaptive button sets, back routing, clock freeze, menu continue/summary and new-run confirmation.

### Modified Capabilities

- `roguelike-progression`: run state tracks the checkpoint and status; the summary deletes the finished run on exit.
- `draft-system`: the draft is a safe node, each pick is persisted and the same round is resumed.
- `android-platform`: Android back and lifecycle events route to the back service and the checkpoint instead of exiting.
- `ui-responsive`: pause overlay and the redesigned main menu keep the portrait layout.

## Impact

- Persistence: new `SnapClone/Persistence/` layer with `IStorage`, `JsonProfileStore`, `JsonRunStore`, DTOs and migration.
- Run state: `RunState`, `ProgressionManager`, `RunDirector`, `LevelUpState`, `DraftSystem`.
- Scenes: `MenuScene`, `CombatScene`, `DraftScene`, `LevelUpScene`, `RunSummaryScene`, plus a shared `PauseOverlay`.
- Platform: `SnapClone/Game1.cs`, `SnapClone.Android/MainActivity.cs`.
- Time: `TimeService`.
- Verification: build plus a storage harness for atomic write/backup/corruption, a draft resume test, a mid-combat resume test and a manual desktop/Android pause pass.
