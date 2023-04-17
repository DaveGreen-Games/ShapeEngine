using System;
using System.Numerics;
using Raylib_CsLo;
using ShapeColor;


namespace ShapeLib
{
    public enum EasingType
    {
        LINEAR_NONE = 0,
        LINEAR_IN = 1,
        LINEAR_OUT = 2,
        LINEAR_INOUT = 3,
        SINE_IN = 4,
        SINE_OUT = 5,
        SINE_INOUT = 6,
        CIRC_IN = 7,
        CIRC_OUT = 8,
        CIRC_INOUT = 9,
        CUBIC_IN = 10,
        CUBIC_OUT = 11,
        CUBIC_INOUT = 12,
        QUAD_IN = 13,
        QUAD_OUT = 14,
        QUAD_INOUT = 15,
        EXPO_IN = 16,
        EXPO_OUT = 17,
        EXPO_INOUT = 18,
        BACK_IN = 19,
        BACK_OUT = 20,
        BACK_INOUT = 21,
        BOUNCE_IN = 22,
        BOUNCE_OUT = 23,
        BOUNCE_INOUT = 24,
        ELASTIC_IN = 25,
        ELASTIC_OUT = 26,
        ELASTIC_INOUT = 27
    }

    public static class SEase
    {
        public static float Advanced(float from, float change, float time, float maxTime, EasingType easingType)
        {
            switch (easingType)
            {
                case EasingType.LINEAR_NONE: return SUtils.LerpFloat(from, from + change, time / maxTime);// Easings.EaseLinearNone(time, from, to, maxTime);
                case EasingType.LINEAR_IN: return Easings.EaseLinearIn(time, from, change, maxTime);
                case EasingType.LINEAR_OUT: return Easings.EaseLinearOut(time, from, change, maxTime);
                case EasingType.LINEAR_INOUT: return Easings.EaseLinearInOut(time, from, change, maxTime);
                case EasingType.SINE_IN: return Easings.EaseSineIn(time, from, change, maxTime);
                case EasingType.SINE_OUT: return Easings.EaseSineOut(time, from, change, maxTime);
                case EasingType.SINE_INOUT: return Easings.EaseSineInOut(time, from, change, maxTime);
                case EasingType.CIRC_IN: return Easings.EaseCircIn(time, from, change, maxTime);
                case EasingType.CIRC_OUT: return Easings.EaseCircOut(time, from, change, maxTime);
                case EasingType.CIRC_INOUT: return Easings.EaseCircInOut(time, from, change, maxTime);
                case EasingType.CUBIC_IN: return Easings.EaseCubicIn(time, from, change, maxTime);
                case EasingType.CUBIC_OUT: return Easings.EaseCubicOut(time, from, change, maxTime);
                case EasingType.CUBIC_INOUT: return Easings.EaseCubicInOut(time, from, change, maxTime);
                case EasingType.QUAD_IN: return Easings.EaseQuadIn(time, from, change, maxTime);
                case EasingType.QUAD_OUT: return Easings.EaseQuadOut(time, from, change, maxTime);
                case EasingType.QUAD_INOUT: return Easings.EaseQuadInOut(time, from, change, maxTime);
                case EasingType.EXPO_IN: return Easings.EaseExpoIn(time, from, change, maxTime);
                case EasingType.EXPO_OUT: return Easings.EaseExpoOut(time, from, change, maxTime);
                case EasingType.EXPO_INOUT: return Easings.EaseExpoInOut(time, from, change, maxTime);
                case EasingType.BACK_IN: return Easings.EaseBackIn(time, from, change, maxTime);
                case EasingType.BACK_OUT: return Easings.EaseBackOut(time, from, change, maxTime);
                case EasingType.BACK_INOUT: return Easings.EaseBackInOut(time, from, change, maxTime);
                case EasingType.BOUNCE_IN: return Easings.EaseBounceIn(time, from, change, maxTime);
                case EasingType.BOUNCE_OUT: return Easings.EaseBounceOut(time, from, change, maxTime);
                case EasingType.BOUNCE_INOUT: return Easings.EaseBounceInOut(time, from, change, maxTime);
                case EasingType.ELASTIC_IN: return Easings.EaseElasticIn(time, from, change, maxTime);
                case EasingType.ELASTIC_OUT: return Easings.EaseElasticOut(time, from, change, maxTime);
                case EasingType.ELASTIC_INOUT: return Easings.EaseElasticInOut(time, from, change, maxTime);
                default: return from;
            }
        }
        public static float Advanced(float from, float change, int frames, int maxFrames, EasingType easingType)
        {
            return Advanced(from, change, frames, (float)maxFrames, easingType);
        }
        public static Vector2 Advanced(Vector2 from, Vector2 change, float time, float maxTime, EasingType easingType)
        {
            return new
            (
                Advanced(from.X, change.X, time, maxTime, easingType),
                Advanced(from.Y, change.Y, time, maxTime, easingType)
            );
        }
        public static Vector2 Advanced(Vector2 from, Vector2 change, int frames, int maxFrames, EasingType easingType)
        {
            return new
            (
                Advanced(from.X, change.X, frames, (float)maxFrames, easingType),
                Advanced(from.Y, change.Y, frames, (float)maxFrames, easingType)
            );
        }
        public static Color Advanced(Color from, Color change, float time, float maxTime, EasingType easingType)
        {
            return new
            (
                (byte)Advanced(from.r, change.r, time, maxTime, easingType),
                (byte)Advanced(from.g, change.g, time, maxTime, easingType),
                (byte)Advanced(from.b, change.b, time, maxTime, easingType),
                (byte)Advanced(from.a, change.a, time, maxTime, easingType)
            );
        }
        public static Color Advanced(Color from, Color change, int frames, int maxFrames, EasingType easingType)
        {
            return new
            (
                (byte)Advanced(from.r, change.r, frames, (float)maxFrames, easingType),
                (byte)Advanced(from.g, change.g, frames, (float)maxFrames, easingType),
                (byte)Advanced(from.b, change.b, frames, (float)maxFrames, easingType),
                (byte)Advanced(from.a, change.a, frames, (float)maxFrames, easingType)
            );
        }

        public static float AdvancedTo(float from, float to, float time, float maxTime, EasingType easingType)
        {
            return Advanced(from, to - from, time, maxTime, easingType);
        }
        public static float AdvancedTo(float from, float to, int frames, int maxFrames, EasingType easingType)
        {
            return Advanced(from, to - from, frames, (float)maxFrames, easingType);
        }
        public static Vector2 AdvancedTo(Vector2 from, Vector2 to, float time, float maxTime, EasingType easingType)
        {
            return new
            (
                Advanced(from.X, to.X - from.X, time, maxTime, easingType),
                Advanced(from.Y, to.Y - from.Y, time, maxTime, easingType)
            );
        }
        public static Vector2 AdvancedTo(Vector2 from, Vector2 to, int frames, int maxFrames, EasingType easingType)
        {
            return new
            (
                Advanced(from.X, to.X - from.Y, frames, (float)maxFrames, easingType),
                Advanced(from.Y, to.X - from.Y, frames, (float)maxFrames, easingType)
            );
        }
        public static Color AdvancedTo(Color from, Color to, float time, float maxTime, EasingType easingType)
        {
            return new
            (
                (byte)Advanced(from.r, to.r - from.r, time, maxTime, easingType),
                (byte)Advanced(from.g, to.g - from.g, time, maxTime, easingType),
                (byte)Advanced(from.b, to.b - from.b, time, maxTime, easingType),
                (byte)Advanced(from.a, to.a - from.a, time, maxTime, easingType)
            );
        }
        public static Color AdvancedTo(Color from, Color to, int frames, int maxFrames, EasingType easingType)
        {
            return new
            (
                (byte)Advanced(from.r, to.r - from.r, frames, (float)maxFrames, easingType),
                (byte)Advanced(from.g, to.g - from.g, frames, (float)maxFrames, easingType),
                (byte)Advanced(from.b, to.b - from.b, frames, (float)maxFrames, easingType),
                (byte)Advanced(from.a, to.a - from.a, frames, (float)maxFrames, easingType)
            );
        }




        public static float Simple(float t, EasingType easingType)
        {
            switch (easingType)
            {
                case EasingType.LINEAR_NONE: return t;
                case EasingType.LINEAR_IN: return t;
                case EasingType.LINEAR_OUT: return t;
                case EasingType.LINEAR_INOUT: return t;
                case EasingType.SINE_IN: return EaseInSine(t);
                case EasingType.SINE_OUT: return EaseOutSine(t);
                case EasingType.SINE_INOUT: return EaseInOutSine(t);
                case EasingType.CIRC_IN: return EaseInCirc(t);
                case EasingType.CIRC_OUT: return EaseOutCirc(t);
                case EasingType.CIRC_INOUT: return EaseInOutCirc(t);
                case EasingType.CUBIC_IN: return EaseInCubic(t);
                case EasingType.CUBIC_OUT: return EaseOutCubic(t);
                case EasingType.CUBIC_INOUT: return EaseInOutCubic(t);
                case EasingType.QUAD_IN: return EaseInQuad(t);
                case EasingType.QUAD_OUT: return EaseOutQuad(t);
                case EasingType.QUAD_INOUT: return EaseInOutQuad(t);
                case EasingType.EXPO_IN: return EaseInExpo(t);
                case EasingType.EXPO_OUT: return EaseOutExpo(t);
                case EasingType.EXPO_INOUT: return EaseInOutExpo(t);
                case EasingType.BACK_IN: return EaseInBack(t);
                case EasingType.BACK_OUT: return EaseOutBack(t);
                case EasingType.BACK_INOUT: return EaseInOutBack(t);
                case EasingType.BOUNCE_IN: return EaseInBounce(t);
                case EasingType.BOUNCE_OUT: return EaseOutBounce(t);
                case EasingType.BOUNCE_INOUT: return EaseInOutBounce(t);
                case EasingType.ELASTIC_IN: return EaseInElastic(t);
                case EasingType.ELASTIC_OUT: return EaseOutElastic(t);
                case EasingType.ELASTIC_INOUT: return EaseInOutElastic(t);
                default: return t;
            }
        }
        public static float Simple(float from, float to, float t, EasingType easingType)
        {
            return SUtils.LerpFloat(from, to, Simple(t, easingType));
        }
        public static Vector2 Simple(Vector2 from, Vector2 to, float t, EasingType easingType)
        {
            return SVec.Lerp(from, to, Simple(t, easingType));
        }
        public static Color Simple(Color from, Color to, float t, EasingType easingType)
        {
            return SColor.LerpColor(from, to, Simple(t, easingType));
        }

        public static float EaseInSine(float t)
        {
            return 1f - MathF.Cos(t * PI / 2f);
        }
        public static float EaseOutSine(float t)
        {
            return MathF.Sin(t * PI / 2f);
        }
        public static float EaseInOutSine(float t)
        {
            return -(MathF.Cos(PI * t) - 1f) / 2f;
        }

        public static float EaseInCubic(float t)
        {
            return t * t * t;
        }
        public static float EaseOutCubic(float t)
        {
            return 1f - MathF.Pow(1f - t, 3f);
        }
        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static float EaseInQuint(float t)
        {
            return t * t * t * t * t;
        }
        public static float EaseOutQuint(float t)
        {
            return 1f - MathF.Pow(1f - t, 5f);
        }
        public static float EaseInOutQuint(float t)
        {
            return t < 0.5f ? 16f * t * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 5f) / 2f;
        }

        public static float EaseInCirc(float t)
        {
            return 1f - MathF.Sqrt(1f - MathF.Pow(t, 2f));
        }
        public static float EaseOutCirc(float t)
        {
            return MathF.Sqrt(1f - MathF.Pow(t - 1f, 2f));
        }
        public static float EaseInOutCirc(float t)
        {
            return t < 0.5f
                ? (1f - MathF.Sqrt(1f - MathF.Pow(2f * t, 2f))) / 2f
                : (MathF.Sqrt(1f - MathF.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;
        }

        public static float EaseInElastic(float t)
        {
            const float c4 = 2f * PI / 3f;
            return t == 0f
            ? 0f
            : t == 1f
            ? 1f
              : -MathF.Pow(2f, 10f * t - 10f) * MathF.Sin((t * 10f - 10.75f) * c4);
        }
        public static float EaseOutElastic(float t)
        {
            const float c4 = 2f * PI / 3f;

            return t == 0f
              ? 0f
              : t == 1f
              ? 1f
              : MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
        public static float EaseInOutElastic(float t)
        {
            const float c5 = 2f * PI / 4.5f;

            return t == 0f
              ? 0f
              : t == 1f
              ? 1f
              : t < 0.5f
              ? -(MathF.Pow(2f, 20f * t - 10f) * MathF.Sin((20f * t - 11.125f) * c5)) / 2f
              : MathF.Pow(2f, -20f * t + 10f) * MathF.Sin((20f * t - 11.125f) * c5) / 2f + 1f;
        }

        public static float EaseInQuad(float t)
        {
            return t * t;
        }
        public static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;
        }

        public static float EaseInQuart(float t)
        {
            return t * t * t * t;
        }
        public static float EaseOutQuart(float t)
        {
            return 1f - MathF.Pow(1f - t, 4f);
        }
        public static float EaseInOutQuart(float t)
        {
            return t < 0.5f ? 8f * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 4f) / 2f;
        }

        public static float EaseInExpo(float t)
        {
            return t == 0f ? 0f : MathF.Pow(2f, 10f * t - 10f);
        }
        public static float EaseOutExpo(float t)
        {
            return t == 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);
        }
        public static float EaseInOutExpo(float t)
        {
            return t == 0f
                ? 0f
                : t == 1f
                ? 1f
                : t < 0.5f ? MathF.Pow(2f, 20f * t - 10f) / 2f
                : (2f - MathF.Pow(2f, -20f * t + 10f)) / 2f;
        }

        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return c3 * t * t * t - c1 * t * t;
        }
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }
        public static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return t < 0.5f
              ? MathF.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2) / 2f
              : (MathF.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }

        public static float EaseInBounce(float t)
        {
            return 1f - EaseOutBounce(1f - t);
        }
        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1f / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2f / d1)
            {
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
            }
        }
        public static float EaseInOutBounce(float t)
        {
            return t < 0.5f
                ? (1f - EaseOutBounce(1f - 2f * t)) / 2f
                : (1f + EaseOutBounce(2f * t - 1f)) / 2f;
        }

    }
}



/*
public struct EaseValue
{
    public enum EaseValueType
    {
        FLOAT = 0,
        VEC = 1,
        COLOR = 2,
        FLOAT_VEC = 3,
        VEC_COLOR = 4,
        FLOAT_COLOR = 5,
        FLOAT_VEL_COLOR = 6
    }
    public float f;
    public Vector2 v;
    public Color c;
    public EaseValueType type = EaseValueType.FLOAT;
    public EaseValue(float f) { this.f = f; this.v = new(); this.c = new(); type = EaseValueType.FLOAT; }
    public EaseValue(Vector2 v) { this.v = v; this.f = 0f; this.c = new(); type = EaseValueType.VEC; }
    public EaseValue(Color c) { this.c = c; this.f = 0f; this.v = new(); type = EaseValueType.COLOR; }
    public EaseValue(float f, Vector2 v) { this.f = f; this.v = v; this.c = new(); type = EaseValueType.FLOAT_VEC; }
    public EaseValue(Vector2 v, Color c) { this.v = v; this.c = c; this.f = 0f; type = EaseValueType.VEC_COLOR; }
    public EaseValue(float f, Color c) { this.f = f; this.c = c; this.v = new(); type = EaseValueType.FLOAT_COLOR; }
    public EaseValue(float f, Vector2 v, Color c) { this.f = f; this.v = v; this.c = c; type = EaseValueType.FLOAT_VEL_COLOR; }

    public readonly bool HasFloat() { return type == EaseValueType.FLOAT || type == EaseValueType.FLOAT_VEC || type == EaseValueType.FLOAT_COLOR || type == EaseValueType.FLOAT_VEL_COLOR; }
    public readonly bool HasVec() { return type == EaseValueType.VEC || type == EaseValueType.FLOAT_VEC || type == EaseValueType.VEC_COLOR || type == EaseValueType.FLOAT_VEL_COLOR; }
    public readonly bool HasColor() { return type == EaseValueType.COLOR || type == EaseValueType.FLOAT_COLOR || type == EaseValueType.VEC_COLOR || type == EaseValueType.FLOAT_VEL_COLOR; }
}
public struct EaseInfo
{
    public EaseValue change;
    public float duration = 0f;
    public float delay = 0f;
    public EasingType easingType = EasingType.LINEAR_NONE;

    public EaseInfo(float change, float duration, EasingType easingType, float delay = 0f)
    {
        this.change = new(change);
        this.duration = duration;
        this.delay = delay;
        this.easingType = easingType;
    }
    public EaseInfo(Vector2 change, float duration, EasingType easingType, float delay = 0f)
    {
        this.change = new(change);
        this.duration = duration;
        this.delay = delay;
        this.easingType = easingType;
    }
    public EaseInfo(Color change, float duration, EasingType easingType, float delay = 0f)
    {
        this.change = new(change);
        this.duration = duration;
        this.delay = delay;
        this.easingType = easingType;
    }
    public EaseInfo(EaseValue change, float duration, EasingType easingType, float delay = 0f)
    {
        this.change = change;
        this.duration = duration;
        this.delay = delay;
        this.easingType = easingType;
    }
}
public class EaseChain
{
    List<EaseInfo> easeInfo = new();
    EaseValue from;
    EaseValue cur;
    EaseOrder? curEaseOrder = null;

    public EaseChain(float from, params EaseInfo[] chains)
    {
        easeInfo = chains.ToList();
        easeInfo.Reverse(); //so we can pop from the back of the list
        this.from = new(from);
        this.cur = new(from);
        curEaseOrder = GetNextEaseOrder();
    }
    public EaseChain(Vector2 from, params EaseInfo[] chains)
    {
        easeInfo = chains.ToList();
        easeInfo.Reverse(); //so we can pop from the back of the list
        this.from = new(from);
        this.cur = new(from);
        curEaseOrder = GetNextEaseOrder();
    }
    public EaseChain(Color from, params EaseInfo[] chains)
    {
        easeInfo = chains.ToList();
        easeInfo.Reverse(); //so we can pop from the back of the list
        this.from = new(from);
        this.cur = new(from);
        curEaseOrder = GetNextEaseOrder();
    }
    public EaseChain(EaseValue from, params EaseInfo[] chains)
    {
        easeInfo = chains.ToList();
        easeInfo.Reverse(); //so we can pop from the back of the list
        this.from = from;
        this.cur = from;
        curEaseOrder = GetNextEaseOrder();
    }

    public bool IsFinished() { return curEaseOrder == null; }
    public void Update(float dt)
    {
        if (curEaseOrder == null) return;
        curEaseOrder.Update(dt);
        cur = curEaseOrder.GetCur();
        if (curEaseOrder.IsFinished())
        {
            curEaseOrder = GetNextEaseOrder();
        }

    }
    public EaseValue GetCur() { return cur; }
    private EaseOrder? GetNextEaseOrder()
    {
        if (easeInfo.Count <= 0) return null;

        int lastIndex = easeInfo.Count - 1;
        var info = easeInfo[lastIndex];
        easeInfo.RemoveAt(lastIndex);
        return new(cur, info.change, info.duration, info.delay, info.easingType);
    }
}
public class EaseOrder
{
    EaseValue from;
    EaseValue change;
    EaseValue cur;

    EasingType easingType = EasingType.LINEAR_NONE;
    float duration;
    float timer;
    bool started = false;

    public EaseOrder(float from, float change, float duration, float delay, EasingType easingType)
    {
        this.from = new(from);
        this.change = new(change);
        this.cur = new(from);
        this.duration = duration;
        if(delay <= 0f)
        {
            this.timer = duration;
            this.started = true;
        }
        else this.timer = delay;
        this.easingType = easingType;

        if(this.started) UpdateCur();
    }
    public EaseOrder(Vector2 from, Vector2 change, float duration, float delay, EasingType easingType)
    {
        this.from = new(from);
        this.change = new(change);
        this.cur = new(from);
        this.duration = duration;
        if (delay <= 0f)
        {
            this.timer = duration;
            this.started = true;
        }
        else this.timer = delay;
        this.easingType = easingType;

        if (this.started) UpdateCur();
    }
    public EaseOrder(Color from, Color change, float duration, float delay, EasingType easingType)
    {
        this.from = new(from);
        this.change = new(change);
        this.cur = new(from);
        this.duration = duration;
        if (delay <= 0f)
        {
            this.timer = duration;
            this.started = true;
        }
        else this.timer = delay;
        this.easingType = easingType;

        if (this.started) UpdateCur();
    }
    public EaseOrder(EaseValue from, EaseValue change, float duration, float delay, EasingType easingType)
    {
        this.from = from;
        this.change = change;
        this.cur = from;
        this.duration = duration;
        if (delay <= 0f)
        {
            this.timer = duration;
            this.started = true;
        }
        else this.timer = delay;
        this.easingType = easingType;

        if (this.started) UpdateCur();
    }

    public EaseValue GetCur() { return cur; }
    public bool IsFinished() { return started && timer <= 0f; }
    public void Update(float dt)
    {
        if (!IsFinished())
        {
            timer -= dt;
            if(timer <= 0f)
            {
                timer = 0f;
                if (!started)
                {
                    started = true;
                    return;
                }
            }
            if (started) UpdateCur();
        }

    }

    private void UpdateCur()
    {
        switch (cur.type)
        {
            case EaseValue.EaseValueType.FLOAT:
                cur.f = Ease.Advanced(from.f, change.f, duration - timer, duration, easingType);
                break;
            case EaseValue.EaseValueType.VEC:
                cur.v = Ease.Advanced(from.v, change.v, duration - timer, duration, easingType);
                break;
            case EaseValue.EaseValueType.COLOR:
                cur.c = Ease.Advanced(from.c, change.c, duration - timer, duration, easingType);
                break;
            case EaseValue.EaseValueType.FLOAT_VEC:
                cur.f = Ease.Advanced(from.f, change.f, duration - timer, duration, easingType);
                cur.v = Ease.Advanced(from.v, change.v, duration - timer, duration, easingType);
                break;
            case EaseValue.EaseValueType.VEC_COLOR:
                cur.v = Ease.Advanced(from.v, change.v, duration - timer, duration, easingType);
                cur.c = Ease.Advanced(from.c, change.c, duration - timer, duration, easingType);
                break;
            case EaseValue.EaseValueType.FLOAT_COLOR:
                cur.f = Ease.Advanced(from.f, change.f, duration - timer, duration, easingType);
                cur.c = Ease.Advanced(from.c, change.c, duration - timer, duration, easingType);
                break;
            case EaseValue.EaseValueType.FLOAT_VEL_COLOR:
                cur.f = Ease.Advanced(from.f, change.f, duration - timer, duration, easingType);
                cur.v = Ease.Advanced(from.v, change.v, duration - timer, duration, easingType);
                cur.c = Ease.Advanced(from.c, change.c, duration - timer, duration, easingType);
                break;
        }
    }
}
public class Easer
{
    private Dictionary<string, EaseChain> chains = new();

    public Easer() { }

    public bool HasChain(string name) { return chains.ContainsKey(name); }
    public void Add(string name, EaseChain chain)
    {
        if (HasChain(name)) return; // chains[name] = chain;
        else chains.Add(name, chain);
    }
    public void Remove(string chainName)
    {
        if(!HasChain(chainName)) return;
        chains.Remove(chainName);
    }
    public void Update(float dt)
    {
        var remove = chains.Where(kvp => kvp.Value.IsFinished());
        foreach(var chain in remove)
        {
            chains.Remove(chain.Key);
        }
        foreach (var chain in chains.Values)
        {
            if (chain.IsFinished()) continue;
            chain.Update(dt);
        }
    }
    public EaseValue Get(string chainName)
    {
        if (!HasChain(chainName)) return new();
        return chains[chainName].GetCur();
    }
}
*/


