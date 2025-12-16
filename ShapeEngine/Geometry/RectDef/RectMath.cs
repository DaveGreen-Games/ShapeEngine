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

    /// <summary>
    /// Scales the size of the rectangle by the specified horizontal and vertical amounts, keeping the center fixed.
    /// </summary>
    /// <param name="horizontalAmount">The amount to scale horizontally (positive to expand, negative to shrink).</param>
    /// <param name="verticalAmount">The amount to scale vertically (positive to expand, negative to shrink).</param>
    /// <returns>A new rectangle with the scaled size.</returns>
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

    /// <summary>
    /// Scales the size of the rectangle by a uniform factor, using the specified anchor point for alignment.
    /// </summary>
    /// <param name="scale">The uniform scale factor.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the scaled size.</returns>
    /// <remarks> Use a negative scale value to mirror the rect.</remarks>
    public Rect ScaleSize(float scale, AnchorPoint alignment) => new(GetPoint(alignment), Size * scale, alignment);

    /// <summary>
    /// Scales the size of the rectangle by a vector factor, using the specified anchor point for alignment.
    /// </summary>
    /// <param name="scale">The scale factor for width and height.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the scaled size.</returns>
    /// <remarks> Use negative scale values to mirror the rect.</remarks>
    public Rect ScaleSize(Vector2 scale, AnchorPoint alignment) => new(GetPoint(alignment), Size * scale, alignment);

    /// <summary>
    /// Sets the size of the rectangle to the specified value, keeping the top-left corner fixed.
    /// </summary>
    /// <param name="newSize">The new size for the rectangle.</param>
    /// <returns>A new rectangle with the specified size.</returns>
    /// <remarks>Use negative size values to mirror the rect.</remarks>
    public Rect SetSize(Size newSize) => new(TopLeft, newSize);

    /// <summary>
    /// Sets the size of the rectangle to the specified value, using the specified anchor point for alignment.
    /// </summary>
    /// <param name="newSize">The new size for the rectangle.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the specified size and alignment.</returns>
    public Rect SetSize(Size newSize, AnchorPoint alignment) => new(GetPoint(alignment), newSize, alignment);

    /// <summary>
    /// Sets the size of the rectangle to a uniform value, using the specified anchor point for alignment.
    /// </summary>
    /// <param name="newSize">The new size for both width and height.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the specified size and alignment.</returns>
    public Rect SetSize(float newSize, AnchorPoint alignment) => new(GetPoint(alignment), new Size(newSize), alignment);

    /// <summary>
    /// Changes the size of the rectangle by a uniform amount, using the specified anchor point for alignment.
    /// </summary>
    /// <param name="amount">The amount to change the size by (positive to expand, negative to shrink).</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the changed size and alignment.</returns>
    public Rect ChangeSize(float amount, AnchorPoint alignment) => new(GetPoint(alignment), Size + amount, alignment);

    /// <summary>
    /// Changes the size of the rectangle by a vector amount, using the specified anchor point for alignment.
    /// </summary>
    /// <param name="amount">The amount to change the size by for width and height.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the changed size and alignment.</returns>
    public Rect ChangeSize(Size amount, AnchorPoint alignment) => new(GetPoint(alignment), Size + amount, alignment);

    /// <summary>
    /// Sets the position of the rectangle to the specified value, using the specified anchor point for alignment.
    /// </summary>
    /// <param name="newPosition">The new position for the rectangle.</param>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the specified position and alignment.</returns>
    public Rect SetPosition(Vector2 newPosition, AnchorPoint alignment) => new(newPosition, Size, alignment);

    /// <summary>
    /// Sets the position of the rectangle to the specified value, keeping the anchor at (0,0).
    /// </summary>
    /// <param name="newPosition">The new position for the rectangle.</param>
    /// <returns>A new rectangle with the specified position.</returns>
    public Rect SetPosition(Vector2 newPosition) => new(newPosition, Size, new AnchorPoint(0f));

    /// <summary>
    /// Changes the position of the rectangle by the specified amount, keeping the anchor at (0,0).
    /// </summary>
    /// <param name="amount">The amount to move the rectangle by.</param>
    /// <returns>A new rectangle with the changed position.</returns>
    public Rect ChangePosition(Vector2 amount)
    {
        return new(TopLeft + amount, Size, new(0f));
    }

    /// <summary>
    /// Moves the rect by offset.Position
    /// Changes the size of the moved rect by offset.ScaledSize
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="alignment"></param>
    /// <returns></returns>
    public Rect ApplyOffset(Transform2D offset, AnchorPoint alignment)
    {
        var newRect = ChangePosition(offset.Position);
        return newRect.ChangeSize(offset.ScaledSize, alignment);
    }

    /// <summary>
    /// Moves the rect to transform.Position
    /// Sets the size of the moved rect to transform.ScaledSize
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="alignment"></param>
    /// <returns></returns>
    public Rect SetTransform(Transform2D transform, AnchorPoint alignment)
    {
        var newRect = SetPosition(transform.Position, alignment);
        return newRect.SetSize(transform.ScaledSize, alignment);
    }

    #endregion

    #region Math

    /// <summary>
    /// Gets the projected shape points of the rectangle in the direction of the given vector.
    /// </summary>
    /// <param name="v">The vector indicating the projection direction.</param>
    /// <returns>The projected shape points.</returns>
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

    /// <summary>
    /// Projects the shape of the rectangle in the direction of the given vector, returning a convex hull.
    /// </summary>
    /// <param name="v">The vector indicating the projection direction.</param>
    /// <returns>The projected convex hull of the rectangle's shape.</returns>
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

    /// <summary>
    /// Floors the rectangle's position and size, returning a new rectangle with integer values.
    /// </summary>
    /// <returns>A new rectangle with floored position and size.</returns>
    public Rect Floor()
    {
        return new Rect(
            MathF.Floor(X),
            MathF.Floor(Y),
            MathF.Floor(Width),
            MathF.Floor(Height));
    }

    /// <summary>
    /// Ceils the rectangle's position and size, returning a new rectangle with integer values.
    /// </summary>
    /// <returns>A new rectangle with ceiled position and size.</returns>
    public Rect Ceiling()
    {
        return new Rect(
            MathF.Ceiling(X),
            MathF.Ceiling(Y),
            MathF.Ceiling(Width),
            MathF.Ceiling(Height));
    }

    /// <summary>
    /// Truncates the rectangle's position and size to integers, returning a new rectangle.
    /// </summary>
    /// <returns>A new rectangle with truncated position and size.</returns>
    public Rect Truncate()
    {
        return new Rect(
            MathF.Truncate(X),
            MathF.Truncate(Y),
            MathF.Truncate(Width),
            MathF.Truncate(Height));
    }

    /// <summary>
    /// Rounds the rectangle's position and size to the nearest integers, returning a new rectangle.
    /// </summary>
    /// <returns>A new rectangle with rounded position and size.</returns>
    public Rect Round()
    {
        return new Rect(
            MathF.Round(X),
            MathF.Round(Y),
            MathF.Round(Width),
            MathF.Round(Height));
    }

    /// <summary>
    /// Calculates the perimeter of the rectangle.
    /// </summary>
    /// <returns>The perimeter of the rectangle.</returns>
    public float GetPerimeter()
    {
        return Width * 2 + Height * 2;
    }

    /// <summary>
    /// Calculates the squared perimeter of the rectangle.
    /// </summary>
    /// <returns>The squared perimeter of the rectangle.</returns>
    public float GetPerimeterSquared()
    {
        var p = Width * 2 + Height * 2;
        return p * p;
    }

    /// <summary>
    /// Calculates the area of the rectangle.
    /// </summary>
    /// <returns>The area of the rectangle.</returns>
    public float GetArea()
    {
        return Width * Height;
    }

    /// <summary>
    /// Checks if there is a separating axis for the rectangle defined by the given start and end points.
    /// </summary>
    /// <param name="axisStart">The start point of the axis.</param>
    /// <param name="axisEnd">The end point of the axis.</param>
    /// <returns>True if there is a separating axis, false otherwise.</returns>
    public bool SeparateAxis(Vector2 axisStart, Vector2 axisEnd)
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

    /// <summary>
    /// Linearly interpolates between this rectangle and another rectangle by a given factor.
    /// </summary>
    /// <param name="to">The target rectangle to interpolate to.</param>
    /// <param name="f">The interpolation factor (0 = this, 1 = to).</param>
    /// <returns>A new rectangle interpolated between this and the target rectangle.</returns>
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

    /// <summary>
    /// Returns a new rectangle with the same size and top-left, but with a different anchor alignment.
    /// </summary>
    /// <param name="alignment">The anchor point for alignment.</param>
    /// <returns>A new rectangle with the specified alignment.</returns>
    public Rect Align(AnchorPoint alignment)
    {
        return new(TopLeft, Size, alignment);
    }

    /// <summary>
    /// Gets the anchor point (relative 0-1) for a given absolute point within the rectangle.
    /// </summary>
    /// <param name="point">The absolute point to convert to an anchor point.</param>
    /// <returns>The anchor point corresponding to the given absolute point.</returns>
    public AnchorPoint GetAnchorPoint(Vector2 point)
    {
        var dif = point - TopLeft;
        var f = dif / Size;
        return new AnchorPoint(f);
    }

    /// <summary>
    /// Gets the absolute point in the rectangle for a given anchor point (relative 0-1).
    /// </summary>
    /// <param name="anchor">The anchor point (relative 0-1).</param>
    /// <returns>The absolute point in the rectangle.</returns>
    public Vector2 GetAbsolutePoint(AnchorPoint anchor)
    {
        return Size.ToVector2() * anchor;
    }

    /// <summary>
    /// Converts a point in absolute coordinates to a relative (0-1) position within the rectangle.
    /// </summary>
    /// <param name="p">The absolute point to convert.</param>
    /// <returns>The relative (0-1) position within the rectangle.</returns>
    public Vector2 PointToRelative(Vector2 p)
    {
        var dif = p - TopLeft;
        var intensity = dif / Size;

        float xFactor = intensity.X < 0f ? 0f : intensity.X > 1f ? 1f : intensity.X;
        float yFactor = intensity.Y < 0f ? 0f : intensity.Y > 1f ? 1f : intensity.Y;
        return new(xFactor, yFactor);
    }

    /// <summary>
    /// Converts a relative (0-1) position to an absolute point within the rectangle.
    /// </summary>
    /// <param name="relativePoint">The relative (0-1) position.</param>
    /// <returns>The absolute point in the rectangle.</returns>
    public Vector2 PointToAbsolute(Vector2 relativePoint)
    {
        return relativePoint * Size;
    }

    /// <summary>
    /// Returns a value between 0 and 1 for the x axis based on where the value is within the rectangle's width.
    /// </summary>
    /// <param name="x">The x coordinate to evaluate.</param>
    /// <returns>The normalized position (0-1) along the width.</returns>
    public float GetWidthFactor(float x)
    {
        float dif = x - Left;
        float intensity = dif / Width;
        return intensity < 0f ? 0f : intensity > 1f ? 1f : intensity;
    }

    /// <summary>
    /// Returns a value between 0 and 1 for the y axis based on where the value is within the rectangle's height.
    /// </summary>
    /// <param name="y">The y coordinate to evaluate.</param>
    /// <returns>The normalized position (0-1) along the height.</returns>
    public float GetHeightFactor(float y)
    {
        float dif = y - Top;
        float intensity = dif / Height;
        return intensity < 0f ? 0f : intensity > 1f ? 1f : intensity;
    }

    /// <summary>
    /// Returns a new rectangle enlarged to include the specified point.
    /// </summary>
    /// <param name="p">The point to include.</param>
    /// <returns>A new rectangle that includes the point.</returns>
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

    /// <summary>
    /// Clamps a point to be within the bounds of the rectangle.
    /// </summary>
    /// <param name="p">The point to clamp.</param>
    /// <returns>The clamped point.</returns>
    public Vector2 ClampOnRect(Vector2 p)
    {
        return new
        (
            ShapeMath.Clamp(p.X, X, X + Width),
            ShapeMath.Clamp(p.Y, Y, Y + Height)
        );
    }

    /// <summary>
    /// Clamps this rectangle to be within the bounds of another rectangle.
    /// </summary>
    /// <param name="bounds">The bounding rectangle.</param>
    /// <returns>A new rectangle clamped to the bounds.</returns>
    public Rect Clamp(Rect bounds)
    {
        var tl = bounds.ClampOnRect(TopLeft);
        var br = bounds.ClampOnRect(BottomRight);
        return new(tl, br);
    }

    /// <summary>
    /// Clamps this rectangle to be within the bounds defined by two points.
    /// </summary>
    /// <param name="min">The minimum point (top-left).</param>
    /// <param name="max">The maximum point (bottom-right).</param>
    /// <returns>A new rectangle clamped to the bounds.</returns>
    public Rect Clamp(Vector2 min, Vector2 max)
    {
        return Clamp(new Rect(min, max));
    }

    
    #endregion
}