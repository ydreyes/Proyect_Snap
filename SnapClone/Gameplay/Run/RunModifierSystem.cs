using System;
using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;

namespace SnapClone.Gameplay.Run
{
    /// <summary>
    /// Catalogo y reglas de los modificadores globales de la run. Cada id se
    /// guarda como maximo una vez y solo cambia los puntos documentados de
    /// energia, mano inicial, tienda y recompensas.
    /// </summary>
    public class RunModifierSystem
    {
        public const string EnergyBonus = "energy_bonus";
        public const string EnergyCap = "energy_cap";
        public const string StartingHand = "starting_hand";
        public const string ShopDiscount = "shop_discount";
        public const string MoneyBonus = "money_bonus";
        public const string XpBonus = "xp_bonus";

        public const int BaseEnergyCap = 6;
        public const int BaseEnergyBonus = 1;
        public const int BaseStartingHandBonus = 1;
        public const int BaseShopDiscountPercent = 20;
        public const int BaseMoneyBonus = 3;
        public const int BaseXpBonus = 10;

        public static IReadOnlyList<RunModifierDef> Catalog { get; } =
            new List<RunModifierDef>
            {
                new RunModifierDef
                {
                    Id = EnergyBonus,
                    Name = "Energia extra",
                    Description = "El pool de energia de cada turno gana 1."
                },
                new RunModifierDef
                {
                    Id = EnergyCap,
                    Name = "Tope de energia",
                    Description = "El tope de energia sube de 6 a 7."
                },
                new RunModifierDef
                {
                    Id = StartingHand,
                    Name = "Mano inicial",
                    Description = "La mano inicial gana 1 carta."
                },
                new RunModifierDef
                {
                    Id = ShopDiscount,
                    Name = "Descuento de tienda",
                    Description = "Los precios de la tienda bajan un 20%."
                },
                new RunModifierDef
                {
                    Id = MoneyBonus,
                    Name = "Botin extra",
                    Description = "Cada combate ganado da 3 dinero extra."
                },
                new RunModifierDef
                {
                    Id = XpBonus,
                    Name = "Experiencia extra",
                    Description = "Cada combate ganado da 10 XP extra."
                }
            }.AsReadOnly();

        private readonly ModifierRegistry _registry;

        public RunModifierSystem(ModifierRegistry registry = null)
        {
            _registry = registry;
        }

        public List<RunModifierDef> GetAll()
        {
            if (_registry != null)
                return _registry.GetAll();

            return new List<RunModifierDef>(Catalog);
        }

        public RunModifierDef Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            if (_registry != null)
                return _registry.Get(id);

            foreach (var modifier in Catalog)
            {
                if (string.Equals(modifier.Id, id, StringComparison.Ordinal))
                    return modifier;
            }

            return null;
        }

        public string NameOf(string id)
        {
            var modifier = Get(id);
            return modifier != null ? modifier.Name : id;
        }

        public bool IsKnown(string id) => Get(id) != null;

        /// <summary>Nombres de los modificadores activos, en orden de catalogo.</summary>
        public IReadOnlyList<string> ActiveNames(RunState runState)
        {
            var names = new List<string>();
            if (runState?.RunModifiers == null) return names;

            foreach (var modifier in GetAll())
            {
                if (runState.RunModifiers.Contains(modifier.Id))
                    names.Add(modifier.Name);
            }

            return names;
        }

        /// <summary>
        /// Intenta otorgar un modificador. Un id desconocido o repetido se
        /// rechaza sin tocar el estado de la run.
        /// </summary>
        public bool TryAdd(RunState runState, string modifierId)
        {
            if (runState == null || !IsKnown(modifierId)) return false;

            runState.RunModifiers ??= new HashSet<string>();
            return runState.RunModifiers.Add(modifierId);
        }

        public bool Has(RunState runState, string modifierId) =>
            HasModifier(runState, modifierId);

        public static bool HasModifier(RunState runState, string modifierId)
        {
            if (runState?.RunModifiers == null || string.IsNullOrWhiteSpace(modifierId))
                return false;

            return runState.RunModifiers.Contains(modifierId);
        }

        /// <summary>Activa el efecto de un id solo si la run lo tiene.</summary>
        public static bool IsActive(RunState runState, string modifierId) =>
            HasModifier(runState, modifierId);

        public static int EnergyCapFor(RunState runState) =>
            BaseEnergyCap + (IsActive(runState, EnergyCap) ? 1 : 0);

        public static int EnergyPoolFor(RunState runState, int turn)
        {
            int cap = EnergyCapFor(runState);
            int pool = Math.Min(turn, cap);
            return pool + (IsActive(runState, EnergyBonus) ? BaseEnergyBonus : 0);
        }

        public static int StartingHandSizeFor(RunState runState, int baseSize) =>
            baseSize + (IsActive(runState, StartingHand) ? BaseStartingHandBonus : 0);

        public static int ShopPriceFor(RunState runState, int basePrice)
        {
            if (basePrice <= 0) return 0;
            if (!IsActive(runState, ShopDiscount)) return basePrice;

            return (int)Math.Floor(basePrice * (100 - BaseShopDiscountPercent) / 100f);
        }

        public static int MoneyBonusFor(RunState runState) =>
            IsActive(runState, MoneyBonus) ? BaseMoneyBonus : 0;

        public static int XpBonusFor(RunState runState) =>
            IsActive(runState, XpBonus) ? BaseXpBonus : 0;

        /// <summary>Lista compacta para las pantallas de la run.</summary>
        public static string Describe(RunState runState)
        {
            var names = new List<string>();
            foreach (var modifier in Catalog)
            {
                if (HasModifier(runState, modifier.Id))
                    names.Add(modifier.Name);
            }

            return names.Count == 0
                ? "SIN MODIFICADORES"
                : "MODS: " + string.Join(", ", names);
        }
    }
}
