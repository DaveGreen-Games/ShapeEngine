namespace ShapeEngine.ShapeClipper;

/// <summary>
/// Filling rules determine which sub-regions of complex polygons are considered "inside".
/// Complex polygons are defined by one or more closed contours; only portions of these contours
/// may contribute to filled regions, so a filling rule is required to decide which sub-regions
/// are treated as inside when performing clipping operations.
/// </summary>
/// <example>
/// Example algorithm (winding number):
/// From a point outside the polygon draw a ray through the polygon. Start with winding number 0.
/// For each contour crossed, increment the winding number if the crossing goes right-to-left
/// relative to the ray, otherwise decrement. Each sub-region gets the current winding number.
/// </example>
/// <remarks>
/// The supported fill rules are based on winding numbers derived from the orientation of each path:
/// <list type="bullet">
/// <item>Even-Odd: toggles inside/outside each time a contour is crossed.</item>
/// <item>Non-Zero: considers the sum of winding contributions; non-zero means inside.</item>
/// <item>Positive / Negative: depend on the sign of the winding number.</item>
/// </list>
/// Notes:
/// <list type="bullet">
/// <item>The most commonly used rules are Even-Odd and Non-Zero.</item>
/// <item>Reversing a path reverses its orientation (and the sign of winding numbers) but does not affect parity or whether a winding number is zero.</item>
/// <item>Filling rules are required only for clipping operations; they do not affect polygon offsetting.</item>
/// </list>
/// </remarks>
public enum ShapeClipperFillRule
{
    /// <summary>
    /// Even-Odd rule: only sub-regions with odd winding parity are filled.
    /// </summary>
    EvenOdd,

    /// <summary>
    /// Non-Zero rule: any sub-region with a non-zero winding number is filled.
    /// </summary>
    NonZero,

    /// <summary>
    /// Positive rule: only sub-regions with winding counts &gt; 0 are filled.
    /// </summary>
    Positive,

    /// <summary>
    /// Negative rule: only sub-regions with winding counts &lt; 0 are filled.
    /// </summary>
    Negative
}