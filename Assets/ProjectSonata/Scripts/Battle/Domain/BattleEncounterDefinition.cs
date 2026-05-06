using UnityEngine;

namespace ProjectSonata.Battle.Domain
{
    [CreateAssetMenu(menuName = "Project Sonata/Battle/Encounter", fileName = "Encounter_")]
    public sealed class BattleEncounterDefinition : ScriptableObject
    {
        [Header("Player Party")]
        [SerializeField] private CharacterDefinition[] playerParty;

        [Header("Enemy Party")]
        [SerializeField] private EnemyDefinition[] enemyParty;

        [Header("Battle State")]
        [SerializeField] private Tonality startingTonality = Tonality.Major;

        public CharacterDefinition[] PlayerParty => playerParty;
        public EnemyDefinition[] EnemyParty => enemyParty;
        public Tonality StartingTonality => startingTonality;
    }
}
