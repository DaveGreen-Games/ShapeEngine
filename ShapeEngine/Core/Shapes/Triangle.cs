using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes;

/// <summary>
/// Class that represents a triangle by holding three points. Points a, b, c should be in ccw order!
/// </summary>
public readonly struct Triangle : IEquatable<Triangle>
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

    #region Math
    
    public bool IsValid() { return GetArea() > 0f; }

    public Vector2 GetCentroid()  => (A + B + C) / 3;

    public Points? GetProjectedShapePoints(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A,
            B,
            C,
            A + v,
            B + v,
            C + v
        };
        return points;
    }
    public Polygon? ProjectShape(Vector2 v)
    {
        if (v.LengthSquared() <= 0f) return null;
        var points = new Points
        {
            A,
            B,
            C,
            A + v,
            B + v,
            C + v
        };
        return Polygon.FindConvexHull(points);
    }
    
    public Triangle Floor() { return new(A.Floor(), B.Floor(), C.Floor()); }
    public Triangle Ceiling() { return new(A.Ceiling(), B.Ceiling(), C.Ceiling()); }
    public Triangle Round() { return new(A.Round(), B.Round(), C.Round()); }
    public Triangle Truncate() { return new(A.Truncate(), B.Truncate(), C.Truncate()); }

    public float GetPerimeter() => SideA.Length() + SideB.Length() + SideC.Length();
    public float GetPerimeterSquared() => SideA.LengthSquared() + SideB.LengthSquared() + SideC.LengthSquared();
    public float GetArea() => MathF.Abs((A.X - C.X) * (B.Y - C.Y) - (A.Y - C.Y) * (B.X - C.X)) / 2f;

    public bool IsNarrow(float narrowValue = 0.2f)
    {
        var prev = C;
        var cur = A;
        var next = B;
        
        var nextToCur = (next - cur).Normalize();
        var prevToCur = (prev - cur).Normalize();
        float cross = nextToCur.Cross(prevToCur);
        if (MathF.Abs(cross) < narrowValue) return true;
        
        prev = A;
        cur = B;
        next = C;
        
        nextToCur = (next - cur).Normalize();
        prevToCur = (prev - cur).Normalize();
        cross = nextToCur.Cross(prevToCur);
        if (MathF.Abs(cross) < narrowValue) return true;
        
        prev = B;
        cur = C;
        next = A;
        
        nextToCur = (next - cur).Normalize();
        prevToCur = (prev - cur).Normalize();
        cross = nextToCur.Cross(prevToCur);
        return MathF.Abs(cross) < narrowValue;
        // Points points = new() { A, B, C };
        // for (int i = 0; i < 3; i++)
        // {
        //     var a = points[i];
        //     var b = Game.GetItem(points, i + 1);
        //     var c = Game.GetItem(points, i - 1);
        //
        //     var ba = (b - a).Normalize();
        //     var ca = (c - a).Normalize();
        //     float cross = ba.Cross(ca);
        //     if (MathF.Abs(cross) < narrowValue) return true;
        // }
        // return false;
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
    
    #region Transform
    public Triangle ChangeRotation(float rad) { return ChangeRotation(rad, GetCentroid()); }
    public Triangle ChangeRotation(float rad, Vector2 origin)
    {
        var newA = origin + (A - origin).Rotate(rad);
        var newB = origin + (B - origin).Rotate(rad);
        var newC = origin + (C - origin).Rotate(rad);
        return new(newA, newB, newC);
    }

    public Triangle SetRotation(float rad)
    {
        var origin = GetCentroid();
        var w = A - origin;
        var currentAngleRad = w.AngleRad();
        var amount = ShapeMath.GetShortestAngleRad(currentAngleRad, rad);
        return ChangeRotation(amount, origin);
    }
    public Triangle SetRotation(float rad, Vector2 origin)
    {
        var w = A - origin;
        if (w.LengthSquared() <= 0f)//origin is A
        {
            w = B - origin;
            if (w.LengthSquared() <= 0f)//origin is B
            {
                w = C - origin;
            }
        }

        var currentAngleRad = w.AngleRad();
        var amount = ShapeMath.GetShortestAngleRad(currentAngleRad, rad);
        return ChangeRotation(amount, origin);
    }
    
    public Triangle ScaleSize(float scale) => this * scale;
    public Triangle ScaleSize(Size scale) => new Triangle(A * scale, B * scale, C * scale);
    public Triangle ScaleSize(float scale, Vector2 origin)
    {
        var newA = origin + (A - origin) * scale;
        var newB = origin + (B - origin) * scale;
        var newC = origin + (C - origin) * scale;
        return new(newA, newB, newC);
    }
    public Triangle ScaleSize(Size scale, Vector2 origin)
    {
        var newA = origin + (A - origin) * scale;
        var newB = origin + (B - origin) * scale;
        var newC = origin + (C - origin) * scale;
        return new(newA, newB, newC);
    }

    public Triangle ChangeSize(float amount) => ChangeSize(amount, GetCentroid());
    public Triangle ChangeSize(float amount, Vector2 origin)
    {
        Vector2 newA, newB, newC;
        
        var wA = (A - origin);
        var lSqA = wA.LengthSquared();
        if (lSqA <= 0f) newA = A;
        else
        {
            var l = MathF.Sqrt(lSqA);
            var dir = wA / l;
            newA = origin + dir * (l + amount);
        }
        
        var wB = (B - origin);
        var lSqB = wB.LengthSquared();
        if (lSqB <= 0f) newB = B;
        else
        {
            var l = MathF.Sqrt(lSqB);
            var dir = wB / l;
            newB = origin + dir * (l + amount);
        }
       
        var wC = (C - origin);
        var lSqC = wC.LengthSquared();
        if (lSqC <= 0f) newC = C;
        else
        {
            var l = MathF.Sqrt(lSqC);
            var dir = wC / l;
            newC = origin + dir * (l + amount);
        }
        return new(newA, newB, newC);
    }

    public Triangle SetSize(float size) => SetSize(size, GetCentroid());
    public Triangle SetSize(float size, Vector2 origin)
    {
        Vector2 newA, newB, newC;
        
        var wA = (A - origin);
        var lSqA = wA.LengthSquared();
        if (lSqA <= 0f) newA = A;
        else
        {
            var l = MathF.Sqrt(lSqA);
            var dir = wA / l;
            newA = origin + dir * size;
        }
        
        var wB = (B - origin);
        var lSqB = wB.LengthSquared();
        if (lSqB <= 0f) newB = B;
        else
        {
            var l = MathF.Sqrt(lSqB);
            var dir = wB / l;
            newB = origin + dir * size;
        }
       
        var wC = (C - origin);
        var lSqC = wC.LengthSquared();
        if (lSqC <= 0f) newC = C;
        else
        {
            var l = MathF.Sqrt(lSqC);
            var dir = wC / l;
            newC = origin + dir * size;
        }
        return new(newA, newB, newC);
    }
    
    public Triangle ChangePosition(Vector2 offset) { return new(A + offset, B + offset, C + offset); }
    public Triangle SetPosition(Vector2 position)
    {
        var centroid = GetCentroid();
        var delta = position - centroid;
        return ChangePosition(delta);
    }
    public Triangle SetPosition(Vector2 position, Vector2 origin)
    {
        var delta = position - origin;
        return ChangePosition(delta);
    }

    
    /// <summary>
    /// Moves the triangle by transform.Position
    /// Rotates the moved triangle by transform.RotationRad
    /// Changes the size of the rotated triangle by transform.Size.Width!
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Triangle ApplyOffset(Transform2D offset)
    {
        var newTriangle = ChangePosition(offset.Position);
        newTriangle = newTriangle.ChangeRotation(offset.RotationRad);
        return newTriangle.ChangeSize(offset.ScaledSize.Length);
    }
    
    /// <summary>
    /// Moves the triangle by transform.Position
    /// Rotates the moved triangle by transform.RotationRad
    /// Changes the size of the rotated triangle by transform.Size.Width!
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public Triangle ApplyOffset(Transform2D offset, Vector2 origin)
    {
        var newTriangle = ChangePosition(offset.Position);
        newTriangle = newTriangle.ChangeRotation(offset.RotationRad, origin);
        return newTriangle.ChangeSize(offset.ScaledSize.Length, origin);
    }
    
    
    /// <summary>
    /// Moves the triangle to transform.Position
    /// Rotates the moved triangle to transform.RotationRad
    /// Sets the size of the rotated triangle to transform.ScaledSize.Length
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public Triangle SetTransform(Transform2D transform)
    {
        var newTriangle = SetPosition(transform.Position);
        newTriangle = newTriangle.SetRotation(transform.RotationRad);
        return newTriangle.SetSize(transform.ScaledSize.Length);
    }
    
    /// <summary>
    /// Moves the triangle to transform.Position
    /// Rotates the moved triangle to transform.RotationRad
    /// Sets the size of the rotated triangle to transform.ScaledSize.Length
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public Triangle SetTransform(Transform2D transform, Vector2 origin)
    {
        var newTriangle = SetPosition(transform.Position, origin);
        newTriangle = newTriangle.SetRotation(transform.RotationRad, origin);
        return newTriangle.SetSize(transform.ScaledSize.Length, origin);
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

    #region Contains

    public static bool ContainsPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
    {
        var ab = b - a;
        var bc = c - b;
        var ca = a - c;

        var ap = point - a;
        var bp = point - b;
        var cp = point - c;

        float c1 = ab.Cross(ap);
        float c2 = bc.Cross(bp);
        float c3 = ca.Cross(cp);

        return c1 < 0f && c2 < 0f && c3 < 0f;
    }
    public bool ContainsPoint(Vector2 p)
    {
        var ab = B - A;
        var bc = C - B;
        var ca = A - C;

        var ap = p - A;
        var bp = p - B;
        var cp = p - C;

        float c1 = ab.Cross(ap);
        float c2 = bc.Cross(bp);
        float c3 = ca.Cross(cp);

        return c1 < 0f && c2 < 0f && c3 < 0f;
    }
    // public static bool IsPointInside(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    // {
    //     var ab = b - a;
    //     var bc = c - b;
    //     var ca = a - c;
    //
    //     var ap = p - a;
    //     var bp = p - b;
    //     var cp = p - c;
    //
    //     float c1 = ShapeVec.Cross(ab, ap);
    //     float c2 = ShapeVec.Cross(bc, bp);
    //     float c3 = ShapeVec.Cross(ca, cp);
    //
    //     if (c1 < 0f && c2 < 0f && c3 < 0f)
    //     {
    //         return true;
    //     }
    //
    //     return false;
    // }

    public bool ContainsCollisionObject(CollisionObject collisionObject)
    {
        if (!collisionObject.HasColliders) return false;
        foreach (var collider in collisionObject.Colliders)
        {
            if (!ContainsCollider(collider)) return false;
        }

        return true;
    }
    public bool ContainsCollider(Collider collider)
    {
        switch (collider.GetShapeType())
        {
            case ShapeType.Circle: return ContainsShape(collider.GetCircleShape());
            case ShapeType.Segment: return ContainsShape(collider.GetSegmentShape());
            case ShapeType.Triangle: return ContainsShape(collider.GetTriangleShape());
            case ShapeType.Quad: return ContainsShape(collider.GetQuadShape());
            case ShapeType.Rect: return ContainsShape(collider.GetRectShape());
            case ShapeType.Poly: return ContainsShape(collider.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(collider.GetPolylineShape());
        }

        return false;
    }

    public bool ContainsShape(Segment segment)
    {
        return ContainsPoint(segment.Start) && ContainsPoint(segment.End);
    }
    public bool ContainsShape(Circle circle)
    {
        return ContainsPoint(circle.Top) &&
               ContainsPoint(circle.Left) &&
               ContainsPoint(circle.Bottom) &&
               ContainsPoint(circle.Right);
    }
    public bool ContainsShape(Rect rect)
    {
        return ContainsPoint(rect.TopLeft) &&
            ContainsPoint(rect.BottomLeft) &&
            ContainsPoint(rect.BottomRight) &&
            ContainsPoint(rect.TopRight);
    }
    public bool ContainsShape(Triangle triangle)
    {
        return ContainsPoint(triangle.A) &&
            ContainsPoint(triangle.B) &&
            ContainsPoint(triangle.C);
    }
    public bool ContainsShape(Quad quad)
    {
        return ContainsPoint(quad.A) &&
               ContainsPoint(quad.B) &&
               ContainsPoint(quad.C) &&
               ContainsPoint(quad.D);
    }
    public bool ContainsShape(Points points)
    {
        if (points.Count <= 0) return false;
        foreach (var p in points)
        {
            if (!ContainsPoint(p)) return false;
        }
        return true;
    }


    #endregion
    
    #region Closest Point
    public static Vector2 GetClosestPointTrianglePoint(Vector2 a, Vector2 b, Vector2 c, Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(a, b, p, out disSquared);

        var cp = Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
        }
        cp = Segment.GetClosestPointSegmentPoint(c, a, p, out dis);
        if (dis < disSquared)
        {
            disSquared = dis;
            return cp;
        }
        
        return min;
    }

    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;

        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - C;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        var normal = B - A;
        index = 0;

        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = C - B;
            index = 1;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(C, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - C;
            index = 2;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public ClosestPointResult GetClosestPoint(Line other)
    {
        var closestResult = Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;
        
        var result = Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        result = Segment.GetClosestPointSegmentLine(C, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var closestResult = Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var index = 0;
        
        var result = Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        result = Segment.GetClosestPointSegmentRay(C, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var index = 0;
        
        var result = Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            index);
    }
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var closestResult = Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var index = 0;
        var result = Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            index = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        result = Segment.GetClosestPointSegmentCircle(C, A, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            index = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = A - C;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, (closestResult.other - other.Center).Normalize()),
            disSquared,
            index);
    }
    public ClosestPointResult GetClosestPoint(Triangle other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        
        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.A - other.C;
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Quad other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.A - other.D;
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Rect other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.A, other.B, out float disSquared);
        var curSelfNormal = B - A;
        var curOtherNormal = B - A;
        var selfIndex = 0;
        var otherIndex = 0;
        var result = Segment.GetClosestPointSegmentSegment(A, B, other.B, other.C, out float dis);
        if (dis < disSquared)
        {
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(A, B, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(B, C, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(B, C, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = C - B;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(C, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - C;
            curOtherNormal = other.A - other.D;
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polygon other)
    {
        if (other.Count < 3) return new();
        
        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var selfIndex = -1;
        var otherIndex = -1;
        var disSquared = -1f;
        
        for (var i = 0; i < other.Count; i++)
        {
            var p1 = other[i];
            var p2 = other[(i + 1) % other.Count];
            
            var result = Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }
            
            result = Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }
            
            result = Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - C;
            }
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Polyline other)
    {
        if (other.Count < 2) return new();
        
        (Vector2 self, Vector2 other) closestResult = (Vector2.Zero, Vector2.Zero);
        var curSelfNormal = Vector2.Zero;
        var curOtherNormal = Vector2.Zero;
        var selfIndex = -1;
        var otherIndex = -1;
        var disSquared = -1f;
        
        for (var i = 0; i < other.Count - 1; i++)
        {
            var p1 = other[i];
            var p2 = other[i + 1];
            
            var result = Segment.GetClosestPointSegmentSegment(A, B, p1, p2, out float dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 0;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = B - A;
            }
            
            result = Segment.GetClosestPointSegmentSegment(B, C, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 1;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = C - B;
            }
            
            result = Segment.GetClosestPointSegmentSegment(C, A, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - C;
            }
        }
        
        return new(
            new(closestResult.self, curSelfNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, curOtherNormal.GetPerpendicularRight().Normalize()),
            disSquared,
            selfIndex,
            otherIndex);
    }
    public ClosestPointResult GetClosestPoint(Segments other)
    {
        if (other.Count <= 0) return new();
        
        ClosestPointResult closestResult = new();
        
        for (var i = 0; i < other.Count; i++)
        {
            var segment = other[i];
            var result = GetClosestPoint(segment);
            
            if (!closestResult.Valid || result.IsCloser(closestResult))
            {
                closestResult = result;
            }
        }
        
        return closestResult;
    }
    public (Segment segment, CollisionPoint segmentPoint) GetClosestSegment(Vector2 p, out float disSquared)
    {
        var closestSegment = SegmentAToB;
        var closestResult = closestSegment.GetClosestPoint(p, out disSquared);
            
        var currentSegment = SegmentBToC;
        var result = currentSegment.GetClosestPoint(p, out float dis);
        if (dis < disSquared)
        {
            closestSegment = currentSegment;
            disSquared = dis;
            closestResult = result;
        }
            
        currentSegment = SegmentCToA;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < disSquared)
        {
            closestSegment = currentSegment;
            disSquared = dis;
            closestResult = result;
        }

        return (closestSegment, closestResult);
    }
   
    public Vector2 GetClosestVertex(Vector2 p, out float disSquared, out int index)
    {
        var closest = A;
        disSquared = (A - p).LengthSquared();
        index = 0;
        float l = (B - p).LengthSquared();
        if (l < disSquared)
        {
            closest = B;
            disSquared = l;
            index = 1;
        }

        l = (C - p).LengthSquared();
        if (l < disSquared)
        {
            closest = C;
            disSquared = l;
            index = 2;
        }

        return closest;
    }
    #endregion
    
    #region Overlap
    public static bool OverlapTriangleSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentTriangle(segmentStart, segmentEnd, a, b, c);
    }
    public static bool OverlapTriangleLine(Vector2 a, Vector2 b, Vector2 c, Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineTriangle(linePoint, lineDirection, a, b, c);
    }
    public static bool OverlapTriangleRay(Vector2 a, Vector2 b, Vector2 c, Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayTriangle(rayPoint, rayDirection, a, b, c);
    }
    public static bool OverlapTriangleCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleTriangle(circleCenter, circleRadius, a, b, c);
    }
    public static bool OverlapTriangleTriangle(Vector2 a1, Vector2 b1, Vector2 c1,  Vector2 a2, Vector2 b2, Vector2 c2)
    {
        if (ContainsPoint(a1, b1, c1, a2)) return true;
        if (ContainsPoint(a2, b2, c2, a1)) return true;

        if( Segment.OverlapSegmentSegment(a1, b1, a2, b2) ) return true;
        if( Segment.OverlapSegmentSegment(a1, b1, b2, c2) ) return true;
        if( Segment.OverlapSegmentSegment(a1, b1, c2, a2) ) return true;
        
        if( Segment.OverlapSegmentSegment(b1, c1, a2, b2) ) return true;
        if( Segment.OverlapSegmentSegment(b1, c1, b2, c2) ) return true;
        if( Segment.OverlapSegmentSegment(b1, c1, c2, a2) ) return true;
        
        if( Segment.OverlapSegmentSegment(c1, a1, a2, b2) ) return true;
        if( Segment.OverlapSegmentSegment(c1, a1, b2, c2) ) return true;
        if( Segment.OverlapSegmentSegment(c1, a1, c2, a2) ) return true;

        return false;

    }
    public static bool OverlapTriangleQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        if (ContainsPoint(a, b, c, qa)) return true;
        if (Quad.ContainsPoint(qa, qb, qc,qd,a)) return true;

        if(Segment.OverlapSegmentSegment(a, b, qa, qb) ) return true;
        if(Segment.OverlapSegmentSegment(a, b, qb, qc) ) return true;
        if(Segment.OverlapSegmentSegment(a, b, qc, qd) ) return true;
        if(Segment.OverlapSegmentSegment(a, b, qd, qa) ) return true;
        
        if(Segment.OverlapSegmentSegment(b, c, qa, qb) ) return true;
        if(Segment.OverlapSegmentSegment(b, c, qb, qc) ) return true;
        if(Segment.OverlapSegmentSegment(b, c, qc, qd) ) return true;
        if(Segment.OverlapSegmentSegment(b, c, qd, qa) ) return true;
        
        if(Segment.OverlapSegmentSegment(c, a, qa, qb) ) return true;
        if(Segment.OverlapSegmentSegment(c, a, qb, qc) ) return true;
        if(Segment.OverlapSegmentSegment(c, a, qc, qd) ) return true;
        if(Segment.OverlapSegmentSegment(c, a, qd, qa) ) return true;

        return false;
    }
    public static bool OverlapTriangleRect(Vector2 a, Vector2 b, Vector2 c,  Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return OverlapTriangleQuad(a, b, c, ra, rb, rc, rd);
    }
    public static bool OverlapTrianglePolygon(Vector2 a, Vector2 b, Vector2 c, List<Vector2> points)
    {
        if (points.Count < 3) return false;
        if (ContainsPoint(a, b, c, points[0])) return true;
        
        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if( Segment.OverlapSegmentSegment(a, b, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(b, c, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(c, a, p1, p2) ) return true;
            
            if(Polygon.ContainsPointCheck(p1, p2, a)) oddNodes = !oddNodes;
        }
        return oddNodes;
    }
    public static bool OverlapTrianglePolyline(Vector2 a, Vector2 b, Vector2 c,  List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1];
            if( Segment.OverlapSegmentSegment(a, b, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(b, c, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(c, a, p1, p2) ) return true;
        }
        return false;
    }
    public static bool OverlapTriangleSegments(Vector2 a, Vector2 b, Vector2 c, List<Segment> segments)
    {
        if (segments.Count < 3) return false;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if( Segment.OverlapSegmentSegment(a, b, segment.Start, segment.End) ) return true;
            if( Segment.OverlapSegmentSegment(b, c, segment.Start, segment.End) ) return true;
            if( Segment.OverlapSegmentSegment(c, a, segment.Start, segment.End) ) return true;
        }
        return false;
    }

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapTriangleSegment(A, B, C,segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapTriangleLine(A, B, C,linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapTriangleRay(A, B, C,rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapTriangleCircle(A, B, C,circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapTriangleTriangle(A, B, C,a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapTriangleQuad(A, B, C,a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapTriangleQuad(A, B, C,a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapTrianglePolygon(A, B, C,points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapTrianglePolyline(A, B, C,points);
    public bool OverlapSegments(List<Segment> segments) => OverlapTriangleSegments(A, B, C,segments);
    
    public bool OverlapShape(Line line) => OverlapTriangleLine(A, B, C, line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapTriangleRay(A, B, C, ray.Point, ray.Direction);
    
    public  bool Overlap(Collider collider)
    {
        if (!collider.Enabled) return false;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return OverlapShape(c);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return OverlapShape(s);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return OverlapShape(l);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return OverlapShape(rayShape);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return OverlapShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return OverlapShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return OverlapShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return OverlapShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return OverlapShape(pl);
        }

        return false;
    }
    
    public  bool OverlapShape(Segments segments)
    {
        if (segments.Count <= 0) return false;

        if (ContainsPoint(segments[0].Start)) return true;
        
        foreach (var seg in segments)
        {
            if (Segment.OverlapSegmentSegment(A, B, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(B, C, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(C, A, seg.Start, seg.End)) return true;
        }
        return false;
    }

    public  bool OverlapShape(Segment s) => s.OverlapShape(this);
    public  bool OverlapShape(Circle c) => c.OverlapShape(this);
    public  bool OverlapShape(Triangle b)
    {
        if (ContainsPoint(b.A)) return true;
        
        if (b.ContainsPoint(A)) return true;
        
        if (Segment.OverlapSegmentSegment(A, B, b.A, b.B)) return true;
        if (Segment.OverlapSegmentSegment(A, B, b.B, b.C)) return true;
        if (Segment.OverlapSegmentSegment(A, B, b.C, b.A)) return true;
        
        if (Segment.OverlapSegmentSegment(B, C, b.A, b.B)) return true;
        if (Segment.OverlapSegmentSegment(B, C, b.B, b.C)) return true;
        if (Segment.OverlapSegmentSegment(B, C, b.C, b.A)) return true;
        
        if (Segment.OverlapSegmentSegment(C, A, b.A, b.B)) return true;
        if (Segment.OverlapSegmentSegment(C, A, b.B, b.C)) return true;
        return Segment.OverlapSegmentSegment(C, A, b.C, b.A);
    }
    public  bool OverlapShape(Rect r)
    {
        var a = r.TopLeft;
        if (ContainsPoint(a)) return true;
        
        if (r.ContainsPoint(A)) return true;
        
        var b = r.BottomLeft;
        if (Segment.OverlapSegmentSegment(A, B, a, b)) return true;
        
        var c = r.BottomRight;
        if (Segment.OverlapSegmentSegment(A, B, b, c)) return true;
        
        var d = r.TopRight;
        if (Segment.OverlapSegmentSegment(A, B, c, d)) return true;
        if (Segment.OverlapSegmentSegment(A, B, d, a)) return true;
        
        if (Segment.OverlapSegmentSegment(B, C, a, b)) return true;
        if (Segment.OverlapSegmentSegment(B, C, b, c)) return true;
        if (Segment.OverlapSegmentSegment(B, C, c, d)) return true;
        if (Segment.OverlapSegmentSegment(B, C, d, a)) return true;
        
        if (Segment.OverlapSegmentSegment(C, A, a, b)) return true;
        if (Segment.OverlapSegmentSegment(C, A, b, c)) return true;
        if (Segment.OverlapSegmentSegment(C, A, c, d)) return true;
        return Segment.OverlapSegmentSegment(C, A, d, a);
    }
    public  bool OverlapShape(Quad q)
    {
        if (ContainsPoint(q.A)) return true;
        
        if (q.ContainsPoint(A)) return true;
        
        if (Segment.OverlapSegmentSegment(A, B, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(A, B, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(A, B, q.C, q.D)) return true;
        if (Segment.OverlapSegmentSegment(A, B, q.D, q.A)) return true;
        
        if (Segment.OverlapSegmentSegment(B, C, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(B, C, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(B, C, q.C, q.D)) return true;
        if (Segment.OverlapSegmentSegment(B, C, q.D, q.A)) return true;
        
        if (Segment.OverlapSegmentSegment(C, A, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(C, A, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(C, A, q.C, q.D)) return true;
        return Segment.OverlapSegmentSegment(C, A, q.D, q.A);
    }
    public  bool OverlapShape(Polygon poly)
    {
        if (poly.Count < 3) return false;
        
        if (ContainsPoint(poly[0])) return true;
        
        var oddNodes = false;
        
        for (var i = 0; i < poly.Count; i++)
        {
            var start = poly[i];
            var end = poly[(i + 1) % poly.Count];
            if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.OverlapSegmentSegment(C, A, start, end)) return true;
            
            if(Polygon.ContainsPointCheck(start, end, A)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }
    public  bool OverlapShape(Polyline pl)
    {
        if (pl.Count < 2) return false;
        
        if (ContainsPoint(pl[0])) return true;
        
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.OverlapSegmentSegment(C, A, start, end)) return true;
            
        }

        return false;
    }


    #endregion

    #region Intersect
    
    public CollisionPoints? Intersect(Collider collider)
    {
        if (!collider.Enabled) return null;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl);
        }

        return null;
    }

    public CollisionPoints? IntersectShape(Segments segments)
    {
        if (segments.Count <= 0) return null;
        
        CollisionPoints? points = null;

        foreach (var seg in segments)
        {
            var result = Segment.IntersectSegmentSegment(A, B, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(C, A, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add((CollisionPoint)result);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Ray r)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    public CollisionPoints? IntersectShape(Line l)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    public CollisionPoints? IntersectShape(Segment s)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentSegment(A, B, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentSegment(B, C, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentSegment(C, A, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
   
    public CollisionPoints? IntersectShape(Circle c)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
            
        result = Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = Segment.IntersectSegmentCircle(C, A, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }

        return points;
    }
    public CollisionPoints? IntersectShape(Triangle t)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentSegment(A, B, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(A, B, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(A, B, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
            
        result = Segment.IntersectSegmentSegment(B, C, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        result = Segment.IntersectSegmentSegment(C, A, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        return points;
    }
    public CollisionPoints? IntersectShape(Rect r)
    {
        CollisionPoints? points = null;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var result = Segment.IntersectSegmentSegment(A, B, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        var c = r.BottomRight;
        result = Segment.IntersectSegmentSegment(A, B, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        var d = r.TopRight;
        result = Segment.IntersectSegmentSegment(A, B, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        result = Segment.IntersectSegmentSegment(A, B, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        
        
        result = Segment.IntersectSegmentSegment(B, C, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        
        result = Segment.IntersectSegmentSegment(C, A, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Quad q)
    {
        CollisionPoints? points = null;
        var result = Segment.IntersectSegmentSegment(A, B, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(A, B, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(A, B, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(A, B, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        
        result = Segment.IntersectSegmentSegment(B, C, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(B, C, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }

        
        result = Segment.IntersectSegmentSegment(C, A, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, A, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polygon p)
    {
        if (p.Count < 3) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < p.Count; i++)
        {
            var colPoint = Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
            
            colPoint = Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
            
            colPoint = Segment.IntersectSegmentSegment(C, A, p[i], p[(i + 1) % p.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
            
        }
        return points;
    }
    public CollisionPoints? IntersectShape(Polyline pl)
    {
        if (pl.Count < 2) return null;

        CollisionPoints? points = null;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            CollisionPoint colPoint = Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
            
            colPoint = Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
            
            colPoint = Segment.IntersectSegmentSegment(C, A, pl[i], pl[(i + 1) % pl.Count]);
            if (colPoint.Valid)
            {
                points ??= new();
                points.Add(colPoint);
            }
            
        }
        return points;
    }
    
    
    public int Intersect(Collider collider, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!collider.Enabled) return 0;

        switch (collider.GetShapeType())
        {
            case ShapeType.Circle:
                var c = collider.GetCircleShape();
                return IntersectShape(c, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = collider.GetRayShape();
                return IntersectShape(rayShape, ref points);
            case ShapeType.Line:
                var l = collider.GetLineShape();
                return IntersectShape(l, ref points);
            case ShapeType.Segment:
                var s = collider.GetSegmentShape();
                return IntersectShape(s, ref points);
            case ShapeType.Triangle:
                var t = collider.GetTriangleShape();
                return IntersectShape(t, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = collider.GetRectShape();
                return IntersectShape(r, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = collider.GetQuadShape();
                return IntersectShape(q, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = collider.GetPolygonShape();
                return IntersectShape(p, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = collider.GetPolylineShape();
                return IntersectShape(pl, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int IntersectShape(Ray r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
            
        result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
            
        result = Segment.IntersectSegmentRay(A, B, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    public int IntersectShape(Line l, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
            
        result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
            
        result = Segment.IntersectSegmentLine(A, B, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    public int IntersectShape(Segment s, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentSegment(A, B, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
            
        result = Segment.IntersectSegmentSegment(B, C, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
            
        result = Segment.IntersectSegmentSegment(C, A, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    public int IntersectShape(Circle c, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }
            
        result = Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentCircle(C, A, c.Center, c.Radius);
        if (result.a.Valid)
        {
            points.Add(result.a);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (result.b.Valid)
        {
            points.Add(result.b);
            count++;
        }

        return count;
    }
    public int IntersectShape(Triangle t, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var result = Segment.IntersectSegmentSegment(A, B, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(A, B, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(A, B, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
            
        result = Segment.IntersectSegmentSegment(B, C, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentSegment(C, A, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, A, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, A, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }

        return count;
    }
    public int IntersectShape(Quad q, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        //Todo: revert back to original
        //NOTE: CountPerSegment implemented as an example here (not implemented in any other function yet) - CANCELED!
        //checking the entire quad against each segment of the triangle
        //therefore the maximum number of intersections per segment is 2
        var countPerSegment = 0;
        var result = Segment.IntersectSegmentSegment(A, B, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
            countPerSegment++;
        }
        result = Segment.IntersectSegmentSegment(A, B, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
            countPerSegment++;
        }

        if (countPerSegment < 2)
        {
            result = Segment.IntersectSegmentSegment(A, B, q.C, q.D);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
                countPerSegment++;
            }

            if (countPerSegment < 2)
            {
                result = Segment.IntersectSegmentSegment(A, B, q.D, q.A);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
            
        }

        countPerSegment = 0;
        
        result = Segment.IntersectSegmentSegment(B, C, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
            countPerSegment++;
        }
        result = Segment.IntersectSegmentSegment(B, C, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
            countPerSegment++;
        }

        if (countPerSegment < 2)
        {
            result = Segment.IntersectSegmentSegment(B, C, q.C, q.D);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
                countPerSegment++;
            }
            if (countPerSegment < 2)
            {
                result = Segment.IntersectSegmentSegment(B, C, q.D, q.A);
                if (result.Valid)
                {
                    points.Add(result);
                    if (returnAfterFirstValid) return 1;
                    count++;
                }
            }
        }


        countPerSegment = 0;
        
        result = Segment.IntersectSegmentSegment(C, A, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
            countPerSegment++;
        }
        result = Segment.IntersectSegmentSegment(C, A, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
            countPerSegment++;
        }

        if (countPerSegment < 2)
        {
            result = Segment.IntersectSegmentSegment(C, A, q.C, q.D);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
                countPerSegment++;
            }
        
            if (countPerSegment < 2)
            {
                result = Segment.IntersectSegmentSegment(C, A, q.D, q.A);
                if (result.Valid)
                {
                    points.Add(result);
                    count++;
                }
            }
        }
        
        
        return count;
    }
    public int IntersectShape(Rect r, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        var count = 0;
        var a = r.TopLeft;
        var b = r.BottomLeft;
        var result = Segment.IntersectSegmentSegment(A, B, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var c = r.BottomRight;
        result = Segment.IntersectSegmentSegment(A, B, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        var d = r.TopRight;
        result = Segment.IntersectSegmentSegment(A, B, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentSegment(A, B, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        
        
        result = Segment.IntersectSegmentSegment(B, C, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        
        result = Segment.IntersectSegmentSegment(C, A, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, A, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, A, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, A, d, a);
        if (result.Valid)
        {
            points.Add(result);
            count++;
        }
        return count;
    }
    public int IntersectShape(Polygon p, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (p.Count < 3) return 0;

        var count = 0;
        for (var i = 0; i < p.Count; i++)
        {
            var result = Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(C, A, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Polyline pl, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (pl.Count < 2) return 0;

        var count = 0;
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var result = Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(C, A, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
    public int IntersectShape(Segments shape, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (shape.Count <= 0) return 0;

        var count = 0;

        foreach (var seg in shape)
        {
            var result = Segment.IntersectSegmentSegment(A, B, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(C, A, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
        }
        return count;
    }
   
    #endregion
    
}
