# Tasks

## 1. Domain state and definitions

- [x] 1.1 Add `ActionEventKind`, `ActionEvent`, `ActionCause`, `ActionResult`, `PlayResult`, `DrawResult` and `PlayRecord` under `SnapClone/Gameplay/Actions/`; verify with `dotnet build SnapClone.sln` and a harness check that the new types compile and a `PlayRecord` preserves side, card, zone, turn and play order.
- [x] 1.2 Add `CardInstance.HasRevealed` and `CardInstance.AbilitiesDisabled`; make `ClearPlayState()` clear pending effects but leave both new fields untouched; verify with a harness check that returning a card to hand keeps `HasRevealed` and clears pending effects.
- [x] 1.3 Replace the single `PendingBonusTurn`/`PendingBonusZoneIndex`/`PendingBonusAmount` channel with a `PendingEffects` list that supports at least two simultaneous pending effects; verify with a harness check that two pending effects can be armed on one card and resolved independently.
- [x] 1.4 Add `BoardState.PlayLog`, `BoardState.PlayedThisTurn` and per-side energy ledgers; verify with a harness check that a new board starts with an empty log, empty play set and zero charged/refunded energy.
- [x] 1.5 Add `CardDef.IsToken` (default `false`) and `CardRegistry.GetDraftable()`; verify with a harness check that the current 20-card pool has 20 draftable cards and a synthetic token is excluded.
- [x] 1.6 Add `ZoneInstance.CanAdd(BoardSide)` and an empty `IZonePlayConstraint` hook list; verify with a harness check that a zone with four cards on one side rejects a fifth addition for that side and still accepts one for the other side.

## 2. Energy ledger and play log

- [x] 2.1 Implement `TurnEnergyLedger` with `Charge`, `Refund`, `Charged`, `Refunded` and `Available(pool)`; verify with a harness check that charge decreases available, refund restores it and a refund for an uncharged card is rejected.
- [x] 2.2 Wire `TurnManager.AvailableEnergy(BoardSide)` to the ledger for both sides and reset both ledgers in `StartTurn`; verify with a harness check that the player and AI start each turn with `min(turn, 6)` and that unspent energy is lost at turn end.
- [x] 2.3 Implement `PlayRecord` append/remove helpers on `BoardState`; verify with a harness check that a play adds a record, returning the same card to hand removes its records and moving it later keeps the original zone.
- [x] 2.4 Remove `TurnManager.ReserveEnergy`, `ReleaseReservation` and the derived `SpentEnergy` scan; verify with `dotnet build` and a harness check that lifting a played card does not change available energy.

## 3. CardActionService

- [x] 3.1 Create combat-scoped `CardActionService` with `PlayCard`, `ReturnToHand`, `LiftCard` and `MoveCard`; verify with harness checks for a legal play, an unaffordable play, a full-zone play, a same-zone lift and a cross-zone move.
- [x] 3.2 Implement `PlayCard` validation and recording: energy charge, capacity, `PlayedTurn`, `PlayOrderIndex`, `PlayedZoneIndex`, `Location`, `IsInPlay`, play log and `PlayedThisTurn`; verify with a harness check that a successful play records all fields and a rejected play mutates nothing.
- [x] 3.3 Implement `ReturnToHand` with cost refund only for cards charged this turn, pending-effect clearing, reveal-marker preservation and play-log removal; verify with a harness check for play, return, replay and second return.
- [x] 3.4 Implement `LiftCard` with no energy change and no event, and make a lift followed by a drop back into a slot preserve the charge; verify with a harness check that available energy is identical before the lift and after the drop.
- [x] 3.5 Implement `MoveCard` with destination capacity checks, original played zone preservation and no energy change; verify with a harness check that a move into a full zone is rejected and a legal move preserves `PlayedZoneIndex`.
- [x] 3.6 Implement `DestroyCard`, `DiscardCard`, `BanishCard`, `AddToHand`, `AddTokenToZone` and `DrawCard`; verify with harness checks that destroy/discard/banish do not refund energy, a token is rejected when the zone is full and a draw respects deck order.
- [x] 3.7 Add a `CanDestroy` hook and a `GameStart` dispatch entry point; verify with a harness check that a synthetic destroy protection blocks the action without changing piles or energy, and that `GameStart` can be dispatched before turn 1.

## 4. Event dispatch and effect processor

- [x] 4.1 Extend `EffectTrigger` with `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn` and `GameStart`; verify with a harness check that each enum value round-trips through an `EffectDef` and a factory-created effect.
- [x] 4.2 Extend `EffectContext` with the action event payload (kind, triggering card, origin zone, destination zone and play log); verify with a harness check that an event-triggered effect receives all payload fields.
- [x] 4.3 Implement `EffectProcessor.Dispatch(ActionEvent)` with the deterministic card scan described in `design.md`; verify with a harness check that the same event sequence produces the same reactor order on two runs and for player and AI initiators.
- [x] 4.4 Add the loop guard with a 256-event cap and a `LastActionHitLimit` flag; verify with a harness check that a synthetic effect that re-emits the same event stops at the cap and reports the limit.
- [x] 4.5 Make `CardActionService` emit `AfterPlayed` once per instance per turn, using `BoardState.PlayedThisTurn`; verify with a harness check that play, return and replay emit one event.
- [x] 4.6 Make `CardActionService` emit `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn` and `GameStart` at the points described in the spec; verify with a harness check that a prevented destroy emits no event and a successful one emits exactly one.
- [x] 4.7 Add the `EffectProcessor` validation pass for card-referenced effects and exempt zone effects; verify with a harness check that the current pool passes, a missing card effect id fails, and the three registered zone effects do not fail.

## 5. Reveal and pending-effect rules

- [x] 5.1 Make `RevealResolver` exclude cards with `HasRevealed` while still selecting cards played this turn; verify with a harness check that a revealed card returned to hand and replayed is absent from the next reveal order.
- [x] 5.2 Make `EffectProcessor.ApplyOnReveal` set `HasRevealed`, skip cards that are no longer in a zone or `IsInPlay`, and apply each effect in order; verify with a harness check that a card destroyed by an earlier reveal does not apply its `OnReveal`.
- [x] 5.3 Preserve `HasRevealed` through `CardPileManager.Move`, `CardInstance.ClearPlayState` and `AddToHand`, and clear it in `ResetForCombat`; verify with a harness check for return to hand, destroy, discard and new combat.
- [x] 5.4 Migrate `HawkeyeOnRevealEffect` and `EffectProcessor.ResolvePendingBonuses` to the `PendingEffects` list; verify with the existing Hawkeye harness checks: gains on the following turn, does not gain without an ally, clears the pending effect and does not apply twice.
- [x] 5.5 Migrate `StarLordOnRevealEffect` from the board scan to `BoardState.PlayLog`; verify with a harness check that a rival card played in the zone and then moved or destroyed still grants +4.

## 6. Scene integration

- [x] 6.1 Replace `CombatScene.PlaceInSlot`'s hand path with `CardActionService.PlayCard` and attach the returned play record to the view; verify with a harness check for the model result and a manual check that a hand card still lands in the chosen slot and deducts energy.
- [x] 6.2 Replace the board-return path (`AddToHand`, `CancelDragToHand`) with `CardActionService.ReturnToHand`; verify with a harness check for the refund and a manual check that dropping a played card on the hand area restores its cost.
- [x] 6.3 Replace `LiftFromSlot` and cross-zone board drops with `LiftCard`/`MoveCard`, keep same-zone rearranges view-only, and remove `ReserveEnergy`/`ReleaseReservation` calls; verify with a harness check for lift/drop energy and a manual check that a same-turn board card can still be repositioned.
- [x] 6.4 Replace `PlayAIAll`'s direct model mutation with the `AIController` play record and create the opponent view at the returned slot; verify with a manual check that AI cards still appear immediately and a harness check that AI plays mutate the same fields as player plays.
- [x] 6.5 Make `SyncViewsIfDirty` refresh power totals after any `Version` bump, including power-only changes; verify with a manual check that a triggered power change updates the card number and plaque in the same frame.

## 7. AI integration

- [x] 7.1 Make `AIController.TryPlayOne` call `CardActionService.PlayCard` and return its record; verify with a harness check that AI plays respect energy, capacity and the play log.
- [x] 7.2 Remove `AIStrategy.GetAvailableEnergy`'s derived scan and use `TurnManager.AvailableEnergy(BoardSide.AI)`; verify with a harness check that the AI cannot overspend after a destroy or merge and can spend again after a return.
- [x] 7.3 Verify AI event parity: an AI play emits `AfterPlayed` once, an AI destroy emits `AfterDestroyed` and an AI token respects the four-card limit; verify with harness checks for all three.

## 8. Draft, rewards and content validation

- [x] 8.1 Make `DraftSystem.GetChoices` use `CardRegistry.GetDraftable()`; verify with a harness check that a synthetic token never appears in draft choices.
- [x] 8.2 Make `RewardSystem.GetCardRewards` use `CardRegistry.GetDraftable()`; verify with a harness check that a synthetic token never appears in win or loss reward offers.
- [x] 8.3 Make `CombatScene.GetCardDefinitions` use `GetDraftable()` for the fallback player deck and the AI deck; verify with a harness check that a synthetic token is absent from both generated decks.
- [x] 8.4 Add content validation at `ContentLoader.LoadAll` for card-referenced effects and zone-effect exemption; verify with a harness check that the current 20-card pool loads and a deliberately broken card effect id fails before combat.

## 9. Integration checks

- [x] 9.1 Run `dotnet build SnapClone.sln` with zero warnings introduced by this change; verify the command exits successfully.
- [x] 9.2 Run the extended `%TEMP%\opencode\eps-verify` harness with zero failures, including the existing checks and the new reveal, energy, capacity, token, event-order and AI-parity checks; verify the exit code and the failure count.
- [x] 9.3 Play one full combat with the existing 20-card pool and verify that Medusa, Star-Lord and Sentinel no longer gain a second reveal after a takeback, that destroying a card does not increase energy, and that the pool still contains exactly 20 draftable cards.
- [x] 9.4 Confirm no Series 1 card, token, location or zone effect was added in this change; verify by listing `ContentLoader` registrations and comparing the card count to 20.
