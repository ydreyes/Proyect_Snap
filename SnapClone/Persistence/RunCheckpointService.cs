using System;
using System.Collections.Generic;
using System.Linq;
using SnapClone.Core.Services;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Draft;
using SnapClone.Gameplay.Meta;
using SnapClone.Gameplay.Run;

namespace SnapClone.Persistence
{
    public readonly struct SaveResult
    {
        public bool RunSaved { get; }
        public bool ProfileSaved { get; }
        public bool Skipped { get; }
        public string Error { get; }

        private SaveResult(bool runSaved, bool profileSaved, bool skipped, string error)
        {
            RunSaved = runSaved;
            ProfileSaved = profileSaved;
            Skipped = skipped;
            Error = error;
        }

        public static SaveResult Ok(bool profileSaved, string error) =>
            new SaveResult(true, profileSaved, false, error);

        public static SaveResult ProfileOnly(bool saved, string error) =>
            new SaveResult(false, saved, false, error);

        public static SaveResult Fail(string error) =>
            new SaveResult(false, false, false, error);

        public static SaveResult Skip() =>
            new SaveResult(false, false, true, null);
    }

    /// <summary>
    /// Coordina run.json, profile.json, el estado en memoria y el volcado de
    /// descubrimientos. Es el unico punto que escribe checkpoints.
    /// </summary>
    public class RunCheckpointService
    {
        private readonly RunState _run;
        private readonly DraftSystem _draft;
        private readonly JsonRunStore _runStore;
        private readonly JsonProfileStore _profileStore;
        private readonly CardRegistry _cards;
        private readonly RandomService _random;
        private readonly GlobalStoreSystem _store;

        private ProfileDto _profile;
        private RunDto _loadedRun;

        public RunCheckpointService(RunState runState, DraftSystem draftSystem,
            JsonRunStore runStore, JsonProfileStore profileStore,
            CardRegistry cardRegistry, RandomService randomService,
            GlobalStoreSystem storeSystem = null)
        {
            _run = runState;
            _draft = draftSystem;
            _runStore = runStore;
            _profileStore = profileStore;
            _cards = cardRegistry;
            _random = randomService;
            _store = storeSystem;
        }

        public ProfileDto Profile => _profile;

        public RunDto LoadedRun => _loadedRun;

        public bool HasRun => _loadedRun != null;

        public bool HasActiveRun => _loadedRun != null && _loadedRun.Status == RunStatus.Active;

        public bool HasEndedRun => _loadedRun != null && _loadedRun.Status == RunStatus.Ended;

        public void Initialize()
        {
            ValidateStarterSet();
            _profile = _profileStore.Load();

            if (!_profileStore.Exists())
            {
                _profile = ProfileDto.CreateDefault(StarterSet.CardIds);
                try
                {
                    _profileStore.Save(_profile);
                }
                catch
                {
                    // El perfil por defecto vive en memoria aunque el disco falle.
                }
            }

            RefreshRun();
        }

        public void RefreshRun()
        {
            _loadedRun = _runStore.Load();
        }

        /// <summary>
        /// El set inicial debe ser de 21 ids unicos y existir en el catalogo.
        /// Falla antes de crear un perfil que pueda quedar con cartas invalidas.
        /// </summary>
        private void ValidateStarterSet()
        {
            if (_cards == null) return;

            var errors = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);

            if (StarterSet.CardIds == null || StarterSet.CardIds.Length != 21)
                errors.Add("El set inicial debe tener exactamente 21 cartas.");

            foreach (string id in StarterSet.CardIds ?? new string[0])
            {
                if (string.IsNullOrWhiteSpace(id) || !ids.Add(id))
                {
                    errors.Add($"{id}: id inicial vacio o duplicado.");
                    continue;
                }

                if (_cards.Get(id) == null)
                    errors.Add($"{id}: starter no registrado en el catalogo.");
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Starter set invalido:\n" + string.Join("\n", errors));
            }
        }

        public static bool IsSafeNode(RunNode node)
        {
            switch (node)
            {
                case RunNode.Draft:
                case RunNode.EnemySelect:
                case RunNode.LevelUp:
                case RunNode.Shop:
                case RunNode.Event:
                case RunNode.ActTransition:
                case RunNode.SubBossIntro:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsCombatNode => _run.PendingNode == RunNode.Combat;

        public SaveResult StartNewRun()
        {
            _run.StartNewRun();
            _run.PendingNode = RunNode.Draft;

            // Rotacion de lotes y consumo de la vida extra del perfil.
            _store?.PrepareNewRun(_profile, _run);

            _draft.Reset();
            var offers = _draft.GetChoices();
            foreach (var offer in offers)
                _run.DiscoverCard(offer.Definition?.Id);

            return SaveRun();
        }

        public SaveResult SaveSafeNode()
        {
            if (!IsSafeNode(_run.PendingNode) && _run.Outcome == RunOutcome.Active)
                return SaveResult.Skip();

            return SaveRun();
        }

        public SaveResult SavePreCombat()
        {
            if (_run.PendingCombat == null)
                return SaveResult.Fail("No hay combate pendiente para guardar.");

            return SaveRun();
        }

        public SaveResult SaveAfterCombat()
        {
            // La run solo se escribe fuera de combate. Si el resultado dejo
            // otro combate pendiente, se conserva la instantanea previa.
            if (_run.PendingNode == RunNode.Combat && _run.Outcome == RunOutcome.Active)
                return SaveResult.Skip();

            return SaveRun();
        }

        public SaveResult SaveDraftCheckpoint()
        {
            if (_run.PendingNode != RunNode.Draft)
                return SaveResult.Skip();

            return SaveRun();
        }

        public SaveResult SaveOnClose()
        {
            if (_run.PendingNode == RunNode.Combat)
            {
                bool profileSaved = FlushProfile(out string error);
                return SaveResult.ProfileOnly(profileSaved, error);
            }

            return SaveSafeNode();
        }

        public SaveResult FlushProfileOnly()
        {
            bool saved = FlushProfile(out string error);
            return SaveResult.ProfileOnly(saved, error);
        }

        /// <summary>
        /// Guarda solo el perfil y devuelve si la escritura termino bien. La
        /// usan las compras de la tienda global y el cobro del resumen para no
        /// dar por aplicado un cambio que no llego al disco.
        /// </summary>
        public bool SaveProfile(out string error) => FlushProfile(out error);

        public void DeleteRun()
        {
            _runStore.Delete();
            _loadedRun = null;
        }

        public bool RestoreRun()
        {
            if (_loadedRun == null) return false;

            ApplyDto(_loadedRun);
            return true;
        }

        /// <summary>
        /// Mantiene al mismo rival y numero de intento, pero cambia la semilla
        /// del tablero para reanudar un combate interrumpido.
        /// </summary>
        public bool RestoreCombatForResume()
        {
            if (_run.PendingNode != RunNode.Combat || _run.PendingCombat == null)
                return false;

            _run.PendingCombat.Seed = NewSeed();
            return true;
        }

        private int NewSeed()
        {
            int seed = _random.Next(1, int.MaxValue);
            return seed == 0 ? 1 : seed;
        }

        private SaveResult SaveRun()
        {
            var dto = MapToDto();

            try
            {
                _runStore.Save(dto);
                _loadedRun = dto;
            }
            catch (Exception ex)
            {
                return SaveResult.Fail("No se pudo guardar la run: " + ex.Message);
            }

            bool profileSaved = FlushProfile(out string profileError);
            return SaveResult.Ok(profileSaved, profileError);
        }

        private bool FlushProfile(out string error)
        {
            error = null;

            if (_profile == null)
            {
                error = "No hay perfil cargado.";
                return false;
            }

            try
            {
                MergeSet(_profile.DiscoveredCardIds, _run.DiscoveredCardIds);
                MergeSet(_profile.DiscoveredPerkIds, _run.DiscoveredPerkIds);
                MergeSet(_profile.DiscoveredEnemyIds, _run.DiscoveredEnemyIds);
                _profileStore.Save(_profile);
                return true;
            }
            catch (Exception ex)
            {
                error = "No se pudo guardar el perfil: " + ex.Message;
                return false;
            }
        }

        private static void MergeSet(List<string> target, HashSet<string> source)
        {
            var merged = new HashSet<string>(target, StringComparer.Ordinal);
            foreach (string id in source)
            {
                if (!string.IsNullOrWhiteSpace(id))
                    merged.Add(id);
            }

            target.Clear();
            target.AddRange(merged.OrderBy(id => id, StringComparer.Ordinal));
        }

        private RunDto MapToDto()
        {
            var dto = new RunDto
            {
                SchemaVersion = RunDto.CurrentSchemaVersion,
                Status = _run.Outcome == RunOutcome.Active
                    ? RunStatus.Active
                    : RunStatus.Ended,
                RunId = _run.EnsureRunId(),
                ActIndex = _run.ActIndex,
                CombatIndexInAct = _run.CombatIndexInAct,
                TotalCombats = _run.TotalCombats,
                NormalCombatsWon = _run.NormalCombatsWon,
                SubBossesWon = _run.SubBossesWon,
                FinalBossDefeated = _run.FinalBossDefeated,
                ShopEventChance = _run.ShopEventChance,
                FinalActStep = _run.FinalActStep,
                PendingNode = _run.PendingNode,
                Xp = _run.Xp,
                Level = _run.Level,
                Money = _run.Money,
                XpEarned = _run.XpEarned,
                MoneyEarned = _run.MoneyEarned,
                CombatsWon = _run.CombatsWon,
                Outcome = _run.Outcome,
                Deck = _run.Deck.Select(ToCardDto).ToList(),
                PendingLevelUps = _run.PendingLevelUps.Select(ToLevelUpDto).ToList(),
                Draft = _run.PendingNode == RunNode.Draft ? ToDraftDto(_draft.State) : null,
                PendingCombat = _run.PendingCombat != null
                    ? ToPendingCombatDto(_run.PendingCombat)
                    : null,
                RematchCombat = _run.RematchCombat != null
                    ? ToPendingCombatDto(_run.RematchCombat)
                    : null,
                RunModifiers = _run.RunModifiers
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList(),
                ExtraLifeCharges = _run.ExtraLifeCharges,
                Shop = ToShopStateDto(_run.ShopState),
                Event = ToEventStateDto(_run.EventState),
                SeenEventIds = _run.SeenEventIds
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList()
            };

            return dto;
        }

        private void ApplyDto(RunDto dto)
        {
            dto.Normalize();

            _run.RunId = string.IsNullOrWhiteSpace(dto.RunId)
                ? Guid.NewGuid().ToString("N")
                : dto.RunId;
            _run.ActIndex = dto.ActIndex;
            _run.CombatIndexInAct = dto.CombatIndexInAct;
            _run.TotalCombats = dto.TotalCombats;
            _run.NormalCombatsWon = dto.NormalCombatsWon;
            _run.SubBossesWon = dto.SubBossesWon;
            _run.FinalBossDefeated = dto.FinalBossDefeated;
            _run.ShopEventChance = dto.ShopEventChance;
            _run.FinalActStep = dto.FinalActStep;
            _run.PendingNode = dto.PendingNode;
            _run.Xp = dto.Xp;
            _run.Level = dto.Level;
            _run.Money = dto.Money;
            _run.XpEarned = dto.XpEarned;
            _run.MoneyEarned = dto.MoneyEarned;
            _run.CombatsWon = dto.CombatsWon;
            _run.Outcome = dto.Outcome;

            _run.RunModifiers.Clear();
            foreach (string modifierId in dto.RunModifiers)
            {
                if (!string.IsNullOrWhiteSpace(modifierId))
                    _run.RunModifiers.Add(modifierId);
            }

            _run.ExtraLifeCharges = dto.ExtraLifeCharges;
            _run.ShopState = FromShopStateDto(dto.Shop);
            _run.EventState = FromEventStateDto(dto.Event);

            _run.SeenEventIds.Clear();
            foreach (string eventId in dto.SeenEventIds)
            {
                if (!string.IsNullOrWhiteSpace(eventId))
                    _run.SeenEventIds.Add(eventId);
            }

            _run.Deck.Clear();
            foreach (var cardDto in dto.Deck)
            {
                var card = FromCardDto(cardDto);
                if (card != null)
                    _run.Deck.Add(card);
            }

            _run.PendingLevelUps.Clear();
            foreach (var levelDto in dto.PendingLevelUps)
                _run.PendingLevelUps.Add(FromLevelUpDto(levelDto, _run.Deck));

            _run.PendingCombat = dto.PendingCombat != null
                ? FromPendingCombatDto(dto.PendingCombat)
                : null;
            _run.RematchCombat = dto.RematchCombat != null
                ? FromPendingCombatDto(dto.RematchCombat)
                : null;

            _run.DiscoveredCardIds.Clear();
            _run.DiscoveredPerkIds.Clear();
            _run.DiscoveredEnemyIds.Clear();

            if (dto.PendingNode == RunNode.Draft)
            {
                if (dto.Draft != null)
                {
                    _draft.Load(FromDraftDto(dto.Draft));
                }
                else
                {
                    _draft.Reset();
                    _draft.GetChoices();
                }
            }
            else
            {
                _draft.Reset();
            }
        }

        private static ShopStateDto ToShopStateDto(ShopState shop)
        {
            if (shop == null) return null;

            shop.Normalize();
            return new ShopStateDto
            {
                Generated = shop.Generated,
                Stock = shop.Stock.Select(offer => new ShopCardOfferDto
                {
                    CardId = offer.CardId,
                    Sold = offer.Sold
                }).ToList(),
                UpgradeOffers = shop.UpgradeOffers,
                ExtraLifeSold = shop.ExtraLifeSold
            };
        }

        private static ShopState FromShopStateDto(ShopStateDto dto)
        {
            var shop = new ShopState();
            if (dto == null) return shop;

            dto.Normalize();
            shop.Generated = dto.Generated;
            shop.UpgradeOffers = dto.UpgradeOffers;
            shop.ExtraLifeSold = dto.ExtraLifeSold;

            foreach (var offerDto in dto.Stock)
            {
                if (offerDto == null || string.IsNullOrWhiteSpace(offerDto.CardId))
                    continue;

                shop.Stock.Add(new ShopCardOffer
                {
                    CardId = offerDto.CardId,
                    Sold = offerDto.Sold
                });
            }

            return shop;
        }

        private static EventStateDto ToEventStateDto(EventState state)
        {
            if (state == null) return null;

            return new EventStateDto
            {
                EventId = state.EventId,
                Resolved = state.Resolved
            };
        }

        private static EventState FromEventStateDto(EventStateDto dto)
        {
            if (dto == null) return new EventState();

            return new EventState
            {
                EventId = dto.EventId,
                Resolved = dto.Resolved
            };
        }

        private static CardSnapshotDto ToCardDto(CardInstance card)
        {
            return new CardSnapshotDto
            {
                DefinitionId = card.Definition?.Id,
                CostDelta = card.CostDelta,
                PowerDelta = card.PowerDelta,
                UpgradeCount = card.UpgradeCount,
                AttachedPerks = new List<string>(card.AttachedPerks ?? new List<string>()),
                BonusPower = card.BonusPower
            };
        }

        private CardInstance FromCardDto(CardSnapshotDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.DefinitionId))
                return null;

            var definition = _cards.Get(dto.DefinitionId);
            if (definition == null)
                return null;

            var card = new CardInstance(definition)
            {
                CostDelta = dto.CostDelta,
                PowerDelta = dto.PowerDelta,
                UpgradeCount = dto.UpgradeCount,
                BonusPower = dto.BonusPower,
                CurrentPower = definition.BasePower + dto.PowerDelta + dto.BonusPower
            };

            // Migracion: los perks retirados desaparecen de la carta; sus
            // mejoras, deltas y el nodo de la run se conservan.
            card.AttachedPerks = new List<string>();
            foreach (string perkId in dto.AttachedPerks ?? new List<string>())
            {
                if (PerkCatalog.IsKnown(perkId))
                    card.AttachedPerks.Add(perkId);
            }

            return card;
        }

        private LevelUpDto ToLevelUpDto(LevelUpState state)
        {
            var dto = new LevelUpDto
            {
                Level = state.Level,
                Branch = state.Branch,
                OffersGenerated = state.OffersGenerated,
                CardOffers = state.CardOffers.Select(ToCardDto).ToList(),
                PerkOffers = new List<string>(state.PerkOffers)
            };

            foreach (var candidate in state.UpgradeCandidates)
            {
                int index = _run.Deck.IndexOf(candidate);
                if (index >= 0)
                    dto.UpgradeCandidateIndices.Add(index);
            }

            return dto;
        }

        private LevelUpState FromLevelUpDto(LevelUpDto dto, List<CardInstance> deck)
        {
            dto.Normalize();

            var state = new LevelUpState(dto.Level)
            {
                Branch = dto.Branch,
                OffersGenerated = dto.OffersGenerated
            };

            foreach (var candidate in dto.CardOffers)
            {
                var card = FromCardDto(candidate);
                if (card != null)
                    state.CardOffers.Add(card);
            }

            foreach (int index in dto.UpgradeCandidateIndices)
            {
                if (index >= 0 && index < deck.Count)
                    state.UpgradeCandidates.Add(deck[index]);
            }

            state.PerkOffers.AddRange(dto.PerkOffers ?? new List<string>());
            return state;
        }

        private static DraftDto ToDraftDto(DraftState state)
        {
            state.Normalize();

            return new DraftDto
            {
                Phase = state.Phase,
                StepIndex = state.StepIndex,
                Groups = state.Groups.Select(group => new List<string>(group)).ToList(),
                GroupFilled = new List<bool>(state.GroupFilled),
                CurrentOffers = new List<string>(state.CurrentOffers),
                Picks = new List<string>(state.Picks),
                CopyCounts = new Dictionary<string, int>(state.CopyCounts)
            };
        }

        private static DraftState FromDraftDto(DraftDto dto)
        {
            dto.Normalize();

            var state = new DraftState
            {
                Phase = dto.Phase,
                StepIndex = dto.StepIndex,
                Groups = dto.Groups.Select(group => new List<string>(group)).ToList(),
                GroupFilled = new List<bool>(dto.GroupFilled),
                CurrentOffers = new List<string>(dto.CurrentOffers),
                Picks = new List<string>(dto.Picks),
                CopyCounts = new Dictionary<string, int>(dto.CopyCounts)
            };

            state.Normalize();
            return state;
        }

        private static PendingCombatDto ToPendingCombatDto(PendingCombat combat)
        {
            return new PendingCombatDto
            {
                EnemyId = combat.EnemyId,
                EnemyName = combat.EnemyName,
                EnemyPhrase = combat.EnemyPhrase,
                Difficulty = combat.Difficulty,
                Attempt = combat.Attempt,
                Seed = combat.Seed,
                CompositionSeed = combat.CompositionSeed,
                PreCombat = ToSnapshotDto(combat.PreCombat)
            };
        }

        private static PendingCombat FromPendingCombatDto(PendingCombatDto dto)
        {
            return new PendingCombat
            {
                EnemyId = dto.EnemyId,
                EnemyName = dto.EnemyName,
                EnemyPhrase = dto.EnemyPhrase,
                Difficulty = dto.Difficulty,
                Attempt = dto.Attempt,
                Seed = dto.Seed,
                CompositionSeed = dto.CompositionSeed,
                PreCombat = FromSnapshotDto(dto.PreCombat)
            };
        }

        private static RunSnapshotDto ToSnapshotDto(RunSnapshot snapshot)
        {
            if (snapshot == null) return null;

            return new RunSnapshotDto
            {
                ActIndex = snapshot.ActIndex,
                CombatIndexInAct = snapshot.CombatIndexInAct,
                TotalCombats = snapshot.TotalCombats,
                Xp = snapshot.Xp,
                Level = snapshot.Level,
                Money = snapshot.Money,
                XpEarned = snapshot.XpEarned,
                MoneyEarned = snapshot.MoneyEarned,
                CombatsWon = snapshot.CombatsWon,
                ShopEventChance = snapshot.ShopEventChance,
                FinalActStep = snapshot.FinalActStep,
                PendingNode = snapshot.PendingNode,
                Outcome = snapshot.Outcome,
                DeckCount = snapshot.DeckCount
            };
        }

        private static RunSnapshot FromSnapshotDto(RunSnapshotDto dto)
        {
            if (dto == null) return null;

            return new RunSnapshot
            {
                ActIndex = dto.ActIndex,
                CombatIndexInAct = dto.CombatIndexInAct,
                TotalCombats = dto.TotalCombats,
                Xp = dto.Xp,
                Level = dto.Level,
                Money = dto.Money,
                XpEarned = dto.XpEarned,
                MoneyEarned = dto.MoneyEarned,
                CombatsWon = dto.CombatsWon,
                ShopEventChance = dto.ShopEventChance,
                FinalActStep = dto.FinalActStep,
                PendingNode = dto.PendingNode,
                Outcome = dto.Outcome,
                DeckCount = dto.DeckCount
            };
        }
    }
}
