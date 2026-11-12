# Proposal

## Why

The game already has the full loop, but its presentation is still a programmer UI: one Arial font scaled from 0.3 to 2.2, zero animation, hardcoded per-scene colors, no card art and no shared chrome. The reference material shows a cohesive sci-fi identity built from strong typography, angular chrome, glow, motion and card art. The player-facing screens are the first impression of the game, and today they do not communicate the Marvel Snap-style fantasy the rest of the design is aiming for.

This change closes that presentation gap without changing rules, economy or persistence. It gives every screen one visual language so the seven screens stop looking like seven unrelated prototypes.

## What Changes

- Introduce a shared visual design system: design tokens (palette, type scale, metrics), a cached procedural texture set, a chrome renderer, a tween/animator, button states and scene transitions.
- Replace the single `DefaultFont` usage with runtime TTF typography (FontStashSharp + OFL fonts): a condensed display face, a UI face and a body face.
- Redesign the seven existing player-facing screens:
  - main menu (run hub with reactor, currency chips, primary action and bottom navigation);
  - draft (8-slot progress strip, themed card offers, fly-to-deck feedback);
  - run summary / game over (reactor, deck strip, points and actions);
  - collection (art-driven grid, filter chips, detail overlay, perks tab);
  - level-up / upgrade selection (single ceremony template with option tiles and a card medallion);
  - global store (currency header, tabs, offer grid, sold state);
  - in-run shop (currency header, card offers and service rows).
- Add deterministic procedural card art with an optional real-art slot. When a card has no art asset, the renderer generates a stable composition from its id, theme and cost; when art exists, only the art layer is replaced.
- Add a reusable ambient background and motion layer: gradient, hex grid, vignette, particles, glow, entrance stagger, hover/press feedback and scene fade/wipe.
- Preserve every existing behavior and constraint: 8 draft picks, 59-card collection, two stores with their current currencies, extra life consumed automatically, points-only meta progression, portrait 720x1280 minimum, touch targets and safe-area insets.

No gameplay, economy, save-format, AI or persistence behavior changes. No new currencies, timers, seasons, saved decks, notifications or in-app purchases.

## Capabilities

### New Capabilities

- `ui-visual-design`: the durable visual system and screen presentation rules: design tokens, typography, chrome, procedural card art and its optional art slot, motion, background, transitions, and the visual requirements for the seven redesigned screens.

### Modified Capabilities

- `ui-responsive`: the text-rendering and legibility wording moves from the single default font to the shared typography system, with the default font kept as the load-failure fallback. Layout, touch, safe-area and overlap requirements are unchanged.
- `game-framework`: primitive UI drawing now explicitly allows textures generated in memory and the shared typography system, and it defines the optional card-art slot without requiring external image assets.
- `card-piles`: the pile-viewer legibility wording moves from the default font to the shared typography system. Placement and behavior are unchanged.

These are constraint reconciliations required by the approved typography and art decisions, not new gameplay behavior. The `collection`, `draft-system`, `global-store`, `ingame-shop`, `meta-progression` and `roguelike-progression` requirements remain unchanged; this change must continue to satisfy them.

## Impact

- **Code**: `SnapClone/UI/` (new theme, texture cache, chrome renderer, animator, card art provider; refactor of `ShapeDrawer`, `FontService`, `CardView`, `AvatarView`, `SpaceBackground`), `SnapClone/Scenes/` (Menu, Draft, RunSummary, Collection, LevelUp, GlobalStore, Shop), `SnapClone/Game1.cs` (shared UI services and transitions), `SnapClone/Content/` (fonts), `SnapClone/Content/Content.mgcb`, `SnapClone/SnapClone.csproj`, `SnapClone.Android/SnapClone.Android.csproj`.
- **Dependencies**: `FontStashSharp.MonoGame` 1.6.2; bundled OFL fonts (Bebas Neue or Anton, Rajdhani or Chakra Petch, Inter). No UI framework. No art library.
- **Systems**: no changes to combat rules, run state, economy, persistence or save schema. Combat may consume shared chrome and card rendering only where it does not alter behavior.
- **Risks**: runtime TTF loading on Android, overdraw and memory on low-end devices, and the blast radius of refactoring shared UI components used by combat. Mitigation is a font spike, a single-screen style frame and a texture/memory budget before applying the system to all screens.
