
using ShapeEngine.Core;
using System.Numerics;
using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Lib
{
    public static class ShapePhysics
    {
        private static void ApplyAccumulatedForce(this IPhysicsObject p, float dt)
        {
            p.Vel += p.GetAccumulatedForce() * dt;
            p.ClearAccumulatedForce();
        }
        private static void ApplyAcceleration(this IPhysicsObject p, float dt)
        {
            Vector2 force = p.ConstAcceleration * dt;
            p.Vel += force;
            p.Vel = ShapePhysics.ApplyDragForce(p.Vel, p.Drag, dt);
        }

        /// <summary>
        /// Update the state of a physics object. That includes: 
        /// - applying and reseting the accumulatedForce of the frame,
        /// - applying the constant acceleration and drag,
        /// - adding the final velocity to the current position
        /// </summary>
        /// <param name="p">The physics objects to update.</param>
        /// <param name="dt">The current frames delta time.</param>
        public static void UpdateState(this IPhysicsObject p, float dt)
        {
            ApplyAccumulatedForce(p, dt);
            ApplyAcceleration(p, dt);
            p.Pos += p.Vel * dt;
        }
        /// <summary>
        /// Used for adding a force every frame. The total accumualted force should be applied at the end of the frame an reset to zero.
        /// </summary>
        /// <param name="force">The force that should be applied.</param>
        /// <param name="accumulatedForce">The current accumualted force from all AddForce calls this frame.</param>
        /// <param name="mass">The mass for calculating the final force that is added. Mass <= 0 has no impact.</param>
        /// <returns>Returns the new accumulated force value.</returns>
        public static Vector2 AddForce(Vector2 force, Vector2 accumulatedForce, float mass)
        {
            if( mass <= 0) accumulatedForce += force;
            else accumulatedForce += force / mass;
            return accumulatedForce;
        }

        /// <summary>
        /// Adds the force to the current accumulated force and returns the new accumulatedForce;
        /// AddForce should be used for adding forces every frame vs. AddImpuls that adds a force instantly to the velocty.
        /// </summary>
        /// <param name="p">The physics object that the force should be applied to.</param>
        /// <param name="force">The force to apply.</param>
        /// <returns>Returns the new accumulated force.</returns>
        public static Vector2 AddForce(this IPhysicsObject p, Vector2 force)
        {
            return AddForce(force, p.GetAccumulatedForce(), p.Mass);
        }

        /// <summary>
        /// Add an instant impulse to the velocity.
        /// </summary>
        /// <param name="force">The force to apply once.</param>
        /// <param name="velocity">The current velocity the force should be applied to.</param>
        /// <param name="mass">The mass for calculating the final force that is added. Mass <= 0 has no impact.</param>
        /// <returns>Returns the new velocty.</returns>
        public static Vector2 AddImpulse(Vector2 force, Vector2 velocity, float mass)
        {
            if (mass <= 0.0f) velocity += force;
            else velocity = velocity + force / mass;
            return velocity;
        }
        /// <summary>
        /// Instantly applies the force to the physics objects vel. Should be used for one time impulses vs. AddForce that is used to add force every frame.
        /// </summary>
        /// <param name="p">Physics Object that the force should be applied to.</param>
        /// <param name="force">Impuls force to apply.</param>
        public static void AddImpuls(this IPhysicsObject p, Vector2 force)
        {
            p.Vel = AddImpulse(force, p.Vel, p.Mass);
        }



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

    }
}
