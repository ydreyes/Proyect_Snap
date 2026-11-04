using System;
using System.Collections.Generic;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Board;

namespace SnapClone.Effects
{
    public class EffectContext
    {
        public CardInstance SourceCard { get; set; }
        public ZoneInstance SourceZone { get; set; }
        public BoardSide Owner { get; set; }
        public List<CardInstance> Targets { get; set; } = new List<CardInstance>();
        public BoardState BoardState { get; set; }
        public int TurnNumber { get; set; }
        public string PriorityOwner { get; set; }
        public CardRegistry CardRegistry { get; set; }
        public EffectRegistry EffectRegistry { get; set; }

        /// <summary>Evento que disparo el efecto, si no es OnReveal.</summary>
        public ActionEvent Event { get; set; }

        /// <summary>Permite a un efecto encolar otro evento durante una cascada.</summary>
        public Action<ActionEvent> EmitEvent { get; set; }

        /// <summary>Puerta de acciones para destruir, mover, copiar, fusionar, etc.</summary>
        public ICardEffectActions Actions { get; set; }

        public RandomService Random { get; set; }
    }

    public interface IEffect
    {
        string EffectId { get; }
        EffectTrigger Trigger { get; }
        void Apply(EffectContext context);
    }

    /// <summary>
    /// Contrato continuo para el poder de una carta. Se consulta en cada
    /// calculo y nunca escribe el poder actual, asi el resultado es idempotente.
    /// </summary>
    public interface ICardPowerModifier
    {
        int GetPowerBonus(EffectContext context);
    }

    /// <summary>
    /// Contrato continuo para el total de una zona. Varios multiplicadores
    /// de la misma zona se multiplican entre si.
    /// </summary>
    public interface IZonePowerMultiplier
    {
        int GetMultiplier(EffectContext context);
    }

    /// <summary>Aura que suma poder a otra carta.</summary>
    public interface ICardAura
    {
        int GetBonusForCard(EffectContext sourceContext, CardInstance target,
            ZoneInstance targetZone, BoardSide targetSide);
    }

    /// <summary>Efecto de zona que suma poder a una carta concreta.</summary>
    public interface IZoneCardPowerModifier
    {
        int GetBonusForCard(ZoneInstance zone, CardInstance card, BoardSide side);
    }

    /// <summary>Efecto que suma poder a una zona distinta de la de su fuente.</summary>
    public interface IZonePowerAddition
    {
        int GetAdditionForZone(EffectContext sourceContext, ZoneInstance targetZone,
            BoardSide targetSide);
    }

    /// <summary>Reemplazo de una destruccion antes de mover la carta a una pila.</summary>
    public interface ICardDestroyReplacement
    {
        bool TryReplaceDestroy(EffectContext context);
    }

    /// <summary>Reemplazo de un descarte antes de mover la carta a una pila.</summary>
    public interface ICardDiscardReplacement
    {
        bool TryReplaceDiscard(EffectContext context);
    }

    /// <summary>
    /// Operaciones que un efecto puede ejecutar sin mutar el tablero por su
    /// cuenta. La implementa CardActionService.
    /// </summary>
    public interface ICardEffectActions
    {
        ActionResult DestroyCard(CardInstance card, BoardSide side,
            ActionCause cause = ActionCause.Effect);
        ActionResult DiscardCard(CardInstance card, BoardSide side,
            ActionCause cause = ActionCause.Effect);
        ActionResult MoveCard(CardInstance card, BoardSide side,
            int fromZone, int toZone, ActionCause cause = ActionCause.Effect);
        ActionResult MoveCards(List<CardMoveRequest> moves,
            ActionCause cause = ActionCause.Effect);
        ActionResult AddToHand(CardInstance card, BoardSide side);
        ActionResult AddCardToZone(CardInstance card, BoardSide side, int zoneIndex);
        ActionResult RemoveFromHand(CardInstance card, BoardSide side);
        ActionResult RemoveFromContainers(CardInstance card, BoardSide side);
        CardInstance CreateCopy(CardInstance source, bool copyPower = true,
            bool copyPerks = false);
        ActionResult Transform(CardInstance card, CardDef newDefinition);
        ActionResult MergeInto(CardInstance host, CardInstance merged, BoardSide side);
        ActionResult ReplaceZone(int zoneIndex);
        ActionResult BuffTopCard(BoardSide side, int amount);
        ActionResult DrawFromEnemyDeck(BoardSide side);
        ActionResult ShuffleTokenIntoDeck(BoardSide side, CardDef tokenDefinition);
        ActionResult BanishLowestCost(BoardSide side, bool combatScoped);
        void RefreshPowers();
    }
}
