using System.Numerics;

namespace ShapeEngine.Core.Structs;

public readonly struct BitFlag : IBitwiseOperators<BitFlag, BitFlag, BitFlag>, IBitwiseOperators<BitFlag, uint, BitFlag>
{
    public readonly uint Flag;
    public BitFlag(uint value)
    {
        Flag = value;
    }
    public BitFlag(params uint[] values)
    {
        foreach (var v in values)
        {
            Flag |= v;
        }
    }
    // public BitFlag(params int[] values)
    // {
    //     foreach (var v in values)
    //     {
    //         Flag |= (uint)v;
    //     }
    // }
    #region Operations Uint
    public static BitFlag Clear() => Empty;
    public bool IsEmpty() => Flag == 0;
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
    #endregion
    
    #region Operation BitFlag
    public bool Has(BitFlag value)
    {
        if (Flag == 0) return false;
        return (Flag & value.Flag) != 0;
    }
    public BitFlag Set(BitFlag value)
    {
        return new(Flag ^ value.Flag);
    }
    public BitFlag Add(BitFlag value)
    {
        return new(Flag | value.Flag);
    }
    public BitFlag Remove(BitFlag value)
    {
        return new(Flag & ~value.Flag);
    }
    public BitFlag Add(params BitFlag[] values)
    {
        var flag = Flag;
        foreach (var v in values)
        {
            flag |= v.Flag;
        }

        return new(flag);
    }
    public BitFlag Remove(params BitFlag[] values)
    {
        var flag = Flag;
        foreach (var v in values)
        {
            flag &= ~v.Flag;
        }

        return new(flag);
    }
    #endregion
    
    #region Static Generate

    public static BitFlag Get(uint power) => new((uint)MathF.Pow(2, power));
    public static BitFlag Empty => new (0);

    #endregion
    
    #region Static Int

    public static int EmptyIntFlag() => 0;
    public static int GetFlagInt(int power) => (int)MathF.Pow(2, power);
    public static bool HasIntFlag(int flag, int value)
    {
        if (flag == 0) return false;
        
        return (flag & value) != 0;
    }
    public static int SetIntFlag(int flag, int value)
    {
        return flag ^= value;
    }
    public static int AddIntFlag(int flag, int value)
    {
        return flag |= value;
    }
    public static int RemoveIntFlag(int flag, int value)
    {
        return flag &= ~value;
    }
    public static int AddIntFlag(int flag, params int[] values)
    {
        foreach (var v in values)
        {
            flag |= v;
        }

        return flag;
    }
    public static int RemoveIntFlag(int flag, params int[] values)
    {
        foreach (var v in values)
        {
            flag &= ~v;
        }

        return flag;
    }
    #endregion
    
    #region Static Uint

    private static uint flagCounter = 1;
    public static uint NextFlag => GetFlagUint(flagCounter++);
    
    
    private static int flagCounterInt = 1;
    public static int NextFlagInt => GetFlagInt(flagCounterInt++);
    
    
    public static uint EmptyUintFlag => 0;
    public static uint GetFlagUint(uint power) => (uint)MathF.Pow(2, power);
    public static bool HasUintFlag(uint flag, uint value)
    {
        if (flag == 0) return false;
        
        return (flag & value) != 0;
    }
    public static uint SetUintFlag(uint flag, uint value)
    {
        return flag ^= value;
    }
    public static uint AddUintFlag(uint flag, uint value)
    {
        return flag |= value;
    }
    public static uint RemoveUintFlag(uint flag, uint value)
    {
        return flag &= ~value;
    }
    public static uint AddUintFlag(uint flag, params uint[] values)
    {
        foreach (var v in values)
        {
            flag |= v;
        }

        return flag;
    }
    public static uint RemoveUintFlag(uint flag, params uint[] values)
    {
        foreach (var v in values)
        {
            flag &= ~v;
        }

        return flag;
    }
    #endregion
    
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


// public static class ShapeFlag
// {
//     #region Int
//
//     public static int GetFlagInt(int power) => (int)MathF.Pow(2, power);
//     public static int ClearIntFlag()
//     {
//         return 0;
//     }
//     public static bool HasIntFlag(this int flag, int value)
//     {
//         if (flag == 0) return false;
//         
//         return (flag & value) != 0;
//     }
//     public static int SetIntFlag(this int flag, int value)
//     {
//         return flag ^= value;
//     }
//     public static int AddIntFlag(this int flag, int value)
//     {
//         return flag |= value;
//     }
//     public static int RemoveIntFlag(this int flag, int value)
//     {
//         return flag &= ~value;
//     }
//     public static int AddIntFlag(this int flag, params int[] values)
//     {
//         foreach (var v in values)
//         {
//             flag |= v;
//         }
//
//         return flag;
//     }
//     public static int RemoveIntFlag(this int flag, params int[] values)
//     {
//         foreach (var v in values)
//         {
//             flag &= ~v;
//         }
//
//         return flag;
//     }
//     #endregion
//     
//     #region Uint
//     public static uint GetFlagUint(uint power) => (uint)MathF.Pow(2, power);
//     public static uint ClearUintFlag()
//     {
//         return 0;
//     }
//     public static bool HasUintFlag(this uint flag, uint value)
//     {
//         if (flag == 0) return false;
//         
//         return (flag & value) != 0;
//     }
//     public static uint SetUintFlag(this uint flag, uint value)
//     {
//         return flag ^= value;
//     }
//     public static uint AddUintFlag(this uint flag, uint value)
//     {
//         return flag |= value;
//     }
//     public static uint RemoveUintFlag(this uint flag, uint value)
//     {
//         return flag &= ~value;
//     }
//     public static uint AddUintFlag(this uint flag, params uint[] values)
//     {
//         foreach (var v in values)
//         {
//             flag |= v;
//         }
//
//         return flag;
//     }
//     public static uint RemoveUintFlag(this uint flag, params uint[] values)
//     {
//         foreach (var v in values)
//         {
//             flag &= ~v;
//         }
//
//         return flag;
//     }
//     #endregion
//     
// }