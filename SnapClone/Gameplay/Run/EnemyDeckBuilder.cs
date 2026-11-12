using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;

namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Construye el mazo rival a partir de la definicion fija del enemigo.
    /// La vista previa y el combate usan la misma expansion; el combate agrega
    /// el barajado con la semilla del encuentro.
    /// </summary>
    public static class EnemyDeckBuilder
    {
        public const int DeckSize = 8;

        public static List<CardDef> BuildDefs(CardRegistry registry, EnemyDef enemy)
        {
            var result = new List<CardDef>();
            if (registry == null || enemy?.Deck == null) return result;

            foreach (var entry in enemy.Deck)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.CardId))
                    continue;

                var def = registry.Get(entry.CardId);
                if (def == null) continue;

                for (int i = 0; i < entry.Count; i++)
                    result.Add(def);
            }

            return result;
        }

        /// <summary>
        /// Expansion declarada mas barajado determinista. La misma definicion
        /// y la misma semilla producen el mismo mazo inicial.
        /// </summary>
        public static List<CardDef> BuildShuffledDefs(CardRegistry registry,
            EnemyDef enemy, int seed)
        {
            var defs = BuildDefs(registry, enemy);
            Shuffle(defs, new RandomService(seed == 0 ? 1 : seed));
            return defs;
        }

        public static void Shuffle<T>(List<T> items, RandomService random)
        {
            if (items == null || random == null) return;

            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T tmp = items[i];
                items[i] = items[j];
                items[j] = tmp;
            }
        }
    }

    /// <summary>
    /// Tirada de los dos candidatos normales del acto. Nunca repite un id
    /// dentro de la misma oferta.
    /// </summary>
    public static class EnemySelection
    {
        public static List<EnemyDef> RollTwo(EnemyRegistry registry,
            RandomService random, int actIndex)
        {
            var result = new List<EnemyDef>();
            if (registry == null || random == null) return result;

            var pool = new List<EnemyDef>(registry.GetByAct(actIndex));

            for (int i = 0; i < 2 && pool.Count > 0; i++)
            {
                int index = random.Next(pool.Count);
                result.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return result;
        }
    }
}
