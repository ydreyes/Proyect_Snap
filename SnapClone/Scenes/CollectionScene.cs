using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Meta;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Coleccion de 217 cartas y de los 28 perks. Grilla de cuatro columnas con
    /// arte, estados, filtros, paginado y overlay de detalle. Es solo lectura.
    /// </summary>
    public class CollectionScene : UiScene
    {
        private const int Columns = 4;
        private const int RowsPerPage = 3;
        private const int PageSize = Columns * RowsPerPage;
        private const int PerkColumns = 2;
        private const int PerkRowsPerPage = 3;
        private const int PerkPageSize = PerkColumns * PerkRowsPerPage;

        private enum Tab
        {
            Cards,
            Perks
        }

        private CollectionSystem _collection;
        private RunCheckpointService _checkpoint;
        private RunState _runState;
        private ProfileDto _profile;

        private readonly CollectionFilter _filter = new CollectionFilter();
        private readonly List<CollectionCardEntry> _cards =
            new List<CollectionCardEntry>();
        private readonly List<CollectionPerkEntry> _perks =
            new List<CollectionPerkEntry>();

        private Tab _tab;
        private int _page;
        private int _perkPage;
        private CollectionCardEntry _selected;
        private CollectionPerkEntry _selectedPerk;

        public CollectionScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title => "COLECCION";
        protected override string Subtitle => "CATALOGO DE CARTAS Y PERKS";
        protected override Color Accent => UiTheme.Gold;
        protected override string CurrencyIcon => "P";
        protected override string CurrencyValue => (_profile?.Points ?? 0).ToString();
        protected override string CurrencyLabel => "PUNTOS";
        protected override int BackgroundSeed => 4404;

        protected override void OnEnterScene()
        {
            _collection = Services.Get<CollectionSystem>();
            _checkpoint = Services.Get<RunCheckpointService>();
            _runState = Services.Has<RunState>() ? Services.Get<RunState>() : null;
            _profile = _checkpoint.Profile;

            _tab = Tab.Cards;
            _page = 0;
            _perkPage = 0;
            _selected = null;
            _selectedPerk = null;
            _filter.Clear();
            Refresh();
        }

        private void Refresh()
        {
            _cards.Clear();
            _cards.AddRange(_collection.BuildCards(_profile, _runState));

            _perks.Clear();
            _perks.AddRange(_collection.BuildPerks(_profile, _runState));
        }

        private List<CollectionCardEntry> GetFilteredCards()
        {
            if (_filter.IsClear) return _cards;
            return _cards.Where(entry => CollectionSystem.Matches(entry, _filter))
                .ToList();
        }

        private int CardPageCount(List<CollectionCardEntry> filtered) =>
            Math.Max(1, (filtered.Count + PageSize - 1) / PageSize);

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

        private Rectangle FiltersRow
        {
            get
            {
                var inner = ContentInner;
                int y = TabsRow.Bottom + UiTheme.Px(UiTheme.SpaceSm,
                    Scaffold.UiScale);
                return new Rectangle(inner.X, y, inner.Width,
                    UiTheme.Px(40, Scaffold.UiScale));
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

        private Rectangle GridArea
        {
            get
            {
                var inner = ContentInner;
                int top = (_tab == Tab.Cards
                    ? FiltersRow.Bottom
                    : TabsRow.Bottom) + UiTheme.Px(UiTheme.SpaceMd,
                    Scaffold.UiScale);
                int bottom = PagerRow.Y - UiTheme.Px(UiTheme.SpaceSm,
                    Scaffold.UiScale);
                return new Rectangle(inner.X, top, inner.Width,
                    Math.Max(0, bottom - top));
            }
        }

        private Rectangle GetCardsTab()
        {
            var row = TabsRow;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (row.Width - gap) / 2;
            return new Rectangle(row.X, row.Y, width, row.Height);
        }

        private Rectangle GetPerksTab()
        {
            var row = TabsRow;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (row.Width - gap) / 2;
            return new Rectangle(row.X + width + gap, row.Y, width, row.Height);
        }

        private Rectangle GetFilterBounds(int index)
        {
            var row = FiltersRow;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (row.Width - gap * 2) / 3;
            return new Rectangle(row.X + index * (width + gap), row.Y, width,
                row.Height);
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

        private Rectangle GetCardBounds(int index)
        {
            var area = GridArea;
            int gapX = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int gapY = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (area.Width - gapX * (Columns - 1)) / Columns;
            int maxHeight = (area.Height - gapY * (RowsPerPage - 1)) /
                            RowsPerPage;
            int height = Math.Min((int)(width * 1.42f), maxHeight);
            int startY = area.Y;
            int row = index / Columns;
            int column = index % Columns;
            return new Rectangle(area.X + column * (width + gapX),
                startY + row * (height + gapY), width, height);
        }

        private int PerkPageCount =>
            Math.Max(1, (_perks.Count + PerkPageSize - 1) / PerkPageSize);

        private Rectangle GetPerkBounds(int index)
        {
            var area = GridArea;
            int gapX = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int gapY = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int top = area.Y + UiTheme.Px(28, Scaffold.UiScale);
            int width = (area.Width - gapX * (PerkColumns - 1)) / PerkColumns;
            int height = System.Math.Max(0,
                (area.Bottom - top - gapY * (PerkRowsPerPage - 1)) /
                PerkRowsPerPage);
            int row = index / PerkColumns;
            int column = index % PerkColumns;
            return new Rectangle(area.X + column * (width + gapX),
                top + row * (height + gapY), width, height);
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

        private Rectangle DetailPanel
        {
            get
            {
                var content = Scaffold.Content;
                int width = (int)(content.Width * 0.86f);
                int height = (int)(content.Height * 0.84f);
                return new Rectangle((content.Width - width) / 2,
                    content.Y + (content.Height - height) / 2, width, height);
            }
        }

        private Rectangle DetailCloseBounds
        {
            get
            {
                var panel = DetailPanel;
                int pad = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
                int height = UiTheme.Px(50, Scaffold.UiScale);
                return new Rectangle(panel.X + pad,
                    panel.Bottom - pad - height,
                    panel.Width - pad * 2, height);
            }
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (_selected != null)
            {
                if (Tap("detail_close", DetailCloseBounds))
                    _selected = null;
                return;
            }

            if (_selectedPerk != null)
            {
                if (Tap("perk_close", DetailCloseBounds))
                    _selectedPerk = null;
                return;
            }

            if (Tap("back", BackBounds))
            {
                Scenes.ChangeScene("Menu");
                return;
            }

            if (Tap("tab_cards", GetCardsTab()))
            {
                _tab = Tab.Cards;
                _page = 0;
                return;
            }

            if (Tap("tab_perks", GetPerksTab()))
            {
                _tab = Tab.Perks;
                _page = 0;
                _perkPage = 0;
                return;
            }

            if (_tab == Tab.Cards)
            {
                if (Tap("filter_state", GetFilterBounds(0)))
                {
                    CycleStateFilter();
                    return;
                }

                if (Tap("filter_theme", GetFilterBounds(1)))
                {
                    CycleThemeFilter();
                    return;
                }

                if (Tap("filter_cost", GetFilterBounds(2)))
                {
                    CycleCostFilter();
                    return;
                }

                var filtered = GetFilteredCards();
                int pages = CardPageCount(filtered);
                if (_page >= pages) _page = pages - 1;

                bool canPrevious = _page > 0;
                bool canNext = _page + 1 < pages;

                if (Tap("previous", GetPreviousButton(), canPrevious) &&
                    canPrevious)
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
                    if (!Tap($"cell{i - start}", GetCardBounds(i - start)))
                        continue;

                    _selected = filtered[i];
                    return;
                }
            }
            else
            {
                int pages = PerkPageCount;
                if (_perkPage >= pages) _perkPage = pages - 1;

                bool canPrevious = _perkPage > 0;
                bool canNext = _perkPage + 1 < pages;

                if (Tap("previous", GetPreviousButton(), canPrevious) &&
                    canPrevious)
                {
                    _perkPage--;
                    return;
                }

                if (Tap("next", GetNextButton(), canNext) && canNext)
                {
                    _perkPage++;
                    return;
                }

                int start = _perkPage * PerkPageSize;
                int end = Math.Min(start + PerkPageSize, _perks.Count);
                for (int i = start; i < end; i++)
                {
                    if (!Tap($"perk{i - start}", GetPerkBounds(i - start)))
                        continue;

                    _selectedPerk = _perks[i];
                    return;
                }
            }
        }

        private void CycleStateFilter()
        {
            if (!_filter.State.HasValue)
                _filter.State = CollectionCardState.Owned;
            else if (_filter.State == CollectionCardState.Owned)
                _filter.State = CollectionCardState.Discovered;
            else if (_filter.State == CollectionCardState.Discovered)
                _filter.State = CollectionCardState.InStore;
            else if (_filter.State == CollectionCardState.InStore)
                _filter.State = CollectionCardState.Hidden;
            else
                _filter.State = null;

            _page = 0;
        }

        private void CycleThemeFilter()
        {
            if (!_filter.Theme.HasValue)
                _filter.Theme = DraftTheme.Control;
            else if (_filter.Theme == DraftTheme.Control)
                _filter.Theme = DraftTheme.Destroy;
            else if (_filter.Theme == DraftTheme.Destroy)
                _filter.Theme = DraftTheme.OnReveal;
            else if (_filter.Theme == DraftTheme.OnReveal)
                _filter.Theme = DraftTheme.Neutral;
            else
                _filter.Theme = null;

            _page = 0;
        }

        private void CycleCostFilter()
        {
            if (!_filter.Cost.HasValue)
                _filter.Cost = 1;
            else if (_filter.Cost.Value < 6)
                _filter.Cost = _filter.Cost.Value + 1;
            else
                _filter.Cost = null;

            _page = 0;
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            DrawTabs(spriteBatch, uiScale);

            if (_tab == Tab.Cards)
                DrawCardsTab(spriteBatch, uiScale);
            else
                DrawPerksTab(spriteBatch, uiScale);

            DrawTapButtonScaled("back", "VOLVER AL MENU", UiTheme.AccentSoft,
                UiTheme.LabelSmall);

            if (_selected != null)
                DrawDetail(spriteBatch, uiScale);
            else if (_selectedPerk != null)
                DrawPerkDetail(spriteBatch, uiScale);
        }

        private void DrawTabs(SpriteBatch spriteBatch, float uiScale)
        {
            DrawTapButtonScaled("tab_cards", "CARTAS",
                _tab == Tab.Cards ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.LabelSmall);
            DrawTapButtonScaled("tab_perks", "PERKS",
                _tab == Tab.Perks ? UiTheme.Perk : UiTheme.BorderBright,
                UiTheme.LabelSmall);
        }

        private void DrawCardsTab(SpriteBatch spriteBatch, float uiScale)
        {
            DrawTapButton("filter_state",
                CollectionSystem.FilterStateLabel(_filter.State),
                _filter.State.HasValue ? UiTheme.Accent : UiTheme.BorderBright,
                UiTheme.Caption);
            DrawTapButton("filter_theme",
                CollectionSystem.ThemeFilterLabel(_filter.Theme),
                _filter.Theme.HasValue ? UiTheme.Violet : UiTheme.BorderBright,
                UiTheme.Caption);
            DrawTapButton("filter_cost",
                CollectionSystem.CostFilterLabel(_filter.Cost),
                _filter.Cost.HasValue ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.Caption);

            var filtered = GetFilteredCards();
            int pages = CardPageCount(filtered);
            if (_page >= pages) _page = pages - 1;

            int start = _page * PageSize;
            int end = Math.Min(start + PageSize, filtered.Count);

            for (int i = start; i < end; i++)
                DrawCardCell(spriteBatch, filtered[i], GetCardBounds(i - start));

            if (filtered.Count == 0)
            {
                var area = GridArea;
                DrawTextCenteredFit("NO HAY CARTAS CON ESOS FILTROS",
                    new Rectangle(area.X, area.Y + area.Height / 2,
                        area.Width, UiTheme.Px(30, uiScale)),
                    UiTheme.TextMuted, UiTheme.LabelSmall);
            }

            bool canPrevious = _page > 0;
            bool canNext = _page + 1 < pages;
            DrawTapButtonScaled("previous", "ANTERIOR",
                canPrevious ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);
            DrawTapButtonScaled("next", "SIGUIENTE",
                canNext ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);

            DrawTextCenteredFit(
                $"PAGINA {_page + 1}/{pages}   {filtered.Count}/{_cards.Count} CARTAS",
                new Rectangle(PagerRow.X + (int)(PagerRow.Width * 0.28f),
                    PagerRow.Y + UiTheme.Px(12, uiScale),
                    (int)(PagerRow.Width * 0.44f),
                    UiTheme.Px(22, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);
        }

        private void DrawCardCell(SpriteBatch spriteBatch, CollectionCardEntry entry,
            Rectangle bounds)
        {
            var options = new CardRenderOptions
            {
                Variant = CardFrameVariant.Collection,
                ShowPower = entry.State != CollectionCardState.Hidden,
                ShowName = entry.State != CollectionCardState.Hidden,
                Tag = entry.InRunDeck
                    ? "EN RUN DECK"
                    : CollectionSystem.StateLabel(entry.State)
            };

            switch (entry.State)
            {
                case CollectionCardState.Hidden:
                    options.State = CardVisualState.Undiscovered;
                    break;
                case CollectionCardState.Owned:
                    options.State = CardVisualState.Owned;
                    break;
                case CollectionCardState.InStore:
                    options.State = CardVisualState.Normal;
                    break;
                default:
                    options.State = CardVisualState.Normal;
                    break;
            }

            if (entry.State == CollectionCardState.Discovered)
                options.Opacity = 0.92f;

            DrawCardDefinition(bounds, entry.Card, options);

            if (entry.InRunDeck)
            {
                var tag = new Rectangle(bounds.X + 2,
                    bounds.Bottom - UiTheme.Px(18, Scaffold.UiScale),
                    bounds.Width - 4, UiTheme.Px(16, Scaffold.UiScale));
                Ui.Chrome.DrawStateTag(spriteBatch, tag, "EN RUN DECK",
                    UiTheme.Gold, Scaffold.UiScale);
            }
        }

        private void DrawPerksTab(SpriteBatch spriteBatch, float uiScale)
        {
            int pages = PerkPageCount;
            if (_perkPage >= pages) _perkPage = pages - 1;

            DrawTextCenteredFit(
                "28 PERKS DE HABILIDAD: SE ADJUNTAN A UNA CARTA DEL MAZO",
                new Rectangle(GridArea.X, GridArea.Y,
                    GridArea.Width, UiTheme.Px(22, uiScale)),
                UiTheme.TextMuted, UiTheme.Caption);

            int start = _perkPage * PerkPageSize;
            int end = Math.Min(start + PerkPageSize, _perks.Count);

            for (int index = start; index < end; index++)
            {
                var entry = _perks[index];
                var bounds = GetPerkBounds(index - start);
                var tone = entry.Owned ? PanelTone.Perk : PanelTone.Sunken;

                DrawPanel(bounds, tone);
                int pad = UiTheme.Px(UiTheme.SpaceMd, uiScale);

                // Cada tarjeta muestra nombre, texto y requisito de anfitrion.
                DrawSectionLabel(entry.Perk?.Name ?? "PERK",
                    new Rectangle(bounds.X + pad,
                        bounds.Y + UiTheme.Px(UiTheme.SpaceSm, uiScale),
                        bounds.Width - pad * 2,
                        UiTheme.Px(20, uiScale)),
                    entry.Owned ? UiTheme.Perk : UiTheme.TextMuted,
                    UiTheme.LabelSmall);

                DrawWrapped(entry.Perk?.RuleText,
                    new Rectangle(bounds.X + pad,
                        bounds.Y + UiTheme.Px(24, uiScale),
                        bounds.Width - pad * 2,
                        Math.Max(0, bounds.Height - UiTheme.Px(62, uiScale))),
                    UiTheme.TextSecondary, UiTheme.Caption);

                DrawTextCenteredFit(entry.FooterText,
                    new Rectangle(bounds.X + pad,
                        bounds.Bottom - UiTheme.Px(34, uiScale),
                        bounds.Width - pad * 2,
                        UiTheme.Px(16, uiScale)),
                    entry.HasCompatibleHost ? UiTheme.Gold : UiTheme.TextMuted,
                    UiTheme.Caption);

                if (entry.Perk != null && entry.Perk.HasHostRequirement)
                {
                    DrawTextCenteredFit(entry.Perk.CompatibilityText,
                        new Rectangle(bounds.X + pad,
                            bounds.Bottom - UiTheme.Px(54, uiScale),
                            bounds.Width - pad * 2,
                            UiTheme.Px(14, uiScale)),
                        UiTheme.TextMuted, UiTheme.Caption);
                }
            }

            bool canPrevious = _perkPage > 0;
            bool canNext = _perkPage + 1 < pages;
            DrawTapButtonScaled("previous", "ANTERIOR",
                canPrevious ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);
            DrawTapButtonScaled("next", "SIGUIENTE",
                canNext ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);

            DrawTextCenteredFit(
                $"PAGINA {_perkPage + 1}/{pages}   {_perks.Count} PERKS",
                new Rectangle(PagerRow.X + (int)(PagerRow.Width * 0.28f),
                    PagerRow.Y + UiTheme.Px(12, uiScale),
                    (int)(PagerRow.Width * 0.44f),
                    UiTheme.Px(22, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);
        }

        private void DrawDetail(SpriteBatch spriteBatch, float uiScale)
        {
            var screen = new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Ui.Chrome.DrawOverlay(spriteBatch, screen, 0.76f);

            var panel = DetailPanel;
            DrawPanel(panel, PanelTone.Raised, false);
            int pad = UiTheme.Px(UiTheme.SpaceXl, uiScale);

            bool hidden = _selected.State == CollectionCardState.Hidden;

            var cardBounds = new Rectangle(panel.Center.X -
                (int)(panel.Width * 0.19f), panel.Y + pad,
                (int)(panel.Width * 0.38f),
                (int)(panel.Height * 0.44f));

            var options = new CardRenderOptions
            {
                Variant = CardFrameVariant.Collection,
                ShowPower = !hidden,
                ShowName = !hidden,
                Tag = _selected.InRunDeck
                    ? "EN RUN DECK"
                    : CollectionSystem.StateLabel(_selected.State)
            };
            if (hidden) options.State = CardVisualState.Undiscovered;
            else if (_selected.State == CollectionCardState.Owned)
                options.State = CardVisualState.Owned;

            DrawCardDefinition(cardBounds, _selected.Card, options);

            int textY = cardBounds.Bottom + UiTheme.Px(UiTheme.SpaceMd, uiScale);
            int textHeight = panel.Bottom - textY -
                UiTheme.Px(70, uiScale);

            string name = hidden ? "CARTA OCULTA" : _selected.Card?.Name;
            DrawTextCenteredFit(name,
                new Rectangle(panel.X + pad, textY,
                    panel.Width - pad * 2, UiTheme.Px(30, uiScale)),
                UiTheme.TextPrimary, UiTheme.Heading);

            string state = CollectionSystem.StateLabel(_selected.State);
            DrawTextCenteredFit(
                $"{state}{(_selected.InRunDeck ? "   EN RUN DECK" : string.Empty)}",
                new Rectangle(panel.X + pad, textY +
                    UiTheme.Px(34, uiScale), panel.Width - pad * 2,
                    UiTheme.Px(20, uiScale)),
                _selected.State == CollectionCardState.Owned
                    ? UiTheme.Gold
                    : UiTheme.TextSecondary,
                UiTheme.Caption);

            string obtain = hidden
                ? "DESCUBRELA EN LA PARTIDA"
                : _selected.ObtainPath;
            DrawTextCenteredFit(obtain,
                new Rectangle(panel.X + pad, textY +
                    UiTheme.Px(58, uiScale), panel.Width - pad * 2,
                    UiTheme.Px(20, uiScale)),
                UiTheme.Gold, UiTheme.Caption);

            if (!hidden)
            {
                var rules = new Rectangle(panel.X + pad, textY +
                    UiTheme.Px(84, uiScale), panel.Width - pad * 2,
                    Math.Max(0, textHeight - UiTheme.Px(84, uiScale)));
                DrawWrapped(_selected.Card?.Description, rules,
                    UiTheme.TextSecondary, UiTheme.Body);

                DrawTextCenteredFit(
                    $"COSTE {_selected.Card?.Cost ?? 0}   " +
                    $"PODER {_selected.Card?.BasePower ?? 0}",
                    new Rectangle(panel.X + pad, rules.Bottom, rules.Width,
                        UiTheme.Px(20, uiScale)),
                    UiTheme.TextMuted, UiTheme.Caption);
            }

            DrawTapButtonScaled("detail_close", "CERRAR",
                UiTheme.AccentSoft, UiTheme.LabelSmall);
        }

        private void DrawPerkDetail(SpriteBatch spriteBatch, float uiScale)
        {
            var screen = new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Ui.Chrome.DrawOverlay(spriteBatch, screen, 0.76f);

            var panel = DetailPanel;
            DrawPanel(panel, _selectedPerk.Owned
                ? PanelTone.Perk
                : PanelTone.Raised, false);

            int pad = UiTheme.Px(UiTheme.SpaceXl, uiScale);
            var inner = Inset(panel, pad);

            DrawDisplayCentered(_selectedPerk.Perk?.Name ?? "PERK",
                new Rectangle(inner.X, inner.Y, inner.Width,
                    UiTheme.Px(48, uiScale)),
                _selectedPerk.Owned ? UiTheme.Perk : UiTheme.TextPrimary,
                UiTheme.Heading);

            var ruleBounds = new Rectangle(inner.X,
                inner.Y + UiTheme.Px(58, uiScale), inner.Width,
                (int)(inner.Height * 0.42f));
            DrawWrapped(_selectedPerk.Perk?.RuleText, ruleBounds,
                UiTheme.TextPrimary, UiTheme.Body);

            string state = _selectedPerk.Owned
                ? $"EN COLECCION   COPIAS {_selectedPerk.OwnedCopies}   " +
                  $"ADJUNTOS {_selectedPerk.AttachedCopies}"
                : "NO OBTENIDO";
            DrawTextCenteredFit(state,
                new Rectangle(inner.X, ruleBounds.Bottom +
                    UiTheme.Px(UiTheme.SpaceMd, uiScale), inner.Width,
                    UiTheme.Px(22, uiScale)),
                _selectedPerk.Owned ? UiTheme.Perk : UiTheme.TextMuted,
                UiTheme.Caption);

            DrawTextCenteredFit(_selectedPerk.TriggerText,
                new Rectangle(inner.X, ruleBounds.Bottom +
                    UiTheme.Px(UiTheme.SpaceMd + 26, uiScale), inner.Width,
                    UiTheme.Px(22, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);

            DrawTextCenteredFit(_selectedPerk.CompatibilityText,
                new Rectangle(inner.X, ruleBounds.Bottom +
                    UiTheme.Px(UiTheme.SpaceMd + 52, uiScale), inner.Width,
                    UiTheme.Px(22, uiScale)),
                _selectedPerk.HasCompatibleHost
                    ? UiTheme.Gold
                    : UiTheme.TextMuted,
                UiTheme.Caption);

            DrawTextCenteredFit(_selectedPerk.HasCompatibleHost
                    ? "TIENE ANFITRION EN EL MAZO ACTUAL"
                    : "SIN ANFITRION COMPATIBLE EN EL MAZO ACTUAL",
                new Rectangle(inner.X, ruleBounds.Bottom +
                    UiTheme.Px(UiTheme.SpaceMd + 78, uiScale), inner.Width,
                    UiTheme.Px(22, uiScale)),
                _selectedPerk.HasCompatibleHost
                    ? UiTheme.Success
                    : UiTheme.TextMuted,
                UiTheme.Caption);

            DrawTapButtonScaled("perk_close", "CERRAR",
                UiTheme.AccentSoft, UiTheme.LabelSmall);
        }
    }
}
