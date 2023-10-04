using ShapeEngine.Lib;

namespace ShapeEngine.Timing;

public class TweenInt : ISequenceable
{
    public delegate bool TweenFunc(int result);

    private TweenFunc func;
    private float duration;
    private float timer;
    private TweenType tweenType;
    private int from;
    private int to;

    public TweenInt(TweenFunc tweenFunc, int from, int to, float duration, TweenType tweenType)
    {
        this.func = tweenFunc;
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
    }
    public TweenInt(TweenInt tween)
    {
        this.func = tween.func;
        this.duration = tween.duration;
        this.timer = tween.timer;
        this.tweenType = tween.tweenType;
        this.from = tween.from;
        this.to = tween.to;
    }

    public ISequenceable Copy() => new TweenInt(this);
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = Clamp(timer / duration, 0f, 1f);
        timer += dt;
        int result = ShapeTween.Tween(from, to, t, tweenType);

        return func(result) || t >= 1f;
    }
}