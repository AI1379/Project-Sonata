using System.Collections.Generic;
using System.Linq;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Identifies musical resonances formed by the tones currently attached to a unit.
    /// Phase 1 uses a hard-coded, table-driven approach (see README "Phase 1 共鸣规则").
    /// Detection priority: 3-tone triads first, then 2-tone intervals.
    /// </summary>
    public static class ResonanceDetector
    {
        public static ResonanceMatch Detect(IReadOnlyList<Tone> tones)
        {
            if (tones == null || tones.Count == 0)
                return ResonanceMatch.Empty;

            if (tones.Count >= 3)
            {
                var triad = DetectTriad(tones);
                if (triad.IsDetected)
                    return triad;
            }

            if (tones.Count >= 2)
            {
                var pair = DetectPair(tones);
                if (pair.IsDetected)
                    return pair;
            }

            return ResonanceMatch.Empty;
        }

        private static ResonanceMatch DetectTriad(IReadOnlyList<Tone> tones)
        {
            var lastThree = tones
                .Skip(tones.Count - 3)
                .Take(3)
                .ToArray();

            if (MatchesSet(lastThree, Tone.C, Tone.E, Tone.G) ||
                MatchesSet(lastThree, Tone.F, Tone.A, Tone.C) ||
                MatchesSet(lastThree, Tone.G, Tone.B, Tone.D))
            {
                return new ResonanceMatch(ResonanceType.MajorTriad, lastThree);
            }

            if (MatchesSet(lastThree, Tone.A, Tone.C, Tone.E) ||
                MatchesSet(lastThree, Tone.D, Tone.F, Tone.A) ||
                MatchesSet(lastThree, Tone.E, Tone.G, Tone.B))
            {
                return new ResonanceMatch(ResonanceType.MinorTriad, lastThree);
            }

            return ResonanceMatch.Empty;
        }

        private static ResonanceMatch DetectPair(IReadOnlyList<Tone> tones)
        {
            var lastTwo = new[]
            {
                tones[tones.Count - 2],
                tones[tones.Count - 1]
            };

            if (IsPerfectFifth(lastTwo[0], lastTwo[1]))
                return new ResonanceMatch(ResonanceType.PerfectFifth, lastTwo);

            if (IsDissonant(lastTwo[0], lastTwo[1]))
                return new ResonanceMatch(ResonanceType.Dissonance, lastTwo);

            return ResonanceMatch.Empty;
        }

        private static bool MatchesSet(IReadOnlyList<Tone> tones, params Tone[] expected)
        {
            if (tones.Count != expected.Length)
                return false;

            foreach (var tone in tones)
            {
                if (!expected.Contains(tone))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Perfect fifth up: interval of 4 scale steps in the natural-note set
        /// (C-G, D-A, E-B, F-C, G-D, A-E). B-F is technically a diminished fifth
        /// but is treated as a perfect fifth in Phase 1 simplification.
        /// </summary>
        private static bool IsPerfectFifth(Tone a, Tone b)
        {
            var diff = ((int)b - (int)a + 7) % 7;
            return diff == 4;
        }

        /// <summary>
        /// Dissonant: adjacent natural notes (C-D, D-E, E-F, F-G, G-A, A-B).
        /// </summary>
        private static bool IsDissonant(Tone a, Tone b)
        {
            var diff = System.Math.Abs((int)b - (int)a);
            return diff == 1;
        }
    }
}
