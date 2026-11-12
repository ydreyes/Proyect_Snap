using System;
using System.Collections.Generic;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Run
{
    public enum RunOutcome
    {
        Active,
        Won,
        Lost
    }

    /// <summary>Estado persistido de la run: se puede continuar o ya termino.</summary>
    public enum RunStatus
    {
        Active,
        Ended
    }

    public class RunState
    {
        public const int MaxCombats = RunActTable.TotalCombats;
        public const int MaxDeckSize = 12;
        public const int MinDeckSize = 6;

        /// <summary>Acto actual: 1, 2, 3 o 4 para el acto final.</summary>
        public int ActIndex { get; set; } = 1;

        /// <summary>Indice 0..15 dentro del acto. El acto final no lo usa.</summary>
        public int CombatIndexInAct { get; set; } = 0;

        /// <summary>Identidad estable de la run; la usa el award idempotente.</summary>
        public string RunId { get; set; } = string.Empty;

        /// <summary>Combates ganados en toda la run, 0..47.</summary>
        public int TotalCombats { get; set; } = 0;

        /// <summary>Combates normales ganados; alimenta la formula de puntos.</summary>
        public int NormalCombatsWon { get; set; } = 0;

        /// <summary>Subjefes ganados (15/30/45/46); alimenta la formula.</summary>
        public int SubBossesWon { get; set; } = 0;

        /// <summary>El jefe final fue derrotado.</summary>
        public bool FinalBossDefeated { get; set; } = false;

        /// <summary>Probabilidad de tienda/evento en el proximo punto de tirada (25..100).</summary>
        public int ShopEventChance { get; set; } = 25;

        /// <summary>Paso del acto final: 0 tirada, 1 tienda, 2 subjefe 4, 3 jefe final.</summary>
        public int FinalActStep { get; set; } = 0;

        /// <summary>Nodo que el director abrira a continuacion.</summary>
        public RunNode PendingNode { get; set; } = RunNode.Draft;

        /// <summary>Instantanea del combate en curso; null fuera de combate.</summary>
        public PendingCombat PendingCombat { get; set; } = null;

        /// <summary>
        /// Identidad del ultimo combate perdido, disponible para reintentar
        /// desde el resumen. Se limpia al iniciar una run o al ganar.
        /// </summary>
        public PendingCombat RematchCombat { get; set; } = null;

        /// <summary>Descubrimientos acumulados en la run, pendientes de volcar al perfil.</summary>
        public HashSet<string> DiscoveredCardIds { get; set; } = new HashSet<string>();
        public HashSet<string> DiscoveredPerkIds { get; set; } = new HashSet<string>();
        public HashSet<string> DiscoveredEnemyIds { get; set; } = new HashSet<string>();

        public int Xp { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int Money { get; set; } = 0;
        public int XpEarned { get; set; } = 0;
        public int MoneyEarned { get; set; } = 0;
        public int CombatsWon { get; set; } = 0;
        public RunOutcome Outcome { get; set; } = RunOutcome.Active;

        public List<CardInstance> Deck { get; set; } = new List<CardInstance>();

        /// <summary>Modificadores globales de la run; como maximo uno de cada id.</summary>
        public HashSet<string> RunModifiers { get; set; } = new HashSet<string>();

        /// <summary>Cargas de vida extra disponibles (0 o 1).</summary>
        public int ExtraLifeCharges { get; set; }

        /// <summary>Stock generado al abrir la tienda actual.</summary>
        public ShopState ShopState { get; set; } = new ShopState();

        /// <summary>Evento elegido para el nodo actual y su estado de resolucion.</summary>
        public EventState EventState { get; set; } = new EventState();

        /// <summary>Eventos ya mostrados en esta run, sin repetir hasta agotar el pool.</summary>
        public HashSet<string> SeenEventIds { get; set; } = new HashSet<string>();

        /// <summary>
        /// Niveles ganados que aun no se han resuelto. Se atienden en orden y
        /// solo el primero esta activo.
        /// </summary>
        public List<LevelUpState> PendingLevelUps { get; set; } = new List<LevelUpState>();

        public bool IsDeckAtCap => Deck.Count >= MaxDeckSize;

        /// <summary>Garantiza que una run cargada de un guardado viejo tenga id.</summary>
        public string EnsureRunId()
        {
            if (string.IsNullOrWhiteSpace(RunId))
                RunId = Guid.NewGuid().ToString("N");

            return RunId;
        }

        /// <summary>Experiencia necesaria para pasar del nivel indicado al siguiente.</summary>
        public static int XpToNext(int level) => 100 + 50 * (level - 1);

        public void StartNewRun()
        {
            Deck.Clear();
            PendingLevelUps.Clear();
            RunId = Guid.NewGuid().ToString("N");
            ActIndex = 1;
            CombatIndexInAct = 0;
            TotalCombats = 0;
            NormalCombatsWon = 0;
            SubBossesWon = 0;
            FinalBossDefeated = false;
            ShopEventChance = 25;
            FinalActStep = 0;
            PendingNode = RunNode.Draft;
            PendingCombat = null;
            RematchCombat = null;
            DiscoveredCardIds.Clear();
            DiscoveredPerkIds.Clear();
            DiscoveredEnemyIds.Clear();
            Xp = 0;
            Level = 1;
            Money = 0;
            XpEarned = 0;
            MoneyEarned = 0;
            CombatsWon = 0;
            Outcome = RunOutcome.Active;
            RunModifiers.Clear();
            ExtraLifeCharges = 0;
            ShopState = new ShopState();
            EventState = new EventState();
            SeenEventIds.Clear();
        }

        /// <summary>
        /// Suma experiencia y encola un level-up por cada umbral cruzado. El
        /// sobrante se conserva en el nivel actual.
        /// </summary>
        public bool AddExperience(int amount)
        {
            if (amount <= 0) return false;

            XpEarned += amount;
            Xp += amount;

            bool leveled = false;
            while (Xp >= XpToNext(Level))
            {
                Xp -= XpToNext(Level);
                Level++;
                PendingLevelUps.Add(new LevelUpState(Level));
                leveled = true;
            }

            return leveled;
        }

        public void AddMoney(int amount)
        {
            if (amount <= 0) return;

            Money += amount;
            MoneyEarned += amount;
        }

        public void DiscoverCard(string cardId)
        {
            if (!string.IsNullOrWhiteSpace(cardId))
                DiscoveredCardIds.Add(cardId);
        }

        public void DiscoverPerk(string perkId)
        {
            if (!string.IsNullOrWhiteSpace(perkId))
                DiscoveredPerkIds.Add(perkId);
        }

        public void DiscoverEnemy(string enemyId)
        {
            if (!string.IsNullOrWhiteSpace(enemyId))
                DiscoveredEnemyIds.Add(enemyId);
        }
    }
}
