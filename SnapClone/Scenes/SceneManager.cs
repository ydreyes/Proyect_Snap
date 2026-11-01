using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnapClone.Scenes
{
    public class SceneManager
    {
        private readonly Dictionary<string, IScene> _scenes = new Dictionary<string, IScene>();
        private readonly HashSet<string> _initialized = new HashSet<string>();
        private IScene _currentScene;

        public void AddScene(string name, IScene scene)
        {
            _scenes[name] = scene;
        }
        
        public void ChangeScene(string name)
        {
            if (!_scenes.TryGetValue(name, out IScene newScene)) return;

            _currentScene = newScene;

            if (_initialized.Add(name))
            {
                _currentScene.Initialize();
            }

            // Cada entrada reinicia el estado de la escena (nueva run, nuevo tablero).
            _currentScene.OnEnter();
        }
        
        public void LoadContent()
        {
            foreach (var scene in _scenes.Values)
            {
                scene?.LoadContent();
            }
        }
        
        public void Update(GameTime gameTime)
        {
            _currentScene?.Update(gameTime);
        }
        
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _currentScene?.Draw(gameTime, spriteBatch);
        }
    }
}
