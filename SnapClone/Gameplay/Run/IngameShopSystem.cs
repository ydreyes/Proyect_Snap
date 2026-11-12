using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;

namespace SnapClone.Gameplay.Run
{
    public readonly struct ShopPurchaseResult
    {
        public bool Success { get; }
        public string Error { get; }

        private ShopPurchaseResult(bool success, string error)
        {
            Success = success;
            Error = error;
        }

        public static ShopPurchaseResult Ok() => new ShopPurchaseResult(true, null);

        public static ShopPurchaseResult Fail(string error) =>
            new ShopPurchaseResult(false, error);
    }

    /// <summary>
    /// Tienda dentro de la run. Genera el stock una sola vez por nodo y
    /// aplica compras sin tocar <see cref="RunState.MoneyEarned"/>.
    /// </summary>
    public class IngameShopSystem
    {
        public const int StockSize = 4;
        public const int UpgradePrice = 70;
        public const int RemovalPrice = 55;
        public const int ExtraLifePrice = 220;
        public const int MaxCardCopies = 2;

        private readonly CardRegistry _cardRegistry;
        private readonly RandomService _random;
        private readonly RunModifierSystem _modifiers;
        private readonly PowerResolver _powerResolver;

        public IngameShopSystem(CardRegistry cardRegistry, RandomService random,
            RunModifierSystem modifiers = null, PowerResolver powerResolver = null)
        {
            _cardRegistry = cardRegistry;
            _random = random;
            _modifiers = modifiers;
            _powerResolver = powerResolver;
        }

        // ------------------------------------------------------------------
        // Stock y precios
        // ------------------------------------------------------------------

        /// <summary>Genera el stock del nodo si todavia no existe. No rebaraja.</summary>
        public void EnsureStock(RunState runState)
        {
            if (runState == null) return;

            runState.ShopState ??= new ShopState();
            if (runState.ShopState.Generated) return;

            var shop = runState.ShopState;
            shop.Stock.Clear();
            shop.UpgradeOffers = UpgradeOfferCount(runState.ActIndex);
            shop.ExtraLifeSold = false;

            GenerateStock(runState, shop);
            shop.Generated = true;
        }

        /// <summary>Acto 1: 0, acto 2: 1, acto 3: 2, acto final: 3.</summary>
        public static int UpgradeOfferCount(int actIndex) =>
            Math.Max(0, Math.Min(3, actIndex - 1));

        /// <summary>Precio base por coste de carta: 1-2 = 110, 3-4 = 140, 5-6 = 170.</summary>
        public static int BaseCardPrice(CardDef card)
        {
            int cost = card?.Cost ?? 1;

            if (cost <= 2) return 110;
            if (cost <= 4) return 140;
            return 170;
        }

        public int PriceFor(RunState runState, ShopCardOffer offer)
        {
            var card = _cardRegistry?.Get(offer?.CardId);
            if (card == null) return 0;

            return RunModifierSystem.ShopPriceFor(runState, BaseCardPrice(card));
        }

        public int UpgradePriceFor(RunState runState) =>
            RunModifierSystem.ShopPriceFor(runState, UpgradePrice);

        public int RemovalPriceFor(RunState runState) =>
            RunModifierSystem.ShopPriceFor(runState, RemovalPrice);

        public int ExtraLifePriceFor(RunState runState) =>
            RunModifierSystem.ShopPriceFor(runState, ExtraLifePrice);

        public CardDef GetCard(ShopCardOffer offer) =>
            offer == null ? null : _cardRegistry?.Get(offer.CardId);

        public int CountInDeck(RunState runState, string cardId)
        {
            if (runState?.Deck == null || string.IsNullOrWhiteSpace(cardId)) return 0;

            int count = 0;
            foreach (var card in runState.Deck)
            {
                if (string.Equals(card?.Definition?.Id, cardId, StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        // ------------------------------------------------------------------
        // Compras
        // ------------------------------------------------------------------

        public bool CanBuyCard(RunState runState, ShopCardOffer offer) =>
            runState != null && offer != null && !offer.Sold &&
            GetCard(offer) != null &&
            CountInDeck(runState, offer.CardId) < MaxCardCopies &&
            runState.Money >= PriceFor(runState, offer);

        /// <summary>
        /// Compra una carta. Con el mazo en 12 la carta a reemplazar es
        /// obligatoria; cancelar el reemplazo equivale a no comprar.
        /// </summary>
        public ShopPurchaseResult BuyCard(RunState runState, ShopCardOffer offer,
            CardInstance removeCard = null)
        {
            if (runState == null) return ShopPurchaseResult.Fail("No hay run activa.");
            if (offer == null) return ShopPurchaseResult.Fail("Oferta invalida.");

            var stockOffer = runState.ShopState?.Stock?
                .FirstOrDefault(entry => string.Equals(entry.CardId, offer.CardId,
                    StringComparison.Ordinal));

            if (stockOffer == null)
                return ShopPurchaseResult.Fail("La oferta ya no esta en la tienda.");

            if (stockOffer.Sold)
                return ShopPurchaseResult.Fail("Esa oferta ya fue comprada.");

            var definition = GetCard(stockOffer);
            if (definition == null)
                return ShopPurchaseResult.Fail("La carta de la oferta no existe.");

            if (CountInDeck(runState, stockOffer.CardId) >= MaxCardCopies)
                return ShopPurchaseResult.Fail("Ya tienes el maximo de copias de esa carta.");

            int price = PriceFor(runState, stockOffer);
            if (runState.Money < price)
                return ShopPurchaseResult.Fail("No alcanza el dinero.");

            bool atCap = runState.Deck.Count >= RunState.MaxDeckSize;

            if (atCap && removeCard == null)
                return ShopPurchaseResult.Fail("Elige una carta del mazo para reemplazar.");

            if (!atCap && removeCard != null)
                return ShopPurchaseResult.Fail("El mazo tiene espacio; no hay que reemplazar.");

            if (removeCard != null && !runState.Deck.Contains(removeCard))
                return ShopPurchaseResult.Fail("La carta a reemplazar no esta en el mazo.");

            runState.Money -= price;

            if (removeCard != null)
                runState.Deck.Remove(removeCard);

            var instance = new CardInstance(definition);
            _powerResolver?.ResolveCard(instance);
            runState.Deck.Add(instance);
            runState.DiscoverCard(definition.Id);

            stockOffer.Sold = true;
            return ShopPurchaseResult.Ok();
        }

        public bool CanBuyUpgrade(RunState runState) =>
            runState != null && runState.ShopState != null &&
            runState.ShopState.UpgradeOffers > 0 &&
            runState.Deck.Any(card => card != null && card.CanUpgrade) &&
            runState.Money >= UpgradePriceFor(runState);

        public ShopPurchaseResult BuyUpgrade(RunState runState, CardInstance card,
            UpgradeType type)
        {
            if (runState?.ShopState == null)
                return ShopPurchaseResult.Fail("No hay tienda activa.");

            if (runState.ShopState.UpgradeOffers <= 0)
                return ShopPurchaseResult.Fail("No quedan mejoras en esta tienda.");

            if (card == null || !runState.Deck.Contains(card))
                return ShopPurchaseResult.Fail("La carta no esta en el mazo.");

            if (!card.CanUpgrade)
                return ShopPurchaseResult.Fail("La carta ya tiene el maximo de mejoras.");

            if (type == UpgradeType.Cost && !card.CanUpgradeCost)
                return ShopPurchaseResult.Fail("La carta ya no puede bajar de coste.");

            int price = UpgradePriceFor(runState);
            if (runState.Money < price)
                return ShopPurchaseResult.Fail("No alcanza el dinero.");

            bool applied = type == UpgradeType.Cost
                ? card.TryUpgradeCost()
                : card.TryUpgradePower();

            if (!applied)
                return ShopPurchaseResult.Fail("La mejora no se pudo aplicar.");

            runState.Money -= price;
            runState.ShopState.UpgradeOffers--;
            _powerResolver?.ResolveCard(card);
            return ShopPurchaseResult.Ok();
        }

        public bool CanRemove(RunState runState) =>
            runState != null && runState.Deck.Count > RunState.MinDeckSize &&
            runState.Money >= RemovalPriceFor(runState);

        public ShopPurchaseResult BuyRemoval(RunState runState, CardInstance card)
        {
            if (runState == null) return ShopPurchaseResult.Fail("No hay run activa.");

            if (runState.Deck.Count <= RunState.MinDeckSize)
                return ShopPurchaseResult.Fail("El mazo no puede bajar de 6 cartas.");

            if (card == null || !runState.Deck.Contains(card))
                return ShopPurchaseResult.Fail("La carta no esta en el mazo.");

            int price = RemovalPriceFor(runState);
            if (runState.Money < price)
                return ShopPurchaseResult.Fail("No alcanza el dinero.");

            runState.Money -= price;
            runState.Deck.Remove(card);
            return ShopPurchaseResult.Ok();
        }

        public bool CanBuyExtraLife(RunState runState) =>
            runState != null && runState.ExtraLifeCharges <= 0 &&
            runState.ShopState != null && !runState.ShopState.ExtraLifeSold &&
            runState.Money >= ExtraLifePriceFor(runState);

        public ShopPurchaseResult BuyExtraLife(RunState runState)
        {
            if (runState == null) return ShopPurchaseResult.Fail("No hay run activa.");

            if (runState.ExtraLifeCharges > 0)
                return ShopPurchaseResult.Fail("Ya tienes una vida extra.");

            if (runState.ShopState == null || runState.ShopState.ExtraLifeSold)
                return ShopPurchaseResult.Fail("La vida extra ya se vendio en esta tienda.");

            int price = ExtraLifePriceFor(runState);
            if (runState.Money < price)
                return ShopPurchaseResult.Fail("No alcanza el dinero.");

            runState.Money -= price;
            runState.ExtraLifeCharges = 1;
            runState.ShopState.ExtraLifeSold = true;
            return ShopPurchaseResult.Ok();
        }

        // ------------------------------------------------------------------
        // Generacion
        // ------------------------------------------------------------------

        private void GenerateStock(RunState runState, ShopState shop)
        {
            var catalog = _cardRegistry?.GetDraftable() ?? new List<CardDef>();
            if (catalog.Count == 0) return;

            var deckCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var card in runState.Deck)
            {
                string id = card?.Definition?.Id;
                if (string.IsNullOrWhiteSpace(id)) continue;

                deckCounts.TryGetValue(id, out int count);
                deckCounts[id] = count + 1;
            }

            var chosen = new List<CardDef>();

            // 1) Una carta que no esta en el mazo.
            var newCandidates = catalog
                .Where(card => !deckCounts.ContainsKey(card.Id))
                .ToList();
            PickRandom(newCandidates, chosen);

            // 2) Una segunda copia de una carta del mazo con menos de 2 copias.
            var secondCandidates = catalog
                .Where(card => deckCounts.TryGetValue(card.Id, out int count) &&
                               count < MaxCardCopies)
                .Where(card => !ContainsId(chosen, card.Id))
                .ToList();
            PickRandom(secondCandidates, chosen);

            // 3) Relleno aleatorio sin duplicar ofertas ni superar 2 copias.
            var fillCandidates = catalog
                .Where(card => !ContainsId(chosen, card.Id))
                .Where(card => !deckCounts.TryGetValue(card.Id, out int count) ||
                               count < MaxCardCopies)
                .ToList();

            while (chosen.Count < StockSize && fillCandidates.Count > 0)
                PickRandom(fillCandidates, chosen);

            // Degradacion: el mazo ya tiene todas las copias posibles.
            if (chosen.Count < StockSize)
            {
                var fallback = catalog
                    .Where(card => !ContainsId(chosen, card.Id))
                    .ToList();

                while (chosen.Count < StockSize && fallback.Count > 0)
                    PickRandom(fallback, chosen);
            }

            foreach (var card in chosen)
                shop.Stock.Add(new ShopCardOffer { CardId = card.Id, Sold = false });
        }

        private void PickRandom(List<CardDef> candidates, List<CardDef> chosen)
        {
            if (candidates == null || candidates.Count == 0) return;
            if (chosen.Count >= StockSize) return;

            int index = _random.Next(candidates.Count);
            chosen.Add(candidates[index]);
            candidates.RemoveAt(index);
        }

        private static bool ContainsId(List<CardDef> cards, string id)
        {
            foreach (var card in cards)
            {
                if (string.Equals(card.Id, id, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
