using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core;

public abstract class CollisionObject : PhysicsObject
{
    /// <summary>
    /// If enabled this CollisionObject will subscribe to every colliders Collision/ CollisionEnded /ColliderOverlap / ColliderIntersected event and report them.
    /// </summary>
    protected readonly bool ReportColliderCollisions;
    
    public CollisionObject(bool reportColliderCollisions = false)
    {
        ReportColliderCollisions = reportColliderCollisions;
        this.Transform = new();
    }
    public CollisionObject(Vector2 position, bool reportColliderCollisions = false)
    {
        ReportColliderCollisions = reportColliderCollisions;
        this.Transform = new(position);
    }
    public CollisionObject(Transform2D transform, bool reportColliderCollisions = false)
    {
        ReportColliderCollisions = reportColliderCollisions;
        this.Transform = transform;
    }

    private bool enabled = true;

    /// <summary>
    /// If a Passive CollisionObject checks for collisions/intersections against Non-Passive CollisionObjects,
    /// the CollisionInformation will be generated as if the Non-Passive CollisionObject checked for the collision.
    /// This mostly affects CollisionPoint normal generation. Normally the normals are pointing towards the CollisionObject that is checking for the collision.
    /// If this is not wanted and the normals should point away from the CollisionObject that is checking for the collision, the Passive flag should be set to true.
    /// This only comes into effect for colliders that compute intersections!
    /// </summary>
    public bool Passive = false;
    public bool Enabled
    {
        get => enabled && !IsDead;
        set => enabled = value;
    }

    public bool ProjectShape = false;

    // public Polygon? GetProjectedShape(float dt)
    // {
    //     if (Colliders.Count <= 0) return null;
    //     if (Velocity.LengthSquared() <= 0f || dt <= 0f) return null;
    //     Points points = new(Colliders.Count * 6);
    //     var velFraction = Velocity * dt;
    //     foreach (var collider in Colliders)
    //     {
    //         var projectedPoints = collider.Project(velFraction);
    //         if (projectedPoints != null && projectedPoints.Count > 0)
    //         {
    //             points.AddRange(projectedPoints);
    //         }
    //     }
    //
    //     if (points.Count > 0) return points.ToPolygon();
    //
    //     return null;
    // }

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
            if (ReportColliderCollisions)
            {
                col.OnColliderIntersected += ColliderIntersected;
                col.OnColliderOverlapped += ColliderOverlapped;
                col.OnCollision += ColliderCollision;
                col.OnCollisionEnded += ColliderCollisionEnded;
            }
            return true;
        }
        return false;
    }

    public bool RemoveCollider(Collider col)
    {
        if (Colliders.Remove(col))
        {
            col.Parent = null;
            if (ReportColliderCollisions)
            {
                col.OnColliderIntersected -= ColliderIntersected;
                col.OnColliderOverlapped -= ColliderOverlapped;
                col.OnCollision -= ColliderCollision;
                col.OnCollisionEnded -= ColliderCollisionEnded;
            }
            return true;
        }
            
        return false;
    }
    
    /// <summary>
    /// Called when a collider on this CollisionObject reports a collision.
    /// AdvancedCollisionNotifications have to be enabled on the collider!
    /// ComputeIntersection has to be enabled on the collider!
    /// ReportColliderCollision has to be enabled
    /// </summary>
    protected virtual void ColliderIntersected(Collision.Collision collision) { }

    /// <summary>
    /// Called when a collider on this CollisionObject reports an overlap.
    /// AdvancedCollisionNotifications have to be enabled on the collider!
    /// ComputeIntersection has to be disabled on the collider!
    /// ReportColliderCollision has to be enabled
    /// </summary>
    protected virtual void ColliderOverlapped(Collider self, Collider other, bool firstContact) { }

    /// <summary>
    /// Called when a collider on this CollisionObject reports an ended collision.
    /// Only works when ReportColliderCollision is set to true!
    /// </summary>
    protected virtual void ColliderCollisionEnded(Collider self, Collider other) { }
    
    /// <summary>
    /// Called when a collider on this CollisionObject reports a collision.
    /// Only works when ReportColliderCollision is set to true!
    /// </summary>
    protected virtual void ColliderCollision(Collider self, CollisionInformation info) { }
    

    /// <summary>
    /// Is called when collision object is added to a collision handler.
    /// </summary>
    public virtual void OnCollisionSystemEntered(CollisionHandler handler){}
    /// <summary>
    /// Is called when the collision object is removed from a collision handler.
    /// </summary>
    public virtual void OnCollisionSystemLeft(CollisionHandler handler){}
    
    public HashSet<Collider> Colliders { get; } = new();
    public bool HasColliders => Colliders.Count > 0;

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

    protected virtual void OnColliderUpdated(Collider collider) { }
    protected virtual void OnColliderUpdateFinished() { }
    
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
    public bool Overlap(Collider other)
    {
        if (!Enabled || !other.Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(other)) return true;
        }

        return false;
    }
    public bool Overlap(Segment shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    public bool Overlap(Circle shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    public bool Overlap(Triangle shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    public bool Overlap(Rect shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
    public bool Overlap(Polygon shape)
    {
        if (!Enabled || !HasColliders) return false;
        foreach (var col in Colliders)
        {
            if (col.Overlap(shape)) return true;
        }

        return false;
    }
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
    public CollisionPoints? Intersect(CollisionObject other)
    {
        if (!Enabled || !other.Enabled || !HasColliders || !other.HasColliders) return null;
            
        CollisionPoints? result = null;
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
    public CollisionPoints? Intersect(Collider other)
    {
        if (!Enabled || !other.Enabled || !HasColliders) return null;
        CollisionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(other);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    public CollisionPoints? Intersect(Segment shape)
    {
        if (!Enabled || !HasColliders) return null;
        CollisionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    public CollisionPoints? Intersect(Circle shape)
    {
        if (!Enabled || !HasColliders) return null;
        CollisionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    public CollisionPoints? Intersect(Triangle shape)
    {
        if (!Enabled || !HasColliders) return null;
        CollisionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    public CollisionPoints? Intersect(Rect shape)
    {
        if (!Enabled || !HasColliders) return null;
        CollisionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    public CollisionPoints? Intersect(Polygon shape)
    {
        if (!Enabled || !HasColliders) return null;
        CollisionPoints? result = null;
        foreach (var col in Colliders)
        {
            var points = col.Intersect(shape);
            if (points == null) continue;
            result ??= new();
            result.AddRange(points);
        }

        return result;
    }
    public CollisionPoints? Intersect(Polyline shape)
    {
        if (!Enabled || !HasColliders) return null;
        CollisionPoints? result = null;
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