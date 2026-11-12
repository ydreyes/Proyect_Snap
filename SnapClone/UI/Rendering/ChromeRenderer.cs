using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Components;
using SnapClone.UI.Theme;

namespace SnapClone.UI.Rendering
{
    public enum PanelTone
    {
        Default,
        Raised,
        Sunken,
        Accent,
        Gold,
        Danger,
        Perk,
        Violet
    }

    /// <summary>
    /// Unica implementacion de paneles, botones, chips, cintas, badges,
    /// slots de progreso y overlays. Todos los estados comparten geometria.
    /// </summary>
    public class ChromeRenderer
    {
        private readonly TextureCache _textures;
        private readonly FontService _fonts;

        public ChromeRenderer(TextureCache textures, FontService fonts)
        {
            _textures = textures;
            _fonts = fonts;
        }

        public TextureCache Textures => _textures;
        public FontService Fonts => _fonts;

        // ------------------------------------------------------------------
        // Paleta por tono
        // ------------------------------------------------------------------

        private static void ToneColors(PanelTone tone, out Color fill,
            out Color border)
        {
            switch (tone)
            {
                case PanelTone.Raised:
                    fill = UiTheme.PanelRaised;
                    border = UiTheme.BorderBright;
                    break;
                case PanelTone.Sunken:
                    fill = UiTheme.PanelSunken;
                    border = UiTheme.Border;
                    break;
                case PanelTone.Accent:
                    fill = new Color(22, 44, 72);
                    border = UiTheme.AccentSoft;
                    break;
                case PanelTone.Gold:
                    fill = new Color(48, 38, 20);
                    border = UiTheme.Gold;
                    break;
                case PanelTone.Danger:
                    fill = new Color(54, 24, 22);
                    border = UiTheme.DangerSoft;
                    break;
                case PanelTone.Perk:
                    fill = new Color(26, 46, 32);
                    border = UiTheme.Perk;
                    break;
                case PanelTone.Violet:
                    fill = new Color(42, 32, 62);
                    border = UiTheme.Violet;
                    break;
                default:
                    fill = UiTheme.Panel;
                    border = UiTheme.Border;
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Paneles
        // ------------------------------------------------------------------

        public void DrawPanel(SpriteBatch spriteBatch, Rectangle bounds,
            PanelTone tone, float uiScale, bool shadow = true)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            ToneColors(tone, out Color fill, out Color border);

            if (shadow)
                _textures.DrawPanel(spriteBatch,
                    UiTheme.ShadowOf(bounds, uiScale), Color.Black * 0.5f);

            _textures.DrawPanel(spriteBatch, bounds, border);
            _textures.DrawPanel(spriteBatch, UiTheme.Inset(bounds, uiScale), fill);

            // Highlight interior superior: una linea fina que da volumen.
            int inset = UiTheme.Px(UiTheme.BorderThickness, uiScale);
            int lineHeight = Math.Max(1, UiTheme.Px(1f, uiScale));
            int cut = UiTheme.Px(UiTheme.CornerCut, uiScale);

            spriteBatch.Draw(_textures.Pixel,
                new Rectangle(bounds.X + cut, bounds.Y + inset,
                    Math.Max(0, bounds.Width - cut * 2), lineHeight),
                Color.White * 0.10f);
        }

        public void DrawPanelWithAccent(SpriteBatch spriteBatch, Rectangle bounds,
            PanelTone tone, Color accent, float uiScale, int accentWidth = 4)
        {
            DrawPanel(spriteBatch, bounds, tone, uiScale);
            int width = Math.Max(1, UiTheme.Px(accentWidth, uiScale));
            spriteBatch.Draw(_textures.Pixel,
                new Rectangle(bounds.X + UiTheme.Px(2, uiScale),
                    bounds.Y + UiTheme.Px(8, uiScale), width,
                    Math.Max(0, bounds.Height - UiTheme.Px(16, uiScale))),
                accent);
        }

        // ------------------------------------------------------------------
        // Botones
        // ------------------------------------------------------------------

        public void DrawButton(SpriteBatch spriteBatch, Rectangle bounds,
            string label, ButtonVisualState state, float uiScale,
            float fontSize = UiTheme.Label, Color? accent = null)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            Color baseColor = accent ?? UiTheme.Accent;
            Color fill;
            Color border;
            Color text;

            switch (state)
            {
                case ButtonVisualState.Disabled:
                    fill = UiTheme.PanelSunken;
                    border = UiTheme.Border * 0.7f;
                    text = UiTheme.TextMuted;
                    break;
                case ButtonVisualState.Pressed:
                    fill = Color.Lerp(baseColor, Color.Black, 0.45f);
                    border = Color.Lerp(baseColor, Color.White, 0.35f);
                    text = UiTheme.TextOnAccent;
                    break;
                case ButtonVisualState.Hover:
                    fill = Color.Lerp(baseColor, Color.Black, 0.10f);
                    border = Color.Lerp(baseColor, Color.White, 0.55f);
                    text = UiTheme.TextOnAccent;
                    break;
                default:
                    fill = Color.Lerp(baseColor, Color.Black, 0.28f);
                    border = Color.Lerp(baseColor, Color.White, 0.30f);
                    text = UiTheme.TextOnAccent;
                    break;
            }

            _textures.DrawPanel(spriteBatch, UiTheme.ShadowOf(bounds, uiScale),
                Color.Black * 0.45f);
            _textures.DrawPanel(spriteBatch, bounds, border);
            _textures.DrawPanel(spriteBatch, UiTheme.Inset(bounds, uiScale), fill);

            int cut = UiTheme.Px(UiTheme.CornerCut, uiScale);
            int lineHeight = Math.Max(1, UiTheme.Px(1f, uiScale));
            spriteBatch.Draw(_textures.Pixel,
                new Rectangle(bounds.X + cut, bounds.Y + UiTheme.Px(2, uiScale),
                    Math.Max(0, bounds.Width - cut * 2), lineHeight),
                Color.White * (state == ButtonVisualState.Pressed ? 0.06f : 0.16f));

            int pad = UiTheme.Px(UiTheme.SpaceMd, uiScale);
            var labelBounds = new Rectangle(bounds.X + pad,
                bounds.Y + UiTheme.Px(2, uiScale),
                Math.Max(0, bounds.Width - pad * 2),
                Math.Max(0, bounds.Height - UiTheme.Px(4, uiScale)));

            _fonts.DrawRoleFit(spriteBatch, TextRole.Ui, label, labelBounds,
                text, fontSize, uiScale);
        }

        // ------------------------------------------------------------------
        // Chips de moneda
        // ------------------------------------------------------------------

        public void DrawCurrencyChip(SpriteBatch spriteBatch, Rectangle bounds,
            string icon, string value, string label, Color accent, float uiScale)
        {
            DrawPanel(spriteBatch, bounds, PanelTone.Raised, uiScale);

            int padding = UiTheme.Px(UiTheme.SpaceSm, uiScale);
            float radius = Math.Max(4f, bounds.Height * 0.34f);
            var iconCenter = new Vector2(bounds.X + padding + radius,
                bounds.Center.Y);

            spriteBatch.Draw(_textures.Pixel,
                new Rectangle((int)(iconCenter.X - radius),
                    (int)(iconCenter.Y - radius),
                    (int)(radius * 2f), (int)(radius * 2f)),
                accent * 0.28f);
            _fonts.DrawRoleCentered(spriteBatch, TextRole.Numeric, icon,
                new Rectangle((int)(iconCenter.X - radius),
                    (int)(iconCenter.Y - radius),
                    (int)(radius * 2f), (int)(radius * 2f)),
                accent, UiTheme.NumericSmall, uiScale);

            int textX = (int)(iconCenter.X + radius + padding);
            var valueBounds = new Rectangle(textX, bounds.Y,
                Math.Max(0, bounds.Right - textX - padding), bounds.Height);
            _fonts.DrawRoleCentered(spriteBatch, TextRole.Numeric, value,
                valueBounds, UiTheme.TextPrimary, UiTheme.NumericSmall, uiScale);

            if (!string.IsNullOrEmpty(label))
            {
                var labelSize = _fonts.MeasureRole(TextRole.Ui, label,
                    UiTheme.Caption, uiScale);
                _fonts.DrawRole(spriteBatch, TextRole.Ui, label,
                    new Vector2(textX, bounds.Bottom - padding - labelSize.Y),
                    UiTheme.TextMuted, UiTheme.Caption, uiScale);
            }
        }

        // ------------------------------------------------------------------
        // Cintas, badges y tags
        // ------------------------------------------------------------------

        public void DrawRibbon(SpriteBatch spriteBatch, Rectangle bounds,
            string title, Color accent, float uiScale)
        {
            DrawPanel(spriteBatch, bounds, PanelTone.Sunken, uiScale, false);

            int notch = UiTheme.Px(UiTheme.SpaceSm, uiScale);
            spriteBatch.Draw(_textures.Pixel,
                new Rectangle(bounds.X, bounds.Y, Math.Max(1, notch),
                    bounds.Height), accent);
            spriteBatch.Draw(_textures.Pixel,
                new Rectangle(bounds.Right - Math.Max(1, notch), bounds.Y,
                    Math.Max(1, notch), bounds.Height), accent);

            _fonts.DrawDisplayCentered(spriteBatch, title, bounds, accent,
                UiTheme.Label, uiScale, true, 1.2f);
        }

        public void DrawBadge(SpriteBatch spriteBatch, Vector2 center, float radius,
            string text, Color fill, Color border, float uiScale,
            float fontSize = UiTheme.NumericSmall)
        {
            var bounds = new Rectangle((int)(center.X - radius),
                (int)(center.Y - radius), (int)(radius * 2f), (int)(radius * 2f));

            // Un hexagono de poder dibujado con el panel 9-slice no aplica;
            // se usa un cuadrado biselado compacto.
            _textures.DrawPanel(spriteBatch, bounds, border);
            _textures.DrawPanel(spriteBatch, UiTheme.Inset(bounds, uiScale), fill);
            _fonts.DrawRoleCentered(spriteBatch, TextRole.Numeric, text, bounds,
                UiTheme.TextPrimary, fontSize, uiScale);
        }

        public void DrawStateTag(SpriteBatch spriteBatch, Rectangle bounds,
            string text, Color color, float uiScale)
        {
            spriteBatch.Draw(_textures.Pixel, bounds, color * 0.18f);
            _fonts.DrawRoleFit(spriteBatch, TextRole.Ui, text, bounds, color,
                UiTheme.Caption, uiScale);
        }

        // ------------------------------------------------------------------
        // Progreso
        // ------------------------------------------------------------------

        public void DrawProgressSlots(SpriteBatch spriteBatch, Rectangle bounds,
            int count, int filled, int current, Color accent, float uiScale)
        {
            if (count <= 0 || bounds.Width <= 0) return;

            int gap = UiTheme.Px(UiTheme.SpaceXs, uiScale);
            int slotWidth = Math.Max(4,
                (bounds.Width - gap * (count - 1)) / count);
            int height = Math.Max(4, Math.Min(bounds.Height,
                (int)(slotWidth * 1.35f)));

            for (int i = 0; i < count; i++)
            {
                var slot = new Rectangle(bounds.X + i * (slotWidth + gap),
                    bounds.Y + (bounds.Height - height) / 2, slotWidth, height);

                bool isFilled = i < filled;
                bool isCurrent = i == current;

                Color fill = isFilled
                    ? Color.Lerp(accent, Color.Black, 0.35f)
                    : UiTheme.PanelSunken;
                Color border = isCurrent
                    ? UiTheme.Gold
                    : isFilled ? accent : UiTheme.Border;

                _textures.DrawPanel(spriteBatch, slot, border);
                _textures.DrawPanel(spriteBatch, UiTheme.Inset(slot, uiScale), fill);

                if (isCurrent)
                {
                    int inset = UiTheme.Px(3, uiScale);
                    spriteBatch.Draw(_textures.Pixel,
                        new Rectangle(slot.X, slot.Bottom + inset, slot.Width,
                            Math.Max(1, UiTheme.Px(2, uiScale))),
                        UiTheme.Gold);
                }
            }
        }

        // ------------------------------------------------------------------
        // Overlays y glow
        // ------------------------------------------------------------------

        public void DrawOverlay(SpriteBatch spriteBatch, Rectangle bounds,
            float dim)
        {
            spriteBatch.Draw(_textures.Pixel, bounds, Color.Black * dim);
            _textures.DrawVignette(spriteBatch, bounds);
        }

        /// <summary>
        /// Cierra el batch normal, abre uno aditivo y deja listo el batch
        /// normal al terminar. Permite agrupar varios glows en una pasada.
        /// </summary>
        public void BeginAdditive(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
        }

        public void EndAdditive(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin();
        }

        public void DrawGlowRaw(SpriteBatch spriteBatch, Vector2 center,
            float radius, Color color)
        {
            _textures.DrawGlow(spriteBatch, center, radius, color);
        }

        /// <summary>Pasada aditiva autocontenida para un unico glow.</summary>
        public void DrawGlow(SpriteBatch spriteBatch, Vector2 center, float radius,
            Color color)
        {
            BeginAdditive(spriteBatch);
            _textures.DrawGlow(spriteBatch, center, radius, color);
            EndAdditive(spriteBatch);
        }

        // ------------------------------------------------------------------
        // Separadores
        // ------------------------------------------------------------------

        public void DrawDivider(SpriteBatch spriteBatch, Rectangle bounds,
            Color color, float uiScale)
        {
            int thickness = Math.Max(1, UiTheme.Px(1, uiScale));
            spriteBatch.Draw(_textures.Pixel,
                new Rectangle(bounds.X, bounds.Center.Y, bounds.Width, thickness),
                color * 0.55f);
        }
    }
}
