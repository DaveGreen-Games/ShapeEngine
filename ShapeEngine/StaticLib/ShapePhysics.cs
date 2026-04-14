using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib;

/// <summary>
/// Provides gameplay-oriented and physics-inspired 2D motion helpers.
/// </summary>
/// <remarks>
/// <para>
/// This type contains two categories of helpers:
/// </para>
/// <list type="bullet">
/// <item>
/// <description><b>Gameplay / arcade helpers</b> such as frame-rate-independent drag scaling and simplified motion utilities that are convenient in a game engine but are not strict real-world physics.</description>
/// </item>
/// <item>
/// <description><b>Physics-inspired helpers</b> whose formulas are closer to classical mechanics, such as inverse-square attraction or quadratic drag. These still operate in engine units, not necessarily SI units.</description>
/// </item>
/// </list>
/// <para>
/// In this engine, a returned vector is usually meant to be passed to <see cref="PhysicsObject.AddForce(Vector2)"/>, unless the method remarks explicitly say otherwise.
/// <see cref="PhysicsObject.AddForce(Vector2)"/> converts a force-like vector into acceleration by dividing by mass. A mass less than or equal to zero is treated as <c>1</c> by the helpers in this class.
/// </para>
/// <para>
/// Be aware that some older APIs use the word <c>Force</c> for gameplay damping values that are actually applied as a change in velocity rather than a Newtonian force.
/// In particular, <see cref="CalculateDragForce(Vector2,float,float)"/> and its directional overload return the velocity delta produced by drag over a time step, while <see cref="CalculateDragForceRealistic(Vector2,float,float,float)"/> returns a force-like vector based on the drag equation.
/// </para>
/// <para>
/// Methods named <c>Calculate*</c> return values and do not mutate inputs, except where a compatibility overload is explicitly documented otherwise.
/// Methods named <c>Apply*</c> mutate one or more <see cref="PhysicsObject"/> instances by adding forces or changing velocity.
/// </para>
/// </remarks>
public static class ShapePhysics
{
    private const float Epsilon = 0.0000001f;

    /// <summary>
    /// Gravitational constant 6.67430e-11
    /// </summary>
    public static readonly float GReal = 0.0000000000667430f;
 
    /// <summary>
    /// This is the gravitational constant used in all functions. The default value is 1f, essentially making the values that can be used much smaller and therefore more convenient.
    /// If the real value is needed, set it to GReal.
    /// </summary>
    public static float G = 1f;

    #region Private Functions
    
    private static float SanitizeMass(float mass) => mass > 0f ? mass : 1f;

    private static float SanitizeRestitution(float restitution) => ShapeMath.Clamp(restitution, 0f, 1f);

    private static bool TryNormalize(Vector2 v, out Vector2 normalized)
    {
        var lengthSquared = v.LengthSquared();
        if (lengthSquared <= Epsilon)
        {
            normalized = Vector2.Zero;
            return false;
        }

        normalized = v / MathF.Sqrt(lengthSquared);
        return true;
    }

    private static Vector2 GetPerpendicular(Vector2 unitVector) => new(-unitVector.Y, unitVector.X);

    private static bool TryGetCollisionNormal(Vector2 collisionNormal, out Vector2 unitCollisionNormal)
    {
        return TryNormalize(collisionNormal, out unitCollisionNormal);
    }

    private static Vector2 GetCircleCollisionNormal(Vector2 position1, Vector2 velocity1, Vector2 position2, Vector2 velocity2)
    {
        var impactVector = position2 - position1;
        if (TryGetCollisionNormal(impactVector, out var collisionNormal)) return collisionNormal;

        var relativeVelocity = velocity2 - velocity1;
        if (TryGetCollisionNormal(relativeVelocity, out collisionNormal)) return collisionNormal;

        return Vector2.UnitX;
    }

    private static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateElasticCollisionInternal(Vector2 unitCollisionNormal, Vector2 velocity1, float mass1, Vector2 velocity2, float mass2, float restitution)
    {
        var m1 = SanitizeMass(mass1);
        var m2 = SanitizeMass(mass2);
        var r = SanitizeRestitution(restitution);
        var collisionTangent = GetPerpendicular(unitCollisionNormal);

        var normalVelocity1 = Vector2.Dot(unitCollisionNormal, velocity1);
        var tangentialVelocity1 = Vector2.Dot(collisionTangent, velocity1);

        var normalVelocity2 = Vector2.Dot(unitCollisionNormal, velocity2);
        var tangentialVelocity2 = Vector2.Dot(collisionTangent, velocity2);

        var massSum = m1 + m2;
        var newNormalVelocity1 = ((m1 - r * m2) * normalVelocity1 + (1f + r) * m2 * normalVelocity2) / massSum;
        var newNormalVelocity2 = ((1f + r) * m1 * normalVelocity1 + (m2 - r * m1) * normalVelocity2) / massSum;

        var n1 = unitCollisionNormal * newNormalVelocity1;
        var t1 = collisionTangent * tangentialVelocity1;

        var n2 = unitCollisionNormal * newNormalVelocity2;
        var t2 = collisionTangent * tangentialVelocity2;

        return (n1 + t1, n2 + t2);
    }

    private static Vector2 GetTangentComponent(Vector2 velocity, Vector2 unitSurfaceNormal)
    {
        var dot = Vector2.Dot(velocity, unitSurfaceNormal);
        return velocity - dot * unitSurfaceNormal;
    }

    private static bool TryGetDirectionalFactor(Vector2 direction, Vector2 referenceNormal, out float factor, bool reverse = false)
    {
        factor = 0f;
        if (!TryNormalize(direction, out var unitDirection)) return false;
        if (!TryNormalize(referenceNormal, out var unitReferenceNormal)) return false;

        factor = reverse
            ? unitDirection.CalculateDotFactorReverse(unitReferenceNormal)
            : unitDirection.CalculateDotFactor(unitReferenceNormal);
        return true;
    }
    
    #endregion
    
    #region Elastic Collision
    
    /// <summary>
    /// Calculates new velocities for a circle-circle collision.
    /// This is a gameplay-friendly elastic collision helper that preserves tangential velocity and applies restitution only to the normal component.
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="velocity1"></param>
    /// <param name="mass1"></param>
    /// <param name="position2"></param>
    /// <param name="velocity2"></param>
    /// <param name="mass2"></param>
    /// <param name="r">The elasticity of the collision. 0 means all energy is lost after collision, 1 means full energy is retained after collision.</param>
    /// <returns></returns>
    /// <remarks>
    /// If both circle centers are identical, the method falls back to relative velocity to derive a collision normal. If that is also zero, a deterministic unit X normal is used.
    /// Masses less than or equal to zero are treated as <c>1</c>. Restitution is clamped to <c>[0, 1]</c>.
    /// </remarks>
    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateElasticCollisionCircles(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2, float r = 1f)
    {
        // Source reference for base derivation: https://www.vobarian.com/collisions/2dcollisions2.pdf
        var collisionNormal = GetCircleCollisionNormal(position1, velocity1, position2, velocity2);
        return CalculateElasticCollisionInternal(collisionNormal, velocity1, mass1, velocity2, mass2, r);
    }
    /// <summary>
    /// Calculates the post-collision velocity of the first circle in a circle-circle collision.
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="velocity1"></param>
    /// <param name="mass1"></param>
    /// <param name="position2"></param>
    /// <param name="velocity2"></param>
    /// <param name="mass2"></param>
    /// <param name="r">The elasticity of the collision. 0 means all energy is lost after collision, 1 means full energy is retained after collision.</param>
    /// <returns></returns>
    /// <remarks>
    /// This is equivalent to <see cref="CalculateElasticCollisionCircles(Vector2,Vector2,float,Vector2,Vector2,float,float)"/> and returning <c>newVelocity1</c>.
    /// </remarks>
    public static Vector2 CalculateElasticCollisionCirclesSelf(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2, float r = 1f)
    {
        var result = CalculateElasticCollisionCircles(position1, velocity1, mass1, position2, velocity2, mass2, r);
        return result.newVelocity1;
    }

    /// <summary>
    /// Calculates new velocities for a collision using the supplied collision normal.
    /// </summary>
    /// <param name="collisionNormal"></param>
    /// <param name="velocity1"></param>
    /// <param name="mass1"></param>
    /// <param name="velocity2"></param>
    /// <param name="mass2"></param>
    /// <param name="r">The elasticity of the collision. 0 means all energy is lost after collision, 1 means full energy is retained after collision.</param>
    /// <returns></returns>
    /// <remarks>
    /// The supplied normal is normalized internally. If the normal is too small to normalize, the original velocities are returned unchanged.
    /// Masses less than or equal to zero are treated as <c>1</c>. Restitution is clamped to <c>[0, 1]</c>.
    /// </remarks>
    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateElasticCollision(Vector2 collisionNormal, Vector2 velocity1, float mass1, Vector2 velocity2, float mass2, float r = 1f)
    {
        if (!TryGetCollisionNormal(collisionNormal, out var unitCollisionNormal)) return (velocity1, velocity2);
        return CalculateElasticCollisionInternal(unitCollisionNormal, velocity1, mass1, velocity2, mass2, r);
    }
    /// <summary>
    /// Calculates the post-collision velocity of the first body for a collision using the supplied collision normal.
    /// </summary>
    /// <param name="collisionNormal"></param>
    /// <param name="velocity1"></param>
    /// <param name="mass1"></param>
    /// <param name="velocity2"></param>
    /// <param name="mass2"></param>
    /// <param name="r">The elasticity of the collision. 0 means all energy is lost after collision, 1 means full energy is retained after collision.</param>
    /// <returns>Returns new velocity 1.</returns>
    public static Vector2 CalculateElasticCollisionSelf(Vector2 collisionNormal, Vector2 velocity1, float mass1, Vector2 velocity2, float mass2, float r = 1f)
    {
        var result = CalculateElasticCollision(collisionNormal, velocity1, mass1, velocity2, mass2, r);
        return result.newVelocity1;
    }
    /// <summary>
    /// Calculates new velocities for both physics objects based on the given collision normal and sets the new velocities of both objects.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <param name="collisionNormal"></param>
    /// <param name="r">The elasticity of the collision. 0 means all energy is lost after collision, 1 means full energy is retained after collision.</param>
    public static void ApplyElasticCollision(this PhysicsObject obj1, PhysicsObject obj2, Vector2 collisionNormal, float r = 1f)
    {
        var result = CalculateElasticCollision(collisionNormal, obj1.Velocity, obj1.Mass, obj2.Velocity, obj2.Mass, r);
        obj1.Velocity = result.newVelocity1;
        obj2.Velocity = result.newVelocity2;
    }
    /// <summary>
    /// Calculates the new velocity for obj1 based on the given collision normal and sets the new velocity of obj1.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <param name="collisionNormal"></param>
    /// <param name="r">The elasticity of the collision. 0 means all energy is lost after collision, 1 means full energy is retained after collision.</param>
    public static void ApplyElasticCollisionSelf(this PhysicsObject obj1, PhysicsObject obj2, Vector2 collisionNormal, float r = 1f)
    {
        var result = CalculateElasticCollisionSelf(collisionNormal, obj1.Velocity, obj1.Mass, obj2.Velocity, obj2.Mass, r);
        obj1.Velocity = result;
    }
    
    /// <summary>
    /// Calculates and applies the new velocity to <paramref name="obj1"/> after an elastic collision with <paramref name="obj2"/>.
    /// Assumes both objects are circles. Only <paramref name="obj1"/>'s velocity is updated.
    /// </summary>
    /// <param name="obj1">The first physics object (velocity will be updated).</param>
    /// <param name="obj2">The second physics object.</param>
    /// <param name="r">
    /// The elasticity of the collision. 0 means all energy is lost after collision, 1 means full energy is retained after collision.
    /// </param>
    public static void ApplyElasticCollisionCircleSelf(this PhysicsObject obj1, PhysicsObject obj2, float r = 1f)
    {
        var result = CalculateElasticCollisionCirclesSelf(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, r);
        obj1.Velocity = result;
    }

    #endregion

    #region Drag

    /// <summary>
    /// This function calculates a frame rate independent factor for applying drag.
    /// </summary>
    /// <param name="dragCoefficient">Drag coefficient between 0 and 1. How much energy the velocity should lose each second.</param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public static float CalculateDragFactor(float dragCoefficient, float deltaTime)
    {
        if (deltaTime <= 0f) return 1f;
        if (dragCoefficient <= 0) return 1;
        if (dragCoefficient >= 1) return 0;
        dragCoefficient = ShapeMath.Clamp(dragCoefficient, 0f, 1f);
        float dragFactor = (float)Math.Pow(1.0 - dragCoefficient, deltaTime);

        return dragFactor;
    }
    
    /// <summary>
    /// Calculates the drag-induced velocity delta to apply to an object over the given time step.
    /// This is a gameplay damping helper, not a Newtonian force calculation.
    /// </summary>
    /// <param name="velocity">The current velocity of the object.</param>
    /// <param name="dragCoefficient">The drag coefficient (between 0 and 1). 0 means no drag, 1 means full stop in one frame.</param>
    /// <param name="deltaTime">The time step over which to apply the drag.</param>
    /// <returns>The velocity delta produced by drag over the time step.</returns>
    /// <remarks>Returns a zero vector if the drag coefficient is less than or equal to 0,
    /// and returns <c>-velocity</c> if the drag coefficient is greater than or equal to 1.</remarks>
    public static Vector2 CalculateDragForce(Vector2 velocity, float dragCoefficient, float deltaTime)
    {
        if (deltaTime <= 0f) return Vector2.Zero;
        if (dragCoefficient <= 0) return Vector2.Zero;
        if (dragCoefficient >= 1) return -velocity;
        var factor = CalculateDragFactor(dragCoefficient, deltaTime);
        return -velocity * (1f - factor);
    }
    
    /// <summary>
    /// Calculates a directional drag-induced velocity delta based on the supplied velocity and drag normal.
    /// This is a gameplay damping helper, not a Newtonian force calculation.
    /// </summary>
    /// <param name="velocity">The velocity of the object.</param>
    /// <param name="dragCoefficient">A value between 0-1.</param>
    /// <param name="dragNormal">Drag is applied against the drag normal.
    /// A velocity pointing in the same direction as the dragNormal does not receive any drag.
    /// A velocity pointing in the opposite direction as the dragNormal does receive max drag force.</param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    /// <remarks>
    /// If <paramref name="dragNormal"/> is too small to normalize, this falls back to the non-directional overload.
    /// </remarks>
    public static Vector2 CalculateDragForce(Vector2 velocity, float dragCoefficient, Vector2 dragNormal, float deltaTime)
    {
        if (deltaTime <= 0f) return Vector2.Zero;
        if (dragCoefficient <= 0) return Vector2.Zero;
        if (!TryGetDirectionalFactor(velocity, dragNormal, out var factor, reverse: true))
        {
            return CalculateDragForce(velocity, dragCoefficient, deltaTime);
        }

        if (dragCoefficient >= 1)
        {
            return -velocity * factor;
        }

        var dragFactor = CalculateDragFactor(dragCoefficient, deltaTime);
        return -velocity * (1f - dragFactor) * factor;
    }

    /// <summary>
    /// Applies frame-rate-independent gameplay drag to the supplied velocity.
    /// </summary>
    /// <param name="velocity">The affected velocity.</param>
    /// <param name="dragCoefficient">Drag coefficient between 0 and 1. How much energy the velocity should lose each second.</param>
    /// <param name="deltaTime"></param>
    /// <returns>Returns the new scaled velocity.</returns>
    public static Vector2 ApplyDragForce(Vector2 velocity, float dragCoefficient, float deltaTime)
    {
        if (deltaTime <= 0f) return velocity;
        if (dragCoefficient <= 0) return velocity;
        if (dragCoefficient >= 1) return Vector2.Zero;
        var factor = CalculateDragFactor(dragCoefficient, deltaTime);
        return velocity * factor;
    }
    
    /// <summary>
    /// Applies frame-rate-independent gameplay drag to the supplied speed.
    /// </summary>
    /// <param name="speed">The affected speed.</param>
    /// <param name="dragCoefficient">Drag coefficient between 0 and 1. How much energy the velocity should lose each second.</param>
    /// <param name="deltaTime"></param>
    /// <returns>Returns the new scaled velocity.</returns>
    public static float ApplyDragForce(float speed, float dragCoefficient, float deltaTime)
    {
        if (deltaTime <= 0f) return speed;
        if (dragCoefficient <= 0) return speed;
        if (dragCoefficient >= 1) return 0f;
        var factor = CalculateDragFactor(dragCoefficient, deltaTime);
        return speed * factor;
    }
    
    /// <summary>
    /// Applies frame-rate-independent gameplay drag to the supplied velocity with directional modulation.
    /// </summary>
    /// <param name="velocity">The affected velocity.</param>
    /// <param name="dragCoefficient">Drag coefficient between 0 and 1. How much energy the velocity should lose each second.</param>
    /// <param name="dragNormal">Drag is applied against the drag normal.
    /// A velocity pointing in the same direction as the dragNormal does not receive any drag.
    /// A velocity pointing in the opposite direction as the dragNormal does receive max drag force.</param>
    /// <param name="deltaTime"></param>
    /// <returns>Returns the new scaled velocity.</returns>
    /// <remarks>
    /// If <paramref name="dragNormal"/> is too small to normalize, this falls back to the non-directional overload.
    /// </remarks>
    public static Vector2 ApplyDragForce(Vector2 velocity, float dragCoefficient, Vector2 dragNormal, float deltaTime)
    {
        if (deltaTime <= 0f) return velocity;
        if (dragCoefficient <= 0) return velocity;
        if (!TryGetDirectionalFactor(velocity, dragNormal, out var factor, reverse: true))
        {
            return ApplyDragForce(velocity, dragCoefficient, deltaTime);
        }

        if (dragCoefficient >= 1)
        {
            return velocity - velocity * factor;
        }

        var dragFactor = CalculateDragFactor(dragCoefficient, deltaTime);
        return velocity - velocity * (1f - dragFactor) * factor;
    }
    
    /// <summary>
    /// Calculates a realistic drag force.
    /// Force = FluidDensity * Speed * Speed * DragCoefficient * ReferenceArea
    /// </summary>
    /// <param name="velocity">The velocity of the object.</param>
    /// <param name="dragCoefficient">The drag coefficient to scale the resulting force.</param>
    /// <param name="referenceArea">The surface area of the object resisting the fluid. </param>
    /// <param name="fluidDensity">The density of the fluid the object is moving through. Air at sea level has a density value of 1.225f</param>
    /// <returns></returns>
    public static Vector2 CalculateDragForceRealistic(Vector2 velocity, float dragCoefficient, float referenceArea, float fluidDensity = 1.225f)
    {
        if (dragCoefficient <= 0f || referenceArea <= 0f || fluidDensity <= 0f) return Vector2.Zero;
        float speedSquared = velocity.LengthSquared();
        if(speedSquared <= Epsilon) return Vector2.Zero;
        
        // Calculate the magnitude of the velocity
        float speed = MathF.Sqrt(speedSquared);
        var velocityDirection = velocity / speed;
        
        // Calculate the drag force magnitude
        float dragForceMagnitude = 0.5f * fluidDensity * speed * speed * dragCoefficient * referenceArea;

        // Calculate the drag force vector (opposite to the direction of velocity)
        var dragForce = -velocityDirection * dragForceMagnitude;

        return dragForce;
    }
    
    #endregion
    
    #region Attraction
    
    /// <summary>
    /// Calculates an inverse-square attraction force between two objects.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <remarks>
    /// This is a gameplay-friendly gravity-style helper in engine units. Masses less than or equal to zero are treated as <c>1</c>.
    /// </remarks>
    /// <returns>Returns the resulting force vectors.</returns>
    public static (Vector2 force1, Vector2 force2) CalculateAttraction(Vector2 position1, float mass1, Vector2 position2, float mass2)
    {
        // Calculate the direction and distance between the two objects
        var direction = position2 - position1;
        var distanceSquared = direction.LengthSquared();
        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return (Vector2.Zero, Vector2.Zero);
        }

        float distance = MathF.Sqrt(distanceSquared);
        
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        var m1 = SanitizeMass(mass1);
        var m2 = SanitizeMass(mass2);

        // Calculate the force magnitude
        float forceMagnitude = G * (m1 * m2) / distanceSquared;

        var force1 = forceMagnitude * normalizedDirection;
        var force2 = forceMagnitude * -normalizedDirection;

        return (force1, force2);
    }
 
    /// <summary>
    /// Applies the gravitational attraction force between two <see cref="PhysicsObject"/> instances.
    /// The force is calculated using their positions and masses, and applied to both objects.
    /// </summary>
    /// <param name="obj1">The first physics object (force will be applied).</param>
    /// <param name="obj2">The second physics object (force will be applied).</param>
    public static void ApplyAttraction(this PhysicsObject obj1, PhysicsObject obj2)
    {
        var result = CalculateAttraction(obj1.Transform.Position, obj1.Mass, obj2.Transform.Position, obj2.Mass);
        obj1.AddForce(result.force1); 
        obj2.AddForce(result.force2);
    }
    
    /// <summary>
    /// Calculate the force for 1 object based on attraction point and force.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <returns>Returns the resulting forces. </returns>
    public static Vector2 CalculateAttraction(Vector2 position, Vector2 attractionPoint, float attractionForce)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = attractionPoint - position;
        var distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return Vector2.Zero;
        }
       
        float distance = MathF.Sqrt(distanceSquared);
        
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = attractionForce / distanceSquared;

        return forceMagnitude * normalizedDirection;
    }
    
    /// <summary>
    /// Applies an attraction force to the specified <paramref name="obj"/> towards a given <paramref name="attractionPoint"/>.
    /// The force magnitude is determined by <paramref name="attractionForce"/> and decreases with the square of the distance.
    /// </summary>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <param name="attractionPoint">The point towards which the object is attracted.</param>
    /// <param name="attractionForce">The strength of the attraction force.</param>
    public static void ApplyAttraction(this PhysicsObject obj, Vector2 attractionPoint, float attractionForce)
    {
        var force = CalculateAttraction(obj.Transform.Position, attractionPoint, attractionForce);
        obj.AddForce(force);
    }
    
    /// <summary>
    /// Calculate the force for 1 object based on attraction point and force.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="position"></param>
    /// <param name="attractionPoint"></param>
    /// <param name="attractionForce"></param>
    /// <param name="attractionNormal">Determines the direction from which the attraction force works.
    /// Pointing in the same direction of attractionNormal will result in max attraction force.</param>
    /// <returns></returns>
    public static Vector2 CalculateAttraction(Vector2 position, Vector2 attractionPoint, float attractionForce, Vector2 attractionNormal)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = attractionPoint - position;
        var distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return Vector2.Zero;
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;
        if (!TryGetDirectionalFactor(normalizedDirection, attractionNormal, out var dotFactor))
        {
            return CalculateAttraction(position, attractionPoint, attractionForce);
        }

        // Calculate the force magnitude
        float forceMagnitude = attractionForce / distanceSquared;

        return forceMagnitude * normalizedDirection * dotFactor;
    }
    
    /// <summary>
    /// Applies an attraction force to the specified <paramref name="obj"/> towards a given <paramref name="attractionPoint"/>.
    /// The force magnitude is determined by <paramref name="attractionForce"/> and decreases with the square of the distance,
    /// modulated by the direction of <paramref name="attractionNormal"/>.
    /// </summary>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <param name="attractionPoint">The point towards which the object is attracted.</param>
    /// <param name="attractionForce">The strength of the attraction force.</param>
    /// <param name="attractionNormal">
    /// Determines the direction from which the attraction force works.
    /// Pointing in the same direction as <paramref name="attractionNormal"/> results in maximum attraction force.
    /// </param>
    public static void ApplyAttraction(this PhysicsObject obj, Vector2 attractionPoint, float attractionForce, Vector2 attractionNormal)
    {
        var force = CalculateAttraction(obj.Transform.Position, attractionPoint, attractionForce, attractionNormal);
        obj.AddForce(force);
    }
    
    /// <summary>
    /// Calculate the force for 1 object based on attraction point and force.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <returns>Returns the resulting forces. </returns>
    public static Vector2 CalculateAttraction(Vector2 position, Vector2 attractionPoint, float attractionForce, float distanceScalePower)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = attractionPoint - position;
        var distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return Vector2.Zero;
        }

        float distance = MathF.Sqrt(distanceSquared);
        
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = attractionForce / MathF.Pow(distance, distanceScalePower);

        return forceMagnitude * normalizedDirection;
    }

    /// <summary>
    /// Applies an attraction force to the specified <paramref name="obj"/> towards a given <paramref name="attractionPoint"/>.
    /// The force magnitude is determined by <paramref name="attractionForce"/> and decreases with distance raised to the power of <paramref name="distanceScalePower"/>.
    /// </summary>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <param name="attractionPoint">The point towards which the object is attracted.</param>
    /// <param name="attractionForce">The strength of the attraction force.</param>
    /// <param name="distanceScalePower">
    /// The exponent for distance scaling. A value of 2 means the force decreases with the square of the distance.
    /// </param>
    public static void ApplyAttraction(this PhysicsObject obj, Vector2 attractionPoint, float attractionForce, float distanceScalePower)
    {
       var force = CalculateAttraction(obj.Transform.Position, attractionPoint, attractionForce, distanceScalePower);
       obj.AddForce(force);
    }
    
    #endregion
    
    #region Repulsion

    /// <summary>
    /// Calculates an inverse-square repulsion force between two objects.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <remarks>
    /// This is the non-mutating repulsion counterpart to <see cref="ApplyRepulsion(PhysicsObject,PhysicsObject)"/>.
    /// Masses less than or equal to zero are treated as <c>1</c>.
    /// </remarks>
    /// <returns>Returns the resulting force vectors.</returns>
    public static (Vector2 force1, Vector2 force2) CalculateRepulsion(Vector2 position1, float mass1, Vector2 position2, float mass2)
    {
        // Calculate the direction and distance between the two objects
        var direction = position2 - position1;
        var distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return (Vector2.Zero, Vector2.Zero);
        }
        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        var m1 = SanitizeMass(mass1);
        var m2 = SanitizeMass(mass2);

        // Calculate the force magnitude
        float forceMagnitude = G * (m1 * m2) / distanceSquared;

        var force1 = forceMagnitude * -normalizedDirection;
        var force2 = forceMagnitude * normalizedDirection;

        return (force1, force2);
    }
  
    /// <summary>
    /// Applies a repulsion force between two <see cref="PhysicsObject"/> instances.
    /// The force is calculated using their positions and masses,
    /// and applied to both objects.
    /// </summary>
    /// <param name="obj1">The first physics object (force will be applied).</param>
    /// <param name="obj2">The second physics object (force will be applied).</param>
    public static void ApplyRepulsion(PhysicsObject obj1, PhysicsObject obj2)
    {
       var result = CalculateRepulsion(obj1.Transform.Position, obj1.Mass, obj2.Transform.Position, obj2.Mass);
       obj1.AddForce(result.force1);
       obj2.AddForce(result.force2);
    }
    
    /// <summary>
    /// Calculates a repulsion force for one object based on a repulsion point and force magnitude.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <returns>Returns the resulting force vector.</returns>
    public static Vector2 CalculateRepulsion(Vector2 position, Vector2 repulsionPoint, float repulsionForce)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = position - repulsionPoint;
        var distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return Vector2.Zero;
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = repulsionForce / distanceSquared;

        return forceMagnitude * normalizedDirection;
    }
    
    /// <summary>
    /// Applies a repulsion force to the specified <paramref name="obj"/> away from a given <paramref name="repulsionPoint"/>.
    /// The force magnitude is determined by <paramref name="repulsionForce"/> and decreases with the square of the distance.
    /// </summary>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <param name="repulsionPoint">The point from which the object is repelled.</param>
    /// <param name="repulsionForce">The strength of the repulsion force.</param>
    public static void ApplyRepulsion(this PhysicsObject obj, Vector2 repulsionPoint, float repulsionForce)
    {
        var force = CalculateRepulsion(obj.Transform.Position, repulsionPoint, repulsionForce);
        obj.AddForce(force);
    }

    /// <summary>
    /// Calculates a directionally modulated repulsion force for one object based on a repulsion point and force magnitude.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="repulsionNormal">Determines the direction from which the attraction force works.
    /// Pointing in the same direction of attractionNormal will result in max attraction force.</param>
    /// <param name="position"></param>
    /// <param name="repulsionPoint"></param>
    /// <param name="repulsionForce"></param>
    /// <returns>Returns the resulting force vector.</returns>
    public static Vector2 CalculateRepulsion(Vector2 position, Vector2 repulsionPoint, float repulsionForce, Vector2 repulsionNormal)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = position - repulsionPoint;
        var distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return Vector2.Zero;
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;
        if (!TryGetDirectionalFactor(normalizedDirection, repulsionNormal, out var dotFactor))
        {
            return CalculateRepulsion(position, repulsionPoint, repulsionForce);
        }
        // Calculate the force magnitude
        float forceMagnitude = repulsionForce / distanceSquared;

        return forceMagnitude * normalizedDirection * dotFactor;
    }
 
    /// <summary>
    /// Applies a repulsion force to the specified <paramref name="obj"/> away from a given <paramref name="repulsionPoint"/>,
    /// modulated by the direction of <paramref name="repulsionNormal"/>.
    /// The force magnitude is determined by <paramref name="repulsionForce"/> and decreases with the square of the distance.
    /// </summary>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <param name="repulsionPoint">The point from which the object is repelled.</param>
    /// <param name="repulsionForce">The strength of the repulsion force.</param>
    /// <param name="repulsionNormal">
    /// Determines the direction from which the repulsion force works.
    /// Pointing in the same direction as <paramref name="repulsionNormal"/> results in maximum repulsion force.
    /// </param>
    public static void ApplyRepulsion(this PhysicsObject obj, Vector2 repulsionPoint, float repulsionForce, Vector2 repulsionNormal)
    {
        var force = CalculateRepulsion(obj.Transform.Position, repulsionPoint, repulsionForce, repulsionNormal);
        obj.AddForce(force);
    }
    
    /// <summary>
    /// Calculates a repulsion force for one object using a custom distance falloff power.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <returns>Returns the resulting force vector.</returns>
    public static Vector2 CalculateRepulsion(Vector2 position, Vector2 repulsionPoint, float repulsionForce, float distanceScalePower)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = position - repulsionPoint;
        var distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return Vector2.Zero;
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = repulsionForce / MathF.Pow(distance, distanceScalePower);

        return forceMagnitude * normalizedDirection;
    }

    /// <summary>
    /// Applies a repulsion force to the specified <paramref name="obj"/> away from a given <paramref name="repulsionPoint"/>.
    /// The force magnitude is determined by <paramref name="repulsionForce"/> and decreases with distance raised to the power of <paramref name="distanceScalePower"/>.
    /// </summary>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <param name="repulsionPoint">The point from which the object is repelled.</param>
    /// <param name="repulsionForce">The strength of the repulsion force.</param>
    /// <param name="distanceScalePower">
    /// The exponent for distance scaling. A value of 2 means the force decreases with the square of the distance.
    /// </param>
    public static void ApplyRepulsion(this PhysicsObject obj, Vector2 repulsionPoint, float repulsionForce, float distanceScalePower)
    {
        var force = CalculateRepulsion(obj.Transform.Position, repulsionPoint, repulsionForce, distanceScalePower);
        obj.AddForce(force);
    }

    #endregion
    
    #region Friction

    /// <summary>
    /// Calculates the opposite tangent component of the velocity relative to the surface normal.
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="surfaceNormal"></param>
    /// <returns></returns>
    public static Vector2 CalculateFrictionTangent(Vector2 velocity, Vector2 surfaceNormal)
    {
        if (!TryNormalize(surfaceNormal, out var unitSurfaceNormal)) return Vector2.Zero;
        var tangent = GetTangentComponent(velocity, unitSurfaceNormal);
        return -tangent;
    }
    
    /// <summary>
    /// Calculates a gameplay-style friction force along the surface tangent.
    /// This is not a full physical friction model; it simply removes velocity parallel to the surface.
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="surfaceNormal"></param>
    /// <param name="frictionForce"></param>
    /// <returns></returns>
    public static Vector2 CalculateFrictionForce(Vector2 velocity, Vector2 surfaceNormal, float frictionForce)
    {
        if(frictionForce <= 0f) return Vector2.Zero;
        if(velocity.LengthSquared() <= Epsilon) return Vector2.Zero;

        if (!TryNormalize(surfaceNormal, out var unitSurfaceNormal)) return Vector2.Zero;
        var tangent = GetTangentComponent(velocity, unitSurfaceNormal);
        if(tangent.LengthSquared() <= Epsilon) return Vector2.Zero;
        return -tangent * frictionForce;
    }
    
    /// <summary>
    /// Calculates a friction force along the surface tangent using the tangent magnitude directly.
    /// This more closely matches the structure of a velocity-proportional damping term, but it is still an engine-space approximation rather than a full Coulomb friction model.
    /// </summary>
    /// <param name="velocity"></param>
    /// <param name="surfaceNormal"></param>
    /// <param name="frictionForce"></param>
    /// <returns></returns>
    public static Vector2 CalculateFrictionForceRealistic(Vector2 velocity, Vector2 surfaceNormal, float frictionForce)
    {
        if (frictionForce <= 0f) return Vector2.Zero;
        if (!TryNormalize(surfaceNormal, out var unitSurfaceNormal)) return Vector2.Zero;
        var tangent = GetTangentComponent(velocity, unitSurfaceNormal);
        if(tangent.IsSimilar(0f, 0.0000001f)) return Vector2.Zero;
        
        return -tangent * frictionForce;

    }
    
    /// <summary>
    /// Applies a gameplay-style friction force to the specified <paramref name="obj"/> based on its velocity and the given surface normal.
    /// The friction force acts opposite to the tangent component of motion and is scaled by the provided <paramref name="frictionForce"/>.
    /// The force is only applied if the object is moving and the tangent component of the velocity is non-zero.
    /// </summary>
    /// <param name="obj">The physics object to apply the friction force to.</param>
    /// <param name="surfaceNormal">The normal vector of the surface the object is in contact with.</param>
    /// <param name="frictionForce">The coefficient or magnitude of the friction force to apply.</param>
    /// <returns>Returns true if a friction force was applied; otherwise, false.</returns>
    public static bool ApplyFrictionForce(PhysicsObject obj, Vector2 surfaceNormal, float frictionForce)
    {
        if(frictionForce <= 0f) return false;
        if (obj.Velocity.LengthSquared() <= Epsilon) return false;

        if (!TryNormalize(surfaceNormal, out var unitSurfaceNormal)) return false;
        var tangent = GetTangentComponent(obj.Velocity, unitSurfaceNormal);
        if (tangent.LengthSquared() <= Epsilon) return false;
        var force = -tangent * frictionForce;
        force *= SanitizeMass(obj.Mass);
        obj.AddForce(force);
        return true;
    }
    
    /// <summary>
    /// Applies a physics-inspired tangent damping force to the specified <paramref name="obj"/> based on its velocity and the given <paramref name="surfaceNormal"/>.
    /// The friction force acts opposite to the tangent component of the velocity and is scaled by <paramref name="frictionForce"/>.
    /// The force is only applied if the tangent component of the velocity is non-zero.
    /// </summary>
    /// <param name="obj">The physics object to apply the friction force to.</param>
    /// <param name="surfaceNormal">The normal vector of the surface the object is in contact with.</param>
    /// <param name="frictionForce">The coefficient or magnitude of the friction force to apply.</param>
    /// <returns>Returns true if a friction force was applied; otherwise, false.</returns>
    public static bool ApplyFrictionForceRealistic(PhysicsObject obj, Vector2 surfaceNormal, float frictionForce)
    {
        if (frictionForce <= 0f) return false;
        if (!TryNormalize(surfaceNormal, out var unitSurfaceNormal)) return false;
        var tangent = GetTangentComponent(obj.Velocity, unitSurfaceNormal);
        if (tangent.IsSimilar(0f, 0.0000001f)) return false;
        
        var force = -tangent * frictionForce;
        force *= SanitizeMass(obj.Mass);
        obj.AddForce(force);
        return true;
    
    }
    
    #endregion

    #region Reverse Attraction

    /// <summary>
    /// Calculates a reverse-attraction force between two objects that scales with distance squared.
    /// This is a physics-inspired gameplay helper, not a real-world gravity formula.
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="mass1"></param>
    /// <param name="position2"></param>
    /// <param name="mass2"></param>
    /// <remarks>
    /// The returned vectors are force-like engine-space values intended for <see cref="PhysicsObject.AddForce(Vector2)"/>.
    /// Masses less than or equal to zero are treated as <c>1</c>.
    /// </remarks>
    /// <returns>Returns the resulting force vectors.</returns>
    public static (Vector2 force1, Vector2 force2) CalculateReverseAttractionForceRealistic(Vector2 position1, float mass1, Vector2 position2, float mass2)
    {
        // Calculate the direction and distance between the two objects
        var direction = position2 - position1;
        float distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return (Vector2.Zero, Vector2.Zero);
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        var m1 = SanitizeMass(mass1);
        var m2 = SanitizeMass(mass2);

        // Calculate the force magnitude
        float forceMagnitude = G * (m1 * m2) * distanceSquared;

        var force1 = forceMagnitude * normalizedDirection;
        var force2 = forceMagnitude * -normalizedDirection;

        return (force1, force2);
    }
 
    /// <summary>
    /// Applies a reverse-attraction force between two objects that scales with distance squared.
    /// Unlike real gravity, the force becomes stronger as distance increases.
    /// </summary>
    /// <param name="obj1"></param>
    /// <param name="obj2"></param>
    /// <returns></returns>
    public static void ApplyReverseAttractionForceRealistic(PhysicsObject obj1, PhysicsObject obj2)
    {
        // Calculate the direction and distance between the two objects
        var direction = obj2.Transform.Position - obj1.Transform.Position;
        float distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared <= Epsilon)
        {
            return;
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        var mass1 = SanitizeMass(obj1.Mass);
        var mass2 = SanitizeMass(obj2.Mass);
        // Calculate the force magnitude
        float forceMagnitude = G * (mass1 * mass2) * distanceSquared;

        obj1.AddForce(forceMagnitude * normalizedDirection);
        obj2.AddForce(forceMagnitude * -normalizedDirection);
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// The force scales with distance squared and the mass of the attraction object!
    /// A velocity pointing away from the origin has the strongest force, pointing towards the origin has the weakest force.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionMass">The mass of the attraction object.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <returns>Returns the final force. Use AddForce() in PhysicsObject.</returns>
    public static Vector2 CalculateReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, Vector2 objectPosition, Vector2 objectVelocity)
    {
        attractionMass = SanitizeMass(attractionMass);
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            
            float forceMagnitude = G * attractionMass * distanceSquared * dot;
            
            return -dir * forceMagnitude;
            
        }
        return Vector2.Zero;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// The force scales with distance squared and the mass of the attraction object!
    /// A velocity pointing away from the origin has the strongest force, pointing towards the origin has the weakest force.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionMass">The mass of the attraction object.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <returns>Returns the final force. Use AddForce() in PhysicsObject.</returns>
    public static Vector2 CalculateReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, Vector2 objectPosition, Vector2 objectVelocity, ValueRange dotRange)
    {
        attractionMass = SanitizeMass(attractionMass);
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            float forceMagnitude = G * attractionMass * distanceSquared * dot;
            return -dir * forceMagnitude;
            
        }
        return Vector2.Zero;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// The force scales with distance squared and the mass of the attraction object!
    /// A velocity pointing away from the origin has the strongest force, pointing towards the origin has the weakest force.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionMass">The mass of the attraction object.</param>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, PhysicsObject obj)
    {
        attractionMass = SanitizeMass(attractionMass);
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if (distanceSquared <= Epsilon) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            
            float forceMagnitude = G * attractionMass * distanceSquared * dot;
            var force =  -dir * forceMagnitude;
            obj.AddForce(force);
            
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// The force scales with distance squared and the mass of the attraction object!
    /// A velocity pointing away from the origin has the strongest force, pointing towards the origin has the weakest force.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionMass">The mass of the attraction object.</param>
    /// <param name="obj">The physics object to apply the force to.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, PhysicsObject obj, ValueRange dotRange)
    {
        attractionMass = SanitizeMass(attractionMass);
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if (distanceSquared <= Epsilon) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            float forceMagnitude = G * attractionMass * distanceSquared * dot;
            var force = -dir * forceMagnitude;
            obj.AddForce(force);
            return true;
        }

        return false;
    }
     
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, Vector2 objectVelocity)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);
            return -dir * attractionForce * dot * distanceFactor;
            
        }
        return Vector2.Zero;
    }
 
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);

            var force = -dir * attractionForce * dot * distanceFactor;
            obj.AddForce(force);
            
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, Vector2 objectVelocity, ValueRange dotRange)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);
            return -dir * attractionForce * dot * distanceFactor;
            
        }
        return Vector2.Zero;
    }
 
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj, ValueRange dotRange)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);

            var force = -dir * attractionForce * dot * distanceFactor;
            obj.AddForce(force);
            
            return true;
        }
        return false;
    }

    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, Vector2 objectVelocity, Func<float, float> distanceFactorAdjustor)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            return -dir * attractionForce * dot * distanceFactor;
            
        }
        return Vector2.Zero;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static bool ApplyReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj, Func<float, float> distanceFactorAdjustor)
    {
        if (!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if (distanceSquared <= Epsilon) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            var force =  -dir * attractionForce * dot * distanceFactor;
            obj.AddForce(force);
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, Vector2 objectVelocity, ValueRange dotRange, Func<float, float> distanceFactorAdjustor)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            return -dir * attractionForce * dot * distanceFactor;
            
        }
        return Vector2.Zero;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj, ValueRange dotRange, Func<float, float> distanceFactorAdjustor)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared > Epsilon)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared <= Epsilon) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            var force = -dir * attractionForce * dot * distanceFactor;
            obj.AddForce(force);
            
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var displacement = objectPosition - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if(distanceSquared <= Epsilon) return Vector2.Zero;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;

        var distanceFactor = attractionRadius.GetFactor(distance);
       
        return -dir * attractionForce * distanceFactor;
    }

    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, Func<float, float> distanceFactorAdjustor)
    {
        if(!attractionRadius.HasPositiveRange() ||  attractionForce <= 0f) return Vector2.Zero;
        
        var displacement = objectPosition - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if(distanceSquared <= Epsilon) return Vector2.Zero;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var distanceFactor = attractionRadius.GetFactor(distance);
        distanceFactor = distanceFactorAdjustor(distanceFactor);
        return -dir * attractionForce * distanceFactor;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// The force is applied to the obj.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj)
    {
        if (!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var displacement = obj.Transform.Position - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if (distanceSquared <= Epsilon) return false;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var distanceFactor = attractionRadius.GetFactor(distance);
        var force = -dir * attractionForce * distanceFactor;
        obj.AddForce(force);
        return true;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// The force is applied to the obj.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj,  Func<float, float> distanceFactorAdjustor)
    {
        if(!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var displacement = obj.Transform.Position - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if(distanceSquared <= Epsilon) return false;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var distanceFactor = attractionRadius.GetFactor(distance);
        distanceFactor = distanceFactorAdjustor(distanceFactor);
        var force = -dir * attractionForce  * distanceFactor;
        obj.AddForce(force);
        return true;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// Use AddForce() if force should be applied to PhysicsObjects!
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force scaled by distance squared.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, Vector2 objectPosition)
    {
        if( attractionForce <= 0f) return Vector2.Zero;
        
        var displacement = objectPosition - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if(distanceSquared <= Epsilon) return Vector2.Zero;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        return -dir * attractionForce * distanceSquared;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// Adds the calculated force to the obj.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force to apply, scaled by distance squared.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <returns>Returns if a force was applied to the obj.</returns>
    public static bool ApplyReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, PhysicsObject obj)
    {
        if (attractionForce <= 0f) return false;
        
        var displacement = obj.Transform.Position - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if (distanceSquared <= Epsilon) return false;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var force = -dir * attractionForce * distanceSquared;
        obj.AddForce(force);
        return true;
    }
   
    #endregion
}