using ShapeEngine.StaticLib;

namespace ShapeEngine.Timing;

/// <summary>
/// Represents an actionable sequenceable object that performs a specified action over a given duration.
/// </summary>
public class Actionable : ISequenceable
{
    /// <summary>
    /// Delegate for the action to be performed.
    /// </summary>
    /// <param name="timeF">Normalized time (0 to 1) representing the progress of the action.</param>
    /// <param name="dt">Delta time since the last update, in seconds.</param>
    public delegate void ActionableFunc(float timeF, float dt);

    private readonly ActionableFunc action;
    private readonly float duration;
    private float timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Actionable"/> class.
    /// </summary>
    /// <param name="action">The action to perform during the sequence.</param>
    /// <param name="duration">The total duration of the action, in seconds.</param>
    public Actionable(ActionableFunc action, float duration)
    {
        this.action = action;
        this.duration = duration;
        this.timer = 0f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Actionable"/> class by copying another instance.
    /// </summary>
    /// <param name="actionable">The <see cref="Actionable"/> instance to copy.</param>
    private Actionable(Actionable actionable)
    {
        this.duration = actionable.duration;
        this.timer = actionable.timer;
        this.action = actionable.action;
    }

    /// <summary>
    /// Creates a copy of the current <see cref="Actionable"/> instance.
    /// </summary>
    /// <returns>A new <see cref="ISequenceable"/> instance that is a copy of this object.</returns>
    public ISequenceable Copy() => new Actionable(this);

    /// <summary>
    /// Updates the action with the given delta time.
    /// </summary>
    /// <param name="dt">The time elapsed since the last update, in seconds.</param>
    /// <returns>True if the action has completed; otherwise, false.</returns>
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = ShapeMath.Clamp(timer / duration, 0f, 1f);

        timer += dt;
        action(t, dt);
        return t >= 1f;
    }
}


