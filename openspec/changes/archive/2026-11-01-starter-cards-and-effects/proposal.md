# Proposal

## Why

The card pool is twenty interchangeable bodies with generic descriptions, and the effect system is an empty skeleton: `EffectProcessor` never applies anything, `RevealResolver` computes a reveal order that `TurnManager` discards, and card power is only base plus perks. The game therefore has no card identity and no reason for the reveal order to exist.

This change introduces the first real card set and makes the engine resolve it. It is the smallest step that turns the existing plumbing (trigger enum, reveal order, effect registry) into visible gameplay: cards that do something when revealed and cards whose power is always on.

## What Changes

- **Replace the 20-card pool** with 13 named starter cards plus 7 vanilla fillers, still 20 cards. Seven of the starter cards have abilities (Hawkeye, Iron Man, Medusa, Punisher, Quicksilver, Sentinel, Star-Lord); the other thirteen cards are vanilla. The old generic Sentinel 6/5 is removed, so it no longer collides with the new Sentinel 2/3. Names use the official spelling: Medusa, Sentinel, Shocker, Punisher, Star-Lord.
- **Every card keeps a description**. Vanilla cards get a generic role text; ability cards get their ability text instead, using `Al revelarse:` for On Reveal and `Continuo:` for Ongoing. All texts are Spanish without accents, matching the existing font coverage.
- **On Reveal resolves instantly on FINALIZAR TURNO**. Cards played this turn are revealed in priority order, then play order, and each ability applies before the next card. No new phase, animation or input block; the next turn starts in the same frame as today.
- **Ongoing is derived, never accumulated**. Card power is always `base + perks + persistent bonus + ongoing`; zone total is the sum of card powers times the zone multipliers. Punisher gains +1 per enemy card in its zone. Iron Man doubles the allied zone total while every card keeps its own power.
- **Persistent bonuses survive movement**. Medusa gains +3 when revealed in the middle zone; Star-Lord gains +4 when the rival played in its zone that turn; Hawkeye gains +5 at the end of the next turn if an ally was played in the zone where Hawkeye was played. These bonuses live on the card instance, not in the current-power field.
- **Card creation and opening hand**. Sentinel adds a new Sentinel to its owner's hand when revealed, with no hand limit. Quicksilver starts in the opening hand of both sides, including the AI, and every copy does.
- **Priority is calculated after effects and power**. The current order (priority before power) is corrected so the next turn's reveal order uses the final board.
- **Remove dead effect content**. Placeholder effect definitions that no card references are replaced by the six real card effects; the unused `ZonePowerModifierEffect` mutation path (`CurrentPower +=`) is removed.

## Capabilities

### New Capabilities

- None. The change extends capabilities the project already declares.

### Modified Capabilities

- `effect-system`: On Reveal and Ongoing become real triggers; the processor resolves On Reveal in reveal order, Ongoing is queried during every power recalculation, and the effect context carries owner and card registry.
- `card-mechanics`: card instances gain a persistent `BonusPower`, the zone where they were played, and a pending delayed-bonus state; current power is always derived.
- `content-data`: the card pool becomes the 13 starter cards plus 7 fillers, the effect library contains the effects those cards reference, and ability cards carry ability text.
- `combat-system`: TurnEnd resolves On Reveal before priority and victory, priority uses post-effect totals, and cards tagged to start in hand are dealt before the random opening hand for both sides.
- `zone-mechanics`: zone total power applies Ongoing card bonuses and zone multipliers, with Iron Man doubling the allied total; the same calculation feeds priority, victory, the plaque and the detail panel.

## Impact

- Content: `SnapClone/Data/Content/ContentLoader.cs`, `Data/Definitions/Definitions.cs`, `Data/Definitions/Enums.cs` (no new trigger types).
- Effects: `Effects/EffectProcessor.cs`, `Effects/IEffect.cs`, `Effects/Implementations/BaseEffectFactory.cs`, new effect classes, removal of `Effects/Implementations/ZonePowerModifierEffect.cs`.
- Gameplay: `Gameplay/Cards/CardInstance.cs`, `Gameplay/Board/ZoneInstance.cs`, `Gameplay/Board/BoardState.cs`, `Gameplay/Combat/PowerResolver.cs`, `Gameplay/Combat/TurnManager.cs`, `Gameplay/Combat/RevealResolver.cs`, `Gameplay/Combat/PriorityCalculator.cs`, `Gameplay/Combat/VictoryResolver.cs`, `Gameplay/AI/AIController.cs`.
- Presentation: `Scenes/CombatScene.cs` (turn end, power refresh, hand refresh, opening hand), `UI/Components/LocationView.cs` (totals from the shared calculation).
- Verification: `dotnet build SnapClone.sln` and new checks in the `%TEMP%\opencode\eps-verify` harness for each ability, idempotence, reveal order, Iron Man, Sentinel and Quicksilver.
