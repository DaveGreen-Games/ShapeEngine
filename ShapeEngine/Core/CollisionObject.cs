using System.ComponentModel;
using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.Core;

public abstract class CollisionObject : PhysicsObject
{
    public CollisionObject()
    {
        this.Transform = new();
    }
    public CollisionObject(Vector2 position)
    {
        this.Transform = new(position);
    }
    public CollisionObject(Transform2D transform)
    {
        this.Transform = transform;
    }

    private bool enabled = true;

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
            col.SetupTransform(Transform);
            col.Parent = this;
            return true;
        }
        return false;
    }
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
            collider.UpdateTransform(trans);
            collider.Update(time.Delta);
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