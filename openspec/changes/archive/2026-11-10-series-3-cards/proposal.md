# Proposal

## Why

The catalog stops at 92 cards: 21 starters, 46 Series 1 and 25 Series 2. The requested Series 3 batch adds 125 cards, takes the pool to 217, and introduces mechanics the engine cannot express yet: Activate, end-of-game resolution, copied ability text, dynamic cost reduction, destroyed-card tracking, set/reset/double power, resurrection, zone swapping, Max Energy changes, forced play, delayed reveal and hand-based restrictions. Several of these touch rules that are currently closed on purpose.

Adding this batch keeps the draft and store growing with recognizable content and completes the third series with the mechanics the Series 2 foundation deliberately left out.

## What Changes

- **Add 125 card definitions** with the current Series 3 costs, powers and abilities, corrected against the official November 2026 values. Descriptions follow the Spanish ASCII convention. **BREAKING** for the `content-data` requirement of exactly 92 draftable cards: the pool becomes 217.
- **Add `CardSeries.Series3`** to `CardDef` and a third store partition. The legacy Series 1 and Series 2 partitions keep their existing direct cards and bundle ids; Series 3 adds a new batch of direct cards and bundles. **BREAKING** for the global-store partition counts.
- **Add about eleven tokens**: Widow's Bite (0/-1, hand draw lock), Broodling (0/2), DoomBot (0/5), Drone (0/2), Illusion (0/0), Demon (0/6), Mjolnir (0/0, gives Thor +7), Snowguard Hawk (3/3, suppresses zone abilities), Snowguard Bear (3/4, repeats the current zone effect), Void (0/-9) and the existing `card_sentinel` reused for Master Mold. **BREAKING** for the token inventory.
- **Add the Activate subsystem**: a real trigger, a per-instance activation limit, a card action and a UI affordance. Cards: Black Widow, Hellcow, Lockjaw, Mysterio and Spider-Ham. **BREAKING** for the `effect-system` requirement that says Activate never fires.
- **Add an end-of-game phase** after turn 6 and before victory, with a deterministic order and a bound. Cards: Captain Marvel, Dracula, M'Baku and Martyr; Invisible Woman feeds delayed reveals into the same phase. **BREAKING** for the `combat-system` match-end flow.
- **Add copied ability text** with a per-instance override: Absorbing Man, Mystique, Rogue, Super-Skrull, Leader and Taskmaster. Super-Skrull copies only printed Ongoing effects, one level deep, with no recursion.
- **Add a cost pipeline** that resolves base cost, run upgrades, persistent and temporary deltas, dynamic self reductions, auras, taxes, overrides, caps and floors in one place. Cards: Death, She-Hulk, Stature, Miles Morales, Sera, Quinjet, Zabu, Mobius M. Mobius, Wave, Baron Mordo, Beast and Psylocke's next-turn energy.
- **Add destroyed-card tracking** at board level, with power and owner, for Death and Knull and for destroy reactions.
- **Add set/reset/double power** as an explicit power-composition step: Bast, Valkyrie, Shadow King, Black Panther and Shuri.
- **Add the missing actions**: draw for either side, resurrect from the discard pile, copy a card to other zones, swap two zones, pull an enemy hand card into play, discard from the enemy hand, change Max Energy, and mark created cards. Cards: Adam Warlock, Crystal, Maximus, Jane Foster, Ghost Rider, Hela, Arnim Zola, Leader, Nimrod, Quake, Doctor Octopus, Black Bolt, Moon Knight, Silver Samurai, Master Mold, Electro, Havok and Psylocke.
- **Add generalized play and zone restrictions**: Crossbones (winning only), Giganto (left only), Sentry (not right), Goose (cost 4+ blocked), Jean Grey (first card must go there), Electro (one card per turn) and Wave (global cost cap). Add the Limbo zone for Magik and the "doesn't work after turn 5" rule.
- **Add zone and play reactions**: Echo, Negasonic Teenage Warhead, Silk, Titania, Hercules, Nebula, Kingpin, Dagger, Human Torch and Spider-Man 2099.
- **Add bounded information, auto-play and delayed reveal**: Agatha Harkness auto-plays the player's even turns with the existing legality query; Daredevil exposes the opponent's turn-5 intent; Howard the Duck exposes the top card of the deck; Invisible Woman delays reveals until the end of the game.
- **Fix the repeat pass scope**: the Odin/Wong repeat pass currently revisits every already-revealed card on every turn. It must repeat only the cards revealed on the turn the repeater resolved, or snapshot the targets at the repeater's reveal, so a single On Reveal does not fire again later.
- **Update presentation and verification**: collection and store counts (217 draftables), Activate button and status, end-of-game banner, delayed-reveal face-down state, zone status labels, per-card harness checks and per-wave counts.
- **Non-goals**: redesigning the 23 enemy decks, adding new locations beyond the existing three plus Limbo, adding new perks, a search-based AI, a general scripting language, a full official rules emulator, and online/networked play.

## Capabilities

### New Capabilities

- None. The change extends capabilities the project already declares.

### Modified Capabilities

- `content-data`: the draftable pool becomes 217 (21 starters + 46 Series 1 + 25 Series 2 + 125 Series 3), the starter set stays at 21, the Series 3 card set and about eleven tokens are registered, and every referenced effect resolves to an implementation.
- `effect-system`: Activate becomes a live trigger; end-of-game effects resolve; ability text can be copied or stolen; cost and power composition gain dynamic, set, reset and double steps; destroyed-card tracking is available; the new action and event surface is exposed; the On Reveal repeat pass is bounded to the correct turn.
- `card-mechanics`: instances gain activation state, copied effect ids, delayed-reveal state, power set/override state, a created-card marker, destroyed-card identity, and cost modifiers that read from the board.
- `combat-system`: a new end-of-game phase runs after turn 6 and before victory; new actions handle draw, resurrect, copy-to-zone, zone swap, enemy-hand pull/discard and Max Energy; match end waits for the end-of-game resolution.
- `ai-opponent`: the AI respects the new restrictions, cost pipeline and zone constraints, does not stall on Activate or end-of-game states, and can drive Agatha's even-turn auto-play with the same legality query.
- `zone-mechanics`: Limbo is a replacement zone; zones can be swapped; Hawk suppresses zone effects; Bear repeats a zone effect; Goose, Jean Grey and Magik impose new zone rules.
- `draft-system`: the owned pool can grow to 217 cards; themed rounds still have enough Control, Destroy, On Reveal and Neutral cards; the 8-pick structure is unchanged.
- `collection`: the collection lists 217 cards with the new series and filters.
- `global-store`: the catalog partition covers 217 cards, preserves the existing Series 1 and Series 2 bundle contents, and adds a Series 3 batch.
- `ui-visual-design`: the Activate control, end-of-game state, delayed-reveal face-down card, zone status labels and new counts follow the shared visual language.
- `ui-responsive`: the Activate control and new status indicators remain legible and non-overlapping at the smallest supported viewport.

## Impact

- Content: `SnapClone/Data/Content/ContentLoader.cs`, `SnapClone/Data/Definitions/Definitions.cs`, `SnapClone/Data/Definitions/Enums.cs`.
- Effects: `SnapClone/Effects/EffectProcessor.cs`, `SnapClone/Effects/IEffect.cs`, `SnapClone/Effects/Implementations/BaseEffectFactory.cs`, `SnapClone/Effects/Implementations/Series3Effects.cs` (new), existing Series 1 and Series 2 implementations where a shared family changes.
- Actions and board: `SnapClone/Gameplay/Actions/`, `SnapClone/Gameplay/Board/BoardState.cs`, `ZoneInstance.cs`, `CardPileManager.cs`, `SnapClone/Gameplay/Cards/CardInstance.cs`, `CardInstanceFactory.cs`.
- Combat and power: `SnapClone/Gameplay/Combat/PowerResolver.cs`, `TurnManager.cs`, `RevealResolver.cs`, `VictoryResolver.cs`, a new end-of-game resolver.
- AI and meta: `SnapClone/Gameplay/AI/`, `SnapClone/Gameplay/Meta/PerkAbilities.cs`, `GlobalStoreCatalog.cs`, `SnapClone/Persistence/ProfileDto.cs`.
- Presentation: `SnapClone/Scenes/CombatScene.cs`, `CollectionScene.cs`, `GlobalStoreScene.cs`, `SnapClone/UI/Components/CardView.cs`, `LocationView.cs`, `DetailPreviewView.cs`.
- Verification: `SnapClone.Harness/Program.cs`, plus per-wave card checks, subsystem checks and a manual combat pass.
- Data: existing profiles keep their owned cards; Series 3 remains unowned until purchased or drafted. No profile schema change is required.
