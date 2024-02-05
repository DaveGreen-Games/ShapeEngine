using ShapeEngine.Core.Shapes;
using ShapeEngine.Lib;

namespace ShapeEngine.Timing;

public class TweenRect : ISequenceable
{
    public delegate bool TweenFunc(Rect result);

    private TweenFunc func;
    private float duration;
    private float timer;
    private TweenType tweenType;
    private Rect from;
    private Rect to;

    public TweenRect(TweenFunc tweenFunc, Rect from, Rect to, float duration, TweenType tweenType)
    {
        this.func = tweenFunc;
        this.duration = duration;
        this.timer = 0f;
        this.tweenType = tweenType;
        this.from = from;
        this.to = to;
    }
    public TweenRect(TweenRect tween)
    {
        this.func = tween.func;
        this.duration = tween.duration;
        this.timer = tween.timer;
        this.tweenType = tween.tweenType;
        this.from = tween.from;
        this.to = tween.to;
    }

    public ISequenceable Copy() => new TweenRect(this);
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = ShapeMath.Clamp(timer / duration, 0f, 1f);
        timer += dt;
        Rect result = ShapeTween.Tween(from, to, t, tweenType);

        return func(result) || t >= 1f;
    }
}