using System.Numerics;
using ShapeCore;
using ShapeCollision;
using ShapeEngineDemo.Bodies;
using Raylib_CsLo;
using ShapeAudio;
using ShapeLib;
//using ShapeEngineCore.Globals.Timing;

namespace ShapeEngineDemo.Projectiles
{
    public class Bouncer : Projectile
    {
        protected ICollidable? bounced = null;


        public Bouncer(ProjectileInfo info, Dictionary<string, StatSimple> bonuses, string type = "bouncer") : base(type, info, bonuses)
        {

        }
        public Bouncer(ProjectileInfo info, string type = "bouncer") : base(type, info)
        {

        }
        public override void Overlap(CollisionInfo info)
        {
            if (info.overlapping)
            {
                if (info.other != null)
                {
                    if (bounced == null || bounced != info.other)
                    {
                        bounced = info.other;
                        var obj = info.other as IDamageable;
                        if (obj != null)
                        {
                            var dmgInfo = ImpactDamage(obj);
                            if (dmgInfo.recieved > 0f) AudioHandler.PlaySFX("projectile bounce", -1f, -1f, 0.1f);
                        }
                        float angle = 180 + SRNG.randF(-45f, 45f);
                        collider.Vel = SVec.Rotate(collider.Vel, angle * DEG2RAD);
                    }
                }
                //Kill();
            }
            else
            {
                bounced = null;
            }
        }

        //public override void Draw()
        //{
        //    DrawLineEx(collider.Pos, collider.Pos - Vec.Normalize(collider.Vel) * collider.Radius * 2f, collider.Radius * 0.5f, color);
        //}
    }
}
