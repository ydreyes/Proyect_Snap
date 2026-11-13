using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Layout;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    /// <summary>
    /// Popover que muestra las pilas de un lado. Se abre desde el avatar,
    /// conserva la pila y la pagina seleccionadas y solo lee el estado: nunca
    /// mueve cartas.
    /// </summary>
    public class PileViewerView
    {
        public const int CardsPerPage = 3;

        private static readonly PileKind[] Tabs =
        {
            PileKind.Discard,
            PileKind.Destroyed,
            PileKind.Banished
        };

        public static int TabCount => Tabs.Length;

        public static PileKind TabAt(int index) => Tabs[index];

        private readonly CardView _cardView = new CardView(null, Vector2.Zero);

        public bool IsOpen { get; private set; }
        public BoardSide Side { get; private set; } = BoardSide.Player;
        public PileKind SelectedPile { get; private set; } = PileKind.Discard;
        public int Page { get; private set; }

        public void Open(BoardSide side)
        {
            IsOpen = true;
            Side = side;
            SelectedPile = PileKind.Discard;
            Page = 0;
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void Select(PileKind pile)
        {
            SelectedPile = pile;
            Page = 0;
        }

        public void PreviousPage() => Page = Math.Max(0, Page - 1);

        public void NextPage(int cardCount) =>
            Page = Math.Min(Page + 1, Math.Max(1, PageCount(cardCount)) - 1);

        public int PageCount(int cardCount) =>
            Math.Max(1, (cardCount + CardsPerPage - 1) / CardsPerPage);

        public void ClampPage(int cardCount) =>
            Page = Math.Max(0, Math.Min(Page, PageCount(cardCount) - 1));

        public Rectangle GetBounds(LayoutManager layout) =>
            layout.GetPileViewerBounds(Side == BoardSide.Player);

        public Rectangle GetTabBounds(LayoutManager layout, int index)
        {
            var tabs = layout.GetPileViewerTabs(Side == BoardSide.Player);
            int gap = Math.Max(3, tabs.Width / 36);
            int width = (tabs.Width - gap * (Tabs.Length - 1)) / Tabs.Length;
            int x = tabs.X + index * (width + gap);

            return new Rectangle(x, tabs.Y, width, tabs.Height);
        }

        public Rectangle GetPreviousBounds(LayoutManager layout)
        {
            var header = layout.GetPileViewerHeader(Side == BoardSide.Player);
            int size = header.Height;
            return new Rectangle(header.Right - size * 2 - 8, header.Y, size, size);
        }

        public Rectangle GetNextBounds(LayoutManager layout)
        {
            var header = layout.GetPileViewerHeader(Side == BoardSide.Player);
            int size = header.Height;
            return new Rectangle(header.Right - size, header.Y, size, size);
        }

        public void Draw(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            LayoutManager layout, PileSet piles)
        {
            if (!IsOpen || piles == null) return;

            bool player = Side == BoardSide.Player;
            var accent = player ? new Color(72, 126, 210) : new Color(188, 84, 62);
            var bounds = layout.GetPileViewerBounds(player);
            var header = layout.GetPileViewerHeader(player);
            var cardArea = layout.GetPileViewerCardArea(player);

            shapes.DrawBeveledPanel(bounds, new Color(12, 16, 28) * 0.97f, accent, 3, 12);

            int count = piles.Count(SelectedPile);
            ClampPage(count);

            DrawHeader(spriteBatch, shapes, fonts, layout, header, accent, count);
            DrawCards(spriteBatch, shapes, fonts, cardArea, piles);
            DrawTabs(spriteBatch, shapes, fonts, layout, piles, accent);
        }

        private void DrawHeader(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            LayoutManager layout, Rectangle header, Color accent, int count)
        {
            shapes.DrawRectangle(header, Color.Black * 0.25f);

            var prev = GetPreviousBounds(layout);
            var next = GetNextBounds(layout);

            int pageSize = PageCount(count);

            var pageRect = new Rectangle(prev.X - 64, header.Y, 60, header.Height);
            fonts.DrawTextCentered(spriteBatch, $"{Page + 1} / {pageSize}", pageRect,
                new Color(196, 214, 240), 0.62f);

            bool canPage = pageSize > 1;
            var arrowColor = canPage ? Color.White : new Color(120, 128, 140);

            shapes.DrawBeveledPanel(prev, new Color(24, 32, 52), canPage ? accent : new Color(60, 68, 82), 2, 6);
            shapes.DrawBeveledPanel(next, new Color(24, 32, 52), canPage ? accent : new Color(60, 68, 82), 2, 6);
            fonts.DrawTextCentered(spriteBatch, "<", prev, arrowColor, 0.9f);
            fonts.DrawTextCentered(spriteBatch, ">", next, arrowColor, 0.9f);

            var title = new Rectangle(header.X + 6, header.Y, Math.Max(10, pageRect.X - header.X - 10), header.Height);
            fonts.DrawTextCenteredFit(spriteBatch, SelectedPile.DisplayName(), title,
                new Color(232, 226, 210), 0.75f, 0.42f);
        }

        private void DrawCards(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            Rectangle area, PileSet piles)
        {
            var cards = piles.Get(SelectedPile);

            if (cards.Count == 0)
            {
                fonts.DrawTextCentered(spriteBatch, "SIN CARTAS", area, Color.White * 0.45f, 0.8f);
                return;
            }

            for (int i = 0; i < CardsPerPage; i++)
            {
                int index = Page * CardsPerPage + i;
                if (index >= cards.Count) break;

                var card = cards[cards.Count - 1 - index];
                var rect = GetCardRect(area, i);

                _cardView.Card = card;
                _cardView.IsAffordable = true;
                _cardView.SnapTo(rect);
                _cardView.DrawInHand(spriteBatch, shapes, fonts, rect);
            }
        }

        /// <summary>Carta visible bajo el puntero, o null si no hay ninguna.</summary>
        public CardInstance HitTestCard(LayoutManager layout, PileSet piles, Vector2 point)
        {
            if (!IsOpen || piles == null) return null;

            var cards = piles.Get(SelectedPile);
            if (cards.Count == 0) return null;

            var area = layout.GetPileViewerCardArea(Side == BoardSide.Player);

            for (int i = 0; i < CardsPerPage; i++)
            {
                int index = Page * CardsPerPage + i;
                if (index >= cards.Count) break;

                if (GetCardRect(area, i).Contains((int)point.X, (int)point.Y))
                    return cards[cards.Count - 1 - index];
            }

            return null;
        }

        private static Rectangle GetCardRect(Rectangle area, int index)
        {
            int gap = Math.Max(4, area.Width / 30);
            int cardWidth = (area.Width - gap * 4) / CardsPerPage;
            int cardHeight = (int)(cardWidth * 1.35f);

            if (cardHeight > area.Height)
            {
                cardHeight = area.Height;
                cardWidth = (int)(cardHeight / 1.35f);
            }

            int totalWidth = cardWidth * CardsPerPage + gap * (CardsPerPage - 1);
            int startX = area.X + (area.Width - totalWidth) / 2;
            int y = area.Y + (area.Height - cardHeight) / 2;

            return new Rectangle(startX + index * (cardWidth + gap), y, cardWidth, cardHeight);
        }

        private void DrawTabs(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts,
            LayoutManager layout, PileSet piles, Color accent)
        {            for (int i = 0; i < Tabs.Length; i++)
            {
                var pile = Tabs[i];
                var rect = GetTabBounds(layout, i);
                bool selected = pile == SelectedPile;

                shapes.DrawBeveledPanel(rect,
                    selected ? new Color(28, 44, 74) : new Color(18, 22, 36),
                    selected ? accent : new Color(58, 68, 88),
                    selected ? 3 : 2, 8);

                var nameRect = new Rectangle(rect.X + 2, rect.Y + (int)(rect.Height * 0.14f), rect.Width - 4, (int)(rect.Height * 0.42f));
                var countRect = new Rectangle(rect.X + 2, nameRect.Bottom, rect.Width - 4, (int)(rect.Height * 0.36f));

                fonts.DrawTextCenteredFit(spriteBatch, pile.DisplayName(), nameRect,
                    selected ? Color.White : new Color(190, 198, 212), 0.52f, 0.30f);

                fonts.DrawTextCentered(spriteBatch, piles.Count(pile).ToString(), countRect,
                    selected ? new Color(255, 214, 128) : new Color(150, 160, 178), 0.72f);
            }
        }
    }
}
