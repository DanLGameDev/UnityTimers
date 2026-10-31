using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DGP.UnityTimers.Editor.Tests
{
    public class IntervalTimerTests
    {
        [Test]
        public void RunningInterval_FiresOncePerIntervalAndPassesWholeSteps()
        {
            var root = new TimeProvider();
            var interval = new IntervalTimer(root, 0.25f);
            var children = new ReceivedTime(interval);
            var fired = 0;

            interval.OnInterval += () => fired++;
            interval.Start();
            root.Tick(0.625f);

            Assert.That(interval.Interval, Is.EqualTo(0.25f));
            Assert.That(fired, Is.EqualTo(2));
            Assert.That(children.Ticks, Is.EqualTo(2));
            Assert.That(children.Total, Is.EqualTo(0.5f));

            root.Tick(0.375f);

            Assert.That(fired, Is.EqualTo(4));
            Assert.That(children.Total, Is.EqualTo(1f));
        }

        [Test]
        public void ChildrenReceiveEachStepBeforeItsInterval()
        {
            var root = new TimeProvider();
            var interval = new IntervalTimer(root, 0.25f);
            var order = new List<string>();

            interval.AddHandler(_ => order.Add("Step"));
            interval.OnInterval += () => order.Add("Interval");
            interval.Start();
            root.Tick(0.5f);

            Assert.That(order, Is.EqualTo(new[] { "Step", "Interval", "Step", "Interval" }));
        }

        [Test]
        public void LongTick_CatchesUpEveryStep()
        {
            var root = new TimeProvider();
            var interval = new IntervalTimer(root, 0.25f);
            var fired = 0;

            interval.OnInterval += () => fired++;
            interval.Start();
            root.Tick(2f);

            Assert.That(fired, Is.EqualTo(8));
        }

        [Test]
        public void StoppingDuringAnInterval_SkipsTheRemainingSteps()
        {
            var root = new TimeProvider();
            var interval = new IntervalTimer(root, 0.25f);
            var fired = 0;

            interval.OnInterval += () => {
                fired++;
                interval.Stop();
            };
            interval.Start();
            root.Tick(1f);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void StartAfterStop_ClearsPartialProgress()
        {
            var root = new TimeProvider();
            var interval = new IntervalTimer(root, 0.25f);
            var fired = 0;

            interval.OnInterval += () => fired++;
            interval.Start();
            root.Tick(0.125f);
            interval.Stop();
            interval.Start();
            root.Tick(0.125f);

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void PauseAndResume_KeepPartialProgress()
        {
            var root = new TimeProvider();
            var interval = new IntervalTimer(root, 0.25f);
            var fired = 0;

            interval.OnInterval += () => fired++;
            interval.Start();
            root.Tick(0.125f);
            interval.Pause();
            interval.Resume();
            root.Tick(0.125f);

            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void BackwardsTime_StepsBackAcrossEachBoundary()
        {
            var root = new TimeProvider();
            var interval = new IntervalTimer(root, 0.25f);
            var children = new ReceivedTime(interval);
            var fired = 0;

            interval.OnInterval += () => fired++;
            interval.Start();
            root.Tick(0.625f);
            interval.TimeScale = -1f;
            root.Tick(0.25f);

            Assert.That(fired, Is.EqualTo(3));
            Assert.That(children.Total, Is.EqualTo(0.25f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveInterval_Throws(float length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new IntervalTimer(new TimeProvider(), length));
            Assert.Throws<ArgumentOutOfRangeException>(() => new IntervalTimer(length));
        }
    }
}
