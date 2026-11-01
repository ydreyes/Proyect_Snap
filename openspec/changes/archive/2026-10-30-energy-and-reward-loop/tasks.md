# Tasks

## 1. Energy Model

- [x] 1.1 Add a per-turn energy pool (`min(turn, 6)`) and an available-energy derivation that subtracts the cost of player cards in zones whose `PlayedTurn` equals the current turn, plus an optional reserved cost. Verify: turn 1 shows 1; after a 1-cost, 0; turn 2 shows 2.
- [x] 1.2 Stamp `PlayedTurn` only when a card enters a zone from hand, and clear it when a card returns to hand. Verify: a slot-to-slot move keeps its original play turn, and a returned card is charged again when replayed.
- [x] 1.3 Remove the unused `PlaySystem` (its only call validates against energy 0) and confirm no references remain. Verify: `dotnet build` succeeds.

## 2. Cost-Aware Drag and Drop

- [x] 2.1 Reject a hand-to-slot drop when the card's cost exceeds the available energy, returning it to the hand with no board or energy change. Verify: a 3-cost card cannot be placed on turn 1.
- [x] 2.2 Release a current-turn card's cost when it is dragged from the board to the hand, and leave energy untouched for earlier-turn cards. Verify both cases on turn 3.
- [x] 2.3 Reserve the dragged card's cost while a current-turn board card is in the air. Verify: lifting a 3-cost card, playing 2+1, then returning the 3-cost is impossible.
- [x] 2.4 Show available energy in the badge (not the turn number) and dim unaffordable hand cards. Verify: after playing a 2-cost on turn 3, the badge reads 1 and only affordable cards look enabled.

## 3. AI Energy

- [x] 3.1 Filter AI plays to cards it can afford and rank them by power per cost, with cost 0 first and a random tie-break. Verify: the AI never plays a card above its available energy.
- [x] 3.2 Let the AI play repeatedly while an affordable card and a free slot exist, keeping the fewest-cards zone spread. Verify: on turn 3 the AI can play two 1-cost cards, and plays nothing when nothing is affordable.
- [x] 3.3 Bind every AI play to a board view and recalculate zone power. Verify: all AI plays for a turn appear on the board before the player acts.

## 4. Perks and Power

- [x] 4.1 Compute power as base power plus attached perk modifiers, never reading `CurrentPower` as an input. Verify: a card with `+2` keeps the same power after several `UpdateAllPowers` calls.
- [x] 4.2 Re-resolve power when a perk is attached and when a combat starts. Verify: a perk gained from a reward is reflected in hand and on the board in the next combat.
- [x] 4.3 Draw a perk badge on the card view for hand and board rendering. Verify: cards with `+1`, `+2` and `-1` render their modifier legibly.

## 5. Run Progression and Reward Offers

- [x] 5.1 Map combat numbers 1..15 to 3 cycles of 5, returning card reward at positions 1 and 3, perk reward at 2, no reward at 4 and 5, and FinalBoss at 15. Verify: positions 1..5, 10 and 15.
- [x] 5.2 Add pending-reward kind and advance-on-completion flags to the run state, plus deck cap helpers. Verify: a new run resets both.
- [x] 5.3 Implement combat-result routing for win with reward, win without reward, loss, tie and game over. Verify the scene target for each case.
- [x] 5.4 Build duplicate-free card offers from the catalog honoring the copy limit, and perk offers from the deck using positive perks only. Verify: no repeated offer and no negative perk.
- [x] 5.5 Register the reward system and progression manager in the service container. Verify both resolve at runtime and the build succeeds.

## 6. Reward Screen

- [x] 6.1 Render 3 offered cards with their perks, a header and pick interaction. Verify: picking adds the card, with its perk, to the run deck.
- [x] 6.2 Add the deck-cap state: at 12 cards the pick asks for one card to remove, with a skip action. Verify: the deck never exceeds 12 and skipping leaves it unchanged.
- [x] 6.3 Render the perk-reward mode from the deck and attach the chosen perk to the chosen card. Verify: deck size is unchanged and that card's power increases.
- [x] 6.4 Route out of the reward screen from the run state flag. Verify: a win reward advances the run, a loss reward replays the same combat.

## 7. Combat Result and HUD

- [x] 7.1 Treat a tie as a defeat in the modal and in routing. Verify: a tied match costs a life and repeats the combat.
- [x] 7.2 Replace the hard-coded reward transition with progression routing. Verify: win, loss and tie leave combat through the correct path.
- [x] 7.3 Reset per-combat card state when combat starts. Verify: a card played in a lost combat begins the retry out of play with base-plus-perk power.
- [x] 7.4 Show combat number out of 15 and remaining lives in the top bar. Verify: values after a win, a loss and a retry.
- [x] 7.5 Show a victory end state when combat 15 is won and a game-over end state when lives reach 0, both returning to the menu. Verify both.

## 8. Integration Verification

- [x] 8.1 Drive a run through one full cycle and a jump to combat 15. Verify: rewards at combats 1, 2 and 3, none at 4 and 5, and the FinalBoss at 15.
- [x] 8.2 Verify a loss at 1 life ends the run with no reward, and a loss with lives left offers a card and replays the same combat.
- [x] 8.3 Run `dotnet build` and confirm the project compiles with no leftover references to removed types.
