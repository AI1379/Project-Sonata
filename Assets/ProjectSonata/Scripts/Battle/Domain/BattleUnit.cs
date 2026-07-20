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

        /// <summary>
        /// Battle timeline progress. Reaches <see cref="ActionTimeline.ReadyThreshold"/>
        /// when the unit is ready to act. See BattleDesign §9.
        /// </summary>
        public float ActionGauge { get; private set; }

        /// <summary>
        /// Active while the unit's ToneStack contains (or contained) a Perfect Fifth
        /// pair without yet triggering a triad. Consumed on the next resonance
        /// resolution. See BattleDesign §7 Perfect Fifth dual-path mechanism.
        /// </summary>
        public bool HasHarmonicArmor { get; private set; }

        public bool IsAlive => CurrentHp > 0;

        /// <summary>
        /// True when the unit's ActionGauge has reached the ready threshold.
        /// </summary>
        public bool IsReady => IsAlive && ActionGauge >= ActionTimeline.ReadyThreshold;

        public event Action<BattleUnit> OnHpChanged;
        public event Action<BattleUnit> OnHarmonicArmorChanged;
        public event Action<BattleUnit> OnActionGaugeChanged;

        public void ApplyHarmonicArmor()
        {
            if (HasHarmonicArmor)
                return;

            HasHarmonicArmor = true;
            OnHarmonicArmorChanged?.Invoke(this);
        }

        public bool ConsumeHarmonicArmor()
        {
            if (!HasHarmonicArmor)
                return false;

            HasHarmonicArmor = false;
            OnHarmonicArmorChanged?.Invoke(this);
            return true;
        }

        /// <summary>
        /// Advances the unit's ActionGauge by the given delta, clamped to
        /// a small overshoot above the ready threshold (so speed ties
        /// resolve in insertion order rather than piling up indefinitely).
        /// </summary>
        public void AdvanceGauge(float delta)
        {
            if (!IsAlive || IsReady)
                return;

            ActionGauge += delta;

            // Clamp to just above threshold to prevent runaway accumulation.
            var cap = ActionTimeline.ReadyThreshold * 1.5f;
            if (ActionGauge > cap)
                ActionGauge = cap;

            OnActionGaugeChanged?.Invoke(this);
        }

        /// <summary>
        /// Directly modifies the gauge (used by Tempo skills to push/pull allies/enemies).
        /// Values &lt; 0 reduce gauge; values &gt; 0 increase. Can push past threshold.
        /// </summary>
        public void ModifyGauge(float delta)
        {
            if (!IsAlive)
                return;

            ActionGauge = System.Math.Clamp(ActionGauge + delta, 0f, ActionTimeline.ReadyThreshold * 2f);
            OnActionGaugeChanged?.Invoke(this);
        }

        /// <summary>
        /// Consumes one ready action, dropping the gauge by the threshold.
        /// Called by ActionTimeline after a unit finishes acting.
        /// </summary>
        public void ConsumeAction()
        {
            if (ActionGauge >= ActionTimeline.ReadyThreshold)
            {
                ActionGauge -= ActionTimeline.ReadyThreshold;
                OnActionGaugeChanged?.Invoke(this);
            }
        }

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