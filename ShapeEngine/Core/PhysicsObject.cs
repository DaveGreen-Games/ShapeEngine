using System.Numerics;
using ShapeEngine.Core.GameDef;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

/// <summary>
/// Represents a game object with physics properties and simulation logic.
/// Inherits from <see cref="GameObject"/> and adds velocity, mass, drag, and force/impulse accumulation.
/// </summary>
/// <remarks>
/// PhysicsObject provides a simple physics simulation model for 2D games, including drag, momentum, and energy calculations.
/// </remarks>
public abstract class PhysicsObject : GameObject
{
    /// <summary>
    /// Gets the squared magnitude of the current velocity vector.
    /// </summary>
    public float CurVelocityMagnitudeSquared {get; private set; }
    /// <summary>
    /// Gets the magnitude (length) of the current velocity vector.
    /// </summary>
    public float CurVelocityMagnitude { get; private set; }
    /// <summary>
    /// Gets the normalized direction of the current velocity vector.
    /// </summary>
    public Vector2 CurVelocityDirection { get; private set; } = Vector2.Zero;
    /// <summary>
    /// Gets or sets the current velocity vector.
    /// </summary>
    public Vector2 Velocity { get; set; }
    /// <summary>
    /// Gets or sets the mass of the object. Used for force/impulse calculations.
    /// </summary>
    public float Mass { get; set; }
    /// <summary>
    /// Gets or sets the drag coefficient <c>(0-1)</c>.
    /// <list type="bullet">
    /// <item>0 = no drag</item>
    /// <item>1 = full drag</item>
    /// </list>
    /// </summary>
    /// <remarks>Determines the percentage of velocity lost per second.</remarks>
    public float DragCoefficient { get; set; }
    /// <summary>
    /// Gets or sets the constant acceleration applied every update (e.g., gravity).
    /// </summary>
    public Vector2 ConstAcceleration { get; set; }
    /// <summary>
    /// Gets the accumulated force to be applied on the next update.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="AddForce"/> or <see cref="AddForceRaw"/> changes <see cref="AccumulatedForce"/>.
    /// Use this to add constant forces each frame.
    /// <see cref="AccumulatedForce"/> will be added to <see cref="Velocity"/> with delta time applied in the physics step.
    /// </remarks>
    public Vector2 AccumulatedForce { get; private set; } = new(0f);
    /// <summary>
    /// Gets the accumulated impulses to be applied on the next update.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="AddImpulse"/> changes <see cref="AccumulatedImpulses"/>.
    /// <see cref="AccumulatedImpulses"/> are added as is to the <see cref="Velocity"/> in the physics step.
    /// Use this for one-time impulses, not for constant forces!.
    /// </remarks>
    public Vector2 AccumulatedImpulses { get; private set; } = new(0f); 

    /// <summary>
    /// Gets the current momentum <c>(mass * velocity magnitude)</c>.
    /// </summary>
    public float Momentum => Mass * CurVelocityMagnitude;
    /// <summary>
    /// Gets the current kinetic energy <c>(0.5 * mass * velocity^2)</c>.
    /// </summary>
    public float KineticEnergy => Mass * CurVelocityMagnitudeSquared * 0.5f;
    /// <summary>
    /// Gets whether the object is currently in motion <c>(velocity is nonzero)</c>.
    /// </summary>
    public bool IsInMotion => Velocity.LengthSquared() > 0.00000001f;

    /// <summary>
    /// Clears the accumulated force for the next update.
    /// </summary>
    public void ClearAccumulatedForce() => AccumulatedForce = new(0f);
    /// <summary>
    /// Clears the accumulated impulses for the next update.
    /// </summary>
    public void ClearAccumulatedImpulses() => AccumulatedImpulses = new(0f);
    /// <summary>
    /// Adds a force to be applied on the next update.
    /// </summary>
    /// <param name="force">The force vector to add.
    /// If mass is positive, divides <paramref name="force"/> by mass.</param>
    /// <remarks>Uses this to add constant forces each frame.
    /// <see cref="AccumulatedForce"/> will be added to <see cref="Velocity"/> with delta time applied in the physics step.</remarks>
    public void AddForce(Vector2 force)
    {
        if(Mass <= 0) AccumulatedForce += force;
        else AccumulatedForce += force / Mass;
    }
    /// <summary>
    /// Adds a force to be applied on the next update, without dividing by mass.
    /// </summary>
    /// <param name="force">The force vector to add.</param>
    /// <remarks>Use this to add constant forces each frame.
    /// <see cref="AccumulatedForce"/> will be added to <see cref="Velocity"/> with delta time applied in the physics step.</remarks>
    public void AddForceRaw(Vector2 force)
    {
        AccumulatedForce += force;
    }
    /// <summary>
    /// Adds an impulse to be applied on the next update. If mass is positive, divides by mass.
    /// </summary>
    /// <param name="force">The impulse vector to add.</param>
    /// <remarks>
    /// <see cref="AccumulatedImpulses"/> are added as is to the <see cref="Velocity"/> in the physics step.
    /// Use this for one-time impulses, not for constant forces!.
    /// </remarks>
    public void AddImpulse(Vector2 force)
    {
        if (Mass <= 0.0f) AccumulatedImpulses += force;
        else AccumulatedImpulses += force / Mass;
    }

    /// <summary>
    /// Updates the physics state of the object. Called every frame.
    /// </summary>
    /// <param name="time">The current game time.</param>
    /// <param name="game">Game screen info.</param>
    /// <param name="gameUi">Game UI screen info.</param>
    /// <param name="ui">UI screen info.</param>
    public override void Update(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        if (!time.FixedMode)
        {
            UpdatePhysicsState(time.Delta);
        }
    }

    /// <summary>
    /// Called on a fixed timestep to perform deterministic physics updates.
    /// This method is invoked by the engine's fixed-update loop and should be used
    /// for simulation steps that require a constant delta time (e.g., physics).
    /// </summary>
    /// <param name="fixedTime">The fixed game time containing the delta for this physics step.</param>
    /// <param name="game">Primary game screen information.</param>
    /// <param name="gameUi">Game UI screen information.</param>
    /// <param name="ui">Global UI screen information.</param>
    public override void FixedUpdate(GameTime fixedTime, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        UpdatePhysicsState(fixedTime.Delta);
    }

    /// <summary>
    /// Called after the physics state is updated. Override for custom logic.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
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