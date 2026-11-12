# Tasks

## 1. Storage and Serialization

- [x] 1.1 Add `IStorage`, `FileStorage` and `InMemoryStorage` with platform data-directory resolution and no direct `System.IO` calls in the core. Verify with `dotnet build SnapClone.sln` and a harness that writes and reads a key on both storage implementations.
- [x] 1.2 Add `ProfileDto`, `RunDto`, `schemaVersion` and the domain mappers, including card snapshots by definition id. Verify in the harness that a populated `RunState` round-trips deck, upgrades, perks, XP, money and graph position.
- [x] 1.3 Implement the atomic writer with `.tmp`, `.bak`, replace-after-success and load fallback to the backup. Verify in the harness that a failed write leaves the previous document and that a corrupt main file loads the backup.

## 2. Run and Profile Checkpoints

- [x] 2.1 Write `run.json` at `Draft`, `EnemySelect`, `LevelUp`, `Shop`, `Event`, `ActTransition` and `SubBossIntro`, and never during `Combat`. Verify with a scene-by-scene harness that only safe nodes produce a write.
- [x] 2.2 Add the run status (`Active`/`Ended`) and the deletion paths for `Menu principal`, `Re-run` and `Abandonar run`. Verify in the harness that the file exists after a checkpoint and is gone after each exit path.
- [x] 2.3 Add discovery sets to `RunState` and the set-union flush into `ProfileDto` at safe checkpoints and on close. Verify in the harness that duplicates are not added and that a failed profile write keeps the run checkpoint.
- [x] 2.4 Add `lastAwardedRunId` and the reserved profile fields with empty defaults. Verify with a first-run profile that the file is written and loaded without a schema migration.

## 3. Combat and Draft Resume

- [x] 3.1 Create `pendingCombat` before entering combat and clear it after applying the result. Verify in the harness that the snapshot has enemy, attempt, seed and pre-combat state, and that clearing it prevents a second reward.
- [x] 3.2 Resume an interrupted combat with the same enemy and attempt but a new seed. Verify by writing a snapshot, discarding the live board and rebuilding the combat from the checkpoint.
- [x] 3.3 Replace the in-memory draft with a serializable `DraftState` and persist phase, step, groups, offers, picks and copy counts after every pick. Verify in the harness that a resumed draft produces the same offers and does not reroll.

## 4. Pause, Back and Menu

- [x] 4.1 Add `PauseOverlay` with the safe-node and combat button sets, including the save error state and the double-confirmation abandon. Verify by opening it at a safe node and in combat.
- [x] 4.2 Add `BackService`, route desktop Escape and Android back to it, and make the active scene consume the request. Verify desktop Escape opens/closes the pause and no longer exits during a run.
- [x] 4.3 Add `PauseTurnTimer`/`ResumeTurnTimer` and skip the scene update while paused. Verify that the timer and AI do not advance behind the modal.
- [x] 4.4 Rework `MenuScene` for `Continuar`, `Ver resumen`, `Nueva run` confirmation and the empty state; wire the summary deletion paths. Verify each menu state from a real run file.

## 5. Android Lifecycle

- [x] 5.1 Override `OnBackPressed` in `MainActivity` to forward to the game back service and override `OnPause` to write a safe checkpoint and flush discovery. Verify with a device or emulator: back opens the pause, background/resume keeps the scene, and no corrupted run file remains.
- [x] 5.2 Verify the desktop build still runs without the Android workload and that the Android build produces the APK with the new persistence code.

## 6. Integration Verification

- [x] 6.1 Run the storage harness for atomic write, backup recovery, corrupt file, discovery flush and profile/run independence.
- [x] 6.2 Run the resume harness for draft mid-round and mid-combat interruption, including the reward-once check.
- [x] 6.3 Manual pass on desktop: save and exit from draft, level-up and act transition; continue; abandon; finish a run and open the summary from the menu.
- [x] 6.4 Manual pass on Android: back gesture, background/resume, continue after process death and summary back navigation.
