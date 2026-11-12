using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Menu principal como hub de run: reactor con estado, chip de puntos,
    /// un slot de accion primaria y navegacion inferior.
    /// </summary>
    public class MenuScene : UiScene
    {
        private RunCheckpointService _checkpoint;
        private RunState _runState;
        private RunDirector _runDirector;

        private bool _confirmNewRun;
        private int _cardCount;
        private int _perkCount;

        public MenuScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title => "SNAPCLONE";
        protected override string Subtitle => "MARVEL SNAP STYLE";
        protected override Color Accent => UiTheme.Gold;
        protected override string CurrencyIcon => "P";
        protected override string CurrencyValue =>
            (_checkpoint?.Profile?.Points ?? 0).ToString();
        protected override string CurrencyLabel => "PUNTOS";
        protected override int BackgroundSeed => 1101;

        protected override void OnEnterScene()
        {
            _checkpoint = Services.Get<RunCheckpointService>();
            _runState = Services.Get<RunState>();
            _runDirector = Services.Get<RunDirector>();
            _checkpoint.RefreshRun();
            _cardCount = Services.Has<CardRegistry>()
                ? Services.Get<CardRegistry>().GetDraftable().Count
                : 0;
            _perkCount = Services.Has<PerkRegistry>()
                ? Services.Get<PerkRegistry>().GetAll().Count
                : 0;
            _confirmNewRun = false;
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle PanelBounds
        {
            get
            {
                var inner = ContentInner;
                int height = (int)(inner.Height * 0.66f);
                return new Rectangle(inner.X, inner.Y, inner.Width, height);
            }
        }

        private Rectangle PrimaryBounds
        {
            get
            {
                var inner = ContentInner;
                int height = UiTheme.Px(64, Scaffold.UiScale);
                return new Rectangle(inner.X, inner.Bottom - height,
                    inner.Width, height);
            }
        }

        private Rectangle ConfirmYesBounds
        {
            get
            {
                var panel = ConfirmPanelBounds;
                int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
                int width = (panel.Width - gap - UiTheme.Px(UiTheme.SpaceXl,
                    Scaffold.UiScale)) / 2;
                int height = UiTheme.Px(56, Scaffold.UiScale);
                return new Rectangle(panel.X + UiTheme.Px(UiTheme.SpaceXl,
                    Scaffold.UiScale), panel.Bottom - height -
                    UiTheme.Px(UiTheme.SpaceXl, Scaffold.UiScale), width, height);
            }
        }

        private Rectangle ConfirmNoBounds
        {
            get
            {
                var yes = ConfirmYesBounds;
                int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
                return new Rectangle(yes.Right + gap, yes.Y, yes.Width,
                    yes.Height);
            }
        }

        private Rectangle ConfirmPanelBounds
        {
            get
            {
                var content = Scaffold.Content;
                int width = (int)(content.Width * 0.86f);
                int height = (int)(content.Height * 0.40f);
                return new Rectangle((content.Width - width) / 2,
                    content.Y + (content.Height - height) / 2, width, height);
            }
        }

        private Rectangle NavBounds(int index)
        {
            var footer = Scaffold.Footer;
            int pad = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (footer.Width - pad * 2 - gap * 3) / 4;
            int height = Math.Max(UiTheme.Px(UiTheme.TouchTarget, Scaffold.UiScale),
                UiTheme.Px(48, Scaffold.UiScale));
            int y = footer.Y + (footer.Height - height) / 2;
            return new Rectangle(footer.X + pad + index * (width + gap), y,
                width, height);
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (_confirmNewRun)
            {
                if (Tap("confirm_yes", ConfirmYesBounds))
                {
                    _confirmNewRun = false;
                    StartNewRun();
                }
                else if (Tap("confirm_no", ConfirmNoBounds))
                {
                    _confirmNewRun = false;
                }

                return;
            }

            if (Tap("primary", PrimaryBounds))
            {
                if (_checkpoint.HasActiveRun)
                    ContinueRun();
                else if (_checkpoint.HasEndedRun)
                    ViewSummary();
                else
                    StartNewRun();
                return;
            }

            if (Tap("nav_new", NavBounds(2)))
            {
                RequestNewRun();
                return;
            }

            if (Tap("nav_store", NavBounds(1)))
            {
                Scenes.ChangeScene("GlobalStore");
                return;
            }

            if (Tap("nav_collection", NavBounds(0)))
            {
                Scenes.ChangeScene("Collection");
                return;
            }

            if (Tap("nav_quit", NavBounds(3)))
            {
                var game = Services.Get<Game>();
                if (game is Game1 game1)
                    game1.RequestAppExit();
                else
                    game.Exit();
            }
        }

        private void ContinueRun()
        {
            if (!_checkpoint.RestoreRun()) return;

            if (_runState.PendingNode == RunNode.Combat &&
                _runState.PendingCombat != null)
                _checkpoint.RestoreCombatForResume();

            Scenes.ChangeScene(_runDirector.GetNextScene());
        }

        private void ViewSummary()
        {
            if (!_checkpoint.RestoreRun()) return;
            Scenes.ChangeScene("Summary");
        }

        private void RequestNewRun()
        {
            if (_checkpoint.HasActiveRun)
            {
                _confirmNewRun = true;
                return;
            }

            StartNewRun();
        }

        private void StartNewRun()
        {
            _checkpoint.DeleteRun();
            _checkpoint.StartNewRun();
            Scenes.ChangeScene("Draft");
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            var panel = PanelBounds;
            DrawPanel(panel, PanelTone.Raised);
            DrawReactor(spriteBatch, panel, uiScale);

            DrawPrimaryAction(spriteBatch, uiScale);
            DrawFooterNav(spriteBatch, uiScale);

            if (_confirmNewRun)
                DrawConfirmDialog(spriteBatch, uiScale);
        }

        private void DrawReactor(SpriteBatch spriteBatch, Rectangle panel,
            float uiScale)
        {
            int pad = UiTheme.Px(UiTheme.SpaceXl, uiScale);

            string status = _checkpoint.HasActiveRun
                ? "RUN ACTIVA"
                : _checkpoint.HasEndedRun
                    ? "RUN TERMINADA"
                    : "SIN RUN";

            Color statusColor = _checkpoint.HasActiveRun
                ? UiTheme.Success
                : _checkpoint.HasEndedRun ? UiTheme.Gold : UiTheme.TextSecondary;

            var statusBounds = new Rectangle(panel.X + pad, panel.Y + pad,
                UiTheme.Px(220, uiScale), UiTheme.Px(40, uiScale));
            Ui.Chrome.DrawStateTag(spriteBatch, statusBounds, status,
                statusColor, uiScale);

            // Anillo/emblema del reactor con glow aditivo.
            var ringCenter = new Vector2(panel.Center.X,
                panel.Y + panel.Height * 0.40f);
            float radius = Math.Min(panel.Width, panel.Height) * 0.26f;
            Ui.Chrome.DrawGlow(spriteBatch, ringCenter, radius * 1.4f,
                statusColor * 0.30f);

            var ringBounds = new Rectangle(
                (int)(ringCenter.X - radius), (int)(ringCenter.Y - radius),
                (int)(radius * 2f), (int)(radius * 2f));
            Ui.Chrome.DrawPanel(spriteBatch, ringBounds, PanelTone.Sunken,
                uiScale);

            string core = _checkpoint.HasActiveRun
                ? _runState.ActIndex.ToString()
                : "?";
            DrawNumeric(core, ringCenter, statusColor,
                radius > 70f ? UiTheme.DisplayXl : UiTheme.Display);

            string coreLabel = _checkpoint.HasActiveRun
                ? "ACTO"
                : "REACTOR";
            DrawSectionLabel(coreLabel,
                new Rectangle(ringBounds.X, ringBounds.Bottom +
                    UiTheme.Px(2, uiScale), ringBounds.Width,
                    UiTheme.Px(20, uiScale)),
                UiTheme.TextMuted, UiTheme.Caption);

            var info = new Rectangle(panel.X + pad,
                (int)(panel.Y + panel.Height * 0.52f),
                panel.Width - pad * 2,
                (int)(panel.Height * 0.46f) - pad);

            if (_checkpoint.HasActiveRun || _checkpoint.HasEndedRun)
            {
                string enemy = _checkpoint.HasActiveRun &&
                               _runState.PendingCombat != null
                    ? _runState.PendingCombat.EnemyName
                    : _checkpoint.HasEndedRun
                        ? "RUN FINALIZADA"
                        : "SIN COMBATE PENDIENTE";

                DrawTextCenteredFit(enemy ?? "SIN COMBATE PENDIENTE",
                    new Rectangle(info.X, info.Y, info.Width,
                        UiTheme.Px(28, uiScale)),
                    UiTheme.TextPrimary, UiTheme.Label);

                DrawTextCenteredFit(
                    $"COMBATES {_runState.TotalCombats}/{RunState.MaxCombats}   " +
                    $"NIVEL {_runState.Level}   MAZO {_runState.Deck.Count}/" +
                    $"{RunState.MaxDeckSize}",
                    new Rectangle(info.X, info.Y + UiTheme.Px(32, uiScale),
                        info.Width, UiTheme.Px(22, uiScale)),
                    UiTheme.TextSecondary, UiTheme.Caption);

                DrawDeckStrip(spriteBatch,
                    new Rectangle(info.X, info.Y + UiTheme.Px(60, uiScale),
                        info.Width,
                        Math.Max(0, info.Bottom - info.Y -
                            UiTheme.Px(60, uiScale))),
                    uiScale);
            }
            else
            {
                DrawTextCenteredFit("REACTOR EN ESPERA",
                    new Rectangle(info.X, info.Y + UiTheme.Px(12, uiScale),
                        info.Width, UiTheme.Px(34, uiScale)),
                    UiTheme.TextPrimary, UiTheme.Heading);
                DrawTextCenteredFit("EMPIEZA UNA NUEVA RUN PARA ACTIVARLO",
                    new Rectangle(info.X, info.Y + UiTheme.Px(50, uiScale),
                        info.Width, UiTheme.Px(22, uiScale)),
                    UiTheme.TextMuted, UiTheme.Caption);

                var stats = new Rectangle(info.X,
                    info.Bottom - UiTheme.Px(78, uiScale), info.Width,
                    UiTheme.Px(70, uiScale));
                if (stats.Y > info.Y + UiTheme.Px(80, uiScale))
                {
                    int gap = UiTheme.Px(UiTheme.SpaceSm, uiScale);
                    int width = (stats.Width - gap * 2) / 3;
                    DrawStatTile(new Rectangle(stats.X, stats.Y, width,
                        stats.Height), _cardCount, "CARTAS", uiScale);
                    DrawStatTile(new Rectangle(stats.X + width + gap,
                        stats.Y, width, stats.Height), _perkCount, "PERKS",
                        uiScale);
                    DrawStatTile(new Rectangle(stats.X + (width + gap) * 2,
                        stats.Y, width, stats.Height), RunState.MaxCombats,
                        "COMBATES", uiScale);
                }
            }
        }

        private void DrawStatTile(Rectangle bounds, int value, string label,
            float uiScale)
        {
            Ui.Textures.DrawPanel(SpriteBatch, bounds, UiTheme.Border * 0.7f);
            Ui.Textures.DrawPanel(SpriteBatch, UiTheme.Inset(bounds, uiScale),
                UiTheme.PanelSunken);

            DrawNumeric(value.ToString(),
                new Vector2(bounds.Center.X,
                    bounds.Y + bounds.Height * 0.42f),
                UiTheme.Gold, UiTheme.Numeric);
            DrawTextCenteredFit(label,
                new Rectangle(bounds.X, bounds.Bottom -
                    UiTheme.Px(20, uiScale), bounds.Width,
                    UiTheme.Px(16, uiScale)),
                UiTheme.TextMuted, UiTheme.Caption);
        }

        private void DrawDeckStrip(SpriteBatch spriteBatch, Rectangle bounds,
            float uiScale)
        {
            if (bounds.Height < UiTheme.Px(24, uiScale)) return;

            int count = _runState?.Deck?.Count ?? 0;
            int max = RunState.MaxDeckSize;
            int gap = UiTheme.Px(UiTheme.SpaceXs, uiScale);
            int cardWidth = Math.Max(2,
                (bounds.Width - gap * (max - 1)) / max);
            int cardHeight = Math.Min(bounds.Height,
                (int)(cardWidth * 1.35f));

            for (int i = 0; i < max; i++)
            {
                var slot = new Rectangle(bounds.X + i * (cardWidth + gap),
                    bounds.Y, cardWidth, cardHeight);

                if (i < count)
                {
                    DrawCardInstance(slot, _runState.Deck[i],
                        new CardRenderOptions
                        {
                            Variant = CardFrameVariant.MiniDeck,
                            ShowName = false
                        });
                }
                else
                {
                    Ui.Textures.DrawPanel(spriteBatch, slot,
                        UiTheme.Border * 0.5f);
                    Ui.Textures.DrawPanel(spriteBatch,
                        UiTheme.Inset(slot, uiScale),
                        UiTheme.PanelSunken * 0.8f);
                }
            }
        }

        private void DrawPrimaryAction(SpriteBatch spriteBatch, float uiScale)
        {
            string label = _checkpoint.HasActiveRun
                ? "CONTINUAR"
                : _checkpoint.HasEndedRun ? "VER RESUMEN" : "NUEVA RUN";

            Color accent = _checkpoint.HasActiveRun
                ? UiTheme.Accent
                : _checkpoint.HasEndedRun ? UiTheme.Gold : UiTheme.Success;

            DrawTapButtonScaled("primary", label, accent, UiTheme.Label);
        }

        private void DrawFooterNav(SpriteBatch spriteBatch, float uiScale)
        {
            DrawTapButtonScaled("nav_collection", "COLECCION",
                UiTheme.AccentSoft, UiTheme.LabelSmall);
            DrawTapButtonScaled("nav_store", "TIENDA", UiTheme.Violet,
                UiTheme.LabelSmall);
            DrawTapButtonScaled("nav_new", "NUEVA RUN", UiTheme.Success,
                UiTheme.LabelSmall);
            DrawTapButtonScaled("nav_quit", "SALIR", UiTheme.Danger,
                UiTheme.LabelSmall);
        }

        private void DrawConfirmDialog(SpriteBatch spriteBatch, float uiScale)
        {
            var screen = new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height);
            Ui.Chrome.DrawOverlay(spriteBatch, screen, 0.72f);

            var panel = ConfirmPanelBounds;
            DrawPanel(panel, PanelTone.Danger);
            int pad = UiTheme.Px(UiTheme.SpaceXl, uiScale);

            DrawTextCenteredFit("HAY UNA RUN ACTIVA",
                new Rectangle(panel.X + pad, panel.Y + pad,
                    panel.Width - pad * 2, UiTheme.Px(34, uiScale)),
                UiTheme.TextPrimary, UiTheme.Heading);

            DrawTextCenteredFit("SI EMPIEZAS OTRA RUN SE PERDERA EL PROGRESO.",
                new Rectangle(panel.X + pad, panel.Y + pad +
                    UiTheme.Px(40, uiScale), panel.Width - pad * 2,
                    UiTheme.Px(44, uiScale)),
                UiTheme.TextSecondary, UiTheme.LabelSmall);

            DrawTapButtonScaled("confirm_yes", "SI, NUEVA RUN",
                UiTheme.Danger);
            DrawTapButtonScaled("confirm_no", "CANCELAR",
                UiTheme.BorderBright);
        }
    }
}
