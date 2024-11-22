using System;

namespace DGP.PlayerLoopTimers
{
    public class Timer : TimerBase
    {
        public float TimeScale { get; set; } = 1.0f;
        public bool Enabled { get; set; } = false;
        
        protected override void Tick(float deltaTime)
        {
            if (!Enabled) return;
            
            base.Tick(deltaTime * TimeScale);
        }

        public Timer() { }
        public Timer(ITimeProvider timeProvider) : base(timeProvider) { }
        public Timer(ITimeProvider.TickHandler onTickHandler) : base() => AddHandler(onTickHandler);
        public Timer(ITimeProvider timeProvider, ITimeProvider.TickHandler onTickHandler) : base(timeProvider) => AddHandler(onTickHandler);
    }
}