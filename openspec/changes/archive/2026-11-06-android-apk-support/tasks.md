# Tasks

## 1. Toolchain Spike

- [x] 1.1 Add `global.json` pinning the 9.0.3xx SDK feature band (`rollForward: latestFeature`) and verify `dotnet --version` at the repo root reports a 9.0.3xx SDK and `dotnet build SnapClone.sln` still succeeds.
- [x] 1.2 Install Microsoft OpenJDK 17 and verify `java -version` reports 17.x (not the IntelliJ JBR 25) in a shell that has `JAVA_HOME` set to it.
- [x] 1.3 Install the .NET 9 Android workload and verify `dotnet workload list` lists `android` and an empty `net9.0-android` project builds successfully.
- [x] 1.4 Create a throwaway spike project outside the solution (`spike/android`) that references `MonoGame.Framework.Android` 3.8.6 and hosts a minimal `AndroidGameActivity`; verify it produces a signed debug APK under `bin/Debug/net9.0-android/**` and record the exact APK path pattern.
- [x] 1.5 Install the spike APK on a device and confirm the activity opens without crashing; if no device is available, record the blocking state and keep the APK-path verification as the passing criterion.
- [x] 1.6 Decide the head TFM from the spike result (`net9.0-android`, or `net8.0-android34.0` as fallback) and record the outcome in `design.md` before starting the real head.

## 2. Android Head And Host Presentation

- [x] 2.1 Create `SnapClone.Android/SnapClone.Android.csproj` with `net9.0-android`, `OutputType=Exe`, `ApplicationId=com.integritysolution.snapclone`, `SupportedOSPlatformVersion=24`, `RuntimeIdentifiers=android-arm64` and the MonoGame Android packages; verify `dotnet build SnapClone.Android -c Debug` succeeds.
- [x] 2.2 Add app resources (label `SnapClone`, launcher icon derived from `Icon.bmp`, fullscreen dark theme) and the activity attributes for `MainLauncher`, `ScreenOrientation=Portrait` and `ConfigurationChanges`; verify `aapt2 dump badging` on the APK reports the package id, `sdkVersion:'24'`, `targetSdkVersion:'35'`, `native-code: 'arm64-v8a'` and a portrait-only activity.
- [x] 2.3 Implement `MainActivity : AndroidGameActivity` that creates the game, sets its view via `SetContentView` and calls `Run()`; verify the activity launches and renders without a desktop window.
- [x] 2.4 Implement immersive sticky mode with the API 30+ controller and the legacy `SystemUiVisibility` fallback for API 24-29, reapplied in `OnWindowFocusChanged`; verify system bars stay hidden after a swipe reveal and after leaving and returning to the app.
- [x] 2.5 Add `KeepScreenOn` while resumed and clear it in `OnPause`; verify the display does not sleep during play and does sleep in the background.
- [x] 2.6 Verify back navigation exits the app cleanly by pressing the system back gesture on the device (or, without a device, confirm the activity does not override back handling).

## 3. Shared Code Portability

- [x] 3.1 Link `..\SnapClone\**\*.cs` into the Android head excluding `Program.cs`, `bin/**` and `obj/**`, and verify the head compiles the shared sources without duplicate-type errors.
- [x] 3.2 Guard the `Game1` constructor so `GetWorkArea`, the `user32` P/Invoke and `PreferredBackBuffer*` sizing only run on desktop; verify the Android build succeeds and the desktop executable still opens at its current size.
- [x] 3.3 Guard `Window.AllowUserResizing` and the `ClientSizeChanged` subscription to desktop only; verify the APK builds and the desktop window still resizes and relayouts.
- [x] 3.4 Guard the `Escape`/gamepad-Back exit check to desktop only; verify escaping still exits on desktop and the Android app is not closed by phantom input.
- [x] 3.5 Confirm no other shared file calls desktop-only APIs (search the shared sources for `DllImport`, `Mouse.`, `Keyboard.`, `Window.`, `GraphicsAdapter`); verify each remaining call is inside a platform guard or is available on Android, and record the file/line list in a comment or in the change notes.
- [x] 3.6 Run `dotnet build SnapClone.sln` and `dotnet build SnapClone.Android -c Debug` back to back; verify both succeed before moving on.

## 4. Content Pipeline

- [x] 4.1 Add `<MonoGameContentReference Include="..\SnapClone\Content\Content.mgcb" Link="Content\Content.mgcb" />` to the head; verify the Android build compiles `DefaultFont` and the APK contains `Content/DefaultFont.xnb` (`unzip -l` on the APK).
- [x] 4.2 Verify the desktop build still compiles the same manifest and that adding a temporary asset to `Content.mgcb` appears in both outputs; remove the temporary asset afterwards.
- [x] 4.3 Verify the game loads `DefaultFont` on Android without a content-not-found fallback path being triggered (device run, or startup log inspection if no device is available).

## 5. Touch Pointer

- [x] 5.1 Refactor `InputService` to keep one internal pointer state (`position`, `down`, `previous down`) and derive `IsLeftMousePressed/Down/Released` and `MousePosition` from it; verify the desktop build succeeds and the desktop input still drags cards and opens detail panels.
- [x] 5.2 Implement the Android branch with a single `TouchPanel.GetState()` per `Update()`, tracking the first active touch id across `Pressed`/`Moved`/`Released`; verify the touch panel is read exactly once per frame and no other caller exists.
- [x] 5.3 Map `Canceled`/`Invalid` touch states to a pointer release so an interrupted touch cannot leave a drag active; verify with a manual device check (or a unit test over the state mapping if the mapping is extracted into a pure function).
- [x] 5.4 Ignore secondary touches while the first is active; verify on the device that a second finger does not start another drag or duplicate a click.
- [x] 5.5 Verify on the device that a tap opens the card detail panel and a drag beyond the tap threshold plays or returns a card exactly like the desktop mouse path.

## 6. Safe Area Layout

- [x] 6.1 Add `ISafeAreaProvider` (insets in pixels) to `Core/Services` and apply the insets by shrinking `GraphicsDevice.Viewport` from `Game1.Update`, keeping `LayoutManager` reading the adjusted viewport; verify the desktop build keeps a zero-inset full viewport and identical geometry.
- [x] 6.2 Implement the provider in the Android head using `WindowInsets` with a pre-API-30 fallback, and set it on the game before `Run()`; verify the APK builds and the provider is registered when `Initialize` runs.
- [x] 6.3 Recalculate the layout when insets change (once per frame on Android) and verify a system-bar reveal does not leave controls under the bars.
- [x] 6.4 Verify on the device that the top bar, plaques, controls and hand stay inside the safe area and that the layout still fills the display at 720x1280 and at the device's native portrait resolution.

## 7. Scripts And Documentation

- [x] 7.1 Add `scripts/build-android.ps1` that sets `ANDROID_HOME`/`JAVA_HOME` defaults, builds the head in Debug, locates the signed APK and prints its absolute path; verify it fails with a clear message when `JAVA_HOME` points to a non-17 JDK.
- [x] 7.2 Add `scripts/install-android.ps1` that validates `adb devices`, installs with `adb install -r` and launches the activity resolved by `cmd package resolve-activity`; verify the no-device path exits non-zero with a message naming the missing device.
- [x] 7.3 Document in the README the one-time toolchain setup, both scripts, the APK output path and the fact that `dotnet build SnapClone.sln` remains PC-only; verify the documented commands run as written in a fresh shell.

## 8. End-To-End Verification

- [x] 8.1 From a clean `bin`/`obj` state, build the desktop solution and the Android head in the same shell; verify both succeed and the APK path printed by the script matches the artifact on disk.
- [x] 8.2 On the device, play the full loop (menu, draft, combat, drag a card to a zone, end turn, reward) and verify no crash, no stuck drag, and correct energy and power values.
- [x] 8.3 On the device, background the app mid-combat and resume it; verify the same scene, hand, board and turn timer are preserved.
- [x] 8.4 On desktop, run the same smoke path with the mouse and verify window sizing, dragging, detail panels and turn flow are unchanged from before the change.

## Verification Notes

- All device-side checks (2.3-2.5, 4.3, 5.3-5.5, 6.3, 6.4, 8.2, 8.3) were executed on the `snapclone-test` emulator running the `arm64-v8a` APK: launch and render, immersive reveal plus auto-hide, keep-screen-on across pause/resume, safe area at native and 720x1280, tap-to-open detail, drag into a zone, two-finger isolation, canceled-touch release, and background/resume mid-combat preserving scene, hand, board and turn timer.
- Confirming the same flows on a physical handset is still pending because none was attached; the APK is a self-contained Debug build and can be installed with `scripts/install-android.ps1` once a device appears.
- Desktop (8.4) was verified with a scripted mouse session on the 538x958 client at 125% DPI: menu, 8 draft picks, CONTINUAR, combat, tap detail open/close, hand card dragged into player slot 0 of zone 0, End Turn through the result modal into the reward scene.
