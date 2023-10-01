using System.Numerics;

namespace ShapeEngine.Lib;

public static class ShapeMath
{
    public const float DEGTORAD = MathF.PI / 180f;
    public const float RADTODEG = 180f / MathF.PI;

    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }
    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }

    public static int MaxInt(int value1, int value2)
    {
        if (value1 > value2) return value1;
        else return value2;
    }
    public static int MinInt(int value1, int value2)
    {
        if (value1 < value2) return value1;
        else return value2;
    }
    public static int AbsInt(int value)
    {
        return (int)MathF.Abs(value);
    }

    
    public static T LerpCollection<T>(List<T> collection, float f)
    {
        int index = WrapIndex(collection.Count, (int)(collection.Count * f));
        return collection[index];
    }

    public static dynamic LerpDynamic(dynamic from, dynamic to, float f)
    {
        if (from is float) return LerpFloat(from, to, f);
        else if (from is int) return LerpInt(from, to, f);
        else if (from is Vector2) return ShapeVec.Lerp(from, to, f);
        else if (from is Raylib_CsLo.Color) return ShapeColor.Lerp(from, to, f);
        else return from;
    }

    /// <summary>
    /// Get a frame independent rate to use in lerp functions as t value.
    /// </summary>
    /// <param name="speed">Value has to be positive.
    /// A value of 1 means the halfway point is reached in 0.5 seconds.
    /// A value of 2 means twice as fast and so on.</param>
    /// <param name="dt">The current delta time.</param>
    /// <param name="power">The power to use.</param>
    /// <returns>Returns the rate to be used in lerp functions.</returns>
    public static float FrameIndepentLerpRate(float speed, float dt, float power = 2f)
    {
        if (power <= 0f) return 0f;
        float factor = speed * dt;
        if (factor < 0f) return 0f;
        if (factor > 1f) return 1f;
        
        return MathF.Pow(-speed * dt, power);
    }
    private static float CalculateFrameIndependentLerpFactor_DEPRECATED(float lerpPercentage, float dt)
    {
        float rate = 1f - MathF.Pow(1f - lerpPercentage, dt);
        return rate;
    }

    public static float LerpFloat(float from, float to, float f)
    {
        return (1.0f - f) * from + to * f;
        //return RayMath.Lerp(from, to, f);
    }
    public static float LerpInverseFloat(float from, float to, float value)
    {
        return (value - from) / (to - from);
    }
    public static float RemapFloat(float value, float minOld, float maxOld, float minNew, float maxNew)
    {
        return LerpFloat(minNew, maxNew, LerpInverseFloat(minOld, maxOld, value));
    }

    public static int LerpInt(int from, int to, float f)
    {
        return (int)LerpFloat(from, to, f);
    }
    public static float LerpInverseInt(int from, int to, int value)
    {
        float cur = (float)(value - from);
        float max = (float)(to - from);
        return cur / max;
    }
    public static int RemapInt(int value, int minOld, int maxOld, int minNew, int maxNew)
    {
        return LerpInt(minNew, maxNew, LerpInverseInt(minOld, maxOld, value));
    }

    
    public static float LerpAngleRad(float from, float to, float f)
    {
        return from + GetShortestAngleRad(from, to) * f;
    }

    public static float LerpAngleDeg(float from, float to, float f)
    {
        return from + GetShortestAngleDeg(from, to) * f;
    }

    public static int WrapIndex(int count, int index)
    {
        if (index >= count) return index % count;
        else if (index < 0) return (index % count) + count;
        else return index;
    }
    public static float WrapF(float value, float min, float max)
    {
        float range = max - min;
        return range == 0 ? min : value - range * MathF.Floor((value - min) / range);
    }

    public static int WrapI(int value, int min, int max)
    {
        int range = max - min;
        return range == 0 ? min : value - range * (int)MathF.Floor((value - min) / range);
    }

    
    public static float WrapAngleRad(float amount)
    {
        return WrapF(amount, 0f, 2.0f * MathF.PI);
    }
    public static float WrapAngleDeg(float amount)
    {
        return WrapF(amount, 0f, 360f);
    }
    
    public static float GetShortestAngleRad(float from, float to)
    {
        //from = WrapAngleRad(from);
        //to = WrapAngleRad(to);
        float dif = to - from;
        if (MathF.Abs(dif) > MathF.PI)
        {
            if (dif > 0) dif -= 2f * MathF.PI;
            else if (dif < 0) dif += 2f * MathF.PI;
        }
        return dif;

        //return WrapF(to - from, 0f, PI * 2f);
    }
    public static float GetShortestAngleDeg(float from, float to)
    {
        float dif = to - from;
        if (MathF.Abs(dif) > 180f)
        {
            if (dif > 0) dif -= 360f;
            else if (dif < 0) dif += 360f;
        }
        return dif;
    }
}