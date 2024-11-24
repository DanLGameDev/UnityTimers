using System;
using System.Security.Cryptography;
using UnityEditor;

namespace DGP.UnityTimers
{
    public class CountdownTimer : TimerBase
    {
        public event Action<bool> OnCountdownStatusChanged;
        public event Action<float> OnCountdownUpdated;
        public event Action OnCountdownComplete;
        
        private readonly float _initialTime;
        private float _timeRemaining;
        
        public float TimeRemaining => _timeRemaining;
        public float TimeRemainingNormalized => _timeRemaining / _initialTime;
        public bool IsComplete() => _timeRemaining <= 0;

        public CountdownTimer(ITimeProvider timeProvider, float duration, bool startImmediately = false) : this(duration, startImmediately)
        {
            timeProvider.AddHandler(Tick);
        }
        
        public CountdownTimer(float duration, bool startImmediately = false) : base()
        {
            _initialTime = duration;
            _timeRemaining = duration;

            if (startImmediately)
                Start();
        }
        
        public void Start()
        {
            if (Enabled) return;
            Enabled = true;
            OnCountdownStatusChanged?.Invoke(Enabled);
        }

        public void ResetAndStart()
        {
            Halt();
            _timeRemaining = _initialTime;
            Start();
        }

        public void Halt()
        {
            if (!Enabled) return;
            
            Enabled = false;
            OnCountdownStatusChanged?.Invoke(Enabled);
        }

        public void CancelAndReset()
        {
            if (!Enabled) return;

            Halt();
            _timeRemaining = _initialTime;
        }
        

        protected override void TickInternal(float deltaTime)
        {
            
            if (_timeRemaining > 0) {
                _timeRemaining -= deltaTime;

                if (_timeRemaining <= 0) {
                    Halt();
                    _timeRemaining = 0f;
                    OnCountdownComplete?.Invoke();
                }
                
                OnCountdownUpdated?.Invoke(_timeRemaining);
            }
            else {
                Enabled = false;
            }
        }
    }
}