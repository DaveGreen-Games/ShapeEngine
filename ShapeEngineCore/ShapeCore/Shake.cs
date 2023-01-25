using System.Numerics;
using ShapeLib;

namespace ShapeCore
{
    public class Shake
    {
        private float timer = 0.0f;
        private float duration = 0.0f;
        private float smoothness = 0.0f;
        private float curX = 0.0f;
        private float curY = 0.0f;
        private float f = 0.0f;

        public Shake() { }

        public float GetCurX() { return curX; }
        public float GetCurY() { return curY; }
        public bool IsActive() { return timer > 0.0f; }
        public float GetF() { return f; }
        public Vector2 GetCur() { return new(curX, curY); }

        public void Start(float duration, float smoothness)
        {
            timer = duration;
            this.duration = duration;
            this.smoothness = smoothness;
            curX = 0.0f; //RNG.randRangeFloat(-1.0f, 1.0f);
            curX = 0.0f; //RNG.randRangeFloat(0.0f, 1.0f);
            f = 0.0f;
        }

        public void Update(float dt)
        {
            if (timer > 0.0f)
            {
                timer -= dt;
                if (timer <= 0.0f)
                {
                    timer = 0.0f;
                    curX = 0.0f;
                    curY = 0.0f;
                    f = 0.0f;
                    return;
                }
                f = timer / duration;
                //curValue = Lerp(RNG.randRangeFloat(-1.0f, 1.0f) * f, curValue, MathF.Pow(smoothness, dt));
                //float t = MathF.Pow(smoothness, f);
                //curX = Lerp(RNG.randRangeFloat(-1.0f, 1.0f), curX, t);
                //curY = Lerp(RNG.randRangeFloat(-1.0f, 1.0f), curY, t);
                curX = Lerp(SRNG.randF(-1.0f, 1.0f), curX, smoothness) * f;
                curY = Lerp(SRNG.randF(-1.0f, 1.0f), curY, smoothness) * f;
            }
        }
    }


}
