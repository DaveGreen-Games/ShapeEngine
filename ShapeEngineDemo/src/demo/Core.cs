using System.Numerics;
using Raylib_CsLo;
using ShapeEngineCore.Globals;
using ShapeEngineCore.Globals.Timing;

namespace ShapeEngineCore
{
    public class CoreParticle : Particle
    {
        private float size = 6f;
        private Color color;
        private float rotDeg = 0f;
        private float rotSpeedDeg = 0f;
        public CoreParticle(Vector2 pos, float angle, Color color) : base(pos, 15f)
        {
            angle += RNG.randF(-25, 25) * DEG2RAD;
            float speed = 60f * RNG.randF(0.9f, 1.1f);
            vel = Vec.Rotate(Vec.Right() * speed, angle);
            this.color = color;
            rotDeg = RNG.randF(0f, 360f);
            rotSpeedDeg = RNG.randF(90, 180) * (RNG.randF() < 0.5f ? 1f : -1f);
            drag = 0.99f;
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            rotDeg += rotSpeedDeg * dt;
            rotSpeedDeg *= drag;
        }
        public override void Draw()
        {
            float f = Ease.EaseOutBack(lifetimeTimer.GetF());
            DrawPoly(pos, 6, RNG.randF(0.85f, 1.15f) * size * f, rotDeg, color);
        }
    }
    public class Core
    {
        float curAmount = 0f;
        BasicTimer cooldownTimer = new();
        //protected StatHandler stats = new(("replenish", 0f), ("max", 0f), ("cooldown", 0f));
        protected StatSimple replenish, max, cooldown;
        float useCooldownTimer = 0f;

        public Core(float maxAmount, float replenishPerSecond, float cooldownTime)
        {
            max = new(maxAmount);
            replenish = new(replenishPerSecond);
            cooldown = new(cooldownTime);
            //this.stats.SetBase("max", maxAmount);
            //this.stats.SetBase("replenish", replenishPerSecond);
            //this.stats.SetBase("cooldown", cooldownTime);

            curAmount = maxAmount;
        }

        public (string name, StatSimple stat)[] GetStats(string prefix)
        {
            return new[] { (prefix + "Max", max), (prefix + "Replenish", replenish), (prefix + "Cooldown", cooldown) };
        }
        public float F
        {
            get
            {
                if (max.Cur <= 0f) return 0f;
                return curAmount / max.Cur;
            }
        }
        public float CooldownF
        {
            get
            {
                return cooldownTimer.GetF();
            }
        }
        public bool IsFull() { return curAmount >= max.Cur; }
        public bool IsEmpty() { return curAmount <= 0f; }
        public bool HasReplenish() { return replenish.Cur > 0f; }
        public bool IsCooldownActive() { return cooldownTimer.IsRunning(); }
        public float Cur { get { return curAmount; } }
        public float Max { get { return max.Cur; } }

        //Reloaded, Emptied, Replenished

        public event GameLoop.Triggered? OnTriggered;

        protected void OnReloaded()
        {
            OnTriggered?.Invoke("reloaded");
        }
        protected void OnEmptied()
        {
            OnTriggered?.Invoke("emptied");
        }
        protected void OnReplenished()
        {
            OnTriggered?.Invoke("replenished");
        }

        public void Reload()
        {
            if (IsCooldownActive()) return;
            cooldownTimer.Start(cooldown.Cur);
        }

        public float Use(float amount)
        {
            if (IsEmpty()) return 0f;
            if (IsCooldownActive()) return 0f;
            useCooldownTimer = 0.25f;
            float decrease = MathF.Min(curAmount, amount);
            curAmount -= decrease;
            if (IsEmpty())
            {
                Reload();
                OnEmptied();
            }
            return decrease;
        }
        public float Refill(float amount)
        {
            if (IsFull()) return 0f;
            if (IsCooldownActive()) return 0f;
            float increase = MathF.Min(amount, max.Cur - curAmount);
            curAmount += increase;
            return increase;
        }

        public void Update(float dt)
        {
            if (IsCooldownActive())
            {
                cooldownTimer.Update(dt);
                if (cooldownTimer.IsFinished())
                {
                    Refill(max.Cur);
                    OnReloaded();
                }
            }
            else
            {
                if (useCooldownTimer > 0f)
                {
                    useCooldownTimer -= MathF.Min(dt, useCooldownTimer);
                }
                else
                {
                    if (!IsFull() && HasReplenish())
                    {
                        Refill(replenish.Cur * dt);
                        if (IsFull()) OnReplenished();
                    }
                }
            }
        }
    }
}
