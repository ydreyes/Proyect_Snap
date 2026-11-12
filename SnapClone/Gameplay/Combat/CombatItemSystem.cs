using System;
using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Effects;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Persistence;

namespace SnapClone.Gameplay.Combat
{
    /// <summary>Resultado de comprobar si un objeto puede usarse ahora.</summary>
    public sealed class ItemUseCheck
    {
        public bool CanUse { get; }
        public string Reason { get; }
        public int CandidateCount { get; }

        private ItemUseCheck(bool canUse, string reason, int candidateCount)
        {
            CanUse = canUse;
            Reason = reason;
            CandidateCount = candidateCount;
        }

        public static ItemUseCheck Ok(int candidates) =>
            new ItemUseCheck(true, null, candidates);

        public static ItemUseCheck Fail(string reason) =>
            new ItemUseCheck(false, reason, 0);
    }

    /// <summary>Resultado de intentar consumir y resolver un objeto.</summary>
    public sealed class CombatItemUseResult
    {
        public bool Success { get; }
        public string Error { get; }
        public string ItemId { get; }
        public string EffectId { get; }
        public bool EffectApplied { get; }

        private CombatItemUseResult(bool success, string error, string itemId,
            string effectId, bool effectApplied)
        {
            Success = success;
            Error = error;
            ItemId = itemId;
            EffectId = effectId;
            EffectApplied = effectApplied;
        }

        public static CombatItemUseResult Ok(string itemId, string effectId) =>
            new CombatItemUseResult(true, null, itemId, effectId, true);

        public static CombatItemUseResult Fail(string error, string itemId = null,
            string effectId = null, bool effectApplied = false) =>
            new CombatItemUseResult(false, error, itemId, effectId, effectApplied);
    }

    /// <summary>
    /// Resuelve el uso de un objeto de combate como accion libre del turno.
    /// Valida fase, inventario y objetivos antes de tocar el tablero; consume
    /// el perfil solo despues de que el efecto tenga exito y escribe de
    /// inmediato. Si la escritura falla, restaura el contador y reporta el
    /// error sin deshacer el efecto ya aplicado.
    /// </summary>
    public class CombatItemSystem
    {
        private readonly BoardState _board;
        private readonly CardActionService _actions;
        private readonly EffectProcessor _effects;
        private readonly ItemRegistry _items;
        private readonly RandomService _random;

        public CombatItemSystem(BoardState boardState, CardActionService actions,
            EffectProcessor effectProcessor, ItemRegistry items,
            RandomService random)
        {
            _board = boardState;
            _actions = actions;
            _effects = effectProcessor;
            _items = items;
            _random = random ?? new RandomService(1);
        }

        /// <summary>True mientras se intenta persistir el perfil.</summary>
        public bool IsBusy { get; private set; }

        /// <summary>Resumen corto del efecto para la barra de objetos.</summary>
        public static string SummaryFor(string effectId)
        {
            switch (effectId)
            {
                case ItemEffectIds.EnergySurge: return "+2 ENERGIA";
                case ItemEffectIds.PowerBoost: return "+2 PODER";
                case ItemEffectIds.DestroyEnemy: return "DESTRUIR";
                case ItemEffectIds.DrawTwo: return "ROBAR 2";
                case ItemEffectIds.MoveEnemy: return "MOVER";
                case ItemEffectIds.AfflictZone: return "-2 ENEMIGOS";
                case ItemEffectIds.CopyHand: return "COPIAR";
                case ItemEffectIds.HealZone: return "+3 ALIADOS";
                default: return "OBJETO";
            }
        }

        public ItemUseCheck CheckUse(string itemId, ProfileDto profile,
            bool isPlayPhase)
        {
            var item = _items?.Get(itemId);
            if (item == null)
                return ItemUseCheck.Fail("Objeto desconocido.");

            if (profile == null)
                return ItemUseCheck.Fail("No hay perfil.");

            if (!isPlayPhase)
                return ItemUseCheck.Fail("Solo puedes usar objetos en tu fase de juego.");

            int count = CountOf(profile, itemId);
            if (count <= 0)
                return ItemUseCheck.Fail("No te quedan copias de ese objeto.");

            var candidates = BuildCandidates(item, out string reason);
            if (candidates == null)
                return ItemUseCheck.Fail(reason);

            return ItemUseCheck.Ok(candidates.Count);
        }

        public List<ItemDef> GetInventory(ProfileDto profile)
        {
            var result = new List<ItemDef>();
            if (profile == null || _items == null) return result;

            foreach (var item in _items.GetAll())
            {
                if (CountOf(profile, item.Id) > 0)
                    result.Add(item);
            }

            return result;
        }

        public int CountOf(ProfileDto profile, string itemId)
        {
            if (profile?.ItemCounts == null || string.IsNullOrWhiteSpace(itemId))
                return 0;

            return profile.ItemCounts.TryGetValue(itemId, out int count)
                ? Math.Max(0, count)
                : 0;
        }

        /// <summary>
        /// Usa un objeto. <paramref name="save"/> es la escritura inmediata del
        /// perfil; si devuelve false o lanza, el contador se restaura.
        /// </summary>
        public CombatItemUseResult TryUse(string itemId, ProfileDto profile,
            bool isPlayPhase, Func<ProfileDto, bool> save)
        {
            var check = CheckUse(itemId, profile, isPlayPhase);
            if (!check.CanUse)
                return CombatItemUseResult.Fail(check.Reason, itemId);

            var item = _items.Get(itemId);
            var candidates = BuildCandidates(item, out _);
            var target = candidates.Count == 0
                ? null
                : candidates[_random.Next(candidates.Count)];

            ActionResult applied;
            try
            {
                applied = Resolve(item, target);
            }
            catch (Exception ex)
            {
                return CombatItemUseResult.Fail(
                    "No se pudo resolver el objeto: " + ex.Message, itemId, item.EffectId);
            }

            if (applied == null || !applied.Success)
            {
                return CombatItemUseResult.Fail(
                    string.IsNullOrWhiteSpace(applied?.Reason)
                        ? "El objeto no encontro un objetivo valido."
                        : applied.Reason,
                    itemId, item.EffectId);
            }

            int previous = CountOf(profile, itemId);
            profile.ItemCounts[itemId] = Math.Max(0, previous - 1);

            IsBusy = true;
            try
            {
                if (save != null && !save(profile))
                {
                    profile.ItemCounts[itemId] = previous;
                    return CombatItemUseResult.Fail(
                        "No se pudo guardar el perfil. El objeto no se consumio.",
                        itemId, item.EffectId, true);
                }
            }
            catch (Exception ex)
            {
                profile.ItemCounts[itemId] = previous;
                return CombatItemUseResult.Fail(
                    "No se pudo guardar el perfil: " + ex.Message +
                    " El objeto no se consumio.",
                    itemId, item.EffectId, true);
            }
            finally
            {
                IsBusy = false;
            }

            return CombatItemUseResult.Ok(itemId, item.EffectId);
        }

        private ActionResult Resolve(ItemDef item, ItemTarget target)
        {
            switch (item.EffectId)
            {
                case ItemEffectIds.EnergySurge:
                    return _actions.AddEnergy(BoardSide.Player, 2);

                case ItemEffectIds.PowerBoost:
                    return _actions.AdjustCardPower(target.Card, BoardSide.Player, 2);

                case ItemEffectIds.DestroyEnemy:
                    return _actions.DestroyCard(target.Card, BoardSide.AI,
                        ActionCause.Effect);

                case ItemEffectIds.DrawTwo:
                    return DrawUpTo(2);

                case ItemEffectIds.MoveEnemy:
                    return _actions.MoveCard(target.Card, BoardSide.AI,
                        target.ZoneIndex, target.DestinationZoneIndex,
                        ActionCause.Effect);

                case ItemEffectIds.AfflictZone:
                    return _actions.AdjustZonePower(target.ZoneIndex, BoardSide.AI, -2);

                case ItemEffectIds.CopyHand:
                    var copy = _actions.CreateCopy(target.Card);
                    if (copy == null)
                        return ActionResult.Fail("No se pudo copiar la carta.");
                    return _actions.AddToHand(copy, BoardSide.Player);

                case ItemEffectIds.HealZone:
                    return _actions.AdjustZonePower(target.ZoneIndex, BoardSide.Player, 3);

                default:
                    return ActionResult.Fail("Efecto de objeto desconocido.");
            }
        }

        private ActionResult DrawUpTo(int count)
        {
            int drawn = 0;
            string reason = null;

            for (int i = 0; i < count; i++)
            {
                var result = _actions.DrawCard(BoardSide.Player);
                if (!result.Success)
                {
                    reason = result.Reason;
                    break;
                }

                drawn++;
            }

            return drawn > 0
                ? ActionResult.Ok()
                : ActionResult.Fail(reason ?? "No se pudo robar ninguna carta.");
        }

        /// <summary>
        /// Conjunto legal de objetivos. Devuelve null cuando no hay ninguno y
        /// deja en <paramref name="reason"/> la causa para la UI.
        /// </summary>
        private List<ItemTarget> BuildCandidates(ItemDef item, out string reason)
        {
            reason = null;
            var candidates = new List<ItemTarget>();

            switch (item.TargetRule)
            {
                case ItemTargetRule.None:
                    return candidates;

                case ItemTargetRule.AllyCard:
                    foreach (var zone in _board.Zones)
                    {
                        foreach (var card in zone.PlayerCards)
                            candidates.Add(new ItemTarget { Card = card, ZoneIndex = zone.ZoneIndex });
                    }

                    if (candidates.Count == 0)
                        reason = "No hay cartas aliadas en juego.";
                    return candidates.Count == 0 ? null : candidates;

                case ItemTargetRule.EnemyCard:
                    foreach (var zone in _board.Zones)
                    {
                        if (IsRemovalLocked(zone)) continue;

                        foreach (var card in zone.AICards)
                        {
                            if (CanDestroy(card))
                                candidates.Add(new ItemTarget
                                {
                                    Card = card,
                                    ZoneIndex = zone.ZoneIndex
                                });
                        }
                    }

                    if (candidates.Count == 0)
                        reason = "No hay una carta enemiga legal para destruir.";
                    return candidates.Count == 0 ? null : candidates;

                case ItemTargetRule.EnemyMove:
                    foreach (var origin in _board.Zones)
                    {
                        if (IsRemovalLocked(origin)) continue;

                        foreach (var card in origin.AICards)
                        {
                            foreach (var destination in _board.Zones)
                            {
                                if (destination.ZoneIndex == origin.ZoneIndex) continue;
                                if (!destination.CanAdd(BoardSide.AI)) continue;
                                if (IsPlayLocked(destination)) continue;

                                candidates.Add(new ItemTarget
                                {
                                    Card = card,
                                    ZoneIndex = origin.ZoneIndex,
                                    DestinationZoneIndex = destination.ZoneIndex
                                });
                            }
                        }
                    }

                    if (candidates.Count == 0)
                        reason = "No hay un movimiento enemigo legal.";
                    return candidates.Count == 0 ? null : candidates;

                case ItemTargetRule.AllyZone:
                    foreach (var zone in _board.Zones)
                    {
                        if (zone.PlayerCards.Count > 0)
                            candidates.Add(new ItemTarget { ZoneIndex = zone.ZoneIndex });
                    }

                    if (candidates.Count == 0)
                        reason = "No hay una zona aliada con cartas.";
                    return candidates.Count == 0 ? null : candidates;

                case ItemTargetRule.EnemyZone:
                    foreach (var zone in _board.Zones)
                    {
                        if (zone.AICards.Count > 0)
                            candidates.Add(new ItemTarget { ZoneIndex = zone.ZoneIndex });
                    }

                    if (candidates.Count == 0)
                        reason = "No hay una zona enemiga con cartas.";
                    return candidates.Count == 0 ? null : candidates;

                case ItemTargetRule.HandCard:
                    if (_board.PlayerHand.Count == 0)
                    {
                        reason = "No hay cartas en tu mano.";
                        return null;
                    }

                    if (_board.PlayerHand.Count >= BoardState.MaxHandSize)
                    {
                        reason = "La mano esta llena.";
                        return null;
                    }

                    foreach (var card in _board.PlayerHand)
                        candidates.Add(new ItemTarget { Card = card });

                    return candidates;

                case ItemTargetRule.Draw:
                    if (_board.PlayerDeck.Count == 0)
                    {
                        reason = "El mazo esta vacio.";
                        return null;
                    }

                    if (_board.PlayerHand.Count >= BoardState.MaxHandSize)
                    {
                        reason = "La mano esta llena.";
                        return null;
                    }

                    candidates.Add(new ItemTarget());
                    return candidates;

                default:
                    reason = "Regla de objetivo desconocida.";
                    return null;
            }
        }

        private bool CanDestroy(CardInstance card)
        {
            if (!_board.CanDestroy(card, BoardSide.AI))
                return false;

            if (_effects == null)
                return true;

            return !_effects.IsCardProtectedFromDestroy(card, BoardSide.AI);
        }

        private bool IsRemovalLocked(ZoneInstance zone) =>
            _effects != null && _effects.IsZoneRemovalLocked(zone);

        private bool IsPlayLocked(ZoneInstance zone) =>
            _effects != null && _effects.IsZonePlayLocked(zone);

        private sealed class ItemTarget
        {
            public CardInstance Card { get; set; }
            public int ZoneIndex { get; set; } = -1;
            public int DestinationZoneIndex { get; set; } = -1;
        }
    }
}
