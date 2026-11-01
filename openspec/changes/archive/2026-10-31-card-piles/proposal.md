# Proposal

## Why

Cards can only live in the hand, the deck or a zone. Nothing records what left the board, so an effect such as "OnReveal: discard the highest-cost card in your hand" has no destination and no way for the player to inspect the result. Piles need to exist, with their lifecycle and their viewer, before any effect is written against them; otherwise every future effect invents its own removal path.

## What Changes

- **Three combat piles per side**: `Descartadas`, `Destruidas` and `Desterradas`, for the player and the AI, held in `BoardState` and reset at the start of every combat.
- **Every card knows where it lives**: `CardInstance` gains a location marker (`Deck`, `Hand`, `Board`, `Discard`, `Destroyed`, `Banished`) so a card cannot sit in two containers at once.
- **One movement gate**: `CardPileManager.Move(card, side, pile)` removes the card from hand, deck or zone, clears `IsInPlay`, `PlayedTurn` and `PlayOrderIndex`, appends it to the pile, removes it from `RunState.Deck` when it is banished, and bumps `BoardState.Version`.
- **Pile lifecycle**: at the end of a combat, discarded and destroyed cards return to the deck for the next combat because they never left `RunState.Deck`; banished cards are removed from the run and never return.
- **Board views stay in sync**: `CombatScene` compares `BoardState.Version` against the last synced value, rebuilds hand views, reconciles slot views, recalculates zone power and cancels a drag whose card no longer exists.
- **Pile viewer on both avatars**: clicking the portrait rectangle of either avatar opens a compact popover with three tabs (`DESTERRADAS`, `DESCARTADAS`, `DESTRUIDAS`), each showing its count, three cards per page with page arrows and an empty state.
- **Viewer behavior**: anchored below the player avatar on the left and below the rival avatar on the right; clicking outside or on the avatar closes it; while open it blocks board clicks only, and the turn timer keeps running, closing the viewer when it expires.
- **Honest empty state until effects exist**: nothing in this change moves a card into a pile on its own, so the viewer opens empty in normal play and is exercised through a temporary harness outside the repository.
- **No effects**: no OnReveal, Ongoing, EndOfTurn or Activate effect is implemented or wired; the pile API is the seam future effects use.

## Capabilities

### New Capabilities

- `card-piles`: per-side discard, destroyed and banished piles, their combat lifecycle, the single movement gate, view synchronization, and the avatar popover that displays them.

### Modified Capabilities

None. The change is additive: existing contracts for card structure, zones, turns and layout stay valid. The run deck gaining a way to shrink when a card is banished does not contradict `roguelike-progression`, which only caps growth at 12.

## Impact

**Affected code:**

- `Gameplay/Cards/CardInstance.cs` - combat location marker.
- `Gameplay/Board/BoardState.cs` - per-side `PileSet`, `Version`.
- `Gameplay/Board/CardPileManager.cs` - new; the only path into a pile.
- `Gameplay/Cards/PileSet.cs` - new; three ordered card lists plus count helpers.
- `Scenes/CombatScene.cs` - version-based `SyncViews()`, avatar hit testing, viewer input priority, viewer drawing.
- `UI/Components/PileViewerView.cs` - new; popover frame, tabs, page navigation, card rendering.
- `Core/Layout/LayoutManager.cs` - popover rectangle derived from avatar bounds as percentages.
- `UI/Components/AvatarView.cs`- expose the clickable portrait region.
- `UI/Components/CardView.cs` - optional compact render used inside the popover.

**Verification:** temporary harness at `%TEMP%\opencode\eps-verify`, reusing the existing `Verify.csproj` and `Program.cs` that reference `SnapClone.csproj`. It seeds cards into each pile through `CardPileManager` and asserts containment, version bumps, banished removal from `RunState.Deck`, and the return of discarded/destroyed cards on the next combat. Visual checks (open, tab switch, pagination, close) are manual.

**Dependencies:** none new; MonoGame primitives and `DefaultFont` only.

**Assumptions recorded for review:**

- Three cards per page with arrows, because a card drawn at avatar width would be illegible.
- The timer keeps running while the viewer is open; inspection must not buy free time.
- Destruida originates on the board, Descartada from hand or deck, Desterrada from any origin.
- The visible term is `DESTERRADAS`.
