using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.UI;
using SnapClone.UI.Animation;
using SnapClone.UI.Components;
using SnapClone.UI.Layout;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Base comun de las siete pantallas de jugador. Resuelve el ciclo de vida
    /// del dibujado (fondo, cabecera, contenido y pie), el scaffold seguro y el
    /// router de botones, para que cada escena solo implemente su contenido.
    /// </summary>
    public abstract class UiScene : IScene
    {
        private readonly Dictionary<string, UiButton> _buttons =
            new Dictionary<string, UiButton>(StringComparer.Ordinal);

        protected readonly GraphicsDevice GraphicsDevice;
        protected readonly SpriteBatch SpriteBatch;
        protected readonly GameServices Services;
        protected readonly SceneManager Scenes;

        protected UiContext Ui;
        protected InputService Input;
        protected BackService Back;

        protected ScreenScaffold Scaffold;
        protected SpaceBackground Background;
        protected readonly UiAnimator Animator = new UiAnimator();

        protected float Elapsed;
        protected float DeltaSeconds;

        protected UiScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = spriteBatch;
            Services = services;
            Scenes = sceneManager;
        }

        // ------------------------------------------------------------------
        // Puntos de extension
        // ------------------------------------------------------------------

        protected abstract string Title { get; }

        protected virtual string Subtitle => null;
        protected virtual Color Accent => UiTheme.Accent;
        protected virtual string CurrencyIcon => null;
        protected virtual string CurrencyValue => null;
        protected virtual string CurrencyLabel => null;
        protected virtual bool ShowHeader => true;
        protected virtual bool ShowFooter => true;
        protected virtual int HeaderDesign => 104;
        protected virtual int FooterDesign => 96;
        protected virtual int BackgroundSeed => 2026;
        protected virtual ScreenInsets SafeInsets => ScreenInsets.None;

        /// <summary>Contenido propio de la pantalla, entre cabecera y pie.</summary>
        protected abstract void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale);

        protected virtual void UpdateScene(GameTime gameTime) { }

        protected virtual void OnEnterScene() { }

        /// <summary>
        /// Se llama una sola vez tras resolver los servicios visuales. La usa
        /// la galeria de UI para cargar recursos de diagnostico.
        /// </summary>
        protected virtual void OnLoadContent() { }

        // ------------------------------------------------------------------
        // Ciclo de vida
        // ------------------------------------------------------------------

        public virtual void Initialize() { }

        public void LoadContent()
        {
            Ui = Services.Get<UiContext>();
            Input = Services.Get<InputService>();
            Back = Services.Has<BackService>() ? Services.Get<BackService>() : null;
            Background = Ui.NewBackground(BackgroundSeed);
            OnLoadContent();
        }

        public void OnEnter()
        {
            ResetButtons();
            Animator.Clear();
            Elapsed = 0f;
            OnEnterScene();
        }

        public void Update(GameTime gameTime)
        {
            DeltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Elapsed += DeltaSeconds;

            RefreshScaffold();
            Animator.Update(DeltaSeconds);
            UpdateScene(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Ui == null) return;

            RefreshScaffold();

            GraphicsDevice.Clear(UiTheme.BackgroundDeep);
            spriteBatch.Begin();

            if (Background != null)
            {
                Background.Draw(spriteBatch, Ui.Textures,
                    new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
                        GraphicsDevice.Viewport.Height), Elapsed);
            }

            if (ShowHeader)
                DrawHeaderBar(spriteBatch);

            DrawScene(spriteBatch, Scaffold.Content, Scaffold.UiScale);

            if (ShowFooter)
                Scaffold.DrawFooterChrome(spriteBatch, Ui.Chrome);

            spriteBatch.End();
        }

        // ------------------------------------------------------------------
        // Scaffold y cabecera
        // ------------------------------------------------------------------

        protected void RefreshScaffold()
        {
            var viewport = GraphicsDevice.Viewport;
            Scaffold = ScreenScaffold.Create(
                new Rectangle(0, 0, viewport.Width, viewport.Height),
                SafeInsets, HeaderDesign, FooterDesign);
        }

        protected Rectangle ContentInner => Scaffold.ContentInner;

        private void DrawHeaderBar(SpriteBatch spriteBatch)
        {
            Scaffold.DrawHeader(spriteBatch, Ui.Chrome, Title, Subtitle,
                CurrencyIcon, CurrencyValue, CurrencyLabel, Accent);
        }

        // ------------------------------------------------------------------
        // Router de botones
        // ------------------------------------------------------------------

        private void ResetButtons()
        {
            foreach (var button in _buttons.Values)
                button.Reset();
        }

        /// <summary>
        /// Actualiza y devuelve true solo en el frame en que se suelta dentro.
        /// Debe llamarse desde <see cref="UpdateScene"/>.
        /// </summary>
        protected bool Tap(string key, Rectangle bounds, bool enabled = true)
        {
            if (!_buttons.TryGetValue(key, out UiButton button))
            {
                button = new UiButton();
                _buttons[key] = button;
            }

            button.Bounds = bounds;
            button.Enabled = enabled;
            button.Visible = true;
            return button.Update(Input, DeltaSeconds);
        }

        protected bool IsHovered(string key) =>
            _buttons.TryGetValue(key, out UiButton button) &&
            button.IsPointerInside;

        protected void DrawTapButton(string key, string label,
            Color? accent = null, float fontSize = UiTheme.Label)
        {
            RectLabel(key, label, accent, fontSize, false);
        }

        /// <summary>Dibuja el boton con su estado real y escala animada.</summary>
        protected void DrawTapButtonScaled(string key, string label,
            Color? accent = null, float fontSize = UiTheme.Label)
        {
            RectLabel(key, label, accent, fontSize, true);
        }

        private void RectLabel(string key, string label, Color? accent,
            float fontSize, bool scaled)
        {
            if (!_buttons.TryGetValue(key, out UiButton button)) return;

            Rectangle bounds = button.Bounds;
            if (scaled && Math.Abs(button.VisualScale - 1f) > 0.001f)
            {
                int width = Math.Max(1,
                    (int)MathF.Round(bounds.Width * button.VisualScale));
                int height = Math.Max(1,
                    (int)MathF.Round(bounds.Height * button.VisualScale));
                bounds = new Rectangle(bounds.X + (bounds.Width - width) / 2,
                    bounds.Y + (bounds.Height - height) / 2, width, height);
            }

            Ui.Chrome.DrawButton(SpriteBatch, bounds, label, button.State,
                Scaffold.UiScale, fontSize, accent);
        }

        // ------------------------------------------------------------------
        // Helpers de dibujo compartidos
        // ------------------------------------------------------------------

        protected void DrawPanel(Rectangle bounds, PanelTone tone = PanelTone.Default,
            bool shadow = true)
        {
            Ui.Chrome.DrawPanel(SpriteBatch, bounds, tone, Scaffold.UiScale,
                shadow);
        }

        protected void DrawRibbon(Rectangle bounds, string text, Color? accent = null)
        {
            Ui.Chrome.DrawRibbon(SpriteBatch, bounds, text,
                accent ?? Accent, Scaffold.UiScale);
        }

        protected void DrawSectionLabel(string text, Rectangle bounds,
            Color? color = null, float size = UiTheme.LabelSmall)
        {
            Ui.Fonts.DrawRoleFit(SpriteBatch, TextRole.Ui, text, bounds,
                color ?? UiTheme.TextSecondary, size, Scaffold.UiScale);
        }

        protected void DrawDisplay(string text, Vector2 position,
            Color? color = null, float size = UiTheme.Display)
        {
            Ui.Fonts.DrawDisplay(SpriteBatch, text, position,
                color ?? UiTheme.TextPrimary, size, Scaffold.UiScale);
        }

        protected void DrawDisplayCentered(string text, Rectangle bounds,
            Color? color = null, float size = UiTheme.Display)
        {
            Ui.Fonts.DrawDisplayCentered(SpriteBatch, text, bounds,
                color ?? UiTheme.TextPrimary, size, Scaffold.UiScale);
        }

        protected void DrawLabel(string text, Vector2 position,
            Color? color = null, float size = UiTheme.Label)
        {
            Ui.Fonts.DrawLabel(SpriteBatch, text, position,
                color ?? UiTheme.TextPrimary, size, Scaffold.UiScale);
        }

        protected void DrawBody(string text, Vector2 position, Color? color = null)
        {
            Ui.Fonts.DrawBody(SpriteBatch, text, position,
                color ?? UiTheme.TextSecondary, Scaffold.UiScale);
        }

        protected void DrawNumeric(string text, Vector2 center,
            Color? color = null, float size = UiTheme.Numeric)
        {
            Ui.Fonts.DrawNumeric(SpriteBatch, text, center,
                color ?? UiTheme.TextPrimary, size, Scaffold.UiScale);
        }

        protected void DrawTextCenteredFit(string text, Rectangle bounds,
            Color? color = null, float size = UiTheme.Label)
        {
            Ui.Fonts.DrawRoleFit(SpriteBatch, TextRole.Ui, text, bounds,
                color ?? UiTheme.TextPrimary, size, Scaffold.UiScale);
        }

        protected void DrawWrapped(string text, Rectangle bounds,
            Color? color = null, float size = UiTheme.Body)
        {
            Ui.Fonts.DrawRoleWrapped(SpriteBatch, TextRole.Body, text, bounds,
                color ?? UiTheme.TextSecondary, size, Scaffold.UiScale);
        }

        protected void DrawChip(Rectangle bounds, string icon, string value,
            string label, Color? accent = null)
        {
            Ui.Chrome.DrawCurrencyChip(SpriteBatch, bounds, icon, value, label,
                accent ?? Accent, Scaffold.UiScale);
        }

        protected void DrawCardDefinition(Rectangle bounds,
            Data.Definitions.CardDef card, CardRenderOptions options = null)
        {
            Ui.Cards.DrawCard(SpriteBatch, bounds, card,
                options ?? new CardRenderOptions(), Scaffold.UiScale);
        }

        protected void DrawCardInstance(Rectangle bounds,
            Gameplay.Cards.CardInstance card, CardRenderOptions options = null)
        {
            Ui.Cards.DrawInstance(SpriteBatch, bounds, card,
                options ?? new CardRenderOptions(), Scaffold.UiScale);
        }

        /// <summary>Dibuja una cabecera interna de seccion con separador.</summary>
        protected void DrawSectionHeader(string text, Rectangle bounds,
            Color? color = null)
        {
            DrawSectionLabel(text, bounds, color ?? UiTheme.TextSecondary,
                UiTheme.LabelSmall);
            int y = bounds.Bottom - Math.Max(1, UiTheme.Px(1, Scaffold.UiScale));
            SpriteBatch.Draw(Ui.Textures.Pixel,
                new Rectangle(bounds.X, y, bounds.Width, Math.Max(1,
                    UiTheme.Px(1, Scaffold.UiScale))),
                UiTheme.Border * 0.6f);
        }

        protected static Rectangle Inset(Rectangle bounds, int padding) =>
            new Rectangle(bounds.X + padding, bounds.Y + padding,
                Math.Max(0, bounds.Width - padding * 2),
                Math.Max(0, bounds.Height - padding * 2));

        protected static bool Contains(Rectangle bounds, Vector2 point) =>
            point.X >= bounds.Left && point.X <= bounds.Right &&
            point.Y >= bounds.Top && point.Y <= bounds.Bottom;
    }
}
