# Proposal

## Why

The combat scene is technically playable, but the game loop around it is not: `DraftScene` is a placeholder that jumps straight to combat, the board lays out four cards per side in a single row (37x53 px slots that are hard to hit on a portrait phone), a played card can never be moved, the energy badge is a hardcoded `4`, and neither side draws or plays cards per turn. The archived `draft-system` capability already describes an 8-card draft that exists in code but is never invoked.

This change makes the draft real and turns combat into a complete six-turn loop with an opponent that actually plays, using the Marvel Snap portrait reference as the visual target and MonoGame primitives only.

## What Changes

- **Real 8-card draft**: 8 rounds of "pick 1 of 3", maximum 2 copies per card, 20-card pool. Picks are written to `RunState.Deck` and handed to combat.
- **2x2 board per side**: each of the 3 locations shows 2 columns x 2 rows for the rival and the player, replacing the current 1x4 row. Slots grow from ~37x53 px to ~78x105 px (~4.2x hit area).
- **Compact location plaque**: name, effect and both power totals in a shorter plaque (0.345-0.470 H); the location art becomes a low-opacity backplate instead of a dedicated 56%-height block.
- **Re-dragging**: a card on the board can be dragged back to the hand and replayed in another slot, or moved directly to another free slot, while the turn timer is running.
- **Magnetic drop**: dropping anywhere on the player half of a location snaps to the nearest free slot, and the target slot is highlighted during the drag.
- **Timer cancel**: when the turn timer reaches 0 with a card in the air, that card returns to the hand and the turn closes.
- **Energy 1..6 as display only**: the badge shows the current turn number; energy does not filter, spend or refund plays in this prototype. **BREAKING** relative to the archived `combat-system` requirement that plays satisfy energy/cost.
- **Per-turn draws**: at the start of each turn both sides draw 1 card; an exhausted deck is not an error.
- **Rival plays**: the AI plays exactly one card per turn when a free slot exists, spreading across zones instead of stacking the first one. Its hand is not rendered.
- **20-card catalog**: 12 new card definitions with distinct cost/power lines.
- **Scene reset on entry**: entering Draft or Combat again starts a clean run instead of reusing the previous board state.

Out of scope: deck growth from 8 to 12 with add/remove UI, a real `RewardScene`, snaps, card effects, energy as a real resource, priority/reveal ordering, audio and external assets.

## Capabilities

### New Capabilities

<!-- None: the change corrects and completes behavior already described by existing capabilities. -->

### Modified Capabilities

- `draft-system`: the 3-choice/8-pick loop becomes a real playable scene; candidate rows never offer a card already at 2 copies; the completed deck is handed to combat.
- `combat-system`: 2x2 slot layout per zone; six turns with a per-turn draw for both sides; energy is informational and does not gate plays; the turn loop is owned by a single turn authority instead of a duplicate counter in the scene.
- `card-drag-drop`: drops resolve by column proximity instead of exact slot bounds; cards can be dragged from the board back to the hand or to another slot; timeout and invalid drops return the card to its defined origin.
- `ai-opponent`: the AI plays exactly one card per turn, spreads across zones, and is not gated by energy while energy is display-only.
- `content-data`: the starter card pool grows from 8 to 20 definitions.
- `turn-flow-modal`: the energy badge reflects the turn (1..6), the end-turn button shows the turn count, and turn start performs the draw step.
- `ui-responsive`: new vertical bands and 2x2 slot geometry for the portrait combat layout, including a compact location plaque.

## Impact

**Affected code:**
- `Data/Content/ContentLoader.cs` - 12 new card definitions
- `Gameplay/Draft/DraftSystem.cs`, `Scenes/DraftScene.cs` - real draft flow and deck handoff
- `Gameplay/Run/RunState.cs` - new-run reset and drafted deck
- `Gameplay/Board/BoardState.cs` - energy display value, deck/hand usage
- `Gameplay/Combat/TurnManager.cs` - single turn authority: draws for both sides, energy, phases
- `Gameplay/AI/AIStrategy.cs`, `Gameplay/AI/AIController.cs` - one play per turn, zone spread, slot result
- `Core/Layout/LayoutManager.cs` - 2x2 slots and new bands
- `UI/Components/LocationView.cs`, `CardView.cs`, `DropZone.cs` - compact plaque, drag origin, slot highlight
- `Scenes/CombatScene.cs` - draws, re-drag, magnetic drop, AI binding, energy and button text
- `Scenes/SceneManager.cs` / `IScene.cs` - reset on scene entry
- `Game1.cs` - register `RunState`, `DraftSystem`, `AIController`

**Dependencies:**
- No new external dependencies. MonoGame primitives and the existing `DefaultFont` only.

**Constraints:**
- Portrait layout must remain adaptive to any viewport size.
- Drafting and card play only during their allowed phases.
- Nine cards are needed to cover 3 initial + 6 draws; an 8-card deck is accepted and the last turn simply draws nothing.
- Energy remains a visual indicator until a later change turns it into a real resource.
