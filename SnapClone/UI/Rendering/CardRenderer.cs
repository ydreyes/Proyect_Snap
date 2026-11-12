using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;
using SnapClone.UI.Components;
using SnapClone.UI.Theme;

namespace SnapClone.UI.Rendering
{
    public enum CardFrameVariant
    {
        Draft,
        Collection,
        Store,
        Medallion,
        MiniDeck
    }

    /// <summary>Estados visuales compartidos por las pantallas de coleccion y tienda.</summary>
    public enum CardVisualState
    {
        Normal,
        Undiscovered,
        Owned,
        Upgraded,
        Sold,
        Unaffordable,
        Selected
    }

    public sealed class CardRenderOptions
    {
        public CardFrameVariant Variant { get; set; } = CardFrameVariant.Draft;
        public CardVisualState State { get; set; } = CardVisualState.Normal;
        public float Opacity { get; set; } = 1f;
        public string Price { get; set; }
        public string Tag { get; set; }
        public bool ShowPower { get; set; } = true;
        public bool ShowName { get; set; } = true;
    }

    /// <summary>
    /// Renderer de carta para las pantallas de jugador. Separa siempre arte,
    /// marco, badges y texto. No se usa en combate: CardView sigue intacto.
    /// </summary>
    public class CardRenderer
    {
        private readonly TextureCache _textures;
        private readonly ChromeRenderer _chrome;
        private readonly CardArtProvider _art;

        public CardRenderer(TextureCache textures, ChromeRenderer chrome,
            CardArtProvider art)
        {
            _textures = textures;
            _chrome = chrome;
            _art = art;
        }

        public CardArtProvider Art => _art;

        public void DrawCard(SpriteBatch spriteBatch, Rectangle bounds,
            CardDef card, CardRenderOptions options, float uiScale)
        {
            if (card == null || bounds.Width < 6 || bounds.Height < 6) return;

            DrawInternal(spriteBatch, bounds, card.Id, card.Name, card.Cost,
                card.BasePower, card.DraftTheme, options, uiScale);
        }

        public void DrawInstance(SpriteBatch spriteBatch, Rectangle bounds,
            CardInstance card, CardRenderOptions options, float uiScale)
        {
            if (card?.Definition == null || bounds.Width < 6 || bounds.Height < 6)
                return;

            DrawInternal(spriteBatch, bounds, card.Id, card.Definition.Name,
                card.EffectiveCost, card.EffectivePower,
                card.Definition.DraftTheme, options, uiScale);
        }

        private void DrawInternal(SpriteBatch spriteBatch, Rectangle bounds,
            string id, string name, int cost, int power, DraftTheme? theme,
            CardRenderOptions options, float uiScale)
        {
            options ??= new CardRenderOptions();
            float opacity = MathHelper.Clamp(options.Opacity, 0f, 1f);
            if (opacity <= 0.001f) return;

            float cardScale = uiScale * MathHelper.Clamp(
                bounds.Width / (160f * uiScale), 0.45f, 2.2f);

            bool undiscovered = options.State == CardVisualState.Undiscovered;
            Color border = BorderFor(options.State, theme);
            Color fill = options.Variant == CardFrameVariant.MiniDeck
                ? UiTheme.PanelSunken
                : UiTheme.Panel;

            // Sombra y marco (geometria compartida con el resto del chrome).
            _textures.DrawPanel(spriteBatch, UiTheme.ShadowOf(bounds, uiScale),
                Color.Black * (0.45f * opacity));
            _textures.DrawPanel(spriteBatch, bounds, border * opacity);
            _textures.DrawPanel(spriteBatch, UiTheme.Inset(bounds, uiScale),
                fill * opacity);

            int inset = Math.Max(1, UiTheme.Px(
                options.Variant == CardFrameVariant.MiniDeck ? 1 : 2, uiScale));
            var inner = new Rectangle(bounds.X + inset, bounds.Y + inset,
                Math.Max(1, bounds.Width - inset * 2),
                Math.Max(1, bounds.Height - inset * 2));

            if (undiscovered)
            {
                DrawUndiscovered(spriteBatch, inner, options, cardScale, opacity);
                return;
            }

            Rectangle artBounds = ArtBounds(inner, options.Variant, cardScale);
            DrawArt(spriteBatch, artBounds, id, theme, cost, opacity);
            DrawScrim(spriteBatch, artBounds, opacity);
            DrawStateTint(spriteBatch, bounds, options.State, opacity);

            if (options.Variant == CardFrameVariant.MiniDeck)
            {
                DrawBadges(spriteBatch, bounds, cost, power, options, cardScale,
                    opacity, compact: true);
                return;
            }

            if (options.Variant == CardFrameVariant.Medallion)
            {
                DrawMedallionPlate(spriteBatch, inner, artBounds, name, cost,
                    power, options, cardScale, opacity);
                return;
            }

            Rectangle plate = NamePlate(inner, options.Variant, cardScale);
            DrawNamePlate(spriteBatch, plate, name, options, cardScale, opacity);

            // El pie de precio va debajo de los badges para que el poder no
            // quede tapado en las variantes de tienda.
            DrawPriceFooter(spriteBatch, bounds, options, cardScale, opacity);
            DrawBadges(spriteBatch, bounds, cost, power, options, cardScale,
                opacity, compact: false);
            DrawCornerTag(spriteBatch, bounds, options, cardScale, opacity);
        }

        // ------------------------------------------------------------------
        // Capas
        // ------------------------------------------------------------------

        private void DrawArt(SpriteBatch spriteBatch, Rectangle artBounds,
            string id, DraftTheme? theme, int cost, float opacity)
        {
            Texture2D texture = _art.Get(id, theme, cost);
            if (texture == null) return;

            Rectangle source = new Rectangle(0, 0, texture.Width, texture.Height);
            if (_art.HasRealArt(id) || texture.Width != CardArtProvider.ArtWidth ||
                texture.Height != CardArtProvider.ArtHeight)
            {
                source = CardArtProvider.CropToFill(texture.Width, texture.Height,
                    artBounds);
            }

            spriteBatch.Draw(texture, artBounds, source, Color.White * opacity);
        }

        private void DrawScrim(SpriteBatch spriteBatch, Rectangle artBounds,
            float opacity)
        {
            int scrimHeight = Math.Max(2, (int)(artBounds.Height * 0.45f));
            var scrim = new Rectangle(artBounds.X,
                artBounds.Bottom - scrimHeight, artBounds.Width, scrimHeight);

            spriteBatch.Draw(_textures.FadeGradient, scrim, null,
                Color.Black * (0.80f * opacity), 0f, Vector2.Zero,
                SpriteEffects.FlipVertically, 0f);
        }

        private void DrawUndiscovered(SpriteBatch spriteBatch, Rectangle inner,
            CardRenderOptions options, float cardScale, float opacity)
        {
            spriteBatch.Draw(_textures.Pixel, inner,
                new Color(10, 12, 20) * (0.92f * opacity));

            // Silueta y signo de interrogacion.
            var dark = new Rectangle(inner.X + inner.Width / 4,
                inner.Y + inner.Height / 5, inner.Width / 2,
                inner.Height / 2);
            _textures.DrawPanel(spriteBatch, dark,
                UiTheme.Border * (0.55f * opacity));

            _chrome.Fonts.DrawRoleCentered(spriteBatch, TextRole.Display, "?",
                new Rectangle(inner.X, inner.Y + inner.Height / 6, inner.Width,
                    inner.Height / 2), UiTheme.TextMuted * opacity,
                34f, cardScale, 0f);

            _chrome.Fonts.DrawRoleFit(spriteBatch, TextRole.Ui, "DESCONOCIDA",
                new Rectangle(inner.X, inner.Bottom - (int)(inner.Height * 0.22f),
                    inner.Width, (int)(inner.Height * 0.18f)),
                UiTheme.TextMuted * opacity, UiTheme.Caption, cardScale);
        }

        private void DrawStateTint(SpriteBatch spriteBatch, Rectangle bounds,
            CardVisualState state, float opacity)
        {
            switch (state)
            {
                case CardVisualState.Sold:
                    spriteBatch.Draw(_textures.Pixel, bounds,
                        Color.Black * (0.55f * opacity));
                    break;

                case CardVisualState.Unaffordable:
                    spriteBatch.Draw(_textures.Pixel, bounds,
                        UiTheme.Danger * (0.10f * opacity));
                    break;

                case CardVisualState.Selected:
                    spriteBatch.Draw(_textures.Pixel, bounds,
                        UiTheme.Gold * (0.10f * opacity));
                    break;
            }
        }

        private void DrawMedallionPlate(SpriteBatch spriteBatch, Rectangle inner,
            Rectangle artBounds, string name, int cost, int power,
            CardRenderOptions options, float cardScale, float opacity)
        {
            int plateHeight = Math.Max(1, (int)(inner.Height * 0.24f));
            var plate = new Rectangle(inner.X, inner.Bottom - plateHeight,
                inner.Width, plateHeight);
            spriteBatch.Draw(_textures.Pixel, plate,
                UiTheme.PanelSunken * (0.90f * opacity));

            _chrome.Fonts.DrawRoleFit(spriteBatch, TextRole.Ui,
                FitName(name, Math.Max(1, plate.Width - 4), cardScale),
                new Rectangle(plate.X + 2, plate.Y + 2,
                    Math.Max(1, plate.Width - 4),
                    (int)(plate.Height * 0.56f)),
                UiTheme.TextPrimary * opacity, UiTheme.LabelSmall, cardScale);

            _chrome.Fonts.DrawRoleCentered(spriteBatch, TextRole.Numeric,
                $"{cost}/{power}",
                new Rectangle(plate.X, plate.Y + (int)(plate.Height * 0.52f),
                    plate.Width, (int)(plate.Height * 0.44f)),
                UiTheme.TextSecondary * opacity, UiTheme.Caption, cardScale);

            DrawBadges(spriteBatch, new Rectangle(inner.X, inner.Y,
                inner.Width, inner.Height), cost, power, options, cardScale,
                opacity, compact: true);
            DrawCornerTag(spriteBatch, new Rectangle(inner.X, inner.Y,
                inner.Width, inner.Height), options, cardScale, opacity);
        }

        private void DrawNamePlate(SpriteBatch spriteBatch, Rectangle plate,
            string name, CardRenderOptions options, float cardScale,
            float opacity)
        {
            if (plate.Width <= 0 || plate.Height <= 0) return;

            spriteBatch.Draw(_textures.Pixel, plate,
                UiTheme.PanelSunken * (0.92f * opacity));

            if (!options.ShowName) return;

            int pricePad = string.IsNullOrEmpty(options.Price) ? 0
                : (int)(plate.Height * 0.34f);
            int powerPad = options.ShowPower
                ? (int)(plate.Height * 0.48f)
                : 0;
            int nameWidth = Math.Max(1, plate.Width - 4 - powerPad);

            _chrome.Fonts.DrawRoleFit(spriteBatch, TextRole.Ui,
                FitName(name, nameWidth, cardScale),
                new Rectangle(plate.X + 2, plate.Y + 1, nameWidth,
                    Math.Max(1, plate.Height - pricePad - 2)),
                UiTheme.TextPrimary * opacity, UiTheme.LabelSmall, cardScale);
        }

        private void DrawBadges(SpriteBatch spriteBatch, Rectangle bounds,
            int cost, int power, CardRenderOptions options, float cardScale,
            float opacity, bool compact)
        {
            float radius = Math.Max(7f, bounds.Width * (compact ? 0.15f : 0.13f));
            var costCenter = new Vector2(bounds.X + radius * 0.86f,
                bounds.Y + radius * 0.86f);

            Color costColor = options.State == CardVisualState.Unaffordable
                ? UiTheme.Danger
                : UiTheme.Accent;
            _chrome.DrawBadge(spriteBatch, costCenter, radius, cost.ToString(),
                UiTheme.PanelSunken, costColor * opacity, cardScale,
                compact ? UiTheme.Caption : UiTheme.Caption);

            if (!options.ShowPower) return;

            var powerCenter = new Vector2(bounds.Right - radius * 0.86f,
                bounds.Bottom - radius * 0.86f);
            _chrome.DrawBadge(spriteBatch, powerCenter, radius, power.ToString(),
                UiTheme.PanelSunken, UiTheme.Gold * opacity, cardScale,
                compact ? UiTheme.Caption : UiTheme.Caption);
        }

        private void DrawCornerTag(SpriteBatch spriteBatch, Rectangle bounds,
            CardRenderOptions options, float cardScale, float opacity)
        {
            string tag = options.Tag;
            Color color = UiTheme.TextSecondary;

            switch (options.State)
            {
                case CardVisualState.Owned:
                    tag = tag ?? "EN COLECCION";
                    color = UiTheme.Gold;
                    break;
                case CardVisualState.Upgraded:
                    tag = tag ?? "MEJORADA";
                    color = UiTheme.Perk;
                    break;
                case CardVisualState.Sold:
                    tag = tag ?? "VENDIDA";
                    color = UiTheme.DangerSoft;
                    break;
                case CardVisualState.Selected:
                    tag = tag ?? "ELEGIDA";
                    color = UiTheme.Gold;
                    break;
            }

            if (string.IsNullOrEmpty(tag)) return;

            int height = Math.Max(9, (int)(bounds.Height * 0.10f));
            int width = Math.Min(bounds.Width - 4,
                Math.Max(20, (int)(bounds.Width * 0.62f)));
            var tagBounds = new Rectangle(bounds.Right - width - 2,
                bounds.Y + 2, width, height);

            _chrome.DrawStateTag(spriteBatch, tagBounds, tag, color * opacity,
                cardScale);
        }

        private void DrawPriceFooter(SpriteBatch spriteBatch, Rectangle bounds,
            CardRenderOptions options, float cardScale, float opacity)
        {
            if (string.IsNullOrEmpty(options.Price)) return;

            int height = Math.Max(10, (int)(bounds.Height * 0.13f));
            var priceBounds = new Rectangle(bounds.X + 2,
                bounds.Bottom - height - 2, bounds.Width - 4, height);
            var plate = new Rectangle(priceBounds.X, priceBounds.Y + 1,
                priceBounds.Width, priceBounds.Height - 1);

            // Reserva el hueco del badge de poder para que el precio y el
            // numero no se dibujen uno encima del otro.
            float radius = Math.Max(7f, bounds.Width * 0.13f);
            int badgeReserve = (int)(radius * 1.9f);
            var priceText = new Rectangle(priceBounds.X, priceBounds.Y,
                Math.Max(1, priceBounds.Width - badgeReserve),
                priceBounds.Height);

            spriteBatch.Draw(_textures.Pixel, plate,
                UiTheme.PanelSunken * (0.92f * opacity));
            _chrome.Fonts.DrawRoleCentered(spriteBatch, TextRole.Numeric,
                options.Price, priceText, UiTheme.Gold * opacity,
                UiTheme.NumericSmall, cardScale);
        }

        private string FitName(string name, int maxWidth, float uiScale)
        {
            if (string.IsNullOrEmpty(name) || maxWidth <= 0) return name;

            float width = _chrome.Fonts.MeasureRole(TextRole.Ui, name,
                UiTheme.LabelSmall, uiScale).X;
            if (width <= maxWidth) return name;

            string candidate = name;
            while (candidate.Length > 3)
            {
                candidate = candidate.Substring(0, candidate.Length - 1)
                    .TrimEnd();
                string shortened = candidate + "..";
                if (_chrome.Fonts.MeasureRole(TextRole.Ui, shortened,
                        UiTheme.LabelSmall, uiScale).X <= maxWidth)
                    return shortened;
            }

            return name.Substring(0, Math.Min(2, name.Length)) + "..";
        }

        // ------------------------------------------------------------------
        // Geometria
        // ------------------------------------------------------------------

        private static Rectangle ArtBounds(Rectangle inner,
            CardFrameVariant variant, float cardScale)
        {
            if (variant == CardFrameVariant.MiniDeck)
                return inner;

            if (variant == CardFrameVariant.Medallion)
            {
                int side = Math.Min(inner.Width, (int)(inner.Height * 0.76f));
                return new Rectangle(inner.X + (inner.Width - side) / 2,
                    inner.Y, side, side);
            }

            int nameHeight = (int)(inner.Height * (variant == CardFrameVariant.Draft
                ? 0.24f
                : 0.26f));
            return new Rectangle(inner.X, inner.Y,
                Math.Max(1, inner.Width),
                Math.Max(1, inner.Height - nameHeight));
        }

        private static Rectangle NamePlate(Rectangle inner,
            CardFrameVariant variant, float cardScale)
        {
            int height = (int)(inner.Height * (variant == CardFrameVariant.Draft
                ? 0.24f
                : 0.26f));
            return new Rectangle(inner.X, inner.Bottom - height,
                Math.Max(1, inner.Width), Math.Max(1, height));
        }

        private static Color BorderFor(CardVisualState state, DraftTheme? theme)
        {
            switch (state)
            {
                case CardVisualState.Owned: return UiTheme.Gold;
                case CardVisualState.Upgraded: return UiTheme.Perk;
                case CardVisualState.Sold: return UiTheme.TextMuted;
                case CardVisualState.Selected: return UiTheme.Gold;
                case CardVisualState.Undiscovered: return UiTheme.Border;
            }

            switch (theme)
            {
                case DraftTheme.Control: return new Color(96, 150, 230);
                case DraftTheme.Destroy: return new Color(220, 110, 100);
                case DraftTheme.OnReveal: return new Color(176, 140, 240);
                case DraftTheme.Neutral: return new Color(120, 200, 170);
                default: return UiTheme.BorderBright;
            }
        }
    }
}
