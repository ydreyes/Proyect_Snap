using SnapClone.Data.Definitions;

namespace SnapClone.Gameplay.Cards
{
    /// <summary>
    /// Crea copias, transformaciones y tokens con definiciones independientes.
    /// Las copias conservan el poder persistente actual, pero no los perks de
    /// run, para no duplicar progresion.
    /// </summary>
    public static class CardInstanceFactory
    {
        public static CardInstance Copy(CardInstance source, bool copyPower = true,
            bool copyPerks = false)
        {
            if (source?.Definition == null) return null;

            var copy = new CardInstance(source.Definition.Clone())
            {
                Location = source.Location,
                IsInPlay = false,
                BonusPower = copyPower ? source.BonusPower : 0
            };

            if (copyPerks && source.AttachedPerks != null)
                copy.AttachedPerks.AddRange(source.AttachedPerks);

            return copy;
        }

        public static void Transform(CardInstance card, CardDef newDefinition)
        {
            if (card == null || newDefinition == null) return;

            card.Definition = newDefinition.Clone();
            card.Id = card.Definition.Id;
            card.IsMerged = false;
        }
    }
}
