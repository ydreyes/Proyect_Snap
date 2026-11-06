using Microsoft.Xna.Framework;

namespace SnapClone.Core.Services
{
    /// <summary>
    /// Informa el area no dibujable del dispositivo (notch, barras del sistema,
    /// zona de gestos). Desktop no registra implementacion y usa cero insets.
    /// </summary>
    public interface ISafeAreaProvider
    {
        /// <summary>Insets en pixeles: X=izquierda, Y=arriba, Width=derecha, Height=abajo.</summary>
        Rectangle GetInsets();
    }
}
