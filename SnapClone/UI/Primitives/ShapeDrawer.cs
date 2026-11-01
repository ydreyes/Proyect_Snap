using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnapClone.UI.Primitives
{
    /// <summary>
    /// Dibuja formas usando solo primitivas de MonoGame.
    /// Las texturas (hexagono, circulo, panel biselado) se generan
    /// proceduralmente en memoria: no se usa ninguna imagen externa.
    /// </summary>
    public class ShapeDrawer
    {
        private const int ShapeTexSize = 128;
        private const int PanelTexSize = 48;
        private const int PanelCut = 12;
        private const float Sqrt3Over2 = 0.8660254f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _pixel;
        private readonly Texture2D _hexPointy;
        private readonly Texture2D _hexFlat;
        private readonly Texture2D _circle;
        private readonly Texture2D _bevel;

        public ShapeDrawer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;

            _pixel = CreateSolidTexture();
            _hexPointy = CreateShapeTexture(ShapeTexSize, InsidePointyHex);
            _hexFlat = CreateShapeTexture(ShapeTexSize, InsideFlatHex);
            _circle = CreateShapeTexture(ShapeTexSize, InsideCircle);
            _bevel = CreateShapeTexture(PanelTexSize, InsideBevel);
        }

        // ------------------------------------------------------------------
        // Texturas procedurales
        // ------------------------------------------------------------------

        private Texture2D CreateSolidTexture()
        {
            var texture = new Texture2D(_graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }

        private Texture2D CreateShapeTexture(int size, Func<float, float, bool> inside)
        {
            var data = new Color[size * size];
            const int samples = 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int hits = 0;
                    for (int sy = 0; sy < samples; sy++)
                    {
                        for (int sx = 0; sx < samples; sx++)
                        {
                            float px = (x + (sx + 0.5f) / samples) / size * 2f - 1f;
                            float py = (y + (sy + 0.5f) / samples) / size * 2f - 1f;
                            if (inside(px, py)) hits++;
                        }
                    }

                    float alpha = hits / (float)(samples * samples);
                    data[y * size + x] = Color.White * alpha;
                }
            }

            var texture = new Texture2D(_graphicsDevice, size, size);
            texture.SetData(data);
            return texture;
        }

        private static bool InsidePointyHex(float x, float y)
        {
            float ax = MathF.Abs(x);
            float ay = MathF.Abs(y);
            return ax <= Sqrt3Over2 && ay <= 1f && ax * 0.5f + ay * Sqrt3Over2 <= Sqrt3Over2;
        }

        private static bool InsideFlatHex(float x, float y) => InsidePointyHex(y, x);

        private static bool InsideCircle(float x, float y) => x * x + y * y <= 1f;

        private static bool InsideBevel(float x, float y)
        {
            float cut = (PanelCut * 2f) / PanelTexSize;
            return MathF.Abs(x) + MathF.Abs(y) <= 2f - cut;
        }

        // ------------------------------------------------------------------
        // Rectangulos
        // ------------------------------------------------------------------

        public void DrawRectangle(Rectangle rect, Color color)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            _spriteBatch.Draw(_pixel, rect, color);
        }

        public void DrawRectangleBorder(Rectangle rect, Color color, int thickness = 1)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            DrawRectangle(new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            DrawRectangle(new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            DrawRectangle(new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            DrawRectangle(new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        public void DrawFilledRectangle(Rectangle rect, Color fillColor, Color borderColor, int borderThickness = 1)
        {
            DrawRectangle(rect, fillColor);
            DrawRectangleBorder(rect, borderColor, borderThickness);
        }

        // ------------------------------------------------------------------
        // Paneles con esquinas biseladas (nine-slice, borde escalable)
        // ------------------------------------------------------------------

        public void DrawBeveledRectangle(Rectangle rect, Color color, int cut = 10)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            int maxCut = Math.Min(rect.Width / 2, rect.Height / 2);
            if (maxCut <= 0) return;

            int c = Math.Min(cut, maxCut);
            int src = PanelCut;
            int mid = PanelTexSize - 2 * src;

            // Esquinas
            _spriteBatch.Draw(_bevel, new Rectangle(rect.X, rect.Y, c, c), new Rectangle(0, 0, src, src), color);
            _spriteBatch.Draw(_bevel, new Rectangle(rect.Right - c, rect.Y, c, c), new Rectangle(PanelTexSize - src, 0, src, src), color);
            _spriteBatch.Draw(_bevel, new Rectangle(rect.X, rect.Bottom - c, c, c), new Rectangle(0, PanelTexSize - src, src, src), color);
            _spriteBatch.Draw(_bevel, new Rectangle(rect.Right - c, rect.Bottom - c, c, c), new Rectangle(PanelTexSize - src, PanelTexSize - src, src, src), color);

            // Bordes
            int innerW = rect.Width - 2 * c;
            int innerH = rect.Height - 2 * c;

            if (innerW > 0)
            {
                _spriteBatch.Draw(_bevel, new Rectangle(rect.X + c, rect.Y, innerW, c), new Rectangle(src, 0, mid, src), color);
                _spriteBatch.Draw(_bevel, new Rectangle(rect.X + c, rect.Bottom - c, innerW, c), new Rectangle(src, PanelTexSize - src, mid, src), color);
            }

            if (innerH > 0)
            {
                _spriteBatch.Draw(_bevel, new Rectangle(rect.X, rect.Y + c, c, innerH), new Rectangle(0, src, src, mid), color);
                _spriteBatch.Draw(_bevel, new Rectangle(rect.Right - c, rect.Y + c, c, innerH), new Rectangle(PanelTexSize - src, src, src, mid), color);
            }

            // Centro
            if (innerW > 0 && innerH > 0)
            {
                _spriteBatch.Draw(_bevel, new Rectangle(rect.X + c, rect.Y + c, innerW, innerH), new Rectangle(src, src, mid, mid), color);
            }
        }

        public void DrawBeveledPanel(Rectangle rect, Color fill, Color border, int borderThickness = 3, int cut = 10)
        {
            DrawBeveledRectangle(rect, border, cut);

            var inner = new Rectangle(
                rect.X + borderThickness,
                rect.Y + borderThickness,
                rect.Width - borderThickness * 2,
                rect.Height - borderThickness * 2);

            if (inner.Width > 0 && inner.Height > 0)
            {
                DrawBeveledRectangle(inner, fill, Math.Max(1, cut - borderThickness));
            }
        }

        // ------------------------------------------------------------------
        // Hexagonos y circulos
        // ------------------------------------------------------------------

        public void DrawHexagon(Vector2 center, float radius, Color color, bool pointyTop = true)
        {
            if (radius <= 0.5f) return;

            var texture = pointyTop ? _hexPointy : _hexFlat;
            var dest = new Rectangle(
                (int)(center.X - radius),
                (int)(center.Y - radius),
                (int)(radius * 2),
                (int)(radius * 2));

            _spriteBatch.Draw(texture, dest, color);
        }

        public void DrawHexagonFramed(Vector2 center, float radius, Color fill, Color border, float borderThickness = 3f, bool pointyTop = true)
        {
            DrawHexagon(center, radius, border, pointyTop);
            DrawHexagon(center, Math.Max(1f, radius - borderThickness), fill, pointyTop);
        }

        public void DrawCircle(Vector2 center, float radius, Color color)
        {
            if (radius <= 0.5f) return;

            var dest = new Rectangle(
                (int)(center.X - radius),
                (int)(center.Y - radius),
                (int)(radius * 2),
                (int)(radius * 2));

            _spriteBatch.Draw(_circle, dest, color);
        }

        public void DrawCircleFramed(Vector2 center, float radius, Color fill, Color border, float borderThickness = 3f)
        {
            DrawCircle(center, radius, border);
            DrawCircle(center, Math.Max(1f, radius - borderThickness), fill);
        }

        // ------------------------------------------------------------------
        // Utilidades
        // ------------------------------------------------------------------

        public void DrawVerticalGradient(Rectangle bounds, Color top, Color bottom, int bands = 32)
        {
            if (bounds.Height <= 0) return;

            float bandHeight = bounds.Height / (float)bands;
            for (int i = 0; i < bands; i++)
            {
                float t = i / (float)(bands - 1);
                var color = Color.Lerp(top, bottom, t);
                int y = bounds.Y + (int)(i * bandHeight);
                int h = (int)MathF.Ceiling(bandHeight);
                DrawRectangle(new Rectangle(bounds.X, y, bounds.Width, h), color);
            }
        }
    }
}
