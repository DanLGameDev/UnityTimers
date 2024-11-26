using System;

namespace DGP.UnityTimers
{
    public class CountdownTimer : TimerBase
    {
        public event Action OnCountdownStart;
        public event Action<float> OnCountdownUpdated;
        public event Action OnCountdownStopped;
        public event Action OnCountdownComplete;
        
        
        private readonly float _timerDuration;
        private float _remainingTime;
        
        public float ElapsedTime => _timerDuration - _remainingTime;
        public float RemainingTime => _remainingTime;
        
        public float ElapsedTimeNormalized => (_timerDuration - _remainingTime) / _timerDuration;
        public float RemainingTimeNormalized => _remainingTime / _timerDuration;
        
        public bool IsComplete() => _remainingTime <= 0;

        public CountdownTimer(ITimeProvider timeProvider, float timerDuration) : this(timerDuration)
        {
            timeProvider.AddHandler(Tick);
        }
        
        public CountdownTimer(float timerDuration) : base()
        {
            _timerDuration = timerDuration;
            _remainingTime = timerDuration;
        }
        
        public void Start()
        {
            if (Enabled) return;
            
            Enabled = true;
            OnCountdownStart?.Invoke();
        }

        public void Reset() => _remainingTime = _timerDuration;

        public void Restart()
        {
            Reset();
            Start();
        }

        public void Stop(bool resetTime = true)
        {
            if (!Enabled) return;
            
            Enabled = false;
            OnCountdownStopped?.Invoke();
            
            if (resetTime)
                _remainingTime = _timerDuration;
        }

        protected override void TickInternal(float deltaTime)
        {
            
            if (_remainingTime > 0) {
                _remainingTime -= deltaTime;

                if (_remainingTime <= 0) {
                    _remainingTime = 0f;
                    
                    Enabled = false;
                    
                    OnCountdownStopped?.Invoke();
                    OnCountdownComplete?.Invoke();
                }
                
                OnCountdownUpdated?.Invoke(_remainingTime);
            }
            else {
                Enabled = false;
            }
        }
    }
}