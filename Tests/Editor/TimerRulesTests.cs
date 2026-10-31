using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DGP.UnityTimers.Editor.Tests
{
    public class TimerRulesTests
    {
        public static IEnumerable<TestCaseData> TimerKinds()
        {
            yield return Kind("Scaled", provider => new ScaledTimer(provider));
            yield return Kind("Countdown", provider => new CountdownTimer(provider, 1000f));
            yield return Kind("Interval", provider => new IntervalTimer(provider, 0.25f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void NewTimer_IsStoppedAndPassesNoTimeToChildren(Func<ITimeProvider, TimerBase> create)
        {
            var root = new TimeProvider();
            var timer = create(root);
            var children = new ReceivedTime(timer);

            root.Tick(1f);

            Assert.That(timer.State, Is.EqualTo(TimerState.Stopped));
            Assert.That(children.Total, Is.EqualTo(0f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void RunningTimer_PassesScaledTimeToChildren(Func<ITimeProvider, TimerBase> create)
        {
            var root = new TimeProvider();
            var timer = create(root);
            var children = new ReceivedTime(timer);

            timer.TimeScale = 0.5f;
            timer.Start();
            root.Tick(1f);

            Assert.That(children.Total, Is.EqualTo(0.5f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void PausedTimer_FreezesItsWholeSubtreeUntilResumed(Func<ITimeProvider, TimerBase> create)
        {
            var root = new TimeProvider();
            var timer = create(root);
            var child = new ScaledTimer(timer);
            var grandchildren = new ReceivedTime(child);

            timer.Start();
            child.Start();
            timer.Pause();
            root.Tick(1f);

            Assert.That(child.State, Is.EqualTo(TimerState.Running));
            Assert.That(grandchildren.Total, Is.EqualTo(0f));

            timer.Resume();
            root.Tick(1f);

            Assert.That(grandchildren.Total, Is.EqualTo(1f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void StoppedTimer_PassesNoTimeToChildren(Func<ITimeProvider, TimerBase> create)
        {
            var root = new TimeProvider();
            var timer = create(root);
            var children = new ReceivedTime(timer);

            timer.Start();
            timer.Stop();
            root.Tick(1f);

            Assert.That(children.Total, Is.EqualTo(0f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void EachStateChange_FiresOneEvent(Func<ITimeProvider, TimerBase> create)
        {
            var timer = create(new TimeProvider());
            var events = new StateEvents(timer);

            timer.Start();
            timer.Start();
            timer.Pause();
            timer.Pause();
            timer.Resume();
            timer.Resume();
            timer.Stop();
            timer.Stop();

            Assert.That(events.Fired, Is.EqualTo(new[] { "Started", "Paused", "Resumed", "Stopped" }));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void PauseAndResume_DoNothingWhenStopped(Func<ITimeProvider, TimerBase> create)
        {
            var timer = create(new TimeProvider());
            var events = new StateEvents(timer);

            timer.Pause();
            timer.Resume();

            Assert.That(timer.State, Is.EqualTo(TimerState.Stopped));
            Assert.That(events.Fired, Is.Empty);
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void StartWhilePaused_Resumes(Func<ITimeProvider, TimerBase> create)
        {
            var timer = create(new TimeProvider());
            var events = new StateEvents(timer);

            timer.Start();
            timer.Pause();
            timer.Start();

            Assert.That(timer.State, Is.EqualTo(TimerState.Running));
            Assert.That(events.Fired, Is.EqualTo(new[] { "Started", "Paused", "Resumed" }));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void Restart_StopsThenStarts(Func<ITimeProvider, TimerBase> create)
        {
            var timer = create(new TimeProvider());
            var events = new StateEvents(timer);

            timer.Restart();
            timer.Restart();

            Assert.That(timer.State, Is.EqualTo(TimerState.Running));
            Assert.That(events.Fired, Is.EqualTo(new[] { "Started", "Stopped", "Started" }));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void ZeroTimeScale_PassesNoTicksToChildren(Func<ITimeProvider, TimerBase> create)
        {
            var root = new TimeProvider();
            var timer = create(root);
            var children = new ReceivedTime(timer);

            timer.TimeScale = 0f;
            timer.Start();
            root.Tick(1f);

            Assert.That(timer.State, Is.EqualTo(TimerState.Running));
            Assert.That(children.Ticks, Is.EqualTo(0));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void NegativeTimeScale_PassesBackwardsTimeToChildren(Func<ITimeProvider, TimerBase> create)
        {
            var root = new TimeProvider();
            var timer = create(root);

            timer.Start();
            root.Tick(1f);

            var children = new ReceivedTime(timer);
            timer.TimeScale = -0.5f;
            root.Tick(1f);

            Assert.That(children.Total, Is.EqualTo(-0.5f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void Dispose_DetachesFromItsTimeProvider(Func<ITimeProvider, TimerBase> create)
        {
            var provider = new CountingTimeProvider();
            var timer = create(provider);

            timer.Dispose();

            Assert.That(provider.HandlerCount, Is.EqualTo(0));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void Dispose_StopsSilentlyAndIsSafeToRepeat(Func<ITimeProvider, TimerBase> create)
        {
            var timer = create(new TimeProvider());
            timer.Start();
            var events = new StateEvents(timer);

            timer.Dispose();
            timer.Dispose();

            Assert.That(timer.State, Is.EqualTo(TimerState.Stopped));
            Assert.That(events.Fired, Is.Empty);
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void DisposedTimer_RefusesToChangeState(Func<ITimeProvider, TimerBase> create)
        {
            var timer = create(new TimeProvider());

            timer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => timer.Start());
            Assert.Throws<ObjectDisposedException>(() => timer.Pause());
            Assert.Throws<ObjectDisposedException>(() => timer.Resume());
            Assert.Throws<ObjectDisposedException>(() => timer.Restart());
            Assert.Throws<ObjectDisposedException>(() => timer.Reset());
            Assert.Throws<ObjectDisposedException>(() => timer.Tick(1f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void AttachedTimer_CannotBeTickedByHand(Func<ITimeProvider, TimerBase> create)
        {
            var timer = create(new TimeProvider());

            Assert.Throws<InvalidOperationException>(() => timer.Tick(1f));
        }

        [TestCaseSource(nameof(UnattachedTimerKinds))]
        public void UnattachedTimer_IsTickedByHand(Func<TimerBase> create)
        {
            var timer = create();
            var children = new ReceivedTime(timer);

            timer.Start();
            timer.Tick(0.5f);

            Assert.That(children.Total, Is.EqualTo(0.5f));
        }

        [TestCaseSource(nameof(TimerKinds))]
        public void DisposingDuringATick_DoesNotThrowOrStarveSiblings(Func<ITimeProvider, TimerBase> create)
        {
            var root = new TimeProvider();
            var timer = create(root);
            var sibling = new ScaledTimer(root);
            var siblingChildren = new ReceivedTime(sibling);

            timer.AddHandler(_ => timer.Dispose());
            timer.Start();
            sibling.Start();

            Assert.DoesNotThrow(() => root.Tick(1f));
            Assert.That(siblingChildren.Total, Is.EqualTo(1f));
        }

        public static IEnumerable<TestCaseData> UnattachedTimerKinds()
        {
            yield return new TestCaseData((Func<TimerBase>)(() => new ScaledTimer())).SetName("{m}(Scaled)");
            yield return new TestCaseData((Func<TimerBase>)(() => new CountdownTimer(1000f))).SetName("{m}(Countdown)");
            yield return new TestCaseData((Func<TimerBase>)(() => new IntervalTimer(0.25f))).SetName("{m}(Interval)");
        }

        private static TestCaseData Kind(string name, Func<ITimeProvider, TimerBase> create) =>
            new TestCaseData(create).SetName($"{{m}}({name})");
    }
}
