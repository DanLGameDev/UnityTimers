namespace DGP.UnityTimers
{
    public class ScaledTimer : TimerBase, ITimeProvider
    {
        public ScaledTimer() { }
        public ScaledTimer(ITimeProvider timeProvider) : base(timeProvider) { }
        public ScaledTimer(ITimeProvider.TickHandler onTickHandler) : base() => AddHandler(onTickHandler);
        public ScaledTimer(ITimeProvider timeProvider, ITimeProvider.TickHandler onTickHandler) : base(timeProvider) => AddHandler(onTickHandler);
    }
}