using System;
using UnityEngine.LowLevel;

namespace DGP.UnityTimers.PlayerLoop
{
    public class PlayerLoopProvider : ITimeProvider
    {
        internal readonly Type Anchor;
        internal readonly LoopPosition Position;
        internal readonly Type SystemType;

        private readonly Func<float> _deltaTime;
        private TickHandlerCollection _tickHandlers = new();

        internal PlayerLoopProvider(Type anchor, LoopPosition position, Type systemType, Func<float> deltaTime)
        {
            Anchor = anchor;
            Position = position;
            SystemType = systemType;
            _deltaTime = deltaTime;
        }

        public void AddHandler(ITimeProvider.TickHandler handler) => _tickHandlers.AddHandler(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _tickHandlers.RemoveHandler(handler);

        internal void Tick(float deltaTime) => _tickHandlers.NotifySubscribers(deltaTime);

        internal void ClearSubscribers() => _tickHandlers = new TickHandlerCollection();

        internal PlayerLoopSystem CreateSystem() =>
            new PlayerLoopSystem { type = SystemType, updateDelegate = TickFromPlayerLoop };

        private void TickFromPlayerLoop() => Tick(_deltaTime());
    }
}
