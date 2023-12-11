namespace ShapeEngine.Core;

public class BitFlag
{
    private int flagValue = 0;

    public BitFlag() { }
    public BitFlag(int startValue)
    {
        flagValue = startValue;
    }
    public BitFlag(params int[] startValues)
    {
        foreach (var v in startValues)
        {
            flagValue |= v;
        }
    }
    
    public void Clear()
    {
        flagValue = 0;
    }
    public bool Has(int flag)
    {
        if (flag == 0) return true;//??
        
        return (flagValue & flag) != 0;
    }
    public void Toggle(int flag)
    {
        flagValue ^= flag;
    }
    public void Set(int flag)
    {
        flagValue |= flag;
    }
    public void Unset(int flag)
    {
        flagValue &= ~flag;
    }
    public void Set(params int[] flags)
    {
        foreach (var flag in flags)
        {
            flagValue |= flag;
        }
    }
    public void Unset(params int[] flags)
    {
        foreach (var flag in flags)
        {
            flagValue &= ~flag;
        }
    }
    
}