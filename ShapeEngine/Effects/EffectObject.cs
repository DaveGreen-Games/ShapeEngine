using ShapeEngine.Core;
using ShapeEngine.StaticLib;
using ShapeEngine.Timing;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Effects;

/// <summary>
/// Represents a base class for effect objects with lifetime and tweening support.
/// </summary>
public abstract class EffectObject : GameObject
{
    /// <summary>
    /// Gets or sets the tween type used for transitions.
    /// </summary>
    public TweenType TweenType { get; set; } = TweenType.LINEAR;

    /// <summary>
    /// Gets the normalized lifetime progress (0 = start, 1 = end).
    /// </summary>
    public float LifetimeF => 1f - lifetimeTimer.F;

    /// <summary>
    /// Timer used to track the lifetime of the effect.
    /// </summary>
    protected BasicTimer lifetimeTimer = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectObject"/> class.
    /// </summary>
    /// <param name="pos">The position of the effect object.</param>
    /// <param name="size">The size of the effect object.</param>
    public EffectObject(Vector2 pos, Size size)
    {
        Transform = new(pos, 0f, size);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectObject"/> class with a specified lifetime.
    /// </summary>
    /// <param name="pos">The position of the effect object.</param>
    /// <param name="size">The size of the effect object.</param>
    /// <param name="lifeTime">The lifetime of the effect object in seconds.</param>
    public EffectObject(Vector2 pos, Size size, float lifeTime)
    {
        Transform = new(pos, 0f, size);
        lifetimeTimer.Start(lifeTime);
    }

    /// <summary>
    /// Attempts to kill the effect object.
    /// </summary>
    /// <param name="killMessage">Optional kill message.</param>
    /// <param name="killer">Optional killer object.</param>
    /// <returns>True if the object was killed; otherwise, false.</returns>
    protected override bool TryKill(string? killMessage = null, GameObject? killer = null)
    {
        lifetimeTimer.Stop();
        return true;
    }
    
    /// <summary>
    /// Gets the bounding box of the effect object.
    /// </summary>
    /// <returns>The bounding box as a <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox() { return new(Transform.Position, Transform.ScaledSize, new(0.5f)); }

    /// <summary>
    /// Tweens a float value from start to end based on the effect's lifetime and tween type.
    /// </summary>
    /// <param name="start">The start value.</param>
    /// <param name="end">The end value.</param>
    /// <returns>The tweened float value.</returns>
    protected float GetTweenFloat(float start, float end) { return ShapeTween.Tween(start, end, LifetimeF, TweenType); }

    /// <summary>
    /// Tweens a <see cref="Vector2"/> value from start to end based on the effect's lifetime and tween type.
    /// </summary>
    /// <param name="start">The start vector.</param>
    /// <param name="end">The end vector.</param>
    /// <returns>The tweened <see cref="Vector2"/> value.</returns>
    protected Vector2 GetTweenVector2(Vector2 start, Vector2 end) { return start.Tween(end, LifetimeF, TweenType); }

    /// <summary>
    /// Tweens a <see cref="ColorRgba"/> value from start to end based on the effect's lifetime and tween type.
    /// </summary>
    /// <param name="startColorRgba">The start color.</param>
    /// <param name="endColorRgba">The end color.</param>
    /// <returns>The tweened <see cref="ColorRgba"/> value.</returns>
    protected ColorRgba GetTweenColor(ColorRgba startColorRgba, ColorRgba endColorRgba) { return startColorRgba.Tween(endColorRgba, LifetimeF, TweenType); }

    /// <summary>
    /// Updates the effect object and its lifetime.
    /// </summary>
    /// <param name="time">The current game time.</param>
    /// <param name="game">The game screen info.</param>
    /// <param name="gameUi">The game UI screen info.</param>
    /// <param name="ui">The UI screen info.</param>
    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        lifetimeTimer.Update(time.Delta);
        if (lifetimeTimer.IsFinished) Kill();
    }
}

