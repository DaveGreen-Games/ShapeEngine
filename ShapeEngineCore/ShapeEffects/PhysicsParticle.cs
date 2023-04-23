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
        public bool CheckCollision { get; set; }
        public bool CheckIntersections { get; set; }

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
        public bool Overlap(ICollider other)
        {
            Rect r = GetBoundingBox();
            if (other is CircleCollider c)
            {
                return SGeometry.OverlapRectCircle(r, c);
            }
            else if (other is SegmentCollider s)
            {
                return SGeometry.OverlapRectSegment(r, s);
            }
            else if (other is RectCollider rect)
            {
                return SGeometry.OverlapRectRect(r, rect);
            }
            else if (other is PolyCollider p)
            {
                return SGeometry.OverlapRectPoly(r, p);
            }
            else return other.Overlap(this);
        }
        public bool OverlapRect(Rect rect)
        {
            return SGeometry.OverlapRectRect(rect, GetBoundingBox());
        }
        public Intersection Intersect(ICollider other)
        {
            Rect r = GetBoundingBox();
            if (other is CircleCollider c)
            {
                return SGeometry.IntersectionRectCircle(r, c);
            }
            else if (other is SegmentCollider s)
            {
                return SGeometry.IntersectionRectSegment(r, s);
            }
            else if (other is RectCollider rect)
            {
                return SGeometry.IntersectionRectRect(r, rect);
            }
            else if (other is PolyCollider p)
            {
                return SGeometry.IntersectionRectPoly(r, p);
            }
            else return other.Intersect(this);
        }

    }
}
