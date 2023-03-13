using System.Numerics;
using ShapeCollision;
using ShapeEngineDemo.Bodies;
using Raylib_CsLo;
using ShapeCore;
using ShapePersistent;
using ShapeEngineDemo.DataObjects;
using ShapeAudio;
using ShapeLib;

namespace ShapeEngineDemo.Projectiles
{
    public struct ProjectileInfo
    {
        public Vector2 pos;
        public Vector2 dir;
        public Color color;
        public IDamageable dmgDealer;
        public float speedVariation;

        public ProjectileInfo(Vector2 pos, Vector2 dir, Color color, IDamageable dmgDealer, float speedVar = 0f)
        {
            this.pos = pos;
            this.dir = dir;
            this.color = color;
            this.dmgDealer = dmgDealer;
            this.speedVariation = speedVar;
        }
    }
    public class Projectile : GameObject, ICollidable
    {
        static readonly string[] collisionMask = new string[] { "asteroid" };

        protected string type = "";
        protected CircleCollider collider;
        protected IDamageable dmgDealer;
        protected Color color;
        protected bool dead = false;
        protected float timer = 0f;
        public StatHandler stats = new(("dmg", 10f), ("critChance", 0f), ("critBonus", 2.5f), ("lifetime", 0f), ("speed", 0f));

        public Projectile(string type, ProjectileInfo info)
        {
            DrawOrder = 15;
            this.type = type;
            this.dmgDealer = info.dmgDealer;
            Vector2 vel = new(0f, 0f);
            float size = 3f;
            var data = Demo.DATA.GetCDBContainer().Get<ProjectileData>("projectiles", type);
            if(data != null)
            {
                stats.SetStat("dmg", data.dmg);
                stats.SetStat("critChance", data.critChance);
                stats.SetStat("critBonus", data.critBonus);
                stats.SetStat("lifetime", data.lifetime);
                stats.SetStat("speed", data.speed);
                this.timer = stats.Get("lifetime");
                this.color = info.color;
                float a = SRNG.randF(-data.accuracy, data.accuracy);
                float speed = stats.Get("speed");
                float finalSpeed = speed + speed * SRNG.randF(-info.speedVariation, info.speedVariation);
                vel = SVec.Rotate(SVec.Normalize(info.dir), a) * finalSpeed;
                size = data.size;
            }
            collider = new CircleCollider(info.pos, vel, size);
        }
        public Projectile(string type, ProjectileInfo info, Dictionary<string, StatSimple> bonuses)
        {
            DrawOrder = 15;
            this.type = type;
            this.dmgDealer = info.dmgDealer;
            stats.SetBonuses(bonuses);
            Vector2 vel = new(0f, 0f);
            float size = 3f;
            var data = Demo.DATA.GetCDBContainer().Get<ProjectileData>("projectiles", type);
            if (data != null)
            {
                stats.SetStat("dmg", data.dmg);
                stats.SetStat("critChance", data.critChance);
                stats.SetStat("critBonus", data.critBonus);
                stats.SetStat("lifetime", data.lifetime);
                stats.SetStat("speed", data.speed);
                this.timer = stats.Get("lifetime");
                this.color = info.color;
                float a = SRNG.randF(-data.accuracy, data.accuracy);
                float speed = stats.Get("speed");
                float finalSpeed = speed + speed * SRNG.randF(-info.speedVariation, info.speedVariation);
                vel = SVec.Rotate(SVec.Normalize(info.dir), a) * finalSpeed;
                size = data.size;
            }
            collider = new CircleCollider(info.pos, vel, size);
            collider.CheckCollision = true;
            collider.CheckIntersections = false;
        }

        public Collider GetCollider() { return collider; }

        public virtual bool HasDynamicBoundingBox() { return true; }
        public string GetCollisionLayer() { return "bullet"; }
        public string[] GetCollisionMask() { return collisionMask; }
        public string GetID() { return type; }
        public override Vector2 GetPosition() { return collider.Pos; }
        public Vector2 GetPos() { return collider.Pos; }

        public override Rectangle GetBoundingBox()
        {
            return collider.GetBoundingRect();
        }
        public virtual void Overlap(CollisionInfo info) { }
        public virtual void OverlapEnded(ICollidable other) { }
        public override bool IsDead() { return dead; }
        public override bool Kill()
        {
            if (dead) return false;
            dead = true;
            collider.Disable();
            WasKilled();
            return dead;
        }

        protected override void WasKilled()
        {
            SpawnDeathEffect();
        }
        public override void Update(float dt)
        {
            if (IsDead()) return;
            if (UpdateLifetimeTimer(dt)) return;
            stats.Update(dt);
            Move(dt);
            
            //var info = GAMELOOP.GetCurArea().GetCurPlayfield().Collide(collider.Pos, collider.Radius);
            var info = SRect.CollidePlayfield(GAMELOOP.GetCurArea().GetInnerArea(), collider.Pos, collider.Radius);
            if (info.collided) PlayfieldCollision(info.hitPoint, info.n);
        }
        public override void Draw()
        {
            DrawCircle((int)collider.Pos.X, (int)collider.Pos.Y, collider.Radius, color);

            if (DEBUG_DRAWCOLLIDERS)
            {
                if (collider.IsEnabled()) collider.DebugDrawShape(DEBUG_ColliderColor);
                else collider.DebugDrawShape(DEBUG_ColliderDisabledColor);

                DrawRectangleLinesEx(GetBoundingBox(), 1f, GREEN);
            }
        }

        protected DamageInfo ImpactDamage(IDamageable target)
        {
            float finalDamage = stats.Get("dmg");
            bool critted = false;
            float critChance = stats.Get("critChance");
            if(critChance > 0f)
            {
                if (SRNG.randF() < critChance) { finalDamage *= stats.Get("critBonus"); critted = true; }
            }
            if (critted) AudioHandler.PlaySFX(SoundIDs.PROJECTILE_Crit, -1f, -1f, 0.1f);
            return target.Damage(finalDamage, collider.Pos, SVec.Normalize(collider.Vel), dmgDealer, critted);
        }
        protected virtual void Move(float dt)
        {
            collider.ApplyAccumulatedForce(dt);
            collider.Pos += collider.Vel * dt;
        }
        protected virtual bool UpdateLifetimeTimer(float dt)
        {
            if (timer > 0f)
            {
                timer -= dt;
                if (timer <= 0f)
                {
                    timer = 0f;
                    Kill();
                    return true;
                }
            }
            return false;
        }
        protected virtual void PlayfieldCollision(Vector2 hitPos, Vector2 normal)
        {
            collider.Pos = hitPos;
            Kill();
        }
        protected virtual void SpawnDeathEffect()
        {
            AsteroidDeathEffect ade = new(collider.Pos, 0.25f, collider.Radius * 1.5f, color);
            GAMELOOP.AddGameObject(ade);
        }

        
    }
}
