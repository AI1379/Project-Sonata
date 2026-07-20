using System.Collections.Generic;
using System.Text;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Pure rule layer: resolves a single skill cast, applies tones, and triggers
    /// any resonance that forms on the target. Has no Unity references.
    ///
    /// Flow (see BattleDesign §7.3):
    ///   1. Apply tone to target.ToneStack
    ///   2. Run ResonanceDetector.Detect
    ///   3. If a triad is detected:
    ///        - Determine formation path
    ///        - If target has Harmonic Armor and path is TriadViaFifth:
    ///          apply bonus multiplier, consume the armor
    ///        - Otherwise base damage (TriadViaThird)
    ///        - Clear stack
    ///   4. If Dissonance is detected: apply damage + self-damage, clear stack
    ///   5. If nothing detected:
    ///        - If stack now contains a Perfect Fifth pair, attach Harmonic Armor
    ///          (idempotent — re-applying while already active is a no-op)
    ///        - If stack is full (3 tones) with no triad AND contains a Perfect Fifth:
    ///          force a Perfect Fifth standalone settlement (consume armor, clear stack,
    ///          no damage — the player lost the chance to complete a triad)
    /// </summary>
    public sealed class SkillResolver
    {
        private readonly IReadOnlyDictionary<ResonanceType, ResonanceConfig> _resonanceOverrides;

        public SkillResolver() : this(null)
        {
        }

        public SkillResolver(IReadOnlyDictionary<ResonanceType, ResonanceConfig> resonanceOverrides)
        {
            _resonanceOverrides = resonanceOverrides;
        }

        /// <summary>
        /// Resolves a skill cast from caster onto target.
        /// Returns null if any required argument is missing.
        /// </summary>
        public SkillResult Resolve(BattleUnit caster, BattleUnit target, SkillDefinition skill)
        {
            if (caster == null || target == null || skill == null)
                return null;

            var log = new StringBuilder();
            log.Append($"{caster.DisplayName} uses {skill.DisplayName} on {target.DisplayName}.");

            ResonanceTriggerResult? resonanceResult = null;

            if (skill.AppliesTone)
            {
                target.ToneStack.AddTone(skill.ToneToApply);
                log.Append($" Applied tone {skill.ToneToApply}.");

                resonanceResult = ResolvePostAddTone(caster, target, log);
            }

            return new SkillResult(caster, target, skill, resonanceResult, log.ToString());
        }

        private ResonanceTriggerResult? ResolvePostAddTone(
            BattleUnit caster,
            BattleUnit target,
            StringBuilder log)
        {
            var match = ResonanceDetector.Detect(target.ToneStack.Tones);

            if (match.IsDetected)
            {
                return match.Type switch
                {
                    ResonanceType.MajorTriad => TriggerTriad(caster, target, match, log),
                    ResonanceType.MinorTriad => TriggerTriad(caster, target, match, log),
                    ResonanceType.Dissonance => TriggerDissonance(caster, target, match, log),
                    _ => null,
                };
            }

            // No resonance detected — check for Perfect Fifth state transitions.
            var tones = target.ToneStack.Tones;

            if (ResonanceDetector.ContainsPerfectFifth(tones))
            {
                if (!target.HasHarmonicArmor)
                {
                    target.ApplyHarmonicArmor();
                    log.Append(" Perfect Fifth pair detected — Harmonic Armor attached.");
                }

                // Stack full without forming a triad → standalone Perfect Fifth settlement.
                if (tones.Count >= 3)
                {
                    return TriggerPerfectFifthStandalone(target, log);
                }
            }

            return null;
        }

        private ResonanceTriggerResult TriggerTriad(
            BattleUnit caster,
            BattleUnit target,
            ResonanceMatch match,
            StringBuilder log)
        {
            var config = ResonanceCatalog.Resolve(_resonanceOverrides, match.Type);

            var effectivePath = match.FormationPath;
            var consumedArmor = false;

            // Path bonus only applies if the player actually went via a fifth base
            // AND the target still had Harmonic Armor (i.e. didn't get cleansed somehow).
            if (effectivePath == ResonanceFormationPath.TriadViaFifth && target.HasHarmonicArmor)
            {
                consumedArmor = target.ConsumeHarmonicArmor();
            }
            else if (effectivePath == ResonanceFormationPath.TriadViaFifth && !target.HasHarmonicArmor)
            {
                // Edge case: path detection said ViaFifth (last-added tone is the third)
                // but target lost the armor somehow. Fall back to Direct path.
                effectivePath = ResonanceFormationPath.Direct;
            }

            var multiplier = effectivePath == ResonanceFormationPath.TriadViaFifth
                ? config.ViaFifthBonusMultiplier
                : 1.0f;

            var damage = System.Convert.ToInt32(System.Math.Round(config.TargetDamage * multiplier));

            if (damage > 0)
            {
                target.TakeDamage(damage);
            }

            log.Append($" {config.DisplayName} triggered via {effectivePath}.");
            if (multiplier > 1.0f)
                log.Append($" Bonus ×{multiplier}: target takes {damage} damage.");
            else
                log.Append($" Target takes {damage} damage.");

            if (consumedArmor)
                log.Append(" Harmonic Armor consumed.");

            target.ToneStack.ClearTones();
            log.Append(" Tones consumed.");

            return new ResonanceTriggerResult(
                match.Type,
                config.DisplayName,
                match.Tones,
                damage,
                config.SelfDamage,
                effectivePath,
                consumedArmor);
        }

        private ResonanceTriggerResult TriggerDissonance(
            BattleUnit caster,
            BattleUnit target,
            ResonanceMatch match,
            StringBuilder log)
        {
            var config = ResonanceCatalog.Resolve(_resonanceOverrides, match.Type);

            if (config.TargetDamage > 0)
                target.TakeDamage(config.TargetDamage);

            if (config.SelfDamage > 0)
                caster.TakeDamage(config.SelfDamage);

            log.Append($" {config.DisplayName}: target takes {config.TargetDamage} damage.");
            if (config.SelfDamage > 0)
                log.Append($" Caster takes {config.SelfDamage} backlash damage.");

            target.ToneStack.ClearTones();
            // Dissonance also wipes Harmonic Armor — the dissonant shock destabilizes any fifth state.
            target.ConsumeHarmonicArmor();
            log.Append(" Tones consumed.");

            return new ResonanceTriggerResult(
                match.Type,
                config.DisplayName,
                match.Tones,
                config.TargetDamage,
                config.SelfDamage,
                ResonanceFormationPath.Direct,
                false);
        }

        private ResonanceTriggerResult TriggerPerfectFifthStandalone(
            BattleUnit target,
            StringBuilder log)
        {
            // Standalone settlement: player added a non-triadic third tone while a
            // Perfect Fifth pair was sitting on the stack. No damage — the armor
            // is wasted, and the stack is cleared as the cost of the failed attempt.
            var consumedArmor = target.ConsumeHarmonicArmor();
            var tones = target.ToneStack.Tones;

            // Snapshot tones before clearing for the result.
            var matchedTones = new List<Tone>(tones);

            target.ToneStack.ClearTones();

            log.Append(" Perfect Fifth standalone settlement: stack cleared, Harmonic Armor consumed.");
            if (!consumedArmor)
                log.Append(" (no armor was active)");

            return new ResonanceTriggerResult(
                ResonanceType.PerfectFifth,
                "Perfect Fifth (wasted)",
                matchedTones,
                targetDamage: 0,
                selfDamage: 0,
                ResonanceFormationPath.Direct,
                consumedArmor);
        }
    }
}
