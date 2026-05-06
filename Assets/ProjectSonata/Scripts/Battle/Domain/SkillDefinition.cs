using UnityEngine;

namespace ProjectSonata.Battle.Domain
{
    [CreateAssetMenu(menuName = "Project Sonata/Battle/Skill", fileName = "Skill_")]
    public sealed class SkillDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "New Skill";

        [Header("Whitebox Settings")]
        [SerializeField] private int power = 10;
        [SerializeField] private bool appliesTone = true;
        [SerializeField] private Tone toneToApply = Tone.C;
        [SerializeField] private bool canTriggerResonance;

        public string DisplayName => displayName;
        public int Power => power;
        public bool AppliesTone => appliesTone;
        public Tone ToneToApply => toneToApply;
        public bool CanTriggerResonance => canTriggerResonance;
    }
}
