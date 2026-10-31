using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace DGP.UnityTimers.PlayerLoop
{
    public static class TimeProviders
    {
        public static readonly PlayerLoopProvider BeforeUpdateTimeProvider =
            new(typeof(Update), LoopPosition.FirstChildOf, typeof(TimersBeforeUpdate), () => Time.unscaledDeltaTime);
        public static readonly PlayerLoopProvider AfterUpdateTimeProvider =
            new(typeof(Update), LoopPosition.LastChildOf, typeof(TimersAfterUpdate), () => Time.unscaledDeltaTime);

        public static readonly PlayerLoopProvider BeforeFixedUpdateTimeProvider =
            new(typeof(FixedUpdate), LoopPosition.FirstChildOf, typeof(TimersBeforeFixedUpdate), () => Time.fixedUnscaledDeltaTime);
        public static readonly PlayerLoopProvider AfterFixedUpdateTimeProvider =
            new(typeof(FixedUpdate), LoopPosition.LastChildOf, typeof(TimersAfterFixedUpdate), () => Time.fixedUnscaledDeltaTime);

        private static readonly PlayerLoopProvider[] Providers = {
            BeforeUpdateTimeProvider,
            AfterUpdateTimeProvider,
            BeforeFixedUpdateTimeProvider,
            AfterFixedUpdateTimeProvider
        };

        private sealed class TimersBeforeUpdate { }
        private sealed class TimersAfterUpdate { }
        private sealed class TimersBeforeFixedUpdate { }
        private sealed class TimersAfterFixedUpdate { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InstallIntoPlayerLoop()
        {
            var loop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(Install(loop));

            Application.quitting -= TearDown;
            Application.quitting += TearDown;
        }

        // Application.quitting also fires on leaving Play Mode, and without a domain reload these providers outlive the session.
        private static void TearDown()
        {
            Application.quitting -= TearDown;

            var loop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(Uninstall(loop));

            ClearSubscribers();
        }

        internal static PlayerLoopSystem Install(PlayerLoopSystem loop)
        {
            loop = Uninstall(loop);

            foreach (var provider in Providers) {
                if (!PlayerLoopUtils.TryInsert(loop, provider.Anchor, provider.Position, provider.CreateSystem(), out var installed)) {
                    Debug.LogError($"{provider.Anchor.Name} not found in the player loop, so {provider.SystemType.Name} timers will not tick.");
                    continue;
                }

                loop = installed;
            }

            return loop;
        }

        internal static PlayerLoopSystem Uninstall(PlayerLoopSystem loop)
        {
            foreach (var provider in Providers) {
                loop = PlayerLoopUtils.RemoveAll(loop, provider.SystemType);
            }

            return loop;
        }

        internal static void ClearSubscribers()
        {
            foreach (var provider in Providers) {
                provider.ClearSubscribers();
            }
        }
    }
}
