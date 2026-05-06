using System.Collections.Generic;
using System.Text;
using GLTFast.Schema;
using ProjectSonata.Battle.Domain;
using ProjectSonata.Battle.UI;
using UnityEngine;

namespace ProjectSonata.Battle.Runtime
{
    public sealed class BattleController : MonoBehaviour
    {
        [SerializeField] private Transform playerSlots;
        [SerializeField] private Transform enemySlots;

        [SerializeField] private BattleUnitView battleUnitViewPrefab;

        private BattleState _battleState;
        private readonly List<BattleUnitView> _spawnedUnitViews = new List<BattleUnitView>();

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

            return true;
        }

        private void SpawnUnit(BattleUnit[] units, Transform parent)
        {
            if (units == null) return;

            foreach (var unit in units)
            {
                if (unit == null) continue;

                var view = Instantiate(battleUnitViewPrefab, parent);
                view.Bind(unit);
                _spawnedUnitViews.Add(view);
            }
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