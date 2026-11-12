using System.Collections.Generic;

namespace SnapClone.Data.Definitions
{
    /// <summary>Rol del enemigo dentro de la estructura de actos.</summary>
    public enum EnemyBossTier
    {
        None,
        Act,
        Final
    }

    /// <summary>Cantidad fija de una carta dentro del mazo de un enemigo.</summary>
    public class EnemyDeckCard
    {
        public string CardId { get; set; }
        public int Count { get; set; } = 1;
    }

    /// <summary>
    /// Definicion de un encuentro rival: identidad, dificultad, acto y mazo
    /// fijo. El mazo se declara como ids y cantidades, no como instancias, para
    /// que cada intento construya cartas nuevas.
    /// </summary>
    public class EnemyDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phrase { get; set; }
        public int Difficulty { get; set; }
        public int ActIndex { get; set; }
        public bool IsBoss { get; set; }
        public EnemyBossTier BossTier { get; set; }
        public List<EnemyDeckCard> Deck { get; set; } = new List<EnemyDeckCard>();

        public int DeckSize
        {
            get
            {
                int total = 0;
                if (Deck == null) return total;

                foreach (var entry in Deck)
                {
                    if (entry != null)
                        total += entry.Count;
                }

                return total;
            }
        }
    }
}
