using System.Collections.Generic;
using System.Linq;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Identifies musical resonances formed by the tones currently attached to a unit.
    /// Phase 1 uses a hard-coded, table-driven approach (see BattleDesign §5/§7).
    ///
    /// Detection priority:
    ///   1. 3-tone triads (with formation path)
    ///   2. 2-tone Dissonance (破坏性共鸣, clears stack immediately)
    ///
    /// Note: 2-tone PerfectFifth is NOT returned by Detect — it's handled by
    /// SkillResolver as a "state attachment" (Harmonic Armor) rather than a
    /// triggered resonance. This avoids PerfectFifth stealing Major Triad
    /// formations (see BattleDesign §7 Perfect Fifth dual-path mechanism).
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
                var lastTwo = new[]
                {
                    tones[tones.Count - 2],
                    tones[tones.Count - 1]
                };

                if (IsDissonant(lastTwo[0], lastTwo[1]))
                    return new ResonanceMatch(ResonanceType.Dissonance, lastTwo, ResonanceFormationPath.Direct);
            }

            return ResonanceMatch.Empty;
        }

        /// <summary>
        /// Returns true if any pair of tones in the list forms a perfect fifth interval.
        /// Used by SkillResolver to attach Harmonic Armor without triggering a resonance.
        /// </summary>
        public static bool ContainsPerfectFifth(IReadOnlyList<Tone> tones)
        {
            if (tones == null || tones.Count < 2)
                return false;

            for (var i = 0; i < tones.Count; i++)
            {
                for (var j = i + 1; j < tones.Count; j++)
                {
                    if (IsPerfectFifth(tones[i], tones[j]))
                        return true;
                }
            }

            return false;
        }

        private static ResonanceMatch DetectTriad(IReadOnlyList<Tone> tones)
        {
            var lastThree = tones
                .Skip(tones.Count - 3)
                .Take(3)
                .ToArray();

            ResonanceType triadType;
            Tone root;

            if (TryMatchMajorTriad(lastThree, out root))
            {
                triadType = ResonanceType.MajorTriad;
            }
            else if (TryMatchMinorTriad(lastThree, out root))
            {
                triadType = ResonanceType.MinorTriad;
            }
            else
            {
                return ResonanceMatch.Empty;
            }

            var path = DetermineTriadPath(lastThree, root);
            return new ResonanceMatch(triadType, lastThree, path);
        }

        private static bool TryMatchMajorTriad(IReadOnlyList<Tone> tones, out Tone root)
        {
            if (MatchesSet(tones, Tone.C, Tone.E, Tone.G)) { root = Tone.C; return true; }
            if (MatchesSet(tones, Tone.F, Tone.A, Tone.C)) { root = Tone.F; return true; }
            if (MatchesSet(tones, Tone.G, Tone.B, Tone.D)) { root = Tone.G; return true; }
            root = default;
            return false;
        }

        private static bool TryMatchMinorTriad(IReadOnlyList<Tone> tones, out Tone root)
        {
            if (MatchesSet(tones, Tone.A, Tone.C, Tone.E)) { root = Tone.A; return true; }
            if (MatchesSet(tones, Tone.D, Tone.F, Tone.A)) { root = Tone.D; return true; }
            if (MatchesSet(tones, Tone.E, Tone.G, Tone.B)) { root = Tone.E; return true; }
            root = default;
            return false;
        }

        /// <summary>
        /// Determines formation path by inspecting the last-added tone's role in the chord.
        /// - If last tone is the third → TriadViaFifth (preceding pair must have been a fifth)
        /// - If last tone is the fifth → TriadViaThird (preceding pair was a third interval)
        /// - Otherwise → Direct
        /// In natural-note set, both major and minor thirds are +2 scale steps from root,
        /// and perfect fifth is +4. So root/third/fifth can be derived uniformly.
        /// </summary>
        private static ResonanceFormationPath DetermineTriadPath(IReadOnlyList<Tone> tones, Tone root)
        {
            var lastTone = tones[tones.Count - 1];
            var thirdAboveRoot = (Tone)(((int)root + 2) % 7);
            var fifthAboveRoot = (Tone)(((int)root + 4) % 7);

            if (lastTone == thirdAboveRoot)
                return ResonanceFormationPath.TriadViaFifth;

            if (lastTone == fifthAboveRoot)
                return ResonanceFormationPath.TriadViaThird;

            return ResonanceFormationPath.Direct;
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
        public static bool IsPerfectFifth(Tone a, Tone b)
        {
            var diff = ((int)b - (int)a + 7) % 7;
            return diff == 4;
        }

        /// <summary>
        /// Dissonant: adjacent natural notes (C-D, D-E, E-F, F-G, G-A, A-B).
        /// </summary>
        public static bool IsDissonant(Tone a, Tone b)
        {
            var diff = System.Math.Abs((int)b - (int)a);
            return diff == 1;
        }
    }
}
