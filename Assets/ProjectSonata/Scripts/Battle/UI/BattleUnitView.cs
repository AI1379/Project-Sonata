using ProjectSonata.Battle.Domain;
using TMPro;
using UnityEngine;

namespace ProjectSonata.Battle.UI
{
    public sealed class BattleUnitView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text roleText;

        private BattleUnit _boundUnit;

        public void Bind(BattleUnit unit)
        {
            _boundUnit = unit;
            Refresh();
        }

        public void Refresh()
        {
            if (_boundUnit == null)
            {
                if (nameText != null)
                {
                    nameText.text = "<missing unit>";
                }

                if (hpText != null)
                {
                    hpText.text = "HP --/--";
                }

                if (roleText != null)
                {
                    roleText.text = "Unknown";
                }

                return;
            }

            if (nameText != null)
            {
                nameText.text = _boundUnit.DisplayName;
            }

            if (hpText != null)
            {
                hpText.text = $"HP {_boundUnit.CurrentHp}/{_boundUnit.MaxHp}";
            }

            if (roleText != null)
            {
                roleText.text = _boundUnit.IsPlayerUnit ? "Player" : "Enemy";
            }
        }
    }
}
