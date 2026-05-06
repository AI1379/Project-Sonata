using UnityEngine;

namespace ProjectSonata.Battle.Domain
{
    [CreateAssetMenu(menuName = "Project Sonata/Battle/Character", fileName = "Character_")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "New Character";

        [Header("Base Stats")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int attack = 10;
        [SerializeField] private int speed = 10;

        [Header("Loadout")]
        [SerializeField] private SkillDefinition[] skills;

        public string DisplayName => displayName;
        public int MaxHp => maxHp;
        public int Attack => attack;
        public int Speed => speed;
        public SkillDefinition[] Skills => skills;
    }
}
