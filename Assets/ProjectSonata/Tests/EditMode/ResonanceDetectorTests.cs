using System.Collections.Generic;
using NUnit.Framework;
using ProjectSonata.Battle.Domain;

namespace ProjectSonata.Tests.EditMode
{
    public class ResonanceDetectorTests
    {
        // ──────────────────────────────────────────────────────────────
        // Basic detection
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Detect_EmptyList_ReturnsNone()
        {
            var match = ResonanceDetector.Detect(System.Array.Empty<Tone>());
            Assert.IsFalse(match.IsDetected);
            Assert.AreEqual(ResonanceType.None, match.Type);
        }

        [Test]
        public void Detect_SingleTone_ReturnsNone()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.C });
            Assert.IsFalse(match.IsDetected);
        }

        // ──────────────────────────────────────────────────────────────
        // Triad detection (type)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Detect_MajorTriad_CEG_ReturnsMajorTriad()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.E, Tone.G });
            Assert.IsTrue(match.IsDetected);
            Assert.AreEqual(ResonanceType.MajorTriad, match.Type);
        }

        [Test]
        public void Detect_MajorTriad_FAC_ReturnsMajorTriad()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.F, Tone.A, Tone.C });
            Assert.AreEqual(ResonanceType.MajorTriad, match.Type);
        }

        [Test]
        public void Detect_MajorTriad_GBD_ReturnsMajorTriad()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.G, Tone.B, Tone.D });
            Assert.AreEqual(ResonanceType.MajorTriad, match.Type);
        }

        [Test]
        public void Detect_MinorTriad_ACE_ReturnsMinorTriad()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.A, Tone.C, Tone.E });
            Assert.AreEqual(ResonanceType.MinorTriad, match.Type);
        }

        [Test]
        public void Detect_MinorTriad_DFA_ReturnsMinorTriad()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.D, Tone.F, Tone.A });
            Assert.AreEqual(ResonanceType.MinorTriad, match.Type);
        }

        [Test]
        public void Detect_MinorTriad_EGB_ReturnsMinorTriad()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.E, Tone.G, Tone.B });
            Assert.AreEqual(ResonanceType.MinorTriad, match.Type);
        }

        [Test]
        public void Detect_MajorTriad_OrderIndependent()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.G, Tone.C, Tone.E });
            Assert.AreEqual(ResonanceType.MajorTriad, match.Type);
        }

        // ──────────────────────────────────────────────────────────────
        // Formation path detection (the dual-path mechanism)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Detect_Triad_CE_ThenG_IsViaThird()
        {
            // C-E laid first, G added last → ViaThird (base damage path)
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.E, Tone.G });
            Assert.AreEqual(ResonanceFormationPath.TriadViaThird, match.FormationPath);
        }

        [Test]
        public void Detect_Triad_CG_ThenE_IsViaFifth()
        {
            // C-G laid first (perfect fifth), E added last → ViaFifth (bonus path)
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.G, Tone.E });
            Assert.AreEqual(ResonanceFormationPath.TriadViaFifth, match.FormationPath);
        }

        [Test]
        public void Detect_Triad_GC_ThenE_IsViaFifth()
        {
            // Same as above but reverse insertion order of the fifth pair
            var match = ResonanceDetector.Detect(new[] { Tone.G, Tone.C, Tone.E });
            Assert.AreEqual(ResonanceFormationPath.TriadViaFifth, match.FormationPath);
        }

        [Test]
        public void Detect_Triad_FA_ThenC_IsViaThird()
        {
            // F-A third pair laid first, C (fifth above F) added last
            var match = ResonanceDetector.Detect(new[] { Tone.F, Tone.A, Tone.C });
            Assert.AreEqual(ResonanceFormationPath.TriadViaThird, match.FormationPath);
        }

        [Test]
        public void Detect_Triad_FC_ThenA_IsViaFifth()
        {
            // F-C fifth pair laid first, A (third above F) added last
            var match = ResonanceDetector.Detect(new[] { Tone.F, Tone.C, Tone.A });
            Assert.AreEqual(ResonanceFormationPath.TriadViaFifth, match.FormationPath);
        }

        // ──────────────────────────────────────────────────────────────
        // Perfect Fifth — NO LONGER auto-triggered as 2-tone resonance
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Detect_PerfectFifth_CG_ReturnsNone()
        {
            // Changed behavior: 2-tone Perfect Fifth is no longer returned by Detect.
            // It's handled by SkillResolver as Harmonic Armor attachment.
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.G });
            Assert.IsFalse(match.IsDetected);
        }

        [Test]
        public void Detect_PerfectFifth_DA_ReturnsNone()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.D, Tone.A });
            Assert.IsFalse(match.IsDetected);
        }

        // ──────────────────────────────────────────────────────────────
        // ContainsPerfectFifth (used by SkillResolver for Harmonic Armor logic)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void ContainsPerfectFifth_CG_ReturnsTrue()
        {
            Assert.IsTrue(ResonanceDetector.ContainsPerfectFifth(new[] { Tone.C, Tone.G }));
        }

        [Test]
        public void ContainsPerfectFifth_CGA_ReturnsTrue()
        {
            // 3 tones, contains a fifth pair (C-G)
            Assert.IsTrue(ResonanceDetector.ContainsPerfectFifth(new[] { Tone.C, Tone.G, Tone.A }));
        }

        [Test]
        public void ContainsPerfectFifth_CE_ReturnsFalse()
        {
            // Major third, not a fifth
            Assert.IsFalse(ResonanceDetector.ContainsPerfectFifth(new[] { Tone.C, Tone.E }));
        }

        [Test]
        public void ContainsPerfectFifth_CEG_ReturnsTrue()
        {
            // Triad contains the C-G fifth pair
            Assert.IsTrue(ResonanceDetector.ContainsPerfectFifth(new[] { Tone.C, Tone.E, Tone.G }));
        }

        // ──────────────────────────────────────────────────────────────
        // Dissonance (still auto-triggers at 2 tones)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Detect_Dissonance_CD_ReturnsDissonance()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.D });
            Assert.AreEqual(ResonanceType.Dissonance, match.Type);
            Assert.AreEqual(ResonanceFormationPath.Direct, match.FormationPath);
        }

        [Test]
        public void Detect_Dissonance_EF_ReturnsDissonance()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.E, Tone.F });
            Assert.AreEqual(ResonanceType.Dissonance, match.Type);
        }

        [Test]
        public void Detect_Dissonance_BA_ReturnsDissonance()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.B, Tone.A });
            Assert.AreEqual(ResonanceType.Dissonance, match.Type);
        }

        // ──────────────────────────────────────────────────────────────
        // Edge cases
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Detect_TwoUnrelatedTones_CE_ReturnsNone()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.E });
            Assert.IsFalse(match.IsDetected);
        }

        [Test]
        public void Detect_LastThreeTonesUsedWhenStackFull()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.A, Tone.C, Tone.E, Tone.G });
            Assert.AreEqual(ResonanceType.MajorTriad, match.Type);
        }
    }
}
