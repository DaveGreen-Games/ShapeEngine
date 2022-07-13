using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Timing;
using ShapeEngineCore.SimpleCollision;
using System.Numerics;

namespace ShapeEngineDemo.Bodies
{
    public class Unit : Body
    {
        //implement stun system
        //implement movement system that can be used by the player and the enemies
        protected float stunResistance = 1f;
        protected BasicTimer stunTimer = new();
        protected CircleCollider collider = new CircleCollider();
        public Vector2 MovementDir { get; protected set; } = new(0f, 0f);
        //public float MaxSpeed { get; protected set; } = 0f;
        public bool IsStunned() { return stunTimer.GetRemaining() > 0; }

        public void Stun(float duration)
        {
            if (IsDead() || stunResistance <= 0f) return;
            stunTimer.Start(duration / stunResistance);
            WasStunned(duration);
        }

        protected virtual void WasStunned(float duration) { }

        public override void Update(float dt)
        {
            base.Update(dt);
            stunTimer.Update(dt);
            stats.Update(dt);
            //if (!IsStunned())
            //{
            //    collider.Vel = Vec.Normalize(MovementDir) * MaxSpeed;
            //}
            //collider.Origin = collider.Origin + collider.Vel * dt;
        }

        public override Collider GetCollider()
        {
            return collider;
        }

        public override Rectangle GetBoundingBox()
        {
            return collider.GetBoundingRect();
        }
        public override Vector2 GetPos()
        {
            return collider.Pos;
        }
        public override Vector2 GetPosition()
        {
            return collider.Pos;
        }
    }
}
