using System;

namespace DGP.UnityTimers
{
    public abstract class TimerBase : ITimeProvider, IDisposable
    {
        private readonly TimeHandlerCollection _timeHandlers = new();
        protected TimeHandlerCollection TimeHandlers => _timeHandlers;

        private ITimeProvider _timeProvider;
        
        public void AddHandler(ITimeProvider.TickHandler handler) => _timeHandlers.AddHandler(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _timeHandlers.RemoveHandler(handler);

        protected virtual void Tick(float deltaTime) => _timeHandlers.NotifySubscribers(deltaTime);

        protected TimerBase() { }

        protected TimerBase(ITimeProvider timeProvider)
        {
            timeProvider.AddHandler(Tick);
            _timeProvider = timeProvider;
        }

        public void Dispose()
        {
            _timeProvider?.RemoveHandler(Tick);
            _timeProvider = null;
        }
    }
}