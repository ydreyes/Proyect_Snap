using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SnapClone.Scenes
{
    public interface IScene
    {
        void Initialize();
        void OnEnter();
        void LoadContent();
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }
}
