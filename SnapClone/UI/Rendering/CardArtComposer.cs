using System;
using SnapClone.Data.Definitions;

namespace SnapClone.UI.Rendering
{
    /// <summary>
    /// Composicion determinista de la capa de arte de una carta. Es pura: no
    /// toca GraphicsDevice y produce siempre los mismos bytes para la misma
    /// identidad, tema y coste. El formato es RGBA de 128x192.
    /// </summary>
    public static class CardArtComposer
    {
        public const int Width = 128;
        public const int Height = 192;
        public const int BytesPerPixel = 4;

        /// <summary>FNV-1a de 32 bits sobre UTF-8, estable entre plataformas.</summary>
        public static uint Fnv1a(string text)
        {
            uint hash = 2166136261u;
            if (string.IsNullOrEmpty(text)) return hash;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            for (int i = 0; i < bytes.Length; i++)
            {
                hash ^= bytes[i];
                hash *= 16777619u;
            }

            return hash;
        }

        /// <summary>Mezcla id, tema y coste en una semilla estable.</summary>
        public static uint SeedFor(string cardId, DraftTheme? theme, int cost)
        {
            uint seed = Fnv1a(cardId ?? string.Empty);
            uint themeBits = (uint)(theme.HasValue ? (int)theme.Value + 1 : 0);
            seed = (seed ^ (themeBits * 16777619u)) * 31u;
            seed = (seed ^ (uint)(cost * 2654435761u)) * 31u;
            return seed;
        }

        /// <summary>
        /// 0 = plano cercano (coste 1-2), 1 = fragmentos (3-4),
        /// 2 = estallido (5-6).
        /// </summary>
        public static int CompositionForCost(int cost)
        {
            if (cost <= 2) return 0;
            if (cost <= 4) return 1;
            return 2;
        }

        public static byte[] Compose(string cardId, DraftTheme? theme, int cost)
        {
            uint seed = SeedFor(cardId, theme, cost);
            var random = new Random(unchecked((int)seed));
            var data = new byte[Width * Height * BytesPerPixel];

            float hue = HueFor(theme, cost, seed);
            float focalX = 0.30f + (float)random.NextDouble() * 0.40f;
            float focalY = 0.22f + (float)random.NextDouble() * 0.30f;
            int composition = CompositionForCost(cost);
            float emblemScale = composition == 0 ? 1.30f : composition == 1 ? 0.95f : 0.72f;
            int emblem = (int)((seed >> 3) % 8);
            float emblemRotation = ((seed >> 7) % 360) * MathF.PI / 180f;

            // 1. Gradiente vertical del tema.
            for (int y = 0; y < Height; y++)
            {
                float t = y / (float)(Height - 1);
                float value = 0.44f - 0.34f * t;
                float saturation = 0.62f - 0.18f * t;
                FromHsv(hue, saturation, value, out byte r, out byte g, out byte b);

                for (int x = 0; x < Width; x++)
                {
                    int index = (y * Width + x) * BytesPerPixel;
                    data[index] = r;
                    data[index + 1] = g;
                    data[index + 2] = b;
                    data[index + 3] = 255;
                }
            }

            // 2. Patron de fondo propio de la carta.
            float centerX = 0.5f + ((seed >> 8) % 21 - 10) * 0.006f;
            float centerY = 0.42f + (float)random.NextDouble() * 0.18f;
            DrawBackdropPattern(data, (int)(seed % 4), centerX, centerY, hue);

            // 3. Glow radial en el foco.
            float focusRadius = 0.52f + (float)random.NextDouble() * 0.20f;
            for (int y = 0; y < Height; y++)
            {
                float ny = y / (float)(Height - 1) - focalY;
                for (int x = 0; x < Width; x++)
                {
                    float nx = x / (float)(Width - 1) - focalX;
                    float distance = MathF.Sqrt(nx * nx + ny * ny);
                    float strength = MathF.Max(0f, 1f - distance / focusRadius);
                    if (strength <= 0f) continue;

                    strength *= strength;
                    int index = (y * Width + x) * BytesPerPixel;
                    data[index] = Add(data[index], strength * 105f);
                    data[index + 1] = Add(data[index + 1], strength * 130f);
                    data[index + 2] = Add(data[index + 2], strength * 165f);
                }
            }

            // 4-5. Emblema/silueta y composicion por coste.
            DrawEmblem(data, emblem, centerX, centerY, emblemScale,
                emblemRotation);
            DrawComposition(data, composition, centerX, centerY, random, hue);
            DrawSignatureStroke(data, (int)((seed >> 11) % 4), centerX,
                centerY, hue);

            // 5. Particulas.
            int particleCount = 26 + composition * 8;
            for (int i = 0; i < particleCount; i++)
            {
                int px = random.Next(Width);
                int py = random.Next(Height);
                int radius = random.Next(1, 3);
                float alpha = 0.35f + (float)random.NextDouble() * 0.5f;
                DrawDot(data, px, py, radius, 235, 245, 255, alpha);
            }

            // 6. Scanlines y ruido fino.
            for (int y = 0; y < Height; y++)
            {
                bool scanline = y % 4 == 0;
                for (int x = 0; x < Width; x++)
                {
                    int index = (y * Width + x) * BytesPerPixel;
                    int noise = random.Next(-5, 6);
                    data[index] = Clamp(data[index] + noise);
                    data[index + 1] = Clamp(data[index + 1] + noise);
                    data[index + 2] = Clamp(data[index + 2] + noise);

                    if (scanline)
                    {
                        data[index] = (byte)(data[index] * 0.86f);
                        data[index + 1] = (byte)(data[index + 1] * 0.86f);
                        data[index + 2] = (byte)(data[index + 2] * 0.86f);
                    }
                }
            }

            // 7. Vineta.
            for (int y = 0; y < Height; y++)
            {
                float ny = y / (float)(Height - 1) * 2f - 1f;
                for (int x = 0; x < Width; x++)
                {
                    float nx = x / (float)(Width - 1) * 2f - 1f;
                    float distance = MathF.Sqrt(nx * nx + ny * ny) / 1.414f;
                    float darken = MathHelperClamp((distance - 0.35f) / 0.65f);
                    darken *= darken * 0.72f;
                    if (darken <= 0f) continue;

                    int index = (y * Width + x) * BytesPerPixel;
                    data[index] = (byte)(data[index] * (1f - darken));
                    data[index + 1] = (byte)(data[index + 1] * (1f - darken));
                    data[index + 2] = (byte)(data[index + 2] * (1f - darken));
                }
            }

            return data;
        }

        private static void DrawEmblem(byte[] data, int emblem, float centerX,
            float centerY, float scale, float rotation)
        {
            float width = Width * 0.27f * scale;
            float height = Height * 0.27f * scale;
            float cos = MathF.Cos(rotation);
            float sin = MathF.Sin(rotation);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    float px = (x - centerX * Width) / width;
                    float py = (y - centerY * Height) / height;
                    float nx = px * cos - py * sin;
                    float ny = px * sin + py * cos;
                    float shape;

                    switch (emblem)
                    {
                        case 1:
                            shape = MathF.Abs(nx) + MathF.Abs(ny);
                            break;
                        case 2:
                            shape = MathF.Abs(ny + MathF.Abs(nx) * 0.9f) * 2.2f;
                            break;
                        case 3:
                            shape = MathF.Max(
                                MathF.Abs(nx) * 1.2f + ny * 0.6f,
                                -ny * 1.1f) - 0.18f;
                            break;
                        case 4:
                            shape = MathF.Min(MathF.Abs(nx), MathF.Abs(ny)) /
                                    0.28f;
                            break;
                        case 5:
                            shape = MathF.Max(MathF.Abs(nx), MathF.Abs(ny));
                            break;
                        case 6:
                        {
                            float distance = MathF.Sqrt(nx * nx + ny * ny);
                            shape = MathF.Min(MathF.Abs(distance - 0.55f),
                                MathF.Abs(distance - 0.95f)) * 4.5f;
                            break;
                        }
                        case 7:
                            shape = MathF.Min(
                                MathF.Abs(nx) * 1.8f + MathF.Abs(ny) * 0.45f,
                                MathF.Abs(ny) * 1.8f + MathF.Abs(nx) * 0.45f) -
                                0.20f;
                            break;
                        default:
                        {
                            float distance = MathF.Sqrt(nx * nx + ny * ny);
                            shape = MathF.Abs(distance - 0.72f) * 2.4f;
                            break;
                        }
                    }

                    if (shape > 1f) continue;

                    float edge = MathHelperClamp(1f - shape * 1.05f);
                    float alpha = edge * 0.78f;
                    if (shape < 0.35f)
                        alpha = MathF.Max(alpha, 0.26f);

                    int index = (y * Width + x) * BytesPerPixel;
                    data[index] = Add(data[index], 200 * alpha);
                    data[index + 1] = Add(data[index + 1], 222 * alpha);
                    data[index + 2] = Add(data[index + 2], 255 * alpha);
                }
            }
        }

        private static void DrawBackdropPattern(byte[] data, int pattern,
            float centerX, float centerY, float hue)
        {
            FromHsv((hue + 0.52f) % 1f, 0.62f, 1f,
                out byte r, out byte g, out byte b);

            for (int y = 0; y < Height; y++)
            {
                float ny = y / (float)(Height - 1);
                for (int x = 0; x < Width; x++)
                {
                    float nx = x / (float)(Width - 1);
                    float alpha = 0f;
                    float phase;
                    float fraction;

                    switch (pattern)
                    {
                        case 0:
                            phase = (nx * 0.78f + ny * 0.58f) * 17f;
                            fraction = phase - MathF.Floor(phase);
                            if (fraction < 0.24f)
                                alpha = 0.11f * (1f - fraction / 0.24f);
                            break;

                        case 1:
                            phase = ny * 19f;
                            fraction = phase - MathF.Floor(phase);
                            if (fraction < 0.32f)
                                alpha = 0.085f * (1f - fraction / 0.32f);
                            break;

                        case 2:
                            phase = MathF.Sqrt((nx - centerX) * (nx - centerX) +
                                               (ny - centerY) * (ny - centerY)) *
                                    24f;
                            fraction = phase - MathF.Floor(phase);
                            if (fraction < 0.18f)
                                alpha = 0.10f * (1f - fraction / 0.18f);
                            break;

                        default:
                            if (x % 15 < 2 || y % 15 < 2)
                                alpha = 0.085f;
                            break;
                    }

                    if (alpha <= 0f) continue;

                    int index = (y * Width + x) * BytesPerPixel;
                    data[index] = Add(data[index], r * alpha);
                    data[index + 1] = Add(data[index + 1], g * alpha);
                    data[index + 2] = Add(data[index + 2], b * alpha);
                }
            }
        }

        private static void DrawSignatureStroke(byte[] data, int stroke,
            float centerX, float centerY, float hue)
        {
            FromHsv((hue + 0.56f) % 1f, 0.58f, 1f,
                out byte r, out byte g, out byte b);

            switch (stroke)
            {
                case 1:
                {
                    float radius = Width * 0.36f;
                    for (float angle = -0.35f; angle <= 1.25f; angle += 0.025f)
                    {
                        int px = (int)(centerX * Width +
                                       MathF.Cos(angle + 2.2f) * radius);
                        int py = (int)(centerY * Height +
                                       MathF.Sin(angle + 2.2f) * radius);
                        DrawDot(data, px, py, 2, r, g, b, 0.24f);
                    }
                    break;
                }

                case 2:
                {
                    int px = (int)(centerX * Width);
                    for (int y = (int)(Height * 0.08f);
                         y < Height * 0.94f; y++)
                        DrawDot(data, px, y, 2, r, g, b, 0.13f);
                    break;
                }

                case 3:
                {
                    int py = (int)(Height * 0.74f);
                    for (int x = 0; x < Width; x++)
                        DrawDot(data, x, py, 2, r, g, b, 0.16f);
                    break;
                }

                default:
                {
                    for (int step = 0; step < Width; step++)
                    {
                        int px = step;
                        int py = (int)(Height * 0.94f - step * 0.58f);
                        DrawDot(data, px, py, 2, r, g, b, 0.18f);
                    }
                    break;
                }
            }
        }


        private static void DrawComposition(byte[] data, int composition,
            float centerX, float centerY, Random random, float hue)
        {
            if (composition == 1)
            {
                // Fragmentos diagonales.
                int shards = 6 + random.Next(4);
                for (int i = 0; i < shards; i++)
                {
                    float angle = (float)random.NextDouble() * MathF.PI;
                    float offset = 0.10f + (float)random.NextDouble() * 0.55f;
                    int cx = (int)(centerX * Width + MathF.Cos(angle) * offset * Width);
                    int cy = (int)(centerY * Height + MathF.Sin(angle) * offset * Height);
                    int length = 10 + random.Next(22);
                    int thickness = 2 + random.Next(4);

                    for (int step = 0; step < length; step++)
                    {
                        int px = cx + (int)(step * MathF.Cos(angle + 0.6f));
                        int py = cy + (int)(step * MathF.Sin(angle + 0.6f));
                        DrawDot(data, px, py, thickness, 225, 240, 255, 0.28f);
                    }
                }
            }
            else if (composition == 2)
            {
                // Estallido radial.
                int rays = 14 + random.Next(8);
                for (int i = 0; i < rays; i++)
                {
                    float angle = i * MathF.PI * 2f / rays +
                                  (float)random.NextDouble() * 0.08f;
                    int length = 34 + random.Next(40);

                    for (int step = 12; step < length; step++)
                    {
                        int px = (int)(centerX * Width + MathF.Cos(angle) * step);
                        int py = (int)(centerY * Height + MathF.Sin(angle) * step);
                        float fade = 1f - step / (float)length;
                        DrawDot(data, px, py, 1 + (step % 3 == 0 ? 1 : 0),
                            235, 248, 255, 0.30f * fade);
                    }
                }
            }
            else
            {
                // Plano cercano: un arco de energia alrededor del emblema.
                float radius = Width * 0.34f;
                for (float angle = -0.9f; angle <= 0.9f; angle += 0.03f)
                {
                    int px = (int)(centerX * Width + MathF.Cos(angle + 1.9f) * radius);
                    int py = (int)(centerY * Height + MathF.Sin(angle + 1.9f) * radius);
                    DrawDot(data, px, py, 2, 235, 245, 255, 0.35f);
                }
            }
        }

        private static void DrawDot(byte[] data, int x, int y, int radius,
            byte r, byte g, byte b, float alpha)
        {
            for (int oy = -radius; oy <= radius; oy++)
            {
                for (int ox = -radius; ox <= radius; ox++)
                {
                    if (ox * ox + oy * oy > radius * radius) continue;

                    int px = x + ox;
                    int py = y + oy;
                    if (px < 0 || py < 0 || px >= Width || py >= Height)
                        continue;

                    int index = (py * Width + px) * BytesPerPixel;
                    float falloff = 1f - MathF.Sqrt(ox * ox + oy * oy) /
                                    MathF.Max(1f, radius);
                    float amount = alpha * MathF.Max(0.15f, falloff);

                    data[index] = Add(data[index], r * amount);
                    data[index + 1] = Add(data[index + 1], g * amount);
                    data[index + 2] = Add(data[index + 2], b * amount);
                }
            }
        }

        public static float HueFor(DraftTheme? theme, int cost, uint seed)
        {
            float baseHue;
            switch (theme)
            {
                case DraftTheme.Control: baseHue = 0.58f; break;   // azul
                case DraftTheme.Destroy: baseHue = 0.01f; break;   // rojo
                case DraftTheme.OnReveal: baseHue = 0.78f; break;  // violeta
                case DraftTheme.Neutral: baseHue = 0.46f; break;    // verde-teal
                default: baseHue = 0.12f; break;                   // dorado (tokens)
            }

            // El coste y la identidad desplazan el tono para que dos cartas del
            // mismo tema no compartan un color plano.
            float shift = (cost - 3) * 0.012f + ((seed >> 16) % 100) / 100f * 0.05f;
            return (baseHue + shift + 1f) % 1f;
        }

        public static void FromHsv(float hue, float saturation, float value,
            out byte r, out byte g, out byte b)
        {
            hue = (hue % 1f + 1f) % 1f;
            saturation = MathHelperClamp(saturation);
            value = MathHelperClamp(value);

            float c = value * saturation;
            float x = c * (1f - MathF.Abs((hue * 6f) % 2f - 1f));
            float m = value - c;

            float rf, gf, bf;
            int sector = (int)(hue * 6f) % 6;

            switch (sector)
            {
                case 0: rf = c; gf = x; bf = 0f; break;
                case 1: rf = x; gf = c; bf = 0f; break;
                case 2: rf = 0f; gf = c; bf = x; break;
                case 3: rf = 0f; gf = x; bf = c; break;
                case 4: rf = x; gf = 0f; bf = c; break;
                default: rf = c; gf = 0f; bf = x; break;
            }

            r = ToByte(rf + m);
            g = ToByte(gf + m);
            b = ToByte(bf + m);
        }

        private static byte Add(byte value, float amount) =>
            (byte)Math.Min(255f, value + amount);

        private static byte Clamp(int value) =>
            (byte)(value < 0 ? 0 : value > 255 ? 255 : value);

        private static byte ToByte(float value) =>
            (byte)MathHelperClamp(value * 255f);

        private static float MathHelperClamp(float value) =>
            value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
