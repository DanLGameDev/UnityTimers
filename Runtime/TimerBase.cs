using System;

namespace DGP.UnityTimers
{
    public abstract class TimerBase : IDisposable
    {
        private ITimeProvider _timeProvider;

        protected abstract void TickInternal(float deltaTime);
        
        protected TimerBase() { }
        protected TimerBase(ITimeProvider timeProvider)
        {
            timeProvider.AddHandler(TickInternal);
            _timeProvider = timeProvider;
        }

        public void Dispose()
        {
            _timeProvider?.RemoveHandler(TickInternal);
            _timeProvider = null;
        }
    }
}