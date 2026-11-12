# Design

## Context

See `proposal.md` - Why. The current implementation has the Series 1 and Series 2 foundation:

- `ContentLoader` registers 92 draftable cards, 3 zones, 28 perks, 23 enemies, 12 events, 6 modifiers and 8 items.
- `EffectProcessor` resolves `OnReveal`, `Ongoing`, `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn`, `AfterAddedToHand`, `GameStart`, `StartOfTurn` and `EndOfTurn`. `Activate` is declared but does not fire. There is no `EndOfGame` phase.
- `CardActionService` is the mutation authority for play, move, destroy, discard, draw, banish, add-to-hand, add-to-zone, copy, transform, merge, replace-zone, buff-top-card, draw-from-enemy-deck, shuffle, banish-lowest, add-energy, add-bonus-power and switch-sides. It does not expose draw, resurrection, copy-to-zone, zone swap, enemy-hand pull/discard or Max Energy.
- `CardInstance.EffectiveCost` is a property without board access: base + deltas, `CombatCostOverride` and a hard cap of 6. `PowerResolver` composes base + persistent bonus + ongoing bonuses + zone additions and multipliers. There is no set/reset/double power step.
- `BoardState` owns zones, hands, decks, piles, play log, energy ledgers, pending next-play entries, taunt reservations, move windows and visibility flags. It does not track destroyed cards, Max Energy deltas or zone-wide suppression.
- `AIStrategy` is hardcoded to `BoardSide.AI` and uses the same legality queries as `CardActionService`.
- `GlobalStoreCatalog` partitions 92 cards into 21 starters, 12 direct cards and 9 bundles (59 bundle cards).
- The `ResolveRepeatOnReveal` pass repeats every already-revealed card in a repeater's zone, not only the cards revealed on the same turn. A multi-turn Odin currently repeats more than intended; Wong will make that visible.

The new content needs hooks that do not exist yet: a live Activate trigger, an end-of-game phase, copied ability text, a board-aware cost pipeline, destroyed-card history, set/reset/double power, new actions, generalized restrictions, zone suppression and swapping, Max Energy changes and delayed reveal.

## Goals / Non-Goals

**Goals:**

- Add the 125 requested cards and keep the existing 92 cards unchanged except where S3 fixes a shared engine bug.
- Keep the existing Series 1 and Series 2 store bundles byte-for-byte stable while adding a separate Series 3 batch.
- Make the new rules derived and idempotent: no `CurrentPower +=` paths, no per-frame mutation.
- Keep every wave buildable and verifiable on its own.
- Keep the AI legal and non-stalling.
- Keep the UI legible at 720x1280 with the new Activate control, end-game state and zone labels.

**Non-Goals:**

- Redesigning the 23 enemy decks or adding new enemies.
- Adding locations beyond the existing three plus Limbo.
- Adding or rebalancing the 28 perks.
- A search-based AI or a full official rules emulator.
- A general scripting language for card abilities.
- Online or networked play.

## Decisions

### 1. Content batch, series model and wave plan

`CardSeries` gains `Series3`; `Series1` stays the default so existing definitions do not change. `RegisterSeries3` sets the value. All 125 cards are draftable and store content; none are starters. The starter set stays at 21.

The change is delivered in ten waves. Each wave registers its cards and raises `ExpectedDraftable`, so the build and harness stay green after every wave.

| Wave | Theme | Cards | Count | Pool after |
|---|---|---|---:|---:|
| 0 | Foundation, tokens, engine primitives, repeat fix | none | 0 | 92 |
| 1 | Simple local effects and auras | Drax, Green Goblin, Debrii, Doctor Doom, Rescue, Rockslide, Silver Surfer, The Hood, Thor, Wasp, Wong, Yellowjacket, Orka, Luke Cage, Omega Red, Red Skull, Typhoid Mary, Mojo, Cerebro, Patriot | 20 | 112 |
| 2 | Local effects, movement, hand/deck | Ronan, Dazzler, Polaris, Stegron, Magneto, Kingpin, Dagger, Human Torch, Spider-Man 2099, Lady Deathstrike, Venom, M.O.D.O.K., Selene, Hazmat, Gambit, Falcon, Ghost, Psylocke, Kitty Pryde, Aero | 20 | 132 |
| 3 | Small token, copy and deck effects | Agent Coulson, Attuma, Brood, Colleen Wing, Destroyer, Ghost-Spider, Maria Hill, Mirage, Nick Fury, Sauron, Shanna, Ultron, Viper, Zero | 14 | 146 |
| 4 | Cost, power and destroyed cards | Death, Knull, She-Hulk, Stature, Miles Morales, Sera, Quinjet, Zabu, Mobius M. Mobius, Wave, Baron Mordo, Beast, Bast, Valkyrie, Shadow King, Black Panther, Shuri, Taskmaster, Deadpool, Mister Negative | 20 | 166 |
| 5 | Draw, resurrection, enemy hand, Max Energy | Adam Warlock, Crystal, Maximus, Jane Foster, Ghost Rider, Hela, Doctor Octopus, Black Bolt, Moon Knight, Silver Samurai, Master Mold, Electro, Havok, Black Cat, Helicarrier | 15 | 181 |
| 6 | Copy-to-zone, swap, move reactions | Arnim Zola, Leader, Nimrod, Quake, Echo, Negasonic Teenage Warhead, Silk, Titania, Hercules, Nebula, Juggernaut, Spider-Man | 12 | 193 |
| 7 | Restrictions, zones, Limbo | Crossbones, Giganto, Sentry, Goose, Jean Grey, Magik | 6 | 199 |
| 8 | Activate and copied text | Absorbing Man, Mystique, Rogue, Super-Skrull, Black Widow, Hellcow, Lockjaw, Mysterio, Spider-Ham | 9 | 208 |
| 9 | End of game, delayed reveal, information | Captain Marvel, Dracula, M'Baku, Martyr, Invisible Woman, Snowguard, Agatha Harkness, Daredevil, Howard the Duck | 9 | 217 |

Why waves ordered this way: Wave 0 builds the primitives that Waves 4-9 depend on. Wave 1 and 2 prove the content pipeline with low-risk cards. Wave 4 is the cost/power refactor; it must land before any card that uses dynamic cost or set power. Waves 5-9 add the remaining actions and the invasive cards last, when the supporting queries already exist.

Alternative considered: register all 125 definitions in Wave 0 with no effect ids and fill them later. Rejected because `ValidateCardEffects` and the harness would be misleading, and the card text would claim abilities that do not work.

### 2. Store partition

`GlobalStoreCatalog` builds a third partition:

- Series 1 legacy: the 46 `Series1` non-starters, sorted by id, 6 direct + `bundle_6_a`, `bundle_6_b`, `bundle_6_c`, `bundle_7_a`, `bundle_7_b`, `bundle_8`.
- Series 2: the 25 `Series2` cards, sorted by id, 6 direct + `bundle_s2_6_a`, `bundle_s2_6_b`, `bundle_s2_7_a`.
- Series 3: the 125 `Series3` cards, sorted by id, 6 direct + 17 bundles of 7 (`bundle_s3_7_a` through `bundle_s3_7_q`).

`ExpectedDraftable` becomes 217, `ExpectedDirectCards` 18, `ExpectedBundles` 26 and `ExpectedBundleCards` 178. The flat `StoreCatalogData` exposes 18 direct ids and 26 bundles. `VisibleBundleCount` stays 3; the store rotates through 26 bundles and paginates direct cards 6 at a time.

Why 6 direct + 17x7: it preserves the existing 6-direct convention per series and does not touch legacy or Series 2 membership. Alternatives: 12 direct + 16 bundles changes the direct-card convention and store pacing; a single S3 mega-bundle breaks the existing bundle pricing model.

### 3. Bounded fidelity for the invasive cards

The requested "bounded fidelity" is implemented as explicit, deterministic rules:

- **Agatha Harkness**: `GameStart` moves her from the deck to the opening hand. On even turns (2, 4 and 6), after the start-of-turn draw, `AIStrategy` plays the player's legal cards with the player's hand and energy, then the scene ends the turn automatically. The AI does not move cards or use Activate. Control persists for the combat once Agatha has been drawn, even if she later leaves hand. Odd turns are normal.
- **Invisible Woman**: cards played in her zone gain `DelayedReveal`. They do not reveal at end of turn and their `OnReveal` effects do not fire; they still count their current power and are drawn face-down. At end of game they reveal in play order and resolve their `OnReveal` before `EndOfGame` effects. If Invisible Woman leaves play, the delayed cards reveal at the next end of turn.
- **Super-Skrull**: copies only the printed `Ongoing` effects of enemy cards in play, one level deep. It skips perks, temporary effects and effects that are themselves copied, and evaluates the copied effects with Super-Skrull's owner as the source side. It copies power/aura/zone-addition effects, not cost or restriction markers. No recursion.
- **Snowguard**: adds Hawk (3/3) and Bear (3/4), both non-draftable and keeping their official cost. Hawk sets a board-level suppression of zone effects until the end of the next turn; Bear re-triggers the current zone's simple effect once. At game end, zone effects resume for scoring.
- **Lockjaw**: Activate once per combat. It arms a pending swap on its zone; the next character played there is swapped with a random card from the owner's deck. The played card goes to the deck; the deck card enters play and reveals normally. No chained swaps; if the deck is empty or the zone is full, no swap.
- **Mysterio**: `OnReveal` adds a 0/0 Illusion to another zone with room. Activate once per combat swaps Mysterio's power with the Illusion's power if the Illusion is still in play. No disguise or identity ambiguity.

### 4. Tokens

| Token | Stats | Ability | Notes |
|---|---:|---|---|
| `token_widows_bite` | 0/-1 | While in hand, its owner cannot draw cards | Blocks draw in `DrawCard` and `DrawFromEnemyDeck` |
| `token_broodling` | 0/2 | None | Copies Brood's power at creation |
| `token_doombot` | 0/5 | None | Added to each other zone |
| `token_drone` | 0/2 | None | Four per other zone |
| `token_illusion` | 0/0 | None | Mysterio's swap target |
| `token_demon` | 0/6 | None | The Hood's token |
| `token_mjolnir` | 0/0 | On Reveal: give Thor +7 power | Finds the owner's Thor |
| `token_snowguard_hawk` | 3/3 | On Reveal: suppress zone effects until end of next turn | Keeps official cost |
| `token_snowguard_bear` | 3/4 | On Reveal: repeat the current zone effect | Keeps official cost |
| `token_void` | 0/-9 | None | Sentry's right-zone token |
| `card_sentinel` (reused) | 2/3 | On Reveal: add another Sentinel | Master Mold adds two; the chain is preserved |

All new tokens are `IsToken = true` and never enter the draft or store. `Hawk` and `Bear` intentionally keep their official 3 cost instead of the existing 0-cost token convention because their cost is part of their balance.

### 5. Cost pipeline

`CardInstance.EffectiveCost` remains as a compatibility property for static/legacy reads, but all gameplay decisions use a new `CostResolver` exposed through `CardActionService.GetEffectiveCost(card, side)` and `EffectProcessor.GetEffectiveCost(card, side, board)`. The resolver composes, in order:

1. `CombatCostOverride` (Sabretooth, Swarm, copies) or base cost + `CostDelta` + `TemporaryCostDelta` + `TurnCostDelta`.
2. Dynamic self reductions: Death (`-1` per destroyed card), She-Hulk (`-1` per unspent energy last turn), Stature and Miles (set to 1 when the condition holds).
3. Auras and container reductions: Sera (hand, minimum 1), Quinjet (created cards, minimum 1), Zabu (4-cost deck cards), Beast (returned cards next turn, minimum 0).
4. Taxes and increases: Sandman, Iceman, and any positive `TurnCostDelta`.
5. Absolute sets: Baron Mordo's enemy deck top card to 6 until turn 6.
6. Caps: Wave's global maximum of 4 until the end of the next turn; global maximum of 6.
7. Floor: 0, or 1 when the effect says minimum 1.
8. Mobius: if the card's owner has Mobius, skip positive increases; if the opponent has Mobius, skip reductions on the opponent's cards. Absolute sets count as increases when they raise the cost.

`CardActionService`, `AIStrategy`, `CombatScene`, `CardView`, `DetailPreviewView` and the collection card detail all call the resolver. The old `EffectiveCost` is not used for legality or payment anymore.

Why a resolver instead of adding more fields: S2 accumulated `TemporaryCostDelta`, `TurnCostDelta`, `CostTaxAppliedTurn` and `CombatCostOverride`; Series 3 would add six more sources. One ordered resolver is testable and prevents order bugs.

### 6. Power pipeline

`PowerResolver` gains an explicit composition:

1. `BasePowerOverride` if set (Bast, Valkyrie, Shadow King reset, Taskmaster copy), otherwise `Definition.BasePower`.
2. `PowerDelta + PerkPowerBonus + BonusPower` (existing).
3. `GetCardPowerBonus` (self modifiers + auras) and `GetZoneCardPowerBonus`.
4. `GetPowerMultiplier` (Apex, Human Torch, Shuri).
5. `GetZoneAddition` for the zone total.

`SetBasePower(card, value)` sets `BasePowerOverride`, clears `PowerDelta`/`BonusPower` and starts a fresh power identity. `ResetPower(card)` sets `BasePowerOverride = Definition.BasePower` and clears persistent and temporary bonuses; current Ongoing auras still apply. `DoublePower(card)` snapshots the resolved power at the moment of resolution and adds the difference to `BonusPower`, so repeated calculations are idempotent.

Why `BasePowerOverride` instead of writing `Definition.BasePower`: definitions are shared registry objects; mutating a definition would change every copy of the card. The override is per-instance and is cleared by `ResetForCombat`.

Specific cards:
- Bast: set every card in hand to base 3 and clear bonuses.
- Valkyrie: set every card in the zone to base 3 and clear bonuses.
- Shadow King: reset every card in the zone to its original base power and clear bonuses; ongoing auras still apply.
- Black Panther: double his resolved power once on reveal.
- Shuri: arm a next-play double at her zone; the next card played there doubles its resolved power once at end of turn.
- Taskmaster: set his base power to the current power of the last card the owner played (if that card is in play).
- Deadpool: when destroyed, return to hand with `BonusPower` equal to his resolved power, so each destroy doubles.

### 7. Destroyed-card tracking

`BoardState` gains a combat-scoped `DestroyedCards` list of `DestroyedCardRecord` (`Card`, `Owner`, `PowerAtDestruction`, `CostAtDestruction`, `Turn`). `CardActionService.DestroyCard` appends a record after a successful destroy, before the pile move. The list is cleared in `ResetCombatFlags`.

- Death's cost reduction counts records.
- Knull's `Ongoing` power sums `PowerAtDestruction`, including negative values.
- "This game" means the current combat in this roguelike, matching the existing Morbius and discard counters.

Why a separate history instead of reading the `Destroyed` pile: resurrected cards leave the pile, and pile viewers do not need to drive cost or Knull. A record is stable even if the card moves again.

### 8. Activate subsystem

`EffectTrigger.Activate` becomes live. `ActionEventKind.Activated` is added. `CardInstance` gains `ActivatedThisCombat`, cleared by `ResetForCombat` but not by `ClearPlayState` (so returning a card to hand does not reset its activation).

`CardActionService.ActivateCard(card, side)` validates that the card is in play, owned by the side, not ability-disabled, has an `Activate` effect and has not activated. It sets the flag, dispatches `Activated` with the card as the source, and applies the card's `Activate` effects through the normal effect processor. Activation costs no energy and does not create a `PlayRecord` or an `AfterPlayed` trigger.

UI: `DetailPreviewView` shows an `ACTIVAR` button for an owned in-play card with an unused Activate effect, placed below the description and above the close control. The button is hidden while the card is not owned, not in play, already activated, or ability-disabled. The scene calls `ActivateCard`, refreshes views and closes the preview.

The AI does not use Activate in this change; it plays the cards for their body. This is a deliberate scope bound, not a rules limitation.

Card-specific bounded rules:
- Black Widow: add a Widow's Bite to the opponent's hand if there is room.
- Hellcow: discard the rightmost card from the owner's hand.
- Lockjaw: arm the pending swap described in Decision 3.
- Mysterio: swap power with the Illusion.
- Spider-Ham: pick a random card in the opponent's hand and set `TextOverride = "oink!"` and `PrintedAbilitiesDisabled = true`; the stats remain.

### 9. End-of-game phase

`EffectTrigger.EndOfGame` is added. `TurnManager` gains an `EndGame` phase and a `ResolveEndOfGame` call after the turn-6 end-of-turn effects and before `VictoryResolver.Evaluate`. `CombatScene` inserts the call in `EvaluateMatch`, disables the end-turn button while resolving, and shows a `FIN DE LA PARTIDA` banner. Victory waits for the phase.

Resolution order:
1. Reveal `DelayedReveal` cards in play order and apply their `OnReveal`.
2. Resolve `EndOfGame` effects in a deterministic order: player side first, then AI; zone index; slot index.
3. Recompute powers after each effect.
4. Evaluate victory.

Bounds:
- Each `EndOfGame` card resolves once. Captain Marvel and Martyr evaluate all legal destination zones from the current board, pick the best/worst margin deterministically, and move once. No iterative re-evaluation.
- End-of-game movement uses the normal move action, but suppresses further `EndOfGame` triggers for the moved card.
- M'Baku jumps from the deck only if the owner's lowest-power zone is not full; he enters revealed and does not trigger `AfterPlayed`.
- Dracula discards a random card from hand, gains its power as `BonusPower`, and the discard fires the normal `AfterDiscarded` effects.

Why a real phase instead of resolving in the scene: the effect processor already owns trigger order; putting end-of-game in a resolver keeps Captain Marvel, Dracula, M'Baku and Martyr testable without a scene, and lets Invisible Woman share the same reveal pass.

### 10. Copied text and effect overrides

`CardInstance` gains `CopiedEffectIds` and `SuppressedEffectIds`. `PerkAbilities.EffectiveEffectIds` includes copied effects after printed/perks/temporary, and skips suppressed ids. `CardInstance.AbilityTextOverride` drives the detail panel.

- Absorbing Man: copy the printed `OnReveal` effects of the last card the owner played (excluding himself); if none, no effect.
- Mystique: copy the printed `Ongoing` effects of the last card played.
- Rogue: copy the printed `Ongoing` effects of an enemy card in the same zone into Rogue and add those ids to the enemy's `SuppressedEffectIds`.
- Super-Skrull: a special `SuperSkrullOngoing` effect evaluated by the power resolver, as described in Decision 3.
- Leader: copy the enemy card(s) with the highest power played this turn onto the owner's side, mark the copies revealed and do not resolve their `OnReveal`.
- Taskmaster: set base power to the last card's current power.

The UI shows the copied ability as a `COPIA: <name>` line and the original card text; Spider-Ham shows `oink!`; Super-Skrull shows the list of copied sources by name.

### 11. New actions

`ICardEffectActions` gains:

- `DrawCard(BoardSide side)` — respects hand cap, Widow's Bite draw lock and `Drawn` dispatch. Used by Adam Warlock, Crystal, Maximus and Jane Foster.
- `ResurrectFromPile(CardInstance card, BoardSide side, int zoneIndex)` — moves a card from the owner's discard pile to a zone with room, marks it revealed and does not resolve `OnReveal`. Used by Ghost Rider and Hela.
- `CopyToOtherZones(CardInstance source, BoardSide side)` — adds copies to the other zones with room, revealed, without `OnReveal`. Used by Arnim Zola and Nimrod.
- `SwapZones(int zoneA, int zoneB)` — swaps the definitions, card lists, flood state and play constraints between two zone positions, keeping `ZoneIndex` and the board reference. Used by Quake.
- `PullEnemyHandCard(BoardSide owner, int zoneIndex)` — pulls the lowest-power enemy hand card to the enemy side of the zone, marks it revealed and resolves `OnReveal`. Used by Doctor Octopus.
- `DiscardFromEnemyHand(BoardSide owner, string selection)` — discards from the enemy hand by least cost, even cost or lowest power; increments the enemy discard counter. Used by Black Bolt, Moon Knight and Silver Samurai.
- `AddToEnemyHand(BoardSide owner, string cardId)` — adds a card or token to the enemy hand if there is room and dispatches `AddedToHand`. Used by Black Widow and Master Mold.
- `ChangeMaxEnergy(BoardSide side, int delta)` — adds a per-side Max Energy delta on `BoardState`. The energy pool becomes `max(0, min(turn, cap) + runBonus + sideDelta)`. Used by Electro and Havok.
- `SetNextTurnEnergy(BoardSide side, int amount)` — adds a next-turn energy bonus consumed by `ResolveStartOfTurn`. Used by Psylocke.
- `MarkCreated(CardInstance card)` — sets `IsCreated`, used by Quinjet. `AddToHand` marks effect-added cards automatically; draws do not.

The existing `PlayCard`, `MoveCard`, `DestroyCard`, `DiscardCard`, `SwitchSides` and pile APIs remain the only mutation paths.

### 12. Restrictions, zones and Limbo

`EffectContext` gains `TargetZone`. `EffectProcessor.CanPlayCard` gains a zone-aware overload. `CardActionService.CanPlayCardInZone` and `AIStrategy.IsZoneUsable` use the same query.

- **Crossbones**: playable only where the owner's current power is strictly greater than the opponent's.
- **Giganto**: only zone index 0.
- **Sentry**: cannot be played at zone index 2. On reveal, add a Void to zone 2 if there is room.
- **Goose**: an `IZonePlayConstraint` on the zone rejects cards with effective cost 4 or more, for both sides.
- **Jean Grey**: a board-level check. If the player has not played a card this turn, there is an active Jean Grey zone, and the card can legally be played there, then plays in other zones are rejected. If the Jean Grey zone is full or illegal for that card, the constraint is skipped.
- **Electro**: owner-level. A side that has played one card this turn cannot play another; `PlayCard` and the AI check `PlayLog`.
- **Wave**: a board-level `GlobalMaxCost` of 4 with an expiry turn; the cost resolver applies it after reductions and before the floor. It affects both sides, hand, deck and created cards.
- **Magik / Limbo**: a new `zone_limbo` definition in `ZoneRegistry`. `ReplaceZoneWith(zoneIndex, "zone_limbo")` replaces the current zone if `CurrentTurn <= 5`; on turn 6 the effect does nothing. Limbo extends the match to 7 turns: `TurnManager.MaxTurnsFor(board)` returns 7 when any zone is Limbo, otherwise 6. `CombatScene` displays the dynamic maximum. Scarlet Witch's random replacement excludes Limbo.

Why swap zone contents instead of swapping `ZoneInstance` objects: card views, drop zones and play constraints hold references to the position index and the object. Swapping the contents keeps the position objects stable and avoids a cascade of reference fixes.

### 13. Reactions

`AfterPlayed` already fires for every in-play card. The new effects filter by event source, side and zone:

- Echo: when the event source is an enemy card in Echo's zone with a printed `Ongoing`, set `PrintedAbilitiesDisabled = true` on the source. This also prevents its `OnReveal`.
- Negasonic: the first enemy card played in her zone is destroyed; `NegasonicUsed` makes it once per combat.
- Silk: after any other card is played in her zone, move Silk to a random other zone with room. Her own play does not trigger it.
- Titania: after any other card is played in her zone, switch sides. Her own play does not trigger it.
- Hercules: the first time another allied card moves into his zone each turn, move it to another zone. A per-card `MovedByHerculesTurn` prevents loops; the normal 256-event guard bounds cascades.
- Nebula: at end of turn, if the opponent did not play in her zone this turn and it is not the turn she was played, gain +2.
- Kingpin: when an enemy card moves into his zone, afflict it with -2.
- Dagger: when she moves, gain +2 per enemy card in the destination zone.
- Human Torch: when he moves, double his power.
- Spider-Man 2099: `PassiveMovePermission` once per combat; when he moves, afflict an enemy card in the destination with -3.

### 14. AI

`AIStrategy` gains a `BoardSide` parameter and uses the same `CanPlayCard`, `CanPlayCardInZone`, cost resolver and side-level checks as the player. It filters cards and zones that the new restrictions reject, stops after one card when Electro limits the side, respects Jean Grey's forced zone, and never chooses a zone where Goose blocks the card.

Agatha's even-turn auto-play calls the same strategy with `BoardSide.Player`, plays all legal cards it can afford, and returns null when no play is legal; the scene then ends the turn. The AI does not use Activate, movement or new actions.

Why not teach the AI every new card: legality and no-stall are the requirements; optimal play is a non-goal. A misplayed Arnim Zola or Leader is acceptable.

### 15. Presentation

- `DetailPreviewView` gains the `ACTIVAR` button, the `COPIA: <name>` line and the `oink!` override.
- `CardView` draws `DelayedReveal` cards face-down with `???`; the detail panel hides an enemy delayed card's identity.
- `LocationView` shows `LIMBO`, `SILENCIADA` (Hawk) and the existing flooded/locked labels; the detail panel uses the same status.
- `CombatScene` shows the `FIN DE LA PARTIDA` banner, disables the end-turn button while resolving, and displays `TURNO n/7` when Limbo is active.
- `CollectionScene` and `GlobalStoreScene` counts and paging handle 217 cards, 18 direct cards and 26 bundles. The store still shows 3 rotating bundles.
- Card art remains procedural and lazy. 217 textures is a bounded increase; the cache is not pre-generated.

### 16. Verification

The repository harness grows with the content:

- Counts per wave: 112, 132, 146, 166, 181, 193, 199, 208 and 217 draftables; 125 Series 3; 21 starters; 18 direct cards; 26 bundles; 178 bundle cards.
- Store: legacy Series 1 and Series 2 bundle membership is identical to the 92-card catalog; the Series 3 batch is 6 direct + 17x7; purchases are atomic.
- Profile: Series 3 stays unowned until purchased or drafted; the starter set stays at 21; no profile schema change.
- Per-card checks: stats, description and one scenario per card, with seeded random where the target is random.
- Subsystem checks: cost precedence (Death, She-Hulk, Sera, Quinjet, Zabu, Mobius, Wave, Baron Mordo), power set/reset/double (Bast, Valkyrie, Shadow King, Black Panther, Shuri, Taskmaster), destroyed tracking (Death, Knull), Activate (five cards + once-per-combat), end of game (Captain Marvel, Dracula, M'Baku, Martyr, Invisible Woman), copied text (six cards + Super-Skrull recursion guard), new actions (draw, resurrect, copy-to-zone, zone swap, enemy hand, Max Energy), restrictions (Crossbones, Giganto, Sentry, Goose, Jean Grey, Electro, Wave), zone effects (Limbo turn 7, Hawk suppression, Bear repeat), reactions (ten cards) and the Odin/Wong multi-turn repeat fix.
- AI: no stall with Crossbones, Goose, Jean Grey, Electro, Wave or Agatha; illegal cards and zones are skipped.
- A full build and a manual combat pass per wave.

The external `%TEMP%\opencode\eps-verify` harness is legacy and predates the current run flow; it is not the source of truth. The repository harness is extended instead.

## Risks / Trade-offs

- **125 cards plus nine subsystems is large** -> Ten waves; each wave builds and has its own counts and checks. The change stays open for a long time, but no wave leaves a red build.
- **Cost pipeline refactor touches many call sites** -> One resolver; legacy `EffectiveCost` is kept for static reads; add a precedence test per cost family and a Mobius test for both sides.
- **Power set/reset semantics** -> Document that set effects override base power and clear bonuses, while ongoing auras still apply; Shadow King is tested with and without an aura.
- **Odin/Wong fix changes existing behavior** -> Add a multi-turn Odin test; the fix is a bug fix and is called out in the proposal.
- **Activate UI adds a button to the detail panel** -> Test at 720x1280 and ensure it does not overlap the close control.
- **End-of-game loops** -> Each card resolves once; Captain Marvel and Martyr evaluate a single move; no iterative passes.
- **Super-Skrull recursion** -> Printed Ongoing only, one level, no copied effects; a test with two Super-Skrulls confirms no growth.
- **Agatha removes agency** -> Only even turns, documented in the UI banner; the AI uses the same legality query.
- **Store has 26 bundles** -> The store already shows 3 rotating bundles and paginates direct cards; no new layout is required.
- **Hawk and Bear keep a 3 cost** -> This breaks the 0-cost token convention intentionally; the spec records the exception.
- **Limbo adds turn 7** -> `MaxTurns` becomes dynamic; the turn display and all end-of-turn guards use the instance value. A test confirms the seventh turn only when Limbo is active.
- **217 procedural textures** -> The lazy cache grows to at most 217 textures. The cache and disposal path keep it bounded.

## Migration Plan

1. **Wave 0 - foundation**: add `CardSeries.Series3`, the token definitions, `ExpectedDraftable` scaffolding, `CostResolver`, `BasePowerOverride`, `DestroyedCards`, the Activate/EndOfGame triggers, the new action interface, the zone/restriction queries, the repeat fix and harness helpers. The build and harness stay green at 92.
2. **Waves 1-3 - low-risk content**: 54 cards, the token families and the local effect classes. Counts rise to 146.
3. **Wave 4 - cost, power and destroyed cards**: 20 cards. Count 166.
4. **Wave 5 - draw, resurrection, enemy hand and Max Energy**: 15 cards. Count 181.
5. **Wave 6 - copy-to-zone, swap and reactions**: 12 cards. Count 193.
6. **Wave 7 - restrictions and Limbo**: 6 cards. Count 199.
7. **Wave 8 - Activate and copied text**: 9 cards. Count 208.
8. **Wave 9 - end of game, delayed reveal and information**: 9 cards. Count 217.
9. **Final integration**: collection/store counts, Activate button, end-game banner, delayed-reveal visuals, dynamic turn display, full harness and manual combat pass.

Rollback is a code revert. Content is additive; the new store bundle ids are new and do not repurpose legacy ids. The profile schema does not change because the new fields are content-only or combat-only. No starter migration is needed.

## Open Questions

None blocking. The remaining details are recorded as decisions: series 3 is store/draft content; the starter set stays at 21; enemy decks stay unchanged; "this game" means the current combat; Agatha controls even turns for the whole combat once drawn; Limbo extends to turn 7; Debrii adds a Rock to each side of each other location; Master Mold reuses `card_sentinel`; Hellcow discards the rightmost card; Dracula discards at random; Hawk and Bear keep their official 3 cost.
