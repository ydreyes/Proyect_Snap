# Design

## Context

See `proposal.md` for motivation. Current constraints that shape the approach:

- Rendering is MonoGame primitives (`ShapeDrawer`) plus one `SpriteFont`; no textures.
- `TurnManager.CurrentEnergy` is `min(turn, 6)` and purely cosmetic. `CombatScene.PlaceInSlot()` never checks cost. `PlaySystem.PlayCard()` validates against a hardcoded `0` and is never called.
- Cards placed in a zone stay there across turns; `CardInstance.PlayedTurn` records when each was played and is preserved when a card is moved between slots.
- `RunState`, `ProgressionManager`, `RewardSystem` and `PerkAssigner` exist but are never wired. `CurrentFightIndex` never changes, `RewardSystem` is not registered, and `RewardScene` is a placeholder that returns to the menu.
- `VictoryResolver` returns `Player`, `AI` or `Draw`; the scene renders `EMPATE` and then routes every outcome to `Reward`.
- `PowerResolver.UpdateAllPowers()` assigns `card.CurrentPower = CalculatePower(...)`, and `CalculatePower` returns `card.CurrentPower`, so any additive modifier would compound each evaluation.
- Perks exist as definitions (`+1`, `-1`, `+2`) and as IDs in `CardInstance.AttachedPerks`; nothing reads `Modifiers`.
- `CombatScene.SetupDecks()` copies the same `CardInstance` objects from `RunState.Deck` into the board on every entry, so per-combat fields persist between fights.
- The previous change `draft-8-combat-2x2` is complete but not archived, so its deltas are not in `openspec/specs/`.

## Goals / Non-Goals

**Goals:**

- Available energy derived from the board state, so refunds and multi-turn boards stay consistent without bookkeeping.
- One closed run flow: result modal → reward → next combat or retry, with a single source of routing truth.
- Perks that change power without compounding across evaluations.

**Non-Goals:**

- Card/zone effects, priority ordering, snap logic, boss-specific rules, audio.
- Deck editing outside the reward screen.
- Changing the 6-turn combat length or the 3-zone structure.
- New card or perk definitions.

## Decisions

### 1. Energy is derived, not a mutable counter

`TurnManager` exposes a pool and a derivation over the board:

```
pool(turn)      = min(turn, 6)
spent(card)     = card is in a player slot AND card.PlayedTurn == CurrentTurn
available       = pool - sum(cost of spent cards) - reserved
reserved        = cost of the dragged card when it came from a slot
                  and was played this turn
```

The reservation matters because `LiftFromSlot()` removes the card from `zone.PlayerCards` before the drop is known. Without it, lifting a 3-cost card refunds 3, allowing a 2-cost and a 1-cost to be played, and then the 3-cost to be re-placed for free.

Transitions:

```
hand -> slot     require cost <= available; stamp PlayedTurn = CurrentTurn
slot -> slot     PlayedTurn preserved; energy unchanged
slot -> hand     card leaves the board, so its cost is released automatically
hand -> hand     unchanged
```

Because affordability is recomputed, there is no second source of truth to desynchronize. A `slot -> hand -> slot` replay re-stamps `PlayedTurn` and is charged again.

Alternative considered: a mutable `CurrentEnergy` decremented on play and incremented on refund (rejected: the drag flow would have to remember which cards were charged, and any missed refund path silently corrupts the pool). Alternative considered: a separate `EnergySpent` field (rejected: duplicates information already carried by `PlayedTurn`).

`PlaySystem` is removed. It is dead code with a broken call, and the actual mutation path is `CombatScene.PlaceInSlot()`.

### 2. AI spends the same pool in a loop

`AIStrategy` filters the hand to cards with `cost <= AI available energy`, ranks them by `power / cost` (cost 0 ranks first), and returns one play at a time. `AIController.TryPlayAll()` repeats until no affordable card or no free slot, keeping the existing "fewest AI cards, random tie-break" zone spread.

The loop is not cosmetic. Power per energy in the current pool is strongly decreasing:

```
Rookie 1/3 = 3.00    Raven 2/4 = 2.00    Colossus 3/5 = 1.67
Juggernaut 4/6 = 1.50    Herald 5/7 = 1.40    Leviathan 6/8 = 1.33
```

An AI restricted to one card per turn would be forced onto expensive cards while the player dumps cheap ones, so paying energy would penalize only the AI. The AI's available energy uses the same derivation over `AICards`.

### 3. Run progression is a combat counter, not a step counter

`RunState.CurrentFightIndex` counts combats (0-based), so combat number is `index + 1`:

```
cycle        = (combatNumber - 1) / 5       // 0..2
position     = (combatNumber - 1) % 5 + 1   // 1..5

position 1, 3 -> card reward
position 2    -> perk reward
position 4    -> nothing
position 5    -> boss; cycle 2 is the FinalBoss
```

`IsRunComplete()` becomes `CurrentFightIndex >= 15`. This also fixes the current `FinalBoss` being unreachable: the old mapping put it at step 23 of a run cut off at step 15.

Routing, owned by `ProgressionManager.ResolveCombat(result)`:

```
Win + card/perk node -> pending reward, advance on completion -> Reward
Win + no reward      -> advance -> combat 16 becomes run victory (combat 15 wins the run)
Loss or Draw         -> lives-- ; if 0 -> GameOver ; else pending card reward, no advance -> Reward
```

After the reward screen: if `AdvanceAfterReward`, advance and check victory; otherwise re-enter the same combat.

### 4. Pending reward lives in `RunState`

`SceneManager.ChangeScene(string)` takes no parameters, so the reward scene cannot be told why it opened. `RunState` gains:

- `PendingRewardKind { None, Card, Perk }`
- `AdvanceAfterReward` (bool)

`RewardScene.OnEnter()` reads both, generates offers, applies the pick, then routes. This keeps the scene stateless between entries and survives `OnEnter` being called on every visit.

### 5. One reward screen, two sources

The screen always shows 3 cards, each with an offered perk, and the player picks one:

| Kind | Card source | Result | Deck size |
|---|---|---|---|
| `Card` | catalog, distinct, copy limit | becomes a new `CardInstance` with its perk | grows, or swap/skip at 12 |
| `Perk` | the player's deck, distinct | the chosen card gains the perk | unchanged |

At the cap the screen switches to a "choose one to remove" state with a skip action; skipping leaves the deck unchanged. The deck is never allowed above 12.

### 6. Power is base + modifiers, recomputed

```
power(card) = card.Definition.BasePower
            + sum(modifiers of perk ids in AttachedPerks)
            + (zone/card effects, when implemented)
```

`PowerResolver` no longer reads `CurrentPower` as an input. `UpdateAllPowers()` writes the resolved value, so calling it twice yields the same result. A perk attached by the reward screen triggers a resolution for that card.

### 7. Normalize card state on combat entry

Because `RunState.Deck` and the board share instances, `CombatScene.OnEnter()` resets `IsInPlay`, `PlayedTurn`, `PlayOrderIndex` and re-resolves `CurrentPower` for every deck card before dealing. This prevents state from a lost combat leaking into the retry, and is required once perks and turn-scoped energy read `PlayedTurn`.

### 8. Result handling

`VictoryResolver` keeps total power as the win condition. `Draw` is treated as a defeat everywhere (modal text, life loss, retry). The modal button no longer hard-codes `ChangeScene("Reward")`; it delegates to `ProgressionManager`.

### 9. Perk visibility

`CardView` draws a small badge with the perk's modifier (for example `+2`) near the power orb, on both hand and board rendering. Without it, a perk is invisible and the reward screen cannot be evaluated.

## Risks / Trade-offs

- **Dead early turns** → With 1 energy on turn 1, only 0/1-cost cards are playable; a top-heavy draft can leave the first turns empty. The draft already lets the player plan a curve. Accept for this change; a curve hint is a later UX improvement.
- **Perk reward shape is an assumption** → The spec defines the second combat of each cycle as "3 deck cards, each with a perk, pick one pair". If the intended shape is "pick a perk, then a card", only `RewardScene`'s perk state changes.
- **Unarchived previous change** → `draft-8-combat-2x2` declared energy informational. Archive it before this change, or its delta will overwrite the `Card Play` requirement at archive time. See Migration Plan.
- **Duplicate rewards** → `RewardSystem` can currently pick the same definition twice. Offers are built from a shuffled candidate list with removal.
- **12-card cap** → With 6 to 8 rewards per run, 2 to 4 screens become swaps or skips. This is intended curation, not a bug; the cap keeps 75% of the deck reachable within 9 drawn cards.
- **Stale hand power** → Hand cards only refresh `CurrentPower` when a resolution is triggered. Resolve at creation, at perk attachment and at combat entry.

## Migration Plan

1. Archive `draft-8-combat-2x2` first so its 2x2 layout and scene-reset decisions are in the main specs.
2. Apply this change on top. No data migration: `RunState` is in-memory and resets per run.
3. Rollback is code-only; no persisted state or content files are affected.

## Open Questions

None. The perk reward shape is recorded as an assumption above and does not block the task breakdown.
