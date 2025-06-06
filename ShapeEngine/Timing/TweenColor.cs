using ShapeEngine.Color;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Timing;

/// <summary>
/// Tween for interpolating between two colors over time.
/// </summary>
public class TweenColor : ISequenceable
{
    /// <summary>
    /// Delegate for the tween function, called with the interpolated color.
    /// </summary>
    /// <param name="result">The interpolated color value.</param>
    /// <returns>True if the tween should finish, otherwise false.</returns>
    public delegate bool TweenFunc(ColorRgba result);

    private readonly TweenFunc func;
    private readonly float duration;
    private readonly TweenType tweenType;
    private readonly ColorRgba from;
    private readonly ColorRgba to;
    private float timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TweenColor"/> class.
    /// </summary>
    /// <param name="tweenFunc">The function to call with the tweened color.</param>
    /// <param name="from">The starting color.</param>
    /// <param name="to">The ending color.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="tweenType">The type of tweening to use.</param>
    public TweenColor(TweenFunc tweenFunc, ColorRgba from, ColorRgba to, float duration, TweenType tweenType)
    {
        this.func = tweenFunc;
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TweenColor"/> class by copying another instance.
    /// </summary>
    /// <param name="tween">The tween to copy.</param>
    public TweenColor(TweenColor tween)
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
    /// <returns>A new <see cref="TweenColor"/> instance with the same parameters.</returns>
    public ISequenceable Copy() => new TweenColor(this);

    /// <summary>
    /// Updates the tween by the given delta time.
    /// </summary>
    /// <param name="dt">The time in seconds since the last update.</param>
    /// <returns>True if the tween is finished or if the timer exceeds the duration, otherwise false.</returns>
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = ShapeMath.Clamp(timer / duration, 0f, 1f);
        timer += dt;
        var result = from.Tween(to, t, tweenType);

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
    /// Gets the starting color.
    /// </summary>
    public ColorRgba From => from;

    /// <summary>
    /// Gets the ending color.
    /// </summary>
    public ColorRgba To => to;

    /// <summary>
    /// Gets the current timer value.
    /// </summary>
    public float Timer => timer;
}