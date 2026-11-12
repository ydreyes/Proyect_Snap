# Design

## Context

See `proposal.md` for motivation. Current state that shapes the approach:

- One project, `SnapClone/SnapClone.csproj`: `WinExe`, `net9.0`, `MonoGame.Framework.DesktopGL` 3.8.6, one `MonoGameContentReference` for `Content/Content.mgcb`. 66 C# files under the project root.
- `Game1` is the only place touching desktop window APIs: a `user32!SystemParametersInfo` work-area query, `PreferredBackBuffer*` sizing in the constructor, `Window.AllowUserResizing` and `Window.ClientSizeChanged` in `Initialize`, and an `Escape`/gamepad-Back exit check in `Update`.
- All scene and UI input goes through `InputService`: `IsLeftMousePressed/Down/Released` and `MousePosition`. No scene reads `Mouse`/`Keyboard` directly except an avatar-only check in `CombatScene` and `Game1`'s exit check.
- `LayoutManager` computes every region with `Percent(x, y, w, h)` over the full `GraphicsDevice.Viewport`, using a `720 x 1280` reference for scale.
- `MonoGame.Framework.Android` 3.8.6 ships only `lib/net8.0-android34.0`; its `build/MonoGame.Framework.Android.targets` only sets `MonoGamePlatform=Android`, which the content task uses as `/platform:Android`.
- Machine state: .NET SDK 10.0.204 (default) plus 9.0.313/9.0.320, Android SDK at `%LOCALAPPDATA%\Android\Sdk` with platforms `android-36`/`android-36.1` and build-tools `36.0.0`, no API 35 platform, no Android workload for .NET 9, no JDK 17/21 (only IntelliJ JBR 25), no device or AVD.

## Goals / Non-Goals

**Goals:**

- One source tree, two build outputs: desktop executable and Android APK, with no duplicated game code or content.
- A debug APK installable by `adb install`, produced by a script with no IDE.
- Touch behaves exactly like the mouse path, so scenes need no input changes.
- Layout stays inside the device safe area.

**Non-Goals:**

- Release signing, Play Store packaging, AAB, versioning policy.
- iOS, emulator-only builds, landscape, tablets or multi-window.
- Multi-touch gestures (pinch, two-finger drag); only the first touch drives the pointer.
- Performance work, renderer changes, new art or audio assets.

## Decisions

### Separate Android head that links the existing sources

`SnapClone.Android/SnapClone.Android.csproj` compiles `..\SnapClone\**\*.cs` directly (excluding `Program.cs`, `bin`, `obj`) and links `..\SnapClone\Content\Content.mgcb` as `Content\Content.mgcb`. Both heads therefore compile the same files with their own TFM; there is no shared assembly to keep in sync and no file moves.

- Alternatives rejected: multi-targeting `SnapClone.csproj` (would make `dotnet build SnapClone.sln` require the Android workload and JDK, breaking the current PC loop) and a Core/Desktop/Android split (66 file moves and namespace churn on top of a just-completed card set, with no behavior gain).
- Trade-off: the Android compile is the only thing that proves the guarded code paths are portable; any unguarded desktop API fails the Android build.

### Target `net9.0-android` with a repo SDK pin

`global.json` pins the 9.0.3xx feature band (`rollForward: latestFeature`) so both the desktop and Android builds use SDK 9, which is the band that carries the Android 35 workload manifest. Desktop `net9.0` keeps building with it, so the PC workflow is unaffected in practice.

- Rejected: `net10.0-android` (only SDK band with packs already installed, but MonoGame 3.8.6 predates .NET 10 support and would need an unproven runtime combination).
- Rejected: trailing the machine default SDK instead of pinning (reproducibility would depend on install order).

### Toolchain spike before any game code changes

Install Microsoft OpenJDK 17 (Android SDK 35 targets JDK 17; JBR 25 is not accepted), install the .NET 9 Android workload, and build a throwaway `net9.0-android` project that references `MonoGame.Framework.Android` 3.8.6 and hosts an empty `AndroidGameActivity`. Only after that APK installs and shows a cleared screen does the head get the real sources. If `net8.0-android34.0` assemblies fail under `net9.0-android`, fall back to a `net8.0-android34.0` head and record the decision.

- Rejected: writing the full head first and diagnosing toolchain failures through the game (too many variables in one failure).

**Spike outcome (2026-11-04):** `net9.0-android` is confirmed. A throwaway `spike/android` project referencing `MonoGame.Framework.Android` 3.8.6 and hosting an `AndroidGameActivity` built successfully with SDK 9.0.320, the Android workload 35.0.78 and Microsoft OpenJDK 17.0.21, producing `bin/Debug/net9.0-android/<ApplicationId>-Signed.apk`. Android SDK platform 35 had to be installed first (`sdkmanager "platforms;android-35"`); only `android-36`/`android-36.1` were present. The build logs a benign `XA0033` warning because the JDK reports a four-part version (`17.0.21.0.2`); compilation and packaging succeed. No device was connected, so the launch check remains pending.

### Platform behavior selected at runtime, not with `#if`

Shared code branches on `OperatingSystem.IsAndroid()`. `#if ANDROID` would tie the source to the build target and make the desktop head's compiler skip code that should still be reviewed.

- `Game1` constructor: on Android skip `GetWorkArea` and `PreferredBackBuffer*` entirely; the platform owns surface size.
- `Game1.Initialize`: on Android skip `AllowUserResizing` and the `ClientSizeChanged` subscription.
- `Game1.Update`: on Android skip the `Escape`/gamepad-Back `Exit()` path; back navigation is the activity's job.

### Touch as the pointer source inside `InputService`

`InputService.Update()` samples either `Mouse` or `TouchPanel`, never both, and exposes the existing boolean/position API from a single internal pointer state:

- Desktop: current behavior unchanged.
- Android: one `TouchPanel.GetState()` call per frame (the state ages on read), tracking the first active touch id. `Pressed` sets the pointer down, `Moved` updates the position, `Released`/`Canceled`/`Invalid` clears it. A new press after a release starts a new id, which `AndroidTouchEventManager` already guarantees.
- Because `CombatScene` derives tap-versus-drag from the distance between press point and current position, no threshold change is needed on touch.
- `IsKeyDown/IsKeyPressed/IsKeyReleased` return false on Android; no scene depends on them during play.

- Rejected: rewriting scenes against `TouchCollection` (large diff, no behavior gain) and synthesizing `MouseState` on Android (would leak a fake device state into public API).

### Safe area applied through the graphics viewport

`ISafeAreaProvider` (interface in shared `Core/Services`, implementation in the Android head using `WindowInsets` with a pre-API-30 fallback) reports the device insets in pixels. `Game1` exposes a settable provider that `MainActivity` fills before `Run()`; each frame `Game1.Update` recomputes the insets, and when they change it sets `GraphicsDevice.Viewport` to the inset-adjusted rect and recalculates the layout. `InputService` receives the same origin as `PointerOffset`, so hit-testing stays aligned with what is drawn.

The viewport transform is what makes this small: every scene already derives its geometry from `GraphicsDevice.Viewport.Width/Height`, so combat, draft, menu, reward and modals all move inside the safe area without per-scene changes. Before drawing, `Game1.Draw` clears the full back buffer to black and then restores the inset viewport, so the notch and gesture bands are a deliberate black letterbox; without that full clear the bands stay unpainted (black only after the surface is recreated on resume) because each scene clears just its own viewport.

- Rejected: insetting only `LayoutManager` (leaves draft, reward, menu, game-over and modals under the bars) and rewriting each scene's percentage math (five files of churn for the same result).
- Rejected: hardcoding an Android inset constant (wrong on devices with and without cutouts).

### Host activity owns presentation and lifecycle

`MainActivity : AndroidGameActivity`:

- `[Activity(Label = "SnapClone", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ...)]` so rotation recreates nothing and the layout never rotates.
- `Window.AddFlags(WindowManagerFlags.KeepScreenOn)` while resumed, cleared in `OnPause` so the screen can sleep in the background.
- Immersive sticky through `WindowInsetsController.Hide(SystemBars)` + `BehaviorShowTransientBarsBySwipe` on API 30+, with the deprecated `SystemUiVisibility` immersive-sticky flags on API 24-29; reapplied in `OnWindowFocusChanged`.
- `SetContentView(game.Services.GetService(typeof(View)) as View)` then `game.Run()`, following the official MonoGame pattern.
- No custom `OnPause`/`OnResume` game handling: `AndroidGameActivity` already pauses and resumes the `Game`; only the screen-on flag is adjusted.
- Back navigation uses the platform default (finish activity), matching the "exit cleanly" requirement.

### Content built per platform from the same manifest

The head declares `<MonoGameContentReference Include="..\SnapClone\Content\Content.mgcb" Link="Content\Content.mgcb" />`. `Link` keeps the content directory name `Content`, which is what `Game1` sets as `Content.RootDirectory`, and MonoGame's targets pass `/platform:Android`, so the same `DefaultFont.spritefont` compiles to a platform-independent XNB inside the APK.

### Scripts as the build interface

`scripts/build-android.ps1` sets `ANDROID_HOME`/`ANDROID_SDK_ROOT` (default `%LOCALAPPDATA%\Android\Sdk`) and `JAVA_HOME` (Microsoft OpenJDK 17) for the session, runs `dotnet build SnapClone.Android -c Debug`, then locates the newest `*-Signed.apk` under `bin\Debug\net9.0-android` and prints its absolute path. `scripts/install-android.ps1` validates `adb devices`, installs with `adb install -r`, and launches with the activity resolved by `cmd package resolve-activity --brief`. Both fail fast with actionable messages instead of raw MSBuild noise.

- Rejected: checking in a `local.properties`/machine-specific `JavaSdkDirectory` (breaks other machines) and requiring global `JAVA_HOME` (silently breaks when the user has another JDK).

### Portability audit of the shared sources

Desktop-only API call sites after the guards: `SnapClone/Game1.cs:60` (`DllImport("user32.dll")`), `:86` (`GraphicsAdapter.DefaultAdapter.CurrentDisplayMode` fallback) — both reachable only through `GetWorkArea`, called inside the desktop-only branch; `:126-127` (`Window.AllowUserResizing`, `Window.ClientSizeChanged`) and `:156-157` (`GamePad`/`Keyboard` exit check) are inside explicit `!OperatingSystem.IsAndroid()` guards. `Core/Services/InputService.cs` reads `Keyboard`/`Mouse` only in the desktop branch and `TouchPanel` only in the Android branch. `Program.cs` is excluded from the Android compile.

## Risks / Trade-offs

- **MonoGame 3.8.6 is built for `net8.0-android34.0`, not `net9.0-android`** → spike builds and runs first; fallback is a `net8.0-android34.0` head with the same shared sources.
- **Workload/JDK install needs network and possibly admin rights** → install commands are documented verbatim in the tasks, and the build script reports the missing component by name.
- **Pinning the SDK to 9.0.3xx could conflict with a future machine setup** → `rollForward: latestFeature` allows later 9.0.4xx SDKs, and the pin is a single small file to revert.
- **No device available** → build, install-script validation and touch behavior cannot be confirmed; those tasks stay open until a phone with USB debugging is connected, while the desktop build remains the regression gate.
- **Guard drift**: a future file using a desktop API outside a guard breaks the Android build only → the Android head build is part of the definition of done for this change, and the tasks include building both heads after every code step.
- **Insets and immersive behavior differ across API 24-35 and vendor ROMs** → keep the fallback path, and verify on the physical device before treating the change as done.
- **Touch state aging** → single `TouchPanel.GetState()` per frame inside `InputService.Update`, with no other caller; a second read would report bogus `Released` states.
- **Duplicate class compilation** if `obj`/`bin` from the desktop project were linked → the Android compile glob excludes them explicitly.

## Migration Plan

1. Toolchain install and spike (no repository code changes beyond `global.json` and the throwaway spike project).
2. Add the Android head with a minimal activity, then link sources and content and fix the platform guards it surfaces.
3. Add touch mapping, safe-area provider, then scripts.
4. Rollback: delete `SnapClone.Android/`, `scripts/*-android.ps1` and `global.json`; the desktop project has no reference to any of them.

## Open Questions

- Exact SDK patch to pin (9.0.320 is installed today; the pin uses the feature band, so this does not affect the plan).
- Device model and Android version for the final install check; resolved when the phone is connected.
