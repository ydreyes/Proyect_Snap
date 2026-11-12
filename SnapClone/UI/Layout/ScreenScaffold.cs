using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Components;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.UI.Layout
{
    /// <summary>Insets del area segura en pixeles de la superficie.</summary>
    public readonly struct ScreenInsets
    {
        public static readonly ScreenInsets None = new ScreenInsets(0, 0, 0, 0);

        public readonly int Left;
        public readonly int Top;
        public readonly int Right;
        public readonly int Bottom;

        public ScreenInsets(int left, int top, int right, int bottom)
        {
            Left = Math.Max(0, left);
            Top = Math.Max(0, top);
            Right = Math.Max(0, right);
            Bottom = Math.Max(0, bottom);
        }

        public bool IsZero =>
            Left == 0 && Top == 0 && Right == 0 && Bottom == 0;
    }

    /// <summary>
    /// Resuelve el viewport en cabecera, contenido y pie, ya descontados los
    /// insets del area segura. Todas las pantallas comparten esta geometria.
    /// </summary>
    public sealed class ScreenScaffold
    {
        public Rectangle Viewport { get; private set; }
        public Rectangle SafeBounds { get; private set; }
        public Rectangle Header { get; private set; }
        public Rectangle Content { get; private set; }
        public Rectangle Footer { get; private set; }
        public Rectangle ContentInner { get; private set; }
        public ScreenInsets Insets { get; private set; }
        public float UiScale { get; private set; }

        public int Gutter { get; private set; }
        public int HeaderHeight { get; private set; }
        public int FooterHeight { get; private set; }

        private ScreenScaffold()
        {
        }

        public static ScreenScaffold Create(Rectangle viewport,
            ScreenInsets insets = default, int headerDesign = 104,
            int footerDesign = 96)
        {
            var scaffold = new ScreenScaffold
            {
                Viewport = viewport,
                Insets = insets
            };

            var safe = new Rectangle(
                viewport.X + insets.Left,
                viewport.Y + insets.Top,
                Math.Max(0, viewport.Width - insets.Left - insets.Right),
                Math.Max(0, viewport.Height - insets.Top - insets.Bottom));

            scaffold.SafeBounds = safe;
            scaffold.UiScale = UiTheme.UiScale(safe.Width, safe.Height);
            scaffold.Gutter = UiTheme.Px(UiTheme.SpaceLg,
                scaffold.UiScale);

            int minHeight = Math.Max(1, safe.Height / 4);
            scaffold.HeaderHeight = Math.Min(minHeight,
                UiTheme.Px(headerDesign, scaffold.UiScale));
            scaffold.FooterHeight = Math.Min(minHeight,
                UiTheme.Px(footerDesign, scaffold.UiScale));

            scaffold.Header = new Rectangle(safe.X, safe.Y, safe.Width,
                scaffold.HeaderHeight);

            scaffold.Footer = new Rectangle(safe.X,
                safe.Bottom - scaffold.FooterHeight, safe.Width,
                scaffold.FooterHeight);

            int contentHeight = Math.Max(0, safe.Height - scaffold.HeaderHeight -
                scaffold.FooterHeight);

            scaffold.Content = new Rectangle(safe.X,
                safe.Y + scaffold.HeaderHeight, safe.Width, contentHeight);

            scaffold.ContentInner = new Rectangle(
                scaffold.Content.X + scaffold.Gutter,
                scaffold.Content.Y + UiTheme.Px(UiTheme.SpaceSm,
                    scaffold.UiScale),
                Math.Max(0, scaffold.Content.Width - scaffold.Gutter * 2),
                Math.Max(0, scaffold.Content.Height - UiTheme.Px(
                    UiTheme.SpaceMd, scaffold.UiScale)));

            return scaffold;
        }

        /// <summary>Dibuja fondo ambiental y devuelve el batch restaurado.</summary>
        public void DrawBackground(SpriteBatch spriteBatch,
            SpaceBackground background, TextureCache textures,
            float timeSeconds)
        {
            background.Draw(spriteBatch, textures, Viewport, timeSeconds);
        }

        /// <summary>Cabecera comun: titulo, subtitulo y chip de moneda.</summary>
        public void DrawHeader(SpriteBatch spriteBatch,
            ChromeRenderer chrome, string title, string subtitle,
            string chipIcon, string chipValue, string chipLabel, Color accent)
        {
            int pad = UiTheme.Px(UiTheme.SpaceMd, UiScale);
            int chipWidth = UiTheme.Px(164, UiScale);
            int chipHeight = UiTheme.Px(46, UiScale);

            var chipBounds = new Rectangle(
                Header.Right - pad - chipWidth,
                Header.Y + (Header.Height - chipHeight) / 2,
                chipWidth, chipHeight);

            var titleBounds = new Rectangle(Header.X + pad,
                Header.Y + UiTheme.Px(6, UiScale),
                Math.Max(0, chipBounds.X - Header.X - pad * 2),
                Header.Height - UiTheme.Px(10, UiScale));

            chrome.Fonts.DrawDisplay(spriteBatch, title,
                new Vector2(titleBounds.X, titleBounds.Y), accent,
                UiTheme.Heading, UiScale, true, 1.6f);

            if (!string.IsNullOrEmpty(subtitle))
            {
                chrome.Fonts.DrawLabel(spriteBatch, subtitle,
                    new Vector2(titleBounds.X,
                        titleBounds.Y + UiTheme.Px(34, UiScale)),
                    UiTheme.TextSecondary, UiTheme.Caption, UiScale);
            }

            if (!string.IsNullOrEmpty(chipValue))
            {
                chrome.DrawCurrencyChip(spriteBatch, chipBounds, chipIcon,
                    chipValue, chipLabel, accent, UiScale);
            }
        }

        /// <summary>Barra inferior con separador superior.</summary>
        public void DrawFooterChrome(SpriteBatch spriteBatch,
            ChromeRenderer chrome)
        {
            chrome.DrawDivider(spriteBatch,
                new Rectangle(Footer.X + Gutter, Footer.Y - 2,
                    Math.Max(0, Footer.Width - Gutter * 2), 4),
                UiTheme.Border, UiScale);
        }
    }
}
