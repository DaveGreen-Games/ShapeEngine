using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.Segment;
using ShapeEngine.Random;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Geometry.Quad;

/// <summary>
/// Points should be in CCW order (A -> B -> C -> D)
/// </summary>
public readonly partial struct Quad : IEquatable<Quad>
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

    public Segment.Segment SegmentAToB => new Segment.Segment(A, B);
    public Segment.Segment SegmentBToC => new Segment.Segment(B, C);
    public Segment.Segment SegmentCToD => new Segment.Segment(C, D);
    public Segment.Segment SegmentDToA => new Segment.Segment(D, A);

    
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
    public Quad(Rect.Rect rect)
    {
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;
        this.A = topLeft;
        this.C = bottomRight;
        this.B = new(topLeft.X, bottomRight.Y);
        this.D = new(bottomRight.X, topLeft.Y);
    }
    public Quad(Rect.Rect rect, float rotRad, AnchorPoint pivot)
    {
        var pivotPoint = rect.GetPoint(pivot);
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;

        this.A = (topLeft - pivotPoint).Rotate(rotRad);
        this.B = (new Vector2(topLeft.X, bottomRight.Y) - pivotPoint).Rotate(rotRad);
        this.C = (bottomRight - pivotPoint).Rotate(rotRad);
        this.D = (new Vector2(bottomRight.X, topLeft.Y) - pivotPoint).Rotate(rotRad);
    }
    public Quad(Vector2 pos, Size size, float rotRad, AnchorPoint alignement)
    {
        var offset = size * alignement.ToVector2();
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
    
    #region Shapes

    public Rect.Rect GetBoundingBox()
    {
        Rect.Rect r = new(A.X, A.Y, 0, 0);
        r = r.Enlarge(B);
        r = r.Enlarge(C);
        r = r.Enlarge(D);
        return r;
    }
   
    public Segment.Segment GetEdge(int index)
    {
        var i = index % 4;
        if (i == 0) return SegmentAToB;
        if (i == 1) return SegmentBToC;
        if (i == 2) return SegmentCToD;
        return SegmentDToA;
    }

    public Segments.Segments GetEdges() => new() { SegmentAToB, SegmentBToC, SegmentCToD, SegmentDToA };

    public Polygon.Polygon ToPolygon() => new() { A, B, C, D };
    public Points.Points ToPoints() => new() { A, B, C, D };
    public Polyline.Polyline ToPolyline() => new() { A, B, C, D };
    public Triangulation Triangulate()
    {
        Triangle.Triangle abc = new(A,B,C);
        Triangle.Triangle cda= new(C,D,A);
        return new Triangulation() { abc, cda };
    }

    #endregion

    #region Points & Vertex
    public Segment.Segment GetSegment(int index)
    {
        if (index < 0) return new Segment.Segment();
        var i = index % 4;
        if(i == 0) return new Segment.Segment(A, B);
        if(i == 1) return new Segment.Segment(B, C);
        if(i == 2) return new Segment.Segment(C, D);
        return new Segment.Segment(D, A);
    }
    public Vector2 GetPoint(AnchorPoint alignement) => GetPoint(alignement.X, alignement.Y);
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

    public Vector2 GetRandomPointInside() => GetPoint(Rng.Instance.RandF(), Rng.Instance.RandF());
    public Points.Points GetRandomPointsInside(int amount)
    {
        var points = new Points.Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointInside());
        }

        return points;
    }
    public Vector2 GetRandomVertex() => GetVertex(Rng.Instance.RandI(0, 3));
    public Segment.Segment GetRandomEdge()  => GetEdge(Rng.Instance.RandI(0, 3));
    public Vector2 GetRandomPointOnEdge() => GetRandomEdge().GetRandomPoint();
    public Points.Points GetRandomPointsOnEdge(int amount)
    {
        var points = new Points.Points();

        var ab = SegmentAToB;
        var bc = SegmentBToC;
        var cd = SegmentCToD;
        var da = SegmentDToA;
        
        for (int i = 0; i < amount; i++)
        {
            var rIndex = Rng.Instance.RandI(0, 3);
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

    #region Interpolated Edge Points

    
    public Points.Points? GetInterpolatedEdgePoints(float t)
    {
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(D, t);
        var d1 = D.Lerp(A, t);
        
        return new Points.Points(4){a1, b1, c1, d1};
    }
    public Points.Points? GetInterpolatedEdgePoints(float t, int steps)
    {
        if(steps <= 1) return GetInterpolatedEdgePoints(t);
        
        var a1 = A.Lerp(B, t);
        var b1 = B.Lerp(C, t);
        var c1 = C.Lerp(D, t);
        var d1 = D.Lerp(A, t);

        //first step is already done
        int remainingSteps = steps - 1;

        while (remainingSteps > 0)
        {
            var a2 = a1.Lerp(b1, t);
            var b2 = b1.Lerp(c1, t);
            var c2 = c1.Lerp(d1, t);
            var d2 = d1.Lerp(a1, t);
            
            (a1, b1, c1, d1) = (a2, b2, c2, d2);
            
            remainingSteps--;
        }
        
        return new Points.Points(4){a1, b1, c1, d1};
    }
    
    #endregion
}