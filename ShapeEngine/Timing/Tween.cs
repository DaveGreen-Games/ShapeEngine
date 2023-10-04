
using ShapeEngine.Core;
using ShapeEngine.Lib;

namespace ShapeEngine.Timing
{
    public class Tween : ISequenceable
    {
        public delegate bool TweenFunc(float f);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;

        public Tween(TweenFunc tweenFunc, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
        }
        public Tween(Tween tween)
        {
            this.func = tween.func;
            this.duration = tween.duration;
            this.timer = tween.timer;
            this.tweenType = tween.tweenType;
        }

        public ISequenceable Copy() => new Tween(this);
        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            float f = ShapeTween.Tween(t, tweenType);

            timer += dt;
            return func(f) || t >= 1f;
        }


    }
}


