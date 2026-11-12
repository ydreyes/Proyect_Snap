using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Tienda del nodo Shop: chip de dinero, cuatro ofertas con arte y precio,
    /// tira del mazo y servicios en mosaico (mejora, retirada, vida extra y
    /// salida). El stock y cada compra siguen viviendo en la run.
    /// </summary>
    public class ShopScene : UiScene
    {
        private enum Phase
        {
            Browse,
            ReplaceCard,
            UpgradeCard,
            UpgradeType,
            RemoveCard
        }

        private RunState _runState;
        private RunDirector _director;
        private RunCheckpointService _checkpoint;
        private IngameShopSystem _shopSystem;

        private Phase _phase;
        private ShopCardOffer _pendingOffer;
        private CardInstance _selectedCard;
        private string _error;
        private bool _finished;

        public ShopScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title => "TIENDA";
        protected override string Subtitle => "MERCADO DE LA RUN";
        protected override Color Accent => UiTheme.Gold;
        protected override string CurrencyIcon => "M";
        protected override string CurrencyValue =>
            (_runState?.Money ?? 0).ToString();
        protected override string CurrencyLabel => "DINERO";
        protected override int BackgroundSeed => 7707;

        protected override void OnEnterScene()
        {
            _runState = Services.Get<RunState>();
            _director = Services.Get<RunDirector>();
            _checkpoint = Services.Get<RunCheckpointService>();
            _shopSystem = Services.Get<IngameShopSystem>();

            _phase = Phase.Browse;
            _pendingOffer = null;
            _selectedCard = null;
            _error = null;
            _finished = false;

            _shopSystem.EnsureStock(_runState);
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle OffersArea
        {
            get
            {
                var inner = ContentInner;
                int height = (int)(inner.Height * 0.36f);
                return new Rectangle(inner.X, inner.Y, inner.Width, height);
            }
        }

        private Rectangle DeckStripArea
        {
            get
            {
                var inner = ContentInner;
                int y = OffersArea.Bottom + UiTheme.Px(UiTheme.SpaceMd,
                    Scaffold.UiScale);
                int height = UiTheme.Px(96, Scaffold.UiScale);
                return new Rectangle(inner.X, y, inner.Width,
                    Math.Max(0, Math.Min(height,
                        inner.Bottom - y)));
            }
        }

        private Rectangle ServicesArea
        {
            get
            {
                var inner = ContentInner;
                int y = DeckStripArea.Bottom + UiTheme.Px(UiTheme.SpaceMd,
                    Scaffold.UiScale);
                int bottom = inner.Bottom - UiTheme.Px(24, Scaffold.UiScale);
                return new Rectangle(inner.X, y, inner.Width,
                    Math.Max(0, bottom - y));
            }
        }

        private Rectangle MessageBounds
        {
            get
            {
                var inner = ContentInner;
                int height = UiTheme.Px(20, Scaffold.UiScale);
                return new Rectangle(inner.X, inner.Bottom - height,
                    inner.Width, height);
            }
        }

        private Rectangle GetOfferBounds(int index)
        {
            var area = OffersArea;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (area.Width - gap * 3) / 4;
            int height = (int)(width * 1.42f);
            height = Math.Min(height, area.Height);
            int y = area.Y + Math.Max(0, (area.Height - height) / 2);
            return new Rectangle(area.X + index * (width + gap), y, width,
                height);
        }

        private Rectangle GetServiceBounds(int index)
        {
            var area = ServicesArea;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            const int rows = 4;
            int height = Math.Min(UiTheme.Px(64, Scaffold.UiScale),
                (area.Height - gap * (rows - 1)) / rows);
            int block = height * rows + gap * (rows - 1);
            int top = area.Bottom - block;
            return new Rectangle(area.X, top + index * (height + gap),
                area.Width, height);
        }

        private Rectangle DeckHeaderBounds
        {
            get
            {
                var area = PhaseDeckArea;
                return new Rectangle(area.X, area.Y, area.Width,
                    UiTheme.Px(22, Scaffold.UiScale));
            }
        }

        private Rectangle PhaseDeckArea
        {
            get
            {
                var inner = ContentInner;
                int bottom = inner.Bottom - UiTheme.Px(56, Scaffold.UiScale) -
                    UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale) -
                    UiTheme.Px(24, Scaffold.UiScale);
                return new Rectangle(inner.X, inner.Y, inner.Width,
                    Math.Max(0, bottom - inner.Y));
            }
        }

        private Rectangle GetDeckCardBounds(int index)
        {
            var area = PhaseDeckArea;
            const int columns = 4;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int top = area.Y + UiTheme.Px(28, Scaffold.UiScale);
            int width = (area.Width - gap * (columns - 1)) / columns;
            int maxHeight = Math.Max(8,
                (area.Bottom - top - gap * 2) / 3);
            int height = Math.Min((int)(width * 1.38f), maxHeight);
            int row = index / columns;
            int column = index % columns;
            return new Rectangle(area.X + column * (width + gap),
                top + row * (height + gap), width, height);
        }

        private Rectangle ActionsRow
        {
            get
            {
                var inner = ContentInner;
                int height = Math.Max(UiTheme.Px(UiTheme.TouchTarget,
                    Scaffold.UiScale), UiTheme.Px(48, Scaffold.UiScale));
                return new Rectangle(inner.X,
                    inner.Bottom - UiTheme.Px(24, Scaffold.UiScale) - height,
                    inner.Width, height);
            }
        }

        private Rectangle GetUpgradeOptionBounds(bool secondary)
        {
            var area = PhaseDeckArea;
            int gap = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
            int width = (area.Width - gap) / 2;
            int height = UiTheme.Px(76, Scaffold.UiScale);
            int y = area.Y + (int)(area.Height * 0.56f);
            return new Rectangle(secondary ? area.X + width + gap : area.X,
                y, width, height);
        }

        private Rectangle UpgradeCancelBounds
        {
            get
            {
                var area = PhaseDeckArea;
                int height = Math.Max(UiTheme.Px(UiTheme.TouchTarget,
                    Scaffold.UiScale), UiTheme.Px(46, Scaffold.UiScale));
                int width = (int)(area.Width * 0.62f);
                var option = GetUpgradeOptionBounds(false);
                return new Rectangle(area.X + (area.Width - width) / 2,
                    option.Bottom + UiTheme.Px(UiTheme.SpaceMd,
                        Scaffold.UiScale), width, height);
            }
        }

        private Rectangle GetActionBounds(bool secondary)
        {
            var row = ActionsRow;
            int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
            int width = (row.Width - gap) / 2;
            return new Rectangle(secondary ? row.X + width + gap : row.X,
                row.Y, width, row.Height);
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (_finished) return;

            switch (_phase)
            {
                case Phase.Browse:
                    UpdateBrowse();
                    break;
                case Phase.ReplaceCard:
                    UpdateReplaceCard();
                    break;
                case Phase.UpgradeCard:
                    UpdateUpgradeCard();
                    break;
                case Phase.UpgradeType:
                    UpdateUpgradeType();
                    break;
                case Phase.RemoveCard:
                    UpdateRemoveCard();
                    break;
            }
        }

        private void UpdateBrowse()
        {
            var stock = _runState?.ShopState?.Stock;
            if (stock != null)
            {
                for (int i = 0; i < stock.Count && i < 4; i++)
                {
                    if (!Tap($"offer{i}", GetOfferBounds(i), !stock[i].Sold))
                        continue;

                    OnOfferClicked(stock[i]);
                    return;
                }
            }

            bool canUpgrade = _shopSystem.CanBuyUpgrade(_runState);
            if (Tap("service_upgrade", GetServiceBounds(0), canUpgrade) &&
                canUpgrade)
            {
                _selectedCard = null;
                _error = null;
                _phase = Phase.UpgradeCard;
                return;
            }

            bool canRemove = _shopSystem.CanRemove(_runState);
            if (Tap("service_remove", GetServiceBounds(1), canRemove) &&
                canRemove)
            {
                _selectedCard = null;
                _error = null;
                _phase = Phase.RemoveCard;
                return;
            }

            bool canBuyLife = _shopSystem.CanBuyExtraLife(_runState);
            if (Tap("service_life", GetServiceBounds(2), canBuyLife) &&
                canBuyLife)
            {
                ResolvePurchase(_shopSystem.BuyExtraLife(_runState));
                return;
            }

            if (Tap("service_leave", GetServiceBounds(3)))
                Finish();
        }

        private void OnOfferClicked(ShopCardOffer offer)
        {
            if (offer == null || offer.Sold) return;

            if (!_shopSystem.CanBuyCard(_runState, offer))
            {
                if (_shopSystem.CountInDeck(_runState, offer.CardId) >=
                    IngameShopSystem.MaxCardCopies)
                    _error = "Ya tienes el maximo de copias de esa carta.";
                else if (_runState.Money <
                         _shopSystem.PriceFor(_runState, offer))
                    _error = "No alcanza el dinero.";
                else
                    _error = "Esa oferta ya no esta disponible.";
                return;
            }

            if (_runState.Deck.Count >= RunState.MaxDeckSize)
            {
                _pendingOffer = offer;
                _selectedCard = null;
                _error = null;
                _phase = Phase.ReplaceCard;
                return;
            }

            ResolvePurchase(_shopSystem.BuyCard(_runState, offer));
        }

        private void UpdateReplaceCard()
        {
            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                if (!Tap($"deck{i}", GetDeckCardBounds(i))) continue;

                _selectedCard = _runState.Deck[i];
                return;
            }

            bool canConfirm = _selectedCard != null && _pendingOffer != null;
            if (Tap("confirm", GetActionBounds(false), canConfirm) && canConfirm)
            {
                var result = _shopSystem.BuyCard(_runState, _pendingOffer,
                    _selectedCard);

                if (result.Success)
                {
                    _pendingOffer = null;
                    _selectedCard = null;
                    _phase = Phase.Browse;
                    _error = null;
                    _checkpoint.SaveSafeNode();
                }
                else
                {
                    _error = result.Error;
                }

                return;
            }

            if (Tap("cancel", GetActionBounds(true)))
            {
                // Cancelar el reemplazo cancela la compra sin gastar dinero.
                _pendingOffer = null;
                _selectedCard = null;
                _error = null;
                _phase = Phase.Browse;
            }
        }

        private void UpdateUpgradeCard()
        {
            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                var card = _runState.Deck[i];
                if (!Tap($"deck{i}", GetDeckCardBounds(i), card.CanUpgrade))
                    continue;

                if (!card.CanUpgrade) return;

                _selectedCard = card;
                _error = null;
                _phase = Phase.UpgradeType;
                return;
            }

            if (Tap("cancel", GetActionBounds(true)))
            {
                _selectedCard = null;
                _error = null;
                _phase = Phase.Browse;
            }
        }

        private void UpdateUpgradeType()
        {
            if (_selectedCard == null)
            {
                _phase = Phase.UpgradeCard;
                return;
            }

            int price = _shopSystem.UpgradePriceFor(_runState);
            bool affordable = _runState.Money >= price &&
                              _runState.ShopState.UpgradeOffers > 0;

            bool costEnabled = affordable && _selectedCard.CanUpgradeCost;
            bool powerEnabled = affordable && _selectedCard.CanUpgrade;

            if (Tap("upgrade_cost", GetUpgradeOptionBounds(false), costEnabled) &&
                costEnabled)
            {
                ResolveUpgrade(UpgradeType.Cost);
                return;
            }

            if (Tap("upgrade_power", GetUpgradeOptionBounds(true), powerEnabled) &&
                powerEnabled)
            {
                ResolveUpgrade(UpgradeType.Power);
                return;
            }

            if (Tap("cancel_type", UpgradeCancelBounds))
            {
                _selectedCard = null;
                _error = null;
                _phase = Phase.UpgradeCard;
            }
        }

        private void ResolveUpgrade(UpgradeType type)
        {
            var result = _shopSystem.BuyUpgrade(_runState, _selectedCard, type);
            if (!result.Success)
            {
                _error = result.Error;
                return;
            }

            _selectedCard = null;
            _phase = Phase.Browse;
            _error = null;
            _checkpoint.SaveSafeNode();
        }

        private void UpdateRemoveCard()
        {
            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                if (!Tap($"deck{i}", GetDeckCardBounds(i))) continue;

                var result = _shopSystem.BuyRemoval(_runState,
                    _runState.Deck[i]);

                if (result.Success)
                {
                    _phase = Phase.Browse;
                    _error = null;
                    _checkpoint.SaveSafeNode();
                }
                else
                {
                    _error = result.Error;
                }

                return;
            }

            if (Tap("cancel", GetActionBounds(true)))
            {
                _error = null;
                _phase = Phase.Browse;
            }
        }

        private void ResolvePurchase(ShopPurchaseResult result)
        {
            if (!result.Success)
            {
                _error = result.Error;
                return;
            }

            _error = null;
            _checkpoint.SaveSafeNode();
        }

        private void Finish()
        {
            if (_finished) return;

            _finished = true;
            _director.OnShopEventComplete();
            _checkpoint.SaveSafeNode();
            Scenes.ChangeScene(_director.GetNextScene());
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            switch (_phase)
            {
                case Phase.Browse:
                    DrawBrowse(spriteBatch, uiScale);
                    break;
                case Phase.ReplaceCard:
                    DrawDeckSelection(spriteBatch, uiScale,
                        "ELIGE LA CARTA QUE SALE DEL MAZO", true, true);
                    break;
                case Phase.UpgradeCard:
                    DrawDeckSelection(spriteBatch, uiScale,
                        "ELIGE UNA CARTA PARA MEJORAR", false, true);
                    break;
                case Phase.UpgradeType:
                    DrawUpgradeType(spriteBatch, uiScale);
                    break;
                case Phase.RemoveCard:
                    DrawDeckSelection(spriteBatch, uiScale,
                        $"ELIGE UNA CARTA PARA RETIRAR ({_shopSystem.RemovalPriceFor(_runState)})",
                        false, true);
                    break;
            }

            if (!string.IsNullOrEmpty(_error))
            {
                DrawTextCenteredFit(_error, MessageBounds,
                    UiTheme.DangerSoft, UiTheme.Caption);
            }
        }

        private void DrawBrowse(SpriteBatch spriteBatch, float uiScale)
        {
            DrawOffers(spriteBatch, uiScale);
            DrawDeckStrip(spriteBatch, uiScale);
            DrawServices(spriteBatch, uiScale);

            DrawTextCenteredFit(RunModifierSystem.Describe(_runState),
                MessageBounds, UiTheme.TextMuted, UiTheme.Caption);
        }

        private void DrawOffers(SpriteBatch spriteBatch, float uiScale)
        {
            var stock = _runState?.ShopState?.Stock;
            if (stock == null) return;

            for (int i = 0; i < stock.Count && i < 4; i++)
            {
                var offer = stock[i];
                var bounds = GetOfferBounds(i);
                var definition = _shopSystem.GetCard(offer);

                if (definition == null)
                {
                    DrawPanel(bounds, PanelTone.Sunken);
                    DrawTextCenteredFit("AGOTADA", bounds, UiTheme.TextMuted,
                        UiTheme.Caption);
                    continue;
                }

                int price = _shopSystem.PriceFor(_runState, offer);
                bool affordable = _runState.Money >= price;

                var state = offer.Sold
                    ? CardVisualState.Sold
                    : affordable
                        ? CardVisualState.Normal
                        : CardVisualState.Unaffordable;

                if (!offer.Sold && IsHovered($"offer{i}") &&
                    Input.IsLeftMouseDown())
                    state = CardVisualState.Selected;

                DrawCardDefinition(bounds, definition, new CardRenderOptions
                {
                    Variant = CardFrameVariant.Store,
                    State = state,
                    Price = offer.Sold ? "VENDIDA" : price.ToString(),
                    Tag = offer.Sold ? "VENDIDA" : null
                });
            }
        }

        private void DrawDeckStrip(SpriteBatch spriteBatch, float uiScale)
        {
            var area = DeckStripArea;
            if (area.Height <= 0) return;

            var deck = _runState?.Deck;
            DrawSectionLabel(
                $"MAZO {deck?.Count ?? 0}/{RunState.MaxDeckSize}   " +
                $"MEJORAS {_runState?.ShopState?.UpgradeOffers ?? 0}",
                new Rectangle(area.X, area.Y, area.Width,
                    UiTheme.Px(20, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);

            var strip = new Rectangle(area.X,
                area.Y + UiTheme.Px(22, uiScale), area.Width,
                Math.Max(0, area.Height - UiTheme.Px(22, uiScale)));

            int count = Math.Max(1, deck?.Count ?? 0);
            int gap = UiTheme.Px(UiTheme.SpaceXs, uiScale);
            int width = Math.Max(8, (strip.Width - gap * (count - 1)) / count);
            int height = Math.Min(strip.Height, (int)(width * 1.36f));

            for (int i = 0; i < (deck?.Count ?? 0); i++)
            {
                var slot = new Rectangle(strip.X + i * (width + gap),
                    strip.Y + Math.Max(0, (strip.Height - height) / 2),
                    width, height);
                DrawCardInstance(slot, deck[i], new CardRenderOptions
                {
                    Variant = CardFrameVariant.MiniDeck,
                    ShowName = false
                });
            }
        }

        private void DrawServices(SpriteBatch spriteBatch, float uiScale)
        {
            int price = _shopSystem.UpgradePriceFor(_runState);
            int offers = _runState?.ShopState?.UpgradeOffers ?? 0;
            DrawServiceTile("service_upgrade", GetServiceBounds(0),
                "MEJORA", offers > 0 ? $"{price}  ({offers})" : "AGOTADA",
                _shopSystem.CanBuyUpgrade(_runState), UiTheme.Accent, uiScale);

            DrawServiceTile("service_remove", GetServiceBounds(1),
                "RETIRAR", $"{_shopSystem.RemovalPriceFor(_runState)}",
                _shopSystem.CanRemove(_runState), UiTheme.Violet, uiScale);

            bool lifeAvailable = _runState != null &&
                                 _runState.ExtraLifeCharges <= 0 &&
                                 _runState.ShopState != null &&
                                 !_runState.ShopState.ExtraLifeSold;
            string lifeDetail = !lifeAvailable
                ? _runState?.ExtraLifeCharges > 0 ? "YA LA TIENES" : "VENDIDA"
                : $"{_shopSystem.ExtraLifePriceFor(_runState)}";
            DrawServiceTile("service_life", GetServiceBounds(2),
                "VIDA EXTRA", lifeDetail,
                _shopSystem.CanBuyExtraLife(_runState), UiTheme.Success,
                uiScale);

            DrawServiceTile("service_leave", GetServiceBounds(3),
                "SALIR", "VOLVER A LA RUN", true, UiTheme.AccentSoft,
                uiScale);
        }

        private void DrawServiceTile(string key, Rectangle bounds, string title,
            string detail, bool enabled, Color accent, float uiScale)
        {
            DrawPanel(bounds, enabled ? PanelTone.Raised : PanelTone.Sunken);
            if (IsHovered(key) && enabled)
                DrawPanel(Inset(bounds, UiTheme.Px(3, uiScale)),
                    PanelTone.Accent, false);

            int pad = UiTheme.Px(UiTheme.SpaceLg, uiScale);
            int accentWidth = UiTheme.Px(4, uiScale);
            SpriteBatch.Draw(Ui.Textures.Pixel,
                new Rectangle(bounds.X + UiTheme.Px(2, uiScale),
                    bounds.Y + UiTheme.Px(8, uiScale), accentWidth,
                    Math.Max(0, bounds.Height - UiTheme.Px(16, uiScale))),
                enabled ? accent : UiTheme.TextMuted);

            var titleBounds = new Rectangle(bounds.X + pad, bounds.Y,
                (int)(bounds.Width * 0.54f) - pad, bounds.Height);
            DrawSectionLabel(title, titleBounds,
                enabled ? accent : UiTheme.TextMuted, UiTheme.Label);

            DrawTextCenteredFit(detail,
                new Rectangle(bounds.X + (int)(bounds.Width * 0.54f),
                    bounds.Y, bounds.Width - (int)(bounds.Width * 0.54f) - pad,
                    bounds.Height),
                enabled ? UiTheme.TextSecondary : UiTheme.TextMuted,
                UiTheme.NumericSmall);
        }

        private void DrawDeckSelection(SpriteBatch spriteBatch, float uiScale,
            string header, bool confirm, bool cancel)
        {
            var area = PhaseDeckArea;
            DrawSectionLabel(header,
                new Rectangle(area.X, area.Y, area.Width,
                    UiTheme.Px(22, uiScale)),
                UiTheme.Gold, UiTheme.LabelSmall);

            bool upgrading = _phase == Phase.UpgradeCard;

            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                var card = _runState.Deck[i];
                var state = CardVisualState.Normal;

                if (ReferenceEquals(card, _selectedCard))
                    state = CardVisualState.Selected;
                else if (upgrading && !card.CanUpgrade)
                    state = CardVisualState.Sold;

                DrawCardInstance(GetDeckCardBounds(i), card,
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.Collection,
                        State = state
                    });
            }

            if (_selectedCard != null)
            {
                DrawTextCenteredFit(
                    $"{_selectedCard.Definition?.Name}   " +
                    $"COSTE {_selectedCard.EffectiveCost}   " +
                    $"PODER {_selectedCard.EffectivePower}   " +
                    $"MEJORAS {_selectedCard.UpgradeCount}/" +
                    $"{CardInstance.MaxUpgrades}",
                    new Rectangle(area.X, ActionsRow.Y -
                        UiTheme.Px(22, uiScale), area.Width,
                        UiTheme.Px(20, uiScale)),
                    UiTheme.TextSecondary, UiTheme.Caption);
            }

            if (confirm)
                DrawTapButtonScaled("confirm", "CONFIRMAR", UiTheme.Success);
            if (cancel)
                DrawTapButtonScaled("cancel", confirm ? "CANCELAR" : "VOLVER",
                    UiTheme.BorderBright);
        }

        private void DrawUpgradeType(SpriteBatch spriteBatch, float uiScale)
        {
            var area = PhaseDeckArea;
            DrawSectionLabel("ELIGE EL TIPO DE MEJORA",
                new Rectangle(area.X, area.Y, area.Width,
                    UiTheme.Px(22, uiScale)),
                UiTheme.Gold, UiTheme.LabelSmall);

            if (_selectedCard != null)
            {
                var portrait = new Rectangle(area.Center.X -
                    (int)(area.Width * 0.14f), area.Y +
                    UiTheme.Px(28, uiScale),
                    (int)(area.Width * 0.28f),
                    Math.Min((int)(area.Height * 0.56f),
                        (int)(area.Width * 0.28f * 1.42f)));

                DrawCardInstance(portrait, _selectedCard,
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.Medallion,
                        State = CardVisualState.Selected
                    });

                DrawTextCenteredFit(
                    $"{_selectedCard.Definition?.Name}   " +
                    $"MEJORAS {_selectedCard.UpgradeCount}/" +
                    $"{CardInstance.MaxUpgrades}",
                    new Rectangle(area.X, portrait.Bottom +
                        UiTheme.Px(UiTheme.SpaceMd, uiScale), area.Width,
                        UiTheme.Px(26, uiScale)),
                    UiTheme.TextPrimary, UiTheme.Label);

                DrawTextCenteredFit(
                    $"COSTE {_selectedCard.EffectiveCost}   " +
                    $"PODER {_selectedCard.EffectivePower}   " +
                    $"PRECIO {_shopSystem.UpgradePriceFor(_runState)}",
                    new Rectangle(area.X, portrait.Bottom +
                        UiTheme.Px(UiTheme.SpaceMd + 30, uiScale),
                        area.Width, UiTheme.Px(22, uiScale)),
                    UiTheme.TextSecondary, UiTheme.Caption);
            }

            int price = _shopSystem.UpgradePriceFor(_runState);
            bool affordable = _runState.Money >= price &&
                              _runState.ShopState.UpgradeOffers > 0;

            bool costEnabled = affordable && _selectedCard != null &&
                               _selectedCard.CanUpgradeCost;
            bool powerEnabled = affordable && _selectedCard != null &&
                                _selectedCard.CanUpgrade;

            DrawUpgradeTile("upgrade_cost", GetUpgradeOptionBounds(false),
                "-1 COSTE", "REDUCE EL COSTE EFECTIVO",
                costEnabled, UiTheme.Accent);
            DrawUpgradeTile("upgrade_power", GetUpgradeOptionBounds(true),
                "+2 PODER", "SUMA PODER A LA CARTA",
                powerEnabled, UiTheme.Gold);

            DrawTapButtonScaled("cancel_type", "VOLVER",
                UiTheme.BorderBright, UiTheme.LabelSmall);
        }

        private void DrawUpgradeTile(string key, Rectangle bounds, string title,
            string detail, bool enabled, Color accent)
        {
            DrawPanel(bounds, enabled ? PanelTone.Raised : PanelTone.Sunken);
            if (IsHovered(key) && enabled)
                DrawPanel(Inset(bounds, UiTheme.Px(3, Scaffold.UiScale)),
                    PanelTone.Accent, false);

            int pad = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
            DrawDisplay(title,
                new Vector2(bounds.X + pad, bounds.Y +
                    UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale)),
                enabled ? accent : UiTheme.TextMuted, UiTheme.Heading);

            DrawTextCenteredFit(detail,
                new Rectangle(bounds.X + pad,
                    bounds.Bottom - UiTheme.Px(26, Scaffold.UiScale),
                    bounds.Width - pad * 2, UiTheme.Px(22, Scaffold.UiScale)),
                enabled ? UiTheme.TextSecondary : UiTheme.TextMuted,
                UiTheme.Caption);
        }

        // ------------------------------------------------------------------
        // Geometria publica (compatibilidad con el layout anterior)
        // ------------------------------------------------------------------

        public static Rectangle GetErrorBounds(int width, int height) =>
            new Rectangle((int)(width * 0.06f), (int)(height * 0.835f),
                (int)(width * 0.88f), (int)(height * 0.04f));

        public static Rectangle GetPriceBounds(int width, int height, int index)
        {
            var card = GetOfferBounds(width, height, index);
            return new Rectangle(card.X, card.Bottom + 2, card.Width,
                (int)(height * 0.03f));
        }

        public static Rectangle GetOfferBounds(int width, int height, int index)
        {
            int cardWidth = (int)(width * 0.23f);
            int cardHeight = (int)(cardWidth * 1.42f);
            int gap = (int)(width * 0.06f);
            int totalWidth = cardWidth * 2 + gap;
            int startX = (width - totalWidth) / 2;
            int priceHeight = (int)(height * 0.03f);
            int rowGap = (int)(height * 0.018f);
            int rowPitch = cardHeight + priceHeight + rowGap;
            int startY = (int)(height * 0.145f);

            int row = index / 2;
            int column = index % 2;

            return new Rectangle(
                startX + column * (cardWidth + gap),
                startY + row * rowPitch,
                cardWidth,
                cardHeight);
        }

        public static Rectangle GetDeckCardBounds(int width, int height,
            int index)
        {
            const int columns = 4;

            int cardWidth = (int)(width * 0.20f);
            int cardHeight = (int)(cardWidth * 1.42f);
            int gap = (int)(width * 0.015f);
            int totalWidth = columns * cardWidth + (columns - 1) * gap;
            int startX = (width - totalWidth) / 2;
            int startY = (int)(height * 0.20f);

            int row = index / columns;
            int column = index % columns;

            return new Rectangle(
                startX + column * (cardWidth + gap),
                startY + row * (cardHeight + gap),
                cardWidth,
                cardHeight);
        }

        public static Rectangle GetUpgradeButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.04f),
                (int)(height * 0.60f),
                (int)(width * 0.44f),
                (int)(height * 0.065f));

        public static Rectangle GetRemovalButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.52f),
                (int)(height * 0.60f),
                (int)(width * 0.44f),
                (int)(height * 0.065f));

        public static Rectangle GetExtraLifeButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.04f),
                (int)(height * 0.765f),
                (int)(width * 0.92f),
                (int)(height * 0.06f));

        public static Rectangle GetLeaveButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.20f),
                (int)(height * 0.90f),
                (int)(width * 0.60f),
                (int)(height * 0.075f));

        public static Rectangle GetUpgradeTypeButton(int width, int height,
            bool cost)
        {
            int buttonWidth = (int)(width * 0.38f);
            int buttonHeight = (int)(height * 0.07f);
            int gap = (int)(width * 0.04f);
            int startX = (width - (buttonWidth * 2 + gap)) / 2;

            return new Rectangle(
                cost ? startX : startX + buttonWidth + gap,
                (int)(height * 0.62f),
                buttonWidth,
                buttonHeight);
        }

        public static Rectangle GetConfirmButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.06f),
                (int)(height * 0.86f),
                (int)(width * 0.40f),
                (int)(height * 0.07f));

        public static Rectangle GetCancelButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.54f),
                (int)(height * 0.86f),
                (int)(width * 0.40f),
                (int)(height * 0.07f));
    }
}
