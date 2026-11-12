using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Presentacion fija del subjefe de cada acto y del jefe final. No tiene
    /// seleccion de rival ni controles de combate.
    /// </summary>
    public class SubBossIntroScene : IScene
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
        private EnemyDef _enemy;
        private bool _continued;

        public SubBossIntroScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
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

            bool finalBoss = _runState.ActIndex == RunActTable.FinalAct &&
                             _runState.FinalActStep >= 3;

            var enemyRegistry = _services.Has<EnemyRegistry>()
                ? _services.Get<EnemyRegistry>()
                : null;

            _enemy = finalBoss
                ? enemyRegistry?.GetFinalBoss()
                : enemyRegistry?.GetSubBoss(_runState.ActIndex);
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
            _runState.DiscoverEnemy(_enemy?.Id);
            _director.OnSubBossIntroComplete();
            _checkpoint.SavePreCombat();
            _sceneManager.ChangeScene(_director.GetNextScene());
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(26, 10, 16));
            spriteBatch.Begin();

            var viewport = _graphicsDevice.Viewport;
            bool finalBoss = _runState.ActIndex == RunActTable.FinalAct &&
                             _runState.FinalActStep >= 3;

            _fontService.DrawTextCentered(spriteBatch,
                finalBoss ? "JEFE FINAL" : "SUBJEFE",
                new Rectangle(0, (int)(viewport.Height * 0.10f), viewport.Width,
                    (int)(viewport.Height * 0.07f)),
                finalBoss ? new Color(250, 170, 140) : new Color(238, 224, 188), 1.2f);

            _fontService.DrawTextCentered(spriteBatch,
                _runState.ActIndex == RunActTable.FinalAct
                    ? "ACTO FINAL"
                    : $"ACTO {_runState.ActIndex}",
                new Rectangle(0, (int)(viewport.Height * 0.18f), viewport.Width,
                    (int)(viewport.Height * 0.04f)),
                new Color(170, 190, 215), 0.55f);

            var nameBounds = new Rectangle(0, (int)(viewport.Height * 0.30f),
                viewport.Width, (int)(viewport.Height * 0.10f));
            _fontService.DrawTextCenteredFit(spriteBatch,
                string.IsNullOrWhiteSpace(_enemy?.Name) ? "ENEMIGO" : _enemy.Name,
                nameBounds, new Color(244, 228, 186), 1.4f, 0.6f);

            var phraseBounds = new Rectangle((int)(viewport.Width * 0.12f),
                (int)(viewport.Height * 0.44f),
                (int)(viewport.Width * 0.76f),
                (int)(viewport.Height * 0.16f));
            _fontService.DrawTextWrapped(spriteBatch, _enemy?.Phrase ?? string.Empty,
                phraseBounds, new Color(196, 208, 226), 0.65f);

            _fontService.DrawTextCentered(spriteBatch,
                $"DIFICULTAD {_enemy?.Difficulty ?? 1}/5",
                new Rectangle(0, (int)(viewport.Height * 0.64f), viewport.Width,
                    (int)(viewport.Height * 0.05f)),
                new Color(224, 196, 128), 0.6f);

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
                (int)(viewport.Height * 0.82f),
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
