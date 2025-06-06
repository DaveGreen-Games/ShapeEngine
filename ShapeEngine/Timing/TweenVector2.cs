using System.Numerics;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Timing;

/// <summary>
/// Tween for interpolating between two Vector2 values over time.
/// </summary>
public class TweenVector2 : ISequenceable
{
    /// <summary>
    /// Delegate for the tween function, called with the interpolated Vector2 value.
    /// </summary>
    /// <param name="result">The interpolated Vector2 value.</param>
    /// <returns>True if the tween should finish, otherwise false.</returns>
    public delegate bool TweenFunc(Vector2 result);

    private readonly TweenFunc func;
    private readonly float duration;
    private readonly TweenType tweenType;
    private readonly Vector2 from;
    private readonly Vector2 to;
    private float timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TweenVector2"/> class.
    /// </summary>
    /// <param name="tweenFunc">The function to call with the tweened value.</param>
    /// <param name="from">The starting Vector2 value.</param>
    /// <param name="to">The ending Vector2 value.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="tweenType">The type of tweening to use.</param>
    public TweenVector2(TweenFunc tweenFunc, Vector2 from, Vector2 to, float duration, TweenType tweenType)
    {
        this.func = tweenFunc;
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TweenVector2"/> class by copying another instance.
    /// </summary>
    /// <param name="tween">The tween to copy.</param>
    public TweenVector2(TweenVector2 tween)
    {
        this.func = tween.func;
        this.duration = tween.duration;
        this.timer = tween.timer;
        this.tweenType = tween.tweenType;
        this.from = tween.from;
        this.to = tween.to;
    }

    /// <summary>
    /// Creates a copy of this tween.
    /// </summary>
    /// <returns>A new <see cref="TweenVector2"/> instance with the same parameters.</returns>
    public ISequenceable Copy() => new TweenVector2(this);

    /// <summary>
    /// Updates the tween by the given delta time.
    /// </summary>
    /// <param name="dt">The time in seconds since the last update.</param>
    /// <returns>True if the tween is finished, otherwise false.</returns>
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = ShapeMath.Clamp(timer / duration, 0f, 1f);
        timer += dt;
        Vector2 result = ShapeTween.Tween(from, to, t, tweenType);

        return func(result) || t >= 1f;
    }

    /// <summary>
    /// Gets the tween function delegate.
    /// </summary>
    public TweenFunc Func => func;

    /// <summary>
    /// Gets the duration of the tween in seconds.
    /// </summary>
    public float Duration => duration;

    /// <summary>
    /// Gets the tween type.
    /// </summary>
    public TweenType TweenType => tweenType;

    /// <summary>
    /// Gets the starting Vector2 value.
    /// </summary>
    public Vector2 From => from;

    /// <summary>
    /// Gets the ending Vector2 value.
    /// </summary>
    public Vector2 To => to;

    /// <summary>
    /// Gets the current timer value.
    /// </summary>
    public float Timer => timer;
}