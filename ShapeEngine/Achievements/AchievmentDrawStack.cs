namespace ShapeEngine.Achievements;

/// <summary>
/// Represents a stack entry for drawing achievement notifications with a duration.
/// </summary>
internal class AchievmentDrawStack
{
    /// <summary>
    /// The remaining duration (in seconds) for which the notification should be displayed.
    /// </summary>
    public float duration;
    /// <summary>
    /// The achievement associated with this draw stack entry.
    /// </summary>
    public Achievement achievement;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievmentDrawStack"/> class.
    /// </summary>
    /// <param name="duration">The display duration in seconds.</param>
    /// <param name="achievement">The associated achievement.</param>
    public AchievmentDrawStack(float duration, Achievement achievement)
    {
        this.duration = duration;
        this.achievement = achievement;
    }
        
    /// <summary>
    /// Updates the remaining duration by subtracting the delta time.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    public void Update(float dt)
    {
        if (duration <= 0f) return;

        duration -= dt;
    }

    /// <summary>
    /// Determines whether the notification display duration has finished.
    /// </summary>
    /// <returns>True if finished; otherwise, false.</returns>
    public bool IsFinished() { return duration <= 0f; }
}