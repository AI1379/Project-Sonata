using System;

namespace ProjectSonata.Battle.Domain
{
    public sealed class BattleUnit
    {
        public BattleUnit(string displayName, int maxHp, int attack, int speed, bool isPlayerUnit,
            SkillDefinition[] skills)
        {
            DisplayName = displayName;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Attack = attack;
            Speed = speed;
            IsPlayerUnit = isPlayerUnit;
            Skills = skills ?? System.Array.Empty<SkillDefinition>();
        }

        public string DisplayName { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; private set; }
        public int Attack { get; }
        public int Speed { get; }
        public bool IsPlayerUnit { get; }
        public SkillDefinition[] Skills { get; }
        public ToneStack ToneStack { get; } = new ToneStack();

        public bool IsAlive => CurrentHp > 0;

        public event Action<BattleUnit> OnHpChanged;

        public void TakeDamage(int amount)
        {
            if (amount < 0)
            {
                amount = 0;
            }

            if (amount == 0)
                return;

            CurrentHp -= amount;

            if (CurrentHp < 0)
            {
                CurrentHp = 0;
            }

            OnHpChanged?.Invoke(this);
        }

        public void Heal(int amount)
        {
            if (amount < 0)
            {
                amount = 0;
            }

            if (amount == 0)
                return;

            CurrentHp += amount;

            if (CurrentHp > MaxHp)
            {
                CurrentHp = MaxHp;
            }

            OnHpChanged?.Invoke(this);
        }

        public static BattleUnit FromCharacter(CharacterDefinition definition)
        {
            return new BattleUnit(
                definition.DisplayName,
                definition.MaxHp,
                definition.Attack,
                definition.Speed,
                true,
                definition.Skills);
        }

        public static BattleUnit FromEnemy(EnemyDefinition definition)
        {
            return new BattleUnit(
                definition.DisplayName,
                definition.MaxHp,
                definition.Attack,
                definition.Speed,
                false,
                definition.Skills);
        }
    }
}