using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Collision;

public abstract class CollisionBody : IPhysicsObject
{
    public CollisionBody()
    {
        this.Transform = new();
    }
    public CollisionBody(Vector2 position)
    {
        this.Transform = new(position);
        // Position = position;
        // CollisionMask = collisionMask;
        // CollisionLayer = collisionLayer;
    }

    public CollisionBody(Transform2D transform)
    {
        this.Transform = transform;
    }

    private Vector2 accumulatedForce = new(0f);
        
    public bool Enabled { get; set; } = true;
    // public Vector2 Position { get; set; }
    // public float RotationRad { get; set; } = 0f;
    // public Vector2 Scale { get; set; } = new(1f);
    // public Transform2D Transform => new(Position, RotationRad, Scale);
    public Transform2D Transform { get; set; }
    public Vector2 Velocity { get; set; } = new(0f);
    public Vector2 ConstAcceleration { get; set; } = new(0f);
        
    public float Mass { get; set; } = 1.0f;
    public float Drag { get; set; } = 0f;


    public bool AddCollider(Collider col)
    {
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

    // private readonly HashSet<Collider> colliders = new();
    public HashSet<Collider> Colliders { get; } = new();

    public bool HasColliders => Colliders.Count > 0;

    //public Collider2? GetSingleCollider() => Colliders.Count != 1 ? null : Colliders[0];
        
    public Vector2 GetAccumulatedForce() => accumulatedForce;

    public void ClearAccumulatedForce() => accumulatedForce = new(0f);

    public void AddForce(Vector2 force) => accumulatedForce = ShapePhysics.AddForce(force, accumulatedForce, Mass);

    public void AddImpulse(Vector2 force) => Velocity = ShapePhysics.AddImpulse(force, Velocity, Mass);

    public void Update(float dt)
    {
        var trans = Transform;
        this.UpdateState(dt);
        foreach (var collider in Colliders)
        {
            collider.UpdateTransform(trans);
            collider.Update(dt);
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

    // public Vector2 GetPosition() => Transform.Position;
    public Rect GetBoundingBox()
    {
        if (!Enabled || !HasColliders) return new();

        Rect boundingBox = new();
        foreach (var col in Colliders)
        {
            if(!col.Enabled) continue;
            boundingBox = boundingBox.Union(col.GetBoundingBox());
        }

        return boundingBox;
    }
}