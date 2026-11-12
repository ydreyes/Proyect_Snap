namespace SnapClone.Persistence
{
    /// <summary>
    /// Cartas que posee un perfil nuevo: 9 Control, 3 Destroy, 7 OnReveal y
    /// 2 Neutral. Las 8 cartas Collection Level 1-14 se suman a los 13 starters
    /// originales. Abomination, Misty Knight y Shocker siguen en el catalogo
    /// como cartas no iniciales.
    /// </summary>
    public static class StarterSet
    {
        public static readonly string[] CardIds =
        {
            // Control.
            "card_iron_man",
            "card_punisher",
            "card_hulk",
            "card_the_thing",
            "card_cyclops",
            "card_jessica_jones",
            "card_ka_zar",
            "card_mister_fantastic",
            "card_spectrum",

            // Destroy.
            "card_elektra",
            "card_carnage",
            "card_deathlok",

            // On Reveal.
            "card_hawkeye",
            "card_medusa",
            "card_sentinel",
            "card_star_lord",
            "card_odin",
            "card_white_tiger",
            "card_wolfsbane",

            // Neutral.
            "card_quicksilver",
            "card_nightcrawler"
        };
    }
}
