using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Gameplay.Draft;
using SnapClone.Gameplay.Run;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Borrador real: 8 rondas de 1 entre 3. Al completar las 8 cartas,
    /// el mazo pasa a la escena de combate.
    /// </summary>
    public class DraftScene : IScene
    {
        private const int CardCount = DraftSystem.ChoicesPerRound;

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;
        private GameServices _services;
        private SceneManager _sceneManager;

        private DraftSystem _draftSystem;
        private RunState _runState;
        private readonly List<CardInstanceView> _choices = new List<CardInstanceView>();

        private sealed class CardInstanceView
        {
            public CardView View;
        }

        public DraftScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameServices services, SceneManager sceneManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _services = services;
            _sceneManager = sceneManager;
        }

        public void Initialize() { }

        public void OnEnter()
        {
            _draftSystem = _services.Get<DraftSystem>();
            _runState = _services.Get<RunState>();

            _draftSystem.Reset();
            _runState.StartNewRun();
            LoadChoices();
        }

        public void LoadContent()
        {
            _shapeDrawer = new ShapeDrawer(_graphicsDevice, _spriteBatch);

            var content = _services.Has<Microsoft.Xna.Framework.Content.ContentManager>()
                ? _services.Get<Microsoft.Xna.Framework.Content.ContentManager>()
                : _services.Get<Game>().Content;

            _fontService = new FontService(content);
        }

        private void LoadChoices()
        {
            _choices.Clear();

            foreach (var card in _draftSystem.GetChoices())
            {
                _choices.Add(new CardInstanceView
                {
                    View = new CardView(card, Vector2.Zero) { Origin = CardOrigin.Hand }
                });
            }
        }

        public void Update(GameTime gameTime)
        {
            var input = _services.Get<InputService>();
            input.Update();

            if (!input.IsLeftMousePressed()) return;

            var mouse = input.MousePosition;

            if (_draftSystem.IsDraftComplete())
            {
                if (IsInside(GetContinueButton(), mouse))
                    CompleteDraft();
                return;
            }

            for (int i = 0; i < _choices.Count; i++)
            {
                if (!IsInside(GetCardBounds(i), mouse)) continue;

                if (_draftSystem.PickCard(_choices[i].View.Card))
                    LoadChoices();

                return;
            }
        }

        private void CompleteDraft()
        {
            _runState.Deck.Clear();
            _runState.Deck.AddRange(_draftSystem.GetDraftedCards());
            _sceneManager.ChangeScene("Combat");
        }

        private Rectangle GetCardBounds(int index)
        {
            var viewport = _graphicsDevice.Viewport;
            int cardWidth = (int)(viewport.Width * 0.28f);
            int cardHeight = (int)(cardWidth * 1.42f);
            int gap = (int)(viewport.Width * 0.04f);
            int totalWidth = CardCount * cardWidth + (CardCount - 1) * gap;
            int startX = (viewport.Width - totalWidth) / 2;
            int y = (int)(viewport.Height * 0.30f);

            return new Rectangle(startX + index * (cardWidth + gap), y, cardWidth, cardHeight);
        }

        private Rectangle GetContinueButton()
        {
            var viewport = _graphicsDevice.Viewport;
            return new Rectangle(
                (int)(viewport.Width * 0.20f),
                (int)(viewport.Height * 0.80f),
                (int)(viewport.Width * 0.60f),
                (int)(viewport.Height * 0.065f));
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(10, 18, 16));
            spriteBatch.Begin();

            var viewport = _graphicsDevice.Viewport;

            var titleRect = new Rectangle(0, (int)(viewport.Height * 0.07f), viewport.Width, (int)(viewport.Height * 0.06f));
            _fontService.DrawTextCentered(spriteBatch, "DRAFT", titleRect, Color.White, 1.0f);

            var progressRect = new Rectangle(0, (int)(viewport.Height * 0.13f), viewport.Width, (int)(viewport.Height * 0.05f));
            _fontService.DrawTextCentered(spriteBatch,
                $"{_draftSystem.PicksMade} / {DraftSystem.TotalPicks}",
                progressRect, new Color(224, 196, 128), 0.8f);

            DrawProgressPips(viewport);

            if (_draftSystem.IsDraftComplete())
            {
                DrawCompletedState(spriteBatch, viewport);
            }
            else
            {
                var hintRect = new Rectangle(0, (int)(viewport.Height * 0.22f), viewport.Width, (int)(viewport.Height * 0.04f));
                _fontService.DrawTextCentered(spriteBatch, "ELIGE 1 DE 3 CARTAS",
                    hintRect, new Color(170, 190, 215), 0.65f);

                for (int i = 0; i < _choices.Count; i++)
                {
                    var bounds = GetCardBounds(i);
                    _choices[i].View.SnapTo(bounds);
                    _choices[i].View.DrawInHand(spriteBatch, _shapeDrawer, _fontService, bounds);
                }
            }

            DrawContinueButton(spriteBatch);

            spriteBatch.End();
        }

        private void DrawProgressPips(Viewport viewport)
        {
            int total = DraftSystem.TotalPicks;
            int radius = System.Math.Max(4, viewport.Width / 90);
            int gap = radius * 3;
            int totalWidth = total * gap;
            int startX = (viewport.Width - totalWidth) / 2 + gap / 2;
            int y = (int)(viewport.Height * 0.195f);

            for (int i = 0; i < total; i++)
            {
                bool filled = i < _draftSystem.PicksMade;
                var center = new Vector2(startX + i * gap, y);
                _shapeDrawer.DrawCircleFramed(center, radius,
                    filled ? new Color(196, 148, 62) : new Color(40, 46, 60),
                    filled ? new Color(255, 220, 150) : new Color(90, 100, 120), 1.5f);
            }
        }

        private void DrawCompletedState(SpriteBatch spriteBatch, Viewport viewport)
        {
            var completeRect = new Rectangle(0, (int)(viewport.Height * 0.34f), viewport.Width, (int)(viewport.Height * 0.10f));
            _fontService.DrawTextCentered(spriteBatch, "MAZO COMPLETO (8)",
                completeRect, new Color(126, 214, 170), 0.9f);

            var instructionRect = new Rectangle(0, (int)(viewport.Height * 0.44f), viewport.Width, (int)(viewport.Height * 0.06f));
            _fontService.DrawTextCentered(spriteBatch, "CONTINUA AL COMBATE",
                instructionRect, new Color(190, 206, 226), 0.65f);
        }

        private void DrawContinueButton(SpriteBatch spriteBatch)
        {
            var button = GetContinueButton();
            bool enabled = _draftSystem.IsDraftComplete();

            var fill = enabled ? new Color(30, 104, 74) : new Color(38, 52, 46);
            var border = enabled ? new Color(126, 214, 170) : new Color(96, 116, 106);

            _shapeDrawer.DrawBeveledPanel(button, fill, border, 3, 12);
            _fontService.DrawTextCentered(spriteBatch, "CONTINUAR",
                button, enabled ? Color.White : new Color(190, 200, 196), 0.8f);
        }

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
