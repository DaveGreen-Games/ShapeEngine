using ShapeEngine.Lib;
using ShapeEngine.Timing;
using System.Numerics;

namespace ShapeEngine.Screen
{
    public interface ICameraTween : ISequenceable
    {
        public Vector2 GetOffset() { return new(0f); }
        public float GetRotationDeg() { return 0f; }
        public float GetScale() { return 1f; }
    }
    public class CameraTweenOffset : ICameraTween
    {
        private float duration;
        private float timer;
        private TweenType tweenType;
        private Vector2 from;
        private Vector2 to;
        private Vector2 cur;
        public CameraTweenOffset(Vector2 from, Vector2 to, float duration, TweenType tweenType)
        {
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
            this.cur = from;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            cur = STween.Tween(from, to, t, tweenType);

            return t >= 1f;
        }

        public Vector2 GetOffset() { return cur; }
    }
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
            cur = STween.Tween(from, to, t, tweenType);

            return t >= 1f;
        }

        public float GetRoation() { return cur; }
    }
    public class CameraTweenScale : ICameraTween
    {
        private float duration;
        private float timer;
        private TweenType tweenType;
        private float from;
        private float to;
        private float cur;
        public CameraTweenScale(float from, float to, float duration, TweenType tweenType)
        {
            this.duration = duration;
            this.timer = 0f;
            this.tweenType = tweenType;
            this.from = from;
            this.to = to;
            this.cur = from;
        }

        public bool Update(float dt)
        {
            if (duration <= 0f) return true;
            float t = Clamp(timer / duration, 0f, 1f);
            timer += dt;
            cur = STween.Tween(from, to, t, tweenType);

            return t >= 1f;
        }

        public float GetScale() { return cur; }
    }

}
