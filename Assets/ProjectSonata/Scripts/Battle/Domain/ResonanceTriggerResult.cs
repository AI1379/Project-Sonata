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

        public ResonanceTriggerResult(
            ResonanceType type,
            string displayName,
            IReadOnlyList<Tone> matchedTones,
            int targetDamage,
            int selfDamage)
        {
            Type = type;
            DisplayName = displayName;
            MatchedTones = matchedTones ?? System.Array.Empty<Tone>();
            TargetDamage = targetDamage;
            SelfDamage = selfDamage;
        }
    }
}
