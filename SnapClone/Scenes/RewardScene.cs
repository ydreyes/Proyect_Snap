using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Run;
using SnapClone.UI.Components;
using SnapClone.UI.Primitives;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Pantalla de recompensa real. En victoria o derrota ofrece 3 cartas con
    /// perk; en el nodo de perk ofrece 3 cartas del mazo emparejadas con un
    /// perk. Con el mazo lleno pide quitar una carta antes de anadir la nueva.
    /// </summary>
    public class RewardScene : IScene
    {
        private enum RewardPhase
        {
            PickOffer,
            ReplaceCard
        }

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;
        private GameServices _services;
        private SceneManager _sceneManager;

        private RunState _runState;
        private RewardSystem _rewardSystem;
        private ProgressionManager _progressionManager;
        private PerkRegistry _perkRegistry;
        private PowerResolver _powerResolver;

        private readonly List<(CardInstance card, string perkId)> _offers =
            new List<(CardInstance card, string perkId)>();

        private RewardPhase _phase;
        private int _chosenOffer = -1;
        private bool _finished;
        private string _header = string.Empty;

        public RewardScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameServices services, SceneManager sceneManager)
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
            _rewardSystem = _services.Get<RewardSystem>();
            _progressionManager = _services.Get<ProgressionManager>();
            _perkRegistry = _services.Has<PerkRegistry>() ? _services.Get<PerkRegistry>() : null;
            _powerResolver = new PowerResolver(_perkRegistry);

            _offers.Clear();
            _phase = RewardPhase.PickOffer;
            _chosenOffer = -1;
            _finished = false;

            if (_runState.PendingReward == PendingRewardKind.Perk)
            {
                _header = "RECOMPENSA DE PERK - ELIGE UNA CARTA";
                _offers.AddRange(_rewardSystem.GetPerkRewards(_runState));

                foreach (var offer in _offers)
                    _powerResolver.ResolveCard(offer.card);
            }
            else if (_runState.PendingReward == PendingRewardKind.Card)
            {
                _header = "RECOMPENSA - ELIGE UNA CARTA";
                _offers.AddRange(_rewardSystem.GetCardRewards(_runState));

                // La carta de catalogo ya trae su perk: se resuelve para
                // mostrar el poder real de la oferta.
                foreach (var offer in _offers)
                    _powerResolver.ResolveCard(offer.card);
            }
            else
            {
                _header = "SIN RECOMPENSA PENDIENTE";
            }
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
            input.Update();

            if (_finished) return;
            if (!input.IsLeftMousePressed()) return;

            var mouse = input.MousePosition;

            if (IsInside(GetSkipButton(), mouse))
            {
                Finish();
                return;
            }

            if (_phase == RewardPhase.PickOffer)
            {
                for (int i = 0; i < _offers.Count; i++)
                {
                    if (!IsInside(GetOfferBounds(i), mouse)) continue;

                    PickOffer(i);
                    return;
                }

                return;
            }

            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                if (!IsInside(GetDeckCardBounds(i), mouse)) continue;

                ReplaceDeckCard(i);
                return;
            }
        }

        private void PickOffer(int index)
        {
            var offer = _offers[index];

            if (_runState.PendingReward == PendingRewardKind.Perk)
            {
                if (offer.perkId != null && !offer.card.AttachedPerks.Contains(offer.perkId))
                    offer.card.AttachedPerks.Add(offer.perkId);

                _powerResolver.ResolveCard(offer.card);
                Finish();
                return;
            }

            if (_runState.IsDeckAtCap)
            {
                _chosenOffer = index;
                _phase = RewardPhase.ReplaceCard;
                return;
            }

            AddChosenCard(index);
            Finish();
        }

        private void AddChosenCard(int index)
        {
            if (index < 0 || index >= _offers.Count) return;
            if (_runState.Deck.Count >= RunState.MaxDeckSize) return;

            _runState.Deck.Add(_offers[index].card);
        }

        private void ReplaceDeckCard(int deckIndex)
        {
            if (deckIndex < 0 || deckIndex >= _runState.Deck.Count) return;
            if (_chosenOffer < 0 || _chosenOffer >= _offers.Count) return;

            _runState.Deck.RemoveAt(deckIndex);
            AddChosenCard(_chosenOffer);
            Finish();
        }

        private void Finish()
        {
            if (_finished) return;

            _finished = true;
            _sceneManager.ChangeScene(_progressionManager.CompleteReward());
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle GetOfferBounds(int index)
        {
            var viewport = _graphicsDevice.Viewport;
            int cardWidth = (int)(viewport.Width * 0.24f);
            int cardHeight = (int)(cardWidth * 1.42f);
            int gap = (int)(viewport.Width * 0.05f);
            int totalWidth = 3 * cardWidth + 2 * gap;
            int startX = (viewport.Width - totalWidth) / 2;
            int y = (int)(viewport.Height * 0.28f);

            return new Rectangle(startX + index * (cardWidth + gap), y, cardWidth, cardHeight);
        }

        private Rectangle GetSkipButton()
        {
            var viewport = _graphicsDevice.Viewport;
            return new Rectangle(
                (int)(viewport.Width * 0.30f),
                (int)(viewport.Height * 0.87f),
                (int)(viewport.Width * 0.40f),
                (int)(viewport.Height * 0.055f));
        }

        private Rectangle GetDeckCardBounds(int index)
        {
            var viewport = _graphicsDevice.Viewport;
            const int columns = 4;

            int cardWidth = (int)(viewport.Width * 0.21f);
            int cardHeight = (int)(cardWidth * 1.42f);
            int gap = (int)(viewport.Width * 0.015f);
            int totalWidth = columns * cardWidth + (columns - 1) * gap;
            int startX = (viewport.Width - totalWidth) / 2;
            int startY = (int)(viewport.Height * 0.24f);

            int row = index / columns;
            int column = index % columns;

            return new Rectangle(
                startX + column * (cardWidth + gap),
                startY + row * (cardHeight + gap),
                cardWidth,
                cardHeight);
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _graphicsDevice.Clear(new Color(16, 14, 10));
            spriteBatch.Begin();

            var viewport = _graphicsDevice.Viewport;

            _fontService.DrawTextCentered(spriteBatch, _header,
                new Rectangle(0, (int)(viewport.Height * 0.08f), viewport.Width, (int)(viewport.Height * 0.06f)),
                new Color(240, 224, 180), 0.9f);

            if (_offers.Count == 0)
            {
                _fontService.DrawTextCentered(spriteBatch, "CONTINUAR",
                    new Rectangle(0, (int)(viewport.Height * 0.40f), viewport.Width, (int)(viewport.Height * 0.06f)),
                    new Color(210, 200, 170), 0.8f);
                DrawSkipButton(spriteBatch);
                spriteBatch.End();
                return;
            }

            if (_phase == RewardPhase.ReplaceCard)
            {
                DrawReplacementState(spriteBatch, viewport);
            }
            else
            {
                DrawOffers(spriteBatch);
            }

            DrawSkipButton(spriteBatch);
            spriteBatch.End();
        }

        private void DrawOffers(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _offers.Count; i++)
            {
                var bounds = GetOfferBounds(i);
                var offer = _offers[i];

                var view = new CardView(offer.card, Vector2.Zero);
                view.SnapTo(bounds);
                view.DrawInHand(spriteBatch, _shapeDrawer, _fontService, bounds);

                var perkRect = new Rectangle(
                    bounds.X,
                    bounds.Bottom + 4,
                    bounds.Width,
                    (int)(bounds.Height * 0.10f));

                _shapeDrawer.DrawBeveledPanel(perkRect, new Color(28, 34, 26), new Color(150, 200, 130), 2, 6);
                _fontService.DrawTextCenteredFit(spriteBatch, PerkLabel(offer.perkId),
                    perkRect, new Color(210, 236, 190), 0.42f, 0.24f);
            }
        }

        private void DrawReplacementState(SpriteBatch spriteBatch, Viewport viewport)
        {
            _fontService.DrawTextCentered(spriteBatch, "ELIGE UNA CARTA PARA QUITAR",
                new Rectangle(0, (int)(viewport.Height * 0.16f), viewport.Width, (int)(viewport.Height * 0.05f)),
                new Color(224, 196, 128), 0.7f);

            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                var bounds = GetDeckCardBounds(i);
                var view = new CardView(_runState.Deck[i], Vector2.Zero);
                view.SnapTo(bounds);
                view.DrawInHand(spriteBatch, _shapeDrawer, _fontService, bounds);
            }
        }

        private void DrawSkipButton(SpriteBatch spriteBatch)
        {
            var button = GetSkipButton();
            _shapeDrawer.DrawBeveledPanel(button, new Color(54, 46, 30), new Color(200, 168, 96), 3, 10);

            string label = _phase == RewardPhase.ReplaceCard || _offers.Count == 0
                ? "SALTAR"
                : "SALTAR RECOMPENSA";

            _fontService.DrawTextCentered(spriteBatch, label, button, Color.White, 0.7f);
        }

        private string PerkLabel(string perkId)
        {
            if (perkId == null) return "SIN PERK";
            var perk = _perkRegistry?.Get(perkId);
            return perk != null ? $"PERK {perk.Name}" : $"PERK {perkId}";
        }

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
