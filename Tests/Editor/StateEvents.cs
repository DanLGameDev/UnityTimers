using System.Collections.Generic;

namespace DGP.UnityTimers.Editor.Tests
{
    public class StateEvents
    {
        public readonly List<string> Fired = new();

        public StateEvents(TimerBase timer)
        {
            timer.OnStarted += () => Fired.Add("Started");
            timer.OnStopped += () => Fired.Add("Stopped");
            timer.OnPaused += () => Fired.Add("Paused");
            timer.OnResumed += () => Fired.Add("Resumed");
        }
    }
}
