using System.Numerics;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;
using ShapeEngine.Random;
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
    public Quad(Rect rect, float rotRad, AnchorPoint pivot)
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

    public float GetPerimeter() => AB.Length() + BC.Length() + CD.Length() + DA.Length();
    public float GetPerimeterSquared() => AB.LengthSquared() + BC.LengthSquared() + CD.LengthSquared() + DA.LengthSquared();

    public float GetArea()
    {
        Triangle abc = new(A,B,C);
        Triangle cda= new(C,D,A);
        return abc.GetArea() + cda.GetArea();
    }

    #endregion

    #region Shapes

    public Rect GetBoundingBox()
    {
        Rect r = new(A.X, A.Y, 0, 0);
        r = r.Enlarge(B);
        r = r.Enlarge(C);
        r = r.Enlarge(D);
        return r;
    }
   
    public Segment GetEdge(int index)
    {
        var i = index % 4;
        if (i == 0) return SegmentAToB;
        if (i == 1) return SegmentBToC;
        if (i == 2) return SegmentCToD;
        return SegmentDToA;
    }

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

    #region Points & Vertex
    public Segment GetSegment(int index)
    {
        if (index < 0) return new Segment();
        var i = index % 4;
        if(i == 0) return new Segment(A, B);
        if(i == 1) return new Segment(B, C);
        if(i == 2) return new Segment(C, D);
        return new Segment(D, A);
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
    public Points GetRandomPointsInside(int amount)
    {
        var points = new Points();
        for (int i = 0; i < amount; i++)
        {
            points.Add(GetRandomPointInside());
        }

        return points;
    }
    public Vector2 GetRandomVertex() => GetVertex(Rng.Instance.RandI(0, 3));
    public Segment GetRandomEdge()  => GetEdge(Rng.Instance.RandI(0, 3));
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
    
    #region Transform
    public Quad ChangeRotation(float rad, AnchorPoint alignement)
    {
        var pivotPoint = GetPoint(alignement);
        var a = (A - pivotPoint).Rotate(rad);
        var b = (B - pivotPoint).Rotate(rad);
        var c = (C - pivotPoint).Rotate(rad);
        var d = (D - pivotPoint).Rotate(rad);
        return new(a,b,c,d);
    }
    public Quad SetRotation(float angleRad, AnchorPoint alignement)
    {
        float amount = ShapeMath.GetShortestAngleRad(AngleRad, angleRad);
        return ChangeRotation(amount, alignement);
    }

    public Quad ChangeRotation(float rad) => ChangeRotation(rad, AnchorPoint.Center);
    public Quad SetRotation(float angleRad) => SetRotation(angleRad, AnchorPoint.Center);

    public Quad ChangePosition(Vector2 offset)
    {
        return new
        (
            A + offset,
            B + offset,
            C + offset,
            D + offset
        );
    }
    public Quad SetPosition(Vector2 newPosition, AnchorPoint alignement)
    {
        var p = GetPoint(alignement);
        var translation = newPosition - p;
        return new
        (
            A + translation,
            B + translation,
            C + translation,
            D + translation
        );
    }

    public Quad SetPosition(Vector2 newPosition) => SetPosition(newPosition, AnchorPoint.Center);

    
    public Quad ScaleSize(float scale) => this * scale;
    public Quad ScaleSize(Size scale) => new Quad(A * scale, B * scale, C * scale, D * scale);
   
    public Quad ScaleSize(float scale, AnchorPoint alignement)
    {
        var p = GetPoint(alignement);
        return new
        (
            A + (A - p) * scale,
            B + (B - p) * scale,
            C + (C - p) * scale,
            D + (D - p) * scale
        );

    }
    public Quad ScaleSize(Size scale, AnchorPoint alignement)
    {
        var p = GetPoint(alignement);
        return new
        (
            A + (A - p) * scale,
            B + (B - p) * scale,
            C + (C - p) * scale,
            D + (D - p) * scale
        );
    }
    
    public Quad ChangeSize(float amount) => ChangeSize(amount, AnchorPoint.Center);
    public Quad ChangeSize(float amount, AnchorPoint alignement)
    {
        Vector2 newA, newB, newC, newD;

        var origin = GetPoint(alignement);
        
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
        
        var wD = (D - origin);
        var lSqD = wD.LengthSquared();
        if (lSqD <= 0f) newD = D;
        else
        {
            var l = MathF.Sqrt(lSqD);
            var dir = wD / l;
            newD = origin + dir * (l + amount);
        }
        return new(newA, newB, newC, newD);
    }
    
    public Quad SetSize(float size) => SetSize(size, AnchorPoint.Center);
    public Quad SetSize(float size, AnchorPoint alignement)
    {
        Vector2 newA, newB, newC, newD;
        
        var origin = GetPoint(alignement);
        
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
        
        var wD = (D - origin);
        var lSqD = wD.LengthSquared();
        if (lSqD <= 0f) newD = D;
        else
        {
            var l = MathF.Sqrt(lSqD);
            var dir = wD / l;
            newD = origin + dir * size;
        }
        return new(newA, newB, newC, newD);
    }
    
    
    /// <summary>
    /// Moves the quad by transform.Position
    /// Rotates the moved quad by transform.RotationRad
    /// Changes the size of the rotated quad by transform.Size!
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Quad ApplyOffset(Transform2D offset)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad);
        return newQuad.ChangeSize(offset.ScaledSize.Length);
    }
    
    /// <summary>
    /// Moves the quad to transform.Position
    /// Rotates the moved quad to transform.RotationRad
    /// Sets the size of the rotated quad to transform.Size.Width
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public Quad SetTransform(Transform2D transform)
    {
        var newQuad = SetPosition(transform.Position);
        newQuad = newQuad.SetRotation(transform.RotationRad);
        return newQuad.SetSize(transform.ScaledSize.Length);
    }
    
    /// <summary>
    /// Moves the quad by transform.Position
    /// Rotates the moved quad by transform.RotationRad
    /// Changes the size of the rotated quad by transform.Size.Width!
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Quad ApplyOffset(Transform2D offset, AnchorPoint alignement)
    {
        var newQuad = ChangePosition(offset.Position);
        newQuad = newQuad.ChangeRotation(offset.RotationRad, alignement);
        return newQuad.ChangeSize(offset.ScaledSize.Length, alignement);
    }
    
    /// <summary>
    /// Moves the quad to transform.Position
    /// Rotates the moved quad to transform.RotationRad
    /// Sets the size of the rotated quad to transform.Size.Width
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="alignement"></param>
    /// <returns></returns>
    public Quad SetTransform(Transform2D transform, AnchorPoint alignement)
    {
        var newQuad = SetPosition(transform.Position, alignement);
        newQuad = newQuad.SetRotation(transform.RotationRad, alignement);
        return newQuad.SetSize(transform.ScaledSize.Length, alignement);
    }
    
    
    
    // public Quad ScaleSize(float scale, Vector2 origin)
    // {
    //     var newA = origin + (A - origin) * scale;
    //     var newB = origin + (B - origin) * scale;
    //     var newC = origin + (C - origin) * scale;
    //     var newD = origin + (D - origin) * scale;
    //     return new(newA, newB, newC, newD);
    // }
    // public Quad ScaleSize(Size scale, Vector2 origin)
    // {
    //     var newA = origin + (A - origin) * scale;
    //     var newB = origin + (B - origin) * scale;
    //     var newC = origin + (C - origin) * scale;
    //     var newD = origin + (D - origin) * scale;
    //     return new(newA, newB, newC, newD);
    // }

    
    
    // public Triangle ChangeRotation(float rad) { return ChangeRotation(rad, GetCentroid()); }
    // public Triangle ChangeRotation(float rad, Vector2 origin)
    // {
    //     var newA = origin + (A - origin).Rotate(rad);
    //     var newB = origin + (B - origin).Rotate(rad);
    //     var newC = origin + (C - origin).Rotate(rad);
    //     return new(newA, newB, newC);
    // }
    //
    // public Triangle SetRotation(float rad)
    // {
    //     var origin = GetCentroid();
    //     var w = A - origin;
    //     var currentAngleRad = w.AngleRad();
    //     var amount = ShapeMath.GetShortestAngleRad(currentAngleRad, rad);
    //     return ChangeRotation(amount, origin);
    // }
    // public Triangle SetRotation(float rad, Vector2 origin)
    // {
    //     var w = A - origin;
    //     if (w.LengthSquared() <= 0f)//origin is A
    //     {
    //         w = B - origin;
    //         if (w.LengthSquared() <= 0f)//origin is B
    //         {
    //             w = C - origin;
    //         }
    //     }
    //
    //     var currentAngleRad = w.AngleRad();
    //     var amount = ShapeMath.GetShortestAngleRad(currentAngleRad, rad);
    //     return ChangeRotation(amount, origin);
    // }
    //
    
    //
    // public Triangle ChangePosition(Vector2 offset) { return new(A + offset, B + offset, C + offset); }
    // public Triangle SetPosition(Vector2 position)
    // {
    //     var centroid = GetCentroid();
    //     var delta = position - centroid;
    //     return ChangePosition(delta);
    // }
    // public Triangle SetPosition(Vector2 position, Vector2 origin)
    // {
    //     var delta = position - origin;
    //     return ChangePosition(delta);
    // }
    //
    

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
    
    
    public static bool ContainsQuadPoint(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 point)
   {
       var oddNodes = false;

       if (Polygon.ContainsPointCheck(qD, qA, point)) oddNodes = !oddNodes;
       if (Polygon.ContainsPointCheck(qA, qB, point)) oddNodes = !oddNodes;
       if (Polygon.ContainsPointCheck(qB, qC, point)) oddNodes = !oddNodes;
       if (Polygon.ContainsPointCheck(qC, qD, point)) oddNodes = !oddNodes;
        
       return oddNodes;
   }
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v)
    {
       return ContainsQuadPoint(qA, qB, qC, qD, u) && 
              ContainsQuadPoint(qA, qB, qC, qD, v);
    }
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v, Vector2 w)
    {
       return ContainsQuadPoint(qA, qB, qC, qD, u) && 
              ContainsQuadPoint(qA, qB, qC, qD, v) && 
              ContainsQuadPoint(qA, qB, qC, qD, w);
    }
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 u, Vector2 v, Vector2 w, Vector2 x)
    {
       return ContainsQuadPoint(qA, qB, qC, qD, u) && 
              ContainsQuadPoint(qA, qB, qC, qD, v) && 
              ContainsQuadPoint(qA, qB, qC, qD, w) && 
              ContainsQuadPoint(qA, qB, qC, qD, x);
    }
    public static bool ContainsQuadPoints(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> points)
    {
       if (points.Count <= 0) return false;
       
       foreach (var point in points)
       {
           if(!ContainsQuadPoint(qA, qB, qC, qD, point)) return false;
       }

       return true;
    }

    public static bool ContainsQuadSegment(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, segmentStart, segmentEnd);
    }
    public static bool ContainsQuadCircle(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 circleCenter, float circleRadius)
    {
       if (!ContainsQuadPoint(qA, qB, qC, qD, circleCenter)) return false;
       
       var result = Segment.IntersectSegmentCircle(qA, qB, circleCenter, circleRadius);
       if (result.a.Valid || result.b.Valid) return false;
           
       result = Segment.IntersectSegmentCircle(qB, qC, circleCenter, circleRadius);
       if (result.a.Valid || result.b.Valid) return false;
       
       result = Segment.IntersectSegmentCircle(qC, qD, circleCenter, circleRadius);
       if (result.a.Valid || result.b.Valid) return false;

       result = Segment.IntersectSegmentCircle(qD, qA, circleCenter, circleRadius);
       if (result.a.Valid || result.b.Valid) return false;
       return true;

    }
    public static bool ContainsQuadTriangle(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 tA, Vector2 tB, Vector2 tc)
    {
      return ContainsQuadPoints(qA, qB, qC, qD, tA, tB, tc);
    }
    public static bool ContainsQuadQuad(Vector2 qA1, Vector2 qB1, Vector2 qC1, Vector2 qD1, Vector2 qA2, Vector2 qB2, Vector2 qC2, Vector2 qD2)
    {
       return ContainsQuadPoints(qA1, qB1, qC1, qD1, qA2, qB2, qC2, qD2);
    }
    public static bool ContainsQuadRect(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, Vector2 rA, Vector2 rB, Vector2 rC, Vector2 rD)
    {
        return ContainsQuadPoints(qA, qB, qC, qD, rA, rB, rC, rD);
    }
    public static bool ContainsQuadPolyline(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD,  List<Vector2> polyline)
    {
       return ContainsQuadPoints(qA, qB, qC, qD, polyline);
    }
    public static bool ContainsQuadPolygon(Vector2 qA, Vector2 qB, Vector2 qC, Vector2 qD, List<Vector2> polygon)
    {
       return ContainsQuadPoints(qA, qB, qC, qD, polygon);
    }
       
    public bool ContainsPoint(Vector2 p) => ContainsQuadPoint(A, B, C, D, p);
    public bool ContainsPoints(Vector2 u, Vector2 v) => ContainsQuadPoints(A, B, C, D, u, v);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w) => ContainsQuadPoints(A, B, C, D, u, v, w);
    public bool ContainsPoints(Vector2 u, Vector2 v, Vector2 w, Vector2 x) => ContainsQuadPoints(A, B, C, D, u, v, w, x);
    public bool ContainsPoints(List<Vector2> points) => ContainsQuadPoints(A, B, C, D, points);

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
           case ShapeType.Rect: return ContainsShape(collider.GetRectShape());
           case ShapeType.Quad: return ContainsShape(collider.GetQuadShape());
           case ShapeType.Poly: return ContainsShape(collider.GetPolygonShape());
           case ShapeType.PolyLine: return ContainsShape(collider.GetPolylineShape());
       }

       return false;
    }

    public bool ContainsShape(Segment segment)
    {
       return ContainsQuadPoints(A, B, C, D, segment.Start, segment.End);
    }
    public bool ContainsShape(Circle circle)
    {
       return ContainsQuadCircle(A, B, C, D, circle.Center, circle.Radius);
    }
    public bool ContainsShape(Rect rect)
    {
       return ContainsQuadPoints(A, B, C, D, rect.A, rect.B, rect.C, rect.D);
    }
    public bool ContainsShape(Triangle triangle)
    {
       return ContainsQuadPoints(A, B, C, D, triangle.A, triangle.B, triangle.C);
    }
    public bool ContainsShape(Quad quad)
    {
       return ContainsQuadPoints(A, B, C, D, quad.A, quad.B, quad.C, quad.D);
    }
    public bool ContainsShape(Polyline polyline)
    {
       return ContainsQuadPoints(A, B, C, D, polyline);
    }
    public bool ContainsShape(Polygon polygon)
    {
       return ContainsQuadPoints(A, B, C, D, polygon);
    }
    public bool ContainsShape(Points points)
    {
       return ContainsQuadPoints(A, B, C, D, points);
    }

    
    /*
    public static  bool ContainsPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p)
    {
        var oddNodes = false;

        if (Polygon.ContainsPointCheck(d, a, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(a, b, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(b, c, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(c, d, p)) oddNodes = !oddNodes;
        
        return oddNodes;
    }
    public bool ContainsPoint(Vector2 p)
    {
        var oddNodes = false;

        if (Polygon.ContainsPointCheck(D, A, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(A, B, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(B, C, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(C, D, p)) oddNodes = !oddNodes;
        
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
    */


    #endregion

    #region Closest Point
    public static Vector2 GetClosestPointQuadPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p, out float disSquared)
    {
        var min = Segment.GetClosestPointSegmentPoint(a, b, p, out float minDisSq);

        var cp = Segment.GetClosestPointSegmentPoint(b, c, p, out float dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(c, d, p, out dis);
        if (dis < minDisSq)
        {
            min = cp;
            minDisSq = dis;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(d, a, p, out dis);
        if (dis < minDisSq)
        {
            disSquared = dis;
            return cp;
        }
        
        disSquared = minDisSq;
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
        
        cp = Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = D - C;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared, out int index)
    {
        var min = Segment.GetClosestPointSegmentPoint(A, B, p, out disSquared);
        index = 0;
        var normal = B - A;
        var cp = Segment.GetClosestPointSegmentPoint(B, C, p, out float dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 1;
            normal = C - B;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(C, D, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 2;
            normal = D - C;
        }
        
        cp = Segment.GetClosestPointSegmentPoint(D, A, p, out dis);
        if (dis < disSquared)
        {
            min = cp;
            disSquared = dis;
            index = 3;
            normal = A - D;
        }

        return new(min, normal.GetPerpendicularRight().Normalize());
    }

    public ClosestPointResult GetClosestPoint(Line other)
    {
        var closestResult = Segment.GetClosestPointSegmentLine(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        
        var result = Segment.GetClosestPointSegmentLine(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        result = Segment.GetClosestPointSegmentLine(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentLine(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Ray other)
    {
        var closestResult = Segment.GetClosestPointSegmentRay(A, B, other.Point, other.Direction, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        var result = Segment.GetClosestPointSegmentRay(B, C, other.Point, other.Direction, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        
        result = Segment.GetClosestPointSegmentRay(C, D, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentRay(D, A, other.Point, other.Direction, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Segment other)
    {
        var closestResult = Segment.GetClosestPointSegmentSegment(A, B, other.Start, other.End, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        
        var result = Segment.GetClosestPointSegmentSegment(B, C, other.Start, other.End, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.Start, other.End, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, other.Normal),
            disSquared,
            selfIndex);
    }
    public ClosestPointResult GetClosestPoint(Circle other)
    {
        var closestResult = Segment.GetClosestPointSegmentCircle(A, B, other.Center, other.Radius, out float disSquared);
        var curNormal = B - A;
        var selfIndex = 0;
        
        var result = Segment.GetClosestPointSegmentCircle(B, C, other.Center, other.Radius, out float dis);
        if (dis < disSquared)
        {
            selfIndex = 1;
            closestResult = result;
            disSquared = dis;
            curNormal = C - B;
        }
        
        result = Segment.GetClosestPointSegmentCircle(C, D, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            closestResult = result;
            disSquared = dis;
            curNormal = D - C;
        }
        
        result = Segment.GetClosestPointSegmentCircle(D, A, other.Center, other.Radius, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            closestResult = result;
            disSquared = dis;
            curNormal = A - D;
        }
        return new(
            new(closestResult.self, curNormal.GetPerpendicularRight().Normalize()), 
            new(closestResult.other, (closestResult.other - other.Center).Normalize()),
            disSquared,
            selfIndex);
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
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.C;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
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
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
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
        
        result = Segment.GetClosestPointSegmentSegment(C, D, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(C, D, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 2;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = D - C;
            curOtherNormal = other.A - other.D;
        }
        
        result = Segment.GetClosestPointSegmentSegment(D, A, other.A, other.B, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 0;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.B - other.A;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.B, other.C, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 1;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.C - other.B;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.C, other.D, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 2;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
            curOtherNormal = other.D - other.C;
        }
        result = Segment.GetClosestPointSegmentSegment(D, A, other.D, other.A, out dis);
        if (dis < disSquared)
        {
            selfIndex = 3;
            otherIndex = 3;
            closestResult = result;
            disSquared = dis;
            curSelfNormal = A - D;
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
        float disSquared = -1;
        var selfIndex = -1;
        var otherIndex = -1;
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
            
            result = Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }
            
            result = Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
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
        var disSquared = -1f;
        var selfIndex = -1;
        var otherIndex = -1;
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
            
            result = Segment.GetClosestPointSegmentSegment(C, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 2;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = D - C;
            }
            
            result = Segment.GetClosestPointSegmentSegment(A, D, p1, p2, out dis);
            if (dis < disSquared || disSquared < 0)
            {
                selfIndex = 3;
                otherIndex = i;
                disSquared = dis;
                closestResult = result;
                curOtherNormal = p2 - p1;
                curSelfNormal = A - D;
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
        var closestResult = closestSegment.GetClosestPoint(p, out float minDisSquared);
        
        var currentSegment = SegmentBToC;
        var result = currentSegment.GetClosestPoint(p, out float dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }
        
        currentSegment = SegmentCToD;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }
        
        currentSegment = SegmentDToA;
        result = currentSegment.GetClosestPoint(p, out dis);
        if (dis < minDisSquared)
        {
            closestSegment = currentSegment;
            minDisSquared = dis;
            closestResult = result;
        }

        disSquared = minDisSquared;
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
    
        l = (D - p).LengthSquared();
        if (l < disSquared)
        {
            disSquared = l;
            closest = D;
            index = 3;
        }
    
        return closest;
    }
    public Vector2 GetFurthestVertex(Vector2 p, out float disSquared, out int index)
    {
        var furthest = A;
        disSquared = (A - p).LengthSquared();
        index = 0;
    
        float l = (B - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = B;
            disSquared = l;
            index = 1;
        }
        l = (C - p).LengthSquared();
        if (l > disSquared)
        {
            furthest = C;
            disSquared = l;
            index = 2;
        }
    
        l = (D - p).LengthSquared();
        if (l > disSquared)
        {
            disSquared = l;
            furthest = D;
            index = 3;
        }
    
        return furthest;
    }

    
    #endregion
    
    #region Overlap
    public static bool OverlapQuadSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 segmentStart, Vector2 segmentEnd)
    {
        return Segment.OverlapSegmentQuad(segmentStart, segmentEnd, a, b, c, d);
    }
    public static bool OverlapQuadLine(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 linePoint, Vector2 lineDirection)
    {
        return Line.OverlapLineQuad(linePoint, lineDirection, a, b, c, d);
    }
    public static bool OverlapQuadRay(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 rayPoint, Vector2 rayDirection)
    {
        return Ray.OverlapRayQuad(rayPoint, rayDirection, a, b, c, d);
    }
    public static bool OverlapQuadCircle(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 circleCenter, float circleRadius)
    {
        return Circle.OverlapCircleQuad(circleCenter, circleRadius, a, b, c, d);
    }
    public static bool OverlapQuadTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 ta, Vector2 tb, Vector2 tc)
    {
        return Triangle.OverlapTriangleQuad(ta, tb, tc, a, b, c, d);

    }
    public static bool OverlapQuadQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d,Vector2 qa, Vector2 qb, Vector2 qc, Vector2 qd)
    {
        if (ContainsQuadPoint(a, b, c, d, qa)) return true;
        if (ContainsQuadPoint(qa, qb, qc,qd,a)) return true;

        if( Segment.OverlapSegmentSegment(a, b, qa, qb) ) return true;
        if( Segment.OverlapSegmentSegment(a, b, qb, qc) ) return true;
        if( Segment.OverlapSegmentSegment(a, b, qc, qd) ) return true;
        if( Segment.OverlapSegmentSegment(a, b, qd, qa) ) return true;
        if( Segment.OverlapSegmentSegment(b, c, qa, qb) ) return true;
        if( Segment.OverlapSegmentSegment(b, c, qb, qc) ) return true;
        if( Segment.OverlapSegmentSegment(b, c, qc, qd) ) return true;
        if( Segment.OverlapSegmentSegment(b, c, qd, qa) ) return true;
        if( Segment.OverlapSegmentSegment(c, d, qa, qb) ) return true;
        if( Segment.OverlapSegmentSegment(c, d, qb, qc) ) return true;
        if( Segment.OverlapSegmentSegment(c, d, qc, qd) ) return true;
        if( Segment.OverlapSegmentSegment(c, d, qd, qa) ) return true;
        if( Segment.OverlapSegmentSegment(d, a, qa, qb) ) return true;
        if( Segment.OverlapSegmentSegment(d, a, qb, qc) ) return true;
        if( Segment.OverlapSegmentSegment(d, a, qc, qd) ) return true;
        if( Segment.OverlapSegmentSegment(d, a, qd, qa) ) return true;

        return false;
    }
    public static bool OverlapQuadRect(Vector2 a, Vector2 b, Vector2 c,  Vector2 d,Vector2 ra, Vector2 rb, Vector2 rc, Vector2 rd)
    {
        return OverlapQuadQuad(a, b, c, d, ra, rb, rc, rd);
    }
    public static bool OverlapQuadPolygon(Vector2 a, Vector2 b, Vector2 c, Vector2 d,List<Vector2> points)
    {
        if (points.Count < 3) return false;
        // if (Polygon.ContainsPoints(points, a)) return true;
        var oddNodes = false;
        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[(i + 1) % points.Count];
            if( Segment.OverlapSegmentSegment(a, b, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(b, c, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(c, d, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(d, a, p1, p2) ) return true;
            if(Polygon.ContainsPointCheck(p1, p2, a)) oddNodes = !oddNodes;
        }

        return oddNodes;
    }
    public static bool OverlapQuadPolyline(Vector2 a, Vector2 b, Vector2 c,  Vector2 d,List<Vector2> points)
    {
        if (points.Count < 2) return false;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1];
            if( Segment.OverlapSegmentSegment(a, b, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(b, c, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(c, d, p1, p2) ) return true;
            if( Segment.OverlapSegmentSegment(d, a, p1, p2) ) return true;
        }
        return false;
    }
    public static bool OverlapQuadSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d,List<Segment> segments)
    {
        if (segments.Count < 3) return false;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if( Segment.OverlapSegmentSegment(a, b, segment.Start, segment.End) ) return true;
            if( Segment.OverlapSegmentSegment(b, c, segment.Start, segment.End) ) return true;
            if( Segment.OverlapSegmentSegment(c, d, segment.Start, segment.End) ) return true;
            if( Segment.OverlapSegmentSegment(d, a, segment.Start, segment.End) ) return true;
        }
        return false;
    }

    public bool OverlapSegment(Vector2 segmentStart, Vector2 segmentEnd) => OverlapQuadSegment(A, B, C, D, segmentStart, segmentEnd);
    public bool OverlapLine(Vector2 linePoint, Vector2 lineDirection) => OverlapQuadLine(A, B, C,D,linePoint, lineDirection);
    public bool OverlapRay(Vector2 rayPoint, Vector2 rayDirection) => OverlapQuadRay(A, B, C,D,rayPoint, rayDirection);
    public bool OverlapCircle(Vector2 circleCenter, float circleRadius) => OverlapQuadCircle(A, B, C,D,circleCenter, circleRadius);
    public bool OverlapTriangle(Vector2 a, Vector2 b, Vector2 c) => OverlapQuadTriangle(A, B, C,D,a, b, c);
    public bool OverlapQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapQuadQuad(A, B, C,D,a, b, c, d);
    public bool OverlapRect(Vector2 a, Vector2 b, Vector2 c, Vector2 d) => OverlapQuadQuad(A, B, C,D,a, b, c, d);
    public bool OverlapPolygon(List<Vector2> points) => OverlapQuadPolygon(A, B, C,D,points);
    public bool OverlapPolyline(List<Vector2> points) => OverlapQuadPolyline(A, B, C,D,points);
    public bool OverlapSegments(List<Segment> segments) => OverlapQuadSegments(A, B, C,D,segments);
    
    public bool OverlapShape(Line line) => OverlapQuadLine(A, B, C, D,line.Point, line.Direction);
    public bool OverlapShape(Ray ray) => OverlapQuadRay(A, B, C, D,ray.Point, ray.Direction);
    
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
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(B, C, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(C, D, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(D, A, seg.Start, seg.End);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
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
            
        result = Segment.IntersectSegmentRay(B, C, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentRay(C, D, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        result = Segment.IntersectSegmentRay(D, A, r.Point, r.Direction, r.Normal);
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
            
        result = Segment.IntersectSegmentLine(B, C, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
            
        result = Segment.IntersectSegmentLine(C, D, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        result = Segment.IntersectSegmentLine(D, A, l.Point, l.Direction, l.Normal);
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
            
        result = Segment.IntersectSegmentSegment(C, D, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        result = Segment.IntersectSegmentSegment(D, A, s.Start, s.End);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        return points;
    }
    public  CollisionPoints? IntersectShape(Circle c)
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
        
        result = Segment.IntersectSegmentCircle(C, D, c.Center, c.Radius);
        if (result.a.Valid || result.b.Valid)
        {
            points ??= new();
            if(result.a.Valid) points.Add(result.a);
            if(result.b.Valid) points.Add(result.b);
        }
        
        result = Segment.IntersectSegmentCircle(D, A, c.Center, c.Radius);
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
        
        result = Segment.IntersectSegmentSegment(C, D, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, t.C, t.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        result = Segment.IntersectSegmentSegment(D, A, t.A, t.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, t.B, t.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, t.C, t.A);
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

        
        result = Segment.IntersectSegmentSegment(C, D, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, d, a);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        
        result = Segment.IntersectSegmentSegment(D, A, a, b);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, b, c);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, c, d);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, d, a);
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

        
        result = Segment.IntersectSegmentSegment(C, D, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(C, D, q.D, q.A);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        
        result = Segment.IntersectSegmentSegment(D, A, q.A, q.B);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, q.B, q.C);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, q.C, q.D);
        if (result.Valid)
        {
            points ??= new();
            points.Add(result);
        }
        result = Segment.IntersectSegmentSegment(D, A, q.D, q.A);
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
            var result = Segment.IntersectSegmentSegment(A, B, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(B, C, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(C, D, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(D, A, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
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
            var result = Segment.IntersectSegmentSegment(A, B, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(B, C, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(C, D, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
            }
            
            result = Segment.IntersectSegmentSegment(D, A, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points ??= new();
                points.Add(result);
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
            
        result = Segment.IntersectSegmentRay(B, C, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentRay(C, D, r.Point, r.Direction, r.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        if(count >= 2) return count;
        
        result = Segment.IntersectSegmentRay(D, A, r.Point, r.Direction, r.Normal);
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
            
        result = Segment.IntersectSegmentLine(B, C, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        if (count >= 2) return count;
        
        result = Segment.IntersectSegmentLine(C, D, l.Point, l.Direction, l.Normal);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        if(count >= 2) return count;
        
        result = Segment.IntersectSegmentLine(D, A, l.Point, l.Direction, l.Normal);
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
        
        result = Segment.IntersectSegmentSegment(C, D, s.Start, s.End);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        if(count >= 2) return count;
        
        result = Segment.IntersectSegmentSegment(D, A, s.Start, s.End);
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
        
        result = Segment.IntersectSegmentCircle(C, D, c.Center, c.Radius);
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
        
        result = Segment.IntersectSegmentCircle(D, A, c.Center, c.Radius);
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
        
        result = Segment.IntersectSegmentSegment(C, D, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, t.C, t.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentSegment(D, A, t.A, t.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, t.B, t.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, t.C, t.A);
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
        var result = Segment.IntersectSegmentSegment(A, B, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(A, B, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(A, B, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(A, B, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        
        result = Segment.IntersectSegmentSegment(B, C, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(B, C, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }

        
        result = Segment.IntersectSegmentSegment(C, D, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        result = Segment.IntersectSegmentSegment(D, A, q.A, q.B);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, q.B, q.C);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, q.C, q.D);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, q.D, q.A);
        if (result.Valid)
        {
            points.Add(result);
            count++;
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

        
        result = Segment.IntersectSegmentSegment(C, D, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(C, D, d, a);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        
        
        result = Segment.IntersectSegmentSegment(D, A, a, b);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, b, c);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, c, d);
        if (result.Valid)
        {
            points.Add(result);
            if (returnAfterFirstValid) return 1;
            count++;
        }
        result = Segment.IntersectSegmentSegment(D, A, d, a);
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
            
            result = Segment.IntersectSegmentSegment(C, D, p[i], p[(i + 1) % p.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(D, A, p[i], p[(i + 1) % p.Count]);
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
            
            result = Segment.IntersectSegmentSegment(C, D, pl[i], pl[(i + 1) % pl.Count]);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(D, A, pl[i], pl[(i + 1) % pl.Count]);
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
            
            result = Segment.IntersectSegmentSegment(C, D, seg.Start, seg.End);
            if (result.Valid)
            {
                points.Add(result);
                if (returnAfterFirstValid) return 1;
                count++;
            }
            
            result = Segment.IntersectSegmentSegment(D, A, seg.Start, seg.End);
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