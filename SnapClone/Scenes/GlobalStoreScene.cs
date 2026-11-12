using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Meta;
using SnapClone.Persistence;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Tienda permanente de puntos: chip de saldo, pestanas OFERTAS/CARTAS,
    /// grilla de ofertas con precio y estado vendido. La economia no cambia.
    /// </summary>
    public class GlobalStoreScene : UiScene
    {
        private const int PageSize = 6;

        private enum Tab
        {
            Offers,
            Cards
        }

        private GlobalStoreSystem _store;
        private RunCheckpointService _checkpoint;
        private CardRegistry _cardRegistry;
        private ProfileDto _profile;

        private readonly List<GlobalStoreEntry> _entries =
            new List<GlobalStoreEntry>();

        private Tab _tab;
        private int _page;
        private string _message;
        private bool _messageIsError;

        public GlobalStoreScene(GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch, GameServices services,
            SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title => "TIENDA GLOBAL";
        protected override string Subtitle => "GASTA PUNTOS EN CONTENIDO PERMANENTE";
        protected override Color Accent => UiTheme.Violet;
        protected override string CurrencyIcon => "P";
        protected override string CurrencyValue => (_profile?.Points ?? 0).ToString();
        protected override string CurrencyLabel => "PUNTOS";
        protected override int BackgroundSeed => 6606;

        protected override void OnEnterScene()
        {
            _store = Services.Get<GlobalStoreSystem>();
            _checkpoint = Services.Get<RunCheckpointService>();
            _cardRegistry = Services.Has<CardRegistry>()
                ? Services.Get<CardRegistry>()
                : null;
            _profile = _checkpoint.Profile;
            _tab = Tab.Offers;
            _page = 0;
            _message = null;
            _messageIsError = false;
            RefreshEntries();
        }

        private void RefreshEntries()
        {
            _entries.Clear();
            if (_store == null || _profile == null) return;

            _entries.AddRange(_store.GetEntries(_profile)
                .Where(entry => !entry.Hidden));

            int pages = PageCount;
            if (_page >= pages) _page = Math.Max(0, pages - 1);
        }

        private List<GlobalStoreEntry> FilteredEntries()
        {
            if (_tab == Tab.Cards)
                return _entries
                    .Where(entry => entry.Kind == GlobalStoreEntryKind.DirectCard)
                    .ToList();

            return _entries
                .Where(entry => entry.Kind != GlobalStoreEntryKind.DirectCard)
                .ToList();
        }

        private int PageCount
        {
            get
            {
                int count = FilteredEntries().Count;
                return Math.Max(1, (count + PageSize - 1) / PageSize);
            }
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle TabsRow
        {
            get
            {
                var inner = ContentInner;
                return new Rectangle(inner.X, inner.Y, inner.Width,
                    UiTheme.Px(46, Scaffold.UiScale));
            }
        }

        private Rectangle PagerRow
        {
            get
            {
                var inner = ContentInner;
                int height = UiTheme.Px(46, Scaffold.UiScale);
                return new Rectangle(inner.X, inner.Bottom - height,
                    inner.Width, height);
            }
        }

        private Rectangle MessageBounds
        {
            get
            {
                var row = PagerRow;
                return new Rectangle(row.X, row.Y - UiTheme.Px(24,
                    Scaffold.UiScale), row.Width, UiTheme.Px(20,
                    Scaffold.UiScale));
            }
        }

        private Rectangle GridArea
        {
            get
            {
                var inner = ContentInner;
                int top = TabsRow.Bottom + UiTheme.Px(UiTheme.SpaceMd,
                    Scaffold.UiScale);
                int bottom = MessageBounds.Y - UiTheme.Px(UiTheme.SpaceSm,
                    Scaffold.UiScale);
                return new Rectangle(inner.X, top, inner.Width,
                    Math.Max(0, bottom - top));
            }
        }

        private Rectangle GetTabBounds(bool cards)
        {
            var row = TabsRow;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (row.Width - gap) / 2;
            return new Rectangle(cards ? row.X + width + gap : row.X, row.Y,
                width, row.Height);
        }

        private Rectangle GetEntryBounds(int index)
        {
            var area = GridArea;

            if (_tab == Tab.Cards)
            {
                const int columns = 3;
                const int rows = 2;
                int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
                int width = (area.Width - gap * (columns - 1)) / columns;
                int height = (area.Height - gap * (rows - 1)) / rows;
                height = Math.Min(height, (int)(width * 1.42f));
                int startY = area.Y + UiTheme.Px(30, Scaffold.UiScale);
                int row = index / columns;
                int column = index % columns;
                return new Rectangle(area.X + column * (width + gap),
                    startY + row * (height + gap), width, height);
            }

            int rowGap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int maxRowHeight = (area.Height - rowGap * (PageSize - 1)) /
                               PageSize;
            int rowHeight = Math.Min(maxRowHeight,
                UiTheme.Px(96, Scaffold.UiScale));
            int top = area.Y + UiTheme.Px(30, Scaffold.UiScale);
            return new Rectangle(area.X, top + index * (rowHeight + rowGap),
                area.Width, rowHeight);
        }

        private Rectangle GetPreviousButton()
        {
            var row = PagerRow;
            int width = (int)(row.Width * 0.26f);
            return new Rectangle(row.X, row.Y, width, row.Height);
        }

        private Rectangle GetNextButton()
        {
            var row = PagerRow;
            int width = (int)(row.Width * 0.26f);
            return new Rectangle(row.Right - width, row.Y, width, row.Height);
        }

        private Rectangle BackBounds
        {
            get
            {
                var footer = Scaffold.Footer;
                int pad = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
                int height = Math.Max(UiTheme.Px(UiTheme.TouchTarget,
                    Scaffold.UiScale), UiTheme.Px(48, Scaffold.UiScale));
                return new Rectangle(footer.X + pad,
                    footer.Y + (footer.Height - height) / 2,
                    footer.Width - pad * 2, height);
            }
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (Tap("back", BackBounds))
            {
                Scenes.ChangeScene("Menu");
                return;
            }

            if (Tap("tab_offers", GetTabBounds(false)))
            {
                _tab = Tab.Offers;
                _page = 0;
                return;
            }

            if (Tap("tab_cards", GetTabBounds(true)))
            {
                _tab = Tab.Cards;
                _page = 0;
                return;
            }

            var filtered = FilteredEntries();
            int pages = PageCount;
            if (_page >= pages) _page = pages - 1;

            bool canPrevious = _page > 0;
            bool canNext = _page + 1 < pages;

            if (Tap("previous", GetPreviousButton(), canPrevious) && canPrevious)
            {
                _page--;
                return;
            }

            if (Tap("next", GetNextButton(), canNext) && canNext)
            {
                _page++;
                return;
            }

            int start = _page * PageSize;
            int end = Math.Min(start + PageSize, filtered.Count);

            for (int i = start; i < end; i++)
            {
                var entry = filtered[i];
                bool enabled = entry.Available && !entry.Owned;
                if (!Tap($"entry{i - start}", GetEntryBounds(i - start), enabled))
                    continue;

                TryBuy(entry);
                return;
            }
        }

        private void TryBuy(GlobalStoreEntry entry)
        {
            if (entry == null) return;

            if (entry.Owned)
            {
                ShowMessage("Ese contenido ya esta en tu perfil.", false);
                return;
            }

            if (!_store.CanPurchase(_profile, entry))
            {
                ShowMessage(_profile.Points < entry.Price
                    ? "No alcanzan los puntos."
                    : "La entrada no esta disponible.", true);
                return;
            }

            var result = _store.Purchase(_profile, entry,
                profile => _checkpoint.SaveProfile(out _));

            if (!result.Success)
            {
                ShowMessage(result.Error, true);
                return;
            }

            ShowMessage($"Compraste {entry.Name}.", false);
            RefreshEntries();
        }

        private void ShowMessage(string message, bool isError)
        {
            _message = message;
            _messageIsError = isError;
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            DrawTapButtonScaled("tab_offers", "OFERTAS",
                _tab == Tab.Offers ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.LabelSmall);
            DrawTapButtonScaled("tab_cards", "CARTAS",
                _tab == Tab.Cards ? UiTheme.AccentSoft : UiTheme.BorderBright,
                UiTheme.LabelSmall);

            var filtered = FilteredEntries();
            int start = _page * PageSize;
            int end = Math.Min(start + PageSize, filtered.Count);

            var grid = GridArea;
            DrawSectionLabel(
                _tab == Tab.Offers ? "OFERTAS DE PUNTOS" : "CARTAS EN VENTA",
                new Rectangle(grid.X, grid.Y, grid.Width,
                    UiTheme.Px(20, uiScale)),
                UiTheme.TextSecondary, UiTheme.LabelSmall);

            for (int i = start; i < end; i++)
            {
                var bounds = GetEntryBounds(i - start);
                int keyIndex = i - start;
                if (_tab == Tab.Offers)
                    DrawOfferTile(spriteBatch, filtered[i], bounds, keyIndex,
                        uiScale);
                else
                    DrawCardTile(spriteBatch, filtered[i], bounds, keyIndex,
                        uiScale);
            }

            if (filtered.Count == 0)
            {
                var area = GridArea;
                DrawTextCenteredFit("NO HAY ENTRADAS EN ESTA PESTANA",
                    new Rectangle(area.X, area.Y + area.Height / 2,
                        area.Width, UiTheme.Px(30, uiScale)),
                    UiTheme.TextMuted, UiTheme.LabelSmall);
            }

            int pages = PageCount;
            bool canPrevious = _page > 0;
            bool canNext = _page + 1 < pages;
            DrawTapButtonScaled("previous", "ANTERIOR",
                canPrevious ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);
            DrawTapButtonScaled("next", "SIGUIENTE",
                canNext ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);

            DrawTextCenteredFit($"PAGINA {_page + 1}/{pages}",
                new Rectangle(PagerRow.X + (int)(PagerRow.Width * 0.30f),
                    PagerRow.Y + UiTheme.Px(12, uiScale),
                    (int)(PagerRow.Width * 0.40f),
                    UiTheme.Px(22, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);

            if (!string.IsNullOrEmpty(_message))
            {
                DrawTextCenteredFit(_message, MessageBounds,
                    _messageIsError ? UiTheme.DangerSoft : UiTheme.Success,
                    UiTheme.Caption);
            }

            DrawTapButtonScaled("back", "VOLVER AL MENU", UiTheme.AccentSoft,
                UiTheme.LabelSmall);
        }

        private void DrawOfferTile(SpriteBatch spriteBatch, GlobalStoreEntry entry,
            Rectangle bounds, int keyIndex, float uiScale)
        {
            PanelTone tone;
            Color accent;

            switch (entry.Kind)
            {
                case GlobalStoreEntryKind.Bundle:
                    tone = PanelTone.Violet;
                    accent = UiTheme.Violet;
                    break;
                case GlobalStoreEntryKind.DirectCard:
                    tone = PanelTone.Accent;
                    accent = UiTheme.AccentSoft;
                    break;
                default:
                    tone = PanelTone.Gold;
                    accent = UiTheme.Gold;
                    break;
            }

            if (entry.Owned) tone = PanelTone.Sunken;

            DrawPanel(bounds, tone);
            if (IsHovered($"entry{keyIndex}") && !entry.Owned)
                DrawPanel(Inset(bounds, UiTheme.Px(3, uiScale)),
                    PanelTone.Accent, false);

            int pad = UiTheme.Px(UiTheme.SpaceLg, uiScale);
            int priceWidth = UiTheme.Px(150, uiScale);

            var titleBounds = new Rectangle(bounds.X + pad, bounds.Y,
                Math.Max(0, bounds.Width - pad * 2 - priceWidth),
                Math.Min(bounds.Height, UiTheme.Px(36, uiScale)));
            DrawSectionLabel(entry.Name, titleBounds,
                entry.Owned ? UiTheme.TextMuted : accent,
                UiTheme.Label);

            bool affordable = (_profile?.Points ?? 0) >= entry.Price;
            string priceText = entry.Owned
                ? "VENDIDA"
                : $"{entry.Price} P";
            Color priceColor = entry.Owned
                ? UiTheme.TextMuted
                : affordable ? UiTheme.Success : UiTheme.DangerSoft;

            DrawTextCenteredFit(priceText,
                new Rectangle(bounds.Right - pad - priceWidth, bounds.Y,
                    priceWidth, UiTheme.Px(32, uiScale)),
                priceColor, UiTheme.NumericSmall);

            string description = entry.Kind == GlobalStoreEntryKind.Bundle
                ? $"{entry.BundleSize} CARTAS. {entry.Description}"
                : entry.Description;

            int textTop = bounds.Y + UiTheme.Px(32, uiScale);
            DrawWrapped(description,
                new Rectangle(bounds.X + pad, textTop,
                    bounds.Width - pad * 2,
                    Math.Max(0, bounds.Height - UiTheme.Px(38, uiScale))),
                UiTheme.TextSecondary, UiTheme.Caption);

            if (entry.Owned)
            {
                spriteBatch.Draw(Ui.Textures.Pixel, bounds,
                    Color.Black * 0.35f);
                var tag = new Rectangle(bounds.X + pad,
                    bounds.Bottom - UiTheme.Px(24, uiScale),
                    UiTheme.Px(96, uiScale), UiTheme.Px(18, uiScale));
                Ui.Chrome.DrawStateTag(spriteBatch, tag, "VENDIDA",
                    UiTheme.TextMuted, uiScale);
            }
        }

        private void DrawCardTile(SpriteBatch spriteBatch, GlobalStoreEntry entry,
            Rectangle bounds, int keyIndex, float uiScale)
        {
            CardDef card = entry.CardId == null
                ? null
                : _cardRegistry?.Get(entry.CardId);
            bool affordable = (_profile?.Points ?? 0) >= entry.Price;

            var options = new CardRenderOptions
            {
                Variant = CardFrameVariant.Store,
                State = entry.Owned
                    ? CardVisualState.Sold
                    : affordable
                        ? CardVisualState.Normal
                        : CardVisualState.Unaffordable,
                Price = entry.Owned ? null : $"{entry.Price} P",
                Tag = entry.Owned ? "VENDIDA" : "EN TIENDA"
            };

            if (card != null)
            {
                DrawCardDefinition(bounds, card, options);
            }
            else
            {
                DrawPanel(bounds, PanelTone.Accent);
                DrawTextCenteredFit(entry.Name, bounds,
                    UiTheme.TextPrimary, UiTheme.Label);
            }
        }
    }
}
