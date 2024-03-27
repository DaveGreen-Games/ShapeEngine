
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes
{
    public class Polyline : Points, IEquatable<Polyline>
    {
        #region Constructors
        public Polyline() { }
        public Polyline(int capacity) : base(capacity) { }
        
        /// <summary>
        /// Points should be in CCW order. Use Reverse if they are in CW order.
        /// </summary>
        /// <param name="edges"></param>
        public Polyline(IEnumerable<Vector2> edges) { AddRange(edges); }
        
        public Polyline(Polyline polyLine) { AddRange(polyLine); }
        public Polyline(Polygon poly) { AddRange(poly); }
        #endregion

        #region Equals & HashCode
        public bool Equals(Polyline? other)
        {
            if (other == null) return false;
            if (Count != other.Count) return false;
            for (var i = 0; i < Count; i++)
            {
                if (!this[i].IsSimilar(other[i])) return false;
                //if (this[i] != other[i]) return false;
            }
            return true;
        }
        public override int GetHashCode() => Game.GetHashCode(this);

        #endregion

        #region Getter Setter
        /// <summary>
        /// Flips the calculated normals for each segment. 
        /// false means default is used. (facing right)
        /// </summary>

        public float Length => MathF.Sqrt(LengthSquared);
        public float LengthSquared
        {
            get
            {
                if (this.Count < 2) return 0f;
                var lengthSq = 0f;
                for (var i = 0; i < Count - 1; i++)
                {
                    var w = this[i+1] - this[i];
                    lengthSq += w.LengthSquared();
                }
                return lengthSq;
            }
        }
        #endregion

        #region Math

        public Polygon Project(Vector2 v)
        {
            if (v.LengthSquared() <= 0f) return ToPolygon();
            var translated = ChangePositionCopy(v); // Move(this, v);
            if (translated == null) return ToPolygon();
            var points = new Points();
            points.AddRange(this);
            points.AddRange(translated);
            return Polygon.FindConvexHull(points);
        }
        
        public Vector2 GetCentroidOnLine()
        {
            return GetPoint(0.5f);
            // if (Count <= 0) return new(0f);
            // else if (Count == 1) return this[0];
            // float halfLengthSq = LengthSquared * 0.5f;
            // var segments = GetEdges();
            // float curLengthSq = 0f; 
            // foreach (var seg in segments)
            // {
            //     float segLengthSq = seg.LengthSquared;
            //     curLengthSq += segLengthSq;
            //     if (curLengthSq >= halfLengthSq)
            //     {
            //         float dif = curLengthSq - halfLengthSq;
            //         return seg.Center + seg.Dir * MathF.Sqrt(dif);
            //     }
            // }
            // return new Vector2();
        }
        public Vector2 GetCentroidMean()
        {
            if (Count <= 0) return new(0f);
            else if (Count == 1) return this[0];
            Vector2 total = new(0f);
            foreach (Vector2 p in this) { total += p; }
            return total / Count;
        }
        public Vector2 GetPoint(float f)
        {
            if (Count == 0) return new();
            if (Count == 1) return this[0];
            if (Count == 2) return this[0].Lerp(this[1], f);
            if (f <= 0f) return this[0];
            if (f >= 1f) return this[^1];
            
            var totalLengthSq = LengthSquared;
            var targetLengthSq = totalLengthSq * f;
            var curLengthSq = 0f;
            for (var i = 0; i < Count - 1; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];
                var lSq = (start - end).LengthSquared();
                if(lSq <= 0) continue;
                
                if (curLengthSq + lSq >= targetLengthSq)
                {
                    var aF = curLengthSq / totalLengthSq;
                    var bF = (curLengthSq + lSq) / totalLengthSq;
                    var curF = ShapeMath.LerpInverseFloat(aF, bF, f);
                    return start.Lerp(end, curF);
                }
                
                curLengthSq += lSq;
            }

            return new();
        }
        #endregion
        
        #region Shapes

        public Circle GetBoundingCircle()
        {
            float maxD = 0f;
            int num = this.Count;
            Vector2 origin = new();
            for (int i = 0; i < num; i++) { origin += this[i]; }
            origin *= (1f / num);
            for (int i = 0; i < num; i++)
            {
                float d = (origin - this[i]).LengthSquared();
                if (d > maxD) maxD = d;
            }

            return new Circle(origin, MathF.Sqrt(maxD));
        }
        public Rect GetBoundingBox()
        {
            if (Count < 2) return new();
            Vector2 start = this[0];
            Rect r = new(start.X, start.Y, 0, 0);

            foreach (var p in this)
            {
                r = r.Enlarge(p); // ShapeRect.Enlarge(r, p);
            }
            return r;
        }

        /// <summary>
        /// Return the segments of the polyline. If points are in ccw order the normals face to the right of the direction of the segments.
        /// If InsideNormals = true the normals face to the left of the direction of the segments.
        /// </summary>
        /// <returns></returns>
        public Segments GetEdges()
        {
            if (Count <= 1) return new();
            if (Count == 2) return new() { new(this[0], this[1]) };

            Segments segments = new();
            for (int i = 0; i < Count - 1; i++)
            {
                segments.Add(new(this[i], this[(i + 1) % Count]));
            }
            return segments;
        }
        
        public Points ToPoints() { return new(this); }

        #endregion
        
        #region Points & Vertex
        
        
        public Vector2 GetRandomVertex() { return ShapeRandom.RandCollection(this); }
        public Segment GetRandomEdge() => GetEdges().GetRandomSegment();
        //public Vector2 GetRandomPoint() => GetRandomEdge().GetRandomPoint();
        //public Points GetRandomPoints(int amount) => GetEdges().GetRandomPoints(amount);

        #endregion

        #region Transform
        public void SetPosition(Vector2 newPosition)
        {
            var delta = newPosition - GetCentroidMean();
            ChangePosition(delta);
        }
        public void SetPosition(Vector2 newPosition, Vector2 origin)
        {
            var delta = newPosition - origin;
            ChangePosition(delta);
        }
        public void ChangePosition(Vector2 offset)
        {
            for (int i = 0; i < Count; i++)
            {
                this[i] += offset;
            }
            //return path;
        }
        public void ChangeRotation(float rotRad, Vector2 origin)
        {
            if (Count < 2) return;
            for (int i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w.Rotate(rotRad);
            }
        }
        public void ChangeRotation(float rotRad)
        {
            if (Count < 2) return;
            var origin = GetCentroidMean();
            for (int i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w.Rotate(rotRad);
            }
        }
        
        public void SetRotation(float angleRad, Vector2 origin)
        {
            if (Count < 2) return;

            var curAngle = (this[0] - origin).AngleRad();
            var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
            ChangeRotation(rotRad, origin);
        }
        public void SetRotation(float angleRad)
        {
            if (Count < 2) return;

            var origin = GetCentroidMean();
            var curAngle = (this[0] - origin).AngleRad();
            var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
            ChangeRotation(rotRad, origin);
        }
        public void ScaleSize(float scale)
        {
            if (Count < 2) return;
            var origin = GetCentroidMean();
            for (int i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w * scale;
            }
        }
        public void ScaleSize(float scale, Vector2 origin)
        {
            if (Count < 2) return;
            for (int i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w * scale;
            }
        }
        public void ScaleSize(Vector2 scale, Vector2 origin)
        {
            if (Count < 2) return;
            for (int i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w * scale;
            }
        }
        public void ChangeSize(float amount, Vector2 origin)
        {
            if (Count < 2) return;
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w.ChangeLength(amount);
            }
            
        }
        public void ChangeSize(float amount)
        {
            if (Count < 2) return;
            var origin = GetCentroidMean();
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w.ChangeLength(amount);
            }
            
        }

        public void SetSize(float size, Vector2 origin)
        {
            if (Count < 2) return;
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w.SetLength(size);
            }

        }
        public void SetSize(float size)
        {
            if (Count < 2) return;
            var origin = GetCentroidMean();
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                this[i] = origin + w.SetLength(size);
            }

        }

        public void SetTransform(Transform2D transform, Vector2 origin)
        {
            SetPosition(transform.Position);
            SetRotation(transform.RotationRad, origin);
            SetSize(transform.Size.Width, origin);
        }
        public void ApplyTransform(Transform2D transform, Vector2 origin)
        {
            ChangePosition(transform.Position);
            ChangeRotation(transform.RotationRad, origin);
            ChangeSize(transform.Size.Width, origin);
            
        }
        
        
        public Polyline? SetPositionCopy(Vector2 newPosition)
        {
            if (Count < 2) return null;
            var centroid = GetCentroidMean();
            var delta = newPosition - centroid;
            return ChangePositionCopy(delta);
        }
        public Polyline? ChangePositionCopy(Vector2 offset)
        {
            if (Count < 2) return null;
            var newPolygon = new Polyline(this.Count);
            for (int i = 0; i < Count; i++)
            {
                newPolygon.Add(this[i] + offset);
            }

            return newPolygon;
        }
        public Polyline? ChangeRotationCopy(float rotRad, Vector2 origin)
        {
            if (Count < 2) return null;
            var newPolygon = new Polyline(this.Count);
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                newPolygon.Add(origin + w.Rotate(rotRad));
            }

            return newPolygon;
        }

        public Polyline? ChangeRotationCopy(float rotRad)
        {
            if (Count < 2) return null;
            return ChangeRotationCopy(rotRad, GetCentroidMean());
        }

        public Polyline? SetRotationCopy(float angleRad, Vector2 origin)
        {
            if (Count < 2) return null;
            var curAngle = (this[0] - origin).AngleRad();
            var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
            return ChangeRotationCopy(rotRad, origin);
        }
        public Polyline? SetRotationCopy(float angleRad)
        {
            if (Count < 2) return null;

            var origin = GetCentroidMean();
            var curAngle = (this[0] - origin).AngleRad();
            var rotRad = ShapeMath.GetShortestAngleRad(curAngle, angleRad);
            return ChangeRotationCopy(rotRad, origin);
        }
        public Polyline? ScaleSizeCopy(float scale)
        {
            if (Count < 2) return null;
            return ScaleSizeCopy(scale, GetCentroidMean());
        }
        public Polyline? ScaleSizeCopy(float scale, Vector2 origin)
        {
            if (Count < 2) return null;
            var newPolyline = new Polyline(this.Count);
            
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                newPolyline.Add( origin + w * scale);
            }

            return newPolyline;
        }
        public Polyline? ScaleSizeCopy(Vector2 scale, Vector2 origin)
        {
            if (Count < 2) return null;
            var newPolyline = new Polyline(this.Count);
            
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                newPolyline.Add(origin + w * scale);
            }

            return newPolyline;
        }
        public Polyline? ChangeSizeCopy(float amount, Vector2 origin)
        {
            if (Count < 2) return null;
            var newPolyline = new Polyline(this.Count);
            
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                newPolyline.Add(origin + w.ChangeLength(amount));
            }

            return newPolyline;

        }
        public Polyline? ChangeSizeCopy(float amount)
        {
            if (Count < 3) return null;
            return ChangeSizeCopy(amount, GetCentroidMean());

        }

        public Polyline? SetSizeCopy(float size, Vector2 origin)
        {
            if (Count < 2) return null;
            var newPolyline = new Polyline(this.Count);
            
            for (var i = 0; i < Count; i++)
            {
                var w = this[i] - origin;
                newPolyline.Add(origin + w.SetLength(size));
            }

            return newPolyline;
        }
        public Polyline? SetSizeCopy(float size)
        {
            if (Count < 2) return null;
            return SetSizeCopy(size, GetCentroidMean());

        }

        public Polyline? SetTransformCopy(Transform2D transform, Vector2 origin)
        {
            if (Count < 2) return null;
            var newPolyline = SetPositionCopy(transform.Position);
            if (newPolyline == null) return null;
            newPolyline.SetRotation(transform.RotationRad, origin);
            newPolyline.SetSize(transform.Size.Width, origin);
            return newPolyline;
        }
        public Polyline? ApplyTransformCopy(Transform2D transform, Vector2 origin)
        {
            if (Count < 2) return null;
            
            var newPolyline = ChangePositionCopy(transform.Position);
            if (newPolyline == null) return null;
            newPolyline.ChangeRotation(transform.RotationRad, origin);
            newPolyline.ChangeSize(transform.Size.Width, origin);
            return newPolyline;
        }
        
        #endregion
        
        #region Closest
        public ClosestDistance GetClosestDistanceTo(Vector2 p)
        {
            if (Count <= 0) return new();
            if (Count == 1) return new(this[0], p);
            if (Count == 2) return new(Segment.GetClosestPoint(this[0], this[1], p), p);
            if (Count == 3) return new(Triangle.GetClosestPoint(this[0], this[1], this[2], p), p);
            if (Count == 4) return new(Quad.GetClosestPoint(this[0], this[1], this[2], this[3], p), p);

            var cp = new Vector2();
            var minDisSq = float.PositiveInfinity;
            for (var i = 0; i < Count - 1; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];
                var next = Segment.GetClosestPoint(start, end, p);
                var disSq = (next - p).LengthSquared();
                if (disSq < minDisSq)
                {
                    minDisSq = disSq;
                    cp = next;
                }

            }

            return new(cp, p);
        }
        
        public ClosestDistance GetClosestDistanceTo(Segment segment)
        {
            if (Count <= 0) return new();
            if (Count == 1)
            {
                var cp = Segment.GetClosestPoint(segment.Start, segment.End, this[0]);
                return new(this[0], cp);
            }
            if (Count == 2) return new Segment(this[0], this[1]).GetClosestDistanceTo(segment);
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1) % Count];
                
                var next = Segment.GetClosestPoint(segment.Start, segment.End, p1);
                var cd = new ClosestDistance(p1, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(segment.Start, segment.End, p2);
                cd = new ClosestDistance(p2, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, segment.Start);
                cd = new ClosestDistance(next, segment.Start);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, segment.End);
                cd = new ClosestDistance(next, segment.End);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            }
            return closestDistance;
        }
        public ClosestDistance GetClosestDistanceTo(Circle circle)
        {
            if (Count <= 0) return new();
            if (Count == 1)
            {
                var cp = Circle.GetClosestPoint(circle.Center, circle.Radius, this[0]);
                return new(this[0], cp);
            }
            if (Count == 2) return new Segment(this[0], this[1]).GetClosestDistanceTo(circle);
            
            Vector2 closestPoint = new();
            Vector2 displacement = new();
            float minDisSq = float.PositiveInfinity;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1) % Count];
                
                var next = Segment.GetClosestPoint(p1, p2, circle.Center);
                var w = (next - circle.Center);
                var disSq = w.LengthSquared();
                if (disSq < minDisSq)
                {
                    minDisSq = disSq;
                    displacement = w;
                    closestPoint = next;
                }
            }

            var dir = displacement.Normalize();
            return new(closestPoint, circle.Center + dir * circle.Radius);
        }
        public ClosestDistance GetClosestDistanceTo(Triangle triangle)
        {
            if (Count <= 0) return new();
            if (Count == 1)
            {
                var cp = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, this[0]);
                return new(this[0], cp);
            }
            if (Count == 2) return new Segment(this[0], this[1]).GetClosestDistanceTo(triangle);
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1) % Count];
                
                var next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, p1);
                var cd = new ClosestDistance(p1, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Triangle.GetClosestPoint(triangle.A, triangle.B, triangle.C, p2);
                cd = new ClosestDistance(p2, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, triangle.A);
                cd = new ClosestDistance(next, triangle.A);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, triangle.B);
                cd = new ClosestDistance(next, triangle.B);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, triangle.C);
                cd = new ClosestDistance( next, triangle.C);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            }
            return closestDistance;
        }
        public ClosestDistance GetClosestDistanceTo(Quad quad)
        {
            if (Count <= 0) return new();
            if (Count == 1)
            {
                var cp = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, this[0]);
                return new(this[0], cp);
            }
            if (Count == 2) return new Segment(this[0], this[1]).GetClosestDistanceTo(quad);
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1) % Count];
                
                var next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, p1);
                var cd = new ClosestDistance(p1, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Quad.GetClosestPoint(quad.A, quad.B, quad.C, quad.D, p2);
                cd = new ClosestDistance(p2, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, quad.A);
                cd = new ClosestDistance(next, quad.A);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, quad.B);
                cd = new ClosestDistance(next, quad.B);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, quad.C);
                cd = new ClosestDistance(next, quad.C);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, quad.D);
                cd = new ClosestDistance(next, quad.D);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            }
            return closestDistance;
        }
        public ClosestDistance GetClosestDistanceTo(Rect rect)
        {
            if (Count <= 0) return new();
            if (Count == 1)
            {
                var cp = Quad.GetClosestPoint(rect.A, rect.B, rect.C, rect.D, this[0]);
                return new(this[0], cp);
            }
            if (Count == 2) return new Segment(this[0], this[1]).GetClosestDistanceTo(rect);
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1) % Count];
                
                var next = Quad.GetClosestPoint(rect.A, rect.B, rect.C, rect.D, p1);
                var cd = new ClosestDistance(p1, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Quad.GetClosestPoint(rect.A, rect.B, rect.C, rect.D, p2);
                cd = new ClosestDistance(p2, next);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, rect.A);
                cd = new ClosestDistance(next, rect.A);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, rect.B);
                cd = new ClosestDistance(next, rect.B);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, rect.C);
                cd = new ClosestDistance(next, rect.C);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                
                next = Segment.GetClosestPoint(p1, p2, rect.D);
                cd = new ClosestDistance(next, rect.D);
                if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
            }
            return closestDistance;
        }
        public ClosestDistance GetClosestDistanceTo(Polygon polygon)
        {
            if (Count <= 0 || polygon.Count <= 0) return new();
            if (Count == 1) return polygon.GetClosestDistanceTo(this[0]).ReversePoints();
            if (Count == 2) return new Segment(this[0], this[1]).GetClosestDistanceTo(polygon);
            if (polygon.Count == 1) return GetClosestDistanceTo(polygon[0]);
            if (polygon.Count == 2) return GetClosestDistanceTo(new Segment(polygon[0], polygon[1]));
            if (polygon.Count == 3) return GetClosestDistanceTo(new Triangle(polygon[0], polygon[1], polygon[2]));
            if (polygon.Count == 4) return GetClosestDistanceTo(new Quad(polygon[0], polygon[1], polygon[2], polygon[3]));
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < Count - 1; i++)
            {
                var self1 = this[i];
                var self2 = this[(i + 1) % Count];

                for (var j = 0; j < polygon.Count; j++)
                {
                    var other1 = polygon[j];
                    var other2 = polygon[(j + 1) % polygon.Count];

                    var next = Segment.GetClosestPoint(self1, self2, other1);
                    var cd = new ClosestDistance(next, other1);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                    
                    next = Segment.GetClosestPoint(self1, self2, other2);
                    cd = new ClosestDistance(next, other2);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                    
                    next = Segment.GetClosestPoint(other1, other2, self1);
                    cd = new ClosestDistance(self1, next);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                    
                    next = Segment.GetClosestPoint(other1, other2, self2);
                    cd = new ClosestDistance(self2, next);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                }
            }
            return closestDistance;
        }
        public ClosestDistance GetClosestDistanceTo(Polyline polyline)
        {
            if (Count <= 0 || polyline.Count <= 0) return new();
            if (Count == 1) return polyline.GetClosestDistanceTo(this[0]).ReversePoints();
            if (Count == 2) return new Segment(this[0], this[1]).GetClosestDistanceTo(polyline);
            if (polyline.Count == 1) return GetClosestDistanceTo(polyline[0]);
            if (polyline.Count == 2) return GetClosestDistanceTo(new Segment(polyline[0], polyline[1]));
            
            ClosestDistance closestDistance = new(new(), new(), float.PositiveInfinity);
            
            for (var i = 0; i < Count - 1; i++)
            {
                var self1 = this[i];
                var self2 = this[(i + 1) % Count];

                for (var j = 0; j < polyline.Count - 1; j++)
                {
                    var other1 = polyline[j];
                    var other2 = polyline[(j + 1) % polyline.Count];

                    var next = Segment.GetClosestPoint(self1, self2, other1);
                    var cd = new ClosestDistance(next, other1);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                    
                    next = Segment.GetClosestPoint(self1, self2, other2);
                    cd = new ClosestDistance(next, other2);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                    
                    next = Segment.GetClosestPoint(other1, other2, self1);
                    cd = new ClosestDistance(self1, next);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                    
                    next = Segment.GetClosestPoint(other1, other2, self2);
                    cd = new ClosestDistance(self2, next);
                    if (cd.DistanceSquared < closestDistance.DistanceSquared) closestDistance = cd;
                }
            }
            return closestDistance;
        }
        
        
        public int GetClosestIndexOnEdge(Vector2 p)
        {
            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            int closestIndex = -1;

            for (var i = 0; i < Count; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];
                var edge = new Segment(start, end);

                Vector2 closest = edge.GetClosestCollisionPoint(p).Point;
                float d = (closest - p).LengthSquared();
                if (d < minD)
                {
                    closestIndex = i;
                    minD = d;
                }
            }
            return closestIndex;
        }
        // internal ClosestPoint GetClosestPoint(Vector2 p)
        // {
        //     var cp = GetEdges().GetClosestCollisionPoint(p);
        //     return new(cp, (cp.Point - p).Length());
        // }
        public CollisionPoint GetClosestCollisionPoint(Vector2 p) => GetEdges().GetClosestCollisionPoint(p);
        public ClosestSegment GetClosestSegment(Vector2 p)
        {
            if (Count <= 1) return new();

            var closestSegment = new Segment(this[0], this[1]);
            var closestDistance = closestSegment.GetClosestDistanceTo(p);
            
            for (var i = 1; i < Count - 1; i++)
            {
                var p1 = this[i];
                var p2 = this[(i + 1) % Count];
                var segment = new Segment(p1, p2);
                var cd = segment.GetClosestDistanceTo(p);
                if (cd.DistanceSquared < closestDistance.DistanceSquared)
                {
                    closestDistance = cd;
                    closestSegment = segment;
                }

            }

            return new(closestSegment, closestDistance);
        }
        #endregion

        #region Contains
        public bool ContainsPoint(Vector2 p)
        {
            var segments = GetEdges();
            foreach (var segment in segments)
            {
                if (segment.ContainsPoint(p)) return true;
            }
            return false;
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
            if (Count < 2 || segments.Count <= 0) return false;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];

                foreach (var seg in segments)
                {
                    if (Segment.OverlapSegmentSegment(start, end, seg.Start, seg.End)) return true;
                }
            }

            return false;
        }
        public bool OverlapShape(Segment s) => s.OverlapShape(this);
        public bool OverlapShape(Circle c) => c.OverlapShape(this);
        public bool OverlapShape(Triangle t) => t.OverlapShape(this);
        public bool OverlapShape(Rect r) => r.OverlapShape(this);
        public bool OverlapShape(Quad q) => q.OverlapShape(this);
        public bool OverlapShape(Polygon p) => p.OverlapShape(this);
        public bool OverlapShape(Polyline b)
        {
            if (Count < 2 || b.Count < 2) return false;
            
            for (var i = 0; i < Count - 1; i++)
            {
                var start = this[i];
                var end = this[(i + 1) % Count];

                for (var j = 0; j < Count - 1; j++)
                {
                    var bStart = b[j];
                    var bEnd = b[(j + 1) % b.Count];

                    if (Segment.OverlapSegmentSegment(start, end, bStart, bEnd)) return true;
                }
            }

            return false;
        }
        #endregion

        #region Intersection
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

        //other shape center is used for checking segment normal and if necessary normal is flipped
        public CollisionPoints? IntersectShape(Segment s)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], s.Start, s.End);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Circle c)
        {
            if (Count < 2) return null;
            
            CollisionPoints? points = null;
            (CollisionPoint? a, CollisionPoint? b) result;

            for (var i = 0; i < Count - 1; i++)
            {
                result = Segment.IntersectSegmentCircle(this[i], this[(i + 1) % Count], c.Center, c.Radius);
                if (result.a != null || result.b != null)
                {
                    points ??= new();
                    if(result.a != null) points.Add((CollisionPoint)result.a);
                    if(result.b != null) points.Add((CollisionPoint)result.b);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Triangle t)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.A, t.B);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.B, t.C);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], t.C, t.A);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Rect r)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            var a = r.TopLeft;
            var b = r.BottomLeft;
            var c = r.BottomRight;
            var d = r.TopRight;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], a, b);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], b, c);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], c, d);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], d, a);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Quad q)
        {
            if (Count < 2) return null;

            CollisionPoints? points = null;
            CollisionPoint? colPoint = null;
            for (var i = 0; i < Count - 1; i++)
            {
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.A, q.B);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.B, q.C);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.C, q.D);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
                
                colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count], q.D, q.A);
                if (colPoint != null)
                {
                    points ??= new();
                    points.Add((CollisionPoint)colPoint);
                }
            }
            return points;
        }
        public CollisionPoints? IntersectShape(Polygon p)
        {
            if (p.Count < 3 || Count < 2) return null;
            CollisionPoints? points = null;
            for (var i = 0; i < Count - 1; i++)
            {
                for (var j = 0; j < p.Count; j++)
                {
                    var colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],p[j], p[(j + 1) % p.Count]);
                    if (colPoint != null)
                    {
                        points ??= new();
                        points.Add((CollisionPoint)colPoint);
                    }
                }
                
            }
            
            return points;
        }
        public CollisionPoints? IntersectShape(Polyline b)
        {
            if (b.Count < 2 || Count < 2) return null;
            CollisionPoints? points = null;
            for (var i = 0; i < Count - 1; i++)
            {
                for (var j = 0; j < b.Count - 1; j++)
                {
                    var colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],b[j], b[(j + 1) % b.Count]);
                    if (colPoint != null)
                    {
                        points ??= new();
                        points.Add((CollisionPoint)colPoint);
                    }
                }
                
            }
            
            return points;
        }
        public CollisionPoints? IntersectShape(Segments segments)
        {
            if (Count < 2 || segments.Count <= 0) return null;
            CollisionPoints? points = null;
            for (var i = 0; i < Count - 1; i++)
            {
                foreach (var seg in segments)
                {
                    var colPoint = Segment.IntersectSegmentSegment(this[i], this[(i + 1) % Count],seg.Start, seg.End);
                    if (colPoint != null)
                    {
                        points ??= new();
                        points.Add((CollisionPoint)colPoint);
                    }
                }
            }
            return points;
        }
        #endregion

        #region Static
        public static Polyline GetShape(Points relative, Transform2D transform)
        {
            if (relative.Count < 3) return new();
            Polyline shape = new();
            for (int i = 0; i < relative.Count; i++)
            {
                shape.Add(transform.Apply(relative[i]));
                // shape.Add(transform.Position + relative[i].Rotate(transform.RotationRad) * transform.Scale);
            }
            return shape;
        }

        // public static Polyline Center(Polyline p, Vector2 newCenter)
        // {
        //     var centroid = p.GetCentroidMean();
        //     var delta = newCenter - centroid;
        //     return Move(p, delta);
        // }
        // public static Polyline Move(Polyline p, Vector2 translation)
        // {
        //     var result = new Polyline();
        //     for (int i = 0; i < p.Count; i++)
        //     {
        //         result.Add(p[i] + translation);
        //     }
        //     return result;
        // }
        #endregion
    }

}

//public bool IsConvex()
//{
//    int num = this.Count;
//    bool isPositive = false;
//    for (int i = 0; i < num; i++)
//    {
//        int prevIndex = (i == 0) ? num - 1 : i - 1;
//        int nextIndex = (i == num - 1) ? 0 : i + 1;
//        var d0 = this[i] - this[prevIndex];
//        var d1 = this[nextIndex] - this[i];
//        var newIsP = d0.Cross(d1) > 0f;
//        if (i == 0) isPositive = true;
//        else if (isPositive != newIsP) return false;
//    }
//    return true;
//}

/*
        //old
        public int GetClosestIndex(Vector2 p)
        {
            if (Count <= 0) return -1;
            if (Count == 1) return 0;

            float minD = float.PositiveInfinity;
            var edges = GetEdges();
            int closestIndex = -1;
            for (int i = 0; i < edges.Count; i++)
            {
                Vector2 c = edges[i].GetClosestPoint(p).Point;
                float d = (c - p).LengthSquared();
                if (d < minD)
                {
                    closestIndex = i;
                    minD = d;
                }
            }
            return closestIndex;
        }
        public Vector2 GetClosestVertex(Vector2 p)
        {
            float minD = float.PositiveInfinity;
            Vector2 closest = new();
            for (int i = 0; i < Count; i++)
            {
                float d = (this[i] - p).LengthSquared();
                if (d < minD)
                {
                    closest = this[i];
                    minD = d;
                }
            }
            return closest;
        }
        */
