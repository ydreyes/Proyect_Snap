using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Theme;

namespace SnapClone.UI.Rendering
{
    /// <summary>
    /// Texturas generadas una sola vez y reutilizadas en todos los dibujos:
    /// gradientes, glow, ruido, hex, panel 9-slice, scanlines y vineta.
    /// </summary>
    public class TextureCache : IDisposable
    {
        private const int PanelSize = 64;
        private const int PanelCorner = 16;

        private readonly GraphicsDevice _graphicsDevice;

        public Texture2D Pixel { get; private set; }
        public Texture2D BackgroundGradient { get; private set; }
        public Texture2D FadeGradient { get; private set; }
        public Texture2D RadialGlow { get; private set; }
        public Texture2D Noise { get; private set; }
        public Texture2D HexTile { get; private set; }
        public Texture2D PanelMask { get; private set; }
        public Texture2D Scanline { get; private set; }
        public Texture2D Vignette { get; private set; }

        public int PanelCornerSize => PanelCorner;

        public TextureCache(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            Pixel = CreateSolid(1, 1, Color.White);
            BackgroundGradient = CreateVerticalGradient(
                UiTheme.BackgroundTop, UiTheme.BackgroundBottom, 256);
            FadeGradient = CreateAlphaGradient(256);
            RadialGlow = CreateRadialGlow(128);
            Noise = CreateNoise(256, 2026);
            HexTile = CreateHexTile(128);
            PanelMask = CreatePanelMask(PanelSize, PanelCorner);
            Scanline = CreateScanline();
            Vignette = CreateVignette(128);
        }

        // ------------------------------------------------------------------
        // Dibujo de alto nivel con texturas cacheadas
        // ------------------------------------------------------------------

        public void DrawBackground(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.Draw(BackgroundGradient, bounds, Color.White);
        }

        public void DrawFadeOverlay(SpriteBatch spriteBatch, Rectangle bounds,
            Color color)
        {
            spriteBatch.Draw(FadeGradient, bounds, color);
        }

        public void DrawGlow(SpriteBatch spriteBatch, Vector2 center, float radius,
            Color color)
        {
            var destination = new Rectangle(
                (int)(center.X - radius), (int)(center.Y - radius),
                (int)(radius * 2f), (int)(radius * 2f));
            spriteBatch.Draw(RadialGlow, destination, color);
        }

        public void DrawVignette(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.Draw(Vignette, bounds, Color.White);
        }

        public void DrawScanlines(SpriteBatch spriteBatch, Rectangle bounds,
            Color color)
        {
            spriteBatch.Draw(Scanline, bounds, color);
        }

        public void DrawNoise(SpriteBatch spriteBatch, Rectangle bounds,
            Color color)
        {
            spriteBatch.Draw(Noise, bounds, color);
        }

        public void DrawPanel(SpriteBatch spriteBatch, Rectangle bounds, Color color)
        {
            DrawNineSlice(spriteBatch, PanelMask, bounds, color);
        }

        public void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture,
            Rectangle bounds, Color color)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            int sourceSize = texture.Width;
            int src = PanelCorner;
            int mid = sourceSize - src * 2;

            int cut = Math.Min(src, Math.Min(bounds.Width / 2, bounds.Height / 2));
            if (cut <= 0) return;

            int innerW = bounds.Width - cut * 2;
            int innerH = bounds.Height - cut * 2;

            spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, cut, cut),
                new Rectangle(0, 0, src, src), color);
            spriteBatch.Draw(texture, new Rectangle(bounds.Right - cut, bounds.Y,
                cut, cut), new Rectangle(sourceSize - src, 0, src, src), color);
            spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Bottom - cut,
                cut, cut), new Rectangle(0, sourceSize - src, src, src), color);
            spriteBatch.Draw(texture, new Rectangle(bounds.Right - cut,
                bounds.Bottom - cut, cut, cut),
                new Rectangle(sourceSize - src, sourceSize - src, src, src), color);

            if (innerW > 0)
            {
                spriteBatch.Draw(texture, new Rectangle(bounds.X + cut, bounds.Y,
                    innerW, cut), new Rectangle(src, 0, mid, src), color);
                spriteBatch.Draw(texture, new Rectangle(bounds.X + cut,
                    bounds.Bottom - cut, innerW, cut),
                    new Rectangle(src, sourceSize - src, mid, src), color);
            }

            if (innerH > 0)
            {
                spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y + cut,
                    cut, innerH), new Rectangle(0, src, src, mid), color);
                spriteBatch.Draw(texture, new Rectangle(bounds.Right - cut,
                    bounds.Y + cut, cut, innerH),
                    new Rectangle(sourceSize - src, src, src, mid), color);
            }

            if (innerW > 0 && innerH > 0)
            {
                spriteBatch.Draw(texture, new Rectangle(bounds.X + cut,
                    bounds.Y + cut, innerW, innerH),
                    new Rectangle(src, src, mid, mid), color);
            }
        }

        // ------------------------------------------------------------------
        // Generacion
        // ------------------------------------------------------------------

        private Texture2D CreateSolid(int width, int height, Color color)
        {
            var data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) data[i] = color;

            var texture = new Texture2D(_graphicsDevice, width, height);
            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateVerticalGradient(Color top, Color bottom,
            int height)
        {
            var data = new Color[height];
            for (int y = 0; y < height; y++)
            {
                float t = height == 1 ? 0f : y / (float)(height - 1);
                data[y] = Color.Lerp(top, bottom, t);
            }

            var texture = new Texture2D(_graphicsDevice, 1, height);
            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateAlphaGradient(int height)
        {
            var data = new Color[height];
            for (int y = 0; y < height; y++)
            {
                float t = height == 1 ? 0f : y / (float)(height - 1);
                data[y] = Color.White * (1f - t);
            }

            var texture = new Texture2D(_graphicsDevice, 1, height);
            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateRadialGlow(int size)
        {
            var data = new Color[size * size];
            float half = size / 2f;
            float maxDistance = half;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f - half) / maxDistance;
                    float dy = (y + 0.5f - half) / maxDistance;
                    float distance = MathF.Sqrt(dx * dx + dy * dy);
                    float alpha = MathF.Max(0f, 1f - distance);
                    alpha *= alpha;
                    data[y * size + x] = Color.White * alpha;
                }
            }

            var texture = new Texture2D(_graphicsDevice, size, size);
            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateNoise(int size, int seed)
        {
            var random = new Random(seed);
            var data = new Color[size * size];

            for (int i = 0; i < data.Length; i++)
            {
                float value = (float)random.NextDouble();
                data[i] = Color.White * (value * 0.07f);
            }

            var texture = new Texture2D(_graphicsDevice, size, size);
            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateHexTile(int size)
        {
            var data = new Color[size * size];
            float center = size / 2f;
            float radius = size * 0.44f;
            float border = 2.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = (x + 0.5f - center) / radius;
                    float py = (y + 0.5f - center) / radius;
                    float distance = HexDistance(px, py);

                    Color color = Color.Transparent;

                    if (distance <= 1f)
                    {
                        float edge = (1f - distance) * radius;
                        if (edge <= border)
                            color = Color.White * 0.55f;
                        else
                            color = Color.White * 0.045f;
                    }

                    data[y * size + x] = color;
                }
            }

            var texture = new Texture2D(_graphicsDevice, size, size);
            texture.SetData(data);
            return texture;
        }

        private static float HexDistance(float x, float y)
        {
            float ax = MathF.Abs(x);
            float ay = MathF.Abs(y);
            const float sqrt3Over2 = 0.8660254f;
            return MathF.Max(ax * 0.5f + ay * sqrt3Over2, ay);
        }

        private Texture2D CreatePanelMask(int size, int corner)
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
                            float px = x + (sx + 0.5f) / samples;
                            float py = y + (sy + 0.5f) / samples;
                            if (InsideCutRect(px, py, size, corner)) hits++;
                        }
                    }

                    data[y * size + x] = Color.White *
                        (hits / (float)(samples * samples));
                }
            }

            var texture = new Texture2D(_graphicsDevice, size, size);
            texture.SetData(data);
            return texture;
        }

        private static bool InsideCutRect(float x, float y, int size, int corner)
        {
            if (x < 0f || y < 0f || x > size || y > size) return false;

            float left = corner - x;
            float top = corner - y;
            if (left > 0f && top > 0f) return left + top <= corner;

            float right = x - (size - corner);
            float bottomTop = corner - y;
            if (right > 0f && bottomTop > 0f) return right + bottomTop <= corner;

            float bottom = y - (size - corner);
            float leftBottom = corner - x;
            if (bottom > 0f && leftBottom > 0f) return bottom + leftBottom <= corner;

            float rightBottom = x - (size - corner);
            if (rightBottom > 0f && bottom > 0f) return rightBottom + bottom <= corner;

            return true;
        }

        private Texture2D CreateScanline()
        {
            var data = new Color[8];
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.White * (i % 4 == 0 ? 0.10f : 0.02f);

            var texture = new Texture2D(_graphicsDevice, 1, data.Length);
            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateVignette(int size)
        {
            var data = new Color[size * size];
            float half = size / 2f;
            float maxDistance = half * 1.414f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - half;
                    float dy = y + 0.5f - half;
                    float distance = MathF.Sqrt(dx * dx + dy * dy) / maxDistance;
                    float alpha = MathHelper.Clamp((distance - 0.42f) / 0.58f,
                        0f, 1f);
                    alpha *= alpha;
                    data[y * size + x] = Color.Black * (alpha * 0.72f);
                }
            }

            var texture = new Texture2D(_graphicsDevice, size, size);
            texture.SetData(data);
            return texture;
        }

        public void Dispose()
        {
            Pixel?.Dispose();
            BackgroundGradient?.Dispose();
            FadeGradient?.Dispose();
            RadialGlow?.Dispose();
            Noise?.Dispose();
            HexTile?.Dispose();
            PanelMask?.Dispose();
            Scanline?.Dispose();
            Vignette?.Dispose();
        }
    }
}
