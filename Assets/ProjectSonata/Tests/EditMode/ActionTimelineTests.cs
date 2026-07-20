using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectSonata.Battle.Domain;

namespace ProjectSonata.Tests.EditMode
{
    public class ActionTimelineTests
    {
        private static BattleUnit MakeUnit(string name, int speed, bool isPlayer = true)
        {
            return new BattleUnit(name, maxHp: 1000, attack: 10, speed: speed, isPlayerUnit: isPlayer, skills: null);
        }

        // ──────────────────────────────────────────────────────────────
        // AdvanceToNextReady — single-unit cases
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Advance_FastestUnitBecomesReadyFirst()
        {
            var slow = MakeUnit("Slow", speed: 10);
            var fast = MakeUnit("Fast", speed: 50);

            var timeline = new ActionTimeline();
            timeline.Add(slow);
            timeline.Add(fast);

            var ready = timeline.AdvanceToNextReady();
            Assert.AreSame(fast, ready);
            Assert.IsTrue(fast.IsReady);
            Assert.IsFalse(slow.IsReady);
        }

        [Test]
        public void Advance_ThenConsume_ProducesNextUnitInOrder()
        {
            var slow = MakeUnit("Slow", speed: 10);
            var fast = MakeUnit("Fast", speed: 50);

            var timeline = new ActionTimeline();
            timeline.Add(slow);
            timeline.Add(fast);

            // First turn: Fast (speed 50, needs 100/50 = 2 virtual time)
            var first = timeline.AdvanceToNextReady();
            Assert.AreSame(fast, first);
            timeline.ConsumeAction(fast);

            // Slow should have advanced by 2 time → gauge = 20
            Assert.AreEqual(20f, slow.ActionGauge, 0.01f);

            // Second turn: Fast again (needs 100/50 = 2 more time, slow advances to 40)
            var second = timeline.AdvanceToNextReady();
            Assert.AreSame(fast, second);
            timeline.ConsumeAction(fast);
            Assert.AreEqual(40f, slow.ActionGauge, 0.01f);

            // Third turn: Fast AGAIN (slow advances to 60)
            var third = timeline.AdvanceToNextReady();
            Assert.AreSame(fast, third);
            timeline.ConsumeAction(fast);
            Assert.AreEqual(60f, slow.ActionGauge, 0.01f);

            // Fourth turn: Fast AGAIN (slow advances to 80)
            var fourth = timeline.AdvanceToNextReady();
            Assert.AreSame(fast, fourth);
            timeline.ConsumeAction(fast);
            Assert.AreEqual(80f, slow.ActionGauge, 0.01f);

            // Fifth turn: Slow finally reaches 100 first
            // Fast needs 2 time to reach 100 again (slow would be at 100)
            // Actually: fast from 0 needs 2 time → slow from 80 → 100, fast → 100. Tie.
            // Tie resolution should pick the one earlier in insertion order = slow.
            var fifth = timeline.AdvanceToNextReady();
            Assert.AreSame(slow, fifth);
        }

        // ──────────────────────────────────────────────────────────────
        // Tie-breaking
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Advance_TieBreaksByInsertionOrder()
        {
            var a = MakeUnit("A", speed: 50);
            var b = MakeUnit("B", speed: 50);

            var timeline = new ActionTimeline();
            timeline.Add(a);
            timeline.Add(b);

            var ready = timeline.AdvanceToNextReady();
            Assert.AreSame(a, ready, "Earlier-inserted unit should win ties");
        }

        // ──────────────────────────────────────────────────────────────
        // Speed = 0 (frozen)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Advance_SkipsZeroSpeedUnits()
        {
            var frozen = MakeUnit("Frozen", speed: 0);
            var normal = MakeUnit("Normal", speed: 25);

            var timeline = new ActionTimeline();
            timeline.Add(frozen);
            timeline.Add(normal);

            var ready = timeline.AdvanceToNextReady();
            Assert.AreSame(normal, ready);
            Assert.AreEqual(0f, frozen.ActionGauge, "Frozen unit should not advance");
        }

        // ──────────────────────────────────────────────────────────────
        // Dead units
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void Advance_DeadUnitSkipped()
        {
            var dead = MakeUnit("Dead", speed: 100);
            var alive = MakeUnit("Alive", speed: 10);

            dead.TakeDamage(10000); // kill

            var timeline = new ActionTimeline();
            timeline.Add(dead);
            timeline.Add(alive);

            var ready = timeline.AdvanceToNextReady();
            Assert.AreSame(alive, ready);
        }

        [Test]
        public void Remove_RemovesFromScheduling()
        {
            var a = MakeUnit("A", speed: 50);
            var b = MakeUnit("B", speed: 50);

            var timeline = new ActionTimeline();
            timeline.Add(a);
            timeline.Add(b);
            timeline.Remove(a);

            var ready = timeline.AdvanceToNextReady();
            Assert.AreSame(b, ready);
        }

        // ──────────────────────────────────────────────────────────────
        // Gauge modification (Tempo skills)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void ModifyGauge_PushesTargetIntoReady()
        {
            var unit = MakeUnit("U", speed: 10);
            unit.ModifyGauge(95f);
            Assert.AreEqual(95f, unit.ActionGauge);
            Assert.IsFalse(unit.IsReady);

            unit.ModifyGauge(10f);
            Assert.IsTrue(unit.IsReady);
        }

        [Test]
        public void ModifyGauge_NegativePullsBackFromReady()
        {
            var unit = MakeUnit("U", speed: 10);
            unit.ModifyGauge(100f);
            Assert.IsTrue(unit.IsReady);

            unit.ModifyGauge(-50f);
            Assert.IsFalse(unit.IsReady);
            Assert.AreEqual(50f, unit.ActionGauge);
        }

        // ──────────────────────────────────────────────────────────────
        // GetProgress (for UI)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void GetProgress_ReturnsClampedPercent()
        {
            var unit = MakeUnit("U", speed: 10);
            var timeline = new ActionTimeline();
            timeline.Add(unit);

            unit.ModifyGauge(50f);
            Assert.AreEqual(0.5f, timeline.GetProgress(unit), 0.001f);

            unit.ModifyGauge(200f);
            Assert.AreEqual(1f, timeline.GetProgress(unit), 0.001f, "Should clamp to 1.0");
        }

        // ──────────────────────────────────────────────────────────────
        // GetReadinessSnapshot (for timeline UI)
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void GetReadinessSnapshot_ReturnsUnitsByGaugeDescending()
        {
            var a = MakeUnit("A", speed: 10);
            var b = MakeUnit("B", speed: 10);
            var c = MakeUnit("C", speed: 10);

            // Different gauges
            a.ModifyGauge(30f);
            b.ModifyGauge(80f);
            c.ModifyGauge(50f);

            var timeline = new ActionTimeline();
            timeline.Add(a);
            timeline.Add(b);
            timeline.Add(c);

            var snapshot = timeline.GetReadinessSnapshot();
            Assert.AreEqual(3, snapshot.Count);
            Assert.AreSame(b, snapshot[0], "Highest gauge first");
            Assert.AreSame(c, snapshot[1]);
            Assert.AreSame(a, snapshot[2]);
        }

        [Test]
        public void GetReadinessSnapshot_CurrentReadyAlwaysFirst()
        {
            var a = MakeUnit("A", speed: 10);
            var b = MakeUnit("B", speed: 10);

            var timeline = new ActionTimeline();
            timeline.Add(a);
            timeline.Add(b);

            // Advance so a becomes ready first
            timeline.AdvanceToNextReady();
            Assert.AreSame(a, timeline.CurrentReady);

            // Even if b has higher gauge, a (CurrentReady) should be first in snapshot.
            b.ModifyGauge(95f);
            var snapshot = timeline.GetReadinessSnapshot();
            Assert.AreSame(a, snapshot[0]);
            Assert.AreSame(b, snapshot[1]);
        }

        [Test]
        public void GetReadinessSnapshot_ExcludesDeadUnits()
        {
            var a = MakeUnit("A", speed: 10);
            var dead = MakeUnit("Dead", speed: 10);
            dead.TakeDamage(10000);

            var timeline = new ActionTimeline();
            timeline.Add(a);
            timeline.Add(dead);

            var snapshot = timeline.GetReadinessSnapshot();
            Assert.AreEqual(1, snapshot.Count);
            Assert.AreSame(a, snapshot[0]);
        }

        // ──────────────────────────────────────────────────────────────
        // EnemyAI
        // ──────────────────────────────────────────────────────────────

        [Test]
        public void EnemyAI_PicksRandomLivingPlayer()
        {
            var enemy = MakeUnit("Enemy", speed: 10, isPlayer: false);
            var p1 = MakeUnit("P1", speed: 10);
            var p2 = MakeUnit("P2", speed: 10);
            var players = new[] { p1, p2 };

            var ai = new EnemyAI();
            var target = ai.PickTarget(enemy, players);
            Assert.IsNotNull(target);
            Assert.Contains(target, new[] { p1, p2 });
        }

        [Test]
        public void EnemyAI_SkipsDeadPlayers()
        {
            var enemy = MakeUnit("Enemy", speed: 10, isPlayer: false);
            var dead = MakeUnit("Dead", speed: 10);
            var alive = MakeUnit("Alive", speed: 10);
            dead.TakeDamage(10000);

            var ai = new EnemyAI();
            var target = ai.PickTarget(enemy, new[] { dead, alive });
            Assert.AreSame(alive, target);
        }

        [Test]
        public void EnemyAI_ExecuteAction_DealsAttackDamage()
        {
            var enemy = MakeUnit("Enemy", speed: 10, isPlayer: false);
            var player = MakeUnit("Player", speed: 10);
            var hpBefore = player.CurrentHp;

            var ai = new EnemyAI();
            var action = ai.ExecuteAction(enemy, player);

            Assert.AreEqual(player, action.Target);
            Assert.AreEqual(enemy.Attack, action.Damage);
            Assert.AreEqual(hpBefore - enemy.Attack, player.CurrentHp);
            Assert.IsFalse(string.IsNullOrEmpty(action.LogMessage));
        }
    }
}
