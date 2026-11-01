using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    /// <summary>
    /// Fondo espacial dibujado con gradientes y puntos procedurales.
    /// Sin imagenes: todo son rectangulos y circulos generados en codigo.
    /// </summary>
    public class SpaceBackground
    {
        private readonly float[] _starX;
        private readonly float[] _starY;
        private readonly float[] _starSize;
        private readonly float[] _starAlpha;
        private readonly float[] _nebulaX;
        private readonly float[] _nebulaY;
        private readonly float[] _nebulaRadius;

        public SpaceBackground(int seed = 2026)
        {
            var random = new Random(seed);

            const int starCount = 90;
            _starX = new float[starCount];
            _starY = new float[starCount];
            _starSize = new float[starCount];
            _starAlpha = new float[starCount];

            for (int i = 0; i < starCount; i++)
            {
                _starX[i] = (float)random.NextDouble();
                _starY[i] = (float)random.NextDouble();
                _starSize[i] = 1f + (float)random.NextDouble() * 2.5f;
                _starAlpha[i] = 0.25f + (float)random.NextDouble() * 0.6f;
            }

            const int nebulaCount = 5;
            _nebulaX = new float[nebulaCount];
            _nebulaY = new float[nebulaCount];
            _nebulaRadius = new float[nebulaCount];

            for (int i = 0; i < nebulaCount; i++)
            {
                _nebulaX[i] = (float)random.NextDouble();
                _nebulaY[i] = (float)random.NextDouble();
                _nebulaRadius[i] = 0.18f + (float)random.NextDouble() * 0.22f;
            }
        }

        public void Draw(ShapeDrawer shapes, Rectangle screen)
        {
            shapes.DrawVerticalGradient(screen,
                new Color(22, 14, 44),
                new Color(8, 6, 18), 40);

            // Nebulosas tenues
            for (int i = 0; i < _nebulaX.Length; i++)
            {
                var center = new Vector2(screen.X + screen.Width * _nebulaX[i], screen.Y + screen.Height * _nebulaY[i]);
                float radius = screen.Width * _nebulaRadius[i];
                shapes.DrawCircle(center, radius, new Color(64, 34, 96) * 0.10f);
                shapes.DrawCircle(center, radius * 0.65f, new Color(88, 44, 110) * 0.10f);
            }

            // Estrellas
            for (int i = 0; i < _starX.Length; i++)
            {
                var position = new Vector2(screen.X + screen.Width * _starX[i], screen.Y + screen.Height * _starY[i]);
                shapes.DrawCircle(position, _starSize[i], Color.White * _starAlpha[i]);
            }
        }
    }
}
