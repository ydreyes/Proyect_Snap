using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Meta;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Resumen de fin de run: reactor de resultado, desglose de puntos, mazo
    /// final y acciones. No entrega puntos por si mismo: el cobro es el
    /// existente e idempotente.
    /// </summary>
    public class RunSummaryScene : UiScene
    {
        private RunState _runState;
        private RunDirector _runDirector;
        private LevelUpSystem _levelUpSystem;
        private RunCheckpointService _checkpoint;
        private MetaProgressionSystem _meta;
        private ProfileDto _profile;
        private AwardResult _award;
        private PointsBreakdown _breakdown;
        private string _awardError;

        public RunSummaryScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title =>
            _runState != null && _runState.Outcome == RunOutcome.Won
                ? "VICTORIA"
                : "DERROTA";

        protected override string Subtitle => "RESUMEN DE RUN";

        protected override Color Accent =>
            _runState != null && _runState.Outcome == RunOutcome.Won
                ? UiTheme.Success
                : UiTheme.DangerSoft;

        protected override string CurrencyIcon => "P";
        protected override string CurrencyValue => (_breakdown?.Total ?? 0).ToString();
        protected override string CurrencyLabel => "GANADOS";
        protected override int BackgroundSeed => 3303;

        protected override void OnEnterScene()
        {
            _runState = Services.Get<RunState>();
            _runDirector = Services.Get<RunDirector>();
            _levelUpSystem = Services.Has<LevelUpSystem>()
                ? Services.Get<LevelUpSystem>()
                : null;
            _checkpoint = Services.Get<RunCheckpointService>();
            _meta = Services.Has<MetaProgressionSystem>()
                ? Services.Get<MetaProgressionSystem>()
                : null;
            _profile = _checkpoint.Profile;
            _award = null;
            _awardError = null;
            _breakdown = null;

            _levelUpSystem?.RefreshDeck(_runState);
            TryAward();
        }

        private void TryAward()
        {
            if (_meta == null || _profile == null)
            {
                _awardError = "No se pudo cargar el perfil para el cobro.";
                _breakdown = _meta?.Compute(_runState);
                return;
            }

            _award = _meta.Award(_runState, _profile,
                profile => _checkpoint.SaveProfile(out _));

            _breakdown = _award.Breakdown;
            _awardError = _award.Error;
        }

        private bool CanRematch =>
            _runState != null &&
            _runState.Outcome == RunOutcome.Lost &&
            _runState.RematchCombat != null;

        private bool Won => _runState != null && _runState.Outcome == RunOutcome.Won;

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle ReactorBounds
        {
            get
            {
                var inner = ContentInner;
                return new Rectangle(inner.X, inner.Y, inner.Width,
                    (int)(inner.Height * 0.24f));
            }
        }

        private Rectangle PointsBounds
        {
            get
            {
                var inner = ContentInner;
                int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
                int y = ReactorBounds.Bottom + gap;
                return new Rectangle(inner.X, y, inner.Width,
                    (int)(inner.Height * 0.24f));
            }
        }

        private Rectangle DeckBounds
        {
            get
            {
                var inner = ContentInner;
                int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
                int y = PointsBounds.Bottom + gap;
                int bottom = inner.Bottom - UiTheme.Px(72, Scaffold.UiScale);
                return new Rectangle(inner.X, y, inner.Width,
                    System.Math.Max(0, bottom - y));
            }
        }

        private Rectangle ActionsRow
        {
            get
            {
                var inner = ContentInner;
                int height = UiTheme.Px(56, Scaffold.UiScale);
                return new Rectangle(inner.X, inner.Bottom - height,
                    inner.Width, height);
            }
        }

        private Rectangle MenuBounds
        {
            get
            {
                var row = ActionsRow;
                int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
                int width = (row.Width - gap) / 2;
                return new Rectangle(row.X, row.Y, width, row.Height);
            }
        }

        private Rectangle ReRunBounds
        {
            get
            {
                var menu = MenuBounds;
                int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
                return new Rectangle(menu.Right + gap, menu.Y, menu.Width,
                    menu.Height);
            }
        }

        private Rectangle RematchBounds
        {
            get
            {
                var row = ActionsRow;
                int height = UiTheme.Px(50, Scaffold.UiScale);
                return new Rectangle(row.X, row.Y - height -
                    UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale),
                    row.Width, height);
            }
        }

        private Rectangle RetryBounds
        {
            get
            {
                var row = ActionsRow;
                int height = UiTheme.Px(50, Scaffold.UiScale);
                return new Rectangle(row.X, row.Y - height -
                    UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale),
                    row.Width, height);
            }
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (_awardError != null && Tap("retry", RetryBounds))
            {
                TryAward();
                return;
            }

            if (_awardError == null && CanRematch && Tap("rematch", RematchBounds))
            {
                _runDirector.OnRematchRequested();
                _checkpoint.SavePreCombat();
                Scenes.ChangeScene("Combat");
                return;
            }

            if (Tap("menu", MenuBounds))
            {
                _checkpoint.DeleteRun();
                Scenes.ChangeScene("Menu");
                return;
            }

            if (Tap("rerun", ReRunBounds))
            {
                _checkpoint.DeleteRun();
                _checkpoint.StartNewRun();
                Scenes.ChangeScene("Draft");
            }
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            DrawReactor(spriteBatch, uiScale);
            DrawPoints(spriteBatch, uiScale);
            DrawDeck(spriteBatch, uiScale);

            if (_awardError != null)
            {
                DrawTapButtonScaled("retry", "REINTENTAR COBRO",
                    UiTheme.Danger);
                DrawTextCenteredFit(_awardError,
                    new Rectangle(RetryBounds.X, RetryBounds.Y -
                        UiTheme.Px(22, uiScale), RetryBounds.Width,
                        UiTheme.Px(18, uiScale)),
                    UiTheme.DangerSoft, UiTheme.Caption);
            }
            else if (CanRematch)
            {
                DrawTapButtonScaled("rematch", "REINTENTAR COMBATE",
                    UiTheme.Gold);
            }

            DrawTapButtonScaled("menu", "MENU PRINCIPAL", UiTheme.AccentSoft,
                UiTheme.LabelSmall);
            DrawTapButtonScaled("rerun", "RE-RUN", UiTheme.Success,
                UiTheme.LabelSmall);
        }

        private void DrawReactor(SpriteBatch spriteBatch, float uiScale)
        {
            var panel = ReactorBounds;
            DrawPanel(panel, Won ? PanelTone.Perk : PanelTone.Danger);

            Color color = Won ? UiTheme.Success : UiTheme.DangerSoft;
            var center = new Vector2(panel.X + panel.Height * 0.52f,
                panel.Center.Y);
            float radius = panel.Height * 0.34f;

            Ui.Chrome.DrawGlow(spriteBatch, center, radius * 1.8f,
                color * 0.34f);

            var badge = new Rectangle((int)(center.X - radius),
                (int)(center.Y - radius), (int)(radius * 2f),
                (int)(radius * 2f));
            Ui.Chrome.DrawPanel(spriteBatch, badge, PanelTone.Sunken, uiScale);
            DrawNumeric(Won ? "W" : "L", center, color, UiTheme.Display);

            int textX = badge.Right + UiTheme.Px(UiTheme.SpaceXl, uiScale);
            int textWidth = panel.Right - textX -
                            UiTheme.Px(UiTheme.SpaceLg, uiScale);

            DrawDisplay(Won ? "RUN COMPLETADA" : "RUN TERMINADA",
                new Vector2(textX, panel.Y + UiTheme.Px(UiTheme.SpaceLg,
                    uiScale)), color, UiTheme.Heading);

            string stats =
                $"NIVEL {_runState.Level}   XP {_runState.XpEarned}   " +
                $"COMBATES {_runState.TotalCombats}/{RunState.MaxCombats}";
            DrawLabel(stats, new Vector2(textX, panel.Y +
                UiTheme.Px(UiTheme.SpaceLg + 34, uiScale)),
                UiTheme.TextSecondary, UiTheme.LabelSmall);

            string actLabel = _runState.ActIndex >= RunActTable.FinalAct
                ? "ACTO FINAL"
                : $"ACTO {_runState.ActIndex}/3";
            DrawLabel($"{actLabel}   DINERO {_runState.MoneyEarned} GANADO / " +
                $"{_runState.Money} DISPONIBLE",
                new Vector2(textX, panel.Y +
                    UiTheme.Px(UiTheme.SpaceLg + 58, uiScale)),
                UiTheme.TextMuted, UiTheme.Caption);

            if (_runDirector != null && _runDirector.LastDefeatConsumedExtraLife)
            {
                var note = new Rectangle(textX, panel.Bottom -
                    UiTheme.Px(34, uiScale), textWidth,
                    UiTheme.Px(22, uiScale));
                Ui.Chrome.DrawStateTag(spriteBatch, note,
                    "VIDA EXTRA CONSUMIDA PARA SEGUIR",
                    UiTheme.Gold, uiScale);
            }
        }

        private void DrawPoints(SpriteBatch spriteBatch, float uiScale)
        {
            if (_breakdown == null) return;

            var panel = PointsBounds;
            DrawPanel(panel, PanelTone.Raised);

            var inner = Inset(panel, UiTheme.Px(UiTheme.SpaceLg, uiScale));
            DrawSectionHeader("DESGLOSE DE PUNTOS",
                new Rectangle(inner.X, inner.Y, inner.Width,
                    UiTheme.Px(20, uiScale)), UiTheme.Gold);

            int rowY = inner.Y + UiTheme.Px(24, uiScale);
            int rowHeight = Math.Max(16,
                (inner.Bottom - rowY - UiTheme.Px(20, uiScale)) / 5);

            DrawPointRow(inner, rowY, rowHeight, uiScale,
                "COMBATES NORMALES",
                $"{_breakdown.NormalCombats} x 3 = {_breakdown.NormalCombatPoints}",
                UiTheme.TextSecondary);
            DrawPointRow(inner, rowY + rowHeight, rowHeight, uiScale,
                "SUBJEFES",
                $"{_breakdown.SubBosses} x 40 = {_breakdown.SubBossPoints}",
                UiTheme.TextSecondary);
            DrawPointRow(inner, rowY + rowHeight * 2, rowHeight, uiScale,
                "NIVEL", $"{_breakdown.Level} x 8 = {_breakdown.LevelPoints}",
                UiTheme.TextSecondary);
            DrawPointRow(inner, rowY + rowHeight * 3, rowHeight, uiScale,
                "JEFE FINAL", _breakdown.FinalBossBonus.ToString(),
                UiTheme.TextSecondary);
            DrawPointRow(inner, rowY + rowHeight * 4, rowHeight, uiScale,
                "TOTAL", _breakdown.Total.ToString(), UiTheme.Gold);

            string status = _award?.Status == AwardStatus.AlreadyCollected
                ? "YA COBRADO"
                : _awardError != null ? "COBRO PENDIENTE" : "PUNTOS COBRADOS";

            DrawTextCenteredFit(status,
                new Rectangle(inner.X, inner.Bottom -
                    UiTheme.Px(18, uiScale), inner.Width,
                    UiTheme.Px(16, uiScale)),
                _awardError != null ? UiTheme.DangerSoft : UiTheme.Success,
                UiTheme.Caption);
        }

        private void DrawPointRow(Rectangle inner, int y, int height,
            float uiScale, string label, string value, Color valueColor)
        {
            var row = new Rectangle(inner.X, y, inner.Width, height);
            DrawSectionLabel(label,
                new Rectangle(row.X, row.Y, (int)(row.Width * 0.55f),
                    row.Height), UiTheme.TextSecondary, UiTheme.Caption);
            DrawTextCenteredFit(value,
                new Rectangle(row.X + (int)(row.Width * 0.55f), row.Y,
                    (int)(row.Width * 0.45f), row.Height),
                valueColor, UiTheme.Caption);
        }

        private void DrawDeck(SpriteBatch spriteBatch, float uiScale)
        {
            var area = DeckBounds;
            DrawSectionHeader("MAZO FINAL", new Rectangle(area.X, area.Y,
                area.Width, UiTheme.Px(20, uiScale)), UiTheme.TextSecondary);

            var grid = new Rectangle(area.X, area.Y + UiTheme.Px(26, uiScale),
                area.Width, Math.Max(0, area.Height - UiTheme.Px(26, uiScale)));

            int count = _runState?.Deck?.Count ?? 0;
            if (count == 0)
            {
                DrawTextCenteredFit("MAZO VACIO",
                    new Rectangle(grid.X, grid.Y, grid.Width, grid.Height),
                    UiTheme.TextMuted, UiTheme.LabelSmall);
                return;
            }

            const int columns = 6;
            int gap = UiTheme.Px(UiTheme.SpaceSm, uiScale);
            int width = (grid.Width - gap * (columns - 1)) / columns;
            int height = (int)(width * 1.36f);
            int rows = (count + columns - 1) / columns;

            if (rows > 1 && height * rows + gap > grid.Height)
                height = Math.Max(8, (grid.Height - gap) / rows);

            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int column = i % columns;
                var bounds = new Rectangle(
                    grid.X + column * (width + gap),
                    grid.Y + row * (height + gap), width, height);

                DrawCardInstance(bounds, _runState.Deck[i],
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.Draft,
                        ShowName = height > UiTheme.Px(64, uiScale)
                    });
            }
        }
    }
}
