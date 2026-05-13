using ProjectSonata.Battle.Domain;
using TMPro;
using UnityEngine;

namespace ProjectSonata.Battle.UI
{
    public sealed class ToneMarkChipView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;

        private Tone _tone;

        public void Bind(Tone tone)
        {
            _tone = tone;

            if (nameText != null)
            {
                nameText.text = tone.ToString();
            }
        }
    }
}
