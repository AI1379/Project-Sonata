using System.Collections.Generic;

namespace ProjectSonata.Battle.Domain
{
    public readonly struct ResonanceTriggerResult
    {
        public ResonanceType Type { get; }
        public string DisplayName { get; }
        public IReadOnlyList<Tone> MatchedTones { get; }
        public int TargetDamage { get; }
        public int SelfDamage { get; }
        public ResonanceFormationPath FormationPath { get; }
        public bool ConsumedHarmonicArmor { get; }

        public ResonanceTriggerResult(
            ResonanceType type,
            string displayName,
            IReadOnlyList<Tone> matchedTones,
            int targetDamage,
            int selfDamage,
            ResonanceFormationPath formationPath = ResonanceFormationPath.Direct,
            bool consumedHarmonicArmor = false)
        {
            Type = type;
            DisplayName = displayName;
            MatchedTones = matchedTones ?? System.Array.Empty<Tone>();
            TargetDamage = targetDamage;
            SelfDamage = selfDamage;
            FormationPath = formationPath;
            ConsumedHarmonicArmor = consumedHarmonicArmor;
        }
    }
}
