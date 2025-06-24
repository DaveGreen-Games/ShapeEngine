using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.Structs;

/// <summary>
/// Contains information for drawing a gapped outline, such as the number of gaps,
/// the starting offset, and the percentage of the perimeter that is gapped.
/// Used for DrawGappedOutline functions.
/// </summary>
public readonly struct GappedOutlineDrawingInfo
{
    /// <summary>
    /// Value between 0 and 1 to change where the first part is drawn (useful for animating it).
    /// The offset value is wrapped between 0 and 1.
    /// </summary>
    /// <example>
    /// For example:
    /// <list type="bullet">
    ///   <item><description>If <c>startOffset</c> is 1.5, it wraps to 0.5.</description></item>
    ///   <item><description>If <c>startOffset</c> is -0.2, it wraps to 0.8.</description></item>
    /// </list>
    /// </example>
    public readonly float StartOffset;

    /// <summary>
    /// The number of gap segments (not drawn) in the outline.
    /// The number of drawn segments always equals the number of gaps.
    /// </summary>
    /// <remarks>
    /// The total gap length, defined by <see cref="GapPerimeterPercentage"/>, is divided equally among all <see cref="Gaps"/>.
    /// The total drawn length, defined by <c>1 - GapPerimeterPercentage</c>, is also divided equally among all <see cref="Gaps"/>.
    /// </remarks>
    /// <example>
    /// For example:
    /// <list type="bullet">
    ///   <item><description>6 gaps result in 6 drawn segments and 6 gaps. If <see cref="GapPerimeterPercentage"/> is 0.5, all segments and gaps are equal in length.</description></item>
    ///   <item><description>If <see cref="GapPerimeterPercentage"/> is 0.75, then 75% of the perimeter is divided into 6 gaps and 25% into 6 drawn segments.</description></item>
    /// </list>
    /// </example>
    public readonly int Gaps;

    /// <summary>
    /// The fraction of the total perimeter that should be a gap (not drawn).
    /// </summary>
    /// <list type="bullet">
    ///   <item><description><c>1 - GapPerimeterPercentage</c> is the fraction of the perimeter that is drawn.</description></item>
    ///   <item><description>If 0, the outline is fully drawn; if 1, nothing is drawn.</description></item>
    /// </list>
    /// <remarks>
    /// The total drawn perimeter is divided equally among all drawn segments.
    /// </remarks>
    public readonly float GapPerimeterPercentage;

    /// <summary>
    /// Indicates whether the current configuration is valid (gaps &gt; 0 and gap percentage between 0 and 1).
    /// </summary>
    public bool IsValid => Gaps > 0 && GapPerimeterPercentage is < 1f and > 0f;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GappedOutlineDrawingInfo"/> struct.
    /// </summary>
    /// <param name="gaps">The count of outline areas that are not drawn. The count of outline areas drawn always equals the count of gaps.</param>
    /// <param name="startOffset">Value between 0 and 1 to change where the first part is drawn (useful for animating it).
    /// The offset value is wrapped between 0 and 1. Examples:
    /// <list type="bullet">
    /// <item><description>1.5 would result in 0.5.</description> </item>
    /// <item><description>-0.2 would result in 0.8</description> </item>
    /// </list>
    /// </param>
    /// <param name="gapPerimeterPercentage">
    /// How much of the total perimeter should be a gap (not drawn).
    /// <list type="bullet">
    /// <item><description> <c>1 - gapPercentage</c> equals the total amount of perimeter drawn.</description> </item>
    /// <item><description>If 0 normal outline is drawn, if 1 nothing is drawn.</description> </item>
    /// </list>
    /// </param>
    public GappedOutlineDrawingInfo(int gaps, float startOffset = 0f, float gapPerimeterPercentage = 0.5f)
    {
        StartOffset = ShapeMath.WrapF(startOffset, 0f, 1f);
        Gaps = gaps;
        GapPerimeterPercentage = gapPerimeterPercentage;
    }

    /// <summary>
    /// Returns a new <see cref="GappedOutlineDrawingInfo"/> with the start offset moved by the specified amount.
    /// </summary>
    /// <param name="amount">The amount to move the start offset by.</param>
    /// <returns>A new <see cref="GappedOutlineDrawingInfo"/> with the updated start offset.</returns>
    public GappedOutlineDrawingInfo MoveStartOffset(float amount) => new(Gaps, StartOffset + amount, GapPerimeterPercentage);

    /// <summary>
    /// Returns a new <see cref="GappedOutlineDrawingInfo"/> with the start offset set to the specified value.
    /// </summary>
    /// <param name="newStartOffset">The new start offset value.</param>
    /// <returns>A new <see cref="GappedOutlineDrawingInfo"/> with the updated start offset.</returns>
    public GappedOutlineDrawingInfo ChangeStartOffset(float newStartOffset) => new(Gaps, newStartOffset, GapPerimeterPercentage);

    /// <summary>
    /// Returns a new <see cref="GappedOutlineDrawingInfo"/> with the number of gaps set to the specified value.
    /// </summary>
    /// <param name="newGaps">The new number of gaps.</param>
    /// <returns>A new <see cref="GappedOutlineDrawingInfo"/> with the updated number of gaps.</returns>
    public GappedOutlineDrawingInfo ChangeGaps(int newGaps) => new(newGaps, StartOffset, GapPerimeterPercentage);

    /// <summary>
    /// Returns a new <see cref="GappedOutlineDrawingInfo"/> with the gap perimeter percentage set to the specified value.
    /// </summary>
    /// <param name="newGapPerimeterPercentage">The new gap perimeter percentage.</param>
    /// <returns>A new <see cref="GappedOutlineDrawingInfo"/> with the updated gap perimeter percentage.</returns>
    public GappedOutlineDrawingInfo ChangeGapPerimeterPercentage(float newGapPerimeterPercentage) => new(Gaps, StartOffset, newGapPerimeterPercentage);
}