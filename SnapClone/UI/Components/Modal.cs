using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Primitives;

namespace SnapClone.UI.Components
{
    /// <summary>
    /// Panel modal dibujado encima de la escena de combate.
    /// Bloquea la interaccion con el tablero mientras esta visible.
    /// </summary>
    public class Modal
    {
        private readonly ShapeDrawer _shapeDrawer;
        private readonly FontService _fontService;

        private bool _isVisible;
        private string _title = string.Empty;
        private string _message = string.Empty;
        private Rectangle _buttonBounds;
        private Color _accent = new Color(72, 126, 210);

        public bool IsVisible => _isVisible;
        public Rectangle ButtonBounds => _buttonBounds;

        public Modal(ShapeDrawer shapeDrawer, FontService fontService)
        {
            _shapeDrawer = shapeDrawer;
            _fontService = fontService;
        }

        public void Show(string title, string message, Color accent)
        {
            _title = title;
            _message = message;
            _accent = accent;
            _isVisible = true;
        }

        public void Hide() => _isVisible = false;

        public void Draw(SpriteBatch spriteBatch, int screenWidth, int screenHeight)
        {
            if (!_isVisible) return;

            // Fondo oscurecido
            _shapeDrawer.DrawRectangle(new Rectangle(0, 0, screenWidth, screenHeight), Color.Black * 0.72f);

            int panelWidth = (int)(screenWidth * 0.80f);
            int panelHeight = (int)(screenHeight * 0.34f);
            var panel = new Rectangle(
                (screenWidth - panelWidth) / 2,
                (screenHeight - panelHeight) / 2,
                panelWidth,
                panelHeight);

            _shapeDrawer.DrawBeveledPanel(panel, new Color(20, 24, 40), _accent, 4, 16);

            var titleRect = new Rectangle(panel.X, panel.Y + (int)(panel.Height * 0.12f), panel.Width, (int)(panel.Height * 0.26f));
            _fontService.DrawTextCentered(spriteBatch, _title, titleRect, Color.White, 1.35f);

            var messageRect = new Rectangle(panel.X + 10, titleRect.Bottom, panel.Width - 20, (int)(panel.Height * 0.24f));
            _fontService.DrawTextCentered(spriteBatch, _message, messageRect, new Color(212, 220, 238), 0.75f);

            int buttonWidth = (int)(panel.Width * 0.55f);
            int buttonHeight = (int)(panel.Height * 0.20f);
            _buttonBounds = new Rectangle(
                panel.Center.X - buttonWidth / 2,
                panel.Bottom - buttonHeight - (int)(panel.Height * 0.10f),
                buttonWidth,
                buttonHeight);

            _shapeDrawer.DrawBeveledPanel(_buttonBounds, new Color(34, 104, 72), new Color(126, 210, 168), 3, 10);
            _fontService.DrawTextCentered(spriteBatch, "CONTINUAR", _buttonBounds, Color.White, 0.9f);
        }

        public bool IsButtonClicked(Vector2 mousePosition)
        {
            return _isVisible && _buttonBounds.Contains((int)mousePosition.X, (int)mousePosition.Y);
        }
    }
}
