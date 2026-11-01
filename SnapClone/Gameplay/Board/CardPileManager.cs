using SnapClone.Gameplay.Cards;
using SnapClone.Gameplay.Run;

namespace SnapClone.Gameplay.Board
{
    public enum BoardSide
    {
        Player,
        AI
    }

    /// <summary>
    /// Unica puerta para mover una carta a una pila. Quita la carta de la mano,
    /// el mazo o la zona del lado indicado, limpia su estado de combate y, si
    /// se destierra, la saca del mazo de la run.
    /// </summary>
    public class CardPileManager
    {
        private readonly BoardState _boardState;
        private readonly RunState _runState;

        public CardPileManager(BoardState boardState, RunState runState = null)
        {
            _boardState = boardState;
            _runState = runState;
        }

        public BoardState BoardState => _boardState;

        public bool Move(CardInstance card, BoardSide side, PileKind pile)
        {
            if (card == null || _boardState == null) return false;
            if (IsInPile(card)) return false;

            bool inHand = HandFor(side).Contains(card);
            bool inDeck = DeckFor(side).Contains(card);
            bool onBoard = IsOnBoard(card, side);

            if (!inHand && !inDeck && !onBoard) return false;
            if (!IsOriginAllowed(pile, inHand, inDeck, onBoard)) return false;

            HandFor(side).Remove(card);
            DeckFor(side).Remove(card);
            RemoveFromZones(card, side);

            card.IsInPlay = false;
            card.PlayedTurn = 0;
            card.PlayOrderIndex = 0;

            if (!_boardState.GetPiles(side).Add(card, pile)) return false;

            card.Location = pile.ToLocation();

            if (pile == PileKind.Banished && side == BoardSide.Player && _runState != null)
                _runState.Deck.Remove(card);

            _boardState.Version++;
            return true;
        }

        private static bool IsOriginAllowed(PileKind pile, bool inHand, bool inDeck, bool onBoard)
        {
            switch (pile)
            {
                case PileKind.Discard:
                    return inHand || inDeck;
                case PileKind.Destroyed:
                    return onBoard;
                case PileKind.Banished:
                    return inHand || inDeck || onBoard;
                default:
                    return false;
            }
        }

        private bool IsInPile(CardInstance card) =>
            _boardState.PlayerPiles.Contains(card) || _boardState.AIPiles.Contains(card);

        private bool IsOnBoard(CardInstance card, BoardSide side)
        {
            foreach (var zone in _boardState.Zones)
            {
                var cards = side == BoardSide.Player ? zone.PlayerCards : zone.AICards;
                if (cards.Contains(card)) return true;
            }

            return false;
        }

        private void RemoveFromZones(CardInstance card, BoardSide side)
        {
            foreach (var zone in _boardState.Zones)
            {
                if (side == BoardSide.Player)
                    zone.PlayerCards.Remove(card);
                else
                    zone.AICards.Remove(card);
            }
        }

        private System.Collections.Generic.List<CardInstance> HandFor(BoardSide side) =>
            side == BoardSide.Player ? _boardState.PlayerHand : _boardState.AIHand;

        private System.Collections.Generic.List<CardInstance> DeckFor(BoardSide side) =>
            side == BoardSide.Player ? _boardState.PlayerDeck : _boardState.AIDeck;
    }
}
