
using System.Numerics;
using System.Runtime.Versioning;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision
{
    public abstract class Collider
    {
        public event Action<CollisionInformation>? OnCollision;
        public event Action<Collider>? OnCollisionEnded;
        
        
        private CollisionBody? parent = null;
        public CollisionBody? Parent
        {
            get => parent;
            internal set
            {
                if (parent == null)
                {
                    if (value != null)
                    {
                        parent = value;
                        OnAddedToCollisionBody(parent);
                    }
                }
                else
                {
                    if (value == null)
                    {
                        OnRemovedFromCollisionBody(parent);
                        parent = null;
                    }
                    else if (value != parent)
                    {
                        OnRemovedFromCollisionBody(parent);
                        parent = value;
                        OnAddedToCollisionBody(parent);
                    }
                }
            }
        }

        private bool enabled = true;
        public bool Enabled
        {
            get
            {
                if (parent != null) return parent.Enabled && enabled;
                return enabled;
            }
            set => enabled = value;
        }

        public bool Moves = true;
        public bool Rotates = true;
        public bool Scales = true;
        // public Vector2 Offset { get; set; }
        public Transform2D Offset { get; set; }
        public Transform2D CurTransform { get; private set; }
        public Transform2D PrevTransform { get; private set; }
        
        // public Vector2 Position { get; private set; }
        public Vector2 Velocity => parent?.Velocity ?? new(0f);
        // public Vector2 PrevPosition { get; private set; }
        
        // public bool FlippedNormals { get; set; } = false;

        public BitFlag CollisionMask { get; set; } = BitFlag.Empty;
        public uint CollisionLayer { get; set; } = 0;
        public bool ComputeCollision { get; set; } = true;
        /// <summary>
        /// If false only overlaps will be reported but no further details on the intersection.
        /// </summary>
        public bool ComputeIntersections { get; set; } = false;

        protected bool Dirty = false;
        
        protected Collider()
        {
            this.Offset = new(new(0f), 0f, new(1f));
        }
        protected Collider(Vector2 offset)
        {
            this.Offset = new(offset, 0f, new(1f));
        }

        protected Collider(Transform2D offset)
        {
            this.Offset = offset;
        }

        
        internal void ResolveCollision(CollisionInformation info)
        {
            Collision(info);
            OnCollision?.Invoke(info);
        }
        internal void ResolveCollisionEnded(Collider other)
        {
            CollisionEnded(other);
            OnCollisionEnded?.Invoke(other);
        }
        protected virtual void Collision(CollisionInformation info) { }
        protected virtual void CollisionEnded(Collider other) { }
        
        public virtual void Update(float dt)
        {
        }

        internal void SetupTransform(Transform2D parentTransform)
        {
            UpdateTransform(parentTransform);
            PrevTransform = CurTransform;
        }
        internal void UpdateTransform(Transform2D parentTransform)
        {
            PrevTransform = CurTransform;

            var rot = Rotates ? parentTransform.RotationRad + Offset.RotationRad : Offset.RotationRad;
            var scale = Scales ? parentTransform.Scale * Offset.Scale : Offset.Scale;
            if (Moves)
            {
                if (Offset.Position.LengthSquared() <= 0) CurTransform = new(parentTransform.Position, rot, scale);
                else
                {
                    var pos = parentTransform.Position + Offset.Position.Rotate(rot) * scale;
                    CurTransform = new(pos, rot, scale);
                }
            }
            else
            {
                CurTransform = new(Offset.Position, rot, scale);
            }

            Dirty = PrevTransform != CurTransform;
            // Position = parentTransform.Apply(Offset);
            // PrevPosition = Position;
        }

        protected virtual void OnAddedToCollisionBody(CollisionBody newParent) { }
        protected virtual void OnRemovedFromCollisionBody(CollisionBody formerParent) { }

        public abstract Rect GetBoundingBox();
        public abstract bool ContainsPoint(Vector2 p);
        public abstract CollisionPoint GetClosestCollisionPoint(Vector2 p);

        #region Overlap
        public bool Overlap(CollisionBody other)
        {
            if (!Enabled || !other.Enabled || !other.HasColliders) return false;
            foreach (var otherCol in other.Colliders)
            {
                // if(!otherCol.Enabled) continue;
                if (Overlap(otherCol)) return true;
            }

            return false;
        }
        public bool Overlap(Collider other)
        {
            if (!Enabled || !other.Enabled) return false;

            switch (other.GetShapeType())
            {
                case ShapeType.Circle: return Overlap(other.GetCircleShape());
                case ShapeType.Segment: return Overlap(other.GetSegmentShape());
                case ShapeType.Triangle: return Overlap(other.GetTriangleShape());
                case ShapeType.Quad: return Overlap(other.GetQuadShape());
                case ShapeType.Rect: return Overlap(other.GetRectShape());
                case ShapeType.Poly: return Overlap(other.GetPolygonShape());
                case ShapeType.PolyLine: return Overlap(other.GetPolylineShape());
            }

            return false;
        }
        public bool Overlap(Segment segment)
        { 
            if (!Enabled) return false;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.OverlapShape(segment);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.OverlapShape(segment);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.OverlapShape(segment);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.OverlapShape(segment);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.OverlapShape(segment);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.OverlapShape(segment);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.OverlapShape(segment);
            }

            return false;
        }
        public bool Overlap(Triangle triangle)
        {
            if (!Enabled) return false;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.OverlapShape(triangle);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.OverlapShape(triangle);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.OverlapShape(triangle);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.OverlapShape(triangle);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.OverlapShape(triangle);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.OverlapShape(triangle);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.OverlapShape(triangle);
            }

            return false;
        }
        public bool Overlap(Circle circle)
        {
            if (!Enabled) return false;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.OverlapShape(circle);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.OverlapShape(circle);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.OverlapShape(circle);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.OverlapShape(circle);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.OverlapShape(circle);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.OverlapShape(circle);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.OverlapShape(circle);
            }

            return false;
        }
        public bool Overlap(Rect rect)
        {
            if (!Enabled) return false;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.OverlapShape(rect);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.OverlapShape(rect);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.OverlapShape(rect);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.OverlapShape(rect);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.OverlapShape(rect);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.OverlapShape(rect);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.OverlapShape(rect);
            }

            return false;
        }
        public bool Overlap(Quad quad)
        {
            if (!Enabled) return false;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.OverlapShape(quad);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.OverlapShape(quad);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.OverlapShape(quad);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.OverlapShape(quad);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.OverlapShape(quad);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.OverlapShape(quad);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.OverlapShape(quad);
            }

            return false;
        }

        public bool Overlap(Polygon poly)
        {
            if (!Enabled) return false;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.OverlapShape(poly);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.OverlapShape(poly);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.OverlapShape(poly);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.OverlapShape(poly);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.OverlapShape(poly);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.OverlapShape(poly);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.OverlapShape(poly);
            }

            return false;
        }
        public bool Overlap(Polyline polyLine)
        {
            if (!Enabled) return false;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.OverlapShape(polyLine);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.OverlapShape(polyLine);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.OverlapShape(polyLine);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.OverlapShape(polyLine);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.OverlapShape(polyLine);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.OverlapShape(polyLine);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.OverlapShape(polyLine);
            }

            return false;
        }
        #endregion

        #region Intersect
        public CollisionPoints? Intersect(CollisionBody other)
        {
            if (!Enabled || !other.Enabled || !other.HasColliders) return null;
            CollisionPoints? result = null;
            foreach (var otherCol in other.Colliders)
            {
                var points = Intersect(otherCol);
                if (points == null) continue;
                
                result ??= new();
                result.AddRange(points);
            }
            return result;
        }
        public CollisionPoints? Intersect(Collider other)
        {
            if (!Enabled || !other.Enabled) return null;

            switch (other.GetShapeType())
            {
                case ShapeType.Circle: return Intersect(other.GetCircleShape());
                case ShapeType.Segment: return Intersect(other.GetSegmentShape());
                case ShapeType.Triangle: return Intersect(other.GetTriangleShape());
                case ShapeType.Rect: return Intersect(other.GetRectShape());
                case ShapeType.Quad: return Intersect(other.GetQuadShape());
                case ShapeType.Poly: return Intersect(other.GetPolygonShape());
                case ShapeType.PolyLine: return Intersect(other.GetPolylineShape());
            }

            return null;
        }
        public CollisionPoints? Intersect(Segment segment)
        {
            if (!Enabled) return null;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.IntersectShape(segment);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.IntersectShape(segment);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.IntersectShape(segment);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.IntersectShape(segment);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.IntersectShape(segment);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.IntersectShape(segment);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.IntersectShape(segment);
            }

            return null;
        }
        public CollisionPoints? Intersect(Triangle triangle)
        {
            if (!Enabled) return null;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.IntersectShape(triangle);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.IntersectShape(triangle);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.IntersectShape(triangle);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.IntersectShape(triangle);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.IntersectShape(triangle);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.IntersectShape(triangle);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.IntersectShape(triangle);
            }

            return null;
        }
        public CollisionPoints? Intersect(Circle circle)
        {
            if (!Enabled) return null;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.IntersectShape(circle);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.IntersectShape(circle);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.IntersectShape(circle);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.IntersectShape(circle);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.IntersectShape(circle);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.IntersectShape(circle);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.IntersectShape(circle);
            }

            return null;
        }
        public CollisionPoints? Intersect(Rect rect)
        {
            if (!Enabled) return null;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.IntersectShape(rect);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.IntersectShape(rect);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.IntersectShape(rect);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.IntersectShape(rect);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.IntersectShape(rect);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.IntersectShape(rect);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.IntersectShape(rect);
            }

            return null;
        }
        public CollisionPoints? Intersect(Quad quad)
        {
            if (!Enabled) return null;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.IntersectShape(quad);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.IntersectShape(quad);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.IntersectShape(quad);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.IntersectShape(quad);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.IntersectShape(quad);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.IntersectShape(quad);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.IntersectShape(quad);
            }

            return null;
        }
        public CollisionPoints? Intersect(Polygon poly)
        {
            if (!Enabled) return null;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.IntersectShape(poly);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.IntersectShape(poly);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.IntersectShape(poly);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.IntersectShape(poly);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.IntersectShape(poly);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.IntersectShape(poly);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.IntersectShape(poly);
            }

            return null;
        }
        public CollisionPoints? Intersect(Polyline polyLine)
        {
            if (!Enabled) return null;

            switch (GetShapeType())
            {
                case ShapeType.Circle:
                    var c = GetCircleShape();
                    return c.IntersectShape(polyLine);
                case ShapeType.Segment:
                    var s = GetSegmentShape();
                    return s.IntersectShape(polyLine);
                case ShapeType.Triangle:
                    var t = GetTriangleShape();
                    return t.IntersectShape(polyLine);
                case ShapeType.Rect:
                    var r = GetRectShape();
                    return r.IntersectShape(polyLine);
                case ShapeType.Quad:
                    var q = GetQuadShape();
                    return q.IntersectShape(polyLine);
                case ShapeType.Poly:
                    var p = GetPolygonShape();
                    return p.IntersectShape(polyLine);
                case ShapeType.PolyLine:
                    var pl = GetPolylineShape();
                    return pl.IntersectShape(polyLine);
            }

            return null;
        }
        #endregion
        
        
        public abstract ShapeType GetShapeType();
        public virtual Circle GetCircleShape() => new();
        public virtual Segment GetSegmentShape() => new();
        public virtual Triangle GetTriangleShape() => new();
        public virtual Quad GetQuadShape() => new();
        public virtual Rect GetRectShape() => new();
        public virtual Polygon GetPolygonShape() => new();
        public virtual Polyline GetPolylineShape() => new();
        
        

    }
}