using System;

namespace DGP.UnityTimers
{
    public class CountdownTimer : TimerBase
    {
        public event Action<float> OnUpdated;
        public event Action OnComplete;

        public float Duration { get; }
        public float RemainingTime { get; private set; }
        public float ElapsedTime => Duration - RemainingTime;

        public float ElapsedTimeNormalized => ElapsedTime / Duration;
        public float RemainingTimeNormalized => RemainingTime / Duration;

        public bool IsComplete => RemainingTime <= 0f;

        public CountdownTimer(ITimeProvider timeProvider, float duration) : base(timeProvider)
        {
            Duration = RequirePositive(duration);
            RemainingTime = Duration;
        }

        public CountdownTimer(float duration)
        {
            Duration = RequirePositive(duration);
            RemainingTime = Duration;
        }

        protected override void Advance(float deltaTime)
        {
            var usedTime = Math.Clamp(deltaTime, -ElapsedTime, RemainingTime);

            if (usedTime == 0f)
                return;

            RemainingTime -= usedTime;
            AdvanceChildren(usedTime);
            OnUpdated?.Invoke(RemainingTime);

            if (RemainingTime > 0f)
                return;

            Stop();
            OnComplete?.Invoke();
        }

        protected override void ResetProgress() => RemainingTime = Duration;

        private static float RequirePositive(float duration)
        {
            if (duration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "A countdown's duration must be greater than zero.");

            return duration;
        }
    }
}
