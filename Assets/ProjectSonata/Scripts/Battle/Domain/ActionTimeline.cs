using System;
using System.Collections.Generic;

namespace ProjectSonata.Battle.Domain
{
    /// <summary>
    /// Phase 1 discrete-advance action timeline. See BattleDesign §9.
    ///
    /// "Discrete" means no real-time ticking — instead of accumulating
    /// gauge over deltaTime, we look at all units and ask:
    ///   "How much virtual time must pass before the next unit becomes ready?"
    /// Then we advance everyone by that amount in a single step.
    ///
    /// This avoids needing Coroutines, deltaTime, or pause logic for the
    /// prototype. Skills with Tempo effects (push/pull gauges) just modify
    /// ActionGauge directly via BattleUnit.ModifyGauge.
    /// </summary>
    public sealed class ActionTimeline
    {
        public const float ReadyThreshold = 100f;

        private readonly List<BattleUnit> _units = new List<BattleUnit>();

        /// <summary>
        /// The unit currently ready to act (gauge ≥ threshold), or null
        /// if none is currently ready. Set by AdvanceToNextReady.
        /// </summary>
        public BattleUnit CurrentReady { get; private set; }

        public event Action<BattleUnit> OnUnitBecameReady;
        public event Action OnTimelineChanged;

        public void Add(BattleUnit unit)
        {
            if (unit == null || _units.Contains(unit))
                return;

            _units.Add(unit);
            unit.OnActionGaugeChanged += HandleUnitGaugeChanged;
        }

        public void Remove(BattleUnit unit)
        {
            if (unit == null)
                return;

            unit.OnActionGaugeChanged -= HandleUnitGaugeChanged;
            _units.Remove(unit);
            if (CurrentReady == unit)
                CurrentReady = null;
        }

        private void HandleUnitGaugeChanged(BattleUnit unit)
        {
            // Bubble up so UI subscribers can refresh their snapshot.
            OnTimelineChanged?.Invoke();
        }

        public IReadOnlyList<BattleUnit> Units => _units;

        /// <summary>
        /// Advances virtual time until exactly one unit becomes ready.
        /// If a unit is already ready, this is a no-op.
        /// </summary>
        public BattleUnit AdvanceToNextReady()
        {
            // Clear any previously-consumed ready state.
            if (CurrentReady != null && !CurrentReady.IsReady)
                CurrentReady = null;

            if (CurrentReady != null)
                return CurrentReady;

            BattleUnit nextUnit = null;
            var minTimeToReady = float.MaxValue;

            foreach (var unit in _units)
            {
                if (unit == null || !unit.IsAlive)
                    continue;

                if (unit.IsReady)
                {
                    // Already ready — pick it immediately, no advance needed.
                    nextUnit = unit;
                    minTimeToReady = 0f;
                    break;
                }

                if (unit.Speed <= 0)
                    continue;

                var remaining = ReadyThreshold - unit.ActionGauge;
                var timeToReady = remaining / unit.Speed;

                if (timeToReady < minTimeToReady ||
                    (Math.Abs(timeToReady - minTimeToReady) < 0.0001f && nextUnit != null &&
                     _units.IndexOf(unit) < _units.IndexOf(nextUnit)))
                {
                    minTimeToReady = timeToReady;
                    nextUnit = unit;
                }
            }

            if (nextUnit == null)
                return null;

            // Advance everyone (including the chosen unit) by minTimeToReady.
            if (minTimeToReady > 0f)
            {
                foreach (var unit in _units)
                {
                    if (unit == null || !unit.IsAlive || unit.IsReady)
                        continue;

                    var delta = unit.Speed * minTimeToReady;
                    unit.AdvanceGauge(delta);
                }
            }

            // Guarantee the chosen unit hits exactly the threshold
            // (floating-point safety; AdvanceGauge clamps to 1.5x threshold).
            if (!nextUnit.IsReady)
            {
                nextUnit.ModifyGauge(ReadyThreshold - nextUnit.ActionGauge);
            }

            CurrentReady = nextUnit;
            OnUnitBecameReady?.Invoke(nextUnit);
            OnTimelineChanged?.Invoke();
            return nextUnit;
        }

        /// <summary>
        /// Called by the runtime after a unit finishes its action.
        /// Drops the unit's gauge by the threshold and clears CurrentReady.
        /// </summary>
        public void ConsumeAction(BattleUnit unit)
        {
            if (unit == null)
                return;

            unit.ConsumeAction();

            if (CurrentReady == unit)
                CurrentReady = null;

            OnTimelineChanged?.Invoke();
        }

        /// <summary>
        /// Returns a 0-1 progress value for UI display.
        /// </summary>
        public float GetProgress(BattleUnit unit)
        {
            if (unit == null)
                return 0f;

            return Math.Clamp(unit.ActionGauge / ReadyThreshold, 0f, 1f);
        }

        /// <summary>
        /// Returns living units ordered by "readiness" (ActionGauge descending).
        /// The unit returned first is the closest to acting next.
        ///
        /// Phase 1 simplification: this is a snapshot of current gauge values,
        /// NOT a precise prediction of future turn order. Because units have
        /// different speeds, the actual next-ready unit may differ from the
        /// one with the current-highest gauge. For display purposes only.
        /// </summary>
        public IReadOnlyList<BattleUnit> GetReadinessSnapshot()
        {
            var snapshot = new List<BattleUnit>(_units.Count);
            foreach (var unit in _units)
            {
                if (unit != null && unit.IsAlive)
                    snapshot.Add(unit);
            }

            snapshot.Sort((a, b) =>
            {
                // CurrentReady always first
                if (a == CurrentReady) return -1;
                if (b == CurrentReady) return 1;
                // Otherwise by gauge descending
                return b.ActionGauge.CompareTo(a.ActionGauge);
            });

            return snapshot;
        }
    }
}
