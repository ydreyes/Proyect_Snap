# Proposal

## Why

None of the 46 Series 1 cards is registered: the current card pool is the 20 starter/filler cards, with no name overlap. The `effect-action-core` change already provides the action authority, action events, reveal-once guard, energy ledger, play log, capacity checks and token/draft separation, so the missing work is the Series 1 content plus the card mechanics that the core deliberately left for later.

Adding the full set gives the draft and reward pools the recognizable Series 1 identity and exercises the event engine with real content: destruction, discard, movement, copying, merging, auras, suppression and zone manipulation.

## What Changes

- **Add 46 card definitions** with the supplied costs, powers and abilities, plus Spanish ASCII descriptions in the existing style (`Al revelarse:`, `Continuo:`, and new trigger prefixes where needed).
- **Replace the 20-card pool with 59**: keep the 13 starter cards and add the 46 Series 1 cards; remove the 7 fillers that are not in the supplied list (Spark, Raven, Champion, Behemoth, Juggernaut, Titan, Leviathan). **BREAKING** for the current `content-data` "exactly 20" pool and its 0-cost curve requirement: the final curve is 1-6. Draft and reward pools use the 59-card draftable catalog.
- **Complete the continuous effect model**: auras that affect other cards (Captain America), cross-zone bonuses (Klaw), repeated Ongoing effects (Onslaught), and suppression/removal (Cosmo, Enchantress).
- **Add destroy, discard and affliction**: replacements (Apocalypse, Wolverine), protection (Armor), destruction reactions (Angel, Nova), self-scaling destruction (Carnage) and persistent debuffs (Spider-Woman).
- **Add movement**: simultaneous/batch moves (Heimdall), next-play moves (Iron Fist), highest-power selection (Doctor Strange), and movement reactions (Kraven, Multiple Man).
- **Add hand/deck manipulation**: top-deck buff (America Chavez), opponent-deck draw (Cable), forced turn-2 draw (Domino), shuffle (Korg) and banish (Yondu).
- **Add copy, token and merge**: next-play buff (Forge), merge (Hulkbuster), copy from hand/zone (Mantis, Moon Girl, White Queen), transform (Morph), and token creation (Mister Sinister, Squirrel Girl).
- **Add ongoing and after-play cards**: Angela, Bishop, Devil Dinosaur, Lizard, Namor, Strong Guy and the supporting Ongoing interactions.
- **Add location, visibility and locks**: Professor X's zone lock, Scarlet Witch's location replacement and Uatu's unrevealed-location visibility.
- **Reuse existing content-only mechanics**: Groot and Rocket Raccoon use the existing "opponent played here this turn" effect with +4.
- **Update AI, UI and content plumbing**: AI must not self-sabotage with discard/destroy cards; hand size, tokens, merged cards and hidden-location visibility must remain legible; draft/reward pools must exclude tokens.
- **Fix the cross-cutting rules**: hand maximum 7 (a draw with a full hand is skipped and the card stays in the deck; an effect that adds to a full hand does not create the card and the source is preserved; returning a played card to a full hand is rejected and the card stays in its slot); Hulkbuster merges into a random other allied card at its location, the host gains its power and Hulkbuster leaves the board without entering a pile; Yondu's banish is combat-scoped and the card returns next combat; cards with multiple eligible targets choose randomly unless their text defines the target set; Scarlet Witch replaces among the existing locations and the three existing zone effects become active so the replacement has a real effect.
- **Stage the work in waves** so the game remains playable and verifiable after each wave:

| Wave | Scope | Cards |
|---|---|---|
| A | Continuous effect model (engine only) | none |
| B | Destroy / discard / affliction | 12 |
| C | Movement | 5 |
| D | Hand / deck | 5 |
| E | Copy / token / merge | 8 |
| F | Ongoing / auras / suppression | 11 |
| G | Locations / visibility / locks | 3 |
| H | Content-only integration | 2 |

### Card inventory

| Wave | Cost | Power | Card | Ability |
|---|---:|---:|---|---|
| B | 2 | 4 | Angel | When an ally is destroyed, flies from hand/deck to replace it |
| B | 6 | 10 | Apocalypse | When discarded, returns to hand with +4 Power |
| B | 2 | 3 | Armor | Ongoing: cards here can't be destroyed |
| B | 1 | 3 | Blade | On Reveal: discard the rightmost card in hand |
| B | 2 | 2 | Carnage | On Reveal: destroy other cards here; +2 Power per destroyed |
| B | 3 | 6 | Deathlok | On Reveal: destroy other cards here |
| B | 1 | 2 | Elektra | On Reveal: destroy an enemy 1-Cost card here |
| B | 3 | 6 | Lady Sif | On Reveal: discard the highest-Cost card in hand |
| B | 1 | 1 | Nova | When destroyed: give your cards +1 Power |
| B | 5 | 8 | Spider-Woman | On Reveal: afflict enemy cards here with -1 Power |
| B | 3 | 7 | Sword Master | On Reveal: discard an odd-Costed card in hand |
| B | 2 | 3 | Wolverine | When discarded or destroyed, regenerate with +2 Power at a random location |
| C | 2 | 4 | Doctor Strange | On Reveal: move your highest-Power card(s) here |
| C | 6 | 10 | Heimdall | On Reveal: move your other cards one location left |
| C | 1 | 3 | Iron Fist | On Reveal: after your next card, move it one location left |
| C | 2 | 3 | Kraven | When a card moves here, +2 Power |
| C | 2 | 4 | Multiple Man | When this moves, add a copy at the old location |
| D | 1 | 2 | America Chavez | On Reveal: give the top card of your deck +3 Power |
| D | 2 | 4 | Cable | On Reveal: draw a card from your opponent's deck |
| D | 2 | 3 | Domino | Always drawn on turn 2, and not before |
| D | 1 | 2 | Korg | On Reveal: shuffle a Rock into your opponent's deck |
| D | 1 | 2 | Yondu | On Reveal: banish the lowest-Cost card in your opponent's deck |
| E | 2 | 2 | Forge | On Reveal: give the next card you play +2 Power |
| E | 2 | 3 | Hulkbuster | On Reveal: merge this into one of your cards here |
| E | 1 | 3 | Mantis | On Reveal: if the rival played here, copy one of those cards into your hand |
| E | 2 | 2 | Mister Sinister | On Reveal: add a Sinister Clone here with the same Power |
| E | 4 | 5 | Moon Girl | On Reveal: copy your hand |
| E | 3 | 0 | Morph | On Reveal: become a copy of a card in the opponent's hand |
| E | 1 | 2 | Squirrel Girl | On Reveal: add a 1-Power Squirrel to each other location |
| E | 3 | 5 | White Queen | On Reveal: copy the highest-Cost card in the opponent's hand |
| F | 2 | 3 | Angela | After you play a card here, +1 Power |
| F | 3 | 2 | Bishop | After you play a card, +1 Power |
| F | 3 | 3 | Captain America | Ongoing: your other Ongoing cards here have +2 Power |
| F | 3 | 3 | Cosmo | Ongoing: On Reveal abilities don't happen here |
| F | 5 | 4 | Devil Dinosaur | Ongoing: +2 Power for each card in hand |
| F | 5 | 8 | Enchantress | On Reveal: remove abilities from all Ongoing cards here |
| F | 4 | 3 | Klaw | Ongoing: the location to the right has +6 Power |
| F | 2 | 6 | Lizard | Ongoing: -4 Power if the opponent has 4 cards here |
| F | 4 | 6 | Namor | Ongoing: +5 Power if this is your only card here |
| F | 6 | 8 | Onslaught | Ongoing: your other Ongoing effects here apply an additional time |
| F | 3 | 4 | Strong Guy | Ongoing: +6 Power if your hand has 1 or fewer cards |
| G | 5 | 2 | Professor X | Ongoing: moving is the only way to add or remove a character here |
| G | 2 | 3 | Scarlet Witch | On Reveal: replace this location with a new one |
| G | 1 | 2 | Uatu the Watcher | Game Start: you can see unrevealed locations |
| H | 3 | 5 | Groot | On Reveal: if the opponent played a card here, +4 Power |
| H | 1 | 1 | Rocket Raccoon | On Reveal: if the opponent played a card here, +4 Power |

## Capabilities

### New Capabilities

- None. The change extends capabilities the project already declares.

### Modified Capabilities

- `content-data`: the pool becomes the 13 starter cards plus the 46 Series 1 definitions (59 total); the 7 fillers are removed; the effect library grows to cover every Series 1 ability; descriptions follow the Spanish ASCII convention; the three existing zone effects become implemented and form the replacement pool.
- `effect-system`: new card effect families and trigger interactions for destruction, discard, movement, play, draw, game start, continuous auras, cross-zone bonuses, repetition, suppression, copying and tokens.
- `combat-system`: destroy/discard/move/copy/token/merge actions, created-card capacity, hand maximum 7, turn-2 draw override, deck search/shuffle/banish, and reveal/suppression ordering.
- `card-mechanics`: instance state for play counters, affliction, copy/transform/merge identity and next-play pending effects.
- `zone-mechanics`: zone power additions, zone locks, location replacement, per-side visibility and the three implemented zone effects.
- `ai-opponent`: the AI must use the new actions without self-sabotage and must respect the new costs, hand cap and constraints.
- `card-piles`: the merged-card state and the combat-scoped Yondu banish.
- `card-drag-drop`: returning a played card to a full hand is rejected.
- `ui-responsive`: 7-card hand legibility, token cards, merged-card removal and Uatu plaque visibility.
- `card-zone-preview`: Uatu visibility and replaced-location detail.

## Impact

- Content: `SnapClone/Data/Content/ContentLoader.cs`, `SnapClone/Data/Definitions/Definitions.cs`, `SnapClone/Data/Definitions/Enums.cs`.
- Effects: `SnapClone/Effects/EffectProcessor.cs`, `SnapClone/Effects/Implementations/` (new effect classes), `SnapClone/Effects/Implementations/BaseEffectFactory.cs`.
- Actions and board: `SnapClone/Gameplay/Actions/`, `SnapClone/Gameplay/Board/BoardState.cs`, `ZoneInstance.cs`, `CardPileManager.cs`, `SnapClone/Gameplay/Cards/CardInstance.cs`.
- Combat and power: `SnapClone/Gameplay/Combat/PowerResolver.cs`, `TurnManager.cs`, `RevealResolver.cs`, `OpeningHandDealer.cs`.
- AI, draft and run: `SnapClone/Gameplay/AI/`, `SnapClone/Gameplay/Draft/DraftSystem.cs`, `SnapClone/Gameplay/Run/RewardSystem.cs`.
- Presentation: `SnapClone/Scenes/CombatScene.cs`, `SnapClone/UI/Components/CardView.cs`, `PileViewerView.cs`, `DetailPreviewView.cs`, `LocationView.cs`.
- Verification: `dotnet build SnapClone.sln`, the `%TEMP%\opencode\eps-verify` harness with per-card and per-wave checks, and a manual combat pass for each wave.
- Resolved decisions: the pool is 59 (13 starter + 46 Series 1, fillers removed); hand maximum 7, with skipped draws, fizzled effect additions and rejected board-to-hand returns; Hulkbuster merges into a random other allied card and leaves no pile; Yondu's banish is combat-scoped; multi-target cards choose randomly; Scarlet Witch uses the three existing locations and their three effects become active. The Onslaught repetition rule is recorded in `design.md`.
