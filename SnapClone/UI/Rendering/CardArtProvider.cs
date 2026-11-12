using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SnapClone.Data.Definitions;

namespace SnapClone.UI.Rendering
{
    /// <summary>
    /// Cache perezoso de arte de carta. Primero consulta el registro opcional
    /// de arte real; si no hay, compone y sube una textura 128x192 una sola vez.
    /// </summary>
    public class CardArtProvider : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, Texture2D> _procedural =
            new Dictionary<string, Texture2D>(StringComparer.Ordinal);
        private readonly Dictionary<string, Texture2D> _real =
            new Dictionary<string, Texture2D>(StringComparer.Ordinal);

        public const int ArtWidth = CardArtComposer.Width;
        public const int ArtHeight = CardArtComposer.Height;
        public const int BytesPerTexture = ArtWidth * ArtHeight * 4;

        public CardArtProvider(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public int Count => _procedural.Count;

        /// <summary>Bytes estimados del cache procedural (solo diagnostico).</summary>
        public long EstimatedBytes => (long)_procedural.Count * BytesPerTexture;

        /// <summary>Registra arte real opcional para una carta.</summary>
        public void RegisterRealArt(string cardId, Texture2D texture)
        {
            if (string.IsNullOrEmpty(cardId) || texture == null) return;
            _real[cardId] = texture;
        }

        public bool HasRealArt(string cardId) =>
            !string.IsNullOrEmpty(cardId) && _real.ContainsKey(cardId);

        public bool IsServedFromCache(string cardId) =>
            !string.IsNullOrEmpty(cardId) && _procedural.ContainsKey(cardId);

        public Texture2D Get(CardDef card)
        {
            if (card == null) return null;
            return Get(card.Id, card.DraftTheme, card.Cost);
        }

        public Texture2D Get(string cardId, DraftTheme? theme, int cost)
        {
            if (string.IsNullOrEmpty(cardId)) cardId = "token";

            if (_real.TryGetValue(cardId, out Texture2D real))
                return real;

            if (_procedural.TryGetValue(cardId, out Texture2D cached))
                return cached;

            byte[] bytes = CardArtComposer.Compose(cardId, theme, cost);
            var colors = new Color[ArtWidth * ArtHeight];
            for (int i = 0; i < colors.Length; i++)
            {
                int index = i * 4;
                colors[i] = new Color(bytes[index], bytes[index + 1],
                    bytes[index + 2], bytes[index + 3]);
            }

            var texture = new Texture2D(_graphicsDevice, ArtWidth, ArtHeight,
                false, SurfaceFormat.Color);
            texture.SetData(colors);
            _procedural[cardId] = texture;
            return texture;
        }

        /// <summary>
        /// Recorte centrado tipo cover: devuelve el rectangulo de origen que
        /// llena el destino sin deformar la imagen.
        /// </summary>
        public static Rectangle CropToFill(int sourceWidth, int sourceHeight,
            Rectangle destination)
        {
            if (sourceWidth <= 0 || sourceHeight <= 0)
                return new Rectangle(0, 0, 1, 1);

            float destinationAspect = destination.Width /
                (float)Math.Max(1, destination.Height);
            float sourceAspect = sourceWidth / (float)sourceHeight;

            if (sourceAspect > destinationAspect)
            {
                int width = Math.Max(1,
                    (int)MathF.Round(sourceHeight * destinationAspect));
                return new Rectangle((sourceWidth - width) / 2, 0, width,
                    sourceHeight);
            }

            int height = Math.Max(1,
                (int)MathF.Round(sourceWidth / destinationAspect));
            return new Rectangle(0, (sourceHeight - height) / 2, sourceWidth,
                height);
        }

        public void Dispose()
        {
            foreach (var texture in _procedural.Values)
                texture?.Dispose();

            _procedural.Clear();
        }
    }
}
