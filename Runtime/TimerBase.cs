using System;

namespace DGP.UnityTimers
{
    public abstract class TimerBase : ITimeProvider, IDisposable
    {
        private readonly TickHandlerCollection _tickHandlers = new();
        protected TickHandlerCollection TickHandlers => _tickHandlers;
        
        private ITimeProvider _timeProvider;
        
        public float TimeScale { get; set; } = 1.0f;
        public bool Enabled { get; set; } = true;

        // Constructors
        protected TimerBase() { }
        protected TimerBase(ITimeProvider timeProvider)
        {
            timeProvider.AddHandler(Tick);
            _timeProvider = timeProvider;
        }
        
        // Time Handling
        public void AddHandler(ITimeProvider.TickHandler handler) => _tickHandlers.AddHandler(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _tickHandlers.RemoveHandler(handler);

        public virtual void Tick(float deltaTime)
        {
            if (!Enabled) 
                return;
            
            TickInternal(deltaTime * TimeScale);
        }

        protected virtual void TickInternal(float deltaTime)
        {
            _tickHandlers.NotifySubscribers(deltaTime);
        }

        // IDisposable
        public void Dispose()
        {
            _timeProvider?.RemoveHandler(TickInternal);
            _timeProvider = null;
        }


    }
}