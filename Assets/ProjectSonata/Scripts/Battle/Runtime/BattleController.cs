using System.Collections.Generic;
using System.Text;
using ProjectSonata.Battle.Domain;
using ProjectSonata.Battle.UI;
using TMPro;
using UnityEngine;

namespace ProjectSonata.Battle.Runtime
{
    public sealed class BattleController : MonoBehaviour
    {
        [SerializeField] private Transform playerSlots;
        [SerializeField] private Transform enemySlots;
        [SerializeField] private Transform skillButtonsRoot;

        [SerializeField] private BattleUnitView battleUnitViewPrefab;
        [SerializeField] private SkillButtonView skillButtonViewPrefab;
        [SerializeField] private ToneMarkChipView chipViewPrefab;

        [SerializeField] private TMP_Text currentEnemy;
        [SerializeField] private TMP_Text currentSkill;
        [SerializeField] private TMP_Text currentCharacter;
        [SerializeField] private TMP_Text timelineDisplay;

        private BattleState _battleState;
        private BattleUnit _selectedUnit;
        private BattleUnit _selectedEnemy;
        private SkillDefinition _selectedSkill;
        private SkillResolver _skillResolver;
        private ActionTimeline _timeline;
        private EnemyAI _enemyAI;
        private bool _battleEnded;
        private readonly List<BattleUnitView> _spawnedUnitViews = new List<BattleUnitView>();
        private readonly List<SkillButtonView> _spawnedSkillButtonViews = new List<SkillButtonView>();

        private void Awake()
        {
            _skillResolver = new SkillResolver();
            _enemyAI = new EnemyAI();
        }

        public void StartBattle(BattleEncounterDefinition encounter)
        {
            if (encounter == null)
            {
                Debug.LogError("BattleController.StartBattle was called with a null encounter.", this);
                return;
            }

            Debug.Log(BuildEncounterSummary(encounter), this);

            if (!ValidateViewReference())
                return;

            ClearSpawnedUnitViews();

            _battleState = BattleState.FromEncounter(encounter);

            SpawnUnit(_battleState.PlayerUnits, playerSlots);
            SpawnUnit(_battleState.EnemyUnits, enemySlots);

            AutoSelectFirstEnemy();

            // Initialize action timeline. All units start at gauge 0;
            // AdvanceToNextReady will virtually advance time so the fastest unit acts first.
            _timeline = new ActionTimeline();
            _timeline.OnTimelineChanged += RefreshTimelineDisplay;
            _battleEnded = false;
            foreach (var unit in _battleState.PlayerUnits)
                _timeline.Add(unit);
            foreach (var unit in _battleState.EnemyUnits)
                _timeline.Add(unit);

            RefreshTimelineDisplay();
            HandleNextTurn();
        }

        private void AutoSelectFirstEnemy()
        {
            var enemies = _battleState?.EnemyUnits;
            if (enemies == null || enemies.Length == 0)
                return;

            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    SelectEnemyUnit(enemy);
                    return;
                }
            }
        }

        private bool ValidateViewReference()
        {
            if (playerSlots == null)
            {
                Debug.LogError("BattleController is missing a reference to the player slots container.", this);
                return false;
            }

            if (enemySlots == null)
            {
                Debug.LogError("BattleController is missing a reference to the enemy slots container.", this);
                return false;
            }

            if (battleUnitViewPrefab == null)
            {
                Debug.LogError("BattleController is missing a reference to the BattleUnitView prefab.", this);
                return false;
            }

            if (skillButtonsRoot == null)
            {
                Debug.LogError("BattleController is missing a reference to the skill buttons container.", this);
                return false;
            }

            if (skillButtonViewPrefab == null)
            {
                Debug.LogError("BattleController is missing a reference to the SkillButtonView prefab.", this);
                return false;
            }

            if (chipViewPrefab == null)
            {
                Debug.LogError("BattleController is missing a reference to the ToneMarkChipView prefab.", this);
                return false;
            }

            return true;
        }

        private void SpawnUnit(BattleUnit[] units, Transform parent)
        {
            if (units == null) return;

            foreach (var unit in units)
            {
                if (unit == null) continue;

                var view = Instantiate(battleUnitViewPrefab, parent);

                if (unit.IsPlayerUnit) view.Bind(unit, chipViewPrefab, SelectPlayerUnit);
                else view.Bind(unit, chipViewPrefab, SelectEnemyUnit);

                _spawnedUnitViews.Add(view);
            }
        }

        private void SelectPlayerUnit(BattleUnit unit)
        {
            if (unit == null || !unit.IsPlayerUnit)
            {
                return;
            }

            // Only the currently-ready player unit can be selected & operated.
            // Tapping other player units just shows a hint so the player understands
            // why nothing happens.
            if (_timeline != null && _timeline.CurrentReady != unit)
            {
                var currentName = _timeline.CurrentReady?.DisplayName ?? "—";
                Debug.Log(
                    $"Cannot select {unit.DisplayName}: current turn is {currentName}.", this);
                if (currentCharacter != null)
                    currentCharacter.text = $"Turn: {currentName}  ({unit.DisplayName} is not ready)";
                return;
            }

            _selectedUnit = unit;
            RefreshSkillButtons(unit);
            Debug.Log($"Selected player unit: {unit.DisplayName}", this);
            if (currentCharacter != null)
                currentCharacter.text = $"Turn: {unit.DisplayName}";
        }

        private void SelectEnemyUnit(BattleUnit unit)
        {
            if (unit == null) return;
            if (unit.IsPlayerUnit) return;

            _selectedEnemy = unit;

            Debug.Log($"Selected enemy unit: {unit.DisplayName}", this);
            currentEnemy.text = $"Selected Enemy: {unit.DisplayName}";
        }

        private void RefreshSkillButtons(BattleUnit unit)
        {
            ClearSpawnedSkillButtonViews();

            if (unit.Skills == null)
            {
                return;
            }

            Debug.Log($"Updating skill buttons for {unit.DisplayName}", this);

            foreach (var skill in unit.Skills)
            {
                if (skill == null)
                {
                    continue;
                }

                var view = Instantiate(skillButtonViewPrefab, skillButtonsRoot);
                view.Bind(skill, OnSkillClicked);
                Debug.Log($"Selected skill: {skill.DisplayName}", this);
                _spawnedSkillButtonViews.Add(view);
            }
        }

        private void OnSkillClicked(SkillDefinition skill)
        {
            if (_selectedUnit == null || skill == null || _battleEnded)
            {
                return;
            }

            if (_timeline == null || _timeline.CurrentReady == null ||
                _timeline.CurrentReady != _selectedUnit)
            {
                Debug.LogWarning(
                    $"Cannot act: {_selectedUnit.DisplayName} is not the current ready unit.", this);
                if (currentSkill != null)
                    currentSkill.text = $"Wait! {_selectedUnit.DisplayName} isn't ready.";
                return;
            }

            if (_selectedEnemy == null || !_selectedEnemy.IsAlive)
            {
                Debug.LogWarning("Cannot cast skill: no living enemy selected.", this);
                if (currentSkill != null)
                    currentSkill.text = "Select an enemy first!";
                return;
            }

            _selectedSkill = skill;

            var result = _skillResolver.Resolve(_selectedUnit, _selectedEnemy, skill);

            if (result == null)
            {
                return;
            }

            Debug.Log(result.LogMessage, this);

            if (result.TriggeredResonance && currentSkill != null)
            {
                var resonance = result.Resonance.Value;
                var pathLabel = resonance.FormationPath switch
                {
                    ResonanceFormationPath.TriadViaFifth => "via 5th",
                    ResonanceFormationPath.TriadViaThird => "via 3rd",
                    _ => "",
                };
                var armorLabel = resonance.ConsumedHarmonicArmor ? " [armor consumed]" : "";
                var pathDisplay = string.IsNullOrEmpty(pathLabel) ? "" : $" ({pathLabel})";
                currentSkill.text = $"RESONANCE: {resonance.DisplayName}{pathDisplay} ({resonance.TargetDamage} dmg){armorLabel}";
            }
            else if (currentSkill != null)
            {
                // Show Harmonic Armor state if it just got attached
                var armorState = _selectedEnemy.HasHarmonicArmor ? " [5° armor]" : "";
                currentSkill.text = $"Current Skill: {skill.DisplayName}{armorState}";
            }

            if (!result.Target.IsAlive)
            {
                Debug.Log($"{result.Target.DisplayName} defeated.", this);
                AutoSelectFirstEnemy();
            }

            // Consume this unit's action and advance the timeline.
            _timeline.ConsumeAction(_selectedUnit);
            HandleNextTurn();
        }

        private void RefreshTimelineDisplay()
        {
            if (_timeline == null || timelineDisplay == null)
                return;

            var snapshot = _timeline.GetReadinessSnapshot();
            var builder = new StringBuilder();
            builder.AppendLine("TURN ORDER");

            foreach (var unit in snapshot)
            {
                var marker = unit == _timeline.CurrentReady ? "★" : " ";
                var percent = System.Math.Round(
                    System.Math.Clamp(unit.ActionGauge / ActionTimeline.ReadyThreshold, 0f, 1f) * 100f);
                var faction = unit.IsPlayerUnit ? "" : " [E]";
                builder.AppendLine($"{marker} {unit.DisplayName}{faction}  {percent}%");
            }

            timelineDisplay.text = builder.ToString();
        }

        private void HandleNextTurn()
        {
            if (_battleEnded)
                return;

            CleanupTimeline();

            if (CheckBattleEnd())
                return;

            var readyUnit = _timeline.AdvanceToNextReady();
            if (readyUnit == null)
            {
                Debug.LogWarning("ActionTimeline produced no ready unit.", this);
                return;
            }

            if (readyUnit.IsPlayerUnit)
            {
                SelectPlayerUnit(readyUnit);
                if (currentCharacter != null)
                    currentCharacter.text = $"Turn: {readyUnit.DisplayName}";
            }
            else
            {
                EnemyAct(readyUnit);
            }
        }

        private void EnemyAct(BattleUnit enemy)
        {
            var target = _enemyAI.PickTarget(enemy, _battleState.PlayerUnits);
            var action = _enemyAI.ExecuteAction(enemy, target);

            Debug.Log(action.LogMessage, this);
            if (currentSkill != null)
                currentSkill.text = action.LogMessage;
            if (currentCharacter != null)
                currentCharacter.text = $"Turn: {enemy.DisplayName} (enemy)";

            _timeline.ConsumeAction(enemy);

            // Defer the next turn to next frame so the UI can render this enemy's action
            // and we avoid deep recursion when multiple enemies chain-act.
            // Phase 1: simple delay. Real version will use Coroutine + animation.
            StartCoroutine(AdvanceAfterEnemyTurnRoutine());
        }

        private System.Collections.IEnumerator AdvanceAfterEnemyTurnRoutine()
        {
            yield return new WaitForSeconds(0.4f);
            HandleNextTurn();
        }

        private void CleanupTimeline()
        {
            if (_timeline == null) return;

            var toRemove = new List<BattleUnit>();
            foreach (var unit in _timeline.Units)
            {
                if (unit == null || !unit.IsAlive)
                    toRemove.Add(unit);
            }

            foreach (var unit in toRemove)
                _timeline.Remove(unit);
        }

        private bool CheckBattleEnd()
        {
            var anyPlayerAlive = AnyAlive(_battleState.PlayerUnits);
            var anyEnemyAlive = AnyAlive(_battleState.EnemyUnits);

            if (anyPlayerAlive && anyEnemyAlive)
                return false;

            _battleEnded = true;
            var result = !anyPlayerAlive ? "DEFEAT" : "VICTORY";
            Debug.Log($"Battle ended: {result}", this);
            if (currentSkill != null)
                currentSkill.text = $"=== {result} ===";
            if (currentCharacter != null)
                currentCharacter.text = $"Battle ended";

            return true;
        }

        private static bool AnyAlive(BattleUnit[] units)
        {
            if (units == null || units.Length == 0)
                return false;

            foreach (var unit in units)
            {
                if (unit != null && unit.IsAlive)
                    return true;
            }

            return false;
        }

        private void ClearSpawnedUnitViews()
        {
            foreach (var view in _spawnedUnitViews)
            {
                if (view != null)
                    Destroy(view.gameObject);
            }

            _spawnedUnitViews.Clear();

            ClearChildren(playerSlots);
            ClearChildren(enemySlots);
            ClearSpawnedSkillButtonViews();
        }

        private void ClearSpawnedSkillButtonViews()
        {
            foreach (var view in _spawnedSkillButtonViews)
            {
                if (view != null)
                    Destroy(view.gameObject);
            }

            _spawnedSkillButtonViews.Clear();
            ClearChildren(skillButtonsRoot);
        }

        private static void ClearChildren(Transform transform)
        {
            if (transform == null) return;

            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private static string BuildEncounterSummary(BattleEncounterDefinition encounter)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Loaded encounter: {encounter.name}");
            builder.AppendLine($"Starting tonality: {encounter.StartingTonality}");
            builder.AppendLine($"Player count: {encounter.PlayerParty?.Length ?? 0}");

            if (encounter.PlayerParty != null)
            {
                for (var i = 0; i < encounter.PlayerParty.Length; i++)
                {
                    var character = encounter.PlayerParty[i];
                    builder.AppendLine($"  Player {i}: {FormatCharacter(character)}");
                }
            }

            builder.AppendLine($"Enemy count: {encounter.EnemyParty?.Length ?? 0}");

            if (encounter.EnemyParty != null)
            {
                for (var i = 0; i < encounter.EnemyParty.Length; i++)
                {
                    var enemy = encounter.EnemyParty[i];
                    builder.AppendLine($"  Enemy {i}: {FormatEnemy(enemy)}");
                }
            }

            return builder.ToString();
        }

        private static string FormatCharacter(CharacterDefinition character)
        {
            if (character == null)
            {
                return "<missing character>";
            }

            return
                $"{character.DisplayName} (HP {character.MaxHp}, ATK {character.Attack}, SPD {character.Speed}, Skills {character.Skills?.Length ?? 0})";
        }

        private static string FormatEnemy(EnemyDefinition enemy)
        {
            if (enemy == null)
            {
                return "<missing enemy>";
            }

            return
                $"{enemy.DisplayName} (HP {enemy.MaxHp}, ATK {enemy.Attack}, SPD {enemy.Speed}, Skills {enemy.Skills?.Length ?? 0})";
        }
    }
}
