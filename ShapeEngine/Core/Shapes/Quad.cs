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

    public Vector2 Center => GetPoint(0.5f);
    public float AngleRad => BC.AngleRad();
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
    public Quad(Vector2 pos, Size size, float rotRad, Vector2 alignement)
    {
        var offset = size * alignement;
        var topLeft = pos - offset;
        
        var a = topLeft;
        var b = topLeft + new Vector2(0f, size.Height);
        var c = topLeft + size;
        var d = topLeft + new Vector2(size.Width, 0f);

        this.A = pos + (a - pos).Rotate(rotRad);
        this.B = pos + (b - pos).Rotate(rotRad);
        this.C = pos + (c - pos).Rotate(rotRad);
        this.D = pos + (d - pos).Rotate(rotRad);
        
    }
    #endregion
    
    #region Public
    public Polygon? Project(Vector2 v)
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
    
    public Quad Floor()
    {
        return new
        (
            A.Floor(),
            B.Floor(),
            C.Floor(),
            D.Floor()
        );
    }
    public Quad Ceiling()
    {
        return new
        (
            A.Ceiling(),
            B.Ceiling(),
            C.Ceiling(),
            D.Ceiling()
        );
    }
    public Quad Round()
    {
        return new
        (
            A.Round(),
            B.Round(),
            C.Round(),
            D.Round()
        );
    }
    public Quad Truncate()
    {
        return new
        (
            A.Truncate(),
            B.Truncate(),
            C.Truncate(),
            D.Truncate()
        );
    }

    public Rect GetBoundingBox()
    {
        Rect r = new(A.X, A.Y, 0, 0);
        r = r.Enlarge(B);
        r = r.Enlarge(C);
        r = r.Enlarge(D);
        return r;
    }
    public Vector2 GetPoint(Vector2 alignement) => GetPoint(alignement.X, alignement.Y);
    public Vector2 GetPoint(float alignementX, float alignementY)
    {
        var ab = A.Lerp(B, alignementY); // B - A;
        var cd = C.Lerp(D, alignementY);
        return ab.Lerp(cd, alignementX);
    }
    public Vector2 GetPoint(float alignement) => GetPoint(alignement, alignement);

    public Vector2 GetVertex(int index)
    {
        var i = index % 4;
        if (i == 0) return A;
        if (i == 1) return B;
        if (i == 2) return C;
        return D;
    }
    public Segment GetEdge(int index)
    {
        var i = index % 4;
        if (i == 0) return SegmentAToB;
        if (i == 1) return SegmentBToC;
        if (i == 2) return SegmentCToD;
        return SegmentDToA;
    }

    
    
    public Vector2 GetRandomPointInside() => GetPoint(ShapeRandom.RandF(), ShapeRandom.RandF());
    public Points GetRandomPointsInside(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointInside());
        }

        return points;
    }
    public Vector2 GetRandomVertex() => GetVertex(ShapeRandom.RandI(0, 3));
    public Segment GetRandomEdge()  => GetEdge(ShapeRandom.RandI(0, 3));
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    public Points GetRandomPointsOnEdge(int amount)
    {
        var points = new Points();

        var ab = SegmentAToB;
        var bc = SegmentBToC;
        var cd = SegmentCToD;
        var da = SegmentDToA;
        
        for (int i = 0; i < amount; i++)
        {
            var rIndex = ShapeRandom.RandI(0, 3);
            switch (rIndex)
            {
                case 0:
                    points.Add(ab.GetRandomPoint());
                    break;
                case 1:
                    points.Add(bc.GetRandomPoint());
                    break;
                case 2:
                    points.Add(cd.GetRandomPoint());
                    break;
                default:
                    points.Add(da.GetRandomPoint());
                    break;
            }
        }

        return points;
    }
        
    
    public Quad RotateByRad(float angle, Vector2 pivot)
    {
        var pivotPoint = GetPoint(pivot);
        var a = (A - pivotPoint).Rotate(angle);
        var b = (B - pivotPoint).Rotate(angle);
        var c = (C - pivotPoint).Rotate(angle);
        var d = (D - pivotPoint).Rotate(angle);
        return new(a,b,c,d);
    }

    public Quad RotateByDeg(float angle, Vector2 pivot) => RotateByRad(angle * ShapeMath.DEGTORAD, pivot);
    public Quad RotateToRad(float target, Vector2 pivot)
    {
        float amount = ShapeMath.GetShortestAngleRad(AngleRad, target);
        return RotateByRad(amount, pivot);
    }
    public Quad RotateToDeg(float target, Vector2 pivot) => RotateToRad(target * ShapeMath.DEGTORAD, pivot);
    
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
    
    public Segments GetEdges() => new() { SegmentAToB, SegmentBToC, SegmentCToD, SegmentDToA };

    public Polygon ToPolygon() => new() { A, B, C, D };
    public Points ToPoints() => new() { A, B, C, D };
    public Polyline ToPolyline() => new() { A, B, C, D };
    public Triangulation Triangulate()
    {
        Triangle abc = new(A,B,C);
        Triangle cda= new(C,D,A);
        return new Triangulation() { abc, cda };
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

    #region Contains
    public bool ContainsPoint(Vector2 p)
    {
        var oddNodes = false;

        if (Polygon.ContainsPointCheck(D, A, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(A, B, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(B, C, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(D, D, p)) oddNodes = !oddNodes;
        
        return oddNodes;
    }
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

    #region Closest
    public static Vector2 GetClosestPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p)
    {
        var ab = Segment.GetClosestPoint(a, b, p);
        var bc = Segment.GetClosestPoint(b, c, p);
        var cd = Segment.GetClosestPoint(c, d, p);
        var da = Segment.GetClosestPoint(d, a, p);

        var min = ab;
        var minDisSq = (ab - p).LengthSquared();

        var bcDisSq = (bc - p).LengthSquared();
        if (bcDisSq < minDisSq)
        {
            min = bc;
            minDisSq = bcDisSq;
        }
        
        var cdDisSq = (cd - p).LengthSquared();
        if (cdDisSq < minDisSq)
        {
            min = cd;
            minDisSq = cdDisSq;
        }
            
        return (da - p).LengthSquared() < minDisSq ? da : min;
    }
    public ClosestDistance GetClosestDistanceTo(Vector2 p)
    {
        var cp = GetClosestPoint(A, B, C, D, p);
        return new ClosestDistance(cp, p);
    }

    public ClosestDistance GetClosestDistanceTo(Segment segment)
    {
        var next = GetClosestPoint(A, B, C, D,segment.Start);
        var disSq = (next - segment.Start).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = segment.Start;

        next = GetClosestPoint(A, B, C, D, segment.End);
        disSq = (next - segment.End).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = segment.End;
        }
        
        next = Segment.GetClosestPoint(segment.Start, segment.End, A);
        disSq = (next - A).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = A;
        }
        
        next = Segment.GetClosestPoint(segment.Start, segment.End, B);
        disSq = (next - B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = B;
        }
        
        next = Segment.GetClosestPoint(segment.Start, segment.End, C);
        disSq = (next - C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpOther = next;
            cpSelf = C;
        }

        next = Segment.GetClosestPoint(segment.Start, segment.End, D);
        disSq = (next - D).LengthSquared();
        if (disSq < minDisSq)
        {
            cpOther = next;
            cpSelf = D;
        }

        return new(cpSelf, cpOther);
    }
    public ClosestDistance GetClosestDistanceTo(Circle circle)
    {
        var point = Segment.GetClosestPoint(A, B, circle.Center);
        var disSq = (circle.Center - point).LengthSquared();
        var minDisSq = disSq;
        var closestPoint = point;
        
        point = Segment.GetClosestPoint(B, C, circle.Center);
        disSq = (circle.Center - point).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            closestPoint = point;
        }
        
        point = Segment.GetClosestPoint(C, D, circle.Center);
        disSq = (circle.Center - point).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            closestPoint = point;
        }

        point = Segment.GetClosestPoint(D, A, circle.Center);
        disSq = (circle.Center - point).LengthSquared();
        if (disSq < minDisSq) closestPoint = point;

        var dir = (closestPoint - circle.Center).Normalize();

        return new(closestPoint, circle.Center + dir * circle.Radius);

    }
    public ClosestDistance GetClosestDistanceTo(Triangle triangle)
    {
        var next = GetClosestPoint(A, B, C, D, triangle.A);
        var disSq = (next - triangle.A).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = triangle.A;

        next = GetClosestPoint(A, B, C, D, triangle.B);
        disSq = (next - triangle.B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = triangle.B;
        }
        
        next = GetClosestPoint(A, B, C, D, triangle.C);
        disSq = (next - triangle.C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = triangle.C;
        }

        next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, A);
        disSq = (next - A).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = A;
            cpOther = next;
        }
        
        next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, B);
        disSq = (next - B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = B;
            cpOther = next;
        }
        
        next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, C);
        disSq = (next - C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = C;
            cpOther = next;
        }
        next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, D);
        disSq = (next - D).LengthSquared();
        if (disSq < minDisSq)
        {
            cpSelf = D;
            cpOther = next;
        }
        
        return new(cpSelf, cpOther);
    }
    public ClosestDistance GetClosestDistanceTo(Quad quad)
    {
        var next = GetClosestPoint(A, B, C, D, quad.A);
        var disSq = (next - quad.A).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = quad.A;

        next = GetClosestPoint(A, B, C, D, quad.B);
        disSq = (next - quad.B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = quad.B;
        }
        
        next = GetClosestPoint(A, B, C, D, quad.C);
        disSq = (next - quad.C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = quad.C;
        }
        
        next = GetClosestPoint(A, B, C, D, quad.D);
        disSq = (next - quad.D).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = quad.D;
        }

        next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, A);
        disSq = (next - A).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = A;
            cpOther = next;
        }
        
        next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, B);
        disSq = (next - B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = B;
            cpOther = next;
        }
        
        next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, C);
        disSq = (next - C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = C;
            cpOther = next;
        }
        
        next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, D);
        disSq = (next - D).LengthSquared();
        if (disSq < minDisSq)
        {
            cpSelf = D;
            cpOther = next;
        }
        
        return new(cpSelf, cpOther);
    }
    public ClosestDistance GetClosestDistanceTo(Rect rect)
    {
        var next = GetClosestPoint(A, B, C, D, rect.A);
        var disSq = (next - rect.TopLeft).LengthSquared();
        var minDisSq = disSq;
        var cpSelf = next;
        var cpOther = rect.TopLeft;

        next = GetClosestPoint(A, B, C, D, rect.B);
        disSq = (next - rect.BottomLeft).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = rect.BottomLeft;
        }
        
        next = GetClosestPoint(A, B, C, D, rect.C);
        disSq = (next - rect.BottomRight).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = rect.BottomRight;
        }
        next = GetClosestPoint(A, B, C, D, rect.D);
        disSq = (next - rect.TopRight).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = next;
            cpOther = rect.TopRight;
        }

        next = Quad.GetClosestPoint(rect.A, rect.B, rect.C, rect.D, A);
        disSq = (next - A).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = A;
            cpOther = next;
        }
        
        next = Quad.GetClosestPoint(rect.A, rect.B, rect.C, rect.D, B);
        disSq = (next - B).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = B;
            cpOther = next;
        }
        
        next = Quad.GetClosestPoint(rect.A, rect.B, rect.C, rect.D, C);
        disSq = (next - C).LengthSquared();
        if (disSq < minDisSq)
        {
            minDisSq = disSq;
            cpSelf = C;
            cpOther = next;
        }
        
        next = Quad.GetClosestPoint(rect.A, rect.B, rect.C, rect.D, D);
        disSq = (next - D).LengthSquared();
        if (disSq < minDisSq)
        {
            cpSelf = D;
            cpOther = next;
        }
        
        return new(cpSelf, cpOther);
    }
    public ClosestDistance GetClosestDistanceTo(Polygon polygon)
    {
        if (polygon.Count <= 0) return new();
        if (polygon.Count == 1) return GetClosestDistanceTo(polygon[0]);
        if (polygon.Count == 2) return GetClosestDistanceTo(new Segment(polygon[0], polygon[1]));
        if (polygon.Count == 3) return GetClosestDistanceTo(new Triangle(polygon[0], polygon[1], polygon[2]));
        if (polygon.Count == 4) return GetClosestDistanceTo(new Quad(polygon[0], polygon[1], polygon[2], polygon[3]));
        
        ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
        
        for (var i = 0; i < polygon.Count; i++)
        {
            var p1 = polygon[i];
            var p2 = polygon[(i + 1) % polygon.Count];
            
            var next = GetClosestPoint(A, B, C, D, p1);
            var cd = new ClosestDistance(next, p1);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(A, B, C, D, p2);
            cd = new ClosestDistance(next, p2);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, A);
            cd = new ClosestDistance(A, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, B);
            cd = new ClosestDistance(B, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, C);
            cd = new ClosestDistance(C, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, D);
            cd = new ClosestDistance(D, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
        }
        return closestDistance;
    }
    public ClosestDistance GetClosestDistanceTo(Polyline polyline)
    {
        if (polyline.Count <= 0) return new();
        if (polyline.Count == 1) return GetClosestDistanceTo(polyline[0]);
        if (polyline.Count == 2) return GetClosestDistanceTo(new Segment(polyline[0], polyline[1]));
        
        ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
        
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var p1 = polyline[i];
            var p2 = polyline[(i + 1) % polyline.Count];
            
            var next = GetClosestPoint(A, B, C,D, p1);
            var cd = new ClosestDistance(next, p1);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = GetClosestPoint(A, B, C, D, p2);
            cd = new ClosestDistance(next, p2);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, A);
            cd = new ClosestDistance(A, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, B);
            cd = new ClosestDistance(B, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, C);
            cd = new ClosestDistance(C, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            
            next = Segment.GetClosestPoint(p1, p2, D);
            cd = new ClosestDistance(D, next);
            if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
        }
        return closestDistance;
    }
    
    
    
    public int GetClosestEdgePointByIndex(Vector2 p)
    {
        float minD = float.PositiveInfinity;
        int closestIndex = -1;

        var edge = SegmentAToB;
        var closest = edge.GetClosestCollisionPoint(p).Point;
        float d = (closest - p).LengthSquared();
        if (d < minD)
        {
            closestIndex = 0;
            minD = d;
        }
        edge = SegmentBToC;
        closest = edge.GetClosestCollisionPoint(p).Point;
        d = (closest - p).LengthSquared();
        if (d < minD)
        {
            closestIndex = 1;
            minD = d;
        }
        edge = SegmentCToD;
        closest = edge.GetClosestCollisionPoint(p).Point;
        d = (closest - p).LengthSquared();
        if (d < minD)
        {
            closestIndex = 2;
            minD = d;
        }
        edge = SegmentDToA;
        closest = edge.GetClosestCollisionPoint(p).Point;
        d = (closest - p).LengthSquared();
        if (d < minD) return 3;
        return closestIndex;
    }
    internal ClosestPoint GetClosestPoint(Vector2 p)
    {
        var closest = GetClosestCollisionPoint(p);
        return new(closest, (closest.Point - p).Length());
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        float minD = float.PositiveInfinity;
            
        CollisionPoint closest = new();

        var c = SegmentAToB.GetClosestCollisionPoint(p);
        float d = (c.Point - p).LengthSquared();
        if (d < minD)
        {
            closest = c;
            minD = d;
        }
        
        c = SegmentBToC.GetClosestCollisionPoint(p);
        d = (c.Point - p).LengthSquared();
        if (d < minD)
        {
            closest = c;
            minD = d;
        }
        
        c = SegmentCToD.GetClosestCollisionPoint(p);
        d = (c.Point - p).LengthSquared();
        if (d < minD)
        {
            closest = c;
            minD = d;
        }
        c = SegmentDToA.GetClosestCollisionPoint(p);
        d = (c.Point - p).LengthSquared();
        if (d < minD)
        {
            closest = c;
        }

        return closest;
    }

    public ClosestSegment GetClosestSegment(Vector2 p)
    {
        float minD = float.PositiveInfinity;
            
        Segment closestSegment = new();
        CollisionPoint closestPoint = new();
        var c = SegmentAToB.GetClosestPoint(p);
        if (c.Distance < minD)
        {
            closestSegment = SegmentAToB;
            closestPoint = c.Closest;
            minD = c.Distance;
        }
        
        c = SegmentBToC.GetClosestPoint(p);
        if (c.Distance < minD)
        {
            closestSegment = SegmentBToC;
            closestPoint = c.Closest;
            minD = c.Distance;
        }
        
        c = SegmentCToD.GetClosestPoint(p);
        if (c.Distance < minD)
        {
            closestSegment = SegmentCToD;
            closestPoint = c.Closest;
            minD = c.Distance;
        }
        c = SegmentDToA.GetClosestPoint(p);
        if (c.Distance < minD)
        {
            closestSegment = SegmentDToA;
            closestPoint = c.Closest;
            minD = c.Distance;
        }
        return new(closestSegment, closestPoint, minD);
    }
    
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