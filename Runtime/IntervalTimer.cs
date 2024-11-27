using System;

namespace DGP.UnityTimers
{
    public class IntervalTimer : TimerBase
    {
        public event Action OnInterval;
        
        private float _interval;
        private float _accumulatedTime;

        public IntervalTimer(float interval) : base()
        {
            _interval = interval;
        }

        public IntervalTimer(ITimeProvider timeProvider, float interval) : base(timeProvider)
        {
            _interval = interval;
        }

        protected override void TickInternal(float deltaTime)
        {   
            _accumulatedTime += deltaTime;

            while (_accumulatedTime >= _interval) {
                _accumulatedTime -= _interval;
                base.TickInternal(_interval);
                OnInterval?.Invoke();
            }
        }
    }
}