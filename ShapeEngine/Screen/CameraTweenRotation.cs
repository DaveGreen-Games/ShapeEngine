using ShapeEngine.Lib;
using ShapeEngine.Timing;

namespace ShapeEngine.Screen;

public class CameraTweenRotation : ICameraTween
{
    private float duration;
    private float timer;
    private TweenType tweenType;
    private float from;
    private float to;
    private float cur;
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

    public ISequenceable Copy() => new CameraTweenRotation(this);
    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = ShapeMath.Clamp(timer / duration, 0f, 1f);
        timer += dt;
        cur = ShapeTween.Tween(from, to, t, tweenType);

        return t >= 1f;
    }

    public float GetRotationDeg() { return cur; }
}