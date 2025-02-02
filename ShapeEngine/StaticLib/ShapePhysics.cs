using System.Numerics;
using ShapeEngine.Core;

namespace ShapeEngine.StaticLib;

public static class ShapePhysics
{
    /// <summary>
    /// Gravitational constant 6.67430e-11
    /// </summary>
    public static readonly float GReal = 0.0000000000667430f;
    /// <summary>
    /// This is the gravitational constant used in all functions. The default value is 1f, basically disabling the constant to make the values that can be used much smaller.
    /// If the real value is needed just set it to GReal.
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
    public static void CalculateElasticCollision(this PhysicsObject obj1, PhysicsObject obj2, float r)
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
    
    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateAttraction(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2, float deltaTime)
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
        Vector2 acceleration1 = (forceMagnitude / mass1) * normalizedDirection;
        Vector2 acceleration2 = (forceMagnitude / mass2) * -normalizedDirection;

        // Update the velocities
        Vector2 newVelocity1 = velocity1 + acceleration1 * deltaTime;
        Vector2 newVelocity2 = velocity2 + acceleration2 * deltaTime;

        return (newVelocity1, newVelocity2);
    }
    public static void CalculateAttraction(PhysicsObject obj1, PhysicsObject obj2, float deltaTime)
    {
        var result = CalculateAttraction(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, deltaTime);
        obj1.Velocity = result.newVelocity1;
        obj2.Velocity = result.newVelocity2;
    }
    
    public static Vector2 CalculateAttraction(Vector2 position1, Vector2 velocity1, float mass1, Vector2 attractionPoint, float attractionForce, float deltaTime)
    {
        // Calculate the direction and distance between the object and the attraction point
        Vector2 direction = attractionPoint - position1;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return velocity1;
        }

        // Normalize the direction vector
        Vector2 normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = attractionForce / (distance * distance);

        // Calculate the acceleration
        Vector2 acceleration = (forceMagnitude / mass1) * normalizedDirection;

        // Update the velocity
        Vector2 newVelocity1 = velocity1 + acceleration * deltaTime;

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
    
    public static void CalculateAttraction(this PhysicsObject obj, Vector2 attractionPoint, float attractionForce, float deltaTime)
    {
        var newVelocity1 = CalculateAttraction(obj.Transform.Position, obj.Velocity, obj.Mass, attractionPoint, attractionForce, deltaTime);
        obj.Velocity = newVelocity1;
    }
    public static void CalculateAttraction(this PhysicsObject obj, Vector2 attractionPoint, float attractionForce, float drag, float deltaTime)
    {
        var newVelocity1 = CalculateAttraction(obj.Transform.Position, obj.Velocity, obj.Mass, attractionPoint, attractionForce, drag, deltaTime);
        obj.Velocity = newVelocity1;
    }

    public static (Vector2 newVelocity1, Vector2 newVelocity2) CalculateRepulsion(Vector2 position1, Vector2 velocity1, float mass1, Vector2 position2, Vector2 velocity2, float mass2, float deltaTime)
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
    public static void CalculateRepulsion(PhysicsObject obj1, PhysicsObject obj2, float deltaTime)
    {
        var result = CalculateRepulsion(obj1.Transform.Position, obj1.Velocity, obj1.Mass, obj2.Transform.Position, obj2.Velocity, obj2.Mass, deltaTime);
        obj1.Velocity = result.newVelocity1;
        obj2.Velocity = result.newVelocity2;
    }
    
    public static Vector2 CalculateRepulsion(Vector2 position1, Vector2 velocity1, float mass1, Vector2 repulsionPoint, float repulsionForce, float deltaTime)
    {
        // Calculate the direction and distance between the object and the attraction point
        Vector2 direction = position1 - repulsionPoint;
        float distance = direction.Length();

        // Avoid division by zero
        if (distance == 0)
        {
            return velocity1;
        }

        // Normalize the direction vector
        Vector2 normalizedDirection = direction / distance;

        // Calculate the force magnitude
        float forceMagnitude = repulsionForce / (distance * distance);

        // Calculate the acceleration
        Vector2 acceleration = (forceMagnitude / mass1) * normalizedDirection;

        // Update the velocity
        Vector2 newVelocity1 = velocity1 + acceleration * deltaTime;

        return newVelocity1;
    }
    public static Vector2 CalculateRepulsion(Vector2 position1, Vector2 velocity1, float mass1, Vector2 repulsionPoint, float repulsionForce, float drag, float deltaTime)
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
    
    public static void CalculateRepulsion(this PhysicsObject obj, Vector2 repulsionPoint, float repulsionForce, float deltaTime)
    {
        var newVelocity1 = CalculateRepulsion(obj.Transform.Position, obj.Velocity, obj.Mass, repulsionPoint, repulsionForce, deltaTime);
        obj.Velocity = newVelocity1;
    }
    public static void CalculateRepulsion(this PhysicsObject obj, Vector2 repulsionPoint, float repulsionForce, float drag, float deltaTime)
    {
        var newVelocity1 = CalculateRepulsion(obj.Transform.Position, obj.Velocity, obj.Mass, repulsionPoint, repulsionForce, drag, deltaTime);
        obj.Velocity = newVelocity1;
    }
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
    public static Vector2 ApplyFrictionForce(Vector2 velocity, float mass, float frictionCoefficient, Vector2 frictionNormal, Vector2 gravitationForce)
    {
        if (frictionNormal.X == 0 && frictionNormal.Y == 0) return velocity;
        if (frictionCoefficient <= 0) return velocity;
        if (gravitationForce.X == 0 && gravitationForce.Y == 0) return velocity;
        
        return velocity - CalculateFrictionForce(velocity, mass, frictionCoefficient, frictionNormal, gravitationForce);
        
    }
    public static Vector2 ApplyFrictionForce(Vector2 velocity, float mass, float frictionCoefficient, Vector2 frictionNormal)
    {
        if (frictionNormal.X == 0 && frictionNormal.Y == 0) return velocity;
        if (frictionCoefficient <= 0) return velocity;
        
        return velocity - CalculateFrictionForce(velocity, mass, frictionCoefficient, frictionNormal);
        
    }
    #endregion
}