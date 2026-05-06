using UnityEngine;

namespace ProjectSonata.Battle.Domain
{
    [CreateAssetMenu(menuName = "Project Sonata/Battle/Enemy", fileName = "Enemy_")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "New Enemy";

        [Header("Base Stats")]
        [SerializeField] private int maxHp = 80;
        [SerializeField] private int attack = 8;
        [SerializeField] private int speed = 8;

        [Header("Loadout")]
        [SerializeField] private SkillDefinition[] skills;

        public string DisplayName => displayName;
        public int MaxHp => maxHp;
        public int Attack => attack;
        public int Speed => speed;
        public SkillDefinition[] Skills => skills;
    }
}
