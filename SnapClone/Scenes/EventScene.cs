using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Nodo Event: muestra el evento guardado en la run, permite aceptar con
    /// coste/recompensa o declinar siempre. No inicia combate.
    /// </summary>
    public class EventScene : IScene
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly GameServices _services;
        private readonly SceneManager _sceneManager;

        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;

        private RunState _runState;
        private RunDirector _director;
        private RunCheckpointService _checkpoint;
        private RunEventSystem _eventSystem;

        private RunEventDef _definition;
        private string _error;
        private bool _finished;

        public EventScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
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
            _eventSystem = _services.Get<RunEventSystem>();

            _definition = _eventSystem.EnsureEvent(_runState);
            _error = null;
            _finished = false;
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
            if (_finished) return;

            var input = _services.Get<InputService>();
            if (!input.IsLeftMousePressed()) return;

            var mouse = input.MousePosition;

            if (_eventSystem.IsResolved(_runState) || _definition == null)
            {
                if (IsInside(GetContinueButton(), mouse))
                    Finish();
                return;
            }

            if (IsInside(GetAcceptButton(), mouse))
            {
                var result = _eventSystem.Resolve(_runState, true);
                if (result.Success)
                    Finish();
                else
                    _error = result.Error;
                return;
            }

            if (IsInside(GetDeclineButton(), mouse))
            {
                var result = _eventSystem.Resolve(_runState, false);
                if (result.Success)
                    Finish();
                else
                    _error = result.Error;
            }
        }

        private void Finish()
        {
            if (_finished) return;

            _finished = true;
            _director.OnShopEventComplete();
            _checkpoint.SaveSafeNode();
            _sceneManager.ChangeScene(_director.GetNextScene());
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(10, 16, 18));
            spriteBatch.Begin();

            var viewport = _graphicsDevice.Viewport;

            _fontService.DrawTextCentered(spriteBatch, "EVENTO",
                new Rectangle(0, (int)(viewport.Height * 0.025f), viewport.Width,
                    (int)(viewport.Height * 0.065f)),
                new Color(196, 230, 210), 1.1f);

            if (_definition == null)
            {
                _fontService.DrawTextCentered(spriteBatch, "SIN EVENTOS DISPONIBLES",
                    new Rectangle(0, (int)(viewport.Height * 0.42f), viewport.Width,
                        (int)(viewport.Height * 0.08f)),
                    new Color(220, 210, 190), 0.8f);
                DrawButton(spriteBatch, GetContinueButton(), "CONTINUAR", true);
                spriteBatch.End();
                return;
            }

            _fontService.DrawTextCenteredFit(spriteBatch, _definition.Title,
                GetTitleBounds(viewport.Width, viewport.Height),
                new Color(238, 224, 188), 1.0f, 0.4f);

            _fontService.DrawTextWrapped(spriteBatch, _definition.Text,
                GetTextBounds(viewport.Width, viewport.Height),
                new Color(196, 208, 226), 0.52f);

            var acceptPanel = GetAcceptPanel();
            _shapeDrawer.DrawBeveledPanel(acceptPanel, new Color(18, 34, 42),
                new Color(120, 180, 170), 3, 12);
            _fontService.DrawTextCenteredFit(spriteBatch, _definition.AcceptText,
                new Rectangle(acceptPanel.X + 8, acceptPanel.Y + 4,
                    acceptPanel.Width - 16, acceptPanel.Height - 8),
                new Color(196, 240, 220), 0.62f, 0.32f);

            bool resolved = _eventSystem.IsResolved(_runState);

            if (resolved)
            {
                _fontService.DrawTextCentered(spriteBatch, "EVENTO RESUELTO",
                    new Rectangle(0, (int)(viewport.Height * 0.44f), viewport.Width,
                        (int)(viewport.Height * 0.05f)),
                    new Color(170, 220, 190), 0.7f);
                DrawButton(spriteBatch, GetContinueButton(), "CONTINUAR", true);
            }
            else
            {
                bool canAccept = _eventSystem.CanAccept(_runState);
                DrawButton(spriteBatch, GetAcceptButton(), "ACEPTAR", canAccept);
                DrawButton(spriteBatch, GetDeclineButton(), "DECLINAR", true);
            }

            _fontService.DrawTextCenteredFit(spriteBatch,
                RunModifierSystem.Describe(_runState),
                GetModifiersBounds(viewport.Width, viewport.Height),
                new Color(170, 190, 215), 0.42f, 0.25f);

            _fontService.DrawTextCenteredFit(spriteBatch,
                $"ACTO {_runState.ActIndex}   DINERO {_runState.Money}   MAZO {_runState.Deck.Count}",
                GetStatusBounds(viewport.Width, viewport.Height),
                new Color(224, 196, 128), 0.5f, 0.3f);

            DrawError(spriteBatch, viewport);
            spriteBatch.End();
        }

        private void DrawError(SpriteBatch spriteBatch, Viewport viewport)
        {
            if (string.IsNullOrEmpty(_error)) return;

            _fontService.DrawTextCenteredFit(spriteBatch, _error,
                GetErrorBounds(viewport.Width, viewport.Height),
                new Color(244, 170, 150), 0.45f, 0.28f);
        }

        private void DrawButton(SpriteBatch spriteBatch, Rectangle bounds, string label,
            bool enabled)
        {
            var fill = enabled ? new Color(30, 104, 74) : new Color(38, 44, 54);
            var border = enabled ? new Color(126, 214, 170) : new Color(92, 100, 112);

            _shapeDrawer.DrawBeveledPanel(bounds, fill, border, 3, 10);
            _fontService.DrawTextCenteredFit(spriteBatch, label, bounds,
                enabled ? Color.White : new Color(180, 186, 196), 0.72f, 0.34f);
        }

        private Rectangle GetAcceptPanel() =>
            GetAcceptPanel(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

        private Rectangle GetAcceptButton() =>
            GetAcceptButton(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

        private Rectangle GetContinueButton() =>
            GetContinueButton(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

        private Rectangle GetDeclineButton() =>
            GetDeclineButton(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

        public static Rectangle GetTitleBounds(int width, int height) =>
            new Rectangle((int)(width * 0.06f), (int)(height * 0.10f),
                (int)(width * 0.88f), (int)(height * 0.07f));

        public static Rectangle GetTextBounds(int width, int height) =>
            new Rectangle((int)(width * 0.08f), (int)(height * 0.19f),
                (int)(width * 0.84f), (int)(height * 0.20f));

        public static Rectangle GetAcceptPanel(int width, int height) =>
            new Rectangle(
                (int)(width * 0.10f),
                (int)(height * 0.40f),
                (int)(width * 0.80f),
                (int)(height * 0.10f));

        public static Rectangle GetAcceptButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.12f),
                (int)(height * 0.53f),
                (int)(width * 0.76f),
                (int)(height * 0.07f));

        public static Rectangle GetContinueButton(int width, int height) =>
            GetAcceptButton(width, height);

        public static Rectangle GetDeclineButton(int width, int height) =>
            new Rectangle(
                (int)(width * 0.12f),
                (int)(height * 0.615f),
                (int)(width * 0.76f),
                (int)(height * 0.07f));

        public static Rectangle GetModifiersBounds(int width, int height) =>
            new Rectangle((int)(width * 0.05f), (int)(height * 0.78f),
                (int)(width * 0.90f), (int)(height * 0.035f));

        public static Rectangle GetStatusBounds(int width, int height) =>
            new Rectangle(0, (int)(height * 0.825f), width, (int)(height * 0.035f));

        public static Rectangle GetErrorBounds(int width, int height) =>
            new Rectangle((int)(width * 0.06f), (int)(height * 0.87f),
                (int)(width * 0.88f), (int)(height * 0.035f));

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
