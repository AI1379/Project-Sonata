using System.Collections.Generic;
using NUnit.Framework;
using ProjectSonata.Battle.Domain;

namespace ProjectSonata.Tests.EditMode
{
    public class ResonanceDetectorTests
    {
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

        [Test]
        public void Detect_PerfectFifth_CG_ReturnsPerfectFifth()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.G });
            Assert.AreEqual(ResonanceType.PerfectFifth, match.Type);
        }

        [Test]
        public void Detect_PerfectFifth_DA_ReturnsPerfectFifth()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.D, Tone.A });
            Assert.AreEqual(ResonanceType.PerfectFifth, match.Type);
        }

        [Test]
        public void Detect_Dissonance_CD_ReturnsDissonance()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.D });
            Assert.AreEqual(ResonanceType.Dissonance, match.Type);
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

        [Test]
        public void Detect_TwoUnrelatedTones_CE_ReturnsNone()
        {
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.E });
            Assert.IsFalse(match.IsDetected);
        }

        [Test]
        public void Detect_TriadBeatsPairPriority()
        {
            // C-E-G is a major triad; the pair E-G would otherwise be a third (no resonance).
            // Triad detection should win when 3 tones form a chord.
            var match = ResonanceDetector.Detect(new[] { Tone.C, Tone.E, Tone.G });
            Assert.AreEqual(ResonanceType.MajorTriad, match.Type);
        }

        [Test]
        public void Detect_LastThreeTonesUsedWhenStackFull()
        {
            // Stack contains more than 3 (e.g. when caller passes a larger list);
            // detection should consider the most recently added 3 tones.
            var match = ResonanceDetector.Detect(new[] { Tone.A, Tone.C, Tone.E, Tone.G });
            Assert.AreEqual(ResonanceType.MajorTriad, match.Type);
        }
    }
}
