using System;

namespace DGP.PlayerLoopTimers
{
    public interface ITimeProvider
    {
        public delegate void TickHandler(float timerDeltaTime);
        
        public void AddHandler(TickHandler handler);
        public void RemoveHandler(TickHandler handler);
    }
}