using System.Collections.Generic;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Outcome of a single skill resolution. Mutable-free; BattleController reads it for UI/logging.
    /// </summary>
    public sealed class SkillResult
    {
        public BattleUnit Caster { get; }
        public BattleUnit Target { get; }
        public SkillDefinition Skill { get; }
        public ResonanceTriggerResult? Resonance { get; }
        public string LogMessage { get; }

        public bool TriggeredResonance => Resonance.HasValue && Resonance.Value.Type != ResonanceType.None;

        public SkillResult(
            BattleUnit caster,
            BattleUnit target,
            SkillDefinition skill,
            ResonanceTriggerResult? resonance,
            string logMessage)
        {
            Caster = caster;
            Target = target;
            Skill = skill;
            Resonance = resonance;
            LogMessage = logMessage ?? string.Empty;
        }
    }
}
