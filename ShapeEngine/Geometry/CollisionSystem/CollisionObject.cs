using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;
using ShapeEngine.Geometry.CircleDef;
using ShapeEngine.Geometry.CollisionSystem.CollisionHandlerDef;
using ShapeEngine.Geometry.PolygonDef;
using ShapeEngine.Geometry.PolylineDef;
using ShapeEngine.Geometry.RectDef;
using ShapeEngine.Geometry.SegmentDef;
using ShapeEngine.Geometry.TriangleDef;

namespace ShapeEngine.Geometry.CollisionSystem;

/// <summary>
/// Represents an object that participates in the collision system and can contain multiple colliders.
/// Handles collision detection, intersection, and overlap logic,
/// and provides events and hooks for collision notifications.
/// </summary>
/// <remarks>
/// This is an abstract base class for objects that require collision handling.
/// It manages a set of colliders and provides
/// advanced notification options for collision events.
/// Inherits from <see cref="PhysicsObject"/>.
/// </remarks>
public abstract class CollisionObject : PhysicsObject
{
    /// <summary>
    /// Occurs when this object is involved in a collision (intersection or overlap).
    /// <list type="bullet">
    /// <item><description>Action parameter: <see cref="CollisionInformation"/> containing details about the collision.</description></item>
    /// </list>
    /// </summary>
    public event Action<CollisionInformation>? OnCollision;
    /// <summary>
    /// Occurs when all colliders between this object and another object have ended their contact.
    /// <list type="bullet">
    /// <item><description>First parameter: This <see cref="CollisionObject"/>.</description></item>
    /// <item><description>Second parameter: The other <see cref="CollisionObject"/> the contact ended with.</description></item>
    /// </list>
    /// </summary>
    public event Action<CollisionObject, CollisionObject>? OnContactEnded;

    /// <summary>
    /// Occurs when a collider of this object intersects with another collider.
    /// <list type="bullet">
    /// <item><description>Action parameter: <see cref="Collision"/> information about the intersection.</description></item>
    /// </list>
    /// </summary>
    public event Action<Collision>? OnColliderIntersected;
    /// <summary>
    /// Occurs when a collider of this object overlaps with another collider.
    /// <list type="bullet">
    /// <item><description>Action parameter: <see cref="CollisionSystem.Overlap"/> information about the overlap.</description></item>
    /// </list>
    /// </summary>
    public event Action<Overlap>? OnColliderOverlapped;
    /// <summary>
    /// Occurs when contact between two colliders ends.
    /// <list type="bullet">
    /// <item><description>First parameter: The collider of this object.</description></item>
    /// <item><description>Second parameter: The collider of the other object.</description></item>
    /// </list>
    /// </summary>
    public event Action<Collider, Collider>? OnColliderContactEnded;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionObject"/> class with a default transform.
    /// </summary>
    public CollisionObject()
    {
        this.Transform = new();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionObject"/> class at the specified position.
    /// </summary>
    /// <param name="position">The initial position of the object.</param>
    public CollisionObject(Vector2 position)
    {
        this.Transform = new(position);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionObject"/> class with the specified transform.
    /// </summary>
    /// <param name="transform">The initial transform of the object.</param>
    public CollisionObject(Transform2D transform)
    {
        this.Transform = transform;
    }

    private bool enabled = true;

    /// <summary>
    /// Gets or sets whether this object is passive in collision calculations.
    /// <para>
    /// If <c>true</c>, when a passive object checks for collisions against non-passive objects,
    /// the collision information is generated as if the non-passive object checked for the collision.
    /// This affects collision normal direction. Only relevant for colliders that compute intersections.
    /// </para>
    /// </summary>
    public bool Passive = false;

    /// <summary>
    /// Gets or sets whether this object is enabled for collision detection.
    /// </summary>
    public bool Enabled
    {
        get => enabled && !IsDead;
        set => enabled = value;
    }

    /// <summary>
    /// Gets or sets whether to project the shape for collision calculations.
    /// Uses the current velocity of the frame to project the shape.
    /// Useful for fast and/or small objects.
    /// Projecting shapes can be expensive!
    /// </summary>
    public bool ProjectShape = false;

    /// <summary>
    /// Gets or sets whether all generated collision points are filtered based on <see cref="CollisionPointsFilterType"/>.
    /// <para>
    /// Only colliders with <c>ComputeIntersections</c> enabled will generate collision points to be filtered.
    /// </para>
    /// </summary>
    public bool FilterCollisionPoints = false;

    /// <summary>
    /// Gets or sets the filter type used when <see cref="FilterCollisionPoints"/> is enabled.
    /// </summary>
    public CollisionPointsFilterType CollisionPointsFilterType = CollisionPointsFilterType.Closest;

    /// <summary>
    /// Gets or sets the motion type of this object for collision detection optimizations.
    /// <para>
    /// <see cref="MotionType.Dynamic"/> is the default and should be used for moving objects.
    /// <see cref="MotionType.Static"/> is optimized for non-moving objects and can improve broadphase collision detection performance.
    /// </para>
    /// </summary>
    public MotionType MotionType = MotionType.Dynamic;
    
   
    /// <summary>
    /// Gets or sets whether advanced collision notification is enabled.
    /// <para>
    /// If set to <c>true</c>:
    /// <list type="bullet">
    /// <item>
        /// <description>
            /// <see cref="ColliderIntersected(CollisionSystem.Collision)"/>,
            /// <see cref="ColliderOverlapped(CollisionSystem.Overlap)"/>,
            /// and <see cref="ColliderContactEnded(Collider, Collider)"/> virtual functions will be called.
            /// Override them for custom logic.
        /// </description>
    /// </item>
    /// <item>
        /// <description>
            /// <see cref="OnColliderIntersected"/>,
            /// <see cref="OnColliderOverlapped"/>,
            /// and <see cref="OnColliderContactEnded"/> events will be invoked.
        /// </description>
    /// </item>
    /// <item>
        /// <description>
            /// <see cref="Collider"/>-level
            /// <see cref="Collider.Intersected"/>,
            /// <see cref="Collider.Overlapped"/>,
            /// and <see cref="Collider.ContactEnded"/> virtual functions will be called.
        /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public bool AdvancedCollisionNotification = false;
    
    internal void ResolveCollision(CollisionInformation information)
    {
        Collision(information);
        OnCollision?.Invoke(information);

        if(AdvancedCollisionNotification == false) return;

        foreach (var collision in information)
        {
            if (collision.Points != null)
            {
                collision.Self.ResolveIntersected(collision);
                ColliderIntersected(collision);
                OnColliderIntersected?.Invoke(collision);
            }
            else
            {
                var overlap = collision.Overlap;
                collision.Self.ResolveOverlapped(overlap);
                ColliderOverlapped(overlap);
                OnColliderOverlapped?.Invoke(overlap);
            }
        }
        
    }
    internal void ResolveContactEnded(CollisionObject other)
    {
        ContactEnded(other);
        OnContactEnded?.Invoke(this, other);
    }
    internal void ResolveColliderContactEnded(Collider self, Collider other)
    {
        if(AdvancedCollisionNotification == false) return;
        ColliderContactEnded(self, other);
        OnColliderContactEnded?.Invoke(self, other);
    }

    
    /// <summary>
    /// Called when this object is involved in a collision (intersection or overlap).
    /// </summary>
    /// <param name="info">The collision information.</param>
    /// <remarks>
    /// Override for custom logic.
    /// </remarks>
    protected virtual void Collision(CollisionInformation info) { }
    
    /// <summary>
    /// Called when all colliders between this object and another have ended their contact.
    /// </summary>
    /// <param name="other">The other collision object the contact has ended with.</param>
    /// <remarks>
    /// Override for custom logic.
    /// </remarks>
    protected virtual void ContactEnded(CollisionObject other) { }
    
    /// <summary>
    /// Called when advanced collision notification is enabled and a collider of this object intersects another collider.
    /// </summary>
    /// <param name="collision">The collision information.</param>
    /// <remarks>
    /// Override for custom logic.
    /// </remarks>
    protected virtual void ColliderIntersected(Collision collision) { }
    /// <summary>
    /// Called when advanced collision notification is enabled and a collider of this object overlaps another collider.
    /// </summary>
    /// <param name="overlap">The overlap information.</param>
    /// <remarks>
    /// Override for custom logic.
    /// </remarks>
    protected virtual void ColliderOverlapped(Overlap overlap) { }
    /// <summary>
    /// Called when advanced collision notification is enabled and contact between two colliders ends.
    /// </summary>
    /// <param name="self">The collider of this object.</param>
    /// <param name="other">The collider of the other object.</param>
    /// <remarks>
    /// Override for custom logic.
    /// </remarks>
    protected virtual void ColliderContactEnded(Collider self, Collider other) { }
    
    /// <summary>
    /// Adds a collider to this object.
    /// </summary>
    /// <param name="col">The collider to add.</param>
    /// <returns><c>true</c> if the collider was added; otherwise, <c>false</c>.</returns>
    public bool AddCollider(Collider col)
    {
        if (col.Parent != null)
        {
            if (col.Parent == this) return false;
            col.Parent.RemoveCollider(col);
        }
        
        if (Colliders.Add(col))
        {
            col.InitializeShape(Transform);
            col.Parent = this;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a collider from this object.
    /// </summary>
    /// <param name="col">The collider to remove.</param>
    /// <returns><c>true</c> if the collider was removed; otherwise, <c>false</c>.</returns>
    public bool RemoveCollider(Collider col)
    {
        if (Colliders.Remove(col))
        {
            col.Parent = null;
            return true;
        }
            
        return false;
    }
    
    
    

    /// <summary>
    /// Called when this object is added to a <see cref="CollisionHandler"/>.
    /// </summary>
    /// <param name="handler">The collision handler.</param>
    public virtual void OnCollisionSystemEntered(CollisionHandler handler){}
    /// <summary>
    /// Called when this object is removed from a <see cref="CollisionHandler"/>.
    /// </summary>
    /// <param name="handler">The collision handler.</param>
    public virtual void OnCollisionSystemLeft(CollisionHandler handler){}
    
    /// <summary>
    /// Gets the set of colliders attached to this object.
    /// </summary>
    public HashSet<Collider> Colliders { get; } = new();

    /// <summary>
    /// Gets a value indicating whether this object has any colliders.
    /// </summary>
    public bool HasColliders => Colliders.Count > 0;

    /// <summary>
    /// Updates the object and all attached colliders.
    /// </summary>
    /// <param name="time">The current game time.</param>
    /// <param name="game">The main game screen info.</param>
    /// <param name="gameUi">The game UI screen info.</param>
    /// <param name="ui">The UI screen info.</param>
    /// <remarks>
    /// Calls <see cref="OnColliderUpdateFinished"/> at the end.
    /// </remarks>
    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        var trans = Transform;
        base.Update(time, game, gameUi, ui); // updates physics state
        foreach (var collider in Colliders)
        {
            if (collider.Parent != this)
            {
                throw new WarningException("Collision Object tried to update collider with different parent!");
            }
            collider.UpdateShape(time.Delta, trans);
            OnColliderUpdated(collider);
        }
        OnColliderUpdateFinished();
    }

    /// <summary>
    /// Called after a collider has been updated.
    /// </summary>
    /// <param name="collider">The collider that was updated.</param>
    /// <remarks>
    /// Override for custom logic.
    /// </remarks>
    protected virtual void OnColliderUpdated(Collider collider) { }
    /// <summary>
    /// Called after all colliders have been updated.
    /// </summary>
    /// <remarks>
    /// Override for custom logic.
    /// </remarks>
    protected virtual void OnColliderUpdateFinished() { }
    
    /// <summary>
    /// Gets the axis-aligned bounding box that contains all enabled colliders of this object.
    /// </summary>
    /// <returns>The bounding <see cref="Rect"/>.</returns>
    public override Rect GetBoundingBox()
    {
        if (!Enabled || !HasColliders) return new();
    
        Rect boundingBox = new();
        foreach (var col in Colliders)
        {
            if(!col.Enabled) continue;
            if (boundingBox.Width <= 0 || boundingBox.Height <= 0) boundingBox = col.GetBoundingBox();
            else boundingBox = boundingBox.Union(col.GetBoundingBox());
        }
    
        return boundingBox;
    }

    #region Overlap
    /// <summary>
    /// Checks if this object overlaps with another <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="other">The other collision object.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(CollisionObject other)
    {
        if (!Enabled || !other.Enabled || !HasColliders || !other.HasColliders) return false;
        foreach (var col in Colliders)
        {
            foreach (var colOther in other.Colliders)
            {
                if (col.Overlap(colOther)) return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Checks if this object overlaps with a specific collider.
    /// </summary>
    /// <param name="other">The other collider.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(Collider other)
    {
        if (!Enabled || !other.Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(other)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if this object overlaps with a segment shape.
    /// </summary>
    /// <param name="shape">The segment shape.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(Segment shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if this object overlaps with a circle shape.
    /// </summary>
    /// <param name="shape">The circle shape.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(Circle shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if this object overlaps with a triangle shape.
    /// </summary>
    /// <param name="shape">The triangle shape.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(Triangle shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if this object overlaps with a rectangle shape.
    /// </summary>
    /// <param name="shape">The rectangle shape.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(Rect shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if this object overlaps with a polygon shape.
    /// </summary>
    /// <param name="shape">The polygon shape.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(Polygon shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    /// <summary>
    /// Checks if this object overlaps with a polyline shape.
    /// </summary>
    /// <param name="shape">The polyline shape.</param>
    /// <returns><c>true</c> if there is an overlap; otherwise, <c>false</c>.</returns>
    public bool Overlap(Polyline shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    #endregion

    #region Intersect
    /// <summary>
    /// Returns the intersection points between this object and another <see cref="CollisionObject"/>.
    /// </summary>
    /// <param name="other">The other collision object.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(CollisionObject other)
    {
        if (!Enabled || !other.Enabled || !HasColliders || !other.HasColliders) return null;
            
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            foreach (var otherCol in other.Colliders)
            {
                var points = col.Intersect(otherCol);
                if (points == null) continue;
                result ??= new();
                result.AddRange(points);
            }
        }
            
        return result;
    }
    /// <summary>
    /// Returns the intersection points between this object and a specific collider.
    /// </summary>
    /// <param name="other">The other collider.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Collider other)
    {
        if (!Enabled || !other.Enabled || !HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(other);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    /// <summary>
    /// Returns the intersection points between this object and a segment shape.
    /// </summary>
    /// <param name="shape">The segment shape.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Segment shape)
    {
        if (!Enabled || !HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    /// <summary>
    /// Returns the intersection points between this object and a circle shape.
    /// </summary>
    /// <param name="shape">The circle shape.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Circle shape)
    {
        if (!Enabled || !HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    /// <summary>
    /// Returns the intersection points between this object and a triangle shape.
    /// </summary>
    /// <param name="shape">The triangle shape.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Triangle shape)
    {
        if (!Enabled || !HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    /// <summary>
    /// Returns the intersection points between this object and a rectangle shape.
    /// </summary>
    /// <param name="shape">The rectangle shape.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Rect shape)
    {
        if (!Enabled || !HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    /// <summary>
    /// Returns the intersection points between this object and a polygon shape.
    /// </summary>
    /// <param name="shape">The polygon shape.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Polygon shape)
    {
        if (!Enabled || !HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    /// <summary>
    /// Returns the intersection points between this object and a polyline shape.
    /// </summary>
    /// <param name="shape">The polyline shape.</param>
    /// <returns>A <see cref="IntersectionPoints"/> collection, or null if no intersection.</returns>
    public IntersectionPoints? Intersect(Polyline shape)
    {
        if (!Enabled || !HasColliders) return null;
        IntersectionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    #endregion

}