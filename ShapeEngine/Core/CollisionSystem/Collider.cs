
using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;


namespace ShapeEngine.Core.CollisionSystem;

public abstract class Collider : Shape
{
    /// <summary>
    /// A collision (Intersection) between this collider and another collider has occurred.
    /// AvancedCollisionNotification has to be enabled on the parent for this event to be invoked.
    /// </summary>
    public event Action<Collision>? OnIntersected;
    /// <summary>
    /// A collision (Overlap) between this collider and another collider has occured.
    ///  AvancedCollisionNotification has to be enabled on the parent for this event to be invoked.
    /// </summary>
    public event Action<Overlap>? OnOverlapped;
    /// <summary>
    /// A collision (Intersection/Overlap) between this collider and another collider has ended.
    ///  AvancedCollisionNotification has to be enabled on the parent for this event to be invoked.
    /// </summary>
    public event Action<Collider>? OnContactEnded;
    
    
    private CollisionObject? parent = null;
    public CollisionObject? Parent
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

    public Vector2 Velocity => parent?.Velocity ?? new(0f);
    public BitFlag CollisionMask { get; set; } = BitFlag.Empty;
    public uint CollisionLayer { get; set; } = 0;
 
    public bool ComputeCollision { get; set; } = true;
    /// <summary>
    /// If false only overlaps will be reported but no further details on the intersection.
    /// </summary>
    public bool ComputeIntersections { get; set; } = false;
    /// <summary>
    /// This determines if ColliderIntersected and ColliderOverlapped will be called and if
    /// OnColliderOverlapped and OnColliderIntersected events will be invoked.
    /// Collision and CollisionEnded will always be called.
    /// This is an additional convenience option to get overlap/intersection information in a different way.
    /// </summary>

    // protected bool Dirty = false;
    
    protected Collider()
    {
        this.Offset = new(new Vector2(0f), 0f, new Size(0f), 1f);
    }
    protected Collider(Vector2 offset)
    {
        this.Offset = new(offset, 0f, new Size(0f), 1f);
    }
    protected Collider(Transform2D offset)
    {
        this.Offset = offset;
    }

    
    internal void ResolveIntersected(Collision collision)
    {
        Intersected(collision);
        OnIntersected?.Invoke(collision);
    }

    internal void ResolveOverlapped(Overlap overlap)
    {
        Overlapped(overlap);
        OnOverlapped?.Invoke(overlap);
    }
   
    internal void ResolveContactEnded(Collider other)
    {
        ContactEnded(other);
        OnContactEnded?.Invoke(other);
    } 
    
    /// <summary>
    /// Will be called from the parent. Is only called when a collision with this collider occurs where the intersection is valid.
    /// AvancedCollisionNotification has to be enabled on the parent for this function to be called.
    /// </summary>
    /// <param name="info"></param>
    protected virtual void Intersected(Collision info) { }
    /// <summary>
    /// Will be called from the parent. Is only called when an overlap with this collider occurs where the intersection is not valid.
    /// AvancedCollisionNotification has to be enabled on the parent for this function to be called.
    /// </summary>
    /// <param name="contact"></param>
    protected virtual void Overlapped(Overlap contact) { }
    /// <summary>
    /// Will be called from the parent. Is only called when a collision (intersection / overlap)  with this collider ends.
    /// AvancedCollisionNotification has to be enabled on the parent for this function to be called.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void ContactEnded(Collider other) { }
    

    public override void InitializeShape(Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnInitialized();
    }

    public override void UpdateShape(float dt, Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnUpdate(dt);
    }

    public override void DrawShape()
    {
        OnDraw();
    }
    
    public override void RecalculateShape() { }

    protected override void OnInitialized() { }

    protected override void OnUpdate(float dt) { }

    protected override void OnDraw() { }
    
    
    
    
    protected virtual void OnAddedToCollisionBody(CollisionObject newParent) { }
    protected virtual void OnRemovedFromCollisionBody(CollisionObject formerParent) { }

    public abstract Rect GetBoundingBox();

    public Polygon? Project(Vector2 v)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.ProjectShape(v, 8);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.ProjectShape(v);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.ProjectShape(v);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.ProjectShape(v);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.ProjectShape(v);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.ProjectShape(v);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.ProjectShape(v);
        }

        return null;
    }

    
    #region Closest

    public ClosestPointResult GetClosestPoint(IShape shape)
    {
        switch (shape.GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetClosestPoint(shape.GetCircleShape());
            case ShapeType.Segment: return GetClosestPoint(shape.GetSegmentShape());
            case ShapeType.Triangle: return GetClosestPoint(shape.GetTriangleShape());
            case ShapeType.Quad: return GetClosestPoint(shape.GetQuadShape());
            case ShapeType.Rect: return GetClosestPoint(shape.GetRectShape());
            case ShapeType.Poly: return GetClosestPoint(shape.GetPolygonShape());
            case ShapeType.PolyLine: return GetClosestPoint(shape.GetPolylineShape());
        }

        return new();
    }
    
    public CollisionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(p, out disSquared);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(p, out disSquared);
        }

        return new();
    }
    public ClosestPointResult GetClosestPoint(Segment shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    public ClosestPointResult GetClosestPoint(Circle shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    public ClosestPointResult GetClosestPoint(Triangle shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    public ClosestPointResult GetClosestPoint(Quad shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    public ClosestPointResult GetClosestPoint(Rect shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    public ClosestPointResult GetClosestPoint(Polygon shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    public ClosestPointResult GetClosestPoint(Polyline shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    
    #endregion

    #region Contains

    public bool ContainsPoint(Vector2 p)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsPoint(p);
            case ShapeType.Segment: return GetSegmentShape().ContainsPoint(p);
            case ShapeType.Triangle: return GetTriangleShape().ContainsPoint(p);
            case ShapeType.Quad: return GetQuadShape().ContainsPoint(p);
            case ShapeType.Rect: return GetRectShape().ContainsPoint(p);
            case ShapeType.Poly: return GetPolygonShape().ContainsPoint(p);
            case ShapeType.PolyLine: return GetPolylineShape().ContainsPoint(p);
        }

        return false;
    }

    public bool ContainsShape(IShape shape)
    {
        switch (shape.GetShapeType())
        {
            case ShapeType.Circle: return ContainsShape(shape.GetCircleShape());
            case ShapeType.Segment: return ContainsShape(shape.GetSegmentShape());
            case ShapeType.Triangle: return ContainsShape(shape.GetTriangleShape());
            case ShapeType.Quad: return ContainsShape(shape.GetQuadShape());
            case ShapeType.Rect: return ContainsShape(shape.GetRectShape());
            case ShapeType.Poly: return ContainsShape(shape.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(shape.GetPolylineShape());
        }

        return false;
    }
    public bool ContainsShape(Segment shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Circle shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Triangle shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }

    public bool ContainsShape(Quad shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Rect shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Polygon shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Polyline shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    #endregion
    
    #region Overlap
    public bool Overlap(CollisionObject other)
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
    public bool Overlap(IShape other)
    {
        if (!Enabled) return false;

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
    public CollisionPoints? Intersect(CollisionObject other)
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
            case ShapeType.Ray: return Intersect(other.GetRayShape());
            case ShapeType.Line: return Intersect(other.GetLineShape());
            case ShapeType.Segment: return Intersect(other.GetSegmentShape());
            case ShapeType.Triangle: return Intersect(other.GetTriangleShape());
            case ShapeType.Rect: return Intersect(other.GetRectShape());
            case ShapeType.Quad: return Intersect(other.GetQuadShape());
            case ShapeType.Poly: return Intersect(other.GetPolygonShape());
            case ShapeType.PolyLine: return Intersect(other.GetPolylineShape());
        }

        return null;
    }
    public CollisionPoints? Intersect(IShape other)
    {
        if (!Enabled) return null;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Intersect(other.GetCircleShape());
            case ShapeType.Ray: return Intersect(other.GetRayShape());
            case ShapeType.Line: return Intersect(other.GetLineShape());
            case ShapeType.Segment: return Intersect(other.GetSegmentShape());
            case ShapeType.Triangle: return Intersect(other.GetTriangleShape());
            case ShapeType.Rect: return Intersect(other.GetRectShape());
            case ShapeType.Quad: return Intersect(other.GetQuadShape());
            case ShapeType.Poly: return Intersect(other.GetPolygonShape());
            case ShapeType.PolyLine: return Intersect(other.GetPolylineShape());
        }

        return null;
    }

    public CollisionPoints? Intersect(Ray ray)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(ray);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(ray);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(ray);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(ray);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(ray);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(ray);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(ray);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(ray);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(ray);
        }

        return null;
    }
    public CollisionPoints? Intersect(Line line)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(line);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(line);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(line);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(line);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(line);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(line);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(line);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(line);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(line);
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
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(segment);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(segment);
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
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(triangle);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(triangle);
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
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(circle);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(circle);
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
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(rect);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(rect);
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
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(quad);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(quad);
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
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(poly);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(poly);
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
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(polyLine);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(polyLine);
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
    
    
    
    public int Intersect(CollisionObject other, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled || !other.Enabled || !other.HasColliders) return 0;
        var count = 0;
        foreach (var otherCol in other.Colliders)
        {
            count += Intersect(otherCol, ref points, returnAfterFirstValid);
        }
        return count;
    }
    public int Intersect(Collider other, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled || !other.Enabled) return 0;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Intersect(other.GetCircleShape(), ref points, returnAfterFirstValid);
            case ShapeType.Ray: return Intersect(other.GetRayShape(), ref points, returnAfterFirstValid);
            case ShapeType.Line: return Intersect(other.GetLineShape(), ref points, returnAfterFirstValid);
            case ShapeType.Segment: return Intersect(other.GetSegmentShape(), ref points, returnAfterFirstValid);
            case ShapeType.Triangle: return Intersect(other.GetTriangleShape(), ref points, returnAfterFirstValid);
            case ShapeType.Rect: return Intersect(other.GetRectShape(), ref points, returnAfterFirstValid);
            case ShapeType.Quad: return Intersect(other.GetQuadShape(), ref points, returnAfterFirstValid);
            case ShapeType.Poly: return Intersect(other.GetPolygonShape(), ref points, returnAfterFirstValid);
            case ShapeType.PolyLine: return Intersect(other.GetPolylineShape(), ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int Intersect(IShape other, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Intersect(other.GetCircleShape(), ref points, returnAfterFirstValid);
            case ShapeType.Ray: return Intersect(other.GetRayShape(), ref points, returnAfterFirstValid);
            case ShapeType.Line: return Intersect(other.GetLineShape(), ref points, returnAfterFirstValid);
            case ShapeType.Segment: return Intersect(other.GetSegmentShape(), ref points, returnAfterFirstValid);
            case ShapeType.Triangle: return Intersect(other.GetTriangleShape(), ref points, returnAfterFirstValid);
            case ShapeType.Rect: return Intersect(other.GetRectShape(), ref points, returnAfterFirstValid);
            case ShapeType.Quad: return Intersect(other.GetQuadShape(), ref points, returnAfterFirstValid);
            case ShapeType.Poly: return Intersect(other.GetPolygonShape(), ref points, returnAfterFirstValid);
            case ShapeType.PolyLine: return Intersect(other.GetPolylineShape(), ref points, returnAfterFirstValid);
        }

        return 0;
    }

    public int Intersect(Ray ray, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(ray, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(ray, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(ray, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(ray, ref points);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(ray, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(ray, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(ray, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(ray, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(ray, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int Intersect(Line line, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(line, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(line, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(line, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(line, ref points);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(line, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(line, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(line, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(line, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(line, ref points, returnAfterFirstValid);
        }

        return 0;
    }

    public int Intersect(Segment segment, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(segment, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(segment, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(segment, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(segment, ref points);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(segment, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(segment, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(segment, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(segment, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(segment, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    
    public int Intersect(Triangle triangle, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(triangle, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(triangle, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(triangle, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(triangle, ref points, returnAfterFirstValid);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(triangle, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(triangle, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(triangle, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(triangle, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(triangle, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int Intersect(Circle circle, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(circle, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(circle, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(circle, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(circle, ref points, returnAfterFirstValid);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(circle, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(circle, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(circle, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(circle, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(circle, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int Intersect(Rect rect, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(rect, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(rect, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(rect, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(rect, ref points, returnAfterFirstValid);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(rect, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(rect, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(rect, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(rect, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(rect, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int Intersect(Quad quad, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(quad, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(quad, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(quad, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(quad, ref points, returnAfterFirstValid);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(quad, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(quad, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(quad, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(quad, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(quad, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int Intersect(Polygon poly, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(poly, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(poly, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(poly, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(poly, ref points, returnAfterFirstValid);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(poly, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(poly, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(poly, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(poly, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(poly, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    public int Intersect(Polyline polyLine, ref CollisionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled) return 0;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(polyLine, ref points, returnAfterFirstValid);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.IntersectShape(polyLine, ref points);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.IntersectShape(polyLine, ref points);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(polyLine, ref points, returnAfterFirstValid);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(polyLine, ref points, returnAfterFirstValid);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(polyLine, ref points, returnAfterFirstValid);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(polyLine, ref points, returnAfterFirstValid);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(polyLine, ref points, returnAfterFirstValid);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(polyLine, ref points, returnAfterFirstValid);
        }

        return 0;
    }
    
    #endregion

}
