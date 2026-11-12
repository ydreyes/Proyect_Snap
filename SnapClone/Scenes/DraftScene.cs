using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Draft;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Animation;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Borrador: tira de ocho slots sobre tres ofertas de arte. Al completar
    /// las ocho cartas el mazo pasa al combate. La logica de pick no cambia.
    /// </summary>
    public class DraftScene : UiScene
    {
        private const int CardCount = DraftSystem.ChoicesPerRound;

        private DraftSystem _draftSystem;
        private RunState _runState;
        private RunDirector _runDirector;
        private RunCheckpointService _checkpoint;
        private readonly List<CardInstanceView> _choices =
            new List<CardInstanceView>();

        private CardInstance _flyCard;
        private Rectangle _flyFrom;
        private Rectangle _flyTo;
        private bool _flyActive;

        private sealed class CardInstanceView
        {
            public CardInstance Card;
        }

        public DraftScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title => "DRAFT";
        protected override string Subtitle => "ELIGE 1 DE 3 CARTAS";
        protected override Color Accent => UiTheme.Gold;
        protected override string CurrencyIcon => "P";
        protected override string CurrencyValue =>
            $"{_draftSystem?.PicksMade ?? 0}/{DraftSystem.TotalPicks}";
        protected override string CurrencyLabel => "PICKS";
        protected override int BackgroundSeed => 2202;

        protected override void OnEnterScene()
        {
            _draftSystem = Services.Get<DraftSystem>();
            _runState = Services.Get<RunState>();
            _runDirector = Services.Get<RunDirector>();
            _checkpoint = Services.Get<RunCheckpointService>();

            _flyActive = false;
            _flyCard = null;

            // El llamador prepara la run o restaura el checkpoint. Aqui solo se
            // asegura que la ronda actual tenga ofertas, sin rebarajar.
            _draftSystem.EnsureOffers();
            LoadChoices();
            _checkpoint.SaveDraftCheckpoint();
            AnimateOffers();
        }

        private void LoadChoices()
        {
            _choices.Clear();

            foreach (var card in _draftSystem.GetChoices())
            {
                _runState.DiscoverCard(card.Definition?.Id);
                _choices.Add(new CardInstanceView { Card = card });
            }
        }

        private void AnimateOffers()
        {
            for (int i = 0; i < CardCount; i++)
            {
                Animator.Remove($"offer{i}");
                Animator.AddEntrance($"offer{i}", i, 0.06f, 0.30f);
            }
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle StripBounds
        {
            get
            {
                var inner = ContentInner;
                int height = UiTheme.Px(34, Scaffold.UiScale);
                return new Rectangle(inner.X, inner.Y, inner.Width, height);
            }
        }

        private Rectangle ThemeBounds
        {
            get
            {
                var inner = ContentInner;
                int y = StripBounds.Bottom + UiTheme.Px(UiTheme.SpaceLg,
                    Scaffold.UiScale);
                return new Rectangle(inner.X, y, inner.Width,
                    UiTheme.Px(40, Scaffold.UiScale));
            }
        }

        private Rectangle CardsArea
        {
            get
            {
                var inner = ContentInner;
                int top = ThemeBounds.Bottom + UiTheme.Px(UiTheme.SpaceMd,
                    Scaffold.UiScale);
                int bottom = inner.Bottom - UiTheme.Px(72, Scaffold.UiScale);
                return new Rectangle(inner.X, top, inner.Width,
                    System.Math.Max(0, bottom - top));
            }
        }

        private Rectangle ContinueBounds
        {
            get
            {
                var inner = ContentInner;
                int height = UiTheme.Px(56, Scaffold.UiScale);
                return new Rectangle(inner.X, inner.Bottom - height,
                    inner.Width, height);
            }
        }

        private Rectangle GetCardBounds(int index)
        {
            var area = CardsArea;
            int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
            int width = (area.Width - gap * (CardCount - 1)) / CardCount;
            int height = (int)(width * 1.42f);
            if (height > area.Height) height = area.Height;
            int y = area.Y + System.Math.Max(0, (area.Height - height) / 2);
            return new Rectangle(area.X + index * (width + gap), y, width,
                height);
        }

        private Rectangle GetStripSlotBounds(int index)
        {
            var strip = StripBounds;
            int gap = UiTheme.Px(UiTheme.SpaceXs, Scaffold.UiScale);
            int width = (strip.Width - gap * (DraftSystem.TotalPicks - 1)) /
                        DraftSystem.TotalPicks;
            return new Rectangle(strip.X + index * (width + gap), strip.Y,
                width, strip.Height);
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (_draftSystem.IsDraftComplete())
            {
                if (Tap("continue", ContinueBounds))
                    CompleteDraft();
                return;
            }

            Tap("continue", ContinueBounds, false);

            for (int i = 0; i < _choices.Count; i++)
            {
                if (!Tap($"card{i}", GetCardBounds(i))) continue;

                var picked = _choices[i].Card;
                int slot = _draftSystem.PicksMade;
                Rectangle from = GetCardBounds(i);

                if (_draftSystem.PickCard(picked))
                {
                    _checkpoint.SaveDraftCheckpoint();

                    _flyCard = picked;
                    _flyFrom = from;
                    _flyTo = GetStripSlotBounds(
                        System.Math.Min(slot, DraftSystem.TotalPicks - 1));
                    _flyActive = true;

                    Animator.Remove("fly");
                    Animator.Add(new Tween(0f, 1f, 0.30f,
                        EasingKind.EaseOutCubic)
                    {
                        OnComplete = () => _flyActive = false
                    });

                    LoadChoices();
                    AnimateOffers();
                }

                return;
            }
        }

        private void CompleteDraft()
        {
            _runState.Deck.Clear();
            _runState.Deck.AddRange(_draftSystem.GetDraftedCards());
            _draftSystem.Reset();
            _runDirector.OnDraftComplete();
            _checkpoint.SaveSafeNode();
            Scenes.ChangeScene(_runDirector.GetNextScene());
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            DrawProgressStrip(spriteBatch, uiScale);

            if (_draftSystem.IsDraftComplete())
            {
                DrawCompleted(spriteBatch, uiScale);
            }
            else
            {
                DrawThemeRibbon(spriteBatch, uiScale);

                for (int i = 0; i < _choices.Count; i++)
                {
                    var bounds = GetCardBounds(i);
                    float t = Animator.Value($"offer{i}", 1f);
                    var animated = AnimateIn(bounds, t, uiScale);

                    DrawCardInstance(animated, _choices[i].Card,
                        new CardRenderOptions
                        {
                            Variant = CardFrameVariant.Draft,
                            Opacity = MathHelper.Clamp(t, 0f, 1f),
                            ShowName = true
                        });
                }
            }

            if (_flyActive && _flyCard != null)
                DrawFlyGhost(spriteBatch, uiScale);

            DrawTapButtonScaled("continue", "CONTINUAR", UiTheme.Success,
                UiTheme.Label);
        }

        private void DrawProgressStrip(SpriteBatch spriteBatch, float uiScale)
        {
            var strip = StripBounds;
            Ui.Chrome.DrawProgressSlots(spriteBatch, strip,
                DraftSystem.TotalPicks, _draftSystem.PicksMade,
                _draftSystem.PicksMade, UiTheme.Gold, uiScale);

            string label = _draftSystem.IsDraftComplete()
                ? "MAZO COMPLETO 8/8"
                : $"PICK {_draftSystem.PicksMade + 1} DE {DraftSystem.TotalPicks}";
            DrawSectionLabel(label,
                new Rectangle(strip.X, strip.Bottom + UiTheme.Px(2, uiScale),
                    strip.Width, UiTheme.Px(18, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);
        }

        private void DrawThemeRibbon(SpriteBatch spriteBatch, float uiScale)
        {
            var theme = _draftSystem.CurrentTheme;
            string label = theme.HasValue
                ? DraftThemeText.RoundLabel(theme.Value)
                : "ALEATORIO";
            Color accent = ThemeAccent(theme);

            DrawRibbon(ThemeBounds, label, accent);
        }

        private void DrawCompleted(SpriteBatch spriteBatch, float uiScale)
        {
            var area = CardsArea;
            Ui.Chrome.DrawGlow(spriteBatch,
                new Vector2(area.Center.X, area.Center.Y),
                area.Width * 0.34f, UiTheme.Success * 0.28f);

            DrawDisplayCentered("MAZO COMPLETO",
                new Rectangle(area.X, area.Y + area.Height / 2 -
                    UiTheme.Px(36, uiScale), area.Width,
                    UiTheme.Px(56, uiScale)),
                UiTheme.Success, UiTheme.Display);

            DrawTextCenteredFit("CONTINUA AL COMBATE",
                new Rectangle(area.X, area.Y + area.Height / 2 +
                    UiTheme.Px(24, uiScale), area.Width,
                    UiTheme.Px(24, uiScale)),
                UiTheme.TextSecondary, UiTheme.LabelSmall);
        }

        private void DrawFlyGhost(SpriteBatch spriteBatch, float uiScale)
        {
            float t = Animator.Value("fly", 1f);
            t = MathHelper.Clamp(t, 0f, 1f);

            int x = (int)MathHelper.Lerp(_flyFrom.X, _flyTo.X, t);
            int y = (int)MathHelper.Lerp(_flyFrom.Y, _flyTo.Y, t);
            int width = (int)MathHelper.Lerp(_flyFrom.Width, _flyTo.Width, t);
            int height = (int)MathHelper.Lerp(_flyFrom.Height, _flyTo.Height, t);

            DrawCardInstance(new Rectangle(x, y, System.Math.Max(6, width),
                System.Math.Max(6, height)), _flyCard,
                new CardRenderOptions
                {
                    Variant = CardFrameVariant.MiniDeck,
                    Opacity = 1f - t * 0.55f,
                    ShowName = false
                });
        }

        private static Rectangle AnimateIn(Rectangle bounds, float t,
            float uiScale)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scale = MathHelper.Lerp(0.92f, 1f, t);
            int y = bounds.Y + (int)((1f - t) * UiTheme.Px(18, uiScale));
            int width = (int)(bounds.Width * scale);
            int height = (int)(bounds.Height * scale);
            return new Rectangle(bounds.X + (bounds.Width - width) / 2, y,
                System.Math.Max(6, width), System.Math.Max(6, height));
        }

        private static Color ThemeAccent(DraftTheme? theme)
        {
            if (!theme.HasValue) return UiTheme.AccentSoft;

            switch (theme.Value)
            {
                case DraftTheme.Control: return new Color(96, 150, 230);
                case DraftTheme.Destroy: return new Color(220, 110, 100);
                case DraftTheme.OnReveal: return new Color(176, 140, 240);
                default: return new Color(120, 200, 170);
            }
        }
    }
}
