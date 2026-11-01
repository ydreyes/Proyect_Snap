using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Layout;
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

        public void OpenCard(CardInstance card, BoardSide side)
        {
            if (card == null) return;

            Card = card;
            Side = side;
            Zone = null;
            Kind = DetailKind.Card;
            IsOpen = true;
        }

        public void OpenZone(ZoneInstance zone)
        {
            if (zone == null) return;

            Zone = zone;
            Card = null;
            Kind = DetailKind.Zone;
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
            Card = null;
            Zone = null;
        }

        public Rectangle GetBounds(LayoutManager layout) => layout.DetailPanel;

        public Rectangle GetCloseBounds(LayoutManager layout) => layout.DetailPanelClose;

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
            int bottom = panel.Bottom - pad * 5;
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

            var textArea = new Rectangle(
                panel.X + pad * 2,
                cardRect.Bottom + pad,
                panel.Width - pad * 4,
                Math.Max(20, bottom - cardRect.Bottom - pad));

            string description = Card?.Definition?.Description;
            if (string.IsNullOrEmpty(description))
                description = "Sin descripcion.";

            fonts.DrawTextWrapped(spriteBatch, description, textArea,
                new Color(236, 238, 244), 0.5f);
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

            if (Zone.IsRevealed)
                LocationView.DrawLocationArt(shapes, art, Zone.ZoneIndex, 1f);

            int nameTop = art.Bottom + pad;
            int nameHeight = Math.Max(24, (int)(contentHeight * 0.10f));
            var nameRect = new Rectangle(panel.X + pad, nameTop, panel.Width - pad * 2, nameHeight);

            if (Zone.IsRevealed)
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
            int totalsHeight = Math.Max(44, (int)(contentHeight * 0.17f));
            int effectBottom = bottom - totalsHeight - pad;
            var effectArea = new Rectangle(
                panel.X + pad * 3,
                effectTop,
                panel.Width - pad * 6,
                Math.Max(20, effectBottom - effectTop));

            string effectText;
            Color effectColor;

            if (Zone.IsRevealed)
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
