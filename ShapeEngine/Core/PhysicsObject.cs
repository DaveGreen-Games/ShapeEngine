using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

public abstract class PhysicsObject : GameObject
{
    public Vector2 Velocity { get; set; }
    public float Mass { get; set; }
    public float Drag { get; set; }
    public float Friction { get; set; } = 0f;
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
        //TODO: test first
        Velocity = ShapePhysics.ApplyFrictionForce(Velocity, Mass, Friction, FrictionNormal);
    }


    #endregion
    
}