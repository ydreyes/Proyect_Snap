using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    /// <summary>
    /// Retrato hexagonal con banner de nombre, usado para el jugador
    /// y el oponente en la cabecera de la escena de combate.
    /// </summary>
    public class AvatarView
    {
        public string Name { get; set; } = "JUGADOR";
        public Rectangle Bounds { get; set; }
        public bool IsPlayer { get; set; }
        public Color Accent { get; set; } = new Color(72, 126, 210);

        /// <summary>Zona clicable del avatar: el recuadro completo del retrato.</summary>
        public bool Contains(Vector2 point) =>
            Bounds.Contains((int)point.X, (int)point.Y);

        public void Draw(SpriteBatch spriteBatch, ShapeDrawer shapes, FontService fonts)
        {
            float radius = Bounds.Height * 0.46f;
            var center = new Vector2(Bounds.Center.X, Bounds.Y + radius + 2);

            // Marco exterior + relleno interior (retrato)
            shapes.DrawHexagonFramed(center, radius,
                new Color(26, 30, 46), new Color(214, 182, 96), 4f, true);

            shapes.DrawHexagon(center, radius * 0.72f,
                IsPlayer ? new Color(38, 62, 110) : new Color(74, 40, 58), true);

            // Inicial del nombre como "retrato" provisional
            string initial = string.IsNullOrEmpty(Name) ? "?" : Name.Substring(0, 1);
            fonts.DrawTextCentered(spriteBatch, initial, center, new Color(226, 218, 198), 1.9f);

            // Banner de nombre
            var banner = new Rectangle(
                Bounds.X,
                (int)(center.Y + radius * 0.72f),
                Bounds.Width,
                (int)(Bounds.Height * 0.30f));

            shapes.DrawBeveledPanel(banner, new Color(18, 22, 38), Accent, 2, 8);
            fonts.DrawTextCenteredFit(spriteBatch, Name.ToUpperInvariant(), banner,
                Color.White, 0.75f, 0.38f);
        }
    }
}
