using System.Collections.Generic;
using System.Text;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Pure rule layer: resolves a single skill cast, applies tones, and triggers
    /// any resonance that forms on the target. Has no Unity references.
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

                var match = ResonanceDetector.Detect(target.ToneStack.Tones);
                if (match.IsDetected)
                {
                    resonanceResult = TriggerResonance(caster, target, match, log);
                }
            }

            return new SkillResult(caster, target, skill, resonanceResult, log.ToString());
        }

        private ResonanceTriggerResult? TriggerResonance(
            BattleUnit caster,
            BattleUnit target,
            ResonanceMatch match,
            StringBuilder log)
        {
            var config = ResonanceCatalog.Resolve(_resonanceOverrides, match.Type);

            if (config.TargetDamage > 0)
            {
                target.TakeDamage(config.TargetDamage);
                log.Append($" {config.DisplayName}: target takes {config.TargetDamage} damage.");
            }

            if (config.SelfDamage > 0)
            {
                caster.TakeDamage(config.SelfDamage);
                log.Append($" {config.DisplayName} backlash: caster takes {config.SelfDamage} damage.");
            }

            target.ToneStack.ClearTones();
            log.Append(" Tones consumed.");

            return new ResonanceTriggerResult(
                match.Type,
                config.DisplayName,
                match.Tones,
                config.TargetDamage,
                config.SelfDamage);
        }
    }
}
