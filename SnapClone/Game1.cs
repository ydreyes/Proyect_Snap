using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnapClone.Core.Services;
using SnapClone.Core.Layout;
using SnapClone.Data.Content;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Draft;
using SnapClone.Gameplay.Run;
using SnapClone.Scenes;

namespace SnapClone
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameServices _services;
        private SceneManager _sceneManager;
        private LayoutManager _layoutManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            // La ventana se ajusta al area de trabajo real (descontando la barra
            // de tareas) manteniendo la relacion 9:16 del modo portrait.
            GetWorkArea(out int workWidth, out int workHeight);

            int maxWidth = (int)(workWidth * 0.92f);
            int maxHeight = (int)(workHeight * 0.94f);

            int height = System.Math.Min(1280, maxHeight);
            int width = (int)(height * 9f / 16f);

            if (width > maxWidth)
            {
                width = maxWidth;
                height = (int)(width * 16f / 9f);
            }

            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left, Top, Right, Bottom;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint action, uint param, ref Rect rect, uint winIni);

        /// <summary>
        /// Devuelve el area util del escritorio (sin barra de tareas).
        /// Si la consulta falla se usa el modo de pantalla actual.
        /// </summary>
        private static void GetWorkArea(out int width, out int height)
        {
            const uint SpiGetWorkArea = 0x0030;
            var rect = new Rect();

            try
            {
                if (SystemParametersInfo(SpiGetWorkArea, 0, ref rect, 0))
                {
                    width = rect.Right - rect.Left;
                    height = rect.Bottom - rect.Top;
                    return;
                }
            }
            catch
            {
                // Sin acceso a user32: se usa el modo de pantalla.
            }

            var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            width = display.Width;
            height = display.Height;
        }

        protected override void Initialize()
        {
            _services = new GameServices();
            _services.Register(this);
            _services.Register<Game>(this);
            _services.Register(Content);
            _services.Register(new InputService());
            _services.Register(new TimeService());
            _services.Register(new RandomService());
            _layoutManager = new LayoutManager(_graphics);
            _services.Register(_layoutManager);

            // Registros de contenido (cartas, zonas, efectos, perks)
            var cardRegistry = new CardRegistry();
            var zoneRegistry = new ZoneRegistry();
            var effectRegistry = new EffectRegistry();
            var perkRegistry = new PerkRegistry();
            ContentLoader.LoadAll(cardRegistry, zoneRegistry, effectRegistry, perkRegistry);
            _services.Register(cardRegistry);
            _services.Register(zoneRegistry);
            _services.Register(effectRegistry);
            _services.Register(perkRegistry);

            var randomService = _services.Get<RandomService>();
            var runState = new RunState();
            _services.Register(runState);
            _services.Register(new DraftSystem(cardRegistry, randomService));
            _services.Register(new RewardSystem(cardRegistry, perkRegistry, randomService));
            _services.Register(new ProgressionManager(runState));
            
            _sceneManager = new SceneManager();
            _services.Register(_sceneManager);

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (_, __) =>
            {
                _layoutManager.CalculateLayout();
                _graphics.ApplyChanges();
            };
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create scenes
            _sceneManager.AddScene("Menu", new MenuScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Draft", new DraftScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Combat", new CombatScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Reward", new RewardScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("GameOver", new GameOverScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            
            _sceneManager.LoadContent();
            _sceneManager.ChangeScene("Menu");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _sceneManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _sceneManager.Draw(gameTime, _spriteBatch);

            base.Draw(gameTime);
        }
    }
}
