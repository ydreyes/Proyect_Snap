using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Menu principal. Todo el layout se calcula con proporciones del
    /// viewport real, asi funciona en cualquier tamano de pantalla.
    /// </summary>
    public class MenuScene : IScene
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;
        private GameServices _services;
        private SceneManager _sceneManager;

        private Rectangle _newRunButton;
        private Rectangle _quitButton;

        public MenuScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameServices services, SceneManager sceneManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _services = services;
            _sceneManager = sceneManager;
        }

        public void Initialize() { }
        public void OnEnter() { }

        public void LoadContent()
        {
            _shapeDrawer = new ShapeDrawer(_graphicsDevice, _spriteBatch);

            var content = _services.Has<Microsoft.Xna.Framework.Content.ContentManager>()
                ? _services.Get<Microsoft.Xna.Framework.Content.ContentManager>()
                : _services.Get<Game>().Content;

            _fontService = new FontService(content);
        }

        private void RecalculateLayout()
        {
            var viewport = _graphicsDevice.Viewport;
            int w = viewport.Width;
            int h = viewport.Height;

            _newRunButton = new Rectangle((int)(w * 0.18f), (int)(h * 0.52f), (int)(w * 0.64f), (int)(h * 0.075f));
            _quitButton = new Rectangle((int)(w * 0.18f), (int)(h * 0.62f), (int)(w * 0.64f), (int)(h * 0.075f));
        }

        public void Update(GameTime gameTime)
        {
            var input = _services.Get<InputService>();
            input.Update();

            if (!input.IsLeftMousePressed()) return;

            var mouse = input.MousePosition;
            if (IsInside(_newRunButton, mouse))
            {
                _sceneManager.ChangeScene("Draft");
            }
            else if (IsInside(_quitButton, mouse))
            {
                _services.Get<Game>().Exit();
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(8, 6, 18));
            spriteBatch.Begin();

            RecalculateLayout();

            var viewport = _graphicsDevice.Viewport;

            // Titulo
            var titleScale = 1.9f;
            var titleSize = _fontService.Measure("SnapClone", titleScale);
            var titlePosition = new Vector2(
                (viewport.Width - titleSize.X) / 2f,
                viewport.Height * 0.31f);

            _fontService.DrawText(spriteBatch, "SnapClone", titlePosition, new Color(238, 224, 188), titleScale);

            var subtitle = "MARVEL SNAP STYLE";
            var subtitleScale = 0.8f;
            var subtitleSize = _fontService.Measure(subtitle, subtitleScale);
            _fontService.DrawText(spriteBatch, subtitle,
                new Vector2((viewport.Width - subtitleSize.X) / 2f, titlePosition.Y + titleSize.Y + 8),
                new Color(150, 170, 205), subtitleScale);

            // Botones
            _shapeDrawer.DrawBeveledPanel(_newRunButton, new Color(30, 84, 160), new Color(140, 190, 250), 3, 12);
            _shapeDrawer.DrawBeveledPanel(_quitButton, new Color(120, 48, 44), new Color(240, 160, 140), 3, 12);

            _fontService.DrawTextCentered(spriteBatch, "NUEVA PARTIDA", _newRunButton, Color.White, 1.0f);
            _fontService.DrawTextCentered(spriteBatch, "SALIR", _quitButton, Color.White, 1.0f);

            spriteBatch.End();
        }

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
