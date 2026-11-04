# Proposal

## Why

The 46-card Series 1 expansion cannot be built on the current effect engine: it only resolves `OnReveal` and queries self-scoped `Ongoing` effects, while the expansion needs play, move, destroy, discard, draw and game-start events plus continuous effects that target other cards and other locations. The game also has two separate play paths (the scene and the AI) and three latent rule bugs that the expansion would turn into exploits: a card can reveal more than once by returning to hand and replaying, destroying a card refunds its energy because energy is derived from cards still on the board, and a card destroyed before its reveal still applies its `OnReveal`.

This change builds the action and event core first, with no new card content, so the six card-family batches that follow can be implemented against one authority.

## What Changes

- **One play authority.** Introduce a model-level action service that owns `PlayCard`, `MoveCard`, `DestroyCard`, `DiscardCard`, `DrawCard`, `BanishCard`, `AddToHand`, `AddTokenToZone`, `Copy`, `Transform` and `Merge`. The combat scene and the AI both call it; neither mutates zones, hands or piles directly.
- **Event dispatch.** Add trigger families for `AfterPlayed`, `AfterMoved`, `AfterDestroyed`, `AfterDiscarded`, `AfterDrawn` and `GameStart`, with a deterministic event queue and a loop guard. No card in this change references them; they are infrastructure for the Series 1 batches.
- **Card instance state.** Add `HasRevealed`, `AbilitiesDisabled` and a multi-entry `PendingEffects` list; add a per-turn play marker, `BoardState.PlayLog` and `BoardState.EnergyLedger`.
- **Reveal once per combat.** `RevealResolver` excludes cards that already revealed, and `EffectProcessor` skips cards that are no longer in play. Returning a card to hand no longer re-arms its `OnReveal`.
- **Explicit energy ledger.** Charge a card's cost when it is played, refund only when it returns to hand, and reserve energy while a card is lifted. Destroying, merging, banishing or moving a card no longer changes the turn's available energy. The player and AI share the ledger.
- **Groundwork for card identity.** Separate draftable card definitions from token definitions and add the token instance-creation path, so the tokens required by Korg, Squirrel Girl, Mister Sinister and the copy/merge cards do not enter the draft or reward pools.
- **Centralized capacity and constraint checks.** Every action that adds a card to a zone checks the four-slot limit, so effects cannot create a fifth card and break the board views. A constraint hook is added for later zone locks; no lock is implemented in this change.
- **Pile state cleanup.** A card entering a pile clears its play and pending state but keeps its persistent bonus, perks and `HasRevealed` flag. `CardPileManager` remains the only writer of pile membership.
- **BREAKING (behavior):** a revealed card cannot reveal again in the same combat by returning to hand; a destroyed card no longer refunds its cost; a card destroyed before its reveal does not apply `OnReveal`.

## Capabilities

### New Capabilities

- None. The change extends capabilities the project already declares.

### Modified Capabilities

- `effect-system`: trigger families expand beyond `OnReveal` and `Ongoing`; the processor dispatches events in a deterministic order, refuses to resolve effects for cards that left play, and treats `HasRevealed` as the once-per-combat guard.
- `combat-system`: card play, movement, destruction, discard and draw go through the action authority; reveal order excludes already-revealed and no-longer-in-play cards; energy is tracked by an explicit ledger and is not refunded by destroy, merge or banish.
- `card-mechanics`: card instances gain reveal, per-turn play, pending-effect and disabled-ability state; returning to hand preserves the reveal flag and persistent bonus; token and copy instances are distinct from draftable definitions.
- `ai-opponent`: the AI plays, moves, destroys, discards and draws through the same action authority and event dispatch as the player, including the explicit energy ledger.
- `draft-system`: the draft pool offers only draftable definitions; token definitions are excluded.
- `roguelike-progression`: reward offers draw only from draftable definitions; token definitions are excluded.

## Impact

- Content: no new cards, zones or effects. `ContentLoader` is unchanged except for any validation hook the design requires.
- Effects: `Effects/IEffect.cs`, `Effects/EffectProcessor.cs`, `Effects/Implementations/BaseEffectFactory.cs`; new event and action types under `Effects/` or `Gameplay/`.
- Gameplay: `Gameplay/Cards/CardInstance.cs`, `Gameplay/Board/BoardState.cs`, `Gameplay/Board/ZoneInstance.cs`, `Gameplay/Board/CardPileManager.cs`, `Gameplay/Combat/TurnManager.cs`, `Gameplay/Combat/RevealResolver.cs`, `Gameplay/Combat/PowerResolver.cs`, `Gameplay/AI/AIController.cs`, `Gameplay/AI/AIStrategy.cs`.
- Presentation: `Scenes/CombatScene.cs` replaces `PlaceInSlot` and the AI view creation path with calls to the action service; no new screen or modal.
- Verification: `dotnet build SnapClone.sln` plus harness checks for double reveal, destroyed-before-reveal, energy on destroy/merge/banish, return-to-hand refund, event ordering, loop guard, capacity and player/AI parity.
- Out of scope: `C0b` continuous auras and repetition; the 46 new cards; merge destination rules; Yondu's run-scoped banish decision; target-selection prompts; hand limit; location effects.
