using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

public abstract class PhysicsObject : GameObject
{
    public float CurVelocityMagnitudeSquared {get; private set; } = 0f;
    public float CurVelocityMagnitude { get; private set; } = 0f;
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
    }


    #endregion
    
}

/*
    //  if (AppliesKineticFriction)
    // {
    //     var kineticFrictionForce = ShapePhysics.CalculateKineticFrictionForce(Velocity, FrictionNormal, KineticFrictionCoefficient);
    //     Velocity += kineticFrictionForce * dt;
    // }
    // public bool AppliesStaticFriction => !FrictionNormal.IsSimilar(0f, 0.00000001f) && StaticFrictionCoefficient > 0;
    // public bool AppliesKineticFriction => !FrictionNormal.IsSimilar(0f, 0.00000001f) && KineticFrictionCoefficient > 0;
    // /// <summary>
    // /// Takes effect when an object is not in motion and works against acceleration.
    // /// 0 or negative values mean no friction.
    // /// The friction system is simplified and not realistic.
    // /// It is similar to drag, but it does not scale with the magnitude of velocity.
    // /// If the velocity is zero, no friction is applied.
    // /// </summary>
    // public float StaticFrictionCoefficient { get; set; } = 0f;
    /// <summary>
    /// 0 or negative values mean no friction.
    /// The friction system is simplified and not realistic.
    /// It is similar to drag, but it does not scale with the magnitude of velocity.
    /// If the velocity is zero, no friction is applied.
    /// </summary>
    public float KineticFrictionCoefficient { get; set; } = 0f;
    /// <summary>
    /// Determines the friction force applied to the object.
    /// A FrictionNormal that is zero applies no friction.
    /// </summary>
    public Vector2 FrictionNormal { get; set; } = Vector2.Zero;
    */