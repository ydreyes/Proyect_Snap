# Proposal

## Why

The game is only runnable as a desktop executable, so it cannot be tested on a phone, where the portrait layout and finger-sized drag targets were meant to be used. A buildable Android APK lets the game be played on real hardware without giving up the desktop loop that is currently used for iteration.

## What Changes

- **Add an Android application head** (`SnapClone.Android`) that compiles the existing game sources and the shared `Content.mgcb` into an installable debug APK (`arm64-v8a`, `ApplicationId com.integritysolution.snapclone`, minSdk 24, target API 35).
- **Keep the desktop build untouched**: `dotnet build SnapClone.sln` stays a PC-only build that does not require the Android workload, SDK or JDK.
- **Add an Android host activity** that owns the game view, locks portrait orientation, hides system bars with sticky immersive mode and keeps the screen on.
- **Make the desktop assumptions platform-conditional**: the `user32` work-area query, the initial window sizing, resizing and the desktop entry point must not run on Android.
- **Add touch as a first-class pointer**: taps and drags on the touch panel must produce the same press / move / release semantics the scenes already use, including the tap threshold that separates a tap (card detail) from a drag.
- **Respect the device display**: the layout must stay inside the safe area so plaques, controls and the hand are not covered by a cutout or the gesture bar.
- **Add repeatable build and install scripts** for the APK so testing does not depend on an IDE.
- **Pin the SDK used by this repository** so the desktop and Android builds resolve deterministic toolchains.

## Capabilities

### New Capabilities
- `android-platform`: building an installable Android APK from the shared game sources, hosting the game in an Android activity with its lifecycle, mapping touch to the existing pointer semantics, and the mobile presentation contract (portrait lock, immersive display, keep-screen-on).

### Modified Capabilities
- `ui-responsive`: the layout must account for device safe-area insets and the immersive display instead of assuming the whole window is drawable, and must define the smallest supported phone viewport.

## Impact

- **New project**: `SnapClone.Android/SnapClone.Android.csproj`, `MainActivity.cs`, `AndroidManifest.xml`, app resources and icons.
- **Shared sources**: `SnapClone/Game1.cs` (platform guards for `user32`, window sizing, resizing), `SnapClone/Core/Services/InputService.cs` (touch pointer source), `SnapClone/Core/Layout/LayoutManager.cs` (safe-area insets), `SnapClone/Program.cs` (excluded from the Android compile).
- **Shared content**: `SnapClone/Content/Content.mgcb` linked into the Android head; no new assets, so the XNB output stays platform-independent.
- **Toolchain**: .NET 9 Android workload, JDK 17/21, `global.json` SDK pin, MonoGame 3.8.6 `MonoGame.Framework.Android` (published as `net8.0-android34.0`).
- **Scripts/docs**: `scripts/build-android.ps1`, `scripts/install-android.ps1`, README build notes.
- **No gameplay, content or card-pool changes**: the 59-card Series 1 implementation stays as is.
- **Verification limit**: installation and runtime checks require a physical device with USB debugging; until one is connected the APK build is verifiable but install is not.
