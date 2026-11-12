using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    public enum CardOrigin
    {
        Hand,
        Board
    }

    /// <summary>
    /// Representacion visual de una carta o de un slot vacio.
    /// Todo se dibuja con primitivas generadas en memoria.
    /// </summary>
    public class CardView
    {
        public CardInstance Card { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 OriginalPosition { get; set; }
        public Vector2 Size { get; set; }
        public bool IsDragging { get; set; }
        public bool IsInZone { get; set; }

        /// <summary>
        /// Indica si el jugador puede pagar la carta con la energia actual.
        /// Solo afecta al dibujo de la mano.
        /// </summary>
        public bool IsAffordable { get; set; } = true;

        /// <summary>Muestra la etiqueta del tema de draft (vista de draft).</summary>
        public bool ShowDraftTheme { get; set; }

        /// <summary>Muestra el indicador de carta movible en el tablero.</summary>
        public bool ShowMoveBadge { get; set; }

        /// <summary>De donde salio la carta en el arrastre actual.</summary>
        public CardOrigin Origin { get; set; } = CardOrigin.Hand;
        public int OriginZone { get; set; } = -1;
        public int OriginSlot { get; set; } = -1;

        public CardView(CardInstance card, Vector2 position)
        {
            Card = card;
            Position = position;
            OriginalPosition = position;
        }

        /// <summary>Centro de la carta, usado para el hit testing al soltar.</summary>
        public Vector2 Center => new Vector2(Position.X + Size.X / 2f, Position.Y + Size.Y / 2f);

        public void SnapTo(Rectangle bounds)
        {
            Size = new Vector2(bounds.Width, bounds.Height);
            Position = new Vector2(bounds.X, bounds.Y);
            OriginalPosition = Position;
        }

        public void EndDrag()
        {
            IsDragging = false;
        }

        /// <summary>Slot vacio: recuadro donde se puede soltar una carta.</summary>
        public static void DrawEmptySlot(SpriteBatch spriteBatch, ShapeDrawer shapes, Rectangle bounds, bool opponent)
        {
            var border = opponent
                ? new Color(150, 80, 70) * 0.65f
                : new Color(80, 120, 190) * 0.65f;

            shapes.DrawBeveledRectangle(bounds, Color.White * 0.05f, 8);
            shapes.DrawRectangleBorder(bounds, border, 1);

            // Esquinas marcadas para indicar zona de drop
            int mark = System.Math.Max(3, bounds.Width / 6);
            shapes.DrawRectangle(new Rectangle(bounds.X, bounds.Y, mark, 2), border);
            shapes.DrawRectangle(new Rectangle(bounds.X, bounds.Y, 2, mark), border);
            shapes.DrawRectangle(new Rectangle(bounds.Right - mark, bounds.Bottom - 2, mark, 2), border);
            shapes.DrawRectangle(new Rectangle(bounds.Right - 2, bounds.Bottom - mark, 2, mark), border);
        }

        /// <summary>Carta jugada en una locacion.</summary>
        public void Draw(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts, Rectangle bounds, bool opponent)
        {
            if (Card != null && Card.DelayedReveal)
            {
                shapes.DrawBeveledRectangle(
                    new Rectangle(bounds.X + 2, bounds.Y + 3, bounds.Width,
                        bounds.Height), Color.Black * 0.45f, 8);
                shapes.DrawBeveledPanel(bounds, new Color(14, 18, 30),
                    new Color(90, 104, 130), 3, 8);
                fonts.DrawTextCentered(spriteBatch, "???", bounds,
                    Color.White * 0.6f, 0.8f);
                return;
            }

            var frame = opponent
                ? new Color(188, 84, 62)
                : new Color(72, 126, 210);

            shapes.DrawBeveledRectangle(
                new Rectangle(bounds.X + 2, bounds.Y + 3, bounds.Width, bounds.Height),
                Color.Black * 0.45f, 8);

            shapes.DrawBeveledPanel(bounds, new Color(22, 25, 40), frame, 3, 8);

            // Banda de arte
            var art = new Rectangle(
                bounds.X + 4,
                bounds.Y + (int)(bounds.Height * 0.18f),
                bounds.Width - 8,
                (int)(bounds.Height * 0.56f));

            shapes.DrawVerticalGradient(art,
                opponent ? new Color(96, 44, 38) : new Color(34, 62, 108),
                new Color(18, 20, 34), 10);

            // Poder
            float orbRadius = bounds.Width * 0.26f;
            var powerCenter = new Vector2(bounds.Right - orbRadius * 0.75f, bounds.Y + orbRadius * 0.75f);
            int power = Card != null ? Card.CurrentPower : 0;

            shapes.DrawHexagonFramed(powerCenter, orbRadius,
                new Color(196, 96, 32), new Color(255, 205, 120), 2f, false);
            fonts.DrawTextCentered(spriteBatch, power.ToString(), powerCenter, Color.White, 0.62f);

            DrawPerkBadge(spriteBatch, shapes, fonts, powerCenter, orbRadius);
            DrawMoveBadge(spriteBatch, shapes, fonts, bounds, orbRadius);

            // Nombre (en el tablero el coste ya no se muestra)
            if (Card != null)
            {
                var nameRect = new Rectangle(bounds.X + 2, bounds.Bottom - (int)(bounds.Height * 0.22f), bounds.Width - 4, (int)(bounds.Height * 0.2f));
                shapes.DrawRectangle(nameRect, Color.Black * 0.55f);
                fonts.DrawTextCenteredFit(spriteBatch, Card.Definition.Name, nameRect, new Color(232, 226, 210), 0.34f, 0.22f);
            }
            else
            {
                fonts.DrawTextCentered(spriteBatch, "?", bounds, Color.White * 0.35f, 0.8f);
            }
        }

        /// <summary>Carta de la mano (muestra coste y poder).</summary>
        public void DrawInHand(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts, Rectangle bounds)
        {
            var frame = IsAffordable
                ? new Color(96, 132, 206)
                : new Color(92, 96, 108);

            shapes.DrawBeveledPanel(bounds, new Color(26, 30, 48), frame, 3, 10);

            var art = new Rectangle(
                bounds.X + 5,
                bounds.Y + (int)(bounds.Height * 0.20f),
                bounds.Width - 10,
                (int)(bounds.Height * 0.50f));

            shapes.DrawVerticalGradient(art, new Color(44, 74, 128), new Color(18, 22, 38), 10);

            float orbRadius = bounds.Width * 0.24f;
            var costCenter = new Vector2(bounds.X + orbRadius * 0.9f, bounds.Y + orbRadius * 0.9f);
            var powerCenter = new Vector2(bounds.Right - orbRadius * 0.9f, bounds.Y + orbRadius * 0.9f);
            int cost = Card != null ? Card.EffectiveCost : 0;
            int power = Card != null ? Card.CurrentPower : 0;

            shapes.DrawCircleFramed(costCenter, orbRadius, new Color(38, 86, 176), new Color(160, 200, 255), 2f);
            fonts.DrawTextCentered(spriteBatch, cost.ToString(), costCenter, Color.White, 0.6f);

            shapes.DrawHexagonFramed(powerCenter, orbRadius, new Color(196, 96, 32), new Color(255, 205, 120), 2f, false);
            fonts.DrawTextCentered(spriteBatch, power.ToString(), powerCenter, Color.White, 0.6f);

            DrawPerkBadge(spriteBatch, shapes, fonts, powerCenter, orbRadius);
            DrawUpgradeBadge(spriteBatch, shapes, fonts, bounds, orbRadius);
            DrawThemeBadge(spriteBatch, shapes, fonts, bounds);

            var nameRect = new Rectangle(bounds.X + 2, bounds.Bottom - (int)(bounds.Height * 0.24f), bounds.Width - 4, (int)(bounds.Height * 0.22f));
            shapes.DrawRectangle(nameRect, Color.Black * 0.5f);
            if (Card != null)
                fonts.DrawTextCentered(spriteBatch, Card.Definition.Name, nameRect, new Color(232, 226, 210), 0.4f);
            else
                fonts.DrawTextCentered(spriteBatch, "MANO", nameRect, Color.White * 0.4f, 0.36f);

            if (!IsAffordable)
                shapes.DrawRectangle(bounds, Color.Black * 0.45f);
        }

        /// <summary>Etiqueta del tema de draft sobre la banda de arte.</summary>
        private void DrawThemeBadge(SpriteBatch spriteBatch, ShapeDrawer shapes,
            FontService fonts, Rectangle bounds)
        {
            if (!ShowDraftTheme || Card?.Definition == null) return;
            if (!Card.Definition.DraftTheme.HasValue) return;

            var theme = Card.Definition.DraftTheme.Value;
            var themeRect = new Rectangle(
                bounds.X + 4,
                bounds.Bottom - (int)(bounds.Height * 0.325f),
                bounds.Width - 8,
                System.Math.Max(10, (int)(bounds.Height * 0.075f)));

            shapes.DrawRectangle(themeRect, Color.Black * 0.5f);
            fonts.DrawTextCenteredFit(spriteBatch, DraftThemeText.Label(theme),
                themeRect, DraftThemeColor(theme), 0.32f, 0.18f);
        }

        /// <summary>Color estable por tema para la etiqueta de draft.</summary>
        public static Color DraftThemeColor(DraftTheme theme)
        {
            switch (theme)
            {
                case DraftTheme.Control: return new Color(140, 190, 250);
                case DraftTheme.Destroy: return new Color(240, 150, 130);
                case DraftTheme.OnReveal: return new Color(180, 225, 155);
                default: return new Color(210, 210, 220);
            }
        }

        /// <summary>Indicador discreto de carta movible.</summary>
        private void DrawMoveBadge(SpriteBatch spriteBatch, ShapeDrawer shapes,
            FontService fonts, Rectangle bounds, float orbRadius)
        {
            if (!ShowMoveBadge || Card == null) return;

            var center = new Vector2(bounds.X + orbRadius * 0.9f,
                bounds.Y + orbRadius * 0.9f);
            float radius = orbRadius * 0.55f;

            shapes.DrawCircleFramed(center, radius,
                new Color(32, 92, 112), new Color(140, 230, 240), 1.5f);
            fonts.DrawTextCentered(spriteBatch, "M", center, Color.White, 0.30f);
        }

        /// <summary>Badge pequeno con el modificador total de los perks.</summary>
        private void DrawPerkBadge(SpriteBatch spriteBatch, ShapeDrawer shapes,
            FontService fonts, Vector2 powerCenter, float orbRadius)
        {
            if (Card == null || Card.PerkPowerBonus == 0) return;

            int bonus = Card.PerkPowerBonus;
            string text = bonus > 0 ? $"+{bonus}" : bonus.ToString();
            var center = new Vector2(powerCenter.X, powerCenter.Y + orbRadius * 1.25f);
            float radius = orbRadius * 0.58f;

            var fill = bonus > 0 ? new Color(38, 112, 72) : new Color(128, 52, 48);
            var border = bonus > 0 ? new Color(150, 230, 170) : new Color(240, 160, 150);

            shapes.DrawCircleFramed(center, radius, fill, border, 1.5f);
            fonts.DrawTextCentered(spriteBatch, text, center, Color.White, 0.34f);
        }

        /// <summary>Marca discreta de mejoras permanentes de la run.</summary>
        private void DrawUpgradeBadge(SpriteBatch spriteBatch, ShapeDrawer shapes,
            FontService fonts, Rectangle bounds, float orbRadius)
        {
            if (Card == null || !Card.HasUpgrades) return;

            var center = new Vector2(bounds.Center.X, bounds.Y + orbRadius * 0.9f);
            float radius = orbRadius * 0.52f;

            shapes.DrawCircleFramed(center, radius,
                new Color(112, 82, 24), new Color(240, 204, 120), 1.5f);
            fonts.DrawTextCentered(spriteBatch, $"+{Card.UpgradeCount}", center, Color.White, 0.30f);
        }
    }
}
