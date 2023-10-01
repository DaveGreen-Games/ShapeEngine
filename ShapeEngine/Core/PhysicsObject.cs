using ShapeEngine.Lib;
using System.Numerics;
using ShapeEngine.Core.Interfaces;

namespace ShapeEngine.Core
{
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
        public void AddForce(Vector2 force) { accumulatedForce = ShapePhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { ShapePhysics.AddImpuls(this, force); }
        public void UpdateState(float dt) { ShapePhysics.UpdateState(this, dt); }
    }

}
