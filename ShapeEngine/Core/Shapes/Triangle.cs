
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes
{
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

            var closest = GetClosestSegment(p);
            return new Triangle(p, closest.Segment);
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
        
        #region Closest

        public static Vector2 GetClosestPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            var ab = Segment.GetClosestPoint(a, b, p);
            var bc = Segment.GetClosestPoint(b, c, p);
            var ca = Segment.GetClosestPoint(c, a, p);

            var min = ab;
            var minDisSq = (ab - p).LengthSquared();

            var bcDisSq = (bc - p).LengthSquared();
            if (bcDisSq < minDisSq)
            {
                min = bc;
                minDisSq = bcDisSq;
            }
            
            return (ca - p).LengthSquared() < minDisSq ? ca : min;
        }
        public ClosestDistance GetClosestDistanceTo(Vector2 p)
        {
            var cp = GetClosestPoint(A, B, C, p);
            return new ClosestDistance(cp, p);
        }
        public ClosestDistance GetClosestDistanceTo(Segment segment)
        {
            var next = GetClosestPoint(A, B, C, segment.Start);
            var disSq = (next - segment.Start).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = segment.Start;

            next = GetClosestPoint(A, B, C, segment.End);
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
            
            point = Segment.GetClosestPoint(C, A, circle.Center);
            disSq = (circle.Center - point).LengthSquared();
            if (disSq < minDisSq) closestPoint = point;

            var dir = (closestPoint - circle.Center).Normalize();

            return new(closestPoint, circle.Center + dir * circle.Radius);

        }
        public ClosestDistance GetClosestDistanceTo(Triangle triangle)
        {
            var next = GetClosestPoint(A, B, C, triangle.A);
            var disSq = (next - triangle.A).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = triangle.A;

            next = GetClosestPoint(A, B, C, triangle.B);
            disSq = (next - triangle.B).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = next;
                cpOther = triangle.B;
            }
            
            next = GetClosestPoint(A, B, C, triangle.C);
            disSq = (next - triangle.C).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = next;
                cpOther = triangle.C;
            }

            next = GetClosestPoint(triangle.A, triangle.B, triangle.C, A);
            disSq = (next - A).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = A;
                cpOther = next;
            }
            
            next = GetClosestPoint(triangle.A, triangle.B, triangle.C, B);
            disSq = (next - B).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = B;
                cpOther = next;
            }
            
            next = GetClosestPoint(triangle.A, triangle.B, triangle.C, C);
            disSq = (next - C).LengthSquared();
            if (disSq < minDisSq)
            {
                cpSelf = C;
                cpOther = next;
            }
            
            return new(cpSelf, cpOther);
        }
        public ClosestDistance GetClosestDistanceTo(Quad quad)
        {
            var next = GetClosestPoint(A, B, C, quad.A);
            var disSq = (next - quad.A).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = quad.A;

            next = GetClosestPoint(A, B, C, quad.B);
            disSq = (next - quad.B).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = next;
                cpOther = quad.B;
            }
            
            next = GetClosestPoint(A, B, C, quad.C);
            disSq = (next - quad.C).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = next;
                cpOther = quad.C;
            }
            next = GetClosestPoint(A, B, C, quad.D);
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
                cpSelf = C;
                cpOther = next;
            }
            
            return new(cpSelf, cpOther);
        }
        public ClosestDistance GetClosestDistanceTo(Rect rect)
        {
            var next = GetClosestPoint(A, B, C, rect.TopLeft);
            var disSq = (next - rect.TopLeft).LengthSquared();
            var minDisSq = disSq;
            var cpSelf = next;
            var cpOther = rect.TopLeft;

            next = GetClosestPoint(A, B, C, rect.BottomLeft);
            disSq = (next - rect.BottomLeft).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = next;
                cpOther = rect.BottomLeft;
            }
            
            next = GetClosestPoint(A, B, C, rect.BottomRight);
            disSq = (next - rect.BottomRight).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = next;
                cpOther = rect.BottomRight;
            }
            next = GetClosestPoint(A, B, C, rect.TopRight);
            disSq = (next - rect.TopRight).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = next;
                cpOther = rect.TopRight;
            }

            next = Quad.GetClosestPoint(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, A);
            disSq = (next - A).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = A;
                cpOther = next;
            }
            
            next = Quad.GetClosestPoint(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, B);
            disSq = (next - B).LengthSquared();
            if (disSq < minDisSq)
            {
                minDisSq = disSq;
                cpSelf = B;
                cpOther = next;
            }
            
            next = Quad.GetClosestPoint(rect.TopLeft, rect.BottomLeft, rect.BottomRight, rect.TopRight, C);
            disSq = (next - C).LengthSquared();
            if (disSq < minDisSq)
            {
                cpSelf = C;
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
                
                var next = GetClosestPoint(A, B, C, p1);
                var cd = new ClosestDistance(next, p1);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(A, B, C, p2);
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
                
                var next = GetClosestPoint(A, B, C, p1);
                var cd = new ClosestDistance(next, p1);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = GetClosestPoint(A, B, C, p2);
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
            }
            return closestDistance;
        }
        
        
        public Vector2 GetClosestVertex(Vector2 p)
        {
            var closest = A;
            float minDisSquared = (A - p).LengthSquared();

            float l = (B - p).LengthSquared();
            if (l < minDisSquared)
            {
                closest = B;
                minDisSquared = l;
            }

            l = (C - p).LengthSquared();
            if (l < minDisSquared) closest = C;

            return closest;
        }

        // internal ClosestPoint GetClosestPoint(Vector2 p)
        // {
        //     var cp = GetClosestCollisionPoint(p);
        //     return new(cp, (cp.Point - p).Length());
        // }
        public ClosestSegment GetClosestSegment(Vector2 p)
        {
            var currentSegment = SegmentAToB;
            var closestSegment = currentSegment;
            var closestDistance = currentSegment.GetClosestDistanceTo(p);


            currentSegment = SegmentBToC;
            var cd = currentSegment.GetClosestDistanceTo(p);
            if (cd.DistanceSquared < closestDistance.DistanceSquared)
            {
                closestDistance = cd;
                closestSegment = currentSegment;
            }
            
            currentSegment = SegmentCToA;
            cd = currentSegment.GetClosestDistanceTo(p);
            if (cd.DistanceSquared < closestDistance.DistanceSquared)
            {
                closestDistance = cd;
                closestSegment = currentSegment;
            }
            
            return new(closestSegment, closestDistance);

        }
        public CollisionPoint GetClosestCollisionPoint(Vector2 p)
        {
            var cp = SegmentAToB.GetClosestCollisionPoint(p);
            float minDisSquared = (cp.Point - p).LengthSquared();

            var curCP = SegmentBToC.GetClosestCollisionPoint(p);
            float l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                cp = curCP;
            }
            curCP = SegmentCToA.GetClosestCollisionPoint(p);
            l = (curCP.Point - p).LengthSquared();
            if (l < minDisSquared)
            {
                minDisSquared = l;
                cp = curCP;
            }

            return cp;
        }

        #endregion
        
        #region Overlap
        public readonly bool Overlap(Collider collider)
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
        
        public readonly bool OverlapShape(Segments segments)
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

        public readonly bool OverlapShape(Segment s) => s.OverlapShape(this);
        public readonly bool OverlapShape(Circle c) => c.OverlapShape(this);
        public readonly bool OverlapShape(Triangle b)
        {
            if (ContainsPoint(b.A)) return true;
            // if (ContainsPoint(b.B)) return true;
            // if (ContainsPoint(b.C)) return true;
            
            if (b.ContainsPoint(A)) return true;
            // if (b.ContainsPoint(B)) return true;
            // if (b.ContainsPoint(C)) return true;
            
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
        public readonly bool OverlapShape(Rect r)
        {
            var a = r.TopLeft;
            if (ContainsPoint(a)) return true;
            
            // if (ContainsPoint(b)) return true;
            // if (ContainsPoint(c)) return true;
            // if (ContainsPoint(d)) return true;
            
            if (r.ContainsPoint(A)) return true;
            // if (r.ContainsPoint(B)) return true;
            // if (r.ContainsPoint(C)) return true;
            
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
        public readonly bool OverlapShape(Quad q)
        {
            if (ContainsPoint(q.A)) return true;
            // if (ContainsPoint(q.B)) return true;
            // if (ContainsPoint(q.C)) return true;
            // if (ContainsPoint(q.D)) return true;
            
            if (q.ContainsPoint(A)) return true;
            // if (q.ContainsPoint(B)) return true;
            // if (q.ContainsPoint(C)) return true;
            
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
        public readonly bool OverlapShape(Polygon poly)
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
        public readonly bool OverlapShape(Polyline pl)
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
        
        public readonly CollisionPoints? Intersect(Collider collider)
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

        public readonly CollisionPoints? IntersectShape(Segments segments)
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
                
                result = Segment.IntersectSegmentSegment(C, A, seg.Start, seg.End);
                if (result != null)
                {
                    points ??= new();
                    points.AddRange((CollisionPoint)result);
                }
                
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Segment s)
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
                
            result = Segment.IntersectSegmentSegment(C, A, s.Start, s.End);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            return points;
        }
        public readonly CollisionPoints? IntersectShape(Circle c)
        {
            CollisionPoints? points = null;
            var result = Segment.IntersectSegmentCircle(A, B, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                // return points;
            }
                
            result = Segment.IntersectSegmentCircle(B, C, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                // return points;
            }
            
            result = Segment.IntersectSegmentCircle(C, A, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                // return points;
            }

            return points;
        }
        public readonly CollisionPoints? IntersectShape(Triangle b)
        {
            CollisionPoints? points = null;
            var result = Segment.IntersectSegmentSegment(A, B, b.A, b.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(A, B, b.B, b.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(A, B, b.C, b.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
                
            result = Segment.IntersectSegmentSegment(B, C, b.A, b.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, b.B, b.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(B, C, b.C, b.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            
            result = Segment.IntersectSegmentSegment(C, A, b.A, b.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, b.B, b.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, b.C, b.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }

            return points;
        }
        public readonly CollisionPoints? IntersectShape(Rect r)
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

            
            result = Segment.IntersectSegmentSegment(C, A, a, b);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, b, c);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, c, d);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, d, a);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Quad q)
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

            
            result = Segment.IntersectSegmentSegment(C, A, q.A, q.B);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, q.B, q.C);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, q.C, q.D);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            result = Segment.IntersectSegmentSegment(C, A, q.D, q.A);
            if (result != null)
            {
                points ??= new();
                points.AddRange((CollisionPoint)result);
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Polygon p)
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
                
                colPoint = Segment.IntersectSegmentSegment(C, A, p[i], p[(i + 1) % p.Count]);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Polyline pl)
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
                
                colPoint = Segment.IntersectSegmentSegment(C, A, pl[i], pl[(i + 1) % pl.Count]);
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
}

