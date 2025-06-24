using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;
/// <summary>
/// Abstract base class for a curve of values of type <typeparamref name="T"/>, allowing interpolation and sampling over a normalized time range [0, 1].
/// </summary>
/// <remarks>
/// Inherits from <see cref="SortedList{TKey, TValue}"/> with <c>float</c> keys (time, normalized 0-1) and <typeparamref name="T"/> values.
/// Provides methods for adding, removing, and sampling values along the curve.
/// </remarks>
public abstract class Curve<T>(int capacity) : SortedList<float, T>(capacity)
{
    /// <summary>
    /// Gets whether the curve contains any keys.
    /// </summary>
    public bool HasKeys => Count > 0;

    /// <summary>
    /// Adds a value to the curve at the specified normalized time.
    /// </summary>
    /// <param name="time">The normalized time (0 to 1).</param>
    /// <param name="key">The value to add.</param>
    /// <returns><c>true</c> if the value was added; otherwise, <c>false</c> (if time is out of range).</returns>
    public new bool Add(float time, T key)
    {
        if (time is < 0 or > 1) return false;
        base.Add(time, key);
        return true;
    }

    /// <summary>
    /// Adds multiple values to the curve.
    /// </summary>
    /// <param name="keys">Array of (time, value) pairs to add.</param>
    /// <returns>The number of values successfully added.</returns>
    public int Add(params (float time, T key)[] keys)
    {
        int added = 0;
        foreach (var (t, k) in keys)
        {
            if (Add(t, k)) added++;
        }
        return added;
    }

    /// <summary>
    /// Gets the index of the value at or before the specified time.
    /// </summary>
    /// <param name="time">The normalized time (0 to 1).</param>
    /// <returns>The index of the value, or -1 if the curve is empty.</returns>
    public int GetIndex(float time)
    {
        //Godot´s Curve::get_index function used as reference (https://github.com/godotengine/godot/blob/9ee1873ae1e09c217ac24a5800007f63cb895615/scene/resources/curve.cpp#L121)
        time = ShapeMath.Clamp(time, 0f, 1f);
        if (Count == 0) return -1;
        int imin = 0;
        int imax = Count - 1;

        while (imax - imin > 1) {
            int m = (imin + imax) / 2;

            float a = Keys[m];
            float b = Keys[m + 1];

            if (a < time && b < time) {
                imin = m;
            } else if (a > time) {
                imax = m;
            } else {
                return m;
            }
        }

        // Will happen if the offset is out of bounds.
        if (time > Keys[imax]) {
            return imax;
        }
        return imin;
    }

    /// <summary>
    /// Gets the value at the specified time, or the default value if not found.
    /// </summary>
    /// <param name="time">The normalized time (0 to 1).</param>
    /// <param name="value">The value at the specified time, or the default value if not found.</param>
    /// <returns><c>true</c> if a value was found; otherwise, <c>false</c>.</returns>
    public bool GetValue(float time, out T value)
    {
        var index = GetIndex(time);
        if (index < 0)
        {
            value = GetDefaultValue();
            return false;
        }
        value = Values[GetIndex(time)];
        return true;
    }

    /// <summary>
    /// Removes all values in the specified time range.
    /// </summary>
    /// <param name="timeStart">Start of the time range (inclusive).</param>
    /// <param name="timeEnd">End of the time range (inclusive).</param>
    /// <returns>The number of values removed.</returns>
    public int RemoveTimeRange(float timeStart, float timeEnd)
    {
        if(timeStart >= timeEnd || Math.Abs(timeStart - timeEnd) < 0.00000001f) return 0;
        if(timeStart <= 0f && timeEnd >= 1f)
        {
            var count = Count;
            Clear();
            return count;
        }
        if(timeStart <= 0f) timeStart = 0f;
        if(timeEnd >= 1f) timeEnd = 1f;

        int removed = 0;
        for (int i = Count - 1; i >= 0; i--)
        {
            var key = Keys[i];
            if (key >= timeStart && key <= timeEnd)
            {
                RemoveAt(i);
                removed++;
            }
        }
        return removed;
    }

    /// <summary>
    /// Removes values at or near the specified time, within a given tolerance.
    /// </summary>
    /// <param name="time">The normalized time (0 to 1).</param>
    /// <param name="tolerance">The tolerance range for removal.</param>
    /// <returns>The number of values removed.</returns>
    public int Remove(float time, float tolerance)
    {
        if(time <= 0f || time > 1f) return 0;
        if (tolerance >= 1f)
        {
            var count = Count;
            Clear();
            return count;
        }
        if (tolerance <= 0f)
        {
            if(Remove(time)) return 1;
            return 0;
        }
        
        return RemoveTimeRange(time - tolerance, time + tolerance);
    }

    /// <summary>
    /// Removes the first occurrence of the specified value from the curve.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns><c>true</c> if the value was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(T value)
    {
        var index = IndexOfValue(value);
        if (index == -1) return false;
        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes all specified values from the curve.
    /// </summary>
    /// <param name="values">The values to remove.</param>
    /// <returns>The number of values removed.</returns>
    public int Remove(params T[] values)
    {
        int removed = 0;
        foreach (var v in values)
        {
            if(Remove(v)) removed++;
        }
        return removed;
    }

    /// <summary>
    /// Samples the curve at the specified time, interpolating between values as needed.
    /// </summary>
    /// <param name="time">The normalized time (0 to 1).</param>
    /// <param name="value">The sampled value at the specified time.</param>
    /// <returns><c>true</c> if a value was sampled; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>If the curve is empty, returns the default value.</description></item>
    /// <item><description>If only one value exists, returns that value.</description></item>
    /// <item><description>If the time is before the first or after the last key, returns the boundary value.</description></item>
    /// </list>
    /// </remarks>
    public bool Sample(float time, out T value)
    {
        value = GetDefaultValue();
        if (Count == 0) return false;
        if (Count == 1)
        {
            value = Values[0];
            return true;
        }

        if (time == 0f)
        {
            value = Values[0];
            return true;
        }
        
        if (Math.Abs(time - 1f) < 0.000000001f) //time == 1f
        {
            value = Values[Count - 1];
            return true;
        }
        
        var index1 = GetIndex(time);
        if (index1 < 0) return false;
        
        //sample index is the last one, therefore we can not interpolate and just return the last value
        if (index1 == Count - 1) 
        {
            value = Values[Count - 1];
            return true;
        }
        
        //sample index is the first one and the sample time is before the first key, therefore we can not interpolate and just return the first value
        if (index1 == 0 && time < Keys[0]) 
        {
            value = Values[0];
            return true;
        }
        
        
        var v1 = Values[index1];
        var index2 = index1 + 1;
        var v2 = Values[index2];
        value = Interpolate(v1, v2, GetSampleTime(index1, index2, time));
        // Console.WriteLine($"I1: {index1}, I2: {index2}, k1: {Keys[index1]}, k2: {Keys[index2]}, v1: {v1}, v2: {v2}, t: {time}, st: {time - Keys[index1]}, result: {value}");
        
        return true;
    }

    /// <summary>
    /// Samples the curve at the specified time.
    /// </summary>
    /// <param name="time">The normalized time (0 to 1).</param>
    /// <returns>The sampled value at the specified time.</returns>
    public T Sample(float time)
    {
        Sample(time, out var value);
        return value;
    }

    /// <summary>
    /// Samples the curve at multiple times.
    /// </summary>
    /// <param name="times">Array of normalized times to sample.</param>
    /// <returns>A list of sampled values, or <c>null</c> if no times are provided.</returns>
    public List<T>? SampleMany(params float[] times)
    {
        if(times.Length == 0) return null;
        var result = new List<T>(times.Length);

        foreach (var time in times)
        {
            Sample(time, out var value);
            result.Add(value);
        }
        
        return result;
    }

    /// <summary>
    /// Interpolates between two values of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="time">The interpolation factor between 0 and 1.</param>
    /// <returns>The interpolated value.</returns>
    protected abstract T Interpolate( T a, T b, float time);

    /// <summary>
    /// Gets the default value for the curve.
    /// </summary>
    /// <returns>The default value of type <typeparamref name="T"/>.</returns>
    protected abstract T GetDefaultValue();
    
    private float GetSampleTime(int index1, int index2, float time) => ShapeMath.GetFactor(time, Keys[index1], Keys[index2]);
}