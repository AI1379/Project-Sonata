using System.Collections.Generic;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Hard-coded Phase 1 resonance table.
    /// Values follow BattleDesign §10:
    ///   - Major triad: high damage (40). ViaFifth path ×1.5 → 60.
    ///   - Minor triad: medium damage (25). ViaFifth path ×1.3 → 33.
    ///     (DoT / tempo slow deferred to a later phase.)
    ///   - Dissonance: small true damage (15) with backlash to the caster (5).
    ///   - Perfect fifth: no direct damage. Attaches Harmonic Armor instead;
    ///     when forced to standalone-settle (3 tones, no triad), still no damage
    ///     but consumes the armor and clears the stack.
    ///   - Unison: currently unreachable because ToneStack rejects duplicates.
    /// </summary>
    public static class ResonanceCatalog
    {
        public static readonly IReadOnlyDictionary<ResonanceType, ResonanceConfig> Default =
            new Dictionary<ResonanceType, ResonanceConfig>
            {
                [ResonanceType.MajorTriad] =
                    new ResonanceConfig(ResonanceType.MajorTriad, "Major Triad",
                        targetDamage: 40, selfDamage: 0, viaFifthBonusMultiplier: 1.5f),
                [ResonanceType.MinorTriad] =
                    new ResonanceConfig(ResonanceType.MinorTriad, "Minor Triad",
                        targetDamage: 25, selfDamage: 0, viaFifthBonusMultiplier: 1.3f),
                [ResonanceType.Dissonance] =
                    new ResonanceConfig(ResonanceType.Dissonance, "Dissonance",
                        targetDamage: 15, selfDamage: 5),
                [ResonanceType.PerfectFifth] =
                    new ResonanceConfig(ResonanceType.PerfectFifth, "Perfect Fifth",
                        targetDamage: 0, selfDamage: 0),
                [ResonanceType.Unison] =
                    new ResonanceConfig(ResonanceType.Unison, "Unison",
                        targetDamage: 0, selfDamage: 0),
            };

        public static ResonanceConfig Resolve(IReadOnlyDictionary<ResonanceType, ResonanceConfig> overrides, ResonanceType type)
        {
            if (overrides != null && overrides.TryGetValue(type, out var custom))
                return custom;

            return Default.TryGetValue(type, out var fallback)
                ? fallback
                : new ResonanceConfig(type, type.ToString(), 0, 0);
        }
    }
}
