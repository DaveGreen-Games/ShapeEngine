using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

/// <summary>
/// Points should be in CCW order (A -> B -> C -> D)
/// </summary>
public readonly struct Quad : IEquatable<Quad>
{
    #region Members
    public readonly Vector2 A;
    public readonly Vector2 B;
    public readonly Vector2 C;
    public readonly Vector2 D;
    #endregion

    #region Getters
    public Vector2 AB => B - A;
    public Vector2 BC => C - B;
    public Vector2 CD => D - C;
    public Vector2 DA => A - D;

    public Segment SegmentAToB => new Segment(A, B);
    public Segment SegmentBToC => new Segment(B, C);
    public Segment SegmentCToD => new Segment(C, D);
    public Segment SegmentDToA => new Segment(D, A);

    public float Area
    {
        get
        {
            Triangle abc = new(A,B,C);
            Triangle cda= new(C,D,A);
            return abc.GetArea() + cda.GetArea();
        }
    }
    #endregion

    #region Constructor
    internal Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        this.A = a;
        this.B = b;
        this.C = c;
        this.D = d;
    }
    public Quad(Vector2 topLeft, Vector2 bottomRight)
    {
        this.A = topLeft;
        this.C = bottomRight;
        this.B = new(topLeft.X, bottomRight.Y);
        this.D = new(bottomRight.X, topLeft.Y);
    }
    public Quad(Vector2 topLeft, float width, float height)
    {
        this.A = topLeft;
        this.B = topLeft + new Vector2(0f, height);
        this.C = topLeft + new Vector2(width, height);
        this.D = topLeft + new Vector2(width, 0f);
    }
    public Quad(Rect rect)
    {
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;
        this.A = topLeft;
        this.C = bottomRight;
        this.B = new(topLeft.X, bottomRight.Y);
        this.D = new(bottomRight.X, topLeft.Y);
    }
    public Quad(Rect rect, float rotRad, Vector2 pivot)
    {
        var pivotPoint = rect.GetPoint(pivot);
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;

        this.A = (topLeft - pivotPoint).Rotate(rotRad);
        this.B = (new Vector2(topLeft.X, bottomRight.Y) - pivotPoint).Rotate(rotRad);
        this.C = (bottomRight - pivotPoint).Rotate(rotRad);
        this.D = (new Vector2(bottomRight.X, topLeft.Y) - pivotPoint).Rotate(rotRad);
    }
    public Quad(Vector2 pos, Vector2 size, float rotRad, Vector2 alignement)
    {
        var offset = size * alignement;
        var topLeft = pos - offset;
        
        var a = topLeft - pos;
        var b = topLeft + new Vector2(0f, size.Y);
        var c = topLeft + size;
        var d = topLeft + new Vector2(size.X, 0f);

        this.A = pos + (a - pos).Rotate(rotRad);
        this.B = pos + (b - pos).Rotate(rotRad);
        this.C = pos + (c - pos).Rotate(rotRad);
        this.D = pos + (d - pos).Rotate(rotRad);
        
    }
    #endregion
    
    #region Public

    //TODO implement
    public Rect GetBoundingBox()
    {
        return new();
    }
    public readonly CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        return new();
    }

    public Segments GetEdges() => new() { SegmentAToB, SegmentBToC, SegmentCToD, SegmentDToA };

    public Polygon ToPolygon() => new() { A, B, C, D };
    public Vector2 GetPoint(Vector2 alignement)
    {

        var ab = A.Lerp(B, alignement.Y); // B - A;
        var cd = C.Lerp(D, alignement.Y);
        return ab.Lerp(cd, alignement.X);
    }
    public Quad RotateRad(float angle, Vector2 pivot)
    {
        var pivotPoint = GetPoint(pivot);
        var a = (A - pivotPoint).Rotate(angle);
        var b = (B - pivotPoint).Rotate(angle);
        var c = (C - pivotPoint).Rotate(angle);
        var d = (D - pivotPoint).Rotate(angle);
        return new(a,b,c,d);
    }

    public Quad RotateDeg(float angle, Vector2 pivot) => RotateRad(angle * ShapeMath.DEGTORAD, pivot);

    public Triangulation Triangulate()
    {
        Triangle abc = new(A,B,C);
        Triangle cda= new(C,D,A);
        return new Triangulation() { abc, cda };
    }

    public Quad MoveBy(Vector2 amount)
    {
        return new
        (
            A + amount,
            B + amount,
            C + amount,
            D + amount
        );
    }
    public Quad MoveTo(Vector2 target, Vector2 alignement)
    {
        var p = GetPoint(alignement);
        var translation = target - p;
        return new
        (
            A + translation,
            B + translation,
            C + translation,
            D + translation
        );
    }

    public Quad ScaleBy(float amount, Vector2 alignement)
    {
        var p = GetPoint(alignement);
        return new
            (
                A + (A - p) * amount,
                B + (B - p) * amount,
                C + (C - p) * amount,
                D + (D - p) * amount
            );

    }
    public Quad ScaleBy(Vector2 amount, Vector2 alignement)
    {
        var p = GetPoint(alignement);
        return new
        (
            A + (A - p) * amount,
            B + (B - p) * amount,
            C + (C - p) * amount,
            D + (D - p) * amount
        );
    }

    // private bool ContainsPointCheck(Vector2 a, Vector2 b, Vector2 pointToCheck)
    // {
    //     if (a.Y < pointToCheck.Y && b.Y >= pointToCheck.Y || b.Y < pointToCheck.Y && a.Y >= pointToCheck.Y)
    //     {
    //         if (a.X + (pointToCheck.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X) < pointToCheck.X)
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    
    public bool ContainsPoint(Vector2 p)
    {
        var oddNodes = false;

        if (Polygon.ContainsPointCheck(D, A, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(A, B, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(B, C, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(D, D, p)) oddNodes = !oddNodes;
        
        return oddNodes;
    }
    #endregion
    
    #region Operators
    public static Quad operator +(Quad left, Quad right)
    {
        return new
        (
            left.A + right.A,
            left.B + right.B,
            left.C + right.C,
            left.D + right.D
        );
    }
    public static Quad operator -(Quad left, Quad right)
    {
        return new
        (
            left.A - right.A,
            left.B - right.B,
            left.C - right.C,
            left.D - right.D
        );
    }
    public static Quad operator *(Quad left, Quad right)
    {
        return new
        (
            left.A * right.A,
            left.B * right.B,
            left.C * right.C,
            left.D * right.D
        );
    }
    public static Quad operator /(Quad left, Quad right)
    {
        return new
        (
            left.A / right.A,
            left.B / right.B,
            left.C / right.C,
            left.D / right.D
        );
    }
    public static Quad operator +(Quad left, Vector2 right)
    {
        return new
        (
            left.A + right,
            left.B + right,
            left.C + right,
            left.D + right
        );
    }
    public static Quad operator -(Quad left, Vector2 right)
    {
        return new
        (
            left.A - right,
            left.B - right,
            left.C - right,
            left.D - right
        );
    }
    public static Quad operator *(Quad left, Vector2 right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right,
            left.D * right
        );
    }
    public static Quad operator /(Quad left, Vector2 right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right,
            left.D / right
        );
    }
    public static Quad operator *(Quad left, float right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right,
            left.D * right
        );
    }
    public static Quad operator /(Quad left, float right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right,
            left.D / right
        );
    }
    public static bool operator ==(Quad left, Quad right) => left.Equals(right);
    public static bool operator !=(Quad left, Quad right) => !(left == right);
    #endregion

    #region Equality
    public bool Equals(Quad other) => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);

    public override bool Equals(object? obj) => obj is Quad other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(A, B, C, D);
    #endregion

    #region Overlap

    public bool Overlap(Collider collider)
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

    public bool OverlapShape(Segments segments)
    {
        if (segments.Count <= 0) return false;
        if (ContainsPoint(segments[0].Start)) return true;
        
        foreach (var seg in segments)
        {
            if (Segment.OverlapSegmentSegment(A, B, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(B, C, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(C, D, seg.Start, seg.End)) return true;
            if (Segment.OverlapSegmentSegment(D, A, seg.Start, seg.End)) return true;
        }
        return false;
    }
    public bool OverlapShape(Segment s) => s.OverlapShape(this);
    public bool OverlapShape(Circle c) => c.OverlapShape(this);
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Quad q)
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
        
        if (Segment.OverlapSegmentSegment(C, D, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(C, D, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(C, D, q.C, q.D)) return true;
        if (Segment.OverlapSegmentSegment(C, D, q.D, q.A)) return true;
        
        if (Segment.OverlapSegmentSegment(D, A, q.A, q.B)) return true;
        if (Segment.OverlapSegmentSegment(D, A, q.B, q.C)) return true;
        if (Segment.OverlapSegmentSegment(D, A, q.C, q.D)) return true;
        return Segment.OverlapSegmentSegment(D, A, q.D, q.A);
    }
    public bool OverlapShape(Rect r)
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
        
        if (Segment.OverlapSegmentSegment(C, D, a, b)) return true;
        if (Segment.OverlapSegmentSegment(C, D, b, c)) return true;
        if (Segment.OverlapSegmentSegment(C, D, c, d)) return true;
        if (Segment.OverlapSegmentSegment(C, D, d, a)) return true;
        
        if (Segment.OverlapSegmentSegment(D, A, a, b)) return true;
        if (Segment.OverlapSegmentSegment(D, A, b, c)) return true;
        if (Segment.OverlapSegmentSegment(D, A, c, d)) return true;
        return Segment.OverlapSegmentSegment(D, A, d, a);
    }
    public bool OverlapShape(Polygon poly)
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
            if (Segment.OverlapSegmentSegment(C, D, start, end)) return true;
            if (Segment.OverlapSegmentSegment(D, A, start, end)) return true;
                
            if(Polygon.ContainsPointCheck(start, end, A)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }
    public bool OverlapShape(Polyline pl)
    {
        if (pl.Count < 2) return false;
            
        if (ContainsPoint(pl[0])) return true;
            
        for (var i = 0; i < pl.Count - 1; i++)
        {
            var start = pl[i];
            var end = pl[(i + 1) % pl.Count];
            if (Segment.OverlapSegmentSegment(A, B, start, end)) return true;
            if (Segment.OverlapSegmentSegment(B, C, start, end)) return true;
            if (Segment.OverlapSegmentSegment(C, D, start, end)) return true;
            if (Segment.OverlapSegmentSegment(D, A, start, end)) return true;
                
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
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
                result = Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
                result = Segment.IntersectSegmentSegment(C, D, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
                result = Segment.IntersectSegmentSegment(D, A, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Segment s)
        {
            CollisionPoints? points = null;
            var result = Segment.IntersectSegmentSegment(A, B, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
                
            result = Segment.IntersectSegmentSegment(B, C, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
                
            result = Segment.IntersectSegmentSegment(C, D, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(D, A, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public  CollisionPoints? IntersectShape(Circle c)
        {
            CollisionPoints? points = null;
            var result = Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
                
            result = Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
            
            result = Segment.IntersectSegmentCircle(C, D, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
            
            result = Segment.IntersectSegmentCircle(D, A, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Triangle t)
        {
            CollisionPoints? points = null;
            var result = Segment.IntersectSegmentSegment(A, B, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(A, B, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(A, B, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
                
            result = Segment.IntersectSegmentSegment(B, C, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(C, D, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(D, A, t.A, t.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, t.B, t.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, t.C, t.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            return points;
        }
        public CollisionPoints? IntersectShape(Rect r)
        {
            CollisionPoints? points = null;
            var a = r.TopLeft;
            var b = r.BottomLeft;
            var result = Segment.IntersectSegmentSegment(A, B, a, b);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            var c = r.BottomRight;
            result = Segment.IntersectSegmentSegment(A, B, b, c);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            var d = r.TopRight;
            result = Segment.IntersectSegmentSegment(A, B, c, d);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(A, B, d, a);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            
            
            result = Segment.IntersectSegmentSegment(B, C, a, b);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, b, c);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, c, d);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, d, a);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            
            result = Segment.IntersectSegmentSegment(C, D, a, b);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, b, c);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, c, d);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, d, a);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            
            result = Segment.IntersectSegmentSegment(D, A, a, b);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, b, c);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, c, d);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, d, a);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Quad q)
        {
            CollisionPoints? points = null;
            var result = Segment.IntersectSegmentSegment(A, B, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(A, B, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(A, B, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(A, B, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            
            result = Segment.IntersectSegmentSegment(B, C, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            
            result = Segment.IntersectSegmentSegment(C, D, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, D, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(D, A, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(D, A, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Polygon p)
        {
            if (p.Count < 3) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < p.Count; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(C, D, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(D, A, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Polyline pl)
        {
            if (pl.Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < pl.Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(C, D, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(D, A, pl[i], pl[(i + 1) % pl.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
            }
            return points;
        }
        
        
        #endregion
        
}