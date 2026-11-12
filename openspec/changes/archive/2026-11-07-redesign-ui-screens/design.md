# Design

## Context

See `proposal.md` for motivation and `specs/ui-visual-design/spec.md` for the behavior contract.

The current UI is immediate-mode and scene-local. Every scene constructs its own `ShapeDrawer` and `FontService`, hardcodes its palette, and draws text with a single Arial `SpriteFont` scaled between roughly 0.3 and 2.2. There is no animation, no card art, and `SpaceBackground` exists but is not used. `SceneManager.ChangeScene` switches immediately, and `InputService` already exposes pointer pressed, down and released states, so no input redesign is required.

The seven redesign targets are `MenuScene`, `DraftScene`, `RunSummaryScene`, `CollectionScene`, `LevelUpScene`, `GlobalStoreScene` and `ShopScene`. `LevelUpScene` already has a phase machine (`ChooseBranch`, `PickCard`, `PickUpgradeCard`, `PickUpgradeType`, `PickPerk`, `PickPerkTarget`, `Unavailable`), and `ShopScene` has its own browse and selection phases. The design keeps both state machines and changes only their presentation.

Shared components also used by combat must keep working: `ShapeDrawer`, `FontService`, `CardView`, `AvatarView`, `LocationView`, `PauseOverlay` and `Modal`. `GameServices` is a small type-keyed container, `Game1` owns the `SpriteBatch` and safe-area viewport, and `LayoutManager` remains the combat layout authority.

Data constraints are fixed: `CardDef` has no art, rarity or series field; the catalog has 59 draftable cards plus 3 tokens and 4 themes; `ProfileDto` has no collection level or XP; `RunDirector.OnCombatLost()` consumes an extra-life charge automatically. The content pipeline currently builds only `DefaultFont.spritefont` through MGCB.

## Goals / Non-Goals

**Goals:**

- One visual language for the seven screens: shared tokens, typography, chrome, card art, background, motion and transitions.
- A foundation that can be adopted screen by screen without a flag day.
- Deterministic procedural card art with an optional real-art slot and no required image assets.
- Runtime scalable typography with the existing default font kept as a load-failure fallback.
- Low blast radius: shared component APIs stay stable, and combat behavior is untouched.
- A device-friendly budget: cached textures, lazy card art, no per-frame texture generation.

**Non-Goals:**

- No combat layout or combat behavior redesign. Combat may inherit typography through `FontService`, but its board, hand, zones and controls stay as they are.
- No new mechanics: no collection level or reward track, no "continue your run?" choice, no currencies, timers, seasons, saved decks, notifications or in-app purchases.
- No persistence or save-schema changes.
- No real art assets in this change. The art slot is an extension point, not a delivered asset set.
- No UI framework (Gum, Myra, ImGui) and no shader dependency in the first pass.

## Decisions

### 1. Foundation services and ownership

Long-lived visual services are registered once in `GameServices`:

| Service | Lifetime | Responsibility |
|---|---|---|
| `UiTheme` | Global | Tokens: palette, type scale, spacing, corner cuts, elevation, semantic states |
| `TextureCache` | Global | Cached gradients, glow, noise, hex tile, 9-slice panel, scanline, vignette |
| `ChromeRenderer` | Global | Panels, buttons, chips, ribbons, badges, progress slots, overlays |
| `CardArtProvider` | Global | Deterministic art cache plus optional real-art lookup |

Per-screen state stays in the scene: `UiAnimator`, `UiButton` instances, filter state, scroll/page state and a `SpaceBackground` instance. This avoids global mutable UI state while keeping the implementation shared.

`ShapeDrawer` and `FontService` become shared instances created in `Game1` and registered in `GameServices`. Their public APIs stay the same so existing scenes and combat keep compiling. New role-specific text methods are additive. Screens are migrated to the shared instances in the same pass that redesigns them; untouched scenes can keep their local construction until then.

Alternatives considered:

- Keep per-scene `ShapeDrawer`/`FontService`: simplest short term, but it duplicates textures and font atlases and guarantees visual drift.
- Replace `GameServices` with a full DI container: unnecessary for a game of this size and adds a dependency.

### 2. Typography: runtime TTF with embedded resources

`FontService` keeps `Measure`, `DrawText`, `DrawTextCentered` and the existing overloads, but delegates to a FontStashSharp `FontSystem`. New methods expose the roles: `DrawDisplay`, `DrawLabel`, `DrawBody` and a numeric variant with tabular figures.

The three TTFs are bundled as embedded resources under `Content/Fonts` and loaded with `Assembly.GetManifestResourceStream` into `FontSystem.AddFont(byte[])`. This avoids MGCB TTF processing and Android content-path differences. If the stream or font load fails, `FontService` falls back to the existing `DefaultFont` `SpriteFont`, preserving the current text path.

`FontSystemSettings` uses a resolution factor that keeps text crisp when scaled, and the character range is limited to Latin-1 plus the punctuation the game uses, keeping the atlas small. Display text is uppercased by the caller, not by the font.

Alternatives considered:

- Multiple `SpriteFont` assets (14/18/28/48): no new dependency, but fixed sizes, build-time font installation and no fine tracking control.
- Keep `DefaultFont`: no visual improvement and the weakest part of the current look.
- MGCB-processed TTF: no standard importer for raw TTF, and platform behavior is less predictable than an embedded resource.

The dependency is `FontStashSharp.MonoGame` 1.6.2 (Zlib). A desktop and Android spike is the first task; if Android loading fails, the fallback is the multiple-`SpriteFont` approach without changing the screen designs.

### 3. Procedural card art and the optional art slot

`CardArtProvider` resolves art in this order:

1. If a real-art texture is registered for the card id, use it.
2. Otherwise, generate and cache a procedural texture.

The procedural seed is a stable hash (FNV-1a) of the card id, mixed with the theme ordinal and cost. `System.Random` is used only as a deterministic sequence from that seed; no `Guid`, time or platform randomness is involved. The same card therefore looks the same across sessions, devices and screen transitions.

Generation layers, in order:

1. Vertical base gradient from the theme hue.
2. Radial glow at a seeded focal point.
3. Emblem or silhouette derived from the id hash.
4. Composition by cost bucket: 1-2 close-up, 3-4 shards, 5-6 burst.
5. Seeded particle fragments.
6. Scanlines and fine noise.
7. Vignette.

Text, cost, power, name and badges are never baked into the texture. They are drawn by the card renderer over a scrim or plate.

Textures are 128x192 RGBA (about 96 KB each), generated lazily on first use and cached by card id. All 59 cards would be about 5.7 MB if every card is displayed; a 96x144 variant is the fallback if device memory is tight. The same cached texture is reused at every display size.

The optional art slot is a registry of card id to texture. This change ships it empty and documents the convention for adding art later (content-pipeline texture or runtime-loaded PNG). The frame, badge and text layers are independent of the art source, and a provided asset is cropped to the art layer's safe area rather than stretched.

Alternatives considered:

- Hash-only color fills: cheap but too samey across 59 cards.
- Pre-generated PNGs for every card: requires an art pipeline and assets now.
- Shader-generated art: platform risk on Android and harder to cache and preview.

### 4. Chrome and texture cache

`TextureCache` generates and owns the reusable textures:

| Texture | Size | Use |
|---|---|---|
| Vertical gradient | 1x256 | Screen and panel backgrounds |
| Radial glow | 128x128 | Energy accents, selection glow |
| Noise | 256x256 tileable | Grain and holo texture |
| Hex tile | 128x128 | Ambient background grid |
| 9-slice panel | 64x64 with 16 px corners | Panels, buttons, chips |
| Scanline | 1x8 | Holo and energy overlays |
| Vignette | 128x128 | Screen depth |

`ChromeRenderer` composes these with `ShapeDrawer` primitives. Panels and buttons use the 9-slice texture so corners stay crisp at any size; the bevel, inner highlight and shadow are drawn as thin rectangles. Glows are drawn in a second additive `SpriteBatch` pass, because the blend state is fixed per batch.

Alternatives considered:

- Draw gradients as 10-40 bands per frame (current approach): simple but wasteful and visibly banded.
- Use a shader for gradients and glows: fewer draw calls, but adds platform-specific compilation and a fallback path.
- Ship external panel textures: unnecessary, since the chrome is geometric.

### 5. Motion

Motion is hand-rolled: `Tween` (delay, duration, easing, from, to, onComplete) and `UiAnimator` (list of tweens, `Update(gameTime)`, `Value` lookup). Easing functions are `Linear`, `EaseOutCubic`, `EaseOutBack`, `EaseInOutQuad` and a sine pulse.

`UiButton` is a small state machine over `InputService`: idle, hover, pressed and disabled. It activates on release inside its bounds, so a drag off the button cancels the press. Press scale is 0.96, hover scale is 1.03, and the transition is 100-200 ms. Entry stagger is 40-60 ms per element, and screen entrances complete within 400 ms.

Alternatives considered:

- `MonoGame.Extended` 6.1.1: provides tweens, particles and primitives, but its current build targets MonoGame 3.8.5 while this project floats on `3.8.*` (currently 3.8.6.x). Adopt only after a desktop and Android compatibility spike; the hand-rolled layer is the baseline.
- A dedicated tween library: another dependency for roughly 150 lines of code.

### 6. Ambient background

`SpaceBackground` is upgraded and connected to the seven screens. It draws, from back to front:

1. A cached vertical gradient and a slow nebula wash.
2. A sparse hex grid with a slow parallax offset.
3. A vignette.
4. A capped particle or dust field (about 60 particles) with a sine drift.

Static layers are cached textures; only the particle positions and the parallax offsets are updated per frame. The background is seeded per screen so it is stable across visits but different enough between screens to avoid a frozen backdrop.

Alternatives considered:

- Keep the background unused: no depth, and the screens stay flat.
- Shader-based starfield: deferred until the shader spike, if ever.

### 7. Scene transitions

`SceneManager` gains a small transition state machine:

```
Idle --ChangeScene(name)--> FadeOut --switch--> FadeIn --> Idle
                              |                    |
                         OnEnter(new)         input enabled
```

`ChangeScene` starts the fade and defers the actual scene switch and `OnEnter` to the midpoint. `CurrentSceneName` keeps reporting the outgoing scene until the switch. `ConsumeSceneChange` returns true on the switch frame so `Game1` skips the input that belonged to the outgoing screen. `SceneManager.Update` skips scene updates while transitioning, and `SceneManager.Draw` draws the current scene and then the transition overlay.

`Game1.Update` calls a transition update before input handling so the fade advances even when scene input is suppressed. The pause overlay is outside the transition and keeps its current behavior.

The transition applies to every `ChangeScene` call, including combat entry. That is a visual-only change and keeps the implementation simple. If combat entry needs to stay instant, a per-call `TransitionMode` parameter can opt out without changing the state machine.

Alternatives considered:

- Fade only on the incoming scene: simpler, but the spec asks for an outgoing transition.
- A separate transition scene: more state and a fake scene in the manager for no benefit.

### 8. Screen composition

All seven screens use a shared `ScreenScaffold` helper that resolves the safe-area viewport into header, content and footer regions, and draws the background, title, currency chip and bottom navigation. The content area is screen-specific.

- **Menu**: run reactor (enemy or act, wins, deck strip), one primary action slot (`CONTINUAR`, `VER RESUMEN` or `NUEVA RUN`), bottom navigation (`COLECCION`, `TIENDA`, `NUEVA RUN`, `SALIR`) and a global points chip.
- **Draft**: eight-slot strip, theme ribbon, three card tiles with procedural art, and a fly-to-slot confirmation. The existing draft state machine and checkpoint resume behavior are unchanged.
- **Run summary**: outcome reactor (defeat or victory), final deck strip, points breakdown and menu/re-run actions. If an extra-life charge was consumed, the screen states that it was consumed and does not show a choice.
- **Collection**: four-column grid, filter chips for state, theme and cost, paging, card detail overlay and perks tab. The existing five card states and 59-card catalog are unchanged.
- **Level-up**: one ceremony template driven by the existing phase. Branch choice and upgrade type are large tiles; card offers and deck targets are medallions or a carousel; perk offers show name, effect and target requirement.
- **Global store**: points chip, `OFERTAS` and `CARTAS` tabs, three bundles plus single entries, and a sold state. The existing pagination and catalog partition are unchanged.
- **In-run shop**: money chip, four card offers with art and sold state, and service rows for upgrade, removal, extra life and leave. The existing purchase phases and checkpoint writes are unchanged.

Card presentation uses a new `CardRenderer` with variants for draft, collection, store, medallion and mini-deck. `CardView` remains the combat renderer and is not switched to the new visual in this change. This keeps combat visuals and behavior stable while the seven screens get art-driven cards.

Alternatives considered:

- Refactor `CardView` into the new renderer immediately: more reuse, but it changes combat visuals and raises regression risk.
- Give each screen its own card drawing code: guarantees drift and duplicates the art and badge logic.

### 9. Data, content and assets

No persistence, run-state or economy changes. The art registry is external to `CardDef`, so no card schema migration is needed. `DefaultFont` stays in the content pipeline as the fallback. TTFs are embedded resources, not MGCB assets. This change adds no images, atlases or shaders.

### 10. Performance budget

| Item | Budget |
|---|---|
| Card art | 128x192 RGBA, about 96 KB each, lazy, up to about 5.7 MB for 59 cards |
| Static textures | About 0.5-1 MB total |
| Font atlases | About 4-8 MB for three faces at the used sizes |
| Particles | About 60 on screen |
| Draw calls | About 100-150 on a full collection page |
| Target | 60 fps at 720x1280 on a mid-range Android device |

Draw paths avoid `Texture2D.SetData`, `new Texture2D` and large list allocations per frame. Textures are generated once, card art is generated on first use, and particle arrays are reused. Glow is the only additive pass, and it is limited to the visible accents rather than full-screen blurs.

## Risks / Trade-offs

- **FontStashSharp fails to load on Android** → Spike first with embedded TTF resources; keep the `DefaultFont` fallback; if it still fails, switch to multiple `SpriteFont` assets without changing the screen specs.
- **Procedural art looks samey** → Vary by theme hue, cost composition and id hash; review the full 59-card set; add composition rules before rolling out.
- **Text over art loses contrast** → Always draw a scrim or plate under names and badges; verify at 720x1280 and with the brightest theme.
- **Card art memory on low-end devices** → Lazy cache, 128x192 baseline, 96x144 fallback, measure the full-catalog case.
- **Blast radius on shared components** → Keep `ShapeDrawer`, `FontService` and `CardView` APIs stable; do not switch combat to the new card renderer; verify combat, pause, pile viewer and detail panels after each phase.
- **Overdraw and frame rate** → Cache static layers, cap particles, use additive passes only for glows, profile on the device before phase 3.
- **Scene transition input bugs** → Keep `ConsumeSceneChange` semantics, block input during the transition, and test every screen change including pause and back.
- **MonoGame.Extended incompatibility** → Do not adopt it in the baseline; run an optional spike and only add it if it passes on DesktopGL and Android.
- **APK size** → Three OFL TTFs add roughly 1-2 MB; acceptable, and no image assets are added.
- **Spec drift with existing capabilities** → The delta specs for `ui-responsive`, `game-framework` and `card-piles` reconcile the typography and art wording; no behavior changes are intended.
- **Dirty working tree** → Isolate the redesign in a branch or worktree before implementation and avoid mixing it with unrelated changes.

## Migration Plan

No data migration is required. The rollout is incremental:

1. Foundation and spikes: font loading on DesktopGL and Android, texture cache, chrome, animator, button, background, transition, card art provider.
2. Style frame: redesign one screen end to end (Draft or Menu) and review it at 720x1280 before multiplying the work.
3. Loop screens: Menu, Draft, Run summary.
4. Collection and level-up.
5. Global store and in-run shop.
6. Polish: particle tuning, optional shader spike, final performance pass.

Rollback is file-level: the save format is unchanged and `DefaultFont` remains available, so reverting the UI files restores the current presentation without data loss.

Verification runs on DesktopGL and Android at 720x1280, with touch, safe-area insets, every scene transition, draft resume, collection filters and detail, level-up phases, both stores, and a combat regression pass.

## Open Questions

- Exact font pairing (Bebas Neue versus Anton; Rajdhani versus Chakra Petch) is a visual choice with no spec impact.
- Card art resolution (128x192 versus 96x144) is decided by the device memory measurement.
- Whether `MonoGame.Extended` is adopted after the compatibility spike; the hand-rolled layer is the baseline.
- Whether combat adopts the new card renderer later; that is a separate change.
- Whether Spanish accents are restored with the new fonts; the default is to keep the current accent-free text.
