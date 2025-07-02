namespace ShapeEngine.Timing;

/// <summary>
/// A basic timer class for tracking elapsed and remaining time, supporting pause, resume, stop, cancel, and restart operations.
/// </summary>
public class BasicTimer
{
    /// <summary>
    /// The current remaining time on the timer.
    /// </summary>
    public float timer;

    /// <summary>
    /// Gets the remaining time on the timer.
    /// </summary>
    public float Remaining => timer;

    /// <summary>
    /// Gets the elapsed time since the timer started.
    /// </summary>
    public float Elapsed { get; protected set; }

    /// <summary>
    /// Gets the total duration set for the timer.
    /// </summary>
    public float Duration { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the timer is paused.
    /// </summary>
    public bool Paused { get; protected set; }

    /// <summary>
    /// Gets the normalized remaining time (from 1.0 at start to 0.0 at finish).
    /// </summary>
    public float F { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicTimer"/> class.
    /// </summary>
    public BasicTimer()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the timer is currently running.
    /// </summary>
    public bool IsRunning => timer > 0.0f;

    /// <summary>
    /// Gets a value indicating whether the timer has finished.
    /// </summary>
    public bool IsFinished => WasStarted && !IsRunning;

    /// <summary>
    /// Gets a value indicating whether the timer was started.
    /// </summary>
    public bool WasStarted => Duration > 0f;

    /// <summary>
    /// Updates the timer by the specified delta time.
    /// </summary>
    /// <param name="dt">The time to decrement from the timer.</param>
    public void Update(float dt)
    {
        if (Paused) return;

        if (timer > 0.0f)
        {
            timer -= dt;
            if (timer <= 0.0f)
            {
                timer = 0.0f;
                Elapsed = Duration;
                F = 0.0f;
                return;
            }
            Elapsed = Duration - timer;
            F = timer / Duration;
        }
    }

    /// <summary>
    /// Starts the timer with the specified duration.
    /// </summary>
    /// <param name="duration">The duration to set for the timer.</param>
    public void Start(float duration)
    {
        if (duration <= 0.0f) return;

        Paused = false;
        timer = duration;
        this.Duration = duration;
        Elapsed = 0.0f;
        F = 1.0f;
    }

    /// <summary>
    /// Adds the specified amount of time to the timer.
    /// </summary>
    /// <param name="amount">The amount of time to add.</param>
    public void Add(float amount)
    {
        if (amount <= 0f) return;
        Paused = false;
        Duration += amount;
        timer += amount;

        F = timer / Duration;
    }

    /// <summary>
    /// Pauses the timer if it is not already paused.
    /// </summary>
    public void Pause()
    {
        if (Paused) return;
        Paused = true;
    }

    /// <summary>
    /// Resumes the timer if it is paused.
    /// </summary>
    public void Resume()
    {
        if (!Paused) return;
        Paused = false;
    }

    /// <summary>
    /// Stops the timer and resets the remaining time to zero.
    /// </summary>
    public void Stop()
    {
        Paused = false;
        timer = 0.0f;
        F = 0.0f;
    }

    /// <summary>
    /// Cancels the timer, resetting all values to their defaults.
    /// </summary>
    public void Cancel()
    {
        Paused = false;
        timer = 0f;
        F = 0f;
        Duration = 0f;
        Elapsed = 0f;
    }

    /// <summary>
    /// Restarts the timer using the last set duration.
    /// </summary>
    public void Restart()
    {
        if (Duration <= 0.0f) return;
        Paused = false;
        timer = Duration;
        F = 1.0f;
    }
}


