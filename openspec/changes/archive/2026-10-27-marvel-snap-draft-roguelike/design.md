# Design

## Context

See proposal.md - Why. This is a greenfield MonoGame project (empty skeleton) requiring a complete game implementation with complex mechanics. Must use only built-in MonoGame capabilities, no external images, and be fully extensible.

## Goals / Non-Goals

**Goals:**
- Build a modular, extensible architecture using design patterns (Strategy, Registry, Factory, State, Observer, Command)
- Implement pluggable effect system supporting multiple trigger types without core changes
- Create zone-based combat with proper priority and reveal ordering
- Implement roguelike draft and progression system
- Use only MonoGame built-ins for all UI (primitive drawing)
- Make adding new cards/zones/effects/perks easy

**Non-Goals:**
- Multiplayer/networking
- Save/load system (prototype focus)
- Advanced AI (keep simple functional AI)
- External asset pipeline or image loading
- Third-party libraries (only with approval)

## Decisions

### Decision 1: Effect System Architecture
**Choice:** Strategy pattern with Registry and EffectProcessor
- **Rationale:** Allows adding new effects without modifying core. Each effect implements `IEffect` interface, registered by ID. EffectProcessor handles triggering based on game events.
- **Alternatives considered:** Hardcoded if/else chains (not extensible), Component system (overcomplicated for this scope)

### Decision 2: Scene Management
**Choice:** Scene-based architecture with State Machine
- **Rationale:** Clean separation of Menu, Draft, Combat, Reward, etc. State machine manages flow.
- **Alternatives considered:** Single monolithic update loop (hard to maintain)

### Decision 3: UI Rendering
**Choice:** Primitive drawing with SpriteBatch + 1x1 texture
- **Rationale:** Meets requirement of "no images", uses only MonoGame built-ins. Create procedural 1x1 white texture for rectangles.
- **Alternatives considered:** None - this is required

### Decision 4: Power Calculation
**Choice:** Dynamic power resolution with ongoing modifier accumulation
- **Rationale:** Power changes as board state changes (ongoing effects). Need PowerResolver to compute current power on-demand.
- **Alternatives considered:** Cached power (would need complex invalidation)

### Decision 5: Reveal Ordering
**Choice:** Priority owner first (all their played cards in play order), then other owner
- **Rationale:** Matches requirements exactly - "se revelaran primero las del jugador/ia quien tenga la prioridad" and "de acuerdo al orden el cual fueron reveladas"
- **Alternatives considered:** Interleaved by some other rule

### Decision 6: Priority Calculation
**Choice:** Count zones controlled by each side at end of turn; most zones wins
- **Rationale:** Matches "quien vaya controlando más zonas". Tie-breaking needs definition - suggest retaining previous priority or player priority on tie (make configurable).
- **Alternatives considered:** Total power across all zones

### Decision 7: Data-Driven Content
**Choice:** C# definitions with Registries for prototype
- **Rationale:** Easier in C# without extra dependencies. Keep definitions in Data/ folder, separate from logic.
- **Alternatives considered:** JSON loading (would need JSON library or custom parser - adds complexity)

### Decision 8: Turn Timer
**Choice:** Configurable time limit per play phase
- **Rationale:** Requirements state time limit exists. Default to reasonable value (e.g., 45 seconds) for prototype.
- **Alternatives considered:** No timer (not matching requirements)

## Risks / Trade-offs

- **[Complexity]** Effect system is complex but necessary for extensibility. *Mitigation:* Start with simple effects, build incrementally.
- **[Time constraints]** 15 fights in reasonable time with time limits per turn. *Mitigation:* Keep AI simple and efficient, tune time limits.
- **[MonoGame limitations]** No built-in UI framework, must build everything from scratch. *Mitigation:* Keep UI simple, focus on functionality.
- **[Ambiguous rules]** Some details unspecified (exact energy curve, tie-breakers). *Mitigation:* Make reasonable assumptions, document as configurable, can adjust.
- **[Performance]** Power recalculation on every change could be expensive. *Mitigation:* For prototype scale (max 4 cards per zone, 3 zones) it's fine.
