using System.Collections.Generic;

namespace DGP.UnityTimers
{
    public class TimeHandlerCollection
    {
        private readonly List<ITimeProvider.TickHandler> _tickHandlers = new();
        
        public void AddHandler(ITimeProvider.TickHandler handler) => _tickHandlers.Add(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _tickHandlers.Remove(handler);
        
        public void NotifySubscribers(float deltaTime)
        {
            foreach (var handler in _tickHandlers) {
                handler.Invoke(deltaTime);
            }
        }
    }
}