namespace DGP.UnityTimers
{
    public class ScaledTimer : TimerBase, ITimeProvider
    {
        public float TimeScale { get; set; } = 1.0f;

        public override void Tick(float deltaTime)
        {
            deltaTime *= TimeScale;
            base.Tick(deltaTime);
        }

        public ScaledTimer() { }
        public ScaledTimer(ITimeProvider timeProvider) : base(timeProvider) { }
        public ScaledTimer(ITimeProvider.TickHandler onTickHandler) : base() => AddHandler(onTickHandler);
        public ScaledTimer(ITimeProvider timeProvider, ITimeProvider.TickHandler onTickHandler) : base(timeProvider) => AddHandler(onTickHandler);
    }
}