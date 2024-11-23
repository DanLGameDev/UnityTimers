using System;

namespace DGP.UnityTimers
{
    public abstract class TimerBase : IDisposable
    {
        private ITimeProvider _timeProvider;

        // Constructors
        protected TimerBase() { }
        protected TimerBase(ITimeProvider timeProvider)
        {
            timeProvider.AddHandler(TickInternal);
            _timeProvider = timeProvider;
        }
        
        // Time Handling
        protected abstract void TickInternal(float deltaTime);
        

        // IDisposable
        public void Dispose()
        {
            _timeProvider?.RemoveHandler(TickInternal);
            _timeProvider = null;
        }
    }
}