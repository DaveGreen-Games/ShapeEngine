using System.Numerics;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Represents a bitwise flag structure for efficient flag manipulation using uint values.
/// </summary>
/// <remarks>
/// Provides bitwise operations and utility methods for managing sets of flags. Supports combining multiple flags and checking for their presence.
/// </remarks>
public readonly struct BitFlag : IBitwiseOperators<BitFlag, BitFlag, BitFlag>, IBitwiseOperators<BitFlag, uint, BitFlag>
{
    /// <summary>
    /// The underlying uint value representing the current set of flags.
    /// </summary>
    public readonly uint Flag;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitFlag"/> struct with a single uint value.
    /// </summary>
    /// <param name="value">The initial flag value.</param>
    public BitFlag(uint value)
    {
        Flag = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitFlag"/> struct with multiple uint values combined.
    /// </summary>
    /// <param name="values">An array of uint values to combine as flags.</param>
    /// <remarks>
    /// Each value in <paramref name="values"/> is bitwise OR'ed into the flag.
    /// </remarks>
    public BitFlag(params uint[] values)
    {
        foreach (var v in values)
        {
            Flag |= v;
        }
    }
    #region Operations Uint
    /// <summary>
    /// Returns an empty <see cref="BitFlag"/> (all bits cleared).
    /// </summary>
    public static BitFlag Clear() => Empty;

    /// <summary>
    /// Determines whether the flag is empty (no bits set).
    /// </summary>
    /// <returns><c>true</c> if the flag is empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty() => Flag == 0;

    /// <summary>
    /// Checks if the specified flag value is present in the current flag set.
    /// </summary>
    /// <param name="value">The flag value to check for.</param>
    /// <returns><c>true</c> if the flag is present; otherwise, <c>false</c>.</returns>
    public bool Has(uint value)
    {
        if (Flag == 0) return false;
        
        return (Flag & value) != 0;
    }

    /// <summary>
    /// Toggles the specified flag value in the current flag set.
    /// </summary>
    /// <param name="value">The flag value to toggle.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag toggled.</returns>
    public BitFlag Set(uint value)
    {
        return new(Flag ^ value);
    }

    /// <summary>
    /// Adds the specified flag value to the current flag set.
    /// </summary>
    /// <param name="value">The flag value to add.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag added.</returns>
    public BitFlag Add(uint value)
    {
        return new(Flag | value);
    }

    /// <summary>
    /// Removes the specified flag value from the current flag set.
    /// </summary>
    /// <param name="value">The flag value to remove.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag removed.</returns>
    public BitFlag Remove(uint value)
    {
        return new(Flag & ~value);
    }

    /// <summary>
    /// Adds multiple flag values to the current flag set.
    /// </summary>
    /// <param name="values">An array of flag values to add.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flags added.</returns>
    public BitFlag Add(params uint[] values)
    {
        var flag = Flag;
        foreach (var v in values)
        {
            flag |= v;
        }

        return new(flag);
    }

    /// <summary>
    /// Removes multiple flag values from the current flag set.
    /// </summary>
    /// <param name="values">An array of flag values to remove.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flags removed.</returns>
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
    /// <summary>
    /// Checks if the specified <see cref="BitFlag"/> value is present in the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitFlag"/> value to check for.</param>
    /// <returns><c>true</c> if the <see cref="BitFlag"/> is present; otherwise, <c>false</c>.</returns>
    public bool Has(BitFlag value)
    {
        if (Flag == 0) return false;
        return (Flag & value.Flag) != 0;
    }

    /// <summary>
    /// Toggles the specified <see cref="BitFlag"/> value in the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitFlag"/> value to toggle.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified <see cref="BitFlag"/> toggled.</returns>
    public BitFlag Set(BitFlag value)
    {
        return new(Flag ^ value.Flag);
    }

    /// <summary>
    /// Adds the specified <see cref="BitFlag"/> value to the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitFlag"/> value to add.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified <see cref="BitFlag"/> added.</returns>
    public BitFlag Add(BitFlag value)
    {
        return new(Flag | value.Flag);
    }

    /// <summary>
    /// Removes the specified <see cref="BitFlag"/> value from the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitFlag"/> value to remove.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified <see cref="BitFlag"/> removed.</returns>
    public BitFlag Remove(BitFlag value)
    {
        return new(Flag & ~value.Flag);
    }

    /// <summary>
    /// Adds multiple <see cref="BitFlag"/> values to the current flag set.
    /// </summary>
    /// <param name="values">An array of <see cref="BitFlag"/> values to add.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified <see cref="BitFlag"/>s added.</returns>
    public BitFlag Add(params BitFlag[] values)
    {
        var flag = Flag;
        foreach (var v in values)
        {
            flag |= v.Flag;
        }

        return new(flag);
    }

    /// <summary>
    /// Removes multiple <see cref="BitFlag"/> values from the current flag set.
    /// </summary>
    /// <param name="values">An array of <see cref="BitFlag"/> values to remove.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified <see cref="BitFlag"/>s removed.</returns>
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

    /// <summary>
    /// Generates a <see cref="BitFlag"/> with a single bit set at the specified power of two.
    /// </summary>
    /// <param name="power">The power of two indicating which bit to set.</param>
    /// <returns>A <see cref="BitFlag"/> with the specified bit set.</returns>
    public static BitFlag Get(uint power) => new((uint)MathF.Pow(2, power));

    /// <summary>
    /// Gets an empty <see cref="BitFlag"/> (all bits cleared).
    /// </summary>
    public static BitFlag Empty => new (0);

    #endregion
    
    #region Static Int

    /// <summary>
    /// Returns an empty integer flag (all bits cleared).
    /// </summary>
    /// <returns>Zero, representing an empty flag.</returns>
    public static int EmptyIntFlag() => 0;

    /// <summary>
    /// Generates an integer flag with a single bit set at the specified power of two.
    /// </summary>
    /// <param name="power">The power of two indicating which bit to set.</param>
    /// <returns>An integer flag with the specified bit set.</returns>
    public static int GetFlagInt(int power) => (int)MathF.Pow(2, power);

    /// <summary>
    /// Checks if the specified integer flag value is present in the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to check for.</param>
    /// <returns><c>true</c> if the flag is present; otherwise, <c>false</c>.</returns>
    public static bool HasIntFlag(int flag, int value)
    {
        if (flag == 0) return false;
        
        return (flag & value) != 0;
    }

    /// <summary>
    /// Toggles the specified integer flag value in the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to toggle.</param>
    /// <returns>The updated flag set with the specified flag toggled.</returns>
    public static int SetIntFlag(int flag, int value)
    {
        return flag ^= value;
    }

    /// <summary>
    /// Adds the specified integer flag value to the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to add.</param>
    /// <returns>The updated flag set with the specified flag added.</returns>
    public static int AddIntFlag(int flag, int value)
    {
        return flag |= value;
    }

    /// <summary>
    /// Removes the specified integer flag value from the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to remove.</param>
    /// <returns>The updated flag set with the specified flag removed.</returns>
    public static int RemoveIntFlag(int flag, int value)
    {
        return flag &= ~value;
    }

    /// <summary>
    /// Adds multiple integer flag values to the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="values">An array of integer flag values to add.</param>
    /// <returns>The updated flag set with the specified flags added.</returns>
    public static int AddIntFlag(int flag, params int[] values)
    {
        foreach (var v in values)
        {
            flag |= v;
        }

        return flag;
    }

    /// <summary>
    /// Removes multiple integer flag values from the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="values">An array of integer flag values to remove.</param>
    /// <returns>The updated flag set with the specified flags removed.</returns>
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
    /// <summary>
    /// Gets the next available flag value as a power of two.
    /// </summary>
    public static uint NextFlag => GetFlagUint(flagCounter++);
    
    
    private static int flagCounterInt = 1;
    /// <summary>
    /// Gets the next available flag value as a power of two (for integer flags).
    /// </summary>
    public static int NextFlagInt => GetFlagInt(flagCounterInt++);
    
    
    /// <summary>
    /// Gets an empty unsigned integer flag (all bits cleared).
    /// </summary>
    public static uint EmptyUintFlag => 0;

    /// <summary>
    /// Generates an unsigned integer flag with a single bit set at the specified power of two.
    /// </summary>
    /// <param name="power">The power of two indicating which bit to set.</param>
    /// <returns>An unsigned integer flag with the specified bit set.</returns>
    public static uint GetFlagUint(uint power) => (uint)MathF.Pow(2, power);

    /// <summary>
    /// Checks if the specified unsigned integer flag value is present in the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to check for.</param>
    /// <returns><c>true</c> if the flag is present; otherwise, <c>false</c>.</returns>
    public static bool HasUintFlag(uint flag, uint value)
    {
        if (flag == 0) return false;
        
        return (flag & value) != 0;
    }

    /// <summary>
    /// Toggles the specified unsigned integer flag value in the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to toggle.</param>
    /// <returns>The updated flag set with the specified flag toggled.</returns>
    public static uint SetUintFlag(uint flag, uint value)
    {
        return flag ^= value;
    }

    /// <summary>
    /// Adds the specified unsigned integer flag value to the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to add.</param>
    /// <returns>The updated flag set with the specified flag added.</returns>
    public static uint AddUintFlag(uint flag, uint value)
    {
        return flag |= value;
    }

    /// <summary>
    /// Removes the specified unsigned integer flag value from the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="value">The flag value to remove.</param>
    /// <returns>The updated flag set with the specified flag removed.</returns>
    public static uint RemoveUintFlag(uint flag, uint value)
    {
        return flag &= ~value;
    }

    /// <summary>
    /// Adds multiple unsigned integer flag values to the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="values">An array of unsigned integer flag values to add.</param>
    /// <returns>The updated flag set with the specified flags added.</returns>
    public static uint AddUintFlag(uint flag, params uint[] values)
    {
        foreach (var v in values)
        {
            flag |= v;
        }

        return flag;
    }

    /// <summary>
    /// Removes multiple unsigned integer flag values from the current flag set.
    /// </summary>
    /// <param name="flag">The current flag set.</param>
    /// <param name="values">An array of unsigned integer flag values to remove.</param>
    /// <returns>The updated flag set with the specified flags removed.</returns>
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
    /// <summary>
    /// Returns the bitwise complement of the specified <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="value">The <see cref="BitFlag"/> to complement.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the complement.</returns>
    public static BitFlag operator ~(BitFlag value) =>new( ~value.Flag);
    
    /// <summary>
    /// Performs a bitwise AND operation between two <see cref="BitFlag"/> values.
    /// </summary>
    /// <param name="left">The left <see cref="BitFlag"/> operand.</param>
    /// <param name="right">The right <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the AND operation.</returns>
    public static BitFlag operator &(BitFlag left, BitFlag right) => new(left.Flag & right.Flag);

    /// <summary>
    /// Performs a bitwise OR operation between two <see cref="BitFlag"/> values.
    /// </summary>
    /// <param name="left">The left <see cref="BitFlag"/> operand.</param>
    /// <param name="right">The right <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the OR operation.</returns>
    public static BitFlag operator |(BitFlag left, BitFlag right) => new(left.Flag | right.Flag);

    /// <summary>
    /// Performs a bitwise XOR operation between two <see cref="BitFlag"/> values.
    /// </summary>
    /// <param name="left">The left <see cref="BitFlag"/> operand.</param>
    /// <param name="right">The right <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the XOR operation.</returns>
    public static BitFlag operator ^(BitFlag left, BitFlag right) => new(left.Flag ^ right.Flag);
    
    /// <summary>
    /// Performs a bitwise AND operation between a <see cref="BitFlag"/> and a uint value.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="right">The uint value operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the AND operation.</returns>
    public static BitFlag operator &(BitFlag left, uint right) => new(left.Flag & right);

    /// <summary>
    /// Performs a bitwise OR operation between a <see cref="BitFlag"/> and a uint value.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="right">The uint value operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the OR operation.</returns>
    public static BitFlag operator |(BitFlag left, uint right) => new(left.Flag | right);

    /// <summary>
    /// Performs a bitwise XOR operation between a <see cref="BitFlag"/> and a uint value.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="right">The uint value operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the XOR operation.</returns>
    public static BitFlag operator ^(BitFlag left, uint right) => new(left.Flag ^ right);
    
    /// <summary>
    /// Performs a bitwise AND operation between a uint value and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="left">The uint value operand.</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the AND operation.</returns>
    public static BitFlag operator &(uint left, BitFlag right) => new(left & right.Flag);

    /// <summary>
    /// Performs a bitwise OR operation between a uint value and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="left">The uint value operand.</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the OR operation.</returns>
    public static BitFlag operator |(uint left, BitFlag right) => new(left | right.Flag);

    /// <summary>
    /// Performs a bitwise XOR operation between a uint value and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="left">The uint value operand.</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the XOR operation.</returns>
    public static BitFlag operator ^(uint left, BitFlag right) => new(left ^ right.Flag);
    #endregion
}