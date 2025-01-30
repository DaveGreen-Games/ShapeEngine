using System.Net.Sockets;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Structs;

public abstract class Curve<T>(int capacity) : SortedList<float, T>(capacity)
{
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
        if (Count == 0) return -1; // Handle empty list
        
        if (time <= 0f) return 0; // Return the first key if time is smaller or equal to 0
        if (time >= 1f) return Count - 1; // Return the last key if time is greater or equal to 1
        
        if (Count == 1)
        {
            return 0;
        }

        if (Count == 2)
        {
            if (Math.Abs(time - Keys[0]) < Math.Abs(time - Keys[1]))
            {
                return 0;
            }
            return 1;
            
        }

        int left = 0;
        int right = Count - 1;

        if (time <= Keys[left])
        {
            return left;
        }
        if (time >= Keys[right])
        {
            return right;
        }
        
        while (left <= right)
        {
            int mid = (left + right) / 2;

            if (Math.Abs(Keys[mid] - time) < 0.000000001f)
            {
                return mid;
            }
            
            if (time < Keys[mid])
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        // After exiting the loop, left and right are the indices of the two closest elements
        int closestIndex = (Math.Abs(Keys[left] - time) < Math.Abs(Keys[right] - time)) ? left : right;
        return closestIndex;
        
        // while (left <= right)
        // {
        //     int mid = left + (right - left) / 2;
        //
        //     if (Math.Abs(Keys[mid] - time) < 0.00000001f)
        //     {
        //         return mid; // Exact match found
        //     }
        //     
        //     if (Keys[mid] < time)
        //     {
        //         left = mid + 1;
        //     }
        //     else
        //     {
        //         right = mid - 1;
        //     }
        // }
        //
        // // If no exact match found, return the closest key
        // if (Math.Abs(time - Keys[left]) < Math.Abs(time - Keys[right]))
        // {
        //     return left;
        // }
        // return right;
        
    }
    public bool GetValue(float time, out T value)
    {
        value = GetDefaultValue();
        if (Count == 0) return false; // Handle empty list

        if (time <= 0f)
        {
            value = Values[0];
            return true;
        }

        if (time >= 1f)
        {
            value = Values[Count - 1];
            return true;
        }
        
        if (Count == 1)
        {
            value = Values[0];
            return false;
        }

        if (Count == 2)
        {
            if (Math.Abs(time - Keys[0]) < Math.Abs(time - Keys[1]))
            {
                value = Values[0];
                return true;
            }
            
            value = Values[1];
            return true;
            
        }

        int left = 0;
        int right = Count - 1;

        if (time <= Keys[left])
        {
            value = Values[left];
            return true;
        }
        if (time >= Keys[right])
        {
            value = Values[right];
            return true;
        }
        
        while (left <= right)
        {
            int mid = (left + right) / 2;

            if (Math.Abs(Keys[mid] - time) < 0.000000001f)
            {
                value = Values[mid];
                return true;
            }
            
            if (time < Keys[mid])
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }
        
        int closestIndex = (Math.Abs(Keys[left] - time) < Math.Abs(Keys[right] - time)) ? left : right;
        value = Values[closestIndex];
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

        if (Count == 2)
        {
            value = Interpolate(Values[0], Values[1], time);
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
        var v1 = Values[index1];
        if (index1 == Count - 1)
        {
            value = Interpolate(v1, GetDefaultValue(), time);
            return true;
        }
        
        var index2 = index1 + 1;
        var v2 = Values[index2];
        value = Interpolate(v1, v2, time);
        
        return true;
    }
    
    public List<T>? Sample(params float[] times)
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