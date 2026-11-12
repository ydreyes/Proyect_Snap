using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SnapClone.Core.Services;
using SnapClone.Core.Layout;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Effects;
using SnapClone.Gameplay.Actions;
using SnapClone.Gameplay.AI;
using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Combat;
using SnapClone.Gameplay.Meta;
using SnapClone.Gameplay.Run;
using SnapClone.Persistence;
using SnapClone.UI.Primitives;
using SnapClone.UI.Components;

namespace SnapClone.Scenes
{
    /// <summary>
    /// Escena de combate: 3 locaciones con rejilla 2x2 por lado, placa
    /// compacta, mano abajo y 6 turnos. La energia limita las jugadas.
    /// Permite arrastrar de la mano al tablero y del tablero a la mano.
    /// </summary>
    public class CombatScene : IScene
    {
        private const int StartingHandSize = 3;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly GameServices _services;
        private readonly SceneManager _sceneManager;

        private ShapeDrawer _shapeDrawer;
        private FontService _fontService;
        private LayoutManager _layoutManager;
        private SpaceBackground _background;
        private Modal _modal;
        private VictoryResolver _victoryResolver;
        private PowerResolver _powerResolver;
        private EffectProcessor _effectProcessor;

        private BoardState _boardState;
        private TurnManager _turnManager;
        private AIController _aiController;
        private RunDirector _runDirector;
        private RunState _runState;
        private RunCheckpointService _checkpoint;
        private string _nextScene = "Summary";

        // Posicion mostrada en la cabecera. Se congela al entrar para que el
        // HUD no salte cuando el resultado ya avanzo el grafo bajo el modal.
        private int _hudAct = 1;
        private int _hudCombatIndexInAct = 0;
        private int _hudFinalActStep = 0;
        private readonly List<LocationView> _locations = new List<LocationView>();
        private readonly List<CardView> _handViews = new List<CardView>();
        private readonly List<DropZone> _dropZones = new List<DropZone>();
        private AvatarView _playerAvatar;
        private AvatarView _rivalAvatar;
        private CardActionService _actionService;
        private int _lastSyncedVersion;
        private PileViewerView _pileViewer;
        private DetailPreviewView _detailPreview;

        private ItemRegistry _itemRegistry;
        private CombatItemSystem _itemSystem;
        private ProfileDto _profile;
        private string _itemMessage;
        private bool _itemMessageIsError;
        private double _itemMessageUntil;

        private CardView _draggedCard;
        private Vector2 _dragOffset;
        private int _hoverZone = -1;
        private int _hoverSlot = -1;

        /// <summary>Distancia que separa un toque de un arrastre.</summary>
        private const float TapThreshold = 10f;

        private bool _pressActive;
        private Vector2 _pressPoint;
        private CardView _pressView;
        private CardOrigin _pressOrigin = CardOrigin.Hand;
        private CardInstance _pressCard;
        private BoardSide _pressSide = BoardSide.Player;
        private int _pressZone = -1;
        private int _pressSlot = -1;

        private bool _gameEvaluated;
        private bool _agathaPlaying;

        public CombatScene(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameServices services, SceneManager sceneManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _services = services;
            _sceneManager = sceneManager;
        }

        public void Initialize() { }

        public void LoadContent()
        {
            _shapeDrawer = new ShapeDrawer(_graphicsDevice, _spriteBatch);

            var content = _services.Has<Microsoft.Xna.Framework.Content.ContentManager>()
                ? _services.Get<Microsoft.Xna.Framework.Content.ContentManager>()
                : _services.Get<Game>().Content;

            _fontService = new FontService(content);
            _layoutManager = _services.Get<LayoutManager>();
            _background = new SpaceBackground();
            _modal = new Modal(_shapeDrawer, _fontService);
            _victoryResolver = new VictoryResolver();
            _pileViewer = new PileViewerView();
            _detailPreview = new DetailPreviewView();
        }

        /// <summary>
        /// Cada entrada a combate empieza una partida nueva: tablero vacio,
        /// mazo de la run, manos repartidas y turno 1.
        /// </summary>
        public void OnEnter()
        {
            _modal.Hide();
            _pileViewer.Close();
            _detailPreview.Close();
            ClearPress();
            _gameEvaluated = false;
            _draggedCard = null;
            _hoverZone = -1;
            _hoverSlot = -1;
            _nextScene = "Summary";
            _itemMessage = null;
            _itemMessageIsError = false;
            _itemMessageUntil = 0;

            _boardState = new BoardState();

            var perkRegistry = _services.Has<PerkRegistry>() ? _services.Get<PerkRegistry>() : null;
            var effectRegistry = _services.Has<EffectRegistry>() ? _services.Get<EffectRegistry>() : null;
            var cardRegistry = _services.Has<CardRegistry>() ? _services.Get<CardRegistry>() : null;
            var zoneRegistry = _services.Has<ZoneRegistry>() ? _services.Get<ZoneRegistry>() : null;
            _effectProcessor = new EffectProcessor(effectRegistry, cardRegistry,
                _services.Get<RandomService>());
            _powerResolver = new PowerResolver(perkRegistry, _effectProcessor);
            _runDirector = _services.Get<RunDirector>();
            _runState = _services.Get<RunState>();
            _checkpoint = _services.Get<RunCheckpointService>();
            _hudAct = _runState.ActIndex;
            _hudCombatIndexInAct = _runState.CombatIndexInAct;
            _hudFinalActStep = _runState.FinalActStep;

            BuildBoard();
            _boardState.PowerResolver = _powerResolver;
            BuildAvatars();
            BuildDropZones();
            SetupDecks();
            DealStartingHands();

            // Puerta unica para mover cartas a las pilas del combate actual.
            var pileManager = new CardPileManager(_boardState, _runState);
            _actionService = new CardActionService(_boardState, pileManager, _powerResolver,
                _effectProcessor, _runState, zoneRegistry);
            _lastSyncedVersion = _boardState.Version;

            _itemRegistry = _services.Has<ItemRegistry>()
                ? _services.Get<ItemRegistry>()
                : null;
            _profile = _checkpoint?.Profile;
            _itemSystem = new CombatItemSystem(_boardState, _actionService,
                _effectProcessor, _itemRegistry, _services.Get<RandomService>());

            _turnManager = new TurnManager(_boardState, _services.Get<TimeService>(),
                _powerResolver, _effectProcessor, _runState);
            _aiController = new AIController(_boardState, _services.Get<RandomService>(),
                _actionService, _turnManager, _runState?.PendingCombat?.Difficulty ?? 1);

            _actionService.StartCombat();
            _turnManager.StartTurn();
            RefreshHandViews();
            PlayAIAll();
            RefreshPowerTotals();

            _layoutManager.CalculateLayout();
            UpdateViewLayout();
        }

        // ------------------------------------------------------------------
        // Construccion del estado
        // ------------------------------------------------------------------

        private void BuildBoard()
        {
            _locations.Clear();
            _boardState.Zones.Clear();

            var zoneRegistry = _services.Has<ZoneRegistry>() ? _services.Get<ZoneRegistry>() : null;
            var zoneDefs = zoneRegistry != null ? zoneRegistry.GetAll() : new List<Data.Definitions.ZoneDef>();

            for (int i = 0; i < 3; i++)
            {
                Data.Definitions.ZoneDef def;
                if (i < zoneDefs.Count)
                {
                    def = zoneDefs[i];
                }
                else
                {
                    def = new Data.Definitions.ZoneDef
                    {
                        Id = $"zone_{i}",
                        Name = $"LOCACION {i + 1}",
                        Description = "Sin efecto definido."
                    };
                }

                _boardState.Zones.Add(new ZoneInstance(def, i) { Board = _boardState });

                _locations.Add(new LocationView
                {
                    Index = i,
                    Name = def.Name,
                    EffectText = def.Description
                });
            }
        }

        private void BuildAvatars()
        {
            _playerAvatar = new AvatarView
            {
                Name = "JUGADOR",
                Bounds = _layoutManager.LeftAvatar,
                IsPlayer = true,
                Accent = new Color(72, 126, 210)
            };

            string rivalName = _runState?.PendingCombat?.EnemyName;
            _rivalAvatar = new AvatarView
            {
                Name = string.IsNullOrWhiteSpace(rivalName) ? "ENEMIGO" : rivalName,
                Bounds = _layoutManager.RightAvatar,
                IsPlayer = false,
                Accent = new Color(188, 84, 62)
            };
        }

        private void BuildDropZones()
        {
            _dropZones.Clear();

            for (int zone = 0; zone < _locations.Count; zone++)
            {
                for (int slot = 0; slot < LocationView.SlotsPerSide; slot++)
                {
                    _dropZones.Add(new DropZone(Rectangle.Empty, zone, slot, true));
                }
            }
        }

        /// <summary>
        /// El jugador combate con el mazo que salio del draft. Si se entra
        /// directo (sin draft), se usa un mazo aleatorio de respaldo.
        /// </summary>
        private void SetupDecks()
        {
            _boardState.PlayerDeck.Clear();
            _boardState.AIDeck.Clear();
            _boardState.PlayerHand.Clear();
            _boardState.AIHand.Clear();

            var runState = _services.Has<RunState>() ? _services.Get<RunState>() : null;
            if (runState != null)
            {
                foreach (var card in runState.Deck)
                {
                    // Normaliza el estado de combate anterior y re-resuelve el
                    // poder desde la base mas los perks.
                    card.ResetForCombat();
                    _powerResolver.ResolveCard(card);

                    _boardState.PlayerDeck.Add(card);
                }
            }

            if (_boardState.PlayerDeck.Count == 0)
            {
                var defs = GetCardDefinitions();
                for (int i = 0; i < 8 && defs.Count > 0; i++)
                {
                    var card = new CardInstance(defs[RandomIndex(defs.Count)]);
                    _powerResolver.ResolveCard(card);
                    _boardState.PlayerDeck.Add(card);
                }
            }

            ShuffleDeck(_boardState.PlayerDeck, _services.Get<RandomService>());

            // El mazo rival es el mazo fijo del enemigo elegido; la semilla
            // del intento solo decide el orden inicial.
            var pending = _runState?.PendingCombat;
            int shuffleSeed = pending?.Seed ?? 0;

            var cardRegistry = _services.Has<CardRegistry>()
                ? _services.Get<CardRegistry>()
                : null;
            var enemy = ResolveEnemy(pending);
            var rivalDefs = EnemyDeckBuilder.BuildShuffledDefs(cardRegistry, enemy,
                shuffleSeed);

            foreach (var def in rivalDefs)
            {
                var card = new CardInstance(def);
                card.Location = CardLocation.Deck;
                _powerResolver.ResolveCard(card);
                _boardState.AIDeck.Add(card);
            }
        }

        /// <summary>
        /// Resuelve el enemigo del snapshot. Si el id no existe, cae al primer
        /// enemigo del acto actual para no volver al pool aleatorio de cartas.
        /// </summary>
        private EnemyDef ResolveEnemy(PendingCombat pending)
        {
            var registry = _services.Has<EnemyRegistry>()
                ? _services.Get<EnemyRegistry>()
                : null;

            if (registry == null) return null;

            var enemy = registry.Get(pending?.EnemyId);
            if (enemy != null) return enemy;

            int actIndex = _runState?.ActIndex ?? 1;
            var pool = registry.GetByAct(actIndex);
            if (pool.Count > 0) return pool[0];

            return registry.GetFinalBoss();
        }

        private List<Data.Definitions.CardDef> GetCardDefinitions()
        {
            return _services.Has<CardRegistry>()
                ? _services.Get<CardRegistry>().GetDraftable()
                : new List<Data.Definitions.CardDef>();
        }

        private int RandomIndex(int count) => _services.Get<RandomService>().Next(count);

        private static void ShuffleDeck<T>(List<T> deck, RandomService random)
        {
            if (deck == null || random == null) return;

            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T tmp = deck[i];
                deck[i] = deck[j];
                deck[j] = tmp;
            }
        }

        private void DealStartingHands()
        {
            int handSize = RunModifierSystem.StartingHandSizeFor(_runState, StartingHandSize);
            OpeningHandDealer.Deal(_boardState.PlayerDeck, _boardState.PlayerHand, handSize);
            OpeningHandDealer.Deal(_boardState.AIDeck, _boardState.AIHand, handSize);
        }

        /// <summary>
        /// Unica escritura de los totales de las placas. Recalcula el poder de
        /// todas las cartas y copia el total de cada zona a su LocationView.
        /// </summary>
        private void RefreshPowerTotals()
        {
            if (_boardState == null || _powerResolver == null) return;

            _powerResolver.UpdateAllPowers(_boardState);

            for (int i = 0; i < _locations.Count && i < _boardState.Zones.Count; i++)
            {
                var zone = _boardState.Zones[i];
                _locations[i].SetPower(zone.GetPlayerPower(), zone.GetAIPower());
                _locations[i].StatusText = GetZoneStatus(zone);
                RefreshMoveBadges(_locations[i]);
            }
        }

        private string GetZoneStatus(ZoneInstance zone)
        {
            if (zone == null) return string.Empty;

            bool flooded = zone.IsFlooded(_turnManager?.CurrentTurn ??
                _boardState.CurrentTurn);
            bool locked = _effectProcessor != null &&
                _effectProcessor.IsZonePlayLocked(zone);
            bool suppressed = _boardState.AreZoneEffectsSuppressed(
                _turnManager?.CurrentTurn ?? _boardState.CurrentTurn);
            bool limbo = zone.Definition != null &&
                zone.Definition.Id == "zone_limbo";
            bool goose = zone.PlayerCards.Concat(zone.AICards).Any(card =>
                card != null && !card.AbilitiesDisabled &&
                SnapClone.Gameplay.Meta.PerkAbilities.HasEffect(card,
                    "effect_goose"));
            bool jean = zone.PlayerCards.Concat(zone.AICards).Any(card =>
                card != null && !card.AbilitiesDisabled &&
                SnapClone.Gameplay.Meta.PerkAbilities.HasEffect(card,
                    "effect_jean_grey"));

            if (limbo) return "LIMBO";
            if (suppressed) return "SILENCIADA";
            if (goose) return "GOOSE";
            if (jean) return "JEAN GREY";
            if (flooded && locked) return "INUNDADA / BLOQUEADA";
            if (flooded) return "INUNDADA";
            if (locked) return "BLOQUEADA";
            return string.Empty;
        }

        private void RefreshMoveBadges(LocationView location)
        {
            if (location == null || _actionService == null) return;

            for (int slot = 0; slot < LocationView.SlotsPerSide; slot++)
            {
                var view = location.PlayerSlotCards[slot];
                if (view?.Card == null)
                    continue;

                string status = _actionService.GetMoveStatus(view.Card,
                    BoardSide.Player);
                view.ShowMoveBadge = status == "MOVIBLE" ||
                    status == "MOVIBLE A CLOAK";
            }
        }

        private void RefreshHandViews()
        {
            _handViews.Clear();

            foreach (var card in _boardState.PlayerHand)
                _handViews.Add(new CardView(card, Vector2.Zero) { Origin = CardOrigin.Hand });
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private void UpdateViewLayout()
        {
            _playerAvatar.Bounds = _layoutManager.LeftAvatar;
            _rivalAvatar.Bounds = _layoutManager.RightAvatar;

            for (int i = 0; i < _locations.Count; i++)
            {
                var location = _locations[i];
                location.Panel = _layoutManager.GetLocationPanel(i);

                if (_boardState != null && i < _boardState.Zones.Count)
                {
                    var zone = _boardState.Zones[i];
                    location.Name = zone.Definition != null ? zone.Definition.Name : "LOCACION";
                    location.EffectText = zone.Definition != null
                        ? zone.Definition.Description
                        : string.Empty;
                    location.IsRevealed = zone.IsVisibleTo(BoardSide.Player);
                }

                for (int slot = 0; slot < LocationView.SlotsPerSide; slot++)
                {
                    location.OpponentSlots[slot] = _layoutManager.GetOpponentCardSlot(i, slot);
                    location.PlayerSlots[slot] = _layoutManager.GetPlayerCardSlot(i, slot);
                }
            }

            UpdateDropZoneBounds();
            UpdateHandPositions();
        }

        private void UpdateDropZoneBounds()
        {
            for (int zone = 0; zone < _locations.Count; zone++)
            {
                for (int slot = 0; slot < LocationView.SlotsPerSide; slot++)
                {
                    var dropZone = _dropZones[zone * LocationView.SlotsPerSide + slot];
                    dropZone.Bounds = _locations[zone].PlayerSlots[slot];
                }
            }
        }

        private void UpdateHandPositions()
        {
            int count = _handViews.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                var view = _handViews[i];
                var bounds = _layoutManager.GetHandCard(i, count);

                view.Size = new Vector2(bounds.Width, bounds.Height);

                if (view.IsDragging) continue;

                view.Position = new Vector2(bounds.X, bounds.Y);
                view.OriginalPosition = view.Position;
                view.Size = new Vector2(bounds.Width, bounds.Height);
            }
        }

        // ------------------------------------------------------------------
        // Sincronizacion de vistas tras un cambio de pila
        // ------------------------------------------------------------------

        /// <summary>
        /// Reconcilia mano y slots cuando una carta cambio de pila. Se apoya en
        /// BoardState.Version: si nadie movio una carta, no hace nada.
        /// </summary>
        private void SyncViewsIfDirty()
        {
            if (_boardState == null || _boardState.Version == _lastSyncedVersion) return;

            _lastSyncedVersion = _boardState.Version;

            SyncHandViews();

            for (int i = 0; i < _locations.Count; i++)
            {
                SyncSlotViews(_locations[i], BoardSide.Player);
                SyncSlotViews(_locations[i], BoardSide.AI);
            }

            RefreshPowerTotals();
            UpdateHandPositions();
            CancelVanishedDrag();
        }

        private void SyncHandViews()
        {
            for (int i = _handViews.Count - 1; i >= 0; i--)
            {
                var card = _handViews[i].Card;
                if (card == null || !_boardState.PlayerHand.Contains(card))
                    _handViews.RemoveAt(i);
            }

            foreach (var card in _boardState.PlayerHand)
            {
                // La carta que se esta arrastrando sigue en el modelo de la
                // mano, pero su vista no debe reaparecer hasta que se suelte.
                if (_draggedCard != null && ReferenceEquals(_draggedCard.Card, card))
                    continue;

                if (HasHandView(card)) continue;
                _handViews.Add(new CardView(card, Vector2.Zero) { Origin = CardOrigin.Hand });
            }
        }

        private bool HasHandView(CardInstance card)
        {
            foreach (var view in _handViews)
            {
                if (ReferenceEquals(view.Card, card)) return true;
            }

            return false;
        }

        private void SyncSlotViews(LocationView location, BoardSide side)
        {
            if (location.Index < 0 || location.Index >= _boardState.Zones.Count) return;

            var zone = _boardState.Zones[location.Index];
            var cards = side == BoardSide.Player ? zone.PlayerCards : zone.AICards;
            var views = side == BoardSide.Player ? location.PlayerSlotCards : location.OpponentSlotCards;

            for (int slot = 0; slot < views.Length; slot++)
            {
                var view = views[slot];
                if (view == null) continue;
                if (view.Card != null && cards.Contains(view.Card)) continue;

                views[slot] = null;
                if (side == BoardSide.Player)
                    SetDropZoneOccupant(location.Index, slot, null);
            }

            foreach (var card in cards)
            {
                if (FindSlot(views, card) >= 0) continue;

                int slot = FirstFreeSlot(views);
                if (slot < 0) break;

                views[slot] = new CardView(card, Vector2.Zero)
                {
                    Origin = CardOrigin.Board,
                    OriginZone = location.Index,
                    OriginSlot = slot,
                    IsInZone = true,
                    IsAffordable = true
                };

                if (side == BoardSide.Player)
                    SetDropZoneOccupant(location.Index, slot, views[slot]);
            }
        }

        private static int FindSlot(CardView[] views, CardInstance card)
        {
            for (int slot = 0; slot < views.Length; slot++)
            {
                if (views[slot] != null && ReferenceEquals(views[slot].Card, card))
                    return slot;
            }

            return -1;
        }

        private static int FirstFreeSlot(CardView[] views)
        {
            for (int slot = 0; slot < views.Length; slot++)
            {
                if (views[slot] == null) return slot;
            }

            return -1;
        }

        private void SetDropZoneOccupant(int zone, int slot, CardView view)
        {
            int index = zone * LocationView.SlotsPerSide + slot;
            if (index < 0 || index >= _dropZones.Count) return;
            _dropZones[index].OccupiedBy = view;
        }

        /// <summary>
        /// Si la carta arrastrada entro en una pila, se cancela el arrastre sin
        /// devolverla a ningun sitio. Se mira la pila y no la mano o la zona
        /// porque empezar a arrastrar ya la saca de ahi.
        /// </summary>
        private void CancelVanishedDrag()
        {
            if (_draggedCard == null) return;

            var card = _draggedCard.Card;
            if (card == null || !IsInAnyPile(card)) return;

            _draggedCard.EndDrag();
            _draggedCard = null;
            _hoverZone = -1;
            _hoverSlot = -1;

            var input = _services.Get<InputService>();
            input.IsDragging = false;
            input.DraggedCard = null;
        }

        private bool IsInAnyPile(CardInstance card) =>
            _boardState.PlayerPiles.Contains(card) || _boardState.AIPiles.Contains(card);

        // ------------------------------------------------------------------
        // Barra de objetos
        // ------------------------------------------------------------------

        private bool HandleItemBarInput(Vector2 point, GameTime gameTime)
        {
            if (_itemSystem == null || _profile == null) return false;

            var items = _itemSystem.GetInventory(_profile);
            if (items.Count == 0) return false;

            var bar = _layoutManager.ItemBar;
            if (!IsInside(bar, point)) return false;

            float slotWidth = bar.Width / (float)items.Count;
            int index = (int)((point.X - bar.X) / slotWidth);
            if (index < 0) index = 0;
            if (index >= items.Count) index = items.Count - 1;

            var item = items[index];
            var result = _itemSystem.TryUse(item.Id, _profile, true,
                profile => _checkpoint.SaveProfile(out _));

            if (result.Success)
                ShowItemMessage($"Usaste {item.Name}.", false, gameTime);
            else
                ShowItemMessage(result.Error, true, gameTime);

            // El efecto ya se aplico: las vistas, el poder y los contadores se
            // reconcilian en el mismo frame.
            SyncViewsIfDirty();
            RefreshPowerTotals();
            UpdateViewLayout();
            return true;
        }

        private void ShowItemMessage(string message, bool isError, GameTime gameTime)
        {
            _itemMessage = message;
            _itemMessageIsError = isError;
            _itemMessageUntil = gameTime.TotalGameTime.TotalSeconds + 2.5;
        }

        // ------------------------------------------------------------------
        // Update
        // ------------------------------------------------------------------

        public void Update(GameTime gameTime)
        {
            var input = _services.Get<InputService>();

            if (_itemMessage != null &&
                gameTime.TotalGameTime.TotalSeconds >= _itemMessageUntil)
                _itemMessage = null;

            _layoutManager.CalculateLayout();
            SyncViewsIfDirty();
            UpdateViewLayout();

            if (_modal.IsVisible)
            {
                if (input.IsLeftMousePressed() && _modal.IsButtonClicked(input.MousePosition))
                {
                    _modal.Hide();
                    _sceneManager.ChangeScene(_nextScene);
                }
                return;
            }

            var timeService = _services.Get<TimeService>();
            timeService.Update(gameTime);

            if (_turnManager.CurrentPhase == TurnPhase.PlayPhase && timeService.IsTurnTimeExpired())
            {
                _pileViewer.Close();
                _detailPreview.Close();
                ClearPress();
                CancelDragToHand();
                EndTurn();
                return;
            }

            if (_detailPreview.IsOpen)
            {
                HandleDetailPreviewInput(input);
                return;
            }

            if (_pileViewer.IsOpen)
            {
                HandlePileViewerInput(input);

                if (input.IsLeftMousePressed() && IsInside(_layoutManager.EndTurnButton, input.MousePosition))
                    EndTurn();

                return;
            }

            if (input.IsLeftMousePressed() && TryOpenPileViewer(input.MousePosition))
                return;

            if (input.IsLeftMousePressed() &&
                _turnManager.CurrentPhase == TurnPhase.PlayPhase &&
                HandleItemBarInput(input.MousePosition, gameTime))
                return;

            if (_turnManager.CurrentPhase == TurnPhase.PlayPhase)
            {
                HandleDragAndDrop(input);

                // Mientras hay una carta en el aire no se procesa el boton.
                if (_draggedCard != null) return;
            }

            if (input.IsLeftMousePressed() && IsInside(_layoutManager.EndTurnButton, input.MousePosition))
            {
                EndTurn();
            }
        }

        // ------------------------------------------------------------------
        // Visor de pilas: entrada
        // ------------------------------------------------------------------

        private bool TryOpenPileViewer(Vector2 point)
        {
            var side = HitTestAvatar(point);
            if (side == null) return false;

            _pileViewer.Open(side.Value);
            return true;
        }

        private BoardSide? HitTestAvatar(Vector2 point)
        {
            if (_playerAvatar != null && _playerAvatar.Contains(point))
                return BoardSide.Player;

            if (_rivalAvatar != null && _rivalAvatar.Contains(point))
                return BoardSide.AI;

            return null;
        }

        private void HandlePileViewerInput(InputService input)
        {
            if (!input.IsLeftMousePressed()) return;

            var mouse = input.MousePosition;
            var clickedSide = HitTestAvatar(mouse);

            if (clickedSide != null)
            {
                if (_pileViewer.Side == clickedSide.Value)
                    _pileViewer.Close();
                else
                    _pileViewer.Open(clickedSide.Value);

                return;
            }

            if (!IsInside(_pileViewer.GetBounds(_layoutManager), mouse))
            {
                _pileViewer.Close();
                return;
            }

            for (int i = 0; i < PileViewerView.TabCount; i++)
            {
                if (!IsInside(_pileViewer.GetTabBounds(_layoutManager, i), mouse)) continue;

                _pileViewer.Select(PileViewerView.TabAt(i));
                return;
            }

            var piles = _pileViewer.Side == BoardSide.Player ? _boardState.PlayerPiles : _boardState.AIPiles;
            int count = piles.Count(_pileViewer.SelectedPile);

            if (IsInside(_pileViewer.GetPreviousBounds(_layoutManager), mouse))
            {
                _pileViewer.PreviousPage();
                return;
            }

            if (IsInside(_pileViewer.GetNextBounds(_layoutManager), mouse))
            {
                _pileViewer.NextPage(count);
                return;
            }

            var tappedCard = _pileViewer.HitTestCard(_layoutManager, piles, mouse);
            if (tappedCard != null)
                _detailPreview.OpenCard(tappedCard, _pileViewer.Side);
        }

        private void HandleDetailPreviewInput(InputService input)
        {
            if (!input.IsLeftMousePressed()) return;

            var mouse = input.MousePosition;

            if (_detailPreview.CanActivate &&
                _detailPreview.Card != null &&
                IsInside(_detailPreview.GetActivateBounds(_layoutManager), mouse))
            {
                _actionService.ActivateCard(_detailPreview.Card,
                    BoardSide.Player);
                _detailPreview.Close();
                RefreshHandViews();
                RefreshPowerTotals();
                UpdateViewLayout();
                return;
            }

            // Cualquier toque fuera del contenido cierra el panel; el boton de
            // cierre queda dentro y solo actua sobre el.
            if (IsInside(_detailPreview.GetCloseBounds(_layoutManager), mouse) ||
                !IsInside(_detailPreview.GetBounds(_layoutManager), mouse))
            {
                _detailPreview.Close();
            }
        }

        // ------------------------------------------------------------------
        // Drag and drop
        // ------------------------------------------------------------------

        private void HandleDragAndDrop(InputService input)
        {
            var mouse = input.MousePosition;

            if (_draggedCard == null)
            {
                if (input.IsLeftMousePressed())
                {
                    BeginPress(mouse);
                    return;
                }

                if (!_pressActive) return;

                if (input.IsLeftMouseDown())
                {
                    if (_pressView != null &&
                        Vector2.Distance(mouse, _pressPoint) > TapThreshold)
                    {
                        var view = _pressView;
                        var origin = _pressOrigin;
                        int zone = _pressZone;
                        int slot = _pressSlot;

                        ClearPress();

                        // El offset se calcula en el punto donde se cruzo el
                        // umbral, asi la carta no salta al empezar el arrastre.
                        BeginDrag(input, view, origin, zone, slot, mouse);
                    }

                    return;
                }

                if (input.IsLeftMouseReleased())
                {
                    if (Vector2.Distance(mouse, _pressPoint) <= TapThreshold)
                        OpenPreviewForPress();

                    ClearPress();
                }

                return;
            }

            _draggedCard.Position = mouse - _dragOffset;
            UpdateHover();

            if (!input.IsLeftMouseReleased()) return;

            if (_hoverZone >= 0 && _hoverSlot >= 0 &&
                PlaceInSlot(_hoverZone, _hoverSlot, _draggedCard))
            {
                ClearDragState(input);
                return;
            }

            if (IsInside(_layoutManager.HandArea, mouse))
            {
                AddToHand(_draggedCard);
                ClearDragState(input);
                return;
            }

            ReturnToOrigin(input);
        }

        /// <summary>
        /// Registra lo que hay bajo el puntero sin tocar el tablero. El arrastre
        /// solo empieza si el puntero se mueve mas alla del umbral; si no, al
        /// soltar se abre el panel de detalle.
        /// </summary>
        private void BeginPress(Vector2 point)
        {
            ClearPress();

            if (IsInside(_layoutManager.EndTurnButton, point) ||
                IsInside(_layoutManager.EnergyBadgeBounds, point))
                return;

            var hand = HitTestHand(point);
            if (hand?.Card != null)
            {
                _pressActive = true;
                _pressPoint = point;
                _pressView = hand;
                _pressOrigin = CardOrigin.Hand;
                _pressCard = hand.Card;
                _pressSide = BoardSide.Player;
                return;
            }

            var board = HitTestBoard(point);
            if (board != null)
            {
                _pressActive = true;
                _pressPoint = point;
                _pressView = board.Value.view;
                _pressOrigin = CardOrigin.Board;
                _pressCard = board.Value.view.Card;
                _pressSide = BoardSide.Player;
                _pressZone = board.Value.zone;
                _pressSlot = board.Value.slot;
                return;
            }

            var rival = HitTestRivalBoard(point);
            if (rival != null)
            {
                _pressActive = true;
                _pressPoint = point;
                _pressCard = rival.Value.view.Card;
                _pressSide = BoardSide.AI;
                return;
            }

            int zoneIndex = HitTestZone(point);
            if (zoneIndex < 0) return;

            _pressActive = true;
            _pressPoint = point;
            _pressZone = zoneIndex;
        }

        private void ClearPress()
        {
            _pressActive = false;
            _pressView = null;
            _pressCard = null;
            _pressZone = -1;
            _pressSlot = -1;
        }

        private void OpenPreviewForPress()
        {
            if (_pressCard != null)
            {
                string status = _pressSide == BoardSide.Player
                    ? _actionService?.GetMoveStatus(_pressCard, _pressSide)
                    : null;

                if (_pressSide == BoardSide.Player &&
                    _pressCard.Definition?.Id == "card_howard")
                {
                    var top = _boardState.PlayerDeck.FirstOrDefault();
                    status = "TOPE: " + (top?.Definition?.Name ?? "VACIO");
                }

                _detailPreview.CanActivate = _pressSide == BoardSide.Player &&
                    _effectProcessor != null &&
                    _effectProcessor.CanActivate(_pressCard);
                _detailPreview.OpenCard(_pressCard, _pressSide, status);
                return;
            }

            _detailPreview.CanActivate = false;

            if (_pressZone >= 0 &&
                _pressZone < _boardState.Zones.Count &&
                _pressZone < _locations.Count)
            {
                var zone = _boardState.Zones[_pressZone];
                _detailPreview.OpenZone(zone, GetZoneStatus(zone));
            }
        }

        private int HitTestZone(Vector2 point)
        {
            for (int i = 0; i < _locations.Count; i++)
            {
                if (IsInside(_layoutManager.GetLocationColumn(i), point))
                    return i;
            }

            return -1;
        }

        private (CardView view, int zone, int slot)? HitTestRivalBoard(Vector2 point)
        {
            for (int zone = 0; zone < _locations.Count; zone++)
            {
                for (int slot = 0; slot < LocationView.SlotsPerSide; slot++)
                {
                    var view = _locations[zone].OpponentSlotCards[slot];
                    if (view?.Card == null) continue;

                    if (_locations[zone].OpponentSlots[slot].Contains((int)point.X, (int)point.Y))
                        return (view, zone, slot);
                }
            }

            return null;
        }

        private void BeginDrag(InputService input, CardView view, CardOrigin origin,
            int zone, int slot, Vector2 mouse)
        {
            if (view?.Card == null) return;

            // Una carta del tablero se puede levantar el turno en que se jugo,
            // o despues si tiene un permiso impreso o una ventana de Cloak.
            if (origin == CardOrigin.Board &&
                !_actionService.CanLiftForMove(view.Card, BoardSide.Player))
                return;

            view.Origin = origin;
            view.OriginZone = zone;
            view.OriginSlot = slot;

            if (origin == CardOrigin.Hand)
            {
                // El modelo conserva la carta en la mano hasta que se suelta;
                // solo se retira la vista para que no se dibuje dos veces.
                _handViews.Remove(view);
            }
            else
            {
                if (!LiftFromSlot(zone, slot, view)) return;
            }

            view.IsDragging = true;
            _draggedCard = view;
            _dragOffset = mouse - view.Position;

            input.IsDragging = true;
            input.DraggedCard = view;
            input.DragStartPosition = view.OriginalPosition;
            input.DragOffset = _dragOffset;
        }

        private bool LiftFromSlot(int zone, int slot, CardView view)
        {
            if (zone < 0 || zone >= _locations.Count) return false;
            if (slot < 0 || slot >= LocationView.SlotsPerSide) return false;

            var location = _locations[zone];
            if (location.PlayerSlotCards[slot] != view) return false;

            var lift = _actionService.LiftCard(view.Card, BoardSide.Player);
            if (!lift.Success) return false;

            location.PlayerSlotCards[slot] = null;
            _dropZones[zone * LocationView.SlotsPerSide + slot].OccupiedBy = null;
            view.IsInZone = false;
            RefreshPowerTotals();
            return true;
        }

        private void ReturnToOrigin(InputService input)
        {
            var view = _draggedCard;
            if (view == null) return;

            if (view.Origin == CardOrigin.Board &&
                PlaceInSlot(view.OriginZone, view.OriginSlot, view))
            {
                ClearDragState(input);
                return;
            }

            AddToHand(view);
            ClearDragState(input);
        }

        private void CancelDragToHand()
        {
            if (_draggedCard == null) return;

            AddToHand(_draggedCard);
            ClearDragState(_services.Get<InputService>());
        }

        private void ClearDragState(InputService input)
        {
            if (_draggedCard != null)
            {
                _draggedCard.EndDrag();
                _draggedCard = null;
            }

            _hoverZone = -1;
            _hoverSlot = -1;

            input.IsDragging = false;
            input.DraggedCard = null;
        }

        private void AddToHand(CardView view)
        {
            // Defensa: BeginDrag ya impide levantar una carta de un turno
            // anterior. Si otra ruta intentara devolverla a la mano, se
            // conserva en su slot de origen.
            if (view.Origin == CardOrigin.Board &&
                view.Card.PlayedTurn != _turnManager.CurrentTurn)
            {
                PlaceInSlot(view.OriginZone, view.OriginSlot, view);
                return;
            }

            var returned = _actionService.ReturnToHand(view.Card, BoardSide.Player);
            if (!returned.Success)
            {
                if (view.Origin == CardOrigin.Board)
                    PlaceInSlot(view.OriginZone, view.OriginSlot, view);
                return;
            }

            view.EndDrag();
            view.IsInZone = false;
            view.Origin = CardOrigin.Hand;
            view.OriginZone = -1;
            view.OriginSlot = -1;

            if (!_handViews.Contains(view))
                _handViews.Add(view);

            RefreshPowerTotals();
        }

        private bool PlaceInSlot(int zone, int slot, CardView view)
        {
            if (view?.Card == null) return false;
            if (zone < 0 || zone >= _locations.Count) return false;
            if (slot < 0 || slot >= LocationView.SlotsPerSide) return false;

            var location = _locations[zone];
            var zoneInstance = _boardState.Zones[zone];

            if (location.PlayerSlotCards[slot] != null) return false;
            if (zoneInstance.PlayerCards.Count >= ZoneInstance.MaxCardsPerSide) return false;

            bool cameFromHand = view.Origin == CardOrigin.Hand;

            if (cameFromHand &&
                view.Card.EffectiveCost > _turnManager.PlayerAvailableEnergy)
                return false;

            ActionResult action;
            if (cameFromHand)
                action = _actionService.PlayCard(view.Card, BoardSide.Player, zone, slot);
            else
                action = _actionService.MoveCardByPlayer(view.Card,
                    BoardSide.Player, view.OriginZone, zone);

            if (!action.Success) return false;

            var bounds = location.PlayerSlots[slot];

            location.PlayerSlotCards[slot] = view;
            _dropZones[zone * LocationView.SlotsPerSide + slot].OccupiedBy = view;

            view.EndDrag();
            view.IsInZone = true;
            view.SnapTo(bounds);
            view.Origin = CardOrigin.Board;
            view.OriginZone = zone;
            view.OriginSlot = slot;

            RefreshPowerTotals();
            return true;
        }

        private void UpdateHover()
        {
            _hoverZone = -1;
            _hoverSlot = -1;

            if (_draggedCard == null) return;

            var target = ResolveTarget(_draggedCard.Center);
            if (target == null) return;

            _hoverZone = target.Value.zone;
            _hoverSlot = target.Value.slot;
        }

        /// <summary>
        /// Snap magnetico: si el puntero esta sobre la mitad del jugador de
        /// una locacion, devuelve su slot libre mas cercano.
        /// </summary>
        private (int zone, int slot)? ResolveTarget(Vector2 point)
        {
            for (int zone = 0; zone < _locations.Count; zone++)
            {
                var region = _layoutManager.GetPlayerDropRegion(zone);
                int padX = (int)(region.Width * 0.08f);
                int padY = (int)(region.Height * 0.06f);
                region.Inflate(padX, padY);

                if (!region.Contains((int)point.X, (int)point.Y)) continue;

                int bestSlot = -1;
                float bestDistance = float.MaxValue;

                for (int slot = 0; slot < LocationView.SlotsPerSide; slot++)
                {
                    if (_locations[zone].PlayerSlotCards[slot] != null) continue;

                    var bounds = _locations[zone].PlayerSlots[slot];
                    var center = new Vector2(bounds.Center.X, bounds.Center.Y);
                    float distance = Vector2.DistanceSquared(point, center);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestSlot = slot;
                    }
                }

                return bestSlot >= 0 ? (zone, bestSlot) : null;
            }

            return null;
        }

        private CardView HitTestHand(Vector2 point)
        {
            for (int i = _handViews.Count - 1; i >= 0; i--)
            {
                var view = _handViews[i];
                var bounds = new Rectangle(
                    (int)view.Position.X,
                    (int)view.Position.Y,
                    (int)view.Size.X,
                    (int)view.Size.Y);

                if (bounds.Contains((int)point.X, (int)point.Y)) return view;
            }

            return null;
        }

        private (CardView view, int zone, int slot)? HitTestBoard(Vector2 point)
        {
            for (int zone = 0; zone < _locations.Count; zone++)
            {
                for (int slot = 0; slot < LocationView.SlotsPerSide; slot++)
                {
                    var view = _locations[zone].PlayerSlotCards[slot];
                    if (view == null) continue;

                    if (_locations[zone].PlayerSlots[slot].Contains((int)point.X, (int)point.Y))
                        return (view, zone, slot);
                }
            }

            return null;
        }

        // ------------------------------------------------------------------
        // Turnos
        // ------------------------------------------------------------------

        private void EndTurn()
        {
            if (_gameEvaluated || _turnManager.CurrentPhase != TurnPhase.PlayPhase) return;

            CancelDragToHand();
            _turnManager.EndTurn();
            RefreshPowerTotals();

            if (_turnManager.IsMatchOver)
            {
                EvaluateMatch();
                return;
            }

            _turnManager.StartTurn();
            RefreshHandViews();
            PlayAIAll();
            RefreshPowerTotals();
            UpdateViewLayout();

            if (_boardState.AgathaControlsEvenTurns && !_gameEvaluated &&
                _turnManager.CurrentTurn % 2 == 0 && !_agathaPlaying)
            {
                PlayAgathaTurn();
            }
        }

        /// <summary>
        /// Agatha juega los turnos pares del jugador con la misma consulta de
        /// legalidad que la accion manual y luego cierra el turno.
        /// </summary>
        private void PlayAgathaTurn()
        {
            _agathaPlaying = true;
            try
            {
                for (int i = 0; i < 8; i++)
                {
                    CardInstance chosen = null;
                    ZoneInstance target = null;

                    foreach (var card in _boardState.PlayerHand.ToList())
                    {
                        if (_actionService == null ||
                            !_actionService.CanPlayCard(card, BoardSide.Player))
                            continue;
                        if (_actionService.GetEffectiveCost(card,
                                BoardSide.Player) >
                            _turnManager.PlayerAvailableEnergy)
                            continue;

                        var zone = _boardState.Zones.FirstOrDefault(z =>
                            _actionService.CanPlayCardInZone(card,
                                BoardSide.Player, z));
                        if (zone == null) continue;

                        chosen = card;
                        target = zone;
                        break;
                    }

                    if (chosen == null || target == null) break;

                    var result = _actionService.PlayCard(chosen,
                        BoardSide.Player, target.ZoneIndex,
                        target.PlayerCards.Count);
                    if (result == null || !result.Success) break;

                    RefreshHandViews();
                    RefreshPowerTotals();
                    UpdateViewLayout();
                }
            }
            finally
            {
                _agathaPlaying = false;
            }

            EndTurn();
        }

        /// <summary>
        /// La IA juega todas las cartas que puede pagar al inicio del turno.
        /// Se crea una vista por jugada y se recalcula el poder de la locacion.
        /// </summary>
        private void PlayAIAll()
        {
            foreach (var play in _aiController.TryPlayAll())
            {
                if (play.Card == null) continue;
                if (play.ZoneIndex < 0 || play.ZoneIndex >= _locations.Count) continue;

                var location = _locations[play.ZoneIndex];
                int slot = play.Slot;

                if (slot < 0 || slot >= LocationView.SlotsPerSide ||
                    location.OpponentSlotCards[slot] != null)
                {
                    slot = FirstFreeSlot(location.OpponentSlotCards);
                }

                if (slot < 0) continue;

                var view = new CardView(play.Card, Vector2.Zero)
                {
                    Origin = CardOrigin.Board,
                    OriginZone = play.ZoneIndex,
                    OriginSlot = slot,
                    IsInZone = true
                };

                location.OpponentSlotCards[slot] = view;
                RefreshPowerTotals();
            }
        }

        private void EvaluateMatch()
        {
            if (_gameEvaluated) return;
            _gameEvaluated = true;

            // Un combate puede terminar antes del turno 3. El resultado se
            // calcula con informacion completa, asi que se revela lo que falte.
            _turnManager.RevealAllZones();
            _effectProcessor?.ResolveEndOfGame(_boardState);

            RefreshPowerTotals();

            string result = _victoryResolver.Evaluate(_boardState);

            // El resultado se aplica aqui, antes de que el modal se cierre.
            // La instantanea del combate impide aplicarlo dos veces.
            bool applied = _runDirector != null && _runDirector.ApplyCombatResult(result);
            _nextScene = applied ? _runDirector.GetNextScene() : "Summary";

            // El checkpoint se escribe antes de mostrar el modal, para que
            // cerrar la app no repita recompensas. Un reintento por vida extra
            // conserva el nodo de combate y guarda la carga consumida.
            if (applied)
            {
                if (_runDirector.GetNextNode() == RunNode.Combat)
                    _checkpoint.SavePreCombat();
                else
                    _checkpoint.SaveAfterCombat();
            }

            int playerTotal = 0;
            int rivalTotal = 0;
            foreach (var zone in _boardState.Zones)
            {
                playerTotal += zone.GetPlayerPower();
                rivalTotal += zone.GetAIPower();
            }

            string title;
            Color accent;

            bool extraLifeRepeat = applied && result != "Player" &&
                                   _runDirector.GetNextNode() == RunNode.Combat;

            if (extraLifeRepeat)
            {
                title = "VIDA EXTRA";
                accent = new Color(224, 196, 128);
            }
            else if (result == "Player")
            {
                title = "VICTORIA";
                accent = new Color(96, 196, 132);
            }
            else if (result == "AI")
            {
                title = "DERROTA";
                accent = new Color(200, 86, 70);
            }
            else
            {
                title = "EMPATE";
                accent = new Color(196, 168, 92);
            }

            string detail = extraLifeRepeat
                ? $"FIN DE LA PARTIDA. Repites contra el mismo rival. Jugador {playerTotal}  -  Rival {rivalTotal}"
                : $"FIN DE LA PARTIDA. Jugador {playerTotal}  -  Rival {rivalTotal}";

            _modal.Show(title, detail, accent);
        }

        // ------------------------------------------------------------------
        // Draw
        // ------------------------------------------------------------------

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_boardState == null || _turnManager == null) return;

            _layoutManager.CalculateLayout();
            SyncViewsIfDirty();
            UpdateViewLayout();

            spriteBatch.Begin();
            DrawBackground(spriteBatch);
            DrawTopBar(spriteBatch);

            foreach (var location in _locations)
                location.Draw(spriteBatch, _shapeDrawer, _fontService);

            DrawHoverHighlight();

            DrawTurnControls(spriteBatch);
            DrawItemBar(spriteBatch);
            DrawHand(spriteBatch);

            if (_draggedCard != null)
                DrawHandCard(spriteBatch, _draggedCard);

            DrawPileViewer(spriteBatch);
            _detailPreview.Draw(spriteBatch, _shapeDrawer, _fontService, _layoutManager);

            spriteBatch.End();

            if (_modal.IsVisible)
            {
                spriteBatch.Begin();
                _modal.Draw(spriteBatch, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
                spriteBatch.End();
            }
        }

        private void DrawPileViewer(SpriteBatch spriteBatch)
        {
            if (!_pileViewer.IsOpen) return;

            // Atenua el tablero para dejar claro que el visor captura el input.
            _shapeDrawer.DrawRectangle(_layoutManager.LocationsArea, Color.Black * 0.28f);

            var piles = _pileViewer.Side == BoardSide.Player
                ? _boardState.PlayerPiles
                : _boardState.AIPiles;

            _pileViewer.Draw(spriteBatch, _shapeDrawer, _fontService, _layoutManager, piles);
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            _background.Draw(_shapeDrawer, new Rectangle(0, 0, _layoutManager.ScreenWidth, _layoutManager.ScreenHeight));
        }

        private void DrawTopBar(SpriteBatch spriteBatch)
        {
            var topBar = _layoutManager.TopBar;
            _shapeDrawer.DrawRectangle(topBar, Color.Black * 0.35f);
            _shapeDrawer.DrawRectangle(new Rectangle(topBar.X, topBar.Bottom - 2, topBar.Width, 2), new Color(190, 142, 56) * 0.7f);

            _playerAvatar.Draw(spriteBatch, _shapeDrawer, _fontService);
            _rivalAvatar.Draw(spriteBatch, _shapeDrawer, _fontService);

            var banner = _layoutManager.StatusBanner;
            _shapeDrawer.DrawBeveledPanel(banner, new Color(14, 22, 40), new Color(120, 186, 240), 3, 12);

            var turnRect = new Rectangle(banner.X, banner.Y + 2, banner.Width, (int)(banner.Height * 0.34f));
            var runRect = new Rectangle(banner.X, turnRect.Bottom, banner.Width, (int)(banner.Height * 0.32f));
            var timerRect = new Rectangle(banner.X, runRect.Bottom, banner.Width, (int)(banner.Height * 0.30f));

            int maxTurns = TurnManager.MaxTurnsFor(_boardState);
            int displayTurn = Math.Min(_turnManager.CurrentTurn, maxTurns);
            _fontService.DrawTextCentered(spriteBatch,
                $"TURNO {displayTurn} / {maxTurns}",
                turnRect, Color.White, 0.8f);

            _fontService.DrawTextCenteredFit(spriteBatch, BuildRunStatus(), runRect,
                new Color(224, 196, 128), 0.48f, 0.28f);

            string timerText = _turnManager.CurrentPhase == TurnPhase.PlayPhase
                ? $"{(int)MathF.Ceiling(_services.Get<TimeService>().TurnTimeRemaining)}s"
                : "--";

            _fontService.DrawTextCentered(spriteBatch, timerText, timerRect, new Color(196, 214, 240), 0.68f);
        }

        /// <summary>
        /// Acto y posicion del combate, mas nivel y dinero. En el acto final
        /// se muestra el paso o el jefe concreto en vez de la posicion normal.
        /// </summary>
        private string BuildRunStatus()
        {
            int level = _runState != null ? _runState.Level : 1;
            int money = _runState != null ? _runState.Money : 0;
            string status = _hudAct >= RunActTable.FinalAct
                ? BuildFinalActStatus(level, money)
                : $"ACTO {_hudAct} - COMBATE {_hudCombatIndexInAct + 1}/" +
                  $"{RunActTable.CombatsPerAct}   NIVEL {level}   DINERO {money}";

            bool daredevil = _boardState.CurrentTurn == 5 &&
                _boardState.PlayerHand.Concat(_boardState.PlayerDeck)
                    .Any(card => card != null &&
                        SnapClone.Gameplay.Meta.PerkAbilities.HasEffect(card,
                            "effect_daredevil"));

            return daredevil
                ? status + "   DAREDEVIL: VES LAS JUGADAS RIVALES"
                : status;
        }

        private string BuildFinalActStatus(int level, int money)
        {
            string step;
            if (_hudFinalActStep >= 3)
                step = "JEFE FINAL";
            else if (_hudFinalActStep == 2)
                step = "SUBJEFE 4";
            else
                step = $"PASO {_hudFinalActStep + 1}/4";

            return $"ACTO FINAL - {step}   NIVEL {level}   DINERO {money}";
        }

        private void DrawHoverHighlight()
        {
            if (_hoverZone < 0 || _hoverSlot < 0) return;
            if (_hoverZone >= _locations.Count) return;

            var bounds = _locations[_hoverZone].PlayerSlots[_hoverSlot];
            var glow = new Color(255, 214, 128);

            _shapeDrawer.DrawRectangle(bounds, glow * 0.18f);
            _shapeDrawer.DrawRectangleBorder(bounds, glow, 3);
        }

        private void DrawTurnControls(SpriteBatch spriteBatch)
        {
            var energyBounds = _layoutManager.EnergyBadgeBounds;
            float radius = Math.Min(energyBounds.Width, energyBounds.Height) * 0.5f;
            var center = new Vector2(energyBounds.Center.X, energyBounds.Center.Y);

            _shapeDrawer.DrawHexagonFramed(center, radius,
                new Color(40, 92, 180), new Color(168, 204, 255), 3f, false);
            _fontService.DrawTextCentered(spriteBatch,
                _turnManager.PlayerAvailableEnergy.ToString(), center, Color.White, 1.0f);

            var button = _layoutManager.EndTurnButton;
            bool enabled = _turnManager.CurrentPhase == TurnPhase.PlayPhase && !_gameEvaluated;

            var fill = enabled ? new Color(30, 104, 74) : new Color(38, 52, 46);
            var border = enabled ? new Color(126, 214, 170) : new Color(96, 116, 106);

            int maxTurns = TurnManager.MaxTurnsFor(_boardState);
            int displayTurn = Math.Min(_turnManager.CurrentTurn, maxTurns);

            _shapeDrawer.DrawBeveledPanel(button, fill, border, 3, 12);
            _fontService.DrawTextCentered(spriteBatch,
                $"FINALIZAR TURNO {displayTurn}/{maxTurns}",
                button, enabled ? Color.White : new Color(190, 200, 196), 0.7f);
        }

        /// <summary>
        /// Barra compacta de objetos con icono de rareza, resumen del efecto y
        /// contador. Solo aparece en la fase del jugador y con inventario.
        /// </summary>
        private void DrawItemBar(SpriteBatch spriteBatch)
        {
            if (_itemSystem == null || _profile == null || _turnManager == null)
                return;

            if (_turnManager.CurrentPhase != TurnPhase.PlayPhase || _gameEvaluated)
                return;

            var items = _itemSystem.GetInventory(_profile);
            if (items.Count == 0) return;

            var bar = _layoutManager.ItemBar;
            _shapeDrawer.DrawRectangle(bar, new Color(10, 14, 26) * 0.88f);
            _shapeDrawer.DrawRectangleBorder(bar,
                new Color(120, 150, 210) * 0.65f, 1);

            float slotWidth = bar.Width / (float)items.Count;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var slot = new Rectangle(
                    bar.X + (int)(i * slotWidth),
                    bar.Y,
                    (int)slotWidth,
                    bar.Height);

                bool canUse = _itemSystem.CheckUse(item.Id, _profile, true).CanUse;
                bool enabled = canUse && !_itemSystem.IsBusy;

                var accent = item.Rarity == ItemRarity.Rare
                    ? new Color(214, 186, 120)
                    : new Color(142, 196, 230);

                var fill = enabled
                    ? new Color(24, 42, 64)
                    : new Color(32, 36, 44);
                var border = enabled ? accent : new Color(96, 102, 112);

                _shapeDrawer.DrawBeveledPanel(slot, fill, border, 1, 5);

                int iconSize = Math.Max(8, Math.Min(slot.Height - 6,
                    (int)(slot.Width * 0.34f)));
                var icon = new Rectangle(slot.X + 3,
                    slot.Y + (slot.Height - iconSize) / 2, iconSize, iconSize);
                _shapeDrawer.DrawRectangle(icon, border * 0.85f);

                string initial = string.IsNullOrWhiteSpace(item.Name)
                    ? "?"
                    : item.Name.Substring(0, 1).ToUpperInvariant();
                _fontService.DrawTextCentered(spriteBatch, initial, icon,
                    enabled ? Color.White : new Color(186, 190, 198),
                    Math.Max(0.25f, iconSize / (float)bar.Height * 0.8f));

                int countWidth = Math.Max(12, (int)(slot.Width * 0.28f));
                var summaryRect = new Rectangle(
                    icon.Right + 2,
                    slot.Y + 1,
                    Math.Max(1, slot.Width - iconSize - countWidth - 8),
                    slot.Height - 2);
                var countRect = new Rectangle(
                    slot.Right - countWidth - 2,
                    slot.Y + 1,
                    countWidth,
                    slot.Height - 2);

                _fontService.DrawTextCenteredFit(spriteBatch,
                    CombatItemSystem.SummaryFor(item.EffectId), summaryRect,
                    enabled ? new Color(220, 230, 244) : new Color(150, 156, 166),
                    0.34f, 0.16f);

                _fontService.DrawTextCenteredFit(spriteBatch,
                    "x" + _itemSystem.CountOf(_profile, item.Id), countRect,
                    enabled ? accent : new Color(160, 164, 172),
                    0.52f, 0.24f);
            }

            DrawItemMessage(spriteBatch);
        }

        private void DrawItemMessage(SpriteBatch spriteBatch)
        {
            if (string.IsNullOrWhiteSpace(_itemMessage)) return;

            var banner = _layoutManager.StatusBanner;
            var topBar = _layoutManager.TopBar;
            int y = banner.Bottom + 1;
            var messageRect = new Rectangle(banner.X, y, banner.Width,
                Math.Max(1, topBar.Bottom - y - 1));

            _fontService.DrawTextCenteredFit(spriteBatch, _itemMessage,
                messageRect,
                _itemMessageIsError ? new Color(244, 170, 150) : new Color(150, 220, 170),
                0.34f, 0.18f);
        }

        private void DrawHand(SpriteBatch spriteBatch)
        {
            var handArea = _layoutManager.HandArea;

            _shapeDrawer.DrawRectangle(handArea, new Color(10, 12, 22) * 0.55f);
            _shapeDrawer.DrawRectangle(new Rectangle(handArea.X, handArea.Y, handArea.Width, 2), new Color(120, 150, 210) * 0.5f);

            foreach (var view in _handViews)
            {
                if (!view.IsDragging)
                {
                    view.IsAffordable = view.Card.EffectiveCost <= _turnManager.PlayerAvailableEnergy;
                    DrawHandCard(spriteBatch, view);
                }
            }
        }

        private void DrawHandCard(SpriteBatch spriteBatch, CardView view)
        {
            var bounds = new Rectangle(
                (int)view.Position.X,
                (int)view.Position.Y,
                (int)view.Size.X,
                (int)view.Size.Y);

            if (view.Origin == CardOrigin.Hand)
                view.IsAffordable = view.Card.EffectiveCost <= _turnManager.PlayerAvailableEnergy;

            view.DrawInHand(spriteBatch, _shapeDrawer, _fontService, bounds);

            if (view.IsDragging)
                _shapeDrawer.DrawRectangleBorder(bounds, new Color(255, 214, 128), 2);
        }

        private static bool IsInside(Rectangle bounds, Vector2 point)
        {
            return point.X >= bounds.Left && point.X <= bounds.Right &&
                   point.Y >= bounds.Top && point.Y <= bounds.Bottom;
        }
    }
}
