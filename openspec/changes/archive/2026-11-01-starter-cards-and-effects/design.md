# Design

## Context

See `proposal.md` for motivation and `specs/` for the behavioral contract.

The engine already has most of the seams this change needs, but none of them are connected:

- `EffectTrigger` declares OnReveal, Ongoing, EndOfTurn and Activate. `EffectProcessor.ProcessTrigger` has empty branches, `BaseEffectFactory` only knows a dead zone effect, and nothing outside `Effects/` creates a processor.
- `RevealResolver.GetRevealOrder` already selects the cards played this turn and orders them by priority owner and `PlayOrderIndex`. `TurnManager.EndTurn` calls it and throws the result away.
- `CardInstance` has `PlayedTurn` and `PlayOrderIndex`, but not the zone where a card was played.
- `PowerResolver` only computes `BasePower + perk`; `ZoneInstance.GetPlayerPower()` is a raw sum of `CurrentPower`.
- Zone totals are computed in four places: `PriorityCalculator`, `VictoryResolver`, `DetailPreviewView` and `LocationView`. The first three use `ZoneInstance`; the plaque sums `CardView.Card.CurrentPower`.
- `ContentLoader` registers 20 generic cards with generic text and 18 effects, 15 of which are unreferenced placeholders.
- `CombatScene.DealStartingHands` deals three random cards per side, then `StartTurn` draws one more.

The code already contains `CardDef.Description` and the detail panel from the completed (not yet archived) `card-inspect-and-zone-reveal` change. This change treats that as the current code and only adds the content and behavior on top.

## Goals / Non-Goals

**Goals:**

- Make the first 20-card pool feel like Marvel Snap: 13 named starter cards plus 7 vanilla fillers, with ability text on the cards that have abilities.
- Turn On Reveal and Ongoing into real, testable behavior with the smallest engine that can support the seven ability cards.
- Keep power derived and idempotent: no path writes `CurrentPower +=`.
- Make preview, plaque, priority and victory read the same zone total.
- Keep the turn flow unchanged: no new phase, no animation, no input block.

**Non-Goals:**

- EndOfTurn and Activate effects. They remain declared trigger types but no content uses them.
- Zone effects. The three zone definitions and their effect ids remain registered but unwired, as today.
- Animations, reveal overlays, card art, or a visual effect log.
- Changing the AI's card-choice heuristic, draft odds, reward odds, run length or deck size.
- Cross-combat power progression. Persistent bonuses are combat-scoped; perks remain the only cross-combat progression.

## Decisions

### Reveal at TurnEnd, instantly

`TurnManager.EndTurn` becomes the single authority for the end-of-turn sequence:

```
FINALIZAR TURNO
  |
  +--> stop timer, CurrentPhase = TurnEnd
  |
  +--> revealOrder = RevealResolver.GetRevealOrder(board)
  |       player-with-priority cards first, then the other side
  |       each side in PlayOrderIndex order
  |
  +--> EffectProcessor.ResolveEndOfTurn(board, revealOrder)
  |       for each card in revealOrder: apply its OnReveal effects
  |       after the sequence: resolve pending delayed bonuses (Hawkeye)
  |
  +--> PowerResolver.UpdateAllPowers(board)
  |       base + perk + BonusPower + Ongoing for every card
  |
  +--> PriorityCalculator.CalculatePriority(board, previousPriority)
  |       uses zone totals that already include Ongoing and multipliers
  |
  +--> CurrentPhase = TurnStart
  |
CombatScene.EndTurn
  |
  +--> if match over: RefreshPowerTotals(); EvaluateMatch(); return
  +--> StartTurn(); RefreshHandViews(); PlayAIAll(); UpdateViewLayout()
```

Why this order: priority currently runs before `UpdateAllPowers`, so the next turn's reveal order is decided with stale power. Resolving effects first also makes the last turn's result use the final board.

Why no reveal phase: the user chose instantaneous resolution. A new phase would need input blocking, timers, view state and a "revealed" flag per card, and would not change any rule. The prototype already shows cards face-up from placement; "reveal" is a logical event at turn end, not a visibility change.

Why `TurnManager` owns it: `CombatScene` already delegates turn advancement to `TurnManager`, and the harness can exercise `TurnManager.EndTurn` without a MonoGame window.

### Triggered and continuous effects are different interfaces

- `IEffect` stays the triggered contract: `EffectId`, `Trigger`, `Apply(EffectContext)`.
- `ICardPowerModifier` is the continuous contract for card power: `int GetPowerBonus(EffectContext)`.
- `IZonePowerMultiplier` is the continuous contract for a zone total: `int GetMultiplier(EffectContext)`.

`EffectProcessor` looks up an effect id in `EffectRegistry`, asks `BaseEffectFactory` to create the implementation on first use, and caches it. Effects are stateless; all mutable state lives on `CardInstance` or `BoardState`. That is what makes caching safe and what keeps the power calculation repeatable.

Why not keep the single `Apply` for Ongoing: an Ongoing effect that runs once and mutates `CurrentPower` accumulates on every recalculation. Deriving Ongoing through a query makes idempotence structural rather than a discipline.

Why not a pure data interpreter: Sentinel creates a card, and Star-Lord and Hawkeye inspect board history. Some effects need code. The factory maps each `EffectType` to a small class; the registry still stores data.

### Effect context carries owner, board and registry

`EffectContext` gains:

- `BoardSide Owner`: player or AI. Needed by Sentinel (which hand) and by effects that compare allies and enemies.
- `CardRegistry CardRegistry`: needed by Sentinel to create a new instance from a definition id.

The existing `SourceCard`, `SourceZone`, `BoardState`, `TurnNumber` and `PriorityOwner` stay. `Targets` remains for future effects; this iteration does not populate it.

### Card instance state

`CardInstance` gains:

| Field | Meaning | Reset |
|---|---|---|
| `BonusPower` | Persistent bonus from resolved OnReveal effects (Medusa, Star-Lord, Hawkeye). | Start of each combat. Kept when the card moves or returns to hand within the combat. |
| `PlayedZoneIndex` | Zone index where the card was played from hand. | Set on play from hand; kept on board moves; cleared on return to hand and at combat start. |
| `PendingBonusTurn` | Turn in which a delayed condition was armed. | Cleared when the condition resolves or does not apply. |
| `PendingBonusZoneIndex` | Zone watched by the delayed condition. | Same as above. |
| `PendingBonusAmount` | Bonus to apply if the condition is met. | Same as above. |

Why a separate `BonusPower` instead of reusing `PerkPowerBonus`: perks are attached through the reward system and are recomputed from `AttachedPerks` on every `ResolveCard`; OnReveal bonuses are combat events that must survive a zone move and must not be wiped by a perk recalculation. Keeping them separate keeps both derivations honest.

Why fields instead of a list of modifiers: this iteration only needs flat, once-per-reveal bonuses. A list can replace the fields later without changing the effect contract.

`BuildDecks` already normalizes combat state for cards coming from the run deck (`IsInPlay`, `PlayedTurn`, `PlayOrderIndex`, `Location`). It will also clear `BonusPower`, `PlayedZoneIndex` and the pending fields, because power changes from one combat do not carry into the next.

`CombatScene.PlaceInSlot` sets `PlayedZoneIndex` only when the card came from hand; moving a card already in play preserves the original zone. `AIController.TryPlayOne` does the same. `AddToHand` clears the play state and pending fields; it keeps `BonusPower` and perks.

### One power pipeline

```
Card power:
  base + perk + BonusPower + Ongoing(card, zone, board, owner)

Zone total:
  sum(card power for the side) * product(zone multipliers for the side)

Consumers:
  PriorityCalculator --------+
  VictoryResolver -----------+
  DetailPreviewView (zone) --+--> ZoneInstance.GetPlayerPower()/GetAIPower()
  LocationView (plaque) -----+        |
                                      +--> BoardState.PowerResolver.CalculateZonePower()
CombatScene result totals ---+
CardView / DetailPreviewView (card) --> CardInstance.CurrentPower
                                      +--> PowerResolver.UpdateAllPowers()
```

`BoardState` holds the `PowerResolver` for the combat. `ZoneInstance` gets a `Board` reference when the board is built, so `GetPlayerPower()` and `GetAIPower()` can delegate to `CalculateZonePower` instead of summing `CurrentPower`. If no resolver is attached (tests or old construction), the zone falls back to the raw sum of `CurrentPower`.

`PowerResolver` exposes:

- `CalculateCardPower(card, zone, board, owner)`: pure; uses base, perks, `BonusPower`, and the `ICardPowerModifier` query. It never reads `CurrentPower`.
- `CalculateZonePower(zone, side)`: sums `CalculateCardPower` for the side and multiplies by the `IZonePowerMultiplier` query for each card on that side.
- `UpdateAllPowers(board)`: writes `PerkPowerBonus` and `CurrentPower` for every card in zones, hands and decks. Hand and deck cards pass a null zone, so only base, perks and `BonusPower` apply.
- `ResolveCard(card)`: existing one-argument form used by `RewardScene` and deck setup; equivalent to a card outside a zone.

`LocationView` loses its own sum. It gains `SetPower(int player, int opponent)`, and `CombatScene.RefreshPowerTotals()` is the only writer: it calls `UpdateAllPowers`, then copies `zone.GetPlayerPower()` and `zone.GetAIPower()` into each plaque.

`RefreshPowerTotals` runs after every board mutation: player play or move, player card returned to hand, AI play, pile sync, end-of-turn resolution, and before match evaluation. This is what makes Punisher live instead of end-of-turn-only.

Why the board owns the resolver: zone power is a derived property of the board, and every consumer must see the same answer. Passing a resolver to five call sites would work but invites one site to keep summing `CurrentPower`. The coupling is intentional and contained in `BoardState` / `ZoneInstance`.

Why Iron Man does not change card power: in Marvel Snap the location total doubles while each card keeps its printed power. Mutating card power would make Hulk show 28 instead of 14 and would make the plaque and the card disagree. The multiplier is applied only to the zone total.

Idempotence:

- `CalculateCardPower` reads definition, perks and `BonusPower`, never `CurrentPower`.
- `BonusPower` changes only when an OnReveal effect resolves, and each effect resolves once per reveal.
- `CalculateZonePower` recomputes the multiplier product from the current cards every time, so moving Iron Man in or out changes the total without storing anything.
- Calling `UpdateAllPowers` twice produces the same values.

### Ability semantics

| Card | Trigger | Implementation | Rule |
|---|---|---|---|
| Hawkeye | OnReveal | `HawkeyeOnRevealEffect` | Arm a pending +5 on the turn it is revealed, watching `PlayedZoneIndex`. At the end of the following turn, if any allied card on the board has `PlayedTurn == current` and `PlayedZoneIndex == watched zone`, add +5 to `BonusPower` and clear the pending state. |
| Iron Man | Ongoing | `IronManOngoingEffect` | Each copy contributes a x2 multiplier to its side's zone total. Two copies multiply by four. Card power stays 0. |
| Medusa | OnReveal | `MedusaOnRevealEffect` | If `SourceZone.ZoneIndex == 1`, add +3 to `BonusPower`. The zone is the one she is in when revealed, so moving her into the middle before turn end counts. |
| Punisher | Ongoing | `PunisherOngoingEffect` | Add +1 per enemy card in its current zone. Recomputed after every play, move or removal. |
| Quicksilver | Opening hand | `OpeningHandDealer` + `starts_in_hand` tag | Move every tagged copy from the deck to the hand before the random fill, for both sides. |
| Sentinel | OnReveal | `SentinelOnRevealEffect` | Create a new `CardInstance` from `CardRegistry.Get("card_sentinel")` and add it to the owner's hand. No hand limit. The copy has the same effect and can create another Sentinel. |
| Star-Lord | OnReveal | `StarLordOnRevealEffect` | Scan every rival card on the board for `PlayedTurn == current` and `PlayedZoneIndex == SourceZone.ZoneIndex`. If at least one exists, add +4 to `BonusPower`. |

Two details that follow from the prototype's timing:

- Both sides place cards before the end-of-turn sequence, so Star-Lord can see a rival card that has not yet had its own OnReveal resolved. Reveal order changes the order of effects, not whether a card was played in the zone.
- Hawkeye watches the zone where each card was played, not where it sits at the end of the next turn. Moving Hawkeye or the ally after the play still counts.

`EffectProcessor.ResolveEndOfTurn` resolves OnReveal in reveal order first, then processes pending bonuses. That order lets any board change made by an OnReveal (none in this pool, but likely later) be visible to the delayed condition.

### Content and texts

Final pool: 13 starter cards (7 with abilities, 6 vanilla) + 7 vanilla fillers.

| Card | Cost | Power | Ability | Id |
|---|---|---|---|---|
| Abomination | 5 | 9 | none | `card_abomination` |
| Cyclops | 3 | 5 | none | `card_cyclops` |
| Hawkeye | 1 | 1 | OnReveal | `card_hawkeye` |
| Hulk | 6 | 14 | none | `card_hulk` |
| Iron Man | 5 | 0 | Ongoing | `card_iron_man` |
| Medusa | 2 | 2 | OnReveal | `card_medusa` |
| Misty Knight | 1 | 3 | none | `card_misty_knight` |
| Punisher | 3 | 3 | Ongoing | `card_punisher` |
| Quicksilver | 1 | 2 | opening hand | `card_quicksilver` |
| Sentinel | 2 | 3 | OnReveal | `card_sentinel` |
| Shocker | 2 | 4 | none | `card_shocker` |
| Star-Lord | 2 | 2 | OnReveal | `card_star_lord` |
| The Thing | 4 | 7 | none | `card_the_thing` |
| Spark | 0 | 1 | none | `card_spark` |
| Raven | 2 | 4 | none | `card_raven` |
| Champion | 3 | 4 | none | `card_champion` |
| Behemoth | 4 | 4 | none | `card_behemoth` |
| Juggernaut | 4 | 6 | none | `card_juggernaut` |
| Titan | 5 | 5 | none | `card_titan` |
| Leviathan | 6 | 8 | none | `card_leviathan` |

Final curve: cost 0 = 1, 1 = 3, 2 = 5, 3 = 3, 4 = 3, 5 = 3, 6 = 2.

Ability texts (ASCII only, no accents):

- Hawkeye: `Al revelarse: si juegas una carta aqui el siguiente turno, gana +5 de Poder.`
- Iron Man: `Continuo: duplica el Poder total de tu zona.`
- Medusa: `Al revelarse: si esta en la zona central, gana +3 de Poder.`
- Punisher: `Continuo: +1 de Poder por cada carta enemiga en esta zona.`
- Quicksilver: `Empieza siempre en tu mano inicial.`
- Sentinel: `Al revelarse: agrega una Sentinel a tu mano.`
- Star-Lord: `Al revelarse: si el rival jugo una carta en esta zona este turno, gana +4 de Poder.`

Vanilla texts are generic role descriptions in the same style as the current pool. The four existing filler texts (Spark, Raven, Champion, Behemoth, Juggernaut, Titan, Leviathan) already fit and are reused.

Effect definitions:

| Id | Type | Trigger | Parameters |
|---|---|---|---|
| `effect_hawkeye_on_reveal` | `OnRevealArmNextTurnBonus` | OnReveal | `amount = 5` |
| `effect_iron_man_ongoing` | `OngoingZonePowerMultiplier` | Ongoing | `multiplier = 2` |
| `effect_medusa_on_reveal` | `OnRevealBonusIfMiddle` | OnReveal | `amount = 3`, `zoneIndex = 1` |
| `effect_punisher_ongoing` | `OngoingPowerPerEnemyCard` | Ongoing | `amount = 1` |
| `effect_sentinel_on_reveal` | `OnRevealAddCardToHand` | OnReveal | `cardId = card_sentinel` |
| `effect_star_lord_on_reveal` | `OnRevealBonusIfEnemyPlayedHere` | OnReveal | `amount = 4` |

`LoadEffects` keeps the three zone effect definitions because the zones reference them, and replaces the fifteen placeholder effects with these six. No card references a placeholder. `BaseEffectFactory` gains the six branches and drops the `ZonePowerModifier` branch; `ZonePowerModifierEffect` is deleted because its `CurrentPower +=` path is the exact pattern this change forbids. Zone effects stay unimplemented, as they are today.

### Opening hand

`OpeningHandDealer.Deal(deck, hand, size)` is a small model class so the harness can test it without a scene:

1. Walk the deck from the end; move every card whose `Definition.Tags` contains `starts_in_hand` to the hand and set `Location = Hand`.
2. While the hand has fewer than `size` cards and the deck is not empty, move `deck[0]` to the hand.
3. If the tagged cards already exceed `size`, leave the hand larger; all tagged copies are kept.

`CombatScene.DealStartingHands` calls it for the player and the AI, then `StartTurn` draws the usual fourth card. Quicksilver is therefore guaranteed in the opening hand, and the AI gets the same guarantee.

Why not special-case the card name: the tag is data, so future cards can start in hand without touching the dealer. Why not force the hand to exactly three: the requirement says every copy starts in hand.

### UI refresh

- `CombatScene.RefreshPowerTotals()` is the only writer of plaque totals. It runs after every mutation and after `TurnManager.EndTurn`.
- `RefreshHandViews()` after `StartTurn` already rebuilds the player's hand; the Sentinel created during `EndTurn` is therefore visible on the next frame.
- `DetailPreviewView` needs no new code: it reads `Card.CurrentPower` and `Zone.GetPlayerPower()`, both of which now include the new calculations.
- `CardView` shows `CurrentPower`; a persistent `BonusPower` changes the number but does not get a separate badge. Only `PerkPowerBonus` keeps the green/red badge, because perks are the run-level progression the badge already communicates.

### AI

`AIController.TryPlayOne` records `PlayedZoneIndex` when it places a card. No strategy change is needed:

- Punisher's `CurrentPower` is refreshed after each play, so the AI sees the correct on-board value if it re-evaluates later in the same turn.
- Sentinel copies go to the AI hand and are played on a later turn through the existing loop.
- Quicksilver is dealt by the same opening-hand rule for both sides.
- The AI hand is not rendered; the copy is still playable.

## Risks / Trade-offs

- **Instant reveal has no feedback.** The player sees power numbers jump when the turn ends. This is accepted for the first iteration; a summary overlay can be added later without changing the engine.
- **"Reveal" is logical, not visual.** Cards are face-up from placement, so Star-Lord can count a rival card before its own OnReveal resolves. This matches the fact that both players place before the reveal sequence.
- **Hawkeye's watched zone is the played zone.** Moving Hawkeye or the ally does not change the condition. This is the agreed semantics, but it is easy to misread; the harness should cover the moved-ally case.
- **Star-Lord only sees cards still on the board.** A rival card played in the zone and then destroyed would not count. No destruction effects exist in this pool; a play log would be needed later.
- **Iron Man's total differs from the sum of the visible cards.** The plaque and panel show the doubled total while each card keeps its printed power. This is intentional and matches Marvel Snap, but it can look like a bug without a multiplier indicator.
- **Multiple Iron Men multiply.** Two copies give x4, three give x8. The pool allows up to four cards per side, so the maximum is x16; no overflow risk.
- **Sentinel has no hand limit by design.** A long chain can grow the hand; the hand fan may overlap more, but the existing layout already handles variable hand sizes.
- **Quicksilver can make the opening hand larger than three.** Only if the deck contains more than three copies, which is unlikely but allowed by the rule.
- **Effect instances are cached and must stay stateless.** Future effects that need per-copy state must store it on the card, not on the effect object.
- **Combat-scoped bonuses reset between combats.** `BonusPower` is cleared in `BuildDecks`; only perks carry progression across the run. This is a deliberate assumption, not a persisted system.
- **The previous change is not archived.** The code already has `Description` and the detail panel, but the main `card-mechanics` and `content-data` specs still reflect the pre-description state. This change's content delta is self-contained; archive `card-inspect-and-zone-reveal` first if the merge order matters.
- **Zone effects remain unwired.** The three zone definitions keep their effect ids but no processor path applies them. This change does not make them worse; it removes their dead mutation implementation.
