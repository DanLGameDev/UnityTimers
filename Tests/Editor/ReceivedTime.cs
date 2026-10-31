namespace DGP.UnityTimers.Editor.Tests
{
    public class ReceivedTime
    {
        public float Total;
        public int Ticks;

        public ReceivedTime(ITimeProvider provider) => provider.AddHandler(Receive);

        private void Receive(float deltaTime)
        {
            Total += deltaTime;
            Ticks++;
        }
    }
}
