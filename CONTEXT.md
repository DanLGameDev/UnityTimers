# Unity Timers

Timers for Unity that advance with game time and can be nested, so pausing or scaling one timer affects every timer that takes its time from it.

## Language

### Time and the timer tree

**Time Provider**:
Anything that hands elapsed time to the timers attached to it. Every timer is also a time provider for its children.
_Avoid_: time source, clock, ticker

**Tick**:
One delivery of elapsed time from a time provider to its attached timers.
_Avoid_: update, step (except for an interval step)

**Timer Tree**:
The hierarchy formed by timers attached to time providers. Time flows from the root down; a timer's children receive only the time that timer passes on.

**Parent** / **Child**:
The time provider a timer is attached to, and the timers attached to it. A timer has at most one parent, fixed when it is created.

**Player Loop Provider**:
A root time provider driven by Unity's player loop at a fixed point, such as before or after Update or FixedUpdate.

**Time Scale**:
A multiplier a timer applies to the time it receives before using it or passing it on. A negative scale runs time backwards; zero freezes it without pausing.

### Run state

**Run State**:
Whether a timer is Stopped, Running, or Paused. Only a Running timer uses time or passes it to its children.
_Avoid_: enabled, active

**Stopped**:
The run state of a new, finished, or stopped timer. Its progress stays readable; starting it again begins from zero.

**Paused**:
A run state that holds progress; resuming continues from where it paused.

### Timer kinds

**Countdown**:
A timer that runs for a fixed duration and then completes. Backwards time rewinds it, never past its full duration.

**Completion**:
The moment a countdown's remaining time reaches zero, leaving it Stopped.

**Interval**:
A timer that fires once per fixed span of time and passes time to its children in whole steps of that span.
