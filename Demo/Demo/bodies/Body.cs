using System.Numerics;
using ShapeEngine;
using ShapeCollision;

namespace Demo.Bodies
{
    public struct HealInfo
    {
        public IDamageable target;
        public float total = 0f;
        public float recieved = 0f;
        public bool fullyHealed = false;
        public bool dead = false;

        //public HealInfo(IDamageable target) { total = 0f; recieved = 0f; fullyHealed = false; dead = false;  this.target = target; }
        public HealInfo(IDamageable target, bool dead) { total = 0f; recieved = 0f; fullyHealed = false; this.dead = dead; this.target = target; }
        public HealInfo(float total, float recieved, bool fullyHealed, bool dead, IDamageable target)
        {
            this.total = total;
            this.recieved = recieved;
            this.fullyHealed = fullyHealed;
            this.target = target;
            this.dead = dead;
        }
    }
    public struct DamageInfo
    {
        public IDamageable target;
        public float total = 0f;
        public float recieved = 0f;
        public float reflected = 0f;
        public bool killed = false;
        public bool dead = false;
        public Vector2 pos = new();
        public Vector2 dir = new();
        public bool crit = false;
        //public DamageInfo(IDamageable target) { total = 0f; reflected = 0f; recieved = 0f; killed = false; dead = false; pos = new(); dir = new(); this.target = target; crit = false; }
        public DamageInfo(IDamageable target, bool dead) { total = 0f; reflected = 0f; recieved = 0f; killed = false; this.dead = dead; pos = new(); dir = new(); this.target = target; crit = false; }
        public DamageInfo(float total, float recieved, float reflected, bool killed, bool dead, Vector2 pos, Vector2 dir, IDamageable target, bool crit)
        {
            this.total = total;
            this.recieved = recieved;
            this.reflected = reflected;
            this.killed = killed;
            this.dead = dead;
            this.pos = pos;
            this.dir = dir;
            this.target = target;
            this.crit = crit;
        }

    }
    public interface IDamageable
    {
        
        public DamageInfo Damage(float amount, Vector2 pos, Vector2 dir, IDamageable dmgDealer, bool crit);
        public HealInfo Heal(float amount, Vector2 pos, IDamageable dmgDealer);

        
        public bool CanBeHealed();
        public bool CanBeDamaged();

        public float GetDamage();
    }
    
    public abstract class Body : GameObject, ICollidable, IDamageable
    {

        //private float totalHealth = 100f;
        private float curHealth = 100f;
        protected string[] collisionMask = new string[0];
        protected bool dead = false;
        //protected Dictionary<string, StatSimple> stats = new() { { "hp", new(100f)} };
        protected StatHandler stats = new(("hp", 100f));

        public Body()
        {
            stats.OnStatChanged += StatChanged;
        }
        public override bool IsDead(){ return dead; }
        public override bool Kill()
        {
            if (IsDead()) return false;
            dead = true;
            WasKilled();
            return dead;
        }

        public virtual void WasDamaged(DamageInfo info) { }
        public virtual void WasHealed(HealInfo info) { }
        public float GetHealthPercentage()
        {
            if (GetTotalHealth() <= 0) return 0f;
            return curHealth / GetTotalHealth();
        }
        public float GetTotalHealth() 
        {
            if (stats.Has("hp")) return stats.Get("hp");
            else return 0f;
        }
        protected void SetTotalHealth(float value)
        {
            if (!stats.Has("hp")) return;
            stats.SetBase("hp", value);
            curHealth = stats.Get("hp");
        }
        
        public float GetHealth() { return curHealth; }
        protected void SetHealth(float value)
        {
            if (value <= 0f) curHealth = 0f;
            else if(value > GetTotalHealth())
            {
                SetTotalHealth(value);
            }
            else
            {
                curHealth = value;
            }
        }
        
        public virtual string GetCollisionLayer()
        {
            return "body";
        }
        public virtual string GetID()
        {
            return "body";
        }
        public virtual bool HasDynamicBoundingBox()
        {
            return true;
        }
        public virtual Collider GetCollider()
        {
            return new Collider();
        }
        public virtual Vector2 GetPos() { return new(); }
        
        public virtual DamageInfo Damage(float amount, Vector2 pos, Vector2 dir, IDamageable dmgDealer, bool crit)
        {
            if (!CanBeDamaged()) return new(this, dead);
            if (IsDead()) return new(this, true);

            float recieved = MathF.Min(amount, curHealth);
            curHealth -= recieved;
            bool killed = false; 
            if (curHealth <= 0f) killed = Kill();
            var dmgInfo = new DamageInfo(amount, recieved, 0f, killed, dead, pos, dir, this, crit);
            WasDamaged(dmgInfo);
            return dmgInfo;
        }
        public HealInfo Heal(float amount, Vector2 pos, IDamageable dmgDealer)
        {
            if (GetTotalHealth() <= 0f) return new(this, dead);
            if (!CanBeHealed()) return new(this, dead);
            if (IsDead()) return new(this, true);

            float recieved = MathF.Min(amount, GetTotalHealth() - curHealth);
            curHealth += recieved;
            bool fullHealed = curHealth >= GetTotalHealth();
            var healInfo = new HealInfo(amount, recieved, fullHealed, dead, this);
            WasHealed(healInfo);
            return healInfo;
        }
        public virtual bool CanBeHealed() { return true; }
        public virtual bool CanBeDamaged() { return true; }
        public virtual float GetDamage() { return 0f; }

        
        public string[] GetCollisionMask()
        {
            return collisionMask;
        }

        public virtual void Overlap(CollisionInfo info)
        {
            return;
        }
        public virtual void OverlapEnded(ICollidable other)
        {
            return;
        }
        protected virtual void StatChanged(string statName) { }

       
    }
}
