using UnityEngine;
using UnityEngine.PlayerLoop;

namespace DGP.UnityTimers.PlayerLoop
{
    public class BeforeUpdateTimeProvider : TimeProvider { }
    public class AfterUpdateTimeProvider : TimeProvider { }
    
    public class BeforeFixedUpdateTimeProvider : TimeProvider { }
    public class AfterFixedUpdateTimeProvider : TimeProvider { }

    public static class TimeProviders
    {
        public static readonly BeforeUpdateTimeProvider BeforeUpdateTimeProvider = new();
        public static readonly AfterUpdateTimeProvider AfterUpdateTimeProvider = new();
        
        public static readonly BeforeFixedUpdateTimeProvider BeforeFixedUpdateTimeProvider = new();
        public static readonly AfterFixedUpdateTimeProvider AfterFixedUpdateTimeProvider = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize()
        {
            PlayerLoopUtils.InjectSystemAsFirstChildOf(typeof(Update), typeof(BeforeUpdateTimeProvider), PlayerLoopBeforeUpdate);
            PlayerLoopUtils.InjectSystemAsLastChildOf(typeof(Update), typeof(AfterUpdateTimeProvider), PlayerLoopAfterUpdate);
            
            PlayerLoopUtils.InjectSystemAsFirstChildOf(typeof(FixedUpdate), typeof(BeforeFixedUpdateTimeProvider), PlayerLoopBeforeFixedUpdate);
            PlayerLoopUtils.InjectSystemAsLastChildOf(typeof(FixedUpdate), typeof(AfterFixedUpdateTimeProvider), PlayerLoopAfterFixedUpdate);
            
            Application.quitting += HandleApplicationQuit;
            PlayerLoopUtils.PrintPlayerLoop(UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop());
        }

        // Fixed Update
        private static void PlayerLoopBeforeFixedUpdate() => BeforeFixedUpdateTimeProvider.Tick(Time.fixedUnscaledDeltaTime);
        private static void PlayerLoopAfterFixedUpdate() => AfterFixedUpdateTimeProvider.Tick(Time.fixedUnscaledDeltaTime);

        // Update
        private static void PlayerLoopBeforeUpdate() => BeforeUpdateTimeProvider.Tick(Time.unscaledDeltaTime);
        private static void PlayerLoopAfterUpdate() => AfterUpdateTimeProvider.Tick(Time.unscaledDeltaTime);

        private static void HandleApplicationQuit()
        {
            PlayerLoopUtils.TryRemoveSystem(typeof(BeforeUpdateTimeProvider));
            PlayerLoopUtils.TryRemoveSystem(typeof(AfterUpdateTimeProvider));
            
            PlayerLoopUtils.TryRemoveSystem(typeof(BeforeFixedUpdateTimeProvider));
            PlayerLoopUtils.TryRemoveSystem(typeof(AfterFixedUpdateTimeProvider));

            BeforeUpdateTimeProvider.Dispose();
            AfterUpdateTimeProvider.Dispose();
            BeforeFixedUpdateTimeProvider.Dispose();
            AfterFixedUpdateTimeProvider.Dispose();
            
            Application.quitting -= HandleApplicationQuit;
        }
    }
}