using System;
using ProjectSonata.Battle.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectSonata.Battle.UI
{
    public sealed class SkillButtonView : MonoBehaviour
    {
        [SerializeField] private TMP_Text skillName = default;
        [SerializeField] private TMP_Text toneText = default;
        [SerializeField] private Button button = default;

        private SkillDefinition _skill;
        private Action<SkillDefinition> _onClicked;

        public void Bind(SkillDefinition skill, Action<SkillDefinition> onClicked)
        {
            _skill = skill;
            _onClicked = onClicked;

            if (skillName != null)
            {
                skillName.text = skill != null ? skill.DisplayName : "<missing skill>";
            }

            if (toneText != null)
            {
                toneText.text = skill != null && skill.AppliesTone ? skill.ToneToApply.ToString() : "--";
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
                button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            if (_skill == null)
            {
                return;
            }

            _onClicked?.Invoke(_skill);
        }
    }
}
