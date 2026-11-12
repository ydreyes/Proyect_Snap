using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnapClone.UI.Theme
{
    /// <summary>
    /// Unica fuente de tokens visuales para las pantallas de jugador:
    /// paleta, tipografia, espaciado, geometria y elevacion.
    /// </summary>
    public static class UiTheme
    {
        // ------------------------------------------------------------------
        // Paleta
        // ------------------------------------------------------------------

        public static readonly Color BackgroundTop = new Color(20, 14, 44);
        public static readonly Color BackgroundBottom = new Color(8, 6, 18);
        public static readonly Color BackgroundDeep = new Color(5, 5, 12);

        public static readonly Color Panel = new Color(18, 24, 38);
        public static readonly Color PanelRaised = new Color(26, 34, 52);
        public static readonly Color PanelSunken = new Color(12, 16, 26);
        public static readonly Color Border = new Color(58, 82, 116);
        public static readonly Color BorderBright = new Color(96, 138, 190);

        public static readonly Color Accent = new Color(76, 166, 255);
        public static readonly Color AccentSoft = new Color(140, 200, 255);
        public static readonly Color Gold = new Color(240, 192, 96);
        public static readonly Color GoldSoft = new Color(255, 224, 150);
        public static readonly Color Violet = new Color(176, 140, 240);
        public static readonly Color Perk = new Color(156, 216, 122);

        public static readonly Color Success = new Color(110, 224, 160);
        public static readonly Color Danger = new Color(230, 92, 78);
        public static readonly Color DangerSoft = new Color(248, 160, 146);

        public static readonly Color TextPrimary = new Color(237, 230, 210);
        public static readonly Color TextSecondary = new Color(168, 184, 206);
        public static readonly Color TextMuted = new Color(126, 140, 162);
        public static readonly Color TextOnAccent = Color.White;

        // Estados semanticos compartidos.
        public static readonly Color StateAffordable = Success;
        public static readonly Color StateUnaffordable = Danger;
        public static readonly Color StateSelected = Gold;
        public static readonly Color StateSold = TextMuted;
        public static readonly Color StateDangerous = Danger;

        // ------------------------------------------------------------------
        // Escala tipografica (unidades de diseno 720x1280)
        // ------------------------------------------------------------------

        public const float DisplayXl = 74f;
        public const float Display = 50f;
        public const float Heading = 37f;
        public const float Label = 26f;
        public const float LabelSmall = 21f;
        public const float Body = 20f;
        public const float Caption = 16f;
        public const float Numeric = 39f;
        public const float NumericSmall = 28f;

        public const float DisplayTracking = 2.5f;

        // ------------------------------------------------------------------
        // Espaciado y geometria (unidades de diseno)
        // ------------------------------------------------------------------

        public const int SpaceXs = 4;
        public const int SpaceSm = 8;
        public const int SpaceMd = 12;
        public const int SpaceLg = 18;
        public const int SpaceXl = 28;
        public const int SpaceXxl = 40;

        public const int CornerCut = 10;
        public const int BorderThickness = 2;
        public const int TouchTarget = 44;
        public const int ElevationShadow = 3;

        // ------------------------------------------------------------------
        // Escalado
        // ------------------------------------------------------------------

        /// <summary>
        /// Factor de las unidades de diseno para el viewport actual.
        /// Mantiene la legibilidad en telefonos mas grandes sin romper el
        /// diseno base de 720x1280.
        /// </summary>
        public static float UiScale(Viewport viewport) =>
            UiScale(viewport.Width, viewport.Height);

        public static float UiScale(int width, int height)
        {
            if (width <= 0 || height <= 0) return 1f;

            float scale = MathF.Min(width / 720f, height / 1280f);
            return MathHelper.Clamp(scale, 0.75f, 2f);
        }

        public static int Px(float designUnits, float uiScale) =>
            Math.Max(1, (int)MathF.Round(designUnits * uiScale));

        // ------------------------------------------------------------------
        // Contraste (WCAG 2.0)
        // ------------------------------------------------------------------

        public static double RelativeLuminance(Color color)
        {
            double Channel(byte value)
            {
                double s = value / 255.0;
                return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * Channel(color.R) +
                   0.7152 * Channel(color.G) +
                   0.0722 * Channel(color.B);
        }

        public static double ContrastRatio(Color a, Color b)
        {
            double la = RelativeLuminance(a);
            double lb = RelativeLuminance(b);
            double lighter = Math.Max(la, lb);
            double darker = Math.Min(la, lb);
            return (lighter + 0.05) / (darker + 0.05);
        }

        // ------------------------------------------------------------------
        // Elevacion
        // ------------------------------------------------------------------

        public static Rectangle ShadowOf(Rectangle bounds, float uiScale)
        {
            int offset = Px(ElevationShadow, uiScale);
            return new Rectangle(bounds.X + offset, bounds.Y + offset,
                bounds.Width, bounds.Height);
        }

        public static Rectangle Inset(Rectangle bounds, float uiScale) =>
            new Rectangle(bounds.X + Px(BorderThickness, uiScale),
                bounds.Y + Px(BorderThickness, uiScale),
                bounds.Width - Px(BorderThickness, uiScale) * 2,
                bounds.Height - Px(BorderThickness, uiScale) * 2);
    }
}
