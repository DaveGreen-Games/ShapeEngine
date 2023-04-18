
using Raylib_CsLo;
using ShapeCore;
using System.Numerics;
using ShapeLib;

namespace ShapeTiming
{
    //alternator class?

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

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            float f = STween.Tween(t, tweenType);

            timer += dt;
            return func(f) || t >= 1f;
        }


    }
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

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            Vector2 result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
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

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            int result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
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

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            float result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
    public class TweenColor
    {
        public delegate bool TweenFunc(Color result);

        private TweenFunc func;
        private float duration;
        private float timer;
        private TweenType tweenType;
        private Color from;
        private Color to;

        public TweenColor(TweenFunc tweenFunc, Color from, Color to, float duration, TweenType tweenType)
        {
            this.func = tweenFunc;
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            Color result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }
    public class TweenRect
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

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            Rect result = STween.Tween(from, to, t, tweenType);

            return func(result) || t >= 1f;
        }
    }

}


