using System.Collections.Generic;

namespace DGP.UnityTimers
{
    public class TickHandlerCollection
    {
        private readonly List<ITimeProvider.TickHandler> _tickHandlers = new();
        private int _notifyDepth;
        private bool _hasPendingRemovals;

        public void AddHandler(ITimeProvider.TickHandler handler) => _tickHandlers.Add(handler);

        public void RemoveHandler(ITimeProvider.TickHandler handler)
        {
            var index = _tickHandlers.IndexOf(handler);

            if (index < 0)
                return;

            // Removing mid-tick would shift the handlers still waiting for this tick, so blank the slot and compact afterwards.
            if (_notifyDepth > 0) {
                _tickHandlers[index] = null;
                _hasPendingRemovals = true;
                return;
            }

            _tickHandlers.RemoveAt(index);
        }

        public void NotifySubscribers(float deltaTime)
        {
            // Handlers added during this tick are counted out here, so they first receive time on the next tick.
            var count = _tickHandlers.Count;
            _notifyDepth++;

            try {
                for (var i = 0; i < count; i++) {
                    _tickHandlers[i]?.Invoke(deltaTime);
                }
            }
            finally {
                _notifyDepth--;

                if (_notifyDepth == 0 && _hasPendingRemovals)
                    CompactRemovedHandlers();
            }
        }

        private void CompactRemovedHandlers()
        {
            _tickHandlers.RemoveAll(handler => handler == null);
            _hasPendingRemovals = false;
        }
    }
}
