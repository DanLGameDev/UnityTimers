using System;

namespace DGP.UnityTimers
{
    public abstract class TimerBase : ITimeProvider, IDisposable
    {
        public event Action OnStarted;
        public event Action OnStopped;
        public event Action OnPaused;
        public event Action OnResumed;

        private readonly TickHandlerCollection _tickHandlers = new();
        private ITimeProvider _timeProvider;
        private bool _isDisposed;

        public TimerState State { get; private set; } = TimerState.Stopped;
        public bool IsRunning => State == TimerState.Running;
        public float TimeScale { get; set; } = 1.0f;

        protected TimerBase() { }

        protected TimerBase(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _timeProvider.AddHandler(ReceiveTick);
        }

        public void Start()
        {
            ThrowIfDisposed();

            if (State == TimerState.Running) return;
            if (State == TimerState.Paused) {
                Resume();
                return;
            }

            ResetProgress();
            State = TimerState.Running;
            OnStarted?.Invoke();
        }

        public void Pause()
        {
            ThrowIfDisposed();

            if (State != TimerState.Running)
                return;

            State = TimerState.Paused;
            OnPaused?.Invoke();
        }

        public void Resume()
        {
            ThrowIfDisposed();

            if (State != TimerState.Paused)
                return;

            State = TimerState.Running;
            OnResumed?.Invoke();
        }

        public void Stop()
        {
            if (State == TimerState.Stopped)
                return;

            State = TimerState.Stopped;
            OnStopped?.Invoke();
        }

        public void Restart()
        {
            ThrowIfDisposed();

            Stop();
            Start();
        }

        public void Reset()
        {
            ThrowIfDisposed();
            ResetProgress();
        }

        public void AddHandler(ITimeProvider.TickHandler handler) => _tickHandlers.AddHandler(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _tickHandlers.RemoveHandler(handler);

        public void Tick(float deltaTime)
        {
            ThrowIfDisposed();

            if (_timeProvider != null)
                throw new InvalidOperationException($"This {GetType().Name} is attached to a time provider, which ticks it.");

            ReceiveTick(deltaTime);
        }

        /// <summary>
        /// Uses the scaled time from one tick of a running timer. Call <see cref="AdvanceChildren"/> to pass time
        /// to child timers; the default passes all of it.
        /// </summary>
        protected virtual void Advance(float deltaTime) => AdvanceChildren(deltaTime);

        protected void AdvanceChildren(float deltaTime) => _tickHandlers.NotifySubscribers(deltaTime);

        /// <summary>
        /// Returns the timer's progress to zero. Called when a stopped timer starts and when <see cref="Reset"/> is called.
        /// </summary>
        protected virtual void ResetProgress() { }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _timeProvider?.RemoveHandler(ReceiveTick);
            _timeProvider = null;
            State = TimerState.Stopped;
        }

        private void ReceiveTick(float deltaTime)
        {
            if (State != TimerState.Running)
                return;

            var scaledDeltaTime = deltaTime * TimeScale;

            if (scaledDeltaTime == 0f)
                return;

            Advance(scaledDeltaTime);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
