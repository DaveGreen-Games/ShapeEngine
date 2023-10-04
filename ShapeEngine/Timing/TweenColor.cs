using ShapeEngine.Lib;

namespace ShapeEngine.Timing;

public class TweenColor : ISequenceable
{
    public delegate bool TweenFunc(Raylib_CsLo.Color result);

    private TweenFunc func;
    private float duration;
    private float timer;
    private TweenType tweenType;
    private Raylib_CsLo.Color from;
    private Raylib_CsLo.Color to;

    public TweenColor(TweenFunc tweenFunc, Raylib_CsLo.Color from, Raylib_CsLo.Color to, float duration, TweenType tweenType)
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
        Raylib_CsLo.Color result = ShapeTween.Tween(from, to, t, tweenType);

        return func(result) || t >= 1f;
    }
}