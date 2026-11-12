# Tasks

## 1. Catalog and data model

- [x] 1.1 Replace the eight rule perk ids with the 28 ability perk ids and definitions; verify `PerkCatalog.AllIds` has 28 entries and the old ids are absent.
- [x] 1.2 Extend the perk definition with ability kind, compatibility metadata and stackability; verify the catalog loads and every perk exposes the new fields.
- [x] 1.3 Map every perk to its effect ids and shared effect implementations; verify every id resolves through the effect registry and no perk uses a scene branch.
- [x] 1.4 Add the non-draftable `token_zombie_horde` definition with 0 cost and 0 Power; verify it is not in the 59 draftable cards and Deadlink can target it.
- [x] 1.5 Update perk validation for 28 entries, multiple effect ids and compatibility metadata; verify `SnapClone.Harness` accepts the catalog and rejects an unknown effect id.
- [x] 1.6 Add a harness check for catalog size, retired ids, shared effect ids and the Zombie Horde token; verify `SnapClone.Harness` passes.

## 2. Effect pipeline bridge

- [x] 2.1 Add an effective-ability lookup that combines `CardDefinition.EffectIds` with `AttachedPerks` effect ids and removes duplicates; verify a card with two perks exposes both exactly once.
- [x] 2.2 Include perk abilities in trigger dispatch, OnReveal, power queries and `HasOngoing`; verify a perk-granted Ongoing and OnReveal both resolve.
- [x] 2.3 Add the `StartOfTurn` trigger and resolve it before the play phase and before energy is finalized; verify a StartOfTurn effect changes the current turn's energy.
- [x] 2.4 Activate the `EndOfTurn` trigger after the reveal sequence and before priority; verify the effect order and that it fires once per instance per turn.
- [x] 2.5 Add `AfterDrawn` and `GameStart` content for perk abilities; verify Quickdraw, Clone and Superstar receive their events at the right time.
- [x] 2.6 Add the Empowered event and dispatch it when another effect increases a card's Power; verify Reinforce reacts and its own +1 does not re-trigger.
- [x] 2.7 Add repeat-own-ability hooks for Amplify, Encore and Overtime with copy bounds and no self-recursion; verify the event limit stops a forced cascade.
- [x] 2.8 Enforce ability location rules (hand, deck, play), disabled abilities and zone suppression for perk-granted effects; verify a hand-only perk resolves from hand and a suppressed OnReveal does not.
- [x] 2.9 Add harness checks for perk dispatch, StartOfTurn/EndOfTurn ordering, Empowered, repeat bounds and the event limit; verify `SnapClone.Harness` passes.

## 3. Compatibility and level-up

- [x] 3.1 Add compatibility predicates for `Any`, `RequiresOngoing`, `RequiresOnReveal`, `RequiresEndOfTurn` and `RequiresEmpower`; verify Amplify, Encore, Overtime and Reinforce resolve their host requirements.
- [x] 3.2 Filter level-up perk offers to perks with at least one compatible host and fewer than two copies; verify no dead offer is generated.
- [x] 3.3 Return an unavailable perk branch when no compatible perk exists; verify the level-up still offers the card branch and can resolve.
- [x] 3.4 Filter the host-selection grid to compatible cards and expose a disabled reason for incompatible cards; verify the level-up screen and demo state.
- [x] 3.5 Persist generated offers and compatibility state across level-up re-entry; verify a resumed level-up does not re-roll its perks.
- [x] 3.6 Apply the same compatibility gate to run events that grant a perk; verify an event cannot attach an incompatible perk.
- [x] 3.7 Add harness checks for compatibility filtering, branch unavailability and resume behavior; verify `SnapClone.Harness` passes.

## 4. Ability perks with existing effect reuse

- [x] 4.1 Implement Afterimage and Clone with unmodified separate copies; verify the copy has base values and no upgrades, bonuses or perks.
- [x] 4.2 Implement Reality Warp on top of the existing location replacement path; verify the zone index, totals and new location effect.
- [x] 4.3 Implement Regeneration with the existing regenerate path and the no-room case; verify discarded and destroyed hosts return with +2 Power only when a zone has room.
- [x] 4.4 Implement Tag In with hand/deck replacement and the destroyed-pile rule; verify the host is played for 0 and the destroyed card remains in the pile.
- [x] 4.5 Implement Mentor on the top deck card; verify the top card gains the host's current Power and an empty deck is a no-op.
- [x] 4.6 Implement Rally and Apex as Ongoing power queries; verify Rally counts other allied cards and Apex doubles only when the host is alone.
- [x] 4.7 Implement Silence and Wakandan Tech on the host; verify Silence keeps perk effects active and Wakandan Tech does not double its bonus twice.
- [x] 4.8 Implement Slingshot and Regroup with move/return and tie-breaking; verify the right-move room case and hand-full case.
- [x] 4.9 Implement Deadlink and Sidekick with the Zombie Horde and random character pools; verify +4 to each Horde and a -2 cost character in hand.
- [x] 4.10 Implement Salvage with lowest-Power selection, random ties and the +5 Power bonus; verify no-other-card is a no-op.
- [x] 4.11 Add harness checks for the eight reuse behaviors; verify `SnapClone.Harness` passes.

## 5. Meta-ability and new-trigger perks

- [x] 5.1 Implement Amplify, Encore and Overtime on the repeat hooks; verify one and two copies produce the expected repetition counts.
- [x] 5.2 Implement Hot Pursuit with last-turn move tracking and the current-turn cost reduction; verify the floor and no self-stacking.
- [x] 5.3 Implement Quickdraw: Discount and Quickdraw: Power on the draw event; verify the cost reduction persists for the combat and the granted OnReveal resolves once.
- [x] 5.4 Implement Overcharge and Overload with start-of-turn energy and the next-turn win check; verify the energy is available for the correct turn only.
- [x] 5.5 Implement Maneuverable with a once-per-combat player move flag; verify a second player move is rejected and effect moves do not consume the flag.
- [x] 5.6 Implement Superstar on the opening-hand dealer; verify forced cards enter before random cards and excess forced cards stay in the deck.
- [x] 5.7 Implement Taunt with a next-turn reservation; verify the reservation is single-use and expires on a full zone.
- [x] 5.8 Implement Unstoppable with play-restriction bypass, destroy immunity and power-reduction immunity; verify each rule independently.
- [x] 5.9 Implement Reinforce on the Empowered event; verify +1 per copy, no self-trigger and the event limit.
- [x] 5.10 Add harness checks for all meta-ability and new-trigger perks; verify `SnapClone.Harness` passes.

## 6. Combat actions and AI

- [x] 6.1 Add the Maneuverable player move to the action authority and combat input; verify the move uses the shared action rules and updates power.
- [x] 6.2 Enforce Taunt on the player's first card of the next turn; verify the input is restricted only when the zone has room.
- [x] 6.3 Enforce Taunt on the AI's first card of the next turn; verify the AI uses the taunted zone and expires the reservation when it cannot.
- [x] 6.4 Add harness checks for the action path, player restriction, AI placement and reservation expiry; verify `SnapClone.Harness` passes.

## 7. Persistence migration

- [x] 7.1 Migrate `profile.json` on load by removing retired perk ids from owned perks and copy counts; verify current ids and the rest of the profile survive.
- [x] 7.2 Migrate `run.json` on load by removing retired perk ids from saved cards; verify the card, upgrades, deltas and run node survive.
- [x] 7.3 Verify the migration writes the cleaned state on the next save and does not change the save schema.
- [x] 7.4 Add harness checks for a profile with old ids, a deck with old attached perks, a current perk and an unknown id; verify the migration result.
- [x] 7.5 Document the irreversible loss of retired attachments and the rollback behavior; verify the note matches the migration code.

## 8. Screens and documentation

- [x] 8.1 Update the level-up perk offer and host grid for ability text, compatibility and disabled hosts; verify the screen at 720x1280 and with a no-compatible-perk deck.
- [x] 8.2 Update the collection perks tab and detail for the 28 owned perks with trigger, compatibility and copies; verify the tab and detail.
- [x] 8.3 Remove the perk entry and purchase path from the global store; verify the store no longer lists or sells perks.
- [x] 8.4 Update the UI gallery/demo state to show the new perk states; verify the gallery renders the 28-perk collection and the level-up compatibility state.
- [x] 8.5 Document the 28-perk catalog, compatibility rules and acquisition model in `docs/perk-abilities.md` or the relevant UI document; verify the document matches the catalog and the screens.
- [x] 8.6 Run the UI visual-design and responsive checks at 720x1280 and 1080x2400; verify no overlap, no clipped text and reachable touch targets.

## 9. Integration and regression

- [x] 9.1 Run `dotnet build SnapClone.sln` and `dotnet run --project SnapClone.Harness`; verify both pass.
- [x] 9.2 Build and install the Android APK; verify the level-up, collection, store and combat screens on the emulator.
- [x] 9.3 Run a full run smoke test with a level-up perk graft, a second copy and a migrated save; verify the run reaches combat and the perk ability resolves.
- [x] 9.4 Run a combat regression pass covering hand, board, drag and drop, pile viewer, detail panels and pause overlay; verify combat behavior is unchanged.
- [x] 9.5 Check the 28-perk catalog for per-frame allocations, repeated effect lookups and the event limit; verify the budgets and no texture allocation per frame.
