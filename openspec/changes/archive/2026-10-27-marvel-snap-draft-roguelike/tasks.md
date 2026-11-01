# Tasks

## 1. Project Structure and Core Foundation
- [x] 1.1 Create directory structure (Core/, Data/, Effects/, Gameplay/, UI/, Scenes/) and verify all directories exist
- [x] 1.2 Create Core/Services/GameServices.cs with service container and verify service registration works
- [x] 1.3 Create Core/Services/InputService.cs, TimeService.cs, RandomService.cs and verify they can be instantiated
- [x] 1.4 Create UI/Primitives/ShapeDrawer.cs with primitive drawing helpers using SpriteBatch and 1x1 texture, verify it compiles
- [x] 1.5 Create Core/StateMachine/IGameState.cs and StateMachine.cs, verify state transitions work
- [x] 1.6 Create Scenes/IScene.cs and SceneManager.cs, integrate with Game1.cs and verify game compiles and runs

## 2. Data Model and Registries
- [x] 2.1 Create Data/Definitions/ enums (CardType, EffectTrigger, etc.) and verify they compile
- [x] 2.2 Create Data/Definitions/CardDef.cs, ZoneDef.cs, EffectDef.cs, PerkDef.cs and verify structure is correct
- [x] 2.3 Create Gameplay/Cards/CardInstance.cs, Gameplay/Board/ZoneInstance.cs and verify they compile
- [x] 2.4 Create Data/Registries/CardRegistry.cs, ZoneRegistry.cs, EffectRegistry.cs, PerkRegistry.cs and verify lookups work
- [x] 2.5 Create initial content definitions for test data (3 zones, 20 effects across 5 types, 3 perks, starter cards) and verify registries load them

## 3. Effect System
- [x] 3.1 Create Effects/IEffect.cs, Effects/EffectContext.cs interfaces/classes and verify they compile
- [x] 3.2 Create Effects/EffectProcessor.cs with trigger processing and verify it compiles
- [x] 3.3 Create Gameplay/Combat/PowerResolver.cs for dynamic power calculation and verify it compiles
- [x] 3.4 Implement 3 test zone effects (+1 all, -1 all, +2 to highest cost) using Strategy pattern and verify they work through effect system
- [x] 3.5 Create Effects/Implementations/ for base effect types and verify registry can instantiate them

## 4. Board and Combat Core
- [x] 4.1 Create Gameplay/Board/BoardState.cs with 3 zones, hands, decks and verify structure
- [x] 4.2 Create Gameplay/Combat/PriorityCalculator.cs for zone control and priority determination and verify logic
- [x] 4.3 Create Gameplay/Combat/PlaySystem.cs for playing cards with validation and verify rules enforced
- [x] 4.4 Create Gameplay/Combat/RevealResolver.cs for card reveal ordering by priority and play order and verify ordering is correct
- [x] 4.5 Create Gameplay/Combat/TurnManager.cs with TurnStart, PlayPhase, TurnEnd flow and time limits and verify turn structure
- [x] 4.6 Implement zone reveal sequentially at turn start and verify zone effects trigger

## 5. AI Opponent
- [x] 5.1 Create Gameplay/AI/AIStrategy.cs with basic heuristic decision making and verify it compiles
- [x] 5.2 Create Gameplay/AI/AIController.cs that follows same rules as player and verify it plays cards legally

## 6. Draft and Roguelike Progression
- [x] 6.1 Create Gameplay/Draft/DraftSystem.cs for initial 8-card draft with 3-choice selections and copy limits (max 2 per card) and verify logic
- [x] 6.2 Create Gameplay/Draft/PerkAssigner.cs for assigning random perks to last 3 draft cards with player choice and verify
- [x] 6.3 Create Gameplay/Run/RunState.cs to track fight index, lives (max 2), deck state and verify
- [x] 6.4 Create Gameplay/Run/ProgressionManager.cs for fight sequence (15 fights with sub-bosses/final boss) and verify sequence
- [x] 6.5 Create Gameplay/Run/RewardSystem.cs for win/loss rewards, deck substitution at 12 cards, perk attachment and verify rules

## 7. UI Scenes and Integration
- [x] 7.1 Create Scenes/MenuScene.cs with New Run and Quit using primitive UI and verify navigation
- [x] 7.2 Create Scenes/DraftScene.cs for card selection and perk assignment with primitive drawing and verify flow
- [x] 7.3 Create Scenes/CombatScene.cs displaying 3 zones, hands, timer, priority with primitive UI and verify display
- [x] 7.4 Create Scenes/RewardScene.cs and Scenes/PerkScene.cs for post-combat rewards and verify flows
- [x] 7.5 Create Scenes/GameOverScene.cs and verify game over at 0 lives
- [x] 7.6 Wire all systems together in Game1/StateMachine for full flow: Menu -> Draft -> Combat -> Rewards -> ... -> Boss/GameOver and verify end-to-end flow compiles and runs
