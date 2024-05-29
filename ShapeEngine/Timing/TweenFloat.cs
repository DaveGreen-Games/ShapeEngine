using ShapeEngine.Lib;

namespace ShapeEngine.Timing;

public class TweenFloat : ISequenceable
{
    public delegate bool TweenFunc(float result);

    private TweenFunc func;
    private float duration;
    private float timer;
    private TweenType tweenType;
    private float from;
    private float to;

    public TweenFloat(TweenFunc tweenFunc, float from, float to, float duration, TweenType tweenType)
    {
        this.func = tweenFunc;
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
    }
    public TweenFloat(TweenFloat tween)
    {
        this.func = tween.func;
        this.duration = tween.duration;
        this.timer = tween.timer;
        this.tweenType = tween.tweenType;
        this.from = tween.from;
        this.to = tween.to;
    }

    public ISequenceable Copy() => new TweenFloat(this);
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = ShapeMath.Clamp(timer / duration, 0f, 1f);
        timer += dt;
        float result = ShapeTween.Tween(from, to, t, tweenType);

        return func(result) || t >= 1f;
    }
}