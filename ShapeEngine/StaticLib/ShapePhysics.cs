using System.Numerics;
using ShapeEngine.Core;
using ShapeEngine.Core.Structs;

namespace ShapeEngine.StaticLib;

public static class ShapePhysics
{
    /// <summary>
    /// Gravitational constant 6.67430e-11
    /// </summary>
    public static readonly float GReal = 0.0000000000667430f;
    /// <summary>
    /// This is the gravitational constant used in all functions. The default value is 1f, essentially making the values that can be used much smaller and therefore more convenient.
    /// If the real value is needed, set it to GReal.
    /// </summary>
    public static float G = 1f;
   
    #region OLD
    // /// <summary>
    // /// Apply drag to the given value.
    // /// </summary>
    // /// <param name="value">The value that is affected by the drag.</param>
    // /// <param name="dragCoefficient">The drag coefficient for calculating the drag force. Has to be positive.
    // /// 1 / drag coefficient = seconds until stop. DC of 4 means object stops in 0.25s.</param>
    // /// <param name="dt">The delta time of the current frame.</param>
    // /// <returns></returns>
    // public static float ApplyDragForce(float value, float dragCoefficient, float dt)
    // {
    //     if (dragCoefficient <= 0f) return value;
    //     float dragForce = dragCoefficient * value * dt;
    //
    //     return value - MathF.Min(dragForce, value);
    // }
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
        // return ApplyDragFactor(vel, dragCoefficient, dt);
        
        if (dragCoefficient <= 0f) return vel;
        Vector2 dragForce = dragCoefficient * vel * dt;
        if (dragForce.LengthSquared() >= vel.LengthSquared()) return new Vector2(0f, 0f);
        return vel - dragForce;
    }
    // public static float GetDragForce(float value, float dragCoefficient, float dt)
    // {
    //     if (dragCoefficient <= 0f) return value;
    //     float dragForce = dragCoefficient * value * dt;
    //
    //     return -MathF.Min(dragForce, value);
    // }
    // public static Vector2 GetDragForce(Vector2 vel, float dragCoefficient, float dt)
    // {
    //     if (dragCoefficient <= 0f) return vel;
    //     Vector2 dragForce = dragCoefficient * vel * dt;
    //     if (dragForce.LengthSquared() >= vel.LengthSquared()) return new Vector2(0f, 0f);
    //     return -dragForce;
    // }
    //
    // public static Vector2 Attraction(Vector2 center, Vector2 otherPos, Vector2 otherVel, float r, float strength, float friction)
    // {
    //     Vector2 w = center - otherPos;
    //     float disSq = w.LengthSquared();
    //     float f = 1.0f - disSq / (r * r);
    //     Vector2 force = ShapeVec.Normalize(w) * strength;// * f;
    //     Vector2 stop = -otherVel * friction * f;
    //     return force + stop;
    // }
    // public static Vector2 ElasticCollision1D(Vector2 vel1, float mass1, Vector2 vel2, float mass2)
    // {
    //     float totalMass = mass1 + mass2;
    //     return vel1 * ((mass1 - mass2) / totalMass) + vel2 * (2f * mass2 / totalMass);
    // }
    // // public static Vector2 ElasticCollision2D(Vector2 p1, Vector2 v1, float m1, Vector2 p2, Vector2 v2, float m2, float R)
    // // {
    // //     float totalMass = m1 + m2;
    // //
    // //     float mf = m2 / m1;
    // //     Vector2 posDif = p2 - p1;
    // //     Vector2 velDif = v2 - v1;
    // //
    // //     Vector2 velCM = (m1 * v1 + m2 * v2) / totalMass;
    // //
    // //     float a = posDif.Y / posDif.X;
    // //     float dvx2 = -2f * (velDif.X + a * velDif.Y) / ((1 + a * a) * (1 + mf));
    // //     //Vector2 newOtherVel = new(v2.X + dvx2, v2.Y + a * dvx2);
    // //     Vector2 newSelfVel = new(v1.X - mf * dvx2, v1.Y - a * mf * dvx2);
    // //
    // //     newSelfVel = (newSelfVel - velCM) * R + velCM;
    // //     //newOtherVel = (newOtherVel - velCM) * R + velCM;
    // //
    // //     return newSelfVel;
    // // }
    // //
    //
     #endregion

    #region Tabnine
    
    // public static (Vector2 newVel1, Vector2 newVel2) ElasticCollision2D(Vector2 vel1, float mass1, Vector2 vel2, float mass2, float r)
    // {
    //     float totalMass = mass1 + mass2;
    //
    //     var velCM = (mass1 * vel1 + mass2 * vel2) / totalMass;
    //
    //     var relVel = vel1 - vel2;
    //     float relVelMag = relVel.Length();
    //
    //     var normal = relVel / relVelMag;
    //     var tangent = new Vector2(-normal.Y, normal.X);
    //
    //     float velNormal1 = Vector2.Dot(vel1, normal);
    //     float velTangent1 = Vector2.Dot(vel1, tangent);
    //
    //     float velNormal2 = Vector2.Dot(vel2, normal);
    //     float velTangent2 = Vector2.Dot(vel2, tangent);
    //
    //     float newVelNormal1 = (velNormal1 * (mass1 - mass2) + 2f * mass2 * velNormal2) / totalMass;
    //     float newVelNormal2 = (velNormal2 * (mass2 - mass1) + 2f * mass1 * velNormal1) / totalMass;
    //
    //     var newVel1 = (newVelNormal1 * normal + velTangent1 * tangent) * r + velCM;
    //     var newVel2 = (newVelNormal2 * normal + velTangent2 * tangent) * r + velCM;
    //
    //     return (newVel1, newVel2);
    // }
    // public static (Vector2 newVel1, Vector2 newVel2) ElasticCollision2D(Vector2 pos1, Vector2 vel1, float mass1, Vector2 pos2, Vector2 vel2, float mass2, float r)
    // {
    //     float totalMass = mass1 + mass2;
    //
    //     var velCM = (mass1 * vel1 + mass2 * vel2) / totalMass;
    //
    //     var relVel = vel1 - vel2;
    //     // float relVelMag = relVel.Length();
    //
    //     var normal = (pos2 - pos1).Normalize(); // / (pos2 - pos1).Length();
    //     var tangent = new Vector2(-normal.Y, normal.X);
    //
    //     float velNormal1 = Vector2.Dot(vel1, normal);
    //     float velTangent1 = Vector2.Dot(vel1, tangent);
    //
    //     float velNormal2 = Vector2.Dot(vel2, normal);
    //     float velTangent2 = Vector2.Dot(vel2, tangent);
    //
    //     float newVelNormal1 = (velNormal1 * (mass1 - mass2) + 2f * mass2 * velNormal2) / totalMass;
    //     float newVelNormal2 = (velNormal2 * (mass2 - mass1) + 2f * mass1 * velNormal1) / totalMass;
    //
    //     var newVel1 = (newVelNormal1 * normal + velTangent1 * tangent) * r + velCM;
    //     var newVel2 = (newVelNormal2 * normal + velTangent2 * tangent) * r + velCM;
    //
    //     return (newVel1, newVel2);
    // }
    //
    // public static void ElasticCollision2D(this PhysicsObject obj1, PhysicsObject obj2, float r)
    // {
    //     var result = ElasticCollision2D(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, r);
    //     obj1.Velocity = result.newVel1;
    //     obj2.Velocity = result.newVel2;
    // }
    // public static (Vector2 newVel1, Vector2 newVel2) ElasticCollision2D2(this PhysicsObject obj1, PhysicsObject obj2, float r)
    // {
    //     var result = ElasticCollision2D(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, r);
    //     return (result.newVel1, result.newVel2);
    // }
    //
    #endregion
    
    #region Github Copilot
    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateElasticCollision(Vector2 velocity1, float mass1, Vector2 velocity2, float mass2, float r)
    {
        // Ensure the elasticity parameter is between 0 and 1
        r = Math.Clamp(r, 0.0f, 1.0f);

        // Calculate the new velocities using the elastic collision formula
        Vector2 newVelocity1 = (velocity1 * (mass1 - r * mass2) + velocity2 * (1 + r) * mass2) / (mass1 + mass2);
        Vector2 newVelocity2 = (velocity2 * (mass2 - r * mass1) + velocity1 * (1 + r) * mass1) / (mass1 + mass2);

        return (newVelocity1, newVelocity2);
    }
    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateElasticCollision(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2, float r)
    {
        // Ensure the elasticity parameter is between 0 and 1
        r = Math.Clamp(r, 0.0f, 1.0f);

        // Calculate the relative velocity
        Vector2 relativeVelocity = velocity1 - velocity2;

        // Calculate the normal vector
        Vector2 normal = Vector2.Normalize(position1 - position2);

        // Calculate the velocity along the normal
        float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

        // If the objects are moving apart, do nothing
        if (velocityAlongNormal > 0)
        {
            return (velocity1, velocity2);
        }

        // Calculate the impulse scalar
        float impulseScalar = (-(1 + r) * velocityAlongNormal) / (1 / mass1 + 1 / mass2);

        // Calculate the impulse
        Vector2 impulse = impulseScalar * normal;

        // Update velocities
        Vector2 newVelocity1 = velocity1 + impulse / mass1;
        Vector2 newVelocity2 = velocity2 - impulse / mass2;

        return (newVelocity1, newVelocity2);
    }
    public static void ApplyElasticCollision(this PhysicsObject obj1, PhysicsObject obj2, float r)
    {
        var result = CalculateElasticCollision(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, r);
        obj1.Velocity = result.newVelocity1;
        obj2.Velocity = result.newVelocity2;
    }
    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateElasticCollision2(this PhysicsObject obj1, PhysicsObject obj2, float r)
    {
        var result = CalculateElasticCollision(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, r);
        return (result.newVelocity1, result.newVelocity2);
    }
    
    /// <summary>
    /// This function calculates a frame rate independent drag force.
    /// </summary>
    /// <param name="drag">Drag coefficient between 0 and 1. How much energy should the velocity loose each second.</param>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public static float CalculateDragFactor(float drag, float deltaTime)
    {
        if (drag <= 0) return 1;
        if (drag >= 1) return 0;
        
        float dragFactor = (float)Math.Pow(1.0 - drag, deltaTime);

        return dragFactor;
    }
    /// <summary>
    /// This function calculates a frame rate independent drag force and applies it to the supplied velocity.
    /// </summary>
    /// <param name="velocity">The affected velocity.</param>
    /// <param name="drag">Drag coefficient between 0 and 1. How much energy should the velocity loose each second.</param>
    /// <param name="deltaTime"></param>
    /// <returns>Returns the new scaled velocity.</returns>
    public static Vector2 ApplyDragFactor(Vector2 velocity, float drag, float deltaTime)
    {
        if (drag <= 0) return velocity;
        if (drag >= 1) return Vector2.Zero;
        float dragFactor = (float)Math.Pow(1.0 - drag, deltaTime);

        return velocity * dragFactor;
    }
    public static void ApplyDragFactor(this PhysicsObject obj, float drag, float deltaTime)
    {
        if (drag <= 0) return;
        if (drag >= 1)
        {
            obj.Velocity = Vector2.Zero;
            return;
        }
        
        var dragFactor = (float)Math.Pow(1.0 - drag, deltaTime);

        obj.Velocity = obj.Velocity  * dragFactor;
    }
    
    
    //TODO: remove delta time and return either the force to apply or use PhysicsObject.ApplyForce function to apply forces.
    // all calculate variantes should divide out the mass for the returned force (AddForceRaw has to be used when dealing with PhysicsObjects!)
    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateAttraction(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2, float deltaTime)
    {
        // Calculate the direction and distance between the two objects
        var direction = position2 - position1;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return (velocity1, velocity2);
        }

        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = G * (mass1 * mass2) / (distance * distance);

        // Calculate the accelerations
        var acceleration1 = (forceMagnitude / mass1) * normalizedDirection;
        var acceleration2 = (forceMagnitude / mass2) * -normalizedDirection;

        // Update the velocities
        var newVelocity1 = velocity1 + acceleration1 * deltaTime;
        var newVelocity2 = velocity2 + acceleration2 * deltaTime;

        return (newVelocity1, newVelocity2);
    }
    public static void ApplyAttraction(PhysicsObject obj1, PhysicsObject obj2, float deltaTime)
    {
        var result = CalculateAttraction(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, deltaTime);
        obj1.Velocity = result.newVelocity1;
        obj2.Velocity = result.newVelocity2;
    }
    
    public static Vector2 CalculateAttraction(Vector2 position1, Vector2 velocity1, float mass1, Vector2 attractionPoint, float attractionForce, float deltaTime)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = attractionPoint - position1;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return velocity1;
        }

        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = attractionForce / (distance * distance);

        // Calculate the acceleration
        var acceleration = (forceMagnitude / mass1) * normalizedDirection;

        // Update the velocity
        var newVelocity1 = velocity1 + acceleration * deltaTime;

        return newVelocity1;
    }
    public static Vector2 CalculateAttraction(Vector2 position1, Vector2 velocity1, float mass1, Vector2 attractionPoint, float attractionForce, float drag, float deltaTime)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = attractionPoint - position1;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return velocity1;
        }

        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = attractionForce / (distance * distance);

        // Calculate the acceleration
        var acceleration = (forceMagnitude / mass1) * normalizedDirection;

        // Update the velocity
        var newVelocity1 = velocity1 + acceleration * deltaTime;
        return ApplyDragFactor(newVelocity1, drag, deltaTime);
    }
    
    public static void ApplyAttraction(this PhysicsObject obj, Vector2 attractionPoint, float attractionForce, float deltaTime)
    {
        var newVelocity1 = CalculateAttraction(obj.Transform.Position, obj.Velocity, obj.Mass, attractionPoint, attractionForce, deltaTime);
        obj.Velocity = newVelocity1;
    }
    public static void ApplyAttraction(this PhysicsObject obj, Vector2 attractionPoint, float attractionForce, float drag, float deltaTime)
    {
        var newVelocity1 = CalculateAttraction(obj.Transform.Position, obj.Velocity, obj.Mass, attractionPoint, attractionForce, drag, deltaTime);
        obj.Velocity = newVelocity1;
    }

    public static (Vector2 newVelocity1, Vector2 newVelocity2) ApplyRepulsion(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2, float deltaTime)
    {
        // Calculate the direction and distance between the two objects
        Vector2 direction = position2 - position1;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return (velocity1, velocity2);
        }

        // Normalize the direction vector
        Vector2 normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = G * (mass1 * mass2) / (distance * distance);

        // Calculate the accelerations
        Vector2 acceleration1 = (forceMagnitude / mass1) * -normalizedDirection;
        Vector2 acceleration2 = (forceMagnitude / mass2) * normalizedDirection;

        // Update the velocities
        Vector2 newVelocity1 = velocity1 + acceleration1 * deltaTime;
        Vector2 newVelocity2 = velocity2 + acceleration2 * deltaTime;

        return (newVelocity1, newVelocity2);
    }
    public static void ApplyRepulsion(PhysicsObject obj1, PhysicsObject obj2, float deltaTime)
    {
        var result = ApplyRepulsion(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, deltaTime);
        obj1.Velocity = result.newVelocity1;
        obj2.Velocity = result.newVelocity2;
    }
    
    public static Vector2 ApplyRepulsion(Vector2 position1, Vector2 velocity1, float mass1, Vector2 repulsionPoint, float repulsionForce, float deltaTime)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = position1 - repulsionPoint;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return velocity1;
        }

        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = repulsionForce / (distance * distance);

        // Calculate the acceleration
        var acceleration = (forceMagnitude / mass1) * normalizedDirection;

        // Update the velocity
        var newVelocity1 = velocity1 + acceleration * deltaTime;

        return newVelocity1;
    }
    public static Vector2 ApplyRepulsion(Vector2 position1, Vector2 velocity1, float mass1, Vector2 repulsionPoint, float repulsionForce, float drag, float deltaTime)
    {
        // Calculate the direction and distance between the object and the attraction point
        var direction = position1 - repulsionPoint;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return velocity1;
        }

        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = repulsionForce / (distance * distance);

        // Calculate the acceleration
        var acceleration = (forceMagnitude / mass1) * normalizedDirection;

        // Update the velocity
        var newVelocity1 = velocity1 + acceleration * deltaTime;
        return ApplyDragFactor(newVelocity1, drag, deltaTime);
    }
    
    public static void ApplyRepulsion(this PhysicsObject obj, Vector2 repulsionPoint, float repulsionForce, float deltaTime)
    {
        var newVelocity1 = ApplyRepulsion(obj.Transform.Position, obj.Velocity, obj.Mass, repulsionPoint, repulsionForce, deltaTime);
        obj.Velocity = newVelocity1;
    }
    public static void ApplyRepulsion(this PhysicsObject obj, Vector2 repulsionPoint, float repulsionForce, float drag, float deltaTime)
    {
        var newVelocity1 = ApplyRepulsion(obj.Transform.Position, obj.Velocity, obj.Mass, repulsionPoint, repulsionForce, drag, deltaTime);
        obj.Velocity = newVelocity1;
    }
   
    
    /// <summary>
    /// Calculates a friction force considering the mass, friction coefficient, friction normal, and gravitational force.
    /// </summary>
    /// <returns>Returns the friction force.</returns>
    public static Vector2 CalculateFrictionForce(Vector2 velocity, float mass, float frictionCoefficient, Vector2 frictionNormal, Vector2 gravitationForce)
    {
        if (frictionNormal.X == 0 && frictionNormal.Y == 0) return Vector2.Zero;
        if (frictionCoefficient <= 0) return Vector2.Zero;
        if (gravitationForce.X == 0 && gravitationForce.Y == 0) return Vector2.Zero;
        
        // Normalize the frictionNormal vector
        Vector2 normalizedFrictionNormal = frictionNormal.Normalize();
        
        // Calculate the normal force magnitude
        float normalForceMagnitude = Vector2.Dot(gravitationForce, normalizedFrictionNormal);

        // Ensure the normal force magnitude is positive
        normalForceMagnitude = Math.Abs(normalForceMagnitude);

        // Calculate the effective normal force considering the mass
        float effectiveNormalForce = normalForceMagnitude * mass;

        // Calculate the friction force magnitude
        float frictionForceMagnitude = frictionCoefficient * effectiveNormalForce;

        // Calculate the friction force vector (opposite to the direction of velocity)
        Vector2 frictionForce = -Vector2.Normalize(velocity) * frictionForceMagnitude;

        return frictionForce;
    }
    /// <summary>
    /// Calculates a friction force considering the mass, friction coefficient, friction normal, and gravitational force.
    /// </summary>
    /// <returns>Returns the friction force.</returns>
    public static Vector2 CalculateFrictionForce(Vector2 velocity, float mass, float frictionCoefficient, Vector2 frictionNormal)
    {
        if (frictionNormal.X == 0 && frictionNormal.Y == 0) return Vector2.Zero;
        if (frictionCoefficient <= 0) return Vector2.Zero;
        
        // Normalize the frictionNormal vector
        Vector2 normalizedFrictionNormal = frictionNormal.Normalize();
        
        // Calculate the component of velocity perpendicular to the friction normal
        float perpendicularComponent = Vector2.Dot(velocity, new Vector2(-normalizedFrictionNormal.Y, normalizedFrictionNormal.X));
        
        // Calculate the friction force magnitude based on the perpendicular component
        float frictionForceMagnitude = frictionCoefficient * Math.Abs(perpendicularComponent) * mass;

        // Calculate the friction force vector (opposite to the direction of velocity)
        Vector2 frictionForce = -Vector2.Normalize(velocity) * frictionForceMagnitude;

        return frictionForce;
    }
    /// <summary>
    /// Calculates and applies a friction force to the velocity considering the mass, friction coefficient, friction normal, and gravitational force.
    /// </summary>
    /// <returns>Returns the new velocity.</returns>
    public static Vector2 ApplyFrictionForce(Vector2 velocity, float mass, float frictionCoefficient, Vector2 frictionNormal, Vector2 gravitationForce)
    {
        if (frictionNormal.X == 0 && frictionNormal.Y == 0) return velocity;
        if (frictionCoefficient <= 0) return velocity;
        if (gravitationForce.X == 0 && gravitationForce.Y == 0) return velocity;
        
        return velocity - CalculateFrictionForce(velocity, mass, frictionCoefficient, frictionNormal, gravitationForce);
        
    }
    /// <summary>
    /// Calculates and applies a friction force to the velocity considering the mass, friction coefficient, friction normal, and gravitational force.
    /// </summary>
    /// <returns>Returns the new velocity.</returns>
    public static Vector2 ApplyFrictionForce(Vector2 velocity, float mass, float frictionCoefficient, Vector2 frictionNormal)
    {
        if (frictionNormal.X == 0 && frictionNormal.Y == 0) return velocity;
        if (frictionCoefficient <= 0) return velocity;
        
        return velocity - CalculateFrictionForce(velocity, mass, frictionCoefficient, frictionNormal);
        
    }
    
    #endregion

    #region Reverse Attraction

    /// <summary>
    /// Calculates a gravitational force between two objects that scales based on distance squared.
    /// It is called realistic because even though it is doing the reverse of gravity...
    /// The further away the objects are, the stronger the gravitational force will be.
    /// Each force is already divided by its corresponding mass.
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="mass1"></param>
    /// <param name="position2"></param>
    /// <param name="mass2"></param>
    /// <returns>Returns the force (acceleration) that can be applied. Corresponding masses are already divided out. Use AddForceRaw in PhysicsObject!</returns>
    public static (Vector2 force1, Vector2 force2) CalculateReverseAttractionForceRealistic(Vector2 position1, float mass1, Vector2 position2, float mass2)
    {
        // Calculate the direction and distance between the two objects
        var direction = position2 - position1;
        float distanceSquared = direction.LengthSquared();

        // Avoid division by zero
        if (distanceSquared == 0)
        {
            return (Vector2.Zero, Vector2.Zero);
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = G * (mass1 * mass2) * distanceSquared;

        // Calculate the accelerations
        var acceleration1 = (forceMagnitude / mass1) * normalizedDirection;
        var acceleration2 = (forceMagnitude / mass2) * -normalizedDirection;

        return (acceleration1, acceleration2);
    }
    /// <summary>
    /// Calculates a gravitational force between two objects that scales based on distance squared.
    /// The further away the objects are, the stronger the gravitational force will be. (reverse than in reality)
    /// Applies the forces to the objects.
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
        if (distanceSquared == 0)
        {
            return;
        }

        float distance = MathF.Sqrt(distanceSquared);
        // Normalize the direction vector
        var normalizedDirection = direction / distance;

        var mass1 = obj1.Mass;
        var mass2 = obj2.Mass;
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
    /// <returns>Returns the final force. Mass is already divided out. Use AddForceRaw in PhysicsObject.</returns>
    public static Vector2 CalculateReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, Vector2 objectPosition, Vector2 objectVelocity)
    {
        if(attractionMass <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return Vector2.Zero;
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
    /// <returns>Returns the final force. Mass is already divided out. Use AddForceRaw in PhysicsObject.</returns>
    public static Vector2 CalculateReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, Vector2 objectPosition, Vector2 objectVelocity, ValueRange dotRange)
    {
        if(attractionMass <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return Vector2.Zero;
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
    /// <returns>Returns the final force. Mass is already divided out. Use AddForceRaw in PhysicsObject.</returns>
    public static bool ApplyReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, PhysicsObject obj)
    {
        if (attractionMass <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if (distanceSquared == 0f) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            
            float forceMagnitude = G * attractionMass * distanceSquared * dot;
            var force =  -dir * forceMagnitude;
            obj.AddForceRaw(force);
            
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
    /// <returns>Returns the final force. Mass is already divided out. Use AddForceRaw in PhysicsObject.</returns>
    public static bool ApplyReverseAttractionForceRealistic(Vector2 attractionOrigin, float attractionMass, PhysicsObject obj, ValueRange dotRange)
    {
        if (attractionMass <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if (distanceSquared == 0f) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            float forceMagnitude = G * attractionMass * distanceSquared * dot;
            var force = -dir * forceMagnitude;
            obj.AddForceRaw(force);
            return true;
        }

        return false;
    }
    
    
     
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectMass">The mass of the object. (Scales the returned force)</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, float objectMass, Vector2 objectVelocity)
    {
        if(objectMass <= 0f || !attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);
            return -dir * attractionForce * objectMass * dot * distanceFactor;
            
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
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj)
    {
        if(obj.Mass <= 0f || !attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);

            var force = -dir * attractionForce * obj.Mass * dot * distanceFactor;
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
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectMass">The mass of the object. (Scales the returned force)</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, float objectMass, Vector2 objectVelocity, ValueRange dotRange)
    {
        if(objectMass <= 0f || !attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);
            return -dir * attractionForce * objectMass * dot * distanceFactor;
            
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
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
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
        if (objectSpeedSquared != 0f)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);

            var force = -dir * attractionForce * dot * distanceFactor;
            obj.AddForceRaw(force);
            
            return true;
        }
        return false;
    }
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectMass">The mass of the object. (Scales the returned force)</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, float objectMass, Vector2 objectVelocity, Func<float, float> distanceFactorAdjustor)
    {
        if(objectMass <= 0f || !attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            return -dir * attractionForce * objectMass * dot * distanceFactor;
            
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
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static bool CalculateReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj, Func<float, float> distanceFactorAdjustor)
    {
        if (!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var objectSpeedSquared = obj.Velocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if (distanceSquared == 0f) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = (dot + 1f) / 2f; //translate range from [-1, 1] to [0, 1]
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            var force =  -dir * attractionForce * dot * distanceFactor;
            obj.AddForceRaw(force);
            return true;
        }

        return false;
    }
    /// <summary>
    /// Calculates a force that gets stronger with distance and direction.
    /// At a distance equal to attractionRadius with a velocity directly in line with the direction from the origin to the object,
    /// the force equals attractionForce.
    /// If the velocity direction points towards the origin or the distance is 0, the force is 0.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectMass">The mass of the object. (Scales the returned force)</param>
    /// <param name="objectVelocity">The velocity of the object.</param>
    /// <param name="dotRange">Set the multiplier range for pointing towards the center or away from the center.
    /// [0 and 1] would make force 0 when pointing towards the center and max when pointing away.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForceDirectional(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, float objectMass, Vector2 objectVelocity, ValueRange dotRange, Func<float, float> distanceFactorAdjustor)
    {
        if(objectMass <= 0f || !attractionRadius.HasPositiveRange() || attractionForce <= 0f) return Vector2.Zero;
        
        var objectSpeedSquared = objectVelocity.LengthSquared();
        if (objectSpeedSquared != 0f)
        {
            var displacement = objectPosition - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return Vector2.Zero;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = objectVelocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            return -dir * attractionForce * objectMass * dot * distanceFactor;
            
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
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
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
        if (objectSpeedSquared != 0f)
        {
            var displacement = obj.Transform.Position - attractionOrigin;
            var distanceSquared = displacement.LengthSquared();
            if(distanceSquared == 0f) return false;
            var distance = MathF.Sqrt(distanceSquared);
            var dir = displacement / distance;
            var objectSpeed = MathF.Sqrt(objectSpeedSquared);
            var objectDir = obj.Velocity / objectSpeed;
            var dot = objectDir.Dot(dir);
            dot = ShapeMath.RemapFloat(dot, -1f, 1f, dotRange.Min, dotRange.Max);
            var distanceFactor = attractionRadius.GetFactor(distance);
            distanceFactor = distanceFactorAdjustor(distanceFactor);
            var force = -dir * attractionForce * dot * distanceFactor;
            obj.AddForceRaw(force);
            
            return true;
        }
        return false;
    }
    
    
    
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectMass">The mass of the object. Is applied to the final force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, float objectMass)
    {
        if(!attractionRadius.HasPositiveRange() || objectMass <= 0f  || attractionForce <= 0f) return Vector2.Zero;
        
        var displacement = objectPosition - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if(distanceSquared == 0f) return Vector2.Zero;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;

        var distanceFactor = attractionRadius.GetFactor(distance);
       
        return -dir * attractionForce * objectMass * distanceFactor;
    }
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectMass">The mass of the object. Is applied to the final force.</param>
    /// <param name="distanceFactorAdjustor">Supply a method that takes a factor between 0 and 1 and returns a new factor as float.
    /// The new factor will be multiplied with the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, Vector2 objectPosition, float objectMass, Func<float, float> distanceFactorAdjustor)
    {
        if(!attractionRadius.HasPositiveRange() || objectMass <= 0f ||  attractionForce <= 0f) return Vector2.Zero;
        
        var displacement = objectPosition - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if(distanceSquared == 0f) return Vector2.Zero;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var distanceFactor = attractionRadius.GetFactor(distance);
        distanceFactor = distanceFactorAdjustor(distanceFactor);
        return -dir * attractionForce * objectMass * distanceFactor;
    }
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// The force is applied to the obj.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
    /// <param name="attractionRadius">The distance range at which minimum and maximum force is calculated.
    /// Distances below the attractionRadius.Min result in 0 force.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <returns>Returns if a force was applied.</returns>
    public static bool ApplyReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, ValueRange attractionRadius, PhysicsObject obj)
    {
        if (!attractionRadius.HasPositiveRange() || attractionForce <= 0f) return false;
        
        var displacement = obj.Transform.Position - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if (distanceSquared == 0f) return false;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var distanceFactor = attractionRadius.GetFactor(distance);
        var force = -dir * attractionForce * distanceFactor;
        obj.AddForceRaw(force);
        return true;
    }
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// At a distance equal to attractionRadius, the force equals attractionForce.
    /// The force is applied to the obj.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by direction and distance factors.</param>
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
        if(distanceSquared == 0f) return false;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var distanceFactor = attractionRadius.GetFactor(distance);
        distanceFactor = distanceFactorAdjustor(distanceFactor);
        var force = -dir * attractionForce  * distanceFactor;
        obj.AddForceRaw(force);
        return true;
    }
    
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The force scaled by distance squared.</param>
    /// <param name="objectPosition">The position of the object.</param>
    /// <param name="objectMass">The mass of the object. Is applied to the resulting force.</param>
    /// <returns>Returns the final force.</returns>
    public static Vector2 CalculateReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, Vector2 objectPosition, float objectMass)
    {
        if(objectMass <= 0f || attractionForce <= 0f) return Vector2.Zero;
        
        var displacement = objectPosition - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if(distanceSquared == 0f) return Vector2.Zero;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        return -dir * attractionForce * objectMass * distanceSquared;
    }
    /// <summary>
    /// Calculates a force that gets stronger with distance.
    /// Adds the calculated force to the obj.
    /// </summary>
    /// <param name="attractionOrigin">The origin of the attraction force.</param>
    /// <param name="attractionForce">The fore to apply scaled by distance squared.</param>
    /// <param name="obj">The object the force should apply to.</param>
    /// <returns>Returns if a force was applied to the obj.</returns>
    public static bool ApplyReverseAttractionForce(Vector2 attractionOrigin, float attractionForce, PhysicsObject obj)
    {
        if (attractionForce <= 0f) return false;
        
        var displacement = obj.Transform.Position - attractionOrigin;
        var distanceSquared = displacement.LengthSquared();
        if (distanceSquared == 0f) return false;
        var distance = MathF.Sqrt(distanceSquared);
        var dir = displacement / distance;
       
        var force = -dir * attractionForce * distanceSquared;
        obj.AddForceRaw(force);
        return true;
    }
    #endregion
}