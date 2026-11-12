using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Primitives;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.UI.Components
{
    /// <summary>
    /// Fondo ambiental compartido por las pantallas de jugador. Las capas
    /// estaticas salen de <see cref="TextureCache"/>; solo el parallax y las
    /// particulas se recalculan por frame. La firma antigua
    /// <see cref="Draw(ShapeDrawer, Rectangle)"/> se conserva para combate.
    /// </summary>
    public class SpaceBackground
    {
        private const int ParticleCount = 60;

        private readonly float[] _particleX;
        private readonly float[] _particleY;
        private readonly float[] _particleSize;
        private readonly float[] _particleAlpha;
        private readonly float[] _particlePhase;
        private readonly float[] _nebulaX;
        private readonly float[] _nebulaY;
        private readonly float[] _nebulaRadius;
        private readonly Color[] _nebulaColor;

        public int Seed { get; }

        public SpaceBackground(int seed = 2026)
        {
            Seed = seed;
            var random = new Random(seed);

            _particleX = new float[ParticleCount];
            _particleY = new float[ParticleCount];
            _particleSize = new float[ParticleCount];
            _particleAlpha = new float[ParticleCount];
            _particlePhase = new float[ParticleCount];

            for (int i = 0; i < ParticleCount; i++)
            {
                _particleX[i] = (float)random.NextDouble();
                _particleY[i] = (float)random.NextDouble();
                _particleSize[i] = 1f + (float)random.NextDouble() * 2.2f;
                _particleAlpha[i] = 0.18f + (float)random.NextDouble() * 0.55f;
                _particlePhase[i] = (float)random.NextDouble() * MathF.PI * 2f;
            }

            const int nebulaCount = 5;
            _nebulaX = new float[nebulaCount];
            _nebulaY = new float[nebulaCount];
            _nebulaRadius = new float[nebulaCount];
            _nebulaColor = new Color[nebulaCount];

            Color[] palette =
            {
                new Color(64, 34, 96),
                new Color(88, 44, 110),
                new Color(34, 72, 120),
                new Color(52, 40, 104),
                new Color(96, 58, 84)
            };

            for (int i = 0; i < nebulaCount; i++)
            {
                _nebulaX[i] = (float)random.NextDouble();
                _nebulaY[i] = (float)random.NextDouble();
                _nebulaRadius[i] = 0.20f + (float)random.NextDouble() * 0.24f;
                _nebulaColor[i] = palette[i % palette.Length];
            }
        }

        /// <summary>
        /// Fondo de las siete pantallas: gradiente, nebulosa, hex con parallax,
        /// vineta y particulas con deriva sinusoidal.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, TextureCache textures,
            Rectangle screen, float timeSeconds)
        {
            textures.DrawBackground(spriteBatch, screen);

            // Nebulosa: glows cacheados con deriva lenta y distinta por capa.
            for (int i = 0; i < _nebulaX.Length; i++)
            {
                float driftX = MathF.Sin(timeSeconds * 0.05f + i * 1.7f) *
                               screen.Width * 0.018f;
                float driftY = MathF.Cos(timeSeconds * 0.04f + i * 2.1f) *
                               screen.Height * 0.014f;
                var center = new Vector2(
                    screen.X + screen.Width * _nebulaX[i] + driftX,
                    screen.Y + screen.Height * _nebulaY[i] + driftY);

                textures.DrawGlow(spriteBatch, center,
                    screen.Width * _nebulaRadius[i],
                    _nebulaColor[i] * 0.14f);
            }

            DrawHexGrid(spriteBatch, textures, screen, timeSeconds);
            textures.DrawVignette(spriteBatch, screen);
            DrawParticles(spriteBatch, textures, screen, timeSeconds);
        }

        private static void DrawHexGrid(SpriteBatch spriteBatch,
            TextureCache textures, Rectangle screen, float timeSeconds)
        {
            const int tile = 176;
            int offsetX = (int)(-timeSeconds * 4f) % tile;
            int offsetY = (int)(-timeSeconds * 2.5f) % tile;

            for (int y = screen.Y + offsetY - tile; y < screen.Bottom; y += tile)
            {
                for (int x = screen.X + offsetX - tile; x < screen.Right; x += tile)
                {
                    var destination = new Rectangle(x, y, tile, tile);
                    spriteBatch.Draw(textures.HexTile, destination,
                        Color.White * 0.10f);
                }
            }
        }

        private void DrawParticles(SpriteBatch spriteBatch, TextureCache textures,
            Rectangle screen, float timeSeconds)
        {
            for (int i = 0; i < _particleX.Length; i++)
            {
                float driftX = MathF.Sin(timeSeconds * 0.25f + _particlePhase[i]) *
                               screen.Width * 0.01f;
                float driftY = MathF.Cos(timeSeconds * 0.18f + _particlePhase[i]) *
                               screen.Height * 0.008f;

                float x = screen.X + screen.Width * _particleX[i] + driftX;
                float y = screen.Y + screen.Height * _particleY[i] + driftY;
                int size = Math.Max(1, (int)MathF.Round(_particleSize[i]));

                float twinkle = 0.75f + 0.25f * MathF.Sin(
                    timeSeconds * 1.4f + _particlePhase[i]);
                Color color = Color.White * (_particleAlpha[i] * twinkle);

                spriteBatch.Draw(textures.Pixel,
                    new Rectangle((int)x, (int)y, size, size), color);
            }
        }

        /// <summary>
        /// Ruta heredada usada por combate; mantiene el aspecto actual sin
        /// depender del cache nuevo.
        /// </summary>
        public void Draw(ShapeDrawer shapes, Rectangle screen)
        {
            shapes.DrawVerticalGradient(screen,
                new Color(22, 14, 44),
                new Color(8, 6, 18), 40);

            for (int i = 0; i < _nebulaX.Length; i++)
            {
                var center = new Vector2(
                    screen.X + screen.Width * _nebulaX[i],
                    screen.Y + screen.Height * _nebulaY[i]);
                float radius = screen.Width * _nebulaRadius[i];
                shapes.DrawCircle(center, radius,
                    _nebulaColor[i] * 0.10f);
                shapes.DrawCircle(center, radius * 0.65f,
                    _nebulaColor[i] * 0.10f);
            }

            for (int i = 0; i < _particleX.Length; i++)
            {
                var position = new Vector2(
                    screen.X + screen.Width * _particleX[i],
                    screen.Y + screen.Height * _particleY[i]);
                shapes.DrawCircle(position, _particleSize[i],
                    Color.White * _particleAlpha[i]);
            }
        }
    }
}
