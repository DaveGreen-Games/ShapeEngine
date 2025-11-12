using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.SegmentDef;

/// <summary>
/// Represents a 2D line segment defined by a start and end point, with a calculated normal.
/// </summary>
public readonly partial struct Segment : IEquatable<Segment>, IShapeTypeProvider
{
    #region Members
    /// <summary>
    /// The start point of the segment.
    /// </summary>
    public readonly Vector2 Start;
    /// <summary>
    /// The end point of the segment.
    /// </summary>
    public readonly Vector2 End;
    /// <summary>
    /// The normal vector of the segment, perpendicular to the direction from Start to End.
    /// </summary>
    public readonly Vector2 Normal;
    #endregion

    #region Getter Setter
    /// <summary>
    /// Gets the center point of the segment.
    /// </summary>
    public Vector2 Center => (Start + End) * 0.5f;
    /// <summary>
    /// Gets the normalized direction vector from Start to End.
    /// </summary>
    public Vector2 Dir => Displacement.Normalize();
    /// <summary>
    /// Gets the displacement vector from Start to End.
    /// </summary>
    public Vector2 Displacement => End - Start;
    /// <summary>
    /// Gets the length of the segment.
    /// </summary>
    public float Length => Displacement.Length();
    /// <summary>
    /// Gets the squared length of the segment. Useful for performance when comparing lengths.
    /// </summary>
    public float LengthSquared => Displacement.LengthSquared();

    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct with specified start and end points.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <param name="flippedNormal">If true, the normal is flipped to the opposite direction. Default is false.</param>
    /// <remarks>Normal is calculated based on the direction from start to end.</remarks>
    public Segment(Vector2 start, Vector2 end, bool flippedNormal = false) 
    { 
        this.Start = start; 
        this.End = end;
        this.Normal = GetNormal(start, end, flippedNormal);
        // this.flippedNormals = flippedNormals;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct with specified start, end, and normal vectors.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <param name="normal">The normal vector to use for the segment.</param>
    /// <remarks>This constructor is internal and allows explicit normal assignment.</remarks>
    internal Segment(Vector2 start, Vector2 end, Vector2 normal) 
    { 
        this.Start = start; 
        this.End = end;
        this.Normal = normal;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct with specified coordinates.
    /// </summary>
    /// <param name="startX">The X coordinate of the start point.</param>
    /// <param name="startY">The Y coordinate of the start point.</param>
    /// <param name="endX">The X coordinate of the end point.</param>
    /// <param name="endY">The Y coordinate of the end point.</param>
    /// <param name="flippedNormal">If true, the normal is flipped to the opposite direction. Default is false.</param>
    /// <remarks>Normal is calculated based on the direction from start to end.</remarks>
    public Segment(float startX, float startY, float endX, float endY, bool flippedNormal = false) 
    { 
        this.Start = new(startX, startY); 
        this.End = new(endX, endY);
        this.Normal = GetNormal(Start, End, flippedNormal);
        // this.flippedNormals = flippedNormals;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Segment"/> struct from an origin, length, and rotation.
    /// </summary>
    /// <param name="origin">The origin point of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <param name="rotRad">The rotation angle of the segment in radians.</param>
    /// <param name="originOffset">The offset of the origin point, relative to the segment length. Default is 0.5 (middle of the segment).</param>
    /// <param name="flippedNormal">If true, the normal is flipped to the opposite direction. Default is false.</param>
    /// <remarks>Normal is calculated based on the direction from start to end.</remarks>
    public Segment(Vector2 origin, float length, float rotRad, float originOffset = 0.5f, bool flippedNormal = false)
    {
        var dir = ShapeVec.VecFromAngleRad(rotRad);
        this.Start = origin - dir * originOffset * length;
        this.End = origin + dir * (1f - originOffset) * length;
        this.Normal = GetNormal(Start, End, flippedNormal);
    }
    #endregion
    
    #region Shapes
    /// <summary>
    /// Gets the bounding box of the segment as a rectangle.
    /// </summary>
    /// <returns>A <see cref="Rect"/> representing the bounding box of the segment.</returns>
    public Rect GetBoundingBox() { return new(Start, End); }

    /// <summary>
    /// Converts the segment to a polyline (a series of connected lines).
    /// </summary>
    /// <returns>A <see cref="Polyline"/> representing the segment.</returns>
    public Polyline ToPolyline() { return new Polyline() {Start, End}; }
    /// <summary>
    /// Gets the segment itself as a collection of edges.
    /// </summary>
    /// <returns>A <see cref="Segments"/> collection containing the segment as the only edge.</returns>
    public Segments GetEdges() { return new Segments(){this}; }
    /// <summary>
    /// Inflates the segment, creating a set of points representing a thicker line.
    /// </summary>
    /// <param name="thickness">The thickness of the inflated segment.</param>
    /// <param name="alignment">The alignment of the inflation, from 0 (start) to 1 (end). Default is 0.5 (center).</param>
    /// <returns>A <see cref="Points"/> collection representing the inflated segment.</returns>
    public Points Inflate(float thickness, float alignment = 0.5f)
    {
        var dir = Dir;
        var left = dir.GetPerpendicularLeft();
        var right = dir.GetPerpendicularRight();
        var a = Start + left * thickness * alignment;
        var b = Start + right * thickness * (1 - alignment);
        var c = End + right * thickness * (1 - alignment);
        var d = End + left * thickness * alignment;

        return new() { a, b, c, d };
    }

    /// <summary>
    /// Splits the segment at a given parameter value, creating two new segments.
    /// </summary>
    /// <param name="f">The parameter value at which to split the segment. Typically between 0 and 1.</param>
    /// <returns>A <see cref="Segments"/> collection containing the two new segments.</returns>
    public Segments Split(float f)
    {
        return Split(this.GetPoint(f));
    }
    /// <summary>
    /// Splits the segment at a given point, creating two new segments.
    /// </summary>
    /// <param name="splitPoint">The point at which to split the segment.</param>
    /// <returns>A <see cref="Segments"/> collection containing the two new segments.</returns>
    public Segments Split(Vector2 splitPoint)
    {
        var a = new Segment(Start, splitPoint);
        var b = new Segment(splitPoint, End);
        return new() { a, b };
    }


    #endregion

    #region Point & Vertext

    /// <summary>
    /// Gets a point on the segment at a given parameter value.
    /// </summary>
    /// <param name="f">The parameter value, typically between 0 and 1.</param>
    /// <returns>The point on the segment at the given parameter value.</returns>
    public Vector2 GetPoint(float f) { return Start.Lerp(End, f); }
    /// <summary>
    /// Gets the start and end points of the segment as a collection of vertices.
    /// </summary>
    /// <returns>A <see cref="Points"/> collection containing the start and end points.</returns>
    public Points GetVertices()
    {
        var points = new Points
        {
            Start,
            End
        };
        return points;
    }

    /// <summary>
    /// Gets a random point on the segment.
    /// </summary>
    /// <returns>A random point on the segment.</returns>
    public Vector2 GetRandomPoint() { return this.GetPoint(Rng.Instance.RandF()); }
    /// <summary>
    /// Gets a collection of random points on the segment.
    /// </summary>
    /// <param name="amount">The number of random points to generate.</param>
    /// <returns>A <see cref="Points"/> collection containing the random points.</returns>
    public Points GetRandomPoints(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPoint());
        }
        return points;
    }
    /// <summary>
    /// Gets a random endpoint of the segment (either start or end).
    /// </summary>
    /// <returns>A random vertex, either the start or end point of the segment.</returns>
    public Vector2 GetRandomVertex() { return Rng.Instance.Chance(0.5f) ? Start : End; }


    #endregion

    #region Static
    /// <summary>
    /// Calculates the normal vector for a segment defined by two points.
    /// </summary>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <param name="flippedNormal">If true, the normal is flipped to the opposite direction.</param>
    /// <returns>The normal vector of the segment.</returns>
    public static Vector2 GetNormal(Vector2 start, Vector2 end, bool flippedNormal)
    {
        if (flippedNormal) return (end - start).GetPerpendicularLeft().Normalize();
        else return (end - start).GetPerpendicularRight().Normalize();
    }
    /// <summary>
    /// Checks if a point is located on a segment.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="start">The start point of the segment.</param>
    /// <param name="end">The end point of the segment.</param>
    /// <returns>True if the point is on the segment, false otherwise.</returns>
    public static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        float minX = Math.Min(start.X, end.X);
        float maxX = Math.Max(start.X, end.X);
        float minY = Math.Min(start.Y, end.Y);
        float maxY = Math.Max(start.Y, end.Y);

        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
        
        //old version
        // var d = end - start;
        // var lp = point - start;
        // var p = lp.Project(d);
        // return lp.IsSimilar(p) && p.LengthSquared() <= d.LengthSquared() && Vector2.Dot(p, d) >= 0.0f;
    }
    
    /// <summary>
    /// Projects a segment onto a given direction, returning the range of the projection.
    /// </summary>
    /// <param name="aPos">The start point of the segment.</param>
    /// <param name="aEnd">The end point of the segment.</param>
    /// <param name="onto">The direction vector to project onto.</param>
    /// <returns>A <see cref="ValueRange"/> representing the range of the projection.</returns>
    public static ValueRange ProjectSegment(Vector2 aPos, Vector2 aEnd, Vector2 onto)
    {
        var unitOnto = onto.Normalize();
        ValueRange r = new(unitOnto.Dot(aPos), unitOnto.Dot(aEnd));
        return r;
    }
    /// <summary>
    /// Checks if both points of a segment are on the same side of a given axis.
    /// </summary>
    /// <param name="axisPos">A point on the axis.</param>
    /// <param name="axisDir">The direction of the axis.</param>
    /// <param name="segmentStart">The start point of the segment.</param>
    /// <param name="segmentEnd">The end point of the segment.</param>
    /// <returns>True if the segment is on one side of the axis, false otherwise.</returns>
    public static bool SegmentOnOneSide(Vector2 axisPos, Vector2 axisDir, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var d1 = segmentStart - axisPos;
        var d2 = segmentEnd - axisPos;
        var n = axisDir.Rotate90CCW();// new(-axisDir.Y, axisDir.X);
        return Vector2.Dot(n, d1) * Vector2.Dot(n, d2) > 0.0f;
    }

    #endregion

    #region Operators

    /// <summary>
    /// Adds the start and end points of two segments.
    /// </summary>
    /// <param name="left">The first segment.</param>
    /// <param name="right">The second segment.</param>
    /// <returns>A new segment with start and end points added component-wise.</returns>
    public static Segment operator +(Segment left, Segment right) => new(left.Start + right.Start, left.End + right.End);
    /// <summary>
    /// Subtracts the start and end points of the right segment from the left segment.
    /// </summary>
    /// <param name="left">The first segment.</param>
    /// <param name="right">The second segment.</param>
    /// <returns>A new segment with start and end points subtracted component-wise.</returns>
    public static Segment operator -(Segment left, Segment right) => new(left.Start - right.Start, left.End - right.End);
    /// <summary>
    /// Multiplies the start and end points of two segments component-wise.
    /// </summary>
    /// <param name="left">The first segment.</param>
    /// <param name="right">The second segment.</param>
    /// <returns>A new segment with start and end points multiplied component-wise.</returns>
    public static Segment operator *(Segment left, Segment right) => new(left.Start * right.Start, left.End * right.End);
    /// <summary>
    /// Divides the start and end points of the left segment by the right segment component-wise.
    /// </summary>
    /// <param name="left">The first segment.</param>
    /// <param name="right">The second segment.</param>
    /// <returns>A new segment with start and end points divided component-wise.</returns>
    public static Segment operator /(Segment left, Segment right) => new(left.Start / right.Start, left.End / right.End);
    /// <summary>
    /// Adds a vector to the start and end points of a segment.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The vector to add.</param>
    /// <returns>A new segment with the vector added to both start and end points.</returns>
    public static Segment operator +(Segment left, Vector2 right) => new(left.Start + right, left.End + right);
    /// <summary>
    /// Subtracts a vector from the start and end points of a segment.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The vector to subtract.</param>
    /// <returns>A new segment with the vector subtracted from both start and end points.</returns>
    public static Segment operator -(Segment left, Vector2 right) => new(left.Start - right, left.End - right);
    /// <summary>
    /// Multiplies the start and end points of a segment by a vector component-wise.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The vector to multiply by.</param>
    /// <returns>A new segment with start and end points multiplied by the vector.</returns>
    public static Segment operator *(Segment left, Vector2 right) => new(left.Start * right, left.End * right);
    /// <summary>
    /// Divides the start and end points of a segment by a vector component-wise.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The vector to divide by.</param>
    /// <returns>A new segment with start and end points divided by the vector.</returns>
    public static Segment operator /(Segment left, Vector2 right) => new(left.Start / right, left.End / right);
    /// <summary>
    /// Adds a scalar value to the start and end points of a segment.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The scalar value to add.</param>
    /// <returns>A new segment with the scalar added to both start and end points.</returns>
    public static Segment operator +(Segment left, float right) => new(left.Start + new Vector2(right), left.End + new Vector2(right));
    /// <summary>
    /// Subtracts a scalar value from the start and end points of a segment.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The scalar value to subtract.</param>
    /// <returns>A new segment with the scalar subtracted from both start and end points.</returns>
    public static Segment operator -(Segment left, float right) => new(left.Start - new Vector2(right), left.End - new Vector2(right));
    /// <summary>
    /// Multiplies the start and end points of a segment by a scalar value.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The scalar value to multiply by.</param>
    /// <returns>A new segment with start and end points multiplied by the scalar.</returns>
    public static Segment operator *(Segment left, float right) => new(left.Start * right, left.End * right);
    /// <summary>
    /// Divides the start and end points of a segment by a scalar value.
    /// </summary>
    /// <param name="left">The segment.</param>
    /// <param name="right">The scalar value to divide by.</param>
    /// <returns>A new segment with start and end points divided by the scalar.</returns>
    public static Segment operator /(Segment left, float right) => new(left.Start / right, left.End / right);
    #endregion
    
    #region Equality & HashCode
    /// <summary>
    /// Checks the equality of 2 segments without the direction.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsSimilar(Segment other)
    {
        return 
            (Start.IsSimilar(other.Start) && End.IsSimilar(other.End)) ||
            (Start.IsSimilar(other.End) && End.IsSimilar(other.Start));
    }
    
    /// <summary>
    /// Checks the equality of 2 segments with the direction.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Segment other)
    {
        return Start.IsSimilar(other.Start) && End.IsSimilar(other.End);
    }
    /// <summary>
    /// Returns the hash code for this segment.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public ShapeType GetShapeType() => ShapeType.Segment;

    /// <summary>
    /// Determines whether two segments are equal.
    /// </summary>
    /// <param name="left">The first segment to compare.</param>
    /// <param name="right">The second segment to compare.</param>
    /// <returns>True if the segments are equal; otherwise, false.</returns>
    public static bool operator ==(Segment left, Segment right)
    {
        return left.Equals(right);
    }
    /// <summary>
    /// Determines whether two segments are not equal.
    /// </summary>
    /// <param name="left">The first segment to compare.</param>
    /// <param name="right">The second segment to compare.</param>
    /// <returns>True if the segments are not equal; otherwise, false.</returns>
    public static bool operator !=(Segment left, Segment right)
    {
        return !(left == right);
    }
    /// <summary>
    /// Determines whether the specified object is equal to the current segment.
    /// </summary>
    /// <param name="obj">The object to compare with the current segment.</param>
    /// <returns>True if the specified object is a Segment and is equal to the current segment; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Segment s) return Equals(s);
        return false;
    }
    #endregion

    #region Lightning

    /// <summary>
    /// Creates lightning-like segments between the start and end points.
    /// </summary>
    /// <param name="segments">The number of segments to create. Default is 10.</param>
    /// <param name="maxSway">The maximum sway (random displacement) of the segments. Default is 80.</param>
    /// <returns>A <see cref="Segments"/> collection representing the lightning segments.</returns>
    public Segments CreateLightningSegments(int segments = 10, float maxSway = 80f)
    {
        Segments result = new();
        var w = End - Start;
        var dir = w.Normalize();
        var n = new Vector2(dir.Y, -dir.X);
        float length = w.Length();

        float prevDisplacement = 0;
        var cur = Start;
        //result.Add(start);

        float segmentLength = length / segments;
        float remainingLength = length;
        List<Vector2> accumulator = new()
        {
            Start
        };
        while (remainingLength > 0f)
        {
            float randSegmentLength = Rng.Instance.RandF() * segmentLength;
            remainingLength -= randSegmentLength;
            if (remainingLength <= 0f)
            {
                if(accumulator.Count == 1)
                {
                    result.Add(new(accumulator[0], End));
                }
                else
                {
                    result.Add(new(result[result.Count - 1].End, End));
                }
                break;
            }
            float scale = randSegmentLength / segmentLength;
            float displacement = Rng.Instance.RandF(-maxSway, maxSway);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            cur = cur + dir * randSegmentLength;
            var p = cur + displacement * n;
            accumulator.Add(p);
            if(accumulator.Count == 2)
            {
                result.Add(new(accumulator[0], accumulator[1]));
                accumulator.Clear();
            }
            prevDisplacement = displacement;
        }
        return result;
    }
    /// <summary>
    /// Creates lightning-like segments between the start and end points, with a fixed segment length.
    /// </summary>
    /// <param name="segmentLength">The length of each segment. Default is 5.</param>
    /// <param name="maxSway">The maximum sway (random displacement) of the segments. Default is 80.</param>
    /// <returns>A <see cref="Segments"/> collection representing the lightning segments.</returns>
    public Segments CreateLightningSegments(float segmentLength = 5f, float maxSway = 80f)
    {
        Segments result = new();
        var w = End - Start;
        var dir = w.Normalize();
        var n = new Vector2(dir.Y, -dir.X);
        float length = w.Length();

        float prevDisplacement = 0;
        var cur = Start;
        List<Vector2> accumulator = new()
        {
            Start
        };
        float remainingLength = length;
        while (remainingLength > 0f)
        {
            float randSegmentLength = Rng.Instance.RandF() * segmentLength;
            remainingLength -= randSegmentLength;
            if (remainingLength <= 0f)
            {
                if (accumulator.Count == 1)
                {
                    result.Add(new(accumulator[0], End));
                }
                else
                {
                    result.Add(new(result[result.Count - 1].End, End));
                }
                break;
            }
            float scale = randSegmentLength / segmentLength;
            float displacement = Rng.Instance.RandF(-maxSway, maxSway);
            displacement -= (displacement - prevDisplacement) * (1 - scale);
            cur = cur + dir * randSegmentLength;
            var p = cur + displacement * n;
            accumulator.Add(p);
            if (accumulator.Count == 2)
            {
                result.Add(new(accumulator[0], accumulator[1]));
                accumulator.Clear();
            }
            prevDisplacement = displacement;
        }
        return result;
    }
    

    #endregion
    
    /// <summary>
    /// Draws the normal vector of the segment for visualization.
    /// </summary>
    /// <param name="lineThickness">The thickness of the normal line.</param>
    /// <param name="length">The length of the normal line.</param>
    /// <param name="colorRgba">The color of the normal line.</param>
    public void DrawNormal(float lineThickness, float length, ColorRgba colorRgba)
    {
        Segment n = new(Center, Center + Normal * length);
        n.Draw(lineThickness, colorRgba);
    }
}