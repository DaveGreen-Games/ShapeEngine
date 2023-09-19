//source
//https://github.com/idbrii/cs-tween/blob/main/Easing.cs

using Raylib_CsLo;
using System.Numerics;
using ShapeEngine.Core;

namespace ShapeEngine.Lib
{
    public enum TweenType
    {
        LINEAR = 0,
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

    public static class STween
    {
        public static float Tween(float t, TweenType tweenType)
        {
            switch (tweenType)
            {
                case TweenType.LINEAR: return t;
                case TweenType.SINE_IN: return SineIn(t);
                case TweenType.SINE_OUT: return SineOut(t);
                case TweenType.SINE_INOUT: return SineInOut(t);
                case TweenType.CIRC_IN: return CircIn(t);
                case TweenType.CIRC_OUT: return CircOut(t);
                case TweenType.CIRC_INOUT: return CircInOut(t);
                case TweenType.CUBIC_IN: return CubicIn(t);
                case TweenType.CUBIC_OUT: return CubicOut(t);
                case TweenType.CUBIC_INOUT: return CubicInOut(t);
                case TweenType.QUAD_IN: return QuadIn(t);
                case TweenType.QUAD_OUT: return QuadOut(t);
                case TweenType.QUAD_INOUT: return QuadInOut(t);
                case TweenType.EXPO_IN: return ExpoIn(t);
                case TweenType.EXPO_OUT: return ExpoOut(t);
                case TweenType.EXPO_INOUT: return ExpoInOut(t);
                case TweenType.BACK_IN: return BackIn(t);
                case TweenType.BACK_OUT: return BackOut(t);
                case TweenType.BACK_INOUT: return BackInOut(t);
                case TweenType.BOUNCE_IN: return BounceIn(t);
                case TweenType.BOUNCE_OUT: return BounceOut(t);
                case TweenType.BOUNCE_INOUT: return BounceInOut(t);
                case TweenType.ELASTIC_IN: return ElasticIn(t);
                case TweenType.ELASTIC_OUT: return ElasticOut(t);
                case TweenType.ELASTIC_INOUT: return ElasticInOut(t);
                default: return t;
            }
        }
        public static float Tween(float from, float to, float t, TweenType tweenType)
        {
            return SUtils.LerpFloat(from, to, Tween(t, tweenType));
        }
        public static int Tween(int from, int to, float t, TweenType tweenType)
        {
            return SUtils.LerpInt(from, to, Tween(t, tweenType));
        }
        public static Vector2 Tween(this Vector2 from, Vector2 to, float t, TweenType tweenType)
        {
            return from.Lerp(to, Tween(t, tweenType));// SVec.Lerp(from, to, Tween(t, tweenType));
        }
        public static Raylib_CsLo.Color Tween(this Raylib_CsLo.Color from, Raylib_CsLo.Color to, float t, TweenType tweenType)
        {
            return from.Lerp(to, Tween(t, tweenType)); // SColor.LerpColor(from, to, Tween(t, tweenType));
        }
        public static Rect Tween(this Rect from, Rect to, float t, TweenType tweenType)
        {
            return from.Lerp(to, Tween(t, tweenType)); // SColor.LerpColor(from, to, Tween(t, tweenType));
        }
        public static float QuartIn(float p)
        {
            return p * p * p * p;
        }
        public static float QuartOut(float p)
        {
            p = 1f - p;
            return 1f - (QuartIn(p));
        }
        public static float QuartInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (QuartIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (QuartIn(p))) + 0.5f;
            }
        }

        public static float QuintIn(float p)
        {
            return p * p * p * p * p;
        }
        public static float QuintOut(float p)
        {
            p = 1f - p;
            return 1f - (QuintIn(p));
        }
        public static float QuintInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (QuintIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (QuintIn(p))) + 0.5f;
            }
        }

        public static float CubicIn(float p)
        {
            return p * p * p;
        }
        public static float CubicOut(float p)
        {
            p = 1f - p;
            return 1f - (CubicIn(p));
        }
        public static float CubicInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (CubicIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (CubicIn(p))) + 0.5f;
            }
        }

        public static float ElasticIn(float p)
        {
            return -(MathF.Pow(2, 10 * (p - 1f)) * MathF.Sin((p - 1.075f) * (MathF.PI * 2) / 0.3f));
        }
        public static float ElasticOut(float p)
        {
            p = 1f - p;
            return 1f - (ElasticIn(p));
        }
        public static float ElasticInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (ElasticIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (ElasticIn(p))) + 0.5f;
            }
        }

        public static float BackIn(float p)
        {
            return p * p * (2.7f * p - 1.7f);
        }
        public static float BackOut(float p)
        {
            p = 1f - p;
            return 1f - (BackIn(p));
        }
        public static float BackInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (BackIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (BackIn(p))) + 0.5f;
            }
        }

        public static float SineIn(float p)
        {
            return -MathF.Cos(p * (MathF.PI * 0.5f)) + 1f;
        }
        public static float SineOut(float p)
        {
            p = 1f - p;
            return 1f - (SineIn(p));
        }
        public static float SineInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (SineIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (SineIn(p))) + 0.5f;
            }
        }

        public static float ExpoIn(float p)
        {
            return MathF.Pow(2, (10 * (p - 1f)));
        }
        public static float ExpoOut(float p)
        {
            p = 1f - p;
            return 1f - (ExpoIn(p));
        }
        public static float ExpoInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (ExpoIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (ExpoIn(p))) + 0.5f;
            }
        }

        public static float QuadIn(float p)
        {
            return p * p;
        }
        public static float QuadOut(float p)
        {
            p = 1f - p;
            return 1f - (QuadIn(p));
        }
        public static float QuadInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (QuadIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (QuadIn(p))) + 0.5f;
            }
        }

        public static float CircIn(float p)
        {
            return -(MathF.Sqrt(1f - (p * p)) - 1f);
        }
        public static float CircOut(float p)
        {
            p = 1f - p;
            return 1f - (CircIn(p));
        }
        public static float CircInOut(float p)
        {
            p = p * 2;
            if (p < 1f)
            {
                return 0.5f * (CircIn(p));
            }
            else
            {
                p = 2f - p;
                return 0.5f * (1f - (CircIn(p))) + 0.5f;
            }
        }

        public static float BounceIn(float p)
        {
            return 1f - BounceOut(1f - p);
        }
        public static float BounceOut(float p)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (p < 1f / d1)
            {
                return n1 * p * p;
            }
            else if (p < 2f / d1)
            {
                return n1 * (p -= 1.5f / d1) * p + 0.75f;
            }
            else if (p < 2.5f / d1)
            {
                return n1 * (p -= 2.25f / d1) * p + 0.9375f;
            }
            else
            {
                return n1 * (p -= 2.625f / d1) * p + 0.984375f;
            }
        }
        public static float BounceInOut(float p)
        {
            return p < 0.5f
                ? (1f - BounceOut(1f - 2f * p)) / 2f
                : (1f + BounceOut(2f * p - 1f)) / 2f;
        }

    }
}


//SEase backup

/*

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


*/