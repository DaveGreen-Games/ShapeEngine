using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Timing;

namespace ShapeEngine.Screen;

/// <summary>
/// Provides a camera tween that smoothly interpolates the camera's rotation in degrees over a specified duration and tween type.
/// </summary>
/// <remarks>
/// Use this class to animate camera rotation transitions, such as for cinematic effects or smooth camera turns.
/// </remarks>
public class CameraTweenRotation : ICameraTween
{
    private float duration;
    private float timer;
    private TweenType tweenType;
    private float from;
    private float to;
    private float cur;
    /// <summary>
    /// Initializes a new instance of the <see cref="CameraTweenRotation"/> class.
    /// </summary>
    /// <param name="fromDeg">The starting rotation in degrees.</param>
    /// <param name="toDeg">The ending rotation in degrees.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="tweenType">The type of tweening to use.</param>
    public CameraTweenRotation(float fromDeg, float toDeg, float duration, TweenType tweenType)
    {
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = fromDeg;
        this.to = toDeg;
        this.cur = from;
    }
    private CameraTweenRotation(CameraTweenRotation tween)
    {
        this.duration = tween.duration;
        this.timer = tween.timer;
        this.tweenType = tween.tweenType;
        this.from = tween.from;
        this.to = tween.to;
        this.cur = tween.cur;
    }

    /// <summary>
    /// Creates a copy of this tween instance.
    /// </summary>
    /// <returns>A new <see cref="ISequenceable"/> copy of this tween.</returns>
    public ISequenceable Copy() => new CameraTweenRotation(this);
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
    /// Gets the current rotation in degrees for the camera.
    /// </summary>
    /// <returns>The current interpolated rotation in degrees.</returns>
    public float GetRotationDeg() { return cur; }
}