using System.Numerics;
using ShapeEngine.Lib;

namespace ShapeEngine.Timing;

public class TweenVector2 : ISequenceable
{
    public delegate bool TweenFunc(Vector2 result);

    private TweenFunc func;
    private float duration;
    private float timer;
    private TweenType tweenType;
    private Vector2 from;
    private Vector2 to;

    public TweenVector2(TweenFunc tweenFunc, Vector2 from, Vector2 to, float duration, TweenType tweenType)
    {
        this.func = tweenFunc;
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
    }
    public TweenVector2(TweenVector2 tween)
    {
        this.func = tween.func;
        this.duration = tween.duration;
        this.timer = tween.timer;
        this.tweenType = tween.tweenType;
        this.from = tween.from;
        this.to = tween.to;
    }

    public ISequenceable Copy() => new TweenVector2(this);
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = Clamp(timer / duration, 0f, 1f);
        timer += dt;
        Vector2 result = ShapeTween.Tween(from, to, t, tweenType);

        return func(result) || t >= 1f;
    }
}