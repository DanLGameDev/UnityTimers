using UnityEngine;

namespace DGP.UnityTimers.PlayerLoop
{
    public static class PlayerLoopTimeProviders
    {
        public static readonly TimeProvider BeforeUpdateTimeProvider = new();
        public static readonly TimeProvider AfterUpdateTimeProvider = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize()
        {
            PlayerLoopUtils.InsertSystemBefore(typeof(UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate), typeof(PlayerLoopTimeProviders), PlayerLoopBeforeUpdate);
            PlayerLoopUtils.InjectSystemAfter(typeof(UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate), typeof(PlayerLoopTimeProviders), PlayerLoopAfterUpdate);
            
            Application.quitting += HandleApplicationQuit;
        }
        
        private static void PlayerLoopBeforeUpdate() => BeforeUpdateTimeProvider.Tick(Time.deltaTime);
        private static void PlayerLoopAfterUpdate() => AfterUpdateTimeProvider.Tick(Time.deltaTime);

        private static void HandleApplicationQuit()
        {
            PlayerLoopUtils.TryRemoveSystem(typeof(PlayerLoopTimeProviders));
            PlayerLoopUtils.TryRemoveSystem(typeof(PlayerLoopTimeProviders));
            
            BeforeUpdateTimeProvider.Dispose();
            AfterUpdateTimeProvider.Dispose();
            
            Application.quitting -= HandleApplicationQuit;
        }
    }
}