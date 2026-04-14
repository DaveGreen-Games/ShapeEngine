using System.Numerics;
using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core;

/// <summary>
/// Represents a game object with simple gameplay-oriented 2D physics state and integration.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PhysicsObject"/> extends <see cref="GameObject"/> with velocity, mass, drag, constant acceleration,
/// and per-frame accumulated force, acceleration, and impulse inputs.
/// </para>
/// <para>
/// The simulation step is intentionally simple and game-friendly:
/// accumulated impulses are applied directly to velocity,
/// accumulated force inputs are converted to acceleration,
/// accumulated acceleration inputs are applied directly,
/// drag is applied,
/// and then position is advanced using the resulting velocity.
/// </para>
/// <para>
/// A mass less than or equal to zero is treated as <c>1</c> for force and impulse conversion.
/// </para>
/// <para>
/// In this class, <b>force</b> is mass-dependent input that is converted to acceleration during the physics step,
/// <b>acceleration</b> is mass-independent input that is applied directly to velocity over time,
/// and <b>impulse</b> is an immediate velocity change that is applied before acceleration and drag for the current step.
/// Use <see cref="AddForce(Vector2)"/> for force-like input,
/// <see cref="AddAcceleration(Vector2)"/> for direct acceleration input,
/// and <see cref="AddImpulse(Vector2)"/> for one-shot impacts, jumps, knockback, or collision responses.
/// </para>
/// </remarks>
public abstract class PhysicsObject : GameObject
{
    #region Private Fields
    
    private const float MotionEpsilon = 0.0000001f;
    
    private Vector2 velocity = Vector2.Zero;
    
    #endregion

    #region Public Properties
    
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
    public Vector2 Velocity
    {
        get => velocity;
        set => SetVelocityState(value);
    }
    
    /// <summary>
    /// Gets or sets the mass of the object. Used for force and impulse conversion.
    /// </summary>
    /// <remarks>
    /// A mass less than or equal to zero is treated as <c>1</c> internally.
    /// </remarks>
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
    /// Gets the accumulated force input to be applied on the next update.
    /// </summary>
    /// <remarks>
    /// This member stores force-like input in engine units.
    /// It is converted to acceleration during the physics step by dividing it by the effective mass.
    /// Calling <see cref="AddForce(Vector2)"/> changes <see cref="AccumulatedForce"/>.
    /// </remarks>
    public Vector2 AccumulatedForce { get; private set; } = Vector2.Zero;

    /// <summary>
    /// Gets the accumulated acceleration contribution to be applied on the next update.
    /// </summary>
    /// <remarks>
    /// This member stores acceleration input directly, without any mass conversion.
    /// Calling <see cref="AddAcceleration(Vector2)"/> changes <see cref="AccumulatedAcceleration"/>.
    /// </remarks>
    public Vector2 AccumulatedAcceleration { get; private set; } = Vector2.Zero;
    
    /// <summary>
    /// Gets the accumulated impulses to be applied on the next update.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="AddImpulse(Vector2)"/> changes <see cref="AccumulatedImpulses"/>.
    /// <see cref="AccumulatedImpulses"/> stores the velocity delta that will be added during the next physics step.
    /// Use this for one-time impulses, not for constant acceleration.
    /// </remarks>
    public Vector2 AccumulatedImpulses { get; private set; } = Vector2.Zero; 

    #endregion
    
    #region Public Getters
    
    /// <summary>
    /// Gets the current momentum <c>(effective mass * velocity magnitude)</c>.
    /// </summary>
    public float Momentum => GetEffectiveMass() * CurVelocityMagnitude;
  
    /// <summary>
    /// Gets the current kinetic energy <c>(0.5 * effective mass * velocity^2)</c>.
    /// </summary>
    public float KineticEnergy => GetEffectiveMass() * CurVelocityMagnitudeSquared * 0.5f;
    
    /// <summary>
    /// Gets whether the object is currently in motion <c>(velocity is nonzero)</c>.
    /// </summary>
    public bool IsInMotion => velocity.LengthSquared() > MotionEpsilon;

    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Clears the accumulated force input for the next update.
    /// </summary>
    public void ClearAccumulatedForce() => AccumulatedForce = Vector2.Zero;

    /// <summary>
    /// Clears the accumulated acceleration contribution for the next update.
    /// </summary>
    public void ClearAccumulatedAcceleration() => AccumulatedAcceleration = Vector2.Zero;
    
    /// <summary>
    /// Clears the accumulated impulses for the next update.
    /// </summary>
    public void ClearAccumulatedImpulses() => AccumulatedImpulses = Vector2.Zero;
    
    /// <summary>
    /// Adds a force-like input to be applied on the next update.
    /// </summary>
    /// <param name="force">The force vector to add. It is converted to acceleration by dividing by the effective mass.</param>
    /// <remarks>
    /// A mass less than or equal to zero is treated as <c>1</c>.
    /// </remarks>
    public void AddForce(Vector2 force)
    {
        AccumulatedForce += force;
    }
    
    /// <summary>
    /// Adds an acceleration contribution to be applied on the next update.
    /// </summary>
    /// <param name="acceleration">The acceleration vector to add.</param>
    public void AddAcceleration(Vector2 acceleration)
    {
        AccumulatedAcceleration += acceleration;
    }
  
    /// <summary>
    /// Adds an impulse to be applied on the next update.
    /// </summary>
    /// <param name="force">The impulse vector to add. It is converted to a velocity delta by dividing by the effective mass.</param>
    /// <remarks>
    /// A mass less than or equal to zero is treated as <c>1</c>.
    /// Use this for one-time impulses, not for constant acceleration.
    /// </remarks>
    public void AddImpulse(Vector2 force)
    {
        AccumulatedImpulses += force / GetEffectiveMass();
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
        UpdatePhysicsState(time.Delta);
    }

    /// <summary>
    /// Called after the physics state is updated. Override for custom logic.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    protected virtual void OnPhysicsStateUpdated(float dt) { }
    
    #endregion
    
    #region Private Methods
    
    private void UpdatePhysicsState(float dt)
    {
        if(dt <= 0) return;
        
        ApplyForces(dt); 
        Transform = Transform.ChangePosition(Velocity * dt);
        OnPhysicsStateUpdated(dt);
    }
    
    private void ApplyForces(float dt)
    {
        if(dt <= 0) return;
        
        var acceleration = ConstAcceleration + AccumulatedAcceleration + (AccumulatedForce / GetEffectiveMass());

        var newVelocity = velocity;
        newVelocity += AccumulatedImpulses;
        newVelocity += acceleration * dt;
        newVelocity += ShapePhysics.CalculateDragForce(newVelocity, DragCoefficient, dt);
        Velocity = newVelocity;
        
        ClearAccumulatedImpulses();
        ClearAccumulatedForce();
        ClearAccumulatedAcceleration();
    }
    
    private float GetEffectiveMass() => Mass > 0f ? Mass : 1f;

    private void SetVelocityState(Vector2 newVelocity)
    {
        velocity = newVelocity;
        CurVelocityMagnitudeSquared = velocity.LengthSquared();
        CurVelocityMagnitude = MathF.Sqrt(CurVelocityMagnitudeSquared);
        CurVelocityDirection = CurVelocityMagnitudeSquared <= MotionEpsilon
            ? Vector2.Zero
            : velocity / CurVelocityMagnitude;
    }
    
    #endregion
}