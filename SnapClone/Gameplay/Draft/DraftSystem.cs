using System;
using System.Collections.Generic;
using SnapClone.Gameplay.Cards;
using SnapClone.Data.Definitions;
using SnapClone.Data.Registries;
using SnapClone.Core.Services;

namespace SnapClone.Gameplay.Draft
{
    /// <summary>
    /// Borrador de 8 cartas: tres rondas tematicas (Control, Destroy y
    /// OnReveal) y cinco rondas aleatorias sobre el pool propio. Cada carta
    /// puede elegirse una sola vez durante el draft. Los grupos se generan
    /// una vez y se guardan en el estado para reanudar sin rebarajar.
    /// </summary>
    public class DraftSystem
    {
        public const int TotalPicks = 8;
        public const int ThemedPicks = 3;
        public const int MaxCopies = 1;
        public const int ChoicesPerRound = 3;

        private static readonly DraftTheme[] ThemedOrder =
        {
            DraftTheme.Control,
            DraftTheme.Destroy,
            DraftTheme.OnReveal
        };

        private readonly CardRegistry _cardRegistry;
        private readonly RandomService _randomService;
        private readonly Func<IEnumerable<string>> _ownedCardIds;
        private DraftState _state = new DraftState();

        public DraftSystem(CardRegistry cardRegistry, RandomService randomService,
            Func<IEnumerable<string>> ownedCardIds = null)
        {
            _cardRegistry = cardRegistry;
            _randomService = randomService;
            _ownedCardIds = ownedCardIds;
        }

        public DraftState State => _state;

        public int PicksMade => _state.Picks.Count;
        public int RemainingPicks => TotalPicks - PicksMade;

        /// <summary>Tema de la ronda actual, o null en la fase aleatoria.</summary>
        public DraftTheme? CurrentTheme =>
            _state.Phase == DraftPhase.Themed && _state.StepIndex < ThemedPicks
                ? ThemedOrder[_state.StepIndex]
                : (DraftTheme?)null;

        public bool IsDraftComplete() =>
            PicksMade >= TotalPicks || _state.Phase == DraftPhase.Complete;

        public void Reset()
        {
            _state = new DraftState();
        }

        public void Load(DraftState state)
        {
            _state = state ?? new DraftState();
            _state.Normalize();
        }

        /// <summary>
        /// Genera los tres grupos si el estado es nuevo. No hace nada cuando
        /// el checkpoint ya los trae: reanudar nunca rebaraja.
        /// </summary>
        public void EnsureGroups()
        {
            _state.Normalize();
            if (_state.Groups.Count == 3)
                return;

            var owned = GetOwnedDraftable();
            var used = new HashSet<string>(StringComparer.Ordinal);
            var groups = new List<List<string>>(ThemedPicks);
            var filled = new List<bool>(ThemedPicks);

            for (int themeIndex = 0; themeIndex < ThemedOrder.Length; themeIndex++)
            {
                var theme = ThemedOrder[themeIndex];
                var candidates = new List<CardDef>();
                foreach (var card in owned)
                {
                    if (card.DraftTheme == theme && !used.Contains(card.Id))
                        candidates.Add(card);
                }

                var chosen = PickDistinct(candidates, ChoicesPerRound);
                bool needsFill = chosen.Count < ChoicesPerRound;

                if (needsFill)
                    FillFromNeutral(owned, used, chosen);
                if (chosen.Count < ChoicesPerRound)
                    FillFromAny(owned, used, chosen);

                var group = new List<string>(chosen.Count);
                foreach (var card in chosen)
                {
                    group.Add(card.Id);
                    used.Add(card.Id);
                }

                groups.Add(group);
                filled.Add(needsFill);
            }

            _state.Groups = groups;
            _state.GroupFilled = filled;
        }

        public void EnsureOffers()
        {
            if (IsDraftComplete()) return;

            EnsureGroups();
            if (_state.CurrentOffers.Count == 0)
                DrawOffers();
        }

        /// <summary>
        /// Devuelve las 3 ofertas actuales. Si no hay, las genera y las deja
        /// guardadas en el estado para que el checkpoint no las rebaraje.
        /// </summary>
        public List<CardInstance> GetChoices()
        {
            var choices = new List<CardInstance>();
            if (IsDraftComplete())
                return choices;

            EnsureOffers();

            foreach (string id in _state.CurrentOffers)
            {
                var definition = _cardRegistry.Get(id);
                if (definition != null)
                    choices.Add(new CardInstance(definition));
            }

            return choices;
        }

        public bool PickCard(CardInstance card)
        {
            if (card?.Definition == null) return false;
            if (IsDraftComplete()) return false;

            string cardId = card.Definition.Id;
            if (!_state.CurrentOffers.Contains(cardId))
                return false;

            int currentCopies = GetCopyCount(cardId);
            if (currentCopies >= MaxCopies)
                return false;

            _state.Picks.Add(cardId);
            _state.CopyCounts[cardId] = currentCopies + 1;
            _state.StepIndex++;
            _state.CurrentOffers.Clear();

            if (PicksMade >= TotalPicks)
            {
                _state.Phase = DraftPhase.Complete;
            }
            else
            {
                _state.Phase = _state.StepIndex < ThemedPicks
                    ? DraftPhase.Themed
                    : DraftPhase.Random;
                DrawOffers();
            }

            return true;
        }

        public int GetCopyCount(string cardId) =>
            _state.CopyCounts.TryGetValue(cardId, out int copies) ? copies : 0;

        public List<CardInstance> GetDraftedCards()
        {
            var cards = new List<CardInstance>();
            foreach (string id in _state.Picks)
            {
                var definition = _cardRegistry.Get(id);
                if (definition != null)
                    cards.Add(new CardInstance(definition));
            }

            return cards;
        }

        private void DrawOffers()
        {
            _state.CurrentOffers.Clear();

            if (_state.StepIndex < ThemedPicks && _state.Groups.Count == ThemedPicks)
            {
                var group = _state.Groups[_state.StepIndex];
                foreach (string id in group)
                {
                    if (GetCopyCount(id) < MaxCopies && !_state.CurrentOffers.Contains(id))
                        _state.CurrentOffers.Add(id);
                }

                // Un grupo incompleto se termina con el pool propio restante.
                if (_state.CurrentOffers.Count < ChoicesPerRound)
                    FillOffersFromPool();
                return;
            }

            FillOffersFromPool();
        }

        private void FillOffersFromPool()
        {
            var pool = new List<CardDef>();
            foreach (var definition in GetOwnedDraftable())
            {
                if (GetCopyCount(definition.Id) < MaxCopies &&
                    !_state.CurrentOffers.Contains(definition.Id))
                    pool.Add(definition);
            }

            int count = ChoicesPerRound - _state.CurrentOffers.Count;
            foreach (var card in PickDistinct(pool, count))
                _state.CurrentOffers.Add(card.Id);
        }

        /// <summary>Pool propio: perfil interseccion catalogo draftable.</summary>
        private List<CardDef> GetOwnedDraftable()
        {
            var owned = _ownedCardIds?.Invoke();
            HashSet<string> allowed = owned == null
                ? null
                : new HashSet<string>(owned, StringComparer.Ordinal);

            var result = new List<CardDef>();
            foreach (var card in _cardRegistry.GetDraftable())
            {
                if (allowed == null || allowed.Contains(card.Id))
                    result.Add(card);
            }

            return result;
        }

        private List<CardDef> PickDistinct(List<CardDef> candidates, int count)
        {
            var left = new List<CardDef>(candidates);
            var chosen = new List<CardDef>();

            while (chosen.Count < count && left.Count > 0)
            {
                int index = _randomService.Next(left.Count);
                chosen.Add(left[index]);
                left.RemoveAt(index);
            }

            return chosen;
        }

        private void FillFromNeutral(List<CardDef> owned,
            HashSet<string> used, List<CardDef> chosen)
        {
            var fillers = new List<CardDef>();
            foreach (var card in owned)
            {
                if (card.DraftTheme == DraftTheme.Neutral &&
                    !used.Contains(card.Id) && !Contains(chosen, card.Id))
                    fillers.Add(card);
            }

            chosen.AddRange(PickDistinct(fillers, ChoicesPerRound - chosen.Count));
        }

        private void FillFromAny(List<CardDef> owned,
            HashSet<string> used, List<CardDef> chosen)
        {
            var fillers = new List<CardDef>();
            foreach (var card in owned)
            {
                if (!used.Contains(card.Id) && !Contains(chosen, card.Id))
                    fillers.Add(card);
            }

            chosen.AddRange(PickDistinct(fillers, ChoicesPerRound - chosen.Count));
        }

        private static bool Contains(List<CardDef> cards, string id)
        {
            foreach (var card in cards)
            {
                if (string.Equals(card.Id, id, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }
    }
}
