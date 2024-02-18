using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public abstract class CollisionBody : IPhysicsObject
{
    public CollisionBody(Vector2 position)
    {
        Position = position;
        // CollisionMask = collisionMask;
        // CollisionLayer = collisionLayer;
    }

    private Vector2 accumulatedForce = new(0f);
        
    public bool Enabled { get; set; } = true;
    public Vector2 Position { get; set; }
    public float RotationRad { get; set; } = 0f;
    public Vector2 Scale { get; set; } = new(1f);
    public Transform2D Transform => new(Position, RotationRad, Scale);
    public Vector2 Velocity { get; set; } = new(0f);
    public Vector2 ConstAcceleration { get; set; } = new(0f);
        
    public float Mass { get; set; } = 1.0f;
    public float Drag { get; set; } = 0f;


    public bool AddCollider(Collider col)
    {
        if (colliders.Add(col))
        {
            col.SetupPosition(Transform);
            col.Parent = this;
            return true;
        }
        return false;
    }

    public bool RemoveCollider(Collider col)
    {
        if (colliders.Remove(col))
        {
            col.Parent = null;
            return true;
        }
            
        return false;
    }

    private HashSet<Collider> colliders = new();
    public IEnumerable<Collider> Colliders => colliders;
    public bool HasColliders => colliders.Count > 0;

    //public Collider2? GetSingleCollider() => Colliders.Count != 1 ? null : Colliders[0];
        
    public Vector2 GetAccumulatedForce() => accumulatedForce;

    public void ClearAccumulatedForce() => accumulatedForce = new(0f);

    public void AddForce(Vector2 force) => accumulatedForce = ShapePhysics.AddForce(force, accumulatedForce, Mass);

    public void AddImpulse(Vector2 force) => Velocity = ShapePhysics.AddImpulse(force, Velocity, Mass);

    public void UpdateState(float dt)
    {
        var trans = Transform;
        ShapePhysics.UpdateState(this, dt);
        foreach (var collider in Colliders)
        {
            collider.UpdateState(dt, trans);
        }
        OnUpdateState(dt);
    }

    #region Overlap
    public bool Overlap(CollisionBody other)
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
    public CollisionPoints? Intersect(CollisionBody other)
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
        
    protected virtual void OnUpdateState(float dt)
    {
            
    }

    public Vector2 GetPosition() => Position;
    public Rect GetBoundingBox()
    {
        if (!Enabled || !HasColliders) return new();

        Rect boundingBox = new();
        foreach (var col in colliders)
        {
            if(!col.Enabled) continue;
            boundingBox = boundingBox.Union(col.GetBoundingBox());
        }

        return boundingBox;
    }
}