using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Rendering;
using SnapClone.UI.Theme;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Nodo seguro de subida de nivel. Una sola plantilla de ceremonia se
    /// adapta a las siete fases; la maquina de estados y las ofertas se
    /// conservan sin cambios.
    /// </summary>
    public class LevelUpScene : UiScene
    {
        private enum Phase
        {
            ChooseBranch,
            PickCard,
            PickUpgradeCard,
            PickUpgradeType,
            PickPerk,
            PickPerkTarget,
            Unavailable
        }

        private RunState _runState;
        private LevelUpSystem _levelUpSystem;
        private ProgressionManager _progressionManager;
        private RunCheckpointService _checkpoint;
        private PerkRegistry _perkRegistry;

        private LevelUpState _levelUp;
        private Phase _phase;
        private CardInstance _selectedCard;
        private string _selectedPerk;
        private bool _finished;

        public LevelUpScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch,
            GameServices services, SceneManager sceneManager)
            : base(graphicsDevice, spriteBatch, services, sceneManager)
        {
        }

        protected override string Title => "SUBE DE NIVEL";
        protected override string Subtitle => "CEREMONIA DE NIVEL";
        protected override Color Accent => UiTheme.Violet;
        protected override int BackgroundSeed => 5505;
        protected override string CurrencyIcon => "N";
        protected override string CurrencyValue =>
            _levelUp != null ? _levelUp.Level.ToString() : "-";
        protected override string CurrencyLabel => "NIVEL";

        protected override void OnEnterScene()
        {
            _runState = Services.Get<RunState>();
            _levelUpSystem = Services.Get<LevelUpSystem>();
            _progressionManager = Services.Get<ProgressionManager>();
            _checkpoint = Services.Get<RunCheckpointService>();
            _perkRegistry = Services.Has<PerkRegistry>()
                ? Services.Get<PerkRegistry>()
                : null;

            _selectedCard = null;
            _selectedPerk = null;
            _finished = false;
            _levelUp = _runState.PendingLevelUps.Count > 0
                ? _runState.PendingLevelUps[0]
                : null;

            if (_levelUp == null)
            {
                _phase = Phase.Unavailable;
                return;
            }

            _levelUpSystem.EnsureOffers(_runState, _levelUp);
            _levelUpSystem.RefreshDeck(_runState);

            bool hasCard = _levelUpSystem.CanUseCardBranch(_runState, _levelUp);
            bool hasPerk = _levelUpSystem.CanUsePerkBranch(_runState, _levelUp);
            _phase = hasCard || hasPerk ? Phase.ChooseBranch : Phase.Unavailable;
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private Rectangle CeremonyBounds
        {
            get
            {
                var inner = ContentInner;
                return new Rectangle(inner.X, inner.Y, inner.Width,
                    UiTheme.Px(104, Scaffold.UiScale));
            }
        }

        private Rectangle PhaseArea
        {
            get
            {
                var inner = ContentInner;
                int gap = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
                int y = CeremonyBounds.Bottom + gap;
                return new Rectangle(inner.X, y, inner.Width,
                    System.Math.Max(0, inner.Bottom - y));
            }
        }

        private Rectangle GetBranchButton(bool card)
        {
            var area = PhaseArea;
            int gap = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
            int width = (area.Width - gap) / 2;
            int height = Math.Min((int)(area.Height * 0.34f),
                UiTheme.Px(236, Scaffold.UiScale));
            int y = area.Y + UiTheme.Px(38, Scaffold.UiScale);
            return new Rectangle(card ? area.X : area.X + width + gap, y,
                width, height);
        }

        private Rectangle BranchDeckArea
        {
            get
            {
                var area = PhaseArea;
                var branch = GetBranchButton(true);
                int y = branch.Bottom + UiTheme.Px(UiTheme.SpaceLg,
                    Scaffold.UiScale);
                int height = Math.Min(UiTheme.Px(120, Scaffold.UiScale),
                    Math.Max(0, area.Bottom - y));
                return new Rectangle(area.X, y, area.Width, height);
            }
        }

        private Rectangle GetUnavailableButton()
        {
            var area = PhaseArea;
            int width = (int)(area.Width * 0.64f);
            int height = UiTheme.Px(54, Scaffold.UiScale);
            return new Rectangle(area.X + (area.Width - width) / 2,
                area.Y + (int)(area.Height * 0.52f), width, height);
        }

        private Rectangle GetCardOfferBounds(int index)
        {
            var area = PhaseArea;
            int gap = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
            int width = (area.Width - gap * 2) / 3;
            int height = System.Math.Min((int)(width * 1.42f),
                (int)(area.Height * 0.82f));
            int y = area.Y + UiTheme.Px(UiTheme.SpaceXl, Scaffold.UiScale);
            return new Rectangle(area.X + index * (width + gap), y, width,
                height);
        }

        private Rectangle GetDeckCardBounds(int index)
        {
            var area = PhaseArea;
            const int columns = 4;
            int gap = UiTheme.Px(UiTheme.SpaceSm, Scaffold.UiScale);
            int width = (area.Width - gap * (columns - 1)) / columns;
            int top = area.Y + UiTheme.Px(34, Scaffold.UiScale);
            int maxHeight = (area.Height - UiTheme.Px(34, Scaffold.UiScale)) /
                            3;
            int height = System.Math.Min((int)(width * 1.38f), maxHeight);
            int row = index / columns;
            int column = index % columns;
            return new Rectangle(area.X + column * (width + gap),
                top + row * (height + gap), width, height);
        }

        private Rectangle GetUpgradeButton(bool cost)
        {
            var area = PhaseArea;
            int gap = UiTheme.Px(UiTheme.SpaceLg, Scaffold.UiScale);
            int width = (area.Width - gap) / 2;
            int height = (int)(area.Height * 0.26f);
            return new Rectangle(cost ? area.X : area.X + width + gap,
                area.Y + (int)(area.Height * 0.56f), width, height);
        }

        private Rectangle GetPerkBounds(int index)
        {
            var area = PhaseArea;
            int gap = UiTheme.Px(UiTheme.SpaceMd, Scaffold.UiScale);
            int height = UiTheme.Px(96, Scaffold.UiScale);
            int top = area.Y + UiTheme.Px(UiTheme.SpaceXl, Scaffold.UiScale);
            return new Rectangle(area.X, top + index * (height + gap),
                area.Width, height);
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        protected override void UpdateScene(GameTime gameTime)
        {
            if (_finished) return;

            if (_levelUp == null)
            {
                Finish();
                return;
            }

            switch (_phase)
            {
                case Phase.ChooseBranch:
                    UpdateBranchChoice();
                    break;
                case Phase.PickCard:
                    UpdateCardPick();
                    break;
                case Phase.PickUpgradeCard:
                    UpdateUpgradeCardPick();
                    break;
                case Phase.PickUpgradeType:
                    UpdateUpgradeType();
                    break;
                case Phase.PickPerk:
                    UpdatePerkPick();
                    break;
                case Phase.PickPerkTarget:
                    UpdatePerkTarget();
                    break;
                case Phase.Unavailable:
                    if (Tap("continue", GetUnavailableButton()))
                        Finish();
                    break;
            }
        }

        private void UpdateBranchChoice()
        {
            bool cardEnabled = _levelUpSystem.CanUseCardBranch(_runState, _levelUp);
            bool perkEnabled = _levelUpSystem.CanUsePerkBranch(_runState, _levelUp);

            if (Tap("branch_card", GetBranchButton(true), cardEnabled) &&
                cardEnabled)
            {
                _levelUp.Branch = LevelUpBranch.Card;
                _phase = _runState.IsDeckAtCap
                    ? Phase.PickUpgradeCard
                    : Phase.PickCard;
                return;
            }

            if (Tap("branch_perk", GetBranchButton(false), perkEnabled) &&
                perkEnabled)
            {
                _levelUp.Branch = LevelUpBranch.Perk;
                _phase = Phase.PickPerk;
                return;
            }

            if (!cardEnabled && !perkEnabled &&
                Tap("continue", GetUnavailableButton()))
                Finish();
        }

        private void UpdateCardPick()
        {
            for (int i = 0; i < _levelUp.CardOffers.Count; i++)
            {
                if (!Tap($"card_offer{i}", GetCardOfferBounds(i))) continue;

                if (_levelUpSystem.AddCard(_runState, _levelUp,
                    _levelUp.CardOffers[i]))
                    Finish();
                return;
            }
        }

        private void UpdateUpgradeCardPick()
        {
            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                var card = _runState.Deck[i];
                if (!Tap($"deck{i}", GetDeckCardBounds(i), card.CanUpgrade))
                    continue;

                if (!card.CanUpgrade) return;

                _selectedCard = card;
                _phase = Phase.PickUpgradeType;
                return;
            }
        }

        private void UpdateUpgradeType()
        {
            if (_selectedCard == null) return;

            if (Tap("upgrade_cost", GetUpgradeButton(true),
                _selectedCard.CanUpgradeCost))
            {
                if (_levelUpSystem.ApplyUpgrade(_runState, _levelUp,
                    _selectedCard, UpgradeType.Cost))
                    Finish();
                return;
            }

            if (Tap("upgrade_power", GetUpgradeButton(false),
                _selectedCard.CanUpgrade))
            {
                if (_levelUpSystem.ApplyUpgrade(_runState, _levelUp,
                    _selectedCard, UpgradeType.Power))
                    Finish();
            }
        }

        private void UpdatePerkPick()
        {
            for (int i = 0; i < _levelUp.PerkOffers.Count; i++)
            {
                if (!Tap($"perk_offer{i}", GetPerkBounds(i))) continue;

                _selectedPerk = _levelUp.PerkOffers[i];
                _phase = Phase.PickPerkTarget;
                return;
            }
        }

        private void UpdatePerkTarget()
        {
            if (_selectedPerk == null) return;

            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                var card = _runState.Deck[i];
                bool compatible = _levelUpSystem.CanAttach(_selectedPerk, card);
                if (!Tap($"deck{i}", GetDeckCardBounds(i), compatible)) continue;

                if (_levelUpSystem.ApplyPerk(_runState, _levelUp,
                    _selectedPerk, card))
                    Finish();
                return;
            }
        }

        private void Finish()
        {
            if (_finished) return;

            _finished = true;
            _levelUpSystem.ResolveHead(_runState);
            string nextScene = _progressionManager.CompleteLevelUp();
            _checkpoint.SaveSafeNode();
            Scenes.ChangeScene(nextScene);
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        protected override void DrawScene(SpriteBatch spriteBatch, Rectangle content,
            float uiScale)
        {
            DrawCeremony(spriteBatch, uiScale);

            if (_levelUp == null)
            {
                DrawTextCenteredFit("SIN NIVEL PENDIENTE",
                    new Rectangle(content.X,
                        content.Y + content.Height / 2,
                        content.Width, UiTheme.Px(34, uiScale)),
                    UiTheme.TextMuted, UiTheme.Heading);
                return;
            }

            switch (_phase)
            {
                case Phase.ChooseBranch:
                    DrawBranchChoice(spriteBatch, uiScale);
                    break;
                case Phase.PickCard:
                    DrawCardOffers(spriteBatch, uiScale);
                    break;
                case Phase.PickUpgradeCard:
                    DrawDeckGrid(spriteBatch, uiScale,
                        "ELIGE UNA CARTA PARA MEJORAR");
                    break;
                case Phase.PickUpgradeType:
                    DrawUpgradeType(spriteBatch, uiScale);
                    break;
                case Phase.PickPerk:
                    DrawPerkOffers(spriteBatch, uiScale);
                    break;
                case Phase.PickPerkTarget:
                    DrawDeckGrid(spriteBatch, uiScale,
                        $"ELIGE LA CARTA QUE RECIBE {PerkName(_selectedPerk)}");
                    break;
                case Phase.Unavailable:
                    DrawUnavailable(spriteBatch, uiScale);
                    break;
            }
        }

        private void DrawCeremony(SpriteBatch spriteBatch, float uiScale)
        {
            var bounds = CeremonyBounds;
            var center = new Vector2(bounds.Center.X, bounds.Center.Y);

            Ui.Chrome.DrawGlow(spriteBatch, center, bounds.Width * 0.30f,
                UiTheme.Violet * 0.28f);

            DrawDisplayCentered("UPGRADE UNLOCKED",
                new Rectangle(bounds.X, bounds.Y, bounds.Width,
                    UiTheme.Px(52, uiScale)),
                UiTheme.Violet, UiTheme.Display);

            if (_levelUp != null)
            {
                DrawTextCenteredFit($"NIVEL {_levelUp.Level}   " +
                    $"MAZO {_runState.Deck.Count}/{RunState.MaxDeckSize}",
                    new Rectangle(bounds.X, bounds.Bottom -
                        UiTheme.Px(26, uiScale), bounds.Width,
                        UiTheme.Px(22, uiScale)),
                    UiTheme.TextSecondary, UiTheme.Caption);
            }
        }

        private void DrawBranchChoice(SpriteBatch spriteBatch, float uiScale)
        {
            DrawSectionLabel("ELIGE UNA RAMA",
                new Rectangle(PhaseArea.X, PhaseArea.Y, PhaseArea.Width,
                    UiTheme.Px(24, uiScale)),
                UiTheme.TextSecondary, UiTheme.LabelSmall);

            bool cardEnabled = _levelUpSystem.CanUseCardBranch(_runState, _levelUp);
            bool perkEnabled = _levelUpSystem.CanUsePerkBranch(_runState, _levelUp);

            DrawBranchTile("branch_card", GetBranchButton(true), "CARTA",
                _runState.IsDeckAtCap
                    ? "EL MAZO ESTA LLENO: MEJORA UNA CARTA"
                    : $"ANADE 1 CARTA AL MAZO ({_runState.Deck.Count}/" +
                      $"{RunState.MaxDeckSize})",
                cardEnabled, UiTheme.Accent);
            DrawBranchTile("branch_perk", GetBranchButton(false), "PERK",
                "ADJUNTA UNA HABILIDAD A UNA CARTA COMPATIBLE DEL MAZO",
                perkEnabled, UiTheme.Perk);

            DrawBranchDeck(spriteBatch, uiScale);
            DrawBranchHelp(spriteBatch, uiScale);

            if (!cardEnabled && !perkEnabled)
                DrawTapButtonScaled("continue", "CONTINUAR", UiTheme.Gold);
        }

        private void DrawBranchHelp(SpriteBatch spriteBatch, float uiScale)
        {
            var area = PhaseArea;
            var deck = BranchDeckArea;
            int height = UiTheme.Px(140, uiScale);
            var bounds = new Rectangle(area.X, area.Bottom - height,
                area.Width, height);

            if (bounds.Y < deck.Bottom + UiTheme.Px(UiTheme.SpaceMd, uiScale))
                return;

            DrawPanel(bounds, PanelTone.Sunken);
            int pad = UiTheme.Px(UiTheme.SpaceLg, uiScale);

            DrawSectionHeader("COMO FUNCIONA",
                new Rectangle(bounds.X + pad, bounds.Y + pad,
                    bounds.Width - pad * 2, UiTheme.Px(20, uiScale)),
                UiTheme.TextMuted);

            DrawWrapped(
                "CARTA: anade una carta nueva si hay espacio o mejora una " +
                "carta si el mazo esta lleno.\n" +
                "PERK: adjunta una habilidad a una carta compatible del " +
                "mazo; la habilidad viaja con esa carta.",
                new Rectangle(bounds.X + pad,
                    bounds.Y + pad + UiTheme.Px(28, uiScale),
                    bounds.Width - pad * 2,
                    bounds.Height - pad * 2 - UiTheme.Px(28, uiScale)),
                UiTheme.TextSecondary, UiTheme.Body);
        }

        private void DrawBranchDeck(SpriteBatch spriteBatch, float uiScale)
        {
            var area = BranchDeckArea;
            if (area.Height < UiTheme.Px(50, uiScale)) return;

            DrawSectionLabel(
                $"MAZO ACTUAL {_runState.Deck.Count}/{RunState.MaxDeckSize}",
                new Rectangle(area.X, area.Y, area.Width,
                    UiTheme.Px(20, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);

            var strip = new Rectangle(area.X,
                area.Y + UiTheme.Px(24, uiScale), area.Width,
                Math.Max(0, area.Height - UiTheme.Px(24, uiScale)));

            int count = _runState.Deck.Count;
            if (count == 0) return;

            int gap = UiTheme.Px(UiTheme.SpaceXs, uiScale);
            int width = Math.Max(8, (strip.Width - gap * (count - 1)) / count);
            int height = Math.Min(strip.Height, (int)(width * 1.36f));

            for (int i = 0; i < count; i++)
            {
                var slot = new Rectangle(strip.X + i * (width + gap),
                    strip.Y, width, height);
                DrawCardInstance(slot, _runState.Deck[i],
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.MiniDeck,
                        ShowName = false
                    });
            }
        }

        private void DrawBranchTile(string key, Rectangle bounds, string title,
            string subtitle, bool enabled, Color accent)
        {
            DrawPanel(bounds, enabled ? PanelTone.Raised : PanelTone.Sunken);
            if (IsHovered(key) && enabled)
                DrawPanel(Inset(bounds, UiTheme.Px(3, Scaffold.UiScale)),
                    PanelTone.Accent, false);

            int pad = UiTheme.Px(UiTheme.SpaceXl, Scaffold.UiScale);
            DrawDisplayCentered(title,
                new Rectangle(bounds.X, bounds.Y + pad, bounds.Width,
                    UiTheme.Px(48, Scaffold.UiScale)),
                enabled ? accent : UiTheme.TextMuted, UiTheme.Display);

            DrawTextCenteredFit(subtitle,
                new Rectangle(bounds.X + pad, bounds.Y + pad +
                    UiTheme.Px(54, Scaffold.UiScale),
                    bounds.Width - pad * 2, UiTheme.Px(44, Scaffold.UiScale)),
                enabled ? UiTheme.TextSecondary : UiTheme.TextMuted,
                UiTheme.Caption);
        }

        private void DrawCardOffers(SpriteBatch spriteBatch, float uiScale)
        {
            DrawSectionLabel("ELIGE UNA CARTA",
                new Rectangle(PhaseArea.X, PhaseArea.Y, PhaseArea.Width,
                    UiTheme.Px(24, uiScale)),
                UiTheme.TextSecondary, UiTheme.LabelSmall);

            for (int i = 0; i < _levelUp.CardOffers.Count; i++)
            {
                DrawCardInstance(GetCardOfferBounds(i),
                    _levelUp.CardOffers[i],
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.Medallion,
                        State = IsHovered($"card_offer{i}") &&
                                Input.IsLeftMouseDown()
                            ? CardVisualState.Selected
                            : CardVisualState.Normal
                    });
            }
        }

        private void DrawDeckGrid(SpriteBatch spriteBatch, float uiScale,
            string header)
        {
            DrawSectionLabel(header,
                new Rectangle(PhaseArea.X, PhaseArea.Y, PhaseArea.Width,
                    UiTheme.Px(24, uiScale)),
                UiTheme.Gold, UiTheme.LabelSmall);

            bool upgrading = _phase == Phase.PickUpgradeCard;
            bool pickingHost = _phase == Phase.PickPerkTarget;

            for (int i = 0; i < _runState.Deck.Count; i++)
            {
                var card = _runState.Deck[i];
                var state = CardVisualState.Normal;
                string tag = null;

                if (ReferenceEquals(card, _selectedCard))
                    state = CardVisualState.Selected;
                else if (upgrading && !card.CanUpgrade)
                    state = CardVisualState.Sold;
                else if (pickingHost &&
                         !_levelUpSystem.CanAttach(_selectedPerk, card))
                {
                    state = CardVisualState.Sold;
                    tag = _levelUpSystem.IncompatibleReason(_selectedPerk, card);
                }

                DrawCardInstance(GetDeckCardBounds(i), card,
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.Collection,
                        State = state,
                        Tag = tag
                    });
            }
        }

        private void DrawUpgradeType(SpriteBatch spriteBatch, float uiScale)
        {
            var area = PhaseArea;

            if (_selectedCard != null)
            {
                var portrait = new Rectangle(area.Center.X -
                    (int)(area.Width * 0.16f), area.Y,
                    (int)(area.Width * 0.32f),
                    (int)(area.Height * 0.48f));
                DrawCardInstance(portrait, _selectedCard,
                    new CardRenderOptions
                    {
                        Variant = CardFrameVariant.Medallion,
                        State = CardVisualState.Selected
                    });
            }

            DrawTextCenteredFit(
                $"MEJORAS {_selectedCard?.UpgradeCount ?? 0}/" +
                $"{CardInstance.MaxUpgrades}   " +
                $"COSTE {_selectedCard?.EffectiveCost ?? 0}   " +
                $"PODER {_selectedCard?.CurrentPower ?? 0}",
                new Rectangle(area.X, area.Y + (int)(area.Height * 0.48f),
                    area.Width, UiTheme.Px(22, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);

            bool costEnabled = _selectedCard != null &&
                               _selectedCard.CanUpgradeCost;
            bool powerEnabled = _selectedCard != null &&
                                _selectedCard.CanUpgrade;

            DrawUpgradeTile("upgrade_cost", GetUpgradeButton(true), "-1 COSTE",
                "REDUCE EL COSTE EFECTIVO EN 1", costEnabled,
                UiTheme.Accent);
            DrawUpgradeTile("upgrade_power", GetUpgradeButton(false),
                "+2 PODER", "SUMA 2 DE PODER A LA CARTA", powerEnabled,
                UiTheme.Gold);
        }

        private void DrawUpgradeTile(string key, Rectangle bounds, string title,
            string subtitle, bool enabled, Color accent)
        {
            DrawPanel(bounds, enabled ? PanelTone.Raised : PanelTone.Sunken);
            if (IsHovered(key) && enabled)
                DrawPanel(Inset(bounds, UiTheme.Px(3, Scaffold.UiScale)),
                    PanelTone.Accent, false);

            int pad = UiTheme.Px(UiTheme.SpaceXl, Scaffold.UiScale);
            DrawDisplayCentered(title,
                new Rectangle(bounds.X, bounds.Y + pad, bounds.Width,
                    UiTheme.Px(36, Scaffold.UiScale)),
                enabled ? accent : UiTheme.TextMuted, UiTheme.Heading);
            DrawTextCenteredFit(subtitle,
                new Rectangle(bounds.X + pad, bounds.Y + pad +
                    UiTheme.Px(42, Scaffold.UiScale),
                    bounds.Width - pad * 2, UiTheme.Px(36, Scaffold.UiScale)),
                enabled ? UiTheme.TextSecondary : UiTheme.TextMuted,
                UiTheme.Caption);
        }

        private void DrawPerkOffers(SpriteBatch spriteBatch, float uiScale)
        {
            DrawSectionLabel("ELIGE UN PERK",
                new Rectangle(PhaseArea.X, PhaseArea.Y, PhaseArea.Width,
                    UiTheme.Px(24, uiScale)),
                UiTheme.Perk, UiTheme.LabelSmall);

            for (int i = 0; i < _levelUp.PerkOffers.Count; i++)
            {
                var bounds = GetPerkBounds(i);
                var perk = _perkRegistry?.Get(_levelUp.PerkOffers[i]);

                DrawPanel(bounds, IsHovered($"perk_offer{i}")
                    ? PanelTone.Perk
                    : PanelTone.Sunken);

                int pad = UiTheme.Px(UiTheme.SpaceLg, uiScale);
                DrawLabel(perk?.Name ?? _levelUp.PerkOffers[i],
                    new Vector2(bounds.X + pad, bounds.Y +
                        UiTheme.Px(UiTheme.SpaceSm, uiScale)),
                    UiTheme.Perk, UiTheme.Label);

                DrawTextCenteredFit(perk?.RuleText,
                    new Rectangle(bounds.X + pad, bounds.Y +
                        UiTheme.Px(28, uiScale), bounds.Width - pad * 2,
                        UiTheme.Px(40, uiScale)),
                    UiTheme.TextSecondary, UiTheme.Caption);

                string requirement = perk != null && perk.HasHostRequirement
                    ? perk.CompatibilityText
                    : "OBJETIVO: CUALQUIER CARTA DEL MAZO";
                DrawTextCenteredFit(requirement,
                    new Rectangle(bounds.X + pad, bounds.Bottom -
                        UiTheme.Px(22, uiScale), bounds.Width - pad * 2,
                        UiTheme.Px(18, uiScale)),
                    perk != null && perk.HasHostRequirement
                        ? UiTheme.Gold
                        : UiTheme.TextMuted,
                    UiTheme.Caption);
            }
        }

        private void DrawUnavailable(SpriteBatch spriteBatch, float uiScale)
        {
            var area = PhaseArea;
            DrawDisplayCentered("SIN OPCIONES",
                new Rectangle(area.X, area.Y + (int)(area.Height * 0.24f),
                    area.Width, UiTheme.Px(44, uiScale)),
                UiTheme.TextMuted, UiTheme.Heading);
            DrawTextCenteredFit("NO HAY CARTAS NI PERKS DISPONIBLES",
                new Rectangle(area.X, area.Y + (int)(area.Height * 0.34f),
                    area.Width, UiTheme.Px(24, uiScale)),
                UiTheme.TextSecondary, UiTheme.Caption);
            DrawTapButtonScaled("continue", "CONTINUAR", UiTheme.AccentSoft);
        }

        private string PerkName(string perkId)
        {
            if (perkId == null) return "SIN PERK";
            var perk = _perkRegistry?.Get(perkId);
            return perk != null ? perk.Name : perkId;
        }
    }
}
