using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace SnapClone.UI.Components
{
    public class FontService
    {
        private readonly SpriteFont _defaultFont;

        public FontService(ContentManager content)
        {
            try
            {
                _defaultFont = content.Load<SpriteFont>("DefaultFont");
            }
            catch
            {
                _defaultFont = null;
            }
        }

        public bool HasFont => _defaultFont != null;

        public Vector2 Measure(string text, float scale = 1f)
        {
            if (_defaultFont == null || string.IsNullOrEmpty(text)) return Vector2.Zero;
            return _defaultFont.MeasureString(text) * scale;
        }

        public void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 1f)
        {
            if (_defaultFont == null || string.IsNullOrEmpty(text)) return;
            spriteBatch.DrawString(_defaultFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public void DrawTextCentered(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color, float scale = 1f)
        {
            if (_defaultFont == null || string.IsNullOrEmpty(text)) return;

            var size = _defaultFont.MeasureString(text) * scale;
            var position = new Vector2(
                bounds.X + (bounds.Width - size.X) / 2f,
                bounds.Y + (bounds.Height - size.Y) / 2f);

            spriteBatch.DrawString(_defaultFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public void DrawTextCentered(SpriteBatch spriteBatch, string text, Vector2 center, Color color, float scale = 1f)
        {
            if (_defaultFont == null || string.IsNullOrEmpty(text)) return;

            var size = _defaultFont.MeasureString(text) * scale;
            var origin = new Vector2(center.X - size.X / 2f, center.Y - size.Y / 2f);
            spriteBatch.DrawString(_defaultFont, text, origin, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        /// <summary>Dibuja centrado reduciendo la escala hasta que el texto quepa en el ancho.</summary>
        public void DrawTextCenteredFit(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color, float maxScale = 1f, float minScale = 0.3f)
        {
            if (_defaultFont == null || string.IsNullOrEmpty(text)) return;

            float scale = maxScale;
            var size = _defaultFont.MeasureString(text).X * scale;

            if (size > bounds.Width)
            {
                scale = maxScale * (bounds.Width / size);
                if (scale < minScale) scale = minScale;
            }

            DrawTextCentered(spriteBatch, text, bounds, color, scale);
        }

        public void DrawTextWrapped(SpriteBatch spriteBatch, string text, Rectangle bounds, Color color, float scale = 1f)
        {
            if (_defaultFont == null || string.IsNullOrEmpty(text)) return;

            var words = text.Split(' ');
            string line = string.Empty;
            float y = bounds.Y;

            foreach (var word in words)
            {
                string candidate = line.Length == 0 ? word : line + " " + word;
                if (_defaultFont.MeasureString(candidate).X * scale > bounds.Width && line.Length > 0)
                {
                    spriteBatch.DrawString(_defaultFont, line, new Vector2(bounds.X, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    y += _defaultFont.LineSpacing * scale;
                    line = word;
                }
                else
                {
                    line = candidate;
                }
            }

            if (line.Length > 0)
            {
                spriteBatch.DrawString(_defaultFont, line, new Vector2(bounds.X, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
