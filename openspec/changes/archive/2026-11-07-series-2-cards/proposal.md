# Proposal

## Why

The catalog stops at 59 cards: the 13 current starters plus the 46 Series 1 cards. The requested Series 2 batch adds 33 more cards, takes the pool to 92, and introduces mechanics the engine does not express yet: repeated On Reveal, cards entering hand from anywhere, play restrictions, printed movement permissions, a temporary movement window, flooded and play-only-locked locations, cost increases with a cap, and destroy/discard replacements that change identity.

Adding this batch keeps the draft and store growing with recognizable content and exercises the effect engine with the mechanics the Series 1 foundation deliberately left out.

## What Changes

- **Add 33 card definitions** with the supplied costs, powers and abilities: 8 Collection Level 1-14 cards and 25 Series 2 cards. Descriptions follow the Spanish ASCII convention. **BREAKING** for the `content-data` requirement of exactly 59 draftable cards: the pool becomes 92.
- **Expand the starter set from 13 to 21** with the 8 CL cards (Jessica Jones, Ka-Zar, Mister Fantastic, Nightcrawler, Odin, Spectrum, White Tiger, Wolfsbane). A profile migration ensures every current starter is owned. **BREAKING** for the "13 starter cards" requirement.
- **Add a Series 2 store batch** of 25 cards: 6 direct cards and 3 bundles of 6, 6 and 7 cards. The existing Series 1 partition (6 direct + 6 bundles) is recomputed from the same 46-card pool and its contents stay unchanged. The store then offers 12 direct cards and 9 bundles. **BREAKING** for the global-store partition counts.
- **Add the missing effect families**: repeat allied On Reveal at a location (Odin), cards entering hand from anywhere except the owner's deck (The Collector), play restrictions by turn and by last-turn play (Ebony Maw, The Infinaut), printed movement permissions (Nightcrawler, Vision), a temporary movement window (Cloak), flooded locations (Storm), play-only zone locks (Ebony Maw) separate from Professor X's full lock, adjacent zone additions (Mister Fantastic), a cost cap of 6 (Iceman, Sandman), per-side discard counters (Morbius), a delayed "did not play here next turn" condition (Jessica Jones), and switch-sides (Hobgoblin).
- **Add the supporting action and state surface**: deck-to-location pull (Jubilee), random card to hand (Agent 13), destroy-all-1-cost (Killmonger), destroy-10-plus (Shang-Chi), token replacement (Bucky Barnes), zero-cost discard copies (Swarm), zero-cost destroyed return (Sabretooth), hand and deck persistent buffs (Nakia, Okoye), enemy-hand affliction (Scorpion), and the `token_winter_soldier` (0/8) and `token_tiger` (0/10) tokens.
- **Keep the AI safe**: filter cards that cannot be played this turn and zones that are play-locked or flooded so the AI does not stall. Enemy decks stay unchanged; the AI continues to use its fixed Series 1 decks.
- **Update presentation and verification**: collection count, store pagination, moveable/flooded/locked indicators, harness counts and per-card checks.
- **Non-goals**: redesigning the 23 enemy decks, adding new locations beyond the existing three, adding new perks, new scenes or animations, a search-based AI, or changing the existing 0-cost token convention.

## Capabilities

### New Capabilities

- None. The change extends capabilities the project already declares.

### Modified Capabilities

- `content-data`: the draftable pool becomes 92 (21 starters + 46 Series 1 + 25 Series 2), the starter set grows to 21, and the two new tokens and all new effect definitions are registered.
- `effect-system`: new trigger `AfterAddedToHand`, repeat-allied-OnReveal, play restrictions, movement permissions, movement windows, play-only locks, flooded zones, adjacent additions, discard-count conditions, delayed inverse play conditions, cost increases with a cap, switch-sides, deck pulls, random card creation, and zero-cost copies/returns.
- `card-mechanics`: instances gain combat cost overrides, per-combat/per-turn move usage, discard counters, the inverse pending condition, and printed/disabled rule interactions.
- `combat-system`: new actions for switch-sides, deck-to-location pulls, random cards to hand, token replacement, zero-cost discard copies and zero-cost destroyed returns; `AddedToHand` event dispatch; cost cap 6.
- `zone-mechanics`: flooded locations, play-only locks distinct from Professor X's full lock, and adjacent zone additions.
- `ai-opponent`: the AI must skip cards and zones that are not legal this turn and must not stall when an ability, lock or flood removes an option.
- `collection`: the collection lists 92 cards.
- `draft-system`: the owned pool starts from 21 starters instead of 13.
- `global-store`: the catalog partition covers 92 cards, preserves the Series 1 bundle contents, and adds the Series 2 batch of 6 direct cards and 3 bundles.
- `card-drag-drop`: a card played on an earlier turn may be lifted only when its printed ability or an active Cloak window permits it; returning an older card to hand stays blocked.
- `card-zone-preview`: revealed zone details expose the flooded or play-locked status.
- `ui-visual-design`: collection and store counts grow to 92 and the new moveable/flooded/locked indicators follow the shared visual language.
- `ui-responsive`: the new status indicators remain legible and non-overlapping at the smallest supported viewport.

## Impact

- Content: `SnapClone/Data/Content/ContentLoader.cs`, `SnapClone/Data/Definitions/Definitions.cs`, `SnapClone/Data/Definitions/Enums.cs`.
- Effects: `SnapClone/Effects/EffectProcessor.cs`, `SnapClone/Effects/Implementations/`, `SnapClone/Effects/Implementations/BaseEffectFactory.cs`.
- Actions and board: `SnapClone/Gameplay/Actions/`, `SnapClone/Gameplay/Board/BoardState.cs`, `ZoneInstance.cs`, `CardPileManager.cs`, `SnapClone/Gameplay/Cards/CardInstance.cs`.
- Combat and power: `SnapClone/Gameplay/Combat/PowerResolver.cs`, `TurnManager.cs`, `RevealResolver.cs`.
- AI and meta: `SnapClone/Gameplay/AI/`, `SnapClone/Gameplay/Meta/GlobalStoreCatalog.cs`, `SnapClone/Persistence/ProfileDto.cs`, `StarterSet.cs`.
- Presentation: `SnapClone/Scenes/CombatScene.cs`, `CollectionScene.cs`, `GlobalStoreScene.cs`, `SnapClone/UI/Components/LocationView.cs`, `CardView.cs`, `DetailPreviewView.cs`.
- Verification: `SnapClone.Harness/Program.cs`, plus per-wave card checks and a manual combat pass.
- Data migration: existing profiles receive the current starters and the 8 CL cards; Series 2 cards remain unowned until purchased.
