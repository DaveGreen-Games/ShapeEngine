using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Interfaces;

// public interface ISpatialTest
// {
//     public Vector2 Position { get; set; }
//     public Rect GetBoundingBox();
//         
// }
// public interface IUpdateableTest
// {
//     public void Update(GameTime time, ScreenInfo game, ScreenInfo ui);
// }
// public interface IDrawableTest
// {
//     public void DrawGame(ScreenInfo game);
//     public void DrawGameUI(ScreenInfo ui);
//     
// }
// public interface IKillableTest
// {
//     public bool Kill();
//     public bool IsDead();
// }
// public interface IGameObjectTest : ISpatialTest, IUpdateableTest, IDrawableTest, IKillableTest//, IBehaviorReceiver
// {
//
//     public bool DrawToGame(Rect gameArea);
//     public bool DrawToGameUI(Rect screenArea);
//         
//     public int Layer { get; set; }
//     public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }
//     public sealed bool IsInLayer(int layer) { return this.Layer == layer; }
//     public void AddedToHandler(GameObjectHandler gameObjectHandler);
//     public void RemovedFromHandler(GameObjectHandler gameObjectHandler);
//     public bool CheckHandlerBounds();
//     public void LeftHandlerBounds(BoundsCollisionInfo info);
//     // public virtual bool HasCollisionBody() { return false; }
//     // public virtual CollisionBodyTest? GetCollisionBody() { return null; }
//
// }
// public interface IPhysicsObjectTest : IGameObjectTest
// {
//     public Vector2 Velocity { get; set; }
//     public float Mass { get; set; }
//     public float Drag { get; set; }
//     public Vector2 ConstAcceleration { get; set; }
//     public void AddForce(Vector2 force);
//     public void AddImpulse(Vector2 force);
//     public bool IsStatic(float deltaSq) { return Velocity.LengthSquared() <= deltaSq; }
//     public Vector2 GetAccumulatedForce();
//     public void ClearAccumulatedForce();
//     public void UpdateState(float dt);
// }
//
// public class CollisionBodyTest : IPhysicsObjectTest
// {
//     
// }



public abstract class GameObject2
{
    public Transform2D Transform { get; set; }
    public bool IsDead { get; private set; } = false;
    

    public abstract Rect GetBoundingBox();

    public abstract void Update(GameTime time, ScreenInfo game, ScreenInfo ui);

    public abstract void DrawGame(ScreenInfo game);

    public abstract void DrawGameUI(ScreenInfo ui);

    /// <summary>
    /// Tries to kill this game object.
    /// </summary>
    /// <returns>Returns if kill was successful.</returns>
    public bool Kill(string? killMessage = null, GameObject2? killer = null)
    {
        if (IsDead) return false;

        if (TryKill(killMessage, killer))
        {
            IsDead = true;
            OnKilled();
            return true;
        }

        return false;
    }
    protected virtual void OnKilled() { }
    protected virtual bool TryKill(string? killMessage = null, GameObject2? killer = null) => true;
}
public abstract class PhysicsObject2 : GameObject2
{
    public Vector2 Velocity { get; set; }
    public float Mass { get; set; }
    public float Drag { get; set; }
    public Vector2 ConstAcceleration { get; set; }
    public Vector2 AccumulatedForce { get; private set; } = new(0f);
    
    
    public void ClearAccumulatedForce() => AccumulatedForce = new(0f);
    public void AddForce(Vector2 force)
    {
        if(Mass <= 0) AccumulatedForce += force;
        else AccumulatedForce += force / Mass;
    }
    public void AddImpulse(Vector2 force)
    {
        if (Mass <= 0.0f) Velocity += force;
        else Velocity += force / Mass;
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
    {
        UpdatePhysicsState(time.Delta);
    }

    protected virtual void OnPhysicsStateUpdated() { }
    
    #region Private

    private void UpdatePhysicsState(float dt)
    {
        ApplyAccumulatedForce(dt);
        ApplyAcceleration(dt); 
        Transform = Transform.MoveBy(Velocity * dt);
        OnPhysicsStateUpdated();
    }
    private void ApplyAccumulatedForce(float dt)
    {
        Velocity += AccumulatedForce * dt;
        ClearAccumulatedForce();
    }
    private void ApplyAcceleration(float dt)
    {
        var force = ConstAcceleration * dt;
        Velocity += force;
        Velocity = ApplyDragForce(Velocity, Drag, dt);
    }


    #endregion
    
    #region Static

    /// <summary>
    /// Apply drag to the given value.
    /// </summary>
    /// <param name="value">The value that is affected by the drag.</param>
    /// <param name="dragCoefficient">The drag coefficient for calculating the drag force. Has to be positive.
    /// 1 / drag coefficient = seconds until stop. DC of 4 means object stops in 0.25s.</param>
    /// <param name="dt">The delta time of the current frame.</param>
    /// <returns></returns>
    public static float ApplyDragForce(float value, float dragCoefficient, float dt)
    {
        if (dragCoefficient <= 0f) return value;
        float dragForce = dragCoefficient * value * dt;

        return value - MathF.Min(dragForce, value);
    }
    /// <summary>
    /// Apply drag to the given velocity.
    /// </summary>
    /// <param name="vel">The velocity that is affected by the drag.</param>
    /// <param name="dragCoefficient">The drag coefficient for calculating the drag force. Has to be positive.
    /// 1 / drag coefficient = seconds until stop. DC of 4 means object stops in 0.25s.</param>
    /// <param name="dt">The delta time of the current frame.</param>
    /// <returns>Returns the new velocity.</returns>
    public static Vector2 ApplyDragForce(Vector2 vel, float dragCoefficient, float dt)
    {
        if (dragCoefficient <= 0f) return vel;
        Vector2 dragForce = dragCoefficient * vel * dt;
        if (dragForce.LengthSquared() >= vel.LengthSquared()) return new Vector2(0f, 0f);
        return vel - dragForce;
    }
    public static float GetDragForce(float value, float dragCoefficient, float dt)
    {
        if (dragCoefficient <= 0f) return value;
        float dragForce = dragCoefficient * value * dt;

        return -MathF.Min(dragForce, value);
    }
    public static Vector2 GetDragForce(Vector2 vel, float dragCoefficient, float dt)
    {
        if (dragCoefficient <= 0f) return vel;
        Vector2 dragForce = dragCoefficient * vel * dt;
        if (dragForce.LengthSquared() >= vel.LengthSquared()) return new Vector2(0f, 0f);
        return -dragForce;
    }

    public static Vector2 Attraction(Vector2 center, Vector2 otherPos, Vector2 otherVel, float r, float strength, float friction)
    {
        Vector2 w = center - otherPos;
        float disSq = w.LengthSquared();
        float f = 1.0f - disSq / (r * r);
        Vector2 force = ShapeVec.Normalize(w) * strength;// * f;
        Vector2 stop = -otherVel * friction * f;
        return force + stop;
    }
    public static Vector2 ElasticCollision1D(Vector2 vel1, float mass1, Vector2 vel2, float mass2)
    {
        float totalMass = mass1 + mass2;
        return vel1 * ((mass1 - mass2) / totalMass) + vel2 * (2f * mass2 / totalMass);
    }
    public static Vector2 ElasticCollision2D(Vector2 p1, Vector2 v1, float m1, Vector2 p2, Vector2 v2, float m2, float R)
    {
        float totalMass = m1 + m2;

        float mf = m2 / m1;
        Vector2 posDif = p2 - p1;
        Vector2 velDif = v2 - v1;

        Vector2 velCM = (m1 * v1 + m2 * v2) / totalMass;

        float a = posDif.Y / posDif.X;
        float dvx2 = -2f * (velDif.X + a * velDif.Y) / ((1 + a * a) * (1 + mf));
        //Vector2 newOtherVel = new(v2.X + dvx2, v2.Y + a * dvx2);
        Vector2 newSelfVel = new(v1.X - mf * dvx2, v1.Y - a * mf * dvx2);

        newSelfVel = (newSelfVel - velCM) * R + velCM;
        //newOtherVel = (newOtherVel - velCM) * R + velCM;

        return newSelfVel;
    }

    #endregion
}

public abstract class CollisionObject2 : PhysicsObject2
{
    public CollisionObject2()
    {
        this.Transform = new();
    }
    public CollisionObject2(Vector2 position)
    {
        this.Transform = new(position);
    }
    public CollisionObject2(Transform2D transform)
    {
        this.Transform = transform;
    }

    private bool enabled = true;

    public bool Enabled
    {
        get => enabled && !IsDead;
        set => enabled = value;
    }

    // public bool AddCollider(Collider col)
    // {
    //     if (Colliders.Add(col))
    //     {
    //         col.SetupTransform(Transform);
    //         col.Parent = this;
    //         return true;
    //     }
    //     return false;
    // }
    //
    // public bool RemoveCollider(Collider col)
    // {
    //     if (Colliders.Remove(col))
    //     {
    //         col.Parent = null;
    //         return true;
    //     }
    //         
    //     return false;
    // }

    public HashSet<Collider> Colliders { get; } = new();
    public bool HasColliders => Colliders.Count > 0;

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo ui)
    {
        var trans = Transform;
        base.Update(time, game, ui);
        foreach (var collider in Colliders)
        {
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
            boundingBox = boundingBox.Union(col.GetBoundingBox());
        }
    
        return boundingBox;
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

}


public interface IGameObject : ISpatial, IUpdateable, IDrawable, IKillable//, IBehaviorReceiver
{

    public bool DrawToGame(Rect gameArea);
    public bool DrawToGameUI(Rect screenArea);
        
    /// <summary>
    /// The area layer the object is stored in. Higher layers are draw on top of lower layers.
    /// </summary>
    public int Layer { get; set; }
    /// <summary>
    /// Is called by the area. Can be used to update the objects position based on the new parallax position.
    /// </summary>
    /// <param name="newParallaxPosition">The new parallax position from the layer the object is in.</param>
    public virtual void UpdateParallaxe(Vector2 newParallaxPosition) { }

    /// <summary>
    /// Check if the object is in a layer.
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public sealed bool IsInLayer(int layer) { return this.Layer == layer; }

    /// <summary>
    /// Is called when gameobject is added to an area.
    /// </summary>
    public void AddedToHandler(GameObjectHandler gameObjectHandler);
    /// <summary>
    /// Is called by the area once a game object is dead.
    /// </summary>
    public void RemovedFromHandler(GameObjectHandler gameObjectHandler);

    /// <summary>
    /// Should this object be checked for leaving the bounds of the area?
    /// </summary>
    /// <returns></returns>
    public bool CheckHandlerBounds();
    /// <summary>
    /// Will be called if the object left the bounds of the area. The BoundingCircle is used for this check.
    /// </summary>
    /// <param name="info">The info about where the object left the bounds.</param>
    public void LeftHandlerBounds(BoundsCollisionInfo info);
        
    ///// <summary>
    ///// Can be used to adjust the follow position of an attached camera.
    ///// </summary>
    ///// <param name="camPos"></param>
    ///// <returns></returns>
    //public Vector2 GetCameraFollowPosition(Vector2 camPos);

    /// <summary>
    /// Should the area add the collidables from this object to the collision system on area entry.
    /// </summary>
    /// <returns></returns>
    public virtual bool HasCollisionBody() { return false; }
    /// <summary>
    /// All the collidables that should be added to the collision system on area entry.
    /// </summary>
    /// <returns></returns>
    public virtual CollisionBody? GetCollisionBody() { return null; }

}