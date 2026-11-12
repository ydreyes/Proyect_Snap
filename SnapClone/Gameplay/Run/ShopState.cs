using System.Collections.Generic;

namespace SnapClone.Gameplay.Run
{
    /// <summary>Una oferta de carta de la tienda. El precio se deriva del coste base.</summary>
    public class ShopCardOffer
    {
        public string CardId { get; set; }
        public bool Sold { get; set; }
    }

    /// <summary>
    /// Stock generado al abrir una tienda. Se guarda en la run para que la
    /// reentrada no rebaraje ofertas ni revenda lo ya comprado.
    /// </summary>
    public class ShopState
    {
        public bool Generated { get; set; }

        public List<ShopCardOffer> Stock { get; set; } = new List<ShopCardOffer>();

        /// <summary>Mejoras de carta que quedan disponibles en esta tienda.</summary>
        public int UpgradeOffers { get; set; }

        public bool ExtraLifeSold { get; set; }

        public void Normalize()
        {
            Stock ??= new List<ShopCardOffer>();
        }
    }

    /// <summary>Evento elegido para el nodo actual y si ya se resolvio.</summary>
    public class EventState
    {
        public string EventId { get; set; }
        public bool Resolved { get; set; }
    }
}
