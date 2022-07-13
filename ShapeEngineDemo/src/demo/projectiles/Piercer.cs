﻿using System.Numerics;
using ShapeEngineCore.Globals;
using ShapeEngineCore.SimpleCollision;
using ShapeEngineDemo.Bodies;
using ShapeEngineCore.Globals.Audio;
using ShapeEngineCore;
using Raylib_CsLo;
//using ShapeEngineCore.Globals.Timing;

namespace ShapeEngineDemo.Projectiles
{
    public class Piercer : Projectile 
    {
        protected ICollidable? pierced = null;
        

        public Piercer(ProjectileInfo info, Dictionary<string, StatSimple> bonuses, string type = "piercer") : base(type, info, bonuses)
        {

        }
        public Piercer(ProjectileInfo info, string type = "piercer") : base(type, info)
        {

        }
        public override void Overlap(OverlapInfo info)
        {
            if (info.overlapping)
            {
                if (info.other != null)
                {
                    if(pierced == null || pierced != info.other)
                    {
                        pierced = info.other;
                        var obj = info.other as IDamageable;
                        if (obj != null)
                        {
                            var dmgInfo = ImpactDamage(obj);
                            if(dmgInfo.recieved > 0f) AudioHandler.PlaySFX("projectile pierce", -1f, -1f, 0.1f);
                            //obj.Damage(damage, collider.Pos, Vec.Normalize(collider.Vel), dmgDealer);
                        }
                    }
                }
                //Kill();
            }
            else
            {
                pierced = null;
            }
        }

        public override void Draw()
        {
            DrawLineEx(collider.Pos, collider.Pos - Vec.Normalize(collider.Vel) * collider.Radius * 2f, collider.Radius * 0.5f, color);
        }
    }
}
