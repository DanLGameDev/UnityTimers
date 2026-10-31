using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DGP.UnityTimers.Editor.Tests
{
    public class CountdownTimerTests
    {
        [Test]
        public void RunningCountdown_TracksElapsedAndRemainingTime()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 2f);

            countdown.Start();
            root.Tick(0.5f);

            Assert.That(countdown.Duration, Is.EqualTo(2f));
            Assert.That(countdown.RemainingTime, Is.EqualTo(1.5f));
            Assert.That(countdown.ElapsedTime, Is.EqualTo(0.5f));
            Assert.That(countdown.ElapsedTimeNormalized, Is.EqualTo(0.25f));
            Assert.That(countdown.RemainingTimeNormalized, Is.EqualTo(0.75f));
            Assert.That(countdown.IsComplete, Is.False);
        }

        [Test]
        public void ReachingZero_UpdatesThenStopsThenCompletes()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 1f);
            var fired = new List<string>();

            countdown.Start();
            countdown.OnUpdated += remaining => fired.Add($"Updated({remaining})");
            countdown.OnStopped += () => fired.Add("Stopped");
            countdown.OnComplete += () => fired.Add("Complete");
            root.Tick(1.5f);

            Assert.That(fired, Is.EqualTo(new[] { "Updated(0)", "Stopped", "Complete" }));
            Assert.That(countdown.State, Is.EqualTo(TimerState.Stopped));
            Assert.That(countdown.IsComplete, Is.True);
            Assert.That(countdown.RemainingTime, Is.EqualTo(0f));
        }

        [Test]
        public void CompletingCountdown_PassesOnlyTheTimeItUsed()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 1f);
            var children = new ReceivedTime(countdown);

            countdown.Start();
            root.Tick(1.5f);
            root.Tick(1f);

            Assert.That(children.Total, Is.EqualTo(1f));
        }

        [Test]
        public void StoppedCountdown_KeepsItsValuesUntilStartedAgain()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 2f);

            countdown.Start();
            root.Tick(0.5f);
            countdown.Stop();

            Assert.That(countdown.RemainingTime, Is.EqualTo(1.5f));

            countdown.Start();

            Assert.That(countdown.RemainingTime, Is.EqualTo(2f));
        }

        [Test]
        public void PauseAndResume_KeepProgress()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 2f);

            countdown.Start();
            root.Tick(0.5f);
            countdown.Pause();
            root.Tick(1f);
            countdown.Resume();
            root.Tick(0.5f);

            Assert.That(countdown.RemainingTime, Is.EqualTo(1f));
        }

        [Test]
        public void Reset_ZeroesProgressWithoutChangingStateOrFiringEvents()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 2f);

            countdown.Start();
            root.Tick(0.5f);
            var events = new StateEvents(countdown);
            countdown.Reset();

            Assert.That(countdown.RemainingTime, Is.EqualTo(2f));
            Assert.That(countdown.State, Is.EqualTo(TimerState.Running));
            Assert.That(events.Fired, Is.Empty);
        }

        [Test]
        public void StartingFromOnComplete_RunsAgainFromTheFullDuration()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 1f);

            countdown.OnComplete += countdown.Start;
            countdown.Start();
            root.Tick(1.5f);

            Assert.That(countdown.State, Is.EqualTo(TimerState.Running));
            Assert.That(countdown.RemainingTime, Is.EqualTo(1f));
        }

        [Test]
        public void BackwardsTime_RewindsNoFurtherThanTheFullDuration()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 2f);
            var children = new ReceivedTime(countdown);

            countdown.Start();
            root.Tick(1f);
            countdown.TimeScale = -1f;
            root.Tick(0.5f);

            Assert.That(countdown.RemainingTime, Is.EqualTo(1.5f));

            root.Tick(5f);

            Assert.That(countdown.RemainingTime, Is.EqualTo(2f));
            Assert.That(countdown.State, Is.EqualTo(TimerState.Running));
            Assert.That(children.Total, Is.EqualTo(0f));
        }

        [Test]
        public void CompletedCountdown_StaysCompleteWhenTimeRunsBackwards()
        {
            var root = new TimeProvider();
            var countdown = new CountdownTimer(root, 1f);

            countdown.Start();
            root.Tick(1f);
            countdown.TimeScale = -1f;
            root.Tick(0.5f);

            Assert.That(countdown.IsComplete, Is.True);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void NonPositiveDuration_Throws(float duration)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(new TimeProvider(), duration));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(duration));
        }
    }
}
