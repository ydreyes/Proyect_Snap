using System;
using System.Collections.Generic;
using SnapClone.Data.Definitions;

namespace SnapClone.Data.Registries
{
    public class CardRegistry
    {
        private Dictionary<string, CardDef> _cards = new Dictionary<string, CardDef>();
        
        public void Register(CardDef card)
        {
            _cards[card.Id] = card;
        }
        
        public CardDef Get(string id)
        {
            return _cards.TryGetValue(id, out CardDef card) ? card : null;
        }
        
        public List<CardDef> GetAll()
        {
            return new List<CardDef>(_cards.Values);
        }

        /// <summary>Solo definiciones que pueden ofrecerse en draft y level-up.</summary>
        public List<CardDef> GetDraftable()
        {
            var draftable = new List<CardDef>();
            foreach (var card in _cards.Values)
            {
                if (!card.IsToken)
                    draftable.Add(card);
            }

            return draftable;
        }
    }
    
    public class ZoneRegistry
    {
        private Dictionary<string, ZoneDef> _zones = new Dictionary<string, ZoneDef>();
        
        public void Register(ZoneDef zone)
        {
            _zones[zone.Id] = zone;
        }
        
        public ZoneDef Get(string id)
        {
            return _zones.TryGetValue(id, out ZoneDef zone) ? zone : null;
        }
        
        public List<ZoneDef> GetAll()
        {
            return new List<ZoneDef>(_zones.Values);
        }
    }
    
    public class EffectRegistry
    {
        private Dictionary<string, EffectDef> _effects = new Dictionary<string, EffectDef>();
        
        public void Register(EffectDef effect)
        {
            _effects[effect.Id] = effect;
        }
        
        public EffectDef Get(string id)
        {
            return _effects.TryGetValue(id, out EffectDef effect) ? effect : null;
        }
        
        public List<EffectDef> GetAll()
        {
            return new List<EffectDef>(_effects.Values);
        }
    }
    
    /// <summary>
    /// Catalogo de los 8 objetos de combate. Conserva el orden de registro para
    /// que las compras aleatorias sean deterministas con una semilla.
    /// </summary>
    public class ItemRegistry
    {
        public const int ExpectedTotal = 8;

        private readonly Dictionary<string, ItemDef> _items =
            new Dictionary<string, ItemDef>();
        private readonly List<ItemDef> _order = new List<ItemDef>();

        public void Register(ItemDef item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id))
                return;

            _items[item.Id] = item;
            if (!_order.Contains(item))
                _order.Add(item);
        }

        public ItemDef Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _items.TryGetValue(id, out ItemDef item) ? item : null;
        }

        public List<ItemDef> GetAll() => new List<ItemDef>(_order);

        /// <summary>
        /// Falla la carga si el catalogo no tiene los 8 objetos, repite ids o
        /// declara un efecto sin implementacion.
        /// </summary>
        public void Validate()
        {
            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);

            foreach (var item in _order)
            {
                if (item == null)
                {
                    errors.Add("Objeto nulo en el catalogo.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                    errors.Add("Objeto sin id.");
                else
                {
                    if (!item.Id.StartsWith("item_", StringComparison.Ordinal))
                        errors.Add($"{item.Id}: el id debe empezar con 'item_'.");
                    if (!ids.Add(item.Id))
                        errors.Add($"{item.Id}: id de objeto duplicado.");
                }

                if (string.IsNullOrWhiteSpace(item.Name))
                    errors.Add($"{item.Id}: nombre vacio.");

                if (string.IsNullOrWhiteSpace(item.Description))
                    errors.Add($"{item.Id}: descripcion vacia.");

                if (!Enum.IsDefined(typeof(ItemRarity), item.Rarity))
                    errors.Add($"{item.Id}: rareza no soportada.");

                if (!Enum.IsDefined(typeof(ItemTargetRule), item.TargetRule))
                    errors.Add($"{item.Id}: regla de objetivo no soportada.");

                if (string.IsNullOrWhiteSpace(item.EffectId))
                    errors.Add($"{item.Id}: efecto vacio.");
                else if (!ItemEffectIds.IsKnown(item.EffectId))
                    errors.Add($"{item.Id}: efecto sin implementacion '{item.EffectId}'.");
            }

            if (_order.Count != ExpectedTotal)
                errors.Add($"El catalogo debe tener {ExpectedTotal} objetos, tiene {_order.Count}.");

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Contenido de objetos invalido:\n" + string.Join("\n", errors));
            }
        }
    }

    public class PerkRegistry
    {
        private Dictionary<string, PerkDef> _perks = new Dictionary<string, PerkDef>();
        
        public void Register(PerkDef perk)
        {
            _perks[perk.Id] = perk;
        }
        
        public PerkDef Get(string id)
        {
            return _perks.TryGetValue(id, out PerkDef perk) ? perk : null;
        }
        
        public List<PerkDef> GetAll()
        {
            return new List<PerkDef>(_perks.Values);
        }
    }

    /// <summary>Catalogo de los modificadores globales de la run.</summary>
    public class ModifierRegistry
    {
        private readonly Dictionary<string, RunModifierDef> _modifiers =
            new Dictionary<string, RunModifierDef>();
        private readonly List<RunModifierDef> _order = new List<RunModifierDef>();

        public void Register(RunModifierDef modifier)
        {
            if (modifier == null || string.IsNullOrWhiteSpace(modifier.Id))
                return;

            _modifiers[modifier.Id] = modifier;
            if (!_order.Contains(modifier))
                _order.Add(modifier);
        }

        public RunModifierDef Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _modifiers.TryGetValue(id, out RunModifierDef modifier) ? modifier : null;
        }

        public List<RunModifierDef> GetAll() => new List<RunModifierDef>(_order);
    }

    /// <summary>Catalogo de los 12 eventos de la run, en orden de registro.</summary>
    public class RunEventRegistry
    {
        public const int ExpectedTotal = 12;

        private readonly Dictionary<string, RunEventDef> _events =
            new Dictionary<string, RunEventDef>();
        private readonly List<RunEventDef> _order = new List<RunEventDef>();

        public void Register(RunEventDef runEvent)
        {
            if (runEvent == null || string.IsNullOrWhiteSpace(runEvent.Id))
                return;

            _events[runEvent.Id] = runEvent;
            if (!_order.Contains(runEvent))
                _order.Add(runEvent);
        }

        public RunEventDef Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _events.TryGetValue(id, out RunEventDef runEvent) ? runEvent : null;
        }

        public List<RunEventDef> GetAll() => new List<RunEventDef>(_order);

        /// <summary>
        /// Falla la carga de contenido si faltan eventos, hay ids duplicados o
        /// un accept usa una recompensa no soportada.
        /// </summary>
        public void Validate(ModifierRegistry modifierRegistry)
        {
            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);

            foreach (var runEvent in _order)
            {
                if (runEvent == null)
                {
                    errors.Add("Evento nulo en el catalogo.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(runEvent.Id))
                    errors.Add("Evento sin id.");
                else if (!ids.Add(runEvent.Id))
                    errors.Add($"{runEvent.Id}: id de evento duplicado.");

                if (string.IsNullOrWhiteSpace(runEvent.Title))
                    errors.Add($"{runEvent.Id}: titulo vacio.");

                if (string.IsNullOrWhiteSpace(runEvent.Text))
                    errors.Add($"{runEvent.Id}: texto vacio.");

                if (string.IsNullOrWhiteSpace(runEvent.AcceptText))
                    errors.Add($"{runEvent.Id}: texto de aceptar vacio.");

                if (runEvent.CostMoney < 0)
                    errors.Add($"{runEvent.Id}: coste de dinero negativo.");

                if (runEvent.XpAmount < 0 || runEvent.XpAmount > 30)
                    errors.Add($"{runEvent.Id}: XP fuera de 0..30.");

                if (!Enum.IsDefined(typeof(RunEventEffect), runEvent.EffectKind))
                    errors.Add($"{runEvent.Id}: recompensa no soportada.");

                if (runEvent.EffectKind == RunEventEffect.Modifier)
                {
                    if (string.IsNullOrWhiteSpace(runEvent.ModifierId))
                        errors.Add($"{runEvent.Id}: modificador sin id.");
                    else if (modifierRegistry != null &&
                             modifierRegistry.Get(runEvent.ModifierId) == null)
                        errors.Add($"{runEvent.Id}: modificador no registrado '{runEvent.ModifierId}'.");
                }
            }

            if (_order.Count != ExpectedTotal)
                errors.Add($"El catalogo debe tener {ExpectedTotal} eventos, tiene {_order.Count}.");

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Contenido de eventos invalido:\n" + string.Join("\n", errors));
            }
        }
    }

    /// <summary>
    /// Catalogo de enemigos. Conserva el orden de registro para que la
    /// seleccion y los pools por acto sean deterministas.
    /// </summary>
    public class EnemyRegistry
    {
        public const int ExpectedTotal = 23;
        public const int ExpectedNormals = 18;
        public const int ExpectedSubBosses = 4;
        public const int ExpectedFinalBosses = 1;
        public const int ExpectedNormalsPerAct = 6;

        private readonly Dictionary<string, EnemyDef> _enemies =
            new Dictionary<string, EnemyDef>();
        private readonly List<EnemyDef> _order = new List<EnemyDef>();

        public void Register(EnemyDef enemy)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.Id))
                return;

            _enemies[enemy.Id] = enemy;
            if (!_order.Contains(enemy))
                _order.Add(enemy);
        }

        public EnemyDef Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return _enemies.TryGetValue(id, out EnemyDef enemy) ? enemy : null;
        }

        public List<EnemyDef> GetAll() => new List<EnemyDef>(_order);

        /// <summary>Enemigos normales del acto indicado; nunca incluye jefes.</summary>
        public List<EnemyDef> GetByAct(int actIndex)
        {
            var result = new List<EnemyDef>();

            foreach (var enemy in _order)
            {
                if (enemy == null) continue;
                if (enemy.IsBoss || enemy.BossTier != EnemyBossTier.None) continue;
                if (enemy.ActIndex == actIndex)
                    result.Add(enemy);
            }

            return result;
        }

        /// <summary>Subjefe fijo del acto 1, 2, 3 o del subjefe 4 (acto 4).</summary>
        public EnemyDef GetSubBoss(int actIndex)
        {
            foreach (var enemy in _order)
            {
                if (enemy != null && enemy.BossTier == EnemyBossTier.Act &&
                    enemy.ActIndex == actIndex)
                    return enemy;
            }

            return null;
        }

        public EnemyDef GetFinalBoss()
        {
            foreach (var enemy in _order)
            {
                if (enemy != null && enemy.BossTier == EnemyBossTier.Final)
                    return enemy;
            }

            return null;
        }

        /// <summary>
        /// Falla la carga de contenido si el roster o un mazo no cumplen el
        /// contrato: 23 enemigos, 8 cartas por mazo, ids resolubles y sin
        /// tokens.
        /// </summary>
        public void Validate(CardRegistry cardRegistry)
        {
            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var cardNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (cardRegistry != null)
            {
                foreach (var card in cardRegistry.GetAll())
                {
                    if (!string.IsNullOrWhiteSpace(card?.Name))
                        cardNames.Add(card.Name);
                }
            }

            int normals = 0;
            int subBosses = 0;
            int finalBosses = 0;
            var normalsByAct = new Dictionary<int, int>();

            foreach (var enemy in _order)
            {
                if (enemy == null)
                {
                    errors.Add("Enemigo nulo en el roster.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(enemy.Id))
                    errors.Add("Enemigo sin id.");
                else
                {
                    if (!enemy.Id.StartsWith("enemy_", StringComparison.Ordinal))
                        errors.Add($"{enemy.Id}: el id debe empezar con 'enemy_'.");
                    if (!ids.Add(enemy.Id))
                        errors.Add($"{enemy.Id}: id de enemigo duplicado.");
                }

                if (string.IsNullOrWhiteSpace(enemy.Name))
                    errors.Add($"{enemy.Id}: nombre vacio.");
                else
                {
                    if (!names.Add(enemy.Name))
                        errors.Add($"{enemy.Id}: nombre de enemigo duplicado '{enemy.Name}'.");
                    if (cardNames.Contains(enemy.Name))
                        errors.Add($"{enemy.Id}: el nombre reutiliza una carta jugable '{enemy.Name}'.");
                }

                if (enemy.Difficulty < 1 || enemy.Difficulty > 5)
                    errors.Add($"{enemy.Id}: dificultad fuera de 1..5.");

                if (enemy.BossTier == EnemyBossTier.Act)
                {
                    subBosses++;
                    if (enemy.Difficulty < 3)
                        errors.Add($"{enemy.Id}: un subjefe debe tener dificultad >= 3.");
                }
                else if (enemy.BossTier == EnemyBossTier.Final)
                {
                    finalBosses++;
                    if (enemy.Difficulty != 5)
                        errors.Add($"{enemy.Id}: el jefe final debe tener dificultad 5.");
                }
                else
                {
                    normals++;
                    if (enemy.IsBoss)
                        errors.Add($"{enemy.Id}: IsBoss debe ser falso para un enemigo normal.");

                    if (!normalsByAct.TryGetValue(enemy.ActIndex, out int count))
                        count = 0;
                    normalsByAct[enemy.ActIndex] = count + 1;
                }

                ValidateDeck(enemy, cardRegistry, errors);
            }

            if (_order.Count != ExpectedTotal)
                errors.Add($"El roster debe tener {ExpectedTotal} enemigos, tiene {_order.Count}.");
            if (normals != ExpectedNormals)
                errors.Add($"El roster debe tener {ExpectedNormals} enemigos normales, tiene {normals}.");
            if (subBosses != ExpectedSubBosses)
                errors.Add($"El roster debe tener {ExpectedSubBosses} subjefes, tiene {subBosses}.");
            if (finalBosses != ExpectedFinalBosses)
                errors.Add($"El roster debe tener {ExpectedFinalBosses} jefe final, tiene {finalBosses}.");

            for (int act = 1; act <= 3; act++)
            {
                normalsByAct.TryGetValue(act, out int count);
                if (count != ExpectedNormalsPerAct)
                    errors.Add($"El acto {act} debe tener {ExpectedNormalsPerAct} enemigos normales, tiene {count}.");
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Contenido de enemigos invalido:\n" + string.Join("\n", errors));
            }
        }

        private static void ValidateDeck(EnemyDef enemy, CardRegistry cardRegistry,
            List<string> errors)
        {
            if (enemy.Deck == null || enemy.Deck.Count == 0)
            {
                errors.Add($"{enemy.Id}: mazo vacio.");
                return;
            }

            int total = 0;
            var copies = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var entry in enemy.Deck)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.CardId))
                {
                    errors.Add($"{enemy.Id}: entrada de mazo sin card id.");
                    continue;
                }

                if (entry.Count < 1)
                {
                    errors.Add($"{enemy.Id}: cantidad invalida para '{entry.CardId}'.");
                    continue;
                }

                total += entry.Count;

                if (!copies.TryGetValue(entry.CardId, out int count))
                    count = 0;
                copies[entry.CardId] = count + entry.Count;

                var card = cardRegistry?.Get(entry.CardId);
                if (card == null)
                {
                    errors.Add($"{enemy.Id}: card id no registrado '{entry.CardId}'.");
                    continue;
                }

                if (card.IsToken)
                    errors.Add($"{enemy.Id}: el mazo incluye el token '{entry.CardId}'.");
            }

            if (total != 8)
                errors.Add($"{enemy.Id}: el mazo debe tener 8 cartas, tiene {total}.");

            foreach (var pair in copies)
            {
                if (pair.Value > 2)
                    errors.Add($"{enemy.Id}: '{pair.Key}' aparece {pair.Value} veces; el maximo es 2.");
            }
        }
    }
}
