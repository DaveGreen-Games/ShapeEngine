namespace ShapeEngine.Lib;

public static class ShapeFlag
{
    // private int flagValue = 0;

    // public BitFlag() { }
    // public BitFlag(int startValue)
    // {
    //     flagValue = startValue;
    // }
    // public BitFlag(params int[] startValues)
    // {
    //     foreach (var v in startValues)
    //     {
    //         flagValue |= v;
    //     }
    // }
    
    public static int Clear()
    {
        return 0;
    }
    public static bool Has(this int flag, int value)
    {
        if (flag == 0) return true;//??
        
        return (flag & value) != 0;
    }
    public static int Set(this int flag, int value)
    {
        return flag ^= value;
    }
    public static int Add(this int flag, int value)
    {
        return flag |= value;
    }
    public static int Remove(this int flag, int value)
    {
        return flag &= ~value;
    }
    public static int Add(this int flag, params int[] values)
    {
        foreach (var v in values)
        {
            flag |= v;
        }

        return flag;
    }
    public static int Remove(this int flag, params int[] values)
    {
        foreach (var v in values)
        {
            flag &= ~v;
        }

        return flag;
    }
    
}