using System;

namespace DGP.UnityTimers
{
    public class IntervalTimer : TimerBase
    {
        public event Action OnInterval;

        public float Interval { get; }

        private float _accumulatedTime;

        public IntervalTimer(ITimeProvider timeProvider, float interval) : base(timeProvider)
        {
            Interval = RequirePositive(interval);
        }

        public IntervalTimer(float interval)
        {
            Interval = RequirePositive(interval);
        }

        protected override void Advance(float deltaTime)
        {
            _accumulatedTime += deltaTime;

            while (IsRunning && _accumulatedTime >= Interval) {
                Step(Interval);
            }

            while (IsRunning && _accumulatedTime < 0f) {
                Step(-Interval);
            }
        }

        protected override void ResetProgress() => _accumulatedTime = 0f;

        private void Step(float stepTime)
        {
            _accumulatedTime -= stepTime;
            AdvanceChildren(stepTime);
            OnInterval?.Invoke();
        }

        private static float RequirePositive(float interval)
        {
            if (interval <= 0f)
                throw new ArgumentOutOfRangeException(nameof(interval), interval, "An interval must be greater than zero.");

            return interval;
        }
    }
}
