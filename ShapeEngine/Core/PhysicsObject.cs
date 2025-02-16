using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

public abstract class PhysicsObject : GameObject
{
    public Vector2 Velocity { get; set; }
    public float Mass { get; set; }
  
    /// <summary>
    /// The Value Range is  0-1.
    /// 0 means no drag, 1 means full drag.
    /// Drag determines how much energy the velocity loses per second.
    /// Drag of 0.5f would mean the velocity loses half of its energy per second.
    /// </summary>
    public float Drag { get; set; }
    
    /// <summary>
    /// Scales the applied friction force.
    /// 0 or negative values mean no friction.
    /// The friction system is simplified and not realistic.
    /// It is similar to drag, but it does not scale with the magnitude of velocity.
    /// If the velocity is zero, no friction is applied.
    /// </summary>
    public float FrictionCoefficient { get; set; } = 0f;
    /// <summary>
    /// Determines if and how much friction force is applied.
    /// A velocity opposite of the FrictionNormal results in maximum friction force, and a velocity in the direction of the frictionNormal results in no friction force.
    /// If the velocity is zero, no friction is applied.
    /// </summary>
    
    public Vector2 FrictionNormal { get; set; } = Vector2.Zero;
    public Vector2 ConstAcceleration { get; set; }
    public Vector2 AccumulatedForce { get; private set; } = new(0f);

    public Vector2 Momentum => Mass * ConstAcceleration;
    public Vector2 KineticEnergy => Mass * Velocity;
    
    public void ClearAccumulatedForce() => AccumulatedForce = new(0f);
    public void AddForce(Vector2 force)
    {
        if(Mass <= 0) AccumulatedForce += force;
        else AccumulatedForce += force / Mass;
    }
    /// <summary>
    /// Add a force without dividing by mass
    /// </summary>
    /// <param name="force"></param>
    public void AddForceRaw(Vector2 force)
    {
        AccumulatedForce += force;
    }
    public void AddImpulse(Vector2 force)
    {
        if (Mass <= 0.0f) Velocity += force;
        else Velocity += force / Mass;
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        UpdatePhysicsState(time.Delta);
    }

    protected virtual void OnPhysicsStateUpdated(float dt) { }
    
    #region Private

    private void UpdatePhysicsState(float dt)
    {
        ApplyAccumulatedForce(dt);
        ApplyAcceleration(dt); 
        Transform = Transform.ChangePosition(Velocity * dt);
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
        Velocity = ShapePhysics.ApplyDragFactor(Velocity, Drag, dt);

        if (FrictionCoefficient > 0 && FrictionNormal != Vector2.Zero)
        {
            //TODO: test first
            var frictionForce = ShapePhysics.CalculateFrictionForce(Velocity, Mass, FrictionCoefficient, FrictionNormal);
            Velocity += frictionForce * dt;
        }
    }


    #endregion
    
}