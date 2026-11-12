using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnapClone.Core.Services;
using SnapClone.Core.Layout;
using SnapClone.Data.Content;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Draft;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Meta;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.Scenes;
using SnapClone.UI;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;
using SnapClone.UI.Rendering;

namespace SnapClone
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameServices _services;
        private SceneManager _sceneManager;
        private LayoutManager _layoutManager;
        private RunState _runState;
        private RunDirector _runDirector;
        private DraftSystem _draftSystem;
        private RunCheckpointService _checkpoint;
        private BackService _backService;
        private TimeService _timeService;
        private PauseOverlay _pause;
        private bool _isAppPaused;
        private Rectangle _appliedSafeInsets;
        private bool _safeAreaApplied;
        private UiContext _ui;

        /// <summary>
        /// Lo asigna el host Android antes de Run(); en escritorio queda null y
        /// el viewport usa toda la superficie.
        /// </summary>
        public ISafeAreaProvider SafeAreaProvider { get; set; }

        /// <summary>
        /// Directorio de datos persistente. Android pasa FilesDir; en escritorio
        /// se usa la carpeta local del usuario.
        /// </summary>
        public string DataDirectory { get; set; }

        /// <summary>Lo usa el host Android para cerrar la actividad al salir.</summary>
        public Action ExitRequested { get; set; }

        public void RequestAppExit()
        {
            ExitRequested?.Invoke();
            Exit();
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            // En Android el tamano de la superficie lo decide la plataforma.
            if (!OperatingSystem.IsAndroid())
            {
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
            }

            Content.RootDirectory = "Content";
            IsMouseVisible = !OperatingSystem.IsAndroid();
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
            _timeService = new TimeService();
            _services.Register(_timeService);
            _backService = new BackService();
            _services.Register(_backService);
            _services.Register(new RandomService());
            _layoutManager = new LayoutManager(_graphics);
            _services.Register(_layoutManager);

            // Registros de contenido (cartas, zonas, efectos, perks)
            var cardRegistry = new CardRegistry();
            var zoneRegistry = new ZoneRegistry();
            var effectRegistry = new EffectRegistry();
            var perkRegistry = new PerkRegistry();
            var enemyRegistry = new EnemyRegistry();
            var modifierRegistry = new ModifierRegistry();
            var eventRegistry = new RunEventRegistry();
            var itemRegistry = new ItemRegistry();
            ContentLoader.LoadAll(cardRegistry, zoneRegistry, effectRegistry,
                perkRegistry, enemyRegistry, modifierRegistry, eventRegistry,
                itemRegistry);
            _services.Register(cardRegistry);
            _services.Register(zoneRegistry);
            _services.Register(effectRegistry);
            _services.Register(perkRegistry);
            _services.Register(enemyRegistry);
            _services.Register(modifierRegistry);
            _services.Register(eventRegistry);
            _services.Register(itemRegistry);

            var randomService = _services.Get<RandomService>();
            var modifierSystem = new RunModifierSystem(modifierRegistry);
            _runState = new RunState();
            var powerResolver = new PowerResolver(perkRegistry);
            _runDirector = new RunDirector(_runState, randomService, enemyRegistry);
            // El perfil se carga en checkpoint.Initialize, mas abajo; la lambda
            // lo lee en cada draft/oferta y no en el constructor.
            _draftSystem = new DraftSystem(cardRegistry, randomService,
                () => _checkpoint?.Profile?.OwnedCardIds);
            _services.Register(_runState);
            _services.Register(_runDirector);
            _services.Register(_draftSystem);
            _services.Register(modifierSystem);
            _services.Register(new LevelUpSystem(cardRegistry, perkRegistry, randomService,
                powerResolver, () => _checkpoint?.Profile?.OwnedCardIds,
                effectRegistry));
            _services.Register(new IngameShopSystem(cardRegistry, randomService,
                modifierSystem, powerResolver));
            _services.Register(new RunEventSystem(eventRegistry, cardRegistry,
                perkRegistry, randomService, modifierSystem, powerResolver,
                effectRegistry));
            _services.Register(new ProgressionManager(_runDirector));

            var storeSystem = new GlobalStoreSystem(cardRegistry, perkRegistry,
                itemRegistry, randomService);
            var collectionSystem = new CollectionSystem(cardRegistry, perkRegistry,
                storeSystem, effectRegistry);
            _services.Register(storeSystem);
            _services.Register(collectionSystem);
            _services.Register(new MetaProgressionSystem());

            var storage = new FileStorage(DataDirectory);
            var profileStore = new JsonProfileStore(storage);
            var runStore = new JsonRunStore(storage);
            _checkpoint = new RunCheckpointService(_runState, _draftSystem,
                runStore, profileStore, cardRegistry, randomService, storeSystem);
            _services.Register(profileStore);
            _services.Register(runStore);
            _services.Register(_checkpoint);
            _checkpoint.Initialize();

            _sceneManager = new SceneManager();
            _services.Register(_sceneManager);

            if (!OperatingSystem.IsAndroid())
            {
                Window.AllowUserResizing = true;
                Window.ClientSizeChanged += (_, __) =>
                {
                    _layoutManager.CalculateLayout();
                    _graphics.ApplyChanges();
                };
            }
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Servicios visuales compartidos: una textura, una fuente y un
            // renderer para las siete pantallas.
            _ui = new UiContext(GraphicsDevice, Content);
            _services.Register(_ui);
            _services.Register(_ui.Textures);
            _services.Register(_ui.Fonts);
            _services.Register(_ui.Chrome);
            _services.Register(_ui.CardArt);
            _services.Register(_ui.Cards);
            _services.Register(new ShapeDrawer(GraphicsDevice, _spriteBatch));

            _pause = new PauseOverlay(GraphicsDevice, _spriteBatch, _services);
            _pause.LoadContent();
            
            // Create scenes
            _sceneManager.AddScene("Menu", new MenuScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Draft", new DraftScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("EnemySelect", new EnemySelectScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("SubBossIntro", new SubBossIntroScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("ActTransition", new ActTransitionScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Shop", new ShopScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Event", new EventScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Combat", new CombatScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("LevelUp", new LevelUpScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Summary", new RunSummaryScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("GlobalStore", new GlobalStoreScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));
            _sceneManager.AddScene("Collection", new CollectionScene(GraphicsDevice, _spriteBatch, _services, _sceneManager));

            bool diagnostics = TryGetUiDiagnostics(out string demoScreen);
            if (diagnostics)
            {
                _sceneManager.AddScene("UiGallery",
                    new UiGalleryScene(GraphicsDevice, _spriteBatch, _services,
                        _sceneManager));
            }

            _sceneManager.LoadContent();

            if (diagnostics && !string.IsNullOrEmpty(demoScreen))
            {
                UiDemoState.Prepare(_services, demoScreen);
                _sceneManager.ChangeScene(demoScreen,
                    SceneTransitionMode.Instant);
            }
            else if (diagnostics)
            {
                _sceneManager.ChangeScene("UiGallery",
                    SceneTransitionMode.Instant);
            }
            else
            {
                _sceneManager.ChangeScene("Menu", SceneTransitionMode.Instant);
            }
        }

        /// <summary>
        /// Galeria y pantallas de demostracion para validar el rediseno. Solo
        /// se activan por variable de entorno o por un archivo en el
        /// directorio de datos; nunca en el flujo normal.
        /// </summary>
        private bool TryGetUiDiagnostics(out string demoScreen)
        {
            demoScreen = null;

            bool gallery = string.Equals(
                Environment.GetEnvironmentVariable("SNAPCLONE_UI_GALLERY"),
                "1", StringComparison.Ordinal);

            if (string.IsNullOrEmpty(DataDirectory)) return gallery;

            try
            {
                string demoFile = Path.Combine(DataDirectory, "ui-demo.txt");
                if (File.Exists(demoFile))
                {
                    demoScreen = NormalizeDemoScreen(
                        File.ReadAllText(demoFile).Trim());
                    return demoScreen != null || gallery;
                }

                return gallery ||
                       File.Exists(Path.Combine(DataDirectory,
                           "ui-gallery.flag"));
            }
            catch (IOException)
            {
                return gallery;
            }
            catch (UnauthorizedAccessException)
            {
                return gallery;
            }
        }

        private static string NormalizeDemoScreen(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            switch (name.Trim().ToLowerInvariant())
            {
                case "menu": return "Menu";
                case "draft": return "Draft";
                case "summary": return "Summary";
                case "collection": return "Collection";
                case "levelup": return "LevelUp";
                case "globalstore": return "GlobalStore";
                case "shop": return "Shop";
                default: return null;
            }
        }

        protected override void UnloadContent()
        {
            _ui?.Dispose();
            _ui = null;
            base.UnloadContent();
        }

        /// <summary>
        /// Reduce el viewport al area segura del dispositivo. Como todas las
        /// escenas calculan con Viewport.Width/Height, una sola transformacion
        /// mantiene el contenido fuera del notch y de las barras del sistema.
        /// </summary>
        private void ApplySafeArea()
        {
            if (SafeAreaProvider == null)
                return;

            var insets = SafeAreaProvider.GetInsets();

            var presentation = GraphicsDevice.PresentationParameters;
            int x = System.Math.Max(0, insets.X);
            int y = System.Math.Max(0, insets.Y);
            int width = presentation.BackBufferWidth - x - System.Math.Max(0, insets.Width);
            int height = presentation.BackBufferHeight - y - System.Math.Max(0, insets.Height);

            if (width <= 0 || height <= 0)
                return;

            var current = GraphicsDevice.Viewport;
            if (_safeAreaApplied && insets == _appliedSafeInsets &&
                current.X == x && current.Y == y &&
                current.Width == width && current.Height == height)
                return;

            GraphicsDevice.Viewport = new Viewport(x, y, width, height);
            _appliedSafeInsets = insets;
            _safeAreaApplied = true;
            _layoutManager.CalculateLayout();

            if (_services != null && _services.Has<InputService>())
                _services.Get<InputService>().PointerOffset = new Vector2(x, y);
        }

        protected override void Update(GameTime gameTime)
        {
            ApplySafeArea();

            if (_isAppPaused)
            {
                base.Update(gameTime);
                return;
            }

            var input = _services.Get<InputService>();
            input.Update();

            // La transicion avanza antes que el input, incluso cuando la escena
            // tiene el input bloqueado.
            _sceneManager.UpdateTransition(gameTime);

            if (_sceneManager.IsTransitioning)
            {
                _backService.Consume();
                _sceneManager.ConsumeSceneChange();
                base.Update(gameTime);
                return;
            }

            // En Android la actividad reenvia el gesto/tecla a RequestBack().
            if (!OperatingSystem.IsAndroid() &&
                (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                 input.IsKeyPressed(Keys.Escape)))
                _backService.Request();

            string sceneName = _sceneManager.CurrentSceneName;
            bool back = _backService.Consume();

            if (_pause.IsOpen)
            {
                if (back)
                {
                    _pause.Close();
                    _timeService.ResumeTurnTimer();
                }
                else
                {
                    bool isCombat = string.Equals(sceneName, "Combat",
                        System.StringComparison.Ordinal);
                    HandlePauseAction(_pause.Update(input, isCombat));
                }

                base.Update(gameTime);
                return;
            }

            if (IsRunScene(sceneName))
            {
                bool pauseClicked = input.IsLeftMousePressed() &&
                                    _pause.HitPauseButton(input.MousePosition);

                if (back || pauseClicked)
                {
                    _pause.Open();
                    _timeService.PauseTurnTimer();
                    base.Update(gameTime);
                    return;
                }
            }
            else if (back)
            {
                if (sceneName == "Menu")
                {
                    RequestAppExit();
                    return;
                }

                if (sceneName == "Summary")
                {
                    _sceneManager.ChangeScene("Menu");
                    base.Update(gameTime);
                    return;
                }

                if (sceneName == "Collection" ||
                    sceneName == "GlobalStore" ||
                    sceneName == "UiGallery")
                {
                    _sceneManager.ChangeScene("Menu");
                    base.Update(gameTime);
                    return;
                }
            }

            // La escena nueva no procesa el toque que provoco el cambio.
            if (_sceneManager.ConsumeSceneChange())
            {
                base.Update(gameTime);
                return;
            }

            _sceneManager.Update(gameTime);

            base.Update(gameTime);
        }

        private void HandlePauseAction(PauseAction action)
        {
            switch (action)
            {
                case PauseAction.Resume:
                    _pause.Close();
                    _timeService.ResumeTurnTimer();
                    break;

                case PauseAction.SaveAndExit:
                    var result = _checkpoint.SaveSafeNode();
                    if (result.RunSaved)
                    {
                        _pause.Close();
                        _timeService.ResumeTurnTimer();
                        _sceneManager.ChangeScene("Menu");
                    }
                    else
                    {
                        _pause.ShowError(string.IsNullOrEmpty(result.Error)
                            ? "No se pudo guardar la run."
                            : result.Error);
                    }
                    break;

                case PauseAction.ExitToMenu:
                    // El checkpoint previo al combate ya esta en run.json.
                    _checkpoint.FlushProfileOnly();
                    _pause.Close();
                    _timeService.ResumeTurnTimer();
                    _sceneManager.ChangeScene("Menu");
                    break;

                case PauseAction.Abandon:
                    _checkpoint.DeleteRun();
                    _pause.Close();
                    _timeService.ResumeTurnTimer();
                    _sceneManager.ChangeScene("Menu");
                    break;
            }
        }

        private static bool IsRunScene(string sceneName)
        {
            return sceneName == "Draft" ||
                   sceneName == "EnemySelect" ||
                   sceneName == "Combat" ||
                   sceneName == "LevelUp" ||
                   sceneName == "Shop" ||
                   sceneName == "Event" ||
                   sceneName == "ActTransition" ||
                   sceneName == "SubBossIntro";
        }

        /// <summary>Llamado por el host Android desde OnBackPressed.</summary>
        public void RequestBack() => _backService?.Request();

        /// <summary>Llamado por el host Android desde OnPause.</summary>
        public void OnAppPaused()
        {
            _isAppPaused = true;
            _timeService?.PauseTurnTimer();

            if (_checkpoint == null || _sceneManager == null) return;

            string sceneName = _sceneManager.CurrentSceneName;
            if (IsRunScene(sceneName) || sceneName == "Summary")
                _checkpoint.SaveOnClose();
        }

        /// <summary>Llamado por el host Android desde OnResume.</summary>
        public void OnAppResumed()
        {
            _isAppPaused = false;
            if (_pause != null && _pause.IsOpen) return;
            _timeService?.ResumeTurnTimer();
        }

        protected override void Draw(GameTime gameTime)
        {
            // Con area segura el viewport queda reducido: se pinta primero toda
            // la superficie de negro para que las bandas fuera del area segura
            // (notch y barra de gestos) sean un letterbox intencional y no queden
            // sin limpiar cuando Android recrea la superficie al reanudar.
            if (SafeAreaProvider != null)
            {
                var insetViewport = GraphicsDevice.Viewport;
                var presentation = GraphicsDevice.PresentationParameters;

                GraphicsDevice.Viewport = new Viewport(
                    0, 0, presentation.BackBufferWidth, presentation.BackBufferHeight);
                GraphicsDevice.Clear(Color.Black);
                GraphicsDevice.Viewport = insetViewport;
            }

            _sceneManager.Draw(gameTime, _spriteBatch);

            if (_pause != null && IsRunScene(_sceneManager.CurrentSceneName))
            {
                _spriteBatch.Begin();

                if (_pause.IsOpen)
                {
                    _pause.Draw(_spriteBatch, string.Equals(
                        _sceneManager.CurrentSceneName, "Combat",
                        System.StringComparison.Ordinal));
                }
                else
                {
                    _pause.DrawPauseButton(_spriteBatch);
                }

                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
