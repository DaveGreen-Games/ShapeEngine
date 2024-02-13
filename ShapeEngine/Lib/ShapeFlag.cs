using System.Numerics;

namespace ShapeEngine.Lib;

public readonly struct BitFlag : IBitwiseOperators<BitFlag, BitFlag, BitFlag>, IBitwiseOperators<BitFlag, uint, BitFlag>
{
    public readonly uint Flag;

    public BitFlag(uint value)
    {
        Flag = value;
    }
    public BitFlag Clear() => new(0);
    public bool Has(uint value)
    {
        if (Flag == 0) return false;
        
        return (Flag & value) != 0;
    }
    public BitFlag Set(uint value)
    {
        return new(Flag ^ value);
    }
    public BitFlag Add(uint value)
    {
        return new(Flag | value);
    }
    public BitFlag Remove(uint value)
    {
        return new(Flag & ~value);
    }
    public BitFlag Add(params uint[] values)
    {
        var flag = Flag;
        foreach (var v in values)
        {
            flag |= v;
        }

        return new(flag);
    }
    public BitFlag Remove(params uint[] values)
    {
        var flag = Flag;
        foreach (var v in values)
        {
            flag &= ~v;
        }

        return new(flag);
    }

    
    #region Operators
    public static BitFlag operator ~(BitFlag value) =>new( ~value.Flag);
    
    public static BitFlag operator &(BitFlag left, BitFlag right) => new(left.Flag & right.Flag);
    public static BitFlag operator |(BitFlag left, BitFlag right) => new(left.Flag | right.Flag);
    public static BitFlag operator ^(BitFlag left, BitFlag right) => new(left.Flag ^ right.Flag);
    
    public static BitFlag operator &(BitFlag left, uint right) => new(left.Flag & right);
    public static BitFlag operator |(BitFlag left, uint right) => new(left.Flag | right);
    public static BitFlag operator ^(BitFlag left, uint right) => new(left.Flag ^ right);
    
    public static BitFlag operator &(uint left, BitFlag right) => new(left & right.Flag);
    public static BitFlag operator |(uint left, BitFlag right) => new(left | right.Flag);
    public static BitFlag operator ^(uint left, BitFlag right) => new(left ^ right.Flag);
    #endregion
}

public static class ShapeFlag
{
    #region Int
    public static int ClearIntFlag()
    {
        return 0;
    }
    public static bool HasIntFlag(this int flag, int value)
    {
        if (flag == 0) return true;//??
        
        return (flag & value) != 0;
    }
    public static int SetIntFlag(this int flag, int value)
    {
        return flag ^= value;
    }
    public static int AddIntFlag(this int flag, int value)
    {
        return flag |= value;
    }
    public static int RemoveIntFlag(this int flag, int value)
    {
        return flag &= ~value;
    }
    public static int AddIntFlag(this int flag, params int[] values)
    {
        foreach (var v in values)
        {
            flag |= v;
        }

        return flag;
    }
    public static int RemoveIntFlag(this int flag, params int[] values)
    {
        foreach (var v in values)
        {
            flag &= ~v;
        }

        return flag;
    }
    #endregion
    
    #region Uint
    public static uint ClearUintFlag()
    {
        return 0;
    }
    public static bool HasUintFlag(this uint flag, uint value)
    {
        if (flag == 0) return true;//??
        
        return (flag & value) != 0;
    }
    public static uint SetUintFlag(this uint flag, uint value)
    {
        return flag ^= value;
    }
    public static uint AddUintFlag(this uint flag, uint value)
    {
        return flag |= value;
    }
    public static uint RemoveUintFlag(this uint flag, uint value)
    {
        return flag &= ~value;
    }
    public static uint AddUintFlag(this uint flag, params uint[] values)
    {
        foreach (var v in values)
        {
            flag |= v;
        }

        return flag;
    }
    public static uint RemoveUintFlag(this uint flag, params uint[] values)
    {
        foreach (var v in values)
        {
            flag &= ~v;
        }

        return flag;
    }
    #endregion
}