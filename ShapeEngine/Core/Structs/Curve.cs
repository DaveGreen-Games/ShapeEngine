using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;

public abstract class Curve<T>(int capacity) : SortedList<float, T>(capacity)
{
    public bool HasKeys => Count > 0;
    public new bool Add(float time, T key)
    {
        if (time is < 0 or > 1) return false;
        base.Add(time, key);
        return true;
    }
    public int Add(params (float time, T key)[] keys)
    {
        int added = 0;
        foreach (var (t, k) in keys)
        {
            if (Add(t, k)) added++;
        }
        return added;
    }
    
    
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

    public bool Remove(T value)
    {
        var index = IndexOfValue(value);
        if (index == -1) return false;
        RemoveAt(index);
        return true;
    }
    public int Remove(params T[] values)
    {
        int removed = 0;
        foreach (var v in values)
        {
            if(Remove(v)) removed++;
        }
        return removed;
    }

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

    public T Sample(float time)
    {
        Sample(time, out var value);
        return value;
    }
    private float GetSampleTime(int index1, int index2, float time) => ShapeMath.GetFactor(time, Keys[index1], Keys[index2]);
    
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
    
    protected abstract T Interpolate( T a, T b, float time);
    protected abstract T GetDefaultValue();
}


public class CurveInt(int capacity) : Curve<int>(capacity)
{
    protected override int Interpolate(int a, int b, float time)
    {
        return ShapeMath.LerpInt(a, b, time);
    }

    protected override int GetDefaultValue() => 0;
}

public class CurveFloat(int capacity) : Curve<float>(capacity)
{
    protected override float Interpolate(float a, float b, float time)
    {
        return ShapeMath.LerpFloat(a, b, time);
    }

    protected override float GetDefaultValue() => 0f;
}

public class CurveVector2(int capacity) : Curve<Vector2>(capacity)
{
    
    protected override Vector2 Interpolate(Vector2 a, Vector2 b, float time)
    {
        return a.Lerp(b, time);
    }

    protected override Vector2 GetDefaultValue() => Vector2.Zero;
}

public class CurveColor(int capacity) : Curve<ColorRgba>(capacity)
{
    protected override ColorRgba Interpolate(ColorRgba a, ColorRgba b, float time)
    {
        return a.Lerp(b, time);
    }

    protected override ColorRgba GetDefaultValue() => ColorRgba.Clear;
}