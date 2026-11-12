using System;
using System.Collections.Generic;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Meta
{
    /// <summary>
    /// Resuelve las habilidades efectivas de una carta: las impresas en su
    /// definicion mas las que otorgan sus perks adjuntos. Es la unica fuente
    /// que consultan el procesador, la compatibilidad y las pantallas.
    /// </summary>
    public static class PerkAbilities
    {
        /// <summary>
        /// Lista efectiva de ids de efecto. Las habilidades impresas no se
        /// repiten; cada copia de un perk agrega sus efectos una vez.
        /// </summary>
        public static List<string> EffectiveEffectIds(CardInstance card,
            bool includePrinted = true)
        {
            var result = new List<string>();
            if (card == null) return result;

            var suppressed = new HashSet<string>(
                card.SuppressedEffectIds ?? new List<string>(), StringComparer.Ordinal);

            var printed = new HashSet<string>(StringComparer.Ordinal);
            if (includePrinted && card.Definition?.EffectIds != null)
            {
                foreach (string effectId in card.Definition.EffectIds)
                {
                    if (string.IsNullOrWhiteSpace(effectId)) continue;
                    if (suppressed.Contains(effectId)) continue;
                    printed.Add(effectId);
                    if (!result.Contains(effectId))
                        result.Add(effectId);
                }
            }

            if (card.AttachedPerks != null)
            {
                foreach (string perkId in card.AttachedPerks)
                {
                    var effectIds = PerkCatalog.EffectIdsFor(perkId);
                    foreach (string effectId in effectIds)
                    {
                        if (string.IsNullOrWhiteSpace(effectId)) continue;
                        if (suppressed.Contains(effectId)) continue;

                        // Una habilidad impresa y un perk que comparten
                        // implementacion no se duplican en el mismo anfitrion.
                        if (printed.Contains(effectId)) continue;
                        result.Add(effectId);
                    }
                }
            }

            if (card.TemporaryEffectIds != null)
            {
                foreach (string effectId in card.TemporaryEffectIds)
                {
                    if (string.IsNullOrWhiteSpace(effectId)) continue;
                    if (suppressed.Contains(effectId)) continue;
                    result.Add(effectId);
                }
            }

            if (card.CopiedEffectIds != null)
            {
                foreach (string effectId in card.CopiedEffectIds)
                {
                    if (string.IsNullOrWhiteSpace(effectId)) continue;
                    if (suppressed.Contains(effectId)) continue;
                    if (!result.Contains(effectId))
                        result.Add(effectId);
                }
            }

            return result;
        }

        public static List<string> PerkOnlyEffectIds(CardInstance card)
        {
            var result = new List<string>();
            if (card?.AttachedPerks == null) return result;

            foreach (string perkId in card.AttachedPerks)
            {
                foreach (string effectId in PerkCatalog.EffectIdsFor(perkId))
                    result.Add(effectId);
            }

            return result;
        }

        public static bool HasEffect(CardInstance card, string effectId) =>
            EffectiveEffectIds(card).Contains(effectId);

        public static bool HasTrigger(CardInstance card, EffectTrigger trigger,
            EffectRegistry effectRegistry)
        {
            if (effectRegistry == null) return false;

            foreach (string effectId in EffectiveEffectIds(card))
            {
                var definition = effectRegistry.Get(effectId);
                if (definition != null && definition.Trigger == trigger)
                    return true;
            }

            return false;
        }

        /// <summary>Una carta puede hospedar el perk si cumple su requisito.</summary>
        public static bool CanAttach(string perkId, CardInstance card,
            EffectRegistry effectRegistry)
        {
            var perk = PerkCatalog.Get(perkId);
            if (perk == null || card?.Definition == null) return false;

            switch (perk.Compatibility)
            {
                case PerkCompatibility.RequiresOngoing:
                    return HasTrigger(card, EffectTrigger.Ongoing, effectRegistry);
                case PerkCompatibility.RequiresOnReveal:
                    return HasTrigger(card, EffectTrigger.OnReveal, effectRegistry);
                case PerkCompatibility.RequiresEndOfTurn:
                    return HasTrigger(card, EffectTrigger.EndOfTurn, effectRegistry);
                case PerkCompatibility.RequiresEmpower:
                case PerkCompatibility.Any:
                default:
                    return true;
            }
        }

        /// <summary>Razon legible de incompatibilidad; null si es compatible.</summary>
        public static string IncompatibleReason(string perkId, CardInstance card,
            EffectRegistry effectRegistry)
        {
            var perk = PerkCatalog.Get(perkId);
            if (perk == null) return "PERK DESCONOCIDO";
            if (card?.Definition == null) return "CARTA INVALIDA";

            if (CanAttach(perkId, card, effectRegistry)) return null;

            switch (perk.Compatibility)
            {
                case PerkCompatibility.RequiresOngoing:
                    return "NO TIENE EFECTO CONTINUO";
                case PerkCompatibility.RequiresOnReveal:
                    return "NO TIENE EFECTO AL REVELARSE";
                case PerkCompatibility.RequiresEndOfTurn:
                    return "NO TIENE EFECTO DE FIN DE TURNO";
                default:
                    return "NO PUEDE HOSPEDAR ESTE PERK";
            }
        }

        public static int CountPerkCopies(CardInstance card, string perkId) =>
            PerkCatalog.CountPerk(card?.AttachedPerks, perkId);
    }
}
