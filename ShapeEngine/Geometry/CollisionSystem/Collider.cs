using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.LineDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.QuadDef;
using ShapeEngine.Geometry.RayDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Abstract base class for all colliders in the collision system.
/// </summary>
/// <remarks>
/// Provides a unified interface for collision detection, overlap, intersection, and containment logic for various shape types.
/// Handles event notification for collision events and manages parent relationships and collider state.
/// </remarks>
public abstract class Collider : Shape
{
    /// <summary>
    /// Occurs when this collider intersects with another collider.
    /// <para>AdvancedCollisionNotification must be enabled on the parent for this event to be invoked.</para>
    /// <list type="bullet">
    /// <item><description>Action parameter: <see cref="Collision"/> information about the intersection.</description></item>
    /// </list>
    /// </summary>
    public event Action<Collision>? OnIntersected;
    /// <summary>
    /// Occurs when this collider overlaps with another collider.
    /// <para>AdvancedCollisionNotification must be enabled on the parent for this event to be invoked.</para>
    /// <list type="bullet">
    /// <item><description>Action parameter: <see cref="CollisionSystem.Overlap"/> information about the overlap.</description></item>
    /// </list>
    /// </summary>
    public event Action<Overlap>? OnOverlapped;
    /// <summary>
    /// Occurs when a collision (intersection or overlap) with this collider ends.
    /// <para>AdvancedCollisionNotification must be enabled on the parent for this event to be invoked.</para>
    /// <list type="bullet">
    /// <item><description>Action parameter: The <see cref="Collider"/> with which contact ended.</description></item>
    /// </list>
    /// </summary>
    public event Action<Collider>? OnContactEnded;
    
    private CollisionObject? parent;
    /// <summary>
    /// Gets the parent <see cref="CollisionObject"/> of this collider, or sets it internally.
    /// </summary>
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
    /// <summary>
    /// Gets or sets whether this collider is enabled for collision detection.
    /// </summary>
    public bool Enabled
    {
        get
        {
            if (parent != null) return parent.Enabled && enabled;
            return enabled;
        }
        set => enabled = value;
    }
    /// <summary>
    /// Gets the velocity of the parent <see cref="CollisionObject"/>, or <c>Vector2.Zero</c> if no parent.
    /// </summary>
    public Vector2 Velocity => parent?.Velocity ?? new(0f);
    /// <summary>
    /// Gets or sets the collision mask for this collider.
    /// </summary>
    public BitFlag CollisionMask { get; set; } = BitFlag.Empty;
    /// <summary>
    /// Gets or sets the collision layer for this collider.
    /// </summary>
    public uint CollisionLayer { get; set; }
    /// <summary>
    /// Gets or sets whether this collider should compute collisions.
    /// </summary>
    /// <remarks>
    /// If false the collider still takes place in the collision system and other colliders can collide with it, but it will not report overlaps/collisions itself.
    /// </remarks>
    public bool ComputeCollision { get; set; } = true;
    /// <summary>
    /// Gets or sets whether this collider should compute intersection details (if false, only overlaps are reported).
    /// </summary>
    public bool ComputeIntersections { get; set; }
  
    /// <summary>
    /// Initializes a new instance of the <see cref="Collider"/> class with default offset values.
    /// </summary>
    protected Collider()
    {
        this.Offset = new(new Vector2(0f), 0f, new Size(0f), 1f);
    }
   
    /// <summary>
    /// Initializes a new instance of the <see cref="Collider"/> class with a specified offset as a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="offset">The positional offset for the collider.</param>
    protected Collider(Vector2 offset)
    {
        this.Offset = new(offset, 0f, new Size(0f), 1f);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Collider"/> class with a specified <see cref="Transform2D"/> offset.
    /// </summary>
    /// <param name="offset">The transform offset for the collider.</param>
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
    /// Called by the parent when a valid intersection occurs with this collider.
    /// Requires <see cref="CollisionObject.AdvancedCollisionNotification"/> to be enabled on the parent.
    /// </summary>
    /// <param name="info">Information about the collision intersection.</param>
    protected virtual void Intersected(Collision info) { }

    /// <summary>
    /// Called by the parent when an overlap (without valid intersection) occurs with this collider.
    /// Requires <see cref="CollisionObject.AdvancedCollisionNotification"/> to be enabled on the parent.
    /// </summary>
    /// <param name="contact">Information about the overlap.</param>
    protected virtual void Overlapped(Overlap contact) { }

    /// <summary>
    /// Called by the parent when a collision (intersection or overlap) with this collider ends.
    /// Requires <see cref="CollisionObject.AdvancedCollisionNotification"/> to be enabled on the parent.
    /// </summary>
    /// <param name="other">The collider with which contact ended.</param>
    protected virtual void ContactEnded(Collider other) { }
    

    /// <summary>
    /// Initializes the collider shape with the given parent transform.
    /// </summary>
    /// <param name="parentTransform">The transform of the parent object.</param>
    /// Calls:
    /// <code>
    /// UpdateTransform(parentTransform);
    /// OnInitialized();
    /// </code>
    /// <remarks>
    /// Override for custom behavior.
    /// </remarks>
    public override void InitializeShape(Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnInitialized();
    }

    /// <summary>
    /// Updates the collider shape based on the elapsed time and parent transform.
    /// </summary>
    /// <param name="dt">The delta time since the last update.</param>
    /// <param name="parentTransform">The transform of the parent object.</param>
    /// Calls:
    /// <code>
    /// UpdateTransform(parentTransform);
    /// OnUpdate(dt);
    /// </code>
    /// <remarks>
    /// Override for custom behavior.
    /// </remarks>
    public override void UpdateShape(float dt, Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnUpdate(dt);
    }

    /// <summary>
    /// Draws the collider shape.
    /// </summary>
    /// <remarks>
    /// Override for custom behavior.
    /// </remarks>
    public override void DrawShape()
    {
        OnDraw();
    }

    /// <summary>
    /// Recalculates the collider shape. No implementation in base class.
    /// </summary>
    /// <remarks>
    /// Override for custom behavior.
    /// </remarks>
    public override void RecalculateShape() { }

    /// <summary>
    /// Called when the collider is initialized. No implementation in base class.
    /// </summary>
    /// <remarks>
    /// Override for custom behavior.
    /// </remarks>
    protected override void OnInitialized() { }

    /// <summary>
    /// Called when the collider is updated. No implementation in base class.
    /// </summary>
    /// <param name="dt">The delta time since the last update.</param>
    /// <remarks>
    /// Override for custom behavior.
    /// </remarks>
    protected override void OnUpdate(float dt) { }

    /// <summary>
    /// Called when the collider is drawn. No implementation in base class.
    /// </summary>
    /// <remarks>
    /// Override for custom behavior.
    /// </remarks>
    protected override void OnDraw() { }
    
    
    /// <summary>
    /// Called when this collider is added to a <see cref="CollisionObject"/> parent.
    /// </summary>
    /// <param name="newParent">The parent collision object.</param>
    /// <remarks>
    /// No implementation in base class.
    /// Override for custom behavior.
    /// </remarks>
    protected virtual void OnAddedToCollisionBody(CollisionObject newParent) { }
    /// <summary>
    /// Called when this collider is removed from a <see cref="CollisionObject"/> parent.
    /// </summary>
    /// <param name="formerParent">The previous parent collision object.</param>
    /// <remarks>
    /// No implementation in base class.
    /// Override for custom behavior.
    /// </remarks>
    protected virtual void OnRemovedFromCollisionBody(CollisionObject formerParent) { }
    /// <summary>
    /// Gets the axis-aligned bounding box of this collider.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public abstract Rect GetBoundingBox();

    /// <summary>
    /// Projects this collider's shape along a given vector.
    /// </summary>
    /// <param name="v">The direction vector to project along.</param>
    /// <returns>A <see cref="Polygon"/> representing the projection, or null if not supported.</returns>
    public Polygon? Project(Vector2 v)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.ProjectShape(v);
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

    /// <summary>
    /// Returns the closest point on this collider to the given shape.
    /// </summary>
    /// <param name="shape">The shape to test against. Must implement <see cref="IShape"/>.</param>
    /// <returns>A <see cref="ClosestPointResult"/> containing the closest points and related data.</returns>
    /// <remarks>
    /// The result depends on the runtime type of both this collider and the provided shape.
    /// </remarks>
    public ClosestPointResult GetClosestPoint(IShape shape)
    {
        switch (shape.GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetClosestPoint(shape.GetCircleShape());
            case ShapeType.Segment: return GetClosestPoint(shape.GetSegmentShape());
            case ShapeType.Line:return GetClosestPoint(shape.GetLineShape());
            case ShapeType.Ray:return GetClosestPoint(shape.GetRayShape());
            case ShapeType.Triangle: return GetClosestPoint(shape.GetTriangleShape());
            case ShapeType.Quad: return GetClosestPoint(shape.GetQuadShape());
            case ShapeType.Rect: return GetClosestPoint(shape.GetRectShape());
            case ShapeType.Poly: return GetClosestPoint(shape.GetPolygonShape());
            case ShapeType.PolyLine: return GetClosestPoint(shape.GetPolylineShape());
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point on this collider to the given point.
    /// </summary>
    /// <param name="p">The point in world space to test against.</param>
    /// <param name="disSquared">Outputs the squared distance from the point to the closest point on the collider. Returns -1 if not applicable.</param>
    /// <returns>A <see cref="IntersectionPoint"/> representing the closest point on the collider.</returns>
    /// <remarks>
    /// The squared distance is often more efficient for comparisons than the actual distance.
    /// </remarks>
    public IntersectionPoint GetClosestPoint(Vector2 p, out float disSquared)
    {
        disSquared = -1;
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(p, out disSquared);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(p, out disSquared);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(p, out disSquared);
        }

        return new();
    }

    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Segment"/>.
    /// </summary>
    /// <param name="shape">The segment to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Segment shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(shape);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Line"/>.
    /// </summary>
    /// <param name="line">The line to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Line line)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(line);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(line);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(line);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(line);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(line);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(line);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(line);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(line);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(line);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The ray to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Ray ray)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(ray);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(ray);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(ray);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(ray);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(ray);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(ray);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(ray);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(ray);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(ray);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Circle"/>.
    /// </summary>
    /// <param name="shape">The circle to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Circle shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(shape);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Triangle"/>.
    /// </summary>
    /// <param name="shape">The triangle to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Triangle shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(shape);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Quad"/>.
    /// </summary>
    /// <param name="shape">The quad to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Quad shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(shape);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Rect"/>.
    /// </summary>
    /// <param name="shape">The rectangle to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Rect shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(shape);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Polygon"/>.
    /// </summary>
    /// <param name="shape">The polygon to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Polygon shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(shape);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestPoint(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestPoint(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestPoint(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestPoint(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestPoint(shape);
        }

        return new();
    }
    /// <summary>
    /// Returns the closest point(s) between this collider and a <see cref="Polyline"/>.
    /// </summary>
    /// <param name="shape">The polyline to test against.</param>
    /// <returns>A <see cref="ClosestPointResult"/> describing the closest points and related data.</returns>
    public ClosestPointResult GetClosestPoint(Polyline shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None:
                break;
            case ShapeType.Circle: return GetCircleShape().GetClosestPoint(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestPoint(shape);
            case ShapeType.Line:return GetLineShape().GetClosestPoint(shape);
            case ShapeType.Ray:return GetRayShape().GetClosestPoint(shape);
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

    /// <summary>
    /// Checks if this collider contains the given point.
    /// </summary>
    /// <param name="p">The point to check.</param>
    /// <returns>True if the point is contained, otherwise false.</returns>
    public bool ContainsPoint(Vector2 p)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsPoint(p);
            case ShapeType.Segment: return GetSegmentShape().OverlapPoint(p);
            case ShapeType.Line: return GetLineShape().OverlapPoint(p);
            case ShapeType.Ray: return GetRayShape().OverlapPoint(p);
            case ShapeType.Triangle: return GetTriangleShape().ContainsPoint(p);
            case ShapeType.Quad: return GetQuadShape().ContainsPoint(p);
            case ShapeType.Rect: return GetRectShape().ContainsPoint(p);
            case ShapeType.Poly: return GetPolygonShape().ContainsPoint(p);
            case ShapeType.PolyLine: return GetPolylineShape().OverlapPoint(p);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given shape.
    /// </summary>
    /// <param name="shape">The shape to check.</param>
    /// <returns>True if the shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(IShape shape)
    {
        switch (shape.GetShapeType())
        {
            case ShapeType.Segment: return ContainsShape(shape.GetSegmentShape());
            case ShapeType.Line: return false; //ContainsShape(shape.GetLineShape());
            case ShapeType.Ray: return false; //ContainsShape(shape.GetRayShape());
            case ShapeType.Circle: return ContainsShape(shape.GetCircleShape());
            case ShapeType.Triangle: return ContainsShape(shape.GetTriangleShape());
            case ShapeType.Quad: return ContainsShape(shape.GetQuadShape());
            case ShapeType.Rect: return ContainsShape(shape.GetRectShape());
            case ShapeType.Poly: return ContainsShape(shape.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(shape.GetPolylineShape());
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given segment shape.
    /// </summary>
    /// <param name="shape">The segment shape to check.</param>
    /// <returns>True if the segment shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(Segment shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                break;
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given circle shape.
    /// </summary>
    /// <param name="shape">The circle shape to check.</param>
    /// <returns>True if the circle shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(Circle shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                break;
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given triangle shape.
    /// </summary>
    /// <param name="shape">The triangle shape to check.</param>
    /// <returns>True if the triangle shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(Triangle shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                break;
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given quad shape.
    /// </summary>
    /// <param name="shape">The quad shape to check.</param>
    /// <returns>True if the quad shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(Quad shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                break;
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given rectangle shape.
    /// </summary>
    /// <param name="shape">The rectangle shape to check.</param>
    /// <returns>True if the rectangle shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(Rect shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                break;
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given polygon shape.
    /// </summary>
    /// <param name="shape">The polygon shape to check.</param>
    /// <returns>True if the polygon shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(Polygon shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                break;
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider contains the given polyline shape.
    /// </summary>
    /// <param name="shape">The polyline shape to check.</param>
    /// <returns>True if the polyline shape is contained, otherwise false.</returns>
    /// <remarks>
    /// The entire shape must be fully enclosed within this collider for the method to return true.
    /// Partial overlaps or edge contacts are not considered as containment.
    /// </remarks>
    public bool ContainsShape(Polyline shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Line:
            case ShapeType.Ray:
            case ShapeType.Segment:
            case ShapeType.PolyLine:
                break;
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsPoints(shape);
        }

        return false;
    }
    #endregion
    
    #region Overlap
    /// <summary>
    /// Checks if this collider overlaps with another <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="other">The other collision object.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
    /// <summary>
    /// Checks if this collider overlaps with another collider.
    /// </summary>
    /// <param name="other">The other collider.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
    public bool Overlap(Collider other)
    {
        if (!Enabled || !other.Enabled) return false;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Overlap(other.GetCircleShape());
            case ShapeType.Segment: return Overlap(other.GetSegmentShape());
            case ShapeType.Line: return Overlap(other.GetLineShape());
            case ShapeType.Ray: return Overlap(other.GetRayShape());
            case ShapeType.Triangle: return Overlap(other.GetTriangleShape());
            case ShapeType.Quad: return Overlap(other.GetQuadShape());
            case ShapeType.Rect: return Overlap(other.GetRectShape());
            case ShapeType.Poly: return Overlap(other.GetPolygonShape());
            case ShapeType.PolyLine: return Overlap(other.GetPolylineShape());
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider overlaps with the given shape.
    /// </summary>
    /// <param name="other">The other shape.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
    public bool Overlap(IShape other)
    {
        if (!Enabled) return false;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Overlap(other.GetCircleShape());
            case ShapeType.Segment: return Overlap(other.GetSegmentShape());
            case ShapeType.Line: return Overlap(other.GetLineShape());
            case ShapeType.Ray: return Overlap(other.GetRayShape());
            case ShapeType.Triangle: return Overlap(other.GetTriangleShape());
            case ShapeType.Quad: return Overlap(other.GetQuadShape());
            case ShapeType.Rect: return Overlap(other.GetRectShape());
            case ShapeType.Poly: return Overlap(other.GetPolygonShape());
            case ShapeType.PolyLine: return Overlap(other.GetPolylineShape());
        }

        return false;
    }

    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Segment"/>.
    /// </summary>
    /// <param name="segment">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(segment);
            case ShapeType.Ray:
                var rayShape = GetLineShape();
                return rayShape.OverlapShape(segment);
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
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Line"/>.
    /// </summary>
    /// <param name="line">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
    public bool Overlap(Line line)
    { 
        if (!Enabled) return false;
        
        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(line);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(line);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(line);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.OverlapShape(line);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(line);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(line);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(line);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(line);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(line);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
    public bool Overlap(Ray ray)
    { 
        if (!Enabled) return false;
        
        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(ray);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(ray);
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(ray);
            case ShapeType.Ray:
                var rayShape = GetRayShape();
                return rayShape.OverlapShape(ray);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(ray);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(ray);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(ray);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(ray);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(ray);
        }

        return false;
    }
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Triangle"/>.
    /// </summary>
    /// <param name="triangle">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(triangle);
            case ShapeType.Ray:
                var rayShape = GetLineShape();
                return rayShape.OverlapShape(triangle);
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
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Circle"/>.
    /// </summary>
    /// <param name="circle">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(circle);
            case ShapeType.Ray:
                var rayShape = GetLineShape();
                return rayShape.OverlapShape(circle);
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
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Rect"/>.
    /// </summary>
    /// <param name="rect">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(rect);
            case ShapeType.Ray:
                var rayShape = GetLineShape();
                return rayShape.OverlapShape(rect);
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
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Quad"/>.
    /// </summary>
    /// <param name="quad">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(quad);
            case ShapeType.Ray:
                var rayShape = GetLineShape();
                return rayShape.OverlapShape(quad);
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
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Polygon"/>.
    /// </summary>
    /// <param name="poly">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(poly);
            case ShapeType.Ray:
                var rayShape = GetLineShape();
                return rayShape.OverlapShape(poly);
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
    /// <summary>
    /// Checks if this collider overlaps with the given <see cref="Polyline"/>.
    /// </summary>
    /// <param name="polyLine">The segment to check for overlap.</param>
    /// <returns>True if there is an overlap, otherwise false.</returns>
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
            case ShapeType.Line:
                var l = GetLineShape();
                return l.OverlapShape(polyLine);
            case ShapeType.Ray:
                var rayShape = GetLineShape();
                return rayShape.OverlapShape(polyLine);
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
    /// <summary>
    /// Returns the intersection points between this collider and another <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="other">The other collision object.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(CollisionObject other)
    {
        if (!Enabled || !other.Enabled || !other.HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var otherCol in other.Colliders)
        {
            var points = Intersect(otherCol);
            if (points == null) continue;
            
            result ??= new();
            result.AddRange(points);
        }
        return result;
    }
    /// <summary>
    /// Returns the intersection points between this collider and another collider.
    /// </summary>
    /// <param name="other">The other collider.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Collider other)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given shape.
    /// </summary>
    /// <param name="other">The other shape.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(IShape other)
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

    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Ray"/>.
    /// </summary>
    /// <param name="ray">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Ray ray)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Line"/>.
    /// </summary>
    /// <param name="line">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Line line)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Segment"/>.
    /// </summary>
    /// <param name="segment">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Segment segment)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Triangle"/>.
    /// </summary>
    /// <param name="triangle">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Triangle triangle)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Circle"/>.
    /// </summary>
    /// <param name="circle">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Circle circle)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Rect"/>.
    /// </summary>
    /// <param name="rect">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Rect rect)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Quad"/>.
    /// </summary>
    /// <param name="quad">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Quad quad)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Polygon"/>.
    /// </summary>
    /// <param name="poly">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Polygon poly)
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
    /// <summary>
    /// Returns the intersection points between this collider and the given <see cref="Polyline"/>.
    /// </summary>
    /// <param name="polyLine">The ray to check for intersection.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Polyline polyLine)
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
    
    
    /// <summary>
    /// Returns the number of intersection points between this collider and another <see cref="CollisionObject"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="other">The other collision object to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(CollisionObject other, ref IntersectionPoints points, bool returnAfterFirstValid = false)
    {
        if (!Enabled || !other.Enabled || !other.HasColliders) return 0;
        var count = 0;
        foreach (var otherCol in other.Colliders)
        {
            count += Intersect(otherCol, ref points, returnAfterFirstValid);
        }
        return count;
    }
    
    /// <summary>
    /// Returns the number of intersection points between this collider and another <see cref="Collider"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="other">The other collider to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Collider other, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="IShape"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="other">The other shape to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(IShape other, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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

    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Ray"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="ray">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Ray ray, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Line"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="line">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Line line, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Segment"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="segment">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Segment segment, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Triangle"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="triangle">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Triangle triangle, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Circle"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="circle">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Circle circle, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Rect"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="rect">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Rect rect, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Quad"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="quad">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Quad quad, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Polygon"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="poly">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Polygon poly, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
    /// <summary>
    /// Returns the number of intersection points between this collider and the given <see cref="Polyline"/>.
    /// Adds the intersection points to the provided <paramref name="points"/> collection.
    /// </summary>
    /// <param name="polyLine">The ray to check for intersection.</param>
    /// <param name="points">A reference to a <see cref="IntersectionPoints"/> collection to store the intersection points.</param>
    /// <param name="returnAfterFirstValid">
    /// If true, the method returns after finding the first valid intersection; otherwise, it continues to find all intersections.
    /// </param>
    /// <returns>The number of intersection points found and added to <paramref name="points"/>.</returns>
    public int Intersect(Polyline polyLine, ref IntersectionPoints points, bool returnAfterFirstValid = false)
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
