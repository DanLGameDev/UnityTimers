using System;
using System.Linq;
using DGP.UnityTimers.PlayerLoop;
using NUnit.Framework;
using UnityEngine.LowLevel;

namespace DGP.UnityTimers.Editor.Tests
{
    public class PlayerLoopUtilsTests
    {
        private class Root { }
        private class Update { }
        private class FixedUpdate { }
        private class First { }
        private class Second { }
        private class Injected { }

        [Test]
        public void InsertAsFirstChild_PutsSystemBeforeTheAnchorsChildren()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<First>(), Sys<Second>()));

            var inserted = PlayerLoopUtils.TryInsert(loop, typeof(Update), LoopPosition.FirstChildOf, Sys<Injected>(), out var result);

            Assert.That(inserted, Is.True);
            Assert.That(Describe(result), Is.EqualTo("Root(Update(Injected,First,Second))"));
        }

        [Test]
        public void InsertAsLastChild_PutsSystemAfterTheAnchorsChildren()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<First>(), Sys<Second>()));

            PlayerLoopUtils.TryInsert(loop, typeof(Update), LoopPosition.LastChildOf, Sys<Injected>(), out var result);

            Assert.That(Describe(result), Is.EqualTo("Root(Update(First,Second,Injected))"));
        }

        [Test]
        public void InsertAsChild_OfAnchorWithNoChildren_MakesItTheOnlyChild()
        {
            var loop = Sys<Root>(Sys<Update>());

            PlayerLoopUtils.TryInsert(loop, typeof(Update), LoopPosition.FirstChildOf, Sys<Injected>(), out var result);

            Assert.That(Describe(result), Is.EqualTo("Root(Update(Injected))"));
        }

        [Test]
        public void Insert_FindsAnchorNestedDeeperInTheTree()
        {
            var loop = Sys<Root>(Sys<FixedUpdate>(), Sys<Update>(Sys<First>(Sys<Second>())));

            PlayerLoopUtils.TryInsert(loop, typeof(First), LoopPosition.LastChildOf, Sys<Injected>(), out var result);

            Assert.That(Describe(result), Is.EqualTo("Root(FixedUpdate,Update(First(Second,Injected)))"));
        }

        [Test]
        public void Insert_LeavesTheOriginalLoopUnchanged()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<First>(Sys<Second>())));

            PlayerLoopUtils.TryInsert(loop, typeof(First), LoopPosition.FirstChildOf, Sys<Injected>(), out _);

            Assert.That(Describe(loop), Is.EqualTo("Root(Update(First(Second)))"));
        }

        [Test]
        public void Insert_WithMissingAnchor_FailsAndReturnsTheLoopUnchanged()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<First>()));

            var inserted = PlayerLoopUtils.TryInsert(loop, typeof(FixedUpdate), LoopPosition.FirstChildOf, Sys<Injected>(), out var result);

            Assert.That(inserted, Is.False);
            Assert.That(Describe(result), Is.EqualTo("Root(Update(First))"));
        }

        [Test]
        public void InsertBefore_PutsSystemAsTheAnchorsPreviousSibling()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<First>(), Sys<Second>()));

            PlayerLoopUtils.TryInsert(loop, typeof(Second), LoopPosition.Before, Sys<Injected>(), out var result);

            Assert.That(Describe(result), Is.EqualTo("Root(Update(First,Injected,Second))"));
        }

        [Test]
        public void InsertAfter_PutsSystemAsTheAnchorsNextSibling()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<First>(), Sys<Second>()));

            PlayerLoopUtils.TryInsert(loop, typeof(First), LoopPosition.After, Sys<Injected>(), out var result);

            Assert.That(Describe(result), Is.EqualTo("Root(Update(First,Injected,Second))"));
        }

        [Test]
        public void RemoveAll_RemovesEveryCopyAtAnyDepth()
        {
            var loop = Sys<Root>(Sys<Injected>(), Sys<Update>(Sys<First>(), Sys<Injected>(), Sys<Second>(Sys<Injected>())));

            var result = PlayerLoopUtils.RemoveAll(loop, typeof(Injected));

            Assert.That(Describe(result), Is.EqualTo("Root(Update(First,Second))"));
        }

        [Test]
        public void RemoveAll_LeavesTheOriginalLoopUnchanged()
        {
            var loop = Sys<Root>(Sys<Update>(Sys<Injected>()));

            PlayerLoopUtils.RemoveAll(loop, typeof(Injected));

            Assert.That(Describe(loop), Is.EqualTo("Root(Update(Injected))"));
        }

        private static PlayerLoopSystem Sys<T>(params PlayerLoopSystem[] children) =>
            new PlayerLoopSystem { type = typeof(T), subSystemList = children.Length == 0 ? null : children };

        private static string Describe(PlayerLoopSystem system)
        {
            if (system.subSystemList == null || system.subSystemList.Length == 0)
                return system.type.Name;

            return $"{system.type.Name}({string.Join(",", system.subSystemList.Select(Describe))})";
        }
    }
}
