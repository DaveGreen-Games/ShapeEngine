using ShapeEngine.Core;
using ShapeEngine.Lib;
using System.Net.Http.Headers;
using System.Numerics;

namespace ShapeEngine.Effects
{
    public abstract class PhysicsParticle : Particle, ICollidable, ICollider// IPhysicsObject
    {
        public float Mass { get; set; }
        public float Drag { get; set; }
        public Vector2 ConstAcceleration { get; set; }
        public bool Enabled { get; set; } = true;
        public bool ComputeCollision { get; set; } = false;
        public bool ComputeIntersections { get; set; } = false;
        public bool SimplifyCollision { get; set; } = false;
        public bool FlippedNormals { get; set; } = false;

        protected Vector2 accumulatedForce = new(0f);

        private Vector2 prevPos = new();

        public PhysicsParticle(Vector2 pos, Vector2 size) : base(pos, size) { }
        public PhysicsParticle(Vector2 pos, Vector2 size, float lifetime) : base(pos, size, lifetime) { }
        public PhysicsParticle(Vector2 pos, Vector2 size, Vector2 vel, float lifetime) : base(pos, size, vel, lifetime) { }
        public PhysicsParticle(Vector2 pos, Vector2 size, float angleDeg, float lifetime) : base(pos, size, angleDeg, lifetime) { }

        
        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void ClearAccumulatedForce() { accumulatedForce = new(0f); }
        public void AddForce(Vector2 force) { accumulatedForce = SPhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { SPhysics.AddImpuls(this, force); }
        public void UpdateState(float dt) 
        {
            SPhysics.UpdateState(this, dt); 
        }

        public override bool Update(float dt)
        {
            UpdatePreviousPosition(dt);

            bool cancel = base.Update(dt);
            if (cancel) return true;
            else
            {
                UpdateState(dt);
                return false;
            }
        }

        public ICollider GetCollider() { return this; }
        public abstract void Overlap(CollisionInformation info);
        public abstract void OverlapEnded(ICollidable other);
        public abstract uint GetCollisionLayer();
        public abstract uint[] GetCollisionMask();

        public abstract IShape GetShape();
        public abstract IShape GetSimplifiedShape();
        public abstract void DrawDebugShape(Raylib_CsLo.Color color);

        public Vector2 GetPreviousPosition()
        {
            return prevPos;
        }

        public void UpdatePreviousPosition(float dt)
        {
            prevPos = Pos;
        }
    }
}
