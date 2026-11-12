using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Animation;
using SnapClone.UI.Components;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Galeria de desarrollo para validar tipografia, chrome, arte de cartas y
    /// animaciones a 720x1280. Se abre con SNAPCLONE_UI_GALLERY=1 o con el
    /// archivo ui-gallery.flag en el directorio de datos.
    /// </summary>
    public class UiGalleryScene : UiScene
    {
        private enum Tab
        {
            Fonts,
            Chrome,
            Cards,
            Motion,
            Demo
        }

        private const int CardColumns = 4;
        private const int CardRows = 4;
        private const int CardsPerPage = CardColumns * CardRows;

        private readonly List<CardDef> _cards = new List<CardDef>();

        private FontService _fallbackFonts;
        private ContentManager _content;

        private Tab _tab;
        private int _page;
        private int _motionTaps;

        public UiGalleryScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title => "UI GALLERY";
        protected override string Subtitle => "DIAGNOSTICO DE COMPONENTES";
        protected override Color Accent => UiTheme.AccentSoft;
        protected override bool ShowHeader => true;
        protected override bool ShowFooter => false;
        protected override int HeaderDesign => 88;
        protected override int FooterDesign => 0;
        protected override int BackgroundSeed => 9909;

        protected override void OnLoadContent()
        {
            _content = Services.Has<Microsoft.Xna.Framework.Content.ContentManager>()
                ? Services.Get<Microsoft.Xna.Framework.Content.ContentManager>()
                : Services.Get<Game>().Content;

            // Una segunda instancia que omite los TTF para comprobar el camino
            // de fallback con DefaultFont.
            _fallbackFonts = new FontService(_content, true);

            var registry = Services.Get<CardRegistry>();
            _cards.Clear();
            _cards.AddRange(registry.GetDraftable());
            _cards.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
        }

        protected override void OnEnterScene()
        {
            _tab = Tab.Fonts;
            _page = 0;
            _motionTaps = 0;
            for (int i = 0; i < 4; i++)
            {
                Animator.Remove($"motion{i}");
                Animator.AddEntrance($"motion{i}", i, 0.06f, 0.32f);
            }
            AnimateTab();
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle TabsRow
        {
            get
            {
                var inner = ContentInner;
                int height = Math.Max(UiTheme.Px(UiTheme.TouchTarget,
                    Scaffold.UiScale), UiTheme.Px(44, Scaffold.UiScale));
                return new Rectangle(inner.X, inner.Y, inner.Width, height);
            }
        }

        private Rectangle BodyArea
        {
            get
            {
                var inner = ContentInner;
                int y = TabsRow.Bottom + UiTheme.Px(UiTheme.SpaceMd,
                    Scaffold.UiScale);
                return new Rectangle(inner.X, y, inner.Width,
                    Math.Max(0, inner.Bottom - y));
            }
        }

        private Rectangle GetTabBounds(int index)
        {
            var row = TabsRow;
            int gap = UiTheme.Px(UiTheme.SpaceXs, Scaffold.UiScale);
            int width = (row.Width - gap * 4) / 5;
            return new Rectangle(row.X + index * (width + gap), row.Y, width,
                row.Height);
        }

        private Rectangle PagerRow
        {
            get
            {
                var area = BodyArea;
                int height = UiTheme.Px(44, Scaffold.UiScale);
                return new Rectangle(area.X, area.Bottom - height, area.Width,
                    height);
            }
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (Tap("tab_fonts", GetTabBounds(0))) SetTab(Tab.Fonts);
            else if (Tap("tab_chrome", GetTabBounds(1))) SetTab(Tab.Chrome);
            else if (Tap("tab_cards", GetTabBounds(2))) SetTab(Tab.Cards);
            else if (Tap("tab_motion", GetTabBounds(3))) SetTab(Tab.Motion);
            else if (Tap("tab_demo", GetTabBounds(4))) SetTab(Tab.Demo);

            if (_tab == Tab.Cards)
            {
                int pages = PageCount;
                if (Tap("prev", PagerRow, _page > 0) && _page > 0) _page--;
                else if (Tap("next", PagerRow, _page + 1 < pages) &&
                         _page + 1 < pages) _page++;
            }

            if (_tab == Tab.Motion)
            {
                var button = new Rectangle(BodyArea.X + BodyArea.Width / 2 -
                    UiTheme.Px(120, Scaffold.UiScale),
                    BodyArea.Y + UiTheme.Px(150, Scaffold.UiScale),
                    UiTheme.Px(240, Scaffold.UiScale),
                    UiTheme.Px(56, Scaffold.UiScale));
                if (Tap("motion_button", button)) _motionTaps++;
            }

            if (_tab == Tab.Demo) UpdateDemo();
        }

        private void SetTab(Tab tab)
        {
            if (_tab == tab) return;
            _tab = tab;
            AnimateTab();
        }

        private void AnimateTab()
        {
            Animator.Remove("tab");
            Animator.AddEntrance("tab", 0, 0f, 0.22f);
        }

        private void UpdateDemo()
        {
            var area = BodyArea;
            int count = 7;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int top = area.Y + UiTheme.Px(28, Scaffold.UiScale);
            int height = Math.Max(UiTheme.Px(UiTheme.TouchTarget,
                Scaffold.UiScale),
                (area.Bottom - top - gap * (count - 1)) / count);

            string[] screens = { "Menu", "Draft", "Summary", "Collection",
                "LevelUp", "GlobalStore", "Shop" };

            for (int i = 0; i < screens.Length; i++)
            {
                var bounds = new Rectangle(area.X,
                    top + i * (height + gap), area.Width, height);
                if (!Tap($"demo{i}", bounds)) continue;

                UiDemoState.Prepare(Services, screens[i]);
                Scenes.ChangeScene(screens[i]);
                return;
            }
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            DrawTabs(spriteBatch, uiScale);

            switch (_tab)
            {
                case Tab.Fonts:
                    DrawFonts(spriteBatch, uiScale);
                    break;
                case Tab.Chrome:
                    DrawChrome(spriteBatch, uiScale);
                    break;
                case Tab.Cards:
                    DrawCards(spriteBatch, uiScale);
                    break;
                case Tab.Motion:
                    DrawMotion(spriteBatch, uiScale);
                    break;
                case Tab.Demo:
                    DrawDemo(spriteBatch, uiScale);
                    break;
            }
        }

        private void DrawTabs(SpriteBatch spriteBatch, float uiScale)
        {
            DrawTapButtonScaled("tab_fonts", "FUENTES",
                _tab == Tab.Fonts ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.Caption);
            DrawTapButtonScaled("tab_chrome", "CHROME",
                _tab == Tab.Chrome ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.Caption);
            DrawTapButtonScaled("tab_cards", "CARTAS",
                _tab == Tab.Cards ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.Caption);
            DrawTapButtonScaled("tab_motion", "MOTION",
                _tab == Tab.Motion ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.Caption);
            DrawTapButtonScaled("tab_demo", "DEMO",
                _tab == Tab.Demo ? UiTheme.Gold : UiTheme.BorderBright,
                UiTheme.Caption);
        }

        // ------------------------------------------------------------------
        // Fuentes
        // ------------------------------------------------------------------

        private void DrawFonts(SpriteBatch spriteBatch, float uiScale)
        {
            var area = BodyArea;
            float t = Animator.Value("tab", 1f);
            area.Y += (int)((1f - t) * UiTheme.Px(12, uiScale));

            DrawPanel(area, PanelTone.Sunken);

            int pad = UiTheme.Px(UiTheme.SpaceLg, uiScale);
            int y = area.Y + pad;

            DrawSectionLabel(
                $"RUNTIME TTF: {(Ui.Fonts.UsesRuntimeFonts ? "OK" : "FALLBACK")}" +
                $"   ERROR: {Ui.Fonts.LoadError ?? "NINGUNO"}",
                new Rectangle(area.X + pad, y, area.Width - pad * 2,
                    UiTheme.Px(20, uiScale)),
                Ui.Fonts.UsesRuntimeFonts ? UiTheme.Success : UiTheme.DangerSoft,
                UiTheme.Caption);
            y += UiTheme.Px(26, uiScale);

            DrawRoleLine(TextRole.Display, "BEBAS NEUE DISPLAY 44",
                UiTheme.DisplayXl, ref y, area, pad, uiScale);
            DrawRoleLine(TextRole.Ui, "RAJDHANI SEMIBOLD UI 22",
                UiTheme.Label, ref y, area, pad, uiScale);
            DrawRoleLine(TextRole.Body, "Inter Regular body 17: reglas de carta.",
                UiTheme.Body, ref y, area, pad, uiScale);
            DrawRoleLine(TextRole.Numeric, "1234567890", UiTheme.Numeric,
                ref y, area, pad, uiScale);

            y += UiTheme.Px(UiTheme.SpaceMd, uiScale);
            Ui.Chrome.DrawDivider(spriteBatch,
                new Rectangle(area.X + pad, y, area.Width - pad * 2, 2),
                UiTheme.Border, uiScale);
            y += UiTheme.Px(UiTheme.SpaceMd, uiScale);

            DrawSectionLabel(
                $"FALLBACK FORZADO: {(_fallbackFonts.HasFont ? "DEFAULTFONT OK" : "SIN FUENTE")}",
                new Rectangle(area.X + pad, y, area.Width - pad * 2,
                    UiTheme.Px(20, uiScale)), UiTheme.Gold, UiTheme.Caption);
            y += UiTheme.Px(26, uiScale);

            DrawFallbackLine("DISPLAY COMPLETO 44", UiTheme.DisplayXl, ref y,
                area, pad, uiScale);
            DrawFallbackLine("UI SEMIBOLD 22", UiTheme.Label, ref y, area, pad,
                uiScale);
            DrawFallbackLine("CUERPO 17 CON ACENTOS: CORAZON, ENERGIA.", UiTheme.Body,
                ref y, area, pad, uiScale);
            DrawFallbackLine("NUMERICO 1234567890", UiTheme.Numeric, ref y, area,
                pad, uiScale);
        }

        private void DrawRoleLine(TextRole role, string text, float size,
            ref int y, Rectangle area, int pad, float uiScale)
        {
            Ui.Fonts.DrawRole(SpriteBatch, role, text,
                new Vector2(area.X + pad, y), UiTheme.TextPrimary, size,
                uiScale);
            y += (int)(size * uiScale) + UiTheme.Px(UiTheme.SpaceSm, uiScale);
        }

        private void DrawFallbackLine(string text, float size, ref int y,
            Rectangle area, int pad, float uiScale)
        {
            _fallbackFonts.DrawRole(SpriteBatch, TextRole.Ui, text,
                new Vector2(area.X + pad, y), UiTheme.TextSecondary, size,
                uiScale);
            y += (int)(size * uiScale) + UiTheme.Px(UiTheme.SpaceSm, uiScale);
        }

        // ------------------------------------------------------------------
        // Chrome
        // ------------------------------------------------------------------

        private void DrawChrome(SpriteBatch spriteBatch, float uiScale)
        {
            var area = BodyArea;
            int pad = UiTheme.Px(UiTheme.SpaceSm, uiScale);

            int tileWidth = (area.Width - pad * 3) / 4;
            int tileHeight = UiTheme.Px(58, uiScale);

            var tones = (PanelTone[])Enum.GetValues(typeof(PanelTone));
            for (int i = 0; i < tones.Length; i++)
            {
                int row = i / 4;
                int column = i % 4;
                var bounds = new Rectangle(area.X + column * (tileWidth + pad),
                    area.Y + row * (tileHeight + pad), tileWidth, tileHeight);
                DrawPanel(bounds, tones[i]);
                DrawTextCenteredFit(tones[i].ToString().ToUpperInvariant(),
                    bounds, UiTheme.TextPrimary, UiTheme.Caption);
            }

            int y = area.Y + (tileHeight + pad) * 2 +
                    UiTheme.Px(UiTheme.SpaceMd, uiScale);

            DrawSectionLabel("BOTONES: IDLE / HOVER / PRESSED / DISABLED",
                new Rectangle(area.X, y, area.Width, UiTheme.Px(18, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);
            y += UiTheme.Px(24, uiScale);

            int buttonWidth = (area.Width - pad * 3) / 4;
            int buttonHeight = UiTheme.Px(48, uiScale);
            var states = new[]
            {
                ButtonVisualState.Idle, ButtonVisualState.Hover,
                ButtonVisualState.Pressed, ButtonVisualState.Disabled
            };

            for (int i = 0; i < states.Length; i++)
            {
                var bounds = new Rectangle(area.X + i * (buttonWidth + pad), y,
                    buttonWidth, buttonHeight);
                Ui.Chrome.DrawButton(spriteBatch, bounds, states[i].ToString(),
                    states[i], uiScale, UiTheme.Caption, UiTheme.Accent);
            }

            y += buttonHeight + UiTheme.Px(UiTheme.SpaceMd, uiScale);

            var chip = new Rectangle(area.X, y, UiTheme.Px(168, uiScale),
                UiTheme.Px(44, uiScale));
            DrawChip(chip, "P", "1250", "PUNTOS", UiTheme.Gold);

            var tag = new Rectangle(chip.Right + pad, y + UiTheme.Px(8, uiScale),
                UiTheme.Px(130, uiScale), UiTheme.Px(24, uiScale));
            Ui.Chrome.DrawStateTag(spriteBatch, tag, "VICTORIA",
                UiTheme.Success, uiScale);

            y += UiTheme.Px(56, uiScale);

            DrawRibbon(new Rectangle(area.X, y, area.Width,
                UiTheme.Px(36, uiScale)), "CINTA DE TEMA", UiTheme.Violet);
            y += UiTheme.Px(46, uiScale);

            var badgeCenter = new Vector2(area.X + UiTheme.Px(24, uiScale),
                y + UiTheme.Px(20, uiScale));
            Ui.Chrome.DrawBadge(spriteBatch, badgeCenter,
                UiTheme.Px(18, uiScale), "7", UiTheme.PanelSunken,
                UiTheme.Gold, uiScale);

            var progress = new Rectangle(area.X + UiTheme.Px(56, uiScale), y,
                area.Width - UiTheme.Px(56, uiScale), UiTheme.Px(22, uiScale));
            Ui.Chrome.DrawProgressSlots(spriteBatch, progress, 8, 5, 5,
                UiTheme.Gold, uiScale);

            y += UiTheme.Px(42, uiScale);

            DrawSectionLabel("OVERLAY (PULSA PARA VER DIM + VIGNETTE)",
                new Rectangle(area.X, y, area.Width, UiTheme.Px(18, uiScale)),
                UiTheme.TextMuted, UiTheme.Caption);
            y += UiTheme.Px(24, uiScale);

            var overlaySample = new Rectangle(area.X, y, area.Width,
                Math.Max(0, area.Bottom - y));
            if (overlaySample.Height > UiTheme.Px(30, uiScale))
                Ui.Chrome.DrawOverlay(spriteBatch, overlaySample, 0.45f);
        }

        // ------------------------------------------------------------------
        // Cartas
        // ------------------------------------------------------------------

        private int PageCount => Math.Max(1,
            (_cards.Count + CardsPerPage - 1) / CardsPerPage);

        private Rectangle GetCardBounds(int index)
        {
            var area = BodyArea;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (area.Width - gap * (CardColumns - 1)) / CardColumns;
            int maxHeight = (PagerRow.Y - UiTheme.Px(UiTheme.SpaceSm,
                Scaffold.UiScale) - area.Y - gap * (CardRows - 1)) / CardRows;
            int height = Math.Min((int)(width * 1.42f), maxHeight);
            int row = index / CardColumns;
            int column = index % CardColumns;
            return new Rectangle(area.X + column * (width + gap),
                area.Y + row * (height + gap), width, height);
        }

        private void DrawCards(SpriteBatch spriteBatch, float uiScale)
        {
            int start = _page * CardsPerPage;
            int end = Math.Min(start + CardsPerPage, _cards.Count);

            for (int i = start; i < end; i++)
            {
                DrawCardDefinition(GetCardBounds(i - start), _cards[i],
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.Collection,
                        ShowName = true
                    });
            }

            DrawSectionLabel(
                $"ARTE PROCEDURAL {_cards.Count} CARTAS   PAGINA {_page + 1}/{PageCount}",
                new Rectangle(BodyArea.X, PagerRow.Y - UiTheme.Px(26,
                    uiScale), BodyArea.Width, UiTheme.Px(20, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);

            DrawTapButtonScaled("prev", "ANTERIOR",
                _page > 0 ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);
            DrawTapButtonScaled("next", "SIGUIENTE",
                _page + 1 < PageCount ? UiTheme.AccentSoft : UiTheme.Border,
                UiTheme.Caption);
        }

        // ------------------------------------------------------------------
        // Movimiento
        // ------------------------------------------------------------------

        private void DrawMotion(SpriteBatch spriteBatch, float uiScale)
        {
            var area = BodyArea;
            int pad = UiTheme.Px(UiTheme.SpaceMd, uiScale);

            DrawSectionLabel("ENTRADA ESCALONADA + PULSO + RELOJ", 
                new Rectangle(area.X, area.Y, area.Width,
                    UiTheme.Px(20, uiScale)), UiTheme.TextSecondary,
                UiTheme.Caption);

            int tileWidth = (area.Width - pad * 3) / 4;
            int tileHeight = UiTheme.Px(84, uiScale);

            for (int i = 0; i < 4; i++)
            {
                float value = Animator.Value($"motion{i}", 1f);
                var bounds = new Rectangle(area.X + i * (tileWidth + pad),
                    area.Y + UiTheme.Px(28, uiScale) + (int)((1f - value) *
                    UiTheme.Px(18, uiScale)), tileWidth, tileHeight);

                DrawPanel(bounds, PanelTone.Raised);
                DrawTextCenteredFit($"{value:0.00}", bounds,
                    UiTheme.TextPrimary, UiTheme.NumericSmall);
            }

            float pulse = 0.5f + 0.5f * MathF.Sin(Elapsed * 3f);
            var glowCenter = new Vector2(area.Center.X,
                area.Y + UiTheme.Px(170, uiScale));
            Ui.Chrome.DrawGlow(spriteBatch, glowCenter,
                UiTheme.Px(70, uiScale) * (0.7f + pulse * 0.3f),
                UiTheme.Violet * (0.25f + pulse * 0.35f));

            DrawTapButtonScaled("motion_button",
                $"TOCA EL BOTON ({_motionTaps})", UiTheme.Accent,
                UiTheme.Label);

            DrawCurves(spriteBatch, uiScale);
        }

        private void DrawCurves(SpriteBatch spriteBatch, float uiScale)
        {
            var area = BodyArea;
            var graph = new Rectangle(area.X,
                area.Y + UiTheme.Px(250, uiScale), area.Width,
                Math.Max(0, area.Bottom - (area.Y + UiTheme.Px(250, uiScale))));

            if (graph.Height < UiTheme.Px(60, uiScale)) return;

            DrawSectionLabel("CURVAS: LINEAR / OUT CUBIC / OUT BACK / IN OUT QUAD",
                new Rectangle(graph.X, graph.Y, graph.Width,
                    UiTheme.Px(18, uiScale)), UiTheme.TextMuted,
                UiTheme.Caption);

            var plot = new Rectangle(graph.X, graph.Y + UiTheme.Px(24, uiScale),
                graph.Width, Math.Max(0, graph.Height - UiTheme.Px(28,
                    uiScale)));

            if (plot.Height < 20) return;

            var kinds = new[]
            {
                EasingKind.Linear, EasingKind.EaseOutCubic,
                EasingKind.EaseOutBack, EasingKind.EaseInOutQuad
            };
            var colors = new[]
            {
                UiTheme.TextMuted, UiTheme.AccentSoft, UiTheme.Gold,
                UiTheme.Perk
            };

            for (int i = 0; i < kinds.Length; i++)
            {
                DrawCurve(spriteBatch, plot, kinds[i], colors[i], i);
            }
        }

        private void DrawCurve(SpriteBatch spriteBatch, Rectangle plot,
            EasingKind kind, Color color, int offset)
        {
            const int samples = 48;
            int thickness = Math.Max(1, UiTheme.Px(2, Scaffold.UiScale));
            int yOffset = offset * UiTheme.Px(3, Scaffold.UiScale);

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)(samples - 1);
                float value = Easing.Evaluate(kind, t);
                int x = plot.X + (int)(t * plot.Width);
                int y = plot.Bottom - (int)(value * (plot.Height -
                    UiTheme.Px(10, Scaffold.UiScale))) - yOffset;

                spriteBatch.Draw(Ui.Textures.Pixel,
                    new Rectangle(x, MathHelper.Clamp(y, plot.Y, plot.Bottom),
                        thickness, thickness), color);
            }
        }

        // ------------------------------------------------------------------
        // Demo de pantallas
        // ------------------------------------------------------------------

        private void DrawDemo(SpriteBatch spriteBatch, float uiScale)
        {
            DrawSectionLabel("ABRIR PANTALLA CON ESTADO DE DEMOSTRACION",
                new Rectangle(BodyArea.X, BodyArea.Y, BodyArea.Width,
                    UiTheme.Px(20, uiScale)), UiTheme.TextSecondary,
                UiTheme.Caption);

            var area = BodyArea;
            int count = 7;
            int gap = UiTheme.Px(UiTheme.SpaceSm, uiScale);
            int top = area.Y + UiTheme.Px(28, uiScale);
            int height = Math.Max(UiTheme.Px(UiTheme.TouchTarget, uiScale),
                (area.Bottom - top - gap * (count - 1)) / count);

            string[] screens = { "MENU", "DRAFT", "SUMMARY", "COLLECTION",
                "LEVEL UP", "GLOBAL STORE", "SHOP" };

            for (int i = 0; i < screens.Length; i++)
            {
                var bounds = new Rectangle(area.X,
                    top + i * (height + gap), area.Width, height);
                DrawTapButtonScaled($"demo{i}", screens[i],
                    i % 2 == 0 ? UiTheme.AccentSoft : UiTheme.Violet,
                    UiTheme.LabelSmall);
            }
        }
    }
}
