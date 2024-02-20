
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes
{
    public readonly struct Circle : IEquatable<Circle>
    {
        #region Members
        public readonly Vector2 Center;
        public readonly float Radius;
        #endregion

        #region Getter Setter
        public float Diameter => Radius * 2f;
        #endregion
        
        #region Constructors
        public Circle(Vector2 center, float radius) { this.Center = center; this.Radius = radius; }
        public Circle(float x, float y, float radius) { this.Center = new(x, y); this.Radius = radius; }
        public Circle(Circle c, float radius) { Center = c.Center; Radius = radius;}
        public Circle(Circle c, Vector2 center) { Center = center; Radius = c.Radius; }
        public Circle(Rect r) { Center = r.Center; Radius = MathF.Max(r.Width, r.Height); }
        #endregion

        #region Equality & Hashcode
        public bool Equals(Circle other)
        {
            return Center == other.Center && ShapeMath.EqualsF(Radius, other.Radius);// Radius == other.Radius;
        }
        public readonly override int GetHashCode() => HashCode.Combine(Center, Radius);

        public static bool operator ==(Circle left, Circle right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Circle left, Circle right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            if (obj is Circle c) return Equals(c);
            return false;
        }
        #endregion

        #region Public
        public readonly Polygon Project(Vector2 v)
        {
            if (v.LengthSquared() <= 0f) return ToPolygon(8);
            var corners = GetCorners();
            var points = new Points
            {
                corners.top,
                corners.right,
                corners.bottom,
                corners.left,
                corners.top + v,
                corners.right + v,
                corners.bottom + v,
                corners.left +v
            };
            return Polygon.FindConvexHull(points);
        }
        
        public bool ContainsShape(Segment other)
        {
            return ContainsPoint(other.Start) && ContainsPoint(other.End);
        }
        public bool ContainsShape(Circle other)
        {
            float rDif = Radius - other.Radius;
            if(rDif <= 0) return false;

            float disSquared = (Center - other.Center).LengthSquared();
            return disSquared < rDif * rDif;
        }
        public bool ContainsShape(Rect other)
        {
            return ContainsPoint(other.TopLeft) &&
                ContainsPoint(other.BottomLeft) &&
                ContainsPoint(other.BottomRight) &&
                ContainsPoint(other.TopRight);
        }
        public bool ContainsShape(Triangle other)
        {
            return ContainsPoint(other.A) &&
                ContainsPoint(other.B) &&
                ContainsPoint(other.C);
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
        
        
        public readonly Circle Floor() { return new(Center.Floor(), MathF.Floor(Radius)); }
        public readonly Circle Ceiling() { return new(Center.Ceiling(), MathF.Ceiling(Radius)); }
        public readonly Circle Round() { return new(Center.Round(), MathF.Round(Radius)); }
        public readonly Circle Truncate() { return new(Center.Truncate(), MathF.Truncate(Radius)); }
                
        public readonly Vector2 GetPoint(float angleRad, float f) { return Center + new Vector2(Radius * f, 0f).Rotate(angleRad); }
        public readonly Segments GetEdges(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Segments segments = new();
            for (int i = 0; i < pointCount; i++)
            {
                var start = Center + new Vector2(Radius, 0f).Rotate(-angleStep * i);
                var end = Center + new Vector2(Radius, 0f).Rotate(-angleStep * ((i + 1) % pointCount));

                segments.Add(new Segment(start, end));
            }
            return segments;
        }
        public readonly Points GetVertices(int count = 16)
        {
            float angleStep = (MathF.PI * 2f) / count;
            Points points = new();
            for (int i = 0; i < count; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                points.Add(p);
            }
            return points;
        }
        public readonly Polygon ToPolygon(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polygon poly = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                poly.Add(p);
            }
            return poly;
        }
        public readonly Polyline ToPolyline(int pointCount = 16)
        {
            float angleStep = (MathF.PI * 2f) / pointCount;
            Polyline polyLine = new();
            for (int i = 0; i < pointCount; i++)
            {
                Vector2 p = Center + new Vector2(Radius, 0f).Rotate(angleStep * i);
                polyLine.Add(p);
            }
            return polyLine;
        }
        public readonly (Vector2 top, Vector2 right, Vector2 bottom, Vector2 left) GetCorners()
        {
            var top = Center + new Vector2(0, -Radius);
            var right = Center + new Vector2(Radius, 0);
            var bottom = Center + new Vector2(0, Radius);
            var left = Center + new Vector2(-Radius, 0);
            return (top, right, bottom, left);
        }
        public readonly List<Vector2> GetCornersList()
        {
            var top = Center + new Vector2(0, -Radius);
            var right = Center + new Vector2(Radius, 0);
            var bottom = Center + new Vector2(0, Radius);
            var left = Center + new Vector2(-Radius, 0);
            return new() { top, right, bottom, left };
        }
        public readonly (Vector2 tl, Vector2 tr, Vector2 br, Vector2 bl) GetRectCorners()
        {
            var tl = Center + new Vector2(-Radius, -Radius);
            var tr = Center + new Vector2(Radius, -Radius);
            var br = Center + new Vector2(Radius, Radius);
            var bl = Center + new Vector2(-Radius, Radius);
            return (tl, tr, br, bl);
        }
        public readonly List<Vector2> GetRectCornersList()
        {
            var tl = Center + new Vector2(-Radius, -Radius);
            var tr = Center + new Vector2(Radius, -Radius);
            var br = Center + new Vector2(Radius, Radius);
            var bl = Center + new Vector2(-Radius, Radius);
            return new() {tl, tr, br, bl};
        }
        /*
        public Circle ScaleRadius(float scale) { return new(Center, Radius * scale); }
        public Circle ChangeRadius(float amount) { return new(Center, Radius + amount); }
        public Circle SetRadius(float newRadius) { return new(Center, newRadius); }
        public Circle MoveCenter(Vector2 offset) { return new(Center + offset, Radius); }
        public Circle SetCenter(Vector2 newCenter) { return new(newCenter, Radius); }
        */
        public readonly Circle Combine(Circle other)
        {
            return new
                (
                    (Center + other.Center) / 2,
                    Radius + other.Radius
                );
        }
        public static Circle Combine(params Circle[] circles)
        {
            if (circles.Length <= 0) return new();
            Vector2 combinedCenter = new();
            float totalRadius = 0f;
            for (int i = 0; i < circles.Length; i++)
            {
                var circle = circles[i];
                combinedCenter += circle.Center;
                totalRadius += circle.Radius;
            }
            return new(combinedCenter / circles.Length, totalRadius);
        }
        #endregion

        #region Operators

        public static Circle operator +(Circle left, Circle right)
        {
            return new
                (
                    left.Center + right.Center,
                    left.Radius + right.Radius
                );
        }
        public static Circle operator -(Circle left, Circle right)
        {
            return new
            (
                left.Center - right.Center,
                left.Radius - right.Radius
            );
        }
        public static Circle operator *(Circle left, Circle right)
        {
            return new
            (
                left.Center * right.Center,
                left.Radius * right.Radius
            );
        }
        public static Circle operator /(Circle left, Circle right)
        {
            return new
            (
                left.Center / right.Center,
                left.Radius / right.Radius
            );
        }
        public static Circle operator +(Circle left, Vector2 right)
        {
            return new
            (
                left.Center + right,
                left.Radius
            );
        }
        public static Circle operator -(Circle left, Vector2 right)
        {
            return new
            (
                left.Center - right,
                left.Radius
            );
        }
        public static Circle operator *(Circle left, Vector2 right)
        {
            return new
            (
                left.Center * right,
                left.Radius
            );
        }
        public static Circle operator /(Circle left, Vector2 right)
        {
            return new
            (
                left.Center / right,
                left.Radius
            );
        }
        public static Circle operator +(Circle left, float right)
        {
            return new
            (
                left.Center,
                left.Radius + right
            );
        }
        public static Circle operator -(Circle left, float right)
        {
            return new
            (
                left.Center,
                left.Radius - right
            );
        }
        public static Circle operator *(Circle left, float right)
        {
            return new
            (
                left.Center,
                left.Radius * right
            );
        }
        public static Circle operator /(Circle left, float right)
        {
            return new
            (
                left.Center,
                left.Radius / right
            );
        }
        #endregion
        
        #region IShape
        public Triangulation Triangulate() { return ToPolygon().Triangulate(); }
        public float GetArea() { return MathF.PI * Radius * Radius; }
        public float GetCircumference() { return MathF.PI * Radius * 2f; }
        public float GetCircumferenceSquared() { return GetCircumference() * GetCircumference(); }
        public Rect GetBoundingBox() { return new Rect(Center, new Vector2(Radius, Radius) * 2f, new(0.5f)); }

        public bool ContainsPoint(Vector2 p) => (Center - p).LengthSquared() <= Radius * Radius;

        public CollisionPoint GetClosestCollisionPoint(Vector2 p) 
        {
            Vector2 normal = (p - Center).Normalize();
            Vector2 point = Center + normal * Radius;
            return new(point, normal);
        }
        public Vector2 GetClosestVertex(Vector2 p) { return Center + (p - Center).Normalize() * Radius; }
        public Vector2 GetRandomPoint()
        {
            float randAngle = ShapeRandom.RandAngleRad();
            var randDir = ShapeVec.VecFromAngleRad(randAngle);
            return Center + randDir * ShapeRandom.RandF(0, Radius);
        }
        public Points GetRandomPoints(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPoint());
            }
            return points;
        }
        public Vector2 GetRandomVertex() { return ShapeRandom.RandCollection(GetVertices(), false); }
        public Segment GetRandomEdge() { return ShapeRandom.RandCollection(GetEdges(), false); }
        public Vector2 GetRandomPointOnEdge() { return GetRandomEdge().GetRandomPoint(); }
        public Points GetRandomPointsOnEdge(int amount)
        {
            var points = new Points();
            for (int i = 0; i < amount; i++)
            {
                points.Add(GetRandomPointOnEdge());
            }
            return points;
        }
        // public void DrawShape(float linethickness, ColorRgba colorRgba) => this.DrawLines(linethickness, colorRgba);
        #endregion

        #region Static
        public static bool OverlapCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
        {
            if (aRadius <= 0.0f && bRadius > 0.0f) return ContainsCirclePoint(bPos, bRadius, aPos);
            if (bRadius <= 0.0f && aRadius > 0.0f) return ContainsCirclePoint(aPos, aRadius, bPos);
            if (aRadius <= 0.0f && bRadius <= 0.0f) return aPos == bPos;

            float rSum = aRadius + bRadius;
            return (aPos - bPos).LengthSquared() < rSum * rSum;
        }
        public static bool ContainsCirclePoint(Vector2 cPos, float cRadius, Vector2 p) => (cPos - p).LengthSquared() <= cRadius * cRadius;
        public static bool OverlapCircleSegment(Vector2 cPos, float cRadius, Vector2 segStart, Vector2 segEnd)
        {
            if (cRadius <= 0.0f) return Segment.IsPointOnSegment(cPos, segStart, segEnd);
            if (ContainsCirclePoint(cPos, cRadius, segStart)) return true;
            if (ContainsCirclePoint(cPos, cRadius, segEnd)) return true;

            var d = segEnd - segStart;
            var lc = cPos - segStart;
            var p = lc.Project(d);
            var nearest = segStart + p;

            return
                ContainsCirclePoint(cPos, cRadius, nearest) &&
                p.LengthSquared() <= d.LengthSquared() &&
                Vector2.Dot(p, d) >= 0.0f;
        }
        public static bool OverlapCircleLine(Vector2 cPos, float cRadius, Vector2 linePos, Vector2 lineDir)
        {
            var lc = cPos - linePos;
            var p = lc.Project(lineDir);
            var nearest = linePos + p;
            return ContainsCirclePoint(cPos, cRadius, nearest);
        }
        public static bool OverlapCircleRay(Vector2 cPos, float cRadius, Vector2 rayPos, Vector2 rayDir)
        {
            var w = cPos - rayPos;
            float p = w.X * rayDir.Y - w.Y * rayDir.X;
            if (p < -cRadius || p > cRadius) return false;
            float t = w.X * rayDir.X + w.Y * rayDir.Y;
            if (t < 0.0f)
            {
                float d = w.LengthSquared();
                if (d > cRadius * cRadius) return false;
            }
            return true;
        }

        public static (CollisionPoint? a, CollisionPoint? b) IntersectCircleCircle(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius) { return IntersectCircleCircle(aPos.X, aPos.Y, aRadius, bPos.X, bPos.Y, bRadius); }
        public static (CollisionPoint? a, CollisionPoint? b) IntersectCircleCircle(float cx0, float cy0, float radius0, float cx1, float cy1, float radius1)
        {
            // Find the distance between the centers.
            float dx = cx0 - cx1;
            float dy = cy0 - cy1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // See how many solutions there are.
            if (dist > radius0 + radius1)
            {
                // No solutions, the circles are too far apart.
                return (null, null);
            }
            else if (dist < Math.Abs(radius0 - radius1))
            {
                // No solutions, one circle contains the other.
                return (null, null);
            }
            else if ((dist == 0) && ShapeMath.EqualsF(radius0, radius1))// (radius0 == radius1))
            {
                // No solutions, the circles coincide.
                return (null, null);
            }
            else
            {
                // Find a and h.
                double a = (radius0 * radius0 - radius1 * radius1 + dist * dist) / (2 * dist);
                double h = Math.Sqrt(radius0 * radius0 - a * a);

                // Find P2.
                double cx2 = cx0 + a * (cx1 - cx0) / dist;
                double cy2 = cy0 + a * (cy1 - cy0) / dist;

                // Get the points P3.
                var intersection1 = new Vector2(
                    (float)(cx2 + h * (cy1 - cy0) / dist),
                    (float)(cy2 - h * (cx1 - cx0) / dist));
                var intersection2 = new Vector2(
                    (float)(cx2 - h * (cy1 - cy0) / dist),
                    (float)(cy2 + h * (cx1 - cx0) / dist));

                // See if we have 1 or 2 solutions.
                if (ShapeMath.EqualsF((float)dist, radius0 + radius1))
                {
                    var n = intersection1 - new Vector2(cx1, cy1);
                    var cp = new CollisionPoint(intersection1, n.Normalize());
                    return (cp, null);
                }
                
                var n1 = intersection1 - new Vector2(cx1, cy1);
                var cp1 = new CollisionPoint(intersection1, n1.Normalize());
                
                var n2 = intersection2 - new Vector2(cx1, cy1);
                var cp2 = new CollisionPoint(intersection2, n2.Normalize());
                return (cp1, cp2);
            }

        }
        public static (CollisionPoint? a, CollisionPoint? b) IntersectCircleSegment(Vector2 circlePos, float circleRadius, Vector2 start, Vector2 end) 
        {
            return IntersectCircleSegment(
                circlePos.X, circlePos.Y, circleRadius,
                start.X, start.Y,
                end.X, end.Y); 
        }
        public static (CollisionPoint? a, CollisionPoint? b) IntersectCircleSegment(float circleX, float circleY, float circleRadius, float segStartX, float segStartY, float segEndX, float segEndY)
        {
            float difX = segEndX - segStartX;
            float difY = segEndY - segStartY;
            if ((difX == 0) && (difY == 0)) return (null, null);

            float dl = (difX * difX + difY * difY);
            float t = ((circleX - segStartX) * difX + (circleY - segStartY) * difY) / dl;

            // point on a line nearest to circle center
            float nearestX = segStartX + t * difX;
            float nearestY = segStartY + t * difY;

            float dist = (new Vector2(nearestX, nearestY) - new Vector2(circleX, circleY)).Length(); // point_dist(nearestX, nearestY, cX, cY);

            if (ShapeMath.EqualsF(dist, circleRadius))
            {
                // line segment touches circle; one intersection point
                float iX = nearestX;
                float iY = nearestY;

                if (t >= 0f && t <= 1f)
                {
                    // intersection point is not actually within line segment
                    var p = new Vector2(iX, iY);
                    var n = Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); // p - new Vector2(circleX, circleY);
                    var cp = new CollisionPoint(p, n);
                    return (cp, null);
                }
                return (null, null);
            }
            else if (dist < circleRadius)
            {
                // List<Vector2>? intersectionPoints = null;
                CollisionPoint? a = null;
                CollisionPoint? b = null;
                // two possible intersection points

                float dt = MathF.Sqrt(circleRadius * circleRadius - dist * dist) / MathF.Sqrt(dl);

                // intersection point nearest to A
                float t1 = t - dt;
                float i1X = segStartX + t1 * difX;
                float i1Y = segStartY + t1 * difY;
                if (t1 >= 0f && t1 <= 1f)
                {
                    // intersection point is actually within line segment
                    // intersectionPoints ??= new();
                    // intersectionPoints.Add(new Vector2(i1X, i1Y));
                    
                    var p = new Vector2(i1X, i1Y);
                    // var n = p - new Vector2(circleX, circleY);
                    var n = Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); 
                    a = new CollisionPoint(p, n);
                }

                // intersection point farthest from A
                float t2 = t + dt;
                float i2X = segStartX + t2 * difX;
                float i2Y = segStartY + t2 * difY;
                if (t2 >= 0f && t2 <= 1f)
                {
                    // intersection point is actually within line segment
                    // intersectionPoints ??= new();
                    // intersectionPoints.Add(new Vector2(i2X, i2Y));
                    var p = new Vector2(i2X, i2Y);
                    // var n = p - new Vector2(circleX, circleY);
                    var n = Segment.GetNormal(new(segStartX, segStartY), new(segEndX, segEndY), false); 
                    b = new CollisionPoint(p, n);
                }

                return (a, b);
            }

            return (null, null);
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
            foreach (var seg in segments)
            {
                if (seg.OverlapShape(this)) return true;
            }
            return false;
        }
        public readonly bool OverlapShape(Segment s) => OverlapCircleSegment(Center, Radius, s.Start, s.End);
        public readonly bool OverlapShape(Circle b) => OverlapCircleCircle(Center, Radius, b.Center, b.Radius);
        public readonly bool OverlapShape(Triangle t)
        {
            if (ContainsPoint(t.A)) return true;
            // if (ContainsPoint(t.B)) return true;
            // if (ContainsPoint(t.C)) return true;
            if (t.ContainsPoint(Center)) return true;

            if (Segment.OverlapSegmentCircle(t.A, t.B, Center, Radius)) return true;
            if (Segment.OverlapSegmentCircle(t.B, t.C, Center, Radius)) return true;
            return Segment.OverlapSegmentCircle(t.C, t.A, Center, Radius);
        }
        public readonly bool OverlapShape(Quad q)
        {
            if (ContainsPoint(q.A)) return true;
            // if (ContainsPoint(q.B)) return true;
            // if (ContainsPoint(q.C)) return true;
            // if (ContainsPoint(q.D)) return true;
            if (q.ContainsPoint(Center)) return true;
        
            if (Segment.OverlapSegmentCircle(q.A, q.B, Center, Radius)) return true;
            if (Segment.OverlapSegmentCircle(q.B, q.C, Center, Radius)) return true;
            if (Segment.OverlapSegmentCircle(q.C, q.D, Center, Radius)) return true;
            return Segment.OverlapSegmentCircle(q.D, q.A, Center, Radius);
        }
        public readonly bool OverlapShape(Rect r)
        {
            if (Radius <= 0.0f) return r.ContainsPoint(Center);
            return ContainsPoint(r.ClampOnRect(Center));
        }
        public readonly bool OverlapShape(Polygon poly)
        {
            if (poly.Count < 3) return false;
            if (ContainsPoint(poly[0])) return true;
            if (poly.ContainsPoint(Center)) return true;
            for (var i = 0; i < poly.Count; i++)
            {
                var start = poly[i];
                var end = poly[(i + 1) % poly.Count];
                if (Circle.OverlapCircleSegment(Center, Radius, start, end)) return true;
            }
            return false;
        }
        public readonly bool OverlapShape(Polyline pl)
        {
            if (pl.Count <= 0) return false;
            if (pl.Count == 1) return ContainsPoint(pl[0]);

            if (ContainsPoint(pl[0])) return true;
            
            for (var i = 0; i < pl.Count - 1; i++)
            {
                var start = pl[i];
                var end = pl[(i + 1) % pl.Count];
                if (OverlapCircleSegment(Center, Radius, start, end)) return true;
            }

            return false;
        }

        public readonly bool OverlapCircleLine(Vector2 linePos, Vector2 lineDir) =>
            OverlapCircleLine(Center, Radius, linePos, lineDir);

        public readonly bool OverlapCircleRay(Vector2 rayPos, Vector2 rayDir) =>
            OverlapCircleRay(Center, Radius, rayPos, rayDir);
       
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

        public readonly CollisionPoints? IntersectShape(Circle c)
        {
            var result = IntersectCircleCircle(Center, Radius, c.Center, c.Radius);
            if (result.a != null || result.b != null)
            {
                var points = new CollisionPoints();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }

            return null;
            //
            //
            // float cx0 = Center.X;
            // float cy0 = Center.Y;
            // float radius0 = Radius;
            // float cx1 = cB.Center.X;
            // float cy1 = cB.Center.Y;
            // float radius1 = cB.Radius;
            // // Find the distance between the centers.
            // float dx = cx0 - cx1;
            // float dy = cy0 - cy1;
            // double dist = Math.Sqrt(dx * dx + dy * dy);
            //
            // // See how many solutions there are.
            // if (dist > radius0 + radius1)
            // {
            //     // No solutions, the circles are too far apart.
            //     return null;
            // }
            // else if (dist < Math.Abs(radius0 - radius1))
            // {
            //     // No solutions, one circle contains the other.
            //     return null;
            // }
            // else if ((dist == 0) && (ShapeMath.EqualsF(radius0, radius1))) // radius0 == radius1))
            // {
            //     // No solutions, the circles coincide.
            //     return null;
            // }
            // else
            // {
            //     // Find a and h.
            //     double a = (radius0 * radius0 - radius1 * radius1 + dist * dist) / (2 * dist);
            //     double h = Math.Sqrt(radius0 * radius0 - a * a);
            //
            //     // Find P2.
            //     double cx2 = cx0 + a * (cx1 - cx0) / dist;
            //     double cy2 = cy0 + a * (cy1 - cy0) / dist;
            //
            //     // Get the points P3.
            //     var intersection1 = new Vector2(
            //         (float)(cx2 + h * (cy1 - cy0) / dist),
            //         (float)(cy2 - h * (cx1 - cx0) / dist));
            //     var intersection2 = new Vector2(
            //         (float)(cx2 - h * (cy1 - cy0) / dist),
            //         (float)(cy2 + h * (cx1 - cx0) / dist));
            //
            //     // See if we have 1 or 2 solutions.
            //     if (ShapeMath.EqualsF((float)dist, radius0 + radius1)) // dist == radius0 + radius1)
            //     {
            //         var n = ShapeVec.Normalize(intersection1 - new Vector2(cx1, cy1));
            //         return new() { new(intersection1, n) };
            //     }
            //     else
            //     {
            //         var otherPos = new Vector2(cx1, cy1);
            //         var n1 = ShapeVec.Normalize(intersection1 - otherPos);
            //         var n2 = ShapeVec.Normalize(intersection2 - otherPos);
            //         //if problems occur add that back (David)
            //         //p,n
            //         return new() { new(intersection1, n1), new(intersection2, n2) };
            //     }
            // }

        }
        public readonly CollisionPoints? IntersectShape(Segment s)
        {
            var result = IntersectCircleSegment(Center, Radius, s.Start, s.End);
            if (result.a != null || result.b != null)
            {
                var points = new CollisionPoints();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
                return points;
            }

            return null;
            // float cX = Center.X;
            // float cY = Center.Y;
            // float R = Radius;
            // float aX = s.Start.X;
            // float aY = s.Start.Y;
            // float bX = s.End.X;
            // float bY = s.End.Y;
            //
            // float dX = bX - aX;
            // float dY = bY - aY;
            //
            // var segmentNormal = s.Normal;
            //
            // if ((dX == 0) && (dY == 0))
            // {
            //     // A and B are the same points, no way to calculate intersection
            //     return null;
            // }
            //
            // float dl = (dX * dX + dY * dY);
            // float t = ((cX - aX) * dX + (cY - aY) * dY) / dl;
            //
            // // point on a line nearest to circle center
            // float nearestX = aX + t * dX;
            // float nearestY = aY + t * dY;
            //
            // float dist = (new Vector2(nearestX, nearestY) - new Vector2(cX, cY)).Length(); // point_dist(nearestX, nearestY, cX, cY);
            //
            // if ( ShapeMath.EqualsF(dist, R)) //dist == R))
            // {
            //     // line segment touches circle; one intersection point
            //     float iX = nearestX;
            //     float iY = nearestY;
            //
            //     if (t >= 0f && t <= 1f)
            //     {
            //         // intersection point is not actually within line segment
            //         Vector2 ip = new(iX, iY);
            //         return new() { new(ip, segmentNormal) };
            //     }
            //     else return null;
            // }
            // else if (dist < R)
            // {
            //     CollisionPoints? points = null;
            //     float dt = MathF.Sqrt(R * R - dist * dist) / MathF.Sqrt(dl);
            //
            //     // intersection point nearest to A
            //     float t1 = t - dt;
            //     float i1X = aX + t1 * dX;
            //     float i1Y = aY + t1 * dY;
            //     if (t1 >= 0f && t1 <= 1f)
            //     {
            //         // intersection point is actually within line segment
            //         points ??= new();
            //         Vector2 ip = new(i1X, i1Y);
            //         points.Add(new(ip, segmentNormal));
            //     }
            //
            //     // intersection point farthest from A
            //     float t2 = t + dt;
            //     float i2X = aX + t2 * dX;
            //     float i2Y = aY + t2 * dY;
            //     if (t2 >= 0f && t2 <= 1f)
            //     {
            //         points ??= new();
            //         Vector2 ip = new(i2X, i2Y);
            //         points.Add(new(ip, segmentNormal));
            //     }
            //
            //     return points;
            // }
            // else
            // {
            //     // no intersection
            //     return null;
            // }
        }
        public readonly CollisionPoints? IntersectShape(Triangle t)
        {
            CollisionPoints? points = null;
            var result = IntersectCircleSegment(Center, Radius, t.A, t.B);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            
            result = IntersectCircleSegment(Center, Radius, t.B, t.C);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }

            result = IntersectCircleSegment(Center, Radius, t.C, t.A);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }

            return points;
        }
        public readonly CollisionPoints? IntersectShape(Rect r)
        {
            CollisionPoints? points = null;
            var a = r.TopLeft;
            var b = r.BottomLeft;
            
            var result = IntersectCircleSegment(Center, Radius, a, b);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            
            var c = r.BottomRight;
            result = IntersectCircleSegment(Center, Radius, b, c);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            
            var d = r.TopRight;
            result = IntersectCircleSegment(Center, Radius, c, d);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }

            result = IntersectCircleSegment(Center, Radius, d, a);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Quad q)
        {
            CollisionPoints? points = null;
            
            var result = IntersectCircleSegment(Center, Radius, q.A, q.B);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            
            result = IntersectCircleSegment(Center, Radius, q.B, q.C);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            
            result = IntersectCircleSegment(Center, Radius, q.C, q.D);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }

            result = IntersectCircleSegment(Center, Radius, q.D, q.A);
            if (result.a != null || result.b != null)
            {
                points ??= new();
                if(result.a != null) points.Add((CollisionPoint)result.a);
                if(result.b != null) points.Add((CollisionPoint)result.b);
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Polygon p)
        {
            if (p.Count < 3) return null;
            
            CollisionPoints? points = null;
            (CollisionPoint? a, CollisionPoint? b) result;

            for (var i = 0; i < p.Count; i++)
            {
                result = IntersectCircleSegment(Center, Radius, p[i], p[(i + 1) % p.Count]);
                if (result.a != null || result.b != null)
                {
                    points ??= new();
                    if(result.a != null) points.Add((CollisionPoint)result.a);
                    if(result.b != null) points.Add((CollisionPoint)result.b);
                }
                
            }
            return points;
        }
        public readonly CollisionPoints? IntersectShape(Polyline pl)
        {
            if (pl.Count < 2) return null;
            
            CollisionPoints? points = null;
            (CollisionPoint? a, CollisionPoint? b) result;
            // if (pl.Count == 2)
            // {
            //     result = IntersectCircleSegment(Center, Radius, pl[0], pl[1]);
            //     if (result.a != null || result.b != null)
            //     {
            //         points ??= new();
            //         if(result.a != null) points.Add((CollisionPoint)result.a);
            //         if(result.b != null) points.Add((CollisionPoint)result.b);
            //         return points;
            //     }
            //
            //     return null;
            // }


            for (var i = 0; i < pl.Count - 1; i++)
            {
                result = IntersectCircleSegment(Center, Radius, pl[i], pl[(i + 1) % pl.Count]);
                if (result.a != null || result.b != null)
                {
                    points ??= new();
                    if(result.a != null) points.Add((CollisionPoint)result.a);
                    if(result.b != null) points.Add((CollisionPoint)result.b);
                }
                
            }
            return points;
        }

        public readonly CollisionPoints? IntersectShape(Segments shape)
        {
            CollisionPoints? points = null;
            foreach (var seg in shape)
            {
                var result = IntersectCircleSegment(Center, Radius, seg.Start, seg.End);
                if (result.a != null || result.b != null)
                {
                    points ??= new();
                    if(result.a != null) points.Add((CollisionPoint)result.a);
                    if(result.b != null) points.Add((CollisionPoint)result.b);
                }
            }
            return points;
        }
        
        #endregion
    }
}

