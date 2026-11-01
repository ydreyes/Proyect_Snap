# Proposal

## Why

We need to build a Marvel Snap-inspired draft roguelike (SnapClone) in MonoGame from scratch. The game requires complex mechanics like card effects with different trigger types, zone-based combat with priority and reveal order, a roguelike draft system with deck building rewards, and must be fully extensible so new cards, zones, effects, and perks can be added without modifying core systems. This is a substantial prototype that needs careful architectural planning before implementation.

## What Changes

- **Core Game Framework**: Set up MonoGame-based game loop with scene management, state machine, and UI primitives drawn entirely with built-in MonoGame capabilities (no image assets)
- **Effect System**: Implement pluggable effect architecture supporting OnReveal, Ongoing, EndOfTurn, and Activate trigger types with registry/factory patterns for extensibility
- **Combat System**: Build 3-zone board with priority calculation, card reveal ordering by priority and play order, turn-based flow with time limits, and power calculation with ongoing modifiers
- **Card & Zone Mechanics**: Support card types with costs and power, zone effects that can be added without core changes, and modular effect processing
- **Draft & Roguelike Progression**: Implement initial draft (pick 8 from 3-choice selections with copy limits), reward system (card picks with perks, deck substitution, life system), and fight sequence through sub-bosses and final boss (15 fights total)
- **AI Opponent**: Create functional AI opponent for combat
- **Content**: Add initial test content (3 test zones, 20 effects across 5 types, 3 perks, starter card pool)
- **UI**: Build all menus, combat UI, draft UI using only MonoGame primitive drawing

## Capabilities

### New Capabilities
- `game-framework`: Core game infrastructure including scene management, state machine, services (input, time, random), and primitive UI drawing system
- `effect-system`: Pluggable effect architecture with triggers (OnReveal, Ongoing, EndOfTurn, Activate), effect processor, power resolution, and registry for extensibility
- `combat-system`: Zone-based combat with 3 zones, priority calculation based on zone control, card reveal ordering, turn phases (start/draw/reveal zones/play/end), time limits, and power calculation
- `card-mechanics`: Card data model with costs, power, trigger types, and effect attachments; support for Activate abilities
- `zone-mechanics`: Zone system with effects, play constraints, control calculation, and extensible zone behavior
- `draft-system`: Initial deck building (8 cards from 3-choice selections, max 2 copies per card), perk assignment to last 3 cards
- `roguelike-progression`: Run state tracking, fight sequence (15 fights with sub-bosses/final boss), reward mechanics (win/loss rewards), life system (max 2 lives), deck management (up to 12 cards)
- `ai-opponent`: Basic AI opponent that follows same game rules
- `content-data`: Initial game content definitions (test zones, effects, perks, card pool)

### Modified Capabilities
<!-- None - greenfield project -->

## Impact

**Affected code:**
- `Game1.cs`, `Program.cs` - integrate new systems
- New directories: `Core/`, `Data/`, `Effects/`, `Gameplay/`, `UI/`, `Scenes/`
- All game logic is new code

**Dependencies:**
- MonoGame.Framework.DesktopGL 3.8.* (already present)
- MonoGame.Content.Builder.Task 3.8.* (already present)
- No new external dependencies - must use only MonoGame built-ins

**Constraints:**
- No image assets - all UI drawn with MonoGame primitives (SpriteBatch + shapes)
- Must be modular and extensible without core changes
- Must follow design patterns (Strategy, Registry, Factory, State, Observer, Command)
- Time limits per turn (if exceeded, play what was played and proceed)
- All combat must complete within reasonable time per requirements
