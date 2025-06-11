using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Timing;

namespace ShapeEngine.Screen;

/// <summary>
/// Provides a camera tween that smoothly interpolates the camera's zoom factor over a specified duration and tween type.
/// </summary>
/// <remarks>
/// Use this class to animate camera zoom transitions, such as for zooming in/out smoothly during gameplay or cutscenes.
/// </remarks>
public class CameraTweenZoomFactor : ICameraTween
{
    private float duration;
    private float timer;
    private TweenType tweenType;
    private float from;
    private float to;
    private float cur;
    /// <summary>
    /// Initializes a new instance of the <see cref="CameraTweenZoomFactor"/> class.
    /// </summary>
    /// <param name="from">The starting zoom factor.</param>
    /// <param name="to">The ending zoom factor.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="tweenType">The type of tweening to use.</param>
    public CameraTweenZoomFactor(float from, float to, float duration, TweenType tweenType)
    {
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
        this.cur = from;
    }
    private CameraTweenZoomFactor(CameraTweenZoomFactor cameraTween)
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
    public ISequenceable Copy() => new CameraTweenZoomFactor(this);
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
    /// Gets the current zoom factor for the camera.
    /// </summary>
    /// <returns>The current interpolated zoom factor.</returns>
    public float GetZoomFactor() { return cur; }
}