﻿using ShapeCore;
using ShapeCollision;
using ShapeEngineDemo.Bodies;
using ShapeAudio;

namespace ShapeEngineDemo.Projectiles
{
   
    public class Bullet : Projectile
    {
        
        public Bullet(ProjectileInfo info, Dictionary<string, StatSimple> bonuses, string type = "bullet") : base(type, info, bonuses)
        {

        }
        public Bullet(ProjectileInfo info, string type = "bullet") : base(type, info)
        {

        }
        public override void Overlap(CollisionInfo info)
        {
            //if (IsDead()) return;
            if (info.collision || info.overlapping)
            {
                if(info.other != null)
                {
                    var obj = info.other as IDamageable;
                    if(obj != null)
                    {
                        var dmgInfo = ImpactDamage(obj);
                        if (dmgInfo.recieved > 0f) AudioHandler.PlaySFX(SoundIDs.PROJECTILE_Impact, -1f, -1f, 0.1f);
                    }
                }
                Kill();
            }
        }
    }

}
