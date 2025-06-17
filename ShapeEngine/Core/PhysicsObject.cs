using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

public abstract class PhysicsObject : GameObject
{
    public float CurVelocityMagnitudeSquared {get; private set; } = 0f;
    public float CurVelocityMagnitude { get; private set; } = 0f;
    public Vector2 CurVelocityDirection { get; private set; } = Vector2.Zero;
    public Vector2 Velocity { get; set; }
    public float Mass { get; set; }
  
    /// <summary>
    /// The Value Range is  0-1.
    /// 0 means no drag, 1 means full drag.
    /// Drag determines how much energy the velocity loses per second.
    /// Drag of 0.5f would mean the velocity loses half of its energy per second.
    /// </summary>
    public float DragCoefficient { get; set; }
    public Vector2 ConstAcceleration { get; set; }
    public Vector2 AccumulatedForce { get; private set; } = new(0f);
    public Vector2 AccumulatedImpulses { get; private set; } = new(0f); 

    public float Momentum => Mass * CurVelocityMagnitude;
    public float KineticEnergy => Mass * CurVelocityMagnitudeSquared * 0.5f;
    
    public bool IsInMotion => Velocity.LengthSquared() > 0.00000001f;
    public void ClearAccumulatedForce() => AccumulatedForce = new(0f);
    public void ClearAccumulatedImpulses() => AccumulatedImpulses = new(0f);
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
        if (Mass <= 0.0f) AccumulatedImpulses += force;
        else AccumulatedImpulses += force / Mass;
    }

    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        UpdatePhysicsState(time.Delta);
    }

    protected virtual void OnPhysicsStateUpdated(float dt) { }
    
    #region Private

    private void UpdatePhysicsState(float dt)
    {
        ApplyForces(dt); 
        Transform = Transform.ChangePosition(Velocity * dt);
        OnPhysicsStateUpdated(dt);
    }
    private void ApplyForces(float dt)
    {
        var force = ConstAcceleration + AccumulatedForce;
        
        Velocity += AccumulatedImpulses;
        Velocity += force * dt;
        var dragForce = ShapePhysics.CalculateDragForce(Velocity, DragCoefficient, dt);
        Velocity += dragForce;
        
        ClearAccumulatedImpulses();
        ClearAccumulatedForce();
        
        CurVelocityMagnitudeSquared = Velocity.LengthSquared();
        CurVelocityMagnitude = MathF.Sqrt(CurVelocityMagnitudeSquared);
        if(CurVelocityMagnitudeSquared <= 0f) CurVelocityDirection = Vector2.Zero;
        else CurVelocityDirection = Velocity / CurVelocityMagnitude;
    }


    #endregion
    
}