using ShapeEngine.Color;
using ShapeEngine.Lib;

namespace ShapeEngine.Timing;

public class TweenColor : ISequenceable
{
    public delegate bool TweenFunc(ShapeColor result);

    private TweenFunc func;
    private float duration;
    private float timer;
    private TweenType tweenType;
    private ShapeColor from;
    private ShapeColor to;

    public TweenColor(TweenFunc tweenFunc, ShapeColor from, ShapeColor to, float duration, TweenType tweenType)
    {
        this.func = tweenFunc;
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
    }

    public TweenColor(TweenColor tween)
    {
        this.func = tween.func;
        this.duration = tween.duration;
        this.timer = tween.timer;
        this.tweenType = tween.tweenType;
        this.from = tween.from;
        this.to = tween.to;
    }

    public ISequenceable Copy() => new TweenColor(this);
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = Clamp(timer / duration, 0f, 1f);
        timer += dt;
        var result = from.Tween(to, t, tweenType);

        return func(result) || t >= 1f;
    }
}