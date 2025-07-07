using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core;

//TODO: Review 
// Does this make sense...
// The logic itself is very simple but it is hard to inject into the draw/update system
// I think the current implementation is easy but I am not sure yet
public partial class Game
{
    /// <summary>
    /// Represents an event that triggers at a specified interval, allowing actions to be executed
    /// at regular time steps or every frame.
    /// </summary>
    public class IntervalEvent
    {
        /// <summary>
        /// The interval duration in seconds. If less than or equal to 0, the event triggers every update.
        /// </summary>
        public readonly float Interval;

        /// <summary>
        /// Occurs on each update cycle.
        /// The boolean parameter is <c>true</c> if triggered before Update(), or <c>false</c> if after.
        /// The float parameter is the delta time (in seconds) for the update cycle.
        /// </summary>
        public event Action<bool, float>? OnUpdate;

        /// <summary>
        /// Occurs during the game drawing phase.
        /// The boolean parameter is <c>true</c> if triggered before DrawGame(), or <c>false</c> if after.
        /// The <see cref="ScreenInfo"/> parameter provides information about the game screen area.
        /// </summary>
        public event Action<bool, ScreenInfo>? OnDrawGame;

        /// <summary>
        /// Occurs during the game UI drawing phase.
        /// The boolean parameter is <c>true</c> if triggered before DrawGameUi(), or <c>false</c> if after.
        /// The <see cref="ScreenInfo"/> parameter provides information about the game UI screen area.
        /// </summary>
        public event Action<bool, ScreenInfo>? OnDrawGameUi;

        /// <summary>
        /// Occurs during the general UI drawing phase.
        /// The boolean parameter is <c>true</c> if triggered before DrawUi(), or <c>false</c> if after.
        /// The <see cref="ScreenInfo"/> parameter provides information about the general UI screen area.
        /// </summary>
        public event Action<bool, ScreenInfo>? OnDrawUi;

        private float timer;
        private bool finished;

        /// <summary>
        /// Initializes a new <see cref="ShapeEngine.Core.Game.IntervalEvent"/> that triggers every frame.
        /// </summary>
        /// <remarks>
        /// Sets <c>Interval</c> to <c>0</c>, causing the event to fire on every update.
        /// </remarks>
        public IntervalEvent()
        {
            Interval = 0f;
            timer = Interval;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeEngine.Core.Game.IntervalEvent"/> class with a custom interval.
        /// </summary>
        /// <param name="interval">
        /// The interval duration in seconds.
        /// If the value is less than or equal to 0, the event will trigger on every update.
        /// </param>
        public IntervalEvent(float interval)
        {
            Interval = interval;
            timer = Interval;
        }

        internal void TriggerOnUpdate(bool pre, float dt)
        {
            if (!finished) return;
            OnUpdate?.Invoke(pre, dt);
        }

        internal void TriggerOnDrawGame(bool pre, ScreenInfo info)
        {
            if (!finished) return;
            OnDrawGame?.Invoke(pre, info);
        }

        internal void TriggerOnDrawGameUi(bool pre, ScreenInfo info)
        {
            if (!finished) return;
            OnDrawGameUi?.Invoke(pre, info);
        }

        internal void TriggerOnDrawUi(bool pre, ScreenInfo info)
        {
            if (!finished) return;
            OnDrawUi?.Invoke(pre, info);
        }
        
        /// <summary>
        /// Updates the interval timer and determines if the event should be triggered.
        /// </summary>
        /// <param name="dt">The elapsed time in seconds since the last update.</param>
        internal void Update(float dt)
        {
            finished = false;

            if (Interval <= 0)
            {
                finished = true;
            }
            else
            {
                timer -= dt;
                if (timer <= 0)
                {
                    timer += Interval;
                    finished = true;
                }
            }
        }
    }

    /// <summary>
    /// Stores all active <see cref="IntervalEvent"/> instances managed by the game.
    /// </summary>
    private readonly List<IntervalEvent> intervalEvents = [];

    /// <summary>
    /// Adds a new <see cref="IntervalEvent"/> with the specified interval.
    /// </summary>
    /// <param name="interval">
    /// The interval duration in seconds for the event.
    /// If the value is less than or equal to 0, the event will trigger on every update frame.
    /// </param>
    /// <returns>The created <see cref="IntervalEvent"/> instance.</returns>
    public IntervalEvent AddIntervalEvent(float interval)
    {
        var intervalEvent = new IntervalEvent(interval);
        intervalEvents.Add(intervalEvent);
        return intervalEvent;
    }

    /// <summary>
    /// Removes the specified <see cref="IntervalEvent"/> from the list.
    /// </summary>
    /// <param name="intervalEvent">The interval event to remove.</param>
    /// <returns>True if the event was removed; otherwise, false.</returns>
    public bool RemoveIntervalEvent(IntervalEvent intervalEvent) => intervalEvents.Remove(intervalEvent);

    /// <summary>
    /// Checks if the specified <see cref="IntervalEvent"/> is present in the list.
    /// </summary>
    /// <param name="intervalEvent">The interval event to check for.</param>
    /// <returns>True if the event exists; otherwise, false.</returns>
    public bool HasIntervalEvent(IntervalEvent intervalEvent) => intervalEvents.Contains(intervalEvent);

    /// <summary>
    /// Removes all <see cref="IntervalEvent"/> instances from the list.
    /// </summary>
    public void ClearIntervalEvents() => intervalEvents.Clear();

    /// <summary>
    /// Updates all managed <see cref="IntervalEvent"/> instances with the elapsed time.
    /// </summary>
    /// <param name="dt">The delta time in seconds since the last update.</param>
    private void UpdateIntervalEvents(float dt)
    {
        foreach (var intervalEvent in intervalEvents)
        {
            intervalEvent.Update(dt);
        }
    }

    private void TriggerIntervalEventsUpdate(bool pre, float dt)
    {
        foreach (var intervalEvent in intervalEvents)
        {
            intervalEvent.TriggerOnUpdate(pre, dt);
        }
    }

    private void TriggerIntervalEventsDrawGame(bool pre, ScreenInfo info)
    {
        foreach (var intervalEvent in intervalEvents)
        {
            intervalEvent.TriggerOnDrawGame(pre, info);
        }
    }

    private void TriggerIntervalEventsDrawGameUi(bool pre, ScreenInfo info)
    {
        foreach (var intervalEvent in intervalEvents)
        {
            intervalEvent.TriggerOnDrawGameUi(pre, info);
        }
    }

    private void TriggerIntervalEventsDrawUi(bool pre, ScreenInfo info)
    {
        foreach (var intervalEvent in intervalEvents)
        {
            intervalEvent.TriggerOnDrawUi(pre, info);
        }
    }

}