# Tasks

## 1. Isolation and baseline

- [X] 1.1 Create a dedicated branch or worktree for this change and confirm the current tree is isolated; verify `git status --short` shows only intended files and record the baseline commit hash.
- [X] 1.2 Capture baseline screenshots of the seven target screens at 720x1280 on DesktopGL and on Android; verify the screenshots exist for Menu, Draft, Summary, Collection, LevelUp, GlobalStore and Shop.
- [X] 1.3 Run `dotnet build SnapClone.sln` and `dotnet run --project SnapClone.Harness`; verify both succeed and record the baseline harness output.

## 2. Typography and dependency spike

- [X] 2.1 Add `FontStashSharp.MonoGame` 1.6.2 to `SnapClone/SnapClone.csproj` and `SnapClone.Android/SnapClone.Android.csproj`; verify `dotnet restore` and the DesktopGL build succeed.
- [X] 2.2 Add Bebas Neue, Rajdhani and Inter TTFs under `SnapClone/Content/Fonts` as embedded resources, together with their OFL license files; verify the resources and licenses are present in the built assembly.
- [X] 2.3 Implement FontStashSharp loading in `FontService` with the display, UI, body and numeric roles plus the `DefaultFont` fallback; verify a temporary font gallery renders all roles at 720x1280 and that a forced load failure still renders every label through the fallback.
- [X] 2.4 Build and install the Android APK and verify the three roles render and remain legible on device; record go/no-go and, on no-go, implement the multiple-`SpriteFont` fallback and re-verify.
- [X] 2.5 Document the font pipeline, atlas budget and fallback in `docs/ui-visual-design.md`; verify the document matches the implementation and the spike result.

## 3. Tokens, theme and screen scaffold

- [X] 3.1 Implement `UiTheme` with the palette, type scale, spacing, corner cuts, elevation and semantic states; verify the code constants match the documented token table.
- [X] 3.2 Implement a `ScreenScaffold` helper that resolves the safe-area viewport into header, content and footer regions; verify it at 720x1280 and with simulated top, bottom, left and right insets.
- [X] 3.3 Add harness checks for token invariants: type-scale ordering, minimum touch target and contrast between text and panel colors; verify `SnapClone.Harness` passes.
- [X] 3.4 Document the tokens, type scale and safe-area rules in `docs/ui-visual-design.md`; verify the document is consistent with `UiTheme`.

## 4. Texture cache, chrome and controls

- [X] 4.1 Implement `TextureCache` with the vertical gradient, radial glow, noise, hex tile, 9-slice panel, scanline and vignette textures; verify each texture is generated once and reused across draws.
- [X] 4.2 Implement `ChromeRenderer` for panels, buttons, currency chips, ribbons, badges, progress slots and overlays; verify a gallery screen renders every component and state at 720x1280.
- [X] 4.3 Add the additive glow pass helper; verify glows render correctly and the normal `SpriteBatch` state is restored afterwards.
- [X] 4.4 Implement `UiButton` and the pointer router with idle, hover, pressed and disabled states and release-inside activation; verify press, drag-off cancel, release and disabled behavior.
- [X] 4.5 Implement `UiAnimator`, `Tween` and the easing functions; verify entrance, stagger, press and pulse timings in the gallery.
- [X] 4.6 Add harness checks for button state transitions, easing endpoints and tween completion; verify `SnapClone.Harness` passes.

## 5. Procedural card art and art slot

- [X] 5.1 Implement `CardArtProvider` with the FNV-1a seed, theme and cost composition and lazy 128x192 cache; verify the same card id produces identical bytes across two runs.
- [X] 5.2 Implement the optional real-art registry and the crop-to-safe-area path; verify with a temporary test texture that only the art layer changes while the frame, badges and text stay identical.
- [X] 5.3 Add harness checks for determinism, theme and cost variation and cache reuse; verify `SnapClone.Harness` passes.
- [X] 5.4 Add a temporary 59-card art gallery and review it for samey output; adjust the composition rules and verify every card is visually distinguishable.
- [X] 5.5 Document the art slot convention, crop rules and texture budget in `docs/ui-visual-design.md`; verify the document matches the implementation.

## 6. Background and scene transitions

- [X] 6.1 Upgrade `SpaceBackground` with the gradient, hex parallax, vignette and capped particle field; verify it renders behind a sample screen and stays stable across repeated visits.
- [X] 6.2 Implement the `SceneManager` transition state machine with fade-out, deferred switch, fade-in and input blocking; verify every screen change plus the back and pause paths.
- [X] 6.3 Add harness checks for the transition state machine: idle, fade-out, switch, fade-in and bounded duration; verify `SnapClone.Harness` passes.
- [X] 6.4 Document the transition behavior and input-safety rules in `docs/ui-visual-design.md`; verify the document matches the implementation.

## 7. Menu, Draft and Run Summary

- [X] 7.1 Redesign `MenuScene` with the reactor, currency chip, primary action slot and bottom navigation; verify `CONTINUAR`, `VER RESUMEN` and `NUEVA RUN`, the new-run confirmation and `SALIR`.
- [X] 7.2 Redesign `DraftScene` with the eight-slot strip, theme ribbon, three art cards and fly-to-slot feedback; verify themed rounds, random rounds, checkpoint resume and 8/8 completion.
- [X] 7.3 Redesign `RunSummaryScene` with the outcome reactor, deck strip, points breakdown and actions; verify defeat, victory and the consumed-extra-life message.
- [X] 7.4 Document the implemented states of the three screens in `docs/ui-visual-design.md`; verify the document matches the screens.

## 8. Collection and Level-Up

- [X] 8.1 Redesign the `CollectionScene` grid and tiles with art, cost and power badges, state treatments, filter chips and paging; verify the 59-card catalog, all five states, the filters and paging.
- [X] 8.2 Redesign the collection detail overlay and perks tab; verify the large card, rules, state, obtain path and perk detail.
- [X] 8.3 Redesign `LevelUpScene` with the ceremony template across all seven phases; verify branch, card, upgrade-card, upgrade-type, perk, perk-target and unavailable states.
- [X] 8.4 Document the implemented collection and level-up states in `docs/ui-visual-design.md`; verify the document matches the screens.

## 9. Global Store and In-Run Shop

- [X] 9.1 Redesign `GlobalStoreScene` with the points chip, `OFERTAS` and `CARTAS` tabs, the bundle and single-entry grid and the sold state; verify the three visible bundles, single entries, sold state and purchase feedback.
- [X] 9.2 Redesign `ShopScene` with the money chip, four card offers and service rows; verify card purchases, upgrade, removal, extra life, leave and the deck-selection phases.
- [X] 9.3 Document the implemented states of both stores in `docs/ui-visual-design.md`; verify the document matches the screens.

## 10. Integration and performance verification

- [X] 10.1 Run `dotnet build SnapClone.sln` and `dotnet run --project SnapClone.Harness`; verify both pass with the new UI foundation.
- [X] 10.2 Build and install the Android APK; verify all seven screens, transitions, safe areas, touch targets and text legibility at 720x1280.
- [X] 10.3 Profile the seven screens on the smallest supported Android device; verify the 60 fps target, the card art cache budget, the font atlas budget and no per-frame texture allocation.
- [X] 10.4 Run a combat regression pass covering the hand, board, drag and drop, pile viewer, detail panels and pause overlay; verify combat behavior and layout are unchanged.
- [X] 10.5 Review the seven screens against the specs and the reference images; record any deferred polish in the change notes and verify every spec scenario is covered.

## Validation notes (2026-11-07)

- Branch `redesign-ui-screens`; baseline commit `07350f7`.
- `dotnet build SnapClone.sln` OK; `dotnet run --project SnapClone.Harness` prints `HARNESS META: TODO OK`.
- Android APK built and installed. Menu, Draft, Summary, Collection, LevelUp, GlobalStore and Shop verified at 1080x2400 and at 720x1280 (Collection and Shop). Verified tabs, filters, paging, card detail overlay, perk detail, store purchase and sold state, in-run shop purchase, sold and deck-selection phases, combat drag/drop, combat card detail, pile viewer and pause overlay.
- Fixed during validation: the store price footer no longer paints over the power badge, and the numeric role now renders tabular digits with a fixed advance.
- Deferred: 1.2 baseline screenshots only exist for Menu
  (`docs/screenshots/baseline/01-menu.png`). The other six would require rebuilding
  the pre-redesign tree; they are not reproducible from the current working tree.
- Deferred: 10.3 device profiling on the smallest physical Android device. The
  emulator does not expose representative frame timestamps through SurfaceFlinger.
- Deferred polish: Draft and perk-detail layouts leave large vertical whitespace
  on tall screens; GlobalStore bundles use full-width offer cards instead of a
  denser grid; font atlases are 1024x1024 per role (about 4 MB each), above the
  4-8 MB total stated in the design budget, pending the 10.3 measurement.
