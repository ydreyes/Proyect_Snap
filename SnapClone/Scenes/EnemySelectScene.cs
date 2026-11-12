using System.Collections.Generic;
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
    /// Nodo de seleccion de rival. Ofrece dos enemigos normales distintos del
    /// pool del acto actual con su identidad y su mazo fijo.
    /// </summary>
    public class EnemySelectScene : IScene
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
        private CardRegistry _cardRegistry;
        private EnemyRegistry _enemyRegistry;
        private RandomService _random;

        private readonly List<EnemyDef> _candidates = new List<EnemyDef>();
        private readonly List<List<CardDef>> _previews = new List<List<CardDef>>();
        private bool _chosen;

        public EnemySelectScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
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
            _cardRegistry = _services.Has<CardRegistry>() ? _services.Get<CardRegistry>() : null;
            _enemyRegistry = _services.Has<EnemyRegistry>() ? _services.Get<EnemyRegistry>() : null;
            _random = _services.Get<RandomService>();
            _chosen = false;

            _candidates.Clear();
            _previews.Clear();

            _candidates.AddRange(EnemySelection.RollTwo(_enemyRegistry, _random,
                _runState.ActIndex));

            foreach (var candidate in _candidates)
                _previews.Add(EnemyDeckBuilder.BuildDefs(_cardRegistry, candidate));
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


            if (_chosen || !input.IsLeftMousePressed()) return;

            var mouse = input.MousePosition;
            for (int i = 0; i < _candidates.Count; i++)
            {
                if (!IsInside(GetCandidateBounds(i), mouse)) continue;

                _chosen = true;
                _runState.DiscoverEnemy(_candidates[i].Id);
                _director.OnEnemySelected(_candidates[i]);
                _checkpoint.SavePreCombat();
                _sceneManager.ChangeScene(_director.GetNextScene());
                return;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(10, 12, 22));
            spriteBatch.Begin();

            var viewport = _graphicsDevice.Viewport;

            _fontService.DrawTextCentered(spriteBatch, "ELIGE RIVAL",
                new Rectangle(0, (int)(viewport.Height * 0.035f), viewport.Width,
                    (int)(viewport.Height * 0.06f)),
                new Color(238, 224, 188), 1.1f);

            _fontService.DrawTextCentered(spriteBatch,
                $"ACTO {_runState.ActIndex}   COMBATE {_runState.CombatIndexInAct + 1}/{RunActTable.CombatsPerAct}",
                new Rectangle(0, (int)(viewport.Height * 0.095f), viewport.Width,
                    (int)(viewport.Height * 0.04f)),
                new Color(224, 196, 128), 0.55f);

            if (_candidates.Count == 0)
            {
                _fontService.DrawTextCentered(spriteBatch, "SIN RIVALES DISPONIBLES",
                    new Rectangle(0, (int)(viewport.Height * 0.45f), viewport.Width,
                        (int)(viewport.Height * 0.08f)),
                    new Color(230, 170, 150), 0.8f);
                spriteBatch.End();
                return;
            }

            for (int i = 0; i < _candidates.Count; i++)
                DrawCandidate(spriteBatch, i);

            _fontService.DrawTextCentered(spriteBatch,
                "TOCA UN RIVAL PARA EMPEZAR EL COMBATE",
                new Rectangle(0, (int)(viewport.Height * 0.90f), viewport.Width,
                    (int)(viewport.Height * 0.05f)),
                new Color(170, 190, 215), 0.5f);

            spriteBatch.End();
        }

        private void DrawCandidate(SpriteBatch spriteBatch, int index)
        {
            var bounds = GetCandidateBounds(index);
            var candidate = _candidates[index];

            _shapeDrawer.DrawBeveledPanel(bounds, new Color(22, 34, 54),
                new Color(120, 170, 230), 3, 14);

            var header = new Rectangle(bounds.X, bounds.Y + (int)(bounds.Height * 0.02f),
                bounds.Width, (int)(bounds.Height * 0.20f));
            _fontService.DrawTextCenteredFit(spriteBatch, candidate.Name, header,
                new Color(244, 228, 186), 0.9f, 0.45f);

            var phrase = new Rectangle(bounds.X + (int)(bounds.Width * 0.05f),
                header.Bottom, bounds.Width - (int)(bounds.Width * 0.10f),
                (int)(bounds.Height * 0.16f));
            _fontService.DrawTextWrapped(spriteBatch, candidate.Phrase, phrase,
                new Color(168, 190, 218), 0.5f);

            var difficulty = new Rectangle(bounds.X, phrase.Bottom, bounds.Width,
                (int)(bounds.Height * 0.10f));
            _fontService.DrawTextCentered(spriteBatch,
                $"DIFICULTAD {candidate.Difficulty}/5", difficulty,
                new Color(224, 196, 128), 0.52f);

            DrawDeckPreview(spriteBatch, bounds, _previews[index]);
        }

        private void DrawDeckPreview(SpriteBatch spriteBatch, Rectangle panel,
            List<CardDef> deck)
        {
            var area = new Rectangle(panel.X + (int)(panel.Width * 0.04f),
                panel.Y + (int)(panel.Height * 0.50f),
                panel.Width - (int)(panel.Width * 0.08f),
                (int)(panel.Height * 0.46f));

            _shapeDrawer.DrawRectangle(area, Color.Black * 0.22f);

            int columns = 2;
            int rows = (deck.Count + columns - 1) / columns;
            if (rows <= 0) return;

            int rowHeight = area.Height / rows;
            int columnWidth = area.Width / columns;
            float scale = MathHelper.Clamp(rowHeight / 60f, 0.34f, 0.52f);

            for (int i = 0; i < deck.Count; i++)
            {
                int column = i % columns;
                int row = i / columns;
                var line = new Rectangle(area.X + column * columnWidth,
                    area.Y + row * rowHeight, columnWidth, rowHeight);

                var def = deck[i];
                _fontService.DrawTextCenteredFit(spriteBatch,
                    $"{def.Name} {def.Cost}/{def.BasePower}", line,
                    new Color(196, 214, 236), scale, 0.28f);
            }
        }

        private Rectangle GetCandidateBounds(int index)
        {
            var viewport = _graphicsDevice.Viewport;
            int height = (int)(viewport.Height * 0.34f);
            int gap = (int)(viewport.Height * 0.03f);
            int y = (int)(viewport.Height * 0.16f) + index * (height + gap);

            return new Rectangle((int)(viewport.Width * 0.06f), y,
                (int)(viewport.Width * 0.88f), height);
        }

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
