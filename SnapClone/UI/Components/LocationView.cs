using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    /// <summary>
    /// Una de las tres columnas de locacion: muestra la placa compacta entre
    /// dos rejillas 2x2 (rival arriba, jugador abajo) y los totales de poder.
    /// </summary>
    public class LocationView
    {
        public string Name { get; set; } = "LOCACION";
        public string EffectText { get; set; } = string.Empty;
        public int Index { get; set; }

        /// <summary>Una locacion oculta no muestra nombre, efecto ni arte.</summary>
        public bool IsRevealed { get; set; } = true;

        /// <summary>Estado corto de la locacion: INUNDADA, BLOQUEADA o vacio.</summary>
        public string StatusText { get; set; } = string.Empty;

        public Rectangle Panel { get; set; }
        public Rectangle[] OpponentSlots { get; set; } = new Rectangle[4];
        public Rectangle[] PlayerSlots { get; set; } = new Rectangle[4];

        /// <summary>Variante de arte derivada del id de la locacion.</summary>
        public int ArtVariant { get; set; }

        public static int ArtVariantFor(string zoneId)
        {
            if (string.IsNullOrEmpty(zoneId)) return 0;

            unchecked
            {
                int hash = 17;
                foreach (char c in zoneId)
                    hash = hash * 31 + c;

                return (hash & 0x7FFFFFFF) % 4;
            }
        }

        public const int SlotsPerSide = 4;
        public const int SlotsPerRow = 2;

        public CardView[] OpponentSlotCards { get; } = new CardView[SlotsPerSide];
        public CardView[] PlayerSlotCards { get; } = new CardView[SlotsPerSide];

        public int OpponentPower { get; private set; }
        public int PlayerPower { get; private set; }

        private static readonly Color PanelFill = new Color(17, 20, 34);
        private static readonly Color PanelBorder = new Color(190, 142, 56);
        private static readonly Color ArtTop = new Color(38, 58, 92);
        private static readonly Color ArtBottom = new Color(14, 18, 32);

        /// <summary>
        /// Copia los totales de la zona. Solo CombatScene.RefreshPowerTotals
        /// escribe estos valores; la vista no suma cartas por su cuenta.
        /// </summary>
        public void SetPower(int player, int opponent)
        {
            PlayerPower = player;
            OpponentPower = opponent;
        }

        public void Draw(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts)
        {
            DrawPanel(spriteBatch, shapes, fonts);
            DrawSlots(spriteBatch, shapes, fonts);
        }

        private void DrawPanel(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts)
        {
            // Marco exterior dorado
            shapes.DrawBeveledPanel(Panel, PanelFill, PanelBorder, 4, 14);

            int pad = System.Math.Max(3, Panel.Width / 30);
            var inner = new Rectangle(
                Panel.X + pad,
                Panel.Y + pad,
                Panel.Width - pad * 2,
                Panel.Height - pad * 2);

            if (inner.Width <= 4 || inner.Height <= 4) return;

            // El arte de la locacion es ahora una placa de fondo tenue,
            // no un bloque que se coma la mitad del panel.
            shapes.DrawVerticalGradient(inner,
                (IsRevealed ? ArtTop : PanelFill) * 0.45f,
                (IsRevealed ? ArtBottom : PanelFill) * 0.9f, 14);
            if (IsRevealed)
                DrawLocationArt(shapes, inner, ArtVariant, 0.55f);

            // Cabecera con el nombre (u oculta)
            int headerHeight = (int)(inner.Height * 0.26f);
            var header = new Rectangle(inner.X, inner.Y, inner.Width, headerHeight);
            fonts.DrawTextCenteredFit(spriteBatch, IsRevealed ? Name : "???", header,
                new Color(238, 224, 188), 0.72f, 0.38f);

            // Totales: dos hexagonos grandes en la banda inferior
            float radius = System.Math.Max(9f, System.Math.Min(inner.Width * 0.17f, inner.Height * 0.19f));
            int totalsTop = inner.Bottom - (int)(radius * 2f);

            int statusHeight = IsRevealed && !string.IsNullOrEmpty(StatusText)
                ? Math.Max(10, inner.Height / 18)
                : 0;

            var effectArea = new Rectangle(
                inner.X,
                header.Bottom,
                inner.Width,
                Math.Max(1, totalsTop - header.Bottom - 1 - statusHeight));

            if (IsRevealed && effectArea.Height > 8 && !string.IsNullOrEmpty(EffectText))
            {
                fonts.DrawTextWrapped(spriteBatch, EffectText, effectArea,
                    new Color(206, 214, 230), 0.48f);
            }
            else if (!IsRevealed && effectArea.Height > 8)
            {
                fonts.DrawTextCentered(spriteBatch, "? ? ?", effectArea,
                    new Color(120, 130, 150), 0.58f);
            }

            if (statusHeight > 0)
            {
                var statusRect = new Rectangle(inner.X, effectArea.Bottom + 1,
                    inner.Width, statusHeight);
                fonts.DrawTextCenteredFit(spriteBatch, StatusText, statusRect,
                    new Color(255, 200, 120), 0.48f, 0.30f);
            }

            float badgeY = totalsTop + radius;
            var playerBadge = new Vector2(inner.X + inner.Width * 0.26f, badgeY);
            shapes.DrawHexagonFramed(playerBadge, radius,
                new Color(44, 96, 196), new Color(168, 204, 255), 2f, false);
            fonts.DrawTextCentered(spriteBatch, PlayerPower.ToString(), playerBadge, Color.White, 0.95f);

            var opponentBadge = new Vector2(inner.X + inner.Width * 0.74f, badgeY);
            shapes.DrawHexagonFramed(opponentBadge, radius,
                new Color(186, 74, 48), new Color(255, 190, 150), 2f, false);
            fonts.DrawTextCentered(spriteBatch, OpponentPower.ToString(), opponentBadge, Color.White, 0.95f);

            // Hexagono con el indice de la locacion
            var indexCenter = new Vector2(Panel.Center.X, Panel.Y);
            float indexRadius = System.Math.Max(9f, Panel.Width * 0.065f);
            shapes.DrawHexagonFramed(indexCenter, indexRadius,
                new Color(224, 186, 96), new Color(90, 66, 28), 2f, false);
            fonts.DrawTextCentered(spriteBatch, (Index + 1).ToString(), indexCenter,
                new Color(28, 24, 16), 0.7f);
        }

        public static void DrawLocationArt(ShapeDrawer shapes, Rectangle art, int variant, float alpha)
        {
            if (art.Width <= 8 || art.Height <= 8) return;

            Color planet, glow, ground;

            switch (((variant % 4) + 4) % 4)
            {
                case 0:
                    planet = new Color(96, 132, 196);
                    glow = new Color(160, 198, 246);
                    ground = new Color(22, 30, 48);
                    break;
                case 1:
                    planet = new Color(150, 96, 152);
                    glow = new Color(216, 152, 212);
                    ground = new Color(38, 24, 42);
                    break;
                case 2:
                    planet = new Color(96, 168, 142);
                    glow = new Color(162, 228, 198);
                    ground = new Color(20, 42, 36);
                    break;
                default:
                    // Limbo: un sol naranja sobre un cielo violeta.
                    planet = new Color(232, 148, 72);
                    glow = new Color(255, 206, 138);
                    ground = new Color(44, 26, 40);
                    break;
            }

            var center = new Vector2(art.X + art.Width * 0.70f, art.Y + art.Height * 0.32f);
            float radius = art.Width * 0.20f;

            shapes.DrawCircle(center, radius * 1.35f, glow * (0.10f * alpha));
            shapes.DrawCircle(center, radius, planet * (0.8f * alpha));

            shapes.DrawCircle(
                new Vector2(center.X - radius * 0.22f, center.Y - radius * 0.26f),
                radius * 0.58f, glow * (0.16f * alpha));

            shapes.DrawCircle(
                new Vector2(art.X + art.Width * 0.24f, art.Y + art.Height * 0.18f),
                System.Math.Max(2f, art.Width * 0.04f), glow * (0.35f * alpha));
        }

        private void DrawSlots(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts)
        {
            for (int i = 0; i < OpponentSlots.Length; i++)
            {
                var opponentView = OpponentSlotCards[i];
                if (opponentView != null)
                    opponentView.Draw(spriteBatch, shapes, fonts, OpponentSlots[i], true);
                else
                    CardView.DrawEmptySlot(spriteBatch, shapes, OpponentSlots[i], true);
            }

            for (int i = 0; i < PlayerSlots.Length; i++)
            {
                var playerView = PlayerSlotCards[i];
                if (playerView != null)
                    playerView.Draw(spriteBatch, shapes, fonts, PlayerSlots[i], false);
                else
                    CardView.DrawEmptySlot(spriteBatch, shapes, PlayerSlots[i], false);
            }
        }
    }
}
