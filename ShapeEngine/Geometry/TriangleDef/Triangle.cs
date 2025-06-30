using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.PointsDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.SegmentsDef;
using ShapeEngine.Geometry.TriangulationDef;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.TriangleDef;

/// <summary>
/// Class that represents a triangle by holding three points. Points a, b, c should be in ccw order!
/// </summary>
public readonly partial struct Triangle : IEquatable<Triangle>
{
    #region Members
    public readonly Vector2 A;
    public readonly Vector2 B;
    public readonly Vector2 C;
    #endregion

    #region Getter Setter
    public readonly Vector2 SideA => B - A;
    public readonly Vector2 SideB => C - B;
    public readonly Vector2 SideC => A - C;
    public readonly Segment SegmentAToB => new(A, B);
    public readonly Segment SegmentBToC => new(B, C);
    public readonly Segment SegmentCToA => new(C, A);

    #endregion

    #region Constructors
    /// <summary>
    /// Points should be in ccw order!
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    public Triangle(Vector2 a, Vector2 b, Vector2 c) 
    { 
        this.A = a; 
        this.B = b; 
        this.C = c;
    }
    public Triangle(Vector2 p, Segment s)
    {
        var w = s.Displacement;
        var v = p - s.Start;
        float cross = w.Cross(v);
        if(cross <= 0f)
        {
            A = s.Start;
            B = s.End;
            C = p;
        }
        else
        {
            A = s.End;
            B = s.Start;
            C = p;
        }
    }
    #endregion
    
    #region Shapes

    public Rect GetBoundingBox() { return new Rect(A.X, A.Y, 0, 0).Enlarge(B).Enlarge(C); }
    public Circle GetCircumCircle()
    {
        //alternative variant
        //coding train
        //https://editor.p5js.org/codingtrain/sketches/eJnSI84Tw
        // var ab = (B - A).Rotate(ShapeMath.PI / 2);
        // var ac = (C - A).Rotate(ShapeMath.PI / 2);
        // var abMid = (A + B) / 2;
        // var acMid = (A + C) / 2;
        // // Find the intersection between the two perpendicular bisectors
        // var numerator = ac.X * (abMid.Y - acMid.Y) - ac.Y * (abMid.X - acMid.X);
        // var denominator = ac.Y * ab.X - ac.X * ab.Y;
        // ab *= (numerator / denominator);
        // var center = abMid + ab;
        // var r = (C - center).Length();
        
        var SqrA = new Vector2(A.X * A.X, A.Y * A.Y);
        var SqrB = new Vector2(B.X * B.X, B.Y * B.Y); 
        var SqrC = new Vector2(C.X * C.X, C.Y * C.Y);
        
        float D = (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) * 2f;
        float x = ((SqrA.X + SqrA.Y) * (B.Y - C.Y) + (SqrB.X + SqrB.Y) * (C.Y - A.Y) + (SqrC.X + SqrC.Y) * (A.Y - B.Y)) / D;
        float y = ((SqrA.X + SqrA.Y) * (C.X - B.X) + (SqrB.X + SqrB.Y) * (A.X - C.X) + (SqrC.X + SqrC.Y) * (B.X - A.X)) / D;
        
        var center = new Vector2(x, y);
        float r = (A - center).Length();
        
        return new(center, r);
    }
    
    public Points ToPoints() => new() {A, B, C};
    public Polygon ToPolygon() => new() {A, B, C};
    public Polyline ToPolyline() => new() { A, B, C };
    public Segments GetEdges() => new() { SegmentAToB, SegmentBToC, SegmentCToA };
    
    /// <summary>
    /// Construct an adjacent triangle on the closest side to the point p. If p is inside the triangle, the triangle is returned.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Triangle ConstructAdjacentTriangle(Vector2 p)
    {
        if(ContainsPoint(p)) return this;

        var closest = GetClosestSegment(p, out float disSquared);
        return new Triangle(p, closest.segment);
    }

    public Triangulation Triangulate() => this.Triangulate(GetCentroid());

    public Triangulation Triangulate(int pointCount)
    {
        if (pointCount < 0) return new() { new(A, B, C) };

        Points points = new() { A, B, C };

        for (int i = 0; i < pointCount; i++)
        {
            float f1 = Rng.Instance.RandF();
            float f2 = Rng.Instance.RandF();
            Vector2 randPoint = GetPoint(f1, f2);
            points.Add(randPoint);
        }

        return Polygon.TriangulateDelaunay(points);
    }
    public Triangulation Triangulate(float minArea)
    {
        if (minArea <= 0) return new() { new(A,B,C) };

        float triArea = GetArea();
        float pieceCount = triArea / minArea;
        int points = (int)MathF.Floor((pieceCount - 1f) * 0.5f);
        return Triangulate(points);
    }
    public Triangulation Triangulate(Vector2 p)
    {
        return new()
        {
            new(A, B, p),
            new(B, C, p),
            new(C, A, p)
        };
    }
    
    public Triangle GetInsideTriangle(float abF, float bcF, float caF)
    {
        Vector2 newA = ShapeVec.Lerp(A, B, abF);
        Vector2 newB = ShapeVec.Lerp(B, C, bcF);
        Vector2 newC = ShapeVec.Lerp(C, A, caF);
        return new(newA, newB, newC);
    }


    #endregion
    
    #region Points & Vertex
    /// <summary>
    /// Returns a point inside the triangle.
    /// </summary>
    /// <param name="f1">First value in the range 0 - 1.</param>
    /// <param name="f2">Second value in the range 0 - 1.</param>
    /// <returns></returns>

    public Vector2 GetPoint(float f1, float f2)
    {
        if ((f1 + f2) > 1)
        {
            f1 = 1f - f1;
            f2 = 1f - f2;
        }
        Vector2 ac = (C - A) * f1;
        Vector2 ab = (B - A) * f2;
        return A + ac + ab;
        //float f1Sq = MathF.Sqrt(f1);
        //float x = (1f - f1Sq) * t.a.X + (f1Sq * (1f - f2)) * t.b.X + (f1Sq * f2) * t.c.X;
        //float y = (1f - f1Sq) * t.a.Y + (f1Sq * (1f - f2)) * t.b.Y + (f1Sq * f2) * t.c.Y;
        //return new(x, y);
    }

    public Vector2 GetRandomPointInside() => this.GetPoint(Rng.Instance.RandF(), Rng.Instance.RandF());
    public Points GetRandomPointsInside(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointInside());
        }
        return points;
    }
    public Vector2 GetRandomVertex()
    {
        var randIndex = Rng.Instance.RandI(0, 2);
        if (randIndex == 0) return A;
        else if (randIndex == 1) return B;
        else return C;
    }
    public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    public Points GetRandomPointsOnEdge(int amount) => GetEdges().GetRandomPoints(amount);

    public Segment GetSegment(int index)
    {
        if (index < 0) return new Segment();
        var i = index % 3;
        if(i == 0) return SegmentAToB;
        if(i == 1) return SegmentBToC;
        return SegmentCToA;
    }
    #endregion
    
    #region Equality & HashCode
    public bool SharesVertex(Vector2 p) { return A == p || B == p || C == p; }
    public bool SharesVertex(IEnumerable<Vector2> points)
    {
        foreach (var p in points)
        {
            if (SharesVertex(p)) return true;
        }
        return false;
    }
    public bool SharesVertex(Triangle t) { return SharesVertex(t.A) || SharesVertex(t.B) || SharesVertex(t.C); }

    public bool IsSimilar(Triangle other)
    {
        return 
            (A.IsSimilar(A) && B.IsSimilar(B) && C.IsSimilar(C) ) || 
            (C.IsSimilar(A) && A.IsSimilar(B) && B.IsSimilar(C) ) || 
            (B.IsSimilar(A) && C.IsSimilar(B) && A.IsSimilar(C) ) ||
            (B.IsSimilar(A) && A.IsSimilar(B) && C.IsSimilar(C) ) ||
            (C.IsSimilar(A) && B.IsSimilar(B) && A.IsSimilar(C) ) ||
            (A.IsSimilar(A) && C.IsSimilar(B) && B.IsSimilar(C) );
        
        //return 
        //    (A == other.A && B == other.B && C == other.C) || 
        //    (C == other.A && A == other.B && B == other.C) || 
        //    (B == other.A && C == other.B && A == other.C) ||
        //    (B == other.A && A == other.B && C == other.C) ||
        //    (C == other.A && B == other.B && A == other.C) ||
        //    (A == other.A && C == other.B && B == other.C);
    }
    public bool Equals(Triangle other)
    {
        return A.IsSimilar(other.A) && B.IsSimilar(other.B) && C.IsSimilar(other.C);
    }
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(A, B, C);
    }
    public static bool operator ==(Triangle left, Triangle right)
    {
        return left.Equals(right);
    }
    public static bool operator !=(Triangle left, Triangle right)
    {
        return !(left == right);
    }
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj is Triangle t) return Equals(t);
        return false;
    }
    #endregion
    
    #region Operators

    public static Triangle operator +(Triangle left, Triangle right)
    {
        return new
        (
            left.A + right.A,
            left.B + right.B,
            left.C + right.C
        );
    }
    public static Triangle operator -(Triangle left, Triangle right)
    {
        return new
        (
            left.A - right.A,
            left.B - right.B,
            left.C - right.C
        );
    }
    public static Triangle operator *(Triangle left, Triangle right)
    {
        return new
        (
            left.A * right.A,
            left.B * right.B,
            left.C * right.C
        );
    }
    public static Triangle operator /(Triangle left, Triangle right)
    {
        return new
        (
            left.A / right.A,
            left.B / right.B,
            left.C / right.C
        );
    }
    public static Triangle operator +(Triangle left, Vector2 right)
    {
        return new
        (
            left.A + right,
            left.B + right,
            left.C + right
        );
    }
    public static Triangle operator -(Triangle left, Vector2 right)
    {
        return new
        (
            left.A - right,
            left.B - right,
            left.C - right
        );
    }
    public static Triangle operator *(Triangle left, Vector2 right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right
        );
    }
    public static Triangle operator /(Triangle left, Vector2 right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right
        );
    }
    public static Triangle operator +(Triangle left, float right)
    {
        return new
        (
            left.A + new Vector2(right),
            left.B + new Vector2(right),
            left.C + new Vector2(right)
        );
    }
    public static Triangle operator -(Triangle left, float right)
    {
        return new
        (
            left.A - new Vector2(right),
            left.B - new Vector2(right),
            left.C - new Vector2(right)
        );
    }
    public static Triangle operator *(Triangle left, float right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right
        );
    }
    public static Triangle operator /(Triangle left, float right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right
        );
    }
    
    #endregion
    
    #region Static

    public static float AreaSigned(Vector2 a, Vector2 b, Vector2 c) { return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X); }

    public static Triangle GenerateRelative(float minLength, float maxLength)
    {
        float angleStep = ShapeMath.PI * 2.0f / 3;

        var a = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 0) * Rng.Instance.RandF(minLength, maxLength);
        var b = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 1) * Rng.Instance.RandF(minLength, maxLength);
        var c = ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 2) * Rng.Instance.RandF(minLength, maxLength);
        
        return new(a, b, c);
    }
    
    public static Triangle Generate(Vector2 center, float minLength, float maxLength)
    {
        float angleStep = (ShapeMath.PI * 2.0f) / 3;

        var a = center + ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 0) * Rng.Instance.RandF(minLength, maxLength);
        var b = center + ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 1) * Rng.Instance.RandF(minLength, maxLength);
        var c = center + ShapeVec.Rotate(ShapeVec.Right(), -angleStep * 2) * Rng.Instance.RandF(minLength, maxLength);
        
        return new(a, b, c);
    }
    

    #endregion

    #region Interpolated Edge Points

    
    public Points? GetInterpolatedEdgePoints(float t)
    {
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(A, t);
        
        return new Points(3){a1, b1, c1};
    }
    public Points? GetInterpolatedEdgePoints(float t, int steps)
    {
        if(steps <= 1) return GetInterpolatedEdgePoints(t);
        
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(A, t);

        //first step is already done
        int remainingSteps = steps - 1;

        while (remainingSteps > 0)
        {
            var a2 = a1.Lerp(b1, t);
            var b2 = b1.Lerp(c1, t);
            var c2 = c1.Lerp(a1, t);
            
            (a1, b1, c1) = (a2, b2, c2);
            
            remainingSteps--;
        }
        
        return new Points(3){a1, b1, c1};
    }
    
    #endregion
}
