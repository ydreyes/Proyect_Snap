# Proposal

## Why

The run's perk system is still the original eight global rule perks: they attach to a card but affect the whole player side from anywhere in the deck. The 28 ability perks the design now wants are host-scoped card abilities, so the current catalog cannot express them. Replacing the eight rule perks with the 28 ability perks makes each level-up a concrete card-ability graft and turns the perk system into the main build-defining choice of a run.

## What Changes

- **BREAKING** Replace the eight rule perks with a single catalog of 28 ability perks. The old ids (`perk_vanguardia`, `perk_aura`, `perk_francotirador`, `perk_letal`, `perk_agil`, `perk_guardian`, `perk_imparable`, `perk_codicioso`) are retired.
- Each perk attaches to a card in the run deck and grants that ability to the host card. Perks are independent entries even when they share an effect implementation with a current or future card.
- All 28 perks are available from the start. The level-up perk branch offers three random perks from the full catalog; the global store stops selling perks, and the collection's perks tab lists all 28 as owned.
- A perk is only offered when at least one card in the deck can host it, and the host-selection grid only enables compatible cards. Pure effect perks are compatible with any card; meta-abilities (`Amplify`, `Overtime`, `Encore`, `Reinforce`) require the host trait they modify.
- Attached perk effects are resolved by the effect processor, including from hand or deck where the perk text requires it. Power, cost, energy, movement, destruction, zone and play-rule calculations include perk-granted abilities.
- Add the trigger and action support the catalog needs: Start of Turn, active End of Turn, `AfterDrawn` and `GameStart` content, repeat-own-ability hooks, board movement, taunt, play/destroy/power-reduction immunities, deck and hand manipulation, and temporary energy.
- Migrate existing saves: remove unknown old perk ids from `profile.json` and from saved deck cards, keeping the run and the rest of the profile intact. No new save schema is introduced.
- Update the level-up, collection, store and perk-detail screens for the 28-entry catalog, compatibility text and compatible-host filtering.

The perk catalog source is recorded in the appendix. Remaining keyword semantics that the catalog names but the engine does not yet define (`Empowered`, `Quickdraw`, `Horde`, taunt reservation, "character" pool) are design decisions for `design.md`.

## Capabilities

### New Capabilities

None. The perk behavior contract lives in the existing `perk-rules` capability and the surrounding capabilities below.

### Modified Capabilities

- `perk-rules`: replace the eight global rule perks with 28 host-attached ability perks, compatibility filtering and the all-available acquisition model.
- `content-data`: replace the eight-perk library with the 28-perk library, including shared effect ids and no store-only perks.
- `effect-system`: dispatch attached perk effects through the effect processor, add the missing trigger families and the repeat-own-ability hooks, and include perk abilities in power and ability queries.
- `combat-system`: replace the global perk rule hooks with per-card ability hooks and cover the turn, energy, movement, destruction and play-rule changes the perks need.
- `card-mechanics`: define the per-card movement permission, destruction and power-reduction immunity, and opening-hand/deck behavior used by the new perks.
- `ai-opponent`: make the AI respect the taunt reservation for its first card of the following turn.
- `run-economy`: change the level-up perk branch to offer three compatible perks from the full 28-perk catalog and attach one to a compatible deck card.
- `collection`: list all 28 perks as owned and show ability text, compatibility and copies.
- `global-store`: remove the perk entries and their purchase path from the permanent store.
- `run-persistence`: migrate old perk ids out of the profile and run checkpoints without changing the save schema.
- `ui-visual-design`: present the 28 ability perks, their compatibility requirement and the compatible-host target state in the level-up and collection screens.

## Impact

- **Code**: `SnapClone/Data/Definitions/PerkCatalog.cs`, `SnapClone/Data/Content/ContentLoader.cs`, `SnapClone/Gameplay/Meta/PerkRuleSystem.cs`, `SnapClone/Gameplay/Run/LevelUpSystem.cs`, `SnapClone/Gameplay/Run/RunEventSystem.cs`, `SnapClone/Effects/EffectProcessor.cs`, `SnapClone/Effects/IEffect.cs`, `SnapClone/Effects/Implementations/*`, `SnapClone/Gameplay/Combat/PowerResolver.cs`, `SnapClone/Gameplay/Actions/CardActionService.cs`, `SnapClone/Gameplay/Cards/CardInstance.cs`, `SnapClone/Gameplay/AI/AIController.cs` and `AIStrategy.cs`, `SnapClone/Gameplay/Meta/GlobalStoreSystem.cs`, `SnapClone/Gameplay/Meta/CollectionSystem.cs`, `SnapClone/Persistence/ProfileDto.cs`, `SnapClone/Persistence/CardSnapshotDto.cs`, `SnapClone/Persistence/RunCheckpointService.cs`, `SnapClone/Scenes/LevelUpScene.cs`, `SnapClone/Scenes/CollectionScene.cs`, `SnapClone/Scenes/GlobalStoreScene.cs`.
- **Data**: one catalog of 28 perk definitions; no new currencies, cards, dependencies or save files.
- **Systems**: the perk system stops being a global rule layer and becomes a host-ability layer. Existing runs survive; old perk attachments are dropped during migration.
- **Risks**: 28 new effect behaviors, trigger support that does not exist yet, compatibility UX, balance across the 12-card deck, save migration, and recursion in `Amplify`/`Encore`/`Overtime`.

## Appendix: Perk Catalog Source

| # | Perk | Effect |
|---|---|---|
| 1 | Afterimage | When this moves, add an unmodified copy to the old location. |
| 2 | Amplify | This card's Ongoing effect applies an additional time. |
| 3 | Apex | Ongoing: If this is your only card here, double its Power. |
| 4 | Clone | Game Start: Shuffle a copy of this into your deck. |
| 5 | Deadlink | On Reveal: Horde +4. (Give your Zombie Horde that much Power.) |
| 6 | Encore | This card's On Reveal ability repeats. |
| 7 | Hot Pursuit | Costs 3 less if a card moved last turn. |
| 8 | Maneuverable | Moveable once. |
| 9 | Mentor | On Reveal: Add this card's Power to the top character of your deck. |
| 10 | Overcharge | On Reveal: If you're winning here next turn, +1 Energy. |
| 11 | Overload | Start of Turn: +1 Energy if you are winning this location. |
| 12 | Overtime | This card's End of Turn ability repeats. |
| 13 | Quickdraw: Discount | Quickdraw: -2 Cost. |
| 14 | Quickdraw: Power | Quickdraw: On Reveal: +3 Power. |
| 15 | Rally | Ongoing: +1 Power for each of your other cards here. |
| 16 | Reality Warp | On Reveal: Replace this location with a new one. |
| 17 | Regeneration | When Discarded or Destroyed: Regenerate this with +2 Power at a random location. |
| 18 | Regroup | On Reveal: Return your lowest-Cost card here to hand to give it +3 Power. |
| 19 | Reinforce | When this is Empowered, gain an additional +1 Power. (if in hand, in deck, or in play) |
| 20 | Salvage | On Reveal: Destroy your lowest-Power other card here for +5 Power. |
| 21 | Sidekick | On Reveal: Add a random character to your hand. Give it -2 Cost. |
| 22 | Silence | Remove this card's text. |
| 23 | Slingshot | On Reveal: Give one of your other cards here +1 Power and move it to the right. |
| 24 | Superstar | Starts in your opening hand. |
| 25 | Tag In | When one of your cards is destroyed, this replaces it from your hand or deck. |
| 26 | Taunt | On Reveal: Your opponent must play their first card at this location next turn. |
| 27 | Unstoppable | Can be played anywhere. Can't be destroyed or have its Power reduced. |
| 28 | Wakandan Tech | On Reveal: Double this card's Power. |
