using System.Collections.Generic;

namespace SnapClone.Persistence
{
    public class ShopCardOfferDto
    {
        public string CardId { get; set; }
        public bool Sold { get; set; }
    }

    /// <summary>Documento del stock de la tienda dentro de run.json.</summary>
    public class ShopStateDto
    {
        public bool Generated { get; set; }
        public List<ShopCardOfferDto> Stock { get; set; } = new List<ShopCardOfferDto>();
        public int UpgradeOffers { get; set; }
        public bool ExtraLifeSold { get; set; }

        public void Normalize()
        {
            Stock ??= new List<ShopCardOfferDto>();
        }
    }
}
