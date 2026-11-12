using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Transicion segura entre actos: muestra el acto nuevo, el nivel, el
    /// tamano del mazo y el dinero, y espera una accion para continuar.
    /// </summary>
    public class ActTransitionScene : IScene
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private GameServices _services;
        private SceneManager _sceneManager;

        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;

        private RunState _runState;
        private RunDirector _director;
        private RunCheckpointService _checkpoint;
        private bool _continued;

        public ActTransitionScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _services = services;
            _sceneManager = sceneManager;
        }

        public void Initialize() { }

        public void OnEnter()
        {
            _runState = _services.Get<RunState>();
            _director = _services.Get<RunDirector>();
            _checkpoint = _services.Get<RunCheckpointService>();
            _continued = false;
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


            if (_continued || !input.IsLeftMousePressed()) return;
            if (!IsInside(GetContinueButton(), input.MousePosition)) return;

            _continued = true;
            _director.OnActTransitionComplete();
            _checkpoint.SaveSafeNode();
            _sceneManager.ChangeScene(_director.GetNextScene());
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(8, 14, 24));
            spriteBatch.Begin();

            var viewport = _graphicsDevice.Viewport;
            bool finalAct = _runState.ActIndex == RunActTable.FinalAct;

            _fontService.DrawTextCentered(spriteBatch,
                finalAct ? "ACTO FINAL" : $"ACTO {_runState.ActIndex}",
                new Rectangle(0, (int)(viewport.Height * 0.14f), viewport.Width,
                    (int)(viewport.Height * 0.10f)),
                new Color(238, 224, 188), 1.4f);

            _fontService.DrawTextCentered(spriteBatch,
                finalAct ? "ULTIMO TRAMO DE LA RUN" : "TRANSICION DE ACTO",
                new Rectangle(0, (int)(viewport.Height * 0.25f), viewport.Width,
                    (int)(viewport.Height * 0.05f)),
                new Color(170, 190, 215), 0.6f);

            var panel = new Rectangle((int)(viewport.Width * 0.10f),
                (int)(viewport.Height * 0.36f),
                (int)(viewport.Width * 0.80f),
                (int)(viewport.Height * 0.30f));

            _shapeDrawer.DrawBeveledPanel(panel, new Color(18, 30, 48),
                new Color(120, 170, 230), 3, 14);

            var level = new Rectangle(panel.X, panel.Y + (int)(panel.Height * 0.08f),
                panel.Width, (int)(panel.Height * 0.20f));
            var deck = new Rectangle(panel.X, panel.Y + (int)(panel.Height * 0.34f),
                panel.Width, (int)(panel.Height * 0.20f));
            var money = new Rectangle(panel.X, panel.Y + (int)(panel.Height * 0.60f),
                panel.Width, (int)(panel.Height * 0.20f));

            _fontService.DrawTextCentered(spriteBatch, $"NIVEL {_runState.Level}",
                level, new Color(224, 196, 128), 0.8f);
            _fontService.DrawTextCentered(spriteBatch, $"MAZO {_runState.Deck.Count}",
                deck, new Color(196, 214, 236), 0.8f);
            _fontService.DrawTextCentered(spriteBatch, $"DINERO {_runState.Money}",
                money, new Color(150, 220, 170), 0.8f);

            var modifiers = new Rectangle(panel.X + 8,
                panel.Y + (int)(panel.Height * 0.80f),
                panel.Width - 16,
                (int)(panel.Height * 0.16f));
            _fontService.DrawTextCenteredFit(spriteBatch,
                RunModifierSystem.Describe(_runState), modifiers,
                new Color(170, 190, 215), 0.45f, 0.25f);

            var button = GetContinueButton();
            _shapeDrawer.DrawBeveledPanel(button, new Color(30, 104, 74),
                new Color(126, 214, 170), 3, 12);
            _fontService.DrawTextCentered(spriteBatch, "CONTINUAR", button,
                Color.White, 0.8f);

            spriteBatch.End();
        }

        private Rectangle GetContinueButton()
        {
            var viewport = _graphicsDevice.Viewport;
            return new Rectangle(
                (int)(viewport.Width * 0.20f),
                (int)(viewport.Height * 0.80f),
                (int)(viewport.Width * 0.60f),
                (int)(viewport.Height * 0.075f));
        }

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
