using System;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Meta;

namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Unica autoridad de la secuencia de la run. Lee y escribe el nodo
    /// pendiente en <see cref="RunState"/>; ninguna escena calcula el
    /// siguiente nodo por su cuenta.
    /// </summary>
    public class RunDirector
    {
        private readonly RunState _runState;
        private readonly RandomService _random;
        private readonly EnemyRegistry _enemyRegistry;

        public RunDirector(RunState runState, RandomService randomService,
            EnemyRegistry enemyRegistry)
        {
            _runState = runState;
            _random = randomService;
            _enemyRegistry = enemyRegistry;
        }

        public RunState State => _runState;

        /// <summary>
        /// El ultimo resultado fue una derrota cubierta por una vida extra.
        /// La escena lo usa para guardar el checkpoint antes del reintento.
        /// </summary>
        public bool LastDefeatConsumedExtraLife { get; private set; }

        public RunNode GetNextNode() => _runState.PendingNode;

        public string GetNextScene() => SceneName(_runState.PendingNode);

        public CombatKind GetCurrentCombatKind() =>
            RunRewardTable.Classify(_runState.TotalCombats + 1);

        public static string SceneName(RunNode node)
        {
            switch (node)
            {
                case RunNode.Draft: return "Draft";
                case RunNode.EnemySelect: return "EnemySelect";
                case RunNode.Combat: return "Combat";
                case RunNode.LevelUp: return "LevelUp";
                case RunNode.Shop: return "Shop";
                case RunNode.Event: return "Event";
                case RunNode.ActTransition: return "ActTransition";
                case RunNode.SubBossIntro: return "SubBossIntro";
                case RunNode.Summary: return "Summary";
                default: return "Menu";
            }
        }

        // ------------------------------------------------------------------
        // Resultado del combate
        // ------------------------------------------------------------------

        /// <summary>
        /// Aplica el resultado una sola vez. La instantanea es la unica fuente
        /// de verdad: si ya se consumio, el resultado se rechaza.
        /// </summary>
        public bool ApplyCombatResult(string result)
        {
            if (_runState.PendingCombat == null) return false;

            if (result == "Player")
                OnCombatWon();
            else
                OnCombatLost();

            return true;
        }

        public RunNode OnCombatWon()
        {
            LastDefeatConsumedExtraLife = false;

            var kind = GetCurrentCombatKind();
            var reward = RunRewardTable.For(kind);

            _runState.AddExperience(reward.Xp + RunModifierSystem.XpBonusFor(_runState));
            _runState.AddMoney(reward.Money + RunModifierSystem.MoneyBonusFor(_runState));

            if (kind == CombatKind.Normal)
                _runState.NormalCombatsWon++;
            else if (kind == CombatKind.SubBoss)
                _runState.SubBossesWon++;
            else if (kind == CombatKind.FinalBoss)
                _runState.FinalBossDefeated = true;

            _runState.CombatsWon++;
            _runState.TotalCombats++;

            if (_runState.ActIndex <= RunActTable.LastNormalAct)
                _runState.CombatIndexInAct++;

            _runState.PendingCombat = null;

            if (kind == CombatKind.FinalBoss)
            {
                _runState.Outcome = RunOutcome.Won;
                _runState.PendingNode = RunNode.Summary;
                return _runState.PendingNode;
            }

            if (_runState.PendingLevelUps.Count > 0)
            {
                _runState.PendingNode = RunNode.LevelUp;
                return _runState.PendingNode;
            }

            return ResolvePostCombatNode();
        }

        public RunNode OnCombatLost()
        {
            LastDefeatConsumedExtraLife = false;

            // La vida extra se consume antes del reintento y el mismo enemigo
            // vuelve con una semilla nueva. Como maximo hay una carga.
            if (_runState.ExtraLifeCharges > 0 && _runState.PendingCombat != null)
            {
                _runState.ExtraLifeCharges--;

                var repeat = _runState.PendingCombat;
                repeat.Seed = NewSeed();

                _runState.RematchCombat = null;
                _runState.PendingCombat = repeat;
                _runState.PendingNode = RunNode.Combat;
                _runState.Outcome = RunOutcome.Active;
                LastDefeatConsumedExtraLife = true;

                return _runState.PendingNode;
            }

            _runState.Outcome = RunOutcome.Lost;
            _runState.PendingNode = RunNode.Summary;

            // El snapshot se retira del combate pero se conserva para que el
            // resumen pueda ofrecer un reintento contra el mismo enemigo.
            _runState.RematchCombat = _runState.PendingCombat;
            _runState.PendingCombat = null;

            return _runState.PendingNode;
        }

        /// <summary>
        /// Reabre el combate perdido: mismo enemigo y snapshot previo, nuevo
        /// intento y nueva semilla. La seleccion de rival no se repite.
        /// </summary>
        public RunNode OnRematchRequested()
        {
            var rematch = _runState.RematchCombat;
            if (rematch == null) return _runState.PendingNode;

            rematch.Attempt++;
            rematch.Seed = NewSeed();
            rematch.CompositionSeed = rematch.Seed;

            _runState.PendingCombat = rematch;
            _runState.RematchCombat = null;
            _runState.Outcome = RunOutcome.Active;
            _runState.PendingNode = RunNode.Combat;
            return _runState.PendingNode;
        }

        // ------------------------------------------------------------------
        // Nodos seguros
        // ------------------------------------------------------------------

        /// <summary>Se llama al resolver el level-up que esta al frente de la cola.</summary>
        public RunNode OnLevelUpComplete()
        {
            if (_runState.PendingLevelUps.Count > 0)
                _runState.PendingNode = RunNode.LevelUp;
            else
                ResolvePostCombatNode();

            return _runState.PendingNode;
        }

        public RunNode OnActTransitionComplete()
        {
            if (_runState.ActIndex == RunActTable.FinalAct)
                OpenFinalActStep();
            else
                _runState.PendingNode = RunNode.EnemySelect;

            return _runState.PendingNode;
        }

        /// <summary>Se llama al salir de una tienda o un evento.</summary>
        public RunNode OnShopEventComplete()
        {
            if (_runState.ActIndex == RunActTable.FinalAct)
            {
                _runState.FinalActStep++;
                OpenFinalActStep();
            }
            else
            {
                _runState.PendingNode = RunNode.EnemySelect;
            }

            return _runState.PendingNode;
        }

        public RunNode OnSubBossIntroComplete()
        {
            bool finalBoss = _runState.ActIndex == RunActTable.FinalAct &&
                             _runState.FinalActStep >= 3;

            var enemy = finalBoss
                ? _enemyRegistry?.GetFinalBoss()
                : _enemyRegistry?.GetSubBoss(_runState.ActIndex);

            if (enemy == null) return _runState.PendingNode;

            return CreatePendingCombat(enemy, NewSeed());
        }

        public RunNode OnEnemySelected(EnemyDef enemy)
        {
            if (enemy == null) return _runState.PendingNode;
            return CreatePendingCombat(enemy, NewSeed());
        }

        /// <summary>El draft completo entrega el control al primer rival.</summary>
        public RunNode OnDraftComplete()
        {
            _runState.PendingNode = RunNode.EnemySelect;
            return _runState.PendingNode;
        }

        // ------------------------------------------------------------------
        // Tirada de tienda/evento
        // ------------------------------------------------------------------

        /// <summary>
        /// Avanza la probabilidad y decide el nodo. En fallo devuelve
        /// EnemySelect y deja la probabilidad incrementada para la siguiente
        /// tirada; en exito devuelve Shop o Event y la reinicia a 25.
        /// </summary>
        public RunNode RollShopEvent()
        {
            int chance = _runState.ShopEventChance;

            if (chance >= 100 || _random.Next(100) < chance)
            {
                _runState.ShopEventChance = 25;
                return RollShopOrEvent();
            }

            _runState.ShopEventChance = Math.Min(100, chance + 15);
            return RunNode.EnemySelect;
        }

        // ------------------------------------------------------------------
        // Interno
        // ------------------------------------------------------------------

        private RunNode ResolvePostCombatNode()
        {
            if (_runState.Outcome != RunOutcome.Active)
            {
                _runState.PendingNode = RunNode.Summary;
                return _runState.PendingNode;
            }

            if (_runState.ActIndex == RunActTable.FinalAct)
            {
                // El jefe final ya se resolvio como victoria. Llegar aqui
                // significa que se gano el subjefe 4.
                _runState.FinalActStep = 3;
                _runState.PendingNode = RunNode.SubBossIntro;
                return _runState.PendingNode;
            }

            if (_runState.CombatIndexInAct >= RunActTable.CombatsPerAct)
                return MoveToNextAct();

            if (_runState.CombatIndexInAct == RunActTable.CombatsPerAct - 1)
            {
                _runState.PendingNode = RunNode.SubBossIntro;
                return _runState.PendingNode;
            }

            if (IsRollPosition(_runState.CombatIndexInAct))
            {
                _runState.PendingNode = RollShopEvent();
                return _runState.PendingNode;
            }

            _runState.PendingNode = RunNode.EnemySelect;
            return _runState.PendingNode;
        }

        private static bool IsRollPosition(int combatIndexInAct) =>
            combatIndexInAct == 3 ||
            combatIndexInAct == 6 ||
            combatIndexInAct == 9 ||
            combatIndexInAct == 12;

        private RunNode MoveToNextAct()
        {
            _runState.ShopEventChance = 25;
            _runState.CombatIndexInAct = 0;

            if (_runState.ActIndex < RunActTable.LastNormalAct)
            {
                _runState.ActIndex++;
            }
            else
            {
                _runState.ActIndex = RunActTable.FinalAct;
                _runState.FinalActStep = 0;
            }

            _runState.PendingNode = RunNode.ActTransition;
            return _runState.PendingNode;
        }

        /// <summary>Abre el paso actual del acto final (0..3).</summary>
        private RunNode OpenFinalActStep()
        {
            switch (_runState.FinalActStep)
            {
                case 0:
                    _runState.PendingNode = RollShopOrEvent();
                    break;
                case 1:
                    ResetNodeState(RunNode.Shop);
                    _runState.PendingNode = RunNode.Shop;
                    break;
                case 2:
                case 3:
                    _runState.PendingNode = RunNode.SubBossIntro;
                    break;
                default:
                    _runState.PendingNode = RunNode.Summary;
                    break;
            }

            return _runState.PendingNode;
        }

        private RunNode RollShopOrEvent()
        {
            var node = _random.Next(2) == 0 ? RunNode.Shop : RunNode.Event;
            ResetNodeState(node);
            return node;
        }

        /// <summary>
        /// Un nodo nuevo empieza con su estado limpio. La reentrada al mismo
        /// nodo conserva el stock o el evento ya guardados.
        /// </summary>
        private void ResetNodeState(RunNode node)
        {
            if (node == RunNode.Shop)
                _runState.ShopState = new ShopState();
            else if (node == RunNode.Event)
                _runState.EventState = new EventState();
        }

        private int NewSeed()
        {
            int seed = _random.Next(1, int.MaxValue);
            return seed == 0 ? 1 : seed;
        }

        private RunNode CreatePendingCombat(EnemyDef enemy, int seed)
        {
            _runState.PendingCombat = new PendingCombat
            {
                EnemyId = enemy.Id,
                EnemyName = enemy.Name,
                EnemyPhrase = enemy.Phrase,
                Difficulty = enemy.Difficulty,
                Attempt = 1,
                Seed = seed,
                CompositionSeed = seed,
                PreCombat = RunSnapshot.Capture(_runState)
            };

            _runState.PendingNode = RunNode.Combat;
            return _runState.PendingNode;
        }
    }
}
