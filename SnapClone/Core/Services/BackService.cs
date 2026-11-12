namespace SnapClone.Core.Services
{
    /// <summary>
    /// Peticion de "atras" de un solo frame. El host la genera en escritorio
    /// (Escape) o Android (gesto/tecla) y el juego la consume una vez.
    /// </summary>
    public class BackService
    {
        private bool _requested;

        public bool HasRequest => _requested;

        public void Request() => _requested = true;

        public bool Consume()
        {
            if (!_requested) return false;

            _requested = false;
            return true;
        }
    }
}
