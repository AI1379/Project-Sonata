using ProjectSonata.Battle.Domain;
using TMPro;
using UnityEngine;

namespace ProjectSonata.Battle.UI
{
    public sealed class SkillButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text skillName = default;
        [SerializeField] private TMP_Text toneText = default;

        public SkillButtonView(SkillDefinition skill)
        {
            skillName.text = skill.DisplayName ?? "<missing DisplayName>";
            toneText.text = skill.ToneToApply.ToString() ?? "<missing ToneToApply>";
        }
    }
}