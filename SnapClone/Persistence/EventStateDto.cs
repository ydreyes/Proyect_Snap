namespace SnapClone.Persistence
{
    /// <summary>Evento elegido para el nodo actual dentro de run.json.</summary>
    public class EventStateDto
    {
        public string EventId { get; set; }
        public bool Resolved { get; set; }
    }
}
