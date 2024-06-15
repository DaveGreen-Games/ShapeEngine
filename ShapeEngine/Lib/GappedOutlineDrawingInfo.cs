namespace ShapeEngine.Lib;

/// <summary>
/// Used for DrawGappedOutline functions.
/// </summary>
public readonly struct GappedOutlineDrawingInfo
{
    public readonly float StartOffset;
    public readonly int Gaps;
    public readonly float GapPerimeterPercentage;

    public bool IsValid => Gaps > 0 && GapPerimeterPercentage is < 1f and > 0f;
    
    /// <summary>
    /// </summary>
    /// <param name="startOffset">Value between 0 and 1 to change where the first part is drawn (useful for animating it).
    /// The offset value is wrapped between 0 and 1. 1.5 would result in 0.5. -0.2 would result in 0.8.</param>
    /// <param name="gaps">The count of outline areas that are not drawn. The count of outline areas drawn always equals the count of gaps.</param>
    /// <param name="gapPerimeterPercentage">How much of the total perimeter should be not drawn. 1 - gapPercentage equals the total amount of perimeter drawn. If 0 normal outline is drawn, if 1 nothing is drawn.</param>
    public GappedOutlineDrawingInfo(int gaps, float startOffset = 0f, float gapPerimeterPercentage = 0.5f)
    {
        
        StartOffset = ShapeMath.WrapF(startOffset, 0f, 1f);;
        Gaps = gaps;
        GapPerimeterPercentage = gapPerimeterPercentage;
    }


    public GappedOutlineDrawingInfo MoveStartOffset(float amount) => new(Gaps, StartOffset + amount, GapPerimeterPercentage);
    public GappedOutlineDrawingInfo ChangeStartOffset(float newStartOffset) => new(Gaps, newStartOffset, GapPerimeterPercentage);
    public GappedOutlineDrawingInfo ChangeGaps(int newGaps) => new(newGaps, StartOffset, GapPerimeterPercentage);
    public GappedOutlineDrawingInfo ChangeGapPerimeterPercentage(float newGapPerimeterPercentage) => new(Gaps, StartOffset, newGapPerimeterPercentage);
}