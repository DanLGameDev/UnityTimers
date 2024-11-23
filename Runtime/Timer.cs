namespace DGP.UnityTimers
{
    public class Timer : TimerBase, ITimeProvider
    {
        private readonly TickHandlerCollection _tickHandlers = new();
        protected TickHandlerCollection TickHandlers => _tickHandlers;
        
        public float TimeScale { get; set; } = 1.0f;
        public bool Enabled { get; set; } = true;
        
        public void AddHandler(ITimeProvider.TickHandler handler) => _tickHandlers.AddHandler(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _tickHandlers.RemoveHandler(handler);
        
        protected override void TickInternal(float deltaTime)
        {
            if (!Enabled) return;
            _tickHandlers.NotifySubscribers(deltaTime * TimeScale);
        }
        
        public void Tick(float deltaTime) => TickInternal(deltaTime);

        public Timer() { }
        public Timer(ITimeProvider timeProvider) : base(timeProvider) { }
        public Timer(ITimeProvider.TickHandler onTickHandler) : base() => AddHandler(onTickHandler);
        public Timer(ITimeProvider timeProvider, ITimeProvider.TickHandler onTickHandler) : base(timeProvider) => AddHandler(onTickHandler);
    }
}