using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.RectDef;

public readonly partial struct Rect
{
    #region Transform

    public Rect ScaleSize(float horizontalAmount, float verticalAmount)
    {
        return new
        (
            X - horizontalAmount,
            Y - verticalAmount,
            Width + horizontalAmount * 2f,
            Height + verticalAmount * 2f
        );
    }

    public Rect ScaleSize(float scale, AnchorPoint alignement) => new(GetPoint(alignement), Size * scale, alignement);
    public Rect ScaleSize(Vector2 scale, AnchorPoint alignement) => new(GetPoint(alignement), Size * scale, alignement);
    public Rect SetSize(Size newSize) => new(TopLeft, newSize);
    public Rect SetSize(Size newSize, AnchorPoint alignement) => new(GetPoint(alignement), newSize, alignement);
    public Rect SetSize(float newSize, AnchorPoint alignement) => new(GetPoint(alignement), new Size(newSize), alignement);
    public Rect ChangeSize(float amount, AnchorPoint alignement) => new(GetPoint(alignement), Size + amount, alignement);
    public Rect ChangeSize(Size amount, AnchorPoint alignement) => new(GetPoint(alignement), Size + amount, alignement);
    public Rect SetPosition(Vector2 newPosition, AnchorPoint alignement) => new(newPosition, Size, alignement);
    public Rect SetPosition(Vector2 newPosition) => new(newPosition, Size, new AnchorPoint(0f));

    public Rect ChangePosition(Vector2 amount)
    {
        return new(TopLeft + amount, Size, new(0f));
    }

    /// <summary>
    /// Moves the rect by offset.Position
    /// Changes the size of the moved rect by offset.ScaledSize
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Rect ApplyOffset(Transform2D offset, AnchorPoint alignement)
    {
        var newRect = ChangePosition(offset.Position);
        return newRect.ChangeSize(offset.ScaledSize, alignement);
    }

    /// <summary>
    /// Moves the rect to transform.Position
    /// Sets the size of the moved rect to transform.Size
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Rect SetTransform(Transform2D transform, AnchorPoint alignement)
    {
        var newRect = SetPosition(transform.Position, alignement);
        return newRect.SetSize(transform.BaseSize, alignement);
    }

    #endregion

    #region Math

    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A, B, C, D,
            A + v,
            B + v,
            C + v,
            D + v
        };
        return points;
    }

    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;

        var points = new Points
        {
            A, B, C, D,
            A + v,
            B + v,
            C + v,
            D + v
        };
        return Polygon.FindConvexHull(points);
    }

    public Rect Floor()
    {
        return new Rect(
            MathF.Floor(X),
            MathF.Floor(Y),
            MathF.Floor(Width),
            MathF.Floor(Height));
    }

    public Rect Ceiling()
    {
        return new Rect(
            MathF.Ceiling(X),
            MathF.Ceiling(Y),
            MathF.Ceiling(Width),
            MathF.Ceiling(Height));
    }

    public Rect Truncate()
    {
        return new Rect(
            MathF.Truncate(X),
            MathF.Truncate(Y),
            MathF.Truncate(Width),
            MathF.Truncate(Height));
    }

    public Rect Round()
    {
        return new Rect(
            MathF.Round(X),
            MathF.Round(Y),
            MathF.Round(Width),
            MathF.Round(Height));
    }

    public float GetPerimeter()
    {
        return Width * 2 + Height * 2;
    }

    public float GetPerimeterSquared()
    {
        return (Width * Width) * 2 + (Height * Height) * 2;
    }

    public float GetArea()
    {
        return Width * Height;
    }

    public bool SeperateAxis(Vector2 axisStart, Vector2 axisEnd)
    {
        var n = axisStart - axisEnd;
        var corners = ToPolygon();
        var edgeAStart = corners[0];
        var edgeAEnd = corners[1];
        var edgeBStart = corners[2];
        var edgeBEnd = corners[3];

        var edgeARange = Segment.ProjectSegment(edgeAStart, edgeAEnd, n);
        var edgeBRange = Segment.ProjectSegment(edgeBStart, edgeBEnd, n);
        var rProjection = RangeHull(edgeARange, edgeBRange);

        var axisRange = Segment.ProjectSegment(axisStart, axisEnd, n);
        return !axisRange.OverlapValueRange(rProjection);
    }

    public Rect Lerp(Rect to, float f)
    {
        return
            new
            (
                ShapeMath.LerpFloat(X, to.X, f),
                ShapeMath.LerpFloat(Y, to.Y, f),
                ShapeMath.LerpFloat(Width, to.Width, f),
                ShapeMath.LerpFloat(Height, to.Height, f)
            );
    }

    public Rect Align(AnchorPoint alignement)
    {
        return new(TopLeft, Size, alignement);
    }

    public AnchorPoint GetAnchorPoint(Vector2 point)
    {
        var dif = point - TopLeft;
        var f = dif / Size;
        return new AnchorPoint(f);
    }

    public Vector2 GetAbsolutePoint(AnchorPoint anchor)
    {
        return Size.ToVector2() * anchor;
    }

    /// <summary>
    /// Returns a value between 0 - 1 for x & y axis based on where the point is within the rect.
    /// topleft is considered (0,0) and bottomright is considered (1,1).
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Vector2 PointToRelative(Vector2 p)
    {
        var dif = p - TopLeft;
        var intensity = dif / Size;

        float xFactor = intensity.X < 0f ? 0f : intensity.X > 1f ? 1f : intensity.X;
        float yFactor = intensity.Y < 0f ? 0f : intensity.Y > 1f ? 1f : intensity.Y;
        return new(xFactor, yFactor);
    }

    public Vector2 PointToAbsolute(Vector2 relativePoint)
    {
        return relativePoint * Size;
    }

    /// <summary>
    /// Returns a value between 0 - 1 for x axis based on where the point is within the rect.
    /// topleft is considered (0,0) and bottomright is considered (1,1).
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public float GetWidthFactor(float x)
    {
        float dif = x - Left;
        float intensity = dif / Width;
        return intensity < 0f ? 0f : intensity > 1f ? 1f : intensity;
    }

    /// <summary>
    /// Returns a value between 0 - 1 for y axis based on where the point is within the rect.
    /// topleft is considered (0,0) and bottomright is considered (1,1).
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public float GetHeightFactor(float y)
    {
        float dif = y - Top;
        float intensity = dif / Height;
        return intensity < 0f ? 0f : intensity > 1f ? 1f : intensity;
    }

    public Rect Enlarge(Vector2 p)
    {
        Vector2 tl = new
        (
            MathF.Min(X, p.X),
            MathF.Min(Y, p.Y)
        );
        Vector2 br = new
        (
            MathF.Max(X + Width, p.X),
            MathF.Max(Y + Height, p.Y)
        );
        return new(tl, br);
    }

    public Vector2 ClampOnRect(Vector2 p)
    {
        return new
        (
            ShapeMath.Clamp(p.X, X, X + Width),
            ShapeMath.Clamp(p.Y, Y, Y + Height)
        );
    }

    public Rect Clamp(Rect bounds)
    {
        var tl = bounds.ClampOnRect(TopLeft);
        var br = bounds.ClampOnRect(BottomRight);
        return new(tl, br);
    }

    public Rect Clamp(Vector2 min, Vector2 max)
    {
        return Clamp(new Rect(min, max));
    }

    #endregion
}