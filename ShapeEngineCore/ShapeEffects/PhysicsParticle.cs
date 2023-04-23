using Raylib_CsLo;
using ShapeCore;
using ShapeLib;
using System.Numerics;

namespace ShapeEffects
{
    public abstract class PhysicsParticle : Particle, ICollidable, ICollider// IPhysicsObject
    {
        public float Mass { get; set; }
        public float Drag { get; set; }
        public Vector2 ConstAcceleration { get; set; }
        public bool Enabled { get; set; }
        public bool ComputeCollision { get; set; }
        public bool ComputeIntersections { get; set; }

        protected Vector2 accumulatedForce = new(0f);


        public PhysicsParticle(Vector2 pos, Vector2 size) : base(pos, size) { }
        public PhysicsParticle(Vector2 pos, Vector2 size, float lifetime) : base(pos, size, lifetime) { }
        public PhysicsParticle(Vector2 pos, Vector2 size, Vector2 vel, float lifetime) : base(pos, size, vel, lifetime) { }
        public PhysicsParticle(Vector2 pos, Vector2 size, float angleDeg, float lifetime) : base(pos, size, angleDeg, lifetime) { }


        public Vector2 GetAccumulatedForce() { return accumulatedForce; }
        public void ClearAccumulatedForce() { accumulatedForce = new(0f); }
        public void AddForce(Vector2 force) { accumulatedForce = SPhysics.AddForce(this, force); }
        public void AddImpulse(Vector2 force) { SPhysics.AddImpuls(this, force); }
        public void UpdateState(float dt) { SPhysics.UpdateState(this, dt); }

        public override bool Update(float dt)
        {
            bool cancel = base.Update(dt);
            if (cancel) return true;
            else
            {
                UpdateState(dt);
                return false;
            }
        }

        //public uint GetID() { return }
        public ICollider GetCollider() { return this; }
        public abstract void Overlap(CollisionInfo info);
        public abstract void OverlapEnded(ICollidable other);
        public abstract uint GetCollisionLayer();
        public abstract uint[] GetCollisionMask();

        public void DrawDebugShape(Color color)
        {
            Raylib.DrawRectangleLinesEx(GetBoundingBox().Rectangle, 5f, color);
        }
        public (bool valid, bool overlap) CheckOverlap(ICollider other)
        {
            Rect r = GetBoundingBox();
            if (other is CircleCollider c)
            {
                return (true, r.OverlapRectCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, r.OverlapRectSegment(s));
            }
            else if (other is RectCollider rect)
            {
                return (true, r.OverlapRectRect(rect));
            }
            else if (other is PolyCollider p)
            {
                return (true, r.OverlapRectPoly(p));
            }
            return (false, false);
        }
        public (bool valid, Intersection i) CheckIntersection(ICollider other)
        {
            Rect r = GetBoundingBox();
            if (other is CircleCollider c)
            {
                return (true, r.IntersectionRectCircle(c));
            }
            else if (other is SegmentCollider s)
            {
                return (true, r.IntersectionRectSegment(s));
            }
            else if (other is RectCollider rect)
            {
                return (true, r.IntersectionRectRect(rect));
            }
            else if (other is PolyCollider p)
            {
                return (true, r.IntersectionRectPoly(p));
            }
            return (false, new());
        }
        public bool CheckOverlapRect(Rect rect) { return GetBoundingBox().OverlapRectRect(rect); }

    }
}
