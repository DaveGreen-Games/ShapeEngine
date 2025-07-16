using System.Numerics;

namespace ShapeEngine.Core.Structs;


/// <summary>
/// Represents a bitwise flag structure for efficient flag manipulation using uint values.
/// </summary>
/// <remarks>
/// Provides bitwise operations and utility methods for managing sets of flags. Supports combining multiple flags and checking for their presence.
/// </remarks>
public readonly struct BitFlag : IBitwiseOperators<BitFlag, BitFlag.BitValue, BitFlag>, IBitwiseOperators<BitFlag, uint, BitFlag>
{
    
    /// <summary>
    /// Represents a single bit value as a power of two for use in bitwise flag operations.
    /// </summary>
    public readonly struct BitValue
    {
        /// <summary>
        /// The underlying uint value representing the bit (power of two).
        /// </summary>
        public readonly uint Value;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="BitValue"/> struct with the specified power of two.
        /// </summary>
        /// <param name="power">The power of two indicating which bit to set (0 for least significant bit).</param>
        public BitValue(int power)
        {
            if(power < 0) power *= -1; // Ensure power is non-negative
            Value = 1U << power;
        }

        /// <summary>
        /// Returns the uint value representing 2 raised to the specified power.
        /// Ensures the power is non-negative.
        /// </summary>
        /// <param name="power">The exponent for the power of two (negative values are converted to positive).</param>
        /// <returns>The ulong value of 2^power.</returns>
        public static uint GetPowerOfTwoValue(int power)
        {
            if(power < 0) power *= -1; // Ensure power is non-negative
            return 1U << power;
        }
    }
    
    
    /// <summary>
    /// Gets an empty <see cref="BitFlag"/> (all bits cleared).
    /// </summary>
    public static BitFlag Empty => new (0);
    
    /// <summary>
    /// The underlying uint value representing the current set of flags.
    /// </summary>
    public readonly uint FlagValue;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BitFlag"/> struct with a single uint value.
    /// </summary>
    /// <param name="flagValue">The initial flag value.</param>
    public BitFlag(uint flagValue)
    {
        FlagValue = flagValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BitFlag"/> struct using a <see cref="BitValue"/>.
    /// </summary>
    /// <param name="bitValue">The <see cref="BitValue"/> to set as the flag.</param>
    public BitFlag(BitValue bitValue)
    {
        FlagValue = bitValue.Value;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BitFlag"/> struct with multiple uint values combined.
    /// </summary>
    /// <param name="bitValues">An array of uint values to combine as flags.</param>
    /// <remarks>
    /// Each value in <paramref name="bitValues"/> is bitwise OR'ed into the flag.
    /// </remarks>
    public BitFlag(params BitValue[] bitValues)
    {
        foreach (var v in bitValues)
        {
            FlagValue |= v.Value;
        }
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BitFlag"/> struct with multiple uint values combined.
    /// </summary>
    /// <param name="bitValues">An array of uint values to combine as flags. Only values that are powers of two will be included.</param>
    public BitFlag(params uint[] bitValues)
    {
        foreach (var v in bitValues)
        {
            if(!IsPowerOfTwo(v)) continue;
            FlagValue |= v;
        }
    }
   
    /// <summary>
    /// Explicitly converts a <see cref="BitFlag"/> to a <see cref="BitFlagLong"/>.
    /// </summary>
    /// <param name="flag">The <see cref="BitFlag"/> to convert.</param>
    /// <returns>A <see cref="BitFlagLong"/> with the same bits set.</returns>
    public static explicit operator BitFlagLong(BitFlag flag) => new(flag.FlagValue);
    
    #region Public Functions
    
    /// <summary>
    /// Determines whether the flag is empty (no bits set).
    /// </summary>
    /// <returns><c>true</c> if the flag is empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty() => FlagValue == 0;

    /// <summary>
    /// Checks if the specified flag value is present in the current flag set.
    /// </summary>
    /// <param name="bitValue">The flag value to check for. Has to be a power of two value!</param>
    /// <returns><c>true</c> if the flag is present and a power of two; otherwise, <c>false</c>.</returns>
    public bool Has(uint bitValue)
    {
        if (FlagValue == 0) return false;
        
        if (!IsPowerOfTwo(bitValue)) return false;
        
        return (FlagValue & bitValue) != 0;
    }

    /// <summary>
    /// Toggles the specified flag value in the current flag set.
    /// </summary>
    /// <param name="bitValue">The flag value to toggle. Has to be a power of two value, otherwise a copy if this <see cref="BitFlag"/> will be returned!</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag toggled.</returns>
    public BitFlag Set(uint bitValue)
    {
        return !IsPowerOfTwo(bitValue) ? this : new(FlagValue ^ bitValue);
    }

    /// <summary>
    /// Adds the specified flag value to the current flag set.
    /// </summary>
    /// <param name="bitValue">The flag value to add. Has to be a power of two value, otherwise a copy if this <see cref="BitFlag"/> will be returned!</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag added.</returns>
    public BitFlag Add(uint bitValue)
    {
        return !IsPowerOfTwo(bitValue) ? this : new(FlagValue | bitValue);
    }

    /// <summary>
    /// Removes the specified flag value from the current flag set.
    /// </summary>
    /// <param name="bitValue">The flag value to remove. Has to be a power of two value, otherwise a copy if this <see cref="BitFlag"/> will be returned!</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag removed.</returns>
    public BitFlag Remove(uint bitValue)
    {
        return !IsPowerOfTwo(bitValue) ? this : new(FlagValue & ~bitValue);
    }

    /// <summary>
    /// Adds multiple flag values to the current flag set.
    /// </summary>
    /// <param name="bitValues">An array of flag values to add. Have to be a power of two values! Any value that is not a power of two will be skipped!</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flags added.</returns>
    public BitFlag Add(params uint[] bitValues)
    {
        var flag = FlagValue;
        foreach (var v in bitValues)
        {
            if (!IsPowerOfTwo(v)) continue; // Skip values that are not powers of two
            flag |= v;
        }

        return new(flag);
    }

    /// <summary>
    /// Removes multiple flag values from the current flag set.
    /// </summary>
    /// <param name="bitValues">An array of flag values to remove. Have to be a power of two values! Any value that is not a power of two will be skipped!</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flags removed.</returns>
    public BitFlag Remove(params uint[] bitValues)
    {
        var flag = FlagValue;
        foreach (var v in bitValues)
        {
            if (!IsPowerOfTwo(v)) continue; // Skip values that are not powers of two
            flag &= ~v;
        }

        return new(flag);
    }
    
    #endregion
    
    #region Operation

    /// <summary>
    /// Checks if the specified <see cref="BitValue"/> is present in the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitValue"/> to check for.</param>
    /// <returns><c>true</c> if the flag is present; otherwise, <c>false</c>.</returns>
    public bool Has(BitValue value)
    {
        if (FlagValue == 0) return false;
        return (FlagValue & value.Value) != 0;
    }
    
    /// <summary>
    /// Toggles the specified <see cref="BitValue"/> in the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitValue"/> to toggle.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag toggled.</returns>
    public BitFlag Set(BitValue value)
    {
        return new(FlagValue ^ value.Value);
    }
    
    /// <summary>
    /// Adds the specified <see cref="BitValue"/> to the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitValue"/> to add.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag added.</returns>
    public BitFlag Add(BitValue value)
    {
        return new(FlagValue | value.Value);
    }
    
    /// <summary>
    /// Removes the specified <see cref="BitValue"/> from the current flag set.
    /// </summary>
    /// <param name="value">The <see cref="BitValue"/> to remove.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flag removed.</returns>
    public BitFlag Remove(BitValue value)
    {
        return new(FlagValue & ~value.Value);
    }
    
    /// <summary>
    /// Adds multiple <see cref="BitValue"/> flags to the current flag set.
    /// </summary>
    /// <param name="values">An array of <see cref="BitValue"/> flags to add.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flags added.</returns>
    public BitFlag Add(params BitValue[] values)
    {
        var flag = FlagValue;
        foreach (var v in values)
        {
            flag |= v.Value;
        }
    
        return new(flag);
    }
    
    /// <summary>
    /// Removes multiple <see cref="BitValue"/> flags from the current flag set.
    /// </summary>
    /// <param name="values">An array of <see cref="BitValue"/> flags to remove.</param>
    /// <returns>A new <see cref="BitFlag"/> instance with the specified flags removed.</returns>
    public BitFlag Remove(params BitValue[] values)
    {
        var flag = FlagValue;
        foreach (var v in values)
        {
            flag &= ~v.Value;
        }
    
        return new(flag);
    }
    #endregion
    
    #region Static

    /// <summary>
    /// Determines whether the specified value is a power of two.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><c>true</c> if the value is a power of two; otherwise, <c>false</c>.</returns>
    public static bool IsPowerOfTwo(uint value)
    {
        if (value == 0 || (value & (value - 1)) != 0) return false;
        return true;
    }
    
    private static int flagCounter = 0;
    /// <summary>
    /// Gets the next available uint value as a power of two (for unsigned integer flags).
    /// </summary>
    public static uint NextPowerOfTwo => GetPowerOfTwo(flagCounter++);

    /// <summary>
    /// Gets an empty unsigned integer flag (all bits cleared).
    /// </summary>
    public static uint EmptyFlag => 0;
    
    /// <summary>
    /// Generates an unsigned integer value with a single bit set at the specified power of two.
    /// </summary>
    /// <param name="power">The power of two indicating which bit to set.</param>
    /// <returns>An unsigned integer with the specified bit set.</returns>
    public static uint GetPowerOfTwo(int power) => BitValue.GetPowerOfTwoValue(power);

    /// <summary>
    /// Checks if the specified unsigned integer flag value is present in the current flag set.
    /// </summary>
    /// <param name="flagValue">The current flag set.</param>
    /// <param name="bitValue">The flag value to check for. Has to be a power of two!</param>
    /// <returns><c>true</c> if the flag is present; otherwise, <c>false</c>.</returns>
    public static bool HasFlag(uint flagValue, uint bitValue)
    {
        if (flagValue == 0) return false;
        
        return (flagValue & bitValue) != 0;
    }

    /// <summary>
    /// Toggles the specified unsigned integer flag value in the current flag set.
    /// </summary>
    /// <param name="flagValue">The current flag set.</param>
    /// <param name="bitValue">The flag value to toggle. Has to be a power of two!</param>
    /// <returns>The updated flag set with the specified flag toggled.</returns>
    public static uint SetFlag(uint flagValue, uint bitValue)
    {
        return flagValue ^ bitValue;
    }

    /// <summary>
    /// Adds the specified unsigned integer flag value to the current flag set.
    /// </summary>
    /// <param name="flagValue">The current flag set.</param>
    /// <param name="bitValue">The flag value to add. Has to be a power of two!</param>
    /// <returns>The updated flag set with the specified flag added.</returns>
    public static uint AddFlag(uint flagValue, uint bitValue)
    {
        return flagValue | bitValue;
    }

    /// <summary>
    /// Removes the specified unsigned integer flag value from the current flag set.
    /// </summary>
    /// <param name="flagValue">The current flag set.</param>
    /// <param name="bitValue">The flag value to remove. Has to be a power of two!</param>
    /// <returns>The updated flag set with the specified flag removed.</returns>
    public static uint RemoveFlag(uint flagValue, uint bitValue)
    {
        return flagValue & ~bitValue;
    }

    /// <summary>
    /// Adds multiple unsigned integer flag values to the current flag set.
    /// </summary>
    /// <param name="flagValue">The current flag set.</param>
    /// <param name="bitValues">An array of unsigned integer flag values to add. Have to be a power of two values!</param>
    /// <returns>The updated flag set with the specified flags added.</returns>
    public static uint AddFlag(uint flagValue, params uint[] bitValues)
    {
        foreach (var v in bitValues)
        {
            flagValue |= v;
        }

        return flagValue;
    }

    /// <summary>
    /// Removes multiple unsigned integer flag values from the current flag set.
    /// </summary>
    /// <param name="flagValue">The current flag set.</param>
    /// <param name="bitValues">An array of unsigned integer flag values to remove. Have to be a power of two values!</param>
    /// <returns>The updated flag set with the specified flags removed.</returns>
    public static uint RemoveFlag(uint flagValue, params uint[] bitValues)
    {
        foreach (var v in bitValues)
        {
            flagValue &= ~v;
        }

        return flagValue;
    }
    #endregion
    
    #region Operators
    /// <summary>
    /// Returns the bitwise complement of the specified <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="value">The <see cref="BitFlag"/> to complement.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the complement.</returns>
    public static BitFlag operator ~(BitFlag value) =>new( ~value.FlagValue);
    
    /// <summary>
    /// Performs a bitwise AND operation between a <see cref="BitFlag"/> and a <see cref="BitValue"/>.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="bitValue">The <see cref="BitValue"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the AND operation.</returns>
    public static BitFlag operator &(BitFlag left, BitValue bitValue) => new(left.FlagValue & bitValue.Value);
    
    /// <summary>
    /// Performs a bitwise OR operation between a <see cref="BitFlag"/> and a <see cref="BitValue"/>.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="bitValue">The <see cref="BitValue"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the OR operation.</returns>
    public static BitFlag operator |(BitFlag left, BitValue bitValue) => new(left.FlagValue | bitValue.Value);
    
    /// <summary>
    /// Performs a bitwise XOR operation between a <see cref="BitFlag"/> and a <see cref="BitValue"/>.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="bitValue">The <see cref="BitValue"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the XOR operation.</returns>
    public static BitFlag operator ^(BitFlag left, BitValue bitValue) => new(left.FlagValue ^ bitValue.Value);
    
    /// <summary>
    /// Performs a bitwise AND operation between a <see cref="BitValue"/> and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="bitValue">The <see cref="BitValue"/> operand.</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the AND operation.</returns>
    public static BitFlag operator &(BitValue bitValue, BitFlag right) => new(bitValue.Value & right.FlagValue);
    
    /// <summary>
    /// Performs a bitwise OR operation between a <see cref="BitValue"/> and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="bitValue">The <see cref="BitValue"/> operand.</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the OR operation.</returns>
    public static BitFlag operator |(BitValue bitValue, BitFlag right) => new(bitValue.Value | right.FlagValue);
    
    /// <summary>
    /// Performs a bitwise XOR operation between a <see cref="BitValue"/> and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="bitValue">The <see cref="BitValue"/> operand.</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the XOR operation.</returns>
    public static BitFlag operator ^(BitValue bitValue, BitFlag right) => new(bitValue.Value ^ right.FlagValue);
    
    /// <summary>
    /// Performs a bitwise AND operation between a <see cref="BitFlag"/> and a uint value.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="bitValue">The uint value operand. Has to be a power of two, otherwise left will be returned!</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the AND operation.</returns>
    public static BitFlag operator &(BitFlag left, uint bitValue) => IsPowerOfTwo(bitValue) ? new(left.FlagValue & bitValue) : left;

    /// <summary>
    /// Performs a bitwise OR operation between a <see cref="BitFlag"/> and a uint value.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="bitValue">The uint value operand. Has to be a power of two, otherwise left will be returned!</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the OR operation.</returns>
    public static BitFlag operator |(BitFlag left, uint bitValue) => IsPowerOfTwo(bitValue) ? new(left.FlagValue | bitValue) : left;

    /// <summary>
    /// Performs a bitwise XOR operation between a <see cref="BitFlag"/> and a uint value.
    /// </summary>
    /// <param name="left">The <see cref="BitFlag"/> operand.</param>
    /// <param name="bitValue">The uint value operand. Has to be a power of two, otherwise left will be returned!</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the XOR operation.</returns>
    public static BitFlag operator ^(BitFlag left, uint bitValue) => IsPowerOfTwo(bitValue) ? new(left.FlagValue ^ bitValue) : left;
    
    /// <summary>
    /// Performs a bitwise AND operation between a uint value and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="bitValue">The uint value operand. Has to be a power of two, otherwise right will be returned!</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the AND operation.</returns>
    public static BitFlag operator &(uint bitValue, BitFlag right) => IsPowerOfTwo(bitValue) ? new(bitValue & right.FlagValue) : right;

    /// <summary>
    /// Performs a bitwise OR operation between a uint value and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="bitValue">The uint value operand. Has to be a power of two, otherwise right will be returned!</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the OR operation.</returns>
    public static BitFlag operator |(uint bitValue, BitFlag right) => IsPowerOfTwo(bitValue) ? new(bitValue | right.FlagValue) : right;

    /// <summary>
    /// Performs a bitwise XOR operation between a uint value and a <see cref="BitFlag"/>.
    /// </summary>
    /// <param name="bitValue">The uint value operand. Has to be a power of two, otherwise right will be returned!</param>
    /// <param name="right">The <see cref="BitFlag"/> operand.</param>
    /// <returns>A new <see cref="BitFlag"/> instance representing the result of the XOR operation.</returns>
    public static BitFlag operator ^(uint bitValue, BitFlag right) => IsPowerOfTwo(bitValue) ? new(bitValue ^ right.FlagValue) : right;
    #endregion
}