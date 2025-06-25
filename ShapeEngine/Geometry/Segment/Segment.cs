using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.Structs;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;
using ShapeEngine.StaticLib.Drawing;

namespace ShapeEngine.Geometry.Segment;


public readonly partial struct Segment : IEquatable<Segment>
{
    #region Members
    public readonly Vector2 Start;
    public readonly Vector2 End;
    public readonly Vector2 Normal;
    #endregion

    #region Getter Setter
    public Vector2 Center => (Start + End) * 0.5f;
    public Vector2 Dir => Displacement.Normalize();
    public Vector2 Displacement => End - Start;
    public float Length => Displacement.Length();
    public float LengthSquared => Displacement.LengthSquared();

    #endregion

    #region Constructor
    public Segment(Vector2 start, Vector2 end, bool flippedNormal = false) 
    { 
        this.Start = start; 
        this.End = end;
        this.Normal = GetNormal(start, end, flippedNormal);
        // this.flippedNormals = flippedNormals;
    }
    internal Segment(Vector2 start, Vector2 end, Vector2 normal) 
    { 
        this.Start = start; 
        this.End = end;
        this.Normal = normal;
    }
    public Segment(float startX, float startY, float endX, float endY, bool flippedNormal = false) 
    { 
        this.Start = new(startX, startY); 
        this.End = new(endX, endY);
        this.Normal = GetNormal(Start, End, flippedNormal);
        // this.flippedNormals = flippedNormals;
    }

    public Segment(Vector2 origin, float length, float rotRad, float originOffset = 0.5f, bool flippedNormal = false)
    {
        var dir = ShapeVec.VecFromAngleRad(rotRad);
        this.Start = origin - dir * originOffset * length;
        this.End = origin + dir * (1f - originOffset) * length;
        this.Normal = GetNormal(Start, End, flippedNormal);
    }
    #endregion
    
    #region Shapes
    public Rect.Rect GetBoundingBox() { return new(Start, End); }

    public Polyline.Polyline ToPolyline() { return new Polyline.Polyline() {Start, End}; }
    public Segments GetEdges() { return new Segments(){this}; }
    public Points Inflate(float thickness, float alignement = 0.5f)
    {
        var dir = Dir;
        var left = dir.GetPerpendicularLeft();
        var right = dir.GetPerpendicularRight();
        var a = Start + left * thickness * alignement;
        var b = Start + right * thickness * (1 - alignement);
        var c = End + right * thickness * (1 - alignement);
        var d = End + left * thickness * alignement;

        return new() { a, b, c, d };
    }

    public Segments Split(float f)
    {
        return Split(this.GetPoint(f));
    }
    public Segments Split(Vector2 splitPoint)
    {
        var a = new Segment(Start, splitPoint);
        var b = new Segment(splitPoint, End);
        return new() { a, b };
    }


    #endregion

    #region Point & Vertext

    public Vector2 GetPoint(float f) { return Start.Lerp(End, f); }
    public Points GetVertices()
    {
        var points = new Points
        {
            Start,
            End
        };
        return points;
    }

    public Vector2 GetRandomPoint() { return this.GetPoint(Rng.Instance.RandF()); }
    public Points GetRandomPoints(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPoint());
        }
        return points;
    }
    public Vector2 GetRandomVertex() { return Rng.Instance.Chance(0.5f) ? Start : End; }


    #endregion

    #region Static
    public static Vector2 GetNormal(Vector2 start, Vector2 end, bool flippedNormal)
    {
        if (flippedNormal) return (end - start).GetPerpendicularLeft().Normalize();
        else return (end - start).GetPerpendicularRight().Normalize();
    }
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
    
    public static ValueRange ProjectSegment(Vector2 aPos, Vector2 aEnd, Vector2 onto)
    {
        var unitOnto = onto.Normalize();
        ValueRange r = new(unitOnto.Dot(aPos), unitOnto.Dot(aEnd));
        return r;
    }
    public static bool SegmentOnOneSide(Vector2 axisPos, Vector2 axisDir, Vector2 segmentStart, Vector2 segmentEnd)
    {
        var d1 = segmentStart - axisPos;
        var d2 = segmentEnd - axisPos;
        var n = axisDir.Rotate90CCW();// new(-axisDir.Y, axisDir.X);
        return Vector2.Dot(n, d1) * Vector2.Dot(n, d2) > 0.0f;
    }

    #endregion

    #region Operators

    public static Segment operator +(Segment left, Segment right) => new(left.Start + right.Start, left.End + right.End);
    public static Segment operator -(Segment left, Segment right) => new(left.Start - right.Start, left.End - right.End);
    public static Segment operator *(Segment left, Segment right) => new(left.Start * right.Start, left.End * right.End);
    public static Segment operator /(Segment left, Segment right) => new(left.Start / right.Start, left.End / right.End);
    
    public static Segment operator +(Segment left, Vector2 right) => new(left.Start + right, left.End + right);
    public static Segment operator -(Segment left, Vector2 right) => new(left.Start - right, left.End - right);
    public static Segment operator *(Segment left, Vector2 right) => new(left.Start * right, left.End * right);
    public static Segment operator /(Segment left, Vector2 right) => new(left.Start / right, left.End / right);

    public static Segment operator +(Segment left, float right) => new(left.Start + new Vector2(right), left.End + new Vector2(right));
    public static Segment operator -(Segment left, float right) => new(left.Start - new Vector2(right), left.End - new Vector2(right));
    public static Segment operator *(Segment left, float right) => new(left.Start * right, left.End * right);
    public static Segment operator /(Segment left, float right) => new(left.Start / right, left.End / right);
    
    // public static Segment operator +(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.End + left.Dir * right);
    // }
    // public static Segment operator -(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.End - left.Dir * right);
    // }
    // public static Segment operator *(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.Start + left.Displacement * right);
    // }
    // public static Segment operator /(Segment left, float right)
    // {
    //     return right == 0 ? left : new(left.Start, left.Start + left.Displacement / right);
    // }
    //
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
        //return (Start == other.Start && End == other.End) || (Start == other.End && End == other.Start);
    }
    
    /// <summary>
    /// Checks the equality of 2 segments with the direction.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Segment other)
    {
        return Start.IsSimilar(other.Start) && End.IsSimilar(other.End);// Start == other.Start && End == other.End;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }
    public static bool operator ==(Segment left, Segment right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Segment left, Segment right)
    {
        return !(left == right);
    }
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Segment s) return Equals(s);
        return false;
    }
    #endregion

    #region Lightning

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
    
    public void DrawNormal(float lineThickness, float length, ColorRgba colorRgba)
    {
        Segment n = new(Center, Center + Normal * length);
        n.Draw(lineThickness, colorRgba);
    }
}