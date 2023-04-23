using ShapeLib;
using System.Numerics;

namespace ShapeCore
{
    public interface IPhysicsObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public float Mass { get; set; }
        public float Drag { get; set; }
        public Vector2 ConstAcceleration { get; set; }
        public void AddForce(Vector2 force);
        public void AddImpulse(Vector2 force);
        public bool IsStatic(float deltaSq) { return Vel.LengthSquared() <= deltaSq; }
        public Vector2 GetAccumulatedForce();
        public void ClearAccumulatedForce();
        public void UpdateState(float dt);
    }
    public class PhysicsObject : IPhysicsObject
    {
        public Vector2 Pos { get; set; }
        public Vector2 Vel { get; set; }
        public float Mass { get; set; }
        public float Drag { get; set; }
        public Vector2 ConstAcceleration { get; set; }

        protected Vector2 accumulatedForce = new(0f);
        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void ClearAccumulatedForce() { accumulatedForce = new(0f); }
        public void AddForce(Vector2 force) { accumulatedForce = SPhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { SPhysics.AddImpuls(this, force); }
        public void UpdateState(float dt) { SPhysics.UpdateState(this, dt); }
    }

}
