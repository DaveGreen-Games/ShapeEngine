using ShapeEngine.Lib;

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

    public bool Update(float dt)
    {
        if (duration <= 0f) return true;
        float t = Clamp(timer / duration, 0f, 1f);
        timer += dt;
        cur = ShapeTween.Tween(from, to, t, tweenType);

        return t >= 1f;
    }

    public float GetRotationDeg() { return cur; }
}