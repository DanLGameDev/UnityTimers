using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DGP.UnityTimers.PlayerLoop;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.TestTools;
using FixedUpdate = UnityEngine.PlayerLoop.FixedUpdate;
using Update = UnityEngine.PlayerLoop.Update;

namespace DGP.UnityTimers.Editor.Tests
{
    public class TimeProvidersTests
    {
        private class Root { }
        private class First { }
        private class Second { }

        [TearDown]
        public void ClearProviders() => TimeProviders.ClearSubscribers();

        public static IEnumerable<TestCaseData> HookPoints()
        {
            yield return Hook("BeforeUpdate", TimeProviders.BeforeUpdateTimeProvider, typeof(Update), first: true);
            yield return Hook("AfterUpdate", TimeProviders.AfterUpdateTimeProvider, typeof(Update), first: false);
            yield return Hook("BeforeFixedUpdate", TimeProviders.BeforeFixedUpdateTimeProvider, typeof(FixedUpdate), first: true);
            yield return Hook("AfterFixedUpdate", TimeProviders.AfterFixedUpdateTimeProvider, typeof(FixedUpdate), first: false);
        }

        [TestCaseSource(nameof(HookPoints))]
        public void Install_PutsEachProviderAtItsHookPoint(ITimeProvider provider, Type anchor, bool first)
        {
            var received = new ReceivedTime(provider);
            var loop = TimeProviders.Install(FakeLoop());

            var children = Find(loop, anchor).subSystemList;
            var hook = first ? children.First() : children.Last();
            hook.updateDelegate();

            Assert.That(received.Ticks, Is.EqualTo(1));
        }

        [Test]
        public void Install_Twice_LeavesOneCopyOfEachSystem()
        {
            var loop = TimeProviders.Install(TimeProviders.Install(FakeLoop()));

            Assert.That(Find(loop, typeof(Update)).subSystemList.Length, Is.EqualTo(4));
            Assert.That(Find(loop, typeof(FixedUpdate)).subSystemList.Length, Is.EqualTo(3));
        }

        [Test]
        public void Install_WithMissingAnchor_LogsAndStillInstallsTheOtherHooks()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<First>(), Sys<Second>()));

            LogAssert.Expect(LogType.Error, new Regex($"{nameof(FixedUpdate)} not found"));
            LogAssert.Expect(LogType.Error, new Regex($"{nameof(FixedUpdate)} not found"));
            var installed = TimeProviders.Install(loop);

            Assert.That(Find(installed, typeof(Update)).subSystemList.Length, Is.EqualTo(4));
        }

        [Test]
        public void Uninstall_RestoresTheLoopAsItWasBeforeInstall()
        {
            var loop = TimeProviders.Uninstall(TimeProviders.Install(FakeLoop()));

            Assert.That(Find(loop, typeof(Update)).subSystemList.Select(s => s.type), Is.EqualTo(new[] { typeof(First), typeof(Second) }));
            Assert.That(Find(loop, typeof(FixedUpdate)).subSystemList.Select(s => s.type), Is.EqualTo(new[] { typeof(First) }));
        }

        [Test]
        public void ProviderTick_DeliversTimeToAttachedTimers()
        {
            var received = new ReceivedTime(TimeProviders.BeforeUpdateTimeProvider);

            TimeProviders.BeforeUpdateTimeProvider.Tick(0.5f);

            Assert.That(received.Total, Is.EqualTo(0.5f));
        }

        [Test]
        public void ClearSubscribers_StopsDeliveryToEarlierSubscribers()
        {
            var received = new ReceivedTime(TimeProviders.AfterFixedUpdateTimeProvider);

            TimeProviders.ClearSubscribers();
            TimeProviders.AfterFixedUpdateTimeProvider.Tick(1f);

            Assert.That(received.Ticks, Is.EqualTo(0));
        }

        private static TestCaseData Hook(string name, ITimeProvider provider, Type anchor, bool first) =>
            new TestCaseData(provider, anchor, first).SetName($"Install_PutsEachProviderAtItsHookPoint({name})");

        private static PlayerLoopSystem FakeLoop() =>
            Sys<Root>(Sys<Update>(Sys<First>(), Sys<Second>()), Sys<FixedUpdate>(Sys<First>()));

        private static PlayerLoopSystem Find(PlayerLoopSystem loop, Type type) =>
            loop.subSystemList.First(system => system.type == type);

        private static PlayerLoopSystem Sys<T>(params PlayerLoopSystem[] children) =>
            new PlayerLoopSystem { type = typeof(T), subSystemList = children.Length == 0 ? null : children };
    }
}
