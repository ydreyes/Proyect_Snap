using SnapClone.Gameplay.Board;
using SnapClone.Gameplay.Cards;

namespace SnapClone.Gameplay.Actions
{
    public enum ActionEventKind
    {
        Played,
        Moved,
        Destroyed,
        Discarded,
        Drawn,
        AddedToHand,
        GameStart,
        Empowered,
        Activated
    }

    public enum ActionCause
    {
        Manual,
        Effect
    }

    /// <summary>
    /// Evento emitido por el servicio de acciones. El contexto de efecto lo
    /// consume para resolver disparadores que no son OnReveal.
    /// </summary>
    public class ActionEvent
    {
        public ActionEventKind Kind { get; set; }
        public CardInstance SourceCard { get; set; }
        public BoardSide Side { get; set; }
        public int OriginZoneIndex { get; set; } = -1;
        public int DestinationZoneIndex { get; set; } = -1;
        public CardLocation OriginLocation { get; set; } = CardLocation.Board;
        public CardLocation DestinationLocation { get; set; } = CardLocation.Board;
        public int Turn { get; set; }
        public int PlayOrder { get; set; }
        public int Amount { get; set; }
        public ActionCause Cause { get; set; } = ActionCause.Effect;
    }

    public class ActionResult
    {
        public bool Success { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int EventsDispatched { get; set; }
        public bool HitEventLimit { get; set; }

        public static ActionResult Fail(string reason) => new ActionResult
        {
            Success = false,
            Reason = reason
        };

        public static ActionResult Ok(int events = 0, bool hitLimit = false) => new ActionResult
        {
            Success = true,
            EventsDispatched = events,
            HitEventLimit = hitLimit
        };
    }

    public class PlayResult : ActionResult
    {
        public CardInstance Card { get; set; }
        public BoardSide Side { get; set; }
        public int ZoneIndex { get; set; } = -1;
        public int Slot { get; set; } = -1;
        public int PlayedTurn { get; set; }
        public int PlayOrder { get; set; }
    }

    public class DrawResult : ActionResult
    {
        public CardInstance Card { get; set; }
        public BoardSide Side { get; set; }
    }

    /// <summary>
    /// Registro de una jugada desde la mano. Se usa para resolver condiciones
    /// del tipo "si el rival jugo una carta aqui este turno".
    /// </summary>
    public class PlayRecord
    {
        public BoardSide Side { get; set; }
        public CardInstance Card { get; set; }
        public int ZoneIndex { get; set; }
        public int Turn { get; set; }
        public int PlayOrder { get; set; }
    }

    public class CardMoveRequest
    {
        public CardInstance Card { get; set; }
        public BoardSide Side { get; set; }
        public int FromZone { get; set; }
        public int ToZone { get; set; }
    }
}
