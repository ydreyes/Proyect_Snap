# Design

## Context

See `proposal.md` for motivation and `specs/perk-rules/spec.md` for the behavior contract.

The current perk layer is small and global. `PerkCatalog` defines eight fixed ids, `PerkRuleSystem` resolves them as side-wide rules, and `LevelUpSystem` attaches them to any deck card without compatibility checks. `EffectProcessor` reads only `CardDefinition.EffectIds`; `CardInstance.AttachedPerks` is not part of the effect pipeline. `TurnManager.ResolveEndOfTurn` applies `OnReveal` and pending bonuses but no `EndOfTurn` effects, and there is no `StartOfTurn` trigger.

The 28 new perks are not global rules. They are abilities that travel with a host card, can trigger from hand, deck or play, and sometimes modify another ability of the same host. The design therefore moves perk resolution into the effect pipeline, adds a compatibility gate, and adds the trigger and action support the catalog needs.

## Goals / Non-Goals

**Goals:**

- One catalog of 28 independent ability perks, all available from the start.
- A host-scoped ability model that reuses card effect implementations.
- Compatibility filtering for offers and host selection.
- Perk effects dispatched with the same authority and ordering as card effects.
- Save migration that keeps the profile and run without reviving retired perks.
- A level-up, collection and store presentation that explains compatibility.

**Non-Goals:**

- No new currencies, run nodes, reward tracks or store categories.
- No real card art or new draftable cards in this change; the Zombie Horde token is content support, not a draftable card.
- No attempt to balance all 28 perks against each other in this change; values follow the catalog text.
- No change to combat rules unrelated to the catalog abilities.
- No removal of the 59-card pool or the existing card effect system.

## Decisions

### 1. Perk model: host ability definition, not global rule

`PerkDef` becomes an ability definition with:

- `Id`, `Name`, `RuleText`
- `EffectIds` (one or more)
- `Kind`: `GrantedEffect`, `MetaAbility` or `RuleKeyword`
- `Compatibility`: `Any`, `RequiresOngoing`, `RequiresOnReveal`, `RequiresEndOfTurn`, `RequiresEmpower` (any card can be empowered)
- `Stackable` (default true; a second copy adds another grant)

`PerkCatalog` becomes the 28-entry source of truth. `ContentLoader.LoadPerks` registers all 28. `PerkRuleSystem` is removed from the perk path; its remaining power/cost/economy hooks are deleted. The retired ids are not registered.

`CardInstance.AttachedPerks` stays the persisted attachment list. A card can host several different perks; the same perk can appear twice, subject to the existing two-copy limit.

**Alternative considered:** keep `PerkRuleSystem` and add a second dispatcher for ability perks. Rejected because it would split power, cost and trigger logic across two systems and keep the global-rule semantics the change is removing.

### 2. Effective abilities on the host

Add one effective-ability lookup used by every query:

```
Definition.EffectIds + AttachedPerkEffectIds(host)
```

- `EffectProcessor` uses it for trigger dispatch, OnReveal, EndOfTurn, StartOfTurn and power queries.
- `PowerResolver` uses it indirectly through the processor.
- `HasOngoing` and the ability checks used by the UI also use it.
- The lookup must not duplicate an effect id; each copy of a perk contributes its effect once.

Perk effects use the host as `SourceCard`, the host's zone as `SourceZone`, and the host's owner as `Owner`. `BuildReactorOrder` already includes board, hand, deck and piles, so hand/deck abilities such as Tag In, Superstar, Clone and Reinforce can react without a second dispatcher.

**Alternative considered:** materialize perk effects into `CardDefinition.EffectIds` at attach time. Rejected because definitions are shared and run-scoped grants would leak across cards.

### 3. Compatibility gate

Compatibility is a per-perk predicate over the deck:

- `FlatEffect` perks require any card.
- `Amplify` requires the host to have at least one `Ongoing` effect.
- `Encore` requires the host to have at least one `OnReveal` effect.
- `Overtime` requires the host to have at least one `EndOfTurn` effect.
- `Reinforce` requires any card because any card can be empowered.

`LevelUpSystem.GetPerkCandidates` filters offers by `CanAttachAnyCard(perk, runState)`. The target grid uses `CanAttach(perk, card)` and disables incompatible cards with a reason. If no compatible perk exists, the perk branch is unavailable rather than offering a dead choice.

A future card with `Ongoing`, `OnReveal` or `EndOfTurn` automatically makes the corresponding perks eligible. The already generated `LevelUpState` offers are not re-rolled when the deck changes; they were generated against the deck at that point.

**Alternative considered:** allow any host and let the perk do nothing. Rejected by the user: it wastes a level-up choice.

### 4. Trigger and event support

Add to the effect system:

- `StartOfTurn` trigger, resolved before the play phase and before energy is finalized.
- Active `EndOfTurn` trigger, resolved after the reveal sequence and before priority.
- `AfterDrawn` content for Quickdraw.
- `GameStart` content for Clone and Superstar.
- An `Empowered` action event dispatched when another effect increases a card's Power; Reinforce reacts to it.
- A next-turn taunt reservation stored on the board; the AI and the player both honor it.
- A one-time player-move permission and destroy/power-reduction immunity flags on the instance.

The event queue keeps the existing `MaxEventsPerAction` guard. Repeat hooks add their own copy limit and never recurse into themselves.

### 5. Catalog behavior map

| Perk | Kind | Trigger | Implementation approach |
|---|---|---|---|
| Afterimage | Granted | AfterMoved | On host move, create an unmodified copy in the old zone. Reuse the Multiple Man copy path, but strip upgrades, bonuses and perks. |
| Amplify | Meta | Ongoing query | Each attached copy repeats the host's other Ongoing effects once. Exclude Amplify itself. |
| Apex | Granted | Ongoing query | If the host is the only allied card in its zone, double its final power after other bonuses. Compute from the pre-double value. |
| Clone | Granted | GameStart | Shuffle a separate unmodified copy of the host into the owner's deck. |
| Deadlink | Granted | OnReveal | Add +4 persistent Power to each Zombie Horde the owner controls. |
| Encore | Meta | OnReveal | After the host's OnReveal effects resolve, resolve them once more per copy. Do not clear the reveal marker. |
| Hot Pursuit | Rule | Cost query | If any card moved during the previous turn, reduce the host's effective cost by 3 for this turn. |
| Maneuverable | Rule | Player move | Allow one player-initiated move per combat. Effect moves do not consume the permission. |
| Mentor | Granted | OnReveal | Add the host's current Power to the top card of the owner's deck. |
| Overcharge | Granted | OnReveal + next start | Arm a next-turn check; if the owner is winning the host's zone, grant +1 Energy for that turn. |
| Overload | Granted | StartOfTurn | If the owner is winning the host's zone, grant +1 Energy for the turn. |
| Overtime | Meta | EndOfTurn | After the host's EndOfTurn effects resolve, resolve them once more per copy. |
| Quickdraw: Discount | Rule | AfterDrawn | Reduce the host's effective cost by 2 for the rest of the combat. |
| Quickdraw: Power | Granted | AfterDrawn | Grant `OnReveal: +3 Power` for the rest of the combat. |
| Rally | Granted | Ongoing query | +1 Power for each other allied card in the host's zone. |
| Reality Warp | Granted | OnReveal | Reuse the Scarlet Witch location replacement path. |
| Regeneration | Granted | AfterDiscarded / AfterDestroyed | Reuse the Wolverine regenerate path; add +2 Power and a random zone with room. |
| Regroup | Granted | OnReveal | Return the lowest-Cost allied card in the zone to hand and give it +3 Power. Random among ties. |
| Reinforce | Meta | Empowered | +1 Power per copy when another effect increases the host's Power. Exclude its own +1 from re-triggering. |
| Salvage | Granted | OnReveal | Destroy the lowest-Power other allied card in the zone for +5 Power. Random among ties. |
| Sidekick | Granted | OnReveal | Add a random draftable non-token character to hand; its cost is -2 for the combat. |
| Silence | Granted | OnReveal | Disable the host's printed effects for the combat; keep attached perk effects active. |
| Slingshot | Granted | OnReveal | Give one other allied card in the zone +1 Power and move it right if there is room. |
| Superstar | Rule | GameStart | Deal the host in the opening hand before random cards. |
| Tag In | Granted | AfterDestroyed | If the host is in hand or deck and the destroyed card's zone has room, play the host there for 0. |
| Taunt | Rule | OnReveal | Reserve the opponent's first card next turn for the host's zone if it has room. |
| Unstoppable | Rule | Play / power / destroy | Ignore zone restrictions and locks; reject destroy; ignore Power reductions. |
| Wakandan Tech | Granted | OnReveal | Add a persistent bonus equal to the host's Power at reveal. Do not double the bonus again. |

### 6. Ambiguous keyword definitions

The catalog uses keywords the engine does not currently define. The design fixes them as follows:

- **Empowered:** a one-time Power increase caused by an effect other than Reinforce. It dispatches once per event and never from Reinforce's own +1.
- **Quickdraw:** the host's draw event. `Quickdraw: Discount` applies -2 cost for the rest of the combat; `Quickdraw: Power` grants an OnReveal +3 ability for the rest of the combat.
- **Horde / Zombie Horde:** a non-draftable 0-cost, 0-Power token `token_zombie_horde`. Deadlink adds +4 to each Horde the owner controls.
- **Taunt:** the opposing side's first card of the next turn is reserved for the host's zone when it has room; otherwise the reservation expires.
- **Character:** a random draftable, non-token card definition from the card registry.
- **Moveable once:** one player-initiated move per combat. Effect-driven movement is separate and does not consume the permission.
- **Only card here:** the host is the only allied card in its zone.

### 7. Level-up and collection UI

- The perk offer shows name, ability text and compatibility.
- The host grid shows only compatible cards enabled; incompatible cards are visible but disabled with a reason.
- The branch tile is disabled when no compatible host exists.
- The collection perks tab shows 28 entries, all owned, with ability text, compatibility and copies attached in the current deck.
- The global store removes the perk entry; its perk purchase path is deleted.

**Alternative considered:** show incompatible perks with a warning but allow the choice. Rejected by the user.

### 8. Persistence and migration

No schema change. On load:

- Profile: remove ids not in `PerkCatalog.AllIds` from `OwnedPerkIds` and `PerkCopyCounts`; keep all current ids.
- Run checkpoint: remove unknown ids from each saved card's `AttachedPerks`; keep the card, upgrades, cost delta and power delta; keep the run node.
- The next save writes the cleaned state.

The migration is destructive for retired perk attachments. A rollback would not restore them; this is acceptable because the retired perks no longer exist in the catalog.

**Alternative considered:** map the eight old perks to new ones. Rejected because there is no honest 1:1 mapping and it would preserve misleading builds.

### 9. AI behavior

The AI uses the same action authority as the player. Only one new AI rule is required: the Taunt reservation. The AI's first card of the following turn must use the taunted zone when it has room and the card is legal there. If the zone is full or no legal card exists, the reservation expires. The AI continues to respect energy, hand and zone limits.

### 10. Testing strategy

- Harness checks for catalog size and retired ids.
- Harness checks for compatibility: each meta-perk is offered only with a host trait and a future host makes it eligible.
- Harness checks for each of the 28 behaviors, including determinism and loop bounds.
- Harness checks for migration: old ids removed from profile and deck, current ids preserved.
- Harness checks for Taunt AI placement, Maneuverable once, Unstoppable immunity, Quickdraw, Overload and Overcharge energy timing.
- Manual UI checks for the 28-perk collection, compatibility text, disabled hosts and the no-compatible-perk state.

## Risks / Trade-offs

- **28 effects at once** → Implement the infrastructure first, then the catalog in behavior groups; each group has harness coverage before the UI is enabled.
- **Repeat recursion (Amplify, Encore, Overtime)** → Exclude self, cap by copy count and reuse the existing event limit.
- **Dead picks** → Compatibility gate plus disabled host reasons.
- **Power double-counting (Apex, Wakandan Tech, Rally)** → Compute from the pre-bonus value and never accumulate into stored power.
- **Save migration loses old attachments** → Explicit and irreversible; document it and preserve the rest of the save.
- **Balance across 28 perks** → Values follow the supplied catalog; balance remains a tuning pass outside this change.
- **Future cards with EndOfTurn/Quickdraw** → The trigger support is built now; the perks remain unavailable until a compatible host exists.
- **UI size** → The collection and level-up already mount 28 entries; verify at 720x1280 and 1080x2400.

## Migration Plan

1. Add the catalog and effect implementations behind the current UI.
2. Add the effect-pipeline bridge and compatibility gate.
3. Enable the level-up offers and collection tab.
4. Remove the store perk entry and the old rule hooks.
5. Run the save migration on load and verify a migrated profile and run.
6. Rollback is file-level: revert the code and the catalog. Retired perk ids removed during migration are not restored, so a rollback starts those saves without the old perk attachments.

## Open Questions

None blocking this design. Balance values and future cards with `EndOfTurn` or `Quickdraw` hosts are content follow-ups, not changes to this approach.
