using System;
using System.Collections.Generic;
using ProjectSonata.Battle.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectSonata.Battle.UI
{
    public sealed class BattleUnitView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private Transform toneMarksRoot;

        private BattleUnit _boundUnit;
        private ToneMarkChipView _chipPrefab;
        private Action<BattleUnit> _onUnitSelected;
        private readonly List<ToneMarkChipView> _spawnedToneChips = new List<ToneMarkChipView>();

        public void Bind(BattleUnit unit, ToneMarkChipView chipPrefab, Action<BattleUnit> onUnitSelected = null)
        {
            if (_boundUnit != null)
            {
                _boundUnit.ToneStack.OnChange -= HandleToneStackChanged;
                _boundUnit.OnHpChanged -= HandleHpChanged;
            }

            _boundUnit = unit;
            _chipPrefab = chipPrefab;
            _onUnitSelected = onUnitSelected;

            if (toneMarksRoot == null)
            {
                var toneMarksTransform = transform.Find("ToneMarks");
                if (toneMarksTransform != null)
                {
                    toneMarksRoot = toneMarksTransform;
                }
            }

            if (_boundUnit != null)
            {
                _boundUnit.ToneStack.OnChange += HandleToneStackChanged;
                _boundUnit.OnHpChanged += HandleHpChanged;
                _boundUnit.OnHarmonicArmorChanged += HandleHarmonicArmorChanged;
            }

            Refresh();
            RefreshToneChips();
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
                var role = _boundUnit.IsPlayerUnit ? "Player" : "Enemy";
                roleText.text = _boundUnit.HasHarmonicArmor
                    ? $"{role} [5°]"
                    : role;
            }
        }

        private void OnDestroy()
        {
            if (_boundUnit != null)
            {
                _boundUnit.ToneStack.OnChange -= HandleToneStackChanged;
                _boundUnit.OnHpChanged -= HandleHpChanged;
                _boundUnit.OnHarmonicArmorChanged -= HandleHarmonicArmorChanged;
            }
        }

        private void HandleToneStackChanged(ToneStack stack)
        {
            RefreshToneChips();
        }

        private void HandleHpChanged(BattleUnit unit)
        {
            Refresh();
        }

        private void HandleHarmonicArmorChanged(BattleUnit unit)
        {
            Refresh();
        }

        private void RefreshToneChips()
        {
            ClearToneChips();

            if (_boundUnit == null || _chipPrefab == null || toneMarksRoot == null)
            {
                return;
            }

            foreach (var tone in _boundUnit.ToneStack.Tones)
            {
                var chipView = Instantiate(_chipPrefab, toneMarksRoot);
                chipView.Bind(tone);
                _spawnedToneChips.Add(chipView);
            }
        }

        private void ClearToneChips()
        {
            foreach (var chip in _spawnedToneChips)
            {
                if (chip != null)
                {
                    Destroy(chip.gameObject);
                }
            }

            _spawnedToneChips.Clear();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_boundUnit == null) return;

            _onUnitSelected?.Invoke(_boundUnit);
        }
    }
}
