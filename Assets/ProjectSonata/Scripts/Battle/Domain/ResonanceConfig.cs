namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Plain data describing what a resonance does when triggered.
    /// Kept as a POCO so the rule layer stays Unity-free and unit-testable.
    /// A ScriptableObject wrapper can be added later without changing call sites.
    /// </summary>
    public readonly struct ResonanceConfig
    {
        public ResonanceType Type { get; }
        public string DisplayName { get; }
        public int TargetDamage { get; }
        public int SelfDamage { get; }

        public ResonanceConfig(ResonanceType type, string displayName, int targetDamage, int selfDamage)
        {
            Type = type;
            DisplayName = displayName;
            TargetDamage = targetDamage;
            SelfDamage = selfDamage;
        }
    }
}
