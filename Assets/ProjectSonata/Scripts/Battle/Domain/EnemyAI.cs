using System;
using System.Collections.Generic;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Phase 1 placeholder enemy AI. See BattleDesign §10.
    ///
    /// Current behavior: pick a random living player and attack them for
    /// the enemy's Attack value. No tone application, no resonance.
    ///
    /// Designed to be replaced by a ScriptableObject-driven behavior tree
    /// in a later phase, but the PickTarget / ChooseAction split is already
    /// in place so future AIs can slot in without changing the runtime.
    /// </summary>
    public sealed class EnemyAI
    {
        private readonly Random _rng;

        public EnemyAI() : this(new Random())
        {
        }

        public EnemyAI(Random rng)
        {
            _rng = rng ?? new Random();
        }

        /// <summary>
        /// Picks the target for the acting enemy. Phase 1: random living player.
        /// Returns null if no player is alive (battle should have ended).
        /// </summary>
        public BattleUnit PickTarget(BattleUnit actor, IReadOnlyList<BattleUnit> playerUnits)
        {
            if (playerUnits == null || playerUnits.Count == 0)
                return null;

            var living = new List<BattleUnit>(playerUnits.Count);
            foreach (var u in playerUnits)
            {
                if (u != null && u.IsAlive)
                    living.Add(u);
            }

            if (living.Count == 0)
                return null;

            var index = _rng.Next(living.Count);
            return living[index];
        }

        /// <summary>
        /// Executes the enemy's action against the chosen target.
        /// Phase 1: a basic attack dealing the actor's Attack value.
        /// </summary>
        public EnemyAction ExecuteAction(BattleUnit actor, BattleUnit target)
        {
            if (actor == null || target == null || !target.IsAlive)
                return new EnemyAction(null, 0, "Enemy skipped — no valid target.");

            var damage = actor.Attack;
            target.TakeDamage(damage);

            return new EnemyAction(target, damage,
                $"{actor.DisplayName} attacks {target.DisplayName} for {damage} damage.");
        }
    }

    public readonly struct EnemyAction
    {
        public BattleUnit Target { get; }
        public int Damage { get; }
        public string LogMessage { get; }

        public EnemyAction(BattleUnit target, int damage, string logMessage)
        {
            Target = target;
            Damage = damage;
            LogMessage = logMessage ?? string.Empty;
        }
    }
}
