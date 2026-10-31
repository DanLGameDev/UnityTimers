using System.Collections.Generic;

namespace DGP.UnityTimers.Editor.Tests
{
    public class CountingTimeProvider : ITimeProvider
    {
        private readonly List<ITimeProvider.TickHandler> _handlers = new();

        public int HandlerCount => _handlers.Count;

        public void AddHandler(ITimeProvider.TickHandler handler) => _handlers.Add(handler);
        public void RemoveHandler(ITimeProvider.TickHandler handler) => _handlers.Remove(handler);
    }
}
