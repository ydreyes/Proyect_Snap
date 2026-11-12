using System;
using System.Collections.Generic;

namespace SnapClone.Data.Definitions
{
    /// <summary>
    /// Catalogo de los 28 perks de habilidad. Cada perk otorga una o varias
    /// habilidades a su carta anfitriona; ninguna regla se aplica al lado
    /// completo. Los ids son estables y sobreviven a la migracion de saves.
    /// </summary>
    public static class PerkCatalog
    {
        public const string Afterimage = "perk_afterimage";
        public const string Amplify = "perk_amplify";
        public const string Apex = "perk_apex";
        public const string Clone = "perk_clone";
        public const string Deadlink = "perk_deadlink";
        public const string Encore = "perk_encore";
        public const string HotPursuit = "perk_hot_pursuit";
        public const string Maneuverable = "perk_maneuverable";
        public const string Mentor = "perk_mentor";
        public const string Overcharge = "perk_overcharge";
        public const string Overload = "perk_overload";
        public const string Overtime = "perk_overtime";
        public const string QuickdrawDiscount = "perk_quickdraw_discount";
        public const string QuickdrawPower = "perk_quickdraw_power";
        public const string Rally = "perk_rally";
        public const string RealityWarp = "perk_reality_warp";
        public const string Regeneration = "perk_regeneration";
        public const string Regroup = "perk_regroup";
        public const string Reinforce = "perk_reinforce";
        public const string Salvage = "perk_salvage";
        public const string Sidekick = "perk_sidekick";
        public const string Silence = "perk_silence";
        public const string Slingshot = "perk_slingshot";
        public const string Superstar = "perk_superstar";
        public const string TagIn = "perk_tag_in";
        public const string Taunt = "perk_taunt";
        public const string Unstoppable = "perk_unstoppable";
        public const string WakandanTech = "perk_wakandan_tech";

        public const string EffectAfterimage = "perk_effect_afterimage";
        public const string EffectAmplify = "perk_effect_amplify";
        public const string EffectApex = "perk_effect_apex";
        public const string EffectClone = "perk_effect_clone";
        public const string EffectDeadlink = "perk_effect_deadlink";
        public const string EffectEncore = "perk_effect_encore";
        public const string EffectHotPursuit = "perk_effect_hot_pursuit";
        public const string EffectManeuverable = "perk_effect_maneuverable";
        public const string EffectMentor = "perk_effect_mentor";
        public const string EffectOvercharge = "perk_effect_overcharge";
        public const string EffectOverload = "perk_effect_overload";
        public const string EffectOvertime = "perk_effect_overtime";
        public const string EffectQuickdrawDiscount = "perk_effect_quickdraw_discount";
        public const string EffectQuickdrawPower = "perk_effect_quickdraw_power";
        public const string EffectQuickdrawPowerGrant = "effect_quickdraw_power_grant";
        public const string EffectRally = "perk_effect_rally";
        public const string EffectRegroup = "perk_effect_regroup";
        public const string EffectReinforce = "perk_effect_reinforce";
        public const string EffectSalvage = "perk_effect_salvage";
        public const string EffectSidekick = "perk_effect_sidekick";
        public const string EffectSilence = "perk_effect_silence";
        public const string EffectSlingshot = "perk_effect_slingshot";
        public const string EffectSuperstar = "perk_effect_superstar";
        public const string EffectTaunt = "perk_effect_taunt";
        public const string EffectUnstoppable = "perk_effect_unstoppable";
        public const string EffectWakandanTech = "perk_effect_wakandan_tech";

        /// <summary>Token no draftable que Deadlink potencia.</summary>
        public const string ZombieHordeCardId = "token_zombie_horde";

        /// <summary>Retirados: la carga falla si alguno vuelve al catalogo.</summary>
        public static readonly string[] RetiredIds =
        {
            "perk_vanguardia",
            "perk_aura",
            "perk_francotirador",
            "perk_letal",
            "perk_agil",
            "perk_guardian",
            "perk_imparable",
            "perk_codicioso"
        };

        public static readonly string[] AllIds =
        {
            Afterimage,
            Amplify,
            Apex,
            Clone,
            Deadlink,
            Encore,
            HotPursuit,
            Maneuverable,
            Mentor,
            Overcharge,
            Overload,
            Overtime,
            QuickdrawDiscount,
            QuickdrawPower,
            Rally,
            RealityWarp,
            Regeneration,
            Regroup,
            Reinforce,
            Salvage,
            Sidekick,
            Silence,
            Slingshot,
            Superstar,
            TagIn,
            Taunt,
            Unstoppable,
            WakandanTech
        };

        private static readonly Dictionary<string, PerkDef> ById = BuildDefinitions();

        public static IReadOnlyList<string> All => AllIds;

        public static IReadOnlyList<PerkDef> Definitions =>
            new List<PerkDef>(ById.Values);

        public static PerkDef Get(string perkId)
        {
            if (string.IsNullOrWhiteSpace(perkId)) return null;
            return ById.TryGetValue(perkId, out PerkDef perk) ? perk : null;
        }

        public static bool IsKnown(string perkId) => Get(perkId) != null;

        public static bool IsRetired(string perkId) =>
            Array.IndexOf(RetiredIds, perkId) >= 0;

        /// <summary>Ids de efecto que otorga una copia del perk.</summary>
        public static IReadOnlyList<string> EffectIdsFor(string perkId)
        {
            var perk = Get(perkId);
            return perk?.EffectIds ?? (IReadOnlyList<string>)Array.Empty<string>();
        }

        /// <summary>True si la carta lleva al menos una copia del perk.</summary>
        public static bool HasPerk(IEnumerable<string> attachedPerks, string perkId)
        {
            if (attachedPerks == null) return false;

            foreach (string attached in attachedPerks)
            {
                if (string.Equals(attached, perkId, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        public static int CountPerk(IEnumerable<string> attachedPerks, string perkId)
        {
            if (attachedPerks == null) return 0;

            int count = 0;
            foreach (string attached in attachedPerks)
            {
                if (string.Equals(attached, perkId, StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        private static Dictionary<string, PerkDef> BuildDefinitions()
        {
            var definitions = new Dictionary<string, PerkDef>(StringComparer.Ordinal);

            Add(definitions, Afterimage, "Afterimage",
                "Cuando esta carta se mueve, anade una copia sin mejoras a la locacion anterior.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectAfterimage);

            Add(definitions, Amplify, "Amplify",
                "El efecto Continuo de esta carta se aplica una vez mas.",
                PerkKind.MetaAbility, PerkCompatibility.RequiresOngoing, EffectAmplify);

            Add(definitions, Apex, "Apex",
                "Continuo: si es tu unica carta aqui, duplica su Poder.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectApex);

            Add(definitions, Clone, "Clone",
                "Al inicio del combate: baraja una copia sin mejoras de esta carta en tu mazo.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectClone);

            Add(definitions, Deadlink, "Deadlink",
                "Al revelarse: tu Horda Zombie gana +4 de Poder.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectDeadlink);

            Add(definitions, Encore, "Encore",
                "La habilidad Al revelarse de esta carta se repite.",
                PerkKind.MetaAbility, PerkCompatibility.RequiresOnReveal, EffectEncore);

            Add(definitions, HotPursuit, "Hot Pursuit",
                "Cuesta 3 menos si una carta se movio el turno anterior.",
                PerkKind.RuleKeyword, PerkCompatibility.Any, EffectHotPursuit);

            Add(definitions, Maneuverable, "Maneuverable",
                "Se puede mover una vez por combate.",
                PerkKind.RuleKeyword, PerkCompatibility.Any, EffectManeuverable);

            Add(definitions, Mentor, "Mentor",
                "Al revelarse: la carta superior de tu mazo gana Poder igual al de esta carta.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectMentor);

            Add(definitions, Overcharge, "Overcharge",
                "Al revelarse: si ganas aqui el siguiente turno, +1 de Energia.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectOvercharge);

            Add(definitions, Overload, "Overload",
                "Inicio del turno: +1 de Energia si ganas esta locacion.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectOverload);

            Add(definitions, Overtime, "Overtime",
                "La habilidad de Fin de turno de esta carta se repite.",
                PerkKind.MetaAbility, PerkCompatibility.RequiresEndOfTurn, EffectOvertime);

            Add(definitions, QuickdrawDiscount, "Quickdraw: Discount",
                "Al robarla: cuesta 2 menos el resto del combate.",
                PerkKind.RuleKeyword, PerkCompatibility.Any, EffectQuickdrawDiscount);

            Add(definitions, QuickdrawPower, "Quickdraw: Power",
                "Al robarla: gana Al revelarse: +3 de Poder el resto del combate.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectQuickdrawPower);

            Add(definitions, Rally, "Rally",
                "Continuo: +1 de Poder por cada otra carta tuya aqui.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectRally);

            Add(definitions, RealityWarp, "Reality Warp",
                "Al revelarse: reemplaza esta locacion por una nueva.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, "effect_scarlet_witch");

            Add(definitions, Regeneration, "Regeneration",
                "Al ser descartada o destruida: regenera con +2 de Poder en una locacion con espacio.",
                PerkKind.GrantedEffect, PerkCompatibility.Any,
                "effect_wolverine_discard", "effect_wolverine_destroy");

            Add(definitions, Regroup, "Regroup",
                "Al revelarse: devuelve a tu mano la carta de menor coste aqui y dale +3 de Poder.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectRegroup);

            Add(definitions, Reinforce, "Reinforce",
                "Cuando esta carta se potencia: gana +1 de Poder adicional.",
                PerkKind.MetaAbility, PerkCompatibility.RequiresEmpower, EffectReinforce);

            Add(definitions, Salvage, "Salvage",
                "Al revelarse: destruye tu otra carta de menor Poder aqui para ganar +5 de Poder.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectSalvage);

            Add(definitions, Sidekick, "Sidekick",
                "Al revelarse: anade un personaje aleatorio a tu mano y dale -2 de coste.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectSidekick);

            Add(definitions, Silence, "Silence",
                "Quita el texto impreso de esta carta. Sus perks siguen activos.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectSilence);

            Add(definitions, Slingshot, "Slingshot",
                "Al revelarse: da +1 de Poder a otra carta tuya aqui y muevela a la derecha.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectSlingshot);

            Add(definitions, Superstar, "Superstar",
                "Empieza en tu mano inicial.",
                PerkKind.RuleKeyword, PerkCompatibility.Any, EffectSuperstar);

            Add(definitions, TagIn, "Tag In",
                "Cuando destruyen una de tus cartas, esta la reemplaza desde tu mano o mazo.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, "effect_angel");

            Add(definitions, Taunt, "Taunt",
                "Al revelarse: el rival debe jugar su primera carta aqui el siguiente turno.",
                PerkKind.RuleKeyword, PerkCompatibility.Any, EffectTaunt);

            Add(definitions, Unstoppable, "Unstoppable",
                "Se puede jugar en cualquier locacion. No puede ser destruida ni perder Poder.",
                PerkKind.RuleKeyword, PerkCompatibility.Any, EffectUnstoppable);

            Add(definitions, WakandanTech, "Wakandan Tech",
                "Al revelarse: duplica el Poder de esta carta.",
                PerkKind.GrantedEffect, PerkCompatibility.Any, EffectWakandanTech);

            return definitions;
        }

        private static void Add(Dictionary<string, PerkDef> definitions, string id,
            string name, string ruleText, PerkKind kind,
            PerkCompatibility compatibility, params string[] effectIds)
        {
            definitions[id] = new PerkDef
            {
                Id = id,
                Name = name,
                RuleText = ruleText,
                Kind = kind,
                Compatibility = compatibility,
                Stackable = true,
                EffectIds = new List<string>(effectIds ?? Array.Empty<string>())
            };
        }
    }
}
