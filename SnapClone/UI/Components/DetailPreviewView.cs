using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Layout;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    public enum DetailKind
    {
        Card,
        Zone
    }

    /// <summary>
    /// Panel de detalle a pantalla completa para una carta o una zona.
    /// Solo lee el estado: abrirlo nunca mueve una carta ni gasta energia.
    /// </summary>
    public class DetailPreviewView
    {
        private readonly CardView _cardView = new CardView(null, Vector2.Zero);

        public bool IsOpen { get; private set; }
        public DetailKind Kind { get; private set; } = DetailKind.Card;
        public CardInstance Card { get; private set; }
        public ZoneInstance Zone { get; private set; }
        public BoardSide Side { get; private set; } = BoardSide.Player;
        public string CardStatus { get; private set; }
        public string ZoneStatus { get; private set; }
        public bool CanActivate { get; set; }

        public void OpenCard(CardInstance card, BoardSide side,
            string status = null)
        {
            if (card == null) return;

            Card = card;
            Side = side;
            Zone = null;
            CardStatus = status;
            ZoneStatus = null;
            Kind = DetailKind.Card;
            IsOpen = true;
        }

        public void OpenZone(ZoneInstance zone, string status = null)
        {
            if (zone == null) return;

            Zone = zone;
            Card = null;
            CardStatus = null;
            ZoneStatus = status;
            Kind = DetailKind.Zone;
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
            Card = null;
            Zone = null;
            CardStatus = null;
            ZoneStatus = null;
        }

        public Rectangle GetBounds(LayoutManager layout) => layout.DetailPanel;

        public Rectangle GetCloseBounds(LayoutManager layout) => layout.DetailPanelClose;

        public Rectangle GetActivateBounds(LayoutManager layout)
        {
            var panel = layout.DetailPanel;
            int pad = Math.Max(10, panel.Width / 28);
            return new Rectangle(panel.X + pad * 2,
                panel.Bottom - pad * 5 - 48, panel.Width - pad * 4, 40);
        }

        public void Draw(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            LayoutManager layout)
        {
            if (!IsOpen) return;

            shapes.DrawRectangle(
                new Rectangle(0, 0, layout.ScreenWidth, layout.ScreenHeight),
                Color.Black * 0.72f);

            var panel = layout.DetailPanel;
            var accent = Kind == DetailKind.Zone
                ? new Color(190, 142, 56)
                : (Side == BoardSide.Player
                    ? new Color(72, 126, 210)
                    : new Color(188, 84, 62));

            shapes.DrawBeveledPanel(panel, new Color(10, 14, 24) * 0.98f, accent, 3, 16);

            DrawClose(spriteBatch, shapes, fonts, layout);

            if (Kind == DetailKind.Card)
                DrawCard(spriteBatch, shapes, fonts, panel);
            else
                DrawZone(spriteBatch, shapes, fonts, panel);
        }

        private void DrawClose(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            LayoutManager layout)
        {
            var close = layout.DetailPanelClose;
            float radius = Math.Max(10f, close.Width * 0.5f);
            var center = new Vector2(close.Center.X, close.Center.Y);

            shapes.DrawHexagonFramed(center, radius,
                new Color(150, 40, 40), new Color(240, 120, 110), 2.5f, false);
            fonts.DrawTextCentered(spriteBatch, "X", close, Color.White, 0.85f);
        }

        private void DrawCard(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            Rectangle panel)
        {
            int pad = Math.Max(10, panel.Width / 28);
            int top = panel.Y + pad * 2;
            int bottom = panel.Bottom - pad * 5 - (CanActivate ? 52 : 0);
            int contentHeight = Math.Max(20, bottom - top);

            var cardArea = new Rectangle(
                panel.X + pad,
                top,
                panel.Width - pad * 2,
                (int)(contentHeight * 0.66f));

            int cardHeight = cardArea.Height;
            int cardWidth = (int)(cardHeight / 1.42f);

            if (cardWidth > cardArea.Width)
            {
                cardWidth = cardArea.Width;
                cardHeight = (int)(cardWidth * 1.42f);
            }

            var cardRect = new Rectangle(
                cardArea.Center.X - cardWidth / 2,
                cardArea.Y,
                cardWidth,
                cardHeight);

            _cardView.Card = Card;
            _cardView.IsAffordable = true;
            _cardView.IsDragging = false;
            _cardView.SnapTo(cardRect);
            _cardView.DrawInHand(spriteBatch, shapes, fonts, cardRect);

            int textTop = cardRect.Bottom + pad;
            var theme = Card?.Definition?.DraftTheme;
            if (theme.HasValue)
            {
                var themeRect = new Rectangle(
                    panel.X + pad * 2,
                    textTop,
                    panel.Width - pad * 4,
                    Math.Max(18, pad));

                fonts.DrawTextCenteredFit(spriteBatch,
                    $"TEMA: {DraftThemeText.Label(theme.Value)}",
                    themeRect, CardView.DraftThemeColor(theme.Value), 0.55f, 0.32f);

                textTop = themeRect.Bottom + pad / 2;
            }

            if (!string.IsNullOrWhiteSpace(CardStatus))
            {
                var statusRect = new Rectangle(
                    panel.X + pad * 2,
                    textTop,
                    panel.Width - pad * 4,
                    Math.Max(18, pad));

                fonts.DrawTextCenteredFit(spriteBatch, CardStatus, statusRect,
                    new Color(255, 200, 120), 0.5f, 0.28f);

                textTop = statusRect.Bottom + pad / 2;
            }

            var textArea = new Rectangle(
                panel.X + pad * 2,
                textTop,
                panel.Width - pad * 4,
                Math.Max(20, bottom - textTop));

            string description = Card?.AbilityTextOverride;
            if (string.IsNullOrEmpty(description))
                description = Card?.Definition?.Description;
            if (string.IsNullOrEmpty(description))
                description = "Sin descripcion.";
            if (Card?.CopiedEffectIds != null && Card.CopiedEffectIds.Count > 0)
                description = "COPIA. " + description;

            fonts.DrawTextWrapped(spriteBatch, description, textArea,
                new Color(236, 238, 244), 0.5f);

            if (CanActivate)
            {
                var button = new Rectangle(panel.X + pad * 2,
                    panel.Bottom - pad * 5 - 48, panel.Width - pad * 4, 40);
                shapes.DrawBeveledPanel(button, new Color(30, 104, 74),
                    new Color(126, 214, 170), 3, 10);
                fonts.DrawTextCentered(spriteBatch, "ACTIVAR", button,
                    Color.White, 0.7f);
            }
        }

        private void DrawZone(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            Rectangle panel)
        {
            if (Zone == null) return;

            int pad = Math.Max(10, panel.Width / 28);
            int top = panel.Y + pad * 2;
            int bottom = panel.Bottom - pad * 5;
            int contentHeight = Math.Max(20, bottom - top);

            int artHeight = (int)(contentHeight * 0.44f);
            var art = new Rectangle(
                panel.X + pad * 2,
                top,
                panel.Width - pad * 4,
                artHeight);

            shapes.DrawVerticalGradient(art, new Color(30, 40, 62), new Color(12, 16, 28), 16);

            bool visible = Zone.IsVisibleTo(BoardSide.Player);

            if (visible)
                LocationView.DrawLocationArt(shapes, art, Zone.ZoneIndex, 1f);

            int nameTop = art.Bottom + pad;
            int nameHeight = Math.Max(24, (int)(contentHeight * 0.10f));
            var nameRect = new Rectangle(panel.X + pad, nameTop, panel.Width - pad * 2, nameHeight);

            if (visible)
            {
                fonts.DrawTextCenteredFit(spriteBatch,
                    Zone.Definition != null ? Zone.Definition.Name : "LOCACION",
                    nameRect, new Color(240, 226, 190), 0.9f, 0.45f);
            }
            else
            {
                fonts.DrawTextCenteredFit(spriteBatch, "LOCACION DESCONOCIDA",
                    nameRect, new Color(170, 180, 198), 0.72f, 0.42f);
            }

            int effectTop = nameRect.Bottom + pad / 2;

            if (visible && !string.IsNullOrWhiteSpace(ZoneStatus))
            {
                var statusRect = new Rectangle(
                    panel.X + pad * 2,
                    effectTop,
                    panel.Width - pad * 4,
                    Math.Max(18, pad));

                fonts.DrawTextCenteredFit(spriteBatch, ZoneStatus, statusRect,
                    new Color(255, 200, 120), 0.5f, 0.28f);

                effectTop = statusRect.Bottom + pad / 2;
            }

            int totalsHeight = Math.Max(44, (int)(contentHeight * 0.17f));
            int effectBottom = bottom - totalsHeight - pad;
            var effectArea = new Rectangle(
                panel.X + pad * 3,
                effectTop,
                panel.Width - pad * 6,
                Math.Max(20, effectBottom - effectTop));

            string effectText;
            Color effectColor;

            if (visible)
            {
                effectText = Zone.Definition != null ? Zone.Definition.Description : string.Empty;
                effectColor = new Color(214, 222, 236);
            }
            else
            {
                effectText = $"Se revela en el turno {Zone.ZoneIndex + 1}.";
                effectColor = new Color(170, 180, 198);
            }

            fonts.DrawTextWrapped(spriteBatch, effectText, effectArea, effectColor, 0.5f);

            DrawTotals(spriteBatch, shapes, fonts, panel, bottom, pad);
        }

        private void DrawTotals(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            Rectangle panel, int bottom, int pad)
        {
            float radius = Math.Max(14f, panel.Width * 0.055f);
            float centerY = bottom - radius;

            var playerCenter = new Vector2(panel.X + panel.Width * 0.24f, centerY);
            var rivalCenter = new Vector2(panel.X + panel.Width * 0.76f, centerY);

            shapes.DrawHexagonFramed(playerCenter, radius,
                new Color(44, 96, 196), new Color(168, 204, 255), 2f, false);
            fonts.DrawTextCentered(spriteBatch, Zone.GetPlayerPower().ToString(),
                playerCenter, Color.White, 0.85f);

            shapes.DrawHexagonFramed(rivalCenter, radius,
                new Color(186, 74, 48), new Color(255, 190, 150), 2f, false);
            fonts.DrawTextCentered(spriteBatch, Zone.GetAIPower().ToString(),
                rivalCenter, Color.White, 0.85f);

            fonts.DrawTextCentered(spriteBatch, "JUGADOR",
                new Vector2(playerCenter.X, playerCenter.Y - radius - 9f),
                new Color(150, 170, 210), 0.4f);
            fonts.DrawTextCentered(spriteBatch, "RIVAL",
                new Vector2(rivalCenter.X, rivalCenter.Y - radius - 9f),
                new Color(210, 160, 150), 0.4f);
        }
    }
}
