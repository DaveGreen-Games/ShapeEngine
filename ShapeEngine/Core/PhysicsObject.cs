using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core;

public abstract class PhysicsObject : GameObject
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

    protected virtual void OnPhysicsStateUpdated(float dt) { }
    
    #region Private

    private void UpdatePhysicsState(float dt)
    {
        ApplyAccumulatedForce(dt);
        ApplyAcceleration(dt); 
        Transform = Transform.MoveBy(Velocity * dt);
        OnPhysicsStateUpdated(dt);
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