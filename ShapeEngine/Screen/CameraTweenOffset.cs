using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Timing;

namespace ShapeEngine.Screen;

/// <summary>
/// Provides a tween operation for smoothly interpolating the camera's offset over time.
/// </summary>
/// <remarks>
/// This class implements <see cref="ICameraTween"/> to animate the camera's position offset using a specified tween type and duration.
/// </remarks>
public class CameraTweenOffset : ICameraTween
{
    private float duration;
    private float timer;
    private TweenType tweenType;
    private Vector2 from;
    private Vector2 to;
    private Vector2 cur;

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraTweenOffset"/> class.
    /// </summary>
    /// <param name="from">The starting offset value.</param>
    /// <param name="to">The ending offset value.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="tweenType">The type of tweening to use.</param>
    public CameraTweenOffset(Vector2 from, Vector2 to, float duration, TweenType tweenType)
    {
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
        this.cur = from;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraTweenOffset"/> class by copying another instance.
    /// </summary>
    /// <param name="cameraTween">The <see cref="CameraTweenOffset"/> instance to copy.</param>
    private CameraTweenOffset(CameraTweenOffset cameraTween)
    {
        this.duration = cameraTween.duration;
        this.timer = cameraTween.timer;
        this.tweenType = cameraTween.tweenType;
        this.from = cameraTween.from;
        this.to = cameraTween.to;
        this.cur = cameraTween.from;
    }

    /// <summary>
    /// Creates a copy of this tween instance.
    /// </summary>
    /// <returns>A new <see cref="ISequenceable"/> copy of this tween.</returns>
    public ISequenceable Copy() => new CameraTweenOffset(this);

    /// <summary>
    /// Updates the tween's progress.
    /// </summary>
    /// <param name="dt">The time elapsed since the last update, in seconds.</param>
    /// <returns>True if the tween has finished; otherwise, false.</returns>
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = ShapeMath.Clamp(timer / duration, 0f, 1f);
        timer += dt;
        cur = ShapeTween.Tween(from, to, t, tweenType);
        return t >= 1f;
    }

    /// <summary>
    /// Gets the current offset value for the camera.
    /// </summary>
    /// <returns>The current interpolated offset as a <see cref="Vector2"/>.</returns>
    public Vector2 GetOffset() { return cur; }
}