using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.UI.Components;
using SnapClone.UI.Rendering;

namespace SnapClone.UI
{
    /// <summary>
    /// Agrupa los servicios visuales de larga vida que comparten las siete
    /// pantallas. Se crea una sola vez en Game1 y se registra en GameServices.
    /// </summary>
    public sealed class UiContext : IDisposable
    {
        public TextureCache Textures { get; }
        public FontService Fonts { get; }
        public ChromeRenderer Chrome { get; }
        public CardArtProvider CardArt { get; }
        public CardRenderer Cards { get; }

        public UiContext(GraphicsDevice graphicsDevice, ContentManager content)
        {
            Textures = new TextureCache(graphicsDevice);
            Fonts = new FontService(content);
            Chrome = new ChromeRenderer(Textures, Fonts);
            CardArt = new CardArtProvider(graphicsDevice);
            Cards = new CardRenderer(Textures, Chrome, CardArt);
        }

        /// <summary>Fondo sembrado por pantalla para que no se vea congelado.</summary>
        public SpaceBackground NewBackground(int seed) => new SpaceBackground(seed);

        public void Dispose()
        {
            CardArt?.Dispose();
            Fonts?.Dispose();
            Textures?.Dispose();
        }
    }
}
