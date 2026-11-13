using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Theme;

namespace SnapClone.UI.Components
{
    /// <summary>
    /// Roles tipograficos compartidos. Display = Bebas Neue, Ui = Rajdhani
    /// SemiBold, Body = Inter, Numeric = Rajdhani Bold.
    /// </summary>
    public enum TextRole
    {
        Display,
        Ui,
        Body,
        Numeric
    }

    /// <summary>
    /// Servicio de texto compartido. Los metodos heredados (Measure,
    /// DrawText, DrawTextCentered...) conservan la API y el tamano base de
    /// 22 px para no mover el layout existente, pero usan el rol Ui cuando
    /// las fuentes TTF cargan. Si la carga falla se usa DefaultFont.
    /// </summary>
    public class FontService : IDisposable
    {
        private const float LegacyBaseSize = 26f;

        private const string DisplayFile = "BebasNeue-Regular.ttf";
        private const string UiFile = "Rajdhani-SemiBold.ttf";
        private const string BodyFile = "Inter-Regular.ttf";
        private const string NumericFile = "Rajdhani-Bold.ttf";

        private readonly SpriteFont _defaultFont;

        // Escala del viewport para la API heredada: antes el texto se dibujaba
        // en pixeles fijos y quedaba diminuto en pantallas reales.
        private readonly IGraphicsDeviceService _graphicsDeviceService;
        private float _legacyScale = UiTheme.ReadabilityScale;
        private int _legacyViewportWidth = -1;
        private int _legacyViewportHeight = -1;

        private FontSystem _displaySystem;
        private FontSystem _uiSystem;
        private FontSystem _bodySystem;
        private FontSystem _numericSystem;

        private readonly Dictionary<int, SpriteFontBase> _roleFonts =
            new Dictionary<int, SpriteFontBase>();

        // Ancho de celda de las cifras tabulares, cacheado por fuente/tamano.
        private readonly Dictionary<SpriteFontBase, float> _numericCellWidths =
            new Dictionary<SpriteFontBase, float>();

        public FontService(ContentManager content)
        {
            _graphicsDeviceService = ResolveGraphicsDeviceService(content);

            try
            {
                _defaultFont = content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                _defaultFont = null;
            }

            TryLoadRuntimeFonts();
        }

        /// <summary>
        /// Constructor de diagnostico: con <paramref name="forceFallback"/> se
        /// omite la carga de los TTF para verificar que todo el texto sigue
        /// dibujandose con DefaultFont. Solo lo usa la galeria de UI.
        /// </summary>
        public FontService(ContentManager content, bool forceFallback)
        {
            _graphicsDeviceService = ResolveGraphicsDeviceService(content);

            try
            {
                _defaultFont = content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                _defaultFont = null;
            }

            if (forceFallback)
            {
                UsesRuntimeFonts = false;
                LoadError = "fallback forzado (galeria)";
                return;
            }

            TryLoadRuntimeFonts();
        }

        public bool HasFont => _defaultFont != null || UsesRuntimeFonts;
        public bool UsesRuntimeFonts { get; private set; }

        private static IGraphicsDeviceService ResolveGraphicsDeviceService(
            ContentManager content)
        {
            try
            {
                return content?.ServiceProvider?.GetService(
                    typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Escala efectiva del texto heredado. Se recalcula solo cuando cambia
        /// el viewport para no consultar el dispositivo en cada glifo.
        /// </summary>
        private float LegacyScale
        {
            get
            {
                var device = _graphicsDeviceService?.GraphicsDevice;
                if (device == null) return UiTheme.ReadabilityScale;

                var viewport = device.Viewport;
                if (viewport.Width != _legacyViewportWidth ||
                    viewport.Height != _legacyViewportHeight)
                {
                    _legacyViewportWidth = viewport.Width;
                    _legacyViewportHeight = viewport.Height;
                    _legacyScale = UiTheme.UiScale(viewport.Width,
                        viewport.Height);
                }

                return _legacyScale;
            }
        }

        private int LegacyPixelSize(float scale) =>
            Math.Max(8, (int)MathF.Round(LegacyBaseSize * scale * LegacyScale));


        /// <summary>Error de carga, solo para diagnostico y galeria.</summary>
        public string LoadError { get; private set; }

        private void TryLoadRuntimeFonts()
        {
            try
            {
                var assembly = typeof(FontService).Assembly;

                _displaySystem = CreateSystem(LoadResource(assembly, DisplayFile));
                _uiSystem = CreateSystem(LoadResource(assembly, UiFile));
                _bodySystem = CreateSystem(LoadResource(assembly, BodyFile));
                _numericSystem = CreateSystem(LoadResource(assembly, NumericFile));

                // Fuerza un primer glifo para detectar fallos de rasterizado
                // dentro del bloque de carga y poder caer al SpriteFont.
                _uiSystem.GetFont(LegacyBaseSize).MeasureString("A");
                _displaySystem.GetFont(UiTheme.DisplayXl).MeasureString("A");
                _bodySystem.GetFont(UiTheme.Body).MeasureString("A");
                _numericSystem.GetFont(UiTheme.Numeric).MeasureString("0");

                UsesRuntimeFonts = true;
            }
            catch (Exception ex)
            {
                LoadError = ex.Message;
                UsesRuntimeFonts = false;
                DisposeSystems();
            }
        }

        private static FontSystem CreateSystem(byte[] data)
        {
            var system = new FontSystem(new FontSystemSettings
            {
                TextureWidth = 1024,
                TextureHeight = 1024,
                FontResolutionFactor = 1f,
                KernelWidth = 2,
                KernelHeight = 2
            });

            system.AddFont(data);
            return system;
        }

        private static byte[] LoadResource(Assembly assembly, string fileName)
        {
            string suffix = "Fonts." + fileName;
            string resource = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(suffix,
                    StringComparison.OrdinalIgnoreCase));

            if (resource == null)
                throw new InvalidOperationException(
                    $"No se encontro el recurso embebido {suffix}.");

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream == null)
                    throw new InvalidOperationException(
                        $"No se pudo abrir el recurso {resource}.");

                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }

        private FontSystem SystemFor(TextRole role)
        {
            switch (role)
            {
                case TextRole.Display: return _displaySystem;
                case TextRole.Body: return _bodySystem;
                case TextRole.Numeric: return _numericSystem;
                default: return _uiSystem;
            }
        }

        private SpriteFontBase GetRoleFont(TextRole role, float pixels)
        {
            int size = Math.Max(8, (int)MathF.Round(pixels));
            int key = ((int)role << 16) | size;

            if (_roleFonts.TryGetValue(key, out var cached))
                return cached;

            var font = SystemFor(role).GetFont(size);
            _roleFonts[key] = font;
            return font;
        }

        // ------------------------------------------------------------------
        // API heredada (compatibilidad con escenas y combate)
        // ------------------------------------------------------------------

        public Vector2 Measure(string text, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;

            if (UsesRuntimeFonts)
            {
                var font = GetRoleFont(TextRole.Ui, LegacyPixelSize(scale));
                return font.MeasureString(text);
            }

            if (_defaultFont == null) return Vector2.Zero;
            return _defaultFont.MeasureString(text) * (scale * LegacyScale);
        }

        public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position,
            Color color, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (UsesRuntimeFonts)
            {
                var font = GetRoleFont(TextRole.Ui, LegacyPixelSize(scale));
                font.DrawText(spriteBatch, text, position, color, 0f,
                    Vector2.Zero, Vector2.One, 0f);
                return;
            }

            if (_defaultFont == null) return;
            spriteBatch.DrawString(_defaultFont, text, position, color, 0f,
                Vector2.Zero, scale * LegacyScale, SpriteEffects.None, 0f);
        }

        public void DrawTextCentered(SpriteBatch spriteBatch, string text,
            Rectangle bounds, Color color, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;

            var size = Measure(text, scale);
            var position = new Vector2(
                bounds.X + (bounds.Width - size.X) / 2f,
                bounds.Y + (bounds.Height - size.Y) / 2f);

            DrawText(spriteBatch, text, position, color, scale);
        }

        public void DrawTextCentered(SpriteBatch spriteBatch, string text,
            Vector2 center, Color color, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;

            var size = Measure(text, scale);
            var position = new Vector2(
                center.X - size.X / 2f, center.Y - size.Y / 2f);

            DrawText(spriteBatch, text, position, color, scale);
        }

        /// <summary>Dibuja centrado reduciendo la escala hasta que quepa.</summary>
        public void DrawTextCenteredFit(SpriteBatch spriteBatch, string text,
            Rectangle bounds, Color color, float maxScale = 1f,
            float minScale = 0.3f)
        {
            if (string.IsNullOrEmpty(text)) return;

            float scale = FitScale(text, bounds.Width, maxScale, minScale);
            DrawTextCentered(spriteBatch, text, bounds, color, scale);
        }

        public void DrawTextWrapped(SpriteBatch spriteBatch, string text,
            Rectangle bounds, Color color, float scale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;

            float lineHeight = (UsesRuntimeFonts
                ? GetRoleFont(TextRole.Ui, LegacyPixelSize(scale)).LineHeight
                : (_defaultFont?.LineSpacing ?? LegacyBaseSize) * scale *
                    LegacyScale);

            float y = bounds.Y;
            foreach (string paragraph in text.Split('\n'))
            {
                string line = string.Empty;
                foreach (string word in paragraph.Split(' '))
                {
                    string candidate = line.Length == 0 ? word : line + " " + word;
                    if (Measure(candidate, scale).X > bounds.Width && line.Length > 0)
                    {
                        DrawText(spriteBatch, line, new Vector2(bounds.X, y),
                            color, scale);
                        y += lineHeight;
                        line = word;
                    }
                    else
                    {
                        line = candidate;
                    }
                }

                if (line.Length > 0)
                {
                    DrawText(spriteBatch, line, new Vector2(bounds.X, y), color, scale);
                    y += lineHeight;
                }
            }
        }

        // ------------------------------------------------------------------
        // Roles tipograficos
        // ------------------------------------------------------------------

        public Vector2 MeasureRole(TextRole role, string text, float size,
            float uiScale = 1f, float tracking = 0f)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;

            if (!UsesRuntimeFonts)
                return Measure(text, size / LegacyBaseSize);

            if (role == TextRole.Numeric)
                return MeasureNumeric(text, size * uiScale, tracking * uiScale);

            var font = GetRoleFont(role, size * uiScale);
            return font.MeasureString(text, null, tracking * uiScale);
        }

        public void DrawRole(SpriteBatch spriteBatch, TextRole role, string text,
            Vector2 position, Color color, float size, float uiScale = 1f,
            float tracking = 0f)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (!UsesRuntimeFonts)
            {
                DrawText(spriteBatch, text, position, color, size / LegacyBaseSize);
                return;
            }

            var font = GetRoleFont(role, size * uiScale);

            if (role == TextRole.Numeric)
            {
                DrawNumericTabular(spriteBatch, font, text, position, color,
                    tracking * uiScale);
                return;
            }

            font.DrawText(spriteBatch, text, position, color, 0f, Vector2.Zero,
                null, 0f, tracking * uiScale);
        }

        /// <summary>
        /// Dibuja cifras sobre un avance fijo (tabular) para que los valores
        /// no desplacen el layout al cambiar. Los caracteres no numericos
        /// conservan su ancho natural.
        /// </summary>
        private void DrawNumericTabular(SpriteBatch spriteBatch,
            SpriteFontBase font, string text, Vector2 position, Color color,
            float tracking)
        {
            float cell = NumericCellWidth(font);
            float x = position.X;

            foreach (char character in text)
            {
                string glyph = character.ToString();
                font.DrawText(spriteBatch, glyph, new Vector2(x, position.Y),
                    color, 0f, Vector2.Zero, null, 0f, 0f);
                x += (char.IsDigit(character)
                    ? cell
                    : font.MeasureString(glyph).X) + tracking;
            }
        }

        private Vector2 MeasureNumeric(string text, float pixels, float tracking)
        {
            var font = GetRoleFont(TextRole.Numeric, pixels);
            float cell = NumericCellWidth(font);
            float width = 0f;

            foreach (char character in text)
            {
                string glyph = character.ToString();
                width += char.IsDigit(character)
                    ? cell
                    : font.MeasureString(glyph).X;
            }

            if (tracking != 0f && text.Length > 1)
                width += tracking * (text.Length - 1);

            return new Vector2(width, font.MeasureString("0").Y);
        }

        private float NumericCellWidth(SpriteFontBase font)
        {
            if (_numericCellWidths.TryGetValue(font, out float width))
                return width;

            for (char digit = '0'; digit <= '9'; digit++)
                width = MathF.Max(width, font.MeasureString(digit.ToString()).X);

            _numericCellWidths[font] = width;
            return width;
        }

        public void DrawDisplay(SpriteBatch spriteBatch, string text,
            Vector2 position, Color color, float size = UiTheme.Display,
            float uiScale = 1f, bool uppercase = true, float tracking = UiTheme.DisplayTracking)
        {
            DrawRole(spriteBatch, TextRole.Display,
                uppercase ? text?.ToUpperInvariant() : text,
                position, color, size, uiScale, tracking);
        }

        public void DrawDisplayCentered(SpriteBatch spriteBatch, string text,
            Rectangle bounds, Color color, float size = UiTheme.Display,
            float uiScale = 1f, bool uppercase = true, float tracking = UiTheme.DisplayTracking)
        {
            string value = uppercase ? text?.ToUpperInvariant() : text;
            var measured = MeasureRole(TextRole.Display, value, size, uiScale, tracking);
            var position = new Vector2(
                bounds.X + (bounds.Width - measured.X) / 2f,
                bounds.Y + (bounds.Height - measured.Y) / 2f);
            DrawRole(spriteBatch, TextRole.Display, value, position, color,
                size, uiScale, tracking);
        }

        public void DrawLabel(SpriteBatch spriteBatch, string text,
            Vector2 position, Color color, float size = UiTheme.Label,
            float uiScale = 1f)
        {
            DrawRole(spriteBatch, TextRole.Ui, text, position, color, size, uiScale);
        }

        public void DrawBody(SpriteBatch spriteBatch, string text,
            Vector2 position, Color color, float uiScale = 1f)
        {
            DrawRole(spriteBatch, TextRole.Body, text, position, color,
                UiTheme.Body, uiScale);
        }

        public void DrawNumeric(SpriteBatch spriteBatch, string text,
            Vector2 center, Color color, float size = UiTheme.Numeric,
            float uiScale = 1f)
        {
            var measured = MeasureRole(TextRole.Numeric, text, size, uiScale);
            var position = new Vector2(center.X - measured.X / 2f,
                center.Y - measured.Y / 2f);
            DrawRole(spriteBatch, TextRole.Numeric, text, position, color,
                size, uiScale);
        }

        public void DrawRoleCentered(SpriteBatch spriteBatch, TextRole role,
            string text, Rectangle bounds, Color color, float size,
            float uiScale = 1f, float tracking = 0f)
        {
            var measured = MeasureRole(role, text, size, uiScale, tracking);
            var position = new Vector2(
                bounds.X + (bounds.Width - measured.X) / 2f,
                bounds.Y + (bounds.Height - measured.Y) / 2f);
            DrawRole(spriteBatch, role, text, position, color, size, uiScale,
                tracking);
        }

        public void DrawRoleFit(SpriteBatch spriteBatch, TextRole role,
            string text, Rectangle bounds, Color color, float maxSize,
            float uiScale = 1f, float tracking = 0f)
        {
            if (string.IsNullOrEmpty(text)) return;

            float width = MeasureRole(role, text, maxSize, uiScale, tracking).X;
            float size = maxSize;
            if (width > bounds.Width && width > 0f)
                size = maxSize * (bounds.Width / width);

            size = Math.Max(size, maxSize * 0.45f);
            DrawRoleCentered(spriteBatch, role, text, bounds, color, size,
                uiScale, tracking);
        }

        public void DrawRoleWrapped(SpriteBatch spriteBatch, TextRole role,
            string text, Rectangle bounds, Color color, float size,
            float uiScale = 1f)
        {
            if (string.IsNullOrEmpty(text)) return;

            float lineHeight = MeasureRole(role, "Ay", size, uiScale).Y;
            float y = bounds.Y;

            foreach (string paragraph in text.Split('\n'))
            {
                string line = string.Empty;
                foreach (string word in paragraph.Split(' '))
                {
                    string candidate = line.Length == 0 ? word : line + " " + word;
                    if (MeasureRole(role, candidate, size, uiScale).X > bounds.Width &&
                        line.Length > 0)
                    {
                        DrawRole(spriteBatch, role, line,
                            new Vector2(bounds.X, y), color, size, uiScale);
                        y += lineHeight;
                        line = word;
                    }
                    else
                    {
                        line = candidate;
                    }
                }

                if (line.Length > 0)
                {
                    DrawRole(spriteBatch, role, line, new Vector2(bounds.X, y),
                        color, size, uiScale);
                    y += lineHeight;
                }
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private float FitScale(string text, float maxWidth, float maxScale,
            float minScale)
        {
            float scale = maxScale;
            float width = Measure(text, scale).X;

            if (width > maxWidth && width > 0f)
            {
                scale = maxScale * (maxWidth / width);
                if (scale < minScale) scale = minScale;
            }

            return scale;
        }

        private void DisposeSystems()
        {
            _displaySystem?.Dispose();
            _uiSystem?.Dispose();
            _bodySystem?.Dispose();
            _numericSystem?.Dispose();
            _displaySystem = null;
            _uiSystem = null;
            _bodySystem = null;
            _numericSystem = null;
            _roleFonts.Clear();
            _numericCellWidths.Clear();
        }

        public void Dispose()
        {
            DisposeSystems();
        }
    }
}
