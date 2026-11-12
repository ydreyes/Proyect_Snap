using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnapClone.Scenes
{
    public enum SceneTransitionMode
    {
        Fade,
        Instant
    }

    public enum TransitionPhase
    {
        Idle,
        FadeOut,
        FadeIn
    }

    /// <summary>
    /// Administra escenas y una transicion de fundido acotada:
    /// Idle -> FadeOut -> cambio diferido -> FadeIn -> Idle.
    /// Mientras hay transicion no se actualiza la escena ni se acepta input.
    /// </summary>
    public class SceneManager
    {
        private const float FadeOutDuration = 0.18f;
        private const float FadeInDuration = 0.22f;

        private readonly Dictionary<string, IScene> _scenes =
            new Dictionary<string, IScene>();

        private readonly HashSet<string> _initialized =
            new HashSet<string>();

        private IScene _currentScene;
        private bool _sceneChanged;

        private string _pendingScene;
        private float _transitionElapsed;
        private Texture2D _pixel;

        public string CurrentSceneName { get; private set; }
        public TransitionPhase Phase { get; private set; } = TransitionPhase.Idle;
        public float FadeAlpha { get; private set; }
        public string PendingSceneName => _pendingScene;
        public bool IsTransitioning => Phase != TransitionPhase.Idle;

        /// <summary>El input de escena se ignora mientras hay transicion.</summary>
        public bool InputBlocked => IsTransitioning;

        /// <summary>
        /// Consume el aviso de cambio de escena para ignorar el frame de input
        /// que aun pertenece a la escena anterior.
        /// </summary>
        public bool ConsumeSceneChange()
        {
            if (!_sceneChanged) return false;

            _sceneChanged = false;
            return true;
        }

        public void AddScene(string name, IScene scene)
        {
            _scenes[name] = scene;
        }

        public void ChangeScene(string name) =>
            ChangeScene(name, SceneTransitionMode.Fade);

        public void ChangeScene(string name, SceneTransitionMode mode)
        {
            if (!_scenes.ContainsKey(name)) return;

            if (mode == SceneTransitionMode.Instant)
            {
                SwitchTo(name);
                Phase = TransitionPhase.Idle;
                FadeAlpha = 0f;
                _pendingScene = null;
                _transitionElapsed = 0f;
                return;
            }

            _pendingScene = name;

            if (Phase == TransitionPhase.Idle)
            {
                Phase = TransitionPhase.FadeOut;
                _transitionElapsed = 0f;
                FadeAlpha = 0f;
                return;
            }

            // Nunca se apilan transiciones: un cambio durante el fundido de
            // entrada se convierte en una salida desde el alpha actual.
            if (Phase == TransitionPhase.FadeIn)
            {
                Phase = TransitionPhase.FadeOut;
                _transitionElapsed = FadeOutDuration * (1f - FadeAlpha);
                FadeAlpha = MathHelper.Clamp(FadeAlpha, 0f, 1f);
            }
        }

        public void LoadContent()
        {
            foreach (var scene in _scenes.Values)
            {
                scene?.LoadContent();
            }
        }

        /// <summary>
        /// Avanza el fundido. Debe llamarse antes del manejo de input para que
        /// la transicion progrese aunque la escena no se actualice.
        /// </summary>
        public void UpdateTransition(float deltaSeconds)
        {
            if (Phase == TransitionPhase.Idle) return;

            _transitionElapsed += MathF.Max(0f, deltaSeconds);

            if (Phase == TransitionPhase.FadeOut)
            {
                FadeAlpha = MathHelper.Clamp(_transitionElapsed / FadeOutDuration,
                    0f, 1f);

                if (_transitionElapsed >= FadeOutDuration)
                {
                    SwitchTo(_pendingScene);
                    Phase = TransitionPhase.FadeIn;
                    _transitionElapsed = 0f;
                    FadeAlpha = 1f;
                }

                return;
            }

            FadeAlpha = 1f - MathHelper.Clamp(_transitionElapsed / FadeInDuration,
                0f, 1f);

            if (_transitionElapsed >= FadeInDuration)
            {
                Phase = TransitionPhase.Idle;
                FadeAlpha = 0f;
                _pendingScene = null;
                _transitionElapsed = 0f;
            }
        }

        public void UpdateTransition(GameTime gameTime) =>
            UpdateTransition((float)gameTime.ElapsedGameTime.TotalSeconds);

        private void SwitchTo(string name)
        {
            if (string.IsNullOrEmpty(name) ||
                !_scenes.TryGetValue(name, out IScene newScene))
                return;

            _currentScene = newScene;
            CurrentSceneName = name;

            if (_initialized.Add(name))
                _currentScene.Initialize();

            // Cada entrada reinicia el estado de la escena.
            _currentScene.OnEnter();
            _sceneChanged = true;
        }

        public void Update(GameTime gameTime)
        {
            if (IsTransitioning) return;
            _currentScene?.Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _currentScene?.Draw(gameTime, spriteBatch);
            DrawTransitionOverlay(spriteBatch);
        }

        private void DrawTransitionOverlay(SpriteBatch spriteBatch)
        {
            if (FadeAlpha <= 0.001f) return;

            if (_pixel == null)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }

            var viewport = spriteBatch.GraphicsDevice.Viewport;
            spriteBatch.Begin();
            spriteBatch.Draw(_pixel,
                new Rectangle(viewport.X, viewport.Y, viewport.Width,
                    viewport.Height), Color.Black * FadeAlpha);
            spriteBatch.End();
        }
    }
}
