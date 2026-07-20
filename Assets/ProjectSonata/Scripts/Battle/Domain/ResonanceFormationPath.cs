namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// How a 3-tone triad came together. Determines damage multiplier
    /// (see Perfect Fifth dual-path mechanism, BattleDesign §7).
    /// </summary>
    public enum ResonanceFormationPath
    {
        /// <summary>
        /// Default path (e.g. 2-tone resonances, or when path can't be determined).
        /// </summary>
        Direct,

        /// <summary>
        /// Third interval laid first (CE), fifth tone added last (G).
        /// No Harmonic Armor on the target → base damage.
        /// </summary>
        TriadViaThird,

        /// <summary>
        /// Fifth interval laid first (CG), third tone added last (E).
        /// Target has Harmonic Armor → bonus damage multiplier, consumes the armor.
        /// </summary>
        TriadViaFifth,
    }
}
