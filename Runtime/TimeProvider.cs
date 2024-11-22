using System;

namespace DGP.PlayerLoopTimers
{
    public class TimeProvider : ITimeProvider, IDisposable
    {
        private readonly TimeHandlerCollection _timeHandlers = new();
        
        public void AddHandler(ITimeProvider.TickHandler handler) => _timeHandlers.AddHandler(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _timeHandlers.RemoveHandler(handler);

        public virtual void Tick(float deltaTime) => UpdateSubscribers(deltaTime);
        
        protected virtual void UpdateSubscribers(float deltaTime)
        {
            _timeHandlers.NotifySubscribers(deltaTime);
        }

        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        
    }
}