namespace ShapeEngine.Core;

/// <summary>
/// Represents a decimal-place precision setting together with its derived base-10 scale factors.
/// </summary>
/// <remarks>
/// This type is useful when converting floating-point values into quantized integer-like values,
/// for example during geometric hashing or when interfacing with systems that operate on scaled coordinates.
/// Supported precision values are clamped to the inclusive range <c>0</c> to <c>16</c>.
/// </remarks>
public readonly struct DecimalPrecision
{
    #region Members
    
    /// <summary>
    /// Gets the default number of decimal places used by quantized hashing and equality helpers.
    /// </summary>
    public const int DefaultDecimalPlaces = 3;

    /// <summary>
    /// Gets the number of decimal places used for quantization.
    /// </summary>
    public readonly int DecimalPlaces;

    /// <summary>
    /// Gets the base-10 scale factor for <see cref="DecimalPlaces"/>.
    /// </summary>
    /// <remarks>
    /// For example, a precision of <c>4</c> yields a scale factor of <c>10000</c>.
    /// </remarks>
    public readonly double Scale;

    /// <summary>
    /// Gets the reciprocal of <see cref="Scale"/>.
    /// </summary>
    public readonly double InvScale;
    
    private static readonly double[] scaleTable =
    {
        1d,
        10d,
        100d,
        1000d,
        10000d,
        100000d,
        1000000d,
        10000000d,
        100000000d,
        1000000000d,
        10000000000d,
        100000000000d,
        1000000000000d,
        10000000000000d,
        100000000000000d,
        1000000000000000d,
        10000000000000000d
    };
    #endregion
    
    #region Constructors
    
    /// <summary>
    /// Creates a new <see cref="DecimalPrecision"/> value.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The requested number of decimal places. Values are clamped to the inclusive range <c>0</c> to <c>16</c>.
    /// </param>
    public DecimalPrecision(int decimalPlaces = 4)
    {
        DecimalPlaces = Math.Clamp(decimalPlaces, 0, 16);
        Scale = GetScaleFactor(DecimalPlaces);
        InvScale = 1.0 / Scale;
    }

    #endregion

    #region Powers of 10
    /// <summary>Gets 10^0.</summary>
    public static double Pow10_0  => 1d;
    /// <summary>Gets 10^1.</summary>
    public static double Pow10_1  => 10d;
    /// <summary>Gets 10^2.</summary>
    public static double Pow10_2  => 100d;
    /// <summary>Gets 10^3.</summary>
    public static double Pow10_3  => 1000d;
    /// <summary>Gets 10^4.</summary>
    public static double Pow10_4  => 10000d;
    /// <summary>Gets 10^5.</summary>
    public static double Pow10_5  => 100000d;
    /// <summary>Gets 10^6.</summary>
    public static double Pow10_6  => 1000000d;
    /// <summary>Gets 10^7.</summary>
    public static double Pow10_7  => 10000000d;
    /// <summary>Gets 10^8.</summary>
    public static double Pow10_8  => 100000000d;
    /// <summary>Gets 10^9.</summary>
    public static double Pow10_9  => 1000000000d;
    /// <summary>Gets 10^10.</summary>
    public static double Pow10_10 => 10000000000d;
    /// <summary>Gets 10^11.</summary>
    public static double Pow10_11 => 100000000000d;
    /// <summary>Gets 10^12.</summary>
    public static double Pow10_12 => 1000000000000d;
    /// <summary>Gets 10^13.</summary>
    public static double Pow10_13 => 10000000000000d;
    /// <summary>Gets 10^14.</summary>
    public static double Pow10_14 => 100000000000000d;
    /// <summary>Gets 10^15.</summary>
    public static double Pow10_15 => 1000000000000000d;
    /// <summary>Gets 10^16.</summary>
    public static double Pow10_16 => 10000000000000000d;
    #endregion
    
    /// <summary>
    /// Gets the base-10 scale factor for a decimal-place count.
    /// </summary>
    /// <param name="decimalPlaces">
    /// The requested number of decimal places. Values are clamped to the inclusive range <c>0</c> to <c>16</c>.
    /// </param>
    /// <returns>
    /// A base-10 scale factor equivalent to <c>10^decimalPlaces</c> after clamping.
    /// </returns>
    public static double GetScaleFactor(int decimalPlaces)
    {
        decimalPlaces = Math.Clamp(decimalPlaces, 0, 16);
        return scaleTable[decimalPlaces];
    }

}