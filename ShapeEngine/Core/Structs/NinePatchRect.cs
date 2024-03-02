using ShapeEngine.Core.Shapes;

namespace ShapeEngine.Core.Structs;

public readonly struct NinePatchRect
{
    public readonly Rect TopLeft;
    public readonly Rect TopCenter;
    public readonly Rect TopRight;
        
    public readonly Rect CenterLeft;
    public readonly Rect Center;
    public readonly Rect CenterRight;
        
    public readonly Rect BottomLeft;
    public readonly Rect BottomCenter;
    public readonly Rect BottomRight;

    public readonly List<Rect> Rects => new() {TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight };
    public readonly Rect Top => new(TopLeft.TopLeft, TopRight.BottomRight);
    public readonly Rect Bottom => new(BottomLeft.TopLeft, BottomRight.BottomRight);
        
    public readonly Rect Left => new(TopLeft.TopLeft, BottomLeft.BottomRight);
    public readonly Rect Right => new(TopRight.TopLeft, BottomRight.BottomRight);
        
    public readonly Rect CenterV => new(TopCenter.TopLeft, BottomCenter.BottomRight);
    public readonly Rect CenterH => new(CenterLeft.TopLeft, CenterRight.BottomRight);
        
    public readonly Rect Source => new(TopLeft.TopLeft, BottomRight.BottomRight);
        
    public readonly Rect LeftQuadrant => new(TopLeft.TopLeft, BottomCenter.BottomRight);
    public readonly Rect RightQuadrant => new(TopCenter.TopLeft, BottomRight.BottomRight);
    public readonly Rect TopQuadrant => new(TopLeft.TopLeft, CenterRight.BottomRight);
    public readonly Rect BottomQuadrant => new(CenterLeft.TopLeft, BottomRight.BottomRight);
        
    public readonly Rect TopLeftQuadrant => new(TopLeft.TopLeft, Center.BottomRight);
    public readonly Rect TopRightQuadrant => new(TopCenter.TopLeft, CenterRight.BottomRight);
    public readonly Rect BottomLeftQuadrant => new(CenterLeft.TopLeft, BottomCenter.BottomRight);
    public readonly Rect BottomRightQuadrant => new(Center.TopLeft, BottomRight.BottomRight);

    public NinePatchRect(Rect source)
    {
        var rects = source.Split(3, 3);
        TopLeft = rects.Count > 0 ? rects[0] : new();
        TopCenter = rects.Count > 1 ? rects[1] : new();
        TopRight = rects.Count > 2 ? rects[2] : new();
            
        CenterLeft = rects.Count > 3 ? rects[3] : new();
        Center = rects.Count > 4 ? rects[4] : new();
        CenterRight = rects.Count > 5 ? rects[5] : new();
            
        BottomLeft = rects.Count > 6 ? rects[6] : new();
        BottomCenter = rects.Count > 7 ? rects[7] : new();
        BottomRight = rects.Count > 8 ? rects[8] : new();
    }
    public NinePatchRect(Rect source, float h1, float h2, float v1, float v2)
    {
        var rects = source.Split(new[] { h1, h2 }, new[] { v1, v2 });
        TopLeft = rects.Count > 0 ? rects[0] : new();
        TopCenter = rects.Count > 1 ? rects[1] : new();
        TopRight = rects.Count > 2 ? rects[2] : new();
            
        CenterLeft = rects.Count > 3 ? rects[3] : new();
        Center = rects.Count > 4 ? rects[4] : new();
        CenterRight = rects.Count > 5 ? rects[5] : new();
            
        BottomLeft = rects.Count > 6 ? rects[6] : new();
        BottomCenter = rects.Count > 7 ? rects[7] : new();
        BottomRight = rects.Count > 8 ? rects[8] : new();
    }
    public NinePatchRect(Rect source, float h1, float h2, float v1, float v2, float marginH, float marginV)
    {
        var rects = source.Split(new[] { h1, h2 }, new[] { v1, v2 });
        TopLeft = rects.Count > 0 ? rects[0].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        TopCenter = rects.Count > 1 ? rects[1].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        TopRight = rects.Count > 2 ? rects[2].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            
        CenterLeft = rects.Count > 3 ? rects[3].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        Center = rects.Count > 4 ? rects[4].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        CenterRight = rects.Count > 5 ? rects[5].ApplyMargins(marginH, marginH, marginV, marginV) : new();
            
        BottomLeft = rects.Count > 6 ? rects[6].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        BottomCenter = rects.Count > 7 ? rects[7].ApplyMargins(marginH, marginH, marginV, marginV) : new();
        BottomRight = rects.Count > 8 ? rects[8].ApplyMargins(marginH, marginH, marginV, marginV) : new();
    }
    /// <summary>
    /// Sets the rect in left to right & top to bottom order (top left is first, bottomRight is last).
    /// Only the first nine rects are used. If there are less than 9 rects, the remaining will be filled with empty rects.
    /// </summary>
    /// <param name="rects"></param>
    public NinePatchRect(IReadOnlyList<Rect> rects)
    {
        TopLeft = rects.Count > 0 ? rects[0] : new();
        TopCenter = rects.Count > 1 ? rects[1] : new();
        TopRight = rects.Count > 2 ? rects[2] : new();
            
        CenterLeft = rects.Count > 3 ? rects[3] : new();
        Center = rects.Count > 4 ? rects[4] : new();
        CenterRight = rects.Count > 5 ? rects[5] : new();
            
        BottomLeft = rects.Count > 6 ? rects[6] : new();
        BottomCenter = rects.Count > 7 ? rects[7] : new();
        BottomRight = rects.Count > 8 ? rects[8] : new();
    }
    public NinePatchRect(NinePatchRect npr, float marginH, float marginV)
    {
        TopLeft = npr.TopLeft.ApplyMargins(marginH, marginH, marginV, marginV);
        TopCenter = npr.TopCenter.ApplyMargins(marginH, marginH, marginV, marginV);
        TopRight = npr.TopRight.ApplyMargins(marginH, marginH, marginV, marginV);
            
        CenterLeft = npr.CenterLeft.ApplyMargins(marginH, marginH, marginV, marginV);
        Center = npr.Center.ApplyMargins(marginH, marginH, marginV, marginV);
        CenterRight = npr.CenterRight.ApplyMargins(marginH, marginH, marginV, marginV);
            
        BottomLeft = npr.BottomLeft.ApplyMargins(marginH, marginH, marginV, marginV);
        BottomCenter = npr.BottomCenter.ApplyMargins(marginH, marginH, marginV, marginV);
        BottomRight = npr.BottomRight.ApplyMargins(marginH, marginH, marginV, marginV);
    }
}