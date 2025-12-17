using System.Numerics;
using ShapeEngine.Color;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides static mathematical utility functions and constants for shape and vector operations.
/// </summary>
public static class ShapeMath
{

    #region Const

    /// <summary>
    /// The factor to convert degrees to radians.
    /// </summary>
    public const float DEGTORAD = PI / 180f;
    /// <summary>
    /// The factor to convert radians to degrees.
    /// </summary>
    public const float RADTODEG = 180f / PI;
    /// <summary>
    /// The base of the natural logarithm, e.
    /// </summary>
    public const float E = 2.7182817f;
    /// <summary>
    /// The mathematical constant π (pi), the ratio of a circle's circumference to its diameter.
    /// </summary>
    public const float PI = 3.1415927f;
    /// <summary>
    /// The mathematical constant τ (tau), equal to 2π, representing one full turn in radians.
    /// </summary>
    public const float Tau = 6.2831855f;
    /// <summary>
    /// A small constant value used for floating-point comparisons to account for precision errors.
    /// </summary>
    public const double Epsilon = ShapeMath.EpsilonF;
    /// <summary>
    /// A small constant value used for floating-point comparisons to account for precision errors (float version).
    /// </summary>
    public const float EpsilonF = 1e-10f;
    /// <summary>
    /// Number of nanoseconds in one second.
    /// </summary>
    /// <remarks>
    /// 1 second = 1,000,000,000 nanoseconds.
    /// </remarks>
    public const long  NanoSecondsInOneSecond = 1000L * 1000L * 1000L;
    /// <summary>
    /// Number of nanoseconds in one millisecond.
    /// </summary>
    /// <remarks>
    /// 1 millisecond = 1,000,000 nanoseconds.
    /// </remarks>
    public const long NanoSecondsInOneMilliSecond = 1000L * 1000L;
    /// <summary>
    /// Number of milliseconds in one second.
    /// </summary>
    /// <remarks>
    /// 1 second = 1,000 milliseconds.
    /// </remarks>
    public const long MilliSecondsInOneSecond = 1000L;
    #endregion
    
    #region Time
    
    /// <summary>
    /// Converts nanoseconds to seconds.
    /// </summary>
    /// <param name="nanoseconds">The number of nanoseconds.</param>
    /// <returns>The equivalent time in seconds as a <see cref="double"/>.</returns>
    public static double NanoSecondsToSeconds(long nanoseconds)
    {
        return nanoseconds / (double)NanoSecondsInOneSecond;
    }

    /// <summary>
    /// Converts seconds to nanoseconds.
    /// </summary>
    /// <param name="seconds">The time in seconds.</param>
    /// <returns>The equivalent time in nanoseconds as a <see cref="long"/>.</returns>
    /// <remarks>
    /// Fractional nanoseconds are truncated when casting to <see cref="long"/> and large values may overflow
    /// </remarks>
    public static long SecondsToNanoSeconds(double seconds)
    {
        return (long)(seconds * NanoSecondsInOneSecond);
    }

    /// <summary>
    /// Converts milliseconds to seconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds.</param>
    /// <returns>The equivalent time in seconds as a <see cref="double"/>.</returns>
    public static double MilliSecondsToSeconds(long milliseconds)
    {
        return milliseconds / (double)MilliSecondsInOneSecond;
    }

    /// <summary>
    /// Converts seconds to milliseconds.
    /// </summary>
    /// <param name="seconds">The time in seconds.</param>
    /// <returns>The equivalent time in milliseconds as a <see cref="long"/>.</returns>
    /// <remarks>
    /// Fractional milliseconds are truncated when casting to <see cref="long"/> and large values may overflow.
    /// </remarks>
    public static long SecondsToMilliSeconds(double seconds)
    {
        return (long)(seconds * MilliSecondsInOneSecond);
    }

    /// <summary>
    /// Converts milliseconds to nanoseconds.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds.</param>
    /// <returns>The equivalent time in nanoseconds as a <see cref="long"/>.
    /// This is an exact integer multiplication and may overflow for very large input values.</returns>
    public static long MilliSecondsToNanoSeconds(long milliseconds)
    {
        return milliseconds * NanoSecondsInOneMilliSecond;
    }

    /// <summary>
    /// Converts nanoseconds to milliseconds.
    /// </summary>
    /// <param name="nanoseconds">The number of nanoseconds.</param>
    /// <returns>The equivalent time in milliseconds as a <see cref="long"/> (integer division).</returns>
    public static long NanoSecondsToMilliSeconds(long nanoseconds)
    {
        return nanoseconds / NanoSecondsInOneMilliSecond;
    }
    
    #endregion
    
    #region Round
    /// <summary>
    /// Rounds a floating-point number to a specified number of decimal places.
    /// </summary>
    /// <param name="number">The number to round.</param>
    /// <param name="decimals">The number of decimal places to round to.</param>
    /// <returns>The rounded number.</returns>
    public static float RoundToDecimals(float number, int decimals)
    {
        
        if (decimals <= 0) return MathF.Round(number);
        float value = MathF.Pow(10, decimals);
        return MathF.Round(number * value) / value;
    }
    /// <summary>
    /// Rounds the components of a <see cref="Vector2"/> to a specified number of decimal places.
    /// </summary>
    /// <param name="v">The vector to round.</param>
    /// <param name="decimals">The number of decimal places to round to.</param>
    /// <returns>The rounded vector.</returns>
    public static Vector2 RoundToDecimals(Vector2 v, int decimals)
    {

        if (decimals <= 0) return v.Round();
        float value = MathF.Pow(10, decimals);
        return new
        (
            MathF.Round(v.X * value) / value,
            MathF.Round(v.Y * value) / value
        );
    }
    #endregion
   
    #region Clamp
    /// <summary>
    /// Clamps a value to zero if it is negative or within a small epsilon of zero.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="epsilon">A small threshold for zero comparison.</param>
    /// <returns>The clamped value, never less than zero.</returns>
    public static float ClampToZero(float value, float epsilon = 0.0000001f)
    {
        if (value < 0 || Math.Abs(value) < epsilon)
        {
            return 0;
        }
        return value;
    }
    /// <summary>
    /// Clamps a floating-point value to the range [0, 1].
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp(float value)
    {
        return Clamp(value, 0f, 1f);
    }
    
    /// <summary>
    /// Clamps a floating-point value to a specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }
    /// <summary>
    /// Clamps an integer value to a specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped value.</returns>
    public static int Clamp(int value, int min, int max)
    {
        if (value < min) return min;
        else if (value > max) return max;
        else return value;
    }
    /// <summary>
    /// Clamps a byte value to a specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped value.</returns>
    public static byte Clamp(byte value, byte min, byte max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
    #endregion

    #region Int
    /// <summary>
    /// Returns the greater of two integer values.
    /// </summary>
    /// <param name="value1">The first integer.</param>
    /// <param name="value2">The second integer.</param>
    /// <returns>The larger integer.</returns>
    public static int MaxInt(int value1, int value2)
    {
        if (value1 > value2) return value1;
        else return value2;
    }
    /// <summary>
    /// Returns the lesser of two integer values.
    /// </summary>
    /// <param name="value1">The first integer.</param>
    /// <param name="value2">The second integer.</param>
    /// <returns>The smaller integer.</returns>
    public static int MinInt(int value1, int value2)
    {
        if (value1 < value2) return value1;
        else return value2;
    }
    /// <summary>
    /// Returns the absolute value of an integer.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The absolute value.</returns>
    public static int AbsInt(int value)
    {
        return (int)MathF.Abs(value);
    }
    /// <summary>
    /// Determines whether two integers have the same sign (both positive, both negative, or both zero).
    /// </summary>
    /// <param name="a">The first integer.</param>
    /// <param name="b">The second integer.</param>
    /// <returns><c>true</c> if both have the same sign; otherwise, <c>false</c>.</returns>
    public static bool IsSignEqual(int a, int b) => a < 0 && b < 0 || a > 0 && b > 0 || a == 0 && b == 0;
    #endregion

    #region Float

    /// <summary>
    /// Determines whether two floating-point numbers are approximately equal within a specified tolerance.
    /// </summary>
    /// <param name="a">The first float.</param>
    /// <param name="b">The second float.</param>
    /// <param name="tolerance">The allowed difference for equality.</param>
    /// <returns><c>true</c> if the numbers are approximately equal; otherwise, <c>false</c>.</returns>
    public static bool EqualsF(float a, float b, float tolerance = 0.0001f) => MathF.Abs(a - b) < tolerance;
    /// <summary>
    /// Determines whether two double-precision numbers are approximately equal within a specified tolerance.
    /// </summary>
    /// <param name="a">The first double.</param>
    /// <param name="b">The second double.</param>
    /// <param name="tolerance">The allowed difference for equality.</param>
    /// <returns><c>true</c> if the numbers are approximately equal; otherwise, <c>false</c>.</returns>
    public static bool EqualsD(double a, double b, double tolerance = 0.0000001) => Math.Abs(a - b) < tolerance;
    /// <summary>
    /// Determines whether two floating-point numbers have the same sign (both positive, both negative, or both zero).
    /// </summary>
    /// <param name="a">The first float.</param>
    /// <param name="b">The second float.</param>
    /// <returns><c>true</c> if both have the same sign; otherwise, <c>false</c>.</returns>
    public static bool IsSignEqual(float a, float b) => a < 0 && b < 0 || a > 0 && b > 0 || a == 0 && b == 0;

    #endregion
    
    #region Lerp
    
    /// <summary>
    /// Calculates the normalized interpolation factor (0 to 1) for a value within a range.
    /// </summary>
    /// <param name="cur">The current value.</param>
    /// <param name="min">The minimum of the range.</param>
    /// <param name="max">The maximum of the range.</param>
    /// <returns>The normalized interpolation factor.</returns>
    public static float GetFactor(float cur, float min, float max)
    {
        return (cur - min) / (max - min);
    }

    /// <summary>
    /// Selects an element from a collection based on a normalized interpolation factor.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection of values.</param>
    /// <param name="f">The normalized interpolation factor (0 to 1).</param>
    /// <returns>The selected element.</returns>
    public static T LerpCollection<T>(List<T> collection, float f)
    {
        int index = WrapIndex(collection.Count, (int)(collection.Count * f));
        return collection[index];
    }

    /// <summary>
    /// Performs linear interpolation between two dynamic values (float, int, Vector2, or ColorRgba).
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="f">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated value.</returns>
    public static dynamic LerpDynamic(dynamic from, dynamic to, float f)
    {
        if (from is float) return LerpFloat(from, to, f);
        if (from is int) return LerpInt(from, to, f);
        if (from is Vector2) return ShapeVec.Lerp(from, to, f);
        if (from is ColorRgba color) return color.Lerp(to, f);
        return from;
    }

    #region Float
    /// <summary>
    /// Performs linear interpolation between two floating-point values.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="f">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated value.</returns>
    public static float LerpFloat(float from, float to, float f) => (1.0f - f) * from + to * f;
    /// <summary>
    /// Calculates the normalized interpolation factor for a value between two floats.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>The normalized interpolation factor.</returns>
    public static float LerpInverseFloat(float from, float to, float value)
    {
        return (value - from) / (to - from);
    }
    /// <summary>
    /// Remaps a value from one float range to another using linear interpolation.
    /// </summary>
    /// <param name="value">The value to remap.</param>
    /// <param name="minOld">The minimum of the old range.</param>
    /// <param name="maxOld">The maximum of the old range.</param>
    /// <param name="minNew">The minimum of the new range.</param>
    /// <param name="maxNew">The maximum of the new range.</param>
    /// <returns>The remapped value.</returns>
    public static float RemapFloat(float value, float minOld, float maxOld, float minNew, float maxNew)
    {
        return LerpFloat(minNew, maxNew, LerpInverseFloat(minOld, maxOld, value));
    }
    
    /// <summary>
    /// Performs a framerate-independent interpolation using MathF.Pow. More expensive than exponential decay lerp.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="remainder">Fraction remaining after 1 second.</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated value.</returns>
    public static float PowLerpFloat(float from, float to, float remainder, float dt) => to + (from - to) * MathF.Pow(remainder, dt);
    
    /// <summary>
    /// Performs a framerate-independent exponential decay interpolation (cheaper than PowLerp).
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="decay">Decay rate (recommended between 1 and 25).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated value.</returns>
    public static float ExpDecayLerpFloatComplex(float from, float to, float decay, float dt) => to + (from - to) * MathF.Exp(-decay * dt);

    /// <summary>
    /// Performs a framerate-independent exponential decay interpolation with a normalized fraction.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="f">Normalized fraction (0 to 1).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated value.</returns>
    public static float ExpDecayLerpFloat(float from, float to, float f, float dt)
    {
        var decay = LerpFloat(1, 25, f);
        return ExpDecayLerpFloatComplex(from, to, decay, dt);
    }

    
    #endregion

    #region Int
    /// <summary>
    /// Performs linear interpolation between two integer values.
    /// </summary>
    /// <param name="from">The starting integer.</param>
    /// <param name="to">The target integer.</param>
    /// <param name="f">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated integer value.</returns>
    public static int LerpInt(int from, int to, float f)
    {
        return (int)LerpFloat(from, to, f);
    }
    /// <summary>
    /// Calculates the normalized interpolation factor for a value between two integers.
    /// </summary>
    /// <param name="from">The starting integer.</param>
    /// <param name="to">The target integer.</param>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>The normalized interpolation factor.</returns>
    public static float LerpInverseInt(int from, int to, int value)
    {
        float cur = (value - from);
        float max = (to - from);
        return cur / max;
    }
    /// <summary>
    /// Remaps an integer value from one range to another using linear interpolation.
    /// </summary>
    /// <param name="value">The value to remap.</param>
    /// <param name="minOld">The minimum of the old range.</param>
    /// <param name="maxOld">The maximum of the old range.</param>
    /// <param name="minNew">The minimum of the new range.</param>
    /// <param name="maxNew">The maximum of the new range.</param>
    /// <returns>The remapped integer value.</returns>
    public static int RemapInt(int value, int minOld, int maxOld, int minNew, int maxNew)
    {
        return LerpInt(minNew, maxNew, LerpInverseInt(minOld, maxOld, value));
    }

    /// <summary>
    /// Performs a framerate-independent interpolation between integers using MathF.Pow.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="remainder">Fraction remaining after 1 second.</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated integer value.</returns>
    public static int PowLerpInt(int from, int to, float remainder, float dt) => (int)PowLerpFloat(from, to, remainder, dt);

    /// <summary>
    /// Performs a framerate-independent exponential decay interpolation between integers.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="decay">Decay rate (recommended between 1 and 25).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated integer value.</returns>
    public static int ExpDecayLerpIntComplex(int from, int to, float decay, float dt) => (int)ExpDecayLerpFloatComplex(from, to, decay, dt);

    /// <summary>
    /// Performs a framerate-independent exponential decay interpolation between integers with a normalized fraction.
    /// </summary>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="f">Normalized fraction (0 to 1).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated integer value.</returns>
    public static int ExpDecayLerpInt(int from, int to, float f, float dt) => (int)ExpDecayLerpFloat(from, to, f, dt);

    #endregion

    #region Angle

    /// <summary>
    /// Performs linear interpolation between two angles in radians, taking the shortest path.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <param name="f">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated angle in radians.</returns>
    public static float LerpAngleRad(float from, float to, float f) => from + GetShortestAngleRad(from, to) * f;

    /// <summary>
    /// Performs linear interpolation between two angles in degrees, taking the shortest path.
    /// </summary>
    /// <param name="from">The starting angle in degrees.</param>
    /// <param name="to">The target angle in degrees.</param>
    /// <param name="f">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated angle in degrees.</returns>
    public static float LerpAngleDeg(float from, float to, float f) => from + GetShortestAngleDeg(from, to) * f;
    
    /// <summary>
    /// Performs a framerate-independent interpolation between angles in radians using MathF.Pow.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <param name="remainder">Fraction remaining after 1 second.</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated angle in radians.</returns>
    public static float PowLerpAngle(float from, float to, float remainder, float dt) => from + GetShortestAngleRad(from, to) * MathF.Pow(remainder, dt);

    /// <summary>
    /// Performs a framerate-independent exponential decay interpolation between angles in radians.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <param name="decay">Decay rate (recommended between 1 and 25).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated angle in radians.</returns>
    public static float ExpDecayLerpAngleComplex(float from, float to, float decay, float dt) => from + GetShortestAngleRad(from, to) * MathF.Exp(-decay * dt);

    /// <summary>
    /// Performs a framerate-independent exponential decay interpolation between angles in radians with a normalized fraction.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <param name="f">Normalized fraction (0 to 1).</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The interpolated angle in radians.</returns>
    public static float ExpDecayLerpAngle(float from, float to, float f, float dt)
    {
        var decay = LerpFloat(1, 25, f);
        return ExpDecayLerpAngleComplex(from, to, decay, dt);
    }


    #endregion
    
    #endregion  
    
    #region Wrap
    /// <summary>
    /// Wraps an index to stay within the bounds of a collection of a given size.
    /// </summary>
    /// <param name="count">The size of the collection.</param>
    /// <param name="index">The index to wrap.</param>
    /// <returns>The wrapped index.</returns>
    public static int WrapIndex(int count, int index)
    {
        if (count <= 0) return 0;
        if (index >= count) return index % count;
        else if (index < 0) return (index % count) + count;
        else return index;
    }
    /// <summary>
    /// Wraps a floating-point value to a specified range [min, max).
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <param name="min">The minimum bound (inclusive).</param>
    /// <param name="max">The maximum bound (exclusive).</param>
    /// <returns>The wrapped value.</returns>
    public static float WrapF(float value, float min, float max)
    {
        float range = max - min;
        return range == 0 ? min : value - range * MathF.Floor((value - min) / range);
    }

    /// <summary>
    /// Wraps an integer value to a specified range [min, max).
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <param name="min">The minimum bound (inclusive).</param>
    /// <param name="max">The maximum bound (exclusive).</param>
    /// <returns>The wrapped value.</returns>
    public static int WrapI(int value, int min, int max)
    {
        int range = max - min;
        return range == 0 ? min : value - range * (int)MathF.Floor((value - min) / (float)range);
    }

    /// <summary>
    /// Wraps an angle in radians to the range [0, 2π).
    /// </summary>
    /// <param name="amount">The angle in radians.</param>
    /// <returns>The wrapped angle in radians.</returns>
    public static float WrapAngleRad(float amount)
    {
        return WrapF(amount, 0f, 2.0f * MathF.PI);
    }
    /// <summary>
    /// Wraps an angle in degrees to the range [0, 360).
    /// </summary>
    /// <param name="amount">The angle in degrees.</param>
    /// <returns>The wrapped angle in degrees.</returns>
    public static float WrapAngleDeg(float amount)
    {
        return WrapF(amount, 0f, 360f);
    }
    #endregion

    #region Angle
    
    /// <summary>
    /// Gets the sign (-1, 0, or 1) of the shortest angular distance between two angles in radians.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <returns>The sign of the shortest angular distance.</returns>
    public static int GetShortestAngleRadSign(float from, float to) => MathF.Sign(GetShortestAngleRad(from, to));
    /// <summary>
    /// Gets the sign (-1, 0, or 1) of the shortest angular distance between two angles in degrees.
    /// </summary>
    /// <param name="from">The starting angle in degrees.</param>
    /// <param name="to">The target angle in degrees.</param>
    /// <returns>The sign of the shortest angular distance.</returns>
    public static int GetShortestAngleDegSign(float from, float to) => MathF.Sign(GetShortestAngleDeg(from, to));

    /// <summary>
    /// Calculates the shortest angular distance between two angles in radians, taking wrapping into account.
    /// </summary>
    /// <param name="from">The starting angle in radians.</param>
    /// <param name="to">The target angle in radians.</param>
    /// <returns>The shortest angular distance in radians.</returns>
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
    /// <summary>
    /// Calculates the shortest angular distance between two angles in degrees, taking wrapping into account.
    /// </summary>
    /// <param name="from">The starting angle in degrees.</param>
    /// <param name="to">The target angle in degrees.</param>
    /// <returns>The shortest angular distance in degrees.</returns>
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
    
    /// <summary>
    /// Calculates the angular change needed to aim from a current position to a target position, limited by a rotation speed and delta time.
    /// </summary>
    /// <param name="pos">The current position.</param>
    /// <param name="targetPos">The target position.</param>
    /// <param name="curAngleRad">The current angle in radians.</param>
    /// <param name="rotSpeedRad">The rotation speed in radians per second.</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The change in angle in radians.</returns>
    public static float AimAt(Vector2 pos, Vector2 targetPos, float curAngleRad, float rotSpeedRad, float dt)
    {
        return AimAt(curAngleRad, ShapeVec.AngleRad(targetPos - pos), rotSpeedRad, dt);
    }
    /// <summary>
    /// Calculates the angular change needed to aim from a current angle to a target angle, limited by a rotation speed and delta time.
    /// </summary>
    /// <param name="curAngleRad">The current angle in radians.</param>
    /// <param name="targetAngleRad">The target angle in radians.</param>
    /// <param name="rotSpeedRad">The rotation speed in radians per second.</param>
    /// <param name="dt">Delta time in seconds.</param>
    /// <returns>The change in angle in radians.</returns>
    public static float AimAt(float curAngleRad, float targetAngleRad, float rotSpeedRad, float dt)
    {
        float dif = ShapeMath.GetShortestAngleRad(curAngleRad, targetAngleRad);
        float amount = MathF.Min(rotSpeedRad * dt, MathF.Abs(dif));
        float dir = 1;
        if (dif < 0) dir = -1;
        else if (dir == 0) dir = 0;
        return dir * amount;
    }

    #endregion

    #region Coordinates

    /// <summary>
    /// Converts a linear index to 2D grid coordinates (column, row).
    /// </summary>
    /// <param name="index">The linear index.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="cols">The number of columns in the grid.</param>
    /// <param name="leftToRight">If true, fills left-to-right; otherwise, top-to-bottom.</param>
    /// <returns>A tuple (col, row) representing the coordinates.</returns>
    public static (int col, int row) TransformIndexToCoordinates(int index, int rows, int cols, bool leftToRight = true)
    {
        if (leftToRight)
        {
            int row = index / cols;
            int col = index % cols;
            return (col, row);
        }
        else
        {
            int col = index / rows;
            int row = index % rows;
            return (col, row);
        }
            
    }
    /// <summary>
    /// Converts 2D grid coordinates (row, col) to a linear index.
    /// </summary>
    /// <param name="row">The row coordinate.</param>
    /// <param name="col">The column coordinate.</param>
    /// <param name="rows">The number of rows in the grid.</param>
    /// <param name="cols">The number of columns in the grid.</param>
    /// <param name="leftToRight">If true, fills left-to-right; otherwise, top-to-bottom.</param>
    /// <returns>The linear index.</returns>
    public static int TransformCoordinatesToIndex(int row, int col, int rows, int cols, bool leftToRight = true)
    {
        if (leftToRight)
        {
            return row * cols + col;
        }
        else
        {
            return col * rows + row;
        }
    }


    #endregion
    
    /// <summary>
    /// Determines whether a blinking effect should be active at the current timer value and interval.
    /// </summary>
    /// <param name="timer">The current timer value.</param>
    /// <param name="interval">The interval duration for blinking.</param>
    /// <returns><c>true</c> if the blinking effect is active; otherwise, <c>false</c>.</returns>
    public static bool Blinking(float timer, float interval)
    {
        if (interval <= 0f) return false;
        return (int)(timer / interval) % 2 == 0;
    }

}