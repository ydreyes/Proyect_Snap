using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Run;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Pantalla de fin de run: victoria tras el combate 15 o derrota al
    /// quedarse sin vidas. Ambas vuelven al menu con un clic.
    /// </summary>
    public class GameOverScene : IScene
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;
        private GameServices _services;
        private SceneManager _sceneManager;
        private RunState _runState;

        public GameOverScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameServices services, SceneManager sceneManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _services = services;
            _sceneManager = sceneManager;
        }

        public void Initialize() { }

        public void OnEnter()
        {
            _runState = _services.Has<RunState>() ? _services.Get<RunState>() : null;
        }

        public void LoadContent()
        {
            _shapeDrawer = new ShapeDrawer(_graphicsDevice, _spriteBatch);

            var content = _services.Has<Microsoft.Xna.Framework.Content.ContentManager>()
                ? _services.Get<Microsoft.Xna.Framework.Content.ContentManager>()
                : _services.Get<Game>().Content;

            _fontService = new FontService(content);
        }

        public void Update(GameTime gameTime)
        {
            var input = _services.Get<InputService>();
            input.Update();

            if (input.IsLeftMousePressed())
            {
                _sceneManager.ChangeScene("Menu");
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(22, 8, 8));
            spriteBatch.Begin();

            var viewport = _graphicsDevice.Viewport;
            bool victory = _runState != null && _runState.IsVictory;

            string title = victory ? "VICTORIA" : "GAME OVER";
            Color titleColor = victory ? new Color(150, 230, 170) : new Color(240, 170, 150);

            _fontService.DrawTextCentered(spriteBatch, title,
                new Rectangle(0, (int)(viewport.Height * 0.34f), viewport.Width, (int)(viewport.Height * 0.10f)),
                titleColor, 1.6f);

            string subtitle = victory
                ? "HAS COMPLETADO LOS 15 COMBATES"
                : "TE HAS QUEDADO SIN VIDAS";

            _fontService.DrawTextCentered(spriteBatch, subtitle,
                new Rectangle(0, (int)(viewport.Height * 0.48f), viewport.Width, (int)(viewport.Height * 0.06f)),
                new Color(215, 200, 190), 0.8f);

            _fontService.DrawTextCentered(spriteBatch, "CLIC PARA VOLVER AL MENU",
                new Rectangle(0, (int)(viewport.Height * 0.56f), viewport.Width, (int)(viewport.Height * 0.06f)),
                new Color(190, 180, 175), 0.65f);

            spriteBatch.End();
        }
    }
}
