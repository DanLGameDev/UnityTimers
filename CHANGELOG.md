## [0.2.0] - 2026-10-30
### Changed (breaking)
- Timers now have a run state (`TimerState`: Stopped, Running, Paused) owned by `TimerBase`, with `Start`, `Pause`, `Resume`, `Stop`, `Restart` and `Reset`. `Enabled` is removed.
- New timers start Stopped and must be started with `Start()`.
- `Start()` on a stopped timer begins from zero; on a paused timer it resumes. `Stop()` keeps progress readable.
- `OnStarted`, `OnStopped`, `OnPaused` and `OnResumed` live on every timer and fire only when the state actually changes. `CountdownTimer.OnCountdownStart` and `OnCountdownStopped` are removed.
- `CountdownTimer.OnCountdownUpdated` and `OnCountdownComplete` are renamed `OnUpdated` and `OnComplete`; on completion they fire in the order Updated, Stopped, Complete.
- `CountdownTimer.IsComplete` is now a property, `Stop()` takes no arguments, and `Duration` is exposed.
- A paused or stopped timer passes no time to the timers attached to it.
- Ticking a timer that is attached to a time provider by hand now throws `InvalidOperationException`.
- A duration or interval of zero or less throws `ArgumentOutOfRangeException`.

### Added
- Negative time scale runs time backwards: countdowns rewind (never past their full duration) and intervals step back across each boundary.
- `IntervalTimer.Interval` is exposed.
- EditMode tests for the shared timer rules, `CountdownTimer` and `IntervalTimer`.

### Fixed
- `Dispose` now detaches a timer from its time provider; afterwards it is Stopped and further state changes throw `ObjectDisposedException`.
- `CountdownTimer` created with a time provider can now be disposed.
- `CountdownTimer` now passes time to timers attached to it (only the time it actually uses).
- Removing a handler or disposing a timer during a tick no longer throws or skips other handlers.
- An interval stopped from its own `OnInterval` handler no longer keeps catching up.

## [0.1.0] - 2024-11-18
### First Release
- Initial Commit
