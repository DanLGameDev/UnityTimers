using System;

namespace DGP.UnityTimers
{
    public class TimeProvider : ITimeProvider
    {
        private readonly TickHandlerCollection _tickHandlers = new();
        
        public void AddHandler(ITimeProvider.TickHandler handler) => _tickHandlers.AddHandler(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _tickHandlers.RemoveHandler(handler);

        public void Tick(float deltaTime) => UpdateSubscribers(deltaTime);
        
        private void UpdateSubscribers(float deltaTime) => _tickHandlers.NotifySubscribers(deltaTime);
        
    }
}