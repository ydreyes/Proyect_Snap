# Tasks

## 1. Foundation and data model

- [x] 1.1 Add `CardSeries` (`CollectionLevel`, `Series1`, `Series2`) to `CardDef`, copy it in `CardDef.Clone`, and set `CollectionLevel` on the 13 current starters and `Series1` on the 46 Series 1 cards; verify the solution builds and the harness still loads 59 cards.
- [x] 1.2 Add `CombatCostOverride`, `MoveUsedThisCombat` and `MoveUsedTurn` to `CardInstance`; make `EffectiveCost` return the override when set and cap the computed cost at 6, and clear the combat-only fields in `ResetForCombat`; verify with unit checks for cap, override and reset.
- [x] 1.3 Add per-side discard counters, movement windows and the next-turn cost tax to `BoardState`, including reset and expiry helpers; verify with harness checks for increment, reset, window expiry and tax turn.
- [x] 1.4 Add `ActionEventKind.AddedToHand`, `EffectTrigger.AfterAddedToHand` and the `TriggerFor` mapping; verify a fixture effect fires for `AddToHand` and not for a normal draw or a manual return.
- [x] 1.5 Add `FloodedLastPlayableTurn`, `IsFlooded(turn)`, `Flood(turn)` and `ClearFlood()` to `ZoneInstance`; verify the boundary at the grace turn, the following turn and a replacement.
- [x] 1.6 Add the `IPlayRestriction`, `PlayLockEffect`, `PassiveMovePermission` and `IPlayerMovePermission` queries to `EffectProcessor`, and split `IsZonePlayLocked` from `IsZoneRemovalLocked`; verify existing Professor X behavior is unchanged and a play-only marker does not block additions.
- [x] 1.7 Add the per-series scaffold to `GlobalStoreCatalog` while keeping the legacy 59-card partition active; verify the six legacy bundle ids and their card membership are identical to the current catalog.
- [x] 1.8 Update `ContentLoader.ValidateCatalog`, `GlobalStoreCatalog` and the harness to read one `ExpectedDraftable` constant instead of a literal 59; verify the build and harness are green at the current count.

## 2. Wave 1 - twenty simple cards

- [x] 2.1 Register the 20 Wave 1 definitions with exact costs, powers and Spanish ASCII descriptions, and assign their draft themes; verify each card exists, has a non-empty description and resolves its effect ids.
- [x] 2.2 Implement Wolfsbane, Spectrum, White Tiger, Agent 13, Jubilee, Iceman, Scorpion, Nakia, Okoye, Rhino, Leech, Killmonger and Shang-Chi; verify one harness scenario per card, including hand cap, random targets, 1-cost destruction and the 10-Power threshold.
- [x] 2.3 Implement Ka-Zar, Mister Fantastic, Morbius, Warpath, Vulture, Sunspot and Jessica Jones; verify ongoing power, adjacent additions, discard count, empty-location condition, self-move bonus, unspent energy and the delayed inverse condition.
- [x] 2.4 Add the `token_tiger` definition (0/10, non-draftable) and reuse `token_rock` for Rhino; verify neither token enters the draft and White Tiger adds at most one Tiger to a random other location with room.
- [x] 2.5 Update the catalog expected count to 79 and the theme coverage; verify the harness reports 79 draftables, unique names/ids and enough Control/Destroy/OnReveal/Neutral cards for the themed draft.

## 3. Wave 2 - destroy, discard and hand entry

- [x] 3.1 Dispatch `AddedToHand` from `AddToHand`, from `DrawFromEnemyDeck` and from Apocalypse's replacement; verify The Collector gains +1 for effect additions and Cable, and not for a normal draw or a manual drag return.
- [x] 3.2 Add `token_winter_soldier` (0/8, non-draftable) and Bucky Barnes' destroy replacement; verify Bucky leaves play without a pile, the Winter Soldier appears in the same slot, and the destruction event still fires.
- [x] 3.3 Implement Sabretooth's destroy replacement with a zero-cost combat override; verify hand-cap success and failure, cost 0 on replay, and the `AddedToHand` trigger.
- [x] 3.4 Implement Swarm's `AfterDiscarded` copies with a zero-cost override; verify two copies at most, hand-cap behavior, independent instances, and Collector triggers for each copy.
- [x] 3.5 Increment the discard counter on every successful discard, including replaced discards, and read it from Morbius; verify a discard, a replaced discard and a new combat reset.
- [x] 3.6 Update the catalog expected count to 83 and add the Wave 2 harness checks; verify all four cards and the shared hand-entry/discard behaviors pass.

## 4. Wave 3 - zones and play restrictions

- [x] 4.1 Implement the passive play restrictions for Ebony Maw (after turn 3) and The Infinaut (played last turn); verify `PlayCard` rejects both when illegal and accepts them when legal.
- [x] 4.2 Split the play-only lock from the full Professor X lock and update `AddCardToZone` to check only the full lock; verify Ebony Maw still allows effect additions and movement while Professor X blocks them.
- [x] 4.3 Implement Storm's flood, the later-grace rule and flood clearing on location replacement; verify the last playable turn, the blocked turn, movement, effect additions and the plaque status.
- [x] 4.4 Implement Sandman's next-turn cost tax, the hand/deck application and the hand-entry application; verify both sides, stacking, expiry, cap 6 and a card drawn/added during the taxed turn.
- [x] 4.5 Make `AIStrategy` use the same card and zone legality queries; verify no stall with Ebony Maw, Storm or The Infinaut and that a taunt reservation expires on an illegal zone.
- [x] 4.6 Update the catalog expected count to 87 and add the Wave 3 harness checks; verify all four cards, both lock types and the AI no-stall scenarios pass.

## 5. Wave 4 - movement

- [x] 5.1 Implement the printed movement permission query and the per-combat/per-turn usage flags; verify Nightcrawler once per combat and Vision once per turn, including the same-turn move not consuming the permission.
- [x] 5.2 Implement Cloak's movement window on `BoardState`; verify both sides, destination-only scope, one use per card, expiry and a full or fully locked destination.
- [x] 5.3 Route board-origin drag drops through `MoveCardByPlayer` and allow older cards to be lifted only when a permission or window applies; verify older locked cards cannot be lifted, older moveable cards can move but cannot return to hand, and a failed move leaves the permission unused.
- [x] 5.4 Add the move indicator to `CardView` and the movement status to the card detail panel; verify the indicator is visible, does not cover the name/cost/power and updates after a move or turn change.
- [x] 5.5 Update the catalog expected count to 90 and add the Wave 4 harness checks; verify the three cards and the drag/drop scenarios pass at 720x1280.

## 6. Wave 5 - Odin and Hobgoblin

- [x] 6.1 Implement the post-reveal Odin repeat pass; verify Odin repeats White Tiger, Spectrum and Jessica Jones, respects Cosmo and ability-disabled cards, does not repeat another repeat hook, and is bounded with multiple Odins.
- [x] 6.2 Integrate Jubilee's pulled-card reveal with the repeat pass; verify the pulled card reveals and resolves its On Reveal, does not dispatch `AfterPlayed`, and does not become a target for "played last turn" checks.
- [x] 6.3 Implement Hobgoblin's switch-sides action; verify same-location control change, capacity and full-lock rejection, no pile entry, no movement event, and scene slot reconciliation.
- [x] 6.4 Update the catalog expected count to 92 and add the Wave 5 harness checks; verify Odin/Hobgoblin and the complete 92-card registration pass.

## 7. Final integration

- [x] 7.1 Add the 8 CL cards to `StarterSet` and implement the starter migration in `ProfileDto.Normalize`; verify a stale profile gains all 21 starters, no owned card is removed, and the draft pool uses 21 starters.
- [x] 7.2 Build the Series 2 store batch (6 direct + 3 bundles of 6/6/7) with `bundle_s2_` ids and restore the every-draftable-assigned validation; verify the legacy six bundles keep identical membership and the new batch totals 25.
- [x] 7.3 Update the collection and global store counts, comments and paging; verify 92 cards, 12 direct cards and 9 bundles are reachable at 720x1280.
- [x] 7.4 Update the zone status, moveable indicator, card detail and collection/store documentation; verify the labels and indicators match the board state in a manual pass.
- [x] 7.5 Extend the harness to the final counts (92 draftable, 21 starters, 12 direct, 9 bundles, 5x6 / 3x7 / 1x8, 59 bundle cards) and keep the per-wave card checks; verify the full harness passes.
- [x] 7.6 Run `dotnet build SnapClone.sln` and the harness; verify a full six-turn combat with a deck containing one card per wave and an Odin + White Tiger / Jessica Jones combo, plus an AI combat with Ebony Maw, Storm and The Infinaut in play.
- [x] 7.7 Update the affected OpenSpec main specs during archive preparation only after the implementation is verified; verify `openspec validate "series-2-cards" --strict` still passes before archiving.
